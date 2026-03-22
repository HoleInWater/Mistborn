using UnityEngine;

public class BasicPlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f; // Kept this name so your other scripts don't break
    public float sprintSpeed = 10f; 
    public float rotationSpeed = 10f; 
    public float mouseSensitivity = 200f;

    [Header("Stamina Settings")]
    public float drainRate = 25f;

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

    private PlayerStamina staminaSystem;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;

        staminaSystem = GetComponent<PlayerStamina>();
    
    if (staminaSystem == null) {
        Debug.LogError("PlayerMove cannot find PlayerStamina! Make sure BOTH scripts are on the same Player object.");
    }
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // --- SPRINT LOGIC ---
        float currentActiveSpeed = moveSpeed; // Starts at your base moveSpeed
        
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;
        bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 1f;

        if (isTryingToSprint && hasStamina)
        {
            currentActiveSpeed = sprintSpeed;
            staminaSystem.DrainStamina(drainRate);
        }
        // ---------------------

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0; 
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * z + right * x).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Use the calculated speed here
            transform.position += moveDirection * currentActiveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void LateUpdate()
    {
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
