using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Copper Allomancy ability (Smoker).
/// Hides Allomantic pulses of the user and others within the cloud from Seekers (Bronze).
/// </summary>
public class Copper : MonoBehaviour
{
    // Static registry of all active copper clouds for Seeker (Bronze) to check
    public static List<Copper> ActiveClouds = new List<Copper>();

    [Header("Settings")]
    public float baseCloudRadius = 10f;
    
    [Header("Flare Boosts")]
    public float maxCloudRadius = 25f;

    [Header("References")]
    public Allomancer allomancer;
    
    private bool isBurning = false;
    private float currentRadius;

    void OnEnable() => ActiveClouds.Add(this);
    void OnDisable()
    {
        ActiveClouds.Remove(this);
        ResetEffects();
    }

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        // Check if we are currently burning Copper according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Copper;

        if (isBurning)
        {
            float flareMult = GetFlareMultiplier();
            currentRadius = Mathf.Lerp(baseCloudRadius, maxCloudRadius, (flareMult - 1f) / 1.5f);
            // The actual effect is passive for the user, but ActiveClouds lets Bronze check.
        }
        else if (wasBurning)
        {
            ResetEffects();
        }
    }
    
    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.flareIntensity;
        }
        return 1.0f;
    }

    void ResetEffects()
    {
        currentRadius = 0f;
    }

    /// <summary>
    /// Checks if a specific position is hidden within this copper cloud.
    /// </summary>
    public bool IsPositionHidden(Vector3 position)
    {
        if (!isBurning) return false;
        return Vector3.Distance(transform.position, position) <= currentRadius;
    }

    /// <summary>
    /// Static helper to check if any active copper cloud covers a position.
    /// </summary>
    public static bool IsPulseHidden(Vector3 position)
    {
        foreach (var cloud in ActiveClouds)
        {
            if (cloud.IsPositionHidden(position)) return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (isBurning)
        {
            Gizmos.color = new Color(0.5f, 0.4f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}