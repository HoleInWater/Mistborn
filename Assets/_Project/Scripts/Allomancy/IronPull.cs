/// <summary>
/// Iron Pull - Pull metals toward the Allomancer.
/// Usage: IronPull iron = GetComponent<IronPull>();
/// 
/// METHODS:
///   iron.Pull()   // Pull all nearby metals
/// </summary>
public class IronPull : MonoBehaviour
{
    // SETTINGS
    public float pullForce = 500f;            // Force applied
    public float maxRange = 50f;               // Maximum range
    public float metalCostPerSecond = 2f;      // Drain rate
    public LayerMask metalLayer;               // What can be pulled
    
    // STATE
    private bool isBurning = false;
    private Camera playerCamera;
    
    // EVENTS
    public System.Action OnPull;
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }
    
    void Update()
    {
        // Left mouse button to pull
        if (Input.GetMouseButtonDown(0))
        {
            StartBurning();
        }
        
        if (Input.GetMouseButton(0) && isBurning)
        {
            PullMetals();
            DrainMetal();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Iron!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped Iron");
        OnBurnEnd?.Invoke();
    }
    
    void PullMetals()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, metalLayer);
        
        foreach (RaycastHit hit in hits)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Direction from target to player
                Vector3 pullDir = (transform.position - hit.point).normalized;
                rb.AddForce(pullDir * pullForce * Time.deltaTime, ForceMode.Impulse);
            }
        }
        
        if (hits.Length > 0)
        {
            OnPull?.Invoke();
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Iron, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
}
