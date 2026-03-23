/* SteelPush.cs
 * 
 * PURPOSE:
 * Implements the Steel Allomancy ability (Coinshot) - push metal objects away from the player.
 * Requires burning Steel metal to activate.
 * 
 * KEY FIELDS:
 * - pushForce: Base force applied when pushing metal objects
 * - maxRange: Maximum distance for pushing metal (units)
 * - metalCostPerSecond: Metal reserve consumption rate while burning
 * - allomancer: Reference to the Allomancer system for metal reserve checks
 * - playerCamera: Camera for raycasting (determines push direction)
 * 
 * HOW IT WORKS:
 * 1. Player holds Right Mouse Button to burn Steel
 * 2. Raycasts from camera detect metal objects within range
 * 3. Applies force away from player based on pushForce and object mass
 * 4. Can push player away from anchored heavy objects (isAnchored=true)
 * 5. Checks canBurnMetal before allowing push
 * 
 * IMPORTANT NOTES:
 * - Requires Allomancer component to check metal reserves
 * - Heavy/anchored objects push the player instead of moving
 * - Force is proportional to player mass vs target mass
 * - Disabled when metal reserve hits 0
 * 
 * LORE ACCURACY:
 * Steel Push (Coinshot ability) - pushes metal away from center of self.
 * Stronger push when closer (zenith point ~5m). Anchored objects push the allomancer.
 */

