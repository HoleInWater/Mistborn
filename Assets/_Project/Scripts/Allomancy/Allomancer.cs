/* Allomancer.cs
 * 
 * PURPOSE:
 * Core Allomancy system that manages metal reserves, burning state, and coordination
 * with other Allomancy abilities (SteelPush, IronPull, etc.).
 * 
 * KEY FIELDS:
 * - metalReserves: Array of 16 floats representing metal reserves (one per metal type)
 * - currentMetal: Currently selected metal type for burning
 * - isBurningMetal: Whether the player is actively burning metal
 * - canBurnMetal: Whether the player can burn the current metal (false when reserve hits 0)
 * - MetalReserve: Reference to UI display for metal reserves
 * 
 * HOW IT WORKS:
 * - StartBurning/StopBurning: Control metal burning state
 * - DrainMetal/RefillMetal: Adjust metal reserves (called by abilities)
 * - Updates metal HUD when reserves change
 * - Automatically disables burning when metal reserve hits 0
 * 
 * IMPORTANT NOTES:
 * - Metal reserves start at 100% and deplete when burning
 * - canBurnMetal becomes false when current metal reserve <= 0
 * - Triggers UI warning when metal runs out
 * - Must be attached to the Player GameObject
 * 
 * LORE ACCURACY:
 * Allomancers burn metals to gain powers. Each metal type has different abilities.
 * Running out of metal disables allomancy until metal is replenished.
 */

using UnityEngine;

public class Allomancer : MonoBehaviour
{
    [Header("Metal State")]
    bool isBurningMetal = false;
    private AllomancySkill.MetalType currentMetal; // Set by MetalSelector via SetCurrentMetal()

    [Header("Metal Reserves")]
    public float[] metalReserves = new float[16];
    public bool canBurnMetal = true;
    
    [Header("HUD")]
    public MetalReserve metalReserve;
    
    // Reference to metal selector for getting current metal
    private MetalSelector metalSelector;
    
    void Start()
    {
        Debug.Log("[ALLOMANCER] Start() called");
        
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
        }
        
        EnsureAllomancyComponents();
        
        // Get reference to MetalSelector
        metalSelector = GetComponent<MetalSelector>();
        
        Debug.Log("[ALLOMANCER] Ready - canBurnMetal=" + canBurnMetal);
    }
    
    void EnsureAllomancyComponents()
    {
        if (GetComponent<SteelPush>() == null)
            gameObject.AddComponent<SteelPush>();
        
        if (GetComponent<IronPull>() == null)
            gameObject.AddComponent<IronPull>();
        
        if (GetComponent<FlareManager>() == null)
            gameObject.AddComponent<FlareManager>();
        
        if (GetComponent<MetalSelector>() == null)
            gameObject.AddComponent<MetalSelector>();
        
        if (GetComponent<MetalReserve>() == null)
            gameObject.AddComponent<MetalReserve>();
        
        if (GetComponent<MetalBurnEffect>() == null)
            gameObject.AddComponent<MetalBurnEffect>();
    }
    
    public void StartBurning(AllomancySkill.MetalType metal)
    {
        Debug.Log($"[ALLOMANCER] StartBurning({metal}) - reserve={(int)metal}=" + metalReserves[(int)metal]);
        isBurningMetal = true;
        canBurnMetal = metalReserves[(int)metal] > 0;
        Debug.Log($"[ALLOMANCER] canBurnMetal={canBurnMetal}");
    }
    
    public void StopBurning()
    {
        isBurningMetal = false;
    }
    
    public bool IsBurning()
    {
        return isBurningMetal;
    }

    /// <summary>
    /// Called by MetalSelector to update the active metal.
    /// </summary>
    public void SetCurrentMetal(AllomancySkill.MetalType metal)
    {
        currentMetal = metal;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public AllomancySkill.MetalType GetCurrentMetal()
    {
        // Get current metal from MetalSelector if available
        if (metalSelector != null)
            return metalSelector.GetActiveMetal();
        
        // Fallback to stored currentMetal
        return currentMetal;
    }
    
    public float GetMetalReserve(AllomancySkill.MetalType metal)
    {
        return metalReserves[(int)metal];
    }
    
    public void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);
        if (Time.frameCount % 60 == 0)
            Debug.Log($"[ALLOMANCER] DrainMetal({metal}, {amount:F2}) - reserve now: {metalReserves[(int)metal]:F1}");
        UpdateHUD(metal);
        
        AllomancySkill.MetalType activeMetal = GetCurrentMetal();
        if (metal == activeMetal)
        {
            canBurnMetal = metalReserves[(int)metal] > 0;
        }
    }
    
    public void RefillMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
        UpdateHUD(metal);
        
        AllomancySkill.MetalType activeMetal = GetCurrentMetal();
        if (metal == activeMetal)
        {
            canBurnMetal = metalReserves[(int)metal] > 0;
        }
    }

    private void UpdateHUD(AllomancySkill.MetalType metal)
    {
        if (metalReserve != null)
        {
            // Only update the HUD if the metal being changed is the one we are currently looking at
            if (metal == GetCurrentMetal())
            {
                // Set the bar to the ACTUAL remaining amount in the array
                metalReserve.currentMetal = metalReserves[(int)metal];
            }
        }
    }
    
    void Update()
    {
        // TOGGLE BURN WITH 'B' KEY
        if (Input.GetKeyDown(KeyCode.B)) 
        {
            if (isBurningMetal) StopBurning();
            else StartBurning(GetCurrentMetal());
        }
    
        // Check if we are currently using any metal
        bool isFlaring = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;
        bool isUsingMetal = isBurningMetal || isFlaring;
    
        if (isUsingMetal && canBurnMetal)
        {
            // Calculate total drain: 1 (base) + flare cost if active
            float currentBurnRate = 1f;
            if (isFlaring) currentBurnRate += FlareManager.Instance.flareBurnRate;
    
            DrainMetal(GetCurrentMetal(), currentBurnRate * Time.deltaTime);
        }
        else if (!isUsingMetal)
        {
            // Regenerate only when neither burning nor flaring
            RefillMetal(GetCurrentMetal(), metalReserve.passiveRecoveryRate * Time.deltaTime);
        }
    
        if (Input.GetKeyDown(KeyCode.R)) RefillAllMetals();
    }
    
    public void RefillAllMetals()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
            UpdateHUD((AllomancySkill.MetalType)i);
        }
        canBurnMetal = true;
        Debug.Log("[ALLOMANCER] All metal reserves refilled!");
    }
}
