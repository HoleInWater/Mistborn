using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Drives the Allomancy HUD:
///   - Two ProgressBars for the currently selected primary/secondary metals.
///   - A spinning ring indicator in the bottom-right corner.
///
/// Preview mode auto-enables when no Allomancer is present so the ring is
/// always visible and spinning as soon as the scene loads.
/// </summary>
public class MetalReserve : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;

    [Header("Metal Settings")]
    public float maxMetal = 100f;

    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 0.5f;

    [Header("Reserve Warning")]
    [Tooltip("Reserve percentage below which the low-reserve warning class is applied.")]
    public float lowReserveThreshold = 20f;

    [Header("Preview Mode")]
    [Tooltip("Auto-enabled when no Allomancer is found. Animates bars and ring without full game setup.")]
    public bool previewMode = false;

    // ── UI elements ────────────────────────────────────────────────────────────

    private ProgressBar    _primaryBar;
    private ProgressBar    _secondaryBar;
    private VisualElement  _ringSpinner;
    private Label          _metalNameLabel;
    private Label          _metalPctLabel;

    // ── Selection state ────────────────────────────────────────────────────────

    private AllomancySkill.MetalType _primaryMetal   = AllomancySkill.MetalType.Steel;
    private AllomancySkill.MetalType _secondaryMetal = AllomancySkill.MetalType.Iron;
    private AllomancySkill.MetalType _activeMetal    = AllomancySkill.MetalType.Steel;

    // ── Spin / preview ─────────────────────────────────────────────────────────

    private float   _spinAngle      = 0f;
    private float[] _previewReserves;

    // ── Legacy compat (Allomancer.cs calls these) ──────────────────────────────
    public float currentMetal { get; set; }

    // Kept so existing callers that do metalReserve.UpdateAllBars() don't break.
    // Not used internally anymore.
    private Dictionary<AllomancySkill.MetalType, ProgressBar> _metalBars =
        new Dictionary<AllomancySkill.MetalType, ProgressBar>();

    // ── Per-metal fill colours (matches HUD.uss) ───────────────────────────────

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
        // Spin the ring arc continuously
        _spinAngle = (_spinAngle + Time.deltaTime * 180f) % 360f;
        if (_ringSpinner != null)
            _ringSpinner.style.rotate = new Rotate(Angle.Degrees(_spinAngle));

        if (!previewMode) return;

        // Preview: slowly drain the primary metal and cycle it back to full
        if (_previewReserves == null) return;
        int idx = (int)_primaryMetal;
        _previewReserves[idx] -= Time.deltaTime * 10f;
        if (_previewReserves[idx] < 0f) _previewReserves[idx] = maxMetal;

        // Force redraw every frame in preview (skip the dirty-check cache)
        UpdateBarsAndRing(_previewReserves, skipCache: true);
    }

    // ── Setup ──────────────────────────────────────────────────────────────────

    private void SetupUI()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        _primaryBar     = root.Q<ProgressBar>("PrimaryMetalBar");
        _secondaryBar   = root.Q<ProgressBar>("SecondaryMetalBar");
        _ringSpinner    = root.Q<VisualElement>("MetalRingSpinner");
        _metalNameLabel = root.Q<Label>("MetalName");
        _metalPctLabel  = root.Q<Label>("MetalPct");

        if (_primaryBar   != null) { _primaryBar.lowValue   = 0; _primaryBar.highValue   = maxMetal; }
        if (_secondaryBar != null) { _secondaryBar.lowValue = 0; _secondaryBar.highValue = maxMetal; }

        // Init ring label
        if (_metalNameLabel != null) _metalNameLabel.text = _activeMetal.ToString().ToUpper();
        if (_metalPctLabel  != null) _metalPctLabel.text  = "100%";

        ApplyBarFillColor(_primaryBar,   _primaryMetal);
        ApplyBarFillColor(_secondaryBar, _secondaryMetal);
        ApplyRingColor(_activeMetal);
    }

    // ── Public API (called by Allomancer.cs) ───────────────────────────────────

    /// <summary>Updates both selected metal bars and the ring from the full reserves array.</summary>
    public void UpdateAllBars(float[] reserves)
    {
        if (_primaryBar == null) SetupUI();
        UpdateBarsAndRing(reserves, skipCache: false);
    }

    /// <summary>
    /// Swaps which metal is highlighted as active vs secondary.
    /// Updates bar border styles and the ring colour/label.
    /// </summary>
    public void HighlightSelection(AllomancySkill.MetalType primary,
                                   AllomancySkill.MetalType secondary,
                                   bool isPrimaryActive)
    {
        if (_primaryBar == null) SetupUI();

        bool primaryChanged  = primary   != _primaryMetal;
        bool secondaryChanged = secondary != _secondaryMetal;
        bool activeChanged   = (isPrimaryActive ? primary : secondary) != _activeMetal;

        _primaryMetal   = primary;
        _secondaryMetal = secondary;
        _activeMetal    = isPrimaryActive ? primary : secondary;

        // Bar border classes
        SetBarSelectionClasses(_primaryBar,   isPrimaryActive);
        SetBarSelectionClasses(_secondaryBar, !isPrimaryActive);

        if (primaryChanged)  ApplyBarFillColor(_primaryBar,   _primaryMetal);
        if (secondaryChanged) ApplyBarFillColor(_secondaryBar, _secondaryMetal);
        if (activeChanged)   ApplyRingColor(_activeMetal);

        if (_metalNameLabel != null)
            _metalNameLabel.text = _activeMetal.ToString().ToUpper();
    }

    /// <summary>Shows/hides the gold burst-primed ring glow for Duralumin/Nicrosil.</summary>
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

        if (metal == _activeMetal && _ringSpinner != null)
        {
            if (isPrimed) _ringSpinner.AddToClassList("burst-primed");
            else          _ringSpinner.RemoveFromClassList("burst-primed");
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void UpdateBarsAndRing(float[] reserves, bool skipCache)
    {
        UpdateSingleBar(_primaryBar,   _primaryMetal,   reserves);
        UpdateSingleBar(_secondaryBar, _secondaryMetal, reserves);

        int activeIdx = (int)_activeMetal;
        if (activeIdx >= 0 && activeIdx < reserves.Length)
        {
            float pct = reserves[activeIdx] / maxMetal;
            if (_metalPctLabel != null)
                _metalPctLabel.text = $"{Mathf.FloorToInt(pct * 100f)}%";

            // Update ring arc colour based on reserve level
            Color arcColor = pct > 0.5f
                ? Color.Lerp(new Color(1f, 0.85f, 0f), MetalColor(_activeMetal), (pct - 0.5f) * 2f)
                : Color.Lerp(new Color(0.8f, 0.1f, 0.05f), new Color(1f, 0.85f, 0f), pct * 2f);

            if (_ringSpinner != null)
            {
                _ringSpinner.style.borderTopColor   = arcColor;
                _ringSpinner.style.borderRightColor = arcColor;
            }
        }
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

    private void ApplyRingColor(AllomancySkill.MetalType metal)
    {
        if (_ringSpinner == null) return;
        Color c = MetalColor(metal);
        _ringSpinner.style.borderTopColor   = c;
        _ringSpinner.style.borderRightColor = c;
    }

    // ── Legacy API ─────────────────────────────────────────────────────────────

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

    public void SetCurrentMetal(float amount) => currentMetal = amount;
}
