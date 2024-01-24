using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform holdTransform;

    [SerializeField] float speed = 1, maxSpeed = 5, jumpForce, airControlMultiplier = 0.2f, maxSlope = Mathf.PI / 2f, ragdollBoost = 10f;
    [SerializeField] Rigidbody mainRigidbody, ragdollRootRigidbody;
    [SerializeField] LayerMask groundedLayers;

    [HideInInspector] public Material spriteMaterial, flippedSpriteMaterial, unflippedSpriteMaterial;
    [HideInInspector] public bool isFlipped;

    PlayerInput input;
    Animator animator;
    float lastX = 0, baseDynamicFriction;
    bool ragdoll, hasRagdolled, isGrounded;
    List<IInteractable> interactables = new();
    IInteractable nextInteractable = null;
    Vector3 groundNormal, steepGroundNormal;
    Collider baseCollider;
    PhysicMaterialCombine baseCombine;

    public bool Ragdoll
    {
        get { return ragdoll; }
        set
        {
            ragdoll = value;
            SetRagdoll(ragdoll);
        }
    }

    void Awake()
    {
        BindInputs();
        animator = GetComponent<Animator>();
        baseCollider = mainRigidbody.GetComponent<Collider>();
        baseDynamicFriction = baseCollider.material.dynamicFriction;
        baseCombine = baseCollider.material.frictionCombine;
        unflippedSpriteMaterial = GetComponentInChildren<SpriteRenderer>().material;
        flippedSpriteMaterial = new(unflippedSpriteMaterial);
        flippedSpriteMaterial.SetInt("_SpriteFlipped", 1);
        spriteMaterial = new(unflippedSpriteMaterial);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) spriteRenderer.material = spriteMaterial;
    }

    private void BindInputs()
    {
        input = new();
        PlayerInput.DefaultActions actions = input.Default;
        actions.Ragdoll.performed += (_) => Ragdoll = true;
        actions.Ragdoll.canceled += (_) => { if (Ragdoll) Ragdoll = false; };
        actions.Jump.performed += (_) => Jump();
        actions.Interact.performed += (_) => Interact();
        actions.Drop.performed += (_) => Drop();
    }
    void LateUpdate()
    {
        Vector2 inputWalk = input.Default.Walk.ReadValue<Vector2>();
        baseCollider.material.dynamicFriction = inputWalk.magnitude > 0 ? 0 : baseDynamicFriction;
        baseCollider.material.frictionCombine = inputWalk.magnitude > 0 ? PhysicMaterialCombine.Minimum : baseCombine;
        isGrounded = Grounded();
        hasRagdolled &= !isGrounded;
        animator.SetBool("Grounded", isGrounded);
        inputWalk *= speed * Time.deltaTime * (isGrounded? 1 : airControlMultiplier);
        if (Mathf.Abs(inputWalk.x) > 0) lastX = inputWalk.x;
        animator.SetBool("Moving", inputWalk.magnitude > 0);
        if (!Ragdoll)
        {
            isFlipped = lastX < 0;
            transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
            float upAngle = Vector3.SignedAngle(Vector3.up, Vector3.Scale(new Vector3(groundNormal.x * (isFlipped ? -1 : 1), groundNormal.y, groundNormal.z), new Vector3(1, 1, 0)), Vector3.forward);
            transform.rotation *= Quaternion.AngleAxis(upAngle, Vector3.forward);
            spriteMaterial.SetInt("_SpriteFlipped", isFlipped ? 1 : 0);
        }
        Vector3 moveVector = new Vector3(inputWalk.x, 0, inputWalk.y);
        Vector3 planeVector = Vector3.ProjectOnPlane(moveVector, steepGroundNormal);
        if (planeVector.y > 0) moveVector = planeVector;
        float slopeAngle = Vector3.Angle(Vector3.up, steepGroundNormal);
        moveVector = new Vector3(moveVector.x, Mathf.Max(moveVector.y, 0), moveVector.z);
        if (slopeAngle > maxSlope) moveVector = Vector3.Scale(moveVector, new Vector3(1, 0, 1));
        else moveVector = Vector3.Scale(moveVector, new Vector3(1, 1, 1));
        (Ragdoll? ragdollRootRigidbody : mainRigidbody).AddForce(moveVector, ForceMode.Acceleration);
        if (new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).magnitude > maxSpeed) mainRigidbody.velocity = new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).normalized * maxSpeed + Vector3.up * mainRigidbody.velocity.y;
    }

    private void Jump()
    {
        if (!isGrounded || Ragdoll) return;
        else
        {
            mainRigidbody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    private bool Grounded()
    {
        CapsuleCollider collider = mainRigidbody.GetComponent<CapsuleCollider>();
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + (collider.height / 2) * Vector3.up, collider.radius, -transform.up, collider.height / 2 + 0.1f, groundedLayers);
        if (hits.Length > 0)
        {
            RaycastHit steepestHit = default, closestHit = default;
            float steepestAngle = 0, closestDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                float hitAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (hitAngle > steepestAngle) { steepestAngle = hitAngle; steepestHit = hit; }
                if (hit.distance < closestDistance) { closestDistance = hit.distance; closestHit = hit; }
            }
            groundNormal = closestHit.normal;
            steepGroundNormal = steepestHit.normal;
            return true;
        }
        else return false;
    }

    private void SetRagdoll(bool enabled)
    {
        foreach (Rigidbody rigidbody in transform.GetComponentsInChildren<Rigidbody>())
        {
            if (mainRigidbody == rigidbody)
            {
                if (!enabled) rigidbody.velocity = ragdollRootRigidbody.velocity;
                continue;
            }
            rigidbody.velocity = enabled? mainRigidbody.velocity : Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        if (!enabled) mainRigidbody.transform.position = ragdollRootRigidbody.transform.position;
        animator.enabled = !enabled;
        if (!hasRagdolled) ragdollRootRigidbody.AddForce(mainRigidbody.velocity.normalized * ragdollBoost, ForceMode.Impulse);
        if (enabled) hasRagdolled = true;
    }

    private void Interact()
    {
        if (nextInteractable != null) return;
        if (holdTransform.childCount > 0)
        {
            (holdTransform.GetChild(0).GetComponent<IInteractable>() as Prop).Activate();
            return;
        }
        if (interactables != null && interactables.Count > 0)
        {
            float nearestDistance = float.MaxValue;
            foreach (IInteractable interactable in interactables)
            {
                switch (interactable)
                {
                    case Prop prop:
                        float interactableDistance = Vector3.Distance(prop.transform.position, holdTransform.position);
                        if (interactableDistance < nearestDistance)
                        {
                            nextInteractable = interactable;
                            nearestDistance = interactableDistance;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (nextInteractable is Prop) animator.SetTrigger("Grab");
            else InteractNext();
        }
    }

    private void InteractNext()
    {
        nextInteractable?.Interact(this);
        nextInteractable = null;
    }

    private void Drop()
    {
        if (holdTransform.childCount > 0)
        {
            Transform heldTransform = holdTransform.GetChild(0);
            heldTransform.SetParent(null);
            foreach (Collider collider in heldTransform.GetComponentsInChildren<Collider>(true)) if (!collider.isTrigger) collider.enabled = true;
            heldTransform.GetComponent<Rigidbody>().isKinematic = false;
            foreach (SpriteRenderer spriteRenderer in heldTransform.GetComponentsInChildren<SpriteRenderer>()) spriteRenderer.material = isFlipped? flippedSpriteMaterial : unflippedSpriteMaterial;

        }
    }

    private void OnEnable()
    {
        input.Default.Enable();
    }

    private void OnDisable()
    {
        input?.Default.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable[] newInteractables = other.GetComponentsInChildren<IInteractable>();
        if (newInteractables.Length > 0) interactables.AddRange(newInteractables);
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable[] oldInteractables = other.GetComponentsInChildren<IInteractable>();
        if (oldInteractables.Length > 0)
        {
            interactables.RemoveAll(interactable => oldInteractables.Contains(interactable));
            interactables.TrimExcess();
        }
    }
}
