using UnityEngine;

/// <summary>
/// Implements the Pewter Allomancy ability (enhanced physical capabilities).
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
[PlayerComponent("Allomancy Metals", order: 30)]
public class Pewter : MonoBehaviour
{
    [Header("Pewter Physics — PHYSICS-MATH-BOOK.md Section 8")]
    [Tooltip("Pewter efficiency constant k: S = S_base × (1 + k × P)")]
    public float pewterEfficiencyK = 2f;
    [Tooltip("Muscle growth constant α: m = m_base × (1 + α × P), handbook α≈0.5")]
    public float muscleGrowthAlpha = 0.5f;
    [Tooltip("Max strength multiplier cap (prevents physics instability)")]
    public float maxStrengthMultiplier = 4f;
    [Tooltip("Max speed multiplier cap")]
    public float maxSpeedMultiplier = 2f;

    [Header("References")]
    public Allomancer allomancer;
    public BasicPlayerMove playerMove; 
    
    private bool isBurning = false;
    private float originalMass;
    private float originalSpeed;
    private Rigidbody playerRigidbody;
    
    [Header("Pewter Mend")]
    [Tooltip("Health restored per second while burning Pewter")]
    public float baseHealRate = 0.5f;

    private HealthBarTransitions healthSystem;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        
        if (playerMove == null)
            playerMove = GetComponentInParent<BasicPlayerMove>();
        
        healthSystem = GetComponentInParent<HealthBarTransitions>();

        if (playerMove != null)
        {
            playerRigidbody = playerMove.GetComponent<Rigidbody>();
            if (playerRigidbody != null) originalMass = playerRigidbody.mass;
            originalSpeed = playerMove.moveSpeed;
        }
    }
    
    void Update()
    {
        bool wasBurning = isBurning;
        MetalSelector sel = allomancer?.GetComponent<MetalSelector>();
        bool pewterEquipped = sel == null
            || sel.GetPrimaryMetal()   == AllomancySkill.MetalType.Pewter
            || sel.GetSecondaryMetal() == AllomancySkill.MetalType.Pewter;
        isBurning = allomancer != null
                 && FlareManager.Instance != null && FlareManager.Instance.IsBurning
                 && pewterEquipped
                 && allomancer.GetMetalReserve(AllomancySkill.MetalType.Pewter) > 0;

        if (isBurning)
        {
            // Use the unified FlareMultiplier from FlareManager (includes Duralumin 10x / Nicro 3x boosts)
            float flareMult = (FlareManager.Instance != null) ? FlareManager.Instance.FlareMultiplier : 1.0f;
            
            ApplyPewterEffects(flareMult);
            HandleHealing(flareMult);
        }
        else if (wasBurning)
        {
            ResetPewterEffects();
        }
    }
    
    private void HandleHealing(float flareMult)
    {
        if (healthSystem != null && healthSystem.GetCurrentHealth() < healthSystem.GetMaxHealth())
        {
            healthSystem.Heal(baseHealRate * flareMult * Time.deltaTime);
        }
    }

    // Handbook formula: S_pewter = S_base × (1 + k × P)
    // Where P = power level 0-1, scaled by flare multiplier
    // Muscle mass: m_muscle = m_base × (1 + α × P)
    void ApplyPewterEffects(float flareMult)
    {
        if (playerMove == null) return;

        // P is normalized power level: 1.0 at base burn, higher when flaring
        float P = Mathf.Clamp01(flareMult / 2.5f); // Normalize to 0-1 range

        // Strength/speed: S = S_base × (1 + k × P)
        float strengthMult = Mathf.Min(1f + pewterEfficiencyK * P, maxStrengthMultiplier);
        float speedMult    = Mathf.Min(1f + pewterEfficiencyK * P * 0.5f, maxSpeedMultiplier);

        // Muscle mass: m = m_base × (1 + α × P)
        float massMult = 1f + muscleGrowthAlpha * P;

        if (playerRigidbody != null)
            playerRigidbody.mass = originalMass * massMult;

        playerMove.moveSpeed = originalSpeed * speedMult;
    }
    
    void ResetPewterEffects()
    {
        if (playerMove == null) return;

        if (playerRigidbody != null)
        {
            playerRigidbody.mass = originalMass;
        }
        
        playerMove.moveSpeed = originalSpeed;
    }
}
