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

public class SteelPush : MonoBehaviour
{
    [Header("Settings")]
    public float pushForce = 800f;
    public float zenithDistance = 5f;
    public float minDistance = 0.5f;
    public float maxRange = 50f;
    public float metalCostPerSecond = 2f;
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    
    private bool isBurning = false;
    private bool isFlaring = false;
    
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
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, metalLayer);
        
        float playerMass = playerRigidbody != null ? playerRigidbody.mass : 1f;
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.rigidbody != null)
            {
                // Get target mass (use AllomanticTarget if available, else Rigidbody mass)
                float targetMass = 1f;
                AllomanticTarget target = hit.collider.GetComponent<AllomanticTarget>();
                if (target != null)
                {
                    targetMass = target.GetEffectiveMass();
                }
                else
                {
                    targetMass = hit.rigidbody.mass;
                }
                
                // Weight-proportional force: F = pushForce * (playerMass / targetMass)
                float weightFactor = playerMass / Mathf.Max(targetMass, 0.001f);
                float force = pushForce * weightFactor;
                
                // Distance falloff: force inversely proportional to distance
                // LORE: From Coppermind - "The force of the Push is inversely proportional to distance"
                float distance = hit.distance;
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
                bool isAnchored = (target != null && target.isAnchored) || hit.rigidbody.isKinematic;
                Vector3 pushDirection = (hit.point - playerCamera.transform.position).normalized;
                
                if (isAnchored && playerRigidbody != null)
                {
                    // Push player away from anchored object
                    playerRigidbody.AddForce(-pushDirection * force * Time.deltaTime);
                }
                else
                {
                    // Normal push on target
                    hit.rigidbody.AddForce(pushDirection * force * Time.deltaTime);
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
}
