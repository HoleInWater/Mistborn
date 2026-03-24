using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implements the Malatium Allomancy ability.
/// Allows seeing a person’s past or what they could have been.
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Malatium : MonoBehaviour
{
    [Header("Settings")]
    public float baseRevealRange = 15f;
    public Color malatiumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    [Header("Flare Boosts")]
    public float maxRevealRange = 35f;

    [Header("References")]
    public Allomancer allomancer;
    
    private Dictionary<GameObject, GhostRenderer> activeGhosts = new Dictionary<GameObject, GhostRenderer>();
    private bool isBurning = false;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Malatium;

        if (isBurning)
        {
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            RevealTrueNature(flareMult);
        }
        else if (wasBurning)
        {
            ResetReveals();
        }
    }

    void RevealTrueNature(float flareMult)
    {
        float currentRange = Mathf.Lerp(AllomancyConstants.MalatiumRevealRange, maxRevealRange, (flareMult - 1f) / 1.5f);
        
        // Optimized high-performance registry scan
        var enemies = MistbornRegistry.ActiveEnemies;
        HashSet<GameObject> currentTargets = new HashSet<GameObject>();

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.gameObject == gameObject) continue;

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
            // Malatium shadows use a distinct color
            gr.SetupGhost(target, AllomancyConstants.MalatiumGhostColor, AllomancyConstants.MalatiumGhostColor.a);
            activeGhosts.Add(target, gr);
        }

        GhostRenderer ghost = activeGhosts[target];
        // Shadow stands slightly to the side/behind to show the "other" person
        Vector3 offset = target.transform.right * AllomancyConstants.MalatiumShadowOffset.x + target.transform.forward * AllomancyConstants.MalatiumShadowOffset.z;
        ghost.UpdateTransform(target.transform.position + offset, target.transform.rotation);
    }


    void ResetReveals()
    {
        foreach (var ghost in activeGhosts.Values)
        {
            if (ghost != null) Destroy(ghost);
        }
        activeGhosts.Clear();
    }


    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.8f, 0.4f, 0.2f, 0.3f);
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            float currentRange = Mathf.Lerp(baseRevealRange, maxRevealRange, (flareMult - 1f) / 1.5f);
            Gizmos.DrawWireSphere(transform.position, currentRange);
        }
    }
}