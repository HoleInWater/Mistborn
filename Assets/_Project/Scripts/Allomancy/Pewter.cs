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
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        
        if (playerMove == null)
            playerMove = GetComponentInParent<BasicPlayerMove>();
        
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
            float flareMult = GetFlareMultiplier();
            ApplyPewterEffects(flareMult);
        }
        else if (wasBurning)
        {
            ResetPewterEffects();
        }
    }
    
    float GetFlareMultiplier()
    {
        if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
        {
            return FlareManager.Instance.flareIntensity;
        }
        return 1.0f;
    }

    void ApplyPewterEffects(float flareMult)
    {
        if (playerMove == null) return;

        // Scale multipliers based on flare intensity (1.0 to flareIntensity)
        float currentMassMult = Mathf.Lerp(massMultiplierBase, massMultiplierMax, (flareMult - 1f) / 1.5f);
        float currentSpeedMult = Mathf.Lerp(speedMultiplierBase, speedMultiplierMax, (flareMult - 1f) / 1.5f);

        // Apply Mass
        if (playerRigidbody != null)
        {
            playerRigidbody.mass = originalMass * currentMassMult;
        }
        
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