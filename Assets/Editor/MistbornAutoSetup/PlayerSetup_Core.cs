// PlayerSetup_Core.cs — OWNER: shared / rarely changes
//
// Handles: Rigidbody, CapsuleCollider, Player tag, PlayerAutoSetup runtime fixer.
// This file should almost never need editing after initial setup.

using UnityEngine;
using MistbornEditor;

public class PlayerSetup_Core : IPlayerSetupModule
{
    public string ModuleName  => "Core (Rigidbody / Collider)";
    public string Description => "Rigidbody, CapsuleCollider, Player tag, and runtime PlayerAutoSetup fixer.";

    public void Setup(GameObject player, SetupLog log)
    {
        // ── Tag ──────────────────────────────────────────────────────────────
        if (player.tag != "Player")
        {
            player.tag = "Player";
            log.Add("Tag → Player");
        }
        else
        {
            log.Skip("Tag (already Player)");
        }

        // ── Rigidbody ────────────────────────────────────────────────────────
        Rigidbody rb = Util.Ensure<Rigidbody>(player, log);
        rb.mass                    = 80f;
        rb.linearDamping           = 0.5f;
        rb.angularDamping          = 5f;
        rb.collisionDetectionMode  = CollisionDetectionMode.Continuous;
        rb.constraints             = RigidbodyConstraints.FreezeRotation;
        rb.sleepThreshold          = 0f;

        // ── CapsuleCollider ──────────────────────────────────────────────────
        CapsuleCollider col = Util.Ensure<CapsuleCollider>(player, log);
        if (col.height < 0.1f)   // only set if it looks uninitialized
        {
            col.height    = 1.8f;
            col.radius    = 0.4f;
            col.center    = new Vector3(0, 0.9f, 0);
            col.direction = 1; // Y-axis
        }

        // ── Runtime value fixer ──────────────────────────────────────────────
        Util.Ensure<PlayerAutoSetup>(player, log);
    }
}
