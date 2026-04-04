using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Final Boss: The Ashen King — Varek, the Sliver of Infinity.
/// Four-phase fight with Compounding, emotional Metallurgy, Oraculum, and time dilation.
/// Phase 1: Physical (Pewter + Steel pushes)
/// Phase 2: Emotional (Zinc Riots / Brass Soothes the player, screen effects)
/// Phase 3: Compounding (Gold regen, virtually unkillable without removing metalminds)
/// Phase 4: Oraculum Ascension (time dilation, predicted player movements)
/// Weakness: Removing metalminds disables Compounding, making him mortal.
/// </summary>
public class AshenKingBoss : MonoBehaviour, IDamageable
{
    public enum BossPhase { Physical, Emotional, Compounding, OraculumAscension, Defeated }

    [Header("Phase Thresholds (HP %)")]
    public float phase2Threshold = 0.75f;
    public float phase3Threshold = 0.50f;
    public float phase4Threshold = 0.25f;

    [Header("Stats")]
    public float maxHealth = 1000f;
    public float health = 1000f;
    public float baseDamage = 50f;
    public float attackRange = 3f;
    public float detectionRange = 50f;
    public float baseSpeed = 6f;

    [Header("Metallurgic Powers")]
    public float steelPushForce = 200f;
    public float ironPullForce = 150f;
    public float pewterStrengthMult = 3f;

    [Header("Emotional Metallurgy (Phase 2)")]
    public float emotionalPulseRadius = 15f;
    public float emotionalPulseCooldown = 6f;
    public float sootheDuration = 4f;
    public float riotDuration = 3f;

    [Header("Compounding (Phase 3)")]
    public float goldCompoundingHealRate = 20f;
    public float steelCompoundingSpeedScale = 0.5f;
    public float damageResistance = 0.1f;

    [Header("Oraculum Ascension (Phase 4)")]
    public float oraculumTimeDilation = 0.4f;
    public float oraculumDodgeChance = 0.7f;
    public float oraculumBurstDamage = 100f;
    public float oraculumBurstRadius = 8f;

    [Header("Metalmind Removal")]
    public float metalmindRemovalRange = 2f;
    public bool metalmindsExposed = false;
    public float stunDuration = 4f;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public GameObject coinPrefab;
    public GameObject shockwavePrefab;

    [Header("Cutscene Triggers")]
    public string phase2CutsceneEvent = "AshenKing_Phase2";
    public string phase4CutsceneEvent = "AshenKing_Phase4";
    public string defeatCutsceneEvent = "AshenKing_Defeated";

    // Internal
    private BossPhase currentPhase = BossPhase.Physical;
    private Transform player;
    private Rigidbody playerRb;
    private bool compoundingDisabled = false;
    private bool isStunned = false;
    private bool isDead = false;
    private float lastAttackTime;
    private float lastPushTime;
    private float lastEmotionalPulse;
    private float lastOmniBurst;
    private bool isExecutingBurst = false;

    // Compounding
    private Metallurgist metallurgist;
    private Storecrafter storecrafter;
    private Compounding compounding;
    private const int GoldIndex = 8;
    private const int SteelIndex = 0;
    private const int PewterIndex = 2;

