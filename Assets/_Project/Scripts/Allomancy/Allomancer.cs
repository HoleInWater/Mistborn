/* Allomancer.cs
 *
 * PURPOSE:
 * Core Allomancy system that manages metal reserves, burning state, and coordination
 * with metal-specific scripts and FlareManager.
 *
 * FEATURES:
 * - Manages 16 metal reserves.
 * - Handles 2-metal selection (Primary/Secondary) via MetalSelector.
 * - High-speed HUD updates for all 16 bars.
 * - Standardized Burn/Flare drain logic.
 */

using UnityEngine;

public class Allomancer : MonoBehaviour
{
    [Header("Metal State")]
    private bool isBurningMetal = false;
    private AllomancySkill.MetalType currentMetal;

    [Header("Metal Reserves")]
    public float[] metalReserves = new float[20]; // Expanded for complete 18-metal suite + buffer
    public bool[] unlockedMetals = new bool[20];

    public bool canBurnMetal = true;

    [Header("HUD")]
    public MetalReserve metalReserve;

    [Header("Burst State")]
    public bool isDuraluminPrimed = false;
    private float nicroburstTimer = 0f; // Moved here as per instruction's implied placement
    public bool isNicrobursting = false; // This field is not removed by the instruction, only the duplicate is.

    private MetalSelector metalSelector;

    void Awake()
    {
        MistbornRegistry.RegisterAllomancer(this);
        
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
        }

        // Start with basic traversal unlocked
        unlockedMetals[(int)AllomancySkill.MetalType.Steel] = true;
        unlockedMetals[(int)AllomancySkill.MetalType.Iron] = true;

