using UnityEngine;

/// <summary>
/// Implements the Pewter Allomancy ability (enhanced physical capabilities).
/// </summary>
public class Pewter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Base metal burn rate per second")]
    public float metalCostPerSecond = 1f;
    [Tooltip("Multiplier for player mass when burning Pewter (affects push/pull strength)")]
    public float massMultiplier = 1.5f;
    [Tooltip("Multiplier for movement speed when burning Pewter")]
    public float speedMultiplier = 1.2f;
    [Tooltip("Cooldown time in seconds after stopping burn")]
    public float burnCooldown = 0.1f;
    
    [Header("References")]
    public Allomancer allomancer;
    public BasicPlayerMove playerMove; // Reference to the player movement script
    
    private bool isBurning = false;
    private float cooldownTimer = 0f;
    private float originalMass;
    private float originalSpeed;
    
    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponentInParent<Allomancer>();
        
        // Get the player movement script (assuming it's on the same GameObject or parent)
        if (playerMove == null)
            playerMove = GetComponentInParent<BasicPlayerMove>();
        
        if (playerMove != null)
        {
            originalMass = playerMove.GetComponent<Rigidbody>().mass;
            originalSpeed = playerMove.moveSpeed;
        }
    }
    
    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        
        // Check if we can burn pewter
        if (allomancer != null && !allomancer.canBurnMetal)
        {
            if (isBurning) StopBurning();
            return;
        }
        
        // P key to burn Pewter (as per common Allomancy key bindings)
        if (Input.GetKeyDown(KeyCode.P) && cooldownTimer <= 0f)
        {
            if (!isBurning) StartBurning();
        }
        
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (isBurning) StopBurning();
        }
        
        // Continuous metal drain while burning
        if (isBurning)
        {
            DrainMetal();
        }
    }
    
    void StartBurning()
    {
        if (isBurning) return;
        isBurning = true;
        cooldownTimer = burnCooldown;
        allomancer.StartBurning(AllomancySkill.MetalType.Pewter);
        ApplyPewterEffects();
    }
    
    void StopBurning()
    {
        if (!isBurning) return;
        isBurning = false;
        cooldownTimer = burnCooldown;
        allomancer.StopBurning();
        ResetPewterEffects();
    }
    
    void ApplyPewterEffects()
    {
        // Increase player mass for stronger push/pull
        if (playerMove != null)
        {
            Rigidbody rb = playerMove.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = originalMass * massMultiplier;
            }
            
            // Increase movement speed
            playerMove.moveSpeed = originalSpeed * speedMultiplier;
        }
    }
    
    void ResetPewterEffects()
    {
        // Reset player mass
        if (playerMove != null)
        {
            Rigidbody rb = playerMove.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = originalMass;
            }
            
            // Reset movement speed
            playerMove.moveSpeed = originalSpeed;
        }
    }
    
    void DrainMetal()
    {
        if (allomancer == null) return;
        allomancer.DrainMetal(AllomancySkill.MetalType.Pewter, metalCostPerSecond * Time.deltaTime);
    }
}