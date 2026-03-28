using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Zinc Allomancy ability (Riot emotions).
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 50)]
public class Zinc : MonoBehaviour
{
    [Header("Settings")]
    public float baseEmotionRange = 15f;
    public float baseRiotStrength = 1.2f;
    public LayerMask enemyLayer;
    
    [Header("Flare Boosts")]
    public float maxEmotionRange = 40f;
    public float maxRiotStrength = 3.0f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private float riotTimer = 0f;
    private const float RIOT_INTERVAL = 0.3f; // Pulse 3x/sec instead of 60x/sec

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Zinc;

        if (isBurning)
        {
            riotTimer -= Time.deltaTime;
            if (riotTimer <= 0f)
            {
                riotTimer = RIOT_INTERVAL;
                float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
                RiotEmotions(flareMult);
            }
        }
    }
    
    // Removed local GetFlareMultiplier

    void RiotEmotions(float flareMult)
    {
        // Scale values based on flare intensity
        float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
        float currentStrength = Mathf.Lerp(baseRiotStrength, maxRiotStrength, (flareMult - 1f) / 1.5f);

        Collider[] enemies = Physics.OverlapSphere(transform.position, currentRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            // Lore: aluminum-lined helmets (Hazekillers) block emotional Allomancy
            HazekillerAI hazekiller = enemy.GetComponentInParent<HazekillerAI>();
            if (hazekiller != null && hazekiller.IsImmuneToEmotionalAllomancy()) continue;

            AIController ai = enemy.GetComponentInParent<AIController>();
            if (ai == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            float distScale = 1f - (dist / currentRange);
            float appliedStrength = Mathf.Lerp(baseRiotStrength, currentStrength, distScale);

            ai.SetEmotionState(AIController.EmotionState.Enraged);
            ai.SetAggressionMultiplier(appliedStrength);
            ai.SetEmotionalAura(new Color(1f, 0.2f, 0f, 0.8f * distScale), flareMult * distScale);
        }

    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}