using UnityEngine;

/// <summary>
/// Master scene bootstrap — ensures all singletons and systems are initialized
/// when a game scene loads. Attach to a "GameManager" GameObject in every scene.
/// Creates missing managers automatically so the game never crashes from missing references.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    [Header("Auto-Create Missing Managers")]
    public bool autoCreateManagers = true;

    [Header("Player Setup")]
    public bool autoSetupPlayer = true;

    void Awake()
    {
        if (autoCreateManagers)
            EnsureManagers();

        if (autoSetupPlayer)
            SetupPlayer();
    }

    void EnsureManagers()
    {
        EnsureSingleton<GameFlowManager>("GameFlowManager");
        EnsureSingleton<EventManager>("EventManager");
        EnsureSingleton<QuestManager>("QuestManager");
        EnsureSingleton<ObjectiveManager>("ObjectiveManager");
        EnsureSingleton<SaveLoadManager>("SaveLoadManager");
        EnsureSingleton<SoundManager>("SoundManager");
        EnsureSingleton<GameConstants>("GameConstants");
        EnsureSingleton<CompanionManager>("CompanionManager");
        EnsureSingleton<AchievementSystem>("AchievementSystem");
        EnsureSingleton<NotificationSystem>("NotificationSystem");
        EnsureSingleton<TutorialSystem>("TutorialSystem");
        EnsureSingleton<CheckpointSystem>("CheckpointSystem");
        EnsureSingleton<TimeBubbleManager>("TimeBubbleManager");
        EnsureSingleton<InputManager>("InputManager");
        EnsureSingleton<LoreCodex>("LoreCodex");
        EnsureSingleton<WeatherGameplayIntegration>("WeatherIntegration");

        // Optional performance manager
        if (FindObjectOfType<PerformanceManager>() == null)
        {
            GameObject go = new GameObject("PerformanceManager");
            go.AddComponent<PerformanceManager>();
        }
    }

    void EnsureSingleton<T>(string name) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() == null)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
        }
    }

    void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Ensure core player components exist
        EnsureComponent<Allomancer>(player);
        EnsureComponent<BasicPlayerMove>(player);
        EnsureComponent<PlayerHealth>(player);
        EnsureComponent<PlayerStamina>(player);
        EnsureComponent<PlayerExperience>(player);
        EnsureComponent<Inventory>(player);
        EnsureComponent<PlayerCombat>(player);
        EnsureComponent<CrouchSystem>(player);
        EnsureComponent<DodgeRoll>(player);
        EnsureComponent<CoinPouch>(player);
        EnsureComponent<MetalMagnet>(player);
        EnsureComponent<ParkourSystem>(player);
        EnsureComponent<MovementExtras>(player);
        EnsureComponent<EmotionalAllomancy>(player);
        EnsureComponent<CognitiveAllomancy>(player);
        EnsureComponent<TemporalAllomancy>(player);

        // Set up checkpoint at player start position
        CheckpointSystem.Instance?.SetCheckpoint(player.transform.position, player.transform.rotation);

        // Start first quest if QuestDatabase exists
        QuestDatabase qdb = FindObjectOfType<QuestDatabase>();
        if (qdb == null)
        {
            GameObject go = new GameObject("QuestDatabase");
            go.AddComponent<QuestDatabase>();
        }

        // Start dialogue database
        if (FindObjectOfType<DialogueDatabase>() == null)
        {
            GameObject go = new GameObject("DialogueDatabase");
            go.AddComponent<DialogueDatabase>();
        }

        Debug.Log("[BOOTSTRAP] Scene initialized. All systems ready.");
    }

    void EnsureComponent<T>(GameObject obj) where T : MonoBehaviour
    {
        if (obj.GetComponent<T>() == null)
            obj.AddComponent<T>();
    }
}
