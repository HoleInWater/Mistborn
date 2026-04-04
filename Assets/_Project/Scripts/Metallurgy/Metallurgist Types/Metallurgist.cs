/* Metallurgist.cs
 *
 * PURPOSE:
 * Core Metallurgy system that manages metal reserves, burning state, and coordination
 * with metal-specific scripts and FlareManager.
 *
 * FEATURES:
 * - Manages 16 metal reserves.
 * - Handles 2-metal selection (Primary/Secondary) via MetalSelector.
 * - High-speed HUD updates for all 16 bars.
 * - Standardized Burn/Flare drain logic.
 *
 * BUG FIX: Awake() unlock loop was `i < 16` but metalReserves is sized to 20 and
 *          EnsureMetallurgyComponents() adds Chromium and Nicrosil as components.
 *          Because the loop never called UnlockMetal() for them, the wheel always
 *          showed them as LOCKED regardless of any other fix. Changed to use
 *          Enum.GetValues so every metal in the MetalType enum is unlocked,
 *          future-proofing against any further additions.
 */

using UnityEngine;

[PlayerComponent("Metallurgy", order: 10)]
public class Metallurgist : MonoBehaviour
{
    [Header("Metal State")]
    private bool isBurningMetal = false;
    private MetallurgySkill.MetalType currentMetal;

    [Header("Metal Reserves")]
    public float[] metalReserves = new float[20]; // Expanded for complete 18-metal suite + buffer
    public bool[] unlockedMetals = new bool[20];

    public bool canBurnMetal = true;

    [Header("Burn Rate")]
    // Set to 0 — every metal script owns its own drain rate (calibrated to MAG values).
    // This generic drain was double-counting on top of per-script rates.
    public float baseBurnRate = 0f;

    [Header("HUD")]
    public MetalReserve metalReserve;

    [Header("Burst State")]
    public bool isDuraluminPrimed = false;
    private float nicroburstTimer = 0f;
    public bool isNicrobursting = false;

    private MetalSelector metalSelector;

    void Awake()
    {
        AshwalkerRegistry.RegisterMetallurgist(this);
        
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
        }

        // FIX: Was `for (int i = 0; i < 16; i++)` — stopped before Chromium and Nicrosil,
        // leaving them permanently locked and invisible/unselectable on the wheel.
        // Now iterates every value in the enum so all metals are unlocked correctly.
        foreach (MetallurgySkill.MetalType metal in System.Enum.GetValues(typeof(MetallurgySkill.MetalType)))
        {
            UnlockMetal(metal);
        }