        EnsureAllomancyComponents();
        metalSelector = GetComponent<MetalSelector>();
    }

    void OnDestroy()
    {
        MistbornRegistry.UnregisterAllomancer(this);
    }
    
    // Kept for scene-start safety if components are added at runtime
    void Start()
    {
        if (metalSelector == null) metalSelector = GetComponent<MetalSelector>();
    }

    public void UnlockMetal(AllomancySkill.MetalType metal)
    {
        unlockedMetals[(int)metal] = true;
        Debug.Log($"[ALLOMANCER] Metal Unlocked: {metal}");
    }


    void EnsureAllomancyComponents()
    {
        if (GetComponent<SteelPush>() == null) gameObject.AddComponent<SteelPush>();
        if (GetComponent<IronPull>() == null) gameObject.AddComponent<IronPull>();
        if (GetComponent<FlareManager>() == null) gameObject.AddComponent<FlareManager>();
        if (GetComponent<MetalSelector>() == null) gameObject.AddComponent<MetalSelector>();
        if (GetComponent<MetalReserve>() == null) gameObject.AddComponent<MetalReserve>();
        if (GetComponent<MetalBurnEffect>() == null) gameObject.AddComponent<MetalBurnEffect>();
        
        // Add all 16 metal components
        if (GetComponent<Tin>() == null) gameObject.AddComponent<Tin>();
        if (GetComponent<Pewter>() == null) gameObject.AddComponent<Pewter>();
        if (GetComponent<Zinc>() == null) gameObject.AddComponent<Zinc>();
        if (GetComponent<Brass>() == null) gameObject.AddComponent<Brass>();
        if (GetComponent<Copper>() == null) gameObject.AddComponent<Copper>();
        if (GetComponent<Bronze>() == null) gameObject.AddComponent<Bronze>();
        if (GetComponent<Atium>() == null) gameObject.AddComponent<Atium>();
        if (GetComponent<Malatium>() == null) gameObject.AddComponent<Malatium>();
        if (GetComponent<Gold>() == null) gameObject.AddComponent<Gold>();
        if (GetComponent<Electrum>() == null) gameObject.AddComponent<Electrum>();
        if (GetComponent<Aluminum>() == null) gameObject.AddComponent<Aluminum>();
        if (GetComponent<Duralumin>() == null) gameObject.AddComponent<Duralumin>();
        if (GetComponent<Bendalloy>() == null) gameObject.AddComponent<Bendalloy>();
        if (GetComponent<Cadmium>() == null) gameObject.AddComponent<Cadmium>();
        if (GetComponent<Chromium>() == null) gameObject.AddComponent<Chromium>();
        if (GetComponent<Nicrosil>() == null) gameObject.AddComponent<Nicrosil>();
    }


    private float nicroburstTimer = 0f;

    void Update()
    {
        // Toggle burn with B key
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isBurningMetal) StopBurning();
            else StartBurning(GetCurrentMetal());
        }

        // Handle Nicroburst consumption
        if (isNicrobursting)
        {
            if (isBurningMetal)
            {
                nicroburstTimer += Time.deltaTime;
                if (nicroburstTimer > AllomancyConstants.NicroburstDuration) // Consume after burst duration
                {
                    isNicrobursting = false;
                    nicroburstTimer = 0f;
                    Debug.Log("[NICROSIL] Burst exhausted.");
                }
            }
        }
        else
        {
            nicroburstTimer = 0f;
        }

        // Determine active states
        bool isFlaring = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;
        bool isUsingMetal = isBurningMetal || isFlaring;

        if (isUsingMetal && canBurnMetal)
        {
            float drainRate = baseBurnRate;
            if (isFlaring && FlareManager.Instance != null)
                drainRate += FlareManager.Instance.flareBurnRate;

            // Duralumin Burst Logic: Lore-accurate "Forced Flare"
            if (isDuraluminPrimed)
            {
                Debug.Log($"[DURALUMIN] FORCED BURST! Expending all {GetCurrentMetal()} reserves.");
                // Instant drain:
                float remaining = GetMetalReserve(GetCurrentMetal());
                DrainMetal(GetCurrentMetal(), remaining);
                isDuraluminPrimed = false;
                isNicrobursting = false; // Nicroburst is also consumed by the burst
            }
            else
            {
                float drainRate = baseBurnRate;
                if (isFlaring && FlareManager.Instance != null)
                    drainRate += FlareManager.Instance.flareBurnRate;
                
                DrainMetal(GetCurrentMetal(), drainRate * Time.deltaTime);
                
                // If we were nicrobursting but not duralumin priming, clear it after some burn
                if (isNicrobursting && drainRate > baseBurnRate)
                {
                    // For now, nicroburst lasts for one "flare action" or until metal is stopped
                    // We'll clear it when they stop burning in StopBurning()
                }
            }
        }
        else if (!isUsingMetal && metalReserve != null)
        {
            // Passive recovery only when idle
            RefillMetal(GetCurrentMetal(), metalReserve.passiveRecoveryRate * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.R)) RefillAllMetals();

        // Ensure HUD selection highlights are up-to-date every frame
        if (metalReserve != null && metalSelector != null)
        {
            metalReserve.HighlightSelection(
                metalSelector.GetPrimaryMetal(),
                metalSelector.GetSecondaryMetal(),
                metalSelector.IsPrimaryActive()
            );
            metalReserve.VisualizePrimedState(GetCurrentMetal(), isDuraluminPrimed);
        }
    }

    public void StartBurning(AllomancySkill.MetalType metal)
    {
        if (!unlockedMetals[(int)metal])
        {
            Debug.Log($"[ALLOMANCER] Cannot burn {metal} - Not unlocked!");
            isBurningMetal = false;
            return;
        }

        isBurningMetal = true;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }


    public void StopBurning()
    {
        isBurningMetal = false;
        isNicrobursting = false; // Clear nicroburst on stop
    }
    public bool IsBurning() => isBurningMetal;

    public void SetCurrentMetal(AllomancySkill.MetalType metal)
    {
        currentMetal = metal;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public AllomancySkill.MetalType GetCurrentMetal()
    {
        if (metalSelector != null) return metalSelector.GetActiveMetal();
        return currentMetal;
    }

    public float GetMetalReserve(AllomancySkill.MetalType metal) => metalReserves[(int)metal];

    public void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);
        UpdateHUD(metal);
        
        if (metal == GetCurrentMetal())
            canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public void RefillMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
        UpdateHUD(metal);
    }

    public void RefillAllMetals()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
            UpdateHUD((AllomancySkill.MetalType)i);
        }
        canBurnMetal = true;
        Debug.Log("[ALLOMANCER] All metal reserves refilled.");
    }

    public void ClearAllReserves()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 0f;
            UpdateHUD((AllomancySkill.MetalType)i);
        }
        canBurnMetal = false;
        Debug.Log("[ALLOMANCER] All metal reserves emptied by Chromium leeching.");
    }

    private void UpdateHUD(AllomancySkill.MetalType metal)
    {
        // HUD Refresh
        if (metalReserve != null)
        {
            metalReserve.UpdateAllBars(metalReserves);
        }
    }
}
