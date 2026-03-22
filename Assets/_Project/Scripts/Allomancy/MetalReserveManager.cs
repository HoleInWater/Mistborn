using UnityEngine;

public class MetalReserveManager : MonoBehaviour
{
    [Header("Metal Reserves")]
    public float[] reserves = new float[16];
    
    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 0.5f;
    public float metalFlareRecovery = 25f;
    
    public MetalType[] metalTypes = (MetalType[])System.Enum.GetValues(typeof(MetalType));
    
    void Start()
    {
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = 100f;
        }
    }
    
    void Update()
    {
        PassiveRecovery();
    }
    
    void PassiveRecovery()
    {
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = Mathf.Min(100f, reserves[i] + passiveRecoveryRate * Time.deltaTime);
        }
    }
    
    public float GetReserve(MetalType metal)
    {
        return reserves[(int)metal];
    }
    
    public void Drain(MetalType metal, float amount)
    {
        reserves[(int)metal] = Mathf.Max(0, reserves[(int)metal] - amount);
    }
    
    public void Refill(MetalType metal, float amount)
    {
        reserves[(int)metal] = Mathf.Min(100f, reserves[(int)metal] + amount);
    }
    
    public void MetalFlare()
    {
        foreach (MetalType metal in metalTypes)
        {
            Refill(metal, metalFlareRecovery);
        }
        Debug.Log("Metal Flare! All reserves partially restored.");
    }
    
    public void PurgeAll()
    {
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = 0f;
        }
        Debug.Log("All metal reserves purged!");
    }
}
