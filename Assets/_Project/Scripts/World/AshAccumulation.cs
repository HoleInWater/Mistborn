/* AshAccumulation.cs
 *
 * Simulates ash accumulating on surfaces over time.
 *
 * Lore: ash falls constantly from the Ashmounts. Skaa workers sweep streets
 * and rooftops daily. Without sweeping, ash would bury Luthadel in weeks.
 * From PHYSICS-MATH-BOOK.md Section 16:
 *   Accumulation rate: ~0.1-1.0 mm/day on flat surfaces
 *
 * This system:
 *   - Tracks ash depth on tagged surfaces
 *   - Visually darkens surfaces as ash accumulates (color shift)
 *   - Skaa NPCs can "sweep" to reset accumulation
 *   - Affects gameplay: thick ash slows movement, covers metal (harder to detect)
 *   - Integrates with DayNightCycle for time-based accumulation
 */

using UnityEngine;
using System.Collections.Generic;

public class AshAccumulation : MonoBehaviour
{
    public static AshAccumulation Instance { get; private set; }

    [Header("Accumulation Settings")]
    [Tooltip("Millimeters of ash per in-game day")]
    public float accumulationRate = 0.5f; // mm/day, within the 0.1-1.0 range from physics book
    [Tooltip("Maximum ash depth before it stops accumulating (mm)")]
    public float maxDepth = 50f;
    [Tooltip("Ash depth that starts affecting movement speed")]
    public float movementPenaltyDepth = 10f;
    [Tooltip("Maximum movement speed reduction from deep ash")]
    public float maxMovementPenalty = 0.3f;
    [Tooltip("Ash depth that starts hiding metal objects from Allomantic sight")]
    public float metalHidingDepth = 20f;

    [Header("Visual")]
    [Tooltip("Color shift as ash accumulates — blends toward this color")]
    public Color ashColor = new Color(0.35f, 0.32f, 0.28f);
    [Tooltip("How quickly the visual change appears")]
    public float visualBlendSpeed = 0.1f;

    [Header("Sweeping")]
    [Tooltip("How much depth is cleared per sweep action")]
    public float sweepAmount = 5f;
    [Tooltip("Radius of a single sweep")]
    public float sweepRadius = 3f;

    // Tracked surfaces
    private Dictionary<Renderer, AshSurfaceData> trackedSurfaces = new Dictionary<Renderer, AshSurfaceData>();
    private float dayAccumulator;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Find all surfaces tagged "AshSurface" or on the ground layer
        RegisterGroundSurfaces();
    }

    void Update()
    {
        if (DayNightCycle.Instance == null) return;

        // Accumulate based on day/night cycle time
        float hoursPerSecond = 24f / (DayNightCycle.Instance.dayLengthMinutes * 60f);
        float daysPerSecond = hoursPerSecond / 24f;
        dayAccumulator += daysPerSecond * Time.deltaTime;

        if (dayAccumulator >= 0.01f) // update every 1% of a day
        {
            float daysElapsed = dayAccumulator;
            dayAccumulator = 0f;

            foreach (var kvp in trackedSurfaces)
            {
                if (kvp.Key == null) continue;

                var data = kvp.Value;
                float oldDepth = data.currentDepth;

                // Accumulate
                data.currentDepth = Mathf.Min(data.currentDepth + accumulationRate * daysElapsed, maxDepth);

                // Update visual
                UpdateSurfaceVisual(kvp.Key, data);
            }
        }
    }

    void RegisterGroundSurfaces()
    {
        // Find all renderers on objects tagged "AshSurface" or on the ground
        GameObject[] ashObjects = GameObject.FindGameObjectsWithTag("Untagged"); // fallback
        foreach (var go in FindObjectsOfType<Renderer>())
        {
            // Only track horizontal surfaces (ground, roofs, ledges)
            if (go.transform.up.y > 0.7f) // mostly facing up
            {
                if (!trackedSurfaces.ContainsKey(go))
                {
                    trackedSurfaces[go] = new AshSurfaceData
                    {
                        originalColor = go.material.color,
                        currentDepth = Random.Range(0f, 2f) // slight starting variation
                    };
                }
            }
        }
    }

    void UpdateSurfaceVisual(Renderer rend, AshSurfaceData data)
    {
        if (rend == null || rend.material == null) return;

        float blend = Mathf.Clamp01(data.currentDepth / maxDepth) * visualBlendSpeed;
        Color target = Color.Lerp(data.originalColor, ashColor, blend);

        if (rend.material.HasProperty("_BaseColor"))
            rend.material.SetColor("_BaseColor", target);
        else
            rend.material.color = target;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Get ash depth at a world position (finds nearest tracked surface).</summary>
    public float GetAshDepthAt(Vector3 position)
    {
        float closestDist = float.MaxValue;
        float depth = 0f;

        foreach (var kvp in trackedSurfaces)
        {
            if (kvp.Key == null) continue;
            float dist = Vector3.Distance(position, kvp.Key.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                depth = kvp.Value.currentDepth;
            }
        }
        return depth;
    }

    /// <summary>Get movement speed multiplier based on ash depth at position.</summary>
    public float GetMovementMultiplier(Vector3 position)
    {
        float depth = GetAshDepthAt(position);
        if (depth < movementPenaltyDepth) return 1f;
        float t = Mathf.Clamp01((depth - movementPenaltyDepth) / (maxDepth - movementPenaltyDepth));
        return 1f - t * maxMovementPenalty;
    }

    /// <summary>Is metal at this position hidden by ash?</summary>
    public bool IsMetalHiddenByAsh(Vector3 position)
    {
        return GetAshDepthAt(position) >= metalHidingDepth;
    }

    /// <summary>Sweep ash in a radius around a position.</summary>
    public void SweepAsh(Vector3 center)
    {
        foreach (var kvp in trackedSurfaces)
        {
            if (kvp.Key == null) continue;
            float dist = Vector3.Distance(center, kvp.Key.transform.position);
            if (dist <= sweepRadius)
            {
                kvp.Value.currentDepth = Mathf.Max(0f, kvp.Value.currentDepth - sweepAmount);
                UpdateSurfaceVisual(kvp.Key, kvp.Value);
            }
        }
    }

    class AshSurfaceData
    {
        public Color originalColor;
        public float currentDepth; // mm
    }
}
