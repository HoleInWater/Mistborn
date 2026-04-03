using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Settings panel used in both the main menu and the pause menu.
/// Handles audio sliders, graphics quality, resolution, and fullscreen.
/// </summary>
public class MainMenuSettingsPanel : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI musicValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Navigation")]
    public Button backButton;

    private Resolution[] resolutions;

    void OnEnable()
    {
        LoadCurrentSettings();
        WireListeners();
    }

    void WireListeners()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolume);
        if (musicVolumeSlider != null)  musicVolumeSlider.onValueChanged.AddListener(OnMusicVolume);
        if (sfxVolumeSlider != null)    sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolume);
        if (qualityDropdown != null)    qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullscreenToggle != null)   fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (backButton != null)         backButton.onClick.AddListener(OnBack);
    }

    void LoadCurrentSettings()
    {
        // Audio
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music  = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfx    = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (masterVolumeSlider != null) { masterVolumeSlider.value = master; UpdateVolumeText(masterValueText, master); }
        if (musicVolumeSlider != null)  { musicVolumeSlider.value = music;   UpdateVolumeText(musicValueText, music); }
        if (sfxVolumeSlider != null)    { sfxVolumeSlider.value = sfx;       UpdateVolumeText(sfxValueText, sfx); }

        // Graphics quality
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
        }

        // Resolutions
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            var options = new System.Collections.Generic.List<string>();
            int currentIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string opt = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRateRatio.value:F0}Hz";
                options.Add(opt);

                if (resolutions[i].width == Screen.currentResolution.width
                    && resolutions[i].height == Screen.currentResolution.height)
                    currentIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // Fullscreen
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
    }

    void OnMasterVolume(float val)
    {
        PlayerPrefs.SetFloat("MasterVolume", val);
        AudioListener.volume = val;
        UpdateVolumeText(masterValueText, val);

        if (SoundManager.Instance != null)
            SoundManager.Instance.masterVolume = val;
    }

    void OnMusicVolume(float val)
    {
        PlayerPrefs.SetFloat("MusicVolume", val);
        UpdateVolumeText(musicValueText, val);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.musicVolume = val;
            if (SoundManager.Instance.musicSource != null)
                SoundManager.Instance.musicSource.volume = val * SoundManager.Instance.masterVolume;
        }
    }

    void OnSfxVolume(float val)
    {
        PlayerPrefs.SetFloat("SFXVolume", val);
        UpdateVolumeText(sfxValueText, val);

        if (SoundManager.Instance != null)
            SoundManager.Instance.sfxVolume = val;
    }

    void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    void OnResolutionChanged(int index)
    {
        if (resolutions == null || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void OnBack()
    {
        PlayerPrefs.Save();

        // Try the UI MainMenuController.OnBack() first
        var menu = GetComponentInParent<MainMenuController>();
        if (menu != null)
        {
            menu.OnBack();
            return;
        }

        // Try PauseMenuSystem
        var pause = FindObjectOfType<PauseMenuSystem>();
        if (pause != null)
        {
            pause.BackToPause();
            return;
        }

        gameObject.SetActive(false);
    }

    void UpdateVolumeText(TextMeshProUGUI text, float val)
    {
        if (text != null) text.text = $"{Mathf.RoundToInt(val * 100)}%";
    }
}
