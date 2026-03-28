using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's companion roster. Supports switching companions,
/// issuing combat commands, companion dialogue triggers, and companion-specific abilities.
/// </summary>
public class CompanionManager : MonoBehaviour
{
    public static CompanionManager Instance { get; private set; }

    [Header("Active Companion")]
    public GameObject currentCompanion;
    public int currentCompanionIndex = -1;

    [Header("Settings")]
    public float followDistance = 3f;
    public float teleportDistance = 30f;
    public KeyCode switchKey = KeyCode.Alpha5;
    public KeyCode commandKey = KeyCode.Alpha6;

    [Header("Roster")]
    public List<GameObject> companionRoster = new List<GameObject>();

    // Companion info
    [System.Serializable]
    public class CompanionData
    {
        public string name;
        public string title;
        public string specialAbility;
        public GameObject prefab;
    }

    public CompanionData[] availableCompanions = new CompanionData[]
    {
        new CompanionData { name = "Kelsier", title = "The Survivor", specialAbility = "Steel Push Barrage" },
        new CompanionData { name = "Vin", title = "Mistborn", specialAbility = "Stealth Strike" },
        new CompanionData { name = "Sazed", title = "Keeper of Terris", specialAbility = "Feruchemy Heal" }
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Switch companion with number key
        if (Input.GetKeyDown(switchKey))
            CycleCompanion();

        // Command companion to attack current target
        if (Input.GetKeyDown(commandKey))
            CommandAttackTarget();
    }

    public void RegisterCompanion(GameObject companion)
    {
        if (!companionRoster.Contains(companion))
        {
            companionRoster.Add(companion);

            // First companion registered becomes active
            if (currentCompanion == null)
            {
                currentCompanionIndex = 0;
                currentCompanion = companion;
            }
            else
            {
                companion.SetActive(false);
            }
        }
    }

    public void CycleCompanion()
    {
        if (companionRoster.Count <= 1) return;

        // Deactivate current
        if (currentCompanion != null)
            currentCompanion.SetActive(false);

        // Cycle to next
        currentCompanionIndex = (currentCompanionIndex + 1) % companionRoster.Count;
        currentCompanion = companionRoster[currentCompanionIndex];
        currentCompanion.SetActive(true);

        // Teleport to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentCompanion.transform.position = player.transform.position - player.transform.forward * 2f;
            var agent = currentCompanion.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.Warp(currentCompanion.transform.position);
        }

        string name = currentCompanion.name;
        NotificationSystem.Instance?.ShowNotification($"{name} is now following you.");
        SoundManager.Instance?.PlayNotification();
    }

    public void SwitchCompanion(int index)
    {
        if (index < 0 || index >= companionRoster.Count) return;

        if (currentCompanion != null)
            currentCompanion.SetActive(false);

        currentCompanionIndex = index;
        currentCompanion = companionRoster[index];
        currentCompanion.SetActive(true);
    }

    public void RequestCombatSupport(Transform enemy)
    {
        if (currentCompanion == null || enemy == null) return;
        currentCompanion.SendMessage("OnSupportRequested", enemy, SendMessageOptions.DontRequireReceiver);
    }

    void CommandAttackTarget()
    {
        // Raycast from camera to find target
        Camera cam = Camera.main;
        if (cam == null || currentCompanion == null) return;

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 30f))
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            AIController ai = hit.collider.GetComponent<AIController>();
            if (enemy != null || ai != null)
            {
                RequestCombatSupport(hit.transform);
                NotificationSystem.Instance?.ShowNotification($"Commanding {currentCompanion.name} to attack!");
            }
        }
    }

    /// <summary>
    /// Dismiss the current companion. They stop following.
    /// </summary>
    public void DismissCompanion()
    {
        if (currentCompanion != null)
        {
            currentCompanion.SetActive(false);
            currentCompanion = null;
            currentCompanionIndex = -1;
        }
    }

    public string GetCurrentCompanionName()
    {
        return currentCompanion != null ? currentCompanion.name : "None";
    }

    public int GetCompanionCount() => companionRoster.Count;
}