    public System.Action<BossPhase> OnPhaseChanged;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = animator != null ? animator : GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody>();
        }

        InitializeCompounding();
    }

    void InitializeCompounding()
    {
        metallurgist = GetComponent<Metallurgist>();
        storecrafter = GetComponent<Storecrafter>();
        compounding = GetComponent<Compounding>();

        if (metallurgist == null) metallurgist = gameObject.AddComponent<Metallurgist>();
        if (storecrafter == null) storecrafter = gameObject.AddComponent<Storecrafter>();
        if (compounding == null) compounding = gameObject.AddComponent<Compounding>();

        // 1000 years of stored attributes
        compounding.PreChargeMetalmind(GoldIndex, 900f);
        compounding.PreChargeMetalmind(SteelIndex, 700f);
        compounding.PreChargeMetalmind(PewterIndex, 700f);
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isStunned) return;

        UpdatePhase();
        ProcessCompounding();

        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentPhase)
        {
            case BossPhase.Physical:    UpdatePhysical(dist); break;
            case BossPhase.Emotional:   UpdateEmotional(dist); break;
            case BossPhase.Compounding: UpdateCompoundingPhase(dist); break;
            case BossPhase.OraculumAscension: UpdateOraculumAscension(dist); break;
        }

        if (animator != null)
        {
            float speed = agent != null ? agent.velocity.magnitude : 0f;
            animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
            animator.SetInteger("BossPhase", (int)currentPhase);
        }
    }

    // ── Phase Management ─────────────────────────────────────────────────

    void UpdatePhase()
    {
        if (maxHealth <= 0f) return;
        float hpPercent = health / maxHealth;
        BossPhase newPhase;

        if (hpPercent <= phase4Threshold)
            newPhase = BossPhase.OraculumAscension;
        else if (hpPercent <= phase3Threshold)
            newPhase = BossPhase.Compounding;
        else if (hpPercent <= phase2Threshold)
            newPhase = BossPhase.Emotional;
        else
            newPhase = BossPhase.Physical;

        if (newPhase != currentPhase)
            TransitionPhase(newPhase);
    }

    void TransitionPhase(BossPhase newPhase)
    {
        BossPhase oldPhase = currentPhase;
        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        CameraShakeManager.Instance?.Shake(0.8f, 0.4f);

        switch (newPhase)
        {
            case BossPhase.Emotional:
                EventManager.TriggerEvent(phase2CutsceneEvent);
                break;

            case BossPhase.Compounding:
                if (agent != null) agent.speed = baseSpeed * 1.5f;
                break;

            case BossPhase.OraculumAscension:
                EventManager.TriggerEvent(phase4CutsceneEvent);
                Time.timeScale = oraculumTimeDilation;
                if (agent != null) agent.speed = baseSpeed * 2.5f;
                break;
        }
    }

    // ── Phase 1: Physical ────────────────────────────────────────────────

    void UpdatePhysical(float dist)
    {
        if (dist > attackRange)
        {
            agent?.SetDestination(player.position);

            if (Time.time - lastPushTime > 3f && dist < 15f && Random.value < 0.05f)
            {
                SteelPush();
            }
        }
        else
        {
            MeleeAttack(baseDamage * pewterStrengthMult);
        }
    }

    // ── Phase 2: Emotional ───────────────────────────────────────────────

    void UpdateEmotional(float dist)
    {
        // Alternate between Soothe and Riot
        if (Time.time - lastEmotionalPulse > emotionalPulseCooldown && dist < emotionalPulseRadius)
        {
            lastEmotionalPulse = Time.time;
            if (Random.value > 0.5f)
                SoothePlayer();
            else
                RiotPlayer();
        }

        // Still fights physically
        if (dist > attackRange)
            agent?.SetDestination(player.position);
        else
            MeleeAttack(baseDamage * pewterStrengthMult);

        // Occasional steel push for space
        if (dist < 5f && Time.time - lastPushTime > 4f)
            SteelPush();
    }

    void SoothePlayer()
    {
        // Brass Soothe — suppress player's aggression, slow them
        BasicPlayerMove pm = player.GetComponent<BasicPlayerMove>();
        if (pm != null) pm.externalSpeedMultiplier = 0.5f;

        // Screen effect — blue/cold tint
        CameraShakeManager.Instance?.Shake(0.3f, 0.1f);

        StartCoroutine(ResetPlayerSpeed(sootheDuration));
    }

    void RiotPlayer()
    {
        // Zinc Riot — inflame fear, camera shake and disorientation
        CameraShakeManager.Instance?.Shake(riotDuration, 0.25f);

        StartCoroutine(ResetPlayerSpeed(riotDuration));
    }

    IEnumerator ResetPlayerSpeed(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        BasicPlayerMove pm = player.GetComponent<BasicPlayerMove>();
        if (pm != null) pm.externalSpeedMultiplier = 1f;
    }

    // ── Phase 3: Compounding ─────────────────────────────────────────────

    void UpdateCompoundingPhase(float dist)
    {
        // Extremely aggressive — heals faster than player can damage
        if (dist > attackRange)
        {
            agent?.SetDestination(player.position);
            if (agent != null) agent.speed = baseSpeed * 1.8f;
        }
        else
        {
            MeleeAttack(baseDamage * pewterStrengthMult * 1.5f);
        }

        // Steel push barrage
        if (dist < 20f && Time.time - lastPushTime > 2f && Random.value < 0.08f)
            SteelPush();
    }

    // ── Phase 4: Oraculum Ascension ─────────────────────────────────────────

    void UpdateOraculumAscension(float dist)
    {
        // Oraculum future sight — dodge most attacks
        // Omni-burst periodically
        if (!isExecutingBurst && Time.time - lastOmniBurst > 10f)
        {
            StartCoroutine(OmniBurst());
        }

        if (dist > attackRange)
        {
            agent?.SetDestination(player.position);
        }
        else
        {
            // Devastating melee with all compounding active
            MeleeAttack(baseDamage * pewterStrengthMult * 2f);
        }
    }

    IEnumerator OmniBurst()
    {
        isExecutingBurst = true;
        lastOmniBurst = Time.time;

        animator?.SetTrigger("OmniBurst");

        yield return new WaitForSecondsRealtime(1f);

        // AOE shockwave
        Collider[] hits = Physics.OverlapSphere(transform.position, oraculumBurstRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            target?.TakeDamage(oraculumBurstDamage);

            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized + Vector3.up;
                rb.AddForce(dir * 50f, ForceMode.Impulse);
            }
        }

        CameraShakeManager.Instance?.Shake(1f, 0.5f);
        if (shockwavePrefab != null)
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSecondsRealtime(2f);
        isExecutingBurst = false;
    }

    // ── Compounding Processing ───────────────────────────────────────────

    void ProcessCompounding()
    {
        if (compounding == null || compoundingDisabled) return;

        // Gold compounding — always active for regeneration
        if (health < maxHealth)
        {
            compounding.ForceStartCompounding(GoldIndex);
            if (compounding.IsCompounding(GoldIndex))
            {
                float mult = compounding.GetOutputMultiplier(GoldIndex);
                health = Mathf.Min(maxHealth, health + goldCompoundingHealRate * mult * Time.deltaTime);
            }
        }

        // Steel compounding in phase 3+
        if (currentPhase >= BossPhase.Compounding)
        {
            compounding.ForceStartCompounding(SteelIndex);
            if (compounding.IsCompounding(SteelIndex) && agent != null)
            {
                float speedMult = compounding.GetOutputMultiplier(SteelIndex);
                agent.speed = baseSpeed * (1f + speedMult * steelCompoundingSpeedScale);
            }
        }

        // Pewter compounding in phase 4
        if (currentPhase == BossPhase.OraculumAscension)
            compounding.ForceStartCompounding(PewterIndex);
    }

    // ── Attacks ──────────────────────────────────────────────────────────

    void MeleeAttack(float damage)
    {
        if (player == null) return;
        if (agent != null) agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (Time.time - lastAttackTime < 1.2f) return;
        lastAttackTime = Time.time;

        animator?.SetTrigger("Attack");
        IDamageable target = player.GetComponentInParent<IDamageable>();
        target?.TakeDamage(damage);

        if (playerRb != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            playerRb.AddForce(dir * 15f, ForceMode.Impulse);
        }
    }

    void SteelPush()
    {
        lastPushTime = Time.time;
        if (playerRb != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            playerRb.AddForce(dir * steelPushForce, ForceMode.Impulse);
        }
    }

    // ── Damage & Death ───────────────────────────────────────────────────

    public float GetMaxHealth()     => maxHealth;
    public float GetCurrentHealth() => health;

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // Phase 4 Oraculum dodge
        if (currentPhase == BossPhase.OraculumAscension && Random.value < oraculumDodgeChance)
        {
            return;
        }

        // Compounding resistance
        float resistance = compoundingDisabled ? 1f : damageResistance;
        if (compounding != null && compounding.IsCompounding(GoldIndex) && !compoundingDisabled)
            resistance *= 0.5f;

        health -= amount * resistance;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        currentPhase = BossPhase.Defeated;

        // Restore time scale if in Oraculum phase
        Time.timeScale = 1f;

        if (agent != null) agent.isStopped = true;
        animator?.SetBool("IsDead", true);

        EventManager.TriggerEvent(defeatCutsceneEvent);
        CameraShakeManager.Instance?.Shake(2f, 0.6f);

        Destroy(gameObject, 8f);
    }

    // ── Metalmind Removal (Key Mechanic) ─────────────────────────────────

    /// <summary>
    /// Stun the Ashen King (e.g., Duralumin-enhanced push or pulling off his bracers).
    /// Exposes metalminds for removal.
    /// </summary>
    public void StunAndExpose()
    {
        if (isStunned || isDead) return;
        StartCoroutine(StunSequence());
    }

    IEnumerator StunSequence()
    {
        isStunned = true;
        metalmindsExposed = true;
        if (agent != null) agent.isStopped = true;
        animator?.SetBool("IsStunned", true);

        yield return new WaitForSecondsRealtime(stunDuration);

        isStunned = false;
        metalmindsExposed = false;
        if (agent != null) agent.isStopped = false;
        animator?.SetBool("IsStunned", false);
    }

    /// <summary>
    /// Remove the Ashen King's metalminds. This disables Compounding and makes him mortal.
    /// Player must be in range and metalminds must be exposed (stunned).
    /// </summary>
    public bool TryRemoveMetalminds(Transform playerTransform)
    {
        if (!metalmindsExposed || isDead) return false;
        if (Vector3.Distance(transform.position, playerTransform.position) > metalmindRemovalRange) return false;

        DisableCompounding();
        return true;
    }

    public void DisableCompounding()
    {
        compoundingDisabled = true;
        damageResistance = 1f; // Full damage now

        if (compounding != null)
        {
            for (int i = 0; i < Storecrafter.MetalmindCount; i++)
                compounding.ForceStopCompounding(i);
        }

        // Restore normal time
        Time.timeScale = 1f;
        if (agent != null) agent.speed = baseSpeed * 0.7f; // Weakened

    }

    public bool CanRemoveMetalminds(Transform playerTransform)
    {
        return metalmindsExposed && !isDead &&
               Vector3.Distance(transform.position, playerTransform.position) <= metalmindRemovalRange;
    }

    // ── Public API ───────────────────────────────────────────────────────
    public BossPhase GetPhase() => currentPhase;
    public float GetHealthPercent() => maxHealth > 0f ? health / maxHealth : 0f;
    public bool IsCompoundingDisabled() => compoundingDisabled;
    public bool IsDead() => isDead;
}
