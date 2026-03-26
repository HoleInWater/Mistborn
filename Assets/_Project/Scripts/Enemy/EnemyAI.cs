using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Core enemy AI controller with type-specific stats and behavior.
/// Supports Guard, Coinshot, Lurcher, Thug, Smoker, Rioter, Seeker,
/// Koloss, SteelInquisitor, NobleGuard, Mistwraith, Obligator, SkaaRebel.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Guard;
    public enum EnemyType
    {
        Guard, Coinshot, Seeker, Koloss, SteelInquisitor, NobleGuard,
        Mistwraith, Thug, Smoker, Rioter, Obligator, SkaaRebel, Lurcher
    }

    [Header("Stats")]
    public float health = 100f;
    public float moveSpeed = 3.5f;
    public float runSpeed = 6f;
    public float attackDamage = 25f;
    public float attackRange = 2.5f;
    public float detectionRange = 15f;
    public float patrolRadius = 20f;

    [Header("Combat")]
    public bool canUseAllomancy = false;
    public AllomancySkill.MetalType[] availableMetals;
    public float attackCooldown = 2f;
    public bool useMeleeAttacks = true;
    public bool useRangedAttacks = false;
    public bool useFlanking = true;
    public bool autoPatrol = true;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent navAgent;
    public Transform target;

    public enum State { Idle, Patrol, Chase, Attack, Flee, Investigate, Dead }
    private State currentState = State.Idle;
    private float lastAttackTime;
    private Vector3 patrolCenter;
    private bool isFlanking;
    private Vector3 flankingPosition;

    void Start()
    {
        patrolCenter = transform.position;
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        ApplyEnemyTypeDefaults();

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = attackRange - 1f;
        }

        if (autoPatrol && enemyType != EnemyType.Koloss)
            currentState = State.Patrol;
    }

    void ApplyEnemyTypeDefaults()
    {
        switch (enemyType)
        {
            case EnemyType.Guard:
                health = 80f; moveSpeed = 3.5f; runSpeed = 6f; attackDamage = 20f;
                detectionRange = 15f; attackRange = 2.5f;
                break;
            case EnemyType.NobleGuard:
                health = 120f; moveSpeed = 4f; runSpeed = 7f; attackDamage = 30f;
                detectionRange = 18f; attackRange = 2.5f;
                break;
            case EnemyType.Coinshot:
                health = 60f; moveSpeed = 5f; runSpeed = 8f; attackDamage = 15f;
                detectionRange = 25f; attackRange = 20f;
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Steel };
                break;
            case EnemyType.Lurcher:
                health = 70f; moveSpeed = 4f; runSpeed = 6f; attackDamage = 20f;
                detectionRange = 20f; attackRange = 15f;
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Iron };
                break;
            case EnemyType.Thug:
                health = 200f; moveSpeed = 5f; runSpeed = 9f; attackDamage = 45f;
                detectionRange = 12f; attackRange = 3f;
                canUseAllomancy = true; useFlanking = false;
                availableMetals = new[] { AllomancySkill.MetalType.Pewter };
                break;
            case EnemyType.Smoker:
                health = 50f; moveSpeed = 3f; runSpeed = 5f; attackDamage = 10f;
                detectionRange = 10f; attackRange = 2f;
                canUseAllomancy = true;
                availableMetals = new[] { AllomancySkill.MetalType.Copper };
                break;
            case EnemyType.Rioter:
                health = 55f; moveSpeed = 3.5f; runSpeed = 5.5f; attackDamage = 12f;
                detectionRange = 20f; attackRange = 15f;
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Zinc };
                break;
            case EnemyType.Seeker:
                health = 50f; moveSpeed = 3f; runSpeed = 5f; attackDamage = 10f;
                detectionRange = 30f; attackRange = 2f;
                canUseAllomancy = true;
                availableMetals = new[] { AllomancySkill.MetalType.Bronze };
                break;
            case EnemyType.Koloss:
                health = 500f; moveSpeed = 2.5f; runSpeed = 7f; attackDamage = 80f;
                detectionRange = 20f; attackRange = 4f;
                autoPatrol = false; useFlanking = false;
                break;
            case EnemyType.SteelInquisitor:
                health = 800f; moveSpeed = 7f; runSpeed = 12f; attackDamage = 60f;
                detectionRange = 35f; attackRange = 3f; attackCooldown = 0.8f;
                canUseAllomancy = true; useFlanking = true;
                availableMetals = new[] {
                    AllomancySkill.MetalType.Steel, AllomancySkill.MetalType.Iron,
                    AllomancySkill.MetalType.Pewter, AllomancySkill.MetalType.Tin,
                    AllomancySkill.MetalType.Atium
                };
                break;
            case EnemyType.Mistwraith:
                health = 150f; moveSpeed = 2f; runSpeed = 4f; attackDamage = 35f;
                detectionRange = 10f; attackRange = 3f; useFlanking = false;
                break;
            case EnemyType.Obligator:
                health = 40f; moveSpeed = 2.5f; runSpeed = 4f; attackDamage = 5f;
                detectionRange = 20f; attackRange = 2f; useMeleeAttacks = false;
                break;
            case EnemyType.SkaaRebel:
                health = 60f; moveSpeed = 4f; runSpeed = 6.5f; attackDamage = 15f;
                detectionRange = 12f; attackRange = 2f; useFlanking = true;
                break;
        }
    }

    void Update()
    {
        if (health <= 0) { if (currentState != State.Dead) Die(); return; }

        DetectTarget();

        switch (currentState)
        {
            case State.Idle: if (autoPatrol) currentState = State.Patrol; break;
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
        }

        UpdateAnimations();
    }

    void DetectTarget()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= detectionRange && currentState != State.Attack)
            currentState = State.Chase;
        else if (dist > detectionRange * 1.5f && currentState == State.Chase)
            currentState = State.Patrol;
    }

    void HandlePatrol()
    {
        if (navAgent == null || !navAgent.enabled) return;
        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            Vector3 randomDir = Random.insideUnitSphere * patrolRadius + patrolCenter;
            randomDir.y = transform.position.y;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
                navAgent.SetDestination(hit.position);
        }
    }

    void HandleChase()
    {
        if (target == null || navAgent == null) return;
        navAgent.speed = runSpeed;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= attackRange)
        {
            currentState = State.Attack;
            return;
        }

        if (useFlanking && !isFlanking)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.up) * (Random.value > 0.5f ? 5f : -5f);
            flankingPosition = target.position + perp;
            isFlanking = true;
            navAgent.SetDestination(flankingPosition);
        }
        else if (isFlanking && Vector3.Distance(transform.position, flankingPosition) < 2f)
        {
            isFlanking = false;
        }

        if (!isFlanking)
            navAgent.SetDestination(target.position);
    }

    void HandleAttack()
    {
        if (target == null) { currentState = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > attackRange * 1.5f) { currentState = State.Chase; return; }

        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        if (useMeleeAttacks)
        {
            animator?.SetTrigger("Attack");
            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
        }

        if (canUseAllomancy && availableMetals != null && availableMetals.Length > 0)
            UseAllomanticAttack();
    }

    void UseAllomanticAttack()
    {
        if (target == null) return;
        AllomancySkill.MetalType metal = availableMetals[Random.Range(0, availableMetals.Length)];
        Vector3 dir = (target.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, target.position);

        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:
                Rigidbody targetRb = target.GetComponent<Rigidbody>();
                if (targetRb != null) targetRb.AddForce(dir * 80f, ForceMode.Impulse);
                break;
            case AllomancySkill.MetalType.Iron:
                Rigidbody playerRb = target.GetComponent<Rigidbody>();
                if (playerRb != null) playerRb.AddForce(-dir * 50f, ForceMode.Impulse);
                break;
            case AllomancySkill.MetalType.Zinc:
                if (dist < 15f)
                {
                    BasicPlayerMove pm = target.GetComponent<BasicPlayerMove>();
                    if (pm != null) pm.externalSpeedMultiplier = 0.8f;
                }
                break;
        }
    }

    public void TakeDamage(float damage, string source = "Unknown")
    {
        if (health <= 0) return;
        health -= damage;
        if (health <= 0) Die();
        else if (currentState != State.Chase && currentState != State.Attack)
            currentState = State.Chase;
    }

    void Die()
    {
        currentState = State.Dead;
        animator?.SetBool("IsDead", true);
        if (navAgent != null) navAgent.isStopped = true;

        // Grant XP
        PlayerExperience xp = PlayerExperience.Instance;
        if (xp != null) xp.AddXP(GetXPValue());

        // Drop loot
        SpawnLoot();

        // Track achievements
        EventManager.TriggerEvent("EnemyKilled");
        if (enemyType == EnemyType.Koloss)
            AchievementSystem.Instance?.TryUnlock("kill_koloss_10");

        // Particle effect
        ParticleEffectsManager.Instance?.PlayDeathEffect(transform.position);

        Destroy(gameObject, 3f);
    }

    float GetXPValue()
    {
        switch (enemyType)
        {
            case EnemyType.Guard: return 25f;
            case EnemyType.NobleGuard: return 40f;
            case EnemyType.Coinshot: return 50f;
            case EnemyType.Lurcher: return 45f;
            case EnemyType.Thug: return 60f;
            case EnemyType.Koloss: return 100f;
            case EnemyType.SteelInquisitor: return 500f;
            case EnemyType.Mistwraith: return 35f;
            default: return 20f;
        }
    }

    void SpawnLoot()
    {
        // 40% chance to drop loot
        if (Random.value > 0.4f) return;

        GameObject lootObj = new GameObject("LootDrop");
        lootObj.transform.position = transform.position + Vector3.up * 0.5f;
        LootDrop loot = lootObj.AddComponent<LootDrop>();
        loot.lootType = Random.value > 0.5f ? LootDrop.LootType.Coin : LootDrop.LootType.MetalVial;
        loot.minAmount = 1;
        loot.maxAmount = 3;

        // Add trigger collider for pickup
        SphereCollider col = lootObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.5f;

        // Simple visual (cube placeholder until prefab assigned)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(lootObj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.3f;
        Destroy(visual.GetComponent<Collider>()); // Remove extra collider
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (navAgent != null) navAgent.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.AddForce(direction * force, ForceMode.Impulse);
        StartCoroutine(ReEnableNav());
    }

    IEnumerator ReEnableNav()
    {
        yield return new WaitForSeconds(0.5f);
        if (navAgent != null) navAgent.enabled = true;
    }

    public float GetHealth() => health;
    public State GetState() => currentState;
}
