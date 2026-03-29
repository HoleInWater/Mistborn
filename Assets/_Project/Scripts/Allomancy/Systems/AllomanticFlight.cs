using UnityEngine;

/// <summary>
/// Allomantic flight — the core Mistborn traversal fantasy.
/// Push off metals below to gain altitude, Pull toward metals above to swing.
/// Alternating Push/Pull creates the iconic "steel jumping" locomotion.
///
/// Physics from PHYSICS-MATH-BOOK.md Section 2:
/// F = A × m1 × m2 / r² — anchored metals (buildings, lampposts) are effectively infinite mass,
/// so the Allomancer receives all the force.
///
/// Lore: Vin describes it as "leaping between invisible anchors in the sky."
/// </summary>
public class AllomanticFlight : MonoBehaviour
{
    [Header("Push Launch")]
    public float pushLaunchForce = 20f;
    public float pushLaunchCooldown = 0.3f;
    public float pushMetalDetectRadius = 5f;
    public LayerMask metalLayer;

    [Header("Pull Swing")]
    public float pullSwingForce = 15f;
    public float pullSwingDamping = 0.95f;
    public float maxPullRange = 30f;

    [Header("Continuous Flight")]
    public float hoverForce = 12f;
    public float maxFlightSpeed = 25f;
    public float airControlForce = 8f;
    public float flightMetalDrainRate = 4f;

    [Header("Visual")]
    public LineRenderer pushLine;
    public LineRenderer pullLine;
    public Color pushLineColor = new Color(0.3f, 0.5f, 1f, 0.6f);
    public Color pullLineColor = new Color(0.2f, 0.8f, 1f, 0.6f);

    [Header("References")]
    public Rigidbody playerRb;
    public Allomancer allomancer;
    public Camera playerCamera;

