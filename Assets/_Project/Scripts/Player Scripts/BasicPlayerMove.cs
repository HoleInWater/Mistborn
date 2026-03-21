using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;

    [Header("Camera Collision")]
    public Transform cameraTransform;    // Your Main Camera
    public LayerMask collisionLayers;    // Select "Default", "Ground", and "Wall" layers here
    public float cameraRadius = 0.2f;    // Thickness of the camera "bubble"
    public float minDistance = 0.5f;     // Closest zoom

    private float xRotation = 0f;
    private Vector3 cameraDirection;
    private float maxDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Store the original offset
        cameraDirection = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
    }

    void Update()
    {
        // 1. WASD Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;

        // 2. Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f); // Prevent vertical flipping
        
        cameraTransform.parent.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. Smooth Camera Collision
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Vector3 desiredPos = cameraTransform.parent.TransformPoint(cameraDirection * maxDistance);
        RaycastHit hit;

        // SphereCast checks for volume, not just a single point
        if (Physics.SphereCast(cameraTransform.parent.position, cameraRadius, (desiredPos - cameraTransform.parent.position).normalized, out hit, maxDistance, collisionLayers))
        {
            // Move camera to hit point, slightly offset by radius to prevent clipping
            float distance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            cameraTransform.localPosition = cameraDirection * distance;
        }
        else
        {
            // No hit, go to full distance
            cameraTransform.localPosition = cameraDirection * maxDistance;
        }
    }
}
