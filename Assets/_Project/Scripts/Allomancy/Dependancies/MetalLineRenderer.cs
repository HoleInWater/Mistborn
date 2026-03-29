using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic Sight — Left Ctrl toggle.
/// Blue lines from chest to all nearby metals. Closest metal gets a dark blue
/// mesh highlight (material tint). All others show lines only, no mesh change.
///
/// Lines and highlight are only active when Iron or Steel is the selected metal
/// in MetalSelector (primary or secondary slot).
///
/// HDRP highlight: drives _EmissiveColor so the glow is always visible
/// regardless of _BaseColor pipeline quirks. Brightness scales with proximity
/// so the highlight matches the line tip colour at that distance.
/// </summary>
[PlayerComponent("Allomancy", order: 50)]
public class MetalLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float maxRange        = 30f;
    public float lineBaseWidth   = 0.01f;
    public float lineMaxWidth    = 0.06f;
    public float massScaleFactor = 0.01f;
    public float updateInterval  = 0.15f;

    [Header("Line Colors")]
    public Color baseLineColor  = new Color(0.2f, 0.4f, 1f, 0.35f);
    public Color closeLineColor = new Color(0.3f, 0.6f, 1f, 0.6f);

    [Header("Closest Metal Line")]
    public Color closestLineColor = new Color(0.15f, 0.2f, 0.8f, 0.9f);

    [Header("Highlight")]
    [Tooltip("Emissive intensity multiplier at closest range (HDR brightness).")]
    public float highlightMaxIntensity = 4f;
    [Tooltip("Emissive intensity multiplier at max range.")]
    public float highlightMinIntensity = 0.5f;

    [Header("References")]
    public Transform     chestPoint;
    public Allomancer    allomancer;
    public LayerMask     metalLayer;
    public MetalSelector metalSelector;

    // ── Line pool ─────────────────────────────────────────────────────────────
    private List<LineRenderer>  linePool    = new List<LineRenderer>();
    private List<MetalLineData> activeLines = new List<MetalLineData>();
    private float    updateTimer;
    private Material lineMaterial;
    private bool     metalSightActive = false;

    // ── Closest metal tracking ────────────────────────────────────────────────
    private Transform closestMetalRoot;
    private Rigidbody closestMetalRigidbody;
    private float     closestMetalDistance;

    // ── Highlight state ───────────────────────────────────────────────────────
    private Renderer  highlightedRenderer;
    private Transform highlightedRoot;
    // Saved original emissive so we can restore exactly on clear
    private Color     originalEmissiveColor;
    private bool      hadEmissiveEnabled;

    struct MetalLineData
    {
        public Transform colTransform;
        public Transform root;
        public float     mass;
        public float     distance;
        public bool      isClosest;
    }

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (allomancer    == null) allomancer    = GetComponent<Allomancer>();
        if (chestPoint    == null) chestPoint    = transform;
        if (metalSelector == null) metalSelector = GetComponentInParent<MetalSelector>();

        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        lineMaterial = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));

        for (int i = 0; i < 50; i++)
        {
            GameObject go = new GameObject($"MetalLine_{i}");
            go.transform.SetParent(transform);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.material      = lineMaterial;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.gameObject.SetActive(false);
            linePool.Add(lr);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(Keybinds.MetalSight))
            metalSightActive = !metalSightActive;

        bool isBurning   = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool ironOrSteel = IronOrSteelSelected();

        if (!ironOrSteel)
        {
            HideAllLines();
            ClearHighlight();
            return;
        }

        if (!metalSightActive && !isBurning)
        {
            HideAllLines();
            ClearHighlight();
            return;
        }

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanMetals();
        }

        if (metalSightActive)
            DrawLines();
        else
            HideAllLines();

        HighlightClosestMetal();
    }

    // ── Iron / Steel check ────────────────────────────────────────────────────

    bool IronOrSteelSelected()
    {
        if (metalSelector == null) return true;
        var p = metalSelector.GetPrimaryMetal();
        var s = metalSelector.GetSecondaryMetal();
        return p == AllomancySkill.MetalType.Iron  || p == AllomancySkill.MetalType.Steel
            || s == AllomancySkill.MetalType.Iron  || s == AllomancySkill.MetalType.Steel;
    }

    // ── Scan ──────────────────────────────────────────────────────────────────

    void ScanMetals()
    {
        activeLines.Clear();
        closestMetalRoot      = null;
        closestMetalRigidbody = null;
        closestMetalDistance  = float.MaxValue;

        float flare          = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = maxRange * flare;

        Collider[] hits = Physics.OverlapSphere(chestPoint.position, effectiveRange, metalLayer);

        int closestIndex = -1;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

            Rigidbody rb   = col.attachedRigidbody;
            Transform root = rb != null ? rb.transform : col.transform;
            float     mass = rb != null ? rb.mass : 1f;
            float     dist = Vector3.Distance(chestPoint.position, col.transform.position);

            activeLines.Add(new MetalLineData
            {
                colTransform = col.transform,
                root         = root,
                mass         = mass,
                distance     = dist,
                isClosest    = false
            });

            if (dist < closestMetalDistance)
            {
                closestMetalDistance  = dist;
                closestIndex          = activeLines.Count - 1;
                closestMetalRigidbody = rb;
                closestMetalRoot      = root;
            }
        }

        if (closestIndex >= 0)
        {
            var data = activeLines[closestIndex];
            data.isClosest            = true;
            activeLines[closestIndex] = data;
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────────

    void DrawLines()
    {
        for (int i = 0; i < linePool.Count; i++)
            linePool[i].gameObject.SetActive(false);

        int count = Mathf.Min(activeLines.Count, linePool.Count);
        for (int i = 0; i < count; i++)
        {
            MetalLineData data = activeLines[i];
            if (data.colTransform == null || !data.colTransform.gameObject.activeInHierarchy) continue;

            LineRenderer lr = linePool[i];
            lr.gameObject.SetActive(true);
            lr.SetPosition(0, chestPoint.position);
            lr.SetPosition(1, data.colTransform.position);

            float width = Mathf.Clamp(lineBaseWidth + data.mass * massScaleFactor, lineBaseWidth, lineMaxWidth);

            if (data.isClosest)
            {
                lr.startWidth = width * 2f;
                lr.endWidth   = width * 0.8f;
                lr.startColor = closestLineColor;
                lr.endColor   = closestLineColor * 0.5f;
            }
            else
            {
                float proximity = 1f - Mathf.Clamp01(data.distance / maxRange);
                Color c = Color.Lerp(baseLineColor, closeLineColor, proximity);
                c.a *= 1f - Mathf.Clamp01(data.distance / maxRange) * 0.6f;
                lr.startWidth = width;
                lr.endWidth   = width * 0.3f;
                lr.startColor = c;
                lr.endColor   = c * 0.3f;
            }
        }
    }

    // ── Highlight ─────────────────────────────────────────────────────────────

    /// <summary>
    /// HDRP-compatible highlight. Rather than fighting _BaseColor pipeline flags,
    /// we enable emission on the instanced material and set _EmissiveColor to a
    /// proximity-scaled version of closestLineColor (same hue as the line tip).
    /// This is always visible in HDRP regardless of surface type.
    /// </summary>
    void HighlightClosestMetal()
    {
        // Target changed — restore the old object first
        if (highlightedRoot != null && highlightedRoot != closestMetalRoot)
            ClearHighlight();

        if (closestMetalRoot == null)
        {
            ClearHighlight();
            return;
        }

        // Compute proximity colour — same logic as the line tip
        float proximity = 1f - Mathf.Clamp01(closestMetalDistance / maxRange);

        // Line tip colour at this distance (matches closeLineColor at range 0,
        // baseLineColor at max range) — strip alpha so the emissive is fully opaque
        Color tipColor = Color.Lerp(baseLineColor, closeLineColor, proximity);
        tipColor.a = 1f;

        // HDR intensity: brighter when close, dimmer when far
        float intensity = Mathf.Lerp(highlightMinIntensity, highlightMaxIntensity, proximity);
        Color emissive  = tipColor * intensity; // HDR multiply gives bloom in HDRP

        // If we're already on the right object just update the emissive each frame
        // (distance changes continuously so we re-apply every Update)
        if (highlightedRoot == closestMetalRoot && highlightedRenderer != null)
        {
            Material mat = highlightedRenderer.material;
            if (mat != null && mat.HasProperty("_EmissiveColor"))
                mat.SetColor("_EmissiveColor", emissive);
            return;
        }

        // New target — find its renderer
        Renderer r = closestMetalRoot.GetComponent<Renderer>();
        if (r == null) r = closestMetalRoot.GetComponentInChildren<Renderer>(true);
        if (r == null) return;

        // Instance the material so we never touch the shared asset
        Material iMat = r.material; // Unity auto-instances here
        if (iMat == null) return;

        if (!iMat.HasProperty("_EmissiveColor"))
        {
            // Shader doesn't support emission — nothing we can do
            return;
        }

        // Save originals
        originalEmissiveColor = iMat.GetColor("_EmissiveColor");

        // Enable emission keywords so HDRP actually renders it
        // (harmless no-ops on shaders that don't use these keywords)
        iMat.EnableKeyword("_EMISSION");
        iMat.EnableKeyword("_EMISSIVE_COLOR_MAP");
        iMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        iMat.SetColor("_EmissiveColor", emissive);

        highlightedRenderer = r;
        highlightedRoot     = closestMetalRoot;
    }

    void ClearHighlight()
    {
        if (highlightedRenderer != null)
        {
            Material mat = highlightedRenderer.material;
            if (mat != null && mat.HasProperty("_EmissiveColor"))
            {
                mat.SetColor("_EmissiveColor", originalEmissiveColor);
                // Restore GI flags to whatever they were (baked is the default)
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
        }

        highlightedRenderer = null;
        highlightedRoot     = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void HideAllLines()
    {
        foreach (var lr in linePool)
            lr.gameObject.SetActive(false);
        activeLines.Clear();
    }

    void OnDisable()
    {
        ClearHighlight();
        HideAllLines();
        closestMetalRoot      = null;
        closestMetalRigidbody = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public Transform GetClosestMetal()          => closestMetalRoot;
    public Rigidbody GetClosestMetalRigidbody() => closestMetalRigidbody;
    public int       GetVisibleLineCount()      => activeLines.Count;
    public bool      IsActive()                 => metalSightActive;
}
