using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Brass Allomancy ability (Soothe emotions).
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
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
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        // Check if we are currently burning Brass according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Brass;

        if (isBurning)
        {
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            SootheEmotions(flareMult);
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
            AIController ai = enemy.GetComponent<AIController>();
            if (ai != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                float distScale = 1f - (dist / currentRange);
                float appliedStrength = Mathf.Lerp(baseSootheStrength, currentStrength, distScale);

                // Soothing makes enemies calm and less aggressive
                ai.SetEmotionState(AIController.EmotionState.Calm);
                ai.SetAggressionMultiplier(appliedStrength);
                
                // Blue/Cyan aura for soothing
                ai.SetEmotionalAura(new Color(0f, 0.8f, 1f, 0.8f * distScale), flareMult * distScale);
            }
        }

    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
            float flareMult = GetFlareMultiplier();
            float currentRange = Mathf.Lerp(baseEmotionRange, maxEmotionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}