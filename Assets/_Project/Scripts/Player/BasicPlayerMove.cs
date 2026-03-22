using UnityEngine;

public class BasicPlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f; // New: Sprint speed setting
    public float rotationSpeed = 10f; 
    public float mouseSensitivity = 200f;

    [Header("Stamina Settings")]
    public float drainRate = 25f;   // How fast stamina drops while sprinting

    [Header("Camera & Smoothing")]
    public Transform cameraTransform;
    public Transform cameraPivot;
    public LayerMask collisionLayers;
    public float smoothSpeed = 10f;
    public float cameraRadius = 0.2f;
    public float minDistance = 0.5f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float maxDistance;
    private Vector3 dollyDir;
    private float currentDistance;

    // References to other components
    private PlayerStamina staminaSystem;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;

        // Automatically find the stamina script on this object
        staminaSystem = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        // 1. Get Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // --- NEW SPRINT LOGIC ---
        float currentMoveSpeed = walkSpeed;
        
        // Only allow sprinting if we are moving forward (z > 0) and have stamina
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;
        bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 1f;

        if (isTryingToSprint && hasStamina)
        {
            currentMoveSpeed = sprintSpeed;
            staminaSystem.DrainStamina(drainRate); // Drain stamina while running
        }
        // -------------------------

        // 2. Camera-Relative Movement
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0; 
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * z + right * x).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Move the player using the calculated speed (walk or sprint)
            transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;

            // Rotate player to face the direction they are walking
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. Camera Input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void LateUpdate()
    {
        // 4. Smooth Collision Logic (Stayed the same)
        Vector3 desiredPos = cameraPivot.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        float targetDistance = maxDistance;

        if (Physics.SphereCast(cameraPivot.position, cameraRadius, (desiredPos - cameraPivot.position).normalized, out hit, maxDistance, collisionLayers))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothSpeed * Time.deltaTime);
        cameraTransform.localPosition = dollyDir * currentDistance;
    }
}
