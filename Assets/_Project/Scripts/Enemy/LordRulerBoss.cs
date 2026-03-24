using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Final Boss AI for the Lord Ruler. A multi-phase god-tier encounter.
/// </summary>
public class LordRulerBoss : MonoBehaviour
{
    public enum BossPhase { Physical, Spiritual, Godlike }
    public BossPhase currentPhase = BossPhase.Physical;

    [Header("Stats")]
    public float health = 1000f;
    public bool atiumReflexes = true;

    private NavMeshAgent agent;
    private Transform player;
    private bool isExecutingBurst = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        UpdatePhase();
        ExecuteAIBehavior();
    }

    private void UpdatePhase()
    {
        if (health < 300f) currentPhase = BossPhase.Godlike;
        else if (health < 700f) currentPhase = BossPhase.Spiritual;
    }

    private void ExecuteAIBehavior()
    {
        switch (currentPhase)
        {
            case BossPhase.Physical:
                // Heavy Steel/Pewter aggression
                agent.SetDestination(player.position);
                if (Random.value < 0.05f) PerformSteelPush();
                break;
            case BossPhase.Spiritual:
                // Zinc/Brass aura manipulation (Slow player / Reverse inputs)
                agent.speed = 10f;
                // [NARRATIVE] "SOOTHE!"
                break;
            case BossPhase.Godlike:
                // Simultaneous 16-metal flare
                if (!isExecutingBurst) StartCoroutine(PerformOmniBurst());
                break;
        }
    }

    private void PerformSteelPush()
    {
        // God-tier force application
        Vector3 dir = player.position - transform.position;
        player.GetComponent<Rigidbody>()?.AddForce(dir.normalized * 200f, ForceMode.Impulse);
    }

    private System.Collections.IEnumerator PerformOmniBurst()
    {
        isExecutingBurst = true;
        Debug.Log("[LORD RULER] THE WELL IS MINE!");
        // Massive shockwave logic
        yield return new WaitForSeconds(5f);
        isExecutingBurst = false;
    }

    public void TakeDamage(float amount)
    {
        // Extremely high resistance
        health -= amount * 0.1f;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("[BOSS] THE LORD RULER HAS FALLEN. THE ASH STOPS.");
        // End Game logic
    }
}
