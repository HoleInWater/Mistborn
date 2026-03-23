// NOTE: Line 65 contains Debug.Log which should be removed for production
// NOTE: Consider adding [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))] attribute for pathfinding
using UnityEngine;

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
    
    [Header("AI Settings")]
    // NOTE: Consider adding [Range(1f, 100f)] attribute for detectionRange
    public float detectionRange = 20f;
    // NOTE: Consider adding [Range(0.1f, 10f)] attribute for attackRange
    public float attackRange = 2f;
    // NOTE: Consider adding [Range(0.1f, 20f)] attribute for moveSpeed
    public float moveSpeed = 3f;
    
    [Header("State")]
    public EmotionState currentEmotion = EmotionState.Neutral;
    // NOTE: Consider adding [Range(0.1f, 5f)] attribute for aggressionMultiplier
    public float aggressionMultiplier = 1f;
    
    private Transform player;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private float originalSpeed;

    [Header("Emotional Aura")]
    private ParticleSystem auraParticles;
    private float auraExpiryTimer = 0f;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        originalSpeed = moveSpeed;
    }
    
    void Update()
    {
        if (auraExpiryTimer > 0f)
        {
            auraExpiryTimer -= Time.deltaTime;
            if (auraExpiryTimer <= 0f) ResetAura();
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }
    
    void ChasePlayer()
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    void AttackPlayer()
    {
        Debug.Log($"{gameObject.name} attacks player!");
    }
    
    void Patrol()
    {
        if (navAgent != null && !navAgent.hasPath)
        {
            Vector3 randomPos = Random.insideUnitSphere * 10f + transform.position;
            navAgent.SetDestination(randomPos);
        }
    }
    
    public void SetEmotionState(EmotionState newState)
    {
        currentEmotion = newState;
        UpdateBehavior();
    }
    
    public void SetAggressionMultiplier(float multiplier)
    {
        aggressionMultiplier = multiplier;
        moveSpeed = originalSpeed * multiplier;
    }
    
    void UpdateBehavior()
    {
        switch (currentEmotion)
        {
            case EmotionState.Calm:
                moveSpeed = originalSpeed * 0.5f;
                detectionRange = 5f;
                break;
            case EmotionState.Aggressive:
                moveSpeed = originalSpeed * 1.5f;
                detectionRange = 15f;
                break;
            case EmotionState.Enraged:
                moveSpeed = originalSpeed * 2f;
                detectionRange = 25f;
                break;
            case EmotionState.Fearful:
                moveSpeed = originalSpeed * 1.5f;
                detectionRange = 30f;
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
