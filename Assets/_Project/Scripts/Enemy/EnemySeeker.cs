using UnityEngine;

/// <summary>
/// Seeker AI — burns Bronze to detect nearby Allomancers.
/// Lore: Bronze lets a Seeker sense the "pulses" of active Allomancy.
/// Copper clouds (Smokers) block detection.
///
/// On detection: alerts AIController (emotion → Aggressive) AND forces
/// EnemyAI into Chase so the full state machine responds immediately.
/// </summary>
public class EnemySeeker : MonoBehaviour
{
    [Header("Bronze Detection")]
    public float detectionRange = 30f;
    [Tooltip("Seconds the player must be actively burning before detection triggers.")]
    public float detectionThreshold = 0.5f;
    public LayerMask targetLayer;

    private AIController aiController;
    private EnemyAI enemyAI;
    private float detectionTimer = 0f;

    void Awake()
    {
        aiController = GetComponent<AIController>();
        enemyAI      = GetComponent<EnemyAI>();
    }

    void Update()
    {
        SearchForAllomancy();
    }

    void SearchForAllomancy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);
        bool anyFound = false;

        foreach (var col in hits)
        {
            Allomancer allomancer = col.GetComponentInParent<Allomancer>();
            if (allomancer == null || !allomancer.IsBurning()) continue;

            // Lore: Copper clouds hide Allomantic pulses from Seekers
            if (Copper.IsPulseHidden(allomancer.transform.position)) continue;

            anyFound = true;
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= detectionThreshold)
                AlertAI(allomancer.transform);

            break;
        }

        if (!anyFound)
            detectionTimer = Mathf.Max(0f, detectionTimer - Time.deltaTime);
    }

    void AlertAI(Transform detectedTarget)
    {
        // Emotion state — affects speed/detection range via AIController
        aiController?.SetEmotionState(AIController.EmotionState.Aggressive);

        // Force the full state machine to Chase immediately
        if (enemyAI != null)
            enemyAI.ForceChase(detectedTarget, detectedTarget.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
