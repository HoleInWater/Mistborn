using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static registry for high-performance entity lookup.
/// Avoids using FindObjectsOfType in Update loops.
/// </summary>
public static class AshwalkerRegistry
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticState() => ClearAll();

    private static List<AIController> activeEnemies = new List<AIController>();
    private static List<MetallurgicTarget> activeMetalTargets = new List<MetallurgicTarget>();
    private static List<Metallurgist> activeMetallurgists = new List<Metallurgist>();
    private static List<Storecrafter> activeStorecrafters = new List<Storecrafter>();
    private static List<Compounding> activeCompounders = new List<Compounding>();

    public static IReadOnlyList<AIController> ActiveEnemies => activeEnemies;
    public static IReadOnlyList<MetallurgicTarget> ActiveMetalTargets => activeMetalTargets;
    public static IReadOnlyList<Metallurgist> ActiveMetallurgists => activeMetallurgists;
    public static IReadOnlyList<Storecrafter> ActiveStorecrafters => activeStorecrafters;
    public static IReadOnlyList<Compounding> ActiveCompounders => activeCompounders;

    public static void RegisterEnemy(AIController ai)
    {
        if (!activeEnemies.Contains(ai)) activeEnemies.Add(ai);
    }

    public static void UnregisterEnemy(AIController ai)
    {
        activeEnemies.Remove(ai);
    }

    public static void RegisterMetalTarget(MetallurgicTarget target)
    {
        if (!activeMetalTargets.Contains(target)) activeMetalTargets.Add(target);
    }

    public static void UnregisterMetalTarget(MetallurgicTarget target)
    {
        activeMetalTargets.Remove(target);
    }

    public static void RegisterMetallurgist(Metallurgist metallurgist)
    {
        if (!activeMetallurgists.Contains(metallurgist)) activeMetallurgists.Add(metallurgist);
    }

    public static void UnregisterMetallurgist(Metallurgist metallurgist)
    {
        activeMetallurgists.Remove(metallurgist);
    }

    public static void RegisterStorecrafter(Storecrafter storecrafter)
    {
        if (!activeStorecrafters.Contains(storecrafter)) activeStorecrafters.Add(storecrafter);
    }

    public static void UnregisterStorecrafter(Storecrafter storecrafter)
    {
        activeStorecrafters.Remove(storecrafter);
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
        activeMetallurgists.Clear();
        activeStorecrafters.Clear();
        activeCompounders.Clear();
    }
}
