using UnityEngine;
using System.Collections;

/// <summary>
/// Mistborn parkour system: vault, mantle, slide, and Allomancy-assisted traversal.
/// Designed for rooftop-to-rooftop movement in Luthadel.
/// </summary>
public class ParkourSystem : MonoBehaviour
{
    // ── Vault ────────────────────────────────────────────────────────────
    [Header("Vault")]
    public float vaultCheckDistance = 1.5f;
    public float vaultHeight = 1.2f;
    public float vaultSpeed = 6f;
    public float vaultCooldown = 0.5f;

    // ── Mantle ───────────────────────────────────────────────────────────
    [Header("Mantle (Ledge Grab)")]
    public float mantleCheckHeight = 2.5f;
    public float mantleSpeed = 4f;
    public float mantleUpForce = 8f;

    // ── Slide ────────────────────────────────────────────────────────────
    [Header("Slide")]
    public float slideSpeed = 10f;
    public float slideDuration = 0.8f;
    public float slideDecay = 0.7f;
    public float slideCooldown = 1f;
    public float slideColliderHeight = 0.5f;

    // ── Steel Launch ─────────────────────────────────────────────────────
    [Header("Steel Push Launch — PHYSICS-MATH-BOOK.md")]
    [Tooltip("Push off a coin on the ground to launch into the air")]
    public float steelLaunchForce = 25f;
    public float steelLaunchCooldown = 1.5f;

    [Header("References")]
    public Rigidbody playerRb;
    public CapsuleCollider playerCollider;
    public BasicPlayerMove playerMove;
    public Allomancer allomancer;
    public Animator animator;
    public Camera playerCamera;

