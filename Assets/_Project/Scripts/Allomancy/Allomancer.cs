/* Allomancer.cs
 *
 * PURPOSE:
 * Core Allomancy system that manages metal reserves, burning state, and coordination
 * with SteelPush, IronPull, and FlareManager.
 *
 * FLARE INTEGRATION:
 * ==================
 * - FlareManager.Instance.FlareIntensity (0–10) is controlled by the scroll wheel.
 * - While burning AND flaring, drain rate = baseBurnRate + FlareManager.flareBurnRate.
 * - canBurnMetal goes false when the active metal reserve hits 0, stopping all abilities.
 *
 * CONTROLS (unchanged):
 * - B key         → Toggle metal burning on/off
 * - R key         → Refill all metal reserves
 * - Scroll wheel  → Adjust flare intensity (handled inside FlareManager)
 * - Left Ctrl     → Toggle max flare / no flare (handled inside FlareManager)
 */

using UnityEngine;

public class Allomancer : MonoBehaviour
{
    [Header("Metal State")]
    bool isBurningMetal = false;
    private AllomancySkill.MetalType currentMetal;

    [Header("Metal Reserves")]
    public float[] metalReserves = new float[16];
    public bool canBurnMetal = true;

    [Header("HUD")]
    public MetalReserve metalReserve;

    [Header("Burn Settings")]
    [Tooltip("Baseline drain rate per second while burning (not flaring).")]
    public float baseBurnRate = 1f;

    // Private references
    private MetalSelector metalSelector;
    private FlareManager flareManager;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Start()
    {
        Debug.Log("[ALLOMANCER] Start()");

        for (int i = 0; i < metalReserves.Length; i++)
            metalReserves[i] = 100f;

        EnsureAllomancyComponents();

        metalSelector = GetComponent<MetalSelector>();
        flareManager  = GetComponent<FlareManager>();

        Debug.Log("[ALLOMANCER] Ready – canBurnMetal=" + canBurnMetal);
    }

    void EnsureAllomancyComponents()
    {
        if (GetComponent<SteelPush>()    == null) gameObject.AddComponent<SteelPush>();
        if (GetComponent<IronPull>()     == null) gameObject.AddComponent<IronPull>();
        if (GetComponent<FlareManager>() == null) gameObject.AddComponent<FlareManager>();
        if (GetComponent<MetalSelector>()== null) gameObject.AddComponent<MetalSelector>();
        if (GetComponent<MetalReserve>() == null) gameObject.AddComponent<MetalReserve>();
    }

    void Update()
    {
        // ── Toggle burn with B key ────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isBurningMetal) StopBurning();
            else                StartBurning(GetCurrentMetal());
        }

        // ── Determine active states ───────────────────────────────────────────
        bool isFlaring     = FlareManager.Instance != null && FlareManager.Instance.IsFlaring;
        bool isUsingMetal  = isBurningMetal || isFlaring;

        if (isUsingMetal && canBurnMetal)
        {
            // flareBurnRate already includes the base burn rate at all intensities.
            // When not flaring, fall back to baseBurnRate alone.
            float drainRate = (isFlaring && FlareManager.Instance != null)
                ? FlareManager.Instance.flareBurnRate
                : baseBurnRate;

            DrainMetal(GetCurrentMetal(), drainRate * Time.deltaTime);
        }
        else if (!isUsingMetal)
        {
            // Passive recovery only when completely idle
            if (metalReserve != null)
                RefillMetal(GetCurrentMetal(), metalReserve.passiveRecoveryRate * Time.deltaTime);
        }

        // ── Refill cheat key ──────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.R))
            RefillAllMetals();
    }

    // ── Public Burning API ────────────────────────────────────────────────────

    public void StartBurning(AllomancySkill.MetalType metal)
    {
        Debug.Log($"[ALLOMANCER] StartBurning({metal})");
        isBurningMetal = true;
        canBurnMetal   = metalReserves[(int)metal] > 0;
    }

    public void StopBurning()
    {
        isBurningMetal = false;
    }

    public bool IsBurning() => isBurningMetal;

    // ── Metal Selection ───────────────────────────────────────────────────────

    public void SetCurrentMetal(AllomancySkill.MetalType metal)
    {
        currentMetal = metal;
        canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public AllomancySkill.MetalType GetCurrentMetal()
    {
        if (metalSelector != null)
            return metalSelector.GetActiveMetal();
        return currentMetal;
    }

    // ── Reserve Management ────────────────────────────────────────────────────

    public float GetMetalReserve(AllomancySkill.MetalType metal)
        => metalReserves[(int)metal];

    public void DrainMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Max(0, metalReserves[(int)metal] - amount);

        if (Time.frameCount % 60 == 0)
            Debug.Log($"[ALLOMANCER] Drain({metal}, {amount:F2}) → {metalReserves[(int)metal]:F1}");

        UpdateHUD(metal);

        if (metal == GetCurrentMetal())
            canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public void RefillMetal(AllomancySkill.MetalType metal, float amount)
    {
        metalReserves[(int)metal] = Mathf.Min(100f, metalReserves[(int)metal] + amount);
        UpdateHUD(metal);

        if (metal == GetCurrentMetal())
            canBurnMetal = metalReserves[(int)metal] > 0;
    }

    public void RefillAllMetals()
    {
        for (int i = 0; i < metalReserves.Length; i++)
        {
            metalReserves[i] = 100f;
            UpdateHUD((AllomancySkill.MetalType)i);
        }
        canBurnMetal = true;
        Debug.Log("[ALLOMANCER] All metals refilled.");
    }

    // ── HUD ───────────────────────────────────────────────────────────────────

    private void UpdateHUD(AllomancySkill.MetalType metal)
    {
        if (metalReserve == null) return;
        if (metal == GetCurrentMetal())
            metalReserve.currentMetal = metalReserves[(int)metal];
    }
}
