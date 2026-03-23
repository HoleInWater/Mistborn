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
    public KeyCode swapMetalsKey = KeyCode.Tab; // Key to swap between primary and secondary
    
    [Header("References")]
    public Allomancer allomancer;
    public MetalHUD metalHUD;
    
    // Track both selected metals
    private AllomancySkill.MetalType primaryMetal;
    private AllomancySkill.MetalType secondaryMetal;
    private bool isPrimaryActive = true; // Which metal is currently active for E/Q
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponent<Allomancer>();
        
        if (metalHUD == null)
            metalHUD = FindObjectOfType<MetalHUD>();
        
        // Initialize with default metals (Steel primary, Iron secondary)
        primaryMetal = AllomancySkill.MetalType.Steel;
        secondaryMetal = AllomancySkill.MetalType.Iron;
        isPrimaryActive = true;
        
        // Set the active metal
        UpdateActiveMetal();
    }
    
    void Update()
    {
        // Update scroll cooldown timer
        if (scrollTimer > 0f)
            scrollTimer -= Time.deltaTime;
        
        // Handle scroll wheel input for metal selection
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0f && scrollTimer <= 0f)
        {
            // Scroll up = next metal, scroll down = previous metal
            // Apply to currently active metal (primary or secondary)
            if (scroll > 0f)
                SelectNextMetal();
            else if (scroll < 0f)
                SelectPreviousMetal();
            
            scrollTimer = scrollCooldown;
        }
        
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
        
        Debug.Log($"[MetalSelector] Selected {(isPrimaryActive ? "PRIMARY" : "SECONDARY")} metal: {nextMetal}");
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
        
        Debug.Log($"[MetalSelector] Selected {(isPrimaryActive ? "PRIMARY" : "SECONDARY")} metal: {prevMetal}");
    }
    
    void SwapMetals()
    {
        isPrimaryActive = !isPrimaryActive;
        UpdateActiveMetal();
        Debug.Log($"[MetalSelector] Swapped metals. Active is now: {(isPrimaryActive ? "PRIMARY" : "SECONDARY")}");
    }
    
    void UpdateActiveMetal()
    {
        // Notify the allomancer of the newly active metal via the public setter
        AllomancySkill.MetalType activeMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        allomancer.SetCurrentMetal(activeMetal);
        
        if (metalHUD != null)
            metalHUD.SetCurrentMetal(activeMetal);
    }
    
    // Public methods for other scripts to query selected metals
    public AllomancySkill.MetalType GetPrimaryMetal() => primaryMetal;
    public AllomancySkill.MetalType GetSecondaryMetal() => secondaryMetal;
    public AllomancySkill.MetalType GetActiveMetal() => isPrimaryActive ? primaryMetal : secondaryMetal;
    public bool IsPrimaryActive() => isPrimaryActive;
}
