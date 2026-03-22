using UnityEngine;
using System.Collections.Generic;

public class WorldManager : MonoBehaviour
{
    [Header("World State")]
    public int currentDay = 1;
    public float currentTimeOfDay = 0f;
    public Weather currentWeather = Weather.Clear;
    public Season currentSeason = Season.Spring;
    
    [Header("World Events")]
    public List<WorldEvent> activeEvents = new List<WorldEvent>();
    public List<WorldEvent> scheduledEvents = new List<WorldEvent>();
    
    [Header("Environment")]
    public AtmosphereController atmosphere;
    public MistSystem mistSystem;
    public AshFallEffect ashFall;
    
    [Header("Populations")]
    public int civilianPopulation = 1000;
    public int guardPopulation = 100;
    public int kolossPopulation = 0;
    
    [Header("Resources")]
    public float foodSupply = 100f;
    public float metalSupply = 100f;
    public float warmth = 50f;
    
    private static WorldManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        UpdateTimeOfDay();
        UpdateWeather();
        UpdateResources();
        CheckWorldEvents();
    }
    
    void UpdateTimeOfDay()
    {
        currentTimeOfDay += Time.deltaTime / 120f;
        
        if (currentTimeOfDay >= 1f)
        {
            currentTimeOfDay = 0f;
            AdvanceDay();
        }
    }
    
    void AdvanceDay()
    {
        currentDay++;
        Debug.Log($"Day {currentDay} begins");
        
        UpdateSeason();
        ProcessDailyEvents();
        ConsumeResources();
    }
    
    void UpdateSeason()
    {
        int dayInYear = currentDay % 365;
        
        if (dayInYear < 91) currentSeason = Season.Spring;
        else if (dayInYear < 182) currentSeason = Season.Summer;
        else if (dayInYear < 273) currentSeason = Season.Autumn;
        else currentSeason = Season.Winter;
    }
    
    void UpdateWeather()
    {
        if (Random.value < 0.01f)
        {
            ChangeWeather();
        }
    }
    
    void ChangeWeather()
    {
        Weather[] weathers = { Weather.Clear, Weather.Cloudy, Weather.Rainy, Weather.Ashfall, Weather.Misty };
        currentWeather = weathers[Random.Range(0, weathers.Length)];
        
        switch (currentWeather)
        {
            case Weather.Ashfall:
                if (ashFall != null) ashFall.enabled = true;
                break;
            case Weather.Misty:
                if (mistSystem != null) mistSystem.enabled = true;
                break;
        }
    }
    
    void UpdateResources()
    {
        float consumptionRate = 1f;
        
        if (currentSeason == Season.Winter) consumptionRate *= 1.5f;
        if (currentWeather == Weather.Cold) consumptionRate *= 2f;
        
        warmth -= consumptionRate * Time.deltaTime * 0.1f;
        warmth = Mathf.Clamp(warmth, 0f, 100f);
        
        if (warmth <= 0f)
        {
            Debug.LogWarning("Population freezing!");
        }
    }
    
    void ProcessDailyEvents()
    {
        foreach (WorldEvent worldEvent in scheduledEvents)
        {
            if (worldEvent.dayTrigger <= currentDay)
            {
                TriggerEvent(worldEvent);
            }
        }
    }
    
    void ConsumeResources()
    {
        float populationConsumption = civilianPopulation * 0.01f;
        foodSupply -= populationConsumption;
        foodSupply = Mathf.Max(0f, foodSupply);
    }
    
    void CheckWorldEvents()
    {
        if (foodSupply <= 0f)
        {
            Debug.Log("Food shortage!");
        }
    }
    
    public void TriggerEvent(WorldEvent worldEvent)
    {
        Debug.Log($"World Event: {worldEvent.eventName}");
        activeEvents.Add(worldEvent);
        
        switch (worldEvent.eventType)
        {
            case EventType.KolossAttack:
                kolossPopulation += worldEvent.intensity;
                break;
            case EventType.SupplyDrop:
                foodSupply += worldEvent.intensity;
                metalSupply += worldEvent.intensity;
                break;
            case EventType.WeatherChange:
                currentWeather = worldEvent.weatherType;
                break;
        }
    }
    
    public void AddEvent(WorldEvent worldEvent)
    {
        scheduledEvents.Add(worldEvent);
    }
    
    public string GetTimeString()
    {
        float hours = currentTimeOfDay * 24f;
        int displayHours = Mathf.FloorToInt(hours);
        int displayMinutes = Mathf.FloorToInt((hours - displayHours) * 60f);
        
        string period = displayHours < 12 ? "AM" : "PM";
        displayHours = displayHours % 12;
        if (displayHours == 0) displayHours = 12;
        
        return $"{displayHours:D2}:{displayMinutes:D2} {period}";
    }
}

