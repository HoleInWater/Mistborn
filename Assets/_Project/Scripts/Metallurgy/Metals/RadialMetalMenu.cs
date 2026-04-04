using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Radial metal selection wheel (Tab key). Shows all 16 metals in a circle.
/// Player moves mouse to select, release Tab to confirm.
/// Shows metal name, description, reserve amount, and burning status.
/// </summary>
[PlayerComponent("Metallurgy Support", order: 20)]
public class RadialMetalMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject wheelPanel;
    public RectTransform wheelCenter;
    public Text metalNameText;
    public Text metalDescriptionText;
    public Text metalReserveText;
    public Image metalColorIndicator;

    [Header("Settings")]
    public float wheelRadius = 200f;
    public float selectionDeadzone = 30f;
    public float timeSlowScale = 0.2f;

    [Header("References")]
    public Metallurgist metallurgist;
    public MetalSelector metalSelector;

    // Metal display data
    private static readonly string[] metalNames = {
        "Steel", "Iron", "Pewter", "Tin", "Zinc", "Brass", "Copper", "Bronze",
        "Oraculum", "Revelum", "Gold", "Electrum", "Aluminum", "Duralumin",
        "Bendalloy", "Cadmium", "Chromium", "Nicrosil"
    };

    private static readonly string[] metalDescriptions = {
        "Push metals away", "Pull metals toward you", "Enhanced strength and healing",
        "Enhanced senses (all five)", "Riot emotions (inflame)", "Soothe emotions (calm)",
        "Hide Metallurgic pulses", "Detect Metallurgic pulses",
        "See the future", "See another's past", "See your own past", "See your possible futures",
        "Purge your own metals", "Supercharge next metal burned",
        "Speed bubble (faster inside)", "Slow bubble (slower inside)",
        "Leech another's metals", "Amplify another's metals"
    };

    private bool isOpen = false;
    private int selectedIndex = -1;
    private readonly int metalCount = System.Enum.GetValues(typeof(MetallurgySkill.MetalType)).Length;
    private float previousTimeScale;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponentInParent<Metallurgist>();
        if (metalSelector == null) metalSelector = GetComponentInParent<MetalSelector>();
        if (wheelPanel != null) wheelPanel.SetActive(false);
        previousTimeScale = 1f;
    }

    void Update()
    {
        // Input handled by MetalWheelController + MetalWheelInputHandler
        if (isOpen)
            UpdateSelection();
    }

    public void OpenWheel()
    {
        isOpen = true;
        if (wheelPanel != null) wheelPanel.SetActive(true);

        // Slow time
        previousTimeScale = Time.timeScale;
        Time.timeScale = timeSlowScale;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SoundManager.Instance?.PlayMetalWheelOpen();
    }

    public void CloseAndSelect()
    {
        isOpen = false;
        if (wheelPanel != null) wheelPanel.SetActive(false);

        // Restore time
        Time.timeScale = previousTimeScale;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Apply selection
        if (selectedIndex >= 0 && selectedIndex < metalCount && metalSelector != null)
        {
            MetallurgySkill.MetalType metal = (MetallurgySkill.MetalType)selectedIndex;
            metalSelector.SetPrimaryMetal(metal);
            SoundManager.Instance?.PlayMetalWheelSelect();
        }
    }

    void UpdateSelection()
    {
        if (wheelCenter == null) return;

        // Get mouse position relative to wheel center
        Vector2 mousePos = Input.mousePosition;
        Vector2 centerPos = wheelCenter.position;
        Vector2 dir = mousePos - centerPos;

        if (dir.magnitude < selectionDeadzone)
        {
            selectedIndex = -1;
            UpdateDisplay(-1);
            return;
        }

        // Calculate angle and map to metal index
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float segmentAngle = 360f / metalCount;
        selectedIndex = Mathf.FloorToInt(angle / segmentAngle) % metalCount;

        UpdateDisplay(selectedIndex);
    }

    void UpdateDisplay(int index)
    {
        if (index < 0 || index >= metalCount)
        {
            if (metalNameText != null) metalNameText.text = "";
            if (metalDescriptionText != null) metalDescriptionText.text = "Move mouse to select a metal";
            if (metalReserveText != null) metalReserveText.text = "";
            return;
        }

        if (metalNameText != null) metalNameText.text = metalNames[index];
        if (metalDescriptionText != null && index < metalDescriptions.Length)
            metalDescriptionText.text = metalDescriptions[index];

        if (metallurgist != null && metalReserveText != null)
        {
            float reserve = metallurgist.GetMetalReserve((MetallurgySkill.MetalType)index);
            metalReserveText.text = $"Reserve: {reserve:F0}%";
        }

        if (metalColorIndicator != null)
            metalColorIndicator.color = GetMetalColor(index);
    }

    Color GetMetalColor(int index)
    {
        switch ((MetallurgySkill.MetalType)index)
        {
            case MetallurgySkill.MetalType.Steel:    return new Color(0.3f, 0.5f, 1f);
            case MetallurgySkill.MetalType.Iron:     return new Color(0.2f, 0.8f, 1f);
            case MetallurgySkill.MetalType.Pewter:   return new Color(0.8f, 0.2f, 0.2f);
            case MetallurgySkill.MetalType.Tin:      return new Color(1f, 1f, 0.5f);
            case MetallurgySkill.MetalType.Zinc:     return new Color(1f, 0.5f, 0f);
            case MetallurgySkill.MetalType.Brass:    return new Color(0.2f, 0.9f, 0.5f);
            case MetallurgySkill.MetalType.Copper:   return new Color(0.2f, 0.8f, 0.2f);
            case MetallurgySkill.MetalType.Bronze:   return new Color(0.8f, 0.3f, 0.8f);
            case MetallurgySkill.MetalType.Oraculum:     return new Color(0.9f,  0.9f,  1f);
            case MetallurgySkill.MetalType.Revelum:  return new Color(0.50f, 0.28f, 0.62f);
            case MetallurgySkill.MetalType.Gold:      return new Color(1f,    0.85f, 0.2f);
            case MetallurgySkill.MetalType.Electrum:  return new Color(0.90f, 0.90f, 0.40f);
            case MetallurgySkill.MetalType.Aluminum:  return new Color(0.90f, 0.92f, 0.96f);
            case MetallurgySkill.MetalType.Duralumin: return new Color(0.45f, 0.75f, 1.00f);
            case MetallurgySkill.MetalType.Bendalloy: return new Color(0.90f, 0.58f, 0.18f);
            case MetallurgySkill.MetalType.Cadmium:   return new Color(0.20f, 0.72f, 0.70f);
            default:                                  return Color.white;
        }
    }

    public bool IsOpen() => isOpen;
    public int GetSelectedIndex() => selectedIndex;
}
