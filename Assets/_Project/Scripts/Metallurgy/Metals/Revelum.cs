using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implements the Revelum Metallurgy ability.
/// Allows seeing a person’s past or what they could have been.
/// Standardized to follow the Metallurgist-centric burn system.
/// </summary>
[PlayerComponent("Metallurgy Metals", order: 100)]
public class Revelum : MonoBehaviour
{
    [Header("Settings")]
    public float baseRevealRange = 15f;
    public Color revelumColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);
    
    [Header("Flare Boosts")]
    public float maxRevealRange = 35f;

    [Header("References")]
    public Metallurgist metallurgist;
    
    private Dictionary<GameObject, GhostRenderer> activeGhosts = new Dictionary<GameObject, GhostRenderer>();
    private bool isBurning = false;

    void Start()
    {
        if (metallurgist == null)
            metallurgist = GetComponentInParent<Metallurgist>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = metallurgist != null && metallurgist.IsBurning() && metallurgist.GetCurrentMetal() == MetallurgySkill.MetalType.Revelum;

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
        float currentRange = Mathf.Lerp(baseRevealRange, maxRevealRange, (flareMult - 1f) / 1.5f);
        
        // Optimized high-performance registry scan
        var enemies = AshwalkerRegistry.ActiveEnemies;
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
            // Revelum shadows use a distinct color
            gr.SetupGhost(target, MetallurgyConstants.RevelumGhostColor, MetallurgyConstants.RevelumGhostColor.a);
            activeGhosts.Add(target, gr);
        }

        GhostRenderer ghost = activeGhosts[target];
        // Shadow stands slightly to the side/behind to show the "other" person
        Vector3 offset = target.transform.right * MetallurgyConstants.RevelumShadowOffset.x + target.transform.forward * MetallurgyConstants.RevelumShadowOffset.z;
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