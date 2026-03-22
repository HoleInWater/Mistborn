/* MetalHUD.cs
 * 
 * PURPOSE:
 * Heads-Up Display for Allomancy metal reserves and metal information.
 * Shows current metal type, reserve levels, and warnings.
 * 
 * KEY FIELDS:
 * - currentMetalText: UI Text showing current metal name
 * - metalIcon: UI Image for metal icon
 * - metalReserveSlider: UI Slider showing reserve percentage (0-100%)
 * - metalIndicators: Array of GameObjects for metal type indicators
 * - warningText: UI Text for out-of-metal warnings
 * 
 * HOW IT WORKS:
 * - Updates UI when metal reserves change
 * - Displays "Out of Metal!" warning when reserves hit 0
 * - Shows current metal type and icon
 * - Can toggle metal type indicators on/off
 * 
 * IMPORTANT NOTES:
 * - Attach to a UI Canvas
 * - Assign UI references in Inspector
 * - Call UpdateReserve() when metal reserves change
 * - Call SetCurrentMetal() when player switches metals
 * - Warning automatically appears for 2 seconds when metal runs out
 * 
 * LORE ACCURACY:
 * HUD shows metal reserves as percentages. In books, Mistborn know their metal reserves intuitively.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MetalHUD : MonoBehaviour
{
    [Header("Metal Display")]
    // NOTE: Consider adding [Tooltip("Text component to display current metal name")] attribute
    public Text currentMetalText;
    // NOTE: Consider adding [Tooltip("Image component for metal icon")] attribute
    public Image metalIcon;
    // NOTE: Consider adding [Tooltip("Slider showing metal reserve amount")] attribute
    public Slider metalReserveSlider;
    // NOTE: Consider adding [Tooltip("Array of GameObjects for metal type indicators")] attribute
    public GameObject[] metalIndicators;
    
    [Header("Warning")]
    [Tooltip("Text component to show out-of-metal warning")]
    public Text warningText;
    
    private AllomancySkill.MetalType currentMetal = AllomancySkill.MetalType.Steel;
    
    void Start()
    {
        UpdateMetalDisplay();
    }
    
    public void SetCurrentMetal(AllomancySkill.MetalType metal)
    {
        currentMetal = metal;
        UpdateMetalDisplay();
    }
    
    public void UpdateMetalDisplay()
    {
        if (currentMetalText != null)
        {
            currentMetalText.text = currentMetal.ToString();
        }
        
        if (metalReserveSlider != null)
        {
            metalReserveSlider.maxValue = 100f;
        }
    }
    
    public void UpdateReserve(float amount)
    {
        if (metalReserveSlider != null)
        {
            metalReserveSlider.value = amount;
        }
    }
    
    public void ShowMetalIndicator(AllomancySkill.MetalType metal, bool show)
    {
        int index = (int)metal;
        if (index < metalIndicators.Length && metalIndicators[index] != null)
        {
            metalIndicators[index].SetActive(show);
        }
    }
    
    public void ShowOutOfMetalWarning()
    {
        StartCoroutine(OutOfMetalWarningRoutine());
    }
    
    private IEnumerator OutOfMetalWarningRoutine()
    {
        if (warningText != null)
        {
            warningText.text = "Out of Metal!";
            yield return new WaitForSeconds(2f);
            warningText.text = "";
        }
    }
}
