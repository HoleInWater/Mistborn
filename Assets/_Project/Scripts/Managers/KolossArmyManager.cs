using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Optimized manager for large-scale Koloss battles.
/// </summary>
public class KolossArmyManager : MonoBehaviour
{
    public static KolossArmyManager Instance { get; private set; }

    [Header("Swarm Settings")]
    public GameObject kolossPrefab;
    public int spawnCount = 50;
    public float swarmRadius = 20f;

    private List<KolossAI> activeArmy = new List<KolossAI>();
    private Transform player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        SpawnSwarm();
    }

    private void SpawnSwarm()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * swarmRadius;
            spawnPos.y = 0; // Ground level
            
            GameObject k = Instantiate(kolossPrefab, spawnPos, Quaternion.identity);
            KolossAI ai = k.GetComponent<KolossAI>();
            if (ai != null) activeArmy.Add(ai);
        }
    }

    /// <summary>
    /// Directs the entire swarm toward a new tactical objective.
    /// </summary>
    public void CommandSwarm(Vector3 target)
    {
        foreach (var k in activeArmy)
        {
            // Simple messaging for performance
            k.SendMessage("SetTacticalTarget", target, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Update()
    {
        // Global optimization: only update pathfinding for the closest 10 Koloss
        // (Simplified placeholder for advanced DOTS/Jobs logic)
    }
}
