using UnityEngine;

/// <summary>
/// Base weather system controlling weather state transitions and particle effects.
/// Works with WeatherGameplayIntegration for gameplay effects.
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public enum WeatherState { Clear, Mist, AshFall, Rain, Storm }

    [Header("Current Weather")]
    public WeatherState currentWeather = WeatherState.Clear;
    public float transitionDuration = 5f;

    [Header("Particle Systems")]
    public ParticleSystem ashParticles;
    public ParticleSystem mistParticles;
    public ParticleSystem rainParticles;

    [Header("Auto Cycle")]
    public bool autoCycle = true;
    public float minWeatherDuration = 60f;
    public float maxWeatherDuration = 180f;

    private float weatherTimer;
    private float transitionProgress = 1f;
    private WeatherState targetWeather;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        targetWeather = currentWeather;
    }

    void Start()
    {
        weatherTimer = Random.Range(minWeatherDuration, maxWeatherDuration);
        ApplyWeatherImmediate(currentWeather);
    }

    void Update()
    {
        // Transition
        if (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / transitionDuration;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                currentWeather = targetWeather;
            }
        }

        // Auto cycle
        if (autoCycle)
        {
            weatherTimer -= Time.deltaTime;
            if (weatherTimer <= 0f)
            {
                weatherTimer = Random.Range(minWeatherDuration, maxWeatherDuration);
                WeatherState next = (WeatherState)Random.Range(0, 5);
                TransitionTo(next);
            }
        }

        // Sync to gameplay integration
        WeatherGameplayIntegration wgi = WeatherGameplayIntegration.Instance;
        if (wgi != null) wgi.SetWeather((WeatherGameplayIntegration.WeatherType)(int)currentWeather);
    }

    public void TransitionTo(WeatherState weather)
    {
        targetWeather = weather;
        transitionProgress = 0f;
        ApplyWeatherImmediate(weather);
    }

    void ApplyWeatherImmediate(WeatherState weather)
    {
        StopAllParticles();

        switch (weather)
        {
            case WeatherState.AshFall:
                if (ashParticles != null) ashParticles.Play();
                break;
            case WeatherState.Mist:
                if (mistParticles != null) mistParticles.Play();
                break;
            case WeatherState.Rain:
            case WeatherState.Storm:
                if (rainParticles != null) rainParticles.Play();
                break;
        }

        SoundManager.Instance?.PlayAmbientForWeather(weather.ToString());
    }

    void StopAllParticles()
    {
        if (ashParticles != null) ashParticles.Stop();
        if (mistParticles != null) mistParticles.Stop();
        if (rainParticles != null) rainParticles.Stop();
    }

    public WeatherState GetCurrentWeather() => currentWeather;
    public float GetTransitionProgress() => transitionProgress;
}
