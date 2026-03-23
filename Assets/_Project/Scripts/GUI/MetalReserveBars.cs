using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Extended MetalHUD that shows all metal reserves as bars
/// </summary>
public class MetalReserveBars : MonoBehaviour
{
    [Header("References")]
    public Allomancer allomancer;
    public MetalHUD metalHUD;
    
    [Header("Bar Template")]
    public GameObject barTemplate;
    public Transform barsContainer;
    public Color[] metalColors; // Optional: assign colors per metal type
    
    [Header("Layout")]
    public float barHeight = 20f;
    public float barSpacing = 5f;
    public float barWidth = 200f;
    
    private Dictionary<AllomancySkill.MetalType, Slider> metalBars = new Dictionary<AllomancySkill.MetalType, Slider>();
    private Dictionary<AllomancySkill.MetalType, Text> metalLabels = new Dictionary<AllomancySkill.MetalType, Text>();
    
    void Start()
    {
        if (allomancer == null)
            allomancer = FindObjectOfType<Allomancer>();
        
        if (metalHUD == null)
            metalHUD = FindObjectOfType<MetalHUD>();
        
        CreateMetalBars();
        UpdateAllReserves();
    }
    
    void CreateMetalBars()
    {
        if (barTemplate == null || barsContainer == null)
        {
            Debug.LogWarning("[MetalReserveBars] Missing barTemplate or barsContainer reference");
            return;
        }
        
        // Clear existing bars
        foreach (Transform child in barsContainer)
        {
            Destroy(child.gameObject);
        }
        metalBars.Clear();
        metalLabels.Clear();
        
        // Create bar for each metal type
        System.Array metalTypes = System.Enum.GetValues(typeof(AllomancySkill.MetalType));
        int index = 0;
        
        foreach (AllomancySkill.MetalType metal in metalTypes)
        {
            // Instantiate bar from template
            GameObject barObj = Instantiate(barTemplate, barsContainer);
            barObj.name = $"Bar_{metal}";
            
            // Position the bar
            RectTransform rect = barObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0, -index * (barHeight + barSpacing));
                rect.sizeDelta = new Vector2(barWidth, barHeight);
            }
            
            // Find slider and label components
            Slider slider = barObj.GetComponentInChildren<Slider>();
            Text label = barObj.GetComponentInChildren<Text>();
            
            if (slider != null)
            {
                slider.maxValue = 100f;
                slider.value = allomancer.GetMetalReserve(metal);
                metalBars[metal] = slider;
                
                // Set color if provided
                if (metalColors != null && metalColors.Length > (int)metal)
                {
                    Color barColor = metalColors[(int)metal];
                    slider.fillRect.GetComponent<Image>().color = barColor;
                }
            }
            
            if (label != null)
            {
                label.text = metal.ToString();
                metalLabels[metal] = label;
            }
            
            index++;
        }
    }
    
    void Update()
    {
        UpdateAllReserves();
    }
    
    void UpdateAllReserves()
    {
        if (allomancer == null) return;
        
        foreach (var metal in metalBars.Keys)
        {
            if (metalBars.TryGetValue(metal, out Slider slider) && slider != null)
            {
                slider.value = allomancer.GetMetalReserve(metal);
            }
        }
    }
    
    // Call this when metal reserves change
    public void OnMetalReserveChanged(AllomancySkill.MetalType metal, float newAmount)
    {
        if (metalBars.TryGetValue(metal, out Slider slider) && slider != null)
        {
            slider.value = newAmount;
        }
    }
}