        EnsureMetallurgyComponents();
        metalSelector = GetComponent<MetalSelector>();
    }

    void OnDestroy()
    {
        AshwalkerRegistry.UnregisterMetallurgist(this);
    }

    void Start()
    {
        if (metalSelector == null) metalSelector = GetComponent<MetalSelector>();
    }

    public void UnlockMetal(MetallurgySkill.MetalType metal)
    {
        unlockedMetals[(int)metal] = true;
    }

    void EnsureMetallurgyComponents()
    {
        if (GetComponent<SteelPush>() == null) gameObject.AddComponent<SteelPush>();
        if (GetComponent<IronPull>() == null) gameObject.AddComponent<IronPull>();
        if (GetComponent<FlareManager>() == null) gameObject.AddComponent<FlareManager>();
        if (GetComponent<MetalSelector>() == null) gameObject.AddComponent<MetalSelector>();
        if (GetComponent<MetalReserve>() == null) gameObject.AddComponent<MetalReserve>();
        if (GetComponent<MetalBurnEffect>() == null) gameObject.AddComponent<MetalBurnEffect>();
        
        if (GetComponent<Tin>() == null) gameObject.AddComponent<Tin>();
        if (GetComponent<Pewter>() == null) gameObject.AddComponent<Pewter>();
        if (GetComponent<Zinc>() == null) gameObject.AddComponent<Zinc>();
        if (GetComponent<Brass>() == null) gameObject.AddComponent<Brass>();
        if (GetComponent<Copper>() == null) gameObject.AddComponent<Copper>();
        if (GetComponent<Bronze>() == null) gameObject.AddComponent<Bronze>();
        if (GetComponent<Oraculum>() == null) gameObject.AddComponent<Oraculum>();
        if (GetComponent<Maloraculum>() == null) gameObject.AddComponent<Maloraculum>();
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
        if (Input.GetKeyDown(Keybinds.BurnToggle))
        {
            if (isBurningMetal)
            {
                StopBurning();
                FlareManager.Instance?.SetBurning(false);
            }
            else
            {
                StartBurning(GetCurrentMetal());
                FlareManager.Instance?.SetBurning(true);
            }
        }

        if (isNicrobursting)
        {
            if (isBurningMetal)
            {
                nicroburstTimer += Time.deltaTime;
                if (nicroburstTimer > MetallurgyConstants.NicroburstDuration)
                {
                    isNicrobursting = false;
                    nicroburstTimer = 0f;
                }
            }
        }
        else
        {
            nicroburstTimer = 0f;
        }

        bool isFlaring = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;
        bool isUsingMetal = isBurningMetal || isFlaring;

        if (isUsingMetal && canBurnMetal)
        {
            float drainRate = baseBurnRate;
            if (isFlaring && FlareManager.Instance != null)
                drainRate += FlareManager.Instance.flareBurnRate;

            if (isDuraluminPrimed)
            {
                float remaining = GetMetalReserve(GetCurrentMetal());
                DrainMetal(GetCurrentMetal(), remaining);
                isDuraluminPrimed = false;
                isNicrobursting = false;
            }
            else
            {
                DrainMetal(GetCurrentMetal(), drainRate * Time.deltaTime);
            }
        }
        else if (!isUsingMetal && metalReserve != null)
        {
            RefillMetal(GetCurrentMetal(), metalReserve.passiveRecoveryRate * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.RightAlt)) RefillAllMetals();

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

    public void StartBurning(MetallurgySkill.MetalType metal)
    {
        if (!unlockedMetals[(int)metal])
        {
            isBurningMetal = false;
            return;
        }

        isBurningMetal = true;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public void StopBurning()
    {
        isBurningMetal = false;
        isNicrobursting = false;
    }

    public bool IsBurning() => isBurningMetal;

    public bool IsMetalBurning(MetallurgySkill.MetalType metal)
    {
        return isBurningMetal && GetCurrentMetal() == metal;
    }

    public void SetCurrentMetal(MetallurgySkill.MetalType metal)
    {
        currentMetal = metal;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public MetallurgySkill.MetalType GetCurrentMetal()
    {
        if (metalSelector != null) return metalSelector.GetActiveMetal();
        return currentMetal;
    }

    public float GetMetalReserve(MetallurgySkill.MetalType metal) => metalReserves[(int)metal];

    public void DrainMetal(MetallurgySkill.MetalType metal, float amount)
    {
        // Metal Efficiency skills reduce drain across all metals (single hook for all 16)
        if (MetallurgicSkillTree.Instance != null)
        {
            float efficiency = MetallurgicSkillTree.Instance.GetMetalEfficiencyBonus(metal);
            amount *= Mathf.Clamp01(1f - efficiency);
        }

        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);
        UpdateHUD(metal);
        canBurnMetal = metalReserves[(int)GetCurrentMetal()] > 0;
    }

    public void RefillMetal(MetallurgySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
        UpdateHUD(metal);
    }

    public void RefillAllMetals()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
            UpdateHUD((MetallurgySkill.MetalType)i);
        }
        canBurnMetal = true;
    }

    public void ClearAllReserves()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 0f;
            UpdateHUD((MetallurgySkill.MetalType)i);
        }
        canBurnMetal = false;
    }

    private void UpdateHUD(MetallurgySkill.MetalType metal)
    {
        if (metalReserve != null)
        {
            metalReserve.UpdateAllBars(metalReserves);
        }
    }
}
