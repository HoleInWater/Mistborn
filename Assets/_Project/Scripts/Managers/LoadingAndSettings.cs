using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI")]
    public GameObject loadingPanel;
    public Image progressBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;
    public Image backgroundImage;

    [Header("Settings")]
    public float minimumLoadTime = 1f;
    public bool showTips = true;
    public string[] loadingTips = {
        "Steel Push lets you launch metal objects at high speed",
        "Iron Pull draws metal toward you",
        "Burning Tin enhances all five senses",
        "Flaring drains metal reserves faster but boosts power",
        "Use the scroll wheel to switch between metals",
        "Heavy anchored objects pull YOU toward them",
        "Light objects like coins fly toward/away from you",
        "Zinc intensifies enemy emotions, Brass calms them",
        "Copper hides your Allomantic pulses from Seekers",
        "Bronze lets you detect other Allomancers burning",
        "Time bubbles can speed up or slow down time",
        "Duralumin amplifies your next metal burn massively",
        "Atium lets you see enemy's immediate future",
        "Invest in Pewter for enhanced strength and speed",
        "Tin enhances your senses but risks sensory overload"
    };

    private bool isLoading = false;
    private AsyncOperation currentLoadOp;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;
        ShowLoadingScreen();

        if (showTips && tipText != null)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        currentLoadOp = SceneManager.LoadSceneAsync(sceneName);
        currentLoadOp.allowSceneActivation = false;

        float startTime = Time.time;

        while (!currentLoadOp.isDone)
        {
            float progress = Mathf.Clamp01(currentLoadOp.progress / 0.9f);
            UpdateProgress(progress);

            if (currentLoadOp.progress >= 0.9f)
            {
                float elapsed = Time.time - startTime;
                if (elapsed >= minimumLoadTime)
                {
                    currentLoadOp.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        HideLoadingScreen();
        isLoading = false;
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        isLoading = true;
        ShowLoadingScreen();

        if (showTips && tipText != null)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        currentLoadOp = SceneManager.LoadSceneAsync(sceneIndex);
        currentLoadOp.allowSceneActivation = false;

        float startTime = Time.time;

        while (!currentLoadOp.isDone)
        {
            float progress = Mathf.Clamp01(currentLoadOp.progress / 0.9f);
            UpdateProgress(progress);

            if (currentLoadOp.progress >= 0.9f)
            {
                float elapsed = Time.time - startTime;
                if (elapsed >= minimumLoadTime)
                {
                    currentLoadOp.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        HideLoadingScreen();
        isLoading = false;
    }

    void ShowLoadingScreen()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (loadingText != null) loadingText.text = "Loading...";
        UpdateProgress(0f);
    }

    void HideLoadingScreen()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    void UpdateProgress(float progress)
    {
        if (progressBar != null) progressBar.fillAmount = progress;
        if (loadingText != null) loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }

    public void SkipLoading()
    {
        if (currentLoadOp != null && currentLoadOp.progress >= 0.9f)
        {
            currentLoadOp.allowSceneActivation = true;
        }
    }

    public bool IsLoading() => isLoading;
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Graphics")]
    public int qualityLevel = 2;
    public int resolutionIndex = 0;
    public bool fullscreen = true;
    public int targetFps = 60;
    public bool vsync = true;
    public float brightness = 1f;

    [Header("Audio")]
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 0.8f;
    [Range(0, 1)] public float sfxVolume = 1f;
    [Range(0, 1)] public float ambientVolume = 0.5f;

    [Header("Controls")]
    public float mouseSensitivity = 200f;
    public bool invertY = false;
    public float controllerSensitivity = 5f;
    public bool vibrationEnabled = true;

    [Header("Gameplay")]
    public float fieldOfView = 60f;
    public bool showSubtitles = true;
    public bool showTutorials = true;
    public bool showDamageNumbers = true;
    public float uiScale = 1f;

    [Header("Accessibility")]
    public bool colorBlindMode = false;
    public int colorBlindType = 0;
    public bool screenReader = false;
    public float textScale = 1f;
    public bool highContrast = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    public void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(qualityLevel);
        Screen.fullScreen = fullscreen;
        Application.targetFrameRate = targetFps;
        QualitySettings.vSyncCount = vsync ? 1 : 0;

        Resolution[] resolutions = Screen.resolutions;
        if (resolutions.Length > resolutionIndex)
        {
            Screen.SetResolution(
                resolutions[resolutionIndex].width,
                resolutions[resolutionIndex].height,
                fullscreen
            );
        }

        SaveSettings();
    }

    public void ApplyAudioSettings()
    {
        AudioListener.volume = masterVolume;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(masterVolume);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }

        SaveSettings();
    }

    public void ApplyControlSettings()
    {
        BasicPlayerMove move = FindObjectOfType<BasicPlayerMove>();
        if (move != null)
        {
            move.mouseSensitivity = mouseSensitivity;
        }

        SaveSettings();
    }

    public void SetQualityLevel(int level)
    {
        qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(qualityLevel);
        SaveSettings();
    }

    public void SetResolution(int index)
    {
        resolutionIndex = index;
        ApplyGraphicsSettings();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        fullscreen = isFullscreen;
        Screen.fullScreen = fullscreen;
        SaveSettings();
    }

    public void SetVSync(bool enabled)
    {
        vsync = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        SaveSettings();
    }

    public void SetTargetFPS(int fps)
    {
        targetFps = fps;
        Application.targetFrameRate = targetFps;
        SaveSettings();
    }

    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, 0.5f, 1.5f);
        // Apply brightness to render settings
        RenderSettings.ambientIntensity = brightness;
        SaveSettings();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume;
        SaveSettings();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        AudioManager.Instance?.SetMusicVolume(musicVolume);
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        AudioManager.Instance?.SetSFXVolume(sfxVolume);
        SaveSettings();
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
        ApplyControlSettings();
    }

    public void SetInvertY(bool invert)
    {
        invertY = invert;
        SaveSettings();
    }

    public void SetFieldOfView(float fov)
    {
        fieldOfView = Mathf.Clamp(fov, 50, 90);
        Camera.main.fieldOfView = fieldOfView;
        SaveSettings();
    }

    public void SetUIScale(float scale)
    {
        uiScale = Mathf.Clamp(scale, 0.5f, 2f);
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvas.scaleFactor = uiScale;
        }
        SaveSettings();
    }

    public void ResetToDefaults()
    {
        qualityLevel = 2;
        fullscreen = true;
        targetFps = 60;
        vsync = true;
        brightness = 1f;
        masterVolume = 1f;
        musicVolume = 0.8f;
        sfxVolume = 1f;
        ambientVolume = 0.5f;
        mouseSensitivity = 200f;
        invertY = false;
        fieldOfView = 60f;
        showSubtitles = true;
        showTutorials = true;
        uiScale = 1f;

        ApplyGraphicsSettings();
        ApplyAudioSettings();
        ApplyControlSettings();

        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualityLevel", qualityLevel);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("TargetFPS", targetFps);
        PlayerPrefs.SetInt("VSync", vsync ? 1 : 0);
        PlayerPrefs.SetFloat("Brightness", brightness);

        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);

        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("InvertY", invertY ? 1 : 0);

        PlayerPrefs.SetFloat("FieldOfView", fieldOfView);
        PlayerPrefs.SetInt("ShowSubtitles", showSubtitles ? 1 : 0);
        PlayerPrefs.SetInt("ShowTutorials", showTutorials ? 1 : 0);
        PlayerPrefs.SetFloat("UIScale", uiScale);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        targetFps = PlayerPrefs.GetInt("TargetFPS", 60);
        vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);

        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 200f);
        invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;

        fieldOfView = PlayerPrefs.GetFloat("FieldOfView", 60f);
        showSubtitles = PlayerPrefs.GetInt("ShowSubtitles", 1) == 1;
        showTutorials = PlayerPrefs.GetInt("ShowTutorials", 1) == 1;
        uiScale = PlayerPrefs.GetFloat("UIScale", 1f);

        ApplyGraphicsSettings();
        ApplyAudioSettings();
        ApplyControlSettings();
    }
}

