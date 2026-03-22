/// <summary>
/// Aluminum Purge - Empty all metal reserves instantly.
/// Usage: AluminumPurge aluminum = GetComponent<AluminumPurge>();
/// </summary>
public class AluminumPurge : MonoBehaviour
{
    // SETTINGS
    public float purgeCost = 25f;           // Metal cost (instant)
    
    // EVENTS
    public System.Action OnPurged;
    
    void Update()
    {
        // Press O to purge
        if (Input.GetKeyDown(KeyCode.O))
        {
            Purge();
        }
    }
    
    public void Purge()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        
        if (metals != null)
        {
            metals.PurgeAll();
            Debug.Log("Aluminum Purge - All metals emptied!");
            OnPurged?.Invoke();
        }
    }
}
