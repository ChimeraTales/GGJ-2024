using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 1, maxSpeed = 5, jumpForce, airControlMultiplier = 0.2f;
    [SerializeField] Rigidbody2D mainRigidbody, ragdollRootRigidbody;
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
        mainRigidbody.AddForce(new Vector2(inputWalk.x, 0), ForceMode2D.Force);
        if (Mathf.Abs(mainRigidbody.velocity.x) > maxSpeed) mainRigidbody.velocity = new(Mathf.Sign(mainRigidbody.velocity.x) * maxSpeed, mainRigidbody.velocity.y); 
        if (inputWalk.magnitude > 0) lastX = inputWalk.x;
        animator.SetBool("Moving", inputWalk.magnitude > 0);
        if (spriteRenderer != null) spriteRenderer.flipX = lastX < 0;
        else transform.localScale = new Vector3(lastX < 0 ? -1 : 1, 1, 1);
    }

    private void Jump()
    {
        if (!Grounded() || Ragdoll) return;
        else
        {
            mainRigidbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            animator.SetTrigger("Jump");
        }
    }

    private bool Grounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.05f, groundedLayers);
        return hit;
    }

    private void SetRagdoll(bool enabled)
    {
        foreach(HingeJoint2D hinge in transform.GetComponentsInChildren<HingeJoint2D>(true))
        {
            hinge.enabled = enabled;
        }
        foreach (Rigidbody2D rigidbody in transform.GetComponentsInChildren<Rigidbody2D>())
        {
            if (mainRigidbody == rigidbody)
            {
                if (!enabled) rigidbody.velocity = ragdollRootRigidbody.velocity;
                continue;
            }
            rigidbody.velocity = enabled? mainRigidbody.velocity : Vector2.zero;
            rigidbody.angularVelocity = 0;
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
