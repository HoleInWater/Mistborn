using UnityEngine;

/// <summary>
/// Aluminum Allomancy — purges ALL of the user's metal reserves instantly.
/// Lore: Aluminum is the "internal wipe." A Mistborn burns it to clear their own system,
/// useful for purging unwanted metals (e.g., poisoned alloys) or resetting Duralumin priming.
/// Also clears active Feruchemy storage and Compounding.
/// </summary>
public class Aluminum : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time to fully purge all reserves (lore: instant, game: brief for visual feedback)")]
    public float purgeDelay = 0.1f;
    public float selfDrainRate = 50f;

    [Header("Visual")]
    public float screenFlashDuration = 0.3f;

    [Header("References")]
    public Allomancer allomancer;

    private bool isBurning = false;
    private bool hasPurged = false;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning()
                 && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Aluminum;

        if (isBurning && !wasBurning)
        {
            hasPurged = false;
        }

        if (isBurning && !hasPurged)
        {
            PurgeReserves();
        }
    }

    void PurgeReserves()
    {
        if (allomancer == null) return;
        hasPurged = true;

        // Clear all Allomantic reserves
        allomancer.ClearAllReserves();

        // Clear Duralumin priming
        allomancer.isDuraluminPrimed = false;
        allomancer.isNicrobursting = false;

        // Clear Feruchemy if present
        Feruchemist feruchemist = GetComponent<Feruchemist>();
        if (feruchemist != null)
        {
            for (int i = 0; i < Feruchemist.MetalmindCount; i++)
            {
                feruchemist.StopStoring(i);
                feruchemist.StopTapping(i);
            }
        }

        // Clear Compounding
        Compounding compounding = GetComponent<Compounding>();
        if (compounding != null)
        {
            for (int i = 0; i < Feruchemist.MetalmindCount; i++)
                compounding.ForceStopCompounding(i);
        }

        // Screen flash — white pulse for the "emptiness" feeling
        CameraShakeManager.Instance?.Shake(0.2f, 0.15f);
        SoundManager.Instance?.PlayImpactSound();

        // Stop burning
        allomancer.StopBurning();

    }
}
