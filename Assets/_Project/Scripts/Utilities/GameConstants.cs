using UnityEngine;

namespace MistbornGame.Utilities
{
    public class GameConstants : MonoBehaviour
    {
        // ... (Keep your existing SerializedFields here)

        // 1. Add this public property to bridge the gap
        public static GameConstants Instance => _instance;

        public static string Version => Instance.version;
        public static float FixedDeltaTime => Instance.fixedDeltaTime;
        public static int TargetFrameRate => Instance.targetFrameRate;
        public static float DefaultGravity => Instance.defaultGravity;
        public static float TerminalVelocity => Instance.terminalVelocity;
        public static float MasterVolume => Instance.masterVolume;
        public static float MusicVolume => Instance.musicVolume;
        public static float SfxVolume => Instance.sfxVolume;
        public static bool ShowDebugLogs => Instance.showDebugLogs;
        public static bool EnableCheatCodes => Instance.enableCheatCodes;

        private static GameConstants _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ... (Keep InitializeSettings as is)
    }
}
