using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic Sight — Left Ctrl toggle.
/// Blue lines from chest to all nearby metals. Closest metal gets a dark blue
/// mesh highlight (material tint). All others show lines only, no mesh change.
///
/// Lines and highlight are only active when Iron or Steel is the selected metal
/// in MetalSelector (primary or secondary slot).
/// </summary>
[PlayerComponent("Allomancy", order: 50)]
public class MetalLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float maxRange       = 30f;
    public float lineBaseWidth  = 0.01f;
    public float lineMaxWidth   = 0.06f;
    public float massScaleFactor = 0.01f;
    public float updateInterval = 0.15f;

    [Header("Line Colors")]
    public Color baseLineColor  = new Color(0.2f, 0.4f, 1f, 0.35f);
    public Color closeLineColor = new Color(0.3f, 0.6f, 1f, 0.6f);

    [Header("Closest Metal Highlight")]
    [Tooltip("Dark blue tint applied to the closest metal's mesh")]
    public Color closestHighlightColor = new Color(0.1f, 0.15f, 0.5f, 1f);
    [Tooltip("Line color for the closest metal")]
    public Color closestLineColor = new Color(0.15f, 0.2f, 0.8f, 0.9f);

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
    private Transform closestMetalRoot;      // rigidbody root — stable ID for the object
    private Rigidbody closestMetalRigidbody;

    // ── Highlight state ───────────────────────────────────────────────────────
    private Renderer  highlightedRenderer;
    private Transform highlightedRoot;
    private Color     originalColor;
    private string    colorPropertyName;     // whichever property we actually set

    struct MetalLineData
    {
        public Transform colTransform; // collider transform (line endpoint)
        public Transform root;         // rigidbody root  (highlight key)
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

        bool isBurning      = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool ironOrSteel    = IronOrSteelSelected();

        // Lines/highlight are only relevant when Iron or Steel is equipped.
        // If neither is selected, clean up and stop.
        if (!ironOrSteel)
        {
            HideAllLines();
            ClearHighlight();
            return;
        }

        bool showLines = metalSightActive;

        // Also clean up if nothing is happening
        if (!showLines && !isBurning)
        {
            HideAllLines();
            ClearHighlight();
            return;
        }

        // Throttled scan — runs as long as sight is on OR a metal is burning
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanMetals();
        }

        if (showLines)
            DrawLines();
        else
            HideAllLines();

        // Show highlight whenever burning (so the player can see the push/pull target)
        // or whenever metal sight is on.
        HighlightClosestMetal();
    }

    // ── Iron / Steel check ────────────────────────────────────────────────────

    bool IronOrSteelSelected()
    {
        if (metalSelector == null) return true; // no selector → always show (safe fallback)

        AllomancySkill.MetalType primary   = metalSelector.GetPrimaryMetal();
        AllomancySkill.MetalType secondary = metalSelector.GetSecondaryMetal();

        return primary   == AllomancySkill.MetalType.Iron
            || primary   == AllomancySkill.MetalType.Steel
            || secondary == AllomancySkill.MetalType.Iron
            || secondary == AllomancySkill.MetalType.Steel;
    }

    // ── Scan ──────────────────────────────────────────────────────────────────

    void ScanMetals()
    {
        activeLines.Clear();
        closestMetalRoot      = null;
        closestMetalRigidbody = null;

        float flare          = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = maxRange * flare;

        Collider[] hits = Physics.OverlapSphere(chestPoint.position, effectiveRange, metalLayer);

        float closestDist  = float.MaxValue;
        int   closestIndex = -1;

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

            if (dist < closestDist)
            {
                closestDist           = dist;
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
        // Target changed — clear the old tint first
        if (highlightedRoot != null && highlightedRoot != closestMetalRoot)
            ClearHighlight();

        if (closestMetalRoot == null)
        {
            ClearHighlight();
            return;
        }

        // Already tinting the right object
        if (highlightedRoot == closestMetalRoot) return;

        // Find a renderer on or under the root
        Renderer r = closestMetalRoot.GetComponent<Renderer>();
        if (r == null) r = closestMetalRoot.GetComponentInChildren<Renderer>(true);
        if (r == null) return;

        // Grab the instanced material so we don't taint shared materials
        Material mat = r.material; // Unity auto-instances on first access
        if (mat == null) return;

        // Try every common shader color property in priority order.
        // Covers URP (_BaseColor), HDRP (_BaseColor + _EmissiveColor),
        // Standard (_Color), and Unlit (_Color).
        string[] candidates = { "_BaseColor", "_Color", "_MainColor", "_TintColor" };
        colorPropertyName = null;
        foreach (string prop in candidates)
        {
            if (mat.HasProperty(prop))
            {
                colorPropertyName = prop;
                break;
            }
        }

        if (colorPropertyName == null) return; // unknown shader — skip

        originalColor = mat.GetColor(colorPropertyName);
        mat.SetColor(colorPropertyName, closestHighlightColor);

        // HDRP emissive glow — harmless no-op on other pipelines
        if (mat.HasProperty("_EmissiveColor"))
            mat.SetColor("_EmissiveColor", closestHighlightColor * 0.5f);

        highlightedRenderer = r;
        highlightedRoot     = closestMetalRoot;
    }

    void ClearHighlight()
    {
        if (highlightedRenderer != null)
        {
            // highlightedRenderer.material is already the instanced copy we modified
            Material mat = highlightedRenderer.material;
            if (mat != null && colorPropertyName != null)
            {
                mat.SetColor(colorPropertyName, originalColor);
                if (mat.HasProperty("_EmissiveColor"))
                    mat.SetColor("_EmissiveColor", Color.black);
            }
        }

        highlightedRenderer = null;
        highlightedRoot     = null;
        colorPropertyName   = null;
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
