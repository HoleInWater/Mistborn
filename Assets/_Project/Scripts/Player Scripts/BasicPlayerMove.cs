using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;
    
    private float xRotation = 0f;

    void Start()
    {
        // Keeps the mouse stuck in the middle and invisible
        Cursor.lockState = CursorLockMode.Locked;
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

        // Rotate the whole player left/right (Y-axis)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate just the camera up/down (X-axis)
        // Note: For this to work, the Camera should be a CHILD of the Player
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevents flipping upside down

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
