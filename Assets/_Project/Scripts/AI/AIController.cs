using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [Header("AI States")]
    public AgentState currentState = AgentState.Idle;
    
    [Header("References")]
    public Transform player;
    public NavMeshAgent navAgent;
    public Animator animator;
    
    [Header("Detection")]
    public float detectionRange = 15f;
    public float hearingRange = 10f;
    public float sightAngle = 60f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    
    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float damage = 10f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    
    [Header("Wander")]
    public float wanderRadius = 20f;
    public float wanderTimer = 5f;
    
    private float lastAttackTime = 0f;
    private float wanderTimerCurrent = 0f;
    private Vector3 wanderTarget;
    private float stateTimer = 0f;
    
    public enum AgentState
    {
        Idle,
        Patrol,
        Investigate,
        Chase,
        Attack,
        Flee,
        Return
    }
    
    void Start()
    {
        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        SetState(AgentState.Patrol);
    }
    
    void Update()
    {
        stateTimer += Time.deltaTime;
        
        switch (currentState)
        {
            case AgentState.Idle:
                UpdateIdle();
                break;
            case AgentState.Patrol:
                UpdatePatrol();
                break;
            case AgentState.Investigate:
                UpdateInvestigate();
                break;
            case AgentState.Chase:
                UpdateChase();
                break;
            case AgentState.Attack:
                UpdateAttack();
                break;
            case AgentState.Flee:
                UpdateFlee();
                break;
            case AgentState.Return:
                UpdateReturn();
                break;
        }
        
        CheckForPlayer();
        UpdateAnimator();
    }
    
    void CheckForPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angle <= sightAngle / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange, obstacleLayer))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        SetState(AgentState.Chase);
                        return;
                    }
                }
            }
        }
        
        if (distanceToPlayer <= hearingRange)
        {
            NoiseMaker noise = player.GetComponent<NoiseMaker>();
            if (noise != null && noise.IsMakingNoise())
            {
                wanderTarget = player.position;
                SetState(AgentState.Investigate);
            }
        }
    }
    
    void UpdateIdle()
    {
        if (stateTimer > 3f)
        {
            SetState(AgentState.Patrol);
        }
    }
    
    void UpdatePatrol()
    {
        if (navAgent == null || !navAgent.hasPath)
        {
            wanderTimerCurrent += Time.deltaTime;
            
            if (wanderTimerCurrent >= wanderTimer)
            {
                wanderTimerCurrent = 0f;
                GetNewWanderTarget();
            }
        }
    }
    
    void UpdateInvestigate()
    {
        if (navAgent == null) return;
        
        navAgent.SetDestination(wanderTarget);
        
        float distanceToTarget = Vector3.Distance(transform.position, wanderTarget);
        if (distanceToTarget < 2f)
        {
            SetState(AgentState.Patrol);
        }
        
        if (stateTimer > 10f)
        {
            SetState(AgentState.Return);
        }
    }
    
    void UpdateChase()
    {
        if (navAgent == null || player == null) return;
        
        navAgent.SetDestination(player.position);
        navAgent.speed = chaseSpeed;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            SetState(AgentState.Attack);
        }
        else if (distanceToPlayer > detectionRange * 1.5f)
        {
            SetState(AgentState.Return);
        }
    }
    
    void UpdateAttack()
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(transform.position);
        }
        
        if (player != null)
        {
            transform.LookAt(player);
        }
        
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange * 1.5f)
        {
            SetState(AgentState.Chase);
        }
    }
    
    void UpdateFlee()
    {
        if (navAgent == null || player == null) return;
        
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * 20f;
        
        navAgent.SetDestination(fleeTarget);
        navAgent.speed = chaseSpeed * 1.5f;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > hearingRange * 2f)
        {
            SetState(AgentState.Patrol);
        }
    }
    
    void UpdateReturn()
    {
        if (navAgent == null || !navAgent.hasPath)
        {
            SetState(AgentState.Patrol);
        }
    }
    
    void Attack()
    {
        if (player == null) return;
        
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        
        Debug.Log($"{gameObject.name} attacked for {damage} damage!");
    }
    
    void GetNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            navAgent.SetDestination(wanderTarget);
        }
    }
    
    void SetState(AgentState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        switch (newState)
        {
            case AgentState.Idle:
                if (navAgent != null) navAgent.ResetPath();
                break;
            case AgentState.Patrol:
                if (navAgent != null) navAgent.speed = patrolSpeed;
                GetNewWanderTarget();
                break;
            case AgentState.Chase:
                break;
            case AgentState.Attack:
                break;
            case AgentState.Flee:
                break;
        }
    }
    
    void UpdateAnimator()
    {
        if (animator == null) return;
        
        float speed = 0f;
        if (navAgent != null && navAgent.hasPath)
        {
            speed = navAgent.velocity.magnitude;
        }
        
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsChasing", currentState == AgentState.Chase);
        animator.SetBool("IsAttacking", currentState == AgentState.Attack);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.cyan;
        Vector3 leftDir = Quaternion.Euler(0, -sightAngle / 2f, 0) * transform.forward * detectionRange;
        Vector3 rightDir = Quaternion.Euler(0, sightAngle / 2f, 0) * transform.forward * detectionRange;
        Gizmos.DrawRay(transform.position, leftDir);
        Gizmos.DrawRay(transform.position, rightDir);
    }
}

public class NoiseMaker : MonoBehaviour
{
    [Header("Noise Settings")]
    public float noiseRadius = 10f;
    public float noiseDuration = 0.5f;
    
    private float noiseTimer = 0f;
    private bool isMakingNoise = false;
    
    void Update()
    {
        if (isMakingNoise)
        {
            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0f)
            {
                isMakingNoise = false;
            }
        }
    }
    
    public void MakeNoise(float radius)
    {
        noiseRadius = radius;
        noiseTimer = noiseDuration;
        isMakingNoise = true;
    }
    
    public bool IsMakingNoise()
    {
        return isMakingNoise;
    }
    
    public float GetNoiseRadius()
    {
        return noiseRadius;
    }
}
