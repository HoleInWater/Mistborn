using UnityEngine;

public class animationStateController : MonoBehaviour
{

    Animator animator;
    int isWalkingHash;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        Debug.Log(animator);
    }

    // Update is called once per frame
    void Update()
    {
        bool isWalking = animator.GetBool("isWalkingHash");
        bool forwardPressed = Input.GetKey("w");
        
        // If player presses W, the animator starts isWalking
        if (!isWalking && forwardPressed)
        {
            animator.SetBool("isWalkingHash", true);
        }

        // If player stops pressing W, the animator stops isWalking
        if (isWalking && !forwardPressed)
        {
            animator.SetBool("isWalkingHash", false);
        }
    }
}
