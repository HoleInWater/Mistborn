using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialSystem : MonoBehaviour
{
    public static TutorialSystem Instance { get; private set; }

    [Header("UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image tutorialImage;
    public Button nextButton;
    public Button skipButton;
    public GameObject continuePrompt;

    [Header("Settings")]
    public float displayDuration = 5f;
    public bool autoShow = true;
    public bool showHints = true;
    public float hintDelay = 3f;

    [Header("Tutorials")]
    public List<TutorialData> tutorials = new List<TutorialData>();
    public Dictionary<string, bool> completedTutorials = new Dictionary<string, bool>();

    private TutorialData currentTutorial;
    private int currentStep = 0;
    private bool isShowingTutorial = false;
    private Coroutine hintCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(NextStep);
        if (skipButton != null) skipButton.onClick.AddListener(SkipTutorial);
    }

    public void ShowTutorial(string tutorialId)
    {
        TutorialData tutorial = tutorials.Find(t => t.tutorialId == tutorialId);
        if (tutorial == null) return;

        if (completedTutorials.ContainsKey(tutorialId) && completedTutorials[tutorialId])
        {
            return;
        }

        currentTutorial = tutorial;
        currentStep = 0;
        isShowingTutorial = true;

        ShowStep(currentStep);
    }

    void ShowStep(int step)
    {
        if (currentTutorial == null || step >= currentTutorial.steps.Count)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep tutorialStep = currentTutorial.steps[step];

        if (titleText != null) titleText.text = tutorialStep.title;
        if (descriptionText != null) descriptionText.text = tutorialStep.description;
        if (tutorialImage != null && tutorialStep.image != null) tutorialImage.sprite = tutorialStep.image;

        tutorialPanel?.SetActive(true);

        if (continuePrompt != null) continuePrompt.SetActive(tutorialStep.waitForInput);
    }

    public void NextStep()
    {
        if (!isShowingTutorial) return;

        currentStep++;

        if (currentTutorial != null && currentStep >= currentTutorial.steps.Count)
        {
            CompleteTutorial();
        }
        else
        {
            ShowStep(currentStep);
        }
    }

    public void SkipTutorial()
    {
        if (currentTutorial != null)
        {
            completedTutorials[currentTutorial.tutorialId] = true;
        }

        isShowingTutorial = false;
        tutorialPanel?.SetActive(false);
    }

    void CompleteTutorial()
    {
        if (currentTutorial != null)
        {
            completedTutorials[currentTutorial.tutorialId] = true;
            Debug.Log($"[TUTORIAL] Completed: {currentTutorial.tutorialId}");
        }

        isShowingTutorial = false;
        tutorialPanel?.SetActive(false);

        EventManager.TriggerEvent("TutorialCompleted", new Dictionary<string, object> {
            { "tutorialId", currentTutorial?.tutorialId }
        });
    }

    public void ShowHint(string hint)
    {
        if (!showHints) return;

        if (hintCoroutine != null) StopCoroutine(hintCoroutine);
        hintCoroutine = StartCoroutine(ShowHintCoroutine(hint));
    }

    IEnumerator ShowHintCoroutine(string hint)
    {
        yield return new WaitForSeconds(hintDelay);

        if (titleText != null) titleText.text = "Hint";
        if (descriptionText != null) descriptionText.text = hint;
        tutorialPanel?.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        tutorialPanel?.SetActive(false);
    }

    public void ShowMovementTutorial()
    {
        ShowTutorial("MOVEMENT");
    }

    public void ShowJumpTutorial()
    {
        ShowTutorial("JUMP");
    }

    public void ShowSprintTutorial()
    {
        ShowTutorial("SPRINT");
    }

    public void ShowAllomancyTutorial()
    {
        ShowTutorial("ALLOMANCY");
    }

    public void ShowSteelPushTutorial()
    {
        ShowTutorial("STEEL_PUSH");
    }

    public void ShowIronPullTutorial()
    {
        ShowTutorial("IRON_PULL");
    }

    public bool HasCompletedTutorial(string tutorialId)
    {
        return completedTutorials.ContainsKey(tutorialId) && completedTutorials[tutorialId];
    }

    public bool IsTutorialActive() => isShowingTutorial;
}

[System.Serializable]
public class TutorialData
{
    public string tutorialId;
    public string name;
    public List<TutorialStep> steps;
}

[System.Serializable]
public class TutorialStep
{
    public string title;
    [TextArea] public string description;
    public Sprite image;
    public bool waitForInput = true;
    public float autoAdvanceDelay = 5f;
}

public class HelpSystem : MonoBehaviour
{
    public static HelpSystem Instance { get; private set; }

    [Header("UI")]
    public GameObject helpPanel;
    public Transform helpContent;
    public GameObject helpEntryPrefab;
    public TMP_InputField searchField;
    public Button closeButton;

    [Header("Entries")]
    public List<HelpEntry> helpEntries = new List<HelpEntry>();

    private List<HelpEntryUI> entryUIs = new List<HelpEntryUI>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;

        if (helpPanel != null) helpPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(CloseHelp);

        PopulateHelpEntries();
    }

    void PopulateHelpEntries()
    {
        helpEntries.Add(new HelpEntry { id = "MOVEMENT", title = "Movement", content = "Use WASD to move your character. Left Shift to sprint." });
        helpEntries.Add(new HelpEntry { id = "JUMP", title = "Jumping", content = "Press Space to jump. Jump buffer allows you to press jump slightly before landing." });
        helpEntries.Add(new HelpEntry { id = "SPRINT", title = "Sprinting", content = "Hold Left Shift while moving to sprint. Uses stamina." });
        helpEntries.Add(new HelpEntry { id = "ALLOMANCY", title = "Allomancy Basics", content = "Burn metals to gain supernatural abilities. Use scroll wheel to select metals." });
        helpEntries.Add(new HelpEntry { id = "STEEL_PUSH", title = "Steel Push", content = "Push metal objects away from you. Light objects fly away, heavy objects push YOU back." });
        helpEntries.Add(new HelpEntry { id = "IRON_PULL", title = "Iron Pull", content = "Pull metal objects toward you. Light objects come to you, heavy objects pull YOU forward." });
        helpEntries.Add(new HelpEntry { id = "PEWTER", title = "Pewter", content = "Enhances strength, speed, and healing. Drains faster in combat." });
        helpEntries.Add(new HelpEntry { id = "TIN", title = "Tin", content = "Enhances all five senses. Better in low light, but risks sensory overload." });
        helpEntries.Add(new HelpEntry { id = "ZINC", title = "Zinc (Riot)", content = "Intensifies emotions in others. Makes enemies more aggressive." });
        helpEntries.Add(new HelpEntry { id = "BRASS", title = "Brass (Soothe)", content = "Calms emotions in others. Can calm enemies or NPCs." });
        helpEntries.Add(new HelpEntry { id = "COPPER", title = "Copper (Cloud)", content = "Hides your Allomantic pulses from Seekers." });
        helpEntries.Add(new HelpEntry { id = "BRONZE", title = "Bronze (Seek)", content = "Detects Allomantic pulses from other Allomancers." });
        helpEntries.Add(new HelpEntry { id = "ATIUM", title = "Atium", content = "See the immediate future. Shows ghost images of enemy movements." });
        helpEntries.Add(new HelpEntry { id = "ELECTRUM", title = "Electrum", content = "See your possible futures. Useful when Atium is unavailable." });
        helpEntries.Add(new HelpEntry { id = "DURALUMIN", title = "Duralumin", content = "Amplifies your next metal burn massively. Uses all reserves at once." });
        helpEntries.Add(new HelpEntry { id = "BENDALLOY", title = "Bendalloy", content = "Creates a time dilation bubble - speeds up time inside." });
        helpEntries.Add(new HelpEntry { id = "CADMIUM", title = "Cadmium", content = "Creates a time dilation bubble - slows down time inside." });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleHelp();
        }
    }

    public void ShowHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(true);
        DisplayEntries(helpEntries);
    }

    public void CloseHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
    }

    public void ToggleHelp()
    {
        if (helpPanel != null)
        {
            helpPanel.SetActive(!helpPanel.activeSelf);
            if (helpPanel.activeSelf) DisplayEntries(helpEntries);
        }
    }

    void DisplayEntries(List<HelpEntry> entries)
    {
        ClearEntries();

        foreach (var entry in entries)
        {
            CreateEntryUI(entry);
        }
    }

    void ClearEntries()
    {
        foreach (var ui in entryUIs)
        {
            if (ui != null) Destroy(ui.gameObject);
        }
        entryUIs.Clear();
    }

    void CreateEntryUI(HelpEntry entry)
    {
        if (helpEntryPrefab == null || helpContent == null) return;

        GameObject obj = Instantiate(helpEntryPrefab, helpContent);
        HelpEntryUI ui = obj.GetComponent<HelpEntryUI>();

        if (ui != null)
        {
            ui.Initialize(entry);
            entryUIs.Add(ui);
        }
    }

    public void Search(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            DisplayEntries(helpEntries);
            return;
        }

        List<HelpEntry> filtered = helpEntries.FindAll(e =>
            e.title.ToLower().Contains(query.ToLower()) ||
            e.content.ToLower().Contains(query.ToLower())
        );

        DisplayEntries(filtered);
    }

    public void ShowEntry(string entryId)
    {
        HelpEntry entry = helpEntries.Find(e => e.id == entryId);
        if (entry != null)
        {
            ShowHelp();
            // Scroll to or highlight the entry
        }
    }
}

