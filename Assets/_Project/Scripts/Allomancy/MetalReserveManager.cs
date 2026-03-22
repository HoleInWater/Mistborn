using UnityEngine;
using System.Collections.Generic;

public class MetalReserveManager : MonoBehaviour
{
    [Header("Settings")]
    public float maxReserve = 100f;
    public float passiveRecoveryRate = 0.5f;

    // We use a Dictionary so we can look up "Pewter" directly instead of guessing the ID number
    private Dictionary<AllomancySkill.MetalType, float> metalReserves = new Dictionary<AllomancySkill.MetalType, float>();

    void Awake()
    {
        // Initialize every metal type in the enum to be full (100)
        foreach (AllomancySkill.MetalType type in System.Enum.GetValues(typeof(AllomancySkill.MetalType)))
        {
            if (!metalReserves.ContainsKey(type))
                metalReserves.Add(type, maxReserve);
        }
    }

    void Update()
    {
        PassiveRecovery();
    }

    void PassiveRecovery()
    {
        // Create a list of keys to avoid "collection modified" errors
        List<AllomancySkill.MetalType> keys = new List<AllomancySkill.MetalType>(metalReserves.Keys);
        foreach (var type in keys)
        {
            metalReserves[type] = Mathf.Min(maxReserve, metalReserves[type] + passiveRecoveryRate * Time.deltaTime);
        }
    }

    public float GetReserve(AllomancySkill.MetalType metal)
    {
        if (metalReserves.ContainsKey(metal))
            return metalReserves[metal];
        
        return 0f;
    }

    public void Drain(AllomancySkill.MetalType metal, float amount)
    {
        if (metalReserves.ContainsKey(metal))
        {
            metalReserves[metal] = Mathf.Max(0, metalReserves[metal] - amount);
            Debug.Log($"{metal} remaining: {metalReserves[metal]}");
        }
    }

    public void Refill(AllomancySkill.MetalType metal, float amount)
    {
        if (metalReserves.ContainsKey(metal))
            metalReserves[metal] = Mathf.Min(maxReserve, metalReserves[metal] + amount);
    }
        public void PurgeAll()
    {
        // Create a temporary list of keys to iterate through safely
        List<AllomancySkill.MetalType> keys = new List<AllomancySkill.MetalType>(metalReserves.Keys);
        
        foreach (var metal in keys)
        {
            metalReserves[metal] = 0f;
        }
        
        Debug.Log("MetalReserveManager: All reserves purged to 0.");
    }
}
