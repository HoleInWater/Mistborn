using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Copper Allomancy ability (Smoker).
/// Lore: Creates a "Coppercloud" that hides Allomantic pulses from Bronze seekers.
/// </summary>
public class Copper : MonoBehaviour
{
    [Header("Settings")]
    public float baseCloudRadius = 15f;
    public float maxCloudRadius = 40f;

    private Allomancer allomancer;
    private bool isBurning = false;
    private static List<Copper> activeClouds = new List<Copper>();

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
    }

    void OnEnable()
    {
        if (!activeClouds.Contains(this)) activeClouds.Add(this);
    }

    void OnDisable()
    {
        activeClouds.Remove(this);
    }

    void Update()
    {
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Copper;
    }

    /// <summary>
    /// Checks if a position is currently hidden by any active Coppercloud.
    /// Used by Bronze.cs to determine if it can detect an Allomancer.
    /// </summary>
    public static bool IsPulseHidden(Vector3 position)
    {
        foreach (var cloud in activeClouds)
        {
            if (cloud == null || !cloud.isBurning) continue;

            float flareMult = (FlareManager.Instance != null && cloud.gameObject == FlareManager.Instance.gameObject) 
                ? FlareManager.Instance.FlareMultiplier : 1.0f;
            
            float radius = Mathf.Lerp(cloud.baseCloudRadius, cloud.maxCloudRadius, (flareMult - 1f) / 1.5f);
            
            if (Vector3.Distance(position, cloud.transform.position) <= radius)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.8f, 0.4f, 0.2f, 0.3f);
            float radius = baseCloudRadius; // Simple preview
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}