[System.Serializable]
public class HelpEntry
{
    public string id;
    public string title;
    [TextArea] public string content;
}

public class HelpEntryUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public Button expandButton;
    public GameObject contentPanel;

    private bool isExpanded = false;

    public void Initialize(HelpEntry entry)
    {
        if (titleText != null) titleText.text = entry.title;
        if (contentText != null) contentText.text = entry.content;

        if (expandButton != null) expandButton.onClick.AddListener(ToggleExpanded);

        if (contentPanel != null) contentPanel.SetActive(false);
    }

    void ToggleExpanded()
    {
        isExpanded = !isExpanded;
        if (contentPanel != null) contentPanel.SetActive(isExpanded);
    }
}

public class HintManager : MonoBehaviour
{
    [Header("Settings")]
    public float showDelay = 5f;
    public float displayDuration = 3f;
    public int maxHintsPerSession = 10;

    [Header("Hints")]
    public List<GameHint> hints = new List<GameHint>();
    public Dictionary<string, bool> shownHints = new Dictionary<string, bool>();

    private int hintsShown = 0;

    void Start()
    {
        PopulateHints();
    }

    void PopulateHints()
    {
        hints.Add(new GameHint { id = "STEEL_PUSH_HINT", condition = "FIRST_METAL", message = "Press Q to push metal objects away from you!" });
        hints.Add(new GameHint { id = "IRON_PULL_HINT", condition = "FIRST_METAL", message = "Press E to pull metal objects toward you!" });
        hints.Add(new GameHint { id = "FLARE_HINT", condition = "FIRST_BURN", message = "Hold Left Ctrl to flare your metal for more power!" });
        hints.Add(new GameHint { id = "SCROLL_HINT", condition = "FIRST_BURN", message = "Use scroll wheel to change metals!" });
    }

    public void ShowHint(string hintId)
    {
        if (hintsShown >= maxHintsPerSession) return;
        if (shownHints.ContainsKey(hintId) && shownHints[hintId]) return;

        GameHint hint = hints.Find(h => h.id == hintId);
        if (hint == null) return;

        shownHints[hintId] = true;
        hintsShown++;

        TutorialSystem.Instance?.ShowHint(hint.message);

        Debug.Log($"[HINT] Shown: {hintId}");
    }

    public void CheckCondition(string condition)
    {
        List<GameHint> matchingHints = hints.FindAll(h => h.condition == condition);
        foreach (var hint in matchingHints)
        {
            ShowHint(hint.id);
        }
    }
}

[System.Serializable]
public class GameHint
{
    public string id;
    public string condition;
    public string message;
}