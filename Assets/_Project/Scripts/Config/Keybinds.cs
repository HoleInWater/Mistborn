using UnityEngine;

/// <summary>
/// SINGLE SOURCE OF TRUTH for all keybinds.
/// These match the ORIGINAL handoff keybinds. Don't change without discussion.
///
/// CONTROLS:
///   E           = Steel Push
///   Q           = Iron Pull
///   F           = Steel Bubble
///   G           = Interact
///   Mouse Right = Block / Parry
///   Z           = Metal Sight toggle
///   B           = Burn toggle
///   Tab         = Metal Wheel / Swap metals
///   Left Ctrl   = Crouch / Toggle flare
///   Left Shift  = Sprint
///   Left Alt    = Dodge Roll
///   Space       = Jump
///   Mouse Left  = Attack
///   Mouse Right = Heavy Attack
///   Mouse Mid   = Lock On / Grapple
///   X           = Drink vial
///   Escape      = Pause
///   F1          = Help
/// </summary>
public static class Keybinds
{
    // Movement (original)
    public static KeyCode Jump = KeyCode.Space;
    public static KeyCode Sprint = KeyCode.LeftShift;
    public static KeyCode Crouch = KeyCode.LeftControl;
    public static KeyCode DodgeRoll = KeyCode.LeftAlt;

    // Allomancy (original from handoff)
    public static KeyCode SteelPush = KeyCode.E;
    public static KeyCode IronPull = KeyCode.Q;
    public static KeyCode SteelBubble = KeyCode.F;
    public static KeyCode MetalSight = KeyCode.T;
    public static KeyCode BurnToggle = KeyCode.B;
    public static KeyCode MetalWheel = KeyCode.Tab;
    public static KeyCode DrinkVial = KeyCode.X;

    // Combat (original)
    public static int LightAttack = 0;
    public static int HeavyAttack = 1;
    public static int LockOnToggle = 2;

    // Interaction
    public static KeyCode Interact = KeyCode.G;
    public static KeyCode Grapple = KeyCode.Mouse2;
    public static KeyCode Block = KeyCode.Mouse1;

    // UI
    public static KeyCode Inventory = KeyCode.I;
    public static KeyCode Journal = KeyCode.J;
    public static KeyCode Pause = KeyCode.Escape;
    public static KeyCode Help = KeyCode.F1;
}
