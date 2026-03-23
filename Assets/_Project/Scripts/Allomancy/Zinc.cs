using UnityEngine;

/// <summary>
/// Implements the Zinc Allomancy ability (Riot emotions).
/// </summary>
public class Zinc : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base metal burn rate per second")]
    public float metalCostPerSecond = 1f;
    [Tooltip("Radius of emotional riot effect (meters)")]
    public float effectRadius = 10f;
    [Tooltip("Intensity of the riot effect (0-1)")]
    public float riotIntensity = 0.5f;
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
        
        // Check if we can burn zinc
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // Z key to burn Zinc (as per common Allomancy key bindings)
        if (Input.GetKeyDown(KeyCode.Z) && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
        }
        
        if (Input.GetKeyUp(KeyCode.Z))
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
        allomancer.StartBurning(AllomancySkill.MetalType.Zinc);
        // Note: The actual riot effect would be implemented by affecting nearby NPCs or players.
        // For now, we just log that we are burning zinc.
        Debug.Log("[Zinc] Burning Zinc - Rioting emotions");
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = burnCooldown;
        allomancer.StopBurning();
        Debug.Log("[Zinc] Stopped burning Zinc");
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Zinc, metalCostPerSecond * Time.deltaTime);
    }
}