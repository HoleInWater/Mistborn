using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.World
{
    public class AtmosphereManager : MonoBehaviour
    {
        public static AtmosphereManager Instance { get; private set; }

        [Header("Time of Day")]
        [SerializeField] private bool m_useTimeOfDay = true;
        [SerializeField] private float m_dayDuration = 120f;
        [SerializeField] private Gradient m_skyGradient;
        [SerializeField] private AnimationCurve m_sunIntensity;
        [SerializeField] private AnimationCurve m_moonIntensity;

        [Header("Weather")]
        [SerializeField] private WeatherPreset[] m_weatherPresets;
        [SerializeField] private float m_weatherChangeInterval = 60f;
        [SerializeField] private float m_transitionDuration = 10f;

        [Header("Mist")]
        [SerializeField] private bool m_enableMist = true;
        [SerializeField] private float m_mistDensity = 0.3f;
        [SerializeField] private float m_mistVisibility = 50f;
        [SerializeField] private float m_mistMovementSpeed = 2f;

        [Header("Ash")]
        [SerializeField] private bool m_enableAsh = true;
        [SerializeField] private float m_ashFallSpeed = 0.5f;
        [SerializeField] private int m_ashParticleCount = 1000;
        [SerializeField] private Color m_ashColor = new Color(0.3f, 0.3f, 0.3f);

        [Header("Ambient")]
        [SerializeField] private AudioClip m_windSound;
        [SerializeField] private AudioClip m_rainSound;
        [SerializeField] private AudioSource m_ambientSource;

        private float m_currentTime;
        private int m_currentWeatherIndex;
        private WeatherPreset m_currentWeather;
        private WeatherPreset m_targetWeather;
        private float m_transitionProgress;
        private ParticleSystem m_ashParticles;
        private Light m_sunLight;
        private AudioSource m_windSource;

        public float currentTime => m_currentTime / m_dayDuration;
        public WeatherPreset currentWeather => m_currentWeather;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Initialize();
        }

        private void Initialize()
        {
            if (m_weatherPresets != null && m_weatherPresets.Length > 0)
            {
                m_currentWeather = m_weatherPresets[0];
                m_targetWeather = m_currentWeather;
            }

            SetupSunLight();
            SetupAshParticles();
            SetupAmbientAudio();
        }

        private void SetupSunLight()
        {
            GameObject sunObj = new GameObject("SunLight");
            sunObj.transform.SetParent(transform);
            sunObj.transform.localEulerAngles = new Vector3(50f, -30f, 0f);

            m_sunLight = sunObj.AddComponent<Light>();
            m_sunLight.type = LightType.Directional;
            m_sunLight.intensity = 1f;
            m_sunLight.shadows = LightShadows.Soft;
        }

        private void SetupAshParticles()
        {
            if (!m_enableAsh) return;

            GameObject ashObj = new GameObject("AshParticles");
            ashObj.transform.SetParent(transform);

            ParticleSystem ps = ashObj.AddComponent<ParticleSystem>();
            m_ashParticles = ps;

            var main = ps.main;
            main.maxParticles = m_ashParticleCount;
            main.startLifetime = 10f;
            main.startSpeed = m_ashFallSpeed;
            main.startColor = m_ashColor;
            main.startSize = 0.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = m_ashParticleCount / 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.boxThickness = new Vector3(100f, 0f, 100f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Play();
        }

        private void SetupAmbientAudio()
        {
            if (m_ambientSource == null)
            {
                m_ambientSource = gameObject.AddComponent<AudioSource>();
            }

            m_ambientSource.loop = true;
            m_ambientSource.volume = 0.3f;

            if (m_windSound != null)
            {
                m_ambientSource.clip = m_windSound;
                m_ambientSource.Play();
            }

            m_windSource = m_ambientSource;
        }

        private void Update()
        {
            if (m_useTimeOfDay)
            {
                UpdateTimeOfDay();
            }

            UpdateWeatherTransition();
            UpdateMist();
        }

        private void UpdateTimeOfDay()
        {
            m_currentTime += Time.deltaTime;
            if (m_currentTime >= m_dayDuration)
            {
                m_currentTime = 0f;
            }

            float normalizedTime = m_currentTime / m_dayDuration;

            if (m_sunLight != null)
            {
                float sunAngle = normalizedTime * 360f - 90f;
                m_sunLight.transform.localEulerAngles = new Vector3(sunAngle, -30f, 0f);

                float sunHeight = Mathf.Sin(normalizedTime * Mathf.PI);
                m_sunLight.intensity = m_sunIntensity.Evaluate(sunHeight) * (sunHeight > 0 ? 1f : 0.3f);
            }

            RenderSettings.ambientLight = Color.Lerp(new Color(0.1f, 0.1f, 0.15f), new Color(0.5f, 0.5f, 0.6f), Mathf.Clamp01(Mathf.Sin(normalizedTime * Mathf.PI)));
        }

        private void UpdateWeatherTransition()
        {
            if (m_currentWeather == m_targetWeather) return;

            m_transitionProgress += Time.deltaTime / m_transitionDuration;

            if (m_transitionProgress >= 1f)
            {
                m_currentWeather = m_targetWeather;
                m_transitionProgress = 0f;
            }
        }

        private void UpdateMist()
        {
            if (!m_enableMist) return;

            float fogDensity = m_mistDensity * (m_currentWeather?.mistMultiplier ?? 1f);
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogColor = m_currentWeather?.fogColor ?? Color.gray;
        }

        public void SetWeather(string weatherName)
        {
            foreach (WeatherPreset preset in m_weatherPresets)
            {
                if (preset.weatherName == weatherName)
                {
                    ChangeWeather(preset);
                    return;
                }
            }
        }

        public void ChangeWeather(WeatherPreset newWeather)
        {
            m_targetWeather = newWeather;
            m_transitionProgress = 0f;

            UpdateWeatherVisuals();
        }

        private void UpdateWeatherVisuals()
        {
            if (m_currentWeather == null) return;

            RenderSettings.fogColor = m_currentWeather.fogColor;
            RenderSettings.fogDensity = m_currentWeather.fogDensity;
            RenderSettings.ambientSkyColor = m_currentWeather.ambientColor;

            if (m_ashParticles != null)
            {
                var main = m_ashParticles.main;
                main.startColor = m_currentWeather.ashColor;
                main.startSpeed = m_currentWeather.windSpeed;
            }

            if (m_windSource != null && m_currentWeather.windAudio != null)
            {
                m_windSource.clip = m_currentWeather.windAudio;
                m_windSource.Play();
            }
        }

        public void ForceClear()
        {
            if (m_weatherPresets != null && m_weatherPresets.Length > 0)
            {
                foreach (WeatherPreset preset in m_weatherPresets)
                {
                    if (preset.weatherName == "Clear")
                    {
                        ChangeWeather(preset);
                        return;
                    }
                }
                ChangeWeather(m_weatherPresets[0]);
            }
        }

        public void EnableMist(bool enable)
        {
            m_enableMist = enable;
            RenderSettings.fog = enable;
        }

        public void SetMistDensity(float density)
        {
            m_mistDensity = Mathf.Clamp01(density);
        }
    }

    [System.Serializable]
    public class WeatherPreset
    {
        public string weatherName;
        public Color fogColor = Color.gray;
        public float fogDensity = 0.01f;
        public Color ambientColor = Color.gray;
        public Color ashColor = new Color(0.3f, 0.3f, 0.3f);
        public float windSpeed = 2f;
        public AudioClip windAudio;
        public float mistMultiplier = 1f;
    }
}
