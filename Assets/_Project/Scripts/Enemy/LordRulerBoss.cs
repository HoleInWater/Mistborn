using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Final Boss AI for the Lord Ruler. A multi-phase god-tier encounter.
/// Uses Compounding (Gold for regeneration, Steel for speed) to be nearly invincible.
/// </summary>
public class LordRulerBoss : MonoBehaviour
{
    public enum BossPhase { Physical, Spiritual, Godlike }
    public BossPhase currentPhase = BossPhase.Physical;

    [Header("Stats")]
    public float health = 1000f;
    public float maxHealth = 1000f;
    public bool atiumReflexes = true;

    [Header("Compounding")]
    [Tooltip("Gold compounding heal per second (base, scaled by compounding multiplier).")]
    public float goldCompoundingHealRate = 15f;
    [Tooltip("Steel compounding speed multiplier (base, scaled by compounding multiplier).")]
    public float steelCompoundingSpeedScale = 0.5f;
    public bool compoundingDisabled = false;

    // Feruchemist metal indices for Lord Ruler's key compounding metals
    private const int GoldIndex = 8;   // Health
    private const int SteelIndex = 0;  // Speed
    private const int PewterIndex = 2; // Strength

    private NavMeshAgent agent;
    private Transform player;
    private Rigidbody playerRigidbody;
    private bool isExecutingBurst = false;
    private float originalAgentSpeed;

    // Compounding references — Lord Ruler has both Allomancy and Feruchemy
    private Allomancer allomancer;
    private Feruchemist feruchemist;
    private Compounding compounding;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) originalAgentSpeed = agent.speed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
        }
        else Debug.LogWarning("[LordRulerBoss] No 'Player' tag found in scene.");

        // Initialize Lord Ruler's Metallic Arts components
        InitializeCompounding();
    }

    private void InitializeCompounding()
    {
        allomancer = GetComponent<Allomancer>();
        feruchemist = GetComponent<Feruchemist>();
        compounding = GetComponent<Compounding>();

        if (allomancer == null) allomancer = gameObject.AddComponent<Allomancer>();
        if (feruchemist == null) feruchemist = gameObject.AddComponent<Feruchemist>();
        if (compounding == null) compounding = gameObject.AddComponent<Compounding>();

        // Pre-charge key metalminds — the Lord Ruler has had 1000 years to store
        compounding.PreChargeMetalmind(GoldIndex, 700f);   // Health — immortality source
        compounding.PreChargeMetalmind(SteelIndex, 600f);  // Speed
        compounding.PreChargeMetalmind(PewterIndex, 600f); // Strength
    }

    void Update()
    {
        if (player == null) return;

        UpdatePhase();
        ProcessCompounding();
        ExecuteAIBehavior();
    }

    private void UpdatePhase()
    {
        if (health < 300f) currentPhase = BossPhase.Godlike;
        else if (health < 700f) currentPhase = BossPhase.Spiritual;
    }

    /// <summary>
    /// Lord Ruler's compounding behavior — Gold for regen, Steel for speed in later phases.
    /// </summary>
    private void ProcessCompounding()
    {
        if (compounding == null || compoundingDisabled) return;

        // Gold compounding — always active (source of immortality)
        if (health < maxHealth)
        {
            compounding.ForceStartCompounding(GoldIndex);

            if (compounding.IsCompounding(GoldIndex))
            {
                float healMultiplier = compounding.GetOutputMultiplier(GoldIndex);
                float healAmount = goldCompoundingHealRate * healMultiplier * Time.deltaTime;
                health = Mathf.Min(maxHealth, health + healAmount);
            }
        }

        // Steel compounding in Spiritual+ phases — supernatural speed
        if (currentPhase >= BossPhase.Spiritual)
        {
            compounding.ForceStartCompounding(SteelIndex);

            if (compounding.IsCompounding(SteelIndex) && agent != null)
            {
                float speedMultiplier = compounding.GetOutputMultiplier(SteelIndex);
                agent.speed = originalAgentSpeed * (1f + speedMultiplier * steelCompoundingSpeedScale);
            }
        }
        else
        {
            compounding.ForceStopCompounding(SteelIndex);
            if (agent != null) agent.speed = originalAgentSpeed;
        }

        // Pewter compounding in Godlike phase — overwhelming strength
        if (currentPhase == BossPhase.Godlike)
        {
            compounding.ForceStartCompounding(PewterIndex);
        }
    }

    private void ExecuteAIBehavior()
    {
        switch (currentPhase)
        {
            case BossPhase.Physical:
                // Heavy Steel/Pewter aggression
                if (agent != null) agent.SetDestination(player.position);
                if (Random.value < 0.05f) PerformSteelPush();
                break;
            case BossPhase.Spiritual:
                // Zinc/Brass aura manipulation + speed compounding
                if (agent != null) agent.SetDestination(player.position);
                break;
            case BossPhase.Godlike:
                // Simultaneous 16-metal flare + full compounding
                if (agent != null) agent.SetDestination(player.position);
                if (!isExecutingBurst) StartCoroutine(PerformOmniBurst());
                break;
        }
    }

    private void PerformSteelPush()
    {
        if (playerRigidbody == null) return;
        Vector3 dir = player.position - transform.position;
        playerRigidbody.AddForce(dir.normalized * 200f, ForceMode.Impulse);
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
        // Compounding resistance — Gold compounding provides damage reduction
        float resistance = 0.1f;
        if (compounding != null && compounding.IsCompounding(GoldIndex) && !compoundingDisabled)
        {
            // Even more resistant while actively Gold compounding
            resistance *= 0.5f;
        }

        health -= amount * resistance;
        if (health <= 0) Die();
    }

    /// <summary>
    /// Disable the Lord Ruler's compounding — the key to defeating him.
    /// Lore: Removing his metalminds stops his Compounding, making him mortal.
    /// </summary>
    public void DisableCompounding()
    {
        compoundingDisabled = true;

        if (compounding != null)
        {
            for (int i = 0; i < Feruchemist.MetalmindCount; i++)
            {
                compounding.ForceStopCompounding(i);
            }
        }

        Debug.Log("[LORD RULER] METALMINDS REMOVED — Compounding disabled! He is mortal!");
    }

    private void Die()
    {
        Debug.Log("[BOSS] THE LORD RULER HAS FALLEN. THE ASH STOPS.");
        // End Game logic
    }
}
