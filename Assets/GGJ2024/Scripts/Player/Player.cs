using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform holdTransform;

    [SerializeField] float speed = 1, maxSpeed = 5, jumpForce, airControlMultiplier = 0.2f;
    [SerializeField] Rigidbody mainRigidbody, ragdollRootRigidbody;
    [SerializeField] LayerMask groundedLayers;

    [HideInInspector] public Material spriteMaterial, flippedSpriteMaterial, unflippedSpriteMaterial;
    [HideInInspector] public bool isFlipped;

    PlayerInput input;
    Animator animator;
    SpriteRenderer spriteRenderer;
    float lastX = 0;
    bool ragdoll;
    List<IInteractable> interactables = new();
    IInteractable nextInteractable = null;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        bool isGrounded = Grounded();
        animator.SetBool("Grounded", isGrounded);
        Vector2 inputWalk = speed * Time.deltaTime * (isGrounded? 1 : airControlMultiplier) * input.Default.Walk.ReadValue<Vector2>();
        (Ragdoll? ragdollRootRigidbody : mainRigidbody).AddForce(new Vector3(inputWalk.x, 0, inputWalk.y), ForceMode.Acceleration);
        if (new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).magnitude > maxSpeed) mainRigidbody.velocity = new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).normalized * maxSpeed + Vector3.up * mainRigidbody.velocity.y; 
        if (inputWalk.magnitude > 0) lastX = inputWalk.x;
        animator.SetBool("Moving", inputWalk.magnitude > 0);
        if (!Ragdoll)
        {
            isFlipped = lastX < 0;
            if (spriteRenderer != null) spriteRenderer.flipX = isFlipped;
            else transform.rotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
            spriteMaterial.SetInt("_SpriteFlipped", isFlipped ? 1 : 0);
        }
    }

    private void Jump()
    {
        if (!Grounded() || Ragdoll) return;
        else
        {
            mainRigidbody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    private bool Grounded()
    {
        float radius = mainRigidbody.GetComponent<CapsuleCollider>().radius;
        bool hit = Physics.SphereCast(transform.position + (radius + 0.05f) * Vector3.up, mainRigidbody.GetComponent<CapsuleCollider>().radius, Vector2.down, out _, 0.1f, groundedLayers);
        return hit;
    }

    private void SetRagdoll(bool enabled)
    {
        foreach(HingeJoint hinge in transform.GetComponentsInChildren<HingeJoint>(true))
        {
            //hinge.enabled = enabled;
        }
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
