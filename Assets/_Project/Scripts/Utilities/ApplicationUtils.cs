using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for Application operations
    /// </summary>
    public static class ApplicationUtils
    {
        /// <summary>
        /// Gets the platform name
        /// </summary>
        public static string GetPlatformName()
        {
            return Application.platform.ToString();
        }

        /// <summary>
        /// Checks if running on mobile platform
        /// </summary>
        public static bool IsMobile()
        {
            return Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }

        /// <summary>
        /// Checks if running on editor
        /// </summary>
        public static bool IsEditor()
        {
            return Application.isEditor;
        }

        /// <summary>
        /// Checks if running on Windows
        /// </summary>
        public static bool IsWindows()
        {
            return Application.platform == RuntimePlatform.WindowsPlayer || 
                   Application.platform == RuntimePlatform.WindowsEditor;
        }

        /// <summary>
        /// Checks if running on macOS
        /// </summary>
        public static bool IsMacOS()
        {
            return Application.platform == RuntimePlatform.OSXPlayer || 
                   Application.platform == RuntimePlatform.OSXEditor;
        }

        /// <summary>
        /// Checks if running on Linux
        /// </summary>
        public static bool IsLinux()
        {
            return Application.platform == RuntimePlatform.LinuxPlayer || 
                   Application.platform == RuntimePlatform.LinuxEditor;
        }

        /// <summary>
        /// Gets the application version
        /// </summary>
        public static string GetVersion()
        {
            return Application.version;
        }

        /// <summary>
        /// Gets the Unity version
        /// </summary>
        public static string GetUnityVersion()
        {
            return Application.unityVersion;
        }

        /// <summary>
        /// Gets the product name
        /// </summary>
        public static string GetProductName()
        {
            return Application.productName;
        }

        /// <summary>
        /// Gets the company name
        /// </summary>
        public static string GetCompanyName()
        {
            return Application.companyName;
        }

        /// <summary>
        /// Gets the data path
        /// </summary>
        public static string GetDataPath()
        {
            return Application.dataPath;
        }

        /// <summary>
        /// Gets the persistent data path
        /// </summary>
        public static string GetPersistentDataPath()
        {
            return Application.persistentDataPath;
        }

        /// <summary>
        /// Gets the temporary cache path
        /// </summary>
        public static string GetTemporaryCachePath()
        {
            return Application.temporaryCachePath;
        }

        /// <summary>
        /// Gets the streaming assets path
        /// </summary>
        public static string GetStreamingAssetsPath()
        {
            return Application.streamingAssetsPath;
        }

        /// <summary>
        /// Quits the application
        /// </summary>
        public static void Quit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Opens a URL in the default browser
        /// </summary>
        public static void OpenURL(string url)
        {
            Application.OpenURL(url);
        }

        /// <summary>
        /// Gets the system language
        /// </summary>
        public static SystemLanguage GetSystemLanguage()
        {
            return Application.systemLanguage;
        }

        /// <summary>
        /// Gets the internet reachability
        /// </summary>
        public static NetworkReachability GetInternetReachability()
        {
            return Application.internetReachability;
        }

        /// <summary>
        /// Checks if there is internet connectivity
        /// </summary>
        public static bool HasInternet()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// Gets the target frame rate
        /// </summary>
        public static int GetTargetFrameRate()
        {
            return Application.targetFrameRate;
        }

        /// <summary>
        /// Sets the target frame rate
        /// </summary>
        public static void SetTargetFrameRate(int frameRate)
        {
            Application.targetFrameRate = frameRate;
        }

        /// <summary>
        /// Gets the run time in seconds
        /// </summary>
        public static float GetRunTime()
        {
            return Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Gets the run time formatted as hours:minutes:seconds
        /// </summary>
        public static string GetRunTimeFormatted()
        {
            float time = GetRunTime();
            int hours = (int)(time / 3600f);
            int minutes = (int)((time % 3600f) / 60f);
            int seconds = (int)(time % 60f);
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// Checks if the application is focused
        /// </summary>
        public static bool IsFocused()
        {
            return Application.isFocused;
        }

        /// <summary>
        /// Gets the background loading priority
        /// </summary>
        public static ThreadPriority GetBackgroundLoadingPriority()
        {
            return Application.backgroundLoadingPriority;
        }

        /// <summary>
        /// Sets the background loading priority
        /// </summary>
        public static void SetBackgroundLoadingPriority(ThreadPriority priority)
        {
            Application.backgroundLoadingPriority = priority;
        }

        /// <summary>
        /// Gets the install mode
        /// </summary>
        public static ApplicationInstallMode GetInstallMode()
        {
            return Application.installMode;
        }

        /// <summary>
        /// Gets the sandbox type
        /// </summary>
        public static ApplicationSandboxType GetSandboxType()
        {
            return Application.sandboxType;
        }

        /// <summary>
        /// Checks if the application is playing
        /// </summary>
        public static bool IsPlaying()
        {
            return Application.isPlaying;
        }
    }
}
