// PlayerSetup_Allomancy.cs — OWNER: Allomancy / metals devs (Landon)
//
// Handles: Allomancer, all 16 metal components, FlareManager, MetalSelector,
//          MetalLineRenderer, AllomanticSight, RadialMetalMenu.
// Allomancer.EnsureAllomancyComponents() also runs at runtime so this is
// belt-and-suspenders — gets references wired up in the Editor too.

using UnityEngine;
using MistbornEditor;

public class PlayerSetup_Allomancy : IPlayerSetupModule
{
    public string ModuleName  => "Allomancy";
    public string Description => "Allomancer, all 16 metals, FlareManager, MetalSelector, Radial wheel.";

    public void Setup(GameObject player, SetupLog log)
    {
        // Allomancer is the core — adding it will also call EnsureAllomancyComponents
        // in Awake at runtime. In the Editor we add everything explicitly so
        // references are visible in the Inspector immediately.
        var allo = Util.Ensure<Allomancer>(player, log);

        // Physical metals
        Util.Ensure<SteelPush>(player, log);
        Util.Ensure<IronPull>(player, log);
        Util.Ensure<Pewter>(player, log);
        Util.Ensure<Tin>(player, log);

        // Mental metals
        Util.Ensure<Zinc>(player, log);
        Util.Ensure<Brass>(player, log);
        Util.Ensure<Copper>(player, log);
        Util.Ensure<Bronze>(player, log);

        // God metals
        Util.Ensure<Atium>(player, log);
        Util.Ensure<Malatium>(player, log);
        Util.Ensure<Gold>(player, log);
        Util.Ensure<Electrum>(player, log);

        // Enhancement metals
        Util.Ensure<Aluminum>(player, log);
        Util.Ensure<Duralumin>(player, log);
        Util.Ensure<Chromium>(player, log);
        Util.Ensure<Nicrosil>(player, log);

        // Temporal metals
        Util.Ensure<Bendalloy>(player, log);
        Util.Ensure<Cadmium>(player, log);

        // Allomancy support systems
        Util.Ensure<FlareManager>(player, log);

        var selector = Util.Ensure<MetalSelector>(player, log);
        if (allo != null && selector.allomancer == null)
        {
            selector.allomancer = allo;
            log.Info("MetalSelector.allomancer → Allomancer");
        }

        Util.Ensure<MetalLineRenderer>(player, log);
        Util.Ensure<AllomanticSight>(player, log);
        Util.Ensure<RadialMetalMenu>(player, log);
        Util.Ensure<MetalMagnet>(player, log);

        // Vial system (metal refill)
        Util.Ensure<MetalVialSystem>(player, log);
    }
}
