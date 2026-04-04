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
        EnsureSingleton<DamageNumbersUI>("DamageNumbersUI");
        EnsureSingleton<MetalWheelMistOverlay>("MetalWheelMistOverlay");
        EnsureSingleton<CursorManager>("CursorManager");
        EnsureSingleton<HitstopManager>("HitstopManager");
        EnsureSingleton<MinimapSystem>("MinimapSystem");

        // PerformanceManager is optional — only add if the class exists in build
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

        // Quest and Dialogue databases auto-populate if present

        // Start dialogue database
        if (FindObjectOfType<DialogueDatabase>() == null)
        {
            GameObject go = new GameObject("DialogueDatabase");
            go.AddComponent<DialogueDatabase>();
        }

    }

    void EnsureComponent<T>(GameObject obj) where T : MonoBehaviour
    {
        if (obj.GetComponent<T>() == null)
            obj.AddComponent<T>();
    }
}
