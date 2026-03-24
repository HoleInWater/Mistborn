using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Aggressive heavy melee AI for the Koloss.
/// </summary>
public class KolossAI : MonoBehaviour
{
    public float sizeMult = 2.5f;
    public float rageThreshold = 0.3f;
    
    private NavMeshAgent agent;
    private Transform player;
    private EnemyHealth health;

    void Start()
    {
        transform.localScale = Vector3.one * sizeMult;
        agent = GetComponent<NavMeshAgent>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        health = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (player == null || health == null || health.isDead) return;

        agent.SetDestination(player.position);
        
        // Rage mode if low on health
        if (health.GetCurrentHealth() / health.GetMaxHealth() < rageThreshold)
        {
            agent.speed = 10f;
            transform.localScale = Vector3.one * (sizeMult * 1.2f);
        }
    }
}
