using UnityEngine;
using System.Collections;

/// <summary>
/// Handles metal selection via scroll wheel for Metallurgy system
/// Supports two-metal selection for quick switching (primary and secondary)
/// </summary>
[PlayerComponent("Metallurgy", order: 30)]
public class MetalSelector : MonoBehaviour
{
    [Header("Selection")]
    public float scrollCooldown = 0.2f;
    private float scrollTimer = 0f;
    
    [Header("Two-Metal Selection")]
    public KeyCode swapMetalsKey = KeyCode.LeftAlt; // Key to swap between primary and secondary
    
    [Header("References")]
    public Metallurgist metallurgist;
    public MetalReserve metalReserve;
    
    // Track both selected metals
    private MetallurgySkill.MetalType primaryMetal;
    private MetallurgySkill.MetalType secondaryMetal;
    private bool isPrimaryActive = true; // Which metal is currently active for E/Q
    
    void Start()
    {
        if (metallurgist == null)
            metallurgist = GetComponent<Metallurgist>();
        
        if (metalReserve == null)
            metalReserve = GetComponentInParent<MetalReserve>();
        
        // Initialize with default metals (Steel primary, Iron secondary)
        primaryMetal = MetallurgySkill.MetalType.Steel;
        secondaryMetal = MetallurgySkill.MetalType.Iron;
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
        if (metallurgist == null) return;
        
        MetallurgySkill.MetalType[] allMetals = (MetallurgySkill.MetalType[])System.Enum.GetValues(typeof(MetallurgySkill.MetalType));
        MetallurgySkill.MetalType currentMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        
        int currentIndex = System.Array.IndexOf(allMetals, currentMetal);
        int nextIndex = (currentIndex + 1) % allMetals.Length;
        MetallurgySkill.MetalType nextMetal = allMetals[nextIndex];
        
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
        if (metallurgist == null) return;
        
        MetallurgySkill.MetalType[] allMetals = (MetallurgySkill.MetalType[])System.Enum.GetValues(typeof(MetallurgySkill.MetalType));
        MetallurgySkill.MetalType currentMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        
        int currentIndex = System.Array.IndexOf(allMetals, currentMetal);
        int prevIndex = (currentIndex - 1 + allMetals.Length) % allMetals.Length;
        MetallurgySkill.MetalType prevMetal = allMetals[prevIndex];
        
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
        if (metallurgist == null) return;

        MetallurgySkill.MetalType activeMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        metallurgist.SetCurrentMetal(activeMetal);

        // Push highlight change to HUD immediately so the bar border swaps on the
        // same frame the player presses the swap key — don't wait for Metallurgist.Update().
        if (metalReserve == null)
            metalReserve = GetComponentInParent<MetalReserve>();
        if (metalReserve != null)
            metalReserve.HighlightSelection(primaryMetal, secondaryMetal, isPrimaryActive);
    }
    
    // Public methods for other scripts to query selected metals
    public MetallurgySkill.MetalType GetPrimaryMetal() => primaryMetal;
    public MetallurgySkill.MetalType GetSecondaryMetal() => secondaryMetal;
    public MetallurgySkill.MetalType GetActiveMetal() => isPrimaryActive ? primaryMetal : secondaryMetal;
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

    public void SetPrimaryMetal(MetallurgySkill.MetalType metal)
    {
        primaryMetal = metal;
        if (isPrimaryActive) UpdateActiveMetal();
    }

    public void SetSecondaryMetal(MetallurgySkill.MetalType metal)
    {
        secondaryMetal = metal;
        if (!isPrimaryActive) UpdateActiveMetal();
    }
}
