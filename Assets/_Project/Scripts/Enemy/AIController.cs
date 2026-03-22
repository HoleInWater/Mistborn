// NOTE: Line 65 contains Debug.Log which should be removed for production
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
    public float detectionRange = 20f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    
    [Header("State")]
    public EmotionState currentEmotion = EmotionState.Neutral;
    public float aggressionMultiplier = 1f;
    
    private Transform player;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private float originalSpeed;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        originalSpeed = moveSpeed;
    }
    
    void Update()
    {
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
}
