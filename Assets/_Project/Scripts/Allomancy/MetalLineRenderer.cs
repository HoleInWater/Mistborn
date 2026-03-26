using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Allomantic Sight — Left Ctrl toggle.
/// Blue lines from chest to all nearby metals. Closest metal gets a dark blue
/// mesh highlight (material tint). All others show lines only, no mesh change.
/// Lore: Mistborn see translucent blue lines to every nearby metal source.
/// </summary>
public class MetalLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float maxRange = 30f;
    public float lineBaseWidth = 0.01f;
    public float lineMaxWidth = 0.06f;
    public float massScaleFactor = 0.01f;
    public float pulseSpeed = 3f;
    public float updateInterval = 0.15f;

    [Header("Line Colors")]
    public Color baseLineColor = new Color(0.2f, 0.4f, 1f, 0.35f);
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

    // Line pool
    private List<LineRenderer> linePool = new List<LineRenderer>();
    private List<MetalLineData> activeLines = new List<MetalLineData>();
    private float updateTimer;
    private Material lineMaterial;
    private bool metalSightActive = false;

    // Closest metal highlight tracking
    private Renderer closestHighlightedRenderer;
    private Color closestOriginalColor;
    private Transform closestMetalTransform;

    struct MetalLineData
    {
        public Transform target;
        public float mass;
        public float distance;
        public bool isClosest;
    }

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
            lr.material = lineMaterial;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.gameObject.SetActive(false);
            linePool.Add(lr);
        }
    }

    void Update()
    {
        // T key toggles allomantic sight (metal lines + closest highlight)
        if (Input.GetKeyDown(KeyCode.T))
            metalSightActive = !metalSightActive;

        // Only show when manually toggled with Z — no auto-activation
        bool showLines = metalSightActive;

        if (!showLines)
        {
            HideAllLines();
            ClearHighlight();
            return;
        }

        // Throttled scan
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanMetals();
        }

        DrawLines();
        HighlightClosestMetal();
    }

    void ScanMetals()
    {
        activeLines.Clear();
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = maxRange * flare;

        Collider[] hits = Physics.OverlapSphere(chestPoint.position, effectiveRange, metalLayer);

        float closestDist = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            if (col.transform == transform) continue;

            Rigidbody rb = col.attachedRigidbody;
            float mass = rb != null ? rb.mass : 1f;
            float dist = Vector3.Distance(chestPoint.position, col.transform.position);

            activeLines.Add(new MetalLineData
            {
                target = col.transform,
                mass = mass,
                distance = dist,
                isClosest = false
            });

            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = activeLines.Count - 1;
            }
        }

        // Mark closest
        if (closestIndex >= 0)
        {
            var data = activeLines[closestIndex];
            data.isClosest = true;
            activeLines[closestIndex] = data;
            closestMetalTransform = data.target;
        }
        else
        {
            closestMetalTransform = null;
        }
    }

    void DrawLines()
    {
        // Hide all first
        for (int i = 0; i < linePool.Count; i++)
            linePool[i].gameObject.SetActive(false);

        int count = Mathf.Min(activeLines.Count, linePool.Count);

        for (int i = 0; i < count; i++)
        {
            MetalLineData data = activeLines[i];
            if (data.target == null) continue;

            LineRenderer lr = linePool[i];
            lr.gameObject.SetActive(true);

            lr.SetPosition(0, chestPoint.position);
            lr.SetPosition(1, data.target.position);

            // Width — heavier = thicker
            float width = Mathf.Clamp(lineBaseWidth + data.mass * massScaleFactor, lineBaseWidth, lineMaxWidth);
            lr.startWidth = width;
            lr.endWidth = width * 0.3f;

            if (data.isClosest)
            {
                // Closest metal: bright dark blue line, thicker
                lr.startWidth = width * 2f;
                lr.endWidth = width * 0.8f;
                lr.startColor = closestLineColor;
                lr.endColor = closestLineColor * 0.5f;
            }
            else
            {
                // Other metals: normal blue line, no mesh highlight
                float proximity = 1f - Mathf.Clamp01(data.distance / maxRange);
                Color c = Color.Lerp(baseLineColor, closeLineColor, proximity);
                c.a *= (1f - Mathf.Clamp01(data.distance / maxRange) * 0.6f);
                lr.startColor = c;
                lr.endColor = c * 0.3f;
            }
        }
    }

    /// <summary>
    /// Highlight ONLY the closest metal's mesh with a dark blue tint.
    /// All other metals are lines only — no mesh color change.
    /// </summary>
    void HighlightClosestMetal()
    {
        // Clear previous if target changed
        if (closestHighlightedRenderer != null)
        {
            if (closestMetalTransform == null || closestHighlightedRenderer.transform != closestMetalTransform)
                ClearHighlight();
        }

        if (closestMetalTransform == null) return;

        // Find renderer — check self, parent, children
        Renderer targetRenderer = closestMetalTransform.GetComponent<Renderer>();
        if (targetRenderer == null) targetRenderer = closestMetalTransform.GetComponentInChildren<Renderer>();
        if (targetRenderer == null) targetRenderer = closestMetalTransform.GetComponentInParent<Renderer>();
        if (targetRenderer == null) return;

        if (targetRenderer != closestHighlightedRenderer)
        {
            ClearHighlight();
            closestHighlightedRenderer = targetRenderer;

            Material mat = targetRenderer.material;
            if (mat != null)
            {
                // Save original color — try HDRP _BaseColor first, then legacy _Color
                if (mat.HasProperty("_BaseColor"))
                    closestOriginalColor = mat.GetColor("_BaseColor");
                else
                    closestOriginalColor = mat.color;

                // Apply dark blue highlight
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", closestHighlightColor);
                else
                    mat.color = closestHighlightColor;

                // Also set emission for HDRP glow effect
                if (mat.HasProperty("_EmissiveColor"))
                    mat.SetColor("_EmissiveColor", closestHighlightColor * 0.5f);
            }
        }
    }

    void ClearHighlight()
    {
        if (closestHighlightedRenderer != null && closestHighlightedRenderer.material != null)
        {
            Material mat = closestHighlightedRenderer.material;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", closestOriginalColor);
            else
                mat.color = closestOriginalColor;

            if (mat.HasProperty("_EmissiveColor"))
                mat.SetColor("_EmissiveColor", Color.black);
        }
        closestHighlightedRenderer = null;
    }

    void HideAllLines()
    {
        foreach (var lr in linePool)
            lr.gameObject.SetActive(false);
        activeLines.Clear();
        closestMetalTransform = null;
    }

    void OnDisable()
    {
        ClearHighlight();
        HideAllLines();
    }

    /// <summary>
    /// Get the closest metal's transform (for SteelPush/IronPull targeting).
    /// </summary>
    public Transform GetClosestMetal() => closestMetalTransform;
    public int GetVisibleLineCount() => activeLines.Count;
    public bool IsActive() => metalSightActive;
}
