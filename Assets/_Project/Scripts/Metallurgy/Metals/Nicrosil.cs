using UnityEngine;

/// <summary>
/// Nicrosil Metallurgy (Nicroburst) — supercharges another Metallurgist's current burn.
/// Lore: The external version of Duralumin. Touch an ally to amplify their power,
/// or touch an enemy to force them into an uncontrolled flare (drain their metals fast).
/// Requires physical contact.
///
/// BUG FIX: Enemy drain loop was hardcoded to `i < 16`, skipping Chromium and Nicrosil.
/// Fixed to use Enum.GetValues so all metals are always covered.
/// </summary>
[PlayerComponent("Metallurgy Metals", order: 160)]
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
    public Metallurgist metallurgist;

    private bool isBurning = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        if (metallurgist == null)
            metallurgist = GetComponentInParent<Metallurgist>();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        bool wasBurning = isBurning;
        isBurning = metallurgist != null && metallurgist.IsBurning()
            && metallurgist.GetCurrentMetal() == MetallurgySkill.MetalType.Nicrosil;

        if (isBurning && !wasBurning && cooldownTimer <= 0f)
        {
            AttemptNicroburst();
        }

        if (isBurning)
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Nicrosil, 3f * Time.deltaTime);
    }

    void AttemptNicroburst()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, burstRange);
        bool applied = false;

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            Metallurgist target = col.GetComponentInParent<Metallurgist>();
            if (target != null && target != metallurgist)
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

    void NicroburstTarget(Metallurgist target, GameObject targetObj)
    {
        bool isEnemy = targetObj.GetComponentInParent<EnemyAI>() != null
            || targetObj.GetComponentInParent<AIController>() != null;

        if (isEnemy)
        {
            // FIX: Was `for (int i = 0; i < 16; i++)` — skipped Chromium and Nicrosil.
            // Now uses Enum.GetValues to cover every metal regardless of count.
            foreach (MetallurgySkill.MetalType metal in System.Enum.GetValues(typeof(MetallurgySkill.MetalType)))
            {
                target.DrainMetal(metal, enemyDrainRate);
            }
        }
        else
        {
            target.isNicrobursting = true;
        }

        metallurgist.StopBurning();
    }
}
