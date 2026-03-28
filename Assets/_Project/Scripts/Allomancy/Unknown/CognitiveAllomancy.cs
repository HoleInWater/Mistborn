using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified cognitive Allomancy: Copper (hide pulses) and Bronze (detect pulses).
/// Copper creates a "cloud" that blocks Bronze detection.
/// Bronze detects other Allomancers by their burning pulses.
/// </summary>
public class CognitiveAllomancy : MonoBehaviour
{
    [Header("Copper — Smoker Cloud")]
    public float copperCloudRadius = 15f;
    public bool isCloudActive = false;

    [Header("Bronze — Seeker Detection")]
    public float bronzeDetectionRange = 30f;
    public float bronzePulseInterval = 1f;

    [Header("References")]
    public Allomancer allomancer;

    // Static registry of active copper clouds
    private static List<CognitiveAllomancy> activeClouds = new List<CognitiveAllomancy>();
    public static IReadOnlyList<CognitiveAllomancy> ActiveClouds => activeClouds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticState() => activeClouds = new List<CognitiveAllomancy>();

    // Detected Allomancers
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
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
    }

    void Update()
    {
        if (allomancer == null) return;

        // Copper cloud
        bool wasClouding = isCloudActive;
        isCloudActive = allomancer.IsMetalBurning(AllomancySkill.MetalType.Copper);
        if (isCloudActive && !wasClouding) activeClouds.Add(this);
        else if (!isCloudActive && wasClouding) activeClouds.Remove(this);

        if (isCloudActive)
            allomancer.DrainMetal(AllomancySkill.MetalType.Copper, 0.042f * Time.deltaTime); // MAG: 40 min/charge

        // Bronze seeking
        if (allomancer.IsMetalBurning(AllomancySkill.MetalType.Bronze))
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer <= 0f)
            {
                pulseTimer = bronzePulseInterval;
                SeekPulses();
            }
            allomancer.DrainMetal(AllomancySkill.MetalType.Bronze, 0.056f * Time.deltaTime); // MAG: 30 min/charge
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

        // Find all Allomancers in range
        var allomancers = MistbornRegistry.ActiveAllomancers;
        foreach (var allo in allomancers)
        {
            if (allo == null || allo == allomancer) continue;
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

    string GetMetalCategory(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:
            case AllomancySkill.MetalType.Iron:
            case AllomancySkill.MetalType.Pewter:
            case AllomancySkill.MetalType.Tin:
                return "Physical";
            case AllomancySkill.MetalType.Zinc:
            case AllomancySkill.MetalType.Brass:
            case AllomancySkill.MetalType.Copper:
            case AllomancySkill.MetalType.Bronze:
                return "Mental";
            case AllomancySkill.MetalType.Atium:
            case AllomancySkill.MetalType.Gold:
            case AllomancySkill.MetalType.Electrum:
            case AllomancySkill.MetalType.Malatium:
                return "Temporal";
            default:
                return "Enhancement";
        }
    }
}
