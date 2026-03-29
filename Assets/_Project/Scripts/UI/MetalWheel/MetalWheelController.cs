///
/// BUG FIX SUMMARY:
/// FIX 1 - instantiatedSlots was MetalWheelSlot[16] and all loops stopped at i < 16,
///         so Chromium (index 16) and Nicrosil (index 17) were never instantiated.
///         Now sized and iterated from metalData.Count.
///
/// FIX 2 - unlockedMetals[] and metalReserves[] were accessed directly as
///         array[(int)metalType] with no bounds check. If those arrays are length 16,
///         indices 16 and 17 throw IndexOutOfRangeException. Unity silently swallows
///         this in Update loops, so: EquipSelectedMetal's safeguard never passed
///         (can't select), and RefreshAllSlots never reached SetState/SetColor
///         (no colors). All accesses now go through SafeIsUnlocked() / SafeGetReserve().
///
/// ACTION REQUIRED: Also resize unlockedMetals and metalReserves on your Allomancer
///                  component to length 18 (or however many metals are in your enum).
///                  Add entries for Chromium and Nicrosil in your metalData list in
///                  the Inspector, assigned to MetalGroup.Enhancement.
///

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum MetalGroup { Physical, Mental, Enhancement, Temporal }

[System.Serializable]
public class MetalSlotData
{
    public AllomancySkill.MetalType metalType;
    public MetalGroup group;
    public Color themeColor = Color.white;
    public Sprite slotIcon;
}

[RequireComponent(typeof(CanvasGroup))]
public class MetalWheelController : MonoBehaviour
{
    [Header("Dependencies")]
    public MetalWheelTimeManager timeManager;
    public MetalWheelInputHandler inputHandler;
    public MetalWheelAudio audioManager;
    public Allomancer playerAllomancer;

    [Header("UI References")]
    public CanvasGroup wheelCanvasGroup;
    public RectTransform centerElement;
    public Image centerGlyph;
    public Text centerMetalName;
    public Transform slotsContainer;

    [Header("Data")]
    public List<MetalSlotData> metalData = new List<MetalSlotData>();
    public MetalWheelSlot slotPrefab;

    // FIX 1: No longer hardcoded to [16] — allocated dynamically in InitializeSlots()
    private MetalWheelSlot[] instantiatedSlots;

    private int currentSlotIndex = 0;
    private int currentGroupIndex = 0;
    private bool isOpen = false;
    private float targetAlpha = 0f;

    void Awake()
    {
        if (wheelCanvasGroup == null) wheelCanvasGroup = GetComponent<CanvasGroup>();

        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);

        wheelCanvasGroup.alpha = 0f;
        wheelCanvasGroup.interactable = false;
        wheelCanvasGroup.blocksRaycasts = false;

