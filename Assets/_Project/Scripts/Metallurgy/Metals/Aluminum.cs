using UnityEngine;

/// <summary>
/// Aluminum Metallurgy — purges ALL of the user's metal reserves instantly.
/// Lore: Aluminum is the "internal wipe." A Ashwalker burns it to clear their own system,
/// useful for purging unwanted metals (e.g., poisoned alloys) or resetting Duralumin priming.
/// Also clears active Storecraft storage and Compounding.
/// </summary>
[PlayerComponent("Metallurgy Metals", order: 130)]
public class Aluminum : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time to fully purge all reserves (lore: instant, game: brief for visual feedback)")]
    public float purgeDelay = 0.1f;
    public float selfDrainRate = 50f;

    [Header("Visual")]
    public float screenFlashDuration = 0.3f;

    [Header("References")]
    public Metallurgist metallurgist;

    private bool isBurning = false;
    private bool hasPurged = false;

    void Start()
    {
        if (metallurgist == null)
            metallurgist = GetComponentInParent<Metallurgist>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = metallurgist != null && metallurgist.IsBurning()
                 && metallurgist.GetCurrentMetal() == MetallurgySkill.MetalType.Aluminum;

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
        if (metallurgist == null) return;
        hasPurged = true;

        // Clear all Metallurgic reserves
        metallurgist.ClearAllReserves();

        // Clear Duralumin priming
        metallurgist.isDuraluminPrimed = false;
        metallurgist.isNicrobursting = false;

        // Clear Storecraft if present
        Storecrafter storecrafter = GetComponentInParent<Storecrafter>();
        if (storecrafter != null)
        {
            for (int i = 0; i < Storecrafter.MetalmindCount; i++)
            {
                storecrafter.StopStoring(i);
                storecrafter.StopTapping(i);
            }
        }

        // Clear Compounding
        Compounding compounding = GetComponentInParent<Compounding>();
        if (compounding != null)
        {
            for (int i = 0; i < Storecrafter.MetalmindCount; i++)
                compounding.ForceStopCompounding(i);
        }

        // Screen flash — white pulse for the "emptiness" feeling
        CameraShakeManager.Instance?.Shake(0.2f, 0.15f);
        SoundManager.Instance?.PlayImpactSound();

        // Stop burning
        metallurgist.StopBurning();

    }
}
