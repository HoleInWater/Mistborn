using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Advanced AI for the Kelsier companion.
/// </summary>
public class KelsierAI : MonoBehaviour
{
    [Header("Settings")]
    public float steelPushRange = 20f;
    public float steelPushForce = 50f;
    
    private NavMeshAgent agent;
    private Transform player;
    private CompanionManager manager;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        manager = CompanionManager.Instance;
        
        if (manager != null) manager.RegisterCompanion(gameObject);
    }

    void Update()
    {
        if (player == null) return;

        // Follow Logic
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > 3f)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    /// <summary>
    /// Triggered by CompanionManager when player attacks.
    /// </summary>
    public void OnSupportRequested(Transform enemy)
    {
        Debug.Log($"[KELSIER] Pushing coins at {enemy.name}!");
        PerformSteelPush(enemy);
    }

    private void PerformSteelPush(Transform target)
    {
        // Simulate a steel push coin throw
        Vector3 dir = target.position - transform.position;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        
        if (targetRb != null)
        {
            targetRb.AddForce(dir.normalized * steelPushForce, ForceMode.Impulse);
        }
    }
}
