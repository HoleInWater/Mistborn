using UnityEngine;
using System.Collections;

/// <summary>
/// Handles metal selection via scroll wheel for Allomancy system
/// Supports two-metal selection for quick switching (primary and secondary)
/// </summary>
public class MetalSelector : MonoBehaviour
{
    [Header("Selection")]
    public float scrollCooldown = 0.2f;
    private float scrollTimer = 0f;
    
    [Header("Two-Metal Selection")]
    public KeyCode swapMetalsKey = KeyCode.Tab; // Key to swap between primary and secondary (matches Keybinds.MetalWheel)
    
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
        // Scroll wheel selects metal
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scrollTimer <= 0f)
        {
            if (scroll > 0f) { SelectNextMetal();     scrollTimer = scrollCooldown; }
            if (scroll < 0f) { SelectPreviousMetal(); scrollTimer = scrollCooldown; }
        }
        if (scrollTimer > 0f) scrollTimer -= Time.deltaTime;

        // Handle metal swap
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

        // Notify the allomancer of the newly active metal
        AllomancySkill.MetalType activeMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        allomancer.SetCurrentMetal(activeMetal);
        
        // HUD updates are now handled globally in Allomancer.Update() every frame
        // ensuring absolute synchronization between selection and visuals.
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
