using UnityEngine;

/// <summary>
/// Central game constants and configuration.
/// All physics values reference PHYSICS-MATH-BOOK.md.
/// </summary>
public class GameConstants : MonoBehaviour
{
    public static GameConstants Instance { get; private set; }

    [Header("Game")]
    public string version = "0.3.0";
    public int targetFrameRate = 60;

    [Header("Physics — PHYSICS-MATH-BOOK.md")]
    public float gravity = -9.81f;
    public float terminalVelocity = -50f;

    [Header("Metallurgic Constants")]
    [Tooltip("Conservative A from handbook (Ember A = 35316)")]
    public float metallurgicStrengthA = 1500f;
    public float coinMass = 0.01f;
    public float standardCoinVelocity = 490f;

    [Header("Storecraft Constants")]
    public float baseStoreRate = 10f;
    public float baseTapRate = 15f;
    public float defaultDiminishingFactor = 0.1f;

    [Header("Compounding Constants")]
    public float compoundingBaseMultiplier = 10f;
    public float compoundingDiminishingDelta = 0.3f;
    public int maxCompoundingCycles = 4;

    [Header("Time Bubble Constants — Section 9")]
    public float cadmiumSlowFactor = 0.15f;
    public float bendalloyFastFactor = 8f;

    [Header("Pewter Constants — Section 8")]
    public float pewterEfficiencyK = 2f;
    public float pewterMuscleAlpha = 0.5f;

    [Header("Player Defaults")]
    public float playerMaxHealth = 100f;
    public float playerMaxStamina = 100f;
    public float playerBaseDamage = 15f;
    [Tooltip("2 Unity units = 5 feet. See WorldScale.cs")]
    public float playerMoveSpeed = 1.8f;   // WorldScale.WalkSpeed (~4.5 ft/s)
    public float playerSprintSpeed = 6f;   // WorldScale.RunSpeed (~15 ft/s)

    [Header("Audio")]
    public float masterVolume = 0.8f;
    public float musicVolume = 0.6f;
    public float sfxVolume = 0.7f;

    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool enableCheatCodes = false;
    public bool showPerformanceStats = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = targetFrameRate;
        Physics.gravity = new Vector3(0, gravity, 0);
    }
}
