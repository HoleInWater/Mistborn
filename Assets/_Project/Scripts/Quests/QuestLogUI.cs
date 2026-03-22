using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestLogUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questLogPanel;
    public Transform questListContent;
    public GameObject questEntryPrefab;
    
    [Header("Quest Details")]
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public Transform objectiveListContent;
    public GameObject objectiveEntryPrefab;
    
    [Header("Buttons")]
    public Button closeButton;
    public Button abandonButton;
    
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.J;
    
    private Quest selectedQuest;
    private QuestManager questManager;
    
    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseQuestLog);
        }
        
        if (abandonButton != null)
        {
            abandonButton.onClick.AddListener(AbandonSelectedQuest);
        }
        
        CloseQuestLog();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleQuestLog();
        }
    }
    
    public void ToggleQuestLog()
    {
        if (questLogPanel.activeSelf)
        {
            CloseQuestLog();
        }
        else
        {
            OpenQuestLog();
        }
    }
    
    public void OpenQuestLog()
    {
        questLogPanel.SetActive(true);
        RefreshQuestList();
        Time.timeScale = 0f;
    }
    
    public void CloseQuestLog()
    {
        questLogPanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    void RefreshQuestList()
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        
        if (questManager == null) return;
        
        foreach (Quest quest in questManager.GetActiveQuests())
        {
            CreateQuestEntry(quest);
        }
    }
    
    void CreateQuestEntry(Quest quest)
    {
        if (questEntryPrefab == null || questListContent == null) return;
        
        GameObject entryObj = Instantiate(questEntryPrefab, questListContent);
        QuestEntryUI entry = entryObj.GetComponent<QuestEntryUI>();
        
        if (entry != null)
        {
            entry.Initialize(quest, this);
        }
    }
    
    public void SelectQuest(Quest quest)
    {
        selectedQuest = quest;
        DisplayQuestDetails(quest);
    }
    
    void DisplayQuestDetails(Quest quest)
    {
        if (questTitle != null)
        {
            questTitle.text = quest.title;
        }
        
        if (questDescription != null)
        {
            questDescription.text = quest.description;
        }
        
        RefreshObjectiveList(quest);
    }
    
    void RefreshObjectiveList(Quest quest)
    {
        foreach (Transform child in objectiveListContent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (QuestObjective objective in quest.objectives)
        {
            CreateObjectiveEntry(objective);
        }
    }
    
    void CreateObjectiveEntry(QuestObjective objective)
    {
        if (objectiveEntryPrefab == null || objectiveListContent == null) return;
        
        GameObject entryObj = Instantiate(objectiveEntryPrefab, objectiveListContent);
        ObjectiveEntryUI entry = entryObj.GetComponent<ObjectiveEntryUI>();
        
        if (entry != null)
        {
            entry.Initialize(objective);
        }
    }
    
    void AbandonSelectedQuest()
    {
        if (selectedQuest != null && questManager != null)
        {
            questManager.AbandonQuest(selectedQuest);
            selectedQuest = null;
            ClearQuestDetails();
            RefreshQuestList();
        }
    }
    
    void ClearQuestDetails()
    {
        if (questTitle != null) questTitle.text = "";
        if (questDescription != null) questDescription.text = "";
        
        foreach (Transform child in objectiveListContent)
        {
            Destroy(child.gameObject);
        }
    }
}

public class QuestEntryUI : MonoBehaviour
{
    public TextMeshProUGUI questName;
    public TextMeshProUGUI questProgress;
    public Image completionIndicator;
    
    private Quest quest;
    private QuestLogUI questLogUI;
    
    public void Initialize(Quest questData, QuestLogUI ui)
    {
        quest = questData;
        questLogUI = ui;
        
        if (questName != null)
        {
            questName.text = quest.title;
        }
        
        UpdateProgress();
        
        GetComponent<Button>()?.onClick.AddListener(OnClick);
    }
    
    public void UpdateProgress()
    {
        int completed = 0;
        int total = quest.objectives.Length;
        
        foreach (QuestObjective obj in quest.objectives)
        {
            if (obj.isCompleted) completed++;
        }
        
        if (questProgress != null)
        {
            questProgress.text = $"{completed}/{total}";
        }
        
        if (completionIndicator != null)
        {
            completionIndicator.fillAmount = (float)completed / total;
        }
    }
    
    void OnClick()
    {
        if (questLogUI != null)
        {
            questLogUI.SelectQuest(quest);
        }
    }
}

public class ObjectiveEntryUI : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public Image completionCheck;
    
    public void Initialize(QuestObjective objective)
    {
        string status = objective.isCompleted ? "[X]" : "[ ]";
        string progress = objective.requiredCount > 1 ? $" ({objective.currentProgress}/{objective.requiredCount})" : "";
        
        if (objectiveText != null)
        {
            objectiveText.text = $"{status} {objective.targetName}{progress}";
        }
        
        if (completionCheck != null)
        {
            completionCheck.enabled = objective.isCompleted;
        }
    }
}
