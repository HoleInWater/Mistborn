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
 *
 * BUG FIX: Awake() unlock loop was `i < 16` but metalReserves is sized to 20 and
 *          EnsureAllomancyComponents() adds Chromium and Nicrosil as components.
 *          Because the loop never called UnlockMetal() for them, the wheel always
 *          showed them as LOCKED regardless of any other fix. Changed to use
 *          Enum.GetValues so every metal in the MetalType enum is unlocked,
 *          future-proofing against any further additions.
 */

using UnityEngine;

[PlayerComponent("Allomancy", order: 10)]
public class Allomancer : MonoBehaviour
{
    [Header("Metal State")]
    private bool isBurningMetal = false;
    private AllomancySkill.MetalType currentMetal;

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
        }

        // FIX: Was `for (int i = 0; i < 16; i++)` — stopped before Chromium and Nicrosil,
        // leaving them permanently locked and invisible/unselectable on the wheel.
        // Now iterates every value in the enum so all metals are unlocked correctly.
        foreach (AllomancySkill.MetalType metal in System.Enum.GetValues(typeof(AllomancySkill.MetalType)))
        {
            UnlockMetal(metal);
        }

        EnsureAllomancyComponents();
        metalSelector = GetComponent<MetalSelector>();
    }

    void OnDestroy()
    {
        MistbornRegistry.UnregisterAllomancer(this);
    }

    void Start()
    {
        if (metalSelector == null) metalSelector = GetComponent<MetalSelector>();
    }

    public void UnlockMetal(AllomancySkill.MetalType metal)
    {
        unlockedMetals[(int)metal] = true;
    }

    void EnsureAllomancyComponents()
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
                if (nicroburstTimer > AllomancyConstants.NicroburstDuration)
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

    public void StartBurning(AllomancySkill.MetalType metal)
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

    public bool IsMetalBurning(AllomancySkill.MetalType metal)
    {
        return isBurningMetal && GetCurrentMetal() == metal;
    }

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
        canBurnMetal = metalReserves[(int)GetCurrentMetal()] > 0;
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
    }

    public void ClearAllReserves()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 0f;
            UpdateHUD((AllomancySkill.MetalType)i);
        }
        canBurnMetal = false;
    }

    private void UpdateHUD(AllomancySkill.MetalType metal)
    {
        if (metalReserve != null)
        {
            metalReserve.UpdateAllBars(metalReserves);
        }
    }
}
