using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Metallurgic Sight — active automatically while burning Iron or Steel.
/// Draws a single line from the chest to the closest metal object, matching
/// the style of the SteelPush/IronPull prediction line (yellow-green at player,
/// blue at target). Closest metal also gets an Unlit highlight that scales
/// with proximity.
///
/// Everything is hidden when not burning, or when neither Iron nor Steel
/// occupies a slot in MetalSelector.
/// </summary>
[PlayerComponent("Metallurgy", order: 50)]
public class MetalLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float maxRange        = 30f;
    public float massScaleFactor = 0.01f;
    public float updateInterval  = 0.15f;

    [Header("Targeting Line")]
    [Tooltip("Color at the player (chest) end — matches the prediction line start.")]
    public Color lineStartColor = new Color(0.9f, 1f, 0.3f, 0.8f);   // yellow-green
    [Tooltip("Color at the metal (target) end — matches the prediction line end.")]
    public Color lineEndColor   = new Color(0.1f, 0.4f, 1f, 0.6f);   // blue
    public float lineStartWidth = 0.05f;
    public float lineEndWidth   = 0.01f;

    [Header("Highlight")]
    [Tooltip("Not used for rendering — kept for backwards compatibility with PlayerAutoSetup.")]
    public Color closestHighlightColor = new Color(0.1f, 0.15f, 0.5f, 1f);
    [Tooltip("Unlit highlight brightness at closest range.")]
    public float highlightMaxIntensity = 3f;
    [Tooltip("Unlit highlight brightness at max range.")]
    public float highlightMinIntensity = 0.4f;

    [Header("References")]
    public Transform     chestPoint;
    public Metallurgist    metallurgist;
    public LayerMask     metalLayer;
    [Tooltip("Resolved lazily at runtime — can also be assigned manually in the Inspector.")]
    public MetalSelector metalSelector;

    // ── Single targeting line ─────────────────────────────────────────────────
    private LineRenderer targetLine;

    // ── Scan state ────────────────────────────────────────────────────────────
    private float     updateTimer;
    private Transform closestMetalRoot;
    private Rigidbody closestMetalRigidbody;
    private float     closestMetalDistance;
    private Transform closestMetalColTransform; // actual collider position for line endpoint

    // ── Highlight state ───────────────────────────────────────────────────────
    private Renderer   highlightedRenderer;
    private Transform  highlightedRoot;
    private Material[] originalMaterials;
    private Material   highlightMaterial;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
        if (metalSelector == null) metalSelector = GetComponent<MetalSelector>();
        if (chestPoint == null) chestPoint = transform;

        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        // Highlight material — Unlit so it's always visible in HDRP
        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader == null) unlitShader = Shader.Find("Hidden/InternalErrorShader");
        highlightMaterial = new Material(unlitShader);

        // Single targeting line on its own child GO so it doesn't interfere
        // with anything else on this object
        GameObject lineGO = new GameObject("MetallurgicTargetLine");
        lineGO.transform.SetParent(transform);
        targetLine               = lineGO.AddComponent<LineRenderer>();
        targetLine.material      = new Material(Shader.Find("Sprites/Default"));
        targetLine.positionCount = 2;
        targetLine.useWorldSpace = true;
        targetLine.startWidth    = lineStartWidth;
        targetLine.endWidth      = lineEndWidth;
        targetLine.startColor    = lineStartColor;
        targetLine.endColor      = lineEndColor;
        lineGO.SetActive(false); // hidden until burning + iron/steel
    }

    void Update()
    {
        bool isBurning   = FlareManager.Instance != null && FlareManager.Instance.IsBurning;
        bool ironOrSteel = IronOrSteelSelected();
        bool active      = isBurning && ironOrSteel;

        if (!active)
        {
            targetLine.gameObject.SetActive(false);
            ClearHighlight();
            return;
        }

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanMetals();
        }

        DrawTargetLine();
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

        MetallurgySkill.MetalType p = ms.GetPrimaryMetal();
        MetallurgySkill.MetalType s = ms.GetSecondaryMetal();

        return p == MetallurgySkill.MetalType.Iron  || p == MetallurgySkill.MetalType.Steel
            || s == MetallurgySkill.MetalType.Iron  || s == MetallurgySkill.MetalType.Steel;
    }

    // ── Scan ──────────────────────────────────────────────────────────────────

    void ScanMetals()
    {
        closestMetalRoot         = null;
        closestMetalRigidbody    = null;
        closestMetalDistance     = float.MaxValue;
        closestMetalColTransform = null;

        float flare          = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = maxRange * flare;

        Collider[] hits = Physics.OverlapSphere(chestPoint.position, effectiveRange, metalLayer);

        foreach (Collider col in hits)
        {
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

            float dist = Vector3.Distance(chestPoint.position, col.transform.position);
            if (dist >= closestMetalDistance) continue;

            Rigidbody rb = col.attachedRigidbody;
            closestMetalDistance     = dist;
            closestMetalRigidbody    = rb;
            closestMetalRoot         = rb != null ? rb.transform : col.transform;
            closestMetalColTransform = col.transform;
        }
    }

    // ── Line ──────────────────────────────────────────────────────────────────

    void DrawTargetLine()
    {
        if (closestMetalColTransform == null)
        {
            targetLine.gameObject.SetActive(false);
            return;
        }

        targetLine.gameObject.SetActive(true);
        targetLine.SetPosition(0, chestPoint.position);
        targetLine.SetPosition(1, closestMetalColTransform.position);
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
        Color tipColor  = Color.Lerp(lineStartColor, lineEndColor, proximity);
        tipColor.a      = 1f;
        highlightMaterial.color = tipColor * Mathf.Lerp(highlightMinIntensity, highlightMaxIntensity, proximity);

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

    // ── OnDisable ─────────────────────────────────────────────────────────────

    bool HasIronOrSteelSelected()
    {
        if (metalSelector == null) return false;
        var primary   = metalSelector.GetPrimaryMetal();
        var secondary = metalSelector.GetSecondaryMetal();
        return primary  == MetallurgySkill.MetalType.Iron  || primary  == MetallurgySkill.MetalType.Steel
            || secondary == MetallurgySkill.MetalType.Iron || secondary == MetallurgySkill.MetalType.Steel;
    }

    void OnDisable()
    {
        if (targetLine != null) targetLine.gameObject.SetActive(false);
        ClearHighlight();
        closestMetalRoot         = null;
        closestMetalRigidbody    = null;
        closestMetalColTransform = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public Transform GetClosestMetal()          => closestMetalRoot;
    public Rigidbody GetClosestMetalRigidbody() => closestMetalRigidbody;
    public bool      IsActive()                 => FlareManager.Instance != null
                                                && FlareManager.Instance.IsBurning
                                                && IronOrSteelSelected();

    // Kept for backwards compatibility with PlayerHUD
    public int       GetVisibleLineCount()      => IsActive() && closestMetalRoot != null ? 1 : 0;
}
