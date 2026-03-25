using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kelsier companion AI — the Survivor of Hathsin. Full Mistborn.
/// Fights alongside the player using Steel Pushes, Pewter-enhanced melee,
/// and Atium dodges. Charismatic combat bark system.
/// </summary>
public class KelsierAI : MonoBehaviour
{
    [Header("Combat")]
    public float steelPushForce = 80f;
    public float steelPushRange = 20f;
    public float meleeDamage = 35f;
    public float meleeRange = 2.5f;
    public float attackCooldown = 1.5f;
    public float coinThrowCooldown = 3f;

    [Header("Following")]
    public float followDistance = 4f;
    public float teleportDistance = 30f;
    public float combatEngageRange = 15f;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    private Transform player;
    private Transform currentEnemy;
    private float attackTimer;
    private float coinTimer;
    private float barkTimer;

    private enum State { Following, Fighting, Idle }
    private State currentState = State.Following;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        CompanionManager.Instance?.RegisterCompanion(gameObject);
    }

    void Update()
    {
        if (player == null) return;

        attackTimer -= Time.deltaTime;
        coinTimer -= Time.deltaTime;
        barkTimer -= Time.deltaTime;

        // Teleport if too far
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > teleportDistance)
        {
            transform.position = player.position + player.forward * -2f;
            if (agent != null) agent.Warp(transform.position);
        }

        // Find enemies
        currentEnemy = FindNearestEnemy();

        if (currentEnemy != null && Vector3.Distance(transform.position, currentEnemy.position) < combatEngageRange)
            currentState = State.Fighting;
        else
            currentState = State.Following;

        switch (currentState)
        {
            case State.Following: FollowPlayer(); break;
            case State.Fighting: FightEnemy(); break;
        }

        UpdateAnimations();
    }

    void FollowPlayer()
    {
        if (agent == null) return;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > followDistance)
            agent.SetDestination(player.position);
        else
            agent.ResetPath();
    }

    void FightEnemy()
    {
        if (currentEnemy == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, currentEnemy.position);

        if (dist > steelPushRange)
        {
            agent.SetDestination(currentEnemy.position);
        }
        else if (dist > meleeRange && coinTimer <= 0f)
        {
            // Steel Push — throw coins
            SteelPushAttack(currentEnemy);
            coinTimer = coinThrowCooldown;
            CombatBark("steel");
        }
        else if (dist <= meleeRange && attackTimer <= 0f)
        {
            // Pewter-enhanced melee
            MeleeAttack(currentEnemy);
            attackTimer = attackCooldown;
            CombatBark("melee");
        }
        else
        {
            agent.SetDestination(currentEnemy.position);
        }
    }

    void SteelPushAttack(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
            targetRb.AddForce(dir * steelPushForce, ForceMode.Impulse);

        animator?.SetTrigger("Attack");
        SoundManager.Instance?.PlayPushSound();
    }

    void MeleeAttack(Transform target)
    {
        animator?.SetTrigger("Attack");
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        IDamageable damageable = target.GetComponent<IDamageable>();
        damageable?.TakeDamage(meleeDamage);

        // Knockback
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            rb.AddForce(dir * 10f, ForceMode.Impulse);
        }
    }

    void CombatBark(string type)
    {
        if (barkTimer > 0f) return;
        barkTimer = 8f;

        string[] steelBarks = {
            "Catch this!", "Steel's my favorite!", "Push off that, will you!",
            "The Lord Ruler won't save you from coins!"
        };
        string[] meleeBarks = {
            "Pewter makes this easy!", "You're slower than a Koloss!",
            "I survived the Pits — you think YOU scare me?"
        };

        string[] barks = type == "steel" ? steelBarks : meleeBarks;
        Debug.Log($"[KELSIER] \"{barks[Random.Range(0, barks.Length)]}\"");
    }

    Transform FindNearestEnemy()
    {
        float closest = combatEngageRange;
        Transform nearest = null;

        var enemies = MistbornRegistry.ActiveEnemies;
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closest)
            {
                closest = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    public void OnSupportRequested(Transform enemy)
    {
        currentEnemy = enemy;
        currentState = State.Fighting;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = agent != null ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }
}
