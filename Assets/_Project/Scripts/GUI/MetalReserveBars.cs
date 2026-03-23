using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Extended MetalHUD that shows all metal reserves as bars
/// Highlights the currently selected primary and secondary metals
/// </summary>
public class MetalReserveBars : MonoBehaviour
{
    [Header("References")]
    public Allomancer allomancer;
    public MetalSelector metalSelector;
    public MetalHUD metalHUD;
    
    [Header("Bar Template")]
    public GameObject barTemplate;
    public Transform barsContainer;
    public Color[] metalColors; // Optional: assign colors per metal type
    
    [Header("Highlight Colors")]
    public Color primaryActiveColor = Color.yellow; // Color for active primary metal bar
    public Color secondaryActiveColor = Color.cyan; // Color for active secondary metal bar
    public Color normalColor = Color.white; // Color for inactive metals
    
    [Header("Layout")]
    public float barHeight = 20f;
    public float barSpacing = 5f;
    public float barWidth = 200f;
    
    private Dictionary<AllomancySkill.MetalType, Slider> metalBars = new Dictionary<AllomancySkill.MetalType, Slider>();
    private Dictionary<AllomancySkill.MetalType, Text> metalLabels = new Dictionary<AllomancySkill.MetalType, Text>();
    private Dictionary<AllomancySkill.MetalType, Image> barImages = new Dictionary<AllomancySkill.MetalType, Image>();
    
    void Start()
    {
        if (allomancer == null)
            allomancer = FindObjectOfType<Allomancer>();
        
        if (metalHUD == null)
            metalHUD = FindObjectOfType<MetalHUD>();
        
        if (metalSelector == null)
            metalSelector = FindObjectOfType<MetalSelector>();
        
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
        barImages.Clear();
        
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
            
            // Find slider, label, and background image components
            Slider slider = barObj.GetComponentInChildren<Slider>();
            Text label = barObj.GetComponentInChildren<Text>();
            Image bgImage = barObj.GetComponent<Image>();
            
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
                
                // Store background image for highlighting
                if (bgImage != null)
                    barImages[metal] = bgImage;
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
        UpdateBarHighlights();
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
    
    void UpdateBarHighlights()
    {
        if (metalSelector == null) return;
        
        AllomancySkill.MetalType primary = metalSelector.GetPrimaryMetal();
        AllomancySkill.MetalType secondary = metalSelector.GetSecondaryMetal();
        bool isPrimaryActive = metalSelector.IsPrimaryActive();
        
        foreach (var metal in barImages.Keys)
        {
            if (barImages.TryGetValue(metal, out Image img) && img != null)
            {
                // Reset to normal color first
                img.color = normalColor;
                
                // Then apply highlights if active
                if (metal == primary && isPrimaryActive)
                {
                    img.color = primaryActiveColor;
                }
                else if (metal == secondary && !isPrimaryActive)
                {
                    img.color = secondaryActiveColor;
                }
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