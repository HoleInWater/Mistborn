using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic Sight — active automatically while burning Iron or Steel.
/// Blue lines from chest to all nearby metals. Closest metal gets an Unlit
/// highlight that matches the line tip colour and scales with proximity.
///
/// Everything is hidden when not burning, or when neither Iron nor Steel
/// occupies a slot in MetalSelector.
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
    [Tooltip("Not used for rendering — kept for backwards compatibility with PlayerAutoSetup.")]
    public Color closestHighlightColor = new Color(0.1f, 0.15f, 0.5f, 1f);
    [Tooltip("Emissive brightness multiplier at closest range.")]
    public float highlightMaxIntensity = 3f;
    [Tooltip("Emissive brightness multiplier at max range.")]
    public float highlightMinIntensity = 0.4f;

    [Header("References")]
    public Transform     chestPoint;
    public Allomancer    allomancer;
    public LayerMask     metalLayer;
    [Tooltip("Resolved lazily at runtime — can also be assigned manually in the Inspector.")]
    public MetalSelector metalSelector;

    // ── Line pool ─────────────────────────────────────────────────────────────
    private List<LineRenderer>  linePool    = new List<LineRenderer>();
    private List<MetalLineData> activeLines = new List<MetalLineData>();
    private float    updateTimer;
    private Material lineMaterial;

    // ── Closest metal tracking ────────────────────────────────────────────────
    private Transform closestMetalRoot;
    private Rigidbody closestMetalRigidbody;
    private float     closestMetalDistance;

    // ── Highlight state ───────────────────────────────────────────────────────
    private Renderer   highlightedRenderer;
    private Transform  highlightedRoot;
    private Material[] originalMaterials;
    private Material   highlightMaterial;

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

        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        lineMaterial = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));

        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader == null) unlitShader = Shader.Find("Hidden/InternalErrorShader");
        highlightMaterial = new Material(unlitShader);

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
        bool isBurning   = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool ironOrSteel = IronOrSteelSelected();
        bool active      = isBurning && ironOrSteel;

        if (!active)
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

        DrawLines();
        HighlightClosestMetal();
    }

    // ── Iron / Steel check ────────────────────────────────────────────────────

    MetalSelector GetMetalSelector()
    {
        if (metalSelector != null) return metalSelector;
        metalSelector = GetComponentInParent<MetalSelector>();
        if (metalSelector == null) metalSelector = GetComponentInChildren<MetalSelector>();
        if (metalSelector == null) metalSelector = FindObjectOfType<MetalSelector>();
        return metalSelector;
    }

    bool IronOrSteelSelected()
    {
        MetalSelector ms = GetMetalSelector();
        if (ms == null) return false;

        AllomancySkill.MetalType p = ms.GetPrimaryMetal();
        AllomancySkill.MetalType s = ms.GetSecondaryMetal();

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
        if (highlightedRoot != null && highlightedRoot != closestMetalRoot)
            ClearHighlight();

        if (closestMetalRoot == null)
        {
            ClearHighlight();
            return;
        }

        float proximity = 1f - Mathf.Clamp01(closestMetalDistance / maxRange);
        Color tipColor  = Color.Lerp(baseLineColor, closeLineColor, proximity);
        tipColor.a      = 1f;
        float intensity = Mathf.Lerp(highlightMinIntensity, highlightMaxIntensity, proximity);

        highlightMaterial.color = tipColor * intensity;

        if (highlightedRoot == closestMetalRoot && highlightedRenderer != null)
            return;

        Renderer r = closestMetalRoot.GetComponent<Renderer>();
        if (r == null) r = closestMetalRoot.GetComponentInChildren<Renderer>(true);
        if (r == null) return;

        originalMaterials = r.sharedMaterials;

        Material[] swapped = new Material[originalMaterials.Length];
        for (int i = 0; i < swapped.Length; i++)
            swapped[i] = highlightMaterial;
        r.materials = swapped;

        highlightedRenderer = r;
        highlightedRoot     = closestMetalRoot;
    }

    void ClearHighlight()
    {
        if (highlightedRenderer != null && originalMaterials != null)
            highlightedRenderer.materials = originalMaterials;

        highlightedRenderer = null;
        highlightedRoot     = null;
        originalMaterials   = null;
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
    public bool      IsActive()                 => FlareManager.Instance != null
                                                && FlareManager.Instance.IsBurning
                                                && IronOrSteelSelected();
}
