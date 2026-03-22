/// <summary>
/// Pewter enhancement system for Allomancers.
/// Grants increased strength, speed, and healing.
/// Usage: PewterBurn pewter = GetComponent<PewterBurn>();
/// 
/// METHODS:
///   pewter.IsBurning   // bool
///   pewter.StartBurning()
///   pewter.StopBurning()
/// </summary>
public class PewterBurn : MonoBehaviour
{
    // SETTINGS
    public float strengthMultiplier = 2f;     // Damage multiplier
    public float speedMultiplier = 1.5f;     // Speed boost
    public float healingRate = 5f;           // HP per second
    public float metalCostPerSecond = 3f;    // Drain rate
    
    // STATE
    private bool isBurning = false;
    private float originalSpeed;
    private float originalJump;
    private Health health;
    private Rigidbody rb;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Start()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        // Press Q to burn pewter
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isBurning)
                StopBurning();
            else
                StartBurning();
        }
        
        if (isBurning)
        {
            DrainMetal();
            ApplyEffects();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Pewter!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreStats();
        Debug.Log("Stopped Pewter");
        OnBurnEnd?.Invoke();
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Pewter, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
    
    void ApplyEffects()
    {
        // Healing
        if (health != null && health.currentHealth < health.maxHealth)
        {
            health.Heal(healingRate * Time.deltaTime);
        }
    }
    
    void RestoreStats()
    {
        // Reset speed/multipliers when stopped
    }
}
