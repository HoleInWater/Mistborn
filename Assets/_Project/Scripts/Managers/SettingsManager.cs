using UnityEngine;

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
    
    public void SetVSync(bool enabled)
    {
        vsyncEnabled = enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
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
    }
}
