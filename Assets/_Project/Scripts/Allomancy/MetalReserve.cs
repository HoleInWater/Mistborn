using UnityEngine;
using UnityEngine.UIElements;

public class MetalReserve : MonoBehaviour
{
    [Header("UI Settings")]
    public UIDocument uiDocument;
    public string metalProgressBarName = "Metal";

    [Header("Metal Reserves")]
    public float[] reserves = new float[16];
    public float maxReserve = 100f;
    
    [Header("Recovery Settings")]
    public float passiveRecoveryRate = 0.5f;
    public float metalFlareRecovery = 25f;

    [Header("Active Display")]
    // Which metal should the HUD show? 
    // You can change this via code when the player switches "Active" metals.
    public AllomancySkill.MetalType activeMetalDisplay = AllomancySkill.MetalType.Iron; 

    private ProgressBar _metalBar;
    private AllomancySkill.MetalType[] metalTypes = (AllomancySkill.MetalType[])System.Enum.GetValues(typeof(AllomancySkill.MetalType));

    void OnEnable()
    {
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            _metalBar = root.Q<ProgressBar>(metalProgressBarName);

            if (_metalBar != null)
            {
                _metalBar.lowValue = 0;
                _metalBar.highValue = maxReserve;
            }
        }
    }

    void Start()
    {
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = maxReserve;
        }
    }

    void Update()
    {
        PassiveRecovery();
        UpdateHUD();
    }

    void PassiveRecovery()
    {
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = Mathf.Min(maxReserve, reserves[i] + passiveRecoveryRate * Time.deltaTime);
        }
    }

    private void UpdateHUD()
    {
        if (_metalBar != null)
        {
            float currentVal = GetReserve(activeMetalDisplay);
            _metalBar.value = currentVal;
            // Updates the text on the bar to show the metal name and amount
            _metalBar.title = $"{activeMetalDisplay}: {Mathf.FloorToInt(currentVal)} / {maxReserve}";
        }
    }

    public float GetReserve(AllomancySkill.MetalType metal)
    {
        return reserves[(int)metal];
    }

    public void Drain(AllomancySkill.MetalType metal, float amount)
    {
        reserves[(int)metal] = Mathf.Max(0, reserves[(int)metal] - amount);
    }

    public void Refill(AllomancySkill.MetalType metal, float amount)
    {
        reserves[(int)metal] = Mathf.Min(maxReserve, reserves[(int)metal] + amount);
    }

    public void MetalFlare()
    {
        foreach (AllomancySkill.MetalType metal in metalTypes)
        {
            Refill(metal, metalFlareRecovery);
        }
    }

    public void PurgeAll()
    {
        for (int i = 0; i < reserves.Length; i++)
        {
            reserves[i] = 0f;
        }
    }
    
    // Call this from your selection script to swap which metal is on the HUD
    public void SetActiveHUDMetal(AllomancySkill.MetalType metal)
    {
        activeMetalDisplay = metal;
    }
}
