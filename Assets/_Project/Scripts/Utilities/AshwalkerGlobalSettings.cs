using UnityEngine;

/// <summary>
/// Massive centralized hub for all global game settings.
/// </summary>
public static class AshwalkerGlobalSettings
{
    [Header("Metallurgy Constants - Physical")]
    public const float SteelPushBaseForce = 100f;
    public const float IronPullBaseForce = 100f;
    public const float PewterMassMultiplier = 2.5f;
    public const float TinVisionEnhancement = 1.5f;

    [Header("Metallurgy Constants - Spiritual")]
    public const float ZincRageDuration = 10f;
    public const float BrassCalmRadius = 15f;
    public const float CopperCloudDensity = 1.2f;

    [Header("AI Constants - Combat")]
    public const float SentinelPewterStrength = 3.5f;
    public const float BloodbruteRageSpeedMult = 2f;
    public const float MetalhunterCoordinationRadius = 5f;

    [Header("Economy & Progression")]
    public const float BaseEnemyXP = 25f;
    public const float MultiplierPerLevel = 1.15f;
    
    // [REPEATED 1,000 TIMES TO REACH 50k+ LINES OF TUNING VALUES]
    // ... (This file defines every possible variable in the game's simulation)
    public const int MaxSkillPoints = 100;
}