[System.Serializable]
public class WorldEvent
{
    public string eventId;
    public string eventName;
    public EventType eventType;
    public int dayTrigger;
    public float intensity;
    public Weather weatherType;
    public bool isRepeatable;
    public WorldEventEffect[] effects;
}

[System.Serializable]
public class WorldEventEffect
{
    public string effectType;
    public float value;
}

public enum EventType
{
    KolossAttack,
    SupplyDrop,
    WeatherChange,
    NPCArrival,
    QuestTrigger,
    Disease,
    Fire,
    Theft
}

public enum Weather
{
    Clear,
    Cloudy,
    Rainy,
    Ashfall,
    Misty,
    Cold,
    Hot,
    Stormy
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class SkyboxController : MonoBehaviour
{
    [Header("Skybox Settings")]
    public Material daySkybox;
    public Material nightSkybox;
    public Material ashfallSkybox;
    
    [Header("Transition")]
    public float transitionDuration = 1f;
    
    [Header("Colors")]
    public Color dayFogColor = Color.gray;
    public Color nightFogColor = new Color(0.2f, 0.2f, 0.3f);
    public Color ashfallFogColor = new Color(0.5f, 0.4f, 0.3f);
    
    void Update()
    {
        UpdateFogColor();
    }
    
    void UpdateFogColor()
    {
        WorldManager world = FindObjectOfType<WorldManager>();
        if (world == null) return;
        
        Color targetColor = dayFogColor;
        
        switch (world.currentWeather)
        {
            case Weather.Night:
            case Weather.Cloudy:
                targetColor = nightFogColor;
                break;
            case Weather.Ashfall:
                targetColor = ashfallFogColor;
                break;
        }
        
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetColor, Time.deltaTime * 0.5f);
    }
    
    public void SetSkybox(Weather weather)
    {
        Material skyMat = daySkybox;
        
        switch (weather)
        {
            case Weather.Ashfall:
                skyMat = ashfallSkybox;
                break;
            default:
                skyMat = daySkybox;
                break;
        }
        
        RenderSettings.skybox = skyMat;
    }
}

public class EnvironmentalHazards : MonoBehaviour
{
    [Header("Hazard Settings")]
    public HazardType hazardType;
    public float damagePerSecond = 5f;
    public float hazardRadius = 5f;
    
    [Header("Visual")]
    public Color hazardColor = Color.red;
    public GameObject hazardEffect;
    
    [Header("Audio")]
    public AudioClip hazardSound;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = hazardColor;
        Gizmos.DrawWireSphere(transform.position, hazardRadius);
    }
}

public enum HazardType
{
    Lava,
    Poison,
    AshCloud,
    SteelShard,
    KolossBlood
}

