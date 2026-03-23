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
using System.Collections;
using System.Collections.Generic;

public class SteelPush : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base force applied when pushing. Needs calibration for desired coin velocities.")]
    public float pushForce = 800f;
    [Tooltip("Reference distance where force factor = 1. Force = baseForce * (zenithDistance / distance).")]
    public float zenithDistance = 5f;
    [Tooltip("Minimum distance to prevent unrealistic forces at close range.")]
    public float minDistance = 0.5f;
    public float maxRange = 50f;
    public float metalCostPerSecond = 2f;
    
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
    
    [Header("Flight Mechanics")]
    [Tooltip("Extra upward force multiplier when pushing off anchored objects below (1 = normal)")]
    public float flightLaunchMultiplier = 1.5f;
    [Tooltip("Angle threshold (degrees) from downward to consider 'below' for flight boost")]
    public float flightAngleThreshold = 45f;
    
    [Header("Impulse Mode")]
    [Tooltip("Mass threshold (kg) below which objects receive impulse instead of continuous force")]
    public float impulseMassThreshold = 5f;
    [Tooltip("Calibration factor for impulse force (adjust to achieve target coin velocities)")]
    public float impulseCalibration = 0.001f;
    [Tooltip("Enable debug logging for impulse calibration")]
    public bool debugCalibration = false;
    
    private bool isBurning = false;
    private bool isFlaring = false;
    private bool wasFlaring = false;
    private Coroutine vignetteCoroutine;
    
    void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponentInParent<Rigidbody>();
        }
    }
    
    void Update()
    {
        // Check if Allomancer says we can't burn metal (out of metal)
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            StartBurning();
        }
        
        // Flaring: holding Shift while burning increases force
        isFlaring = Input.GetKey(KeyCode.LeftShift) && isBurning;
        
        // Detect flaring start for visual effect
        if (isFlaring && !wasFlaring)
        {
            StartFlaringVignette();
        }
        wasFlaring = isFlaring;
        
        if (Input.GetMouseButton(1) && isBurning)
        {
            PushMetals();
            DrainMetal();
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            StopBurning();
        }
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
        if (allomancer != null)
        {
            allomancer.StopBurning();
        }
#if UNITY_EDITOR
        Debug.Log("Stopped burning Steel");
#endif
    }
    
    void PushMetals()
    {
        if (playerRigidbody == null) return;
        
        // Detect all metal objects within maxRange radius, ignoring line-of-sight
        // LORE: Steel Push works through walls (blue lines in Spiritual Realm)
        Collider[] colliders = Physics.OverlapSphere(playerRigidbody.position, maxRange, metalLayer);
        
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
                targetMass = target.GetEffectiveMass();
            }
            else
            {
                targetMass = targetRigidbody.mass;
            }
            
            // Weight-proportional force: F = pushForce * (playerMass / targetMass)
            float weightFactor = playerMass / Mathf.Max(targetMass, 0.001f);
            float force = pushForce * weightFactor;
            
            // Distance from player to target
            Vector3 directionToTarget = targetRigidbody.position - playerRigidbody.position;
            float distance = directionToTarget.magnitude;
            
            // Distance falloff: force inversely proportional to distance
            // LORE: From Coppermind - "The force of the Push is inversely proportional to distance"
            if (distance > 0.01f) // Avoid division by zero
            {
                // Use minDistance to prevent unrealistic forces at very close range
                float effectiveDistance = Mathf.Max(distance, minDistance);
                float distanceFactor = zenithDistance / effectiveDistance;
                force *= distanceFactor;
            }
            
            // Clamp force to reasonable values
            force = Mathf.Clamp(force, 0f, pushForce * 10f);
            
            // Flaring doubles the force
            if (isFlaring) force *= 2f;
            
            // Anchor detection: if target is anchored (fixed) or kinematic, push player instead
            bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
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
                
                // Camera shake and sound for significant pushes (when pushing off anchored objects)
                if (force > shakeForceThreshold)
                {
                    ShakeCamera(shakeMagnitude);
                    PlayPushSound();
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
                }
                
                // Spawn visual effect at target position
                if (pushEffectPrefab != null && force > 50f)
                {
                    GameObject effect = Instantiate(pushEffectPrefab, targetRigidbody.position, Quaternion.identity);
                    Destroy(effect, 2f); // Auto-destroy after 2 seconds
                }
                
                // Camera shake and sound for significant pushes
                if (force > shakeForceThreshold)
                {
                    ShakeCamera(shakeMagnitude);
                    PlayPushSound();
                }
            }
        }
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        
        float drainAmount = metalCostPerSecond * Time.deltaTime;
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
}
