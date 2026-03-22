using UnityEngine;
using System.Collections.Generic;

public class AllomancyInputController : MonoBehaviour
{
    [Header("References")]
    public MetalReserveManager metalManager;
    public PewterManager pewterManager;
    public AtiumController atiumController;
    public Allomancer allomancer;
    
    [Header("Key Bindings")]
    public KeyCode metalBurningKey = KeyCode.LeftShift;
    public KeyCode flareKey = KeyCode.F;
    public KeyCode metalFlareAllKey = KeyCode.G;
    
    [Header("Metal Selection")]
    public MetalType selectedMetal = MetalType.Steel;
    private Dictionary<MetalType, MonoBehaviour> metalControllers;
    
    void Start()
    {
        metalControllers = new Dictionary<MetalType, MonoBehaviour>();
        
        if (metalManager == null)
            metalManager = GetComponent<MetalReserveManager>();
        if (allomancer == null)
            allomancer = GetComponent<Allomancer>();
        if (pewterManager == null)
            pewterManager = FindObjectOfType<PewterManager>();
        if (atiumController == null)
            atiumController = FindObjectOfType<AtiumController>();
        
        SetupMetalControllers();
    }
    
    void SetupMetalControllers()
    {
        MonoBehaviour[] controllers = GetComponents<MonoBehaviour>();
        foreach (var controller in controllers)
        {
            if (controller is SteelPush) metalControllers[MetalType.Steel] = controller;
            else if (controller is IronPull) metalControllers[MetalType.Iron] = controller;
            else if (controller is PewterBurn || controller is PewterManager) metalControllers[MetalType.Pewter] = controller;
            else if (controller is TinEnhance) metalControllers[MetalType.Tin] = controller;
            else if (controller is AtiumBurn || controller is AtiumController) metalControllers[MetalType.Atium] = controller;
            else if (controller is ZincRiot) metalControllers[MetalType.Zinc] = controller;
            else if (controller is BrassSoothe) metalControllers[MetalType.Brass] = controller;
            else if (controller is CopperCloud) metalControllers[MetalType.Copper] = controller;
            else if (controller is BronzeDetect) metalControllers[MetalType.Bronze] = controller;
            else if (controller is GoldPast) metalControllers[MetalType.Gold] = controller;
            else if (controller is ElectrumFuture) metalControllers[MetalType.Electrum] = controller;
        }
    }
    
    void Update()
    {
        HandleMetalSelection();
        HandleBurningInput();
        HandleFlareInput();
        HandleSpecialAbilities();
    }
    
