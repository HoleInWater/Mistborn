using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks first-time Metallurgy usage and shows contextual tutorial tips.
/// Integrates with TutorialSystem for display.
/// </summary>
public class MetallurgyTutorial : MonoBehaviour
{
    [Header("References")]
    public Metallurgist metallurgist;
    public SteelPush steelPush;
    public IronPull ironPull;

    private HashSet<string> shownTips = new HashSet<string>();
    private float checkTimer;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
        if (steelPush == null) steelPush = GetComponent<SteelPush>();
        if (ironPull == null) ironPull = GetComponent<IronPull>();

        // Show initial tip
        ShowTip("welcome", "Welcome, Ashwalker. You have Snapped.\n" +
            "Press E to Steel Push metals away.\n" +
            "Press Q to Iron Pull metals toward you.\n" +
            "Press T to toggle Metal Sight (see nearby metals).");
    }

    void Update()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = 0.5f;

        // Check for first-time actions
        if (Input.GetKey(Keybinds.SteelPush))
            ShowTip("first_push", "Steel Push: You push metals AWAY from you.\n" +
                "If the metal is heavier (anchored to a wall), YOU move instead.\n" +
                "Push off coins on the ground to launch yourself upward!");

        if (Input.GetKey(Keybinds.IronPull))
            ShowTip("first_pull", "Iron Pull: You pull metals TOWARD you.\n" +
                "Pull toward a heavy metal anchor to yank yourself across gaps.\n" +
                "Light metals (coins) fly toward you instead.");

        if (Input.GetKey(Keybinds.MetalSight))
            ShowTip("first_sight", "Metal Sight: Blue lines show all nearby metals.\n" +
                "The closest metal glows dark blue.\n" +
                "Push/Pull always targets the closest metal.");

        if (Input.GetKey(Keybinds.SteelBubble))
            ShowTip("first_bubble", "Steel Bubble: Pushes ALL metals around you at once.\n" +
                "Great for clearing a room of coins and metal debris.");

        if (Input.GetKey(Keybinds.BurnToggle))
            ShowTip("first_burn", "Burning: Toggle a metal on to gain its passive effects.\n" +
                "Pewter = strength, Tin = enhanced senses.\n" +
                "Use the scroll wheel to select which metal to burn.");

        if (metallurgist != null && metallurgist.GetMetalReserve(MetallurgySkill.MetalType.Steel) < 20f)
            ShowTip("low_steel", "Your Steel reserve is running low!\n" +
                "Press X to drink a metal vial and replenish.");

        if (Input.GetKey(Keybinds.Crouch))
            ShowTip("crouch", "Crouching makes you harder to detect by enemies.\n" +
                "Combine with Copper burning to become nearly invisible to Seekers.");
    }

    void ShowTip(string id, string message)
    {
        if (shownTips.Contains(id)) return;
        shownTips.Add(id);
        TutorialSystem.Instance?.ShowTip(id, message);
    }
}
