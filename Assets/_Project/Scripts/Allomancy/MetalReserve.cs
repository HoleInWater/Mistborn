using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MetalReserve : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;
    public string metalProgressBarPrefix = "Metal_";

    [Header("Metal Settings")]
    public float maxMetal = 100f;
    
    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 0.5f;

    private Dictionary<AllomancySkill.MetalType, ProgressBar> _metalBars = new Dictionary<AllomancySkill.MetalType, ProgressBar>();
    private float[] _lastDisplayedReserves = new float[20]; // Buffer for 18+ metals

    void Start()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        if (uiDocument == null) return;
        
        var root = uiDocument.rootVisualElement;
        _metalBars.Clear();

        foreach (AllomancySkill.MetalType metal in System.Enum.GetValues(typeof(AllomancySkill.MetalType)))
        {
            string barName = metalProgressBarPrefix + metal.ToString();
            ProgressBar bar = root.Q<ProgressBar>(barName);

            if (bar != null)
            {
                bar.lowValue = 0;
                bar.highValue = maxMetal;
                _metalBars.Add(metal, bar);
            }
            else
            {
                Debug.LogWarning($"[MetalReserve] Could not find ProgressBar named {barName} in UI.");
            }
        }
    }

    /// <summary>
    /// Updates all metal bars based on the provided reserves array.
    /// </summary>
    public void UpdateAllBars(float[] reserves)
    {
        if (_metalBars.Count == 0) SetupUI();

        foreach (var kvp in _metalBars)
        {
            int index = (int)kvp.Key;
            float currentValue = reserves[index];

            if (!Mathf.Approximately(_lastDisplayedReserves[index], currentValue))
            {
                kvp.Value.value = currentValue;
                kvp.Value.title = $"{kvp.Key}: {Mathf.FloorToInt(currentValue)}";
                _lastDisplayedReserves[index] = currentValue;
            }
        }
    }

    /// <summary>
    /// Highlights the primary and secondary selected metals in the HUD.
    /// Also hides any metals that are neither primary nor secondary to reduce screen clutter.
    /// </summary>
    public void HighlightSelection(AllomancySkill.MetalType primary, AllomancySkill.MetalType secondary, bool isPrimaryActive)
    {
        if (_metalBars.Count == 0) SetupUI();

        foreach (var kvp in _metalBars)
        {
            kvp.Value.RemoveFromClassList("active-metal");
            kvp.Value.RemoveFromClassList("secondary-metal");

            if (kvp.Key == (isPrimaryActive ? primary : secondary))
            {
                kvp.Value.AddToClassList("active-metal");
                kvp.Value.style.display = DisplayStyle.Flex;
            }
            else if (kvp.Key == (isPrimaryActive ? secondary : primary))
            {
                kvp.Value.AddToClassList("secondary-metal");
                kvp.Value.style.display = DisplayStyle.Flex;
            }
            else
            {
                // Hide unused metals
                kvp.Value.style.display = DisplayStyle.None;
            }
        }

        // Reorder Hierarchy (SendToBack pushes to index 0. Secondary first, then Primary ensures Primary is absolute first)
        if (_metalBars.TryGetValue(secondary, out var secBar)) secBar.SendToBack();
        if (_metalBars.TryGetValue(primary, out var priBar)) priBar.SendToBack();
    }

    /// <summary>
    /// visualizes the Duralumin/Nicrosil primed state.
    /// </summary>
    public void VisualizePrimedState(AllomancySkill.MetalType metal, bool isPrimed)
    {
        if (_metalBars.TryGetValue(metal, out ProgressBar bar))
        {
            if (isPrimed) bar.AddToClassList("burst-primed");
            else bar.RemoveFromClassList("burst-primed");
        }
    }

    // Legacy API — delegates to Allomancer for actual reserve management
    public float currentMetal { get; set; }

    public void Drain(float amount)
    {
        Allomancer allo = GetComponent<Allomancer>();
        if (allo != null) allo.DrainMetal(allo.GetCurrentMetal(), amount);
    }

    public void Refill(float amount)
    {
        Allomancer allo = GetComponent<Allomancer>();
        if (allo != null) allo.RefillMetal(allo.GetCurrentMetal(), amount);
    }

    public void SetCurrentMetal(float amount)
    {
        currentMetal = amount;
    }
}