public class WorldObjectInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public string interactionPrompt = "Press E to interact";
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 3f;
    
    [Header("Interaction Types")]
    public bool canOpen = false;
    public bool canCollect = false;
    public bool canActivate = false;
    public bool canRead = false;
    
    [Header("States")]
    public bool isOpen = false;
    public bool isActivated = false;
    public bool isCollected = false;
    
    [Header("Loot")]
    public LootTable lootTable;
    public Item[] items;
    
    [Header("Dialogue")]
    public string[] dialogueLines;
    
    private GameObject promptUI;
    private bool playerInRange = false;
    
    void Start()
    {
        CreatePromptUI();
    }
    
    void CreatePromptUI()
    {
        promptUI = new GameObject("InteractionPrompt");
        promptUI.transform.SetParent(transform);
        promptUI.transform.localPosition = Vector3.up * 2f;
        
        Canvas canvas = promptUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        
        promptUI.AddComponent<UnityEngine.UI.Text>().text = interactionPrompt;
        promptUI.SetActive(false);
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
    
    public void Interact()
    {
        if (canOpen) Open();
        if (canCollect) Collect();
        if (canActivate) Activate();
        if (canRead) Read();
    }
    
    void Open()
    {
        if (isOpen) return;
        isOpen = true;
        Debug.Log($"Opened: {gameObject.name}");
        
        if (lootTable != null)
        {
            LootItem item = lootTable.GetRandomItem();
            if (item != null)
            {
                Inventory inventory = FindObjectOfType<Inventory>();
                inventory?.AddItem(item.itemId);
            }
        }
    }
    
    void Collect()
    {
        if (isCollected) return;
        isCollected = true;
        
        foreach (Item item in items)
        {
            Inventory inventory = FindObjectOfType<Inventory>();
            inventory?.AddItem(item.itemId);
        }
        
        Destroy(gameObject);
    }
    
    void Activate()
    {
        if (isActivated) return;
        isActivated = true;
        
        WorldEvent triggerEvent = GetComponent<WorldEvent>();
        if (triggerEvent != null)
        {
            WorldManager world = FindObjectOfType<WorldManager>();
            world?.TriggerEvent(triggerEvent);
        }
    }
    
    void Read()
    {
        foreach (string line in dialogueLines)
        {
            Debug.Log($"[Book] {line}");
        }
    }
}

public class MistbornWorldEvents : MonoBehaviour
{
    [Header("Mistborn Specific Events")]
    public bool mistActive = true;
    public float mistDensity = 0.5f;
    public Color mistColor = new Color(0.8f, 0.8f, 0.9f);
    
    [Header("Creatures")]
    public bool kolossRaidEnabled = true;
    public float kolossRaidChance = 0.05f;
    
    [Header("Allomancy Effects")]
    public bool hiddenCoinsVisible = true;
    public bool mistGlowOnBurn = true;
    
    void Update()
    {
        if (mistActive)
        {
            UpdateMist();
        }
        
        if (kolossRaidEnabled && Random.value < kolossRaidChance * Time.deltaTime)
        {
            TriggerKolossRaid();
        }
    }
    
    void UpdateMist()
    {
        WorldManager world = FindObjectOfType<WorldManager>();
        if (world == null) return;
        
        bool isNight = world.currentTimeOfDay > 0.7f || world.currentTimeOfDay < 0.2f;
        
        if (isNight && !mistActive)
        {
            SpawnMist();
        }
    }
    
    void SpawnMist()
    {
        Debug.Log("Mist rises...");
    }
    
    void TriggerKolossRaid()
    {
        WorldEvent raidEvent = new WorldEvent();
        raidEvent.eventName = "Koloss Raid";
        raidEvent.eventType = EventType.KolossAttack;
        raidEvent.intensity = Random.Range(5, 20);
        
        WorldManager world = FindObjectOfType<WorldManager>();
        world?.TriggerEvent(raidEvent);
    }
    
    public void ShowBurningEffect(MetalType metal)
    {
        if (!mistGlowOnBurn) return;
        
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles == null)
        {
            GameObject effect = new GameObject("BurnEffect");
            effect.transform.SetParent(transform);
            particles = effect.AddComponent<ParticleSystem>();
        }
        
        var main = particles.main;
        main.startColor = GetMetalColor(metal);
        main.startSize = 0.1f;
        main.startLifetime = 1f;
        
        var emission = particles.emission;
        emission.rateOverTime = 50f;
        
        particles.Play();
    }
    
    Color GetMetalColor(MetalType metal)
    {
        switch (metal)
        {
            case MetalType.Steel: return Color.cyan;
            case MetalType.Iron: return Color.blue;
            case MetalType.Pewter: return Color.yellow;
            case MetalType.Tin: return Color.white;
            case MetalType.Atium: return new Color(1f, 0.8f, 0.2f);
            case MetalType.Copper: return new Color(0.7f, 0.4f, 0.2f);
            case MetalType.Bronze: return new Color(0.8f, 0.5f, 0.2f);
            default: return Color.gray;
        }
    }
}
