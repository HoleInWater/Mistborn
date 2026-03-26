using UnityEngine;

/// <summary>
/// Standalone crouch controller (follows Sprint.cs pattern).
/// Handles collider shrink, speed reduction, and stealth bonus.
/// Left Ctrl to toggle crouch. Can't crouch while sprinting.
/// </summary>
public class CrouchSystem : MonoBehaviour
{
    [Header("Crouch Settings")]
    public float crouchSpeed = 2.5f;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float transitionSpeed = 10f;

    [Header("Stealth")]
    [Tooltip("Crouching reduces enemy detection range by this multiplier")]
    [Range(0f, 1f)] public float stealthMultiplier = 0.5f;

    [Header("References")]
    public BasicPlayerMove playerMove;
    public CapsuleCollider playerCollider;
    public Camera playerCamera;

    [HideInInspector] public bool isCrouching = false;

    private float originalHeight;
    private float originalCenter;
    private float originalCameraY;
    private float originalSpeed;

    void Start()
    {
        if (playerMove == null) playerMove = GetComponent<BasicPlayerMove>();
        if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCollider != null)
        {
            originalHeight = playerCollider.height;
            originalCenter = playerCollider.center.y;
        }

        if (playerCamera != null)
            originalCameraY = playerCamera.transform.localPosition.y;

        if (playerMove != null)
            originalSpeed = playerMove.moveSpeed;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isCrouching)
                TryStandUp();
            else
                EnterCrouch();
        }

        // Can't crouch while sprinting
        if (isCrouching && Input.GetKey(KeyCode.LeftShift))
            TryStandUp();

        UpdateCollider();
        UpdateSpeed();
    }

    void EnterCrouch()
    {
        isCrouching = true;
    }

    void TryStandUp()
    {
        // Check if there's space above to stand
        if (playerCollider != null)
        {
            float checkDist = standHeight - crouchHeight;
            Vector3 checkPos = transform.position + Vector3.up * crouchHeight;
            if (Physics.SphereCast(checkPos, playerCollider.radius * 0.9f, Vector3.up, out _, checkDist))
                return; // Blocked
        }
        isCrouching = false;
    }

    void UpdateCollider()
    {
        if (playerCollider == null) return;

        float targetHeight = isCrouching ? crouchHeight : originalHeight;
        float targetCenter = isCrouching ? crouchHeight * 0.5f : originalCenter;

        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, Time.deltaTime * transitionSpeed);
        playerCollider.center = new Vector3(0,
            Mathf.Lerp(playerCollider.center.y, targetCenter, Time.deltaTime * transitionSpeed), 0);

        // Lower camera when crouching
        if (playerCamera != null)
        {
            float targetCamY = isCrouching ? originalCameraY * 0.5f : originalCameraY;
            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * transitionSpeed);
            playerCamera.transform.localPosition = camPos;
        }
    }

    void UpdateSpeed()
    {
        if (playerMove == null) return;

        if (isCrouching)
            playerMove.moveSpeed = crouchSpeed;
        else if (playerMove.moveSpeed < originalSpeed)
            playerMove.moveSpeed = originalSpeed;
    }

    public bool IsCrouching() => isCrouching;
    public float GetStealthMultiplier() => isCrouching ? stealthMultiplier : 1f;
}