        InitializeSlots();
    }

    void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnWheelOpenTriggered += OpenWheel;
            inputHandler.OnWheelCloseTriggered += CloseWheel;
            inputHandler.OnMetalClicked += EquipSelectedMetal;
            inputHandler.OnSwitchGroup += SwitchGroup;
        }
    }

    void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnWheelOpenTriggered -= OpenWheel;
            inputHandler.OnWheelCloseTriggered -= CloseWheel;
            inputHandler.OnMetalClicked -= EquipSelectedMetal;
            inputHandler.OnSwitchGroup -= SwitchGroup;
        }
    }

    private void InitializeSlots()
    {
        int totalMetals = metalData.Count; // FIX 1: was hardcoded 16

        instantiatedSlots = new MetalWheelSlot[totalMetals];
        float radius = 140f;

        for (int i = 0; i < totalMetals; i++) // FIX 1: was i < 16
        {
            MetalSlotData data = metalData[i];
            if (slotPrefab == null) continue;

            MetalWheelSlot newSlot = Instantiate(slotPrefab, slotsContainer);

            float angle = i * (Mathf.PI * 2f / totalMetals) + (Mathf.PI / 2f); // FIX 1: was / 16f
            newSlot.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * -radius, 0f);
            newSlot.Setup(data.themeColor, data.slotIcon);
            instantiatedSlots[i] = newSlot;
        }
    }

    // FIX 2: Bounds-safe helpers replace all direct playerAllomancer.array[metalInt] accesses.
    // Raw access throws IndexOutOfRangeException for metals beyond the original array size of 16.
    // Unity swallows exceptions in Update loops — making the failures completely invisible in console.
    private bool SafeIsUnlocked(int metalInt)
    {
        if (playerAllomancer == null || playerAllomancer.unlockedMetals == null) return false;
        if (metalInt < 0 || metalInt >= playerAllomancer.unlockedMetals.Length) return false;
        return playerAllomancer.unlockedMetals[metalInt];
    }

    private float SafeGetReserve(int metalInt)
    {
        if (playerAllomancer == null || playerAllomancer.metalReserves == null) return 0f;
        if (metalInt < 0 || metalInt >= playerAllomancer.metalReserves.Length) return 0f;
        return playerAllomancer.metalReserves[metalInt];
    }

    private void OpenWheel()
    {
        isOpen = true;
        targetAlpha = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timeManager != null) timeManager.SlowTime();
        if (audioManager != null) audioManager.PlayOpenSound();

        if (playerAllomancer != null)
        {
            AllomancySkill.MetalType active = playerAllomancer.GetCurrentMetal();
            int index = metalData.FindIndex(m => m.metalType == active);
            if (index >= 0)
            {
                currentSlotIndex = index;
                currentGroupIndex = (int)metalData[index].group;
            }
        }

        RefreshAllSlots();
        UpdateCenterDisplay();
    }

    private void CloseWheel(bool confirmSelection, bool asSecondary)
    {
        isOpen = false;
        targetAlpha = 0f;
        wheelCanvasGroup.interactable = false;
        wheelCanvasGroup.blocksRaycasts = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (timeManager != null) timeManager.RestoreTime();
        if (audioManager != null) audioManager.PlayCloseSound(confirmSelection);

        if (confirmSelection)
            EquipSelectedMetal(asSecondary);
    }

    private void EquipSelectedMetal(bool asSecondary)
    {
        if (playerAllomancer == null) return;
        if (currentSlotIndex < 0 || currentSlotIndex >= metalData.Count) return;

        MetalSlotData selectedData = metalData[currentSlotIndex];
        int metalInt = (int)selectedData.metalType;

        // FIX 2: SafeIsUnlocked/SafeGetReserve replace the direct array accesses that
        // were throwing IndexOutOfRangeException for Chromium and Nicrosil, silently
        // preventing ANY equip attempt from succeeding for those two metals.
        if (SafeIsUnlocked(metalInt) && SafeGetReserve(metalInt) > 0)
        {
            MetalSelector selector = playerAllomancer.GetComponent<MetalSelector>();
            if (selector != null)
            {
                AllomancySkill.MetalType targetType = selectedData.metalType;

                if (asSecondary)
                {
                    if (selector.GetPrimaryMetal() == targetType)
                        selector.SetPrimaryMetal(selector.GetSecondaryMetal());
                    selector.SetSecondaryMetal(targetType);
                }
                else
                {
                    if (selector.GetSecondaryMetal() == targetType)
                        selector.SetSecondaryMetal(selector.GetPrimaryMetal());
                    selector.SetPrimaryMetal(targetType);
                }

                if (audioManager != null) audioManager.PlayCloseSound(true);
            }
            else
            {
                playerAllomancer.SetCurrentMetal(selectedData.metalType);
                if (audioManager != null) audioManager.PlayCloseSound(true);
            }
        }
        else
        {
            if (audioManager != null) audioManager.PlayDenySound();
        }
    }

    private void HandleRadialHover()
    {
        if (inputHandler == null) return;

        Vector2 dir = inputHandler.GetRadialDirection();
        if (dir == Vector2.zero) return;

        float maxDot = -2f;
        int bestIndex = currentSlotIndex;

        for (int i = 0; i < instantiatedSlots.Length; i++)
        {
            if (instantiatedSlots[i] == null) continue;

            Vector2 slotDir = ((Vector2)instantiatedSlots[i].transform.localPosition).normalized;
            float dot = Vector2.Dot(dir, slotDir);

            if (dot > maxDot)
            {
                maxDot = dot;
                bestIndex = i;
            }
        }

        if (bestIndex != currentSlotIndex)
        {
            currentSlotIndex = bestIndex;
            currentGroupIndex = (int)metalData[currentSlotIndex].group;

            if (audioManager != null) audioManager.PlayTick(metalData[currentSlotIndex].metalType);
            UpdateCenterDisplay();
        }
    }

    private void SwitchGroup(int delta)
    {
        if (!isOpen) return;

        currentGroupIndex = (currentGroupIndex + delta + 4) % 4;
        MetalGroup targetGroup = (MetalGroup)currentGroupIndex;

        bool found = false;
        for (int i = 0; i < metalData.Count; i++)
        {
            if (metalData[i].group == targetGroup && IsSlotAvailable(i))
            {
                currentSlotIndex = i;
                found = true;
                break;
            }
        }

        if (found)
        {
            if (audioManager != null) audioManager.PlayTick(metalData[currentSlotIndex].metalType);
            RefreshAllSlots();
            UpdateCenterDisplay();
        }
        else
        {
            if (audioManager != null) audioManager.PlayDenySound();
        }
    }

    private bool IsSlotAvailable(int index)
    {
        if (index < 0 || index >= metalData.Count) return false;
        int metalInt = (int)metalData[index].metalType;
        return SafeIsUnlocked(metalInt) && SafeGetReserve(metalInt) > 0f; // FIX 2
    }

    void Update()
    {
        wheelCanvasGroup.alpha = Mathf.Lerp(wheelCanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 15f);
        if (targetAlpha == 0f && wheelCanvasGroup.alpha < 0.02f) wheelCanvasGroup.alpha = 0f;
        if (targetAlpha == 1f && wheelCanvasGroup.alpha > 0.98f) wheelCanvasGroup.alpha = 1f;

        if (targetAlpha > 0.5f) { wheelCanvasGroup.interactable = true; wheelCanvasGroup.blocksRaycasts = true; }

        if (isOpen)
        {
            HandleRadialHover();
            RefreshAllSlots();
        }
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < instantiatedSlots.Length; i++)
        {
            if (instantiatedSlots[i] == null || i >= metalData.Count) continue;

            int metalInt = (int)metalData[i].metalType;

            // FIX 2: Safe accessors — without these, slots 16 and 17 threw
            // IndexOutOfRangeException here every frame, so SetState was never called,
            // leaving Chromium and Nicrosil permanently colorless and unresponsive.
            bool unlocked = SafeIsUnlocked(metalInt);
            float reservePercentage = SafeGetReserve(metalInt) / 100f;
            bool isBurningThis = playerAllomancer != null
                && playerAllomancer.IsBurning()
                && playerAllomancer.GetCurrentMetal() == metalData[i].metalType;

            MetalWheelSlot.SlotState state;
            if (!unlocked)                   state = MetalWheelSlot.SlotState.LOCKED;
            else if (reservePercentage <= 0) state = MetalWheelSlot.SlotState.EMPTY;
            else if (isBurningThis)          state = MetalWheelSlot.SlotState.ACTIVE;
            else if (i == currentSlotIndex)  state = MetalWheelSlot.SlotState.SELECTED;
            else                             state = MetalWheelSlot.SlotState.AVAILABLE;

            instantiatedSlots[i].SetState(state);
            instantiatedSlots[i].SetReserveDisplay(reservePercentage);
        }
    }

    private void UpdateCenterDisplay()
    {
        if (currentSlotIndex >= 0 && currentSlotIndex < metalData.Count)
        {
            if (centerGlyph != null) centerGlyph.sprite = metalData[currentSlotIndex].slotIcon;
            if (centerMetalName != null) centerMetalName.text = metalData[currentSlotIndex].metalType.ToString().ToUpperInvariant();
        }
    }
}
