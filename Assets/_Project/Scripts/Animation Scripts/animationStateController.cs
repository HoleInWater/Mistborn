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
        // If player presses W, the animator starts isWalking
        if (Input.Getkey("w"))
        {
            animator.setBool("isWalking", true);
        ]
    }
}
