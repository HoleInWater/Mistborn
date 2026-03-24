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

    public static IReadOnlyList<AIController> ActiveEnemies => activeEnemies;
    public static IReadOnlyList<AllomanticTarget> ActiveMetalTargets => activeMetalTargets;

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
}
