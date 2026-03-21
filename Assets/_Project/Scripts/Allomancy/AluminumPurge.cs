using UnityEngine;

public class AluminumPurge : MonoBehaviour
{
    [Header("Settings")]
    public float purgeCost = 20f;
    public bool purgeOnActivation = true;
    
    private float metalReserve = 100f;
    
    public void TryPurge()
    {
        if (metalReserve >= purgeCost)
        {
            PerformPurge();
        }
        else
        {
            Debug.Log("Not enough Aluminum to purge!");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPurge();
        }
    }
    
    void PerformPurge()
    {
        metalReserve -= purgeCost;
        
        Allomancer allomancer = GetComponent<Allomancer>();
        if (allomancer != null)
        {
            for (int i = 0; i < 16; i++)
            {
                allomancer.DrainMetal(MetalType.Steel, allomancer.GetMetalReserve(MetalType.Steel));
            }
        }
        
        MetalReserveManager manager = GetComponent<MetalReserveManager>();
        if (manager != null)
        {
            manager.PurgeAll();
        }
        
        Debug.Log("Aluminum Purged - All metal reserves emptied!");
    }
    
    public float GetMetalReserve() => metalReserve;
    public void Refill(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
