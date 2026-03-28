using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Tactical squad-based AI for Hazekillers — elite anti-Allomancer soldiers.
/// Lore: Hazekillers wear aluminum-lined helmets (immune to emotional Allomancy),
/// carry non-metallic weapons (immune to Steel/Iron), and fight in coordinated squads.
/// </summary>
public class HazekillerAI : MonoBehaviour, IDamageable
{
    [Header("Detection")]
    public float detectRange = 15f;
    public float attackRange = 2.5f;

    [Header("Stats")]
    public float health = 100f;
    public float attackDamage = 30f;
    public float attackCooldown = 1.5f;
    public float moveSpeed = 4.5f;
    public float runSpeed = 7f;

    [Header("Anti-Allomancy")]
    [Tooltip("Aluminum-lined helmet blocks Zinc/Brass emotional Allomancy")]
    public bool hasAluminumHelmet = true;
    [Tooltip("Non-metallic weapons cannot be Pushed/Pulled")]
    public bool hasNonMetallicWeapon = true;
    [Tooltip("Wooden shield blocks coin projectiles")]
    public bool hasWoodenShield = true;

    [Header("Squad Settings")]
    public float squadCoordinationRange = 12f;
    public float formationSpacing = 3f;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    // ── Squad State ──────────────────────────────────────────────────────
    public enum Formation { None, Triangle, ShieldWall, Flank, Rush }
    public enum Role { Leader, Flanker, Shield, Support }

    private Transform player;
    private Formation currentFormation = Formation.None;
    private Role myRole = Role.Flanker;
    private HazekillerAI squadLeader;
    private List<HazekillerAI> squadMembers = new List<HazekillerAI>();
    private bool isLeader = false;

    private float lastAttackTime;
    private float tacticalReassessTimer;
    private Vector3 formationTarget;
    private bool playerIsAllomancer = false;
    private bool playerIsCoinshot = false;
    private bool playerIsLurcher = false;

