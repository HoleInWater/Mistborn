using UnityEngine;

/// <summary>
/// Player combat system with light/heavy attacks, Allomancy integration,
/// and Pewter strength scaling from PHYSICS-MATH-BOOK.md Section 8.
/// </summary>
[PlayerComponent("Combat", order: 10)]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 2.5f;
    public float attackCooldown = 0.4f;
    public float heavyAttackCooldown = 1.2f;
    public float baseDamage = 15f;
    public float heavyDamageMultiplier = 2.5f;
    public float knockbackForce = 8f;
    public float heavyKnockbackForce = 20f;

    [Header("Pewter Enhancement — PHYSICS-MATH-BOOK.md Section 8")]
    [Tooltip("S = S_base × (1 + k × P), k = pewter efficiency")]
    public float pewterEfficiencyK = 2f;

    [Header("Allomancy Combat")]
    public float coinDamage = 25f;
    public float steelPushKnockback = 30f;

    [Header("References")]
    public ComboSystem comboSystem;
    public LockOnSystem lockOnSystem;
    public Allomancer allomancer;
    public Animator animator;
    public LayerMask enemyLayer;

    private float lastAttackTime;
    private float lastHeavyAttackTime;
    private PlayerStamina stamina;

    void Start()
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator == null) animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina>();
        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) enemyLayer = ~0; // Fallback to all layers
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            LightAttack();
        if (Input.GetMouseButtonDown(1))
            HeavyAttack();
    }

    void LightAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        OrientToLockOn();
        comboSystem?.RegisterHit();
        animator?.SetTrigger("Attack");

        float damage = CalculateDamage(baseDamage);
        HitEnemiesInRange(damage, knockbackForce);

        SoundManager.Instance?.PlayAttackSound();
    }

    void HeavyAttack()
    {
        if (Time.time - lastHeavyAttackTime < heavyAttackCooldown) return;
        if (stamina != null && stamina.currentStamina < 25f) return;

        lastHeavyAttackTime = Time.time;
        lastAttackTime = Time.time;

        stamina?.UseStamina(25f);
        OrientToLockOn();
        animator?.SetTrigger("HeavyAttack");

        float damage = CalculateDamage(baseDamage * heavyDamageMultiplier);
        HitEnemiesInRange(damage, heavyKnockbackForce);

        CameraShakeManager.Instance?.Shake(0.15f, 0.1f);
        SoundManager.Instance?.PlayAttackSound();
    }

    float CalculateDamage(float base_damage)
    {
        float damage = base_damage;

        // Combo multiplier
        if (comboSystem != null)
            damage *= comboSystem.DamageMultiplier;

        // Pewter strength: S = S_base × (1 + k × P) from handbook Section 8
        if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
        {
            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float P = Mathf.Clamp01(flare / 2.5f);
            float pewterMult = AllomancyPhysicsFormulas.CalculatePewterStrength(1f, pewterEfficiencyK, P);
            damage *= pewterMult;
        }

        // Skill tree bonus
        if (AllomanticSkillTree.Instance != null)
        {
            float combatBonus = AllomanticSkillTree.Instance.GetSkillValue("Combat_Damage1")
                              + AllomanticSkillTree.Instance.GetSkillValue("Combat_Damage2");
            damage *= (1f + combatBonus);
        }

        return damage;
    }

    void HitEnemiesInRange(float damage, float knockback)
    {
        Vector3 attackPos = transform.position + transform.forward * 1.2f;
        Collider[] hits = Physics.OverlapSphere(attackPos, attackRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                // Knockback
                Rigidbody rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(dir * knockback, ForceMode.Impulse);
                }

                // Hit effect
                SoundManager.Instance?.PlayHitSound(damage);
                CameraShakeManager.Instance?.Shake(0.1f, 0.05f);
            }
        }
    }

    void OrientToLockOn()
    {
        if (lockOnSystem != null && lockOnSystem.CurrentTarget != null)
        {
            Vector3 dir = (lockOnSystem.CurrentTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.forward = dir;
        }
    }

    public float GetBaseDamage() => baseDamage;
}
