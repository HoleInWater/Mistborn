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
    private float[] _lastDisplayedReserves = new float[16];

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
            }
            else if (kvp.Key == (isPrimaryActive ? secondary : primary))
            {
                kvp.Value.AddToClassList("secondary-metal");
            }
        }
    }

    // Obsolete but kept for simple compatibility
    public float currentMetal { get; set; }
    public void Drain(float amount) { }
    public void Refill(float amount) { }
    public void SetCurrentMetal(float amount) { }
}