    // ── Static Squad Registry ────────────────────────────────────────────
    private static List<HazekillerAI> allHazekillers = new List<HazekillerAI>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticState() => allHazekillers = new List<HazekillerAI>();

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (agent != null) agent.speed = moveSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Allomancer allo = playerObj.GetComponent<Allomancer>();
            if (allo != null)
            {
                playerIsAllomancer = true;
                playerIsCoinshot = allo.unlockedMetals[(int)AllomancySkill.MetalType.Steel];
                playerIsLurcher = allo.unlockedMetals[(int)AllomancySkill.MetalType.Iron];
            }
        }

        allHazekillers.Add(this);
        AssignSquad();
    }

    void OnDestroy()
    {
        allHazekillers.Remove(this);
        if (isLeader)
        {
            foreach (var m in squadMembers)
            {
                if (m != null) m.squadLeader = null;
            }
        }
    }

    void Update()
    {
        if (health <= 0 || player == null || agent == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Reassess tactics periodically
        tacticalReassessTimer -= Time.deltaTime;
        if (tacticalReassessTimer <= 0f)
        {
            tacticalReassessTimer = 1f;
            AssessFormation(distToPlayer);
        }

        // Act based on distance
        if (distToPlayer > detectRange)
        {
            Patrol();
        }
        else if (distToPlayer > attackRange)
        {
            ExecuteFormation(distToPlayer);
        }
        else
        {
            Attack();
        }

        UpdateAnimations();
    }

    // ── Squad Assignment ─────────────────────────────────────────────────

    void AssignSquad()
    {
        // Find nearby hazekillers and form squad
        squadMembers.Clear();
        HazekillerAI closestLeader = null;
        float closestDist = float.MaxValue;

        foreach (var hk in allHazekillers)
        {
            if (hk == this || hk == null) continue;
            float dist = Vector3.Distance(transform.position, hk.transform.position);
            if (dist < squadCoordinationRange)
            {
                if (hk.isLeader && dist < closestDist)
                {
                    closestLeader = hk;
                    closestDist = dist;
                }
            }
        }

        if (closestLeader != null)
        {
            // Join existing squad
            squadLeader = closestLeader;
            closestLeader.squadMembers.Add(this);
            isLeader = false;
            AssignRole(closestLeader.squadMembers.Count);
        }
        else
        {
            // Become squad leader
            isLeader = true;
            squadLeader = this;
            myRole = Role.Leader;

            foreach (var hk in allHazekillers)
            {
                if (hk == this || hk == null) continue;
                float dist = Vector3.Distance(transform.position, hk.transform.position);
                if (dist < squadCoordinationRange && hk.squadLeader == null)
                {
                    hk.squadLeader = this;
                    hk.isLeader = false;
                    squadMembers.Add(hk);
                    hk.AssignRole(squadMembers.Count);
                }
            }
        }
    }

    void AssignRole(int memberIndex)
    {
        switch (memberIndex % 3)
        {
            case 0: myRole = Role.Flanker; break;
            case 1: myRole = Role.Shield; break;
            case 2: myRole = Role.Support; break;
        }
    }

    // ── Tactical Assessment ──────────────────────────────────────────────

    void AssessFormation(float distToPlayer)
    {
        if (!isLeader) return;

        int squadSize = squadMembers.Count + 1;

        if (playerIsCoinshot && distToPlayer < 20f)
        {
            // Spread out vs Coinshot — coins push in lines, spreading reduces multi-kills
            currentFormation = Formation.Flank;
        }
        else if (playerIsLurcher && distToPlayer < 15f)
        {
            // Rush vs Lurcher — close distance fast so pull is less effective
            currentFormation = Formation.Rush;
        }
        else if (squadSize >= 3 && distToPlayer < 10f)
        {
            // Shield wall for close engagement
            currentFormation = Formation.ShieldWall;
        }
        else if (squadSize >= 2)
        {
            // Default triangle approach
            currentFormation = Formation.Triangle;
        }
        else
        {
            currentFormation = Formation.None;
        }

        // Relay formation to squad
        foreach (var m in squadMembers)
        {
            if (m != null) m.currentFormation = currentFormation;
        }
    }

    // ── Formation Execution ──────────────────────────────────────────────

    void ExecuteFormation(float distToPlayer)
    {
        if (agent == null || !agent.enabled || player == null) return;
        agent.speed = runSpeed;

        Vector3 targetPos;

        switch (currentFormation)
        {
            case Formation.Triangle:
                targetPos = GetTrianglePosition();
                break;
            case Formation.ShieldWall:
                targetPos = GetShieldWallPosition();
                break;
            case Formation.Flank:
                targetPos = GetFlankPosition();
                break;
            case Formation.Rush:
                targetPos = player.position;
                agent.speed = runSpeed * 1.3f; // Sprint for rush
                break;
            default:
                targetPos = player.position;
                break;
        }

        agent.SetDestination(targetPos);
    }

    Vector3 GetTrianglePosition()
    {
        Vector3 dirToPlayer = (player.position - GetSquadCenter()).normalized;
        Vector3 perpendicular = Vector3.Cross(dirToPlayer, Vector3.up).normalized;

        switch (myRole)
        {
            case Role.Leader:
                return player.position - dirToPlayer * formationSpacing;
            case Role.Flanker:
                return player.position - dirToPlayer * formationSpacing + perpendicular * formationSpacing;
            case Role.Shield:
                return player.position - dirToPlayer * formationSpacing - perpendicular * formationSpacing;
            default:
                return player.position - dirToPlayer * (formationSpacing * 2f);
        }
    }

    Vector3 GetShieldWallPosition()
    {
        Vector3 dirToPlayer = (player.position - GetSquadCenter()).normalized;
        Vector3 perpendicular = Vector3.Cross(dirToPlayer, Vector3.up).normalized;

        int myIndex = isLeader ? 0 : (squadLeader != null ? squadLeader.squadMembers.IndexOf(this) + 1 : 0);
        float offset = (myIndex - (GetSquadSize() - 1) * 0.5f) * (formationSpacing * 0.7f);

        return player.position - dirToPlayer * attackRange + perpendicular * offset;
    }

    Vector3 GetFlankPosition()
    {
        // Spread wide to avoid coin spread patterns
        Vector3 dirToPlayer = (player.position - GetSquadCenter()).normalized;
        Vector3 perpendicular = Vector3.Cross(dirToPlayer, Vector3.up).normalized;

        int myIndex = isLeader ? 0 : (squadLeader != null ? squadLeader.squadMembers.IndexOf(this) + 1 : 0);
        float side = (myIndex % 2 == 0) ? 1f : -1f;
        float spreadDist = formationSpacing * 2f * ((myIndex / 2) + 1);

        return player.position + perpendicular * side * spreadDist - dirToPlayer * 2f;
    }

    Vector3 GetSquadCenter()
    {
        if (!isLeader && squadLeader != null) return squadLeader.transform.position;
        return transform.position;
    }

    int GetSquadSize()
    {
        if (isLeader) return squadMembers.Count + 1;
        if (squadLeader != null) return squadLeader.squadMembers.Count + 1;
        return 1;
    }

    // ── Combat ───────────────────────────────────────────────────────────

    void Attack()
    {
        if (agent != null) agent.SetDestination(transform.position); // Stop moving

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            animator?.SetTrigger("Attack");

            IDamageable target = player.GetComponentInParent<IDamageable>();
            target?.TakeDamage(attackDamage);

            // Shield bash staggers if close
            if (hasWoodenShield && Vector3.Distance(transform.position, player.position) < 1.5f)
            {
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 bashDir = (player.position - transform.position).normalized;
                    playerRb.AddForce(bashDir * 15f, ForceMode.Impulse);
                }
            }
        }
    }

    void Patrol()
    {
        if (agent == null || !agent.enabled) return;
        agent.speed = moveSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
            randomPoint.y = transform.position.y;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    // ── Anti-Allomancy ───────────────────────────────────────────────────

    /// <summary>
    /// Aluminum helmet blocks emotional Allomancy (Zinc Riot / Brass Soothe).
    /// Called by Zinc.cs and Brass.cs before applying effects.
    /// </summary>
    public bool IsImmuneToEmotionalAllomancy()
    {
        return hasAluminumHelmet;
    }

    /// <summary>
    /// Non-metallic weapons cannot be Pushed or Pulled.
    /// Called by AllomanticTarget detection.
    /// </summary>
    public bool HasMetallicEquipment()
    {
        return !hasNonMetallicWeapon;
    }

    // ── Damage ───────────────────────────────────────────────────────────

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            animator?.SetTrigger("Hit");
            // Alert squad to retaliate
            if (squadLeader != null && squadLeader != this)
            {
                squadLeader.currentFormation = Formation.Rush;
            }
        }
    }

    void Die()
    {
        animator?.SetBool("IsDead", true);
        if (agent != null) agent.isStopped = true;
        allHazekillers.Remove(this);
        Destroy(gameObject, 3f);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = agent != null ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    // ── Leader Command API ───────────────────────────────────────────────

    public void CommandFormation(Formation formation)
    {
        if (!isLeader) return;
        currentFormation = formation;
        foreach (var m in squadMembers)
        {
            if (m != null) m.currentFormation = formation;
        }
    }

    public bool IsSquadLeader() => isLeader;
    public int GetSquadMemberCount() => squadMembers.Count;
    public Formation GetCurrentFormation() => currentFormation;
}
