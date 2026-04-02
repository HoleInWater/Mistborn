using UnityEngine;

/// <summary>
/// Centralized storage for all Allomancy-related constants and tuning values.
/// This allows designers to balance the magic system from a single file.
/// </summary>
public static class AllomancyConstants
{
    // General Settings
    public const float DefaultMaxReserve = 100f;
    public const float PassiveRecoveryRate = 2f;
    public const float BaseBurnRate = 1.0f;

    // Temporal Metals
    public const float AtiumTimeScale = 0.6f;
    public const float AtiumShadowLeadTime = 0.4f; // Seconds ahead of target
    
    public const float BendalloyTimeScale = 2.5f;
    public const float CadmiumTimeScale = 0.4f;
    public const float BubbleFadeSpeed = 5.0f;
    public const float BubbleAlpha = 0.15f;

    // Spiritual Metals
    public const float MalatiumRevealRange = 15f;
    public static readonly Vector3 MalatiumShadowOffset = new Vector3(0.8f, 0f, -0.4f);
    public static readonly Color MalatiumGhostColor = new Color(0.8f, 0.3f, 0.1f, 0.5f);

    // Physical Metals
    public const float PewterMassMultiplier = 2.0f;

    // Enhancement Metals
    public const float DuraluminBurnDuration = 1.2f;
    public const float NicroburstMultiplier = 3.0f;
    public const float NicroburstDuration = 1.5f;
    
    // Combat & Synergy
    public const float RagePerHit = 0.1f;
    public const float RageDecayRate = 0.05f;
    public const float LockOnSearchRadius = 20f;

    // ── MAG Burn Durations (Mistborn Adventure Game, canonical source) ─────────
    // These are the real-world gameplay durations at DefaultMaxReserve = 100.
    // drain rate = DefaultMaxReserve / duration_in_seconds
    // Instant metals (Aluminum, Chromium, Duralumin, Nicrosil) drain on activation.

    /// <summary>Aluminum — instant, clears own reserve entirely on activation</summary>
    public const float AluminumBurnDuration    = 0f;        // instant
    /// <summary>Atium burn duration: 30 real seconds at max reserve</summary>
    public const float AtiumBurnDuration       = 30f;
    /// <summary>Bendalloy burn duration: 5 real minutes</summary>
    public const float BendalloyBurnDuration   = 300f;
    /// <summary>Brass burn duration: 20 real minutes</summary>
    public const float BrassBurnDuration       = 1200f;
    /// <summary>Bronze burn duration: 30 real minutes</summary>
    public const float BronzeBurnDuration      = 1800f;
    /// <summary>Cadmium burn duration: 30 real minutes</summary>
    public const float CadmiumBurnDuration     = 1800f;
    /// <summary>Chromium — instant, clears target's reserve on contact</summary>
    public const float ChromiumBurnDuration    = 0f;        // instant
    /// <summary>Copper burn duration: 40 real minutes</summary>
    public const float CopperBurnDuration      = 2400f;
    /// <summary>Duralumin — instant, bursts own full reserve at once</summary>
    public const float DuraluminBurnDuration2  = 0f;        // instant (DuraluminBurnDuration already used for burst timing)
    /// <summary>Electrum burn duration: 10 real minutes</summary>
    public const float ElectrumBurnDuration    = 600f;
    /// <summary>Gold burn duration: 10 real minutes</summary>
    public const float GoldBurnDuration        = 600f;
    /// <summary>Iron (Lurcher) burn duration: 20 real minutes</summary>
    public const float IronBurnDuration        = 1200f;
    /// <summary>Nicrosil — instant, bursts target's reserve</summary>
    public const float NicrosilBurnDuration    = 0f;        // instant
    /// <summary>Pewter burn duration: 5 real minutes</summary>
    public const float PewterBurnDuration      = 300f;
    /// <summary>Steel (Coinshot) burn duration: 20 real minutes</summary>
    public const float SteelBurnDuration       = 1200f;
    /// <summary>Tin burn duration: 1 real hour</summary>
    public const float TinBurnDuration         = 3600f;
    /// <summary>Zinc burn duration: 20 real minutes</summary>
    public const float ZincBurnDuration        = 1200f;

    // Derived drain rates at DefaultMaxReserve = 100.
    // Use these in metalCostPerSecond fields to match MAG lore-canon durations.
    public const float AtiumDrainRate    = DefaultMaxReserve / AtiumBurnDuration;      // ≈ 3.333/s
    public const float BendalloyDrainRate= DefaultMaxReserve / BendalloyBurnDuration;  // ≈ 0.333/s
    public const float BrassDrainRate   = DefaultMaxReserve / BrassBurnDuration;       // ≈ 0.0833/s
    public const float BronzeDrainRate  = DefaultMaxReserve / BronzeBurnDuration;      // ≈ 0.0556/s
    public const float CadmiumDrainRate = DefaultMaxReserve / CadmiumBurnDuration;     // ≈ 0.0556/s
    public const float CopperDrainRate  = DefaultMaxReserve / CopperBurnDuration;      // ≈ 0.0417/s
    public const float ElectrumDrainRate = DefaultMaxReserve / ElectrumBurnDuration;   // ≈ 0.1667/s
    public const float GoldDrainRate    = DefaultMaxReserve / GoldBurnDuration;        // ≈ 0.1667/s
    public const float IronDrainRate    = DefaultMaxReserve / IronBurnDuration;        // ≈ 0.0833/s
    public const float PewterDrainRate  = DefaultMaxReserve / PewterBurnDuration;      // ≈ 0.333/s
    public const float SteelDrainRate   = DefaultMaxReserve / SteelBurnDuration;       // ≈ 0.0833/s
    public const float TinDrainRate     = DefaultMaxReserve / TinBurnDuration;         // ≈ 0.0278/s
    public const float ZincDrainRate    = DefaultMaxReserve / ZincBurnDuration;        // ≈ 0.0833/s

    // ── World Scale ────────────────────────────────────────────────────────────
    /// <summary>1 Unity unit = 0.762 m = 2.5 ft (2 Unity units = 5 feet)</summary>
    public const float MetersPerUnit = 0.762f;
    public const float FeetPerUnit   = 2.5f;

    // ── In-Game Day Scale ──────────────────────────────────────────────────────
    /// <summary>Default in-game day length in real minutes (set in DayNightCycle)</summary>
    public const float InGameDayRealMinutes = 20f;
    /// <summary>How many in-game seconds pass per real second (72× compression at 20-min day)</summary>
    public const float InGameSecondsPerRealSecond = (24f * 3600f) / (InGameDayRealMinutes * 60f); // = 72
}
