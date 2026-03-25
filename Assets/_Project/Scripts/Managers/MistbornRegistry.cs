using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static registry for high-performance entity lookup.
/// Avoids using FindObjectsOfType in Update loops.
/// </summary>
public static class MistbornRegistry
{
    private static List<AIController> activeEnemies = new List<AIController>();
    private static List<AllomanticTarget> activeMetalTargets = new List<AllomanticTarget>();
    private static List<Allomancer> activeAllomancers = new List<Allomancer>();
    private static List<Feruchemist> activeFeruchemists = new List<Feruchemist>();
    private static List<Compounding> activeCompounders = new List<Compounding>();

    public static IReadOnlyList<AIController> ActiveEnemies => activeEnemies;
    public static IReadOnlyList<AllomanticTarget> ActiveMetalTargets => activeMetalTargets;
    public static IReadOnlyList<Allomancer> ActiveAllomancers => activeAllomancers;
    public static IReadOnlyList<Feruchemist> ActiveFeruchemists => activeFeruchemists;
    public static IReadOnlyList<Compounding> ActiveCompounders => activeCompounders;

    public static void RegisterEnemy(AIController ai)
    {
        if (!activeEnemies.Contains(ai)) activeEnemies.Add(ai);
    }

    public static void UnregisterEnemy(AIController ai)
    {
        activeEnemies.Remove(ai);
    }

    public static void RegisterMetalTarget(AllomanticTarget target)
    {
        if (!activeMetalTargets.Contains(target)) activeMetalTargets.Add(target);
    }

    public static void UnregisterMetalTarget(AllomanticTarget target)
    {
        activeMetalTargets.Remove(target);
    }

    public static void RegisterAllomancer(Allomancer allomancer)
    {
        if (!activeAllomancers.Contains(allomancer)) activeAllomancers.Add(allomancer);
    }

    public static void UnregisterAllomancer(Allomancer allomancer)
    {
        activeAllomancers.Remove(allomancer);
    }

    public static void RegisterFeruchemist(Feruchemist feruchemist)
    {
        if (!activeFeruchemists.Contains(feruchemist)) activeFeruchemists.Add(feruchemist);
    }

    public static void UnregisterFeruchemist(Feruchemist feruchemist)
    {
        activeFeruchemists.Remove(feruchemist);
    }

    public static void RegisterCompounder(Compounding compounder)
    {
        if (!activeCompounders.Contains(compounder)) activeCompounders.Add(compounder);
    }

    public static void UnregisterCompounder(Compounding compounder)
    {
        activeCompounders.Remove(compounder);
    }

    public static void ClearAll()
    {
        activeEnemies.Clear();
        activeMetalTargets.Clear();
        activeAllomancers.Clear();
        activeFeruchemists.Clear();
        activeCompounders.Clear();
    }
}
