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
    public Text currentMetalText;
    public Image metalIcon;
    public Slider metalReserveSlider;
    public GameObject[] metalIndicators;
    
    [Header("Warning")]
    [Tooltip("Text component to show out-of-metal warning")]
    public Text warningText;
    
    [Header("Flare Display")]
    [Tooltip("Text to show flare status")]
    public Text flareStatusText;
    [Tooltip("Image for Iron flare indicator")]
    public Image ironFlareIndicator;
    [Tooltip("Image for Steel flare indicator")]
    public Image steelFlareIndicator;
    [Tooltip("Color when flared")]
    public Color flaredColor = Color.red;
    [Tooltip("Color when not flared")]
    public Color unflaredColor = Color.gray;
    
    private AllomancySkill.MetalType currentMetal = AllomancySkill.MetalType.Steel;
    
    void Start()
    {
        UpdateMetalDisplay();
    }
    
    void Update()
    {
        UpdateFlareDisplay();
    }
    
    void SubscribeToFlareEvents()
    {
        if (FlareManager.Instance != null)
        {
            FlareManager.Instance.OnIronFlareChanged += OnIronFlareChanged;
            FlareManager.Instance.OnSteelFlareChanged += OnSteelFlareChanged;
        }
    }
    
    void OnIronFlareChanged(bool isFlaring)
    {
        UpdateFlareDisplay();
    }
    
    void OnSteelFlareChanged(bool isFlaring)
    {
        UpdateFlareDisplay();
    }
    
    void UpdateFlareDisplay()
    {
        if (FlareManager.Instance == null) return;
        
        bool ironFlaring = FlareManager.Instance.IsIronFlaring;
        bool steelFlaring = FlareManager.Instance.IsSteelFlaring;
        
        Debug.Log($"[METAL HUD] UpdateFlareDisplay() - Iron: {ironFlaring}, Steel: {steelFlaring}");
        
        if (ironFlareIndicator != null)
        {
            ironFlareIndicator.color = ironFlaring ? flaredColor : unflaredColor;
        }
        
        if (steelFlareIndicator != null)
        {
            steelFlareIndicator.color = steelFlaring ? flaredColor : unflaredColor;
        }
        
        if (flareStatusText != null)
        {
            string status = "";
            if (ironFlaring) status += "IRON ";
            if (steelFlaring) status += "STEEL ";
            flareStatusText.text = status.Length > 0 ? "FLARING: " + status : "";
        }
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
