using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI Canvases")]
    public Canvas mainCanvas;
    public Canvas inventoryCanvas;
    public Canvas questCanvas;
    public Canvas settingsCanvas;
    
    [Header("UI Panels")]
    public GameObject[] toggleablePanels;
    
    [Header("UI Elements")]
    public TextMeshProUGUI fpsCounter;
    public TextMeshProUGUI debugInfo;
    
    [Header("Settings")]
    public bool showFPS = true;
    public bool showDebugInfo = false;
    public KeyCode toggleDebug = KeyCode.F3;
    
    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0f;
    private int fpsFrameCount = 0;
    private float fps = 0f;
    
    private Dictionary<string, GameObject> panelStates = new Dictionary<string, GameObject>();
    
    void Start()
    {
        InitializePanels();
    }
    
    void InitializePanels()
    {
        foreach (GameObject panel in toggleablePanels)
        {
            if (panel != null)
            {
                panelStates[panel.name] = panel;
            }
        }
    }
    
    void Update()
    {
        UpdateFPS();
        HandleInput();
    }
    
    void UpdateFPS()
    {
        if (!showFPS || fpsCounter == null) return;
        
        fpsFrameCount++;
        fpsTimer += Time.deltaTime;
        
        if (fpsTimer >= fpsUpdateInterval)
        {
            fps = fpsFrameCount / fpsTimer;
            fpsCounter.text = $"FPS: {Mathf.RoundToInt(fps)}";
            fpsFrameCount = 0;
            fpsTimer = 0f;
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(toggleDebug))
        {
            showDebugInfo = !showDebugInfo;
            
            if (debugInfo != null)
            {
                debugInfo.gameObject.SetActive(showDebugInfo);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            TogglePanel("InventoryPanel");
        }
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            TogglePanel("QuestPanel");
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsCanvas != null && settingsCanvas.gameObject.activeSelf)
            {
                CloseAllPanels();
            }
            else
            {
                TogglePanel("PauseMenu");
            }
        }
    }
    
    public void TogglePanel(string panelName)
    {
        if (panelStates.ContainsKey(panelName))
        {
            GameObject panel = panelStates[panelName];
            bool isActive = panel.activeSelf;
            panel.SetActive(!isActive);
            
            if (!isActive)
            {
                Time.timeScale = 0f;
            }
            else
            {
                CheckAllPanelsClosed();
            }
        }
    }
    
    public void ShowPanel(string panelName)
    {
        if (panelStates.ContainsKey(panelName))
        {
            panelStates[panelName].SetActive(true);
            Time.timeScale = 0f;
        }
    }
    
    public void HidePanel(string panelName)
    {
        if (panelStates.ContainsKey(panelName))
        {
            panelStates[panelName].SetActive(false);
            CheckAllPanelsClosed();
        }
    }
    
    public void CloseAllPanels()
    {
        foreach (var panel in panelStates)
        {
            panel.Value.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }
    
    void CheckAllPanelsClosed()
    {
        foreach (var panel in panelStates)
        {
            if (panel.Value.activeSelf) return;
        }
        
        Time.timeScale = 1f;
    }
    
    public bool IsAnyPanelOpen()
    {
        foreach (var panel in panelStates)
        {
            if (panel.Value.activeSelf) return true;
        }
        return false;
    }
}

public class SliderUIBinding : MonoBehaviour
{
    [Header("Binding Settings")]
    public string settingName;
    public Slider slider;
    public TextMeshProUGUI valueText;
    public float minValue = 0f;
    public float maxValue = 100f;
    public float defaultValue = 50f;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent<float> OnValueChanged;
    
    private static Dictionary<string, float> settings = new Dictionary<string, float>();
    
    void Start()
    {
        if (!settings.ContainsKey(settingName))
        {
            settings[settingName] = defaultValue;
        }
        
        if (slider != null)
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = settings[settingName];
            slider.onValueChanged.AddListener(OnSliderChanged);
        }
        
        UpdateValueText();
    }
    
    void OnSliderChanged(float value)
    {
        settings[settingName] = value;
        UpdateValueText();
        OnValueChanged?.Invoke(value);
        PlayerPrefs.SetFloat(settingName, value);
    }
    
    void UpdateValueText()
    {
        if (valueText != null)
        {
            valueText.text = Mathf.RoundToInt(settings[settingName]).ToString();
        }
    }
    
    public static float GetSetting(string name)
    {
        if (settings.ContainsKey(name))
        {
            return settings[name];
        }
        return 0f;
    }
    
    public static void SetSetting(string name, float value)
    {
        settings[name] = value;
        PlayerPrefs.SetFloat(name, value);
    }
}

public class ToggleUIBinding : MonoBehaviour
{
    [Header("Binding Settings")]
    public string settingName;
    public Toggle toggle;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent<bool> OnValueChanged;
    
    private static Dictionary<string, bool> settings = new Dictionary<string, bool>();
    
    void Start()
    {
        if (!settings.ContainsKey(settingName))
        {
            settings[settingName] = PlayerPrefs.GetInt(settingName, 1) == 1;
        }
        
        if (toggle != null)
        {
            toggle.isOn = settings[settingName];
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }
    
    void OnToggleChanged(bool value)
    {
        settings[settingName] = value;
        OnValueChanged?.Invoke(value);
        PlayerPrefs.SetInt(settingName, value ? 1 : 0);
    }
    
    public static bool GetSetting(string name)
    {
        if (settings.ContainsKey(name))
        {
            return settings[name];
        }
        return false;
    }
}

public class DropdownUIBinding : MonoBehaviour
{
    [Header("Binding Settings")]
    public string settingName;
    public TMP_Dropdown dropdown;
    public string[] optionValues;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent<int> OnValueChanged;
    