// NOTE: Lines 39 and 45 contain Debug.Log which should be removed for production
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SteelPush : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base force applied when pushing. Needs calibration for desired coin velocities.")]
    public float pushForce = 800f;
    [Tooltip("Reference mass for force calculation (average human = 80kg).")]
    public float referenceMass = 80f;
    [Tooltip("Reference distance where force factor = 1. Force = baseForce * (referenceDistance / distance).")]
    public float referenceDistance = 3f;
    [Tooltip("Minimum distance to prevent unrealistic forces at close range.")]
    public float minDistance = 1f;
    public float maxRange = 30f;
    public float metalCostPerSecond = 2f;
    [Tooltip("Cooldown time in seconds after releasing push button")]
    public float pushCooldown = 0.2f;
    
    [Header("Allomancy Physics (Lore-Accurate Model)")]
    [Tooltip("Base allomantic strength (determines max force AND max velocity)")]
    public float allomanticStrength = 1000f;
    [Tooltip("Maximum velocity an allomancer can impart (lore: coins have terminal velocity based on strength)")]
    public float maxCoinVelocity = 400f;
    [Tooltip("Distance exponent (lore: 1 = inverse, 2 = inverse square)")]
    [Range(1f, 2f)]
    public float distanceExponent = 1f;
    [Tooltip("Velocity damping (lore: force decreases as target moves away faster)")]
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
    [Tooltip("Transform where push originates from (chest/center). Uses playerRigidbody if not set.")]
    public Transform chestTransform;
    
    [Header("Visual Effects")]
    [Tooltip("Particle effect prefab to spawn when pushing metal (optional)")]
    public GameObject pushEffectPrefab;
    [Tooltip("Camera shake magnitude when pushing with significant force")]
    public float shakeMagnitude = 0.1f;
    [Tooltip("Duration of camera shake in seconds")]
    public float shakeDuration = 0.1f;
    [Tooltip("Minimum force required to trigger camera shake")]
    public float shakeForceThreshold = 100f;
    
    [Header("Focused Push")]
    [Tooltip("Key to hold for pushing only targeted metal (single selection)")]
    public KeyCode focusKey = KeyCode.LeftControl;
    [Tooltip("Color for focused push crosshair")]
    public Color focusedPushColor = Color.red;
    
    [Header("Audio")]
    [Tooltip("AudioSource for push sounds (optional)")]
    public AudioSource audioSource;
    [Tooltip("Sound to play when pushing metal (optional)")]
    public AudioClip pushSound;
    [Tooltip("Volume multiplier for push sounds")]
    public float soundVolume = 0.5f;
    
    [Header("Flaring Visual Effect")]
    [Tooltip("UI Image for vignette effect when flaring (optional)")]
    public UnityEngine.UI.Image vignetteImage;
    [Tooltip("Color of vignette when flaring")]
    public Color flaringColor = new Color(1f, 0.2f, 0f, 0.3f); // Orange tint
    [Tooltip("Duration of vignette pulse in seconds")]
    public float vignettePulseDuration = 0.5f;
    [Tooltip("Maximum alpha of vignette during pulse")]
    public float vignetteMaxAlpha = 0.3f;
    
    [Header("UI Feedback")]
    [Tooltip("Crosshair UI Image that changes color when metal is in range (optional)")]
    public UnityEngine.UI.Image crosshairImage;
    [Tooltip("Color when metal is within push range")]
    public Color metalInRangeColor = Color.green;
    [Tooltip("Color when no metal in range")]
    public Color noMetalColor = Color.white;
    
    [Header("Push Prediction")]
    [Tooltip("Enable trajectory prediction when targeting metal")]
    public bool enablePushPrediction = true;
    [Tooltip("Color for prediction line")]
    public Color predictionColor = new Color(1f, 1f, 0f, 0.5f); // Semi-transparent yellow
    [Tooltip("Number of points in prediction line")]
    public int predictionPoints = 20;
    [Tooltip("Time step for prediction (seconds)")]
    public float predictionTimeStep = 0.1f;
    [Tooltip("Show prediction when holding push button")]
    public bool showPredictionOnHold = true;
    
    [Header("Push Force Visual Feedback")]
    [Tooltip("Enable screen tint when pushing")]
    public bool enablePushScreenTint = true;
    [Tooltip("Color for weak pushes")]
    public Color weakPushTint = new Color(0f, 1f, 0f, 0.1f); // Green
    [Tooltip("Color for medium pushes")]
    public Color mediumPushTint = new Color(1f, 1f, 0f, 0.2f); // Yellow
    [Tooltip("Color for strong pushes")]
    public Color strongPushTint = new Color(1f, 0f, 0f, 0.3f); // Red
    [Tooltip("Duration of screen tint effect")]
    public float pushTintDuration = 0.2f;
    
    [Header("Steel Bubble (Defensive)")]
    [Tooltip("Enable steel bubble defensive ability")]
    public bool enableSteelBubble = true;
    [Tooltip("Key to activate steel bubble")]
    public KeyCode steelBubbleKey = KeyCode.F;
    [Tooltip("Radius of steel bubble (~6-10 feet like Wax's bubble)")]
    public float steelBubbleRadius = 2.5f;
    [Tooltip("Force applied by steel bubble (lore: gentle push like breeze)")]
    public float steelBubbleForce = 50f;
    [Tooltip("Cooldown between steel bubble activations")]
    public float steelBubbleCooldown = 0.5f;
    [Tooltip("Does steel bubble consume extra metal?")]
    public float steelBubbleMetalCostMultiplier = 1.5f;
    
    [Header("Flight Mechanics")]
    [Tooltip("Extra upward force multiplier when pushing off anchored objects below (1 = normal)")]
    public float flightLaunchMultiplier = 1.5f;
    [Tooltip("Angle threshold (degrees) from downward to consider 'below' for flight boost")]
    public float flightAngleThreshold = 45f;
    
    [Header("Impulse Mode")]
    [Tooltip("Mass threshold (kg) below which objects receive impulse instead of continuous force")]
    public float impulseMassThreshold = 5f;
    [Tooltip("Calibration factor for impulse force (adjust to achieve target coin velocities)")]
    public float impulseCalibration = 0.000917f; // Calibrated for 22.22 m/s at 10m with 10g coin after referenceDistance change
    [Tooltip("Enable debug logging for impulse calibration")]
    public bool debugCalibration = false;
    [Tooltip("Enable debug logging for push operations")]
    public bool debugPushOperations = true;
    
    private bool isBurning = false;
    private bool isFlaring = false;
    private bool pushAppliedThisPress = false;
    private bool bubbleAppliedThisPress = false;
    private Coroutine vignetteCoroutine;
    private bool metalInRange = false;
    private float cooldownTimer = 0f;
    private float steelBubbleCooldownTimer = 0f;
    private bool isSteelBubbleActive = false;
    
    // Targeted metal detection
    private RaycastHit currentTargetHit;
    private AllomanticTarget currentTarget;
    private Rigidbody currentTargetRigidbody;
    private bool hasCurrentTarget = false;
    
    // Push prediction
    private LineRenderer predictionLine;
    private bool isPredictionActive = false;
    
    // Push force visual feedback
    private Coroutine pushTintCoroutine;
    private Color currentPushTint = Color.clear;
    
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
        
        // Create prediction line renderer
        CreatePredictionLine();
    }
    
    void CreatePredictionLine()
    {
        GameObject lineObj = new GameObject("PushPredictionLine");
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
        
        // Update cooldown timers
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (steelBubbleCooldownTimer > 0f) steelBubbleCooldownTimer -= Time.deltaTime;
        
        // Start burning: E key (one per press)
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimer <= 0f)
        {
            StartBurning();
            pushAppliedThisPress = false;
            bubbleAppliedThisPress = false;
        }
        
        // Flaring: Ctrl toggles flaring mode (works anytime)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isFlaring = !isFlaring;
            if (debugPushOperations) Debug.Log($"[STEEL] Flaring: {(isFlaring ? "ON" : "OFF")}");
            if (isFlaring && isBurning) StartFlaringVignette();
        }
        
        // Update targeted metal detection
        UpdateTargetedMetal();
        
        // Steel Bubble: F key (one per press, requires flaring)
        if (enableSteelBubble && Input.GetKeyDown(steelBubbleKey))
        {
            if (isFlaring && steelBubbleCooldownTimer <= 0f)
            {
                if (!isBurning) StartBurning();
                if (!bubbleAppliedThisPress)
                {
                    PushMetalsInBubble();
                    DrainMetal(steelBubbleMetalCostMultiplier);
                    steelBubbleCooldownTimer = steelBubbleCooldown;
                    bubbleAppliedThisPress = true;
                }
            }
        }
        
        // Normal push: E key held (one per press)
        if (Input.GetKey(KeyCode.E) && isBurning && !pushAppliedThisPress)
        {
            Debug.Log($"[PUSH] E pressed - burning={isBurning}, flaring={isFlaring}");
            if (isFlaring) StartFlaringVignette();
            PushMetals();
            DrainMetal(isFlaring ? flaringMetalCostMultiplier : 1f);
            pushAppliedThisPress = true;
        }
        
        // Stop burning when releasing E or F key
        bool pushKeyUp = Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(steelBubbleKey);
        if (pushKeyUp)
        {
            StopBurning();
        }
        
        // Update push prediction
        UpdatePrediction();
    }
    
    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        if (allomancer != null)
        {
            allomancer.StartBurning(AllomancySkill.MetalType.Steel);
        }
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = pushCooldown;
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
        bool isPushing = Input.GetKey(KeyCode.E);
        
        bool shouldShowPrediction = enablePushPrediction && 
                                   showPredictionOnHold && 
                                   isPushing && 
                                   isBurning && 
                                   hasCurrentTarget && 
                                   currentTarget != null && 
                                   currentTarget.canBePushed;
        
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
        
        // Get target mass
        float targetMass = currentTarget.GetEffectiveMass();
        
        // Calculate initial velocity based on push force
        float playerMass = playerRigidbody != null ? playerRigidbody.mass : 80f;
        float weightFactor = playerMass / referenceMass;
        float force = pushForce * weightFactor;
        
        // Apply distance factor with zenith cap
        float distance = currentTargetHit.distance;
        if (distance > 0.01f && distance <= maxRange)
        {
            float effectiveDistance = Mathf.Max(distance, minDistance);
            float distanceFactor = referenceDistance / effectiveDistance;
            distanceFactor = Mathf.Min(distanceFactor, 2f); // Zenith cap
            force *= distanceFactor;
        }
        else if (distance > maxRange)
        {
            force = 0f; // Beyond range
        }
        
        // Apply flaring multiplier
        if (isFlaring) force *= 2f;
        
        // Calculate initial velocity (impulse model for light objects)
        Vector3 initialVelocity;
        float initialSpeed;
        if (targetMass <= impulseMassThreshold)
        {
            // Impulse mode
            float impulseForce = force * impulseCalibration;
            initialSpeed = impulseForce / targetMass;
            Vector3 pushDirection = (currentTargetRigidbody.position - playerRigidbody.position).normalized;
            initialVelocity = pushDirection * initialSpeed;
        }
        else
        {
            // Continuous force - estimate after 0.1 seconds
            float acceleration = force / targetMass;
            Vector3 pushDirection = (currentTargetRigidbody.position - playerRigidbody.position).normalized;
            initialVelocity = pushDirection * acceleration * 0.1f;
            initialSpeed = initialVelocity.magnitude;
        }
        
        // Calculate trajectory points and track final velocity
        Vector3[] points = new Vector3[predictionPoints];
        Vector3 startPos = currentTargetRigidbody.position;
        Vector3 velocity = initialVelocity;
        float timeStep = predictionTimeStep;
        
        for (int i = 0; i < predictionPoints; i++)
        {
            points[i] = startPos;
            
            // Update position and velocity for projectile motion with gravity
            startPos += velocity * timeStep;
            velocity += Physics.gravity * timeStep;
        }
        
        // Color based on initial speed (velocity at push)
        Color startColor = Color.cyan;
        Color endColor;
        
        if (initialSpeed < 10f) // Slow
            endColor = Color.green;
        else if (initialSpeed < 30f) // Medium
            endColor = Color.yellow;
        else // Fast
            endColor = Color.red;
        
        // Set gradient colors
        predictionLine.startColor = startColor;
        predictionLine.endColor = endColor;
        
        // Update line renderer
        predictionLine.positionCount = predictionPoints;
        predictionLine.SetPositions(points);
        predictionLine.gameObject.SetActive(true);
        isPredictionActive = true;
    }
    
    void PushMetals()
    {
        if (playerRigidbody == null)
        {
            Debug.LogError("[PUSH] ERROR: playerRigidbody is null!");
            return;
        }
        
        if (!hasCurrentTarget || currentTargetRigidbody == null)
        {
            if (debugPushOperations) Debug.Log("[PUSH] No target - aim at metal");
            return;
        }
        
        Vector3 pushOrigin = playerRigidbody.position;
        Rigidbody targetRigidbody = currentTargetRigidbody;
        AllomanticTarget target = currentTarget;
        
        if (targetRigidbody == playerRigidbody) return;
        if (target != null && !target.canBePushed) return;
        
        float targetMass = target != null ? target.GetEffectiveMass() : targetRigidbody.mass;
        float distance = Vector3.Distance(pushOrigin, targetRigidbody.position);
        Vector3 directionToTarget = targetRigidbody.position - pushOrigin;
        bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
        
        float playerMass = playerRigidbody.mass;
        float weightFactor = playerMass / referenceMass;
        float strength = allomanticStrength * weightFactor * masteryBonus;
        if (isFlaring)
        {
            strength *= maxFlareMultiplier;
            Debug.Log($"[PUSH] FLARING: strength {allomanticStrength * weightFactor * masteryBonus:F0f} -> {strength:F0f} (x{maxFlareMultiplier})");
        }
        
        float distanceFactor = 1f;
        if (distance > 0.01f && distance <= maxRange)
        {
            float effectiveDistance = Mathf.Max(distance, minDistance);
            distanceFactor = Mathf.Pow(referenceDistance / effectiveDistance, distanceExponent);
        }
        
        Vector3 targetVelocity = targetRigidbody.velocity;
        float velocityAwayFromPlayer = Vector3.Dot(targetVelocity, directionToTarget.normalized);
        float velocityDampingFactor = 1f;
        if (velocityAwayFromPlayer > 0)
        {
            float velocityRatio = Mathf.Clamp01(velocityAwayFromPlayer / maxCoinVelocity);
            velocityDampingFactor = 1f - (velocityRatio * velocityDamping);
        }
        
        float force = strength * distanceFactor * velocityDampingFactor;
        
        if (isAnchored)
        {
            playerRigidbody.AddForce(-directionToTarget.normalized * force);
            if (debugPushOperations) Debug.Log($"[PUSH] Pushed player: {force:F0f}N");
        }
        else if (force > 1f)
        {
            float currentVelocity = targetVelocity.magnitude;
            if (currentVelocity < maxCoinVelocity)
            {
                targetRigidbody.AddForce(directionToTarget.normalized * force, ForceMode.Impulse);
                if (debugPushOperations) Debug.Log($"[PUSH] Pushed {targetRigidbody.name}: {force:F0f}N");
            }
        }
        
        if (force > shakeForceThreshold)
        {
            ShakeCamera(shakeMagnitude);
            TriggerPushTint(force);
        }
    }
    
    void PushMetalsInBubble()
    {
        if (playerRigidbody == null) return;
        
        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, steelBubbleRadius, metalLayer);
        if (debugPushOperations) Debug.Log($"[BUBBLE] {colliders.Length} metals in {steelBubbleRadius}m range");
        
        foreach (Collider collider in colliders)
        {
            Rigidbody targetRigidbody = collider.attachedRigidbody;
            if (targetRigidbody == null || targetRigidbody == playerRigidbody) continue;
            
            AllomanticTarget target = collider.GetComponent<AllomanticTarget>();
            if (target != null && !target.canBePushed) continue;
            
            float force = steelBubbleForce;
            Vector3 direction = (targetRigidbody.position - playerRigidbody.position).normalized;
            bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
            
            if (isAnchored)
            {
                playerRigidbody.AddForce(-direction * force * Time.deltaTime);
                if (debugPushOperations) Debug.Log($"[BUBBLE] Pushed player from {collider.name}");
            }
            else
            {
                targetRigidbody.AddForce(direction * force, ForceMode.Impulse);
                if (debugPushOperations) Debug.Log($"[BUBBLE] Pushed {collider.name}: {force:F0f}N");
            }
            
            TriggerPushTint(force);
        }
    }
    
    void DrainMetal(float multiplier = 1f)
    {
        if (allomancer == null) return;
        
        float drainAmount = metalCostPerSecond * Time.deltaTime * multiplier;
        if (isFlaring) drainAmount *= 3f; // Flaring drains 3x faster
        
        allomancer.DrainMetal(AllomancySkill.MetalType.Steel, drainAmount);
    }
    
    void PlayPushSound()
    {
        if (audioSource != null && pushSound != null)
        {
            audioSource.PlayOneShot(pushSound, soundVolume);
        }
    }
    
    void TriggerPushTint(float pushForce)
    {
        if (!enablePushScreenTint) return;
        if (pushTintCoroutine != null) StopCoroutine(pushTintCoroutine);
        pushTintCoroutine = StartCoroutine(PushTintCoroutine(pushForce));
    }
    
    IEnumerator PushTintCoroutine(float pushForce)
    {
        // Determine color based on push force
        Color tintColor;
        if (pushForce < pushForce * 0.3f) // Weak push
            tintColor = weakPushTint;
        else if (pushForce < pushForce * 0.7f) // Medium push
            tintColor = mediumPushTint;
        else // Strong push
            tintColor = strongPushTint;
        
        // Brief full-screen tint effect using GUI
        float elapsed = 0f;
        while (elapsed < pushTintDuration)
        {
            float alpha = Mathf.Lerp(tintColor.a, 0f, elapsed / pushTintDuration);
            // We'll use OnGUI to draw this - store the current tint color
            currentPushTint = new Color(tintColor.r, tintColor.g, tintColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentPushTint = Color.clear;
        pushTintCoroutine = null;
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
    
    void StartFlaringVignette()
    {
        if (vignetteImage == null) return;
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(VignettePulseCoroutine());
    }
    
    IEnumerator VignettePulseCoroutine()
    {
        vignetteImage.gameObject.SetActive(true);
        vignetteImage.color = flaringColor;
        float elapsed = 0f;
        while (elapsed < vignettePulseDuration)
        {
            float t = elapsed / vignettePulseDuration;
            float alpha = Mathf.Sin(t * Mathf.PI) * vignetteMaxAlpha;
            Color c = flaringColor;
            c.a = alpha;
            vignetteImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }
        vignetteImage.gameObject.SetActive(false);
        vignetteCoroutine = null;
    }
    
    void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        crosshairImage.color = metalInRange ? metalInRangeColor : noMetalColor;
    }
    
    // Draw gizmos in editor for debugging
    void OnDrawGizmosSelected()
    {
        if (playerRigidbody == null) return;
        
        // Draw push range sphere (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerRigidbody.position, maxRange);
        
        // Draw zenith distance sphere (cyan) - peak force zone
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerRigidbody.position, referenceDistance);
        
        // Draw steel bubble sphere
        if (enableSteelBubble)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Semi-transparent orange
            Gizmos.DrawWireSphere(playerRigidbody.position, steelBubbleRadius);
        }
    }
    
    // Real-time debug display for calibration
    void OnGUI()
    {
        // Draw push tint effect (full screen overlay)
        if (currentPushTint.a > 0.01f)
        {
            GUI.color = currentPushTint;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white; // Reset for other GUI elements
        }
        
        if (!debugCalibration || !isBurning) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 14;
        
        float y = 100f;
        GUI.Label(new Rect(10, y, 400, 20), $"Steel Push Debug", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Push Force: {pushForce} N", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Zenith (Reference) Distance: {referenceDistance}m", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Max Range: {maxRange}m", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Physics: 1/r (inverse proportional)", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Impulse Calibration: {impulseCalibration}", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Metal in Range: {metalInRange}", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Flaring: {isFlaring}", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Cooldown: {cooldownTimer:F2}s", style);
        y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"Steel Bubble: {isSteelBubbleActive}", style);
        y += 30;
        
        // Show targeted metal info
        if (hasCurrentTarget && currentTargetRigidbody != null)
        {
            GUI.Label(new Rect(10, y, 400, 20), $"Targeted Metal:", style);
            y += 20;
            
            float distance = currentTargetHit.distance;
            float mass = currentTarget != null ? currentTarget.GetEffectiveMass() : currentTargetRigidbody.mass;
            bool canPush = currentTarget != null ? currentTarget.canBePushed : true;
            bool isAnchored = (currentTarget != null && currentTarget.isAnchored) || currentTargetRigidbody.isKinematic;
            
            GUI.Label(new Rect(20, y, 400, 20), $"Distance: {distance:F2}m", style);
            y += 20;
            GUI.Label(new Rect(20, y, 400, 20), $"Mass: {mass:F2}kg", style);
            y += 20;
            GUI.Label(new Rect(20, y, 400, 20), $"Can Push: {canPush}", style);
            y += 20;
            GUI.Label(new Rect(20, y, 400, 20), $"Anchored: {isAnchored}", style);
            y += 20;
            
            // Calculate expected velocity for this specific target
            if (canPush && distance > 0)
            {
                float playerMass = playerRigidbody != null ? playerRigidbody.mass : 80f;
                float weightFactor = playerMass / referenceMass;
                float force = pushForce * weightFactor;
                
                // Calculate distance factor with zenith cap
                float effectiveDistance = Mathf.Max(distance, minDistance);
                float distanceFactor = referenceDistance / effectiveDistance;
                distanceFactor = Mathf.Min(distanceFactor, 2f); // Zenith cap
                force *= distanceFactor;
                
                // Show force calculation breakdown
                GUI.Label(new Rect(20, y, 400, 20), $"Weight Factor: {weightFactor:F2}", style);
                y += 20;
                GUI.Label(new Rect(20, y, 400, 20), $"Distance Factor: {distanceFactor:F2} (1/{effectiveDistance:F1}m)", style);
                y += 20;
                GUI.Label(new Rect(20, y, 400, 20), $"Final Force: {force:F2} N", style);
                y += 20;
                
                float expectedVelocity = 0f;
                if (mass <= impulseMassThreshold)
                {
                    // Impulse mode for light objects
                    float impulseForce = force * impulseCalibration;
                    expectedVelocity = impulseForce / mass;
                }
                else
                {
                    // Continuous force - estimate after 1 second of push
                    expectedVelocity = (force / mass) * 1f;
                }
                
                GUI.Label(new Rect(20, y, 400, 20), $"Expected Velocity: {expectedVelocity:F2} m/s ({expectedVelocity * 3.6f:F1} km/h)", style);
                y += 20;
            }
            y += 10;
        }
        
        // Show expected coin velocity calculation for generic coin
        if (metalInRange)
        {
            GUI.Label(new Rect(10, y, 400, 20), $"Generic Coin Velocity (10g):", style);
            y += 20;
            GUI.Label(new Rect(20, y, 400, 20), $"At 10m: {CalculateExpectedVelocity(10f, 0.01f):F2} m/s", style);
            y += 20;
            GUI.Label(new Rect(20, y, 400, 20), $"At 5m:  {CalculateExpectedVelocity(5f, 0.01f):F2} m/s", style);
            y += 20;
            GUI.Label(new Rect(20, y, 400, 20), $"At 1m:  {CalculateExpectedVelocity(1f, 0.01f):F2} m/s", style);
        }
    }
    
    // Calculate expected velocity for a coin at given distance and mass
    float CalculateExpectedVelocity(float distance, float coinMass)
    {
        float playerMass = playerRigidbody != null ? playerRigidbody.mass : 80f;
        float weightFactor = playerMass / referenceMass;
        float force = pushForce * weightFactor;
        
        // Apply distance factor with zenith cap
        if (distance > 0.01f && distance <= maxRange)
        {
            float effectiveDistance = Mathf.Max(distance, minDistance);
            float distanceFactor = referenceDistance / effectiveDistance;
            distanceFactor = Mathf.Min(distanceFactor, 2f); // Zenith cap
            force *= distanceFactor;
        }
        else if (distance > maxRange)
        {
            force = 0f;
        }
        
        float impulseForce = force * impulseCalibration;
        return impulseForce / coinMass; // deltaV = impulse / mass
    }
    
    // Static helper to calculate push force (for tests or external use)
    // LORE: Force = baseForce × (playerMass/referenceMass) × (referenceDistance/distance), capped at 2x zenith
    public static float CalculatePushForce(float distance, float basePushForce, float playerMass, 
        float referenceMass = 80f, float referenceDistance = 3f, float maxRange = 30f, bool flaring = false)
    {
        float weightFactor = playerMass / referenceMass;
        float force = basePushForce * weightFactor;
        
        if (distance > 0.01f && distance <= maxRange)
        {
            float distanceFactor = referenceDistance / Mathf.Max(distance, 1f);
            distanceFactor = Mathf.Min(distanceFactor, 2f); // Zenith cap
            force *= distanceFactor;
        }
        else if (distance > maxRange)
        {
            force = 0f;
        }
        
        if (flaring) force *= 2f;
        
        return force;
    }
    
    void OnDestroy()
    {
        if (predictionLine != null)
        {
            Destroy(predictionLine.gameObject);
        }
    }
}
