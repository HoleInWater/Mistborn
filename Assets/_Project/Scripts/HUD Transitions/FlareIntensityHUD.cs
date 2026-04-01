/* FlareIntensityHUD.cs
 *
 * Draws the flare intensity as 10 segments arranged in a ring around the
 * spinning metal indicator in the bottom-right corner.
 *
 * SETUP: Attach to any active GameObject. Reads FlareManager.Instance automatically.
 *
 * DISPLAY STATES:
 * - Not burning  → segments dimmed, label reads "FLARE"
 * - Burning      → segments fill clockwise from 12 o'clock, orange → red
 */

using UnityEngine;

public class FlareIntensityHUD : MonoBehaviour
{
    [Header("Ring Layout")]
    [Tooltip("Match the metal ring's right margin (HUD.uss: right:20px).")]
    public float ringMarginRight  = 20f;
    [Tooltip("Match the metal ring's bottom margin (HUD.uss: bottom:20px).")]
    public float ringMarginBottom = 20f;
    [Tooltip("Match the metal ring size (HUD.uss: 90px).")]
    public float ringSize         = 90f;
    [Tooltip("Radius at which flare segments orbit the ring.")]
    public float segmentRadius    = 58f;
    [Tooltip("Length (radial) of each segment.")]
    public float segmentLength    = 12f;
    [Tooltip("Width of each segment.")]
    public float segmentWidth     = 4f;

    [Header("Colors")]
    public Color lowColor    = new Color(1f, 0.55f, 0f, 1f);
    public Color midColor    = new Color(1f, 0.25f, 0f, 1f);
    public Color highColor   = new Color(1f, 0.05f, 0f, 1f);
    public Color offColor    = new Color(0.15f, 0.15f, 0.15f, 0.8f);
    public Color labelOnColor  = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color labelOffColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Range(0f, 1f)]
    public float idleAlpha = 0.3f;

    // ── Private ───────────────────────────────────────────────────────────────

    private Texture2D _pixel;
    private GUIStyle  _labelStyle;

    void Start()
    {
        _pixel = new Texture2D(1, 1);
        _pixel.SetPixel(0, 0, Color.white);
        _pixel.Apply();
    }

    void OnGUI()
    {
        if (FlareManager.Instance == null) return;

        bool  isBurning = FlareManager.Instance.IsBurning;
        int   intensity = FlareManager.Instance.Intensity;
        int   maxSteps  = FlareManager.Instance.maxIntensitySteps;
        float alpha     = isBurning ? 1f : idleAlpha;

        // Centre of the metal ring in OnGUI coordinates (y=0 at top)
        float cx = Screen.width  - ringMarginRight  - ringSize * 0.5f;
        float cy = Screen.height - ringMarginBottom - ringSize * 0.5f;

        // ── 10 radial segments around the ring ────────────────────────────────
        for (int i = 0; i < maxSteps; i++)
        {
            // Start at 12 o'clock (-90°), go clockwise
            float angleDeg = (360f / maxSteps) * i - 90f;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float segCx = cx + Mathf.Cos(angleRad) * segmentRadius;
            float segCy = cy + Mathf.Sin(angleRad) * segmentRadius;

            bool lit = isBurning && (i + 1) <= intensity;
            Color c;
            if (!lit)
            {
                c   = offColor;
                c.a *= alpha;
            }
            else
            {
                float t = (float)(i + 1) / maxSteps;
                c = t <= 0.4f ? lowColor : t <= 0.7f ? midColor : highColor;
            }

            // Rotate segment to point radially outward
            var savedMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angleDeg + 90f, new Vector2(segCx, segCy));
            GUI.color = c;
            GUI.DrawTexture(
                new Rect(segCx - segmentWidth * 0.5f, segCy - segmentLength * 0.5f,
                         segmentWidth, segmentLength),
                _pixel);
            GUI.color  = Color.white;
            GUI.matrix = savedMatrix;
        }

        // ── "FLARE" label just below the ring ─────────────────────────────────
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        Color lc = isBurning ? labelOnColor : labelOffColor;
        lc.a *= alpha;
        _labelStyle.normal.textColor = lc;

        string labelText = isBurning ? $"FLARE {intensity}/{maxSteps}" : "FLARE";
        float labelW  = 90f;
        float labelH  = 16f;
        float labelX  = cx - labelW * 0.5f;
        float labelY  = cy + ringSize * 0.5f + 4f;    // just below the ring

        GUI.Label(new Rect(labelX, labelY, labelW, labelH), labelText, _labelStyle);
    }

    void OnDestroy()
    {
        if (_pixel != null) Destroy(_pixel);
    }
}
