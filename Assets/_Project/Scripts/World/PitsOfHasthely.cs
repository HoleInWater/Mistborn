using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class PitsOfHasthely : MonoBehaviour
    {
        [Header("Location Settings")]
        [SerializeField] private string locationName = "Pits of Hasthely";
        [SerializeField] private string locationDescription = "The deep caverns where atium is mined.";
        [SerializeField] private bool isDiscovered = false;
        [SerializeField] private bool requiresQuest = true;
        [SerializeField] private string requiredQuestID = "";
        
        [Header("Atium Mining")]
        [SerializeField] private int baseAtiumYield = 5;
        [SerializeField] private float miningTime = 30f;
        [SerializeField] private int maxDailyYield = 50;
        [SerializeField] private int currentDailyYield = 0;
        
        [Header("Environmental Effects")]
        [SerializeField] private float hazardDamage = 5f;
        [SerializeField] private float hazardInterval = 5f;
        [SerializeField] private bool hasAshHazards = true;
        [SerializeField] private bool hasFallingRocks = true;
        
        [Header("Resources")]
        [SerializeField] private int[] resourceNodes;
        [SerializeField] private int[] resourceAmounts;
        
        [Header("Visual")]
        [SerializeField] private GameObject locationMarker;
        [SerializeField] private ParticleSystem atiumGlowEffect;
        [SerializeField] private AudioClip ambientSound;
        
        [Header("Spawns")]
        [SerializeField] private bool enableEnemySpawns = true;
        [SerializeField] private float enemySpawnRate = 120f;
        [SerializeField] private GameObject[] enemyPrefabs;
        
        private Transform playerTransform;
        private bool isPlayerInside = false;
        private float lastHazardTime = 0f;
        private float lastSpawnTime = 0f;
        private AudioSource audioSource;
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (locationMarker != null)
            {
                locationMarker.SetActive(isDiscovered);
            }
            
            if (atiumGlowEffect != null)
            {
                atiumGlowEffect.Stop();
            }
            
            if (ambientSound != null)
            {
                audioSource.clip = ambientSound;
                audioSource.loop = true;
            }
            
            ResetDailyYield();
        }
        
        private void Update()
        {
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                
                if (distance < 50f && !isPlayerInside)
                {
                    EnterLocation();
                }
                else if (distance >= 50f && isPlayerInside)
                {
                    ExitLocation();
                }
            }
            
            if (isPlayerInside)
            {
                UpdateHazardEffects();
            }
            
            if (enableEnemySpawns && isPlayerInside)
            {
                UpdateEnemySpawning();
            }
        }
        
        private void EnterLocation()
        {
            isPlayerInside = true;
            
            CheckQuestRequirement();
            
            if (isDiscovered)
            {
                ShowLocation();
            }
            else
            {
                DiscoverLocation();
            }
            
            if (atiumGlowEffect != null)
            {
                atiumGlowEffect.Play();
            }
            
            if (ambientSound != null)
            {
                audioSource.Play();
            }
            
            if (OnLocationEntered != null)
            {
                OnLocationEntered(locationName);
            }
        }
        
        private void ExitLocation()
        {
            isPlayerInside = false;
            
            if (atiumGlowEffect != null)
            {
                atiumGlowEffect.Stop();
            }
            
            if (ambientSound != null)
            {
                audioSource.Stop();
            }
            
            if (OnLocationExited != null)
            {
                OnLocationExited(locationName);
            }
        }
        
        private void CheckQuestRequirement()
        {
            if (!requiresQuest)
            {
                return;
            }
            
            QuestJournal journal = playerTransform?.GetComponent<QuestJournal>();
            if (journal != null)
            {
                bool hasQuest = journal.HasQuest(requiredQuestID);
                if (!hasQuest)
                {
                    return;
                }
                
                Quest.Quest? quest = journal.GetQuest(requiredQuestID);
                if (quest.HasValue)
                {
                    journal.UpdateQuestObjective(requiredQuestID, "location_" + locationName);
                }
            }
        }
        
        private void DiscoverLocation()
        {
            isDiscovered = true;
            
            if (locationMarker != null)
            {
                locationMarker.SetActive(true);
            }
            
            if (OnLocationDiscovered != null)
            {
                OnLocationDiscovered(locationName);
            }
        }
        
        private void ShowLocation()
        {
            if (locationMarker != null)
            {
                locationMarker.SetActive(true);
            }
        }
        
        private void UpdateHazardEffects()
        {
            if (Time.time - lastHazardTime >= hazardInterval)
            {
                lastHazardTime = Time.time;
                ApplyHazardDamage();
            }
        }
        
        private void ApplyHazardDamage()
        {
            if (hasAshHazards)
            {
                PlayerStats stats = playerTransform?.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(hazardDamage);
                }
            }
        }
        
        private void UpdateEnemySpawning()
        {
            if (Time.time - lastSpawnTime >= enemySpawnRate)
            {
                lastSpawnTime = Time.time;
                SpawnEnemy();
            }
            
            CleanupDeadEnemies();
        }
        
        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                return;
            }
            
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            
            spawnedEnemies.Add(enemy);
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            if (playerTransform != null)
            {
                Vector3 offset = Random.insideUnitSphere * 30f;
                offset.y = 0;
                return playerTransform.position + offset;
            }
            
            return transform.position + Random.insideUnitSphere * 20f;
        }
        
        private void CleanupDeadEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] == null)
                {
                    spawnedEnemies.RemoveAt(i);
                }
            }
        }
        
        public bool CanMine()
        {
            return isPlayerInside && currentDailyYield < maxDailyYield;
        }
        
        public float MineAtium()
        {
            if (!CanMine())
            {
                return 0f;
            }
            
            float yield = Mathf.Min(baseAtiumYield, maxDailyYield - currentDailyYield);
            currentDailyYield += (int)yield;
            
            if (currentDailyYield >= maxDailyYield)
            {
                if (OnDailyYieldExhausted != null)
                {
                    OnDailyYieldExhausted();
                }
            }
            
            if (OnAtiumMined != null)
            {
                OnAtiumMined((int)yield);
            }
            
            return yield;
        }
        
        public void ResetDailyYield()
        {
            currentDailyYield = 0;
        }
        
        public int GetCurrentYield()
        {
            return currentDailyYield;
        }
        
        public int GetMaxYield()
        {
            return maxDailyYield;
        }
        
        public float GetMiningTime()
        {
            return miningTime;
        }
        
        public void SetMiningTime(float time)
        {
            miningTime = time;
        }
        
        public bool IsDiscovered()
        {
            return isDiscovered;
        }
        
        public void Discover()
        {
            if (!isDiscovered)
            {
                DiscoverLocation();
            }
        }
        
        public string GetLocationName()
        {
            return locationName;
        }
        
        public string GetLocationDescription()
        {
            return locationDescription;
        }
        
        public event System.Action<string> OnLocationEntered;
        public event System.Action<string> OnLocationExited;
        public event System.Action<string> OnLocationDiscovered;
        public event System.Action<int> OnAtiumMined;
        public event System.Action OnDailyYieldExhausted;
    }
}
