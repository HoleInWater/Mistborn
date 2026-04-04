using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified emotional Metallurgy system managing both Zinc (Riot) and Brass (Soothe).
/// Tracks emotional state of affected targets with stacking and decay.
/// Metalhunters with aluminum helmets are immune.
/// </summary>
public class EmotionalMetallurgy : MonoBehaviour
{
    [Header("Zinc — Riot (Inflame Emotions)")]
    public float riotRadius = 12f;
    public float riotIntensity = 1f;
    public float riotAggressionBoost = 2f;

    [Header("Brass — Soothe (Calm Emotions)")]
    public float sootheRadius = 12f;
    public float sootheIntensity = 1f;
    public float sootheAggressionReduction = 0.3f;

    [Header("References")]
    public Metallurgist metallurgist;

    // Track emotional influence per target
    private Dictionary<AIController, float> emotionInfluence = new Dictionary<AIController, float>();
    private float pulseTimer;
    private const float PULSE_INTERVAL = 0.5f;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
    }

    void Update()
    {
        if (metallurgist == null) return;

        pulseTimer -= Time.deltaTime;
        if (pulseTimer > 0f) return;
        pulseTimer = PULSE_INTERVAL;

        bool isRioting = metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Zinc);
        bool isSoothing = metallurgist.IsMetalBurning(MetallurgySkill.MetalType.Brass);

        if (isRioting) PulseRiot();
        if (isSoothing) PulseSoothe();

        DecayInfluence();
    }

    void PulseRiot()
    {
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRadius = riotRadius * flare;

        Collider[] hits = Physics.OverlapSphere(transform.position, effectiveRadius);
        foreach (var col in hits)
        {
            // Check Metalhunter immunity
            MetalhunterAI hk = col.GetComponent<MetalhunterAI>();
            if (hk != null && hk.IsImmuneToEmotionalMetallurgy()) continue;

            AIController ai = col.GetComponent<AIController>();
            if (ai == null) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float falloff = effectiveRadius > 0f ? 1f - (dist / effectiveRadius) : 0f;
            float intensity = riotIntensity * flare * falloff;

            ai.currentEmotion = AIController.EmotionState.Aggressive;
            emotionInfluence[ai] = Mathf.Min(emotionInfluence.GetValueOrDefault(ai, 0f) + intensity, 3f);
        }

        metallurgist.DrainMetal(MetallurgySkill.MetalType.Zinc, 0.333f * flare * Time.deltaTime); // MAG: 20 in-game min = 5 real min
    }

    void PulseSoothe()
    {
        float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
        float effectiveRadius = sootheRadius * flare;

        Collider[] hits = Physics.OverlapSphere(transform.position, effectiveRadius);
        foreach (var col in hits)
        {
            MetalhunterAI hk = col.GetComponent<MetalhunterAI>();
            if (hk != null && hk.IsImmuneToEmotionalMetallurgy()) continue;

            AIController ai = col.GetComponent<AIController>();
            if (ai == null) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float falloff = effectiveRadius > 0f ? 1f - (dist / effectiveRadius) : 0f;
            float intensity = sootheIntensity * flare * falloff;

            ai.currentEmotion = AIController.EmotionState.Calm;
            emotionInfluence[ai] = Mathf.Max(emotionInfluence.GetValueOrDefault(ai, 0f) - intensity, -3f);
        }

        metallurgist.DrainMetal(MetallurgySkill.MetalType.Brass, 0.333f * flare * Time.deltaTime); // MAG: 20 in-game min = 5 real min
    }

    void DecayInfluence()
    {
        List<AIController> toRemove = new List<AIController>();
        List<AIController> keys = new List<AIController>(emotionInfluence.Keys);

        foreach (var ai in keys)
        {
            if (ai == null) { toRemove.Add(ai); continue; }

            float val = emotionInfluence[ai];
            val *= 0.95f; // Decay
            if (Mathf.Abs(val) < 0.1f)
            {
                ai.currentEmotion = AIController.EmotionState.Neutral;
                toRemove.Add(ai);
            }
            else
            {
                emotionInfluence[ai] = val;
            }
        }

        foreach (var ai in toRemove) emotionInfluence.Remove(ai);
    }
}
