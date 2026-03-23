using UnityEngine;

/// <summary>
/// Implements the Brass Allomancy ability (Soothe emotions).
/// </summary>
public class Brass : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base metal burn rate per second")]
    public float metalCostPerSecond = 1f;
    [Tooltip("Radius of emotional soothe effect (meters)")]
    public float effectRadius = 10f;
    [Tooltip("Intensity of the soothe effect (0-1)")]
    public float sootheIntensity = 0.5f;
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
        
        // Check if we can burn brass
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // X key to burn Brass (as per common Allomancy key bindings)
        if (Input.GetKeyDown(KeyCode.X) && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
        }
        
        if (Input.GetKeyUp(KeyCode.X))
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
        allomancer.StartBurning(AllomancySkill.MetalType.Brass);
        // Note: The actual soothe effect would be implemented by affecting nearby NPCs or players.
        // For now, we just log that we are burning brass.
        Debug.Log("[Brass] Burning Brass - Soothing emotions");
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = burnCooldown;
        allomancer.StopBurning();
        Debug.Log("[Brass] Stopped burning Brass");
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Brass, metalCostPerSecond * Time.deltaTime);
    }
}