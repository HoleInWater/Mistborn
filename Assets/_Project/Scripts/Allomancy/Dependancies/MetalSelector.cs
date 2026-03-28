using UnityEngine;
using System.Collections;

/// <summary>
/// Handles metal selection via scroll wheel for Allomancy system
/// Supports two-metal selection for quick switching (primary and secondary)
/// </summary>
[PlayerComponent("Allomancy", order: 30)]
public class MetalSelector : MonoBehaviour
{
    [Header("Selection")]
    public float scrollCooldown = 0.2f;
    private float scrollTimer = 0f;
    
    [Header("Two-Metal Selection")]
    public KeyCode swapMetalsKey = KeyCode.LeftAlt; // Key to swap between primary and secondary
    
    [Header("References")]
    public Allomancer allomancer;
    public MetalReserve metalReserve;
    
    // Track both selected metals
    private AllomancySkill.MetalType primaryMetal;
    private AllomancySkill.MetalType secondaryMetal;
    private bool isPrimaryActive = true; // Which metal is currently active for E/Q
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponent<Allomancer>();
        
        if (metalReserve == null)
            metalReserve = GetComponentInParent<MetalReserve>();
        
        // Initialize with default metals (Steel primary, Iron secondary)
        primaryMetal = AllomancySkill.MetalType.Steel;
        secondaryMetal = AllomancySkill.MetalType.Iron;
        isPrimaryActive = true;
        
        // Set the active metal
        UpdateActiveMetal();
    }
    
    void Update()
    {
        // OLD CODE REMOVED: Scroll Wheel metal selection has been migrated exclusively to the
        // new MetalWheelController GUI system. This prevents catastrophic input collisions
        // when the user attempts to Flare their metals using the scroll wheel.

        // Auto-switch slot based on ability keys:
        //   E or F pressed → activate primary slot (if not already active)
        //   Q or V pressed → activate secondary slot (if not already active)
        bool primaryKeyPressed  = Input.GetKeyDown(Keybinds.Ability1) || Input.GetKeyDown(Keybinds.Ability3);
        bool secondaryKeyPressed = Input.GetKeyDown(Keybinds.Ability2) || Input.GetKeyDown(Keybinds.Ability4);

        if (primaryKeyPressed && !isPrimaryActive)
        {
            isPrimaryActive = true;
            UpdateActiveMetal();
        }
        else if (secondaryKeyPressed && isPrimaryActive)
        {
            isPrimaryActive = false;
            UpdateActiveMetal();
        }

        // Manual swap key still works as before.
        if (Input.GetKeyDown(swapMetalsKey))
        {
            SwapMetals();
        }
    }
    
    void SelectNextMetal()
    {
        if (allomancer == null) return;
        
        AllomancySkill.MetalType[] allMetals = (AllomancySkill.MetalType[])System.Enum.GetValues(typeof(AllomancySkill.MetalType));
        AllomancySkill.MetalType currentMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        
        int currentIndex = System.Array.IndexOf(allMetals, currentMetal);
        int nextIndex = (currentIndex + 1) % allMetals.Length;
        AllomancySkill.MetalType nextMetal = allMetals[nextIndex];
        
        if (isPrimaryActive)
        {
            primaryMetal = nextMetal;
        }
        else
        {
            secondaryMetal = nextMetal;
        }
        
        UpdateActiveMetal();
        
    }
    
    void SelectPreviousMetal()
    {
        if (allomancer == null) return;
        
        AllomancySkill.MetalType[] allMetals = (AllomancySkill.MetalType[])System.Enum.GetValues(typeof(AllomancySkill.MetalType));
        AllomancySkill.MetalType currentMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        
        int currentIndex = System.Array.IndexOf(allMetals, currentMetal);
        int prevIndex = (currentIndex - 1 + allMetals.Length) % allMetals.Length;
        AllomancySkill.MetalType prevMetal = allMetals[prevIndex];
        
        if (isPrimaryActive)
        {
            primaryMetal = prevMetal;
        }
        else
        {
            secondaryMetal = prevMetal;
        }
        
        UpdateActiveMetal();
        
    }
    
    void SwapMetals()
    {
        isPrimaryActive = !isPrimaryActive;
        UpdateActiveMetal();
    }
    
    void UpdateActiveMetal()
    {
        if (allomancer == null) return;

        AllomancySkill.MetalType activeMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        allomancer.SetCurrentMetal(activeMetal);

        // Push highlight change to HUD immediately so the bar border swaps on the
        // same frame the player presses the swap key — don't wait for Allomancer.Update().
        if (metalReserve == null)
            metalReserve = GetComponentInParent<MetalReserve>();
        if (metalReserve != null)
            metalReserve.HighlightSelection(primaryMetal, secondaryMetal, isPrimaryActive);
    }
    
    // Public methods for other scripts to query selected metals
    public AllomancySkill.MetalType GetPrimaryMetal() => primaryMetal;
    public AllomancySkill.MetalType GetSecondaryMetal() => secondaryMetal;
    public AllomancySkill.MetalType GetActiveMetal() => isPrimaryActive ? primaryMetal : secondaryMetal;
    public bool IsPrimaryActive() => isPrimaryActive;

    // [AGENT REVIEW] Added explicit setters so the UI Wheel can assign specific metals
    public void SetPrimaryActive(bool primaryStatus)
    {
        if (isPrimaryActive != primaryStatus)
        {
            isPrimaryActive = primaryStatus;
            UpdateActiveMetal();
        }
    }

    public void SetPrimaryMetal(AllomancySkill.MetalType metal)
    {
        primaryMetal = metal;
        if (isPrimaryActive) UpdateActiveMetal();
    }

    public void SetSecondaryMetal(AllomancySkill.MetalType metal)
    {
        secondaryMetal = metal;
        if (!isPrimaryActive) UpdateActiveMetal();
    }
}
