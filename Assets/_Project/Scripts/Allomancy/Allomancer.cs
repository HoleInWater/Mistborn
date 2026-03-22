using UnityEngine;

public class Allomancer : MonoBehaviour
{
    [Header("Metal State")]
    public bool isBurningMetal = false;
    public AllomancySkill.MetalType currentMetal = AllomancySkill.MetalType.Steel;
    
    [Header("Metal Reserves")]
    public float[] metalReserves = new float[16];
    
    void Start()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
        }
    }
    
    public void StartBurning(AllomancySkill.MetalType metal)
    {
        currentMetal = metal;
        isBurningMetal = true;
    }
    
    public void StopBurning()
    {
        isBurningMetal = false;
    }
    
    public bool IsBurning()
    {
        return isBurningMetal;
    }
    
    public AllomancySkill.MetalType GetCurrentMetal()
    {
        return currentMetal;
    }
    
    public float GetMetalReserve(AllomancySkill.MetalType metal)
    {
        return metalReserves[(int)metal];
    }
    
    public void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);
    }
    
    public void RefillMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
    }
}
