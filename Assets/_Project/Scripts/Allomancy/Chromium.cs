using UnityEngine;

/// <summary>
/// Chromium Allomancy (Leecher) — wipes another Allomancer's metal reserves on touch.
/// Lore: The external version of Aluminum. Requires physical contact.
/// Also disrupts enemy Feruchemy and Compounding temporarily.
/// </summary>
public class Chromium : MonoBehaviour
{
    [Header("Settings")]
    public float leechRange = 2.5f;
    public float leechCooldown = 3f;
    public float feruchemyDisruptDuration = 5f;

    [Header("Visual")]
    public float leechScreenPulse = 0.3f;

    [Header("References")]
    public Allomancer allomancer;

    private bool isBurning = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning()
                 && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Chromium;

        if (isBurning && !wasBurning && cooldownTimer <= 0f)
        {
            AttemptLeech();
        }

        if (isBurning)
            allomancer.DrainMetal(AllomancySkill.MetalType.Chromium, 3f * Time.deltaTime);
    }

    void AttemptLeech()
    {
        // Scan for nearby Allomancers
        Collider[] hits = Physics.OverlapSphere(transform.position, leechRange);
        bool leeched = false;

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            Allomancer target = col.GetComponentInParent<Allomancer>();
            if (target != null && target != allomancer)
            {
                LeechTarget(target, col.gameObject);
                leeched = true;
                break; // Leech one target per activation
            }
        }

        if (leeched)
        {
            cooldownTimer = leechCooldown;
            CameraShakeManager.Instance?.Shake(0.2f, 0.15f);
            SoundManager.Instance?.PlayImpactSound();
        }
        // No target found — nothing to leech
    }

    void LeechTarget(Allomancer target, GameObject targetObj)
    {
        // Wipe Allomantic reserves
        target.ClearAllReserves();
        target.StopBurning();

        // Disrupt Feruchemy temporarily
        Feruchemist feruchemist = targetObj.GetComponent<Feruchemist>();
        if (feruchemist != null)
        {
            for (int i = 0; i < Feruchemist.MetalmindCount; i++)
            {
                feruchemist.StopStoring(i);
                feruchemist.StopTapping(i);
            }
        }

        // Disrupt Compounding
        Compounding compounding = targetObj.GetComponent<Compounding>();
        if (compounding != null)
        {
            for (int i = 0; i < Feruchemist.MetalmindCount; i++)
                compounding.ForceStopCompounding(i);
        }

        // Stun the Lord Ruler if we leech him (critical mechanic)
        LordRulerBoss lordRuler = targetObj.GetComponent<LordRulerBoss>();
        if (lordRuler != null)
            lordRuler.StunAndExpose();

        allomancer.StopBurning();
    }
}
