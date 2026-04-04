using UnityEngine;

/// <summary>
/// Duralumin Allomancy — supercharges the NEXT metal burned.
/// Lore: Burning Duralumin causes an explosive burst of whatever other metal is being burned.
/// The burst is incredibly powerful (10x normal) but instantly drains both the target metal
/// AND the Duralumin reserves. A single Duralumin-enhanced Steel Push can launch a person
/// across the city. A Duralumin-Pewter flare gives brief superhuman strength.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 140)]
public class Duralumin : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Multiplier applied to the next metal burned (handbook: ~10x)")]
    public float burstMultiplier = 10f;
    public float burstDuration = 0.5f;

    [Header("Specific Burst Effects")]
    public float steelBurstForce = 500f;
    public float ironBurstForce = 400f;
    public float pewterBurstStrengthMult = 5f;
    public float tinBurstOverloadChance = 0.8f;

    [Header("References")]
    public Allomancer allomancer;

    private bool isBurning = false;
    private bool hasPrimed = false;
    private float primeTimer = 0f;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning()
                 && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Duralumin;

        if (isBurning && !wasBurning)
        {
            PrimeBurst();
        }

        // Auto-expire prime if player takes too long
        if (hasPrimed)
        {
            primeTimer -= Time.deltaTime;
            if (primeTimer <= 0f)
            {
                CancelPrime();
            }
        }
    }

    void PrimeBurst()
    {
        hasPrimed = true;
        primeTimer = 5f; // 5 seconds to use the burst
        allomancer.isDuraluminPrimed = true;

        // VFX: glowing intensification
        CameraShakeManager.Instance?.Shake(0.3f, 0.1f);
        SoundManager.Instance?.PlayFlareSound();


        // Drain Duralumin itself
        allomancer.DrainMetal(AllomancySkill.MetalType.Duralumin,
            allomancer.GetMetalReserve(AllomancySkill.MetalType.Duralumin));
        allomancer.StopBurning();
    }

    void CancelPrime()
    {
        hasPrimed = false;
        if (allomancer != null) allomancer.isDuraluminPrimed = false;
    }

    /// <summary>
    /// Called when a Duralumin-primed metal is burned. Returns the burst multiplier.
    /// </summary>
    public float GetBurstMultiplier() => hasPrimed ? burstMultiplier : 1f;

    /// <summary>
    /// Apply specific burst effects based on which metal was burned with Duralumin.
    /// Called by the individual metal scripts when they detect isDuraluminPrimed.
    /// </summary>
    public void ApplyBurstEffect(AllomancySkill.MetalType metal, GameObject player)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:
                // Massive push — launches nearby metals and player recoils hard
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddForce(-player.transform.forward * steelBurstForce, ForceMode.Impulse);
                CameraShakeManager.Instance?.Shake(0.5f, 0.4f);
                break;

            case AllomancySkill.MetalType.Iron:
                // Massive pull — yank all nearby metals violently toward player
                CameraShakeManager.Instance?.Shake(0.5f, 0.4f);
                break;

            case AllomancySkill.MetalType.Pewter:
                // Brief superhuman mode — already handled by FlareManager multiplier
                break;

            case AllomancySkill.MetalType.Tin:
                // Sensory explosion — can stun enemies but also overloads player
                CameraShakeManager.Instance?.Shake(1f, 0.6f);
                break;

            case AllomancySkill.MetalType.Zinc:
                // Emotional shockwave — instant max-power riot on all enemies in range
                player.GetComponentInChildren<Zinc>()?.TriggerDuraluminBurst();
                break;

            case AllomancySkill.MetalType.Brass:
                // Mass Soothe — instant max-power calm/stun on all enemies in range
                player.GetComponentInChildren<Brass>()?.TriggerDuraluminBurst();
                break;

            case AllomancySkill.MetalType.Copper:
                // Massive coppercloud — hides all allies in huge radius for a burst
                CameraShakeManager.Instance?.Shake(0.3f, 0.2f);
                break;

            case AllomancySkill.MetalType.Bronze:
                // Seeker pulse — detect ALL Allomancers on the map for a brief moment
                CameraShakeManager.Instance?.Shake(0.4f, 0.3f);
                break;

            case AllomancySkill.MetalType.Gold:
                // Intense vision of past self — brief stun/disorientation
                CameraShakeManager.Instance?.Shake(0.8f, 0.5f);
                break;

            case AllomancySkill.MetalType.Electrum:
                // Flash of future shadows — brief invulnerability (dodge window)
                CameraShakeManager.Instance?.Shake(0.5f, 0.3f);
                break;

            case AllomancySkill.MetalType.Atium:
                // Extended future vision — if Duralumin + Atium, see far into the future
                CameraShakeManager.Instance?.Shake(1f, 0.8f);
                break;

            case AllomancySkill.MetalType.Bendalloy:
                // Massive speed bubble — very fast time inside for a brief duration
                CameraShakeManager.Instance?.Shake(0.4f, 0.2f);
                break;

            case AllomancySkill.MetalType.Cadmium:
                // Massive slow bubble — nearly freezes time in large area briefly
                CameraShakeManager.Instance?.Shake(0.4f, 0.2f);
                break;

            case AllomancySkill.MetalType.Chromium:
                // Leeching burst — strip ALL metals from ALL nearby Allomancers
                CameraShakeManager.Instance?.Shake(0.5f, 0.3f);
                break;

            case AllomancySkill.MetalType.Nicrosil:
                // Nicroburst — supercharge target's current burn to extreme levels
                CameraShakeManager.Instance?.Shake(0.5f, 0.3f);
                break;

            case AllomancySkill.MetalType.Aluminum:
                // Duralumin + Aluminum = purge everything (both drain, nothing happens)
                // This is lore-accurate — a waste of both metals
                break;
        }

        // Lore: burning Duralumin instantly expends ALL of the paired metal too
        if (allomancer != null)
            allomancer.DrainMetal(metal, allomancer.GetMetalReserve(metal));

        hasPrimed = false;
        if (allomancer != null) allomancer.isDuraluminPrimed = false;
        SoundManager.Instance?.PlayDuraluminBurst();
    }

    public bool IsPrimed() => hasPrimed;
}
