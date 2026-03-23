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
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    
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
    [Tooltip("Enable E key for pushing (alternative to right mouse button)")]
    public bool enableEKeyPush = true;
    
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
    [Tooltip("Radius of steel bubble (meters)")]
    public float steelBubbleRadius = 3f;
    [Tooltip("Force applied by steel bubble")]
    public float steelBubbleForce = 500f;
    [Tooltip("Does steel bubble consume extra metal?")]
    public float steelBubbleMetalCostMultiplier = 2f;
    
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
    private Coroutine vignetteCoroutine;
    private bool metalInRange = false;
    private float cooldownTimer = 0f;
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
        
        // Update cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Start burning: Q key OR E key
        bool pushKeyDown = Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E);
        if (pushKeyDown && cooldownTimer <= 0f)
        {
            StartBurning();
            pushAppliedThisPress = false; // Reset for new push
        }
        
        // Flaring: Ctrl toggles flaring mode
        if (Input.GetKeyDown(KeyCode.LeftControl) && isBurning)
        {
            isFlaring = !isFlaring;
            if (debugPushOperations)
            {
                Debug.Log($"Flaring: {(isFlaring ? "ON" : "OFF")}");
            }
            if (isFlaring)
            {
                StartFlaringVignette();
            }
        }
        
        // Update targeted metal detection (always update for prediction line)
        UpdateTargetedMetal();
        
        // Steel Bubble defensive ability
        if (enableSteelBubble && Input.GetKey(steelBubbleKey) && isBurning)
        {
            if (!isSteelBubbleActive)
            {
                isSteelBubbleActive = true;
            }
            PushMetalsInBubble();
            DrainMetal(steelBubbleMetalCostMultiplier);
        }
        else
        {
            isSteelBubbleActive = false;
            
            // Push ONCE per key press (not continuously while held)
            bool pushKeyHeld = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E);
            if (pushKeyHeld && isBurning && !pushAppliedThisPress)
            {
                PushMetals();
                DrainMetal();
                pushAppliedThisPress = true;
            }
        }
        
        // Stop burning when releasing Q key OR E key
        bool pushKeyUp = Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E);
        if (pushKeyUp)
        {
            if (debugPushOperations) Debug.Log("E/Q released - stopping push");
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
        isFlaring = false; // Reset flaring when stopping
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
        bool isPushing = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E);
        
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
        Debug.Log($"[PUSH] Started - burning={isBurning}, flaring={isFlaring}");
        
        if (playerRigidbody == null)
        {
            Debug.LogError("[PUSH] ERROR: playerRigidbody is null!");
            return;
        }
        
        // Detect all metal objects within maxRange radius
        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, maxRange, metalLayer);
        
        Debug.Log($"[PUSH] Found {colliders.Length} metals in range ({maxRange}m)");
        
        if (colliders.Length == 0) return;
        
        float playerMass = playerRigidbody.mass;
        int pushedCount = 0;
        
        foreach (Collider collider in colliders)
        {
            Rigidbody targetRigidbody = collider.attachedRigidbody;
            if (targetRigidbody == null || targetRigidbody == playerRigidbody) continue;
            
            AllomanticTarget target = collider.GetComponent<AllomanticTarget>();
            if (target != null && !target.canBePushed) continue;
            
            float targetMass = target != null ? target.GetEffectiveMass() : targetRigidbody.mass;
            float distance = Vector3.Distance(playerRigidbody.position, targetRigidbody.position);
            
            Debug.Log($"[PUSH] {collider.name}: mass={targetMass:F1}kg, dist={distance:F1}m");
            
            // Weight-proportional force: F = pushForce * (playerMass / referenceMass)
            float weightFactor = playerMass / referenceMass;
            float force = pushForce * weightFactor;
            
            // Distance from player to target
            Vector3 directionToTarget = targetRigidbody.position - playerRigidbody.position;
            float distance = directionToTarget.magnitude;
            
            bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
            
            // Calculate force with distance falloff
            if (distance > 0.01f && distance <= maxRange)
            {
                float effectiveDistance = Mathf.Max(distance, minDistance);
                float distanceFactor = Mathf.Min(referenceDistance / effectiveDistance, 2f);
                force *= distanceFactor;
            }
            else
            {
                force = 0f;
            }
            
            force = Mathf.Clamp(force, 0f, pushForce * 10f);
            if (isFlaring) force *= 2f;
            
            Debug.Log($"[PUSH] Force calc: base={pushForce}, final={force:F0f}N, anchored={isAnchored}");
            
            Vector3 pushDirection = directionToTarget.normalized;
            
            if (force <= 0f)
            {
                Debug.Log($"[PUSH] Skipped {collider.name} - force is 0");
                continue;
            }
            
            if (isAnchored)
            {
                Vector3 pushForceVector = -pushDirection * force;
                playerRigidbody.AddForce(pushForceVector);
                Debug.Log($"[PUSH] Pushed PLAYER with {force:F0f}N!");
                pushedCount++;
            }
            else
            {
                targetRigidbody.AddForce(pushDirection * force, ForceMode.Impulse);
                Debug.Log($"[PUSH] Pushed {collider.name} with {force:F0f}N!");
                pushedCount++;
            }
            
            if (force > shakeForceThreshold)
            {
                ShakeCamera(shakeMagnitude);
                TriggerPushTint(force);
            }
        }
        
        Debug.Log($"[PUSH] Done - pushed {pushedCount} objects");
    }
    
    void PushMetalsInBubble()
    {
        if (playerRigidbody == null) return;
        
        // Detect all metal objects within steel bubble radius
        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, steelBubbleRadius, metalLayer);
        
        foreach (Collider collider in colliders)
        {
            Rigidbody targetRigidbody = collider.attachedRigidbody;
            if (targetRigidbody == null) continue;
            if (targetRigidbody == playerRigidbody) continue;
            
            // Get target mass and check if pushable
            float targetMass = 1f;
            AllomanticTarget target = collider.GetComponent<AllomanticTarget>();
            if (target != null)
            {
                if (!target.canBePushed) continue;
                targetMass = target.GetEffectiveMass();
            }
            else
            {
                targetMass = targetRigidbody.mass;
            }
            
            // Bubble pushes all objects equally regardless of distance (within radius)
            float force = steelBubbleForce;
            
            // Weight-proportional factor (bubble uses reference mass)
            float weightFactor = playerRigidbody.mass / referenceMass;
            force *= weightFactor;
            
            // Direction: radial from player center
            Vector3 direction = (targetRigidbody.position - playerRigidbody.position).normalized;
            
            // Anchor detection for bubble
            bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
            if (isAnchored)
            {
                // Push player away from anchored object
                playerRigidbody.AddForce(-direction * force * Time.deltaTime);
            }
            else
            {
                // Push object away from player
                targetRigidbody.AddForce(direction * force * Time.deltaTime);
            }
            
            // Visual effect for bubble push
            if (pushEffectPrefab != null && force > 50f)
            {
                GameObject effect = Instantiate(pushEffectPrefab, targetRigidbody.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            
            // Trigger screen tint for bubble pushes
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