    private static Dictionary<string, int> settings = new Dictionary<string, int>();
    
    void Start()
    {
        if (!settings.ContainsKey(settingName))
        {
            settings[settingName] = PlayerPrefs.GetInt(settingName, 0);
        }
        
        if (dropdown != null)
        {
            dropdown.value = settings[settingName];
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }
    
    void OnDropdownChanged(int value)
    {
        settings[settingName] = value;
        OnValueChanged?.Invoke(value);
        PlayerPrefs.SetInt(settingName, value);
    }
    
    public static int GetSetting(string name)
    {
        if (settings.ContainsKey(name))
        {
            return settings[name];
        }
        return 0;
    }
}

public class ResolutionManager : MonoBehaviour
{
    [Header("Resolution Options")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    
    private Resolution[] resolutions;
    
    void Start()
    {
        InitializeResolutions();
    }
    
    void InitializeResolutions()
    {
        resolutions = Screen.resolutions;
        
        if (resolutionDropdown != null)
        {
            List<string> options = new List<string>();
            int currentIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = $"{resolutions[i].width} x {resolutions[i].height}";
                options.Add(option);
                
                if (resolutions[i].width == Screen.width && 
                    resolutions[i].height == Screen.height)
                {
                    currentIndex = i;
                }
            }
            
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }
    
    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}

public class SettingsManager : MonoBehaviour
{
    [Header("Quality Settings")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown textureDropdown;
    public TMP_Dropdown antialiasingDropdown;
    public TMP_Dropdown vSyncDropdown;
    
    [Header("Audio Settings")]
    public SliderUIBinding masterVolume;
    public SliderUIBinding musicVolume;
    public SliderUIBinding sfxVolume;
    public SliderUIBinding dialogueVolume;
    
    [Header("Gameplay Settings")]
    public ToggleUIBinding subtitles;
    public ToggleUIBinding screenShake;
    public SliderUIBinding mouseSensitivity;
    public SliderUIBinding fieldOfView;
    
    void Start()
    {
        LoadSettings();
    }
    
    public void LoadSettings()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
        
        if (subtitles != null)
        {
            subtitles.toggle.isOn = PlayerPrefs.GetInt("Subtitles", 1) == 1;
        }
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }
    
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
    }
    
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
    
    public void SetTextureQuality(int index)
    {
        QualitySettings.masterTextureLimit = index;
    }
    
    public void SetAntialiasing(int level)
    {
        QualitySettings.antiAliasing = level;
    }
    
    public void SetVSync(int enabled)
    {
        QualitySettings.vSyncCount = enabled;
    }
    
    public void ApplyFOV(float fov)
    {
        Camera.main?.GetComponent<UnityEngine.Camera>()?.ResetAspect();
        if (Camera.main != null)
        {
            Camera.main.fieldOfView = fov;
        }
    }
}

public class NotificationSystem : MonoBehaviour
{
    [Header("Notification Settings")]
    public GameObject notificationPrefab;
    public Transform notificationContainer;
    public float notificationDuration = 3f;
    public int maxNotifications = 5;
    
    [Header("Notification Types")]
    public Color infoColor = Color.blue;
    public Color successColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
    public Color questColor = Color.magenta;
    
    private Queue<GameObject> activeNotifications = new Queue<GameObject>();
    
    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        if (notificationPrefab == null || notificationContainer == null) return;
        
        if (activeNotifications.Count >= maxNotifications)
        {
            GameObject oldNotification = activeNotifications.Dequeue();
            Destroy(oldNotification);
        }
        
        GameObject notification = Instantiate(notificationPrefab, notificationContainer);
        NotificationUI ui = notification.GetComponent<NotificationUI>();
        
        if (ui != null)
        {
            ui.Initialize(message, GetColor(type), notificationDuration);
        }
        
        activeNotifications.Enqueue(notification);
    }
    
    Color GetColor(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Info: return infoColor;
            case NotificationType.Success: return successColor;
            case NotificationType.Warning: return warningColor;
            case NotificationType.Error: return errorColor;
            case NotificationType.Quest: return questColor;
            default: return infoColor;
        }
    }
    
    public void ShowQuestNotification(string questName, string objective)
    {
        ShowNotification($"Quest: {questName}\n{objective}", NotificationType.Quest);
    }
    
    public void ShowItemNotification(string itemName, int amount = 1)
    {
        string message = amount > 1 ? $"x{amount} {itemName}" : itemName;
        ShowNotification($"Received: {message}", NotificationType.Success);
    }
    
    public void ShowErrorNotification(string error)
    {
        ShowNotification(error, NotificationType.Error);
    }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Quest
}

public class NotificationUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public Image iconImage;
    
    private float duration;
    private float timer = 0f;
    
    public void Initialize(string message, Color color, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
        
        this.duration = duration;
        timer = 0f;
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        float alpha = 1f - (timer / duration);
        alpha = Mathf.Clamp01(alpha);
        
        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = alpha;
            backgroundImage.color = color;
        }
        
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
