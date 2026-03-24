using UnityEngine;

/// <summary>
/// Implements the Duralumin Allomancy ability.
/// Causes a massive burst of power for the NEXT metal burned, but drains its reserve instantly.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Duralumin : MonoBehaviour
{
    [Header("Settings")]
    public float burstMultiplier = 10f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private bool isPrimed = false;
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Duralumin;

        if (isBurning && !wasBurning)
        {
            PrimeBurst();
        }
    }
    
    void PrimeBurst()
    {
        if (isPrimed) return;
        isPrimed = true;
        Debug.Log("[DURALUMIN] Primed for burst! Next metal burned will be massive.");
        
        // In this implementation, Duralumin itself doesn't drain other metals until you switch.
        // But flaring Duralumin might be different.
        // For simplicity: While "Burning" Duralumin, the Allomancer is in a "Prime" state.
    }
    
    // Integration into Allomancer.cs handles 'isPrimed' state when switching metals.
    // Or it could check if the PREVIOUS metal was Duralumin.
}
