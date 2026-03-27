using UnityEngine;
using System.Collections;

/// <summary>
/// Handles metal selection via scroll wheel for Allomancy system
/// Supports two-metal selection for quick switching (primary and secondary)
/// </summary>
public class MetalSelector : MonoBehaviour
{
    [Header("Selection")]
    public float scrollCooldown = 0.2f;
    private float scrollTimer = 0f;

    [Header("Two-Metal Selection")]
    public KeyCode swapMetalsKey = KeyCode.Tab;

    [Header("References")]
    public Allomancer allomancer;
    public MetalReserve metalReserve;
    public MetalWheelInputHandler metalWheelInputHandler;

    private AllomancySkill.MetalType primaryMetal;
    private AllomancySkill.MetalType secondaryMetal;
    private bool isPrimaryActive = true;

    void Start()
    {
        if (allomancer == null)
            allomancer = GetComponent<Allomancer>();

        if (metalReserve == null)
            metalReserve = GetComponentInParent<MetalReserve>();

        if (metalWheelInputHandler == null)
            metalWheelInputHandler = FindObjectOfType<MetalWheelInputHandler>();

        // Subscribe to wheel open so we can flush any pending scroll input
        if (metalWheelInputHandler != null)
            metalWheelInputHandler.OnWheelOpenTriggered += OnWheelOpened;

        primaryMetal = AllomancySkill.MetalType.Steel;
        secondaryMetal = AllomancySkill.MetalType.Iron;
        isPrimaryActive = true;

        UpdateActiveMetal();
    }

    void OnDestroy()
    {
        if (metalWheelInputHandler != null)
            metalWheelInputHandler.OnWheelOpenTriggered -= OnWheelOpened;
    }

    private void OnWheelOpened()
    {
        // Force cooldown so any scroll input that triggered the wheel open
        // doesn't immediately fire a metal change
        scrollTimer = scrollCooldown;
    }

    void Update()
    {
        bool wheelIsOpen = metalWheelInputHandler != null && metalWheelInputHandler.IsWheelOpen;

        if (!wheelIsOpen)
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scrollTimer <= 0f)
            {
                if (scroll > 0f) { SelectNextMetal();     scrollTimer = scrollCooldown; }
                if (scroll < 0f) { SelectPreviousMetal(); scrollTimer = scrollCooldown; }
            }
            if (scrollTimer > 0f) scrollTimer -= Time.deltaTime;
        }
        else
        {
            // Keep draining the timer while open so scroll can't fire the moment it closes
            if (scrollTimer > 0f) scrollTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(swapMetalsKey))
            SwapMetals();
    }

    void SelectNextMetal()
    {
        if (allomancer == null) return;

        AllomancySkill.MetalType[] allMetals = (AllomancySkill.MetalType[])System.Enum.GetValues(typeof(AllomancySkill.MetalType));
        AllomancySkill.MetalType currentMetal = isPrimaryActive ? primaryMetal : secondaryMetal;

        int currentIndex = System.Array.IndexOf(allMetals, currentMetal);
        int nextIndex = (currentIndex + 1) % allMetals.Length;
        AllomancySkill.MetalType nextMetal = allMetals[nextIndex];

        if (isPrimaryActive) primaryMetal = nextMetal;
        else secondaryMetal = nextMetal;

        UpdateActiveMetal();
    }

    void SelectPreviousMetal()
    {
        if (allomancer == null) return;

        AllomancySkill.MetalType[] allMetals = (AllomancySkill.MetalType[])System.Enum.GetValues(typeof(AllomancySkill.MetalType));
        AllomancySkill.MetalType currentMetal = isPrimaryActive ? primaryMetal : secondaryMetal;

        int currentIndex = System.Array.IndexOf(allMetals, currentMetal);
        int prevIndex = (currentIndex - 1 + allMetals.Length) % allMetals.Length;
        AllomancySkill.MetalType prevMetal = allMetals[prevIndex];

        if (isPrimaryActive) primaryMetal = prevMetal;
        else secondaryMetal = prevMetal;

        UpdateActiveMetal();
    }

    void SwapMetals()
    {
        isPrimaryActive = !isPrimaryActive;
        UpdateActiveMetal();
    }

    void UpdateActiveMetal()
    {
        if (allomancer == null) return;

        AllomancySkill.MetalType activeMetal = isPrimaryActive ? primaryMetal : secondaryMetal;
        allomancer.SetCurrentMetal(activeMetal);
    }

    public AllomancySkill.MetalType GetPrimaryMetal() => primaryMetal;
    public AllomancySkill.MetalType GetSecondaryMetal() => secondaryMetal;
    public AllomancySkill.MetalType GetActiveMetal() => isPrimaryActive ? primaryMetal : secondaryMetal;
    public bool IsPrimaryActive() => isPrimaryActive;

    public void SetPrimaryActive(bool primaryStatus)
    {
        if (isPrimaryActive != primaryStatus)
        {
            isPrimaryActive = primaryStatus;
            UpdateActiveMetal();
        }
    }

    public void SetPrimaryMetal(AllomancySkill.MetalType metal)
    {
        primaryMetal = metal;
        if (isPrimaryActive) UpdateActiveMetal();
    }

    public void SetSecondaryMetal(AllomancySkill.MetalType metal)
    {
        secondaryMetal = metal;
        if (!isPrimaryActive) UpdateActiveMetal();
    }
}
