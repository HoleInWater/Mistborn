// NOTE: Line 41 contains Debug.LogError which should be removed for production
using UnityEngine;

public class BasicPlayerMove : MonoBehaviour
{
    [Header("Movement")]
    // NOTE: Consider adding [Range(1f, 20f)] attribute for moveSpeed
    public float moveSpeed = 5f; 
    // NOTE: Consider adding [Range(5f, 30f)] attribute for sprintSpeed
    public float sprintSpeed = 10f; 
    // NOTE: Consider adding [Range(1f, 50f)] attribute for rotationSpeed
    public float rotationSpeed = 10f; 
    // NOTE: Consider adding [Range(10f, 1000f)] attribute for mouseSensitivity
    public float mouseSensitivity = 200f;

    [Header("Jumping & Gravity")]
    // NOTE: Consider adding [Range(1f, 20f)] attribute for jumpVelocity
    public float jumpVelocity = 8f; // Use this instead of force
    // NOTE: Consider adding [Range(1f, 10f)] attribute for fallMultiplier
    public float fallMultiplier = 3f; // Fast fall
    // NOTE: Consider adding [Range(1f, 5f)] attribute for lowJumpMultiplier
    public float lowJumpMultiplier = 2f; 
    // NOTE: Consider adding [Range(0f, 1f)] attribute for jumpBufferTime
    public float jumpBufferTime = 0.2f; // Forgiveness window
    // NOTE: Consider adding [Tooltip("Layer mask for ground detection")] attribute
    public LayerMask groundLayer;
    
    private Rigidbody rb;
    private bool isGrounded;
    private float jumpBufferCounter;

    [Header("Stamina Settings")]
    // NOTE: Consider adding [Range(1f, 100f)] attribute for drainRate
    public float drainRate = 25f;

    [Header("Camera & Smoothing")]
    // NOTE: Consider adding [Tooltip("Transform of the main camera")] attribute
    public Transform cameraTransform;
    // NOTE: Consider adding [Tooltip("Pivot point for camera rotation")] attribute
    public Transform cameraPivot;
    // NOTE: Consider adding [Tooltip("Layers that block camera collision")] attribute
    public LayerMask collisionLayers;
    // NOTE: Consider adding [Range(1f, 50f)] attribute for smoothSpeed
    public float smoothSpeed = 10f;
    // NOTE: Consider adding [Range(0.01f, 1f)] attribute for cameraRadius
    public float cameraRadius = 0.2f;
    // NOTE: Consider adding [Range(0.1f, 5f)] attribute for minDistance
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
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; 
        rb.interpolation = RigidbodyInterpolation.Interpolate; // CRITICAL for smoothness

        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;

        staminaSystem = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);

        // Jump Buffering Logic
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        HandleMovement();
        HandleJump();
        HandleCamera();
    }

    void FixedUpdate()
    {
        // Smooth Gravity scaling
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void HandleJump()
    {
        // Jump only if the buffer is active and we are on the ground
        if (jumpBufferCounter > 0f && isGrounded)
        {
            // Reset vertical velocity for a consistent, "soft" launch
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
            jumpBufferCounter = 0; // Clear the buffer
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float currentActiveSpeed = moveSpeed;
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;
        bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 1f;

        if (isTryingToSprint && hasStamina)
        {
            currentActiveSpeed = sprintSpeed;
            staminaSystem.DrainStamina(drainRate);
        }

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 moveDirection = (forward * z + right * x).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Direct position move can feel snappy; consider adding 
            // a Lerp here if you want "weighty" acceleration later.
            transform.position += moveDirection * currentActiveSpeed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleCamera()
    {
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
