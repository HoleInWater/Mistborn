/* MistCycle.cs
 *
 * Controls the global mist behavior for the gameplay world.
 * Integrates with DayNightCycle to bring mists at night and clear them at dawn.
 *
 * Lore from the books:
 *   - Mists come every night without fail
 *   - They begin rolling in at dusk (sunset ~18:00-20:00)
 *   - They cover the land completely by full darkness (~20:00)
 *   - They begin retreating at dawn (~6:00-8:00)
 *   - They are completely gone by mid-morning
 *   - Mists are thicker in certain seasons (Fallingmonth through Lastmonth)
 *   - Mistborn can see through the mists better than normal people
 *   - The mists avoid fires and bright lights (slightly)
 *   - In later books, the mists sometimes come during the day (Ruin's influence)
 *
 * This script manages MistSystem instances across the game world,
 * fading them in/out based on time of day.
 */

using UnityEngine;
using System.Collections.Generic;

public class MistCycle : MonoBehaviour
{
    public static MistCycle Instance { get; private set; }

    [Header("Timing (in-game hours)")]
    public float mistStartHour  = 18f;  // Mists begin rolling in
    public float mistFullHour   = 20f;  // Mists at full density
    public float mistFadeHour   = 6f;   // Mists begin retreating
    public float mistGoneHour   = 8f;   // Mists completely gone

    [Header("Density")]
    [Range(0f, 1f)]
    public float currentMistDensity;
    public float maxDensity = 1f;
    public float seasonalBonus = 0f;    // Set by ScadrialCalendar during mist season

    [Header("Ruin's Influence (story progression)")]
    [Tooltip("0 = normal mists (night only). 1 = mists come during the day too.")]
    [Range(0f, 1f)]
    public float ruinInfluence = 0f;

    [Header("Mist Systems")]
    public List<MistSystem> managedMistSystems = new List<MistSystem>();

    [Header("Visibility")]
    [Tooltip("Normal visibility range in the mists (meters)")]
    public float normalVisibilityRange = 10f;
    [Tooltip("Mistborn visibility range (Tin burning extends this)")]
    public float mistbornVisibilityRange = 30f;

    [Header("Fog Integration")]
    public bool controlSceneFog = true;
    public float clearFogDensity = 0.002f;
    public float mistyFogDensity = 0.03f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (DayNightCycle.Instance == null) return;

        float hour = DayNightCycle.Instance.GetHour();
        currentMistDensity = CalculateMistDensity(hour);

        // Apply seasonal bonus
        if (ScadrialCalendar.Instance != null && ScadrialCalendar.Instance.IsMistSeason())
            seasonalBonus = 0.2f;
        else
            seasonalBonus = 0f;

        float finalDensity = Mathf.Clamp01(currentMistDensity + seasonalBonus);

        // Update all managed mist systems
        foreach (var ms in managedMistSystems)
        {
            if (ms == null) continue;
            ms.emissionRate = ms.emissionRate * finalDensity;
        }

        // Update scene fog
        if (controlSceneFog)
        {
            RenderSettings.fogDensity = Mathf.Lerp(clearFogDensity, mistyFogDensity, finalDensity);
        }
    }

    float CalculateMistDensity(float hour)
    {
        // Ruin's influence: mists linger during the day
        float baseDayDensity = ruinInfluence * 0.3f;

        // Night: full mists
        if (hour >= mistFullHour || hour < mistFadeHour)
            return maxDensity;

        // Dusk: rolling in
        if (hour >= mistStartHour && hour < mistFullHour)
        {
            float t = (hour - mistStartHour) / (mistFullHour - mistStartHour);
            return Mathf.Lerp(baseDayDensity, maxDensity, t * t); // ease-in
        }

        // Dawn: retreating
        if (hour >= mistFadeHour && hour < mistGoneHour)
        {
            float t = (hour - mistFadeHour) / (mistGoneHour - mistFadeHour);
            return Mathf.Lerp(maxDensity, baseDayDensity, t); // linear fade
        }

        // Day: no mists (unless Ruin's influence)
        return baseDayDensity;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public bool AreMistsActive() => currentMistDensity > 0.1f;
    public float GetDensity() => currentMistDensity;

    /// <summary>
    /// Get the visibility range for a given position.
    /// Mistborn with Tin have much better mist visibility.
    /// </summary>
    public float GetVisibilityRange(bool isBurningTin = false)
    {
        if (currentMistDensity < 0.1f) return 200f; // clear day
        float baseRange = isBurningTin ? mistbornVisibilityRange : normalVisibilityRange;
        return baseRange / Mathf.Max(0.1f, currentMistDensity);
    }

    /// <summary>Set Ruin's influence for story progression.</summary>
    public void SetRuinInfluence(float influence)
    {
        ruinInfluence = Mathf.Clamp01(influence);
    }

    /// <summary>Register a MistSystem to be managed by the cycle.</summary>
    public void RegisterMistSystem(MistSystem system)
    {
        if (!managedMistSystems.Contains(system))
            managedMistSystems.Add(system);
    }

    public void UnregisterMistSystem(MistSystem system)
    {
        managedMistSystems.Remove(system);
    }
}
