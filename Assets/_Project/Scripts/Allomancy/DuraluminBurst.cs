/// <summary>
/// Duralumin Burst - Massive temporary power boost.
/// Usage: DuraluminBurst duralumin = GetComponent<DuraluminBurst>();
/// </summary>
public class DuraluminBurst : MonoBehaviour
{
    // SETTINGS
    public float burstCost = 50f;            // Large metal cost
    public float burstMultiplier = 5f;       // Power multiplier
    public float burstDuration = 2f;         // How long burst lasts
    
    // STATE
    private bool isBursting = false;
    
    // EVENTS
    public System.Action OnBurstStart;
    public System.Action OnBurstEnd;
    
    // PUBLIC API
    public bool IsBursting => isBursting;
    
    void Update()
    {
        // Press R to burst
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryBurst();
        }
    }
    
    public void TryBurst()
    {
        if (isBursting) return;
        
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        
        if (metals != null && metals.GetReserve(MetalType.Duralumin) >= burstCost)
        {
            metals.UseMetal(MetalType.Duralumin, burstCost);
            PerformBurst();
        }
    }
    
    void PerformBurst()
    {
        isBursting = true;
        Debug.Log("Duralumin Burst!");
        OnBurstStart?.Invoke();
        
        // Boost all Allomantic forces
        SteelPush steel = GetComponent<SteelPush>();
        if (steel != null)
        {
            // Would need a public force multiplier
        }
        
        Invoke(nameof(EndBurst), burstDuration);
    }
    
    void EndBurst()
    {
        isBursting = false;
        Debug.Log("Burst ended");
        OnBurstEnd?.Invoke();
    }
}
