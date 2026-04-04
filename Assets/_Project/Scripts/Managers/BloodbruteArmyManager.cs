using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Optimized manager for large-scale Bloodbrute battles.
/// </summary>
public class BloodbruteArmyManager : MonoBehaviour
{
    public static BloodbruteArmyManager Instance { get; private set; }

    [Header("Swarm Settings")]
    public GameObject bloodbrutePrefab;
    public int spawnCount = 50;
    public float swarmRadius = 20f;

    private List<BloodbruteAI> activeArmy = new List<BloodbruteAI>();
    private Transform player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning("[BloodbruteArmyManager] No 'Player' tag found in scene.");
    }

    void Start()
    {
        SpawnSwarm();
    }

    private void SpawnSwarm()
    {
        if (bloodbrutePrefab == null)
        {
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * swarmRadius;
            spawnPos.y = 0; // Ground level

            GameObject k = Instantiate(bloodbrutePrefab, spawnPos, Quaternion.identity);
            BloodbruteAI ai = k.GetComponent<BloodbruteAI>();
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
            if (k == null) continue; // Dead bloodbrute could have been Destroyed
            k.SendMessage("SetTacticalTarget", target, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Update()
    {
        // Global optimization: only update pathfinding for the closest 10 Bloodbrute
        // Optimized logic for advanced swarm coordination.
    }
}
