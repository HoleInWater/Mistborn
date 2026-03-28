using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Vin companion AI — stealthy Mistborn. Uses Pewter melee, coin throws,
/// and stays behind the player until combat starts.
/// </summary>
public class VinAI : MonoBehaviour
{
    [Header("Combat")]
    public float meleeDamage = 25f;
    public float coinDamage = 30f;
    public float attackRange = 2f;
    public float rangedRange = 15f;
    public float attackCooldown = 1.2f;

    [Header("Following")]
    public float followDistance = 5f;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    private Transform player;
    private Transform currentEnemy;
    private float attackTimer;
    private bool inCombat = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        CompanionManager.Instance?.RegisterCompanion(gameObject);
    }

    void Update()
    {
        if (player == null) return;
        attackTimer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, player.position) > 25f)
        {
            transform.position = player.position - player.forward * 2f;
            agent?.Warp(transform.position);
        }

        currentEnemy = FindNearestEnemy();
        inCombat = currentEnemy != null && Vector3.Distance(transform.position, currentEnemy.position) < rangedRange;

        if (inCombat) Fight(); else Follow();

        if (animator != null)
        {
            animator.SetFloat("Speed", agent != null ? agent.velocity.magnitude : 0f, 0.1f, Time.deltaTime);
            animator.SetBool("InCombat", inCombat);
        }
    }

    void Follow()
    {
        if (agent == null) return;
        if (Vector3.Distance(transform.position, player.position) > followDistance)
            agent.SetDestination(player.position - player.forward * 2f + player.right * -1.5f);
        else
            agent.ResetPath();
    }

    void Fight()
    {
        if (currentEnemy == null || attackTimer > 0f) return;
        float dist = Vector3.Distance(transform.position, currentEnemy.position);

        if (dist <= attackRange)
        {
            attackTimer = attackCooldown;
            animator?.SetTrigger("Attack");
            transform.LookAt(new Vector3(currentEnemy.position.x, transform.position.y, currentEnemy.position.z));
            currentEnemy.GetComponentInParent<IDamageable>()?.TakeDamage(meleeDamage);
        }
        else if (dist < rangedRange)
        {
            attackTimer = attackCooldown * 1.5f;
            currentEnemy.GetComponentInParent<IDamageable>()?.TakeDamage(coinDamage);
            SoundManager.Instance?.PlayPushSound();
        }
        else
        {
            agent?.SetDestination(currentEnemy.position);
        }
    }

    Transform FindNearestEnemy()
    {
        float closest = rangedRange;
        Transform nearest = null;
        foreach (var e in MistbornRegistry.ActiveEnemies)
        {
            if (e == null) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < closest) { closest = d; nearest = e.transform; }
        }
        return nearest;
    }

    public void OnSupportRequested(Transform enemy) { currentEnemy = enemy; }
}
