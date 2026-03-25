using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies at runtime with wave support.
/// Can spawn mixed enemy types for dynamic encounters.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public int maxEnemies = 8;
    public float spawnInterval = 10f;
    public float spawnRadius = 15f;
    public bool spawnOnStart = true;
    public int initialSpawnCount = 3;

    [Header("Wave Settings")]
    public bool useWaves = false;
    public int enemiesPerWave = 5;
    public float waveInterval = 30f;
    public float difficultyScalePerWave = 1.15f;

    [Header("References")]
    public Transform spawnCenter;

    private List<EnemyAI> activeEnemies = new List<EnemyAI>();
    private float spawnTimer;
    private float waveTimer;
    private int currentWave = 0;

    void Start()
    {
        if (spawnCenter == null) spawnCenter = transform;

        if (spawnOnStart)
        {
            for (int i = 0; i < initialSpawnCount; i++)
                SpawnEnemy();
        }
    }

    void Update()
    {
        CleanUpDead();

        if (!useWaves)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    SpawnEnemy();
                    spawnTimer = 0f;
                }
            }
        }
        else
        {
            if (activeEnemies.Count == 0)
            {
                waveTimer += Time.deltaTime;
                if (waveTimer >= waveInterval)
                {
                    StartWave();
                    waveTimer = 0f;
                }
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) activeEnemies.Add(ai);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = spawnCenter.position + new Vector3(circle.x, 0, circle.y);

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, spawnRadius, UnityEngine.AI.NavMesh.AllAreas))
            return hit.position;
        return pos;
    }

    void StartWave()
    {
        currentWave++;
        int count = Mathf.RoundToInt(enemiesPerWave * Mathf.Pow(difficultyScalePerWave, currentWave - 1));

        for (int i = 0; i < count && activeEnemies.Count < maxEnemies; i++)
            SpawnEnemy();

        EventManager.TriggerEvent("WaveStarted");
    }

    void CleanUpDead()
    {
        activeEnemies.RemoveAll(e => e == null || e.GetState() == EnemyAI.State.Dead);
    }

    public int GetActiveEnemyCount() => activeEnemies.Count;
    public int GetCurrentWave() => currentWave;
}
