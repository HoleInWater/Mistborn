using UnityEngine;

/// <summary>
/// Drives Animator parameters from player input and physics state.
/// Handles locomotion (idle/walk/run), jumping, crouching, and combat triggers.
/// Works alongside AnimationConfig.Params for consistent parameter names.
/// </summary>
public class AnimationStateController : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    BasicPlayerMove playerMove;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = ~0;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        playerMove = GetComponent<BasicPlayerMove>();
    }

    void Update()
    {
        if (animator == null) return;

        // ── Input ────────────────────────────────────────────────────────
        bool forwardPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
                           || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool crouchPressed = Input.GetKey(KeyCode.LeftControl);

        // ── Ground Check ─────────────────────────────────────────────────
        wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f,
            Vector3.down, groundCheckDistance + 0.1f, groundMask);

        // Use BasicPlayerMove's IsGrounded if available for consistency
        if (playerMove != null)
            isGrounded = playerMove.IsGrounded();

        // ── Speed (for blend tree) ───────────────────────────────────────
        float speed = 0f;
        if (rb != null)
        {
            Vector3 horizontalVel = rb.linearVelocity;
            horizontalVel.y = 0;
            speed = horizontalVel.magnitude;
        }
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        // ── Vertical Velocity ────────────────────────────────────────────
        float vertVel = rb != null ? rb.linearVelocity.y : 0f;
        animator.SetFloat("VerticalVelocity", vertVel, 0.1f, Time.deltaTime);

        // ── Grounded ─────────────────────────────────────────────────────
        animator.SetBool("IsGrounded", isGrounded);

        // Landing event
        if (isGrounded && !wasGrounded)
        {
            animator.SetTrigger("Land");
        }

        // ── Walking / Running ────────────────────────────────────────────
        bool isWalking = forwardPressed && !runPressed && isGrounded;
        bool isRunning = forwardPressed && runPressed && isGrounded;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);

        // ── Jumping ──────────────────────────────────────────────────────
        if (jumpPressed && isGrounded)
        {
            if (forwardPressed || runPressed)
            {
                animator.SetBool("isRunJump", true);
                animator.SetBool("isJumping", false);
            }
            else
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isRunJump", false);
            }
        }

        // Clear jump states when landing
        if (isGrounded && !jumpPressed)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isRunJump", false);
        }

        // ── Crouching ────────────────────────────────────────────────────
        animator.SetBool("IsCrouching", crouchPressed && isGrounded);
    }
}
