using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MetalHUDExtended : MonoBehaviour
{
    [Header("UI References")]
    public Image[] metalBarFills;
    public Text[] metalTexts;
    public Image[] metalIcons;
    public GameObject[] metalPanels;
    
    [Header("Metal Colors")]
    public Color physicalColor = new Color(0.7f, 0.7f, 0.7f);
    public Color enhancementColor = new Color(0.3f, 0.5f, 0.8f);
    public Color mentalColor = new Color(0.9f, 0.7f, 0.3f);
    public Color temporalColor = new Color(0.5f, 0.3f, 0.8f);
    public Color godMetalColor = new Color(1f, 0.8f, 0.2f);
    
    [Header("Settings")]
    public float lowThreshold = 25f;
    public float criticalThreshold = 10f;
    public Color normalColor = Color.white;
    public Color lowColor = Color.yellow;
    public Color criticalColor = Color.red;
    
    [Header("Events")]
    public UnityEvent<MetalType> OnMetalDepleted;
    public UnityEvent<MetalType> OnMetalLow;
    
    private MetalReserveManager metalManager;
    private MetalType[] displayOrder = new MetalType[]
    {
        MetalType.Iron, MetalType.Steel, MetalType.Tin, MetalType.Pewter,
        MetalType.Zinc, MetalType.Brass, MetalType.Copper, MetalType.Bronze,
        MetalType.Chromium, MetalType.Nicrosil, MetalType.Aluminum, MetalType.Duralumin,
        MetalType.Cadmium, MetalType.Bendalloy, MetalType.Gold, MetalType.Electrum
    };
    
    void Start()
    {
        metalManager = FindObjectOfType<MetalReserveManager>();
        InitializeUI();
    }
    
    void InitializeUI()
    {
        if (metalBarFills == null || metalBarFills.Length < 16) return;
        
        for (int i = 0; i < displayOrder.Length && i < metalBarFills.Length; i++)
        {
            if (metalBarFills[i] != null)
            {
                MetalType metal = displayOrder[i];
                metalBarFills[i].color = GetMetalColor(metal);
            }
        }
    }
    
    void Update()
    {
        UpdateMetalDisplays();
    }
    
    void UpdateMetalDisplays()
    {
        if (metalManager == null) return;
        
        for (int i = 0; i < displayOrder.Length && i < metalBarFills.Length; i++)
        {
            MetalType metal = displayOrder[i];
            float reserve = metalManager.GetReserve(metal);
            float percentage = reserve / 100f;
            
            if (metalBarFills[i] != null)
            {
                metalBarFills[i].fillAmount = percentage;
            }
            
            if (metalTexts[i] != null)
            {
                metalTexts[i].text = $"{Mathf.RoundToInt(reserve)}";
                metalTexts[i].color = GetTextColor(percentage);
            }
            
            if (metalPanels != null && metalPanels[i] != null)
            {
                UpdatePanelState(metalPanels[i], percentage);
            }
        }
    }
    
    Color GetMetalColor(MetalType metal)
    {
        switch (metal)
        {
            case MetalType.Iron:
            case MetalType.Steel:
            case MetalType.Tin:
            case MetalType.Pewter:
                return physicalColor;
            case MetalType.Zinc:
            case MetalType.Brass:
            case MetalType.Copper:
            case MetalType.Bronze:
                return mentalColor;
            case MetalType.Aluminum:
            case MetalType.Duralumin:
            case MetalType.Chromium:
            case MetalType.Nicrosil:
                return enhancementColor;
            case MetalType.Cadmium:
            case MetalType.Bendalloy:
            case MetalType.Gold:
            case MetalType.Electrum:
                return temporalColor;
            case MetalType.Atium:
            case MetalType.Malatium:
            case MetalType.Lerasium:
            case MetalType.Harmonium:
            case MetalType.Nalatium:
                return godMetalColor;
            default:
                return physicalColor;
        }
    }
    
    Color GetTextColor(float percentage)
    {
        if (percentage <= criticalThreshold / 100f) return criticalColor;
        if (percentage <= lowThreshold / 100f) return lowColor;
        return normalColor;
    }
    
    void UpdatePanelState(GameObject panel, float percentage)
    {
        if (percentage <= criticalThreshold / 100f)
        {
            panel.transform.localScale = Vector3.Lerp(panel.transform.localScale, Vector3.one * 0.9f, Time.deltaTime * 5f);
        }
        else if (percentage <= lowThreshold / 100f)
        {
            panel.transform.localScale = Vector3.one;
        }
        else
        {
            panel.transform.localScale = Vector3.one;
        }
    }
    
    public void ShowMetal(MetalType metal)
    {
        int index = System.Array.IndexOf(displayOrder, metal);
        if (index >= 0 && index < metalPanels.Length && metalPanels[index] != null)
        {
            metalPanels[index].SetActive(true);
        }
    }
    
    public void HideMetal(MetalType metal)
    {
        int index = System.Array.IndexOf(displayOrder, metal);
        if (index >= 0 && index < metalPanels.Length && metalPanels[index] != null)
        {
            metalPanels[index].SetActive(false);
        }
    }
    
    public void FlashMetal(MetalType metal)
    {
        int index = System.Array.IndexOf(displayOrder, metal);
        if (index >= 0 && index < metalBarFills.Length && metalBarFills[index] != null)
        {
            StartCoroutine(FlashCoroutine(metalBarFills[index]));
        }
    }
    
    System.Collections.IEnumerator FlashCoroutine(Image image)
    {
        Color originalColor = image.color;
        image.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        image.color = originalColor;
    }
}
