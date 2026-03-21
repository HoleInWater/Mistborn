using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;

    [Header("Camera & Smoothing")]
    public Transform cameraTransform;
    public Transform cameraPivot;
    public LayerMask collisionLayers;
    public float smoothSpeed = 10f;       // How fast the camera snaps to position
    public float rotationSmoothing = 15f; // How "weighty" the mouse feel is
    public float cameraRadius = 0.2f;
    public float minDistance = 0.5f;

    private float xRotation = 0f;
    private float currentXRotation = 0f;
    private float maxDistance;
    private Vector3 dollyDir;
    private float currentDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;
    }

    void Update()
    {
        // 1. WASD Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;

        // 2. Mouse Input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
    }

    // LateUpdate runs after Update, making camera follow much smoother
    void LateUpdate()
    {
        // Smoothly interpolate the rotation
        currentXRotation = Mathf.Lerp(currentXRotation, xRotation, rotationSmoothing * Time.deltaTime);
        cameraPivot.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);

        // 3. Smooth Collision Logic
        Vector3 desiredPos = cameraPivot.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        float targetDistance = maxDistance;

        if (Physics.SphereCast(cameraPivot.position, cameraRadius, (desiredPos - cameraPivot.position).normalized, out hit, maxDistance, collisionLayers))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        // Lerp the distance so the camera "slides" in and out when hitting walls/floors
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothSpeed * Time.deltaTime);
        cameraTransform.localPosition = dollyDir * currentDistance;
    }
}
