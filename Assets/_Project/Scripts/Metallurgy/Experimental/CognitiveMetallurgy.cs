using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified cognitive Metallurgy: Copper (hide pulses) and Bronze (detect pulses).
/// Copper creates a "cloud" that blocks Bronze detection.
/// Bronze detects other Metallurgists by their burning pulses.
/// </summary>
public class CognitiveMetallurgy : MonoBehaviour
{
    [Header("Copper — Smoker Cloud")]
    public float copperCloudRadius = 15f;
    public bool isCloudActive = false;

    [Header("Bronze — Seeker Detection")]
    public float bronzeDetectionRange = 30f;
    public float bronzePulseInterval = 1f;

    [Header("References")]
    public Metallurgist metallurgist;

    // Static registry of active copper clouds
    private static List<CognitiveMetallurgy> activeClouds = new List<CognitiveMetallurgy>();
    public static IReadOnlyList<CognitiveMetallurgy> ActiveClouds => activeClouds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticState() => activeClouds = new List<CognitiveMetallurgy>();

    // Detected Metallurgists
    private List<DetectedPulse> detectedPulses = new List<DetectedPulse>();
    public IReadOnlyList<DetectedPulse> DetectedPulses => detectedPulses;

    private float pulseTimer;

    public struct DetectedPulse
    {
        public Transform source;
        public float distance;
        public float intensity;
        public string metalCategory; // Physical, Mental, Temporal, Enhancement
    }

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
    }

    void Update()
    {
        if (metallurgist == null) return;

        // Copper cloud
        bool wasClouding = isCloudActive;
        isCloudActive = metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Copper);
        if (isCloudActive && !wasClouding) activeClouds.Add(this);
        else if (!isCloudActive && wasClouding) activeClouds.Remove(this);

        if (isCloudActive)
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Copper, 0.167f * Time.deltaTime); // MAG: 40 in-game min = 10 real min

        // Bronze seeking
        if (metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Bronze))
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer <= 0f)
            {
                pulseTimer = bronzePulseInterval;
                SeekPulses();
            }
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Bronze, 0.222f * Time.deltaTime); // MAG: 30 in-game min = 7.5 real min
        }
        else
        {
            detectedPulses.Clear();
        }
    }

    void OnDestroy()
    {
        activeClouds.Remove(this);
    }

    void SeekPulses()
    {
        detectedPulses.Clear();
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float range = bronzeDetectionRange * flare;
        if (range <= 0f) return;

        // Find all Metallurgists in range
        var metallurgists = AshwalkerRegistry.ActiveMetallurgists;
        foreach (var allo in metallurgists)
        {
            if (allo == null || allo == metallurgist) continue;
            if (!allo.IsBurning()) continue;

            float dist = Vector3.Distance(transform.position, allo.transform.position);
            if (dist > range) continue;

            // Check if target is hidden by a copper cloud
            if (IsHiddenByCloud(allo.transform.position)) continue;

            detectedPulses.Add(new DetectedPulse
            {
                source = allo.transform,
                distance = dist,
                intensity = 1f - (dist / range),
                metalCategory = GetMetalCategory(allo.GetCurrentMetal())
            });
        }
    }

    /// <summary>
    /// Check if a position is inside any active copper cloud.
    /// </summary>
    public static bool IsHiddenByCloud(Vector3 position)
    {
        foreach (var cloud in activeClouds)
        {
            if (cloud == null) continue;
            float dist = Vector3.Distance(position, cloud.transform.position);
            if (dist <= cloud.copperCloudRadius)
                return true;
        }
        return false;
    }

    string GetMetalCategory(MetallurgySkill.MetalType metal)
    {
        switch (metal)
        {
            case MetallurgySkill.MetalType.Steel:
            case MetallurgySkill.MetalType.Iron:
            case MetallurgySkill.MetalType.Pewter:
            case MetallurgySkill.MetalType.Tin:
                return "Physical";
            case MetallurgySkill.MetalType.Zinc:
            case MetallurgySkill.MetalType.Brass:
            case MetallurgySkill.MetalType.Copper:
            case MetallurgySkill.MetalType.Bronze:
                return "Mental";
            case MetallurgySkill.MetalType.Oraculum:
            case MetallurgySkill.MetalType.Gold:
            case MetallurgySkill.MetalType.Electrum:
            case MetallurgySkill.MetalType.Revelum:
                return "Temporal";
            default:
                return "Enhancement";
        }
    }
}
