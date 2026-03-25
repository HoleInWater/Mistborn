using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button settingsButton;
    public Button saveButton;
    public Button loadButton;
    public Button quitButton;
    public Button mainMenuButton;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject saveLoadPanel;
    public GameObject confirmPanel;

    [Header("Settings")]
    public float timeScale = 0f;
    public bool lockCursor = true;

    private bool isPaused = false;
    private GameFlowManager gameFlow;

    void Start()
    {
        gameFlow = GameFlowManager.Instance;

        if (pausePanel != null) pausePanel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (saveButton != null) saveButton.onClick.AddListener(OpenSaveMenu);
        if (loadButton != null) loadButton.onClick.AddListener(OpenLoadMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = timeScale;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (lockCursor) Cursor.lockState = CursorLockMode.None;

        gameFlow?.SetState(GameFlowManager.GameState.Paused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);

        if (lockCursor) Cursor.lockState = CursorLockMode.Locked;

        gameFlow?.SetState(GameFlowManager.GameState.Playing);
    }

    void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            pausePanel.SetActive(false);
        }
    }

    void OpenSaveMenu()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(true);
            pausePanel.SetActive(false);

            SaveLoadUI ui = saveLoadPanel.GetComponent<SaveLoadUI>();
            if (ui != null) ui.ShowSaveSlots();
        }
    }

    void OpenLoadMenu()
    {
        if (saveLoadPanel != null)
        {
            saveLoadPanel.SetActive(true);
            pausePanel.SetActive(false);

            SaveLoadUI ui = saveLoadPanel.GetComponent<SaveLoadUI>();
            if (ui != null) ui.ShowLoadSlots();
        }
    }

    void QuitGame()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);

            ConfirmDialog dialog = confirmPanel.GetComponent<ConfirmDialog>();
            if (dialog != null)
            {
                dialog.Show("Quit", "Are you sure you want to quit?", () => Application.Quit(), () => confirmPanel.SetActive(false));
            }
        }
        else
        {
            Application.Quit();
        }
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        LoadingScreenManager.Instance?.LoadScene("MainMenu");
    }

    public bool IsPaused() => isPaused;
}

public class SaveLoadUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject slotPanel;
    public GameObject slotPrefab;
    public Transform slotContainer;
    public TextMeshProUGUI titleText;
    public Button backButton;

    [Header("Slot Display")]
    public Color hasDataColor = new Color(0.2f, 0.5f, 0.3f);
    public Color emptyDataColor = new Color(0.3f, 0.3f, 0.3f);

    private bool isSaveMode = true;
    private List<SaveSlotUI> slots = new List<SaveSlotUI>();

    void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(Back);
    }

    public void ShowSaveSlots()
    {
        isSaveMode = true;
        if (titleText != null) titleText.text = "Save Game";
        ShowSlots();
    }

    public void ShowLoadSlots()
    {
        isSaveMode = false;
        if (titleText != null) titleText.text = "Load Game";
        ShowSlots();
    }

    void ShowSlots()
    {
        ClearSlots();

        for (int i = 0; i < 10; i++)
        {
            CreateSlot(i);
        }
    }

    void CreateSlot(int slotIndex)
    {
        if (slotPrefab == null || slotContainer == null) return;

        GameObject slotObj = Instantiate(slotPrefab, slotContainer);
        SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

        if (slotUI != null)
        {
            bool hasData = SaveLoadManager.Instance?.HasSaveData(slotIndex) ?? false;
            slotUI.Initialize(slotIndex, hasData, OnSlotClicked);

            slots.Add(slotUI);
        }
    }

    void ClearSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        slots.Clear();
    }

    void OnSlotClicked(int slotIndex)
    {
        if (isSaveMode)
        {
            SaveLoadManager.Instance?.SaveGame(slotIndex, $"Save {slotIndex}");
            ShowSlots();
        }
        else
        {
            SaveLoadManager.Instance?.LoadGame(slotIndex);
            Back();
        }
    }

    void Back()
    {
        gameObject.SetActive(false);

        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.pausePanel.SetActive(true);
        }
    }
}

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI slotNameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI chapterText;
    public Image backgroundImage;
    public Button slotButton;

    private int slotIndex;

    public void Initialize(int index, bool hasData, System.Action<int> onClick)
    {
        slotIndex = index;

        if (slotNameText != null) slotNameText.text = hasData ? $"Slot {index + 1}" : "Empty";
        if (dateText != null) dateText.text = hasData ? "Last saved: Today" : "";
        if (chapterText != null) chapterText.text = hasData ? "Chapter 1" : "";
        if (backgroundImage != null) backgroundImage.color = hasData ? new Color(0.2f, 0.5f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);

        if (slotButton != null) slotButton.onClick.AddListener(() => onClick?.Invoke(index));
    }
}

