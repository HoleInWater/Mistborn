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
 * - metalHUD: Reference to UI display for metal reserves
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
    public AllomancySkill.MetalType currentMetal = AllomancySkill.MetalType.Steel;
    
    [Header("Metal Reserves")]
    public float[] metalReserves = new float[16];
    public bool canBurnMetal = true;

    [Header("HUD")]
    public MetalHUD metalHUD;
    
    void Start()
    {
        Debug.Log("[ALLOMANCER] Start() called");
        
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
        }
        
        EnsureAllomancyComponents();
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
    }
    
    public void StartBurning(AllomancySkill.MetalType metal)
    {
        currentMetal = metal;
        isBurningMetal = true;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }
    
    public void StopBurning()
    {
        isBurningMetal = false;
    }
    
    public bool IsBurning()
    {
        return isBurningMetal;
    }
    
    public AllomancySkill.MetalType GetCurrentMetal()
    {
        return currentMetal;
    }
    
    public float GetMetalReserve(AllomancySkill.MetalType metal)
    {
        return metalReserves[(int)metal];
    }
    
    public void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);
        UpdateHUD(metal);
        
        if (metal == currentMetal)
        {
            canBurnMetal = metalReserves[(int)metal] > 0;
        }
    }
    
    public void RefillMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
        UpdateHUD(metal);
        
        if (metal == currentMetal)
        {
            canBurnMetal = metalReserves[(int)metal] > 0;
        }
    }

    private void UpdateHUD(AllomancySkill.MetalType metal)
    {
        if (metalHUD != null)
        {
            metalHUD.UpdateReserve(metalReserves[(int)metal]);
            if (metal == currentMetal)
            {
                metalHUD.SetCurrentMetal(metal);
            }
        }
    }
    
    void Update()
    {
        // Press R to refill all metal reserves (for testing)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefillAllMetals();
        }
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
