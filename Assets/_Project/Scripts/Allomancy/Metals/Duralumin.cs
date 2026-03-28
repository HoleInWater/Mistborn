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
        }

        hasPrimed = false;
        if (allomancer != null) allomancer.isDuraluminPrimed = false;
        SoundManager.Instance?.PlayDuraluminBurst();
    }

    public bool IsPrimed() => hasPrimed;
}
