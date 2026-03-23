/* IronPull.cs
 * 
 * PURPOSE:
 * Implements the Iron Allomancy ability (Lurcher) - pull metal objects toward the player.
 * Requires burning Iron metal to activate.
 * 
 * KEY FIELDS:
 * - pullForce: Base force applied when pulling metal objects
 * - maxRange: Maximum distance for pulling metal (units)
 * - metalCostPerSecond: Metal reserve consumption rate while burning
 * - allomancer: Reference to the Allomancer system for metal reserve checks
 * - playerCamera: Camera for raycasting (determines pull direction)
 * 
 * HOW IT WORKS:
 * 1. Player holds Left Mouse Button to burn Iron
 * 2. Raycasts from camera detect metal objects within range
 * 3. Applies force toward player based on pullForce and object mass
 * 4. Can pull player toward anchored heavy objects (isAnchored=true)
 * 5. Checks canBurnMetal before allowing pull
 * 
 * IMPORTANT NOTES:
 * - Requires Allomancer component to check metal reserves
 * - Heavy/anchored objects pull the player instead of moving
 * - Force is proportional to player mass vs target mass
 * - Disabled when metal reserve hits 0
 * 
 * LORE ACCURACY:
 * Iron Pull (Lurcher ability) - pulls metal toward center of self.
 * Same physics as Steel Push but opposite direction.
 */

// NOTE: Lines 39 and 45 contain Debug.Log which should be removed for production
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
    [Tooltip("Show prediction when holding pull button")]
    public bool showPredictionOnHold = true;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging for pull operations")]
    public bool debugPullOperations = true;
    
    private bool isBurning = false;
    private bool isFlaring = false;
    private bool pullAppliedThisPress = false;
    
    // Targeted metal detection
    private RaycastHit currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody currentTargetRigidbody;
    private bool hasCurrentTarget = false;
    
    // Visual feedback
    private bool metalInRange = false;
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
        
        if (chestTransform == null)
        {
            Transform player = GetComponentInParent<Transform>();
            if (player != null)
            {
                Transform chest = player.Find("Chest");
                if (chest == null) chest = player.Find("ChestBone");
                if (chest == null) chest = player.Find("Spine2");
                if (chest == null) chest = player.Find("Torso");
                chestTransform = chest != null ? chest : player;
            }
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
        // Check if Allomancer says we can't burn metal (out of metal)
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // Update cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Flaring: Ctrl toggles flaring mode (works anytime)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isFlaring = !isFlaring;
            if (debugPullOperations) Debug.Log($"[IRON] Flaring: {(isFlaring ? "ON" : "OFF")}");
        }
        
        // Update targeted metal detection
        UpdateTargetedMetal();
        
        // Pull: Left Mouse (requires flaring)
        if (Input.GetKeyDown(KeyCode.Mouse0) && isFlaring && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
            if (!pullAppliedThisPress)
            {
                PullMetals();
                DrainMetal(flaringMetalCostMultiplier);
                pullAppliedThisPress = true;
            }
        }
        
        // Stop burning when releasing Left Mouse
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StopBurning();
        }
        
        // Update pull prediction
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
#if UNITY_EDITOR
        Debug.Log("Burning Iron - Pull ready");
#endif
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = pullCooldown; // Start cooldown after releasing
        if (allomancer != null)
        {
            allomancer.StopBurning();
        }
#if UNITY_EDITOR
        Debug.Log("Stopped burning Iron");
