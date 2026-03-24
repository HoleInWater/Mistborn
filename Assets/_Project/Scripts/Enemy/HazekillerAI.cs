using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Tactical AI for the Hazekiller. Uses non-metallic weapons to counter Allomancy.
/// </summary>
public class HazekillerAI : MonoBehaviour
{
    public float detectRange = 10f;
    public float squadCoordinationRange = 5f;
    
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        var _po = GameObject.FindGameObjectWithTag("Player"); if (_po != null) player = _po.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Hazekillers move in squads
        Collider[] nearbySquad = Physics.OverlapSphere(transform.position, squadCoordinationRange);
        if (nearbySquad.Length > 2)
        {
            // Formation logic
            agent.SetDestination(player.position);
        }
    }
}
