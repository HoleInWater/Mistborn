using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.World
{
    public class LocationSystem : MonoBehaviour
    {
        [Header("Location Data")]
        [SerializeField] private string locationName = "Khaldan Keep";
        [SerializeField] private string locationDescription = "A noble house compound in the Central Dominance";
        [SerializeField] private LocationType locationType = LocationType.CityDistrict;
        [SerializeField] private float locationSize = 500f;
        
        [Header("Faction Control")]
        [SerializeField] private Faction controllingFaction;
        [SerializeField] private float factionInfluence = 100f;
        [SerializeField] private float influenceDecayRate = 1f;
        
        [Header("Discovery")]
        [SerializeField] private bool requiresDiscovery = true;
        [SerializeField] private float discoveryRadius = 100f;
        [SerializeField] private bool showOnMapOnDiscovery = true;
        
        [Header("Events")]
        [SerializeField] private LocationEvent[] locationEvents;
        [SerializeField] private float eventTriggerDistance = 50f;
        
        [Header("Ambient")]
        [SerializeField] private AudioClip ambientMusic;
        [SerializeField] private AudioClip ambientSound;
        [SerializeField] private ParticleSystem ambientParticles;
        [SerializeField] private Color skyboxTint = Color.white;
        
        [Header("Map Markers")]
        [SerializeField] private MapMarker mapMarkerPrefab;
        [SerializeField] private bool isOnMap = false;
        [SerializeField] private bool requiresMinimap = true;
        
        private bool isDiscovered = false;
        private bool isPlayerInLocation = false;
        private float currentInfluence;
        private List<string> discoveredSecrets = new List<string>();
        private Dictionary<string, bool> visitedTimes = new Dictionary<string, bool>();
        private MapMarker currentMapMarker;
        
        public string LocationName => locationName;
        public string LocationDescription => locationDescription;
        public LocationType Type => locationType;
        public bool IsDiscovered => isDiscovered;
        public Faction ControllingFaction => controllingFaction;
        
        private void Start()
        {
            currentInfluence = factionInfluence;
            
            if (requiresDiscovery)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Discover();
            }
            
            SetupAmbientAudio();
            SetupAmbientParticles();
        }
        
        private void Update()
        {
            if (!isDiscovered)
            {
                CheckForDiscovery();
            }
            
            if (isPlayerInLocation)
            {
                UpdateLocationEffects();
                CheckLocationEvents();
            }
            
            UpdateFactionInfluence();
        }
        
        private void CheckForDiscovery()
        {
            if (!requiresDiscovery)
                return;
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, discoveryRadius);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    Discover();
                    break;
                }
            }
        }
        
        public void Discover()
        {
            if (isDiscovered)
                return;
            
            isDiscovered = true;
            gameObject.SetActive(true);
            
            OnLocationDiscovered();
            
            if (showOnMapOnDiscovery && requiresMinimap)
            {
                ShowOnMap();
            }
            
            SaveLocationDiscovery();
        }
        
        private void OnLocationDiscovered()
        {
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager?.OnLocationDiscovered(locationName);
            
            AchievementSystem achievementSystem = FindObjectOfType<AchievementSystem>();
            achievementSystem?.UnlockAchievement("LocationDiscovery");
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            uiManager?.ShowNotification($"Discovered: {locationName}");
        }
        
        private void UpdateLocationEffects()
        {
            if (ambientParticles != null && !ambientParticles.isPlaying)
            {
                ambientParticles.Play();
            }
        }
        
        private void UpdateFactionInfluence()
        {
            if (!isPlayerInLocation)
                return;
            
            currentInfluence -= influenceDecayRate * Time.deltaTime;
            currentInfluence = Mathf.Max(currentInfluence, 0f);
            
            if (currentInfluence <= 0f)
            {
                OnInfluenceLost();
            }
        }
        
        private void OnInfluenceLost()
        {
            controllingFaction = Faction.Neutral;
        }
        
        private void CheckLocationEvents()
        {
            if (locationEvents == null || locationEvents.Length == 0)
                return;
            
            foreach (var locationEvent in locationEvents)
            {
                if (locationEvent.hasTriggered)
                    continue;
                
                float distance = Vector3.Distance(transform.position, locationEvent.triggerPosition);
                
                if (distance <= eventTriggerDistance)
                {
                    TriggerLocationEvent(locationEvent);
                }
            }
        }
        
        private void TriggerLocationEvent(LocationEvent locationEvent)
        {
            locationEvent.hasTriggered = true;
            
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager?.OnEventTriggered(locationEvent.eventId);
            
            DialogueSystem dialogue = FindObjectOfType<DialogueSystem>();
            if (dialogue != null && locationEvent.dialogue != null)
            {
                dialogue.StartDialogue(locationEvent.dialogue);
            }
            
            UIManager uiManager = FindObjectOfType<UIManager>();
            uiManager?.ShowLocationEvent(locationEvent.eventName);
        }
        
        private void SetupAmbientAudio()
        {
            if (ambientSound != null)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = ambientSound;
                audioSource.loop = true;
                audioSource.spatialBlend = 1f;
                audioSource.playOnAwake = false;
            }
            
            if (ambientMusic != null && AudioManager.instance != null)
            {
                AudioManager.instance.SetAmbientMusic(ambientMusic);
            }
        }
        
        private void SetupAmbientParticles()
        {
            if (ambientParticles != null)
            {
                ambientParticles.Stop();
            }
        }
        
        public void ShowOnMap()
        {
            if (currentMapMarker != null)
                return;
            
            if (mapMarkerPrefab != null)
            {
                currentMapMarker = Instantiate(mapMarkerPrefab, transform.position, Quaternion.identity);
                currentMapMarker.Initialize(locationName, locationType);
            }
            
            isOnMap = true;
        }
        
        public void HideOnMap()
        {
            if (currentMapMarker != null)
            {
                Destroy(currentMapMarker.gameObject);
                currentMapMarker = null;
            }
            
            isOnMap = false;
        }
        
        public void AddSecret(string secretId)
        {
            if (!discoveredSecrets.Contains(secretId))
            {
                discoveredSecrets.Add(secretId);
            }
        }
        
        public bool HasSecret(string secretId)
        {
            return discoveredSecrets.Contains(secretId);
        }
        
        public void RecordVisit()
        {
            string timeKey = System.DateTime.Now.ToString("yyyy-MM-dd");
            visitedTimes[timeKey] = true;
        }
        
        public int GetVisitCount()
        {
            return visitedTimes.Count;
        }
        
        private void SaveLocationDiscovery()
        {
            PlayerPrefs.SetInt($"Location_{locationName}_Discovered", 1);
            PlayerPrefs.Save();
        }
        
        public void LoadDiscoveryState()
        {
            if (PlayerPrefs.HasKey($"Location_{locationName}_Discovered"))
            {
                Discover();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInLocation = true;
                RecordVisit();
                OnEnterLocation();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInLocation = false;
                OnExitLocation();
            }
        }
        
        private void OnEnterLocation()
        {
            if (ambientSound != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }
            
            FogOfWar fogOfWar = FindObjectOfType<FogOfWar>();
            if (fogOfWar != null)
            {
                fogOfWar.RevealArea(transform.position, discoveryRadius);
            }
        }
        
        private void OnExitLocation()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        
        public void SetFactionControl(Faction newFaction, float influence)
        {
            controllingFaction = newFaction;
            currentInfluence = influence;
        }
        
        public float GetCurrentInfluence()
        {
            return currentInfluence;
        }
    }
    
    public enum LocationType
    {
        City,
        CityDistrict,
        NobleCompound,
        Market,
        Slum,
        Temple,
        Keep,
        Forest,
        Cave,
        Ruins,
        SecretLocation
    }
    
    public enum Faction
    {
        Neutral,
        Obligators,
        SteelMinistry,
        NobleHouses,
        Koloss,
        Citizens,
        Player
    }
    
    [System.Serializable]
    public class LocationEvent
    {
        public string eventId;
        public string eventName;
        public Vector3 triggerPosition;
        public DialogueSystem.Dialogue dialogue;
        public bool hasTriggered = false;
    }
    
    public class MapMarker : MonoBehaviour
    {
        [SerializeField] private string markerName;
        [SerializeField] private LocationType markerType;
        
        public void Initialize(string name, LocationType type)
        {
            markerName = name;
            markerType = type;
        }
    }
}