#endif
    }
    
    void UpdateTargetedMetal()
    {
        hasCurrentTarget = false;
        currentTarget = null;
        currentTargetRigidbody = null;
        
        if (playerCamera == null) return;
        
        // Raycast from camera center to find specific metal target
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out currentTargetHit, maxRange, metalLayer))
        {
            currentTargetRigidbody = currentTargetHit.rigidbody;
            if (currentTargetRigidbody != null && currentTargetRigidbody != playerRigidbody)
            {
                currentTarget = currentTargetHit.collider.GetComponent<AllomanticTarget>();
                hasCurrentTarget = true;
            }
        }
    }
    
    void UpdatePrediction()
    {
        // Show prediction when holding pull button on a valid target
        bool shouldShowPrediction = enablePullPrediction && 
                                   showPredictionOnHold && 
                                   Input.GetMouseButton(0) && 
                                   isBurning && 
                                   hasCurrentTarget && 
                                   currentTarget != null && 
                                   currentTarget.canBePulled;
        
        if (shouldShowPrediction)
        {
            DrawPredictionLine();
        }
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
        if (playerRigidbody == null) return;
        
        // Normal pull: only pull the targeted metal (not all metals)
        if (!hasCurrentTarget || currentTargetRigidbody == null)
        {
            if (debugPullOperations) Debug.Log("[PULL] No target - aim at metal");
            return;
        }
        
        Vector3 pullOrigin = playerRigidbody.position;
        Rigidbody targetRigidbody = currentTargetRigidbody;
        AllomanticTarget target = currentTarget;
        
        if (targetRigidbody == playerRigidbody) return;
        if (target != null && !target.canBePulled) return;
        
        float targetMass = target != null ? target.GetEffectiveMass() : targetRigidbody.mass;
        float distance = Vector3.Distance(pullOrigin, targetRigidbody.position);
        Vector3 directionToTarget = targetRigidbody.position - pullOrigin;
        bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
        
        float playerMass = playerRigidbody.mass;
        float weightFactor = playerMass / referenceMass;
        float strength = allomanticStrength * weightFactor * masteryBonus;
        if (isFlaring)
        {
            strength *= maxFlareMultiplier;
            if (debugPullOperations) Debug.Log($"[PULL] FLARING: strength {allomanticStrength * weightFactor * masteryBonus:F0f} -> {strength:F0f}");
        }
        
        float distanceFactor = 1f;
        if (distance > 0.01f && distance <= maxRange)
        {
            float effectiveDistance = Mathf.Max(distance, minDistance);
            distanceFactor = Mathf.Pow(referenceDistance / effectiveDistance, distanceExponent);
        }
        
        Vector3 targetVelocity = targetRigidbody.velocity;
        float velocityTowardPlayer = Vector3.Dot(targetVelocity, -directionToTarget.normalized);
        float velocityDampingFactor = 1f;
        if (velocityTowardPlayer > 0)
        {
            float velocityRatio = Mathf.Clamp01(velocityTowardPlayer / maxCoinVelocity);
            velocityDampingFactor = 1f - (velocityRatio * velocityDamping);
        }
        
        float force = strength * distanceFactor * velocityDampingFactor;
        
        if (isAnchored)
        {
            playerRigidbody.AddForce(directionToTarget.normalized * force);
            if (debugPullOperations) Debug.Log($"[PULL] Pulled player: {force:F0f}N");
        }
        else if (force > 1f)
        {
            float currentVelocity = targetVelocity.magnitude;
            if (currentVelocity < maxCoinVelocity)
            {
                targetRigidbody.AddForce(-directionToTarget.normalized * force, ForceMode.Impulse);
                if (debugPullOperations) Debug.Log($"[PULL] Pulled {targetRigidbody.name}: {force:F0f}N");
            }
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
        
        float drainAmount = metalCostPerSecond * multiplier;
        allomancer.DrainMetal(AllomancySkill.MetalType.Iron, drainAmount);
    }
    
    void PlayPullSound()
    {
        // Placeholder for audio - similar to SteelPush
        // Could use different sound for pulling
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
    
    IEnumerator PullTintCoroutine(float pullForce)
    {
        // Determine color based on pull force
        Color tintColor;
        if (pullForce < pullForce * 0.3f) // Weak pull
            tintColor = weakPullTint;
        else if (pullForce < pullForce * 0.7f) // Medium pull
            tintColor = mediumPullTint;
        else // Strong pull
            tintColor = strongPullTint;
        
        // Brief full-screen tint effect using GUI
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
        // Draw pull tint effect (full screen overlay)
        if (currentPullTint.a > 0.01f)
        {
            GUI.color = currentPullTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white; // Reset for other GUI elements
        }
        
        // Simple debug display for Iron Pull
        if (isBurning)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.blue;
            style.fontSize = 14;
            
            GUI.Label(new Rect(10, 250, 300, 20), $"Iron Pull Active", style);
            if (hasCurrentTarget && currentTargetRigidbody != null)
            {
                float distance = currentTargetHit.distance;
                float mass = currentTarget != null ? currentTarget.GetEffectiveMass() : currentTargetRigidbody.mass;
                bool canPull = currentTarget != null ? currentTarget.canBePulled : true;
                bool isAnchored = (currentTarget != null && currentTarget.isAnchored) || currentTargetRigidbody.isKinematic;
                
                GUI.Label(new Rect(10, 270, 300, 20), $"Target: {distance:F2}m, {mass:F1}kg", style);
                GUI.Label(new Rect(10, 290, 300, 20), $"Can Pull: {canPull}, Anchored: {isAnchored}", style);
            }
        }
    }
}
