using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 1, maxSpeed = 5, jumpForce, airControlMultiplier = 0.2f;
    [SerializeField] Rigidbody mainRigidbody, ragdollRootRigidbody;
    [SerializeField] LayerMask groundedLayers;

    PlayerInput input;
    Animator animator;
    SpriteRenderer spriteRenderer;
    float lastX = 0;
    bool ragdoll;
    bool Ragdoll
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
        input = new();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        input.Default.Ragdoll.performed += (_) => Ragdoll = true;
        input.Default.Ragdoll.canceled += (_) => { if (Ragdoll) Ragdoll = false; };
        input.Default.Jump.performed += (_) => Jump();
    }

    void LateUpdate()
    {
        bool isGrounded = Grounded();
        animator.SetBool("Grounded", isGrounded);
        Vector2 inputWalk = Ragdoll? Vector2.zero : speed * Time.deltaTime * (isGrounded? 1 : airControlMultiplier) * input.Default.Walk.ReadValue<Vector2>();
        mainRigidbody.AddForce(new Vector3(inputWalk.x, 0, inputWalk.y), ForceMode.Acceleration);
        if (new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).magnitude > maxSpeed) mainRigidbody.velocity = new Vector3(mainRigidbody.velocity.x, 0, mainRigidbody.velocity.z).normalized * maxSpeed + Vector3.up * mainRigidbody.velocity.y; 
        if (inputWalk.magnitude > 0) lastX = inputWalk.x;
        animator.SetBool("Moving", inputWalk.magnitude > 0);
        if (spriteRenderer != null) spriteRenderer.flipX = lastX < 0;
        else transform.rotation = Quaternion.Euler(0, lastX < 0 ? 180 : 0, 0);
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
        return Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector2.down, 0.1f, groundedLayers);
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

    private void OnEnable()
    {
        input.Default.Enable();
    }

    private void OnDisable()
    {
        input?.Default.Disable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        interactable?.Interact();

    }
}
