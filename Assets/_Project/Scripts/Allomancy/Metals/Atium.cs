using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Implements the Atium Allomancy ability (Seer).
/// Shows "Atium Shadows" (future ghosts) of nearby entities.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 90)]
public class Atium : MonoBehaviour
{
    [Header("Settings")]
    public float baseVisionRange = 25f;
    public float ghostAlpha = 0.3f;
    public float maxVisionRange = 50f;
    public float targetTimeScale = AllomancyConstants.AtiumTimeScale;
    public float shadowLeadTime = AllomancyConstants.AtiumShadowLeadTime;
    
    [Header("Atium Dilation")]
    [Tooltip("How fast time transitions")]
    public float dilationLerpSpeed = 5f;

    [Header("References")]
    public Allomancer allomancer;

    private bool isBurning = false;
    private Dictionary<GameObject, GhostRenderer> activeGhosts = new Dictionary<GameObject, GhostRenderer>();
    private float originalTimeScale = 1f;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        originalTimeScale = Time.timeScale;
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Atium;

        if (isBurning)
        {
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            HandleTimeDilation(true);
            UpdateFutureVision(flareMult);
        }
        else
        {
            if (wasBurning) 
            {
                HandleTimeDilation(false);
                ClearFutures();
            }
        }
    }

    private void HandleTimeDilation(bool active)
    {
        float target = active ? targetTimeScale : 1f;
        if (MistbornTimeManager.Instance != null)
            MistbornTimeManager.Instance.SetAtiumModifier(target);
        else
            Time.timeScale = target; // Fallback
    }

    void UpdateFutureVision(float flareMult)
    {
        float currentRange = Mathf.Lerp(baseVisionRange, maxVisionRange, (flareMult - 1f) / 1.5f);
        
        // Optimized high-performance search via Registry
        var enemies = MistbornRegistry.ActiveEnemies;
        HashSet<GameObject> currentTargets = new HashSet<GameObject>();

        // Flare scales how far ahead we see — max flare reveals ~2× the future window
        float leadTime = shadowLeadTime * Mathf.Lerp(1f, 2f, (flareMult - 1f) / 9f);

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= currentRange)
            {
                currentTargets.Add(enemy.gameObject);
                UpdateShadow(enemy.gameObject, leadTime);
            }
        }

        // Cleanup out-of-range ghosts
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var pair in activeGhosts)
        {
            if (!currentTargets.Contains(pair.Key)) toRemove.Add(pair.Key);
        }
        foreach (var key in toRemove)
        {
            Destroy(activeGhosts[key]);
            activeGhosts.Remove(key);
        }
    }

    void UpdateShadow(GameObject target, float leadTime)
    {
        if (!activeGhosts.ContainsKey(target))
        {
            GhostRenderer gr = gameObject.AddComponent<GhostRenderer>();
            gr.SetupGhost(target, Color.white, ghostAlpha);
            activeGhosts.Add(target, gr);
        }

        GhostRenderer ghost = activeGhosts[target];

        Vector3 futurePos = target.transform.position;
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Physics-driven object: extrapolate from current velocity
            futurePos += rb.linearVelocity * leadTime;
        }
        else
        {
            // NavMesh-driven AI: use agent velocity for accurate prediction
            NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
            if (agent != null)
                futurePos += agent.velocity * leadTime;
            else
                futurePos += target.transform.forward * leadTime * 4f; // static fallback
        }

        ghost.UpdateTransform(futurePos, target.transform.rotation);
    }

    void ClearFutures()
    {
        foreach (var ghost in activeGhosts.Values)
        {
            if (ghost != null) Destroy(ghost);
        }
        activeGhosts.Clear();
    }


    void OnDestroy() => ClearFutures();

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            float currentRange = Mathf.Lerp(baseVisionRange, maxVisionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}