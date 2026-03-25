using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Renders the iconic blue lines from the Allomancer to nearby metals.
/// Lore: When burning Steel or Iron, a Mistborn sees translucent blue lines
/// extending from their chest to every nearby metal source. Thicker lines = heavier metal.
/// Line brightness pulses when the metal is being Pushed/Pulled.
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

    [Header("Colors")]
    public Color baseColor = new Color(0.2f, 0.4f, 1f, 0.4f);
    public Color activeColor = new Color(0.4f, 0.7f, 1f, 0.8f);
    public Color targetedColor = new Color(0.8f, 0.9f, 1f, 1f);

    [Header("References")]
    public Transform chestPoint;
    public Allomancer allomancer;
    public LayerMask metalLayer;

    // Line pool
    private List<LineRenderer> linePool = new List<LineRenderer>();
    private List<MetalLineData> activeLines = new List<MetalLineData>();
    private int poolIndex = 0;
    private float updateTimer;
    private Material lineMaterial;

    struct MetalLineData
    {
        public Transform target;
        public float mass;
        public float distance;
        public bool isTargeted;
    }

    void Start()
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (chestPoint == null) chestPoint = transform;
        metalLayer = LayerMask.GetMask("Metal");
        if (metalLayer == 0) metalLayer = ~0;

        lineMaterial = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));

        // Pre-create line pool
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

    private bool metalSightActive = false;

    void Update()
    {
        // Left Ctrl toggles metal sight on/off
        if (Input.GetKeyDown(KeyCode.LeftControl))
            metalSightActive = !metalSightActive;

        // Also show when actively burning Steel or Iron
        bool showLines = metalSightActive ||
            (allomancer != null &&
             (allomancer.IsMetalBurning(AllomancySkill.MetalType.Steel) ||
              allomancer.IsMetalBurning(AllomancySkill.MetalType.Iron)));

        if (!showLines)
        {
            HideAllLines();
            return;
        }

        // Periodic scan
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            ScanMetals();
        }

        DrawLines();
    }

    void ScanMetals()
    {
        activeLines.Clear();
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRange = maxRange * flare;

        Collider[] hits = Physics.OverlapSphere(chestPoint.position, effectiveRange, metalLayer);

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            Rigidbody rb = col.attachedRigidbody;
            float mass = rb != null ? rb.mass : 1f;
            float dist = Vector3.Distance(chestPoint.position, col.transform.position);

            activeLines.Add(new MetalLineData
            {
                target = col.transform,
                mass = mass,
                distance = dist,
                isTargeted = false
            });
        }
    }

    void DrawLines()
    {
        // Hide all first
        for (int i = 0; i < linePool.Count; i++)
            linePool[i].gameObject.SetActive(false);

        int count = Mathf.Min(activeLines.Count, linePool.Count);
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            MetalLineData data = activeLines[i];
            if (data.target == null) continue;

            LineRenderer lr = linePool[i];
            lr.gameObject.SetActive(true);

            // Position
            lr.SetPosition(0, chestPoint.position);
            lr.SetPosition(1, data.target.position);

            // Width based on mass (heavier = thicker line)
            float width = Mathf.Clamp(lineBaseWidth + data.mass * massScaleFactor, lineBaseWidth, lineMaxWidth);
            lr.startWidth = width;
            lr.endWidth = width * 0.3f;

            // Color — brighter for closer/heavier, pulse when active
            Color color = baseColor;
            float proximity = 1f - Mathf.Clamp01(data.distance / maxRange);

            if (data.isTargeted)
                color = Color.Lerp(activeColor, targetedColor, pulse);
            else
                color = Color.Lerp(baseColor, activeColor, proximity);

            // Distance fade
            color.a *= (1f - Mathf.Clamp01(data.distance / maxRange) * 0.7f);

            lr.startColor = color;
            lr.endColor = color * 0.3f;
        }
    }

    void HideAllLines()
    {
        foreach (var lr in linePool)
            lr.gameObject.SetActive(false);
        activeLines.Clear();
    }

    /// <summary>
    /// Mark a specific metal as the active target (drawn brighter).
    /// Called by SteelPush/IronPull when they lock onto a target.
    /// </summary>
    public void SetTargetedMetal(Transform metalTransform)
    {
        for (int i = 0; i < activeLines.Count; i++)
        {
            var data = activeLines[i];
            data.isTargeted = data.target == metalTransform;
            activeLines[i] = data;
        }
    }

    public int GetVisibleLineCount() => activeLines.Count;
}
