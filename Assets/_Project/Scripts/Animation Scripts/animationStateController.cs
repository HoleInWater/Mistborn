// NOTE: Line 12 contains Debug.Log which should be removed for production
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    public Transform groundCheck;
    public float groundDistance = 0.4f; // The size of the "check bubble"
    public LayerMask groundMask;      // Set this to the "Ground" layer in the Inspector

    Animator animator;
    bool isGrounded;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. CONSTANTLY CHECK IF ON THE FLOOR
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        bool isRunning = animator.GetBool("isRunning");
        bool isWalking = animator.GetBool("isWalking");
        bool isJumping = animator.GetBool("isJumping");

        bool forwardPressed = Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d");
        bool runPressed = Input.GetKey("left shift");
        bool jumpPressed = Input.GetKey("space");
        
        // WALKING LOGIC
        if (!isWalking && forwardPressed) animator.SetBool("isWalking", true);
        if (isWalking && !forwardPressed) animator.SetBool("isWalking", false);

        // RUNNING LOGIC
        if (!isRunning && (forwardPressed && runPressed)) animator.SetBool("isRunning", true);
        if (isRunning && (!forwardPressed || !runPressed)) animator.SetBool("isRunning", false);

        // JUMP START: If grounded and space is pressed, start jump
        if (!isJumping && isGrounded && jumpPressed)
        {
            animator.SetBool("isJumping", true);
        }

        // JUMP END: If we were jumping but are now touching the ground, STOP
        if (isJumping && isGrounded)
        {
            animator.SetBool("isJumping", false);
        }
    }
}
