using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 1;
    PlayerInput input;
    Animator animator;
    SpriteRenderer spriteRenderer;
    float lastX = 0;

    void Awake()
    {
        input = new();
        animator = GetComponent<Animator>();
        spriteRenderer= GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        Vector2 inputWalk = speed * Time.deltaTime * input.Default.Walk.ReadValue<Vector2>();
        transform.position = transform.position + new Vector3(inputWalk.x, 0, inputWalk.y);
        animator.SetBool("Walking", Mathf.Abs(inputWalk.x) > 0);
        if (Mathf.Abs(inputWalk.x) > 0) lastX = inputWalk.x;
        spriteRenderer.flipX = lastX < 0;
    }

    private void OnEnable()
    {
        input.Default.Enable();
    }

    private void OnDisable()
    {
        input?.Default.Disable();
    }
}
