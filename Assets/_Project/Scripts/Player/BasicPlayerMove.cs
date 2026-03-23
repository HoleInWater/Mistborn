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

    private PlayerStamina staminaSystem;

    private float inputX;
    private float inputZ;
    private bool sprintHeld;
    private bool spaceHeld;

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
        }
        else
        {
            Debug.LogWarning("cameraTransform is not assigned! Camera dolly will not work.");
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
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        HandleGravity();
    }

    void HandleMovement()
    {
        // Defensive: if cameraPivot is somehow still null, skip to avoid exception
        if (cameraPivot == null) return;

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * inputZ + right * inputX).normalized;

        float targetSpeed = 0f;
        if (moveDirection.magnitude > 0.1f)
        {
            bool hasStamina = staminaSystem == null || staminaSystem.currentStamina > 1f;
            targetSpeed = (sprintHeld && hasStamina) ? sprintSpeed : moveSpeed;

            if (staminaSystem != null && targetSpeed == sprintSpeed)
                staminaSystem.DrainStamina(drainRate * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

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

    void LateUpdate()
    {
        if (cameraTransform == null || cameraPivot == null) return;

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
