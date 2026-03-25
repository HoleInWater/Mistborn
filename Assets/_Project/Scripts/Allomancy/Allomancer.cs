/* Allomancer.cs
 *
 * PURPOSE:
 * Core Allomancy system that manages metal reserves, burning state, and coordination
 * with metal-specific scripts and FlareManager.
 *
 * LORE-ACCURATE BURNING:
 * ======================
 * A Mistborn chooses which metal to burn by mentally focusing on specific "wells" 
 * of energy within their stomach, treating each ingested metal like a distinct 
 * mental switch or muscle to actuate.
 *
 * KEY MECHANICS:
 * - Mistborn can activate multiple metals simultaneously
 * - They can switch rapidly between metals by feeling the unique sensation
 * - Metals are consumed (burned) to channel power from Preservation
 * - Mistborn can burn multiple metals at once (e.g., pewter + iron)
 * - Mistborn can intuitively control all 16 metals
 *
 * FEATURES:
 * - Manages 16 metal reserves.
 * - Handles multiple simultaneous metal burning (all 16 possible).
 * - High-speed HUD updates for all 16 bars.
 * - Standardized Burn/Flare drain logic.
 * - Mental "well" switching for rapid metal changes.
 */

using UnityEngine;

public class Allomancer : MonoBehaviour
{
    [Header("Metal State - Mental Wells")]
    // Lore: Each metal is a distinct mental "well" the Mistborn can focus on
    private bool[] burningMetals = new bool[20]; // Track which metals are currently burning
    private AllomancySkill.MetalType primaryMetal; // Currently focused metal
    private AllomancySkill.MetalType secondaryMetal; // Secondary metal for dual-burn
    
    [Header("Multiple Metal Burning")]
    // Lore: Mistborn can burn multiple metals simultaneously
    public bool canBurnMultipleMetals = true;
    public int maxSimultaneousBurns = 16; // Mistborn can burn all 16 at once
    private int currentSimultaneousBurns = 0;

    [Header("Metal Reserves")]
    public float[] metalReserves = new float[20]; // Expanded for complete 18-metal suite + buffer
    public bool[] unlockedMetals = new bool[20];

    public bool canBurnMetal = true;

    [Header("Burn Rate")]
    public float baseBurnRate = 1f;

    [Header("HUD")]
    public MetalReserve metalReserve;

    [Header("Burst State")]
    public bool isDuraluminPrimed = false;
    private float nicroburstTimer = 0f;
    public bool isNicrobursting = false;

    private MetalSelector metalSelector;

    void Awake()
    {
        MistbornRegistry.RegisterAllomancer(this);
        
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
            burningMetals[i] = false;
        }

        // Start with ALL 16 metals unlocked for full Wheel UI testing
        for (int i = 0; i < 16; i++) {
            UnlockMetal((AllomancySkill.MetalType)i);
        }

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


