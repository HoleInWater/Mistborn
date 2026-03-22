using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Combat
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy Spawning")]
        [SerializeField] private int maxActiveEnemies = 20;
        [SerializeField] private float spawnCheckInterval = 5f;
        [SerializeField] private float enemySpawnRadius = 50f;
        
        [Header("Difficulty Scaling")]
        [SerializeField] private bool scaleWithPlayerLevel = true;
        [SerializeField] private float healthScalingPerLevel = 0.1f;
        [SerializeField] private float damageScalingPerLevel = 0.05f;
        
        [Header("Combat Tracking")]
        [SerializeField] private int enemiesKilledThisSession = 0;
        [SerializeField] private int totalEnemiesSpawned = 0;
        
        [Header("Active Enemies")]
        [SerializeField] private List<Enemy> activeEnemies = new List<Enemy>();
        [SerializeField] private List<Enemy> eliteEnemies = new List<Enemy>();
        
        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;
        
        private float lastSpawnCheck = 0f;
        private int playerLevel = 1;
        
        public static EnemyManager instance;
        
        public event System.Action<Enemy> OnEnemySpawned;
        public event System.Action<Enemy> OnEnemyKilled;
        public event System.Action OnAllEnemiesCleared;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        private void Start()
        {
            UpdatePlayerLevel();
        }
        
        private void Update()
        {
            if (Time.time - lastSpawnCheck > spawnCheckInterval)
            {
                UpdateEnemyPopulation();
                lastSpawnCheck = Time.time;
            }
            
            UpdateActiveEnemies();
        }
        
        private void UpdatePlayerLevel()
        {
            PlayerStats player = FindObjectOfType<PlayerStats>();
            if (player != null)
            {
                playerLevel = player.Level;
            }
        }
        
        private void UpdateActiveEnemies()
        {
            activeEnemies.RemoveAll(e => e == null || e.CurrentHealth <= 0);
            eliteEnemies.RemoveAll(e => e == null || e.CurrentHealth <= 0);
        }
        
        private void UpdateEnemyPopulation()
        {
            int currentEnemyCount = activeEnemies.Count + eliteEnemies.Count;
            
            if (currentEnemyCount < maxActiveEnemies)
            {
                TrySpawnEnemy();
            }
        }
        
        private void TrySpawnEnemy()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return;
            
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 spawnPosition = GetSpawnPosition(spawnPoint);
            
            if (IsValidSpawnPosition(spawnPosition))
            {
                SpawnEnemy(spawnPosition, false);
            }
        }
        
        private Vector3 GetSpawnPosition(Transform spawnPoint)
        {
            Vector3 offset = Random.insideUnitSphere * enemySpawnRadius;
            offset.y = 0;
            
            Vector3 position = spawnPoint.position + offset;
            
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(position, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            return position;
        }
        
        private bool IsValidSpawnPosition(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 10f);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    return false;
                }
                
                if (col.CompareTag("Enemy"))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void SpawnEnemy(Vector3 position, bool isElite)
        {
            Enemy enemyPrefab = GetRandomEnemyPrefab(isElite);
            if (enemyPrefab == null)
                return;
            
            Enemy enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            
            ApplyDifficultyScaling(enemy);
            
            if (isElite)
            {
                eliteEnemies.Add(enemy);
            }
            else
            {
                activeEnemies.Add(enemy);
            }
            
            totalEnemiesSpawned++;
            
            enemy.OnDeath += OnEnemyDied;
            
            OnEnemySpawned?.Invoke(enemy);
        }
        
        public void SpawnEnemyAtPlayer(float distance, bool isElite)
        {
            Transform player = FindObjectOfType<PlayerStats>()?.transform;
            if (player == null)
                return;
            
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = 0;
            randomDir.Normalize();
            
            Vector3 spawnPos = player.position + randomDir * distance;
            
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }
            
            SpawnEnemy(spawnPos, isElite);
        }
        
        private Enemy GetRandomEnemyPrefab(bool isElite)
        {
            Enemy[] enemies = Resources.LoadAll<Enemy>("Enemies");
            
            if (enemies.Length == 0)
                return null;
            
            List<Enemy> validEnemies = new List<Enemy>();
            
            foreach (var enemy in enemies)
            {
                if (isElite && enemy.tag == "Elite")
                {
                    validEnemies.Add(enemy);
                }
                else if (!isElite)
                {
                    validEnemies.Add(enemy);
                }
            }
            
            if (validEnemies.Count == 0)
                return enemies[Random.Range(0, enemies.Length)];
            
            return validEnemies[Random.Range(0, validEnemies.Count)];
        }
        
        private void ApplyDifficultyScaling(Enemy enemy)
        {
            if (!scaleWithPlayerLevel)
                return;
            
            float healthMultiplier = 1f + (playerLevel - 1) * healthScalingPerLevel;
            float damageMultiplier = 1f + (playerLevel - 1) * damageScalingPerLevel;
            
            enemy.MaxHealth *= healthMultiplier;
            enemy.Damage *= damageMultiplier;
        }
        
        private void OnEnemyDied(Enemy enemy)
        {
            enemiesKilledThisSession++;
            
            activeEnemies.Remove(enemy);
            eliteEnemies.Remove(enemy);
            
            enemy.OnDeath -= OnEnemyDied;
            
            OnEnemyKilled?.Invoke(enemy);
            
            CheckForWaveComplete();
            
            DropLoot(enemy.transform.position);
        }
        
        private void DropLoot(Vector3 position)
        {
            if (Random.value < 0.3f)
            {
                GameObject metalPickup = Resources.Load<GameObject>("Pickups/MetalPickup");
                if (metalPickup != null)
                {
                    Instantiate(metalPickup, position + Vector3.up, Quaternion.identity);
                }
            }
        }
        
        private void CheckForWaveComplete()
        {
            if (activeEnemies.Count == 0 && eliteEnemies.Count == 0)
            {
                OnAllEnemiesCleared?.Invoke();
            }
        }
        
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(enemy.CurrentHealth);
                }
            }
            
            foreach (var enemy in eliteEnemies)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(enemy.CurrentHealth);
                }
            }
            
            activeEnemies.Clear();
            eliteEnemies.Clear();
        }
        
        public void AggroAllEnemies(Transform target)
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetTarget(target);
                    enemy.StartChasing();
                }
            }
            
            foreach (var enemy in eliteEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetTarget(target);
                    enemy.StartChasing();
                }
            }
        }
        
        public int GetActiveEnemyCount()
        {
            return activeEnemies.Count + eliteEnemies.Count;
        }
        
        public int GetEnemiesKilledThisSession()
        {
            return enemiesKilledThisSession;
        }
        
        public int GetTotalEnemiesSpawned()
        {
            return totalEnemiesSpawned;
        }
        
        public List<Enemy> GetActiveEnemies()
        {
            return new List<Enemy>(activeEnemies);
        }
        
        public List<Enemy> GetEliteEnemies()
        {
            return new List<Enemy>(eliteEnemies);
        }
    }
}
