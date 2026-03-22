using UnityEngine;

namespace MistbornGame.Utilities
{
    public class GameConstants : MonoBehaviour
    {
        // CHANGE 'private' TO 'protected' FOR ALL OF THESE:
        [Header("Game Version")]
        [SerializeField] protected string version = "1.0.0";
        
        [Header("Time Settings")]
        [SerializeField] protected float fixedDeltaTime = 0.02f;
        [SerializeField] protected int targetFrameRate = 60;
        
        [Header("Physics")]
        [SerializeField] protected float defaultGravity = -9.81f;
        [SerializeField] protected float terminalVelocity = -50f;
        
        [Header("Audio")]
        [SerializeField] protected float masterVolume = 0.8f;
        [SerializeField] protected float musicVolume = 0.6f;
        [SerializeField] protected float sfxVolume = 0.7f;
        
        [Header("Debug")]
        [SerializeField] protected bool showDebugLogs = false;
        [SerializeField] protected bool enableCheatCodes = false;
        
        // This is the property we added in the last step
        public static GameConstants Instance => _instance;

        public static string Version => Instance.version;
        public static float FixedDeltaTime => Instance.fixedDeltaTime;
        // ... (the rest of your static lines)

        private static GameConstants _instance;
        
        // ... (Keep Awake and InitializeSettings as they were)
    }
}
