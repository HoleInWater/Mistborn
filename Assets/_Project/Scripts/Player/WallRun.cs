using UnityEngine;

/// <summary>
/// Wall running system. Player can run along walls when sprinting near them.
/// Enhanced by Steel Pushing off wall anchors for extended runs.
/// Uses physics-based gravity reduction during wall contact.
/// </summary>
public class WallRun : MonoBehaviour
{
    [Header("Wall Run Settings")]
    public float wallRunSpeed = 8f;
    public float maxWallRunDuration = 1.5f;
    public float wallRunGravity = 2f;
    public float wallCheckDistance = 0.7f;
    public float minSpeedToWallRun = 4f;
    public float wallJumpForce = 10f;
    public float wallJumpUpForce = 6f;

    [Header("Camera")]
    public float wallRunTiltAngle = 15f;
    public float tiltSpeed = 8f;

    [Header("Detection")]
    public LayerMask wallMask = ~0;

    [Header("References")]
    public Rigidbody playerRb;
    public BasicPlayerMove playerMove;
    public Animator animator;
    public Transform cameraTransform;

    private bool isWallRunning = false;
    private bool wallOnRight = false;
    private float wallRunTimer = 0f;
    private Vector3 wallNormal;
    private float currentTilt = 0f;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (playerMove == null) playerMove = GetComponent<BasicPlayerMove>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        CheckWall();

        if (isWallRunning)
        {
            wallRunTimer += Time.deltaTime;
            if (wallRunTimer >= maxWallRunDuration || playerMove.IsGrounded())
            {
                StopWallRun();
            }
            else
            {
                PerformWallRun();
            }

            // Wall jump
            if (Input.GetKeyDown(Keybinds.Jump))
            {
                WallJump();
            }
        }

        UpdateCameraTilt();

        if (animator != null)
            animator.SetBool("IsWallRunning", isWallRunning);
    }

    void CheckWall()
    {
        if (isWallRunning || playerMove.IsGrounded()) return;

        // Need to be moving fast enough
        Vector3 horizontalVel = playerRb.linearVelocity;
        horizontalVel.y = 0;
        if (horizontalVel.magnitude < minSpeedToWallRun) return;

        // Check right wall
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallMask))
        {
            StartWallRun(hit.normal, true);
            return;
        }

        // Check left wall
        if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallMask))
        {
            StartWallRun(hit.normal, false);
        }
    }

    void StartWallRun(Vector3 normal, bool rightSide)
    {
        isWallRunning = true;
        wallOnRight = rightSide;
        wallNormal = normal;
        wallRunTimer = 0f;

        // Reduce gravity
        playerRb.useGravity = false;
    }

    void PerformWallRun()
    {
        // Move along wall
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if ((transform.forward - wallForward).magnitude > (transform.forward - (-wallForward)).magnitude)
            wallForward = -wallForward;

        // Apply velocity along wall
        playerRb.linearVelocity = new Vector3(
            wallForward.x * wallRunSpeed,
            -wallRunGravity, // Slow fall
            wallForward.z * wallRunSpeed
        );

        // Stick to wall
        playerRb.AddForce(-wallNormal * 20f, ForceMode.Force);

        // Verify still near wall
        RaycastHit hit;
        Vector3 checkDir = wallOnRight ? transform.right : -transform.right;
        if (!Physics.Raycast(transform.position, checkDir, out hit, wallCheckDistance * 1.5f, wallMask))
        {
            StopWallRun();
        }
    }

    void WallJump()
    {
        StopWallRun();

        // Jump away from wall and up
        Vector3 jumpDir = wallNormal * wallJumpForce + Vector3.up * wallJumpUpForce;
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        playerRb.AddForce(jumpDir, ForceMode.Impulse);

        animator?.SetTrigger("WallJump");
    }

    void StopWallRun()
    {
        isWallRunning = false;
        playerRb.useGravity = true;
    }

    void UpdateCameraTilt()
    {
        if (cameraTransform == null) return;

        float targetTilt = 0f;
        if (isWallRunning)
            targetTilt = wallOnRight ? -wallRunTiltAngle : wallRunTiltAngle;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        Vector3 euler = cameraTransform.localEulerAngles;
        euler.z = currentTilt;
        cameraTransform.localEulerAngles = euler;
    }

    public bool IsWallRunning() => isWallRunning;
    public bool IsWallOnRight() => wallOnRight;
}
