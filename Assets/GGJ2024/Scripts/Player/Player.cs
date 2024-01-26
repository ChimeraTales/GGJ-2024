using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Transform holdTransform;
    public Rigidbody ragdollRootRigidbody;
    public PlayerInput input;

    [SerializeField] float speed = 1, maxSpeed = 5, jumpForce, airControlMultiplier = 0.2f, maxSlope = Mathf.PI / 2f, ragdollBoost = 10f;
    [SerializeField] Rigidbody mainRigidbody;
    [SerializeField] LayerMask groundedLayers;
    [SerializeField] readonly LayerMask cameraZoneLayers;
    [SerializeField] Collider baseCollider;
    [SerializeField] bool jauntyRotate;
    [SerializeField] SpriteRenderer midpointRenderer;

    [HideInInspector] public Material spriteMaterial, flippedSpriteMaterial, unflippedSpriteMaterial;

    Animator animator;
    float lastX = 0, baseDynamicFriction;
    bool ragdoll, hasRagdolled, isGrounded, isFlipped;
    List<IInteractable> interactables = new();
    IInteractable nextInteractable = null;
    Vector3 groundNormal, steepGroundNormal;
    PhysicMaterialCombine baseCombine;
    Dictionary<Transform, Vector3> bonePositions = new();
    Vector2 walk;

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
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.material = spriteMaterial;
            spriteRenderer.sortingOrder = 0;
        }
        SaveBoneZsRecursively(ragdollRootRigidbody.transform);
    }

    private void SaveBoneZsRecursively(Transform currentBone)
    {
        foreach (Transform bone in currentBone)
        {
            bonePositions.Add(bone, bone.localPosition);
            SaveBoneZsRecursively(bone);
        }
    }

    private void BindInputs()
    {
        //input.actions["Ragdoll"].performed += (_) => Ragdoll = true;
        //input.actions["Ragdoll"].canceled += (_) => { if (Ragdoll) Ragdoll = false; };
    }

    private void OnWalk(InputValue value)
    {
        walk = value.Get<Vector2>();
    }

    private void OnRagdoll(InputValue value)
    {
        Ragdoll = value.isPressed;
    }

    private void LateUpdate()
    {
        baseCollider.material.dynamicFriction = walk.magnitude > 0 ? 0 : baseDynamicFriction;
        baseCollider.material.frictionCombine = walk.magnitude > 0 ? PhysicMaterialCombine.Minimum : baseCombine;
        isGrounded = Grounded();
        hasRagdolled &= !isGrounded;
        animator.SetBool("Grounded", isGrounded);
        Vector2 inputWalk = (isGrounded? 1 : airControlMultiplier) * speed * Time.deltaTime * walk;
        if (Mathf.Abs(inputWalk.x) > 0) lastX = inputWalk.x;
        animator.SetBool("Moving", inputWalk.magnitude > 0);
        if (!Ragdoll)
        {
            bool currentFlipped = lastX < 0;
            if (currentFlipped != isFlipped)
            {
                isFlipped = currentFlipped;
                Flip();
            }
            transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
            float upAngle = Vector3.SignedAngle(Vector3.up, Vector3.Scale(new Vector3(groundNormal.x * (isFlipped ? -1 : 1), groundNormal.y, groundNormal.z), new Vector3(1, 1, 0)), Vector3.forward);
            if (jauntyRotate) transform.rotation *= isGrounded? Quaternion.AngleAxis(Mathf.Abs(upAngle) < maxSlope? upAngle : maxSlope * Mathf.Sign(upAngle), Vector3.forward) : Quaternion.identity;
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

    private void Flip()
    {
        foreach (KeyValuePair<Transform, Vector3> kvp in bonePositions)
        {
            kvp.Key.localPosition = isFlipped ? Vector3.Scale(kvp.Value, new Vector3(1, 1, -1)) : kvp.Value;
        }
    }

    private void OnJump()
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
            if (rigidbody.transform.parent == holdTransform) continue;
            if (mainRigidbody == rigidbody)
            {
                if (!enabled) rigidbody.velocity = ragdollRootRigidbody.velocity;
                continue;
            }
            rigidbody.isKinematic = !enabled;
            if (!enabled) continue;
            rigidbody.velocity = enabled? mainRigidbody.velocity : Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        if (!enabled) mainRigidbody.transform.position = ragdollRootRigidbody.transform.position;
        else if (GameManager.Camera.CurrentView.target == transform) GameManager.Camera.target = ragdollRootRigidbody.transform;
        animator.enabled = !enabled;
        if (!hasRagdolled) ragdollRootRigidbody.AddForce(mainRigidbody.velocity.normalized * ragdollBoost, ForceMode.Impulse);
        if (enabled) hasRagdolled = true;
        else GameManager.Camera.target = GameManager.Camera.CurrentView.target;
        if (isFlipped) Flip();
    }

    private IInteractable NearestInteractable()
    {
        IInteractable closestInteractable = null;
        float nearestDistance = float.MaxValue;
        foreach (IInteractable interactable in interactables)
        {
            switch (interactable)
            {
                case Prop prop:
                    float interactableDistance = Vector3.Distance(prop.transform.position, holdTransform.position);
                    if (interactableDistance < nearestDistance)
                    {
                        closestInteractable = interactable;
                        nearestDistance = interactableDistance;
                    }
                    break;
                default:
                    break;
            }
        }
        return closestInteractable;
    }

    private void OnInteract()
    {
        if (nextInteractable != null) return;
        if (holdTransform.childCount > 0)
        {
            (holdTransform.GetChild(0).GetComponent<IInteractable>() as Prop).Activate();
            return;
        }
        if (interactables != null && interactables.Count > 0)
        {
            nextInteractable = NearestInteractable();
            if (nextInteractable is Prop) animator.SetTrigger("Grab");
            else InteractNext();
        }
    }

    private void InteractNext()
    {
        nextInteractable?.Interact(this);
        nextInteractable = null;
    }

    private void OnDrop()
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

    private void SetPrompts()
    {
        HUD.SetEPrompt(holdTransform.childCount > 0 ? "Use" : interactables.Count == 0 ? "" : NearestInteractable() is Prop ? "Pick up" : "Activate");
    }

    private void OnEnable()
    {
        //input.Default.Enable();
    }

    private void OnDisable()
    {
        //input?.Default.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable[] newInteractables = other.GetComponentsInChildren<IInteractable>();
        if (newInteractables.Length > 0) { interactables.AddRange(newInteractables); SetPrompts(); return; }

        if (other.gameObject.layer == LayerMask.NameToLayer("Camera Zone"))
        {
            GameManager.Camera.CurrentView = other.gameObject.GetComponent<CameraView>();
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable[] oldInteractables = other.GetComponentsInChildren<IInteractable>();
        if (oldInteractables.Length > 0)
        {
            interactables.RemoveAll(interactable => oldInteractables.Contains(interactable));
            interactables.TrimExcess();
            SetPrompts();
        }
    }
}
