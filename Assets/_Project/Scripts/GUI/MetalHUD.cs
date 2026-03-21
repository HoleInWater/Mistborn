using UnityEngine;
using UnityEngine.UI;

public class MetalHUD : MonoBehaviour
{
    [Header("Metal Display")]
    public Text currentMetalText;
    public Image metalIcon;
    public Slider metalReserveSlider;
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
