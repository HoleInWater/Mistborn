using UnityEngine;
using System.Collections;

public class AssassinMovement : MonoBehaviour
{
    [Header("Core Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float sprintSpeed = 15f;
    public float acceleration = 50f;
    public float deceleration = 20f;
    public float rotationSpeed = 15f;
    public float airControl = 0.3f;

    [Header("Parkour")]
    public float climbSpeed = 4f;
    public float wallRunSpeed = 8f;
    public float wallRunGravity = 0.5f;
    public float jumpVaultForce = 8f;
    public float ledgeGrabForce = 5f;
    public float ledgeClimbSpeed = 2f;
    public float slideSpeed = 12f;
    public float slideDeceleration = 15f;
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 1f;
    public float coverMoveSpeed = 3f;

    [Header("Detection")]
    public float wallRunDetectDistance = 1.5f;
    public float climbDetectHeight = 2f;
    public float ledgeDetectDistance = 0.8f;
    public float vaultDetectDistance = 2f;
    public LayerMask parkourLayers;

    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";
    public string jumpParam = "IsJumping";
    public string climbParam = "IsClimbing";
    public string wallRunParam = "IsWallRunning";
    public string rollParam = "IsRolling";
    public string slideParam = "IsSliding";
    public string coverParam = "IsInCover";

    [Header("References")]
    public Transform cameraTransform;
    public Rigidbody rb;
    public CapsuleCollider playerCollider;

    private enum MovementState { Grounded, Airborne, Climbing, WallRunning, LedgeGrab, Sliding, Rolling, Cover }
    private MovementState currentState = MovementState.Grounded;
    private MovementState previousState = MovementState.Grounded;

    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isSprinting;
    private bool isVaulting;
    private bool isWallRunning;
    private bool isClimbing;
    private bool isLedgeGrabbing;
    private bool isSliding;
    private bool isRolling;
    private bool isInCover;
    private bool isCoverCrouch;

    private float rollTimer;
    private float rollCooldownTimer;
    private float slideTimer;

    private Vector3 wallRunDirection;
    private Transform ledgeGrabPoint;
    private Transform climbTarget;
    private Vector3 coverPosition;
    private Transform coverTransform;

    private bool jumpRequested;
    private bool sprintHeld;
    private bool crouchHeld;
    private bool rollHeld;
    private bool coverHeld;

    private RaycastHit wallHit;
    private RaycastHit ledgeHit;
    private RaycastHit climbHit;
    private RaycastHit coverHit;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        CacheInputs();
        CheckEnvironment();
        HandleStateTransitions();
        HandleInputActions();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case MovementState.Grounded:
                HandleGroundedMovement();
                break;
            case MovementState.Airborne:
                HandleAirborneMovement();
                break;
            case MovementState.Climbing:
                HandleClimbing();
                break;
            case MovementState.WallRunning:
                HandleWallRun();
                break;
            case MovementState.LedgeGrab:
                HandleLedgeGrab();
                break;
            case MovementState.Sliding:
                HandleSliding();
                break;
            case MovementState.Rolling:
                HandleRolling();
                break;
            case MovementState.Cover:
                HandleCover();
                break;
        }
    }

