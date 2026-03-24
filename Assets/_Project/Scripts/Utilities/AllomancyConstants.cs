using UnityEngine;

/// <summary>
/// Centralized storage for all Allomancy-related constants and tuning values.
/// This allows designers to balance the magic system from a single file.
/// </summary>
public static class AllomancyConstants
{
    [Header("General Settings")]
    public const float DefaultMaxReserve = 100f;
    public const float PassiveRecoveryRate = 2f;
    public const float BaseBurnRate = 1.0f;

    [Header("Temporal Metals")]
    public const float AtiumTimeScale = 0.6f;
    public const float AtiumShadowLeadTime = 0.4f; // Seconds ahead of target
    
    public const float BendalloyTimeScale = 2.5f;
    public const float CadmiumTimeScale = 0.4f;
    public const float BubbleFadeSpeed = 5.0f;
    public const float BubbleAlpha = 0.15f;

    [Header("Spiritual Metals")]
    public const float MalatiumRevealRange = 15f;
    public static readonly Vector3 MalatiumShadowOffset = new Vector3(0.8f, 0f, -0.4f);
    public static readonly Color MalatiumGhostColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);

    [Header("Enhancement Metals")]
    public const float DuraluminBurnDuration = 1.2f;
    public const float NicroburstMultiplier = 3.0f;
    public const float NicroburstDuration = 1.5f;
    
    [Header("Combat & Synergy")]
    public const float RagePerHit = 0.1f;
    public const float RageDecayRate = 0.05f;
    public const float LockOnSearchRadius = 20f;
}
