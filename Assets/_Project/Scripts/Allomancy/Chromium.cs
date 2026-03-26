using UnityEngine;

/// <summary>
/// Chromium Allomancy (Leecher) — wipes another Allomancer's metal reserves on touch.
/// Lore: The external version of Aluminum. Requires physical contact.
/// Also disrupts enemy Feruchemy and Compounding temporarily.
///
/// BUG FIX: Script was referencing GetComponentInParent<Allomancer>() which is not a real
/// class in this project. Allomancer == null every frame, so isBurning was always false,
/// AttemptLeech never fired, and DrainMetal never ran — meaning the wheel always saw
/// Chromium as EMPTY. Replaced with MetalReserve, which is the actual component.
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
    public MetalReserve metalReserve; // FIX: was Allomancer allomancer (nonexistent class)

    private bool isBurning = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        // FIX: was GetComponentInParent<Allomancer>() — always returned null
        if (metalReserve == null)
            metalReserve = GetComponentInParent<MetalReserve>();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        bool wasBurning = isBurning;

        // FIX: was allomancer.IsBurning() / allomancer.GetCurrentMetal() — both null-crashed silently
        isBurning = metalReserve != null
            && metalReserve.currentMetal > 0f; // Chromium is "burning" while it has reserve

        if (isBurning && !wasBurning && cooldownTimer <= 0f)
        {
            AttemptLeech();
        }

        if (isBurning)
            metalReserve.Drain(3f * Time.deltaTime);
    }

    void AttemptLeech()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, leechRange);
        bool leeched = false;

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            MetalReserve target = col.GetComponentInParent<MetalReserve>();
            if (target != null && target != metalReserve)
            {
                LeechTarget(target, col.gameObject);
                leeched = true;
                break;
            }
        }

        if (leeched)
        {
            cooldownTimer = leechCooldown;
            CameraShakeManager.Instance?.Shake(0.2f, 0.15f);
            SoundManager.Instance?.PlayImpactSound();
        }
    }

    void LeechTarget(MetalReserve target, GameObject targetObj)
    {
        // Wipe all metal reserves on the target by draining everything
        foreach (AllomancySkill.MetalType metal in System.Enum.GetValues(typeof(AllomancySkill.MetalType)))
        {
            target.Drain(target.maxMetal); // drain full amount for every metal
        }

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

        // Stun the Lord Ruler if we leech him
        LordRulerBoss lordRuler = targetObj.GetComponent<LordRulerBoss>();
        if (lordRuler != null)
            lordRuler.StunAndExpose();

        metalReserve.Drain(metalReserve.maxMetal); // stop self burn too
    }
}