    // State
    private bool isVaulting = false;
    private bool isMantling = false;
    private bool isSliding = false;
    private float vaultTimer = 0f;
    private float slideTimer = 0f;
    private float steelLaunchTimer = 0f;
    private float originalColliderHeight;
    private float originalColliderCenter;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody>();
        if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider>();
        if (playerMove == null) playerMove = GetComponent<BasicPlayerMove>();
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerCamera == null) playerCamera = Camera.main;

        if (playerCollider != null)
        {
            originalColliderHeight = playerCollider.height;
            originalColliderCenter = playerCollider.center.y;
        }
    }

    void Update()
    {
        if (vaultTimer > 0) vaultTimer -= Time.deltaTime;
        if (slideTimer > 0) slideTimer -= Time.deltaTime;
        if (steelLaunchTimer > 0) steelLaunchTimer -= Time.deltaTime;

        if (isVaulting || isMantling || isSliding) return;

        // Auto-vault when running toward low obstacle
        if (playerMove != null && playerMove.IsGrounded() && IsMovingForward())
        {
            if (CanVault())
                StartCoroutine(Vault());
            else if (CanMantle())
                StartCoroutine(Mantle());
        }

        // Slide: sprint + crouch
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift)
            && playerMove.IsGrounded() && slideTimer <= 0f)
        {
            StartCoroutine(Slide());
        }

        // Steel Push launch: push off ground coin for vertical boost
        if (Input.GetKeyDown(KeyCode.Space) && !playerMove.IsGrounded()
            && steelLaunchTimer <= 0f && allomancer != null
            && allomancer.IsMetalBurning(AllomancySkill.MetalType.Steel))
        {
            SteelPushLaunch();
        }
    }

    // ── Vault ────────────────────────────────────────────────────────────

    bool CanVault()
    {
        if (vaultTimer > 0f) return false;

        // Check for low obstacle ahead
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        RaycastHit hit;
        if (Physics.Raycast(origin, transform.forward, out hit, vaultCheckDistance))
        {
            // Check if we can clear it (nothing above vault height)
            Vector3 aboveObstacle = hit.point + Vector3.up * vaultHeight;
            if (!Physics.Raycast(aboveObstacle, Vector3.down, vaultHeight * 0.5f))
                return true;
        }
        return false;
    }

    IEnumerator Vault()
    {
        isVaulting = true;
        vaultTimer = vaultCooldown;
        animator?.SetTrigger("Vault");

        Vector3 startPos = transform.position;
        Vector3 overPos = startPos + transform.forward * vaultCheckDistance + Vector3.up * vaultHeight;
        Vector3 landPos = overPos + transform.forward * 1f - Vector3.up * vaultHeight;

        // Arc over the obstacle
        if (vaultSpeed <= 0f) yield break;
        float duration = vaultCheckDistance * 2f / vaultSpeed;
        float elapsed = 0f;

        playerRb.isKinematic = true;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos;
            if (t < 0.5f)
                pos = Vector3.Lerp(startPos, overPos, t * 2f);
            else
                pos = Vector3.Lerp(overPos, landPos, (t - 0.5f) * 2f);

            transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerRb.isKinematic = false;
        isVaulting = false;
    }

    // ── Mantle ───────────────────────────────────────────────────────────

    bool CanMantle()
    {
        // Check for ledge above (high obstacle that can be climbed)
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;
        if (Physics.Raycast(origin, transform.forward, out hit, vaultCheckDistance))
        {
            float obstacleHeight = hit.collider.bounds.max.y - transform.position.y;
            if (obstacleHeight > vaultHeight && obstacleHeight < mantleCheckHeight)
            {
                // Check if top is flat (can stand on it)
                Vector3 topCheck = hit.point + Vector3.up * obstacleHeight + transform.forward * 0.3f;
                if (Physics.Raycast(topCheck, Vector3.down, 1f))
                    return true;
            }
        }
        return false;
    }

    IEnumerator Mantle()
    {
        isMantling = true;
        animator?.SetTrigger("Mantle");

        // Pull up to ledge
        Vector3 startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * mantleCheckHeight;
        Vector3 forwardPos = upPos + transform.forward * 1f;

        if (mantleSpeed <= 0f) { isMantling = false; yield break; }

        playerRb.isKinematic = true;

        // Phase 1: Pull up
        float elapsed = 0f;
        float upDuration = mantleCheckHeight / mantleSpeed;
        while (elapsed < upDuration)
        {
            transform.position = Vector3.Lerp(startPos, upPos, elapsed / upDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Pull forward onto ledge
        elapsed = 0f;
        float fwdDuration = 0.3f;
        while (elapsed < fwdDuration)
        {
            transform.position = Vector3.Lerp(upPos, forwardPos, elapsed / fwdDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerRb.isKinematic = false;
        isMantling = false;
    }

    // ── Slide ────────────────────────────────────────────────────────────

    IEnumerator Slide()
    {
        isSliding = true;
        slideTimer = slideCooldown;
        animator?.SetBool("IsSliding", true);

        // Shrink collider
        if (playerCollider != null)
        {
            playerCollider.height = slideColliderHeight;
            playerCollider.center = new Vector3(0, slideColliderHeight * 0.5f, 0);
        }

        float elapsed = 0f;
        Vector3 slideDir = transform.forward;
        float currentSpeed = slideSpeed;

        while (elapsed < slideDuration)
        {
            currentSpeed *= slideDecay;
            playerRb.MovePosition(playerRb.position + slideDir * currentSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;

            // Cancel slide if player releases crouch
            if (!Input.GetKey(KeyCode.LeftControl)) break;
            yield return null;
        }

        // Restore collider
        if (playerCollider != null)
        {
            playerCollider.height = originalColliderHeight;
            playerCollider.center = new Vector3(0, originalColliderCenter, 0);
        }

        animator?.SetBool("IsSliding", false);
        isSliding = false;
    }

    // ── Steel Push Launch ────────────────────────────────────────────────

    void SteelPushLaunch()
    {
        steelLaunchTimer = steelLaunchCooldown;

        // Lore: Push off a coin dropped below you, using F = A × m1 × m2 / r²
        // At close range (r≈1), with a coin on the ground (anchored by ground mass),
        // the player gets launched upward
        float A = AllomancyPhysicsFormulas.A_CONSERVATIVE;
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float force = steelLaunchForce * flare;

        playerRb.AddForce(Vector3.up * force, ForceMode.Impulse);
        allomancer?.DrainMetal(AllomancySkill.MetalType.Steel, 3f);

        CameraShakeManager.Instance?.Shake(0.2f, 0.15f);
        SoundManager.Instance?.PlayPushSound();

        animator?.SetTrigger("SteelLaunch");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    bool IsMovingForward()
    {
        return Input.GetKey(KeyCode.W) && playerRb.linearVelocity.magnitude > 2f;
    }

    public bool IsVaulting() => isVaulting;
    public bool IsMantling() => isMantling;
    public bool IsSliding() => isSliding;
    public bool IsPerformingParkour() => isVaulting || isMantling || isSliding;
}
