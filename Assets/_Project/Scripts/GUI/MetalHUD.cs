using UnityEngine;
using UnityEngine.UI;

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
}
