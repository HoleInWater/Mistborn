using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized input manager. Caches all input per frame so other scripts
/// can read it without calling Input multiple times. Supports rebindable keys.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // ── Movement ─────────────────────────────────────────────────────────
    [Header("Movement Input")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool jumpPressed;
    public bool jumpHeld;
    public bool sprintHeld;
    public bool crouchPressed;
    public bool crouchHeld;

    // ── Combat ───────────────────────────────────────────────────────────
    [Header("Combat Input")]
    public bool attackPressed;
    public bool heavyAttackPressed;
    public bool blockHeld;
    public bool parryPressed;
    public bool dodgePressed;

    // ── Allomancy ────────────────────────────────────────────────────────
    [Header("Allomancy Input")]
    public bool pushPressed;
    public bool pushHeld;
    public bool pullPressed;
    public bool pullHeld;
    public bool burnTogglePressed;
    public bool metalWheelHeld;
    public float scrollDelta;

    // ── UI ────────────────────────────────────────────────────────────────
    [Header("UI Input")]
    public bool pausePressed;
    public bool helpPressed;
    public bool interactPressed;
    public bool recoverCoinsPressed;

    // ── Keybindings (rebindable) ─────────────────────────────────────────
    [Header("Key Bindings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode dodgeKey = KeyCode.LeftAlt;
    public KeyCode pushKey = KeyCode.E;
    public KeyCode pullKey = KeyCode.Q;
    public KeyCode burnToggleKey = KeyCode.B;
    public KeyCode metalWheelKey = KeyCode.Tab;
    public KeyCode interactKey = KeyCode.F;
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode helpKey = KeyCode.F1;
    public KeyCode recoverKey = KeyCode.R;
    public KeyCode groundSlamKey = KeyCode.LeftControl;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Update()
    {
<<<<<<< HEAD
        // Movement
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        jumpPressed = Input.GetKeyDown(jumpKey);
        jumpHeld = Input.GetKey(jumpKey);
        sprintHeld = Input.GetKey(sprintKey);
        crouchPressed = Input.GetKeyDown(crouchKey);
        crouchHeld = Input.GetKey(crouchKey);

        // Combat
        attackPressed = Input.GetMouseButtonDown(0);
        heavyAttackPressed = Input.GetMouseButtonDown(1);
        blockHeld = Input.GetKey(interactKey); // F to block
        parryPressed = Input.GetKeyDown(interactKey);
        dodgePressed = Input.GetKeyDown(dodgeKey);

        // Allomancy
        pushPressed = Input.GetKeyDown(pushKey);
        pushHeld = Input.GetKey(pushKey);
        pullPressed = Input.GetKeyDown(pullKey);
        pullHeld = Input.GetKey(pullKey);
        burnTogglePressed = Input.GetKeyDown(burnToggleKey);
        metalWheelHeld = Input.GetKey(metalWheelKey);
        scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        // UI
        pausePressed = Input.GetKeyDown(pauseKey);
        helpPressed = Input.GetKeyDown(helpKey);
        interactPressed = Input.GetKeyDown(interactKey);
        recoverCoinsPressed = Input.GetKeyDown(recoverKey);
    }

    /// <summary>
    /// Rebind a key at runtime. Saves to PlayerPrefs.
    /// </summary>
    public void RebindKey(string actionName, KeyCode newKey)
    {
        switch (actionName)
        {
            case "Jump": jumpKey = newKey; break;
            case "Sprint": sprintKey = newKey; break;
            case "Crouch": crouchKey = newKey; break;
            case "Dodge": dodgeKey = newKey; break;
            case "Push": pushKey = newKey; break;
            case "Pull": pullKey = newKey; break;
            case "BurnToggle": burnToggleKey = newKey; break;
            case "MetalWheel": metalWheelKey = newKey; break;
            case "Interact": interactKey = newKey; break;
        }

        PlayerPrefs.SetInt($"Keybind_{actionName}", (int)newKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load saved keybindings from PlayerPrefs.
    /// </summary>
    public void LoadKeybindings()
    {
        jumpKey = LoadKey("Jump", jumpKey);
        sprintKey = LoadKey("Sprint", sprintKey);
        crouchKey = LoadKey("Crouch", crouchKey);
        dodgeKey = LoadKey("Dodge", dodgeKey);
        pushKey = LoadKey("Push", pushKey);
        pullKey = LoadKey("Pull", pullKey);
        burnToggleKey = LoadKey("BurnToggle", burnToggleKey);
        metalWheelKey = LoadKey("MetalWheel", metalWheelKey);
        interactKey = LoadKey("Interact", interactKey);
    }

    KeyCode LoadKey(string action, KeyCode defaultKey)
    {
        if (PlayerPrefs.HasKey($"Keybind_{action}"))
            return (KeyCode)PlayerPrefs.GetInt($"Keybind_{action}");
        return defaultKey;
=======
        // Always read from Keybinds.* so SettingsManager rebinds take effect immediately.
        // The instance key fields below are kept for legacy inspector overrides but are
        // NOT polled here — Keybinds is the single source of truth.

        // Movement
        moveInput  = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        lookInput  = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        jumpPressed   = Input.GetKeyDown(Keybinds.Jump);
        jumpHeld      = Input.GetKey(Keybinds.Jump);
        sprintHeld    = Input.GetKey(Keybinds.Sprint);
        crouchPressed = Input.GetKeyDown(Keybinds.Crouch);
        crouchHeld    = Input.GetKey(Keybinds.Crouch);

        // Combat
        attackPressed      = Input.GetMouseButtonDown(Keybinds.LightAttack);
        heavyAttackPressed = Input.GetMouseButtonDown(Keybinds.HeavyAttack);
        blockHeld          = Input.GetKey(Keybinds.SteelBubble);
        parryPressed       = Input.GetKeyDown(Keybinds.SteelBubble);
        dodgePressed       = Input.GetKeyDown(Keybinds.DodgeRoll);

        // Allomancy
        pushPressed      = Input.GetKeyDown(Keybinds.SteelPush);
        pushHeld         = Input.GetKey(Keybinds.SteelPush);
        pullPressed      = Input.GetKeyDown(Keybinds.IronPull);
        pullHeld         = Input.GetKey(Keybinds.IronPull);
        burnTogglePressed = Input.GetKeyDown(Keybinds.BurnToggle);
        metalWheelHeld   = Input.GetKey(Keybinds.MetalWheel);
        scrollDelta      = Input.GetAxis("Mouse ScrollWheel");

        // UI
        pausePressed        = Input.GetKeyDown(Keybinds.Pause);
        helpPressed         = Input.GetKeyDown(Keybinds.Help);
        interactPressed     = Input.GetKeyDown(Keybinds.Interact);
        recoverCoinsPressed = Input.GetKeyDown(recoverKey); // no Keybinds entry; keep local field
    }

    /// <summary>
    /// Rebind a key at runtime. Delegates to SettingsManager so the change is
    /// persisted and applied to Keybinds.* in one place.
    /// Also updates this component's legacy instance fields for any code that
    /// still reads them directly.
    /// </summary>
    public void RebindKey(string actionName, KeyCode newKey)
    {
        // Persist via SettingsManager (updates Keybinds.* + PlayerPrefs "KB_" keys)
        SettingsManager.Instance?.SaveKeybind(actionName, newKey);

        // Keep legacy instance fields in sync
        switch (actionName)
        {
            case "Jump":       jumpKey       = newKey; break;
            case "Sprint":     sprintKey     = newKey; break;
            case "Crouch":     crouchKey     = newKey; break;
            case "DodgeRoll":  dodgeKey      = newKey; break;
            case "SteelPush":  pushKey       = newKey; break;
            case "IronPull":   pullKey       = newKey; break;
            case "BurnToggle": burnToggleKey = newKey; break;
            case "MetalWheel": metalWheelKey = newKey; break;
            case "Interact":   interactKey   = newKey; break;
        }
    }

    /// <summary>
    /// Sync legacy instance fields from current Keybinds.* values.
    /// Call after SettingsManager.LoadSettings() if you need the fields current.
    /// </summary>
    public void SyncFromKeybinds()
    {
        jumpKey       = Keybinds.Jump;
        sprintKey     = Keybinds.Sprint;
        crouchKey     = Keybinds.Crouch;
        dodgeKey      = Keybinds.DodgeRoll;
        pushKey       = Keybinds.SteelPush;
        pullKey       = Keybinds.IronPull;
        burnToggleKey = Keybinds.BurnToggle;
        metalWheelKey = Keybinds.MetalWheel;
        interactKey   = Keybinds.SteelBubble;
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }
}
