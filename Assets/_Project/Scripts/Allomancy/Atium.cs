using UnityEngine;

/// <summary>
/// Implements the Atium Allomancy ability (see enemy futures).
/// </summary>
public class Atium : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base metal burn rate per second")]
    public float metalCostPerSecond = 2f; // Atium burns faster
    [Tooltip("Duration of future vision (seconds)")]
    public float visionDuration = 5f;
    [Tooltip("Cooldown time in seconds after stopping burn")]
    public float burnCooldown = 0.1f;
    
    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private float cooldownTimer = 0f;
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        
        // Check if we can burn atium
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // T key to burn Atium? Actually, Atium is not in the standard key bindings. 
        // We'll use Y for Atium? Actually, in the books, Atium is rare and not typically bound.
        // For simplicity, we'll bind it to the Y key (as per some fan adaptations).
        if (Input.GetKeyDown(KeyCode.Y) && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
        }
        
        if (Input.GetKeyUp(KeyCode.Y))
        {
            if (isBurning) StopBurning();
        }
        
        // Continuous metal drain while burning
        if (isBurning)
        {
            DrainMetal();
        }
    }
    
    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        cooldownTimer = burnCooldown;
        allomancer.StartBurning(AllomancySkill.MetalType.Atium);
        // Note: The actual future vision effect would be implemented by showing enemy actions.
        // For now, we just log that we are burning atium.
        Debug.Log("[Atium] Burning Atium - seeing enemy futures");
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = burnCooldown;
        allomancer.StopBurning();
        Debug.Log("[Atium] Stopped burning Atium");
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Atium, metalCostPerSecond * Time.deltaTime);
    }
}