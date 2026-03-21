using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;
    public Transform cameraTransform; // Drag your Main Camera here
    public float minDistance = 1f;    // Closest the camera can get
    
    private float xRotation = 0f;
    private Vector3 cameraDirection;
    private float maxDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Store the starting distance and direction of the camera
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
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // Rotate the Pivot (which rotates the camera child)
        cameraTransform.parent.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. Prevent Ground Clipping
        CheckCameraCollision();
    }

    void CheckCameraCollision()
    {
        // Calculate where the camera *wants* to be
        Vector3 desiredCameraPos = cameraTransform.parent.TransformPoint(cameraDirection * maxDistance);
        RaycastHit hit;

        // Shoot a ray from the pivot to the desired camera position
        if (Physics.Linecast(cameraTransform.parent.position, desiredCameraPos, out hit))
        {
            // If we hit something tagged "Ground", move the camera to that hit point
            if (hit.collider.CompareTag("Ground"))
            {
                float distance = Mathf.Clamp(hit.distance * 0.85f, minDistance, maxDistance);
                cameraTransform.localPosition = cameraDirection * distance;
                return;
            }
        }
        
        // If no ground is hit, stay at max distance
        cameraTransform.localPosition = cameraDirection * maxDistance;
    }
}
