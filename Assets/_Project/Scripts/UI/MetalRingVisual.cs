// MetalRingVisual.cs
//
// Custom UIToolkit element that draws two proportional arcs — one per
// selected metal. Each arc occupies half the ring (180°) and fills from
// its anchor point proportional to the metal's current reserve.
//
//   Primary   → top half, fills clockwise from 12-o'clock downward
//   Secondary → bottom half, fills clockwise from 6-o'clock upward
//
// Both at 100%  → complete circle
// Primary at 50% → top half-arc shrinks to a quarter arc
// Secondary drains → its bottom arc shrinks, making the imbalance obvious

using UnityEngine;
using UnityEngine.UIElements;

public class MetalRingVisual : VisualElement
{
    // ── Public data ────────────────────────────────────────────────────────────

    private float _primaryPct    = 1f;
    private float _secondaryPct  = 1f;
    private Color _primaryColor  = new Color(0.63f, 0.71f, 0.82f);
    private Color _secondaryColor = new Color(0.39f, 0.47f, 0.63f);

    // ── Drawing constants ──────────────────────────────────────────────────────

    private const float LINE_W       = 7f;    // arc stroke width
    private const float GAP_DEG      = 8f;    // degrees of gap between the two arcs
    private const float SPIN_DEG_PER_TICK = 0.75f; // degrees per 16 ms tick (~8 s/rotation)

    // ── Spin state ─────────────────────────────────────────────────────────────

    private float _spin = 0f;
    private IVisualElementScheduledItem _ticker;

    // ── Constructor ────────────────────────────────────────────────────────────

    public MetalRingVisual()
    {
        generateVisualContent += Draw;

        // Default size — can be overridden from UXML/USS
        style.width  = 90;
        style.height = 90;
        style.position = Position.Absolute;
        style.left = 0;
        style.top  = 0;

        // Start spinning once attached to a panel, pause when detached
        RegisterCallback<AttachToPanelEvent>(_ =>
            _ticker = schedule.Execute(Tick).Every(16));
        RegisterCallback<DetachFromPanelEvent>(_ =>
            _ticker?.Pause());
    }

    private void Tick()
    {
        _spin = (_spin + SPIN_DEG_PER_TICK) % 360f;
        MarkDirtyRepaint();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Update both arcs. primaryPct and secondaryPct are 0..1.
    /// Triggers a repaint automatically.
    /// </summary>
    public void SetValues(float primaryPct, float secondaryPct,
                          Color primaryColor, Color secondaryColor)
    {
        _primaryPct    = Mathf.Clamp01(primaryPct);
        _secondaryPct  = Mathf.Clamp01(secondaryPct);
        _primaryColor  = primaryColor;
        _secondaryColor = secondaryColor;
        MarkDirtyRepaint();
    }

    // ── Drawing ────────────────────────────────────────────────────────────────

    private void Draw(MeshGenerationContext ctx)
    {
        var p      = ctx.painter2D;
        var rect   = contentRect;
        var center = rect.center;
        float r    = Mathf.Min(rect.width, rect.height) * 0.42f;

        float halfGap = GAP_DEG * 0.5f;

        // ── Background ring (full dim circle) ─────────────────────────────────
        p.lineWidth   = LINE_W;
        p.strokeColor = new Color(0.25f, 0.25f, 0.25f, 0.55f);
        p.BeginPath();
        p.Arc(center, r, 0f, 360f, ArcDirection.Clockwise);
        p.Stroke();

        // ── Primary arc — top half, clockwise from 12-o'clock ─────────────────
        // 0° = right, so top (12 o'clock) = -90°. _spin rotates the whole ring.
        float primaryStart = -90f + halfGap + _spin;
        float primarySpan  = (180f - GAP_DEG) * _primaryPct;

        if (primarySpan > 0.5f)
        {
            p.lineWidth   = LINE_W;
            p.strokeColor = _primaryColor;
            p.BeginPath();
            p.Arc(center, r,
                  primaryStart,
                  primaryStart + primarySpan,
                  ArcDirection.Clockwise);
            p.Stroke();
        }

        // ── Secondary arc — bottom half, clockwise from 6-o'clock ─────────────
        float secondaryStart = 90f + halfGap + _spin;
        float secondarySpan  = (180f - GAP_DEG) * _secondaryPct;

        if (secondarySpan > 0.5f)
        {
            p.lineWidth   = LINE_W;
            p.strokeColor = _secondaryColor;
            p.BeginPath();
            p.Arc(center, r,
                  secondaryStart,
                  secondaryStart + secondarySpan,
                  ArcDirection.Clockwise);
            p.Stroke();
        }
    }
}
