using UnityEngine;

public class StealthSystem : MonoBehaviour
{
    [Header("Stealth Settings")]
    public float crouchSpeed = 2f;
    public float normalSpeed = 5f;
    public float crouchHeight = 1f;
    public float normalHeight = 2f;
    
    [Header("References")]
    public BasicPlayerMove playerController;
    public CapsuleCollider playerCollider;
    public Camera playerCamera;
    
    private bool isCrouching = false;
    private float originalHeight;
    private float originalCenter;
    
    void Start()
    {
        if (playerCollider != null)
        {
            originalHeight = playerCollider.height;
            originalCenter = playerCollider.center.y;
        }
        
        if (playerCamera != null)
        {
            originalHeight = playerCamera.transform.localPosition.y;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCrouch();
        }
    }
    
    void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        
        if (isCrouching)
        {
            Crouch();
        }
        else
        {
            StandUp();
        }
    }
    
    void Crouch()
    {
        if (playerCollider != null)
        {
            playerCollider.height = crouchHeight;
            playerCollider.center = new Vector3(0, crouchHeight / 2f, 0);
        }
        
        if (playerController != null)
        {
            playerController.moveSpeed = crouchSpeed;
        }
        
        Debug.Log("Crouching");
    }
    
    void StandUp()
    {
        if (playerCollider != null)
        {
            playerCollider.height = originalHeight;
            playerCollider.center = new Vector3(0, originalCenter, 0);
        }
        
        if (playerController != null)
        {
            playerController.moveSpeed = normalSpeed;
        }
        
        Debug.Log("Standing");
    }
    
    public bool IsCrouching()
    {
        return isCrouching;
    }
}
