using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Advanced AI for the Vin companion. Focuses on agility and dual-push/pull.
/// </summary>
public class VinAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        if (CompanionManager.Instance != null) CompanionManager.Instance.RegisterCompanion(gameObject);
    }

    void Update()
    {
        if (player == null) return;

        // Vin is closer following
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > 2f) agent.SetDestination(player.position);
    }

    public void OnSupportRequested(Transform enemy)
    {
        Debug.Log("[VIN] Attacking with Iron/Steel combo!");
        // Simulate Vin's unique "head-shot" coin throw
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (enemy.position - transform.position).normalized;
            rb.AddForce(pushDir * 100f, ForceMode.Impulse);
        }
    }
}
