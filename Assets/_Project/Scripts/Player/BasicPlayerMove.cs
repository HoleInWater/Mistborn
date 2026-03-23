// NOTE: Line 41 contains Debug.LogError which should be removed for production
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

    [Header("Smoothness Settings")]
    public float acceleration = 10f;

    private Rigidbody rb;
    private bool isGrounded;
    private float jumpBufferCounter;
    private bool jumpRequested = false;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float maxDistance;
    private Vector3 dollyDir;
    private float currentDistance;

    private PlayerStamina staminaSystem;

    // Cached inputs — read in Update, consumed in FixedUpdate
    private float inputX;
    private float inputZ;
    private bool sprintHeld;
    private bool spaceHeld; // Separate from sprint — needed for low-jump gravity

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = false; // Safety: must be false for velocity to work

        dollyDir = cameraTransform.localPosition.normalized;
        maxDistance = cameraTransform.localPosition.magnitude;
        currentDistance = maxDistance;

        staminaSystem = GetComponent<PlayerStamina>();
        rb.sleepThreshold = 0.0f;
    }

    void Update()
    {
        // Ground check stays in Update for instant responsiveness
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);

        // Cache ALL inputs here — FixedUpdate reads these, never calls Input directly
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        spaceHeld = Input.GetKey(KeyCode.Space); // For low-jump gravity check in FixedUpdate

        // Jump buffer — GetKeyDown MUST be in Update or it gets missed
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Set jump flag here in Update where input is reliable
        // FixedUpdate will execute the actual velocity change
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
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        HandleGravity();
    }

    void HandleMovement()
    {
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Use cached inputs — never call Input.GetAxis inside FixedUpdate
        Vector3 moveDirection = (forward * inputZ + right * inputX).normalized;

        float targetSpeed = 0f;
        if (moveDirection.magnitude > 0.1f)
        {
            bool hasStamina = staminaSystem != null && staminaSystem.currentStamina > 1f;
            targetSpeed = (sprintHeld && hasStamina) ? sprintSpeed : moveSpeed;

            if (targetSpeed == sprintSpeed)
                staminaSystem.DrainStamina(drainRate * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

        // Preserve Y entirely — jump and gravity are never overwritten here
        Vector3 currentHorizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 targetHorizontalVel = moveDirection * targetSpeed;
        Vector3 smoothedVel = Vector3.MoveTowards(currentHorizontalVel, targetHorizontalVel, acceleration * Time.fixedDeltaTime);

        rb.velocity = new Vector3(smoothedVel.x, rb.velocity.y, smoothedVel.z);

        if (moveDirection.magnitude > 0.1f && rb.IsSleeping())
            rb.WakeUp();
    }

    void HandleJump()
    {
        if (!jumpRequested) return;

        // Set Y velocity directly — horizontal momentum is completely preserved
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpRequested = false;
    }

    void HandleGravity()
    {
        if (rb.velocity.y < 0)
        {
            // Falling — apply fast-fall multiplier
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !spaceHeld) // BUG FIX: was !sprintHeld before
        {
            // Rising but Space released — apply low-jump cut
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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
