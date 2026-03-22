using UnityEngine;
using UnityEngine.Events;

public class AtiumController : MonoBehaviour
{
    [Header("Atium Settings")]
    public float atiumReserve = 100f;
    public float burnRate = 5f;
    public float mentalProcessingBonus = 2f;
    public float reactionTimeBonus = 0.5f;
    
    [Header("Future Shadow Settings")]
    public int maxFutureShadows = 20;
    public float shadowUpdateRate = 0.1f;
    
    [Header("Duralumin Combo")]
    public bool duraluminAvailable = false;
    public float duraluminDuration = 1f;
    
    [Header("State")]
    public bool isBurningAtium = false;
    public bool isDuraluminBoosted = false;
    
    [Header("Events")]
    public UnityEvent OnAtiumActivated;
    public UnityEvent OnAtiumDeactivated;
    public UnityEvent OnFutureSightTriggered;
    
    private float timeSinceLastUpdate = 0f;
    private float duraluminTimeRemaining = 0f;
    
    void Update()
    {
        if (isBurningAtium)
        {
            atiumReserve -= burnRate * Time.deltaTime;
            
            if (isDuraluminBoosted)
            {
                duraluminTimeRemaining -= Time.deltaTime;
                if (duraluminTimeRemaining <= 0)
                {
                    DeactivateDuraluminBoost();
                }
            }
            
            if (atiumReserve <= 0)
            {
                StopBurningAtium();
            }
            
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= shadowUpdateRate)
            {
                UpdateFutureShadows();
                timeSinceLastUpdate = 0f;
            }
        }
    }
    
    void UpdateFutureShadows()
    {
        OnFutureSightTriggered?.Invoke();
    }
    
    public void StartBurningAtium()
    {
        if (!isBurningAtium && atiumReserve > 0)
        {
            isBurningAtium = true;
            OnAtiumActivated?.Invoke();
            Debug.Log("Started burning Atium - Future sight active");
        }
    }
    
    public void StopBurningAtium()
    {
        if (isBurningAtium)
        {
            isBurningAtium = false;
            isDuraluminBoosted = false;
            OnAtiumDeactivated?.Invoke();
            Debug.Log("Stopped burning Atium");
        }
    }
    
    public void ActivateDuraluminBoost()
    {
        if (!isBurningAtium || !duraluminAvailable) return;
        
        isDuraluminBoosted = true;
        duraluminTimeRemaining = duraluminDuration;
        
        float boostedBurnRate = burnRate * 10f;
        atiumReserve -= (boostedBurnRate - burnRate) * duraluminDuration;
        atiumReserve = Mathf.Max(0, atiumReserve);
        
        Debug.Log($"DURALUMIN BOOST - Extended futuresight for {duraluminDuration}s!");
    }
    
    void DeactivateDuraluminBoost()
    {
        isDuraluminBoosted = false;
        Debug.Log("Duralumin boost ended");
    }
    
    public float GetReactionTimeBonus()
    {
        float bonus = reactionTimeBonus;
        if (isDuraluminBoosted)
        {
            bonus *= 5f;
        }
        return bonus;
    }
    
    public int GetMaxPredictedActions()
    {
        int baseCount = maxFutureShadows;
        if (isDuraluminBoosted)
        {
            baseCount *= 3;
        }
        return baseCount;
    }
    
    public float GetProcessingCapacity()
    {
        float capacity = mentalProcessingBonus;
        if (isDuraluminBoosted)
        {
            capacity *= 5f;
        }
        return capacity;
    }
    
    public void RefillAtium(float amount)
    {
        atiumReserve = Mathf.Min(100f, atiumReserve + amount);
    }
}
