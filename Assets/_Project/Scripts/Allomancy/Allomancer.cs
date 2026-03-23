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
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
        }
        
        SpawnTestCoins();
    }
    
    void SpawnTestCoins()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-3f, 3f),
                1f,
                Random.Range(-3f, 3f)
            );
            
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.name = $"TestCoin_{i}";
            coin.transform.position = spawnPos;
            coin.transform.localScale = new Vector3(0.08f, 0.08f, 0.02f);
            
            Rigidbody rb = coin.AddComponent<Rigidbody>();
            rb.mass = 0.005f;
            rb.drag = 0.5f;
            
            AllomanticTarget target = coin.AddComponent<AllomanticTarget>();
            target.canBePushed = true;
            target.canBePulled = true;
            target.isAnchored = false;
            target.metalType = AllomancySkill.MetalType.Steel;
            target.effectiveMass = 0.005f;
            
            Renderer renderer = coin.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = new Color(0.9f, 0.8f, 0.3f);
                renderer.material.SetFloat("_Metallic", 1f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }
            
            Debug.Log($"[ALLOMANCER] Spawned test coin at {spawnPos}");
        }
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
            if (!canBurnMetal && metalHUD != null)
            {
                metalHUD.ShowOutOfMetalWarning();
            }
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
}
