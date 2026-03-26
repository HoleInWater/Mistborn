using UnityEngine;

/// <summary>
/// WORLD SCALE REFERENCE — Mistborn World Units
///
/// 2 Unity units = 5 feet in the Mistborn world
/// 1 Unity unit = 2.5 feet = 0.762 meters
///
/// Use these constants to convert between real-world measurements
/// and Unity units throughout the codebase.
///
/// Examples:
///   A 6-foot person = 2.4 Unity units tall
///   A 10-foot room = 4 Unity units
///   A 30-foot detection range = 12 Unity units
///   A Koloss at 12 feet = 4.8 Unity units
///   Kredik Shaw spire 200 feet = 80 Unity units
/// </summary>
public static class WorldScale
{
    // ── Core Conversion ──────────────────────────────────────────────────
    public const float UnityUnitsPerFoot = 0.4f;         // 2 units / 5 feet
    public const float FeetPerUnityUnit = 2.5f;
    public const float MetersPerUnityUnit = 0.762f;
    public const float UnityUnitsPerMeter = 1.312f;      // 1 / 0.762

    // ── Conversion Methods ───────────────────────────────────────────────
    public static float FeetToUnits(float feet) => feet * UnityUnitsPerFoot;
    public static float UnitsToFeet(float units) => units * FeetPerUnityUnit;
    public static float MetersToUnits(float meters) => meters * UnityUnitsPerMeter;
    public static float UnitsToMeters(float units) => units * MetersPerUnityUnit;

    // ── Common Mistborn World Measurements ───────────────────────────────

    // Character Heights
    public static readonly float PlayerHeight = FeetToUnits(5.5f);        // ~5'6" Vin-sized
    public static readonly float KelsierHeight = FeetToUnits(6f);         // 6'
    public static readonly float KolossSmall = FeetToUnits(7f);           // Young Koloss
    public static readonly float KolossLarge = FeetToUnits(12f);          // Mature Koloss
    public static readonly float SteelInquisitorHeight = FeetToUnits(6.5f);

    // Building Scales
    public static readonly float SkaaHouseHeight = FeetToUnits(10f);      // One-story shack
    public static readonly float NobleKeepHeight = FeetToUnits(60f);      // Multi-story keep
    public static readonly float KredikShawSpire = FeetToUnits(200f);     // Tallest spire
    public static readonly float WallHeight = FeetToUnits(15f);           // City wall
    public static readonly float LamppostHeight = FeetToUnits(12f);       // Street lamppost
    public static readonly float DoorwayHeight = FeetToUnits(7f);

    // Distances
    public static readonly float MeleeRange = FeetToUnits(8f);            // Sword reach
    public static readonly float CoinPushRange = FeetToUnits(100f);       // Effective coin range
    public static readonly float AllomanticSightRange = FeetToUnits(200f); // Metal detection
    public static readonly float BronzeSeekerRange = FeetToUnits(500f);   // Allomantic pulse detection
    public static readonly float CopperCloudRadius = FeetToUnits(50f);    // Smoker cloud
    public static readonly float EmotionalRange = FeetToUnits(40f);       // Zinc/Brass range
    public static readonly float TimeBubbleRadius = FeetToUnits(20f);     // Bendalloy/Cadmium
    public static readonly float ShoutingDistance = FeetToUnits(100f);     // Can hear a shout

    // Speeds (feet per second → Unity units per second)
    public static readonly float WalkSpeed = FeetToUnits(4.5f);           // ~3 mph walk
    public static readonly float RunSpeed = FeetToUnits(15f);             // ~10 mph run
    public static readonly float SprintSpeed = FeetToUnits(25f);          // ~17 mph sprint
    public static readonly float PewterSprintSpeed = FeetToUnits(40f);    // ~27 mph Pewter sprint
    public static readonly float CoinVelocity = FeetToUnits(1600f);       // ~490 m/s pushed coin
    public static readonly float SteelPushRecoil = FeetToUnits(80f);      // Launch off building

    // Physics (PHYSICS-MATH-BOOK.md adapted to scale)
    public static readonly float Gravity = 9.81f * UnityUnitsPerMeter;   // ~12.88 units/s²
    public static readonly float TerminalVelocity = FeetToUnits(175f);    // ~120 mph
}
