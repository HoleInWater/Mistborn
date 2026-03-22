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
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel));
            }
        }
        
        MetalReserveManager manager = GetComponent<MetalReserveManager>();
        if (manager != null)
        {
            manager.PurgeAll();
        }
        
        Debug.Log("Aluminum Purged - All metal reserves emptied!");
    }

        public void PurgeAll()
    {
        // If you use an array or list to store reserves, loop through and zero them out
        // Example (replace 'reserves' with your actual variable name):
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = 0f;
        }
        
        // If you use a Dictionary or individual variables, reset them here
        Debug.Log("All metals have been purged from the manager.");
    }
    
    public float GetMetalReserve() => metalReserve;
    public void Refill(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
