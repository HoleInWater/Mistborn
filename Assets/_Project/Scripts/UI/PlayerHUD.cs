using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Master HUD controller — XP bar, coin counter, quest tracker, compass markers.
/// Single Update loop drives all lightweight HUD elements for performance.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("XP & Level")]
    public Image xpBarFill;
    public Text levelText;
    public Text xpText;
    public GameObject levelUpEffect;

    [Header("Coin Counter")]
    public Text coinCountText;
    public Text vialCountText;

    [Header("Quest Tracker")]
    public Text questTrackerText;
    public int maxTrackedQuests = 3;

    [Header("Compass / Waypoint")]
    public RectTransform compassBar;
    public GameObject waypointMarkerPrefab;
    public RectTransform waypointContainer;

    [Header("Metal Sight Indicator")]
    public Image metalSightIcon;
    public Color metalSightActiveColor = new Color(0.3f, 0.5f, 1f);
    public Color metalSightInactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

    [Header("Status Effect Icons")]
    public Transform statusEffectContainer;
    public GameObject statusEffectIconPrefab;

    // Cached refs
    private PlayerExperience xp;
    private CoinPouch coins;
    private MetalVialSystem vials;
    private StatusEffects statusEffects;
    private Camera mainCam;
    private float hudUpdateTimer;
    private const float HUD_UPDATE_INTERVAL = 0.15f; // Update 7x/sec, not 60

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            xp = player.GetComponent<PlayerExperience>();
            coins = player.GetComponent<CoinPouch>();
            vials = player.GetComponent<MetalVialSystem>();
            statusEffects = player.GetComponent<StatusEffects>();
        }

        mainCam = Camera.main;

        // Listen for level ups
        if (xp != null)
            xp.OnLevelUp += OnLevelUp;
    }

    void Update()
    {
        hudUpdateTimer -= Time.deltaTime;
        if (hudUpdateTimer > 0f) return;
        hudUpdateTimer = HUD_UPDATE_INTERVAL;

        UpdateXPBar();
        UpdateCoinCounter();
        UpdateQuestTracker();
        UpdateCompass();
        UpdateMetalSightIndicator();
        UpdateStatusEffects();
    }

    // ── XP ───────────────────────────────────────────────────────────────

    void UpdateXPBar()
    {
        if (xp == null) return;

        if (xpBarFill != null)
            xpBarFill.fillAmount = xp.currentXP / xp.xpToNextLevel;
        if (levelText != null)
            levelText.text = $"Lv {xp.currentLevel}";
        if (xpText != null)
            xpText.text = $"{Mathf.FloorToInt(xp.currentXP)}/{Mathf.FloorToInt(xp.xpToNextLevel)}";
    }

    void OnLevelUp()
    {
        if (levelUpEffect != null)
        {
            levelUpEffect.SetActive(true);
            Invoke("HideLevelUp", 2f);
        }
        NotificationSystem.Instance?.ShowNotification($"Level Up! Now level {xp.currentLevel}");
        SoundManager.Instance?.PlaySkillUnlock();
    }

    void HideLevelUp()
    {
        if (levelUpEffect != null) levelUpEffect.SetActive(false);
    }

    // ── Coins & Vials ────────────────────────────────────────────────────

    void UpdateCoinCounter()
    {
        if (coins != null && coinCountText != null)
            coinCountText.text = $"{coins.GetCoinCount()}";
        if (vials != null && vialCountText != null)
            vialCountText.text = $"{vials.GetTotalVialCount()} vials";
    }

    // ── Quest Tracker ────────────────────────────────────────────────────

    void UpdateQuestTracker()
    {
        if (questTrackerText == null || QuestManager.Instance == null) return;

        var active = QuestManager.Instance.GetActiveQuests();
        if (active.Count == 0)
        {
            questTrackerText.text = "";
            return;
        }

        string text = "";
        int count = Mathf.Min(active.Count, maxTrackedQuests);
        for (int i = 0; i < count; i++)
        {
            Quest q = active[i];
            text += $"<b>{q.title}</b>\n";
            foreach (var obj in q.objectives)
            {
                string check = obj.isCompleted ? "[x]" : "[ ]";
                text += $"  {check} {obj.description}";
                if (obj.targetCount > 1)
                    text += $" ({obj.currentCount}/{obj.targetCount})";
                text += "\n";
            }
        }
        questTrackerText.text = text;
    }

    // ── Compass ──────────────────────────────────────────────────────────

    void UpdateCompass()
    {
        if (compassBar == null || mainCam == null) return;

        // Rotate compass strip based on camera Y rotation
        float camY = mainCam.transform.eulerAngles.y;
        compassBar.anchoredPosition = new Vector2(-camY * 2f, compassBar.anchoredPosition.y);
    }

    // ── Metal Sight ──────────────────────────────────────────────────────

    void UpdateMetalSightIndicator()
    {
        if (metalSightIcon == null) return;

        MetalLineRenderer mlr = FindObjectOfType<MetalLineRenderer>();
        bool active = mlr != null && mlr.GetVisibleLineCount() > 0;
        metalSightIcon.color = active ? metalSightActiveColor : metalSightInactiveColor;
    }

    // ── Status Effects ───────────────────────────────────────────────────

    void UpdateStatusEffects()
    {
        if (statusEffects == null || statusEffectContainer == null || statusEffectIconPrefab == null) return;

        // Simple: show text for each active effect
        foreach (Transform child in statusEffectContainer)
            Destroy(child.gameObject);

        foreach (var effect in statusEffects.activeEffects)
        {
            GameObject icon = Instantiate(statusEffectIconPrefab, statusEffectContainer);
            Text text = icon.GetComponentInChildren<Text>();
            if (text != null)
                text.text = $"{effect.displayName} {effect.RemainingTime:F0}s";
        }
    }

    void OnDestroy()
    {
        if (xp != null) xp.OnLevelUp -= OnLevelUp;
    }
}