    void Update()
    {
        // Handle Nicroburst consumption
        if (isNicrobursting)
        {
            if (IsAnyMetalBurning())
            {
                nicroburstTimer += Time.deltaTime;
                if (nicroburstTimer > AllomancyConstants.NicroburstDuration)
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

        // Drain all currently burning metals
        DrainBurningMetals();

        // Handle Duralumin burst
        if (isDuraluminPrimed && IsAnyMetalBurning())
        {
            Debug.Log($"[DURALUMIN] FORCED BURST! Expending all reserves.");
            DrainAllReservesInstantly();
            isDuraluminPrimed = false;
            isNicrobursting = false;
        }

        // Handle out-of-metal state
        if (!IsAnyMetalBurning() && metalReserve != null)
        {
            RefillMetal(GetCurrentMetal(), metalReserve.passiveRecoveryRate * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.RightAlt)) RefillAllMetals();

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

    // ── Mental "Well" System - Lore-Accurate Metal Selection ─────────────────

    /// <summary>
    /// Focus on a specific metal's mental "well" to start burning it.
    /// Lore: Mistborn mentally focus on specific "wells" of energy in their stomach.
    /// </summary>
    public void FocusOnMetal(AllomancySkill.MetalType metal)
    {
        if (!unlockedMetals[(int)metal])
        {
            Debug.Log($"[ALLOMANCER] Cannot focus on {metal} - Not unlocked!");
            return;
        }

        if (metalReserves[(int)metal] <= 0)
        {
            Debug.Log($"[ALLOMANCER] Cannot focus on {metal} - Reserve empty!");
            return;
        }

        // Start burning this metal's "well"
        burningMetals[(int)metal] = true;
        primaryMetal = metal;
        currentSimultaneousBurns++;

        Debug.Log($"[ALLOMANCER] Focused on {metal} well. Currently burning {currentSimultaneousBurns} metals.");
    }

    /// <summary>
    /// Release focus on a specific metal's mental "well" to stop burning it.
    /// </summary>
    public void ReleaseFocus(AllomancySkill.MetalType metal)
    {
        if (burningMetals[(int)metal])
        {
            burningMetals[(int)metal] = false;
            currentSimultaneousBurns--;

            if (currentSimultaneousBurns < 0) currentSimultaneousBurns = 0;

            Debug.Log($"[ALLOMANCER] Released {metal} well. Currently burning {currentSimultaneousBurns} metals.");
        }
    }

    /// <summary>
    /// Toggle burning a specific metal's mental "well".
    /// </summary>
    public void ToggleMetalBurn(AllomancySkill.MetalType metal)
    {
        if (burningMetals[(int)metal])
        {
            ReleaseFocus(metal);
        }
        else
        {
            FocusOnMetal(metal);
        }
    }

    /// <summary>
    /// Focus on primary metal (selected via scroll wheel).
    /// Lore: Primary metal selection via scroll wheel.
    /// </summary>
    public void FocusOnPrimaryMetal()
    {
        FocusOnMetal(GetCurrentMetal());
    }

    /// <summary>
    /// Focus on secondary metal (swap with Tab).
    /// Lore: Secondary metal selection via Tab.
    /// </summary>
    public void FocusOnSecondaryMetal()
    {
        if (metalSelector != null)
        {
            FocusOnMetal(metalSelector.GetSecondaryMetal());
        }
    }

    /// <summary>
    /// Drain all currently burning metals.
    /// Lore: Metals are consumed at different rates based on burning intensity.
    /// </summary>
    private void DrainBurningMetals()
    {
        if (!canBurnMetal) return;

        bool isFlaring = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;

        for (int i = 0; i < burningMetals.Length; i++)
        {
            if (burningMetals[i])
            {
                AllomancySkill.MetalType metal = (AllomancySkill.MetalType)i;

                // Calculate drain rate based on flaring and metal type
                float drainRate = baseBurnRate;
                if (isFlaring && FlareManager.Instance != null)
                {
                    drainRate += FlareManager.Instance.flareBurnRate;
                }

                // Drain the metal
                DrainMetal(metal, drainRate * Time.deltaTime);

                // If reserve is empty, stop burning this metal
                if (metalReserves[i] <= 0)
                {
                    burningMetals[i] = false;
                    currentSimultaneousBurns--;
                    Debug.Log($"[ALLOMANCER] {metal} reserve depleted - well empty!");
                }
            }
        }
    }

    /// <summary>
    /// Drain all reserves instantly (Duralumin burst).
    /// </summary>
    private void DrainAllReservesInstantly()
    {
        for (int i = 0; i < burningMetals.Length; i++)
        {
            if (burningMetals[i])
            {
                AllomancySkill.MetalType metal = (AllomancySkill.MetalType)i;
                float remaining = metalReserves[i];
                DrainMetal(metal, remaining);
                burningMetals[i] = false;
            }
        }
        currentSimultaneousBurns = 0;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Start burning a specific metal (legacy API - uses FocusOnMetal internally).
    /// </summary>
    public void StartBurning(AllomancySkill.MetalType metal)
    {
        FocusOnMetal(metal);
    }

    /// <summary>
    /// Stop burning all metals.
    /// </summary>
    public void StopBurning()
    {
        for (int i = 0; i < burningMetals.Length; i++)
        {
            burningMetals[i] = false;
        }
        currentSimultaneousBurns = 0;
        isNicrobursting = false;
    }

    /// <summary>
    /// Check if any metal is currently burning.
    /// </summary>
    public bool IsBurning() => IsAnyMetalBurning();

    /// <summary>
    /// Check if any metal is currently burning.
    /// </summary>
    public bool IsAnyMetalBurning() => currentSimultaneousBurns > 0;

    /// <summary>
    /// Check if a specific metal is currently burning.
    /// </summary>
    public bool IsMetalBurning(AllomancySkill.MetalType metal) => burningMetals[(int)metal];

    /// <summary>
    /// Get the number of metals currently burning simultaneously.
    /// </summary>
    public int GetSimultaneousBurnCount() => currentSimultaneousBurns;

    /// <summary>
    /// Set the current primary metal (selected via scroll wheel).
    /// </summary>
    public void SetCurrentMetal(AllomancySkill.MetalType metal)
    {
        primaryMetal = metal;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }

    /// <summary>
    /// Get the current primary metal.
    /// </summary>
    public AllomancySkill.MetalType GetCurrentMetal()
    {
        if (metalSelector != null) return metalSelector.GetActiveMetal();
        return primaryMetal;
    }

    /// <summary>
    /// Get the reserve amount for a specific metal.
    /// </summary>
    public float GetMetalReserve(AllomancySkill.MetalType metal) => metalReserves[(int)metal];

    /// <summary>
    /// Drain a specific metal reserve.
    /// </summary>
    public void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);
        UpdateHUD(metal);
        
        if (metal == GetCurrentMetal())
            canBurnMetal = metalReserves[(int)metal] > 0;
    }

    /// <summary>
    /// Refill a specific metal reserve.
    /// </summary>
    public void RefillMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
        UpdateHUD(metal);
    }

    /// <summary>
    /// Refill all metal reserves.
    /// </summary>
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

    /// <summary>
    /// Clear all metal reserves (Chromium leeching).
    /// </summary>
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

    /// <summary>
    /// Get the mental "sensation" of a metal.
    /// Lore: Each metal has a unique warm sensation the Mistborn can feel.
    /// </summary>
    public string GetMetalSensation(AllomancySkill.MetalType metal)
    {
        switch (metal)
        {
            case AllomancySkill.MetalType.Steel: return "A sharp, electric tingle";
            case AllomancySkill.MetalType.Iron: return "A deep, magnetic pull";
            case AllomancySkill.MetalType.Pewter: return "A warm, powerful surge";
            case AllomancySkill.MetalType.Tin: return "A bright, alert clarity";
            case AllomancySkill.MetalType.Zinc: return "A fiery, aggressive heat";
            case AllomancySkill.MetalType.Brass: return "A cool, calming wave";
            case AllomancySkill.MetalType.Copper: return "A silent, protective hum";
            case AllomancySkill.MetalType.Bronze: return "A pulsing, rhythmic beat";
            case AllomancySkill.MetalType.Atium: return "A cold, crystalline shimmer";
            case AllomancySkill.MetalType.Gold: return "A heavy, nostalgic warmth";
            case AllomancySkill.MetalType.Electrum: return "A quicksilver flash";
            case AllomancySkill.MetalType.Aluminum: return "A sudden, hollow emptiness";
            case AllomancySkill.MetalType.Duralumin: return "An explosive, intense burn";
            case AllomancySkill.MetalType.Bendalloy: return "A stretching, elastic pull";
            case AllomancySkill.MetalType.Cadmium: return "A slowing, viscous drag";
            case AllomancySkill.MetalType.Malatium: return "A shadowy, distorted vision";
            case AllomancySkill.MetalType.Chromium: return "A hungry, consuming void";
            case AllomancySkill.MetalType.Nicrosil: return "A amplifying, resonant hum";
            default: return "An unknown sensation";
        }
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