using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Zinc Allomancy ability (Riot emotions).
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
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
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        // Check if we are currently burning Zinc according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Zinc;

        if (isBurning)
        {
            float flareMult = GetFlareMultiplier();
            RiotEmotions(flareMult);
        }
    }
    
    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.FlareIntensity;
        }
        return 1.0f;
    }

    void RiotEmotions(float flareMult)
    {
        // Scale values based on flare intensity
        float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
        float currentStrength = Mathf.Lerp(baseRiotStrength, maxRiotStrength, (flareMult - 1f) / 1.5f);

        Collider[] enemies = Physics.OverlapSphere(transform.position, currentRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            AIController ai = enemy.GetComponent<AIController>();
            if (ai != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                float distScale = 1f - (dist / currentRange);
                float appliedStrength = Mathf.Lerp(baseRiotStrength, currentStrength, distScale);

                // Rioting makes enemies enraged and more aggressive
                ai.SetEmotionState(AIController.EmotionState.Enraged);
                ai.SetAggressionMultiplier(appliedStrength);

                // Red/Orange aura for rioting
                ai.SetEmotionalAura(new Color(1f, 0.2f, 0f, 0.8f * distScale), flareMult * distScale);
            }
        }

    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            float flareMult = GetFlareMultiplier();
            float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}