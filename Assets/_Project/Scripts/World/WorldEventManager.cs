using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class WorldEventManager : MonoBehaviour
    {
        [Header("Event Settings")]
        [SerializeField] private WorldEvent[] availableEvents;
        [SerializeField] private float eventCheckInterval = 60f;
        
        [Header("Active Events")]
        [SerializeField] private int maxConcurrentEvents = 3;
        
        private List<WorldEvent> activeEvents = new List<WorldEvent>();
        private List<WorldEvent> triggeredEvents = new List<WorldEvent>();
        private float lastEventCheck = 0f;
        private Transform playerTransform;
        
        [System.Serializable]
        public class WorldEvent
        {
            public string eventID;
            public string eventName;
            public string description;
            public float triggerChance = 0.1f;
            public float duration = 300f;
            public bool repeatable = true;
            public int requiredDay = 0;
            public int minPlayerLevel = 1;
            public WorldEventEffect[] effects;
        }
        
        [System.Serializable]
        public class WorldEventEffect
        {
            public EffectType effectType;
            public float effectValue = 1f;
            public string targetTag = "";
            public GameObject spawnPrefab;
            public Vector3 spawnOffset;
            public AudioClip soundEffect;
            public string notificationText = "";
        }
        
        public enum EffectType
        {
            SpawnEnemies,
            ChangeWeather,
            ModifySpawnRate,
            GrantBuff,
            ApplyDebuff,
            SpawnLoot,
            TriggerDialogue,
            ActivateQuest
        }
        
        private void Awake()
        {
        }
        
        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        private void Update()
        {
            if (Time.time - lastEventCheck >= eventCheckInterval)
            {
                CheckForNewEvents();
                lastEventCheck = Time.time;
            }
            
            UpdateActiveEvents();
        }
        
        private void CheckForNewEvents()
        {
            if (activeEvents.Count >= maxConcurrentEvents)
            {
                return;
            }
            
            foreach (WorldEvent worldEvent in availableEvents)
            {
                if (!triggeredEvents.Contains(worldEvent) || worldEvent.repeatable)
                {
                    if (CanTriggerEvent(worldEvent))
                    {
                        if (Random.value <= worldEvent.triggerChance)
                        {
                            TriggerEvent(worldEvent);
                        }
                    }
                }
            }
        }
        
        private bool CanTriggerEvent(WorldEvent worldEvent)
        {
            PlayerStats stats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
            if (stats != null)
            {
                if (stats.GetLevel() < worldEvent.minPlayerLevel)
                {
                    return false;
                }
            }
            
            int currentDay = GetCurrentDay();
            if (worldEvent.requiredDay > 0 && currentDay < worldEvent.requiredDay)
            {
                return false;
            }
            
            return true;
        }
        
        private int GetCurrentDay()
        {
            DayNightCycle dayNight = FindObjectOfType<DayNightCycle>();
            if (dayNight != null)
            {
                return dayNight.GetCurrentDay();
            }
            
            return 1;
        }
        
        private void TriggerEvent(WorldEvent worldEvent)
        {
            activeEvents.Add(worldEvent);
            
            if (!worldEvent.repeatable)
            {
                triggeredEvents.Add(worldEvent);
            }
            
            ApplyEventEffects(worldEvent);
            
            if (OnEventStarted != null)
            {
                OnEventStarted(worldEvent);
            }
            
            Invoke(nameof(EndEvent), worldEvent.duration);
        }
        
        private void ApplyEventEffects(WorldEvent worldEvent)
        {
            foreach (WorldEventEffect effect in worldEvent.effects)
            {
                switch (effect.effectType)
                {
                    case EffectType.SpawnEnemies:
                        SpawnEnemiesForEvent(effect);
                        break;
                    case EffectType.ChangeWeather:
                        ChangeWeather(effect);
                        break;
                    case EffectType.ModifySpawnRate:
                        ModifySpawnRate(effect);
                        break;
                    case EffectType.GrantBuff:
                        GrantBuff(effect);
                        break;
                    case EffectType.SpawnLoot:
                        SpawnLootForEvent(effect);
                        break;
                    case EffectType.TriggerDialogue:
                        TriggerDialogue(effect);
                        break;
                }
            }
        }
        
        private void SpawnEnemiesForEvent(WorldEventEffect effect)
        {
            if (effect.spawnPrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = playerTransform.position + effect.spawnOffset;
                GameObject enemy = Instantiate(effect.spawnPrefab, spawnPos, Quaternion.identity);
            }
        }
        
        private void ChangeWeather(WorldEventEffect effect)
        {
            WeatherSystem weather = FindObjectOfType<WeatherSystem>();
            if (weather != null)
            {
                weather.ForceWeather((WeatherSystem.WeatherType)effect.effectValue);
            }
        }
        
        private void ModifySpawnRate(WorldEventEffect effect)
        {
            EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
            foreach (EnemySpawner spawner in spawners)
            {
            }
        }
        
        private void GrantBuff(WorldEventEffect effect)
        {
            if (playerTransform != null)
            {
                PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.ModifyStat(StatType.AttackPower, effect.effectValue);
                }
            }
        }
        
        private void SpawnLootForEvent(WorldEventEffect effect)
        {
            if (effect.spawnPrefab != null && playerTransform != null)
            {
                Vector3 spawnPos = playerTransform.position + effect.spawnOffset;
                Instantiate(effect.spawnPrefab, spawnPos, Quaternion.identity);
            }
        }
        
        private void TriggerDialogue(WorldEventEffect effect)
        {
        }
        
        private void UpdateActiveEvents()
        {
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
            }
        }
        
        private void EndEvent()
        {
            if (activeEvents.Count > 0)
            {
                WorldEvent endedEvent = activeEvents[0];
                activeEvents.RemoveAt(0);
                
                RemoveEventEffects(endedEvent);
                
                if (OnEventEnded != null)
                {
                    OnEventEnded(endedEvent);
                }
            }
        }
        
        private void RemoveEventEffects(WorldEvent worldEvent)
        {
        }
        
        public List<WorldEvent> GetActiveEvents()
        {
            return new List<WorldEvent>(activeEvents);
        }
        
        public bool IsEventActive(string eventID)
        {
            foreach (WorldEvent worldEvent in activeEvents)
            {
                if (worldEvent.eventID == eventID)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        public void ForceEvent(string eventID)
        {
            foreach (WorldEvent worldEvent in availableEvents)
            {
                if (worldEvent.eventID == eventID)
                {
                    TriggerEvent(worldEvent);
                    return;
                }
            }
        }
        
        public void CancelEvent(string eventID)
        {
            for (int i = 0; i < activeEvents.Count; i++)
            {
                if (activeEvents[i].eventID == eventID)
                {
                    RemoveEventEffects(activeEvents[i]);
                    activeEvents.RemoveAt(i);
                    
                    if (OnEventCancelled != null)
                    {
                        OnEventCancelled(activeEvents[i]);
                    }
                    
                    return;
                }
            }
        }
        
        public event System.Action<WorldEvent> OnEventStarted;
        public event System.Action<WorldEvent> OnEventEnded;
        public event System.Action<WorldEvent> OnEventCancelled;
    }
}
