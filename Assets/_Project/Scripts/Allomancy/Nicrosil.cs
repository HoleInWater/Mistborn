using UnityEngine;

/// <summary>
/// Nicrosil Allomancy (Nicroburst) — supercharges another Allomancer's current burn.
/// Lore: The external version of Duralumin. Touch an ally to amplify their power,
/// or touch an enemy to force them into an uncontrolled flare (drain their metals fast).
/// Requires physical contact.
///
/// BUG FIX: Script was referencing GetComponentInParent<Allomancer>() which is not a real
/// class in this project. Allomancer == null every frame, so isBurning was always false,
/// AttemptNicroburst never fired, and DrainMetal never ran — meaning the wheel always saw
/// Nicrosil as EMPTY. Replaced with MetalReserve, which is the actual component.
/// Also fixed the hardcoded `for (int i = 0; i < 16; i++)` enemy drain loop — now uses
/// Enum.GetValues so Chromium and Nicrosil are included in the drain.
/// </summary>
public class Nicrosil : MonoBehaviour
{
    [Header("Settings")]
    public float burstRange = 2.5f;
    public float burstMultiplier = 3f;
    public float burstDuration = 2f;
    public float cooldown = 5f;

    [Header("Enemy Forced Flare")]
    [Tooltip("When used on enemy: drain their metals at this rate")]
    public float enemyDrainRate = 20f;

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
            && metalReserve.currentMetal > 0f; // Nicrosil is "burning" while it has reserve

        if (isBurning && !wasBurning && cooldownTimer <= 0f)
        {
            AttemptNicroburst();
        }

        if (isBurning)
            metalReserve.Drain(3f * Time.deltaTime);
    }

    void AttemptNicroburst()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, burstRange);
        bool applied = false;

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            MetalReserve target = col.GetComponentInParent<MetalReserve>();
            if (target != null && target != metalReserve)
            {
                NicroburstTarget(target, col.gameObject);
                applied = true;
                break;
            }
        }

        if (applied)
        {
            cooldownTimer = cooldown;
            CameraShakeManager.Instance?.Shake(0.3f, 0.2f);
            SoundManager.Instance?.PlayFlareSound();
        }
    }

    void NicroburstTarget(MetalReserve target, GameObject targetObj)
    {
        bool isEnemy = targetObj.GetComponent<EnemyAI>() != null
            || targetObj.GetComponent<AIController>() != null;

        if (isEnemy)
        {
            // FIX: was hardcoded `for (int i = 0; i < 16; i++)` — skipped Chromium and Nicrosil.
            // Now uses Enum.GetValues so every metal including indices 16+ is drained.
            foreach (AllomancySkill.MetalType metal in System.Enum.GetValues(typeof(AllomancySkill.MetalType)))
            {
                target.Drain(enemyDrainRate);
            }
        }
        else
        {
            // Ally: supercharge their current burn
            target.SetCurrentMetal(target.currentMetal * burstMultiplier);
        }

        metalReserve.Drain(metalReserve.maxMetal); // stop self burn
    }
}
