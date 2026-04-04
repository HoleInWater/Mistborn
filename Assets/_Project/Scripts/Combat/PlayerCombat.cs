using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Player combat: light attack (Mouse0), heavy attack / block / parry (Mouse1).
/// Right-click tap (< 0.15 s) = parry attempt.
/// Right-click hold (>= 0.15 s) = block until released, then heavy attack on release if
/// the hold was short enough (< 0.5 s), otherwise just block.
/// Pewter strength scaling from PHYSICS-MATH-BOOK.md Section 8.
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

    [Header("Right-Click Timing")]
    [Tooltip("Hold duration below this = parry tap. Above this = block.")]
    public float parryTapThreshold = 0.15f;
    [Tooltip("Hold duration below this (after blocking started) = heavy attack on release.")]
    public float heavyAttackHoldMax = 0.5f;

    [Header("Hit-Pause")]
    [Tooltip("Seconds to freeze time on a successful hit for combat feel.")]
    public float hitstopDuration = 0.05f;

    [Header("Pewter Enhancement — PHYSICS-MATH-BOOK.md Section 8")]
    [Tooltip("S = S_base × (1 + k × P), k = pewter efficiency")]
    public float pewterEfficiencyK = 2f;

    [Header("Allomancy Combat")]
    public float coinDamage = 25f;
    public float steelPushKnockback = 30f;

    [Header("References")]
    public ComboSystem comboSystem;
    public LockOnSystem lockOnSystem;
    public ParrySystem parrySystem;
    public PlayerAnimationController animCtrl;
    public EquipmentManager equipment;
    public Allomancer allomancer;
    public Animator animator;
    public LayerMask enemyLayer;

    private float lastAttackTime;
    private float lastHeavyAttackTime;
    private PlayerStamina stamina;

    // Right-click hold tracking
    private float mouse1DownTime = -1f;
    private bool mouse1Blocking = false;

    void Start()
    {
        if (allomancer == null)    allomancer   = GetComponent<Allomancer>();
        if (animator == null)      animator     = GetComponent<Animator>();
        if (parrySystem == null)   parrySystem  = GetComponentInChildren<ParrySystem>();
        if (parrySystem == null)   parrySystem  = GetComponent<ParrySystem>();
        if (animCtrl == null)      animCtrl     = GetComponent<PlayerAnimationController>();
        if (equipment == null)     equipment    = GetComponent<EquipmentManager>();
        stamina = GetComponent<PlayerStamina>();
        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) enemyLayer = ~0;

                  $"ComboSystem={comboSystem != null}, Animator={animator != null}");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            LightAttack();

        HandleRightClickCombat();
    }

    // ── Right-click: parry / block / heavy attack ─────────────────────────────

    void HandleRightClickCombat()
    {
        if (Input.GetMouseButtonDown(1))
        {
            mouse1DownTime = Time.time;
            mouse1Blocking = false;
        }

        if (Input.GetMouseButton(1) && mouse1DownTime >= 0f)
        {
            float held = Time.time - mouse1DownTime;

            if (!mouse1Blocking && held >= parryTapThreshold)
            {
                mouse1Blocking = true;
            }

            parrySystem?.SetBlocking(mouse1Blocking);
        }

        if (Input.GetMouseButtonUp(1) && mouse1DownTime >= 0f)
        {
            float held = Time.time - mouse1DownTime;

            if (held < parryTapThreshold)
            {
                parrySystem?.SetBlocking(false);
                parrySystem?.TryParry();
            }
            else
            {
                parrySystem?.SetBlocking(false);
                if (held < heavyAttackHoldMax)
                    HeavyAttack();
            }

            mouse1DownTime = -1f;
            mouse1Blocking = false;
        }
    }

    // ── Attacks ───────────────────────────────────────────────────────────────

    void LightAttack()
    {
        float cooldown = equipment != null ? equipment.GetAttackCooldown(attackCooldown) : attackCooldown;
        if (Time.time - lastAttackTime < cooldown)
        {
            return;
        }
        lastAttackTime = Time.time;

        OrientToLockOn();
        comboSystem?.RegisterHit();
        animCtrl?.PlayAttack();

        float eDamage    = equipment != null ? equipment.GetDamage(baseDamage)    : baseDamage;
        float eKnockback = equipment != null ? equipment.GetKnockback(knockbackForce) : knockbackForce;
        float damage = CalculateDamage(eDamage);
        bool hit = HitEnemiesInRange(damage, eKnockback);
        if (hit) StartCoroutine(Hitstop());

        SoundManager.Instance?.PlayAttackSound();
    }

    void HeavyAttack()
    {
        if (Time.time - lastHeavyAttackTime < heavyAttackCooldown)
        {
            return;
        }
        if (stamina != null && stamina.currentStamina < 25f)
        {
            return;
        }

        lastHeavyAttackTime = Time.time;
        lastAttackTime      = Time.time;

        stamina?.UseStamina(25f);
        OrientToLockOn();
        animCtrl?.PlayHeavyAttack();

        float eDamage        = equipment != null ? equipment.GetDamage(baseDamage)                        : baseDamage;
        float eHeavyMult     = equipment != null ? equipment.GetHeavyMultiplier(heavyDamageMultiplier)    : heavyDamageMultiplier;
        float eHeavyKnockback= equipment != null ? equipment.GetHeavyKnockback(heavyKnockbackForce)       : heavyKnockbackForce;
        float damage = CalculateDamage(eDamage * eHeavyMult);
        bool hit = HitEnemiesInRange(damage, eHeavyKnockback);
        if (hit) StartCoroutine(Hitstop());

        CameraShakeManager.Instance?.Shake(0.15f, 0.1f);
        SoundManager.Instance?.PlayAttackSound();
    }

    // ── Hitstop (brief time-freeze on hit contact for combat feel) ────────────

    IEnumerator Hitstop()
    {
        float prevScale = Time.timeScale;
        Time.timeScale  = 0f;
        yield return new WaitForSecondsRealtime(hitstopDuration);
        Time.timeScale  = prevScale;   // restore to whatever it was, not unconditionally 1
    }

    // ── Damage calculation ────────────────────────────────────────────────────

    float CalculateDamage(float base_damage)
    {
        float damage = base_damage;

        if (comboSystem != null)
            damage *= comboSystem.DamageMultiplier;

        if (allomancer != null && allomancer.IsMetalBurning(AllomancySkill.MetalType.Pewter))
        {
            float flare = FlareManager.Instance != null ? FlareManager.Instance.FlareMultiplier : 1f;
            float P = Mathf.Clamp01(flare / 2.5f);
            float pewterMult = AllomancyPhysicsFormulas.CalculatePewterStrength(1f, pewterEfficiencyK, P);
            damage *= pewterMult;
        }

        if (AllomanticSkillTree.Instance != null)
        {
            float combatBonus = AllomanticSkillTree.Instance.GetSkillValue("Combat_Damage1")
                              + AllomanticSkillTree.Instance.GetSkillValue("Combat_Damage2");
            damage *= (1f + combatBonus);
        }

        return damage;
    }

    // ── Hit detection ─────────────────────────────────────────────────────────

    bool HitEnemiesInRange(float damage, float knockback)
    {
        float range = equipment != null ? equipment.GetRange(attackRange) : attackRange;
        Vector3 attackPos = transform.position + transform.forward * 1.2f;

        // Cast against ALL layers — enemy layer filter is unreliable if enemies
        // aren't on the "Enemy" layer in the project settings.
        Collider[] hits = Physics.OverlapSphere(attackPos, range);
        bool hitAnything = false;


        // Track already-hit roots so multi-collider enemies only take damage once per swing
        HashSet<Transform> hitRoots = new HashSet<Transform>();

        foreach (Collider col in hits)
        {
            // Skip self
            if (col.transform.IsChildOf(transform) || col.transform == transform) continue;

            // Search up AND down the hierarchy — handles any prefab structure
            EnemyAI enemyAI = col.GetComponentInParent<EnemyAI>()
                           ?? col.GetComponentInChildren<EnemyAI>();

            IDamageable damageable = col.GetComponentInParent<IDamageable>()
                                  ?? col.GetComponentInChildren<IDamageable>();

            if (enemyAI == null && damageable == null)
            {
                continue;
            }

            // One hit per unique enemy root per swing
            Transform root = col.transform.root;
            if (hitRoots.Contains(root)) continue;
            hitRoots.Add(root);

            hitAnything = true;

            // Route through EnemyAI first (owns health + state machine)
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);
            }
            else
            {
                damageable.TakeDamage(damage);
            }

            // Knockback
            Rigidbody rb = col.attachedRigidbody ?? col.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockback, ForceMode.Impulse);
            }

            SoundManager.Instance?.PlayHitSound(damage);
            CameraShakeManager.Instance?.Shake(0.1f, 0.05f);
        }

        return hitAnything;
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
