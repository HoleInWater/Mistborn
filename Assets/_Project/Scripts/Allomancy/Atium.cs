using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Atium Allomancy ability (Seer).
/// Shows "Atium Shadows" (future ghosts) of nearby entities.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Atium : MonoBehaviour
{
    [Header("Settings")]
    public float baseVisionRange = 25f;
    public float ghostAlpha = 0.3f;
    public float targetTimeScale = AllomancyConstants.AtiumTimeScale;
    public float shadowLeadTime = AllomancyConstants.AtiumShadowLeadTime;
    
    [Header("Atium Dilation")]
    [Tooltip("How fast time transitions")]
    public float dilationLerpSpeed = 5f;

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
            float flareMult = GetFlareMultiplier();
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
        Time.timeScale = Mathf.Lerp(Time.timeScale, target, Time.unscaledDeltaTime * dilationLerpSpeed);
        // fixedDeltaTime must be updated to keep physics stable
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void UpdateFutureVision(float flareMult)
    {
        float currentRange = Mathf.Lerp(baseVisionRange, maxVisionRange, (flareMult - 1f) / 1.5f);
        
        // Use a non-allocating search in future for optimization
        AIController[] enemies = FindObjectsOfType<AIController>();
        HashSet<GameObject> currentTargets = new HashSet<GameObject>();

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= currentRange)
            {
                currentTargets.Add(enemy.gameObject);
                UpdateShadow(enemy.gameObject);
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

    void UpdateShadow(GameObject target)
    {
        if (!activeGhosts.ContainsKey(target))
        {
            GhostRenderer gr = gameObject.AddComponent<GhostRenderer>();
            gr.SetupGhost(target, Color.white, ghostAlpha);
            activeGhosts.Add(target, gr);
        }

        GhostRenderer ghost = activeGhosts[target];
        
        // Offset logically based on current velocity to represent "future"
        Rigidbody rb = target.GetComponent<Rigidbody>();
        Vector3 futurePos = target.transform.position;
        if (rb != null)
        {
            futurePos += rb.velocity * 0.4f; // Look 400ms into the future
        }
        else
        {
            // Fallback for non-rigidbody movement
            futurePos += target.transform.forward * 2f;
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
            float flareMult = GetFlareMultiplier();
            float currentRange = Mathf.Lerp(baseVisionRange, maxVisionRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}