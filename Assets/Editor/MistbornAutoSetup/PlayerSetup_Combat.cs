// PlayerSetup_Combat.cs — OWNER: combat devs (Garrett)
//
// Handles: PlayerCombat, ComboSystem, LockOnSystem, BlockAbility,
//          StealthSystem, StatusEffects, PlayerExperience, Inventory.
// Add new combat/progression components here. Do NOT edit other module files.

using UnityEngine;
using MistbornEditor;

public class PlayerSetup_Combat : IPlayerSetupModule
{
    public string ModuleName  => "Combat";
    public string Description => "PlayerCombat, ComboSystem, LockOn, Block, Stealth, StatusEffects, XP, Inventory.";

    public void Setup(GameObject player, SetupLog log)
    {
        // Core combat
        Util.Ensure<PlayerCombat>(player, log);
        Util.Ensure<ComboSystem>(player, log);

        // Parry / block
        Util.Ensure<BlockAbility>(player, log);

        // Lock-on targeting
        var lockOn = Util.Ensure<LockOnSystem>(player, log);
        if (lockOn.playerCamera == null)
        {
            lockOn.playerCamera = Camera.main;
            if (lockOn.playerCamera != null)
                log.Info("LockOnSystem.playerCamera → Camera.main");
        }

        // Stealth
        Util.Ensure<StealthSystem>(player, log);

        // Status effects (burn, bleed, fear, etc.)
        Util.Ensure<StatusEffects>(player, log);

        // Progression
        Util.Ensure<PlayerExperience>(player, log);
        Util.Ensure<Inventory>(player, log);

        // Misc player systems
        Util.Ensure<PlayerInteractor>(player, log);
        Util.Ensure<PlayerRagdoll>(player, log);
    }
}
