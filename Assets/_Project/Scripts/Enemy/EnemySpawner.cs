using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private int maxActiveEnemies = 10;
        [SerializeField] private float spawnRadius = 20f;
        [SerializeField] private float despawnDistance = 50f;
        
        [Header("Spawn Timing")]
        [SerializeField] private float baseSpawnInterval = 5f;
        [SerializeField] private float minSpawnInterval = 2f;
        [SerializeField] private float spawnIntervalDecreaseRate = 0.1f;
        
        [Header("Wave System")]
        [SerializeField] private bool useWaveSystem = false;
        [SerializeField] private int enemiesPerWave = 5;
        [SerializeField] private float waveInterval = 30f;
        
        [Header("Spawn Conditions")]
        [SerializeField] private float activationDistance = 30f;
        [SerializeField] private LayerMask spawnAreaMask;
        
        private Transform playerTransform;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private float currentSpawnInterval;
        private float lastSpawnTime;
        private int currentWave = 1;
        private float lastWaveTime;
        private bool isActivated = false;
        private int enemiesSpawnedThisWave = 0;
        
        private void Awake()
        {
            currentSpawnInterval = baseSpawnInterval;
            lastSpawnTime = Time.time;
            lastWaveTime = Time.time;
        }
        
        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (playerTransform == null)
            {
                Debug.LogWarning("EnemySpawner: No player found in scene");
            }
        }
        
        private void Update()
        {
            if (!isActivated)
            {
                CheckActivation();
                return;
            }
            
            UpdateSpawning();
            UpdateActiveEnemies();
            
            if (useWaveSystem)
            {
                UpdateWaveSystem();
            }
        }
        
        private void CheckActivation()
        {
            if (playerTransform == null)
            {
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer < activationDistance)
            {
                Activate();
            }
        }
        
        private void Activate()
        {
            isActivated = true;
            
            if (OnSpawnerActivated != null)
            {
                OnSpawnerActivated();
            }
            
            SpawnInitialEnemies();
        }
        
        private void SpawnInitialEnemies()
        {
            int initialSpawnCount = Mathf.Min(3, enemyPrefabs.Length);
            
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnEnemy();
            }
        }
        
        private void UpdateSpawning()
        {
            if (activeEnemies.Count >= maxActiveEnemies)
            {
                return;
            }
            
            if (Time.time - lastSpawnTime >= currentSpawnInterval)
            {
                if (useWaveSystem)
                {
                    if (enemiesSpawnedThisWave < enemiesPerWave)
                    {
                        SpawnEnemy();
                        enemiesSpawnedThisWave++;
                    }
                }
                else
                {
                    SpawnEnemy();
                }
                
                lastSpawnTime = Time.time;
                currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - spawnIntervalDecreaseRate);
            }
        }
        
        private void UpdateWaveSystem()
        {
            if (Time.time - lastWaveTime >= waveInterval)
            {
                StartNewWave();
                lastWaveTime = Time.time;
            }
        }
        
        private void StartNewWave()
        {
            currentWave++;
            enemiesSpawnedThisWave = 0;
            enemiesPerWave += 2;
            
            if (OnNewWaveStarted != null)
            {
                OnNewWaveStarted(currentWave);
            }
        }
        
        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                return;
            }
            
            Vector3 spawnPosition = GetValidSpawnPosition();
            
            if (spawnPosition == Vector3.zero)
            {
                return;
            }
            
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.SetSpawner(this);
            }
            
            activeEnemies.Add(enemy);
            
            if (OnEnemySpawned != null)
            {
                OnEnemySpawned(enemy);
            }
        }
        
        private Vector3 GetValidSpawnPosition()
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = Mathf.Abs(randomOffset.y);
                
                Vector3 potentialPosition = transform.position + randomOffset;
                
                if (Physics.CheckSphere(potentialPosition, 2f, spawnAreaMask))
                {
                    continue;
                }
                
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(potentialPosition, playerTransform.position);
                    if (distanceToPlayer < 5f)
                    {
                        continue;
                    }
                }
                
                return potentialPosition;
            }
            
            return Vector3.zero;
        }
        
        private void UpdateActiveEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = activeEnemies[i];
                
                if (enemy == null)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }
                
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(enemy.transform.position, playerTransform.position);
                    
                    if (distanceToPlayer > despawnDistance)
                    {
                        DespawnEnemy(enemy);
                        activeEnemies.RemoveAt(i);
                    }
                }
                
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null && enemyComponent.IsDead())
                {
                    DespawnEnemy(enemy);
                    activeEnemies.RemoveAt(i);
                }
            }
        }
        
        private void DespawnEnemy(GameObject enemy)
        {
            if (enemy != null)
            {
                if (OnEnemyDespawned != null)
                {
                    OnEnemyDespawned(enemy);
                }
                
                Destroy(enemy);
            }
        }
        
        public void RegisterEnemyDeath(GameObject enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                
                if (OnEnemyKilled != null)
                {
                    OnEnemyKilled(enemy);
                }
            }
        }
        
        public int GetActiveEnemyCount()
        {
            return activeEnemies.Count;
        }
        
        public int GetCurrentWave()
        {
            return currentWave;
        }
        
        public bool IsActivated()
        {
            return isActivated;
        }
        
        public void ResetSpawner()
        {
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            
            activeEnemies.Clear();
            currentSpawnInterval = baseSpawnInterval;
            lastSpawnTime = Time.time;
            currentWave = 1;
            lastWaveTime = Time.time;
            enemiesSpawnedThisWave = 0;
        }
        
        public event System.Action OnSpawnerActivated;
        public event System.Action<int> OnNewWaveStarted;
        public event System.Action<GameObject> OnEnemySpawned;
        public event System.Action<GameObject> OnEnemyDespawned;
        public event System.Action<GameObject> OnEnemyKilled;
    }
}
