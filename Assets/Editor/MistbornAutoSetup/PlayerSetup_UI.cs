// PlayerSetup_UI.cs — OWNER: UI devs
//
// Handles: MetalReserve (wired to UIDocument), PlayerHUD, AllomancyHUD,
//          PlayerStamina UI link, CoinPouch.
// Wires up UIDocument references so the HUD works without manual Inspector linking.

using UnityEngine;
using UnityEngine.UIElements;
using MistbornEditor;

public class PlayerSetup_UI : IPlayerSetupModule
{
    public string ModuleName  => "UI / HUD";
    public string Description => "MetalReserve, AllomancyHUD, PlayerHUD — wires UIDocument reference automatically.";

    public void Setup(GameObject player, SetupLog log)
    {
        // ── MetalReserve ─────────────────────────────────────────────────────
        var reserve = Util.Ensure<MetalReserve>(player, log);

        if (reserve.uiDocument == null)
        {
            // Search the whole scene for a UIDocument that has the HUD panel
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var doc in docs)
            {
                // The HUD UIDocument will have the PrimaryMetalBar or Health element
                if (doc.rootVisualElement == null) continue;
                if (doc.rootVisualElement.Q("PrimaryMetalBar") != null
                 || doc.rootVisualElement.Q("Health") != null)
                {
                    reserve.uiDocument = doc;
                    log.Info($"MetalReserve.uiDocument → {doc.gameObject.name}");
                    break;
                }
            }

            if (reserve.uiDocument == null)
                log.Warn("No HUD UIDocument found in scene — assign MetalReserve.uiDocument manually.");
        }

        // Wire Allomancer → MetalReserve reference
        var allo = player.GetComponent<Allomancer>();
        if (allo != null && allo.metalReserve == null)
        {
            allo.metalReserve = reserve;
            log.Info("Allomancer.metalReserve → MetalReserve");
        }

        // Wire MetalSelector → MetalReserve reference
        var selector = player.GetComponent<MetalSelector>();
        if (selector != null && selector.metalReserve == null)
        {
            selector.metalReserve = reserve;
            log.Info("MetalSelector.metalReserve → MetalReserve");
        }

        // ── HUD components ───────────────────────────────────────────────────
        Util.Ensure<AllomancyHUD>(player, log);
        Util.Ensure<PlayerHUD>(player, log);

        // ── Economy / vials ──────────────────────────────────────────────────
        Util.Ensure<CoinPouch>(player, log);
    }
}
