using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-contained settings panel: tabs (Audio / Graphics / Keybinds / Gameplay),
/// sliders, dropdowns, and toggles wired to SettingsManager.
///
/// Attach to the root of the Settings panel in BOTH the Main Menu and the Pause
/// Menu scenes so the same Inspector layout works in both places.
///
/// Call RefreshUI() whenever the panel is shown to sync to the latest saved values.
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    // ── Tab Content ───────────────────────────────────────────────────────
    [Header("Tab Content Panels")]
    public GameObject audioTabContent;
    public GameObject graphicsTabContent;
    public GameObject keybindsTabContent;
    public GameObject gameplayTabContent;

    // ── Audio ─────────────────────────────────────────────────────────────
    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    // ── Graphics ──────────────────────────────────────────────────────────
    [Header("Graphics")]
    public Dropdown resolutionDropdown;
    public Toggle   fullscreenToggle;
    public Toggle   vsyncToggle;
    public Dropdown qualityDropdown;
    public Dropdown fpsDropdown;

    // ── Gameplay ──────────────────────────────────────────────────────────
    [Header("Gameplay")]
    public Slider mouseSensitivitySlider;
    public Toggle invertYToggle;

    // ── Keybinds ──────────────────────────────────────────────────────────
    [Header("Keybinds")]
    public Button resetAllKeybindsButton;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        InitUI();
        ShowTab(audioTabContent);   // default tab
    }

    // ── Tab Switching (wire these to tab header buttons) ──────────────────

    public void ShowAudioTab()    => ShowTab(audioTabContent);
    public void ShowGraphicsTab() => ShowTab(graphicsTabContent);
    public void ShowKeybindsTab() => ShowTab(keybindsTabContent);
    public void ShowGameplayTab() => ShowTab(gameplayTabContent);

    // ── Call this whenever the panel becomes visible ───────────────────────

    public void RefreshUI()
    {
        SettingsManager sm = SettingsManager.Instance;
        if (sm == null) return;

        if (masterVolumeSlider     != null) masterVolumeSlider.SetValueWithoutNotify(sm.masterVolume);
        if (musicVolumeSlider      != null) musicVolumeSlider.SetValueWithoutNotify(sm.musicVolume);
        if (sfxVolumeSlider        != null) sfxVolumeSlider.SetValueWithoutNotify(sm.sfxVolume);
        if (fullscreenToggle       != null) fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        if (vsyncToggle            != null) vsyncToggle.SetIsOnWithoutNotify(sm.vsyncEnabled);
        if (qualityDropdown        != null) qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.SetValueWithoutNotify(sm.mouseSensitivity);
        if (invertYToggle          != null) invertYToggle.SetIsOnWithoutNotify(sm.invertY);

        // Refresh all keybind rows in this panel
        foreach (var rebinder in GetComponentsInChildren<KeybindRebinder>(true))
            rebinder.RefreshDisplay();
    }

    // ── One-time listener setup ───────────────────────────────────────────

    void InitUI()
    {
        SettingsManager sm = SettingsManager.Instance;
        if (sm == null) return;

        // --- Audio ---
        SetupSlider(masterVolumeSlider, sm.masterVolume, sm.SetMasterVolume);
        SetupSlider(musicVolumeSlider,  sm.musicVolume,  sm.SetMusicVolume);
        SetupSlider(sfxVolumeSlider,    sm.sfxVolume,    sm.SetSFXVolume);

        // --- Graphics: Resolution ---
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<Dropdown.OptionData>();
            Resolution[] resolutions = Screen.resolutions;
            int currentIdx = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add(new Dropdown.OptionData(
                    $"{resolutions[i].width}x{resolutions[i].height} @{resolutions[i].refreshRate}Hz"));
                if (resolutions[i].width  == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                    currentIdx = i;
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIdx;
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(
                idx => sm.SetResolution(Screen.resolutions[idx]));
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(sm.SetFullscreen);
        }
        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = sm.vsyncEnabled;
            vsyncToggle.onValueChanged.AddListener(sm.SetVSync);
        }

        // --- Graphics: Quality ---
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<Dropdown.OptionData>();
            foreach (string qName in QualitySettings.names)
                options.Add(new Dropdown.OptionData(qName));
            qualityDropdown.AddOptions(options);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(sm.SetQualityLevel);
        }

        // --- Graphics: FPS ---
        if (fpsDropdown != null)
        {
            fpsDropdown.ClearOptions();
            fpsDropdown.AddOptions(new System.Collections.Generic.List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("30"),
                new Dropdown.OptionData("60"),
                new Dropdown.OptionData("90"),
                new Dropdown.OptionData("120"),
                new Dropdown.OptionData("144"),
                new Dropdown.OptionData("Unlimited"),
            });
            int[] fpsValues = { 30, 60, 90, 120, 144, -1 };
            int fpsIdx = System.Array.IndexOf(fpsValues, sm.targetFrameRate);
            fpsDropdown.value = fpsIdx < 0 ? 1 : fpsIdx;
            fpsDropdown.RefreshShownValue();
            fpsDropdown.onValueChanged.AddListener(
                idx => sm.SetTargetFrameRate(new int[] { 30, 60, 90, 120, 144, -1 }[idx]));
        }

        // --- Gameplay ---
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = 0.1f;
            mouseSensitivitySlider.maxValue = 10f;
            mouseSensitivitySlider.value = sm.mouseSensitivity;
            mouseSensitivitySlider.onValueChanged.AddListener(sm.SetMouseSensitivity);
        }
        if (invertYToggle != null)
        {
            invertYToggle.isOn = sm.invertY;
            invertYToggle.onValueChanged.AddListener(sm.SetInvertY);
        }

        // --- Keybinds: Reset All ---
        if (resetAllKeybindsButton != null)
            resetAllKeybindsButton.onClick.AddListener(OnResetAllKeybinds);
    }

    void OnResetAllKeybinds()
    {
        SettingsManager.Instance?.ResetAllKeybindsToDefault();
        foreach (var rebinder in GetComponentsInChildren<KeybindRebinder>(true))
            rebinder.RefreshDisplay();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void ShowTab(GameObject tab)
    {
        if (audioTabContent    != null) audioTabContent.SetActive(false);
        if (graphicsTabContent != null) graphicsTabContent.SetActive(false);
        if (keybindsTabContent != null) keybindsTabContent.SetActive(false);
        if (gameplayTabContent != null) gameplayTabContent.SetActive(false);
        if (tab != null) tab.SetActive(true);
    }

    static void SetupSlider(Slider slider, float initialValue,
        UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;
        slider.value = initialValue;
        slider.onValueChanged.AddListener(callback);
    }
}