    void CacheInputs()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(h, 0, v).normalized;

        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        crouchHeld = Input.GetKey(KeyCode.LeftControl);
        rollHeld = Input.GetKeyDown(KeyCode.C);
        coverHeld = Input.GetKey(KeyCode.LeftAlt);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
    }

    void CheckEnvironment()
    {
        bool hasWall = Physics.Raycast(transform.position + Vector3.up * 1.5f, transform.forward, out wallHit, wallRunDetectDistance, parkourLayers);
        bool hasLedge = Physics.Raycast(transform.position + Vector3.up * 1.8f, transform.forward, out ledgeHit, ledgeDetectDistance, parkourLayers);
        bool hasClimbable = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out climbHit, climbDetectHeight, parkourLayers);
        bool hasCover = Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, out coverHit, 1.5f, parkourLayers);

        bool canWallRun = hasWall && IsWallRunable(wallHit.normal);
        bool canClimb = hasClimbable && climbHit.collider != null;
        bool canLedgeGrab = hasLedge && !Physics.Raycast(transform.position + Vector3.up * 2.2f, transform.forward, ledgeDetectDistance, parkourLayers);
        bool canEnterCover = hasCover && crouchHeld;
    }

    bool IsWallRunable(Vector3 wallNormal)
    {
        float angle = Vector3.Angle(wallNormal, Vector3.up);
        return angle > 45f && angle < 90f;
    }

    void HandleStateTransitions()
    {
        if (rollCooldownTimer > 0) rollCooldownTimer -= Time.deltaTime;
        if (slideTimer > 0) slideTimer -= Time.deltaTime;

        if (isRolling && rollTimer <= 0)
        {
            currentState = MovementState.Grounded;
            isRolling = false;
            return;
        }

        if (isSliding && slideTimer <= 0)
        {
            currentState = MovementState.Grounded;
            isSliding = false;
            return;
        }

        if (coverHeld && currentState != MovementState.Cover && canEnterCover())
        {
            EnterCover();
            return;
        }

        if (currentState == MovementState.Cover && !coverHeld)
        {
            ExitCover();
            return;
        }

        if (jumpRequested)
        {
            if (currentState == MovementState.WallRunning)
            {
                WallRunJump();
            }
            else if (currentState == MovementState.LedgeGrab)
            {
                LedgeClimb();
            }
            else if (isInCover)
            {
                CoverVault();
            }
            else if (canVault())
            {
                Vault();
            }
            else if (canClimb())
            {
                StartClimb();
            }
            else
            {
                Jump();
            }

            jumpRequested = false;
        }

        if (crouchHeld && !isInCover && currentState == MovementState.Grounded && moveDirection.magnitude > 0.1f)
        {
            StartSlide();
        }

        if (rollHeld && !isRolling && rollCooldownTimer <= 0 && currentState == MovementState.Grounded)
        {
            StartRoll();
        }

        if (currentState == MovementState.Grounded && !IsGrounded())
        {
            currentState = MovementState.Airborne;
        }
        else if (currentState == MovementState.Airborne && IsGrounded())
        {
            currentState = MovementState.Grounded;
        }
    }

    bool canEnterCover()
    {
        return Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, out coverHit, 1.5f, parkourLayers);
    }

    bool canVault()
    {
        return Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, vaultDetectDistance, parkourLayers);
    }

    bool canClimb()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out climbHit, climbDetectHeight, parkourLayers);
    }

    void HandleInputActions()
    {
    }

    void StartSlide()
    {
        isSliding = true;
        currentState = MovementState.Sliding;
        slideTimer = 1f;
    }

    void StartRoll()
    {
        isRolling = true;
        currentState = MovementState.Rolling;
        rollTimer = rollDuration;
        rollCooldownTimer = rollCooldown;

        rb.linearVelocity = transform.forward * rollSpeed;
    }

    void EnterCover()
    {
        currentState = MovementState.Cover;
        isInCover = true;
        coverPosition = coverHit.transform.position - transform.forward * 0.3f;
        coverTransform = coverHit.transform;
    }

    void ExitCover()
    {
        currentState = MovementState.Grounded;
        isInCover = false;
        coverTransform = null;
    }

    void CoverVault()
    {
        Vector3 vaultDir = transform.forward + Vector3.up;
        rb.linearVelocity = vaultDir * jumpVaultForce;
        currentState = MovementState.Airborne;
    }

    void Vault()
    {
        isVaulting = true;
        Vector3 vaultDir = transform.forward + Vector3.up * 0.5f;
        rb.linearVelocity = vaultDir * jumpVaultForce;
        currentState = MovementState.Airborne;

        StartCoroutine(VaultCooldown());
    }

    IEnumerator VaultCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        isVaulting = false;
    }

    void Jump()
    {
        if (currentState == MovementState.Grounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 8f, rb.linearVelocity.z);
            currentState = MovementState.Airborne;
        }
    }

    void WallRunJump()
    {
        Vector3 jumpDir = transform.up + wallRunDirection * 0.5f;
        rb.linearVelocity = jumpDir * 10f;
        currentState = MovementState.Airborne;
    }

    void StartClimb()
    {
        currentState = MovementState.Climbing;
        isClimbing = true;
        climbTarget = climbHit.transform;
    }

    void HandleClimbing()
    {
        Vector3 climbDir = Vector3.up;
        rb.linearVelocity = climbDir * climbSpeed;
    }

    void HandleLedgeGrab()
    {
        if (ledgeGrabPoint != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, ledgeGrabPoint.position, ledgeClimbSpeed * Time.deltaTime);
        }
    }

    void LedgeClimb()
    {
        currentState = MovementState.Airborne;
        isLedgeGrabbing = false;
        rb.linearVelocity = Vector3.up * 8f;
    }

    void HandleGroundedMovement()
    {
        float targetSpeed = isSprinting ? sprintSpeed : (moveDirection.magnitude > 0.1f ? runSpeed : walkSpeed);
        
        if (isSprinting && moveDirection.magnitude > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = moveDirection.magnitude > 0.1f ? targetSpeed : Mathf.Lerp(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        Vector3 targetDir = GetCameraRelativeDirection();
        
        if (targetDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        rb.linearVelocity = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
    }

    void HandleAirborneMovement()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 airDir = GetCameraRelativeDirection();
            Vector3 currentVel = rb.linearVelocity;
            Vector3 targetVel = airDir * runSpeed;
            
            rb.linearVelocity = Vector3.Lerp(currentVel, targetVel, airControl * Time.deltaTime);
        }

        rb.linearVelocity += Physics.gravity * Time.fixedDeltaTime;
    }

    void HandleSliding()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 slideDir = GetCameraRelativeDirection();
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, slideDir * slideSpeed, slideDeceleration * Time.deltaTime);
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, slideDeceleration * Time.deltaTime);
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
    }

    void HandleRolling()
    {
        float speed = rollSpeed * (rollTimer / rollDuration);
        rb.linearVelocity = transform.forward * speed;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);

        rollTimer -= Time.fixedDeltaTime;
    }

    void HandleCover()
    {
        Vector3 coverDir = transform.forward;
        
        float h = Input.GetAxisRaw("Horizontal");
        
        if (Mathf.Abs(h) > 0.1f)
        {
            transform.position += transform.right * h * coverMoveSpeed * Time.deltaTime;
        }

        transform.position = Vector3.Lerp(transform.position, coverPosition, 10f * Time.deltaTime);
    }

    void HandleWallRun()
    {
        rb.linearVelocity = wallRunDirection * wallRunSpeed;
        rb.linearVelocity += Vector3.up * wallRunGravity;
    }

    Vector3 GetCameraRelativeDirection()
    {
        if (cameraTransform == null) return moveDirection;
        
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();
        
        return (forward * moveDirection.z + right * moveDirection.x).normalized;
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position + Vector3.down * 0.5f, 0.4f, parkourLayers);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float speedPercent = currentSpeed / sprintSpeed;
        animator.SetFloat(speedParam, speedPercent, 0.1f, Time.deltaTime);
        animator.SetBool(jumpParam, currentState == MovementState.Airborne);
        animator.SetBool(climbParam, isClimbing);
        animator.SetBool(wallRunParam, isWallRunning);
        animator.SetBool(rollParam, isRolling);
        animator.SetBool(slideParam, isSliding);
        animator.SetBool(coverParam, isInCover);
    }

    public void SetSprinting(bool sprinting) => isSprinting = sprinting;
    public bool IsWallRunning() => currentState == MovementState.WallRunning;
    public bool IsClimbing() => currentState == MovementState.Climbing;
    public bool IsRolling() => isRolling;
    public bool IsSliding() => isSliding;
    public bool IsInCover() => isInCover;
}