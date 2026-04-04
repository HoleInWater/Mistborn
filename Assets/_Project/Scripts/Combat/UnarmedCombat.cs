/* UnarmedCombat.cs
 *
 * Unarmed combat system — punches, kicks, and Pewter-enhanced strikes.
 *
 * From GDD Section 6 (Combat System):
 *   "Weapons: Daggers, Canes, Coins, Punches, Kicks, ect.
 *    Emphasis on improvisation."
 *
 * When the player has no weapon equipped, combat switches to unarmed.
 * Pewter burning dramatically enhances unarmed damage and unlocks
 * special moves (ground slam, rapid combo, throw).
 *
 * Unarmed attacks are faster than weapon attacks but deal less base damage.
 * With Pewter, unarmed becomes the most devastating option.
 */

using UnityEngine;

[PlayerComponent("Combat", order: 12)]
public class UnarmedCombat : MonoBehaviour
{
    [Header("Base Stats")]
    public float punchDamage = 8f;
    public float kickDamage = 12f;
    public float punchRange = 1.5f;
    public float kickRange = 2f;
    public float punchCooldown = 0.25f;
    public float kickCooldown = 0.6f;
    public float punchKnockback = 3f;
    public float kickKnockback = 8f;

    [Header("Pewter Enhancement")]
    [Tooltip("Damage multiplier when burning Pewter")]
    public float pewterDamageMultiplier = 3f;
    [Tooltip("Pewter unlocks special combo finishers")]
    public bool pewterComboEnabled = true;

    [Header("Combo System")]
    public int maxComboHits = 5;
    public float comboResetTime = 1.5f;
    [Tooltip("Damage multiplier for the final hit in a combo")]
    public float comboFinisherMultiplier = 2f;

    [Header("References")]
    public Animator animator;
    public LayerMask enemyLayer;
    public Transform attackOrigin;

    private EquipmentManager equipment;
    private Pewter pewterScript;
    private float punchTimer;
    private float kickTimer;
    private int comboCount;
    private float comboTimer;

    void Start()
    {
        equipment = GetComponentInParent<EquipmentManager>();
        pewterScript = GetComponentInParent<Pewter>();
        if (attackOrigin == null) attackOrigin = transform;
        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) enemyLayer = ~0;
    }

    void Update()
    {
        // Only active when no weapon equipped
        if (equipment != null && equipment.Equipped != null) return;

        punchTimer -= Time.deltaTime;
        kickTimer -= Time.deltaTime;
        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f) comboCount = 0;

        // Left click = punch
        if (Input.GetMouseButtonDown(0) && punchTimer <= 0f)
        {
            Punch();
        }

        // Right click hold > 0.3s = kick (tap = block/parry handled by ParrySystem)
        if (Input.GetMouseButton(1) && kickTimer <= 0f)
        {
            // Only kick if held long enough to not conflict with parry
            // ParrySystem handles the tap — we handle the hold
        }

        // Middle mouse = kick (dedicated)
        if (Input.GetMouseButtonDown(2) && kickTimer <= 0f)
        {
            Kick();
        }
    }

    void Punch()
    {
        punchTimer = punchCooldown;
        comboCount++;
        comboTimer = comboResetTime;

        bool isPewterActive = pewterScript != null && pewterScript.IsBurningPewter();
        float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

        // Calculate damage
        float damage = punchDamage;
        if (isPewterActive) damage *= pewterDamageMultiplier * flareMult;

        // Combo finisher
        bool isFinisher = comboCount >= maxComboHits;
        if (isFinisher)
        {
            damage *= comboFinisherMultiplier;
            comboCount = 0;
        }

        // Animation
        if (animator != null)
        {
            animator.SetTrigger("LightAttack");
            if (isFinisher) animator.SetTrigger("HeavyAttack");
        }

        // Hit detection
        PerformHit(punchRange, damage, punchKnockback * (isFinisher ? 2f : 1f), isFinisher);

        // Sound
        SoundManager.Instance?.PlayAttackSound();

        // Pewter drain for enhanced punches
        if (isPewterActive)
        {
            var metallurgist = GetComponentInParent<Metallurgist>();
            metallurgist?.DrainMetal(MetallurgySkill.MetalType.Pewter,
                MetallurgyConstants.PewterDrainRate * 0.5f);
        }
    }

    void Kick()
    {
        kickTimer = kickCooldown;

        bool isPewterActive = pewterScript != null && pewterScript.IsBurningPewter();
        float flareMult = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;

        float damage = kickDamage;
        if (isPewterActive) damage *= pewterDamageMultiplier * flareMult;

        // Animation
        if (animator != null)
            animator.SetTrigger("HeavyAttack"); // kick uses heavy animation

        // Hit detection — wider arc than punch
        PerformHit(kickRange, damage, kickKnockback, false);

        SoundManager.Instance?.PlayAttackSound();
        CameraShakeManager.Instance?.Shake(0.1f, 0.08f);

        // Pewter drain
        if (isPewterActive)
        {
            var metallurgist = GetComponentInParent<Metallurgist>();
            metallurgist?.DrainMetal(MetallurgySkill.MetalType.Pewter,
                MetallurgyConstants.PewterDrainRate * 0.8f);
        }
    }

    void PerformHit(float range, float damage, float knockback, bool isFinisher)
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector3 forward = transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin + forward * range * 0.5f, range * 0.6f, enemyLayer);

        bool hitSomething = false;
        foreach (var col in hits)
        {
            if (col.gameObject == this.gameObject) continue;
            if (col.transform.root == this.transform.root) continue;

            // Damage
            var damageable = col.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(damage);

            // Knockback
            var rb = col.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                dir.y = 0.2f;
                rb.AddForce(dir * knockback, ForceMode.Impulse);
            }

            // Stagger
            var enemyAI = col.GetComponentInParent<EnemyAI>();
            enemyAI?.animator?.SetTrigger("Hit");

            // Damage numbers
            DamageNumbersUI.Instance?.ShowDamage(
                col.ClosestPoint(origin), damage,
                isFinisher ? DamageNumbersUI.DamageType.Critical : DamageNumbersUI.DamageType.Normal);

            hitSomething = true;
        }

        if (hitSomething)
        {
            CameraShakeManager.Instance?.Shake(0.08f, 0.05f);
            SoundManager.Instance?.PlayHitSound(damage);
        }
    }

    /// <summary>Is unarmed combat currently active (no weapon equipped)?</summary>
    public bool IsActive()
    {
        return equipment == null || equipment.Equipped == null;
    }
}
