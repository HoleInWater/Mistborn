using UnityEngine;
<<<<<<< HEAD

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    
    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 0.5f;
    public float sfxVolume = 1f;
    
    [Header("Graphics Settings")]
    public int targetFrameRate = 60;
    public bool vsyncEnabled = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        AudioListener.volume = volume;
        SaveSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        SaveSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        SaveSettings();
    }
    
    public void SetTargetFrameRate(int fps)
    {
        targetFrameRate = fps;
        Application.targetFrameRate = fps;
        SaveSettings();
    }
    
=======
using System.Reflection;

/// <summary>
/// Singleton settings manager. Persists audio, graphics, gameplay, and
/// keybind settings via PlayerPrefs. Keybind changes update the static
/// Keybinds class at runtime so all gameplay code picks them up immediately.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // ── Audio ─────────────────────────────────────────────────────────────
    [Header("Audio")]
    public float masterVolume = 1f;
    public float musicVolume  = 0.5f;
    public float sfxVolume    = 1f;

    // ── Graphics ──────────────────────────────────────────────────────────
    [Header("Graphics")]
    public int  targetFrameRate = 60;
    public bool vsyncEnabled    = true;
    public int  qualityLevel    = -1;   // -1 = use Unity default on first run

    // ── Gameplay ──────────────────────────────────────────────────────────
    [Header("Gameplay")]
    public float mouseSensitivity = 2f;
    public bool  invertY          = false;

    // ─────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    // ── Audio ─────────────────────────────────────────────────────────────

    public void SetMasterVolume(float v)
    {
        masterVolume = v;
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("MasterVolume", v);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        PlayerPrefs.SetFloat("MusicVolume", v);
        PlayerPrefs.Save();
        SoundManager.Instance?.SetMusicVolume(v);
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = v;
        PlayerPrefs.SetFloat("SFXVolume", v);
        PlayerPrefs.Save();
        SoundManager.Instance?.SetSFXVolume(v);
    }

    // ── Graphics ──────────────────────────────────────────────────────────

    public void SetResolution(Resolution res)
    {
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
        PlayerPrefs.SetInt("ResWidth",   res.width);
        PlayerPrefs.SetInt("ResHeight",  res.height);
        PlayerPrefs.SetInt("ResRefresh", res.refreshRate);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    public void SetVSync(bool enabled)
    {
        vsyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
<<<<<<< HEAD
        SaveSettings();
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("TargetFPS", targetFrameRate);
        PlayerPrefs.SetInt("VSync", vsyncEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        targetFrameRate = PlayerPrefs.GetInt("TargetFPS", 60);
        vsyncEnabled = PlayerPrefs.GetInt("VSync", 1) == 1;
        
        AudioListener.volume = masterVolume;
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
=======
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQualityLevel(int level)
    {
        qualityLevel = level;
        QualitySettings.SetQualityLevel(level, true);
        // Re-apply vsync because SetQualityLevel can reset it
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        PlayerPrefs.SetInt("QualityLevel", level);
        PlayerPrefs.Save();
    }

    public void SetTargetFrameRate(int fps)
    {
        targetFrameRate = fps;
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();
    }

    // ── Gameplay ──────────────────────────────────────────────────────────

    public void SetMouseSensitivity(float v)
    {
        mouseSensitivity = v;
        PlayerPrefs.SetFloat("MouseSensitivity", v);
        PlayerPrefs.Save();
    }

    public void SetInvertY(bool invert)
    {
        invertY = invert;
        PlayerPrefs.SetInt("InvertY", invert ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ── Keybinds ──────────────────────────────────────────────────────────

    /// <summary>
    /// Save a keybind for the given Keybinds field name and immediately
    /// apply it to the static Keybinds class so gameplay code picks it up.
    /// </summary>
    public void SaveKeybind(string actionName, KeyCode key)
    {
        PlayerPrefs.SetInt("KB_" + actionName, (int)key);
        PlayerPrefs.Save();
        SetKeybindField(actionName, key);
    }

    /// <summary>Return the currently saved key for an action, or defaultKey if never saved.</summary>
    public KeyCode GetKeybind(string actionName, KeyCode defaultKey)
    {
        return (KeyCode)PlayerPrefs.GetInt("KB_" + actionName, (int)defaultKey);
    }

    /// <summary>Reset a single keybind back to its default and save.</summary>
    public void ResetKeybind(string actionName, KeyCode defaultKey)
    {
        PlayerPrefs.DeleteKey("KB_" + actionName);
        PlayerPrefs.Save();
        SetKeybindField(actionName, defaultKey);
    }

    /// <summary>Reset every keybind to defaults defined in Keybinds.cs.</summary>
    public void ResetAllKeybindsToDefault()
    {
        // Clear saved overrides
        foreach (FieldInfo field in typeof(Keybinds).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(KeyCode))
                PlayerPrefs.DeleteKey("KB_" + field.Name);
        }
        PlayerPrefs.Save();

        // Re-load (will use compile-time defaults since prefs were cleared)
        LoadKeybinds();
    }

    // ── Save / Load ───────────────────────────────────────────────────────

    void LoadSettings()
    {
        // Audio
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume  = PlayerPrefs.GetFloat("MusicVolume",  0.5f);
        sfxVolume    = PlayerPrefs.GetFloat("SFXVolume",    1f);
        AudioListener.volume = masterVolume;

        // Graphics
        targetFrameRate = PlayerPrefs.GetInt("TargetFPS",  60);
        vsyncEnabled    = PlayerPrefs.GetInt("VSync",       1) == 1;
        Application.targetFrameRate    = targetFrameRate;
        QualitySettings.vSyncCount     = vsyncEnabled ? 1 : 0;

        if (PlayerPrefs.HasKey("QualityLevel"))
        {
            qualityLevel = PlayerPrefs.GetInt("QualityLevel");
            QualitySettings.SetQualityLevel(qualityLevel, true);
            QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        }

        if (PlayerPrefs.HasKey("ResWidth"))
        {
            int w = PlayerPrefs.GetInt("ResWidth");
            int h = PlayerPrefs.GetInt("ResHeight");
            int r = PlayerPrefs.GetInt("ResRefresh", 60);
            Screen.SetResolution(w, h, Screen.fullScreen, r);
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
            Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen") == 1;

        // Gameplay
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        invertY          = PlayerPrefs.GetInt("InvertY", 0) == 1;

        // Keybinds
        LoadKeybinds();
    }

    /// <summary>
    /// Apply saved keybind overrides to the static Keybinds class.
    /// Only overwrites fields that have a saved entry; defaults stay as-is.
    /// </summary>
    void LoadKeybinds()
    {
        foreach (FieldInfo field in typeof(Keybinds).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(KeyCode)) continue;
            string key = "KB_" + field.Name;
            if (PlayerPrefs.HasKey(key))
                field.SetValue(null, (KeyCode)PlayerPrefs.GetInt(key));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static void SetKeybindField(string fieldName, KeyCode key)
    {
        FieldInfo field = typeof(Keybinds).GetField(fieldName,
            BindingFlags.Public | BindingFlags.Static);
        field?.SetValue(null, key);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }
}
