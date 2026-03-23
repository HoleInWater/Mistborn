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
using System.Collections.Generic;

public class IronPull : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base force applied when pulling. Needs calibration for desired coin velocities.")]
    public float pullForce = 800f;
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
        
        if (Input.GetMouseButtonDown(0))
        {
            StartBurning();
        }
        
        // Flaring: holding Shift while burning increases force
        isFlaring = Input.GetKey(KeyCode.LeftShift) && isBurning;
        
        if (Input.GetMouseButton(0) && isBurning)
        {
            PullMetals();
            DrainMetal();
        }
        
        if (Input.GetMouseButtonUp(0))
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
        if (allomancer != null)
        {
            allomancer.StopBurning();
        }
#if UNITY_EDITOR
        Debug.Log("Stopped burning Iron");
#endif
    }
    
    void PullMetals()
    {
        if (playerRigidbody == null) return;
        
        // Detect all metal objects within maxRange radius, ignoring line-of-sight
        // LORE: Iron Pull works through walls (blue lines in Spiritual Realm)
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
            
            // Weight-proportional force: F = pullForce * (playerMass / targetMass)
            float weightFactor = playerMass / Mathf.Max(targetMass, 0.001f);
            float force = pullForce * weightFactor;
            
            // Distance from player to target
            Vector3 directionToTarget = targetRigidbody.position - playerRigidbody.position;
            float distance = directionToTarget.magnitude;
            
            // Distance falloff: force inversely proportional to distance
            // LORE: From Coppermind - "The force of the Pull is inversely proportional to distance"
            if (distance > 0.01f) // Avoid division by zero
            {
                // Use minDistance to prevent unrealistic forces at very close range
                float effectiveDistance = Mathf.Max(distance, minDistance);
                float distanceFactor = zenithDistance / effectiveDistance;
                force *= distanceFactor;
            }
            
            // Clamp force to reasonable values
            force = Mathf.Clamp(force, 0f, pullForce * 10f);
            
            // Flaring doubles the force
            if (isFlaring) force *= 2f;
            
            // Anchor detection: if target is anchored (fixed) or kinematic, pull player instead
            bool isAnchored = (target != null && target.isAnchored) || targetRigidbody.isKinematic;
            Vector3 pullDirection = -directionToTarget.normalized; // Pull toward player
            
            if (isAnchored)
            {
                // Pull player toward anchored object
                playerRigidbody.AddForce(directionToTarget.normalized * force * Time.deltaTime);
            }
            else
            {
                // Normal pull on target toward player
                targetRigidbody.AddForce(pullDirection * force * Time.deltaTime);
            }
        }
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        
        float drainAmount = metalCostPerSecond * Time.deltaTime;
        if (isFlaring) drainAmount *= 3f; // Flaring drains 3x faster
        
        allomancer.DrainMetal(AllomancySkill.MetalType.Iron, drainAmount);
    }
}
