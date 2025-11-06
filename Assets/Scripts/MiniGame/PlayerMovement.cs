using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameManager.IsDead)
        {
            // Detener el movimiento si el jugador está "muerto"
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0);
            return;
        }

        HandleMovement();
        UpdateAnimatorParameters();

    }

    private void HandleMovement()
    {
        // Entrada horizontal
        moveInput = Input.GetAxisRaw("Horizontal");

        // Movimiento horizontal
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Voltear sprite
        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;
    }

    void UpdateAnimatorParameters()
    {
        // Actualiza el parámetro "Speed" con la magnitud del movimiento
        animator.SetFloat("Speed", Mathf.Abs(moveInput));

    }
}
