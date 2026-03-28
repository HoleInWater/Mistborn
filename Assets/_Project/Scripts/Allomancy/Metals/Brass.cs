using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Brass Allomancy ability (Soothe emotions).
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 60)]
public class Brass : MonoBehaviour
{
    [Header("Settings")]
    public float baseEmotionRange = 15f;
    public float baseSootheStrength = 0.8f; // Lower = more soothing
    public LayerMask enemyLayer;
    
    [Header("Flare Boosts")]
    public float maxEmotionRange = 40f;
    public float maxSootheStrength = 0.2f; // Very low = extremely calm

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private float sootheTimer = 0f;
    private const float SOOTHE_INTERVAL = 0.3f;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Brass;

        if (isBurning)
        {
            sootheTimer -= Time.deltaTime;
            if (sootheTimer <= 0f)
            {
                sootheTimer = SOOTHE_INTERVAL;
                float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
                SootheEmotions(flareMult);
            }
        }
    }
    
    // Removed local GetFlareMultiplier

    void SootheEmotions(float flareMult)
    {
        // Scale values based on flare intensity
        float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
        float currentStrength = Mathf.Lerp(baseSootheStrength, maxSootheStrength, (flareMult - 1f) / 1.5f);

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
            float appliedStrength = Mathf.Lerp(baseSootheStrength, currentStrength, distScale);

            ai.SetEmotionState(AIController.EmotionState.Calm);
            ai.SetAggressionMultiplier(appliedStrength);
            ai.SetEmotionalAura(new Color(0f, 0.8f, 1f, 0.8f * distScale), flareMult * distScale);
        }

    }

    /// <summary>
    /// Called by Duralumin burst — mass soothe on everything in full range instantly.
    /// </summary>
    public void TriggerDuraluminBurst()
    {
        SootheEmotions(3f); // force max flare multiplier
        CameraShakeManager.Instance?.Shake(0.3f, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}