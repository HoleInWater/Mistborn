using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic Sight — Left Ctrl toggle.
/// Blue lines from chest to all nearby metals. Closest metal gets a dark blue
/// mesh highlight (material tint). All others show lines only, no mesh change.
/// Lore: Mistborn see translucent blue lines to every nearby metal source.
/// </summary>
[PlayerComponent("Allomancy", order: 50)]
public class MetalLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float maxRange      = 30f;
    public float lineBaseWidth = 0.01f;
    public float lineMaxWidth  = 0.06f;
    public float massScaleFactor = 0.01f;
    public float pulseSpeed    = 3f;
    public float updateInterval = 0.15f;

    [Header("Line Colors")]
    public Color baseLineColor  = new Color(0.2f, 0.4f, 1f, 0.35f);
    public Color closeLineColor = new Color(0.3f, 0.6f, 1f, 0.6f);

    [Header("Closest Metal Highlight")]
    [Tooltip("Dark blue tint applied to the closest metal's mesh")]
    public Color closestHighlightColor = new Color(0.1f, 0.15f, 0.5f);
    [Tooltip("Line color for the closest metal")]
    public Color closestLineColor = new Color(0.15f, 0.2f, 0.8f, 0.9f);

    [Header("References")]
    public Transform chestPoint;
    public Allomancer allomancer;
    public LayerMask metalLayer;

    // ── Line pool ─────────────────────────────────────────────────────────────
    private List<LineRenderer> linePool   = new List<LineRenderer>();
    private List<MetalLineData> activeLines = new List<MetalLineData>();
    private float    updateTimer;
    private Material lineMaterial;
    private bool     metalSightActive = false;

    // ── Closest metal tracking ────────────────────────────────────────────────
    // We store the ROOT transform of the closest metal's rigidbody so that
    // the "is it still the same object?" check in HighlightClosestMetal() is
    // stable even when the collider sits on a child mesh object.
    private Transform closestMetalTransform;   // rigidbody root transform
    private Rigidbody closestMetalRigidbody;

    // Highlight state
    private Renderer closestHighlightedRenderer;
    private Color    closestOriginalColor;
    private Transform highlightedRoot;          // root we applied the tint to

    struct MetalLineData
    {
        public Transform target;   // collider transform (used for line endpoint)
        public Transform root;     // rigidbody root (used for highlight comparison)
        public float mass;
        public float distance;
        public bool  isClosest;
    }

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (chestPoint == null) chestPoint = transform;
        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        lineMaterial = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));

        for (int i = 0; i < 50; i++)
        {
            GameObject go = new GameObject($"MetalLine_{i}");
            go.transform.SetParent(transform);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.material        = lineMaterial;
            lr.positionCount   = 2;
            lr.useWorldSpace   = true;
            lr.gameObject.SetActive(false);
            linePool.Add(lr);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(Keybinds.MetalSight))
            metalSightActive = !metalSightActive;

        bool isBurning = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool showLines = metalSightActive;

        // Early out: nothing to do when sight is off AND not burning
        if (!showLines && !isBurning)
        {
            HideAllLines();
            ClearHighlight();
            return;
        }

        // Throttled scan — runs whether or not lines are visible, as long as
        // sight is on OR a metal is being burned.
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanMetals();
        }

        if (showLines)
        {
            DrawLines();
        }
        else
        {
            // Burning but sight is off: hide lines but still highlight closest
            // so the player can see what Steel/Iron is currently targeting.
            HideAllLines();
        }

        // Always highlight while burning or while sight is on
        HighlightClosestMetal();
    }

    // ── Scan ─────────────────────────────────────────────────────────────────

    void ScanMetals()
    {
        activeLines.Clear();
        closestMetalRigidbody  = null;
        closestMetalTransform  = null;

        float flare          = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = maxRange * flare;

        Collider[] hits = Physics.OverlapSphere(chestPoint.position, effectiveRange, metalLayer);

        float closestDist  = float.MaxValue;
        int   closestIndex = -1;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col.transform == transform) continue;

            Rigidbody rb   = col.attachedRigidbody;
            Transform root = rb != null ? rb.transform : col.transform;
            float     mass = rb != null ? rb.mass : 1f;
            float     dist = Vector3.Distance(chestPoint.position, col.transform.position);

            activeLines.Add(new MetalLineData
            {
                target    = col.transform,
                root      = root,
                mass      = mass,
                distance  = dist,
                isClosest = false
            });

            if (dist < closestDist)
            {
                closestDist           = dist;
                closestIndex          = activeLines.Count - 1;
                closestMetalRigidbody = rb;
                closestMetalTransform = root;
            }
        }

        if (closestIndex >= 0)
        {
            var data = activeLines[closestIndex];
            data.isClosest           = true;
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
            if (data.target == null || !data.target.gameObject.activeInHierarchy) continue;

            LineRenderer lr = linePool[i];
            lr.gameObject.SetActive(true);
            lr.SetPosition(0, chestPoint.position);
            lr.SetPosition(1, data.target.position);

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
                c.a *= (1f - Mathf.Clamp01(data.distance / maxRange) * 0.6f);
                lr.startWidth = width;
                lr.endWidth   = width * 0.3f;
                lr.startColor = c;
                lr.endColor   = c * 0.3f;
            }
        }
    }

    // ── Highlight ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Highlight only the closest metal's mesh with a dark blue tint.
    /// Uses the rigidbody ROOT transform for comparison, not the renderer transform,
    /// so it stays stable when the collider/renderer are on different child objects.
    /// </summary>
    void HighlightClosestMetal()
    {
        // If the closest target changed, clear the old highlight first.
        if (highlightedRoot != null && highlightedRoot != closestMetalTransform)
            ClearHighlight();

        if (closestMetalTransform == null)
        {
            ClearHighlight();
            return;
        }

        // Already highlighting the right object — nothing to do.
        if (highlightedRoot == closestMetalTransform) return;

        // Find a renderer: prefer the root, then search children.
        Renderer targetRenderer = closestMetalTransform.GetComponent<Renderer>();
        if (targetRenderer == null)
            targetRenderer = closestMetalTransform.GetComponentInChildren<Renderer>();
        if (targetRenderer == null) return;

        // Apply tint
        closestHighlightedRenderer = targetRenderer;
        highlightedRoot            = closestMetalTransform;

        Material mat = targetRenderer.material;
        if (mat == null) return;

        if (mat.HasProperty("_BaseColor"))
        {
            closestOriginalColor = mat.GetColor("_BaseColor");
            mat.SetColor("_BaseColor", closestHighlightColor);
        }
        else
        {
            closestOriginalColor = mat.color;
            mat.color = closestHighlightColor;
        }

        if (mat.HasProperty("_EmissiveColor"))
            mat.SetColor("_EmissiveColor", closestHighlightColor * 0.5f);
    }

    void ClearHighlight()
    {
        if (closestHighlightedRenderer != null)
        {
            Material mat = closestHighlightedRenderer.material;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", closestOriginalColor);
                else
                    mat.color = closestOriginalColor;

                if (mat.HasProperty("_EmissiveColor"))
                    mat.SetColor("_EmissiveColor", Color.black);
            }
        }
        closestHighlightedRenderer = null;
        highlightedRoot            = null;
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
        closestMetalTransform = null;
        closestMetalRigidbody = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Closest metal's transform — for legacy callers.</summary>
    public Transform GetClosestMetal() => closestMetalTransform;

    /// <summary>
    /// Closest metal's Rigidbody — used by SteelPush and IronPull when burning
    /// so they target the exact object that is highlighted.
    /// </summary>
    public Rigidbody GetClosestMetalRigidbody() => closestMetalRigidbody;

    public int  GetVisibleLineCount() => activeLines.Count;
    public bool IsActive()            => metalSightActive;
}
