using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Core enemy AI controller with type-specific stats and behavior.
/// Supports Guard, Coinshot, Lurcher, Thug, Smoker, Rioter, Seeker,
/// Koloss, SteelInquisitor, NobleGuard, Mistwraith, Obligator, SkaaRebel.
///
/// Requires AIController so enemies auto-register with MistbornRegistry —
/// letting Tin heartbeat / vibration and Bronze Seeker detection find them.
/// AIController defers its own state machine when EnemyAI is present.
/// </summary>
[RequireComponent(typeof(AIController))]
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
    // WorldScale: 2 Unity units = 5 feet
    public float health = 100f;
    public float moveSpeed = 1.4f;    // ~3.5 ft/s
    public float runSpeed = 2.4f;     // ~6 ft/s
    public float attackDamage = 25f;
    public float attackRange = 1.2f;  // ~3 ft melee (sword length + reach)
    public float detectionRange = 8f; // ~20 ft
    public float patrolRadius = 12f;  // ~30 ft

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

    [Header("Hit Effect")]
    [Tooltip("Optional particle system played at the player's position when a melee hit lands.")]
    public ParticleSystem hitEffect;

    public enum State { Idle, Patrol, Chase, Attack, Flee, Investigate, Dead }
    private State currentState = State.Idle;
    private float lastAttackTime;
    private Vector3 patrolCenter;
    private bool isFlanking;
    private Vector3 flankingPosition;
    private EnemySenses senses;
    private EnemyHealth enemyHealth;
    private EnemyHitFlash hitFlash;
    private AIController aiCtrl;

    [Header("Investigate")]
    public float investigateWaitTime = 3f;
    private Vector3 lastKnownPlayerPosition;
    private float investigateTimer;

    [Header("Flee")]
    [Tooltip("Health fraction below which cowardly enemies flee. Set 0 to disable.")]
    [Range(0f, 0.5f)] public float fleeHealthThreshold = 0.25f;
    private bool canFlee = true;   // set false per type in ApplyEnemyTypeDefaults
    private float startingHealth;  // recorded after ApplyEnemyTypeDefaults for GetMaxHealth()

    [Header("Group Alert")]
    [Tooltip("Radius in which this enemy wakes up nearby allies when it first spots the player.")]
    public float alertRadius = 15f;

    void Start()
    {
        patrolCenter = transform.position;
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        senses    = GetComponent<EnemySenses>();
        enemyHealth = GetComponent<EnemyHealth>();
        hitFlash  = GetComponent<EnemyHitFlash>();
        aiCtrl    = GetComponent<AIController>();

        ApplyEnemyTypeDefaults();
        startingHealth = health;

        // Sync EnemyHealth pool with our type-based health value
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth     = health;
            enemyHealth.currentHealth = health;
        }

        if (navAgent != null)
        {
            navAgent.speed            = moveSpeed;
            navAgent.stoppingDistance = Mathf.Max(0.1f, attackRange - 0.5f);
        }

        if (autoPatrol && enemyType != EnemyType.Koloss)
            currentState = State.Patrol;
    }

    // All distances in WorldScale: 2 Unity units = 5 feet
    void ApplyEnemyTypeDefaults()
    {
        switch (enemyType)
        {
            case EnemyType.Guard: // City guard with sword
                health = 80f; moveSpeed = 1.4f; runSpeed = 2.4f; attackDamage = 20f;
                detectionRange = 8f; attackRange = 1.2f; // 20ft detect, 3ft melee
                break;
            case EnemyType.NobleGuard: // Better equipped
                health = 120f; moveSpeed = 1.6f; runSpeed = 2.8f; attackDamage = 30f;
                detectionRange = 9.6f; attackRange = 1.2f; // 24ft detect
                break;
            case EnemyType.Coinshot: // Steel Misting — ranged
                health = 60f; moveSpeed = 2f; runSpeed = 3.2f; attackDamage = 15f;
                detectionRange = 16f; attackRange = 12f; // 40ft detect, 30ft push range
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Steel };
                break;
            case EnemyType.Lurcher: // Iron Misting — ranged pull
                health = 70f; moveSpeed = 1.6f; runSpeed = 2.4f; attackDamage = 20f;
                detectionRange = 12f; attackRange = 8f; // 30ft detect, 20ft pull
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Iron };
                break;
            case EnemyType.Thug: // Pewter Misting — tank
                health = 200f; moveSpeed = 2f; runSpeed = 3.6f; attackDamage = 45f;
                detectionRange = 6.4f; attackRange = 1.6f; // 16ft detect, 4ft melee
                canUseAllomancy = true; useFlanking = false; canFlee = false;
                availableMetals = new[] { AllomancySkill.MetalType.Pewter };
                break;
            case EnemyType.Smoker: // Copper Misting — hider
                health = 50f; moveSpeed = 1.2f; runSpeed = 2f; attackDamage = 10f;
                detectionRange = 4f; attackRange = 1f; // 10ft detect
                canUseAllomancy = true;
                availableMetals = new[] { AllomancySkill.MetalType.Copper };
                break;
            case EnemyType.Rioter: // Zinc Misting — emotional
                health = 55f; moveSpeed = 1.4f; runSpeed = 2.2f; attackDamage = 12f;
                detectionRange = 12f; attackRange = 8f; // 30ft detect, 20ft riot range
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Zinc };
                break;
            case EnemyType.Seeker: // Bronze Misting — detector
                health = 50f; moveSpeed = 1.2f; runSpeed = 2f; attackDamage = 10f;
                detectionRange = 80f; attackRange = 1f; // 200ft seek range!
                canUseAllomancy = true;
                availableMetals = new[] { AllomancySkill.MetalType.Bronze };
                break;
            case EnemyType.Koloss: // 12ft tall brute
                health = 500f; moveSpeed = 1f; runSpeed = 2.8f; attackDamage = 80f;
                detectionRange = 12f; attackRange = 2.4f; // 30ft detect, 6ft reach
                autoPatrol = false; useFlanking = false; canFlee = false;
                break;
            case EnemyType.SteelInquisitor: // All metals, Hemalurgic spikes
                health = 800f; moveSpeed = 2.8f; runSpeed = 4.8f; attackDamage = 60f;
                detectionRange = 20f; attackRange = 1.6f; attackCooldown = 0.8f; // 50ft detect
                canUseAllomancy = true; useFlanking = true; canFlee = false;
                availableMetals = new[] {
                    AllomancySkill.MetalType.Steel, AllomancySkill.MetalType.Iron,
                    AllomancySkill.MetalType.Pewter, AllomancySkill.MetalType.Tin,
                    AllomancySkill.MetalType.Atium
                };
                break;
            case EnemyType.Mistwraith: // Shapeless bone creature
                health = 150f; moveSpeed = 0.8f; runSpeed = 1.6f; attackDamage = 35f;
                detectionRange = 4f; attackRange = 1.6f; useFlanking = false; canFlee = false; // 10ft detect
                break;
            case EnemyType.Obligator: // Bureaucrat, doesn't fight
                health = 40f; moveSpeed = 1f; runSpeed = 1.6f; attackDamage = 5f;
                detectionRange = 12f; attackRange = 1f; useMeleeAttacks = false; // 30ft detect
                break;
            case EnemyType.SkaaRebel: // Rebellion fighter, ally-ish
                health = 60f; moveSpeed = 1.6f; runSpeed = 2.6f; attackDamage = 15f;
                detectionRange = 6.4f; attackRange = 1f; useFlanking = true; // 16ft detect
                break;
        }
    }

    void Update()
    {
        if (health <= 0) { if (currentState != State.Dead) Die(); return; }

        ApplyEmotionModifiers();
        DetectTarget();

        // Flee overrides everything except Dead
        if (canFlee && fleeHealthThreshold > 0f
            && health / GetMaxHealth() < fleeHealthThreshold
            && currentState != State.Flee && currentState != State.Dead)
        {
            currentState = State.Flee;
        }

        switch (currentState)
        {
            case State.Idle:        if (autoPatrol) currentState = State.Patrol; break;
            case State.Patrol:      HandlePatrol(); break;
            case State.Chase:       HandleChase(); break;
            case State.Attack:      HandleAttack(); break;
            case State.Investigate: HandleInvestigate(); break;
            case State.Flee:        HandleFlee(); break;
        }

        UpdateAnimations();
    }

    float GetMaxHealth()
    {
        if (enemyHealth != null && enemyHealth.maxHealth > 0f) return enemyHealth.maxHealth;
        return Mathf.Max(startingHealth, 1f);
    }

    void DetectTarget()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (target == null) return;

        bool detected;
        if (senses != null)
        {
            // Physics-based: sight cone + hearing ring + suspicion meter
            detected = senses.CanDetectPlayer;
        }
        else
        {
            // Fallback: raw sphere range
            float dist = Vector3.Distance(transform.position, target.position);
            detected = dist <= detectionRange;
        }

        if (detected)
        {
            lastKnownPlayerPosition = target.position;
            if (currentState == State.Patrol || currentState == State.Idle
                || currentState == State.Investigate)
            {
                currentState = State.Chase;
                AlertNearbyEnemies(); // wake up nearby allies the moment we first spot the player
            }
            else if (currentState != State.Attack)
            {
                currentState = State.Chase;
            }
        }
        else if (!detected && (currentState == State.Chase || currentState == State.Attack))
        {
            // Lost sight/sound — investigate the last known position before giving up
            currentState = State.Investigate;
            investigateTimer = investigateWaitTime;
            hasAlertedGroup = false; // reset so re-detection can alert again
            if (navAgent != null) navAgent.SetDestination(lastKnownPlayerPosition);
        }
    }

    bool IsNavReady() => navAgent != null && navAgent.enabled && navAgent.isOnNavMesh;

    void HandlePatrol()
    {
        if (!IsNavReady()) return;
        navAgent.isStopped = false;
        navAgent.speed = moveSpeed;

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
        if (target == null || !IsNavReady()) return;
        navAgent.isStopped = false;
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
        else if (isFlanking)
        {
            // Abort flank if player moved more than 4 units from where we calculated it
            if (Vector3.Distance(target.position, flankingPosition) > 4f)
                isFlanking = false;
            else if (Vector3.Distance(transform.position, flankingPosition) < 2f)
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

        // Stop moving while striking — prevents sliding through the player
        if (navAgent != null) navAgent.isStopped = true;
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            PerformAttack();
        }
    }

    void HandleInvestigate()
    {
        if (navAgent == null) return;
        navAgent.speed = moveSpeed; // walk, not run

        bool reachedDestination = !navAgent.pathPending
                                  && navAgent.remainingDistance <= navAgent.stoppingDistance + 0.5f;
        if (!reachedDestination) return;

        // Arrived at last known position — wait and look around
        investigateTimer -= Time.deltaTime;

        // Slowly rotate while waiting (looking around)
        transform.Rotate(0f, 45f * Time.deltaTime, 0f);

        if (investigateTimer <= 0f)
        {
            currentState = State.Patrol;
            if (navAgent != null) navAgent.speed = moveSpeed;
        }
    }

    // ── Flee ──────────────────────────────────────────────────────────────────

    void HandleFlee()
    {
        if (target == null || !IsNavReady()) return;
        navAgent.isStopped = false;
        navAgent.speed = runSpeed;

        // Only re-issue a flee destination when close to the current one (avoid spamming pathfinder)
        if (!navAgent.pathPending && navAgent.remainingDistance < 2f)
        {
            Vector3 fleeDir    = (transform.position - target.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDir * 12f;

            // Try increasingly larger sample radii to find a valid NavMesh point
            float[] radii = { 4f, 8f, 12f };
            foreach (float r in radii)
            {
                if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, r, NavMesh.AllAreas))
                {
                    navAgent.SetDestination(hit.position);
                    break;
                }
            }
        }

        // Once escaped, stop fleeing and investigate the area
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > detectionRange * 2f)
        {
            currentState     = State.Investigate;
            investigateTimer = investigateWaitTime * 0.5f;
            if (navAgent.isActiveAndEnabled)
                navAgent.SetDestination(lastKnownPlayerPosition);
        }
    }

    // ── Emotion modifiers (Zinc / Brass / Atium Allomancy affects behaviour) ──

    void ApplyEmotionModifiers()
    {
        if (aiCtrl == null) return;

        // Also check if AIController was alerted by EnemySeeker bronze detection
        if (aiCtrl.currentEmotion == AIController.EmotionState.Aggressive
            || aiCtrl.currentEmotion == AIController.EmotionState.Enraged)
        {
            if (currentState == State.Patrol || currentState == State.Idle)
                currentState = State.Chase;
        }
        else if (aiCtrl.currentEmotion == AIController.EmotionState.Fearful && canFlee)
        {
            if (currentState != State.Dead)
                currentState = State.Flee;
        }
    }

    // ── Group alert ───────────────────────────────────────────────────────────

    bool hasAlertedGroup = false;

    void AlertNearbyEnemies()
    {
        if (hasAlertedGroup) return;
        hasAlertedGroup = true;

        foreach (var ally in MistbornRegistry.ActiveEnemies)
        {
            if (ally == null || ally.gameObject == gameObject) continue;
            float dist = Vector3.Distance(transform.position, ally.transform.position);
            if (dist > alertRadius) continue;

            // Alert via EnemyAI if present
            EnemyAI allyAI = ally.GetComponent<EnemyAI>();
            if (allyAI != null)
                allyAI.ForceChase(target, lastKnownPlayerPosition);
            else
                ally.SetEmotionState(AIController.EmotionState.Aggressive);
        }
    }

    /// <summary>
    /// Called by a nearby ally to immediately start chasing.
    /// </summary>
    public void ForceChase(Transform chaseTarget, Vector3 knownPosition)
    {
        if (currentState == State.Dead || currentState == State.Chase
            || currentState == State.Attack) return;

        target = chaseTarget;
        lastKnownPlayerPosition = knownPosition;
        currentState = State.Chase;
    }

    // ── Attack ────────────────────────────────────────────────────────────────

    void PerformAttack()
    {
        if (useMeleeAttacks)
        {
            animator?.SetTrigger("Attack");

            // Line-of-sight check — don't deal damage through walls
            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 toTarget = (target.position + Vector3.up * 1f) - origin;
            bool clearShot = !Physics.Raycast(origin, toTarget.normalized, toTarget.magnitude,
                             Physics.DefaultRaycastLayers & ~(1 << gameObject.layer),
                             QueryTriggerInteraction.Ignore);

            if (clearShot)
            {
                // Route through IDamageable (PlayerHealth) for death/respawn logic
                IDamageable damageable = target.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(attackDamage);

                // Also drive the visible health bar — HealthBarTransitions doesn't implement IDamageable
                HealthBarTransitions hbt = target.GetComponentInParent<HealthBarTransitions>();
                hbt?.TakeDamage(attackDamage);

                // Hit particle at target position
                if (hitEffect != null)
                {
                    hitEffect.transform.position = target.position + Vector3.up * 1f;
                    hitEffect.Play();
                }
            }
        }

        if (canUseAllomancy && availableMetals != null && availableMetals.Length > 0)
            UseAllomanticAttack();
    }

    void UseAllomanticAttack()
    {
        if (target == null) return;
        if (availableMetals == null || availableMetals.Length == 0) return;
        AllomancySkill.MetalType metal = availableMetals[Random.Range(0, availableMetals.Length)];
        Vector3 dir = (target.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, target.position);

        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:
                Rigidbody targetRb = target.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    Pewter targetPewter = target.GetComponentInParent<Pewter>();
                    float steelResist   = targetPewter != null ? targetPewter.GetKnockbackResistance() : 1f;
                    targetRb.AddForce(dir * 80f / steelResist, ForceMode.Impulse);
                }
                break;
            case AllomancySkill.MetalType.Iron:
                Rigidbody playerRb = target.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Pewter playerPewter = target.GetComponentInParent<Pewter>();
                    float ironResist    = playerPewter != null ? playerPewter.GetKnockbackResistance() : 1f;
                    playerRb.AddForce(-dir * 50f / ironResist, ForceMode.Impulse);
                }
                break;
            case AllomancySkill.MetalType.Zinc:
                if (dist < 15f)
                {
                    BasicPlayerMove pm = target.GetComponent<BasicPlayerMove>();
                    if (pm != null) StartCoroutine(ZincSlowCoroutine(pm, 0.8f, 3f));
                }
                break;
        }
    }

    IEnumerator ZincSlowCoroutine(BasicPlayerMove pm, float slowMultiplier, float duration)
    {
        pm.externalSpeedMultiplier = slowMultiplier;
        yield return new WaitForSeconds(duration);
        // Only restore if this enemy's slow is still the active one
        if (Mathf.Approximately(pm.externalSpeedMultiplier, slowMultiplier))
            pm.externalSpeedMultiplier = 1f;
    }

    public void TakeDamage(float damage, string source = "Unknown")
    {
        if (health <= 0) return;
        health -= damage;
        if (enemyHealth != null) enemyHealth.currentHealth = health; // keep pools in sync
        hitFlash?.Flash();
        if (health <= 0) Die();
        else if (currentState != State.Chase && currentState != State.Attack
                 && currentState != State.Investigate)
            currentState = State.Chase;
    }

    void Die()
    {
        currentState = State.Dead;
        health = 0;
        if (enemyHealth != null) { enemyHealth.currentHealth = 0; enemyHealth.isDead = true; }
        animator?.SetBool("IsDead", true);
        if (navAgent != null) { navAgent.isStopped = true; navAgent.enabled = false; }

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

        // Float parameters — "Speed" (raw m/s) and "Velocity" (0-1 normalised, matches reference animator)
        animator.SetFloat("Speed",    speed, 0.1f, Time.deltaTime);
        animator.SetFloat("Velocity", Mathf.Clamp01(speed / Mathf.Max(runSpeed, 0.1f)), 0.1f, Time.deltaTime);

        // State booleans
        animator.SetBool("IsIdle",          currentState == State.Idle);
        animator.SetBool("IsPatrolling",    currentState == State.Patrol);
        animator.SetBool("IsChasing",       currentState == State.Chase);
        animator.SetBool("IsAttacking",     currentState == State.Attack);
        animator.SetBool("IsFleeing",       currentState == State.Flee);
        animator.SetBool("IsInvestigating", currentState == State.Investigate);
        animator.SetBool("IsDead",          currentState == State.Dead);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (navAgent != null) navAgent.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(direction * force, ForceMode.Impulse);
        StartCoroutine(ReEnableNav(rb));
    }

    IEnumerator ReEnableNav(Rigidbody rb)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.isKinematic = true; }
        if (navAgent != null) { navAgent.enabled = true; navAgent.Warp(transform.position); }
    }

    public float GetHealth() => health;
    public State GetState() => currentState;
}
