using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Drives the Allomancy HUD:
///   - Two ProgressBars for the currently selected primary/secondary metals.
///   - A proportional arc ring in the bottom-right corner:
///       top half  = primary metal   (fills clockwise from 12-o'clock)
///       bottom half = secondary metal (fills clockwise from 6-o'clock)
///     As one metal drains its arc shrinks; the other's arc stays independent.
/// </summary>
[PlayerComponent("Allomancy", order: 40)]
public class MetalReserve : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;

    [Header("Metal Settings")]
    public float maxMetal = 100f;

    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 0.5f;

    [Header("Reserve Warning")]
    [Tooltip("Reserve value below which the low-reserve class is applied.")]
    public float lowReserveThreshold = 20f;

    [Header("Preview Mode")]
    [Tooltip("Auto-enabled when no Allomancer is found. Animates the ring without full game setup.")]
    public bool previewMode = false;

    // ── UI elements ────────────────────────────────────────────────────────────

    private ProgressBar      _primaryBar;
    private ProgressBar      _secondaryBar;
    private MetalRingVisual  _ring;
    private Label            _metalNameLabel;
    private Label            _metalPctLabel;

    // ── Selection state ────────────────────────────────────────────────────────

    private AllomancySkill.MetalType _primaryMetal   = AllomancySkill.MetalType.Steel;
    private AllomancySkill.MetalType _secondaryMetal = AllomancySkill.MetalType.Iron;
    private AllomancySkill.MetalType _activeMetal    = AllomancySkill.MetalType.Steel;

    // ── Preview state ──────────────────────────────────────────────────────────

    private float[] _previewReserves;

    // ── Legacy compat ──────────────────────────────────────────────────────────

    public float currentMetal { get; set; }
    private Dictionary<AllomancySkill.MetalType, ProgressBar> _metalBars =
        new Dictionary<AllomancySkill.MetalType, ProgressBar>();

    // ── Metal colours ──────────────────────────────────────────────────────────

    private static readonly Color[] MetalColors =
    {
        new Color(0.63f, 0.71f, 0.82f), // Steel
        new Color(0.39f, 0.47f, 0.63f), // Iron
        new Color(0.71f, 0.69f, 0.65f), // Pewter
        new Color(0.84f, 0.84f, 0.78f), // Tin
        new Color(0.82f, 0.73f, 0.31f), // Zinc
        new Color(0.73f, 0.58f, 0.18f), // Brass
        new Color(0.75f, 0.41f, 0.18f), // Copper
        new Color(0.65f, 0.43f, 0.16f), // Bronze
        new Color(0.86f, 0.86f, 0.94f), // Atium
        new Color(0.67f, 0.31f, 0.16f), // Malatium
        new Color(1.00f, 0.78f, 0.00f), // Gold
        new Color(0.90f, 0.88f, 0.67f), // Electrum
        new Color(0.82f, 0.82f, 0.82f), // Aluminum
        new Color(0.35f, 0.35f, 0.43f), // Duralumin
        new Color(0.94f, 0.63f, 0.24f), // Bendalloy
        new Color(0.31f, 0.63f, 0.86f), // Cadmium
        new Color(0.47f, 0.73f, 0.69f), // Chromium
        new Color(0.61f, 0.47f, 0.75f), // Nicrosil
    };

    static Color MetalColor(AllomancySkill.MetalType m)
    {
        int i = (int)m;
        return i >= 0 && i < MetalColors.Length ? MetalColors[i] : Color.white;
    }

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    void Start()
    {
        SetupUI();

        if (!previewMode
            && GetComponent<Allomancer>() == null
            && GetComponentInParent<Allomancer>() == null)
        {
            previewMode = true;
        }

        if (previewMode)
        {
            int count = System.Enum.GetValues(typeof(AllomancySkill.MetalType)).Length;
            _previewReserves = new float[count];
            for (int i = 0; i < count; i++) _previewReserves[i] = maxMetal;
        }
    }

    void Update()
    {
        if (!previewMode) return;
        if (_previewReserves == null) return;

        // Slowly drain primary metal so the ring visibly changes on load
        int idx = (int)_primaryMetal;
        _previewReserves[idx] -= Time.deltaTime * 10f;
        if (_previewReserves[idx] < 0f) _previewReserves[idx] = maxMetal;

        UpdateBarsAndRing(_previewReserves);
    }

    // ── Setup ──────────────────────────────────────────────────────────────────

    private void SetupUI()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        _primaryBar     = root.Q<ProgressBar>("PrimaryMetalBar");
        _secondaryBar   = root.Q<ProgressBar>("SecondaryMetalBar");
        _metalNameLabel = root.Q<Label>("MetalName");
        _metalPctLabel  = root.Q<Label>("MetalPct");

        if (_primaryBar   != null) { _primaryBar.lowValue   = 0; _primaryBar.highValue   = maxMetal; }
        if (_secondaryBar != null) { _secondaryBar.lowValue = 0; _secondaryBar.highValue = maxMetal; }

        // Inject the custom arc ring into the container
        var container = root.Q<VisualElement>("MetalRingContainer");
        if (container != null && _ring == null)
        {
            _ring = new MetalRingVisual();
            // Size to match the container (90×90 from USS)
            _ring.style.width  = 90;
            _ring.style.height = 90;
            container.Insert(0, _ring);  // behind the labels
        }

        if (_metalNameLabel != null) _metalNameLabel.text = _activeMetal.ToString().ToUpper();
        if (_metalPctLabel  != null) _metalPctLabel.text  = "100%";

        ApplyBarFillColor(_primaryBar,   _primaryMetal);
        ApplyBarFillColor(_secondaryBar, _secondaryMetal);
        RefreshRing(maxMetal, maxMetal);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void UpdateAllBars(float[] reserves)
    {
        if (_primaryBar == null) SetupUI();
        UpdateBarsAndRing(reserves);
    }

    public void HighlightSelection(AllomancySkill.MetalType primary,
                                   AllomancySkill.MetalType secondary,
                                   bool isPrimaryActive)
    {
        if (_primaryBar == null) SetupUI();

        bool primaryChanged   = primary   != _primaryMetal;
        bool secondaryChanged = secondary != _secondaryMetal;

        _primaryMetal   = primary;
        _secondaryMetal = secondary;
        _activeMetal    = isPrimaryActive ? primary : secondary;

        SetBarSelectionClasses(_primaryBar,   isPrimaryActive);
        SetBarSelectionClasses(_secondaryBar, !isPrimaryActive);

        if (primaryChanged)   ApplyBarFillColor(_primaryBar,   _primaryMetal);
        if (secondaryChanged) ApplyBarFillColor(_secondaryBar, _secondaryMetal);

        // Ring colours update immediately on selection change
        if (_ring != null)
            _ring.SetValues(
                GetReservePct(_primaryMetal),
                GetReservePct(_secondaryMetal),
                MetalColor(_primaryMetal),
                MetalColor(_secondaryMetal));

        if (_metalNameLabel != null)
            _metalNameLabel.text = _activeMetal.ToString().ToUpper();
    }

    public void VisualizePrimedState(AllomancySkill.MetalType metal, bool isPrimed)
    {
        ProgressBar bar = metal == _primaryMetal   ? _primaryBar
                        : metal == _secondaryMetal ? _secondaryBar
                        : null;
        if (bar != null)
        {
            if (isPrimed) bar.AddToClassList("burst-primed");
            else          bar.RemoveFromClassList("burst-primed");
        }
    }

    // ── Internal helpers ───────────────────────────────────────────────────────

    // Cached last-known reserves so HighlightSelection can query them
    private float[] _lastReserves;

    private float GetReservePct(AllomancySkill.MetalType metal)
    {
        if (_lastReserves == null) return 1f;
        int i = (int)metal;
        if (i < 0 || i >= _lastReserves.Length) return 1f;
        return _lastReserves[i] / maxMetal;
    }

    private void UpdateBarsAndRing(float[] reserves)
    {
        _lastReserves = reserves;

        UpdateSingleBar(_primaryBar,   _primaryMetal,   reserves);
        UpdateSingleBar(_secondaryBar, _secondaryMetal, reserves);

        int pi = (int)_primaryMetal;
        int si = (int)_secondaryMetal;
        float primaryVal   = pi >= 0 && pi < reserves.Length ? reserves[pi] : maxMetal;
        float secondaryVal = si >= 0 && si < reserves.Length ? reserves[si] : maxMetal;

        RefreshRing(primaryVal, secondaryVal);

        // Percentage label shows active metal
        int ai = (int)_activeMetal;
        if (_metalPctLabel != null && ai >= 0 && ai < reserves.Length)
            _metalPctLabel.text = $"{Mathf.FloorToInt(reserves[ai] / maxMetal * 100f)}%";
    }

    private void RefreshRing(float primaryVal, float secondaryVal)
    {
        if (_ring == null) return;
        _ring.SetValues(
            primaryVal   / maxMetal,
            secondaryVal / maxMetal,
            MetalColor(_primaryMetal),
            MetalColor(_secondaryMetal));
    }

    private void UpdateSingleBar(ProgressBar bar, AllomancySkill.MetalType metal, float[] reserves)
    {
        if (bar == null) return;
        int idx = (int)metal;
        if (idx < 0 || idx >= reserves.Length) return;

        float val = reserves[idx];
        bar.value = val;
        bar.title = $"{metal}  {Mathf.FloorToInt(val)}";

        if (val < lowReserveThreshold) bar.AddToClassList("low-reserve");
        else                           bar.RemoveFromClassList("low-reserve");
    }

    private void SetBarSelectionClasses(ProgressBar bar, bool isActive)
    {
        if (bar == null) return;
        bar.RemoveFromClassList("active-metal");
        bar.RemoveFromClassList("secondary-metal");
        bar.AddToClassList(isActive ? "active-metal" : "secondary-metal");
    }

    private void ApplyBarFillColor(ProgressBar bar, AllomancySkill.MetalType metal)
    {
        if (bar == null) return;
        var fill = bar.Q(className: "unity-progress-bar__progress");
        if (fill != null) fill.style.backgroundColor = MetalColor(metal);
    }

    // ── Legacy API ─────────────────────────────────────────────────────────────

    public void Drain(float amount)
    {
        var allo = GetComponent<Allomancer>();
        if (allo != null) allo.DrainMetal(allo.GetCurrentMetal(), amount);
    }

    public void Refill(float amount)
    {
        var allo = GetComponent<Allomancer>();
        if (allo != null) allo.RefillMetal(allo.GetCurrentMetal(), amount);
    }

    public void SetCurrentMetal(float amount) => currentMetal = amount;
}
