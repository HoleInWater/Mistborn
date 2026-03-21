using UnityEngine;

public class animationStateController : MonoBehaviour
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
        bool isWalking = animator.GetBool("isWalking");
        bool forwardPressed = Input.GetKey("w" || "a" || "s" || "d");
        
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
    }
}
