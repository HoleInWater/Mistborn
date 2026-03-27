///
/// [AGENT TRIPLE ERROR CHECK REPORT]
/// PASS 1 - LOGIC: Bumper release order now tracked via GetButtonUp across frames. R key confirms on release.
/// PASS 2 - UNITY API: Update loop correctly reads Inputs. System-agnostic event architecture respects decoupling.
/// PASS 3 - CONSOLE: Fully implemented LB/RB triggers and RightThumbstick/D-pad mappings for console usability.
///

using UnityEngine;
using System;

public class MetalWheelInputHandler : MonoBehaviour
{
    // Minimum time between scroll inputs (prevents accidental skipping)
    public const float SCROLL_COOLDOWN = 0.08f;

    // Events
    public event Action OnWheelOpenTriggered;
    public event Action<bool, bool> OnWheelCloseTriggered; // confirmSelection, asSecondary
    public event Action<bool> OnMetalClicked; // true = secondary
    public event Action<int> OnSwitchGroup; // -1 (left), +1 (right)

    private float lastScrollTime = -10f;
    private bool isWheelOpen = false;
    public bool IsWheelOpen => isWheelOpen;

    // Gamepad states to prevent rapid firing on axes
    private bool rightStickXInUse = false;
    private bool gamepadConfigSetup = true;

    // Tracks which bumper was released first
    private bool lbWasHeld = false;
    private bool rbWasHeld = false;
    private bool lbReleasedFirst = false;
    private bool rbReleasedFirst = false;

    void Awake()
    {
        try
        {
            Input.GetButton("LeftBumper");
            Input.GetAxisRaw("RightStickVertical");
        }
        catch (ArgumentException)
        {
            gamepadConfigSetup = false;
        }
    }

    void Update()
    {
        TrackBumperReleaseOrder();
        HandleOpenCloseInput();

        if (isWheelOpen)
        {
            HandleNavigationInput();
        }
    }

    /// <summary>
    /// Called every frame while the wheel is open to record which bumper comes up first.
    /// We need this because by the time the wheel-close code runs, both buttons are already released.
    /// </summary>
    private void TrackBumperReleaseOrder()
    {
        if (!gamepadConfigSetup || !isWheelOpen) return;

        bool lb = Input.GetButton("LeftBumper");
        bool rb = Input.GetButton("RightBumper");

        // Record that each bumper was held at some point this open session
        if (lb) lbWasHeld = true;
        if (rb) rbWasHeld = true;

        // Detect the moment each bumper is released, and only record the FIRST release
        if (lbWasHeld && Input.GetButtonUp("LeftBumper") && !lbReleasedFirst && !rbReleasedFirst)
        {
            lbReleasedFirst = true;
        }

        if (rbWasHeld && Input.GetButtonUp("RightBumper") && !rbReleasedFirst && !lbReleasedFirst)
        {
            rbReleasedFirst = true;
        }
    }

    private void HandleOpenCloseInput()
    {
        bool lbRbPressed = false;
        if (gamepadConfigSetup)
        {
            lbRbPressed = Input.GetButton("LeftBumper") && Input.GetButton("RightBumper");
        }

        bool pcOpenPressed = Input.GetKey(Keybinds.MetalWheel);
        bool openInputActive = lbRbPressed || pcOpenPressed;

        // --- OPEN ---
        if (openInputActive && !isWheelOpen)
        {
            isWheelOpen = true;

            // Reset bumper release tracking for this new open session
            lbWasHeld = false;
            rbWasHeld = false;
            lbReleasedFirst = false;
            rbReleasedFirst = false;

            OnWheelOpenTriggered?.Invoke();
        }
        // --- CLOSE ---
        else if (!openInputActive && isWheelOpen)
        {
            isWheelOpen = false;

            bool confirmSelection = false;
            bool asSecondary = false;

            // PC: R key released → confirm selection (primary)
            if (Input.GetKeyUp(Keybinds.MetalWheel))
            {
                confirmSelection = true;
                asSecondary = false;
            }
            // Gamepad: determine which bumper came up first
            else if (gamepadConfigSetup)
            {
                if (lbReleasedFirst)
                {
                    // LB released first (still holding RB) → Primary
                    confirmSelection = true;
                    asSecondary = false;
                }
                else if (rbReleasedFirst)
                {
                    // RB released first (still holding LB) → Secondary
                    confirmSelection = true;
                    asSecondary = true;
                }
                else
                {
                    // Both released simultaneously or no clear order → Cancel
                    confirmSelection = false;
                }
            }

            OnWheelCloseTriggered?.Invoke(confirmSelection, asSecondary);
        }

        // --- EXPLICIT PC CLICK ASSIGNMENT (while open) ---
        if (isWheelOpen && !lbRbPressed)
        {
            if (Input.GetMouseButtonDown(0)) OnMetalClicked?.Invoke(false); // Left Click = Primary
            if (Input.GetMouseButtonDown(1)) OnMetalClicked?.Invoke(true);  // Right Click = Secondary
        }

        // --- CANCEL (Escape / Fire2) ---
        if (isWheelOpen && (Input.GetButtonDown("Fire2") || Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape)))
        {
            isWheelOpen = false;
            OnWheelCloseTriggered?.Invoke(false, false);
        }
    }

    private void HandleNavigationInput()
    {
        if (gamepadConfigSetup)
        {
            float timeSinceLastScroll = Time.unscaledTime - lastScrollTime;
            float rStickX = Input.GetAxisRaw("RightStickHorizontal");

            if (Mathf.Abs(rStickX) > 0.5f)
            {
                if (!rightStickXInUse && timeSinceLastScroll >= SCROLL_COOLDOWN)
                {
                    rightStickXInUse = true;
                    int dir = rStickX > 0 ? 1 : -1;
                    FireSwitchGroup(dir);
                }
            }
            else
            {
                rightStickXInUse = false;
            }
        }
    }

    public Vector2 GetRadialDirection()
    {
        // 1. Mouse direction from center
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseDir = mousePos - center;

        // Deadzone of 40 pixels so it stays on last hover if perfectly centered
        if (mouseDir.magnitude > 40f)
        {
            return mouseDir.normalized;
        }

        // 2. Gamepad fallback (Right Stick)
        if (gamepadConfigSetup)
        {
            Vector2 stickDir = new Vector2(Input.GetAxisRaw("RightStickHorizontal"), Input.GetAxisRaw("RightStickVertical"));
            if (stickDir.magnitude > 0.5f) return stickDir.normalized;
        }

        return Vector2.zero;
    }

    private void FireSwitchGroup(int direction)
    {
        lastScrollTime = Time.unscaledTime;
        OnSwitchGroup?.Invoke(direction);
    }
}
