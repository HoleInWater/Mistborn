using UnityEngine;

namespace MistbornGame.Utilities
{
    public class GameConstants : MonoBehaviour
    {
        [Header("Game Version")]
        [SerializeField] private string version = "1.0.0";
        
        [Header("Time Settings")]
        [SerializeField] private float fixedDeltaTime = 0.02f;
        [SerializeField] private int targetFrameRate = 60;
        
        [Header("Physics")]
        [SerializeField] private float defaultGravity = -9.81f;
        [SerializeField] private float terminalVelocity = -50f;
        
        [Header("Audio")]
        [SerializeField] private float masterVolume = 0.8f;
        [SerializeField] private float musicVolume = 0.6f;
        [SerializeField] private float sfxVolume = 0.7f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private bool enableCheatCodes = false;
        
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
        
        private void InitializeSettings()
        {
            Application.targetFrameRate = targetFrameRate;
            Time.fixedDeltaTime = fixedDeltaTime;
            Physics.gravity = new Vector3(0f, defaultGravity, 0f);
            
            // Set audio levels
            // AudioListener.volume = masterVolume; // Would need actual audio setup
        }
    }
}