using UnityEngine;

public class MistbornController : MonoBehaviour
{
    [Header("Mistborn Status")]
    public bool isMistborn = true;
    
    [Header("All Allomancy Powers")]
    public AllomancyInputController allomancy;
    public FeruchemySystem feruchemy;
    public HemalurgySystem hemalurgy;
    
    [Header("Ability Flags")]
    public bool canUseAllMetals = true;
    public bool canCompound = true;
    
    [Header("Metal Costs")]
    public float normalCostMultiplier = 1f;
    public float enhancedCostMultiplier = 0.5f;
    
    void Start()
    {
        InitializeSystems();
    }
    
    void InitializeSystems()
    {
        if (allomancy == null)
            allomancy = GetComponent<AllomancyInputController>();
        if (feruchemy == null)
            feruchemy = GetComponent<FeruchemySystem>();
        if (hemalurgy == null)
            hemalurgy = GetComponent<HemalurgySystem>();
        
        if (feruchemy != null)
            feruchemy.isFeruchemist = true;
    }
    
    void Update()
    {
        if (!isMistborn) return;
        HandleMistbornAbilities();
    }
    
    void HandleMistbornAbilities()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            DisplayAllomancyInfo();
        }
    }
    
    void DisplayAllomancyInfo()
    {
        Debug.Log("=== MISTBORN ABILITIES ===");
        Debug.Log("Allomantic Powers:");
        Debug.Log("  [1] Steel Push - Push metals away");
        Debug.Log("  [2] Iron Pull - Pull metals toward");
        Debug.Log("  [3] Tin Enhance - Enhanced senses");
        Debug.Log("  [4] Pewter Burn - Physical enhancement");
        Debug.Log("  [5] Zinc Riot - Amplify emotions");
        Debug.Log("  [6] Brass Soothe - Calm emotions");
        Debug.Log("  [7] Copper Cloud - Hide from detection");
        Debug.Log("  [8] Bronze Detect - Sense Allomancy");
        Debug.Log("  [9] Gold Past - See alternate selves");
        Debug.Log("  [0] Electrum Future - See self's futures");
        Debug.Log("");
        Debug.Log("Control Keys:");
        Debug.Log("  [SHIFT] - Burn selected metal");
        Debug.Log("  [F] - Flare Pewter");
        Debug.Log("  [G] - Metal Flare");
        Debug.Log("  [B] - Time Bubble (Cadmium/Bendalloy)");
        Debug.Log("  [K] - Feruchemy Store");
        Debug.Log("  [L] - Feruchemy Tap");
        Debug.Log("  [J] - Compounding");
        Debug.Log("  [M] - This menu");
    }
    
    public void EnableAllomancy()
    {
        isMistborn = true;
        canUseAllMetals = true;
        Debug.Log("Mistborn abilities enabled!");
    }
    
    public void DisableAllomancy()
    {
        isMistborn = false;
        Debug.Log("Mistborn abilities disabled!");
    }
    
    public bool CanUseMetal(MetalType metal)
    {
        return isMistborn && canUseAllMetals;
    }
    
    public float GetMetalCost(MetalType metal, float baseCost)
    {
        float cost = baseCost * normalCostMultiplier;
        
        if (hemalurgy != null && hemalurgy.HasPower(metal))
        {
            cost *= enhancedCostMultiplier;
        }
        
        return cost;
    }
}
