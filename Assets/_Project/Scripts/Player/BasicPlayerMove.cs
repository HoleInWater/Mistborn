using UnityEngine;

public class BasicPlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f; 
    public float sprintSpeed = 10f; 
    public float rotationSpeed = 10f; 
    public float mouseSensitivity = 200f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public LayerMask groundLayer; // Set this to your 'Ground' layer in the Inspector
    private Rigidbody rb;
    private bool isGrounded;

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
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody
        
        // Prevents the capsule from falling over when moving
        rb.freezeRotation = true; 

        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;

        staminaSystem = GetComponent<PlayerStamina>();
    
        if (staminaSystem == null)
        {
            Debug.LogError("PlayerMove cannot find PlayerStamina!");
        }
    }

    void Update()
    {
        // --- GROUND CHECK ---
        // Shoots a tiny ray down to see if we are touching the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // --- SPRINT LOGIC ---
        float currentActiveSpeed = moveSpeed;
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;
        bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 1f;

        if (isTryingToSprint && hasStamina)
        {
            currentActiveSpeed = sprintSpeed;
            staminaSystem.DrainStamina(drainRate);
        }

        // --- MOVEMENT ---
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0; 
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * z + right * x).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Moving via transform.position is okay for simple setups, 
            // but Rigidbody.MovePosition is usually smoother with physics.
            transform.position += moveDirection * currentActiveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- JUMP LOGIC ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- CAMERA ROTATION ---
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