public class ConfirmDialog : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    private System.Action onConfirm;
    private System.Action onCancel;

    void Start()
    {
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
    }

    public void Show(string title, string message, System.Action onConfirm, System.Action onCancel)
    {
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        gameObject.SetActive(true);
    }

    void OnConfirm()
    {
        onConfirm?.Invoke();
        gameObject.SetActive(false);
    }

    void OnCancel()
    {
        onCancel?.Invoke();
        gameObject.SetActive(false);
    }
}

public class SettingsUI : MonoBehaviour
{
    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider brightnessSlider;
    public Slider fovSlider;

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider ambientVolumeSlider;

    [Header("Controls")]
    public Slider sensitivitySlider;
    public Toggle invertYToggle;

    [Header("Gameplay")]
    public Toggle subtitlesToggle;
    public Toggle tutorialsToggle;
    public Slider uiScaleSlider;

    [Header("Buttons")]
    public Button applyButton;
    public Button resetButton;
    public Button backButton;

    private SettingsManager settings;

    void Start()
    {
        settings = SettingsManager.Instance;

        if (applyButton != null) applyButton.onClick.AddListener(ApplySettings);
        if (resetButton != null) resetButton.onClick.AddListener(ResetSettings);
        if (backButton != null) backButton.onClick.AddListener(CloseSettings);

        SetupDropdowns();
        LoadCurrentSettings();
    }

    void SetupDropdowns()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var res in Screen.resolutions)
            {
                options.Add($"{res.width} x {res.height}");
            }
            resolutionDropdown.AddOptions(options);
        }
    }

    void LoadCurrentSettings()
    {
        if (settings == null) return;

        if (qualityDropdown != null) qualityDropdown.value = settings.qualityLevel;
        if (fullscreenToggle != null) fullscreenToggle.isOn = settings.fullscreen;
        if (vsyncToggle != null) vsyncToggle.isOn = settings.vsync;
        if (brightnessSlider != null) brightnessSlider.value = settings.brightness;
        if (fovSlider != null) fovSlider.value = settings.fieldOfView;

        if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = settings.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = settings.sfxVolume;
        if (ambientVolumeSlider != null) ambientVolumeSlider.value = settings.ambientVolume;

        if (sensitivitySlider != null) sensitivitySlider.value = settings.mouseSensitivity;
        if (invertYToggle != null) invertYToggle.isOn = settings.invertY;

        if (subtitlesToggle != null) subtitlesToggle.isOn = settings.showSubtitles;
        if (tutorialsToggle != null) tutorialsToggle.isOn = settings.showTutorials;
        if (uiScaleSlider != null) uiScaleSlider.value = settings.uiScale;
    }

    void ApplySettings()
    {
        if (settings == null) return;

        if (qualityDropdown != null) settings.SetQualityLevel(qualityDropdown.value);
        if (fullscreenToggle != null) settings.SetFullscreen(fullscreenToggle.isOn);
        if (vsyncToggle != null) settings.SetVSync(vsyncToggle.isOn);
        if (brightnessSlider != null) settings.SetBrightness(brightnessSlider.value);
        if (fovSlider != null) settings.SetFieldOfView(fovSlider.value);

        if (masterVolumeSlider != null) settings.SetMasterVolume(masterVolumeSlider.value);
        if (musicVolumeSlider != null) settings.SetMusicVolume(musicVolumeSlider.value);
        if (sfxVolumeSlider != null) settings.SetSFXVolume(sfxVolumeSlider.value);

        if (sensitivitySlider != null) settings.SetMouseSensitivity(sensitivitySlider.value);
        if (invertYToggle != null) settings.invertY = invertYToggle.isOn;

        settings.showSubtitles = subtitlesToggle?.isOn ?? true;
        settings.showTutorials = tutorialsToggle?.isOn ?? true;

        settings.SaveSettings();
    }

    void ResetSettings()
    {
        settings?.ResetToDefaults();
        LoadCurrentSettings();
    }

    void CloseSettings()
    {
        gameObject.SetActive(false);

        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null)
        {
            pauseMenu.pausePanel.SetActive(true);
        }
    }
}