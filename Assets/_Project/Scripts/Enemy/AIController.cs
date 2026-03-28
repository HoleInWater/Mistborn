using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    public enum EmotionState
    {
        Neutral,
        Calm,
        Aggressive,
        Enraged,
        Fearful
    }

    [Header("Detection")]
    [Range(1f, 100f)] public float detectionRange = 20f;
    [Range(0.1f, 10f)]  public float attackRange = 2f;

    [Header("Movement")]
    [Range(0.1f, 20f)] public float moveSpeed = 3f;
    public float patrolRadius = 10f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("State")]
    public EmotionState currentEmotion = EmotionState.Neutral;

    [Header("Temporal")]
    public float externalTimeScaleMultiplier = 1f;

    private Transform player;
    private NavMeshAgent navAgent;
    private EnemySenses senses;         // optional — falls back to range-only detection
    private float originalSpeed;
    private float aggressionMultiplier = 1f;
    private ParticleSystem auraParticles;
    private float auraExpiryTimer = 0f;
    private float originalDetectionRange;
    private float attackTimer = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning("[AIController] No GameObject tagged 'Player' found in scene.");

        navAgent = GetComponent<NavMeshAgent>();
        senses = GetComponent<EnemySenses>();
        originalSpeed = moveSpeed;
        originalDetectionRange = detectionRange;

        MistbornRegistry.RegisterEnemy(this);
    }

    void OnDestroy()
    {
        MistbornRegistry.UnregisterEnemy(this);
    }

    void Update()
    {
        UpdateSpeed();

        if (auraExpiryTimer > 0f)
        {
            auraExpiryTimer -= Time.deltaTime;
            if (auraExpiryTimer <= 0f) ResetAura();
        }

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerDetected = senses != null
            ? senses.CanDetectPlayer
            : distanceToPlayer <= detectionRange;

        if (playerDetected)
        {
            if (distanceToPlayer <= attackRange)
                AttackPlayer();
            else
                ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void UpdateSpeed()
    {
        float targetSpeed = originalSpeed * aggressionMultiplier * externalTimeScaleMultiplier;
        if (navAgent != null) navAgent.speed = targetSpeed;
        moveSpeed = targetSpeed;
    }

    void ChasePlayer()
    {
        if (navAgent != null) navAgent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        if (attackTimer > 0f) return;

        // Stop and face the player while attacking
        if (navAgent != null) navAgent.SetDestination(transform.position);
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        PlayerHealth ph = PlayerHealth.Instance;
        if (ph != null) ph.TakeDamage(attackDamage);

        attackTimer = attackCooldown;
    }

    void Patrol()
    {
        if (navAgent == null) return;
        if (navAgent.pathPending) return;
        if (navAgent.remainingDistance > navAgent.stoppingDistance) return;

        // Sample a random point on the NavMesh within patrol radius
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            navAgent.SetDestination(hit.position);
    }
    
    public void SetEmotionState(EmotionState newState)
    {
        currentEmotion = newState;
        UpdateBehavior();
    }
    
    public void SetAggressionMultiplier(float multiplier)
    {
        aggressionMultiplier = multiplier;
    }
    
    void UpdateBehavior()
    {
        switch (currentEmotion)
        {
            case EmotionState.Calm:
                SetAggressionMultiplier(0.5f);
                detectionRange = 5f;
                break;
            case EmotionState.Aggressive:
                SetAggressionMultiplier(1.5f);
                detectionRange = 15f;
                break;
            case EmotionState.Enraged:
                SetAggressionMultiplier(2.0f);
                detectionRange = 25f;
                break;
            case EmotionState.Fearful:
                SetAggressionMultiplier(1.5f);
                detectionRange = 30f;
                break;
            default:
                SetAggressionMultiplier(1.0f);
                detectionRange = originalDetectionRange; // Reset to inspector value
                break;
        }
    }


    public void SetEmotionalAura(Color color, float intensity)
    {
        if (auraParticles == null) CreateAuraParticles();

        auraExpiryTimer = 0.5f; // Short duration, must be refreshed by the burning Allomancer
        
        var main = auraParticles.main;
        main.startColor = color;
        
        var emission = auraParticles.emission;
        emission.rateOverTime = 20f * intensity;
        
        if (!auraParticles.isPlaying) auraParticles.Play();
    }

    private void CreateAuraParticles()
    {
        GameObject go = new GameObject("EmotionalAura");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up * 1f;

        auraParticles = go.AddComponent<ParticleSystem>();
        
        var main = auraParticles.main;
        main.startLifetime = 0.5f;
        main.startSize = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = auraParticles.emission;
        emission.rateOverTime = 20f;

        var shape = auraParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default")); // Generic glow
    }

    private void ResetAura()
    {
        if (auraParticles != null) auraParticles.Stop();
        currentEmotion = EmotionState.Neutral;
        UpdateBehavior();
    }
}
