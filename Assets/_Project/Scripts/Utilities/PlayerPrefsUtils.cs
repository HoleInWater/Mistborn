using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for managing PlayerPrefs with default values and safety checks
    /// </summary>
    public static class PlayerPrefsUtils
    {
        /// <summary>
        /// Gets an integer value from PlayerPrefs with a default fallback
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <summary>
        /// Sets an integer value in PlayerPrefs
        /// </summary>
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        /// <summary>
        /// Gets a float value from PlayerPrefs with a default fallback
        /// </summary>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        /// <summary>
        /// Sets a float value in PlayerPrefs
        /// </summary>
        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        /// <summary>
        /// Gets a string value from PlayerPrefs with a default fallback
        /// </summary>
        public static string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        /// <summary>
        /// Sets a string value in PlayerPrefs
        /// </summary>
        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        /// <summary>
        /// Gets a boolean value from PlayerPrefs (stored as int: 0=false, 1=true)
        /// </summary>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        /// <summary>
        /// Sets a boolean value in PlayerPrefs (stored as int: 0=false, 1=true)
        /// </summary>
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        /// <summary>
        /// Checks if a PlayerPrefs key exists
        /// </summary>
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// Deletes a specific PlayerPrefs key
        /// </summary>
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// Deletes all PlayerPrefs
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        /// <summary>
        /// Saves PlayerPrefs to disk
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets an integer value and increments it, useful for counters
        /// </summary>
        public static int GetAndIncrement(string key, int defaultValue = 0)
        {
            int value = GetInt(key, defaultValue);
            SetInt(key, value + 1);
            return value;
        }

        /// <summary>
        /// Gets a float value and adds to it, useful for accumulators
        /// </summary>
        public static float GetAndAdd(string key, float amount, float defaultValue = 0f)
        {
            float value = GetFloat(key, defaultValue);
            SetFloat(key, value + amount);
            return value;
        }
    }
}
