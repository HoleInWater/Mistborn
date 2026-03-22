// NOTE: Line 12 contains Debug.Log which should be removed for production
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{

    Animator animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log(animator);
    }

    // Update is called once per frame
    void Update()
    {
        bool isRunning = animator.GetBool("isRunning");
        bool isWalking = animator.GetBool("isWalking");
        bool isJumping = animator.GetBool("isJumping");
        bool forwardPressed = Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d");
        bool runPressed = Input.GetKey("left shift");
        bool jumpPressed = Input.GetKey("space");
        
        // If player presses W, the animator starts isWalking
        if (!isWalking && forwardPressed)
        {
            animator.SetBool("isWalking", true);
        }

        // If player stops pressing W, the animator stops isWalking
        if (isWalking && !forwardPressed)
        {
            animator.SetBool("isWalking", false);
        }

        //If player is holding left shift and w then run
        if (!isRunning && (forwardPressed && runPressed))
        {
            animator.SetBool("isRunning", true);
        }
        
        //If player is not holding left shift and w then don't run
        if (isRunning && (!forwardPressed || !runPressed))
        {
            animator.SetBool("isRunning", false);
        }

        //If player is idle, then you can jump
        if (!isJumping && (!forwardPressed && jumpPressed))
        {
            animator.SetBool("isJumping", true);
        }

        //If player is not idle, then you can't jump
        if (isJumping && (forwardPressed && !jumpPressed))
        {
            animator.SetBool("isJumping", false);
        }
    }
}