    // State
    private bool isFlying = false;
    private float pushCooldownTimer;
    private Transform currentPullAnchor;
    private Transform lastPushAnchor;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (playerCamera == null) playerCamera = Camera.main;
        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        SetupLines();
    }

    void Update()
    {
        pushCooldownTimer -= Time.deltaTime;

        bool pushHeld = Input.GetKey(Keybinds.SteelPush);
        bool pullHeld = Input.GetKey(Keybinds.IronPull);
        bool hasSteelReserve = allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel) > 0;
        bool hasIronReserve = allomancer != null && allomancer.GetMetalReserve(AllomancySkill.MetalType.Iron) > 0;

        isFlying = !IsGrounded() && (pushHeld || pullHeld);

        // Steel Push — launch off metals below
        if (pushHeld && hasSteelReserve && pushCooldownTimer <= 0f)
        {
            TryPushLaunch();
        }

        // Iron Pull — swing toward metals above/ahead
        if (pullHeld && hasIronReserve)
        {
            TryPullSwing();
        }
        else
        {
            currentPullAnchor = null;
        }

        // Air control while flying
        if (isFlying)
        {
            ApplyAirControl();
            DrainFlightMetal(pushHeld, pullHeld);
        }

        // Hover — sustained push off ground metals
        if (pushHeld && hasSteelReserve && !IsGrounded())
        {
            TryHover();
        }

        UpdateVisualLines();
    }

    // ── Push Launch ──────────────────────────────────────────────────────

    void TryPushLaunch()
    {
        // Find metal below or behind to push off
        Collider[] metals = Physics.OverlapSphere(transform.position, pushMetalDetectRadius, metalLayer);
        Transform bestAnchor = null;
        float bestScore = 0f;

        foreach (var col in metals)
        {
            if (col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic) continue; // Skip loose metals

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float belowness = transform.position.y - col.transform.position.y; // Prefer metals below

            float score = belowness / Mathf.Max(dist, 0.5f);
            if (score > bestScore)
            {
                bestScore = score;
                bestAnchor = col.transform;
            }
        }

        if (bestAnchor != null)
        {
            pushCooldownTimer = pushLaunchCooldown;
            lastPushAnchor = bestAnchor;

            // F = A × m1 × m2 / r² — anchored, so all force goes to player
            float dist = Vector3.Distance(transform.position, bestAnchor.position);
            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float force = AllomancyPhysicsFormulas.CalculateAllomanticForce(
                AllomancyPhysicsFormulas.A_CONSERVATIVE * flare,
                playerRb.mass, playerRb.mass * 10f, dist);

            force = Mathf.Min(force, pushLaunchForce * flare);

            Vector3 launchDir = (transform.position - bestAnchor.position).normalized;
            playerRb.AddForce(launchDir * force, ForceMode.Impulse);

            SoundManager.Instance?.PlayPushSound();
            CameraShakeManager.Instance?.Shake(0.1f, 0.05f);
        }
    }

    // ── Pull Swing ───────────────────────────────────────────────────────

    void TryPullSwing()
    {
        if (currentPullAnchor == null)
        {
            if (playerCamera == null) return;
            // Find metal in camera direction
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.6f, 0)); // Slightly above center
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxPullRange, metalLayer))
            {
                // Prefer anchored metals
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb == null || rb.isKinematic)
                    currentPullAnchor = hit.transform;
            }
        }

        if (currentPullAnchor != null)
        {
            float dist = Vector3.Distance(transform.position, currentPullAnchor.position);
            if (dist > maxPullRange)
            {
                currentPullAnchor = null;
                return;
            }

            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float force = AllomancyPhysicsFormulas.CalculateAllomanticForce(
                AllomancyPhysicsFormulas.A_CONSERVATIVE * flare,
                playerRb.mass, playerRb.mass * 10f, dist);

            force = Mathf.Min(force, pullSwingForce * flare);

            Vector3 pullDir = (currentPullAnchor.position - transform.position).normalized;
            playerRb.AddForce(pullDir * force, ForceMode.Force);

            // Damping to prevent oscillation
            playerRb.linearVelocity *= pullSwingDamping;

            // Detach when close
            if (dist < 2f)
                currentPullAnchor = null;
        }
    }

    // ── Hover ────────────────────────────────────────────────────────────

    void TryHover()
    {
        // Push off ground metals to maintain altitude
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, pushMetalDetectRadius, metalLayer))
        {
            float dist = hit.distance;
            float upForce = hoverForce / Mathf.Max(dist, 0.5f);

            // Only hover, don't accelerate upward past a point
            if (playerRb.linearVelocity.y < 2f)
                playerRb.AddForce(Vector3.up * upForce, ForceMode.Force);
        }
    }

    // ── Air Control ──────────────────────────────────────────────────────

    void ApplyAirControl()
    {
        if (playerCamera == null) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f) return;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0; forward.Normalize();
        right.y = 0; right.Normalize();

        Vector3 controlDir = (forward * v + right * h).normalized;
        playerRb.AddForce(controlDir * airControlForce, ForceMode.Force);

        // Speed cap
        Vector3 horizontalVel = playerRb.linearVelocity;
        horizontalVel.y = 0;
        if (horizontalVel.magnitude > maxFlightSpeed)
        {
            horizontalVel = horizontalVel.normalized * maxFlightSpeed;
            playerRb.linearVelocity = new Vector3(horizontalVel.x, playerRb.linearVelocity.y, horizontalVel.z);
        }
    }

    void DrainFlightMetal(bool pushing, bool pulling)
    {
        if (allomancer == null) return;
        float drain = flightMetalDrainRate * Time.deltaTime;
        if (pushing) allomancer.DrainMetal(AllomancySkill.MetalType.Steel, drain);
        if (pulling) allomancer.DrainMetal(AllomancySkill.MetalType.Iron, drain);
    }

    // ── Visual Lines ─────────────────────────────────────────────────────

    void SetupLines()
    {
        if (pushLine == null) pushLine = CreateLine("PushLine", pushLineColor);
        if (pullLine == null) pullLine = CreateLine("PullLine", pullLineColor);
    }

    LineRenderer CreateLine(string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.startWidth = 0.03f;
        lr.endWidth = 0.015f;
        lr.material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
        lr.startColor = color;
        lr.endColor = color * 0.5f;
        lr.positionCount = 0;
        return lr;
    }

    void UpdateVisualLines()
    {
        // Push line
        if (lastPushAnchor != null && Input.GetKey(Keybinds.SteelPush))
        {
            pushLine.positionCount = 2;
            pushLine.SetPosition(0, transform.position);
            pushLine.SetPosition(1, lastPushAnchor.position);
        }
        else
        {
            pushLine.positionCount = 0;
        }

        // Pull line
        if (currentPullAnchor != null)
        {
            pullLine.positionCount = 2;
            pullLine.SetPosition(0, transform.position);
            pullLine.SetPosition(1, currentPullAnchor.position);
        }
        else
        {
            pullLine.positionCount = 0;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }

    // ── Public API ───────────────────────────────────────────────────────
    public bool IsFlying() => isFlying;
    public Transform GetPullAnchor() => currentPullAnchor;
    public Transform GetLastPushAnchor() => lastPushAnchor;
}
