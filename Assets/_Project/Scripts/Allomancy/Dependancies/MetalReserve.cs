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

    [Header("Preview Mode")]
    [Tooltip("When enabled (or auto-triggered when no Allomancer is found), bars animate with test data so the HUD is visible without full game setup.")]
    public bool previewMode = false;

    private Dictionary<AllomancySkill.MetalType, ProgressBar> _metalBars = new Dictionary<AllomancySkill.MetalType, ProgressBar>();
    private float[] _lastDisplayedReserves = new float[20]; // Buffer for 18+ metals
    private float[] _previewReserves;

    void Start()
    {
        SetupUI();

        // Auto-enable preview mode if no Allomancer is present on the same GameObject
        if (!previewMode && GetComponent<Allomancer>() == null && GetComponentInParent<Allomancer>() == null)
            previewMode = true;

        if (previewMode)
        {
            int metalCount = System.Enum.GetValues(typeof(AllomancySkill.MetalType)).Length;
            _previewReserves = new float[metalCount];
            // Stagger starting phase per metal so they don't all pulse together
            for (int i = 0; i < metalCount; i++)
                _previewReserves[i] = (i * (100f / metalCount)) % 100f;
        }
    }

    void Update()
    {
        if (!previewMode) return;

        int count = System.Enum.GetValues(typeof(AllomancySkill.MetalType)).Length;
        if (_previewReserves == null || _previewReserves.Length < count) return;

        // Each bar cycles at a slightly different rate so they're visually distinct
        for (int i = 0; i < count; i++)
        {
            float speed = 8f + i * 1.3f;            // different depletion speeds per metal
            _previewReserves[i] -= Time.deltaTime * speed;
            if (_previewReserves[i] < 0f) _previewReserves[i] = maxMetal;
        }

        // Force update by clearing the cache so UpdateAllBars always writes
        for (int i = 0; i < _lastDisplayedReserves.Length; i++)
            _lastDisplayedReserves[i] = -1f;

        UpdateAllBars(_previewReserves);

        // Highlight bar 0 (Steel) as "active" for visual preview
        AllomancySkill.MetalType[] metals = (AllomancySkill.MetalType[])System.Enum.GetValues(typeof(AllomancySkill.MetalType));
        if (metals.Length >= 2)
            HighlightSelection(metals[0], metals[1], true);
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
            // Bar not found in UIDocument — will be null for this metal
        }
    }

    [Header("Reserve Warning")]
    [Tooltip("Reserve percentage below which the low-reserve warning class is applied.")]
    public float lowReserveThreshold = 20f;

    /// <summary>
    /// Updates all metal bars based on the provided reserves array.
    /// Applies low-reserve warning styling when a bar drops below the threshold.
    /// </summary>
    public void UpdateAllBars(float[] reserves)
    {
        if (_metalBars.Count == 0) SetupUI();

        foreach (var kvp in _metalBars)
        {
            int index = (int)kvp.Key;
            if (index < 0 || index >= reserves.Length) continue;
            float currentValue = reserves[index];

            if (!Mathf.Approximately(_lastDisplayedReserves[index], currentValue))
            {
                kvp.Value.value = currentValue;
                kvp.Value.title = $"{kvp.Key}: {Mathf.FloorToInt(currentValue)}";
                _lastDisplayedReserves[index] = currentValue;

                // Low-reserve warning — turns the fill red when nearly empty
                if (currentValue < lowReserveThreshold)
                    kvp.Value.AddToClassList("low-reserve");
                else
                    kvp.Value.RemoveFromClassList("low-reserve");
            }
        }
    }

    /// <summary>
    /// Highlights the active and secondary metals. All bars remain visible —
    /// active gets a bright white border, secondary gets a dotted grey border,
    /// everything else is shown at reduced opacity so the full picture is readable.
    /// </summary>
    public void HighlightSelection(AllomancySkill.MetalType primary, AllomancySkill.MetalType secondary, bool isPrimaryActive)
    {
        if (_metalBars.Count == 0) SetupUI();

        AllomancySkill.MetalType active = isPrimaryActive ? primary : secondary;
        AllomancySkill.MetalType passive = isPrimaryActive ? secondary : primary;

        foreach (var kvp in _metalBars)
        {
            var bar = kvp.Value;
            bar.RemoveFromClassList("active-metal");
            bar.RemoveFromClassList("secondary-metal");

            // All bars stay visible — dim inactive ones slightly
            if (kvp.Key == active)
            {
                bar.AddToClassList("active-metal");
                bar.style.opacity = 1f;
            }
            else if (kvp.Key == passive)
            {
                bar.AddToClassList("secondary-metal");
                bar.style.opacity = 0.85f;
            }
            else
            {
                bar.style.opacity = 0.45f;
            }
        }
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
