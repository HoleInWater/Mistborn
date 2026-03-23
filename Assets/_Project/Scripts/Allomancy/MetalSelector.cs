using UnityEngine;
using System.Collections;

/// <summary>
/// Handles metal selection via scroll wheel for Allomancy system
/// </summary>
public class MetalSelector : MonoBehaviour
{
    [Header("Selection")]
    public float scrollCooldown = 0.2f;
    private float scrollTimer = 0f;
    
    [Header("References")]
    public Allomancer allomancer;
    public MetalHUD metalHUD;
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponent<Allomancer>();
        
        if (metalHUD == null)
            metalHUD = FindObjectOfType<MetalHUD>();
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
            if (scroll > 0f)
                SelectNextMetal();
            else if (scroll < 0f)
                SelectPreviousMetal();
            
            scrollTimer = scrollCooldown;
        }
    }
    
    void SelectNextMetal()
    {
        if (allomancer == null) return;
        
        int currentIndex = (int)allomancer.currentMetal;
        int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(AllomancySkill.MetalType)).Length;
        AllomancySkill.MetalType nextMetal = (AllomancySkill.MetalType)nextIndex;
        
        allomancer.currentMetal = nextMetal;
        if (metalHUD != null)
            metalHUD.SetCurrentMetal(nextMetal);
        
        Debug.Log($"[MetalSelector] Selected metal: {nextMetal}");
    }
    
    void SelectPreviousMetal()
    {
        if (allomancer == null) return;
        
        int currentIndex = (int)allomancer.currentMetal;
        int prevIndex = (currentIndex - 1 + System.Enum.GetValues(typeof(AllomancySkill.MetalType)).Length) % System.Enum.GetValues(typeof(AllomancySkill.MetalType)).Length;
        AllomancySkill.MetalType prevMetal = (AllomancySkill.MetalType)prevIndex;
        
        allomancer.currentMetal = prevMetal;
        if (metalHUD != null)
            metalHUD.SetCurrentMetal(prevMetal);
        
        Debug.Log($"[MetalSelector] Selected metal: {prevMetal}");
    }
}