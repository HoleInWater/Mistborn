using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;

    [Header("Camera Settings")]
    public Transform cameraTransform;    // The Main Camera
    public Transform cameraPivot;        // The Empty Object the camera is inside
    public LayerMask collisionLayers;    // MUST check "Default" or "Ground" in Inspector
    public float cameraRadius = 0.2f;    // Thickness of the camera "bubble"
    public float minDistance = 0.5f;
    
    private float xRotation = 0f;
    private float maxDistance;
    private Vector3 dollyDir;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Calculate original direction and distance
        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
    }

    void Update()
    {
        // 1. Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;

        // 2. Mouse Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // Stops camera from flipping
        
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. Collision Logic
        UpdateCameraCollision();
    }

    void UpdateCameraCollision()
    {
        // Target position if there were no obstacles
        Vector3 desiredCameraPos = cameraPivot.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        // SphereCast acts like a thick beam to prevent corner clipping
        if (Physics.SphereCast(cameraPivot.position, cameraRadius, (desiredCameraPos - cameraPivot.position).normalized, out hit, maxDistance, collisionLayers))
        {
            // If we hit something, move camera to that point (clamped for safety)
            cameraTransform.localPosition = dollyDir * Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            // No obstacles, stay at max distance
            cameraTransform.localPosition = dollyDir * maxDistance;
        }
    }
}
