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
    private bool wasFlaring = false;
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
        
        if (Input.GetMouseButtonDown(1) && cooldownTimer <= 0f)
        {
            StartBurning();
        }
        
        // Flaring: holding Shift while burning increases force
        isFlaring = Input.GetKey("e") && isBurning;
        
        // Detect flaring start for visual effect
        if (isFlaring && !wasFlaring)
        {
            StartFlaringVignette();
        }
        wasFlaring = isFlaring;
        
        // Steel Bubble defensive ability
        if (enableSteelBubble && Input.GetKey(steelBubbleKey) && isBurning)
        {
            if (!isSteelBubbleActive)
            {
                isSteelBubbleActive = true;
                // Could add visual effect for bubble activation
            }
            PushMetalsInBubble();
            DrainMetal(steelBubbleMetalCostMultiplier);
        }
        else
        {
            isSteelBubbleActive = false;
            
            if (Input.GetMouseButton(1) && isBurning)
            {
                PushMetals();
                DrainMetal();
            }
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            StopBurning();
        }
        
        // Update targeted metal detection
        UpdateTargetedMetal();
        
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
#if UNITY_EDITOR
        Debug.Log("Burning Steel - Push ready");
#endif
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = pushCooldown; // Start cooldown after releasing
        if (allomancer != null)
        {
            allomancer.StopBurning();
        }
