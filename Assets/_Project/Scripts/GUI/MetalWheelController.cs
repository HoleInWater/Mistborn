///
/// [AGENT TRIPLE ERROR CHECK REPORT]
/// PASS 1 - LOGIC: Loops advancing slots safely, skipping empty/locked. Null checks added before modifying ThenBuzzard100's Allomancer properties.
/// PASS 2 - UNITY API: Subscribes and unsubscribes to InputHandler events gracefully. Fades using CanvasGroup. Designed to be slapped onto a Self-Contained Prefab + Canvas.
/// PASS 3 - CONSOLE: Navigates by groups (tabs) without mouse input required. Protects existing Allomancy system.
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
    // Note: reserve, unlocked, and burning states are pulled dynamically from Allomancer
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
    public Allomancer playerAllomancer; // Target player scripts

    [Header("UI References")]
    public CanvasGroup wheelCanvasGroup;
    public RectTransform centerElement;
    public Image centerGlyph;
    public Text centerMetalName;
    public Transform slotsContainer;
    
    [Header("Data")]
    public List<MetalSlotData> metalData = new List<MetalSlotData>();
    public MetalWheelSlot slotPrefab;
    
    private MetalWheelSlot[] instantiatedSlots = new MetalWheelSlot[16];
    private int currentSlotIndex = 0;
    private int currentGroupIndex = 0;
    private bool isOpen = false;

    // Animation states
    private float targetAlpha = 0f;

    void Awake()
    {
        if (wheelCanvasGroup == null) wheelCanvasGroup = GetComponent<CanvasGroup>();
        
        // Ensure game object persists if it's treated as a singleton prefab
        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);
            
        // Initial setup
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
        // For the sake of standard radial menu: arrange slots in a circle
        float radius = 140f; 
        for (int i = 0; i < 16; i++)
        {
            // Fallback generation if visual UI isn't fully assigned in inspector
            if (metalData.Count > i)
            {
                MetalSlotData data = metalData[i];
                if (slotPrefab != null)
                {
                    MetalWheelSlot newSlot = Instantiate(slotPrefab, slotsContainer);
                    float angle = i * (Mathf.PI * 2f / 16f) + (Mathf.PI / 2f); // Start top
                    newSlot.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * -radius, 0f);
                    newSlot.Setup(data.themeColor, data.slotIcon);
                    instantiatedSlots[i] = newSlot;
                }
            }
        }
    }

    private void OpenWheel()
    {
        isOpen = true;
        targetAlpha = 1f;
        
        // Prevent player from looking around while menu is open
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timeManager != null) timeManager.SlowTime();
        if (audioManager != null) audioManager.PlayOpenSound();

        // Try to auto-focus the currently active metal
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
        
        // Restore player mouse look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (timeManager != null) timeManager.RestoreTime();
        if (audioManager != null) audioManager.PlayCloseSound(confirmSelection);

        if (confirmSelection) 
        {
            EquipSelectedMetal(asSecondary);
        }
    }

    private void EquipSelectedMetal(bool asSecondary)
    {
        if (playerAllomancer == null) return;
        if (currentSlotIndex < 0 || currentSlotIndex >= metalData.Count) return;

        MetalSlotData selectedData = metalData[currentSlotIndex];

        // STRICT SAFEGUARD: Ensure unlocked and valid reserve before applying
        if (playerAllomancer.unlockedMetals != null && 
            playerAllomancer.metalReserves != null &&
            playerAllomancer.unlockedMetals[(int)selectedData.metalType] &&
            playerAllomancer.metalReserves[(int)selectedData.metalType] > 0)
        {
            MetalSelector selector = playerAllomancer.GetComponent<MetalSelector>();
            if (selector != null)
            {
                AllomancySkill.MetalType targetType = selectedData.metalType;
                
                if (asSecondary) 
                {
                    if (selector.GetPrimaryMetal() == targetType) {
                        selector.SetPrimaryMetal(selector.GetSecondaryMetal());
                    }
                    selector.SetSecondaryMetal(targetType);
                }
                else 
                {
                    if (selector.GetSecondaryMetal() == targetType) {
                        selector.SetSecondaryMetal(selector.GetPrimaryMetal());
                    }
                    selector.SetPrimaryMetal(targetType);
                }
                
                // Audio feedback for successful click equip
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
        if (dir == Vector2.zero) return; // Deadzone hit, keep current selection

        float maxDot = -2f;
        int bestIndex = currentSlotIndex;
        
        // Find the slot that best matches the mouse/stick direction vector via Dot Product
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
        
        // Connect to UI: allow hovering even if locked/empty so they can view it
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
        
        // Find first available metal in the new group to snap to
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
            // User tried switching to a tab where everything is locked
            if (audioManager != null) audioManager.PlayDenySound();
        }
    }

    private bool IsSlotAvailable(int index)
    {
        if (playerAllomancer == null || index < 0 || index >= metalData.Count) return false;
        
        int metalInt = (int)metalData[index].metalType;
        bool unlocked = playerAllomancer.unlockedMetals != null && playerAllomancer.unlockedMetals[metalInt];
        float reserve = playerAllomancer.metalReserves != null ? playerAllomancer.metalReserves[metalInt] : 0f;
        
        return unlocked && reserve > 0f;
    }

    void Update()
    {
        // Canvas Fading
        wheelCanvasGroup.alpha = Mathf.Lerp(wheelCanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 15f);
        if (targetAlpha == 0f && wheelCanvasGroup.alpha < 0.02f) wheelCanvasGroup.alpha = 0f;
        if (targetAlpha == 1f && wheelCanvasGroup.alpha > 0.98f) wheelCanvasGroup.alpha = 1f;

        if (targetAlpha > 0.5f) { wheelCanvasGroup.interactable = true; wheelCanvasGroup.blocksRaycasts = true; }

        if (isOpen)
        {
            HandleRadialHover();
            RefreshAllSlots(); // Constantly updating to pulse reserves
        }
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < instantiatedSlots.Length; i++)
        {
            if (instantiatedSlots[i] == null || i >= metalData.Count) continue;

            int metalInt = (int)metalData[i].metalType;
            bool unlocked = playerAllomancer != null && playerAllomancer.unlockedMetals[metalInt];
            float reservePercentage = playerAllomancer != null ? playerAllomancer.metalReserves[metalInt] / 100f : 0f;
            bool isBurningThis = playerAllomancer != null && playerAllomancer.IsBurning() && playerAllomancer.GetCurrentMetal() == metalData[i].metalType;

            MetalWheelSlot.SlotState state;
            if (!unlocked) state = MetalWheelSlot.SlotState.LOCKED;
            else if (reservePercentage <= 0) state = MetalWheelSlot.SlotState.EMPTY;
            else if (isBurningThis) state = MetalWheelSlot.SlotState.ACTIVE;
            else if (i == currentSlotIndex) state = MetalWheelSlot.SlotState.SELECTED;
            else state = MetalWheelSlot.SlotState.AVAILABLE;

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
