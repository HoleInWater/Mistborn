using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Multi-phase Steel Inquisitor boss fight.
/// Phase 1 (100-60% HP): Steel/Iron pushes and pulls, melee attacks.
/// Phase 2 (60-30% HP): Burns Atium — dodges player attacks with future sight.
/// Phase 3 (below 30% HP): Burns ALL metals simultaneously, extremely dangerous.
/// Weakness: removing the linchpin spike from their back kills instantly.
/// </summary>
public class SteelInquisitorAI : MonoBehaviour
{
    public enum BossPhase { Physical, Atium, AllBurn, Dead }

    [Header("Phase Thresholds")]
    [Range(0f, 1f)] public float phase2Threshold = 0.6f;
    [Range(0f, 1f)] public float phase3Threshold = 0.3f;

    [Header("Stats")]
    public float maxHealth = 800f;
    public float baseDamage = 40f;
    public float moveSpeed = 7f;
    public float sprintSpeed = 14f;
    public float attackRange = 3f;
    public float detectionRange = 40f;

    [Header("Allomantic Powers")]
    public float steelPushForce = 100f;
    public float ironPullForce = 80f;
    public float pewterStrengthMult = 3f;
    public float atiumDodgeWindow = 0.8f;

    [Header("Phase 3 — All Burn")]
    public float allBurnDamageMultiplier = 2.5f;
    public float allBurnSpeedMultiplier = 1.5f;
    public float emotionalAuraRadius = 10f;

    [Header("Linchpin Spike")]
    public float spikeRemovalRange = 1.5f;
    public float stunDuration = 3f;
    public bool linchpinExposed = false;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public EnemyHealth healthComponent;
    public GameObject coinPrefab;

    // State
    private Transform player;
    private BossPhase currentPhase = BossPhase.Physical;
    private float currentHealth;
    private float lastAttackTime;
    private float lastPushTime;
    private float lastDodgeTime;
    private bool isStunned = false;
    private bool isDead = false;
    private bool isDodging = false;

    // Phase 3 timers
    private float emotionalPulseTimer;
    private float coinBarrageTimer;

    public System.Action<BossPhase> OnPhaseChanged;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (healthComponent == null) healthComponent = GetComponent<EnemyHealth>();

        currentHealth = maxHealth;
        if (agent != null) agent.speed = moveSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Sync health from component
        if (healthComponent != null)
            currentHealth = healthComponent.GetCurrentHealth();

        if (currentHealth <= 0) { Die(); return; }
        if (isStunned) return;