public class WeatherSystem : MonoBehaviour
{
    [Header("Current Weather")]
    public WeatherType currentWeather = WeatherType.Clear;
    public enum WeatherType { Clear, Ash, Mist, Rain, Storm }

    [Header("Settings")]
    public float weatherTransitionTime = 5f;
    public bool autoWeather = false;
    public float weatherChangeInterval = 300f;

    [Header("Visuals")]
    public ParticleSystem ashParticles;
    public ParticleSystem mistParticles;
    public ParticleSystem rainParticles;
    public ParticleSystem stormParticles;
    public Light sunLight;
    public Color ashSkyColor = Color.gray;
    public Color mistSkyColor = new Color(0.5f, 0.5f, 0.7f);
    public Color rainSkyColor = new Color(0.3f, 0.3f, 0.4f);

    [Header("Effects")]
    public float ashVisibilityReduction = 0.5f;
    public float mistTinBoost = 1.5f;
    public float rainMovementPenalty = 0.8f;

    [Header("References")]
    public Camera playerCamera;
    private Color originalSkyColor;
    private float weatherTimer;
    private WeatherType targetWeather;
    private float transitionProgress;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        originalSkyColor = RenderSettings.fogColor;

        SetWeather(currentWeather);
    }

    void Update()
    {
        if (autoWeather)
        {
            weatherTimer += Time.deltaTime;
            if (weatherTimer >= weatherChangeInterval)
            {
                ChangeWeatherRandom();
                weatherTimer = 0f;
            }
        }

        UpdateWeatherEffects();
        UpdateSkyColor();
    }

    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;
        targetWeather = weather;

        DisableAllParticles();

        switch (weather)
        {
            case WeatherType.Clear:
                EnableClearWeather();
                break;
            case WeatherType.Ash:
                EnableAshWeather();
                break;
            case WeatherType.Mist:
                EnableMistWeather();
                break;
            case WeatherType.Rain:
                EnableRainWeather();
                break;
            case WeatherType.Storm:
                EnableStormWeather();
                break;
        }

        Debug.Log($"[WEATHER] Set to: {weather}");
    }

    void DisableAllParticles()
    {
        if (ashParticles != null) ashParticles.Stop();
        if (mistParticles != null) mistParticles.Stop();
        if (rainParticles != null) rainParticles.Stop();
        if (stormParticles != null) stormParticles.Stop();
    }

    void EnableClearWeather()
    {
        if (sunLight != null) sunLight.intensity = 1f;
        RenderSettings.fog = false;
    }

    void EnableAshWeather()
    {
        if (ashParticles != null) ashParticles.Play();
        if (sunLight != null) sunLight.intensity = 0.7f;
        RenderSettings.fog = true;
        RenderSettings.fogColor = ashSkyColor;
        RenderSettings.fogDensity = 0.02f;
    }

    void EnableMistWeather()
    {
        if (mistParticles != null) mistParticles.Play();
        if (sunLight != null) sunLight.intensity = 0.5f;
        RenderSettings.fog = true;
        RenderSettings.fogColor = mistSkyColor;
        RenderSettings.fogDensity = 0.05f;
    }

    void EnableRainWeather()
    {
        if (rainParticles != null) rainParticles.Play();
        if (sunLight != null) sunLight.intensity = 0.6f;
        RenderSettings.fog = true;
        RenderSettings.fogColor = rainSkyColor;
        RenderSettings.fogDensity = 0.03f;
    }

    void EnableStormWeather()
    {
        if (stormParticles != null) stormParticles.Play();
        if (sunLight != null) sunLight.intensity = 0.4f;
        RenderSettings.fog = true;
        RenderSettings.fogColor = rainSkyColor;
        RenderSettings.fogDensity = 0.04f;
    }

    void UpdateWeatherEffects()
    {
        if (currentWeather == WeatherType.Mist)
        {
            Tin tin = FindObjectOfType<Tin>();
            if (tin != null)
            {
                Debug.Log("[WEATHER] Tin enhanced in mist");
            }
        }

        if (currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm)
        {
            BasicPlayerMove move = FindObjectOfType<BasicPlayerMove>();
            if (move != null)
            {
                move.externalSpeedMultiplier *= rainMovementPenalty;
            }
        }
    }

    void UpdateSkyColor()
    {
        Color targetColor;
        switch (currentWeather)
        {
            case WeatherType.Ash:
                targetColor = ashSkyColor;
                break;
            case WeatherType.Mist:
                targetColor = mistSkyColor;
                break;
            case WeatherType.Rain:
            case WeatherType.Storm:
                targetColor = rainSkyColor;
                break;
            default:
                targetColor = originalSkyColor;
                break;
        }

        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetColor, Time.deltaTime * 2f);
    }

    public void TransitionToWeather(WeatherType newWeather, float transitionTime = -1)
    {
        if (transitionTime < 0) transitionTime = weatherTransitionTime;

        targetWeather = newWeather;
        StartCoroutine(WeatherTransition(newWeather, transitionTime));
    }

    System.Collections.IEnumerator WeatherTransition(WeatherType newWeather, float transitionTime)
    {
        float elapsed = 0f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            transitionProgress = elapsed / transitionTime;

            yield return null;
        }

        SetWeather(newWeather);
        transitionProgress = 1f;
    }

    public void ChangeWeatherRandom()
    {
        WeatherType[] types = (WeatherType[])System.Enum.GetValues(typeof(WeatherType));
        WeatherType newWeather = types[Random.Range(0, types.Length)];
        TransitionToWeather(newWeather);
    }

    public WeatherType GetCurrentWeather() => currentWeather;
    public bool IsRaining() => currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm;
    public bool IsMisty() => currentWeather == WeatherType.Mist;
}