using UnityEngine;

/// <summary>
/// Nicrosil Allomancy (Nicroburst) — supercharges another Allomancer's current burn.
/// Lore: The external version of Duralumin. Touch an ally to amplify their power,
/// or touch an enemy to force them into an uncontrolled flare (drain their metals fast).
/// Requires physical contact.
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
                 && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Nicrosil;

        if (isBurning && !wasBurning && cooldownTimer <= 0f)
        {
            AttemptNicroburst();
        }

        if (isBurning)
            allomancer.DrainMetal(AllomancySkill.MetalType.Nicrosil, 3f * Time.deltaTime);
    }

    void AttemptNicroburst()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, burstRange);
        bool applied = false;

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            Allomancer target = col.GetComponentInParent<Allomancer>();
            if (target != null && target != allomancer)
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
        // No target found
    }

    void NicroburstTarget(Allomancer target, GameObject targetObj)
    {
        // Check if target is ally or enemy
        bool isEnemy = targetObj.GetComponent<EnemyAI>() != null
                    || targetObj.GetComponent<AIController>() != null;

        if (isEnemy)
        {
            // Force uncontrolled flare — drain their metals rapidly
            // This is a tactical use: forcing an enemy to waste resources
            for (int i = 0; i < 16; i++)
            {
                AllomancySkill.MetalType metal = (AllomancySkill.MetalType)i;
                target.DrainMetal(metal, enemyDrainRate);
            }
        }
        else
        {
            // Ally: supercharge their current burn
            target.isNicrobursting = true;
        }

        allomancer.StopBurning();
    }
}
