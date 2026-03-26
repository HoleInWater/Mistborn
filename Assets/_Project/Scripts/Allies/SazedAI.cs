using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sazed companion AI — Terris Keeper and Feruchemist.
/// Lore: Sazed uses Feruchemy (pewter for strength, steel for speed, gold for healing).
/// Support role: heals the player by tapping gold metalminds, buffs with pewter.
/// Non-aggressive — only fights when directly attacked.
/// </summary>
public class SazedAI : MonoBehaviour
{
    [Header("Support")]
    public float healRange = 8f;
    public float healAmountPerSecond = 10f;
    public float healCooldown = 0.5f;
    public float playerHealThreshold = 0.6f;

    [Header("Self Defense")]
    public float pewterDamage = 40f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;

    [Header("Following")]
    public float followDistance = 5f;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    private Transform player;
    private PlayerHealth playerHealth;
    private float attackTimer;
    private float healTimer;
    private bool inCombat = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }
        CompanionManager.Instance?.RegisterCompanion(gameObject);
    }

    void Update()
    {
        if (player == null) return;
        attackTimer -= Time.deltaTime;
        healTimer -= Time.deltaTime;

        // Teleport if too far
        if (Vector3.Distance(transform.position, player.position) > 25f)
        {
            transform.position = player.position + player.forward * -3f;
            agent?.Warp(transform.position);
        }

        // Priority 1: Heal the player if injured
        if (ShouldHealPlayer())
        {
            HealPlayer();
        }

        // Priority 2: Self defense
        Transform threat = FindNearestThreat();
        if (threat != null && Vector3.Distance(transform.position, threat.position) < attackRange)
        {
            if (attackTimer <= 0f)
            {
                PewterPunch(threat);
                attackTimer = attackCooldown;
            }
        }
        else
        {
            Follow();
        }

        if (animator != null)
            animator.SetFloat("Speed", agent != null ? agent.velocity.magnitude : 0f, 0.1f, Time.deltaTime);
    }

    bool ShouldHealPlayer()
    {
        if (playerHealth == null || healTimer > 0f) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > healRange) return false;
        return playerHealth.GetCurrentHealth() / playerHealth.GetMaxHealth() < playerHealThreshold;
    }

    void HealPlayer()
    {
        healTimer = healCooldown;
        playerHealth.Heal(healAmountPerSecond * Time.deltaTime);

        // Lore bark
        if (Random.value < 0.01f);
    }

    void PewterPunch(Transform target)
    {
        animator?.SetTrigger("Attack");
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        IDamageable damageable = target.GetComponent<IDamageable>();
        damageable?.TakeDamage(pewterDamage);

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            rb.AddForce(dir * 15f, ForceMode.Impulse);
        }

        if (Random.value < 0.3f)
    }

    void Follow()
    {
        if (agent == null) return;
        if (Vector3.Distance(transform.position, player.position) > followDistance)
            agent.SetDestination(player.position + player.forward * -3f);
        else
            agent.ResetPath();
    }

    Transform FindNearestThreat()
    {
        float closest = 5f;
        Transform nearest = null;
        foreach (var e in MistbornRegistry.ActiveEnemies)
        {
            if (e == null) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < closest) { closest = d; nearest = e.transform; }
        }
        return nearest;
    }

    public void OnSupportRequested(Transform enemy) { /* Sazed avoids combat */ }
}
