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
        bool isrunning = animator.GetBool("isRunning");
        bool isWalking = animator.GetBool("isWalking");
        bool forwardPressed = Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d");
        bool runPressed = Input.GetKey("left shift");
        bool offGround = animator.GetBool("Offground");
        bool jump = Input.GetKey(KeyCode.Space);
        
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
        if (!isrunning && (forwardPressed && runPressed))
        {
            animator.SetBool("isRunning", true);
        }
        
        //If player is not holding left shift and w then don't run
        if (isrunning && (!forwardPressed ||!runPressed))
        {
            animator.SetBool("isRunning", false);
        }

        // If player is on ground
        if (offGround && jump)
        {
            animator.SetBool("offGround", true);
        }

        // If player is off ground
        if (!offGround && jump)
        {
            animator.SetBool("offGround", false);
        }
    }
}
