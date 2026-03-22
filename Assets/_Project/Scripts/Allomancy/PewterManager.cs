using UnityEngine;

public class PewterManager : MonoBehaviour
{
    public static PewterManager Instance { get; private set; }

    [Header("Pewter Enhancement Settings")]
    public float strengthMultiplier = 2f;
    public float speedMultiplier = 1.2f;
    public float durabilityMultiplier = 1.5f;
    public float painToleranceBonus = 100f;
    public float balanceBonus = 2f;
    public float regenerationBonus = 1.5f;
    
    [Header("Current State")]
    public bool isBurningPewter = false;
    public bool isFlared = false;
    
    [Header("Pewter Drag")]
    public float dragThreshold = 0.3f;
    public float dragRecoveryRate = 0.25f;
    public float maxDragDuration = 60f;
    private float currentDragTime = 0f;
    
    private CharacterController controller;
    private Health health;
    private Stamina stamina;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        stamina = GetComponent<Stamina>();
    }
    
    void Update()
    {
        if (isBurningPewter)
        {
            HandlePewterDrag();
        }
    }
    
    public float GetCurrentStrengthMultiplier()
    {
        if (!isBurningPewter) return 1f;
        
        float multiplier = isFlared ? strengthMultiplier * 1.5f : strengthMultiplier;
        multiplier *= GetDragPenalty();
        
        return multiplier;
    }
    
    public float GetCurrentSpeedMultiplier()
    {
        if (!isBurningPewter) return 1f;
        
        float multiplier = isFlared ? speedMultiplier * 1.5f : speedMultiplier;
        multiplier *= GetDragPenalty();
        
        return multiplier;
    }
    
    public float GetCurrentDurabilityMultiplier()
    {
        if (!isBurningPewter) return 1f;
        
        float multiplier = isFlared ? durabilityMultiplier * 1.5f : durabilityMultiplier;
        multiplier *= GetDragPenalty();
        
        return multiplier;
    }
    
    public bool IsInPewterDrag()
    {
        return currentDragTime > dragThreshold;
    }
    
    float GetDragPenalty()
    {
        if (currentDragTime < dragThreshold) return 1f;
        
        float dragFactor = 1f - ((currentDragTime - dragThreshold) / maxDragDuration);
        dragFactor = Mathf.Clamp01(dragFactor);
        
        return dragFactor * dragRecoveryRate;
    }
    
    void HandlePewterDrag()
    {
        currentDragTime += Time.deltaTime;
        
        if (currentDragTime >= maxDragDuration)
        {
            Debug.Log("Pewter Drag: Maximum duration reached!");
            StopBurningPewter();
        }
    }
    
    public void StartBurningPewter()
    {
        if (!isBurningPewter)
        {
            isBurningPewter = true;
            currentDragTime = 0f;
            Debug.Log("Started burning Pewter");
        }
    }
    
    public void StopBurningPewter()
    {
        isBurningPewter = false;
        isFlared = false;
        Debug.Log("Stopped burning Pewter");
    }
    
    public void Flare()
    {
        if (isBurningPewter)
        {
            isFlared = true;
            Debug.Log("Pewter Flared!");
        }
    }
    
    public void Unflare()
    {
        isFlared = false;
    }
    
    public float TakeReducedDamage(float baseDamage)
    {
        if (!isBurningPewter) return baseDamage;
        
        return baseDamage / GetCurrentDurabilityMultiplier();
    }
    
    public bool CanPerformAction(float staminaCost)
    {
        if (!isBurningPewter) return true;
        
        if (IsInPewterDrag())
        {
            return Random.value < GetDragPenalty();
        }
        
        return true;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
