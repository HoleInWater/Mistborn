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
    public float fallMultiplier = 3f;       // Extra downward force on the way down for snappier falls
    public float lowJumpMultiplier = 2f;    // Extra downward force when jump key is released early (short hop)
    public float jumpBufferTime = 0.2f;     // Seconds before landing that a jump input is still accepted
    public LayerMask groundLayer;
 
    [Header("Stamina Settings")]
    public float drainRate = 25f;           // Stamina drained per second while sprinting
 
    [Header("Camera & Smoothing")]
    public Transform cameraTransform;       // The actual camera object (child of cameraPivot)
    public Transform cameraPivot;           // Empty parent that rotates for mouse look; camera hangs off this
    public LayerMask collisionLayers;       // Layers the camera dolly should collide with
    public float smoothSpeed = 10f;
    public float cameraRadius = 0.2f;       // SphereCast radius for camera collision — larger = pulls in sooner
    public float minDistance = 0.5f;        // Closest the collision system will push the camera in
 
    [Header("Camera Zoom")]
    // defaultZoomDistance sets where the camera sits at startup.
    // It must fall between minZoomDistance and maxZoomDistance or it will be clamped.
    [Tooltip("Starting distance of the camera from the pivot on play")]
    public float defaultZoomDistance = 5f;
    [Tooltip("Minimum zoom distance — closest the camera can get to the player")]
    public float minZoomDistance = 1f;
    [Tooltip("Maximum zoom distance — furthest the camera can pull back from the player")]
    public float maxZoomDistance = 10f;
    [Tooltip("How far the camera moves per scroll tick")]
    public float zoomStep = 1f;
    [Tooltip("How smoothly the camera lerps to the new zoom level (higher = snappier)")]
    public float zoomSmoothSpeed = 8f;
 
    [Header("Smoothness Settings")]
    public float acceleration = 50f;        // Raised: 10 was too slow to feel responsive
 
    // ── Internal state ──────────────────────────────────────────────────────────
 
    private Rigidbody rb;
    private bool isGrounded;
    private float jumpBufferCounter;        // Counts down after jump is pressed; allows early jump input
    private bool jumpRequested = false;     // Set in Update, consumed in FixedUpdate via HandleJump
 
    private float xRotation = 0f;          // Accumulated vertical mouse look (pitch)
    private float yRotation = 0f;          // Accumulated horizontal mouse look (yaw)
 
    // dollyDir: the normalised local-space direction the camera sits along relative to the pivot.
    // Always Vector3.back (0,0,-1) for a standard third-person rig — camera is behind the pivot.
    private Vector3 dollyDir;
 
    private float currentDistance;         // Actual current camera distance, smoothly lerped each frame
    private float targetZoomDistance;      // Desired camera distance set by the scroll wheel
 
    private PlayerStamina staminaSystem;
 
    // Input values cached in Update and consumed in FixedUpdate / HandleMovement
    private float inputX;
    private float inputZ;
    private bool sprintHeld;
    private bool spaceHeld;
    private Vector3 _moveDirection;
    private float _currentActiveSpeed;
 
    // ── Unity lifecycle ──────────────────────────────────────────────────────────
 
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
 
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;                               // Prevent physics from tipping the player over
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth visual position between physics steps
        rb.isKinematic = false;
        rb.sleepThreshold = 0.0f;                              // Never let the rigidbody sleep mid-movement
 
        // If cameraPivot isn't assigned in the Inspector, fall back to this transform
        // so movement doesn't silently break — though look direction will be wrong
        if (cameraPivot == null)
        {
            Debug.LogWarning("cameraPivot is not assigned! Falling back to player transform. Assign it in the Inspector.");
            cameraPivot = this.transform;
        }
 
        if (cameraTransform != null)
        {
            // FIX: Always set dollyDir to Vector3.back so the camera sits directly
            // behind the pivot in local space. The previous approach used
            // cameraTransform.localPosition.normalized, which would return a zero
            // vector if the camera's local position was (0,0,0) — making zoom have
            // no visible effect at all in a standard third-person hierarchy.
            dollyDir = Vector3.back;
 
            // Clamp the starting zoom in case Inspector values are inconsistent
            targetZoomDistance = Mathf.Clamp(defaultZoomDistance, minZoomDistance, maxZoomDistance);
            currentDistance = targetZoomDistance;
 
            // Snap the camera to the correct position immediately so there's no
            // pop or lerp from the wrong position on the very first frame
            cameraTransform.localPosition = dollyDir * currentDistance;
        }
        else
        {
            Debug.LogWarning("cameraTransform is not assigned! Camera dolly and zoom will not work.");
            targetZoomDistance = defaultZoomDistance;
        }
 
        staminaSystem = GetComponent<PlayerStamina>();
    }
 
    void Update()
    {
        // Ground check: short downward ray from the player's centre
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
 
        // Cache raw inputs so they can be reused across Update and FixedUpdate
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        spaceHeld = Input.GetKey(KeyCode.Space);
 
        // Jump buffer: record a jump request for up to jumpBufferTime seconds so the
        // player can press Space slightly before landing and still trigger a jump
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
                jumpBufferCounter = 0f;     // Consume the buffered input so it doesn't fire again
 
                if (staminaSystem != null)
                    staminaSystem.UseStamina(jumpCost);
            }
        }
 
        HandleCamera();  // Mouse look runs in Update for maximum responsiveness
        HandleZoom();    // Scroll wheel zoom runs in Update to catch every scroll event
    }
 
    void FixedUpdate()
    {
        // Build horizontal velocity from the cached move direction, while keeping
        // the current vertical velocity (preserves gravity and the jump arc)
        Vector3 horizontalVelocity = _moveDirection * _currentActiveSpeed;
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
 
        HandleMovement(); // Recalculate move direction and speed from input
        HandleJump();     // Apply jump velocity if a jump was requested this frame
        HandleGravity();  // Apply fall/low-jump multipliers for a better-feeling jump arc
    }
 
    // ── Movement ─────────────────────────────────────────────────────────────────
 
    void HandleMovement()
    {
        // Use GetAxis (smoothed) rather than GetAxisRaw for analogue-feel movement
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
 
        // Project camera forward/right onto the horizontal plane so camera tilt
        // doesn't push the player into or out of the ground when moving
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
 
        // Combine axes into a world-space move direction relative to where the camera faces
        Vector3 moveDirection = (forward * z + right * x).normalized;
 
        // Store for use at the top of FixedUpdate on the same physics step
        _moveDirection = moveDirection;
        _currentActiveSpeed = currentActiveSpeed;
 
        // Rotate the player model to face the direction of travel
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
 
    void HandleJump()
    {
        // jumpRequested is set in Update and consumed here on a FixedUpdate step
        // so the jump velocity is applied consistently regardless of frame rate
        if (!jumpRequested) return;
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpRequested = false;
    }
 
    void HandleGravity()
    {
        // Extra downward force while falling — makes the jump arc feel less floaty
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // If the player releases Space early, apply a stronger pull-down for a short hop
        else if (rb.velocity.y > 0 && !spaceHeld)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
 
    // ── Camera ───────────────────────────────────────────────────────────────────
 
    void HandleCamera()
    {
        if (cameraPivot == null) return;
 
        // Scale by Time.deltaTime to keep sensitivity consistent across frame rates
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
 
        yRotation += mouseX;
        xRotation -= mouseY; // Subtract so moving the mouse up tilts the camera upward
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // Prevent flipping past vertical
 
        cameraPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
 
    void HandleZoom()
    {
        // Mouse ScrollWheel returns a positive value when scrolling up (zoom in)
        // and a negative value when scrolling down (zoom out)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
 
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Subtract because scrolling up (positive) should reduce distance (zoom in)
            targetZoomDistance -= scroll * zoomStep;
 
            // Clamp so the player can't zoom past the min/max set in the Inspector
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }
    }
 
    void LateUpdate()
    {
        // LateUpdate runs after all Update calls, so the camera always reads the
        // final pivot rotation for the frame — this prevents one-frame jitter
        if (cameraTransform == null || cameraPivot == null) return;
 
        // Work out the world-space position the camera wants to sit at
        Vector3 desiredCameraPos = cameraPivot.TransformPoint(dollyDir * targetZoomDistance);
        Vector3 directionFromPivot = (desiredCameraPos - cameraPivot.position).normalized;
 
        float targetDistance = targetZoomDistance;
 
        // SphereCast from the pivot outward — if geometry is in the way (e.g. a wall),
        // pull the camera in to sit just in front of the obstacle
        RaycastHit hit;
        if (Physics.SphereCast(cameraPivot.position, cameraRadius, directionFromPivot, out hit, targetZoomDistance, collisionLayers))
        {
            // Clamp so collision can only bring the camera closer, never push it
            // further than the current zoom target
            targetDistance = Mathf.Clamp(hit.distance, minDistance, targetZoomDistance);
        }
 
        // Smoothly lerp the actual distance toward the target.
        // This handles both zoom transitions and the camera easing back out
        // after the player walks away from a wall.
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothSpeed * Time.deltaTime);
 
        // Apply the final distance along the dolly direction in local space
        cameraTransform.localPosition = dollyDir * currentDistance;
    }
}
 
