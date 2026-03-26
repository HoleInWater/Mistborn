using UnityEngine;

/// <summary>
/// Extra movement mechanics: swimming, ladders, ziplines.
/// Each is a trigger-zone based system that overrides normal movement.
/// </summary>
public class MovementExtras : MonoBehaviour
{
    // ── Swimming ─────────────────────────────────────────────────────────
    [Header("Swimming")]
    public float swimSpeed = 3f;
    public float swimUpSpeed = 2f;
    public float maxOxygen = 10f;
    public float oxygenDrainRate = 1f;
    public float drowningDamagePerSecond = 15f;

    // ── Ladders ──────────────────────────────────────────────────────────
    [Header("Ladders")]
    public float climbSpeed = 3f;

    // ── Ziplines ─────────────────────────────────────────────────────────
    [Header("Ziplines")]
    public float ziplineSpeed = 12f;
    public float ziplineDismountForce = 5f;

    [Header("References")]
    public Rigidbody playerRb;
    public BasicPlayerMove playerMove;

    // State
    private bool isSwimming = false;
    private bool isClimbing = false;
    private bool isOnZipline = false;
    private float currentOxygen;
    private Transform currentLadder;
    private Transform ziplineStart;
    private Transform ziplineEnd;
    private float ziplineProgress;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (playerMove == null) playerMove = GetComponent<BasicPlayerMove>();
        currentOxygen = maxOxygen;
    }

    void Update()
    {
        if (isSwimming) UpdateSwimming();
        if (isClimbing) UpdateClimbing();
        if (isOnZipline) UpdateZipline();
    }

    // ── Swimming ─────────────────────────────────────────────────────────

    void UpdateSwimming()
    {
        if (playerRb == null) return;

        playerRb.useGravity = false;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float upDown = 0f;
        if (Input.GetKey(Keybinds.Jump)) upDown = 1f;
        if (Input.GetKey(Keybinds.Crouch)) upDown = -1f;

        Vector3 swimDir = (Camera.main.transform.forward * v + Camera.main.transform.right * h + Vector3.up * upDown).normalized;
        playerRb.linearVelocity = swimDir * swimSpeed;

        // Oxygen
        if (transform.position.y < 0f) // Rough underwater check
        {
            currentOxygen -= oxygenDrainRate * Time.deltaTime;
            if (currentOxygen <= 0f)
            {
                IDamageable hp = GetComponent<IDamageable>();
                hp?.TakeDamage(drowningDamagePerSecond * Time.deltaTime);
            }
        }
        else
        {
            currentOxygen = Mathf.Min(currentOxygen + oxygenDrainRate * 2f * Time.deltaTime, maxOxygen);
        }
    }

    // ── Climbing ─────────────────────────────────────────────────────────

    void UpdateClimbing()
    {
        if (playerRb == null || currentLadder == null) return;

        playerRb.useGravity = false;
        float v = Input.GetAxis("Vertical");
        playerRb.linearVelocity = new Vector3(0, v * climbSpeed, 0);

        // Dismount
        if (Input.GetKeyDown(Keybinds.Jump))
        {
            ExitClimb();
            playerRb.AddForce(-currentLadder.forward * 3f + Vector3.up * 3f, ForceMode.Impulse);
        }
    }

    // ── Zipline ──────────────────────────────────────────────────────────

    void UpdateZipline()
    {
        if (ziplineStart == null || ziplineEnd == null) return;

        ziplineProgress += ziplineSpeed * Time.deltaTime / Vector3.Distance(ziplineStart.position, ziplineEnd.position);
        transform.position = Vector3.Lerp(ziplineStart.position, ziplineEnd.position, ziplineProgress);

        if (playerRb != null) playerRb.linearVelocity = Vector3.zero;

        // Auto-dismount at end or on jump
        if (ziplineProgress >= 1f || Input.GetKeyDown(Keybinds.Jump))
        {
            ExitZipline();
        }
    }

    // ── Trigger Zone Entry/Exit ──────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water") && !isSwimming)
            EnterSwim();
        else if (other.CompareTag("Ladder") && !isClimbing)
            EnterClimb(other.transform);
        else if (other.CompareTag("ZiplineStart") && !isOnZipline)
            EnterZipline(other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water") && isSwimming)
            ExitSwim();
    }

    void EnterSwim()
    {
        isSwimming = true;
        if (playerMove != null) playerMove.enabled = false;
    }

    void ExitSwim()
    {
        isSwimming = false;
        if (playerRb != null) playerRb.useGravity = true;
        if (playerMove != null) playerMove.enabled = true;
    }

    void EnterClimb(Transform ladder)
    {
        isClimbing = true;
        currentLadder = ladder;
        if (playerMove != null) playerMove.enabled = false;
    }

    void ExitClimb()
    {
        isClimbing = false;
        currentLadder = null;
        if (playerRb != null) playerRb.useGravity = true;
        if (playerMove != null) playerMove.enabled = true;
    }

    void EnterZipline(Transform startPoint)
    {
        // Find end point (child or linked object)
        Transform endPoint = startPoint.Find("ZiplineEnd");
        if (endPoint == null) return;

        isOnZipline = true;
        ziplineStart = startPoint;
        ziplineEnd = endPoint;
        ziplineProgress = 0f;
        if (playerMove != null) playerMove.enabled = false;
        if (playerRb != null) { playerRb.useGravity = false; playerRb.linearVelocity = Vector3.zero; }
    }

    void ExitZipline()
    {
        isOnZipline = false;
        if (playerRb != null)
        {
            playerRb.useGravity = true;
            Vector3 dir = (ziplineEnd.position - ziplineStart.position).normalized;
            playerRb.AddForce(dir * ziplineDismountForce + Vector3.up * 3f, ForceMode.Impulse);
        }
        if (playerMove != null) playerMove.enabled = true;
        ziplineStart = null;
        ziplineEnd = null;
    }

    // ── Public API ───────────────────────────────────────────────────────
    public bool IsSwimming() => isSwimming;
    public bool IsClimbing() => isClimbing;
    public bool IsOnZipline() => isOnZipline;
    public float GetOxygenPercent() => currentOxygen / maxOxygen;
}
