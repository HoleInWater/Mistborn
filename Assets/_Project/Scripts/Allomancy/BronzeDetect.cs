/// <summary>
/// Bronze Detect - Detect burning Allomancers nearby.
/// Usage: BronzeDetect bronze = GetComponent<BronzeDetect>();
/// </summary>
public class BronzeDetect : MonoBehaviour
{
    // SETTINGS
    public float detectionRange = 50f;        // How far to detect
    public float metalCostPerSecond = 2f;    // Drain rate
    public float pulseInterval = 0.5f;       // How often to check
    
    // STATE
    private bool isBurning = false;
    private float lastPulseTime;
    private System.Collections.Generic.List<GameObject> detectedAllomancers;
    
    // EVENTS
    public System.Action OnAllomancerDetected;
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    public System.Collections.Generic.List<GameObject> DetectedAllomancers => detectedAllomancers;
    
    void Start()
    {
        detectedAllomancers = new System.Collections.Generic.List<GameObject>();
    }
    
    void Update()
    {
        // Press V to burn bronze
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (isBurning)
                StopBurning();
            else
                StartBurning();
        }
        
        if (isBurning)
        {
            DrainMetal();
            DetectPulses();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Bronze!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        detectedAllomancers.Clear();
        Debug.Log("Stopped Bronze");
        OnBurnEnd?.Invoke();
    }
    
    void DetectPulses()
    {
        if (Time.time - lastPulseTime < pulseInterval)
            return;
        
        lastPulseTime = Time.time;
        detectedAllomancers.Clear();
        
        // Find all Allomancer components in range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        
        foreach (Collider hit in hits)
        {
            Allomancer allomancer = hit.GetComponent<Allomancer>();
            if (allomancer != null && allomancer.IsBurning())
            {
                detectedAllomancers.Add(hit.gameObject);
                Debug.Log($"Detected Allomancer: {hit.name} burning {allomancer.GetCurrentMetal()}");
                OnAllomancerDetected?.Invoke();
            }
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Bronze, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
}
