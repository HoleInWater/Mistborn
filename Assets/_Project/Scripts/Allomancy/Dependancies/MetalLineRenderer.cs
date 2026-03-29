using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic Sight — MetalSight key toggle.
/// Blue lines from chest to all nearby metals. Closest metal gets an emissive
/// highlight that matches the line tip colour and scales with proximity.
///
/// Lines and highlight only show when Iron or Steel occupies the primary or
/// secondary metal slot in MetalSelector.
///
/// HDRP emissive: sets both _EmissiveColor and _EmissiveIntensity (required
/// when "Use Emission Intensity" is checked on the HDRP Lit material).
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

    [Header("Highlight (HDRP Emissive)")]
    [Tooltip("_EmissiveIntensity value at closest range.")]
    public float highlightMaxIntensity = 8f;
    [Tooltip("_EmissiveIntensity value at max range.")]
    public float highlightMinIntensity = 1f;

    [Header("References")]
    public Transform     chestPoint;
    public Allomancer    allomancer;
    public LayerMask     metalLayer;
    [Tooltip("Auto-found at Start. Assign manually if the component is on a different branch of the hierarchy.")]
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
    private Color     savedEmissiveColor;
    private float     savedEmissiveIntensity;

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
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (chestPoint == null) chestPoint = transform;

        // Search the whole scene for MetalSelector if not assigned or not found
        // via parent — handles any hierarchy layout.
        if (metalSelector == null) metalSelector = GetComponentInParent<MetalSelector>();
        if (metalSelector == null) metalSelector = GetComponentInChildren<MetalSelector>();
        if (metalSelector == null) metalSelector = FindObjectOfType<MetalSelector>();

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
        if (metalSelector == null) return true; // safe fallback if still not found

        AllomancySkill.MetalType p = metalSelector.GetPrimaryMetal();
        AllomancySkill.MetalType s = metalSelector.GetSecondaryMetal();

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
            Collider  col  = hits[i];
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

    void HighlightClosestMetal()
    {
        // Target changed — restore previous object before switching
        if (highlightedRoot != null && highlightedRoot != closestMetalRoot)
            ClearHighlight();

        if (closestMetalRoot == null)
        {
            ClearHighlight();
            return;
        }

        // Proximity colour — matches the line tip colour at this distance
        float proximity = 1f - Mathf.Clamp01(closestMetalDistance / maxRange);
        Color tipColor  = Color.Lerp(baseLineColor, closeLineColor, proximity);
        tipColor.a      = 1f;

        // Intensity scales with proximity: close = bright bloom, far = dim glow
        float intensity = Mathf.Lerp(highlightMinIntensity, highlightMaxIntensity, proximity);

        // Already on the right object — just update values each frame
        if (highlightedRoot == closestMetalRoot && highlightedRenderer != null)
        {
            Material mat = highlightedRenderer.material;
            if (mat != null)
            {
                // _EmissiveColor stores the HDR colour (linear, can exceed 1)
                if (mat.HasProperty("_EmissiveColor"))
                    mat.SetColor("_EmissiveColor", tipColor * intensity);

                // _EmissiveIntensity is the separate multiplier used when
                // "Use Emission Intensity" is checked in HDRP Lit
                if (mat.HasProperty("_EmissiveIntensity"))
                    mat.SetFloat("_EmissiveIntensity", intensity);
            }
            return;
        }

        // New target — find renderer
        Renderer r = closestMetalRoot.GetComponent<Renderer>();
        if (r == null) r = closestMetalRoot.GetComponentInChildren<Renderer>(true);
        if (r == null) return;

        // r.material auto-instances so we never modify the shared asset
        Material iMat = r.material;
        if (iMat == null) return;

        // Save originals so ClearHighlight can restore exactly
        savedEmissiveColor = iMat.HasProperty("_EmissiveColor")
            ? iMat.GetColor("_EmissiveColor") : Color.black;

        savedEmissiveIntensity = iMat.HasProperty("_EmissiveIntensity")
            ? iMat.GetFloat("_EmissiveIntensity") : 0f;

        // Enable emission keywords (required for HDRP to evaluate emissive at runtime)
        iMat.EnableKeyword("_EMISSION");
        iMat.EnableKeyword("_EMISSIVE_COLOR_MAP");
        iMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // Apply
        if (iMat.HasProperty("_EmissiveColor"))
            iMat.SetColor("_EmissiveColor", tipColor * intensity);

        if (iMat.HasProperty("_EmissiveIntensity"))
            iMat.SetFloat("_EmissiveIntensity", intensity);

        highlightedRenderer = r;
        highlightedRoot     = closestMetalRoot;
    }

    void ClearHighlight()
    {
        if (highlightedRenderer != null)
        {
            Material mat = highlightedRenderer.material;
            if (mat != null)
            {
                if (mat.HasProperty("_EmissiveColor"))
                    mat.SetColor("_EmissiveColor", savedEmissiveColor);

                if (mat.HasProperty("_EmissiveIntensity"))
                    mat.SetFloat("_EmissiveIntensity", savedEmissiveIntensity);

                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
        }

        highlightedRenderer    = null;
        highlightedRoot        = null;
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
