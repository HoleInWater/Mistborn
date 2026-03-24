using UnityEngine;

/// <summary>
/// Implements the Pewter Allomancy ability (enhanced physical capabilities).
/// Standardized to follow the Allomancer-centric burn system.
/// </summary>
public class Pewter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Multiplier for player mass when burning Pewter")]
    public float massMultiplierBase = 1.5f;
    [Tooltip("Multiplier for movement speed when burning Pewter")]
    public float speedMultiplierBase = 1.2f;
    
    [Header("Flare Boosts")]
    [Tooltip("Extra mass multiplier when flaring")]
    public float massMultiplierMax = 2.5f;
    [Tooltip("Extra speed multiplier when flaring")]
    public float speedMultiplierMax = 1.6f;

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

    private PlayerHealth healthSystem;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        
        if (playerMove == null)
            playerMove = GetComponentInParent<BasicPlayerMove>();
        
        healthSystem = GetComponentInParent<PlayerHealth>();

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
        // Check if we are currently burning Pewter according to the central Allomancer
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Pewter;

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

    // Pewter effects scale relative to the base multipliers (1.5x mass, 1.2x speed)
    // Multiplied by the flare multiplier which is 1.0 at base and 10.0 during burst.
    void ApplyPewterEffects(float flareMult)
    {
        if (playerMove == null) return;

        // Scale factors: flareMult is 1.0 to max (e.g. 10.0 for Duralumin).
        // Clamped to inspector maximums to prevent physics instability.
        float currentMassMult  = Mathf.Clamp(massMultiplierBase  * flareMult, 1f, massMultiplierMax);
        float currentSpeedMult = Mathf.Clamp(speedMultiplierBase * flareMult, 1f, speedMultiplierMax);

        // Apply Mass
        if (playerRigidbody != null)
            playerRigidbody.mass = originalMass * currentMassMult;

        // Apply Move Speed
        playerMove.moveSpeed = originalSpeed * currentSpeedMult;
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