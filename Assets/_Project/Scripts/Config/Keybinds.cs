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
///   T           = Metal Sight toggle
///   B           = Burn toggle
///   Tab         = Metal Wheel / Swap metals
///   H           = Feruchemy mode toggle
///   Left Ctrl   = Crouch / Toggle flare
///   Left Shift  = Sprint
///   Left Alt    = Dodge Roll
///   Space       = Jump
///   Mouse Left  = Attack
///   Mouse Right = Heavy Attack / Block
///   Mouse Mid   = Lock On / Grapple
///   X           = Drink vial
///   V           = Coin Shotgun
///   C           = Coin Bounce
///   Z           = Coin Trail
///   R           = Coin Recover
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

    // Coin abilities
    public static KeyCode CoinShotgun = KeyCode.V;
    public static KeyCode CoinBounce = KeyCode.C;
    public static KeyCode CoinTrail = KeyCode.Z;
    public static KeyCode CoinRecover = KeyCode.R;

    // Feruchemy (all three are gated behind FeruchemyMode being active)
    public static KeyCode FeruchemyMode = KeyCode.H;
    public static KeyCode FeruchemyStore = KeyCode.Z;    // conflicts w/ CoinTrail when in feruchemy mode
    public static KeyCode FeruchemyTap = KeyCode.X;      // conflicts w/ DrinkVial when in feruchemy mode
    public static KeyCode FeruchemyStopAll = KeyCode.C;  // conflicts w/ CoinBounce when in feruchemy mode

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
