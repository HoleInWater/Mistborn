/* PlayerCamera.cs
 * 
 * PURPOSE:
 * Implements a third-person follow camera with smooth collision detection.
 * Handles camera rotation based on mouse input and maintains proper distance from the player.
 * 
 * KEY FIELDS:
 * - cameraTransform: Reference to the actual camera transform (child of pivot)
 * - cameraPivot: Pivot point for camera rotation (usually at player's shoulders)
 * - collisionLayers: Layers that block camera movement (geometry, walls, etc.)
 * 
 * HOW IT WORKS:
 * 1. Mouse input rotates the camera pivot (horizontal and vertical)
 * 2. LateUpdate adjusts camera position to avoid collisions with geometry
 * 3. SphereCast detects obstacles and adjusts camera distance smoothly
 * 
 * IMPORTANT NOTES:
 * - Attach to the same GameObject as the PlayerController
 * - Assign cameraTransform and cameraPivot in Inspector
 * - Set collisionLayers to include all obstructing geometry
 * - Mouse sensitivity can be adjusted via SetMouseSensitivity()
 * 
 * LORE ACCURACY:
 * Standard third-person camera for Mistborn game. Not allomancy-specific.
 */

using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform of the main camera")]
    public Transform cameraTransform;
    [Tooltip("Pivot point for camera rotation (usually empty GameObject at player's shoulders)")]
    public Transform cameraPivot;
    [Tooltip("Layers that block camera collision")]
    public LayerMask collisionLayers;

    [Header("Camera Settings")]
    [Tooltip("Mouse sensitivity for camera rotation")]
    public float mouseSensitivity = 200f;
    [Tooltip("Smooth speed for camera collision adjustment")]
    public float smoothSpeed = 10f;
    [Tooltip("Radius of sphere for collision detection")]
    public float cameraRadius = 0.2f;
    [Tooltip("Minimum distance from pivot when collision occurs")]
    public float minDistance = 0.5f;

    // Private state for rotation
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float maxDistance;
    private Vector3 dollyDir;
    private float currentDistance;

    void Start()
    {
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;

        // Calculate initial camera direction and distance
        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Apply rotation
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply rotation to pivot
        cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void LateUpdate()
    {
        // Calculate desired camera position based on pivot rotation and max distance
        Vector3 desiredPos = cameraPivot.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        float targetDistance = maxDistance;

        // SphereCast to detect collisions between pivot and desired camera position
        if (Physics.SphereCast(cameraPivot.position, cameraRadius, (desiredPos - cameraPivot.position).normalized, out hit, maxDistance, collisionLayers))
        {
            // If collision, clamp distance to hit point (but not closer than minDistance)
            targetDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        // Smoothly interpolate current distance to target distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothSpeed * Time.deltaTime);
        // Set camera local position
        cameraTransform.localPosition = dollyDir * currentDistance;
    }

    // Public method to set mouse sensitivity (for options menu)
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
}