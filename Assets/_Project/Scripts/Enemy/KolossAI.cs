using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Lore-accurate Koloss AI. Koloss are massive blue-skinned creatures held together
/// by Hemalurgic spikes. They grow larger with age (skin tears and stretches).
/// Rage mode escalates damage/speed. Ground slam AOE. Removing spikes makes them docile.
/// </summary>
public class KolossAI : MonoBehaviour
{
    [Header("Size & Age")]
    [Range(1f, 4f)] public float sizeMultiplier = 2.5f;
    public float ageYears = 5f;

    [Header("Stats")]
    public float baseHealth = 500f;
    public float baseDamage = 60f;
    public float baseMoveSpeed = 3f;
    public float baseRunSpeed = 6f;
    public float attackRange = 4f;
    public float detectionRange = 25f;
    public float attackCooldown = 2.5f;

    [Header("Rage Mode")]
    [Range(0f, 1f)] public float rageHealthThreshold = 0.3f;
    public float rageDamageMultiplier = 2f;
    public float rageSpeedMultiplier = 1.8f;
    public float rageScaleIncrease = 0.3f;
    [Range(0f, 1f)] public float rageDefenseReduction = 0.5f;

    [Header("Ground Slam")]
    public float slamRadius = 6f;
    public float slamDamage = 80f;
    public float slamForce = 30f;
    public float slamCooldown = 8f;
    public GameObject slamEffectPrefab;

    [Header("Hemalurgic Spikes")]
    public int spikeCount = 4;
    public bool spikesIntact = true;
    [Tooltip("Removing spikes makes Koloss docile and eventually die")]
    public float spikeRemovalRange = 2f;

    [Header("Koloss Sword")]
    public GameObject kolossSwordPrefab;
    public bool dropSwordOnDeath = true;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public EnemyHealth healthComponent;

    // Internal state
    private Transform player;
    private bool isRaging = false;
    private bool isDying = false;
    private bool isDocile = false;
    private float lastAttackTime;
    private float lastSlamTime;
    private float currentDamage;
    private float currentScale;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (healthComponent == null) healthComponent = GetComponent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Size scales with age — older Koloss are bigger
        sizeMultiplier = Mathf.Clamp(1.5f + ageYears * 0.3f, 1.5f, 4f);
        currentScale = sizeMultiplier;
        transform.localScale = Vector3.one * currentScale;

        // Stats scale with size
        currentDamage = baseDamage * (sizeMultiplier / 2.5f);
        if (agent != null)
        {
            agent.speed = baseMoveSpeed;
            agent.stoppingDistance = attackRange - 1f;
        }
    }

    void Update()
    {
        if (isDying || isDocile || player == null) return;
        if (healthComponent != null && healthComponent.isDead) { isDying = true; OnDeath(); return; }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Check rage mode
        UpdateRageState();

        if (distToPlayer > detectionRange)
        {
            Idle();
        }
        else if (distToPlayer > attackRange)
        {
            Chase();
        }
        else
        {
            MeleeAttack(distToPlayer);
        }

        UpdateAnimations();
    }

    // ── Rage Mode ────────────────────────────────────────────────────────

    void UpdateRageState()
    {
        if (healthComponent == null) return;
        float maxHp = healthComponent.GetMaxHealth();
        if (maxHp <= 0f) return;
        float hpPercent = healthComponent.GetCurrentHealth() / maxHp;

        if (!isRaging && hpPercent <= rageHealthThreshold)
        {
            EnterRage();
        }
    }

    void EnterRage()
    {
        isRaging = true;
        currentDamage = baseDamage * (sizeMultiplier / 2.5f) * rageDamageMultiplier;
        currentScale = sizeMultiplier + rageScaleIncrease;
        transform.localScale = Vector3.one * currentScale;

        if (agent != null)
        {
            agent.speed = baseRunSpeed * rageSpeedMultiplier;
        }

        animator?.SetBool("IsRaging", true);
    }

    // ── Movement ─────────────────────────────────────────────────────────

    void Idle()
    {
        if (agent != null) agent.speed = baseMoveSpeed * 0.5f;
    }

    void Chase()
    {
        if (agent == null || !agent.enabled) return;
        agent.speed = isRaging ? baseRunSpeed * rageSpeedMultiplier : baseRunSpeed;
        agent.SetDestination(player.position);
    }

    // ── Attacks ──────────────────────────────────────────────────────────

    void MeleeAttack(float distToPlayer)
    {
        if (agent != null) agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // Ground slam when multiple enemies are close or in rage
        if (isRaging && Time.time - lastSlamTime >= slamCooldown && distToPlayer < slamRadius)
        {
            GroundSlam();
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            animator?.SetTrigger("Attack");

            IDamageable target = player.GetComponentInParent<IDamageable>();
            target?.TakeDamage(currentDamage);

            // Koloss attacks have knockback
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 knockDir = (player.position - transform.position).normalized;
                playerRb.AddForce(knockDir * 20f * (sizeMultiplier / 2.5f), ForceMode.Impulse);
            }
        }
    }

    void GroundSlam()
    {
        lastSlamTime = Time.time;
        animator?.SetTrigger("Slam");

        // AOE damage to everything in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, slamRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = slamRadius > 0f ? 1f - (dist / slamRadius) : 1f;
                damageable.TakeDamage(slamDamage * Mathf.Max(0f, falloff));
            }

            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * slamForce, ForceMode.Impulse);
            }
        }

        // Camera shake
        CameraShakeManager.Instance?.Shake(0.5f, 0.4f);

        // Slam VFX
        if (slamEffectPrefab != null)
            Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);

    }

    // ── Hemalurgic Spikes ────────────────────────────────────────────────

    /// <summary>
    /// Remove a spike from the Koloss. When all spikes are removed, it becomes docile and dies.
    /// Called by player interaction when close enough and Koloss is stunned.
    /// </summary>
    public void RemoveSpike()
    {
        if (!spikesIntact || spikeCount <= 0) return;

        spikeCount--;

        // Each spike removal weakens the Koloss
        currentDamage *= 0.7f;
        if (agent != null) agent.speed *= 0.8f;

        if (spikeCount <= 0)
        {
            BecomeDocile();
        }
    }

    void BecomeDocile()
    {
        isDocile = true;
        spikesIntact = false;
        if (agent != null) agent.isStopped = true;
        animator?.SetBool("IsDocile", true);

        StartCoroutine(DocileDeath());
    }

    IEnumerator DocileDeath()
    {
        yield return new WaitForSecondsRealtime(5f);
        OnDeath();
    }

    /// <summary>
    /// Can the player attempt to remove a spike? (Must be in range)
    /// </summary>
    public bool CanRemoveSpike(Transform playerTransform)
    {
        if (!spikesIntact || spikeCount <= 0) return false;
        return Vector3.Distance(transform.position, playerTransform.position) < spikeRemovalRange;
    }

    // ── Death ────────────────────────────────────────────────────────────

    void OnDeath()
    {
        isDying = true;
        animator?.SetBool("IsDead", true);
        if (agent != null) agent.isStopped = true;

        if (dropSwordOnDeath && kolossSwordPrefab != null)
        {
            Vector3 dropPos = transform.position + transform.forward * 1.5f;
            Instantiate(kolossSwordPrefab, dropPos, Quaternion.identity);
        }

        Destroy(gameObject, 4f);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = agent != null ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    public bool IsRaging() => isRaging;
    public bool IsDocile() => isDocile;
    public int GetSpikeCount() => spikeCount;
}
