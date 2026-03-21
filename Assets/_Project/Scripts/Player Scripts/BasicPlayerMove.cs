using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sensitivity = 100f;

    void Update()
    {
        // WASD Movement
        float x = Input.GetAxis("Horizontal"); // A, D
        float z = Input.GetAxis("Vertical");   // W, S

        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Arrow Key Looking (Rotation)
        float rotateX = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) rotateX = -1;
        if (Input.GetKey(KeyCode.RightArrow)) rotateX = 1;

        transform.Rotate(Vector3.up * rotateX * sensitivity * Time.deltaTime);
    }
}
