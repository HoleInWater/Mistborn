// PlayerSetup_Movement.cs — OWNER: movement / parkour devs
//
// Handles: BasicPlayerMove, Sprint, DodgeRoll, CrouchSystem, VaultJump,
//          WallRun, ParkourSystem, GrappleSystem, MovementExtras, PlayerStamina.
// Add new movement components here. Do NOT edit other module files.

using UnityEngine;
using MistbornEditor;

public class PlayerSetup_Movement : IPlayerSetupModule
{
    public string ModuleName  => "Movement";
    public string Description => "BasicPlayerMove, Sprint, DodgeRoll, Crouch, Vault, WallRun, Grapple, Parkour.";

    public void Setup(GameObject player, SetupLog log)
    {
        // Core locomotion
        var move = Util.Ensure<BasicPlayerMove>(player, log);

        // Wire camera references from the hierarchy (best-effort)
        if (move.cameraTransform == null)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                move.cameraTransform = cam.transform;
                log.Info("BasicPlayerMove.cameraTransform → Camera.main");

                // If the camera has a parent (pivot), assign that too
                if (cam.transform.parent != null && cam.transform.parent != player.transform)
                {
                    move.cameraPivot = cam.transform.parent;
                    log.Info("BasicPlayerMove.cameraPivot → Camera.main.parent");
                }
            }
            else
            {
                log.Warn("No Camera.main found — assign cameraTransform manually.");
            }
        }

        // Set ground layer mask
        if (move.groundLayer == 0)
        {
            move.groundLayer = LayerMask.GetMask("Ground", "Default");
            log.Info("BasicPlayerMove.groundLayer → Ground|Default");
        }

        // Additional movement abilities
        Util.Ensure<Sprint>(player, log);
        Util.Ensure<DodgeRoll>(player, log);
        Util.Ensure<CrouchSystem>(player, log);
        Util.Ensure<VaultJump>(player, log);
        Util.Ensure<WallRun>(player, log);
        Util.Ensure<ParkourSystem>(player, log);
        Util.Ensure<GrappleSystem>(player, log);
        Util.Ensure<MovementExtras>(player, log);
        Util.Ensure<PlayerStamina>(player, log);
        Util.Ensure<FallDamage>(player, log);
        Util.Ensure<GroundSlam>(player, log);
    }
}
