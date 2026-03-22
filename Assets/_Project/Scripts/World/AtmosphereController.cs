using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class AtmosphereController : MonoBehaviour
{
    [Header("Time of Day")]
    public float dayLength = 120f;
    public float currentTime = 0f;
    public Gradient skyGradient;
    
    [Header("Lighting")]
    public Light directionalLight;
    public Color dayColor = Color.white;
    public Color nightColor = Color.blue;
    public float dayIntensity = 1f;
    public float nightIntensity = 0.2f;
    
    [Header("Fog")]
    public Color dayFogColor = Color.gray;
    public Color nightFogColor = Color.black;
    public FogMode fogMode = FogMode.Exponential;
    public float dayFogDensity = 0.01f;
    public float nightFogDensity = 0.05f;
    
    [Header("Ambient")]
    public Color dayAmbient = Color.white;
    public Color nightAmbient = Color.blue;
    
    [Header("Special Effects")]
    public GameObject mistEffect;
    public GameObject ashEffect;
    public float mistIntensity = 1f;
    public float ashFallSpeed = 1f;
    
    private float dayProgress = 0f;
    private bool isNight = false;
    
    void Start()
    {
        InitializeAtmosphere();
    }
    
    void InitializeAtmosphere()
    {
        if (mistEffect != null)
        {
            mistEffect.SetActive(true);
        }
        
        if (ashEffect != null)
        {
            ashEffect.SetActive(true);
        }
        
        UpdateAtmosphere(0f);
    }
    
    void Update()
    {
        currentTime += Time.deltaTime;
        dayProgress = currentTime / dayLength;
        dayProgress = dayProgress % 1f;
        
        UpdateAtmosphere(dayProgress);
        UpdateAshFall();
    }
    
    void UpdateAtmosphere(float progress)
    {
        bool wasNight = isNight;
        isNight = progress > 0.7f || progress < 0.2f;
        
        if (isNight != wasNight)
        {
            OnDayNightChange();
        }
        
        float t = 0f;
        
        if (progress < 0.2f)
        {
            t = 0f;
        }
        else if (progress < 0.3f)
        {
            t = (progress - 0.2f) / 0.1f;
        }
        else if (progress < 0.7f)
        {
            t = 1f;
        }
        else if (progress < 0.8f)
        {
            t = 1f - ((progress - 0.7f) / 0.1f);
        }
        else
        {
            t = 0f;
        }
        
        UpdateLighting(t);
        UpdateFog(t);
        UpdateAmbient(t);
        UpdateSkybox(t);
        UpdateMist(t);
    }
    
    void UpdateLighting(float dayFactor)
    {
        if (directionalLight == null) return;
        
        directionalLight.color = Color.Lerp(nightColor, dayColor, dayFactor);
        directionalLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayFactor);
        
        float sunAngle = dayFactor * 180f - 90f;
        directionalLight.transform.localEulerAngles = new Vector3(sunAngle, -30f, 0f);
    }
    
    void UpdateFog(float dayFactor)
    {
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, dayFactor);
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, dayFactor);
    }
    
    void UpdateAmbient(float dayFactor)
    {
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, dayFactor);
    }
    
    void UpdateSkybox(float dayFactor)
    {
        if (skyGradient != null)
        {
            Color skyColor = skyGradient.Evaluate(dayFactor);
            RenderSettings.skybox.SetColor("_SkyColor", skyColor);
        }
    }
    
    void UpdateMist(float dayFactor)
    {
        if (mistEffect != null)
        {
            ParticleSystem mist = mistEffect.GetComponent<ParticleSystem>();
            if (mist != null)
            {
                var main = mist.main;
                main.startColor = new Color(1f, 1f, 1f, dayFactor * mistIntensity);
            }
        }
    }
    
    void UpdateAshFall()
    {
        if (ashEffect != null)
        {
            ashEffect.transform.position += Vector3.down * ashFallSpeed * Time.deltaTime;
        }
    }
    
    void OnDayNightChange()
    {
        if (isNight)
        {
            Debug.Log("Night has fallen");
        }
        else
        {
            Debug.Log("Dawn breaks");
        }
    }
    
    public bool IsNight()
    {
        return isNight;
    }
    
    public float GetDayProgress()
    {
        return dayProgress;
    }
    
    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(dayProgress * 24f);
        int minutes = Mathf.FloorToInt((dayProgress * 24f - hours) * 60f);
        
        string timeOfDay = hours < 6 ? "AM" : "PM";
        hours = hours % 12;
        if (hours == 0) hours = 12;
        
        return $"{hours:D2}:{minutes:D2} {timeOfDay}";
    }
    
    public void SetTime(float time)
    {
        currentTime = Mathf.Clamp01(time) * dayLength;
    }
    
    public void SkipToNight()
    {
        SetTime(0.8f);
    }
    
    public void SkipToDay()
    {
        SetTime(0.5f);
    }
}

public class AshFallEffect : MonoBehaviour
{
    public float fallSpeed = 2f;
    public float driftSpeed = 0.5f;
    public float spawnHeight = 50f;
    public float despawnHeight = -10f;
    
    private ParticleSystem particles;
    
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        
        if (particles != null)
        {
            var main = particles.main;
            main.simulationSpace = SimulationSpace.World;
        }
    }
    
    void Update()
    {
        Vector3 offset = Vector3.down * fallSpeed * Time.deltaTime;
        offset += Vector3.right * Mathf.Sin(Time.time * driftSpeed) * driftSpeed * Time.deltaTime;
        
        transform.position += offset;
        
        if (transform.position.y < despawnHeight)
        {
            ResetPosition();
        }
    }
    
    void ResetPosition()
    {
        transform.position = new Vector3(
            Random.Range(-50f, 50f),
            spawnHeight,
            Random.Range(-50f, 50f)
        );
    }
}

public class MistSystem : MonoBehaviour
{
    [Header("Mist Settings")]
    public float mistDensity = 0.5f;
    public Color mistColor = new Color(0.8f, 0.8f, 0.9f, 0.3f);
    public float mistHeight = 5f;
    public float mistFalloff = 0.1f;
    
    [Header("Movement")]
    public Vector3 windDirection = Vector3.right;
    public float windSpeed = 1f;
    
    private ParticleSystem mistParticles;
    
    void Start()
    {
        InitializeMist();
    }
    
    void InitializeMist()
    {
        mistParticles = GetComponent<ParticleSystem>();
        
        if (mistParticles == null)
        {
            GameObject mistObj = new GameObject("MistParticles");
            mistObj.transform.SetParent(transform);
            mistParticles = mistObj.AddComponent<ParticleSystem>();
        }
        
        var main = mistParticles.main;
        main.startColor = mistColor;
        main.startLifetime = 20f;
        main.startSpeed = windSpeed;
        main.simulationSpace = SimulationSpace.World;
        main.maxParticles = 1000;
        
        var shape = mistParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.boxThickness = new Vector3(100f, 10f, 100f);
        
        var emission = mistParticles.emission;
        emission.rateOverTime = 50f;
    }
    
    void Update()
    {
        transform.position += windDirection * windSpeed * Time.deltaTime;
        
        var main = mistParticles.main;
        main.startColor = new Color(mistColor.r, mistColor.g, mistColor.b, mistDensity);
    }
}
