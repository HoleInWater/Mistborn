/* SteelPush.cs
 *
 * PURPOSE:
 * Implements the Steel Allomancy ability (Coinshot) – push metal objects away.
 *
 * FLARE INTEGRATION:
 * ==================
 * Uses the single shared FlareManager intensity (1–10, scroll wheel).
 * Left Ctrl starts burning; E executes a push while burning is active.
 * Force scales smoothly with FlareManager.FlareMultiplier.
 *
 * CONTROLS:
 * - Left Ctrl    → Toggle burning ON / OFF (via FlareManager)
 * - Scroll wheel → Adjust shared intensity 1–10 (via FlareManager)
 * - E key        → Execute push (requires burning to be active)
 * - F key        → Steel bubble radial push (requires burning)
 * - E release    → Stop burning Steel locally
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SteelPush : MonoBehaviour
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Normalised 0–1 flare level from shared intensity.</summary>
    private float FlareLevel =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning
            ? (float)(FlareManager.Instance.Intensity - 1) / (FlareManager.Instance.maxIntensitySteps - 1)
            : 0f;

    /// <summary>Force multiplier from shared intensity.</summary>
    private float CurrentFlareMultiplier =>
        FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

    /// <summary>True when burning is active.</summary>
    private bool IsFlaring =>
        FlareManager.Instance != null && FlareManager.Instance.IsBurning;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Push Settings")]
    public float pushForce         = 800f;
    public float referenceMass     = 80f;
    public float referenceDistance = 3f;
    public float minDistance       = 1f;
    public float maxRange          = 30f;
    public float metalCostPerSecond= 2f;
    public float pushCooldown      = 0.2f;

    [Header("Allomancy Physics")]
    public float allomanticStrength = 1000f;
    public float maxCoinVelocity    = 400f;
    [Range(1f, 2f)] public float distanceExponent = 1f;
    [Range(0f, 1f)] public float velocityDamping  = 0.5f;

    [Header("Flare Scaling")]
    [Tooltip("Metal cost multiplier at full intensity.")]
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;

    [Tooltip("Skill mastery bonus.")]
    [Range(1f, 2f)]
    public float masteryBonus = 1f;

    [Header("References")]
    public Camera     playerCamera;
    public LayerMask  metalLayer;
    public Allomancer allomancer;
    public Rigidbody  playerRigidbody;
    public Transform  chestTransform;

    [Header("Visual Effects")]
    public float shakeMagnitude      = 0.1f;
    public float shakeDuration       = 0.1f;
    public float shakeForceThreshold = 100f;
    public bool  enablePushScreenTint = true;
    public Color weakPushTint   = new Color(0f, 1f, 0f, 0.1f);
    public Color mediumPushTint = new Color(1f, 1f, 0f, 0.2f);
    public Color strongPushTint = new Color(1f, 0f, 0f, 0.3f);
    public float pushTintDuration = 0.2f;

    [Header("Flaring Vignette")]
    public UnityEngine.UI.Image vignetteImage;
    public Color flaringColor        = new Color(1f, 0.2f, 0f, 0.3f);
    public float vignettePulseDuration = 0.5f;
    public float vignetteMaxAlpha    = 0.3f;

    [Header("UI")]
    public UnityEngine.UI.Image crosshairImage;
    public Color metalInRangeColor = Color.green;
    public Color noMetalColor      = Color.white;

    [Header("Push Prediction")]
    public bool  enablePushPrediction  = true;
    public Color predictionColor       = new Color(1f, 1f, 0f, 0.5f);
    public int   predictionPoints      = 20;
    public float predictionTimeStep    = 0.1f;
    public bool  showPredictionOnHold  = true;

    [Header("Steel Bubble")]
    public bool    enableSteelBubble             = true;
    public KeyCode steelBubbleKey                = KeyCode.F;
    public float   steelBubbleRadius             = 2.5f;
    public float   steelBubbleForce              = 50f;
    public float   steelBubbleCooldown           = 0.5f;
    public float   steelBubbleMetalCostMultiplier= 1.5f;

    [Header("Impulse Mode")]
    public float impulseMassThreshold = 5f;
    public float impulseCalibration   = 0.000917f;

    [Header("Debug")]
    public bool debugPushOperations = false;

    // ── Private State ─────────────────────────────────────────────────────────
    private bool  isBurning              = false;
    private bool  eKeyWasPressed         = false;
    private bool  bubbleAppliedThisPress = false;
    private float cooldownTimer          = 0f;
    private float steelBubbleCooldownTimer = 0f;

    private RaycastHit       currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody        currentTargetRigidbody;
    private bool             hasCurrentTarget = false;
    private bool             metalInRange     = false;

    private Coroutine    vignetteCoroutine;
    private Coroutine    pushTintCoroutine;
    private Color        currentPushTint = Color.clear;
    private LineRenderer predictionLine;
    private bool         isPredictionActive = false;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerCamera    == null) playerCamera    = Camera.main;
        if (allomancer      == null) allomancer      = GetComponentInParent<Allomancer>();

        if (chestTransform == null)
        {
            Transform p = GetComponentInParent<Transform>();
            if (p != null)
            {
                Transform chest = p.Find("Chest") ?? p.Find("ChestBone")
                               ?? p.Find("Spine2") ?? p.Find("Torso");
                chestTransform = chest != null ? chest : p;
            }
        }

        CreatePredictionLine();
    }

    void Update()
    {
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }

        if (cooldownTimer > 0f)            cooldownTimer            -= Time.deltaTime;
        if (steelBubbleCooldownTimer > 0f) steelBubbleCooldownTimer -= Time.deltaTime;

        UpdateTargetedMetal();

        bool eDown = Input.GetKeyDown(KeyCode.E);
        bool eUp   = Input.GetKeyUp(KeyCode.E);

        if (eDown && !eKeyWasPressed && cooldownTimer <= 0f && IsFlaring)
        {
            eKeyWasPressed = true;
            if (!isBurning) StartBurning();
            PushMetals();
            DrainMetal(flaringMetalCostMultiplier);
            StartFlaringVignette();
        }

        if (eUp)
        {
            eKeyWasPressed = false;
            StopBurning();
        }

        if (isBurning) DrainMetal(1f);

        if (enableSteelBubble)
        {
            if (Input.GetKeyDown(steelBubbleKey) && IsFlaring
                && steelBubbleCooldownTimer <= 0f && !bubbleAppliedThisPress)
            {
                if (!isBurning) StartBurning();
                PushMetalsInBubble();
                DrainMetal(steelBubbleMetalCostMultiplier);
                steelBubbleCooldownTimer = steelBubbleCooldown;
                bubbleAppliedThisPress   = true;
            }
            if (Input.GetKeyUp(steelBubbleKey))
                bubbleAppliedThisPress = false;
        }

        UpdatePrediction();
        UpdateCrosshairColor();
    }

    // ── Burning ───────────────────────────────────────────────────────────────

    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        allomancer?.StartBurning(AllomancySkill.MetalType.Steel);
    }

    void StopBurning()
    {
        if (!isBurning) return;
        isBurning     = false;
        cooldownTimer = pushCooldown;
        allomancer?.StopBurning();
    }

    // ── Target Detection ──────────────────────────────────────────────────────

    void UpdateTargetedMetal()
    {
        hasCurrentTarget = false; currentTarget = null;
        currentTargetRigidbody = null; metalInRange = false;

        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out currentTargetHit, maxRange, metalLayer))
        {
            Rigidbody rb = currentTargetHit.rigidbody;
            if (rb != null && rb != playerRigidbody)
            {
                currentTargetRigidbody = rb;
                currentTarget          = currentTargetHit.collider.GetComponent<AllomanticTarget>();
                hasCurrentTarget       = true;
                metalInRange           = true;
            }
        }
    }

    // ── Push ──────────────────────────────────────────────────────────────────

    void PushMetals()
    {
        if (playerRigidbody == null || !hasCurrentTarget || currentTargetRigidbody == null) return;

        Rigidbody        targetRb  = currentTargetRigidbody;
        AllomanticTarget target    = currentTarget;

        if (targetRb == playerRigidbody)           return;
        if (target != null && !target.canBePushed) return;

        float   distance   = Vector3.Distance(playerRigidbody.position, targetRb.position);
        Vector3 dir        = targetRb.position - playerRigidbody.position;
        bool    isAnchored = (target != null && target.isAnchored) || targetRb.isKinematic;

        // Scale strength by shared intensity multiplier
        float strength = allomanticStrength
                       * (playerRigidbody.mass / referenceMass)
                       * masteryBonus
                       * CurrentFlareMultiplier;

        float eff           = Mathf.Max(distance, minDistance);
        float distanceFactor= Mathf.Pow(referenceDistance / eff, distanceExponent);

        Vector3 targetVel      = targetRb.linearVelocity;
        float   velAway        = Vector3.Dot(targetVel, dir.normalized);
        float   velDampFactor  = 1f;
        if (velAway > 0)
            velDampFactor = 1f - Mathf.Clamp01(velAway / maxCoinVelocity) * velocityDamping;

        float force = strength * distanceFactor * velDampFactor;

        if (debugPushOperations)
            Debug.Log($"[PUSH] Intensity={FlareManager.Instance?.Intensity} Multiplier={CurrentFlareMultiplier:F2} Force={force:F0}");

        if (isAnchored)
            playerRigidbody.AddForce(-dir.normalized * force);
        else if (force > 1f && targetVel.magnitude < maxCoinVelocity)
            targetRb.AddForce(dir.normalized * force, ForceMode.Impulse);

        if (force > shakeForceThreshold)
        {
            ShakeCamera(shakeMagnitude * Mathf.Clamp01(FlareLevel + 0.3f));
            TriggerPushTint(force);
        }
    }

    void PushMetalsInBubble()
    {
        if (playerRigidbody == null) return;

        foreach (Collider col in Physics.OverlapSphere(playerRigidbody.position, steelBubbleRadius, metalLayer))
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb == playerRigidbody) continue;

            AllomanticTarget target = col.GetComponent<AllomanticTarget>();
            if (target != null && !target.canBePushed) continue;

            float   force      = steelBubbleForce * CurrentFlareMultiplier;
            Vector3 dir        = (rb.position - playerRigidbody.position).normalized;
            bool    isAnchored = (target != null && target.isAnchored) || rb.isKinematic;

            if (isAnchored) playerRigidbody.AddForce(-dir * force * Time.deltaTime);
            else            rb.AddForce(dir * force, ForceMode.Impulse);

            TriggerPushTint(force);
        }
    }

    // ── Metal Drain ───────────────────────────────────────────────────────────

    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;
        float flareCostScale = Mathf.Lerp(1f, flaringMetalCostMultiplier, FlareLevel);
        float drain          = metalCostPerSecond * Time.deltaTime * multiplier * flareCostScale;
        allomancer.DrainMetal(AllomancySkill.MetalType.Steel, drain);
    }

    // ── Prediction ────────────────────────────────────────────────────────────

    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PushPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();
        Shader shader  = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        predictionLine.material      = new Material(shader);
        predictionLine.startColor    = predictionColor;
        predictionLine.endColor      = predictionColor;
        predictionLine.startWidth    = 0.03f;
        predictionLine.endWidth      = 0.01f;
        predictionLine.positionCount = predictionPoints;
        predictionLine.useWorldSpace = true;
        predictionLine.gameObject.SetActive(false);
    }

    void UpdatePrediction()
    {
        bool isPushing    = Input.GetKey(KeyCode.E);
        bool shouldShow   = enablePushPrediction && showPredictionOnHold
                         && isPushing && isBurning
                         && hasCurrentTarget && currentTarget != null && currentTarget.canBePushed;

        if (shouldShow) DrawPredictionLine();
        else if (isPredictionActive)
        {
            predictionLine.gameObject.SetActive(false);
            isPredictionActive = false;
        }
    }

    void DrawPredictionLine()
    {
        if (predictionLine == null || currentTargetRigidbody == null) return;

        float   targetMass = currentTarget != null
            ? currentTarget.GetEffectiveMass() : currentTargetRigidbody.mass;
        float   force      = pushForce * (playerRigidbody.mass / referenceMass) * CurrentFlareMultiplier;
        float   dist       = currentTargetHit.distance;

        if (dist > 0.01f && dist <= maxRange)
            force *= Mathf.Min(referenceDistance / Mathf.Max(dist, minDistance), 2f);
        else if (dist > maxRange)
            force = 0f;

        Vector3 dir = (currentTargetRigidbody.position - playerRigidbody.position).normalized;
        Vector3 initVel = targetMass <= impulseMassThreshold
            ? dir * (force * impulseCalibration / targetMass)
            : dir * (force / targetMass) * 0.1f;

        Vector3[] points  = new Vector3[predictionPoints];
        Vector3   pos     = currentTargetRigidbody.position;
        Vector3   vel     = initVel;
        for (int i = 0; i < predictionPoints; i++)
        {
            points[i] = pos;
            pos += vel * predictionTimeStep;
            vel += Physics.gravity * predictionTimeStep;
        }

        float speed = initVel.magnitude;
        predictionLine.startColor    = Color.cyan;
        predictionLine.endColor      = speed < 10f ? Color.green : speed < 30f ? Color.yellow : Color.red;
        predictionLine.positionCount = predictionPoints;
        predictionLine.SetPositions(points);
        predictionLine.gameObject.SetActive(true);
        isPredictionActive = true;
    }

    // ── Visual Helpers ────────────────────────────────────────────────────────

    void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        crosshairImage.color = metalInRange ? metalInRangeColor : noMetalColor;
    }

    void TriggerPushTint(float force)
    {
        if (!enablePushScreenTint) return;
        if (pushTintCoroutine != null) StopCoroutine(pushTintCoroutine);
        pushTintCoroutine = StartCoroutine(PushTintCoroutine(force));
    }

    IEnumerator PushTintCoroutine(float force)
    {
        Color tint = force < pushForce * 0.3f ? weakPushTint
                   : force < pushForce * 0.7f ? mediumPushTint
                   : strongPushTint;
        float elapsed = 0f;
        while (elapsed < pushTintDuration)
        {
            float a     = Mathf.Lerp(tint.a, 0f, elapsed / pushTintDuration);
            currentPushTint = new Color(tint.r, tint.g, tint.b, a);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentPushTint   = Color.clear;
        pushTintCoroutine = null;
    }

    void StartFlaringVignette()
    {
        if (vignetteImage == null) return;
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(VignettePulseCoroutine());
    }

    IEnumerator VignettePulseCoroutine()
    {
        vignetteImage.gameObject.SetActive(true);
        float maxAlpha = vignetteMaxAlpha * CurrentFlareMultiplier;
        float elapsed  = 0f;
        while (elapsed < vignettePulseDuration)
        {
            float t = elapsed / vignettePulseDuration;
            Color c = flaringColor;
            c.a     = Mathf.Sin(t * Mathf.PI) * maxAlpha;
            vignetteImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }
        vignetteImage.gameObject.SetActive(false);
        vignetteCoroutine = null;
    }

    void ShakeCamera(float magnitude)
    {
        if (playerCamera == null || magnitude <= 0f) return;
        StartCoroutine(ShakeCoroutine(magnitude));
    }

    IEnumerator ShakeCoroutine(float magnitude)
    {
        Vector3 orig    = playerCamera.transform.localPosition;
        float   elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            playerCamera.transform.localPosition = orig
                + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = orig;
    }

    void OnGUI()
    {
        if (currentPushTint.a > 0.01f)
        {
            GUI.color = currentPushTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (playerRigidbody == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerRigidbody.position, maxRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerRigidbody.position, referenceDistance);
        if (enableSteelBubble)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(playerRigidbody.position, steelBubbleRadius);
        }
    }

    void OnDestroy()
    {
        if (predictionLine != null) Destroy(predictionLine.gameObject);
    }
}
