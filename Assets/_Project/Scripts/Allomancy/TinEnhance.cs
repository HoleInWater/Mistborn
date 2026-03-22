/// <summary>
/// Tin sensory enhancement.
/// Grants enhanced sight, hearing, and awareness.
/// Usage: TinEnhance tin = GetComponent<TinEnhance>();
/// 
/// METHODS:
///   tin.IsBurning   // bool
///   tin.StartBurning()
///   tin.StopBurning()
/// </summary>
public class TinEnhance : MonoBehaviour
{
    // SETTINGS
    public float sightMultiplier = 1.5f;     // FOV boost
    public float hearingRange = 50f;         // Hearing distance
    public float metalCostPerSecond = 2f;   // Drain rate
    
    // STATE
    private bool isBurning = false;
    private float originalFOV;
    private Camera playerCamera;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;
    }
    
    void Update()
    {
        // Press E to burn tin
        if (Input.GetKeyDown(KeyCode.E))
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
        Debug.Log("Burning Tin!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        RestoreSenses();
        Debug.Log("Stopped Tin");
        OnBurnEnd?.Invoke();
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Tin, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
    
    void ApplyEffects()
    {
        // Enhanced FOV
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV * sightMultiplier;
        }
    }
    
    void RestoreSenses()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
        }
    }
    
    void OnDestroy()
    {
        RestoreSenses();
    }
}
