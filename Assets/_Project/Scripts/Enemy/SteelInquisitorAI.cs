using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Elite Boss AI for the Steel Inquisitor.
/// </summary>
public class SteelInquisitorAI : MonoBehaviour
{
    [Header("Powers")]
    public bool isBurningAtium = true;
    public float pewterStrengthMult = 3f;
    public float steelPushForce = 80f;

    [Header("References")]
    private NavMeshAgent agent;
    private Transform player;
    private EnemyHealth health;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        var _po = GameObject.FindGameObjectWithTag("Player"); if (_po != null) player = _po.transform;
        health = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (player == null || health == null || health.isDead) return;

        // Inquisitors are aggressive stalkers
        if (agent != null) agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) < 2f)
        {
            PerformMeleeAttack();
        }
        else
        {
            // Occasionally push coins
            if (Random.value < 0.02f) PerformSteelPush();
        }
    }

    private void PerformMeleeAttack()
    {
        // Inquisitors use Pewter for massive damage
        player.SendMessage("TakeDamage", 25f * pewterStrengthMult, SendMessageOptions.DontRequireReceiver);
    }

    private void PerformSteelPush()
    {
        Vector3 dir = player.position - transform.position;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(dir.normalized * steelPushForce, ForceMode.Impulse);
        }
    }
}
