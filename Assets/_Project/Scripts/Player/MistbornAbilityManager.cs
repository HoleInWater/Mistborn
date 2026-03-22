using UnityEngine;

public class MistbornAbilityManager : MonoBehaviour
{
    [Header("Allomancy")]
    public SteelPush steelPush;
    public IronPull ironPull;
    public TinEnhance tinEnhance;
    public PewterBurn pewterBurn;
    public ZincRiot zincRiot;
    public BrassSoothe brassSoothe;
    public CopperCloud copperCloud;
    public BronzeDetect bronzeDetect;
    public GoldPast goldPast;
    public ElectrumFuture electrumFuture;
    public AtiumBurn atiumBurn;
    public MalatiumReveal malatiumReveal;
    
    [Header("Feruchemy")]
    public FeruchemySystem feruchemySystem;
    
    [Header("Hemalurgy")]
    public HemalurgySystem hemalurgySystem;
    
    [Header("Special")]
    public PewterManager pewterManager;
    public AtiumController atiumController;
    public SpeedBubbleController speedBubble;
    public LerasiumController lerasiumController;
    public HarmoniumController harmoniumController;
    
    [Header("Input Controller")]
    public AllomancyInputController inputController;
    
    [Header("Current State")]
    public MetalType activeMetal = MetalType.Steel;
    public bool isBurning = false;
    
    void Start()
    {
        CacheComponents();
        SetupInputController();
    }
    
    void CacheComponents()
    {
        steelPush = GetComponent<SteelPush>();
        ironPull = GetComponent<IronPull>();
        tinEnhance = GetComponent<TinEnhance>();
        pewterBurn = GetComponent<PewterBurn>();
        zincRiot = GetComponent<ZincRiot>();
        brassSoothe = GetComponent<BrassSoothe>();
        copperCloud = GetComponent<CopperCloud>();
        bronzeDetect = GetComponent<BronzeDetect>();
        goldPast = GetComponent<GoldPast>();
        electrumFuture = GetComponent<ElectrumFuture>();
        atiumBurn = GetComponent<AtiumBurn>();
        malatiumReveal = GetComponent<MalatiumReveal>();
        
        feruchemySystem = GetComponent<FeruchemySystem>();
        hemalurgySystem = GetComponent<HemalurgySystem>();
        
        pewterManager = GetComponent<PewterManager>();
        atiumController = GetComponent<AtiumController>();
        speedBubble = GetComponent<SpeedBubbleController>();
        lerasiumController = GetComponent<LerasiumController>();
        harmoniumController = GetComponent<HarmoniumController>();
        
        inputController = GetComponent<AllomancyInputController>();
    }
    
    void SetupInputController()
    {
        if (inputController == null)
        {
            inputController = gameObject.AddComponent<AllomancyInputController>();
        }
        
        inputController.metalManager = GetComponent<MetalReserveManager>();
        inputController.pewterManager = pewterManager;
        inputController.atiumController = atiumController;
        inputController.allomancer = GetComponent<Allomancer>();
    }
    
    public void SelectMetal(MetalType metal)
    {
        if (IsMetalAvailable(metal))
        {
            activeMetal = metal;
            Debug.Log($"Selected: {metal}");
        }
        else
        {
            Debug.LogWarning($"{metal} is not available!");
        }
    }
    
    public bool IsMetalAvailable(MetalType metal)
    {
        Allomancer allomancer = GetComponent<Allomancer>();
        if (allomancer == null) return false;
        
        float reserve = allomancer.GetMetalReserve(metal);
        return reserve > 0;
    }
    
    public void StartBurning()
    {
        if (!IsMetalAvailable(activeMetal))
        {
            Debug.LogWarning($"No {activeMetal} reserves!");
            return;
        }
        
        isBurning = true;
        
        switch (activeMetal)
        {
            case MetalType.Steel:
                steelPush?.StartBurning();
                break;
            case MetalType.Iron:
                ironPull?.StartBurning();
                break;
            case MetalType.Tin:
                tinEnhance?.StartEnhancing();
                break;
            case MetalType.Pewter:
                if (pewterManager != null) pewterManager.StartBurningPewter();
                else pewterBurn?.StartBurning();
                break;
            case MetalType.Zinc:
                zincRiot?.StartRioting();
                break;
            case MetalType.Brass:
                brassSoothe?.StartSoothing();
                break;
            case MetalType.Copper:
                copperCloud?.StartHiding();
                break;
            case MetalType.Bronze:
                bronzeDetect?.StartSeeking();
                break;
            case MetalType.Gold:
                goldPast?.StartSeeing();
                break;
            case MetalType.Electrum:
                electrumFuture?.StartSeeing();
                break;
            case MetalType.Atium:
                if (atiumController != null) atiumController.StartBurningAtium();
                else atiumBurn?.StartBurning();
                break;
            case MetalType.Malatium:
                malatiumReveal?.StartBurning();
                break;
        }
    }
    
    public void StopBurning()
    {
        isBurning = false;
        
        switch (activeMetal)
        {
            case MetalType.Steel:
                steelPush?.StopBurning();
                break;
            case MetalType.Iron:
                ironPull?.StopBurning();
                break;
            case MetalType.Tin:
                tinEnhance?.StopEnhancing();
                break;
            case MetalType.Pewter:
                if (pewterManager != null) pewterManager.StopBurningPewter();
                else pewterBurn?.StopBurning();
                break;
            case MetalType.Zinc:
                zincRiot?.StopRioting();
                break;
            case MetalType.Brass:
                brassSoothe?.StopSoothing();
                break;
            case MetalType.Copper:
                copperCloud?.StopHiding();
                break;
            case MetalType.Bronze:
                bronzeDetect?.StopSeeking();
                break;
            case MetalType.Gold:
                goldPast?.StopSeeing();
                break;
            case MetalType.Electrum:
                electrumFuture?.StopSeeing();
                break;
            case MetalType.Atium:
                if (atiumController != null) atiumController.StopBurningAtium();
                else atiumBurn?.StopBurning();
                break;
            case MetalType.Malatium:
                malatiumReveal?.StopBurning();
                break;
        }
    }
    
    public void Flare()
    {
        if (activeMetal == MetalType.Pewter)
        {
            if (pewterManager != null) pewterManager.Flare();
        }
    }
    
    public void MetalFlare()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        metals?.MetalFlare();
    }
    
    public float GetActiveMetalReserve()
    {
        Allomancer allomancer = GetComponent<Allomancer>();
        if (allomancer == null) return 0f;
        return allomancer.GetMetalReserve(activeMetal);
    }
    
    public string GetActiveMetalStatus()
    {
        string status = $"Active Metal: {activeMetal}\n";
        status += $"Reserve: {GetActiveMetalReserve():F1}%\n";
        status += $"Burning: {isBurning}";
        return status;
    }
    
    public void PrintAbilityList()
    {
        Debug.Log("=== MISTBORN ABILITIES ===");
        Debug.Log("Allomantic Powers:");
        foreach (MetalType metal in System.Enum.GetValues(typeof(MetalType)))
        {
            bool available = IsMetalAvailable(metal);
            Debug.Log($"  [{metal}] - {(available ? "Available" : "Empty")}");
        }
        
        if (feruchemySystem != null && feruchemySystem.isFeruchemist)
        {
            Debug.Log("\nFeruchemy: Active");
        }
        
        if (hemalurgySystem != null && hemalurgySystem.hasSpikes)
        {
            Debug.Log($"\nHemalurgy: {hemalurgySystem.spikes.Count} spikes");
        }
    }
}
