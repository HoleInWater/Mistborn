/* FlareIntensityHUD.cs
 *
 * Draws 10 radial segments around the spinning metal ring using UI Toolkit
 * Painter2D — so they always sit exactly at the ring's screen position.
 *
 * SETUP:
 *   Attach to any active GameObject. The script finds the HUD UIDocument and
 *   injects itself into MetalRingContainer automatically. If you have multiple
 *   UIDocuments, drag the correct one into the 'uiDocument' field.
 *
 * The ring in HUD.uss is 90px × 90px; segments orbit just outside its edge.
 */

using UnityEngine;
using UnityEngine.UIElements;

public class FlareIntensityHUD : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Leave blank to auto-find the UIDocument that has MetalRingContainer.")]
    public UIDocument uiDocument;

    [Header("Ring Layout")]
    [Tooltip("Distance from ring centre to segment midpoint (ring radius = 45 px).")]
    public float segmentOrbitRadius = 48f;
    [Tooltip("Radial length of each segment in px.")]
    public float segmentLength = 9f;
    [Tooltip("Stroke width of each segment in px.")]
    public float segmentWidth = 3f;

    [Header("Colors")]
    public Color lowColor  = new Color(1f, 0.55f, 0f, 1f);
    public Color midColor  = new Color(1f, 0.25f, 0f, 1f);
    public Color highColor = new Color(1f, 0.05f, 0f, 1f);
    public Color offColor  = new Color(0.15f, 0.15f, 0.15f, 0.8f);

    [Range(0f, 1f)]
    public float idleAlpha = 0.3f;

    // ── Private ───────────────────────────────────────────────────────────────

    private FlareSegmentsVisual _visual;

    void Start()
    {
        // ── Find UIDocument ────────────────────────────────────────────────────
        if (uiDocument == null)
        {
            // Look through all UIDocuments for the one that has MetalRingContainer
            foreach (var doc in FindObjectsOfType<UIDocument>())
            {
                if (doc.rootVisualElement?.Q<VisualElement>("MetalRingContainer") != null)
                {
                    uiDocument = doc;
                    break;
                }
            }
        }

        if (uiDocument == null)
        {
            Debug.LogWarning("[FlareIntensityHUD] Could not find a UIDocument with MetalRingContainer.");
            return;
        }

        var container = uiDocument.rootVisualElement.Q<VisualElement>("MetalRingContainer");
        if (container == null)
        {
            Debug.LogWarning("[FlareIntensityHUD] MetalRingContainer not found in UXML.");
            return;
        }

        // Allow the visual to extend outside the 90×90 container
        container.style.overflow = Overflow.Visible;

        // The visual element must be large enough to contain all Painter2D drawing.
        // Centre it over the container (which is 90 × 90 px).
        float halfViz  = segmentOrbitRadius + segmentLength * 0.5f + 4f;
        float vizSize  = halfViz * 2f;
        float vizOffset = (90f - vizSize) * 0.5f;   // negative → extends outside container

        _visual = new FlareSegmentsVisual(
            segmentOrbitRadius, segmentLength, segmentWidth,
            lowColor, midColor, highColor, offColor, idleAlpha,
            vizSize);

        _visual.style.position = Position.Absolute;
        _visual.style.left     = vizOffset;
        _visual.style.top      = vizOffset;

        container.Add(_visual);
        Debug.Log($"[FlareIntensityHUD] Injected into MetalRingContainer " +
                  $"(vizSize={vizSize:F0}px, offset={vizOffset:F0}px).");
    }

    void Update()
    {
        if (_visual == null || FlareManager.Instance == null) return;

        _visual.UpdateData(
            FlareManager.Instance.IsBurning,
            FlareManager.Instance.Intensity,
            FlareManager.Instance.maxIntensitySteps);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Custom VisualElement — draws the 10 radial segments via Painter2D.
// Must NOT be in a separate .cs file (inner declaration in same assembly is fine).
// ─────────────────────────────────────────────────────────────────────────────

public class FlareSegmentsVisual : VisualElement
{
    // Layout constants
    private readonly float _orbitRadius;
    private readonly float _segLen;
    private readonly float _segWidth;

    // Colors
    private readonly Color _lowColor, _midColor, _highColor, _offColor;
    private readonly float _idleAlpha;

    // Runtime data (updated every frame from FlareIntensityHUD.Update)
    private bool _isBurning;
    private int  _intensity;
    private int  _maxSteps;

    public FlareSegmentsVisual(
        float orbitRadius, float segLen, float segWidth,
        Color lowColor, Color midColor, Color highColor,
        Color offColor, float idleAlpha,
        float vizSize)
    {
        _orbitRadius = orbitRadius;
        _segLen      = segLen;
        _segWidth    = segWidth;
        _lowColor    = lowColor;
        _midColor    = midColor;
        _highColor   = highColor;
        _offColor    = offColor;
        _idleAlpha   = idleAlpha;

        style.width  = vizSize;
        style.height = vizSize;

        // Do not block pointer events from reaching the ring below
        pickingMode = PickingMode.Ignore;

        generateVisualContent += Draw;
    }

    /// <summary>Called every frame by FlareIntensityHUD. Triggers repaint when data changes.</summary>
    public void UpdateData(bool isBurning, int intensity, int maxSteps)
    {
        if (_isBurning == isBurning && _intensity == intensity && _maxSteps == maxSteps)
            return;

        _isBurning = isBurning;
        _intensity = intensity;
        _maxSteps  = maxSteps;
        MarkDirtyRepaint();
    }

    private void Draw(MeshGenerationContext ctx)
    {
        if (_maxSteps <= 0) return;

        var painter = ctx.painter2D;
        var center  = contentRect.center;
        float alpha = _isBurning ? 1f : _idleAlpha;

        for (int i = 0; i < _maxSteps; i++)
        {
            // Start at 12 o'clock (−90°), advance clockwise
            float angleDeg = (360f / _maxSteps) * i - 90f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float cosA     = Mathf.Cos(angleRad);
            float sinA     = Mathf.Sin(angleRad);

            bool lit = _isBurning && (i + 1) <= _intensity;

            Color c;
            if (!lit)
            {
                c   = _offColor;
                c.a *= alpha;
            }
            else
            {
                float t = (float)(i + 1) / _maxSteps;
                c = t <= 0.4f ? _lowColor : (t <= 0.7f ? _midColor : _highColor);
            }

            // Segment midpoint on the orbit circle
            float mx = center.x + cosA * _orbitRadius;
            float my = center.y + sinA * _orbitRadius;

            // Endpoints along the outward radial direction
            float halfLen = _segLen * 0.5f;
            var segStart = new Vector2(mx - cosA * halfLen, my - sinA * halfLen);
            var segEnd   = new Vector2(mx + cosA * halfLen, my + sinA * halfLen);

            painter.lineWidth   = _segWidth;
            painter.strokeColor = c;
            painter.BeginPath();
            painter.MoveTo(segStart);
            painter.LineTo(segEnd);
            painter.Stroke();
        }
    }
}