#if UNITY_EDITOR
        Debug.Log("Stopped burning Steel");
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
        // Show prediction when holding push button on a valid target
        bool shouldShowPrediction = enablePushPrediction && 
                                   showPredictionOnHold && 
                                   Input.GetMouseButton(1) && 
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
        if (playerRigidbody == null) return;
        
        // Detect all metal objects within maxRange radius, ignoring line-of-sight
        // LORE: Steel Push works through walls (blue lines in Spiritual Realm)
        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, maxRange, metalLayer);
        metalInRange = colliders.Length > 0;
        UpdateCrosshairColor();
        
        if (debugPushOperations && colliders.Length > 0)
        {
            Debug.Log($"Steel Push: Detected {colliders.Length} metal objects within {maxRange}m range");
        }
        
        float playerMass = playerRigidbody.mass;
        
        foreach (Collider collider in colliders)
        {
            Rigidbody targetRigidbody = collider.attachedRigidbody;
            if (targetRigidbody == null) continue;
            if (targetRigidbody == playerRigidbody) continue; // Skip player's own rigidbody
            
            // Get target mass (use AllomanticTarget if available, else Rigidbody mass)
            float targetMass = 1f;
            AllomanticTarget target = collider.GetComponent<AllomanticTarget>();
            if (target != null)
            {
                // Skip if target cannot be pushed (e.g., aluminum)
                if (!target.canBePushed) continue;
                targetMass = target.GetEffectiveMass();
            }
            else
            {
                targetMass = targetRigidbody.mass;
            }
            
            // Weight-proportional force: F = pushForce * (playerMass / referenceMass)
            float weightFactor = playerMass / referenceMass;
            float force = pushForce * weightFactor;
            
            // Distance from player to target
            Vector3 directionToTarget = targetRigidbody.position - playerRigidbody.position;
            float distance = directionToTarget.magnitude;
            
            // Anchor detection: if target is anchored (fixed) or kinematic, push player instead
            bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
            
            // Distance falloff: force inversely proportional to distance
            // LORE: From Coppermind - "The force of the Push is inversely proportional to distance"
            // LORE: "This continues until the Coinshot hits a zenith, or point of maximum altitude"
            // The zenith point is where force maxes out. Force increases until zenith, then decreases.
            if (distance > 0.01f && distance <= maxRange)
            {
                float effectiveDistance = Mathf.Max(distance, minDistance);
                
                // Calculate distance factor using inverse proportional (1/r)
                float distanceFactor = referenceDistance / effectiveDistance;
                
                // Zenith cap: force cannot exceed zenith multiplier (prevents infinite force at close range)
                // At referenceDistance (zenith), force = pushForce * weightFactor (distanceFactor = 1)
                // Closer than zenith: force would exceed base, but we cap at zenith * 2 for gameplay
                float zenithCap = 2f; // Maximum force multiplier at point-blank
                distanceFactor = Mathf.Min(distanceFactor, zenithCap);
                
                // Apply distance falloff
                force *= distanceFactor;
            }
            else if (distance > maxRange)
            {
                // Beyond max range: no force
                force = 0f;
            }
            
            // Clamp force to reasonable values
            force = Mathf.Clamp(force, 0f, pushForce * 10f);
            
            // Flaring doubles the force
            if (isFlaring) force *= 2f;
            
            if (debugPushOperations)
            {
                Debug.Log($"Steel Push: Target={collider.gameObject.name}, Distance={distance:F2}m, Mass={targetMass:F2}kg, Force={force:F2}N, Anchored={isAnchored}");
            }
            
            Vector3 pushDirection = directionToTarget.normalized;
            
            if (isAnchored)
            {
                // Push player away from anchored object
                Vector3 pushForceVector = -pushDirection * force * Time.deltaTime;
                
                // Flight mechanics: extra upward boost when pushing off objects below
                float angleFromDown = Vector3.Angle(-pushDirection, Vector3.down);
                if (angleFromDown < flightAngleThreshold)
                {
                    // Object is below player, apply flight boost
                    pushForceVector *= flightLaunchMultiplier;
                }
                
                playerRigidbody.AddForce(pushForceVector);
                
                if (debugPushOperations)
                {
                    Debug.Log($"Steel Push APPLIED: Pushed player from anchored object '{collider.gameObject.name}', Force={force:F2}N, Direction={-pushDirection}");
                }
                
                // Camera shake, sound, and screen tint for significant pushes (when pushing off anchored objects)
                if (force > shakeForceThreshold)
                {
                    ShakeCamera(shakeMagnitude);
                    PlayPushSound();
                    TriggerPushTint(force);
                }
            }
            else
            {
                // Normal push on target
                if (targetMass <= impulseMassThreshold)
                {
                    // Impulse mode for light objects (coins, small metal)
                    float impulseForce = force * impulseCalibration;
                    targetRigidbody.AddForce(pushDirection * impulseForce, ForceMode.Impulse);
                    
                    if (debugPushOperations)
                    {
                        float deltaV = impulseForce / targetMass;
                        Debug.Log($"Steel Push APPLIED: Impulse to '{collider.gameObject.name}', Mass={targetMass:F3}kg, ImpulseForce={impulseForce:F2}, DeltaV={deltaV:F2} m/s, Direction={pushDirection}");
                    }
                    
                    if (debugCalibration)
                    {
                        float deltaV = impulseForce / targetMass;
                        Debug.Log($"Impulse: mass={targetMass:F3}kg, impulseForce={impulseForce:F2}, deltaV={deltaV:F2} m/s, distance={distance:F2}m");
                    }
                }
                else
                {
                    // Continuous force for heavy objects
                    targetRigidbody.AddForce(pushDirection * force * Time.deltaTime);
                    
                    if (debugPushOperations)
                    {
                        Debug.Log($"Steel Push APPLIED: Continuous force to '{collider.gameObject.name}', Mass={targetMass:F2}kg, Force={force:F2}N, Direction={pushDirection}");
                    }
                }
                
                // Spawn visual effect at target position
                if (pushEffectPrefab != null && force > 50f)
                {
                    GameObject effect = Instantiate(pushEffectPrefab, targetRigidbody.position, Quaternion.identity);
                    Destroy(effect, 2f);
                }
                
                // Camera shake, sound, and screen tint for significant pushes
                if (force > shakeForceThreshold)
                {
                    ShakeCamera(shakeMagnitude);
                    PlayPushSound();
                    TriggerPushTint(force);
                }
            }
        }
        
        if (debugPushOperations && colliders.Length == 0)
        {
            Debug.Log($"Steel Push: No metal objects detected within {maxRange}m range");
        }
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
