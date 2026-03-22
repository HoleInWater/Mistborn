using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("Dialogue System")]
        [SerializeField] private bool isDialogueActive = false;
        [SerializeField] private Dialogue currentDialogue;
        [SerializeField] private int currentLineIndex = 0;
        
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private UnityEngine.UI.Text speakerNameText;
        [SerializeField] private UnityEngine.UI.Text dialogueText;
        [SerializeField] private UnityEngine.UI.Text continuePrompt;
        
        [Header("Choices")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private UnityEngine.UI.Button[] choiceButtons;
        
        [Header("Settings")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool skipTextOnContinue = true;
        
        private Queue<string> textQueue = new Queue<string>();
        private bool isTyping = false;
        private string currentFullText = "";
        
        public event System.Action OnDialogueStarted;
        public event System.Action OnDialogueEnded;
        public event System.Action<int> OnDialogueLineChanged;
        
        public static DialogueManager instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        
        private void Start()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!isDialogueActive)
                return;
            
            HandleDialogueInput();
        }
        
        private void HandleDialogueInput()
        {
            if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    CompleteCurrentLine();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
        }
        
        public void StartDialogue(Dialogue dialogue)
        {
            if (dialogue == null)
                return;
            
            currentDialogue = dialogue;
            currentLineIndex = 0;
            isDialogueActive = true;
            
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
            
            ShowCurrentLine();
            
            Time.timeScale = 0f;
            
            OnDialogueStarted?.Invoke();
        }
        
        public void EndDialogue()
        {
            isDialogueActive = false;
            currentDialogue = null;
            currentLineIndex = 0;
            
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
            
            Time.timeScale = 1f;
            
            OnDialogueEnded?.Invoke();
        }
        
        private void ShowCurrentLine()
        {
            if (currentDialogue == null)
                return;
            
            DialogueLine[] lines = currentDialogue.GetLines();
            if (lines == null || currentLineIndex >= lines.Length)
            {
                EndDialogue();
                return;
            }
            
            DialogueLine line = lines[currentLineIndex];
            
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName;
            }
            
            if (line.hasChoices && line.choices != null && line.choices.Length > 0)
            {
                ShowChoices(line.choices);
            }
            else
            {
                HideChoices();
                TypeText(line.dialogueText);
            }
            
            OnDialogueLineChanged?.Invoke(currentLineIndex);
        }
        
        private void TypeText(string text)
        {
            currentFullText = text;
            textQueue.Clear();
            
            foreach (char letter in text.ToCharArray())
            {
                textQueue.Enqueue(letter.ToString());
            }
            
            isTyping = true;
            
            StartCoroutine(TypeTextCoroutine());
        }
        
        private System.Collections.IEnumerator TypeTextCoroutine()
        {
            if (dialogueText != null)
            {
                dialogueText.text = "";
            }
            
            while (textQueue.Count > 0 && isTyping)
            {
                string letter = textQueue.Dequeue();
                dialogueText.text += letter;
                
                yield return new WaitForSecondsRealtime(textSpeed);
            }
            
            isTyping = false;
            dialogueText.text = currentFullText;
        }
        
        private void CompleteCurrentLine()
        {
            isTyping = false;
            StopAllCoroutines();
            
            if (dialogueText != null)
            {
                dialogueText.text = currentFullText;
            }
        }
        
        private void AdvanceDialogue()
        {
            DialogueLine[] lines = currentDialogue.GetLines();
            if (lines == null)
                return;
            
            DialogueLine line = lines[currentLineIndex];
            
            if (line.hasChoices)
            {
                return;
            }
            
            currentLineIndex++;
            
            if (currentLineIndex >= lines.Length)
            {
                if (!string.IsNullOrEmpty(currentDialogue.nextDialogueId))
                {
                    LoadNextDialogue();
                }
                else
                {
                    EndDialogue();
                }
            }
            else
            {
                ShowCurrentLine();
            }
        }
        
        private void ShowChoices(DialogueChoice[] choices)
        {
            if (choicePanel == null || choiceButtons == null)
                return;
            
            choicePanel.SetActive(true);
            
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].GetComponentInChildren<UnityEngine.UI.Text>().text = choices[i].choiceText;
                    int choiceIndex = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => SelectChoice(choiceIndex));
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
        private void HideChoices()
        {
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
        }
        
        private void SelectChoice(int choiceIndex)
        {
            if (currentDialogue == null)
                return;
            
            DialogueLine[] lines = currentDialogue.GetLines();
            if (lines == null || currentLineIndex >= lines.Length)
                return;
            
            DialogueLine line = lines[currentLineIndex];
            if (line.choices == null || choiceIndex >= line.choices.Length)
                return;
            
            DialogueChoice choice = line.choices[choiceIndex];
            
            if (!string.IsNullOrEmpty(choice.requiredQuestId))
            {
                QuestManager questManager = FindObjectOfType<QuestManager>();
                if (questManager != null && !questManager.IsQuestActive(choice.requiredQuestId))
                {
                    return;
                }
            }
            
            HideChoices();
            
            if (!string.IsNullOrEmpty(choice.nextDialogueId))
            {
                LoadDialogue(choice.nextDialogueId);
            }
            else
            {
                currentLineIndex++;
                if (currentLineIndex < lines.Length)
                {
                    ShowCurrentLine();
                }
                else
                {
                    EndDialogue();
                }
            }
            
            if (!string.IsNullOrEmpty(choice.rewardQuestId))
            {
                QuestManager questManager = FindObjectOfType<QuestManager>();
                questManager?.AcceptQuest(questManager.GetQuest(choice.rewardQuestId));
            }
        }
        
        private void LoadNextDialogue()
        {
            if (string.IsNullOrEmpty(currentDialogue.nextDialogueId))
                return;
            
            LoadDialogue(currentDialogue.nextDialogueId);
        }
        
        private void LoadDialogue(string dialogueId)
        {
            DialogueDatabase database = FindObjectOfType<DialogueDatabase>();
            if (database != null)
            {
                Dialogue dialogue = database.GetDialogue(dialogueId);
                if (dialogue != null)
                {
                    currentDialogue = dialogue;
                    currentLineIndex = 0;
                    ShowCurrentLine();
                }
            }
        }
        
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }
        
        public Dialogue GetCurrentDialogue()
        {
            return currentDialogue;
        }
        
        public int GetCurrentLineIndex()
        {
            return currentLineIndex;
        }
        
        public void SkipDialogue()
        {
            if (isDialogueActive)
            {
                EndDialogue();
            }
        }
    }
    
    [System.Serializable]
    public class Dialogue
    {
        public string dialogueId;
        public string speakerName;
        public DialogueLine[] lines;
        public string nextDialogueId;
        
        public DialogueLine[] GetLines()
        {
            return lines;
        }
    }
    
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea] public string dialogueText;
        public bool hasChoices = false;
        public DialogueChoice[] choices;
        public bool hasCondition = false;
        public string conditionQuestId;
    }
    
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextDialogueId;
        public string requiredQuestId;
        public string rewardQuestId;
        public string[] requiredItems;
    }
    
    public class DialogueDatabase : MonoBehaviour
    {
        [SerializeField] private Dialogue[] dialogues;
        
        private Dictionary<string, Dialogue> dialogueDatabase = new Dictionary<string, Dialogue>();
        
        private void Start()
        {
            InitializeDatabase();
        }
        
        private void InitializeDatabase()
        {
            if (dialogues == null)
                return;
            
            foreach (var dialogue in dialogues)
            {
                if (dialogue != null && !string.IsNullOrEmpty(dialogue.dialogueId))
                {
                    dialogueDatabase[dialogue.dialogueId] = dialogue;
                }
            }
        }
        
        public Dialogue GetDialogue(string dialogueId)
        {
            if (dialogueDatabase.ContainsKey(dialogueId))
            {
                return dialogueDatabase[dialogueId];
            }
            return null;
        }
        
        public void AddDialogue(Dialogue dialogue)
        {
            if (dialogue != null && !string.IsNullOrEmpty(dialogue.dialogueId))
            {
                dialogueDatabase[dialogue.dialogueId] = dialogue;
            }
        }
    }
}
