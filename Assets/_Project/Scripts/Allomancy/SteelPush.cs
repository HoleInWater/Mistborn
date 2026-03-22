/// <summary>
/// Steel Push - Push metals away from the Allomancer.
/// Usage: SteelPush steel = GetComponent<SteelPush>();
/// 
/// METHODS:
///   steel.Push()   // Push all nearby metals
/// </summary>
public class SteelPush : MonoBehaviour
{
    // SETTINGS
    public float pushForce = 500f;            // Force applied
    public float maxRange = 50f;              // Maximum range
    public float metalCostPerSecond = 2f;     // Drain rate
    public LayerMask metalLayer;              // What can be pushed
    
    // STATE
    private bool isBurning = false;
    private Camera playerCamera;
    
    // EVENTS
    public System.Action OnPush;
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
        // Right mouse button to push
        if (Input.GetMouseButtonDown(1))
        {
            StartBurning();
        }
        
        if (Input.GetMouseButton(1) && isBurning)
        {
            PushMetals();
            DrainMetal();
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            StopBurning();
        }
    }
    
    void StartBurning()
    {
        isBurning = true;
        Debug.Log("Burning Steel!");
        OnBurnStart?.Invoke();
    }
    
    void StopBurning()
    {
        isBurning = false;
        Debug.Log("Stopped Steel");
        OnBurnEnd?.Invoke();
    }
    
    void PushMetals()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, metalLayer);
        
        foreach (RaycastHit hit in hits)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Direction from player to target
                Vector3 pushDir = (hit.point - transform.position).normalized;
                rb.AddForce(pushDir * pushForce * Time.deltaTime, ForceMode.Impulse);
            }
        }
        
        if (hits.Length > 0)
        {
            OnPush?.Invoke();
        }
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals != null)
        {
            if (!metals.UseMetal(MetalType.Steel, metalCostPerSecond * Time.deltaTime))
            {
                StopBurning();
            }
        }
    }
}
