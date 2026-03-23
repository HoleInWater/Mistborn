using UnityEngine;

/// <summary>
/// Implements the Copper Allomancy ability (hides Allomantic pulses).
/// </summary>
public class Copper : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base metal burn rate per second")]
    public float metalCostPerSecond = 1f;
    [Tooltip("Radius of copper cloud that hides pulses (meters)")]
    public float cloudRadius = 5f;
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
        
        // Check if we can burn copper
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // C key to burn Copper (as per common Allomancy key bindings)
        if (Input.GetKeyDown(KeyCode.C) && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
        }
        
        if (Input.GetKeyUp(KeyCode.C))
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
        allomancer.StartBurning(AllomancySkill.MetalType.Copper);
        // Note: The actual copper cloud effect would hide the Allomancer's pulses from Seekers (Bronze users).
        // For now, we just log that we are burning copper.
        Debug.Log("[Copper] Burning Copper - hiding Allomantic pulses");
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = burnCooldown;
        allomancer.StopBurning();
        Debug.Log("[Copper] Stopped burning Copper");
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Copper, metalCostPerSecond * Time.deltaTime);
    }
}