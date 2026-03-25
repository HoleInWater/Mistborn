using UnityEngine;
using System.Collections.Generic;

public class CheckpointSystem : MonoBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    [Header("Checkpoint Settings")]
    public float activationRadius = 3f;
    public bool showDebugInfo = false;

    [Header("State")]
    public Checkpoint currentCheckpoint;
    public List<Checkpoint> allCheckpoints = new List<Checkpoint>();
    public Vector3 respawnPosition;
    public Quaternion respawnRotation;

    [Header("References")]
    public Transform player;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (allCheckpoints.Count > 0)
        {
            currentCheckpoint = allCheckpoints[0];
            SetRespawnFromCheckpoint(currentCheckpoint);
        }
    }

    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (!allCheckpoints.Contains(checkpoint))
        {
            allCheckpoints.Add(checkpoint);
            checkpoint.checkpointId = allCheckpoints.Count;
        }
    }

    public void OnPlayerEnterCheckpoint(Checkpoint checkpoint)
    {
        if (currentCheckpoint == null || checkpoint.checkpointId > currentCheckpoint.checkpointId)
        {
            if (currentCheckpoint != null)
                currentCheckpoint.Deactivate();

            currentCheckpoint = checkpoint;
            currentCheckpoint.Activate();
            SetRespawnFromCheckpoint(checkpoint);

            Debug.Log($"[CHECKPOINT] Reached checkpoint #{checkpoint.checkpointId}");
            
            EventManager.TriggerEvent("CheckpointReached", new Dictionary<string, object> {
                { "checkpointId", checkpoint.checkpointId },
                { "position", checkpoint.transform.position }
            });
        }
    }

    void SetRespawnFromCheckpoint(Checkpoint checkpoint)
    {
        respawnPosition = checkpoint.respawnPosition;
        respawnRotation = checkpoint.respawnRotation;
    }

    public void RespawnPlayer()
    {
        if (player == null) return;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = respawnPosition;
        player.rotation = respawnRotation;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.currentHealth = health.maxHealth;
        }

        Allomancer allomancer = player.GetComponent<Allomancer>();
        if (allomancer != null)
        {
            allomancer.RefillAllMetals();
        }

        Debug.Log("[CHECKPOINT] Player respawned");

        EventManager.TriggerEvent("PlayerRespawned");
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        Gizmos.color = Color.yellow;
        foreach (var cp in allCheckpoints)
        {
            if (cp == currentCheckpoint)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(cp.transform.position, activationRadius);
        }
    }
}

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Info")]
    public int checkpointId;
    public string checkpointName = "Checkpoint";

    [Header("Respawn Settings")]
    public Vector3 respawnPosition;
    public Quaternion respawnRotation;
    public bool useThisPosition = true;

    [Header("State")]
    public bool isActive = false;

    [Header("Visuals")]
    public GameObject activeVisual;
    public GameObject inactiveVisual;
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.gray;

    void Start()
    {
        if (useThisPosition)
        {
            respawnPosition = transform.position + Vector3.up * 2f;
            respawnRotation = transform.rotation;
        }

        CheckpointSystem.Instance?.RegisterCheckpoint(this);

        if (inactiveVisual != null) inactiveVisual.SetActive(true);
        if (activeVisual != null) activeVisual.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointSystem.Instance.OnPlayerEnterCheckpoint(this);
        }
    }

    public void Activate()
    {
        isActive = true;

        if (activeVisual != null) activeVisual.SetActive(true);
        if (inactiveVisual != null) inactiveVisual.SetActive(false);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.material.color = activeColor;
        }
    }

    public void Deactivate()
    {
        isActive = false;

        if (activeVisual != null) activeVisual.SetActive(false);
        if (inactiveVisual != null) inactiveVisual.SetActive(true);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.material.color = inactiveColor;
        }
    }
}

public class DeathZone : MonoBehaviour
{
    [Header("Settings")]
    public float damagePerSecond = 50f;
    public bool instantKill = false;
    public bool autoRespawn = true;
    public float respawnDelay = 2f;

    [Header("Visuals")]
    public Color warningColor = Color.red;
    public GameObject warningEffect;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (instantKill)
            {
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(health.maxHealth);
                }
            }
            else
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                damageable?.TakeDamage(damagePerSecond * Time.deltaTime);
            }

            if (autoRespawn && CheckpointSystem.Instance != null)
            {
                StartCoroutine(DelayedRespawn(other.gameObject));
            }
        }
    }

    IEnumerator DelayedRespawn(GameObject player)
    {
        yield return new WaitForSeconds(respawnDelay);
        CheckpointSystem.Instance?.RespawnPlayer();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = warningColor;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Settings")]
    public bool enableDeathEffects = true;
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;

    [Header("References")]
    private PlayerHealth health;
    private Allomancer allomancer;
    private BasicPlayerMove movement;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        allomancer = GetComponent<Allomancer>();
        movement = GetComponent<BasicPlayerMove>();
    }

    void OnEnable()
    {
        EventManager.RegisterEvent("PlayerDied", OnPlayerDied);
    }

    void OnDisable()
    {
        EventManager.UnregisterEvent("PlayerDied", OnPlayerDied);
    }

    void OnPlayerDied()
    {
        HandleDeath();
    }

    void HandleDeath()
    {
        Debug.Log("[DEATH] Player has died");

        if (enableDeathEffects && deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (GetComponent<PlayerRagdoll>())
        {
            GetComponent<PlayerRagdoll>().OnDeath();
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);

        if (CheckpointSystem.Instance != null)
        {
            CheckpointSystem.Instance.RespawnPlayer();
        }

        Debug.Log("[DEATH] Respawning player");
    }

    void Update()
    {
        if (health != null && health.IsDead())
        {
            HandleDeath();
        }
    }
}

public class SavePoint : MonoBehaviour
{
    [Header("Save Point Settings")]
    public SaveLoadManager.SaveSlot saveSlot = SaveLoadManager.SaveSlot.Slot1;
    public bool autoSaveOnEnter = true;
    public bool showSaveIndicator = true;

    [Header("Visuals")]
    public GameObject saveIndicator;
    public Color saveActiveColor = Color.cyan;
    public Color saveReadyColor = Color.white;

    private bool playerInRange = false;
    private float saveCooldown = 5f;
    private float lastSaveTime = -10f;

    void Start()
    {
        if (saveIndicator != null) saveIndicator.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SaveGame();
        }

        if (saveIndicator != null && playerInRange)
        {
            bool canSave = Time.time - lastSaveTime >= saveCooldown;
            saveIndicator.GetComponent<Renderer>().material.color = canSave ? saveReadyColor : Color.gray;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (saveIndicator != null) saveIndicator.SetActive(true);

            if (autoSaveOnEnter && (Time.time - lastSaveTime) >= saveCooldown)
            {
                SaveGame();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (saveIndicator != null) saveIndicator.SetActive(false);
        }
    }

    void SaveGame()
    {
        SaveLoadManager.Instance?.SaveGame((int)saveSlot, $"Save Point {checkpointId}");
        lastSaveTime = Time.time;
        Debug.Log($"[SAVE] Game saved at {gameObject.name}");
    }

    private int checkpointId;
}