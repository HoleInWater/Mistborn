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

public class IronPull : MonoBehaviour
{
    [Header("Settings")]
    public float pullForce = 800f;
    public float zenithDistance = 5f;
    public float maxRange = 50f;
    public float metalCostPerSecond = 2f;
    
    [Header("References")]
    public Camera playerCamera;
    public LayerMask metalLayer;
    public Allomancer allomancer;
    public Rigidbody playerRigidbody;
    
    private float metalReserve = 100f;
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
        isBurning = true;
        Debug.Log("Burning Iron - Pull ready");
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped burning Iron");
    }
    
    void PullMetals()
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
                
                // Weight-proportional force: F = pullForce * (playerMass / targetMass)
                float weightFactor = playerMass / Mathf.Max(targetMass, 0.001f);
                float force = pullForce * weightFactor;
                
                // Distance falloff with zenith point at zenithDistance meters
                float distance = hit.distance;
                if (distance > 0.1f) // Avoid division by zero
                {
                    float distanceFactor;
                    if (distance <= zenithDistance)
                    {
                        distanceFactor = distance / zenithDistance; // Linear increase up to zenith
                    }
                    else
                    {
                        distanceFactor = zenithDistance / distance; // Inverse falloff beyond zenith
                    }
                    force *= distanceFactor;
                }
                
                // Clamp force to reasonable values
                force = Mathf.Clamp(force, 0f, pullForce * 10f);
                
                // Flaring doubles the force
                if (isFlaring) force *= 2f;
                
                // Anchor detection: if target is heavy or kinematic, pull player instead
                bool isAnchored = (targetMass > playerMass * 3) || hit.rigidbody.isKinematic;
                Vector3 pullDirection = (playerCamera.transform.position - hit.point).normalized;
                
                if (isAnchored && playerRigidbody != null)
                {
                    // Pull player toward anchored object
                    Vector3 pullTowardTarget = (hit.point - playerCamera.transform.position).normalized;
                    playerRigidbody.AddForce(pullTowardTarget * force * Time.deltaTime);
                }
                else
                {
                    // Normal pull on target
                    hit.rigidbody.AddForce(pullDirection * force * Time.deltaTime);
                }
            }
        }
    }
    
    void DrainMetal()
    {
        metalReserve -= metalCostPerSecond * Time.deltaTime;
        if (metalReserve <= 0)
        {
            metalReserve = 0;
            StopBurning();
        }
    }
    
    public float GetMetalReserve() => metalReserve;
    public void RefillMetal(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