        UpdatePhase();
        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentPhase)
        {
            case BossPhase.Physical: UpdatePhysical(dist); break;
            case BossPhase.Atium: UpdateAtium(dist); break;
            case BossPhase.AllBurn: UpdateAllBurn(dist); break;
        }

        UpdateAnimations();
    }

    // ── Phase Management ─────────────────────────────────────────────────

    void UpdatePhase()
    {
        if (maxHealth <= 0f) return;
        float hpPercent = currentHealth / maxHealth;
        BossPhase newPhase = currentPhase;

        if (hpPercent <= phase3Threshold)
            newPhase = BossPhase.AllBurn;
        else if (hpPercent <= phase2Threshold)
            newPhase = BossPhase.Atium;
        else
            newPhase = BossPhase.Physical;

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            OnPhaseTransition(newPhase);
        }
    }

    void OnPhaseTransition(BossPhase phase)
    {
        OnPhaseChanged?.Invoke(phase);
        animator?.SetInteger("BossPhase", (int)phase);

        switch (phase)
        {
            case BossPhase.Atium:
                if (agent != null) agent.speed = moveSpeed * 1.3f;
                break;
            case BossPhase.AllBurn:
                if (agent != null) agent.speed = sprintSpeed * allBurnSpeedMultiplier;
                break;
        }

        CameraShakeManager.Instance?.Shake(0.5f, 0.3f);
    }

    // ── Phase 1: Physical ────────────────────────────────────────────────

    void UpdatePhysical(float dist)
    {
        if (dist > attackRange * 3f)
        {
            agent?.SetDestination(player.position);
        }
        else if (dist > attackRange)
        {
            // Alternate between closing distance and ranged push
            if (Time.time - lastPushTime > 4f && Random.value < 0.03f)
            {
                PerformSteelPush();
            }
            else
            {
                agent?.SetDestination(player.position);
            }
        }
        else
        {
            PerformMeleeAttack();
        }
    }

    // ── Phase 2: Atium ───────────────────────────────────────────────────

    void UpdateAtium(float dist)
    {
        // Atium grants future sight — dodge incoming attacks
        if (!isDodging && ShouldDodge())
        {
            StartCoroutine(AtiumDodge());
        }

        if (dist > attackRange)
        {
            // Rapidly close distance with Pewter-enhanced speed
            if (agent != null) agent.speed = sprintSpeed;
            agent?.SetDestination(player.position);
        }
        else
        {
            PerformMeleeAttack();

            // Iron Pull player weapons/coins toward self
            if (Time.time - lastPushTime > 3f)
            {
                PerformIronPull();
                lastPushTime = Time.time;
            }
        }
    }

    bool ShouldDodge()
    {
        if (Time.time - lastDodgeTime < atiumDodgeWindow * 2f) return false;

        // Check if player is attacking (Rigidbody moving fast toward us)
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null) return false;

        Vector3 toMe = (transform.position - player.position).normalized;
        float approachSpeed = Vector3.Dot(playerRb.linearVelocity, toMe);
        return approachSpeed > 5f;
    }

    IEnumerator AtiumDodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        // Sidestep perpendicular to player's approach
        Vector3 dodgeDir = Vector3.Cross((player.position - transform.position).normalized, Vector3.up);
        if (Random.value > 0.5f) dodgeDir = -dodgeDir;

        Vector3 dodgeTarget = transform.position + dodgeDir * 4f;
        if (agent != null) agent.SetDestination(dodgeTarget);
        animator?.SetTrigger("Dodge");

        yield return new WaitForSecondsRealtime(atiumDodgeWindow);
        isDodging = false;
    }

    // ── Phase 3: All Burn ────────────────────────────────────────────────

    void UpdateAllBurn(float dist)
    {
        // Emotional pulse (Zinc Riot — disorient player)
        emotionalPulseTimer -= Time.deltaTime;
        if (emotionalPulseTimer <= 0f && dist < emotionalAuraRadius)
        {
            emotionalPulseTimer = 5f;
            EmotionalPulse();
        }

        // Coin barrage
        coinBarrageTimer -= Time.deltaTime;
        if (coinBarrageTimer <= 0f && dist > attackRange && dist < 20f)
        {
            coinBarrageTimer = 3f;
            CoinBarrage();
        }

        // Dodge with Atium
        if (!isDodging && ShouldDodge())
        {
            StartCoroutine(AtiumDodge());
        }

        // Aggressive melee
        if (dist <= attackRange)
        {
            PerformMeleeAttack();
        }
        else
        {
            agent?.SetDestination(player.position);
        }
    }

    void EmotionalPulse()
    {
        // Zinc Riot — slow player, shake camera
        BasicPlayerMove pm = player.GetComponent<BasicPlayerMove>();
        if (pm != null) pm.externalSpeedMultiplier = 0.6f;
        CameraShakeManager.Instance?.Shake(1f, 0.2f);

        // Reset after duration
        StartCoroutine(ResetEmotionalEffect(3f));
    }

    IEnumerator ResetEmotionalEffect(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        BasicPlayerMove pm = player.GetComponent<BasicPlayerMove>();
        if (pm != null) pm.externalSpeedMultiplier = 1f;
    }

    void CoinBarrage()
    {
        if (coinPrefab == null) return;

        // Push 5 coins in a spread pattern
        Vector3 dir = (player.position - transform.position).normalized;
        for (int i = 0; i < 5; i++)
        {
            float angle = (i - 2) * 10f;
            Vector3 spreadDir = Quaternion.Euler(0, angle, 0) * dir;
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f + spreadDir;

            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            Rigidbody rb = coin.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(spreadDir * steelPushForce, ForceMode.Impulse);
            Destroy(coin, 5f);
        }
    }

    // ── Attacks ──────────────────────────────────────────────────────────

    void PerformMeleeAttack()
    {
        if (player == null) return;
        if (agent != null) agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (Time.time - lastAttackTime < 1f) return;
        lastAttackTime = Time.time;

        float damage = baseDamage * pewterStrengthMult;
        if (currentPhase == BossPhase.AllBurn)
            damage *= allBurnDamageMultiplier;

        animator?.SetTrigger("Attack");
        IDamageable target = player.GetComponent<IDamageable>();
        target?.TakeDamage(damage);
    }

    void PerformSteelPush()
    {
        lastPushTime = Time.time;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            playerRb.AddForce(dir * steelPushForce, ForceMode.Impulse);
        }
    }

    void PerformIronPull()
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            playerRb.AddForce(dir * ironPullForce, ForceMode.Impulse);
        }
    }

    // ── Linchpin Spike ───────────────────────────────────────────────────

    /// <summary>
    /// Stun the Inquisitor (e.g., from a powerful hit or Duralumin burst).
    /// Exposes the linchpin spike for removal.
    /// </summary>
    public void Stun()
    {
        if (isStunned || isDead) return;
        StartCoroutine(StunSequence());
    }

    IEnumerator StunSequence()
    {
        isStunned = true;
        linchpinExposed = true;
        if (agent != null) agent.isStopped = true;
        animator?.SetBool("IsStunned", true);


        yield return new WaitForSecondsRealtime(stunDuration);

        isStunned = false;
        linchpinExposed = false;
        if (agent != null) agent.isStopped = false;
        animator?.SetBool("IsStunned", false);
    }

    /// <summary>
    /// Remove the linchpin spike. Instant kill if exposed.
    /// </summary>
    public bool TryRemoveLinchpin(Transform playerTransform)
    {
        if (!linchpinExposed || isDead) return false;
        if (Vector3.Distance(transform.position, playerTransform.position) > spikeRemovalRange) return false;

        currentHealth = 0;
        if (healthComponent != null) healthComponent.TakeDamage(healthComponent.GetMaxHealth());
        Die();
        return true;
    }

    public bool CanRemoveLinchpin(Transform playerTransform)
    {
        return linchpinExposed && !isDead &&
               Vector3.Distance(transform.position, playerTransform.position) <= spikeRemovalRange;
    }

    // ── Death ────────────────────────────────────────────────────────────

    void Die()
    {
        if (isDead) return;
        isDead = true;
        currentPhase = BossPhase.Dead;

        if (agent != null) agent.isStopped = true;
        animator?.SetBool("IsDead", true);

        CameraShakeManager.Instance?.Shake(1f, 0.5f);
        EventManager.TriggerEvent("InquisitorDefeated");

        Destroy(gameObject, 5f);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = agent != null ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }

    // ── Public API ───────────────────────────────────────────────────────
    public BossPhase GetPhase() => currentPhase;
    public bool IsStunned() => isStunned;
    public float GetHealthPercent() => currentHealth / maxHealth;
}
