using UnityEngine;

public class AluminumPurge : MonoBehaviour
{
    [Header("Settings")]
    public float purgeCost = 20f;
    public bool purgeOnActivation = true;
    
    private float metalReserve = 100f; // Note: This is singular
    
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
        metalReserve -= purgeCost; // Line 41: Subtracting cost from local aluminum
        
        Allomancer allomancer = GetComponent<Allomancer>();
        if (allomancer != null)
        {
            // Drain existing allomancy skills
            for (int i = 0; i < 16; i++)
            {
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel));
            }
        }
        
        MetalReserveManager manager = GetComponent<MetalReserveManager>();
        if (manager != null)
        {
            // This calls the new function we added to MetalReserveManager
            manager.PurgeAll(); 
        }
        
        Debug.Log("Aluminum Purged - All metal reserves emptied!");
    }
    
    public float GetMetalReserve() => metalReserve;
    public void Refill(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
