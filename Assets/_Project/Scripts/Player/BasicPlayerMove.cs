// NOTE: Consider adding [RequireComponent(typeof(Rigidbody))] attribute for physics
using UnityEngine;
 
public class BasicPlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 10f;
    public float mouseSensitivity = 200f;
 
    [Header("Jumping & Gravity")]
    public float jumpVelocity = 8f;
    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 2f;
    public float jumpBufferTime = 0.2f;
    public LayerMask groundLayer;
 
    [Header("Stamina Settings")]
    public float drainRate = 25f;
 
    [Header("Camera & Smoothing")]
    public Transform cameraTransform;
    public Transform cameraPivot;
    public LayerMask collisionLayers;
    public float smoothSpeed = 10f;
    public float cameraRadius = 0.2f;
    public float minDistance = 0.5f;
 
    [Header("Camera Zoom")]
    [Tooltip("Minimum zoom distance (closest the camera can get)")]
    public float minZoomDistance = 1f;
    [Tooltip("Maximum zoom distance (furthest the camera can pull back)")]
    public float maxZoomDistance = 10f;
    [Tooltip("How much each scroll tick moves the zoom")]
    public float zoomStep = 1f;
    [Tooltip("How smoothly the camera lerps to the target zoom level")]
    public float zoomSmoothSpeed = 8f;
 
    [Header("Smoothness Settings")]
    public float acceleration = 50f; // Raised: 10 was too slow to feel responsive
 
    private Rigidbody rb;
    private bool isGrounded;
    private float jumpBufferCounter;
    private bool jumpRequested = false;
 
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float maxDistance;
    private Vector3 dollyDir;
    private float currentDistance;
    private float targetZoomDistance; // Desired zoom level, adjusted by scroll wheel
 
    private PlayerStamina staminaSystem;
 
    private float inputX;
    private float inputZ;
    private bool sprintHeld;
    private bool spaceHeld;
    private Vector3 _moveDirection;
    private float _currentActiveSpeed;
 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = false;
        rb.sleepThreshold = 0.0f;
 
        // If cameraPivot isn't assigned in Inspector, fall back to this transform
        // so movement doesn't silently break
        if (cameraPivot == null)
        {
            Debug.LogWarning("cameraPivot is not assigned! Falling back to player transform. Assign it in the Inspector.");
            cameraPivot = this.transform;
        }
 
        if (cameraTransform != null)
        {
            dollyDir = cameraTransform.localPosition.normalized;
            maxDistance = cameraTransform.localPosition.magnitude;
            currentDistance = maxDistance;
 
            // Initialize zoom target to the camera's starting distance, clamped to zoom range
            targetZoomDistance = Mathf.Clamp(maxDistance, minZoomDistance, maxZoomDistance);
        }
        else
        {
            Debug.LogWarning("cameraTransform is not assigned! Camera dolly will not work.");
            targetZoomDistance = maxZoomDistance;
        }
 
        staminaSystem = GetComponent<PlayerStamina>();
    }
 
    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
 
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        spaceHeld = Input.GetKey(KeyCode.Space);
 
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
 
        if (jumpBufferCounter > 0f && isGrounded)
        {
            float jumpCost = 15f;
            if (staminaSystem == null || staminaSystem.currentStamina >= jumpCost)
            {
                jumpRequested = true;
                jumpBufferCounter = 0f;
 
                if (staminaSystem != null)
                    staminaSystem.UseStamina(jumpCost);
            }
        }
 
        HandleCamera();
        HandleZoom();
    }
 
    void FixedUpdate()
    {
        // Apply horizontal movement via Rigidbody, preserving vertical velocity (gravity/jump)
        Vector3 horizontalVelocity = _moveDirection * _currentActiveSpeed;
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
 
        // Gravity scaling
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        HandleMovement();
        HandleJump();
        HandleGravity();
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
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
 
        Vector3 moveDirection = (forward * z + right * x).normalized;
 
        // Store for use in FixedUpdate
        _moveDirection = moveDirection;
        _currentActiveSpeed = currentActiveSpeed;
 
        // Rotation is fine to keep here
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
 
    void HandleJump()
    {
        if (!jumpRequested) return;
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpRequested = false;
    }
 
    void HandleGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !spaceHeld)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
 
    void HandleCamera()
    {
        if (cameraPivot == null) return;
 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
 
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
 
        cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
 
    // Reads scroll wheel input and updates the target zoom distance
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Scrolling up (positive) zooms in (reduces distance); down zooms out
            targetZoomDistance -= scroll * zoomStep;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }
    }
 
    void LateUpdate()
    {
        if (cameraTransform == null || cameraPivot == null) return;
 
        // Use targetZoomDistance as the desired max pull-back, then let the
        // collision SphereCast bring it in further if geometry is in the way
        Vector3 desiredPos = cameraPivot.TransformPoint(dollyDir * targetZoomDistance);
        RaycastHit hit;
        float targetDistance = targetZoomDistance;
 
        if (Physics.SphereCast(cameraPivot.position, cameraRadius, (desiredPos - cameraPivot.position).normalized, out hit, targetZoomDistance, collisionLayers))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, targetZoomDistance);
        }
 
        // Smooth both zoom and collision response in one lerp
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothSpeed * Time.deltaTime);
        cameraTransform.localPosition = dollyDir * currentDistance;
    }
}
 
