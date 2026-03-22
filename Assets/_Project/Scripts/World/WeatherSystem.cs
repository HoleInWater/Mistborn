using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.World
{
    public class WeatherSystem : MonoBehaviour
    {
        [Header("Weather Configuration")]
        [SerializeField] private WeatherType currentWeather = WeatherType.Clear;
        [SerializeField] private float weatherChangeInterval = 120f;
        [SerializeField] private float weatherTransitionDuration = 10f;
        
        [Header("Weather Effects")]
        [SerializeField] private ParticleSystem rainParticles;
        [SerializeField] private ParticleSystem snowParticles;
        [SerializeField] private ParticleSystem heavyFogParticles;
        [SerializeField] private ParticleSystem stormParticles;
        [SerializeField] private ParticleSystem ashParticles;
        
        [Header("Lighting")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Color clearSkyColor = new Color(0.5f, 0.7f, 1f);
        [SerializeField] private Color overcastColor = new Color(0.4f, 0.45f, 0.5f);
        [SerializeField] private Color stormColor = new Color(0.2f, 0.25f, 0.3f);
        [SerializeField] private float clearLightIntensity = 1f;
        [SerializeField] private float stormLightIntensity = 0.3f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip clearAmbient;
        [SerializeField] private AudioClip rainAmbient;
        [SerializeField] private AudioClip stormAmbient;
        [SerializeField] private AudioClip fogAmbient;
        [SerializeField] private AudioClip ashfallAmbient;
        
        [Header("Gameplay Effects")]
        [SerializeField] private float visibilityInFog = 0.5f;
        [SerializeField] private float movementSpeedInRain = 0.95f;
        [SerializeField] private float visibilityInAshfall = 0.6f;
        [SerializeField] private bool affectsAllomancyInMist = true;
        
        [Header("Ashborn Weather (Era 1 Specific)")]
        [SerializeField] private bool enableAshbornEffects = true;
        [SerializeField] private float ashfallIntensity = 0.5f;
        [SerializeField] private ParticleSystem emberParticles;
        
        private float nextWeatherChange;
        private bool isTransitioning = false;
        private float transitionProgress = 0f;
        private WeatherType targetWeather;
        private AudioSource ambientAudio;
        private AshfallSystem ashfallSystem;
        
        private Dictionary<WeatherType, WeatherData> weatherData = new Dictionary<WeatherType, WeatherData>();
        
        public WeatherType CurrentWeather => currentWeather;
        
        private void Start()
        {
            InitializeWeatherData();
            SetupAudio();
            
            ashfallSystem = FindObjectOfType<AshfallSystem>();
            
            nextWeatherChange = Time.time + weatherChangeInterval;
            
            InitializeWeather();
        }
        
        private void InitializeWeatherData()
        {
            weatherData[WeatherType.Clear] = new WeatherData
            {
                name = "Clear",
                visibilityMultiplier = 1f,
                movementSpeedMultiplier = 1f,
                allomancyModifier = 1f,
                lightIntensity = 1f,
                particleSystem = null
            };
            
            weatherData[WeatherType.Overcast] = new WeatherData
            {
                name = "Overcast",
                visibilityMultiplier = 0.8f,
                movementSpeedMultiplier = 0.98f,
                allomancyModifier = 1f,
                lightIntensity = 0.7f,
                particleSystem = null
            };
            
            weatherData[WeatherType.Rain] = new WeatherData
            {
                name = "Rain",
                visibilityMultiplier = 0.7f,
                movementSpeedMultiplier = movementSpeedInRain,
                allomancyModifier = 1f,
                lightIntensity = 0.5f,
                particleSystem = rainParticles
            };
            
            weatherData[WeatherType.HeavyRain] = new WeatherData
            {
                name = "Heavy Rain",
                visibilityMultiplier = 0.5f,
                movementSpeedMultiplier = 0.9f,
                allomancyModifier = 1f,
                lightIntensity = 0.4f,
                particleSystem = rainParticles
            };
            
            weatherData[WeatherType.Fog] = new WeatherData
            {
                name = "Fog",
                visibilityMultiplier = visibilityInFog,
                movementSpeedMultiplier = 0.95f,
                allomancyModifier = 0.9f,
                lightIntensity = 0.6f,
                particleSystem = heavyFogParticles
            };
            
            weatherData[WeatherType.Mist] = new WeatherData
            {
                name = "Mist",
                visibilityMultiplier = 0.6f,
                movementSpeedMultiplier = 1f,
                allomancyModifier = affectsAllomancyInMist ? 0.8f : 1f,
                lightIntensity = 0.5f,
                particleSystem = heavyFogParticles
            };
            
            weatherData[WeatherType.Ashfall] = new WeatherData
            {
                name = "Ashfall",
                visibilityMultiplier = visibilityInAshfall,
                movementSpeedMultiplier = 0.9f,
                allomancyModifier = 0.95f,
                lightIntensity = 0.4f,
                particleSystem = ashParticles
            };
            
            weatherData[WeatherType.HeavyAshfall] = new WeatherData
            {
                name = "Heavy Ashfall",
                visibilityMultiplier = 0.3f,
                movementSpeedMultiplier = 0.85f,
                allomancyModifier = 0.9f,
                lightIntensity = 0.3f,
                particleSystem = ashParticles
            };
            
            weatherData[WeatherType.AshStorm] = new WeatherData
            {
                name = "Ash Storm",
                visibilityMultiplier = 0.2f,
                movementSpeedMultiplier = 0.7f,
                allomancyModifier = 0.8f,
                lightIntensity = 0.2f,
                particleSystem = ashParticles
            };
            
            weatherData[WeatherType.Storm] = new WeatherData
            {
                name = "Storm",
                visibilityMultiplier = 0.4f,
                movementSpeedMultiplier = 0.8f,
                allomancyModifier = 1f,
                lightIntensity = 0.3f,
                particleSystem = stormParticles
            };
        }
        
        private void SetupAudio()
        {
            ambientAudio = GetComponent<AudioSource>();
            if (ambientAudio == null)
            {
                ambientAudio = gameObject.AddComponent<AudioSource>();
            }
            ambientAudio.loop = true;
            ambientAudio.volume = 0.5f;
        }
        
        private void InitializeWeather()
        {
            ApplyWeatherEffects(currentWeather);
        }
        
        private void Update()
        {
            if (!isTransitioning && Time.time > nextWeatherChange)
            {
                TryChangeWeather();
            }
            
            if (isTransitioning)
            {
                UpdateWeatherTransition();
            }
        }
        
        private void TryChangeWeather()
        {
            WeatherType newWeather = SelectNextWeather();
            
            if (newWeather != currentWeather)
            {
                StartWeatherTransition(newWeather);
            }
            
            nextWeatherChange = Time.time + weatherChangeInterval + Random.Range(-30f, 30f);
        }
        
        private WeatherType SelectNextWeather()
        {
            WeatherType[] possibleWeathers;
            
            if (enableAshbornEffects)
            {
                float ashfallChance = Random.value;
                
                if (ashfallChance < 0.3f * ashfallIntensity)
                {
                    possibleWeathers = new WeatherType[] 
                    { 
                        WeatherType.Ashfall, 
                        WeatherType.HeavyAshfall,
                        WeatherType.AshStorm,
                        WeatherType.Mist
                    };
                }
                else if (ashfallChance < 0.5f)
                {
                    possibleWeathers = new WeatherType[]
                    {
                        WeatherType.Overcast,
                        WeatherType.Rain,
                        WeatherType.Fog,
                        WeatherType.Mist
                    };
                }
                else
                {
                    possibleWeathers = new WeatherType[]
                    {
                        WeatherType.Clear,
                        WeatherType.Overcast,
                        WeatherType.Mist
                    };
                }
            }
            else
            {
                possibleWeathers = new WeatherType[]
                {
                    WeatherType.Clear,
                    WeatherType.Overcast,
                    WeatherType.Rain,
                    WeatherType.Fog,
                    WeatherType.Mist
                };
            }
            
            int index = Random.Range(0, possibleWeathers.Length);
            return possibleWeathers[index];
        }
        
        private void StartWeatherTransition(WeatherType newWeather)
        {
            isTransitioning = true;
            transitionProgress = 0f;
            targetWeather = newWeather;
        }
        
        private void UpdateWeatherTransition()
        {
            transitionProgress += Time.deltaTime / weatherTransitionDuration;
            
            if (transitionProgress >= 1f)
            {
                CompleteWeatherTransition();
            }
        }
        
        private void CompleteWeatherTransition()
        {
            isTransitioning = false;
            transitionProgress = 0f;
            
            StopWeatherEffects(currentWeather);
            
            currentWeather = targetWeather;
            
            ApplyWeatherEffects(currentWeather);
        }
        
        private void ApplyWeatherEffects(WeatherType weather)
        {
            if (!weatherData.ContainsKey(weather))
                return;
            
            WeatherData data = weatherData[weather];
            
            if (directionalLight != null)
            {
                directionalLight.intensity = data.lightIntensity;
            }
            
            if (data.particleSystem != null)
            {
                data.particleSystem.Play();
            }
            
            UpdateAmbientAudio(weather);
            
            if (weather == WeatherType.Ashfall || weather == WeatherType.HeavyAshfall || weather == WeatherType.AshStorm)
            {
                if (ashfallSystem != null)
                {
                    ashfallSystem.SetAshfallIntensity(GetAshfallIntensity(weather));
                }
            }
            
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.SetVisibilityMultiplier(data.visibilityMultiplier);
                playerStats.SetMovementSpeedMultiplier(data.movementSpeedMultiplier);
            }
            
            Allomancy.Allomancer allomancer = FindObjectOfType<Allomancy.Allomancer>();
            if (allomancer != null)
            {
                allomancer.SetMetalSenseModifier(data.allomancyModifier);
            }
        }
        
        private void StopWeatherEffects(WeatherType weather)
        {
            if (!weatherData.ContainsKey(weather))
                return;
            
            WeatherData data = weatherData[weather];
            
            if (data.particleSystem != null)
            {
                data.particleSystem.Stop();
            }
        }
        
        private float GetAshfallIntensity(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Ashfall:
                    return 0.5f;
                case WeatherType.HeavyAshfall:
                    return 0.8f;
                case WeatherType.AshStorm:
                    return 1f;
                default:
                    return 0f;
            }
        }
        
        private void UpdateAmbientAudio(WeatherType weather)
        {
            AudioClip clip = GetAmbientClip(weather);
            
            if (clip != null && ambientAudio != null)
            {
                if (ambientAudio.clip != clip)
                {
                    ambientAudio.clip = clip;
                    ambientAudio.Play();
                }
            }
        }
        
        private AudioClip GetAmbientClip(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Clear:
                    return clearAmbient;
                case WeatherType.Rain:
                case WeatherType.HeavyRain:
                    return rainAmbient;
                case WeatherType.Storm:
                    return stormAmbient;
                case WeatherType.Fog:
                case WeatherType.Mist:
                    return fogAmbient;
                case WeatherType.Ashfall:
                case WeatherType.HeavyAshfall:
                case WeatherType.AshStorm:
                    return ashfallAmbient;
                default:
                    return clearAmbient;
            }
        }
        
        public void ForceWeather(WeatherType weather)
        {
            if (weather == currentWeather)
                return;
            
            StartWeatherTransition(weather);
        }
        
        public float GetVisibilityMultiplier()
        {
            if (weatherData.ContainsKey(currentWeather))
                return weatherData[currentWeather].visibilityMultiplier;
            return 1f;
        }
        
        public float GetMovementSpeedMultiplier()
        {
            if (weatherData.ContainsKey(currentWeather))
                return weatherData[currentWeather].movementSpeedMultiplier;
            return 1f;
        }
    }
    
    public enum WeatherType
    {
        Clear,
        Overcast,
        Rain,
        HeavyRain,
        Fog,
        Mist,
        Ashfall,
        HeavyAshfall,
        AshStorm,
        Storm
    }
    
    [System.Serializable]
    public class WeatherData
    {
        public string name;
        public float visibilityMultiplier;
        public float movementSpeedMultiplier;
        public float allomancyModifier;
        public float lightIntensity;
        public ParticleSystem particleSystem;
    }
}
