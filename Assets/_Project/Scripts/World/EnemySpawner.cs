using UnityEngine;
using System.Collections.Generic;
using Mistborn.Enemy;

namespace Mistborn.World
{
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnWave
        {
            public string waveName;
            public List<EnemyType> enemies = new List<EnemyType>();
            public int[] quantities;
            public float delayBetweenSpawns = 1f;
            public float delayBeforeWave = 3f;
        }

        [System.Serializable]
        public class EnemyType
        {
            public string enemyId;
            public GameObject prefab;
            public float spawnWeight = 1f;
        }

        [Header("Spawn Settings")]
        [SerializeField] private List<EnemyType> m_enemyTypes = new List<EnemyType>();
        [SerializeField] private List<SpawnWave> m_waves = new List<SpawnWave>();
        [SerializeField] private int m_maxActiveEnemies = 10;
        [SerializeField] private float m_spawnRadius = 5f;
        [SerializeField] private bool m_loopWaves = true;
        [SerializeField] private bool m_spawnOnStart = false;

        [Header("Advanced")]
        [SerializeField] private float m_detectionRadius = 30f;
        [SerializeField] private bool m_activateOnPlayerNear = true;
        [SerializeField] private bool m_respawnOnEmpty = true;
        [SerializeField] private float m_respawnDelay = 5f;

        private List<GameObject> m_activeEnemies = new List<GameObject>();
        private int m_currentWaveIndex;
        private bool m_isSpawning;
        private bool m_isActivated;
        private float m_spawnTimer;
        private float m_waveDelayTimer;
        private Transform m_player;
        private int m_enemiesSpawnedThisWave;

        public event System.Action<int> OnWaveStarted;
        public event System.Action OnAllWavesComplete;
        public event System.Action OnSpawnedEnemy;

        private void Awake()
        {
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void Start()
        {
            if (m_spawnOnStart && !m_activateOnPlayerNear)
            {
                StartSpawning();
            }
        }

        private void Update()
        {
            if (!m_isActivated) return;

            CleanupDeadEnemies();

            if (m_activateOnPlayerNear && m_player != null)
            {
                float dist = Vector3.Distance(transform.position, m_player.position);
                if (dist < m_detectionRadius)
                {
                    m_isActivated = true;
                }
            }

            if (!m_isSpawning && m_respawnOnEmpty && m_activeEnemies.Count == 0 && m_currentWaveIndex >= m_waves.Count)
            {
                m_waveDelayTimer -= Time.deltaTime;
                if (m_waveDelayTimer <= 0)
                {
                    RestartWaves();
                }
            }

            if (m_isSpawning)
            {
                UpdateSpawning();
            }
        }

        public void StartSpawning()
        {
            m_isActivated = true;
            m_currentWaveIndex = 0;
            m_isSpawning = true;
            OnWaveStarted?.Invoke(m_currentWaveIndex);
        }

        public void StopSpawning()
        {
            m_isSpawning = false;
        }

        public void RestartWaves()
        {
            m_currentWaveIndex = 0;
            m_isSpawning = true;
            OnWaveStarted?.Invoke(m_currentWaveIndex);
        }

        private void UpdateSpawning()
        {
            if (m_currentWaveIndex >= m_waves.Count)
            {
                CompleteWave();
                return;
            }

            SpawnWave currentWave = m_waves[m_currentWaveIndex];

            if (m_enemiesSpawnedThisWave < GetWaveEnemyCount(currentWave))
            {
                m_spawnTimer -= Time.deltaTime;
                if (m_spawnTimer <= 0 && m_activeEnemies.Count < m_maxActiveEnemies)
                {
                    SpawnEnemy(currentWave);
                    m_spawnTimer = currentWave.delayBetweenSpawns;
                }
            }
            else if (m_enemiesSpawnedThisWave >= GetWaveEnemyCount(currentWave))
            {
                m_isSpawning = false;
                m_currentWaveIndex++;

                if (m_currentWaveIndex < m_waves.Count)
                {
                    m_waveDelayTimer = m_waves[m_currentWaveIndex].delayBeforeWave;
                }
                else
                {
                    CompleteWave();
                }
            }
        }

        private void SpawnEnemy(SpawnWave wave)
        {
            if (wave.enemies.Count == 0) return;

            int spawnIndex = GetWeightedRandomEnemy(wave);
            if (spawnIndex < 0 || spawnIndex >= wave.enemies.Count) return;

            EnemyType enemyType = wave.enemies[spawnIndex];
            if (enemyType.prefab == null) return;

            Vector3 spawnPos = GetSpawnPosition();
            GameObject enemy = Instantiate(enemyType.prefab, spawnPos, Quaternion.identity);
            m_activeEnemies.Add(enemy);
            m_enemiesSpawnedThisWave++;

            SetupEnemy(enemy, enemyType);

            OnSpawnedEnemy?.Invoke();
        }

        private int GetWeightedRandomEnemy(SpawnWave wave)
        {
            float totalWeight = 0f;
            foreach (EnemyType enemy in wave.enemies)
            {
                totalWeight += enemy.spawnWeight;
            }

            float random = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < wave.enemies.Count; i++)
            {
                currentWeight += wave.enemies[i].spawnWeight;
                if (random <= currentWeight)
                    return i;
            }

            return wave.enemies.Count - 1;
        }

        private int GetWaveEnemyCount(SpawnWave wave)
        {
            if (wave.quantities == null || wave.quantities.Length == 0)
                return wave.enemies.Count;

            int total = 0;
            foreach (int q in wave.quantities)
            {
                total += q;
            }
            return total;
        }

        private Vector3 GetSpawnPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y) * m_spawnRadius;
            Vector3 spawnPos = transform.position + offset;

            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                spawnPos = hit.point;
            }
            else
            {
                spawnPos.y = 0.5f;
            }

            return spawnPos;
        }

        private void SetupEnemy(GameObject enemy, EnemyType enemyType)
        {
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.OnDeath += () => OnEnemyDeath(enemy);
            }
        }

        private void OnEnemyDeath(GameObject enemy)
        {
            m_activeEnemies.Remove(enemy);
        }

        private void CleanupDeadEnemies()
        {
            m_activeEnemies.RemoveAll(e => e == null);
        }

        private void CompleteWave()
        {
            m_isSpawning = false;

            if (m_currentWaveIndex >= m_waves.Count)
            {
                OnAllWavesComplete?.Invoke();

                if (m_loopWaves)
                {
                    m_waveDelayTimer = 3f;
                }
            }
        }

        public int GetActiveEnemyCount()
        {
            return m_activeEnemies.Count;
        }

        public int GetCurrentWave()
        {
            return m_currentWaveIndex + 1;
        }

        public int GetTotalWaves()
        {
            return m_waves.Count;
        }

        public bool IsSpawning()
        {
            return m_isSpawning;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_spawnRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_detectionRadius);
        }
    }
}
