using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public enum SettingType { Float, Int, Bool, String }

        [Serializable]
        public class Setting
        {
            public string key;
            public SettingType type;
            public float floatValue;
            public int intValue;
            public bool boolValue;
            public string stringValue;
        }

        [Header("Default Settings")]
        [SerializeField] private List<Setting> m_defaultSettings = new List<Setting>();

        private Dictionary<string, Setting> m_settings = new Dictionary<string, Setting>();

        public event Action<string> OnSettingChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        private void LoadSettings()
        {
            foreach (Setting setting in m_defaultSettings)
            {
                m_settings[setting.key] = new Setting
                {
                    key = setting.key,
                    type = setting.type,
                    floatValue = PlayerPrefs.GetFloat(setting.key, setting.floatValue),
                    intValue = PlayerPrefs.GetInt(setting.key, setting.intValue),
                    boolValue = PlayerPrefs.GetInt(setting.key, setting.boolValue ? 1 : 0) == 1,
                    stringValue = PlayerPrefs.GetString(setting.key, setting.stringValue)
                };
            }
        }

        private void SaveSettings()
        {
            foreach (var kvp in m_settings)
            {
                Setting setting = kvp.Value;
                switch (setting.type)
                {
                    case SettingType.Float:
                        PlayerPrefs.SetFloat(setting.key, setting.floatValue);
                        break;
                    case SettingType.Int:
                        PlayerPrefs.SetInt(setting.key, setting.intValue);
                        break;
                    case SettingType.Bool:
                        PlayerPrefs.SetInt(setting.key, setting.boolValue ? 1 : 0);
                        break;
                    case SettingType.String:
                        PlayerPrefs.SetString(setting.key, setting.stringValue);
                        break;
                }
            }
            PlayerPrefs.Save();
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (m_settings.TryGetValue(key, out Setting setting) && setting.type == SettingType.Float)
                return setting.floatValue;
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (m_settings.TryGetValue(key, out Setting setting) && setting.type == SettingType.Int)
                return setting.intValue;
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (m_settings.TryGetValue(key, out Setting setting) && setting.type == SettingType.Bool)
                return setting.boolValue;
            return defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (m_settings.TryGetValue(key, out Setting setting) && setting.type == SettingType.String)
                return setting.stringValue;
            return defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            EnsureSettingExists(key, SettingType.Float);
            m_settings[key].floatValue = value;
            PlayerPrefs.SetFloat(key, value);
            OnSettingChanged?.Invoke(key);
        }

        public void SetInt(string key, int value)
        {
            EnsureSettingExists(key, SettingType.Int);
            m_settings[key].intValue = value;
            PlayerPrefs.SetInt(key, value);
            OnSettingChanged?.Invoke(key);
        }

        public void SetBool(string key, bool value)
        {
            EnsureSettingExists(key, SettingType.Bool);
            m_settings[key].boolValue = value;
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            OnSettingChanged?.Invoke(key);
        }

        public void SetString(string key, string value)
        {
            EnsureSettingExists(key, SettingType.String);
            m_settings[key].stringValue = value;
            PlayerPrefs.SetString(key, value);
            OnSettingChanged?.Invoke(key);
        }

        private void EnsureSettingExists(string key, SettingType type)
        {
            if (!m_settings.ContainsKey(key))
            {
                m_settings[key] = new Setting { key = key, type = type };
            }
        }

        public void ResetToDefaults()
        {
            foreach (Setting setting in m_defaultSettings)
            {
                SetValue(setting);
            }
            SaveSettings();
        }

        private void SetValue(Setting setting)
        {
            switch (setting.type)
            {
                case SettingType.Float:
                    SetFloat(setting.key, setting.floatValue);
                    break;
                case SettingType.Int:
                    SetInt(setting.key, setting.intValue);
                    break;
                case SettingType.Bool:
                    SetBool(setting.key, setting.boolValue);
                    break;
                case SettingType.String:
                    SetString(setting.key, setting.stringValue);
                    break;
            }
        }

        public void ApplySettings()
        {
            QualitySettings.SetQualityLevel(GetInt("QualityLevel", 2));
            Application.targetFrameRate = GetInt("TargetFPS", 60);

            Screen.fullScreen = GetBool("Fullscreen", true);
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

            AudioListener.volume = GetFloat("MasterVolume", 1f);
        }
    }

    public static class GameSettings
    {
        private const string MASTER_VOLUME = "MasterVolume";
        private const string MUSIC_VOLUME = "MusicVolume";
        private const string SFX_VOLUME = "SFXVolume";
        private const string VOICE_VOLUME = "VoiceVolume";
        private const string GRAPHICS_QUALITY = "QualityLevel";
        private const string FULLSCREEN = "Fullscreen";
        private const string VSYNC = "VSync";
        private const string MOUSE_SENSITIVITY = "MouseSensitivity";
        private const string INVERT_Y = "InvertY";
        private const string FOV = "FOV";
        private const string SUBTITLES = "Subtitles";

        public static float MasterVolume
        {
            get => GetFloat(MASTER_VOLUME, 1f);
            set => SetFloat(MASTER_VOLUME, value);
        }

        public static float MusicVolume
        {
            get => GetFloat(MUSIC_VOLUME, 0.7f);
            set => SetFloat(MUSIC_VOLUME, value);
        }

        public static float SFXVolume
        {
            get => GetFloat(SFX_VOLUME, 0.8f);
            set => SetFloat(SFX_VOLUME, value);
        }

        public static float VoiceVolume
        {
            get => GetFloat(VOICE_VOLUME, 1f);
            set => SetFloat(VOICE_VOLUME, value);
        }

        public static int GraphicsQuality
        {
            get => GetInt(GRAPHICS_QUALITY, 2);
            set
            {
                SetInt(GRAPHICS_QUALITY, value);
                QualitySettings.SetQualityLevel(value);
            }
        }

        public static bool Fullscreen
        {
            get => GetBool(FULLSCREEN, true);
            set
            {
                SetBool(FULLSCREEN, value);
                Screen.fullScreen = value;
            }
        }

        public static bool VSync
        {
            get => GetBool(VSYNC, true);
            set
            {
                SetBool(VSYNC, value);
                QualitySettings.vSyncCount = value ? 1 : 0;
            }
        }

        public static float MouseSensitivity
        {
            get => GetFloat(MOUSE_SENSITIVITY, 2f);
            set => SetFloat(MOUSE_SENSITIVITY, value);
        }

        public static bool InvertY
        {
            get => GetBool(INVERT_Y, false);
            set => SetBool(INVERT_Y, value);
        }

        public static float FOV
        {
            get => GetFloat(FOV, 60f);
            set => SetFloat(FOV, value);
        }

        public static bool Subtitles
        {
            get => GetBool(SUBTITLES, true);
            set => SetBool(SUBTITLES, value);
        }

        private static float GetFloat(string key, float defaultValue)
        {
            return SettingsManager.Instance?.GetFloat(key, defaultValue) ?? defaultValue;
        }

        private static int GetInt(string key, int defaultValue)
        {
            return SettingsManager.Instance?.GetInt(key, defaultValue) ?? defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            return SettingsManager.Instance?.GetBool(key, defaultValue) ?? defaultValue;
        }

        private static void SetFloat(string key, float value)
        {
            SettingsManager.Instance?.SetFloat(key, value);
        }

        private static void SetInt(string key, int value)
        {
            SettingsManager.Instance?.SetInt(key, value);
        }

        private static void SetBool(string key, bool value)
        {
            SettingsManager.Instance?.SetBool(key, value);
        }
    }
}
