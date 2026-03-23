using UnityEngine;

/// <summary>
/// Implements the Aluminum Allomancy ability.
/// Purges all other metal reserves instantly when burned.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Aluminum : MonoBehaviour
{
    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Aluminum;

        if (isBurning && !wasBurning)
        {
            PurgeReserves();
        }
    }
    
    void PurgeReserves()
    {
        if (allomancer == null) return;
        
        Debug.Log("[ALUMINUM] Purging all metal reserves!");
        for (int i = 0; i < 16; i++)
        {
            AllomancySkill.MetalType metal = (AllomancySkill.MetalType)i;
            if (metal == AllomancySkill.MetalType.Aluminum) continue; // Keep aluminum? Or drain it too? 
            // Lore: It drains everything.
            allomancer.DrainMetal(metal, allomancer.GetMetalReserve(metal));
        }
        
        // Stop burning immediately after purge
        allomancer.StopBurning();
    }
}
