using UnityEngine;

/// <summary>
/// Chromium Metallurgy (Leecher) — wipes another Metallurgist's metal reserves on touch.
/// Lore: The external version of Aluminum. Requires physical contact.
/// Also disrupts enemy Storecraft and Compounding temporarily.
/// </summary>
[PlayerComponent("Metallurgy Metals", order: 150)]
public class Chromium : MonoBehaviour
{
    [Header("Settings")]
    public float leechRange = 2.5f;
    public float leechCooldown = 3f;
    public float storecraftDisruptDuration = 5f;

    [Header("Visual")]
    public float leechScreenPulse = 0.3f;

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
            && metallurgist.GetCurrentMetal() == MetallurgySkill.MetalType.Chromium;

        if (isBurning && !wasBurning && cooldownTimer <= 0f)
        {
            AttemptLeech();
        }

        if (isBurning)
            metallurgist.DrainMetal(MetallurgySkill.MetalType.Chromium, 3f * Time.deltaTime);
    }

    void AttemptLeech()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, leechRange);
        bool leeched = false;

        foreach (var col in hits)
        {
            if (col.transform == transform) continue;

            Metallurgist target = col.GetComponentInParent<Metallurgist>();
            if (target != null && target != metallurgist)
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

    void LeechTarget(Metallurgist target, GameObject targetObj)
    {
        target.ClearAllReserves();
        target.StopBurning();

        Storecrafter storecrafter = targetObj.GetComponentInParent<Storecrafter>();
        if (storecrafter != null)
        {
            for (int i = 0; i < Storecrafter.MetalmindCount; i++)
            {
                storecrafter.StopStoring(i);
                storecrafter.StopTapping(i);
            }
        }

        Compounding compounding = targetObj.GetComponentInParent<Compounding>();
        if (compounding != null)
        {
            for (int i = 0; i < Storecrafter.MetalmindCount; i++)
                compounding.ForceStopCompounding(i);
        }

        AshenKingBoss ashenKing = targetObj.GetComponentInParent<AshenKingBoss>();
        if (ashenKing != null)
            ashenKing.StunAndExpose();

        metallurgist.StopBurning();
    }
}