    void HandleMetalSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectMetal(MetalType.Steel);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectMetal(MetalType.Iron);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectMetal(MetalType.Tin);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectMetal(MetalType.Pewter);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SelectMetal(MetalType.Zinc);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SelectMetal(MetalType.Brass);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SelectMetal(MetalType.Copper);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SelectMetal(MetalType.Bronze);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) SelectMetal(MetalType.Gold);
        else if (Input.GetKeyDown(KeyCode.Alpha0)) SelectMetal(MetalType.Electrum);
    }
    
    void SelectMetal(MetalType metal)
    {
        selectedMetal = metal;
        Debug.Log($"Selected metal: {metal}");
    }
    
    void HandleBurningInput()
    {
        if (Input.GetKeyDown(metalBurningKey))
        {
            StartBurningSelectedMetal();
        }
        
        if (Input.GetKeyUp(metalBurningKey))
        {
            StopBurningSelectedMetal();
        }
    }
    
    void HandleFlareInput()
    {
        if (Input.GetKeyDown(flareKey) && IsMetalBurning(MetalType.Pewter))
        {
            if (pewterManager != null)
            {
                pewterManager.Flare();
            }
        }
        
        if (Input.GetKeyDown(metalFlareAllKey))
        {
            if (metalManager != null)
            {
                metalManager.MetalFlare();
            }
        }
    }
    
    void HandleSpecialAbilities()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpeedBubbleController bubble = GetComponent<SpeedBubbleController>();
            if (bubble != null)
            {
                bubble.ToggleBubble();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.H) && IsMetalBurning(MetalType.Atium))
        {
            if (atiumController != null && Input.GetKey(KeyCode.LeftControl))
            {
                atiumController.ActivateDuraluminBoost();
            }
        }
    }
    
    public void StartBurningSelectedMetal()
    {
        if (metalControllers.TryGetValue(selectedMetal, out var controller))
        {
            if (controller is SteelPush steel) steel.StartBurning();
            else if (controller is IronPull iron) iron.StartBurning();
            else if (controller is PewterBurn pewter) pewter.StartBurning();
            else if (controller is PewterManager pewterMgr && !pewterMgr.isBurningPewter) pewterMgr.StartBurningPewter();
            else if (controller is TinEnhance tin) tin.StartEnhancing();
            else if (controller is AtiumBurn atium) atium.StartBurning();
            else if (controller is AtiumController atiumCtrl && !atiumCtrl.isBurningAtium) atiumCtrl.StartBurningAtium();
            else if (controller is ZincRiot zinc) zinc.StartRioting();
            else if (controller is BrassSoothe brass) brass.StartSoothing();
            else if (controller is CopperCloud copper) copper.StartHiding();
            else if (controller is BronzeDetect bronze) bronze.StartSeeking();
            else if (controller is GoldPast gold) gold.StartSeeing();
            else if (controller is ElectrumFuture electrum) electrum.StartSeeing();
            
            allomancer?.StartBurning(selectedMetal);
        }
    }
    
    public void StopBurningSelectedMetal()
    {
        if (metalControllers.TryGetValue(selectedMetal, out var controller))
        {
            if (controller is SteelPush steel) steel.StopBurning();
            else if (controller is IronPull iron) iron.StopBurning();
            else if (controller is PewterBurn pewter) pewter.StopBurning();
            else if (controller is PewterManager pewterMgr) pewterMgr.StopBurningPewter();
            else if (controller is TinEnhance tin) tin.StopEnhancing();
            else if (controller is AtiumBurn atium) atium.StopBurning();
            else if (controller is AtiumController atiumCtrl) atiumCtrl.StopBurningAtium();
            else if (controller is ZincRiot zinc) zinc.StopRioting();
            else if (controller is BrassSoothe brass) brass.StopSoothing();
            else if (controller is CopperCloud copper) copper.StopHiding();
            else if (controller is BronzeDetect bronze) bronze.StopSeeking();
            else if (controller is GoldPast gold) gold.StopSeeing();
            else if (controller is ElectrumFuture electrum) electrum.StopSeeing();
            
            allomancer?.StopBurning();
        }
    }
    
    public bool IsMetalBurning(MetalType metal)
    {
        if (metalControllers.TryGetValue(metal, out var controller))
        {
            if (controller is SteelPush steel) return steel.IsBurning;
            else if (controller is IronPull iron) return iron.IsBurning;
            else if (controller is PewterBurn pewter) return pewter.IsBurning;
            else if (controller is PewterManager pewterMgr) return pewterMgr.isBurningPewter;
            else if (controller is TinEnhance tin) return tin.IsEnhancing;
            else if (controller is AtiumBurn atium) return atium.IsBurning;
            else if (controller is AtiumController atiumCtrl) return atiumCtrl.isBurningAtium;
            else if (controller is ZincRiot zinc) return zinc.IsRioting;
            else if (controller is BrassSoothe brass) return brass.IsSoothing;
            else if (controller is CopperCloud copper) return copper.IsHiding;
            else if (controller is BronzeDetect bronze) return bronze.IsSeeking;
            else if (controller is GoldPast gold) return gold.IsSeeing;
            else if (controller is ElectrumFuture electrum) return electrum.IsSeeing;
        }
        return false;
    }
    
    public float GetMetalReserve(MetalType metal)
    {
        return metalManager != null ? metalManager.GetReserve(metal) : 0f;
    }
    
    public MetalType GetSelectedMetal() => selectedMetal;
}
