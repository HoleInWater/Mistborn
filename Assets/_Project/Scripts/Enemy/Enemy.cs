using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseRange = 10f;
    public float patrolSpeed = 1.5f;
    
    [Header("AI Behavior")]
    public AIAgent.AgentState currentState = AIAgent.AgentState.Patrol;
    public bool isChasing = false;
    public bool isAttacking = false;
    public float stateChangeDelay = 0.5f;
    
    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;
    
    [Header("Effects")]
    public GameObject damageEffect;
    public GameObject deathEffect;
    public AudioClip hurtSound;
    public AudioClip attackSound;
    
    private NavMeshAgent navAgent;
    private float lastAttackTime = 0f;
    private int currentPatrolIndex = 0;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        StartCoroutine(StateMachine());
    }
    
    System.Collections.IEnumerator StateMachine()
    {
        while (!isDead)
        {
            switch (currentState)
            {
                case AIAgent.AgentState.Patrol:
                    Patrol();
                    break;
                case AIAgent.AgentState.Chase:
                    Chase();
                    break;
                case AIAgent.AgentState.Attack:
                    Attack();
                    break;
                case AIAgent.AgentState.Idle:
                    Idle();
                    break;
            }
            
            yield return new WaitForSeconds(stateChangeDelay);
        }
    }
    
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            currentState = AIAgent.AgentState.Idle;
            return;
        }
        
        navAgent.speed = patrolSpeed;
        navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        
        if (Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        
        CheckForPlayer();
    }
    
    void Chase()
    {
        if (player == null) return;
        
        navAgent.speed = moveSpeed;
        navAgent.SetDestination(player.position);
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIAgent.AgentState.Attack;
        }
        else if (distanceToPlayer > chaseRange * 1.5f)
        {
            currentState = AIAgent.AgentState.Patrol;
            isChasing = false;
        }
    }
    
    void Attack()
    {
        if (player == null) return;
        
        navAgent.SetDestination(transform.position);
        
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            currentState = AIAgent.AgentState.Chase;
        }
    }
    
    void Idle()
    {
        navAgent.SetDestination(transform.position);
        CheckForPlayer();
    }
    
    void CheckForPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= chaseRange)
        {
            currentState = AIAgent.AgentState.Chase;
            isChasing = true;
        }
    }
    
    void PerformAttack()
    {
        if (player == null) return;
        
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
        
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        
        Debug.Log($"{gameObject.name} attacked player for {damage} damage!");
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        
        currentHealth -= damageAmount;
        
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        if (hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(hurtSound, transform.position);
        }
        
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        LootDrop loot = GetComponent<LootDrop>();
        if (loot != null)
        {
            loot.DropLoot();
        }
        
        Debug.Log($"{gameObject.name} has been defeated!");
        
        Destroy(gameObject, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.blue;
        if (patrolPoints != null)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float spawnInterval = 10f;
    public float spawnRadius = 10f;
    
    [Header("Spawn Conditions")]
    public bool spawnOnStart = true;
    public bool respawnAfterDeath = true;
    public int currentEnemyCount = 0;
    
    [Header("References")]
    public Transform[] spawnPoints;
    
    private float timeSinceLastSpawn = 0f;
    private System.Collections.Generic.List<GameObject> activeEnemies = new System.Collections.Generic.List<GameObject>();
    
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnInitialEnemies();
        }
    }
    
    void Update()
    {
        CleanupDeadEnemies();
        
        if (respawnAfterDeath && currentEnemyCount < maxEnemies)
        {
            timeSinceLastSpawn += Time.deltaTime;
            
            if (timeSinceLastSpawn >= spawnInterval)
            {
                SpawnEnemy();
                timeSinceLastSpawn = 0f;
            }
        }
    }
    
    void SpawnInitialEnemies()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;
        
        Vector3 spawnPos = GetRandomSpawnPoint();
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
        currentEnemyCount++;
        
        Debug.Log($"Spawned enemy at {spawnPos}");
    }
    
    Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return point.position;
        }
        
        Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
        randomPos += transform.position;
        randomPos.y = transform.position.y;
        
        return randomPos;
    }
    
    void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                currentEnemyCount--;
            }
        }
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        currentEnemyCount = 0;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                }
            }
        }
    }
}
