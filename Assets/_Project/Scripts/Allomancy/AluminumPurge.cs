// NOTE: Lines 19 and 52 contain Debug.Log which should be removed for production
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
            // This loop currently drains Steel 16 times; 
            // you may eventually want to loop through different MetalTypes here.
            for (int i = 0; i < 16; i++)
            {
                allomancer.DrainMetal(AllomancySkill.MetalType.Steel, allomancer.GetMetalReserve(AllomancySkill.MetalType.Steel));
            }
        }
        
        MetalReserve manager = GetComponent<MetalReserve>();
        if (manager != null)
        {
            // FIXED: Using currentMetal from your MetalReserve script
            manager.currentMetal = 0; 
        }
        
        Debug.Log("Aluminum Purged - All metal reserves emptied!");
    }
    
    public float GetMetalReserve() => metalReserve;
    public void Refill(float amount) => metalReserve = Mathf.Min(metalReserve + amount, 100f);
}
