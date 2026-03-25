using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Guard;
    public enum EnemyType { Guard, Coinshot, Seeker, Koloss, SteelInquisitor, NobleGuard, Mistwraith, Thug, Smoker, Rioter, Obligator, SkaaRebel, Lurcher }

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
    public GameObject weaponPrefab;

    [Header("AI Settings")]
    public bool autoPatrol = true;
    public bool useMeleeAttacks = true;
    public bool useRangedAttacks = false;
    public bool useFlanking = true;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent navAgent;
    public Transform target;
    public AudioSource audioSource;

    public enum State { Idle, Patrol, Chase, Attack, Flee, Investigate, Dead }
    private State currentState = State.Idle;
    private State previousState = State.Idle;

    private float lastAttackTime;
    private Vector3 patrolCenter;
    private Vector3? investigatePoint;
    private float investigateTimer;
    private bool isFlanking;
    private Vector3 flankingPosition;
    private Dictionary<string, float> damageSources = new Dictionary<string, float>();

    void Start()
    {
        patrolCenter = transform.position;

        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        // Configure stats based on enemy type
        ApplyEnemyTypeDefaults();

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = attackRange - 1f;
        }

        if (autoPatrol && enemyType != EnemyType.Koloss)
        {
            StartPatrol();
        }
    }

    void ApplyEnemyTypeDefaults()
    {
        switch (enemyType)
        {
            case EnemyType.Guard:
                health = 80f; moveSpeed = 3.5f; runSpeed = 6f; attackDamage = 20f;
                detectionRange = 15f; attackRange = 2.5f; useFlanking = true;
                break;
            case EnemyType.NobleGuard:
                health = 120f; moveSpeed = 4f; runSpeed = 7f; attackDamage = 30f;
                detectionRange = 18f; attackRange = 2.5f; useFlanking = true;
                break;
            case EnemyType.Coinshot: // Steel Misting
                health = 60f; moveSpeed = 5f; runSpeed = 8f; attackDamage = 15f;
                detectionRange = 25f; attackRange = 20f;
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Steel };
                break;
            case EnemyType.Lurcher: // Iron Misting
                health = 70f; moveSpeed = 4f; runSpeed = 6f; attackDamage = 20f;
                detectionRange = 20f; attackRange = 15f;
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Iron };
                break;
            case EnemyType.Thug: // Pewter Misting
                health = 200f; moveSpeed = 5f; runSpeed = 9f; attackDamage = 45f;
                detectionRange = 12f; attackRange = 3f;
                canUseAllomancy = true; useFlanking = false;
                availableMetals = new[] { AllomancySkill.MetalType.Pewter };
                break;
            case EnemyType.Smoker: // Copper Misting
                health = 50f; moveSpeed = 3f; runSpeed = 5f; attackDamage = 10f;
                detectionRange = 10f; attackRange = 2f;
                canUseAllomancy = true;
                availableMetals = new[] { AllomancySkill.MetalType.Copper };
                break;
            case EnemyType.Rioter: // Zinc Misting
                health = 55f; moveSpeed = 3.5f; runSpeed = 5.5f; attackDamage = 12f;
                detectionRange = 20f; attackRange = 15f;
                canUseAllomancy = true; useRangedAttacks = true;
                availableMetals = new[] { AllomancySkill.MetalType.Zinc };
                break;
            case EnemyType.Seeker: // Bronze Misting
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
                detectionRange = 10f; attackRange = 3f;
                useFlanking = false;
                break;
            case EnemyType.Obligator:
                health = 40f; moveSpeed = 2.5f; runSpeed = 4f; attackDamage = 5f;
                detectionRange = 20f; attackRange = 2f;
                useMeleeAttacks = false;
                break;
            case EnemyType.SkaaRebel:
                health = 60f; moveSpeed = 4f; runSpeed = 6.5f; attackDamage = 15f;
                detectionRange = 12f; attackRange = 2f;
                useFlanking = true;
                break;
        }
    }

    void Update()
    {
        if (health <= 0)
        {
            Die();
            return;
        }

        UpdateAI();
        UpdateAnimations();
    }

    void UpdateAI()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Attack:
                HandleAttack();
                break;
            case State.Flee:
                HandleFlee();
                break;
            case State.Investigate:
                HandleInvestigate();
                break;
        }

        DetectTarget();
    }

    void DetectTarget()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance <= detectionRange)
            {
                if (currentState != State.Chase && currentState != State.Attack)
                {
                    SetState(State.Chase);
                }
            }
            else if (currentState == State.Chase)
            {
                if (distance > detectionRange * 1.5f)
                {
                    SetState(State.Patrol);
                }
            }
        }
    }

    void HandleIdle()
    {
        if (autoPatrol)
        {
            SetState(State.Patrol);
        }
    }

    void HandlePatrol()
    {
        if (navAgent == null || !navAgent.enabled) return;

        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = GetRandomPatrolPoint();
            navAgent.SetDestination(randomPoint);
        }
    }

    void HandleChase()
    {
        if (target == null || navAgent == null) return;

        if (canUseAllomancy && useRangedAttacks)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange * 2)
            {
                navAgent.SetDestination(target.position);
            }
            else
            {
                SetState(State.Attack);
            }
        }
        else
        {
            Vector3 destination = target.position;
            
            if (useFlanking && !isFlanking)
            {
                flankingPosition = GetFlankingPosition(target.position);
                isFlanking = true;
                navAgent.SetDestination(flankingPosition);
            }
            else if (isFlanking && Vector3.Distance(transform.position, flankingPosition) < 2f)
            {
                destination = target.position;
                isFlanking = false;
            }

            if (Vector3.Distance(transform.position, destination) <= attackRange)
            {
                SetState(State.Attack);
            }
            else
            {
                navAgent.SetDestination(destination);
                navAgent.speed = runSpeed;
            }
        }
    }

    void HandleAttack()
    {
        if (target == null)
        {
            SetState(State.Patrol);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance > attackRange * 1.5f)
        {
            SetState(State.Chase);
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        transform.LookAt(target);
    }

    void HandleFlee()
    {
        if (navAgent == null) return;

        if (Vector3.Distance(transform.position, patrolCenter) > patrolRadius * 2)
        {
            SetState(State.Patrol);
        }
    }

    void HandleInvestigate()
    {
        investigateTimer -= Time.deltaTime;

        if (investigateTimer <= 0 || (investigatePoint.HasValue && Vector3.Distance(transform.position, investigatePoint.Value) < 2f))
        {
            SetState(State.Patrol);
            investigatePoint = null;
        }
    }

    void PerformAttack()
    {
        if (useMeleeAttacks)
        {
            animator?.SetTrigger("Attack");
            
            if (target != null)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                damageable?.TakeDamage(attackDamage);
            }
        }

        if (canUseAllomancy && availableMetals != null && availableMetals.Length > 0)
        {
            UseAllomanticAttack();
        }
    }

    void UseAllomanticAttack()
    {
        if (target == null) return;
        AllomancySkill.MetalType metal = availableMetals[Random.Range(0, availableMetals.Length)];

        float distance = Vector3.Distance(transform.position, target.position);
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        switch (metal)
        {
            case AllomancySkill.MetalType.Steel:
                // Coinshot: Push coins/metal at player
                Rigidbody targetRb = target.GetComponent<Rigidbody>();
                if (targetRb != null)
                    targetRb.AddForce(dirToTarget * 80f, ForceMode.Impulse);
                break;

            case AllomancySkill.MetalType.Iron:
                // Lurcher: Pull player's metal equipment toward self
                Rigidbody playerRb = target.GetComponent<Rigidbody>();
                if (playerRb != null)
                    playerRb.AddForce(-dirToTarget * 50f, ForceMode.Impulse);
                break;

            case AllomancySkill.MetalType.Pewter:
                // Thug: Enhanced melee — already factored into attackDamage
                break;

            case AllomancySkill.MetalType.Zinc:
                // Rioter: Inflame player's emotions (apply debuff)
                if (distance < 15f)
                {
                    BasicPlayerMove pm = target.GetComponent<BasicPlayerMove>();
                    if (pm != null) pm.externalSpeedMultiplier = 0.8f;
                }
                break;

            case AllomancySkill.MetalType.Copper:
                // Smoker: Hide Allomantic pulses (passive, no attack)
                break;

            case AllomancySkill.MetalType.Bronze:
                // Seeker: Detect player Allomancy (passive detection)
                break;

            case AllomancySkill.MetalType.Atium:
                // Inquisitor Atium: Dodge next attack (set invincibility window)
                break;
        }
    }

    Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += patrolCenter;
        randomDir.y = transform.position.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, patrolRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return patrolCenter;
    }

    Vector3 GetFlankingPosition(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        
        float side = Random.value > 0.5f ? 1f : -1f;
        return targetPos + (perpendicular * side * 5f);
    }

    public void TakeDamage(float damage, string source = "Unknown")
    {
        if (health <= 0) return;

        health -= damage;
        
        if (!damageSources.ContainsKey(source)) damageSources[source] = 0;
        damageSources[source] += damage;

        if (health <= 0)
        {
            Die();
        }
        else if (currentState != State.Chase && currentState != State.Attack)
        {
            investigatePoint = transform.position;
            investigateTimer = 3f;
            SetState(State.Investigate);
        }
    }

    void Die()
    {
        currentState = State.Dead;
        
        animator?.SetBool("IsDead", true);
        if (navAgent != null) navAgent.isStopped = true;
        
        if (enemyType == EnemyType.Koloss || enemyType == EnemyType.SteelInquisitor)
        {
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();
            }
        }

        StartCoroutine(DeathCleanup());

        EventManager.TriggerEvent("EnemyKilled", new Dictionary<string, object> { { "enemy", this }, { "type", enemyType } });
    }

    IEnumerator DeathCleanup()
    {
        yield return new WaitForSeconds(3f);
        
        Destroy(gameObject);
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;
        
        previousState = currentState;
        currentState = newState;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        
        animator.SetBool("IsChasing", currentState == State.Chase);
        animator.SetBool("IsAttacking", currentState == State.Attack);
        animator.SetBool("IsIdle", currentState == State.Idle);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (navAgent != null) navAgent.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        rb.AddForce(direction * force, ForceMode.Impulse);
        
        StartCoroutine(EnableNavMeshAgent());
    }

    IEnumerator EnableNavMeshAgent()
    {
        yield return new WaitForSeconds(0.5f);
        if (navAgent != null) navAgent.enabled = true;
    }

    public float GetHealth() => health;
    public State GetState() => currentState;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnInterval = 10f;
    public float spawnRadius = 10f;
    public bool spawnOnStart = true;

    [Header("Wave Settings")]
    public bool useWaves = false;
    public int enemiesPerWave = 3;
    public float waveInterval = 30f;
    public int currentWave = 0;

    [Header("References")]
    public Transform spawnPoint;

    private List<EnemyAI> activeEnemies = new List<EnemyAI>();
    private float spawnTimer;
    private float waveTimer;

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;

        if (spawnOnStart)
        {
            for (int i = 0; i < maxEnemies; i++)
            {
                SpawnEnemy();
            }
        }
    }

    void Update()
    {
        CleanUpDeadEnemies();

        if (!useWaves)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    SpawnEnemy();
                    spawnTimer = 0;
                }
            }
        }
        else
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                StartWave();
                waveTimer = 0;
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            activeEnemies.Add(enemyAI);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 offset = new Vector3(circle.x, 0, circle.y);
        return spawnPoint.position + offset;
    }

    void CleanUpDeadEnemies()
    {
        activeEnemies.RemoveAll(e => e == null || e.GetState() == EnemyAI.State.Dead);
    }

    void StartWave()
    {
        currentWave++;
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
        }
        
        Debug.Log($"[SPAWNER] Wave {currentWave} started with {enemiesPerWave} enemies");
    }

    public int GetActiveEnemyCount() => activeEnemies.Count;
}

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float waitTime = 3f;
    public bool loopPatrol = true;

    [Header("References")]
    public EnemyAI enemyAI;
    public NavMeshAgent navAgent;

    private int currentPoint;
    private float waitTimer;

    void Start()
    {
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();

        if (patrolPoints.Length > 0)
        {
            navAgent?.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0 || navAgent == null) return;

        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= waitTime)
            {
                NextPoint();
                waitTimer = 0;
            }
        }
    }

    void NextPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentPoint = (currentPoint + 1) % patrolPoints.Length;
        
        if (!loopPatrol && currentPoint == 0)
        {
            currentPoint = patrolPoints.Length - 1;
            if (navAgent != null) navAgent.isStopped = true;
            return;
        }

        navAgent?.SetDestination(patrolPoints[currentPoint].position);
    }
}