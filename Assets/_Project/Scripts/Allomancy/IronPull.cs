/* IronPull.cs
 * 
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) - pull metal objects toward the player.
 * Requires burning Iron metal to activate.
 * 
 * CONTROLS:
 * ==========
 * This script implements a PER-METAL FLARE SYSTEM where each metal has independent
 * flare state. This allows the player to flare only Iron while Steel remains unflared.
 * 
 * CONTROL SCHEME:
 * ---------------
 * 1. Press Q once (first press)  → Start burning Iron metal
 *    - Iron starts burning, isFlaring = false (not flared yet)
 *    - Blue visual lines appear on metal targets
 * 
 * 2. Press Q again (second press) → Toggle Iron flare ON/OFF
 *    - isFlaring flips from false→true or true→false
 *    - While isFlaring = true, pull force is multiplied by 2x
 *    - Visual vignette effect plays when flaring activates
 *    - Can press Q a third time to pull, or press Q again to toggle flare off
 * 
 * 3. Press Q while isFlaring=true (third press) → Execute PULL
 *    - PullMetals() is called
 *    - Metal cost drained at flaring rate (3x normal)
 *    - Single impulse applied per press
 * 
 * 4. Release Q key              → Stop burning Iron
 *    - isBurning = false
 *    - isFlaring state is PRESERVED (remembers if it was flared)
 *    - Ready for next sequence
 * 
 * ALTERNATIVE FLARE CONTROL:
 * --------------------------
 * - Press Ctrl at ANY time → Toggle Iron flare (independent of Q key)
 *   This allows flaring without triggering a pull.
 * 
 * FLARE STATE MACHINE:
 * --------------------
 * State transitions:
 *   [Idle] ──Q press──> [Burning, not flared] ──Q press──> [Burning, flared] ──Q press──> [Pull executed]
 *       ^                                                                      │              │
 *       └────────────────────────── Release Q ─────────────────────────────────┘              │
 *                                                                                                │
 *       [Not Burning] <────────── Release Q ────────────────────────────────────────────────────┘
 * 
 * FLARE VS NO-FLARE COMPARISON:
 * -----------------------------
 * | State       | Force Multiplier | Metal Cost | Use Case                |
 * |-------------|------------------|------------|--------------------------|
 * | Not Flared  | 1x               | 1x         | Normal precision pulls   |
 * | Flared      | 2x               | 3x         | Heavy objects, emergency |
 * 
 * LORE ACCURACY:
 * - Push/Pull originates from the allomancer's chest (center mass)
 * - Blue lines (blue haze) appear on metal targets within range
 * - Flaring intensifies the effect and consumes more metal
 * - Lurchers can feel metal in the world (weight/distance sense)
 * 
 * METAL DETECTION:
 * - Raycasts from camera center to detect metal objects on "Metal" layer
 * - Objects must have Rigidbody component to be pulled
 * - AllomanticTarget component provides additional control (canBePulled, isAnchored)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IronPull : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base force applied when pulling. Needs calibration for desired velocities.")]
    public float pullForce = 800f;
    [Tooltip("Reference mass for force calculation (average human = 80kg).")]
    public float referenceMass = 80f;
    [Tooltip("Reference distance where force factor = 1.")]
    public float referenceDistance = 3f;
    [Tooltip("Minimum distance to prevent unrealistic forces at close range.")]
    public float minDistance = 1f;
    public float maxRange = 30f;
    public float metalCostPerSecond = 2f;
    
    [Header("Allomancy Physics (Lore-Accurate Model)")]
    [Tooltip("Base allomantic strength (determines max force AND max velocity)")]
    public float allomanticStrength = 1000f;
    [Tooltip("Maximum velocity an allomancer can impart (lore: metals have terminal velocity based on strength)")]
    public float maxCoinVelocity = 400f;
    [Tooltip("Distance exponent (lore: 1 = inverse, 2 = inverse square)")]
    [Range(1f, 2f)]
    public float distanceExponent = 1f;
    [Tooltip("Velocity damping (lore: force decreases as target moves toward you faster)")]
    [Range(0f, 1f)]
    public float velocityDamping = 0.5f;
    
    [Header("Legacy Settings")]
    [Tooltip("Maximum force multiplier when flaring")]
    [Range(1.5f, 3f)]
    public float maxFlareMultiplier = 2f;
    [Tooltip("Metal cost multiplier when flaring")]
    [Range(1f, 5f)]
    public float flaringMetalCostMultiplier = 3f;
    [Tooltip("Skill mastery bonus")]
    [Range(1f, 2f)]
    public float masteryBonus = 1f;
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    [Tooltip("Transform where pull originates from (chest/center). Uses playerRigidbody if not set.")]
    public Transform chestTransform;
    
    [Header("Visual Effects")]
    [Tooltip("Particle effect prefab to spawn when pulling metal (optional)")]
    public GameObject pullEffectPrefab;
    [Tooltip("Camera shake magnitude when pulling with significant force")]
    public float shakeMagnitude = 0.1f;
    [Tooltip("Duration of camera shake in seconds")]
    public float shakeDuration = 0.1f;
    [Tooltip("Minimum force required to trigger camera shake")]
    public float shakeForceThreshold = 100f;
    [Tooltip("Enable screen tint when pulling")]
    public bool enablePullScreenTint = true;
    [Tooltip("Color for weak pulls")]
    public Color weakPullTint = new Color(0f, 0.5f, 1f, 0.1f);
    [Tooltip("Color for medium pulls")]
    public Color mediumPullTint = new Color(0f, 0.8f, 1f, 0.2f);
    [Tooltip("Color for strong pulls")]
    public Color strongPullTint = new Color(0f, 1f, 1f, 0.3f);
    [Tooltip("Duration of screen tint effect")]
    public float pullTintDuration = 0.2f;
    
    [Header("Pull Prediction")]
    [Tooltip("Enable trajectory prediction when targeting metal")]
    public bool enablePullPrediction = true;
    [Tooltip("Color for prediction line")]
    public Color predictionColor = new Color(0f, 0.5f, 1f, 0.5f);
    [Tooltip("Number of points in prediction line")]
    public int predictionPoints = 20;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging for pull operations")]
    public bool debugPullOperations = true;
    [Tooltip("Enable debug logging for flare state")]
    public bool debugFlareState = true;
    
    private bool isBurning = false;
    private bool pullAppliedThisPress = false;
    private bool qKeyWasPressed = false;
    
    // Flare state from centralized FlareManager
    private bool IsFlaring => FlareManager.Instance != null ? FlareManager.Instance.IsIronFlaring : false;
    
    // Targeted metal detection
    private RaycastHit currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody currentTargetRigidbody;
    private bool hasCurrentTarget = false;
    
    // Visual feedback
    private float cooldownTimer = 0f;
    private Coroutine pullTintCoroutine;
    private Color currentPullTint = Color.clear;
    
    // Pull prediction
    private LineRenderer predictionLine;
    private bool isPredictionActive = false;
    
    void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponentInParent<Rigidbody>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        if (allomancer == null)
        {
            allomancer = GetComponentInParent<Allomancer>();
        }
        
        if (metalLayer.value == 0)
        {
            metalLayer = LayerMask.GetMask("Metal");
        }
        
        if (chestTransform == null)
        {
            // Use player rigidbody position as chest (center of body)
            chestTransform = playerRigidbody != null ? playerRigidbody.transform : transform;
        }
        
        CreatePredictionLine();
    }
    
    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PullPredictionLine");
        predictionLine = lineObj.AddComponent<LineRenderer>();
        
        // Set up line renderer
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            predictionLine.material = new Material(shader);
        }
        else
        {
            predictionLine.material = new Material(Shader.Find("Unlit/Color"));
        }
        
        predictionLine.startColor = predictionColor;
        predictionLine.endColor = predictionColor;
        predictionLine.startWidth = 0.03f;
        predictionLine.endWidth = 0.01f;
        predictionLine.positionCount = predictionPoints;
        predictionLine.useWorldSpace = true;
        predictionLine.gameObject.SetActive(false);
    }
    
    void Update()
    {
        // EARLY EXIT: Check if Allomancer says we can't burn metal (out of metal)
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // TIMER: Update cooldown timer to prevent rapid re-activation
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // DETECTION: Update targeted metal detection (raycast from camera)
        UpdateTargetedMetal();
        
        // =========================================================================
        // Q KEY HANDLING - Pull mechanics
        // =========================================================================
        // Controls: Q = Pull, Ctrl = Flare toggle
        // Q ALWAYS pulls (flare is optional extra power)
        
        bool qKeyDown = Input.GetKeyDown(KeyCode.Q);
        bool qKeyUp = Input.GetKeyUp(KeyCode.Q);
        
        if (qKeyDown && !qKeyWasPressed)
        {
            qKeyWasPressed = true;
            
            // Start burning (if not on cooldown)
            if (cooldownTimer <= 0f)
            {
                if (!isBurning) StartBurning();
                pullAppliedThisPress = false;
            }
            
            // Execute pull (always, regardless of flare state)
            if (isBurning && !pullAppliedThisPress)
            {
                PullMetals();
                DrainMetal(flaringMetalCostMultiplier);
                pullAppliedThisPress = true;
            }
        }
        
        // Q KEY RELEASED: Stop burning Iron
        if (qKeyUp)
        {
            qKeyWasPressed = false;
            StopBurning();
        }
        
        // NOTE: Ctrl key flare toggling is now handled centrally in FlareManager
        // Press Ctrl to toggle BOTH Iron and Steel flares at once
        
        // Continuous metal drain while burning
        if (isBurning)
        {
            DrainMetal(1f);
        }
        
        // PREDICTION: Update pull trajectory prediction line
        UpdatePrediction();
    }
    
    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        if (allomancer != null)
        {
            allomancer.StartBurning(AllomancySkill.MetalType.Iron);
        }
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = 0.2f;
        if (allomancer != null)
        {
            allomancer.StopBurning();
        }
    }
    
    void UpdateTargetedMetal()
    {
        hasCurrentTarget = false;
        currentTarget = null;
        currentTargetRigidbody = null;
        
        if (playerCamera == null) return;
        
        float closestDist = maxRange;
        
        // Find all AllomanticTargets in scene
        var allTargets = FindObjectsOfType<AllomanticTarget>();
        
        foreach (var metal in allTargets)
        {
            if (metal == null || !metal.canBePulled) continue;
            
            Rigidbody rb = metal.GetComponent<Rigidbody>();
            if (rb == null || rb == playerRigidbody) continue;
            
            float dist = Vector3.Distance(rb.position, playerCamera.transform.position);
            
            // Just find closest metal within range - no angle requirement
            if (dist < closestDist && dist > 0.1f)
            {
                closestDist = dist;
                currentTargetRigidbody = rb;
                currentTarget = metal;
                hasCurrentTarget = true;
            }
        }
    }
    
    void UpdatePrediction()
    {
        bool shouldShowPrediction = enablePullPrediction && IsFlaring && hasCurrentTarget && currentTarget != null && currentTarget.canBePulled;
        
        if (shouldShowPrediction)
            DrawPredictionLine();
        else if (isPredictionActive)
        {
            predictionLine.gameObject.SetActive(false);
            isPredictionActive = false;
        }
    }
    
    void DrawPredictionLine()
    {
        if (predictionLine == null || currentTargetRigidbody == null) return;
        
        // Calculate trajectory for pull: straight line toward player
        Vector3 startPos = currentTargetRigidbody.position;
        Vector3 endPos = playerRigidbody.position;
        
        // Create straight line points from target to player
        Vector3[] points = new Vector3[predictionPoints];
        for (int i = 0; i < predictionPoints; i++)
        {
            float t = i / (float)(predictionPoints - 1);
            points[i] = Vector3.Lerp(startPos, endPos, t);
        }
        
        // Color based on distance (shorter = brighter blue)
        float distance = Vector3.Distance(startPos, endPos);
        Color lineColor;
        if (distance < 5f)
            lineColor = new Color(0f, 1f, 1f, 0.8f); // Bright cyan for close
        else if (distance < 15f)
            lineColor = new Color(0f, 0.7f, 1f, 0.6f); // Medium blue
        else
            lineColor = new Color(0f, 0.5f, 1f, 0.4f); // Dark blue for far
        
        predictionLine.startColor = lineColor;
        predictionLine.endColor = lineColor;
        
        // Update line renderer
        predictionLine.positionCount = predictionPoints;
        predictionLine.SetPositions(points);
        predictionLine.gameObject.SetActive(true);
        isPredictionActive = true;
    }
    
    void PullMetals()
    {
        if (debugPullOperations) Debug.Log($"[PULL] PullMetals: playerRB={playerRigidbody != null}, hasTarget={hasCurrentTarget}, targetRB={currentTargetRigidbody?.name ?? "null"}");
        if (playerRigidbody == null) return;
        if (!hasCurrentTarget || currentTargetRigidbody == null) return;
        
        Rigidbody targetRigidbody = currentTargetRigidbody;
        AllomanticTarget target = currentTarget;
        
        if (targetRigidbody == playerRigidbody) return;
        if (target != null && !target.canBePulled) return;
        
        Vector3 pullOrigin = playerRigidbody.position;
        float distance = Vector3.Distance(pullOrigin, targetRigidbody.position);
        Vector3 directionToTarget = targetRigidbody.position - pullOrigin;
        bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
        
        float strength = allomanticStrength * (playerRigidbody.mass / referenceMass) * masteryBonus;
        if (IsFlaring) strength *= maxFlareMultiplier;
        
        float distanceFactor = 1f - (distance / maxRange);
        distanceFactor = Mathf.Clamp01(distanceFactor);
        
        float force = strength * distanceFactor;
        
        Debug.Log($"[PULL] Force={force}, distance={distance}");
        
        if (force > 0.1f)
        {
            if (isAnchored)
                playerRigidbody.AddForce(directionToTarget.normalized * force);
            else
                targetRigidbody.AddForce(-directionToTarget.normalized * force, ForceMode.Impulse);
        }
        
        if (force > shakeForceThreshold)
        {
            ShakeCamera(shakeMagnitude);
            TriggerPullTint(force);
        }
    }
    
    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;
        
        // Continuous drain while burning
        float drainAmount = metalCostPerSecond * Time.deltaTime * multiplier;
        if (IsFlaring) drainAmount *= 3f; // Flaring drains 3x faster
        
        // Also drain extra per action (pull)
        float actionDrain = metalCostPerSecond * 0.5f * multiplier;
        
        allomancer.DrainMetal(AllomancySkill.MetalType.Iron, drainAmount + actionDrain);
    }
    
    void ShakeCamera(float magnitude)
    {
        if (playerCamera == null || magnitude <= 0f) return;
        StartCoroutine(ShakeCoroutine(magnitude));
    }
    
    IEnumerator ShakeCoroutine(float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.transform.localPosition = originalPos;
    }
    
    void TriggerPullTint(float pullForce)
    {
        if (!enablePullScreenTint) return;
        if (pullTintCoroutine != null) StopCoroutine(pullTintCoroutine);
        pullTintCoroutine = StartCoroutine(PullTintCoroutine(pullForce));
    }
    
    IEnumerator PullTintCoroutine(float force)
    {
        Color tintColor = weakPullTint;
        if (force > shakeForceThreshold * 2f)
            tintColor = strongPullTint;
        else if (force > shakeForceThreshold)
            tintColor = mediumPullTint;
        
        float elapsed = 0f;
        while (elapsed < pullTintDuration)
        {
            float alpha = Mathf.Lerp(tintColor.a, 0f, elapsed / pullTintDuration);
            currentPullTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentPullTint = Color.clear;
        pullTintCoroutine = null;
    }
    
    void OnGUI()
    {
        if (currentPullTint.a > 0.01f)
        {
            GUI.color = currentPullTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        
        if (IsFlaring && debugPullOperations)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.cyan;
            style.fontSize = 14;
            
            GUI.Label(new Rect(10, 250, 300, 20), $"Iron Pull: Flaring", style);
            if (hasCurrentTarget && currentTargetRigidbody != null)
            {
                float distance = Vector3.Distance(playerRigidbody.position, currentTargetRigidbody.position);
                GUI.Label(new Rect(10, 270, 300, 20), $"Target: {distance:F1}m", style);
            }
        }
    }
}
