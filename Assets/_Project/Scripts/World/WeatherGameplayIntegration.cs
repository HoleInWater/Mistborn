using UnityEngine;

/// <summary>
/// Integrates weather with gameplay mechanics.
/// Mist affects Tin, ash storms deal damage, rain affects hearing, day/night cycle controls mist.
/// </summary>
public class WeatherGameplayIntegration : MonoBehaviour
{
    public static WeatherGameplayIntegration Instance { get; private set; }

    public enum WeatherType { Clear, Mist, AshStorm, Rain, Storm }

    [Header("Current State")]
    public WeatherType currentWeather = WeatherType.Clear;
    public float timeOfDay = 12f; // 0-24 hours
    public float daySpeed = 0.01f; // Hours per real second

    [Header("Mist")]
    [Range(0f, 1f)] public float mistIntensity = 0f;
    public float nightMistIntensity = 0.9f;
    public float dayMistIntensity = 0.1f;
    public AnimationCurve mistCurve;

    [Header("Ash Storm")]
    public float ashDamagePerSecond = 5f;
    public float ashCheckInterval = 1f;

    [Header("Mist Spirit")]
    public GameObject mistSpiritPrefab;
    public float mistSpiritSpawnChance = 0.001f;
    [Range(0f, 1f)] public float mistSpiritMistThreshold = 0.8f;

    [Header("References")]
    private Transform player;
    private Tin tinComponent;
    private IDamageable playerHealth;

    private float ashTimer;
    private float mistSpiritCheckTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (mistCurve == null || mistCurve.length == 0)
        {
            mistCurve = new AnimationCurve(
                new Keyframe(0f, 0.9f), new Keyframe(6f, 0.7f), new Keyframe(8f, 0.1f),
                new Keyframe(16f, 0.1f), new Keyframe(18f, 0.5f), new Keyframe(20f, 0.9f),
                new Keyframe(24f, 0.9f)
            );
        }
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            tinComponent = playerObj.GetComponent<Tin>();
            playerHealth = playerObj.GetComponent<IDamageable>();
        }
    }

    void Update()
    {
        // Advance time
        timeOfDay += daySpeed * Time.deltaTime;
        if (timeOfDay >= 24f) timeOfDay -= 24f;

        // Update mist intensity from curve
        mistIntensity = mistCurve.Evaluate(timeOfDay);
        if (currentWeather == WeatherType.Mist)
            mistIntensity = Mathf.Max(mistIntensity, 0.6f);

        // Ash storm damage
        if (currentWeather == WeatherType.AshStorm)
            UpdateAshDamage();

        // Mist spirit spawn
        if (mistIntensity > mistSpiritMistThreshold)
            CheckMistSpiritSpawn();

        // Ambient sounds
        UpdateAmbientAudio();
    }

    void UpdateAshDamage()
    {
        ashTimer += Time.deltaTime;
        if (ashTimer < ashCheckInterval) return;
        ashTimer = 0f;

        if (player == null || playerHealth == null) return;

        // Check if player has cover above them
        bool hasCover = Physics.Raycast(player.position, Vector3.up, 10f);
        if (!hasCover)
        {
            playerHealth.TakeDamage(ashDamagePerSecond * ashCheckInterval);
            CameraShakeManager.Instance?.Shake(0.2f, 0.05f);
        }
    }

    void CheckMistSpiritSpawn()
    {
        mistSpiritCheckTimer += Time.deltaTime;
        if (mistSpiritCheckTimer < 10f) return;
        mistSpiritCheckTimer = 0f;

        if (mistSpiritPrefab != null && Random.value < mistSpiritSpawnChance)
        {
            Vector3 spawnPos = player.position + Random.onUnitSphere * 15f;
            spawnPos.y = player.position.y;
            Instantiate(mistSpiritPrefab, spawnPos, Quaternion.identity);
            Debug.Log("[WEATHER] A mist spirit appears...");
        }
    }

    void UpdateAmbientAudio()
    {
        SoundManager sm = SoundManager.Instance;
        if (sm == null) return;
        sm.PlayAmbientForWeather(currentWeather.ToString());
    }

    // ── Public API ───────────────────────────────────────────────────────
    public float GetMistIntensity() => mistIntensity;
    public WeatherType GetCurrentWeather() => currentWeather;
    public bool IsNightTime() => timeOfDay < 6f || timeOfDay > 20f;
    public float GetTimeOfDay() => timeOfDay;

    public void SetWeather(WeatherType weather) { currentWeather = weather; }
}
