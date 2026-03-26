// ============================================================
// FILE: EnemySeeker.cs
// AGENT: gemini-3-flash via Antigravity
// DATE: 2026-03-23
// ------------------------------------------------------------
// PROBLEM BEING SOLVED:
//   Missing Seeker AI. This enemy needs to detect players burning metals.
//
// APPROACH CHOSEN:
//   Restored version that integrates with the Copper cloud system. 
//   Checks nearby Allomancers and alerts the AIController if found.
//
// FILES TOUCHED:
//   [NEW] EnemySeeker.cs
//
// THENBUZZARD100 FILES NEARBY (READ-ONLY):
//   AIController.cs - Used for alerting.
// ============================================================

using UnityEngine;

/// <summary>
/// AI component that "Seeks" Allomancy by burning Bronze.
/// </summary>
public class EnemySeeker : MonoBehaviour
{
    [Header("Seeker Settings")]
    public float detectionRange = 30f;
    public float detectionThreshold = 0.5f; // How long player must burn to be detected
    public LayerMask targetLayer;

    private AIController aiController;
    private float detectionTimer = 0f;

    void Awake()
    {
        aiController = GetComponent<AIController>();
    }

    void Update()
    {
        SearchForAllomancy();
    }

    private void SearchForAllomancy()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);
        bool anyFound = false;

        foreach (var col in targets)
        {
            Allomancer target = col.GetComponentInParent<Allomancer>();
            if (target != null && target.IsBurning())
            {
                // Lore: Copper clouds hide pulses from Seekers.
                if (Copper.IsPulseHidden(target.transform.position))
                {
                    continue; 
                }

                anyFound = true;
                detectionTimer += Time.deltaTime;
                
                if (detectionTimer >= detectionThreshold)
                {
                    AlertAI(target.transform);
                }
                break;
            }
        }

        if (!anyFound)
        {
            detectionTimer = Mathf.Max(0, detectionTimer - Time.deltaTime);
        }
    }

    private void AlertAI(Transform target)
    {
        if (aiController != null)
        {
            aiController.SetEmotionState(AIController.EmotionState.Aggressive);
            // Future AIController logic should handle pathfinding to target
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
