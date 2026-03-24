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
        allomancer.ClearAllReserves();
        
        // Stop burning immediately after purge
        allomancer.StopBurning();
    }
}
