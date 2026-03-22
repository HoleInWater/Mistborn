using UnityEngine;
using System.Collections.Generic;

namespace MistbornGameplay
{
    public class DialogueSystem : MonoBehaviour
    {
        [Header("Dialogue Settings")]
        [SerializeField] private TextAsset[] dialogueFiles;
        [SerializeField] private bool useProximityTrigger = true;
        [SerializeField] private float triggerDistance = 3f;
        
        [Header("UI References")]
        [SerializeField] private GameObject dialogueUI;
        [SerializeField] private UnityEngine.UI.Text speakerNameText;
        [SerializeField] private UnityEngine.UI.Text dialogueText;
        [SerializeField] private UnityEngine.UI.Button[] choiceButtons;
        [SerializeField] private GameObject continuePrompt;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] dialogueSounds;
        [SerializeField] private float typeSpeed = 0.05f;
        
        private bool isDialogueActive = false;
        private DialogueNode currentNode;
        private List<DialogueNode> dialogueTree = new List<DialogueNode>();
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private string fullText = "";
        private AudioSource audioSource;
        private Transform playerTransform;
        
        [System.Serializable]
        public class DialogueNode
        {
            public string speakerName;
            public string dialogueText;
            public List<DialogueChoice> choices;
            public bool isEndNode = false;
            public string nextNodeID;
            public string requiredQuestID;
            public string givesQuestID;
            public int reputationChange = 0;
        }
        
        [System.Serializable]
        public class DialogueChoice
        {
            public string choiceText;
            public string nextNodeID;
            public string requiredItemID;
            public string grantsItemID;
            public string startsQuestID;
            public int reputationChange = 0;
        }
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }
            
            LoadDialogues();
        }
        
        private void Update()
        {
            if (useProximityTrigger && !isDialogueActive)
            {
                CheckProximity();
            }
            
            if (isDialogueActive)
            {
                HandleDialogueInput();
            }
        }
        
        private void CheckProximity()
        {
            if (playerTransform == null)
            {
                return;
            }
            
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= triggerDistance)
            {
                StartDialogue();
            }
        }
        
        private void LoadDialogues()
        {
            if (dialogueFiles == null || dialogueFiles.Length == 0)
            {
                return;
            }
            
            foreach (TextAsset file in dialogueFiles)
            {
                ParseDialogueFile(file);
            }
        }
        
        private void ParseDialogueFile(TextAsset file)
        {
        }
        
        public void StartDialogue()
        {
            if (isDialogueActive)
            {
                return;
            }
            
            if (dialogueTree.Count == 0)
            {
                return;
            }
            
            isDialogueActive = true;
            currentNode = dialogueTree[0];
            currentLineIndex = 0;
            
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(true);
            }
            
            DisplayCurrentLine();
            
            if (OnDialogueStarted != null)
            {
                OnDialogueStarted();
            }
        }
        
        public void StartDialogue(string startNodeID)
        {
            foreach (DialogueNode node in dialogueTree)
            {
                if (node.speakerName == startNodeID)
                {
                    currentNode = node;
                    currentLineIndex = 0;
                    
                    isDialogueActive = true;
                    
                    if (dialogueUI != null)
                    {
                        dialogueUI.SetActive(true);
                    }
                    
                    DisplayCurrentLine();
                    
                    if (OnDialogueStarted != null)
                    {
                        OnDialogueStarted();
                    }
                    
                    return;
                }
            }
        }
        
        private void DisplayCurrentLine()
        {
            if (currentNode == null)
            {
                EndDialogue();
                return;
            }
            
            if (speakerNameText != null)
            {
                speakerNameText.text = currentNode.speakerName;
            }
            
            fullText = currentNode.dialogueText;
            dialogueText.text = "";
            isTyping = true;
            
            StartCoroutine(TypeText());
        }
        
        private System.Collections.IEnumerator TypeText()
        {
            isTyping = true;
            
            foreach (char letter in fullText)
            {
                dialogueText.text += letter;
                
                if (letter != ' ')
                {
                    PlayDialogueSound();
                }
                
                yield return new WaitForSeconds(typeSpeed);
            }
            
            isTyping = false;
            
            if (continuePrompt != null)
            {
                continuePrompt.SetActive(true);
            }
        }
        
        private void PlayDialogueSound()
        {
            if (dialogueSounds != null && dialogueSounds.Length > 0 && audioSource != null)
            {
                AudioClip clip = dialogueSounds[Random.Range(0, dialogueSounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void HandleDialogueInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    SkipTyping();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
        }
        
        private void SkipTyping()
        {
            StopAllCoroutines();
            dialogueText.text = fullText;
            isTyping = false;
            
            if (continuePrompt != null)
            {
                continuePrompt.SetActive(true);
            }
        }
        
        private void AdvanceDialogue()
        {
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                ShowChoices();
            }
            else if (currentNode.isEndNode)
            {
                EndDialogue();
            }
            else
            {
                AdvanceToNextNode();
            }
        }
        
        private void ShowChoices()
        {
            if (continuePrompt != null)
            {
                continuePrompt.SetActive(false);
            }
            
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < currentNode.choices.Count)
                {
                    DialogueChoice choice = currentNode.choices[i];
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].GetComponentInChildren<UnityEngine.UI.Text>().text = choice.choiceText;
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
        
        private void SelectChoice(int choiceIndex)
        {
            if (choiceIndex >= currentNode.choices.Count)
            {
                return;
            }
            
            DialogueChoice choice = currentNode.choices[choiceIndex];
            
            HideChoices();
            
            ProcessChoice(choice);
        }
        
        private void ProcessChoice(DialogueChoice choice)
        {
            if (choice.grantsItemID != null)
            {
                GrantItem(choice.grantsItemID);
            }
            
            if (choice.startsQuestID != null)
            {
                StartQuest(choice.startsQuestID);
            }
            
            if (choice.reputationChange != 0)
            {
                ChangeReputation(choice.reputationChange);
            }
            
            if (!string.IsNullOrEmpty(choice.nextNodeID))
            {
                DialogueNode nextNode = FindNode(choice.nextNodeID);
                if (nextNode != null)
                {
                    currentNode = nextNode;
                    DisplayCurrentLine();
                }
                else
                {
                    EndDialogue();
                }
            }
            else
            {
                EndDialogue();
            }
        }
        
        private void AdvanceToNextNode()
        {
            if (continuePrompt != null)
            {
                continuePrompt.SetActive(false);
            }
            
            if (!string.IsNullOrEmpty(currentNode.nextNodeID))
            {
                DialogueNode nextNode = FindNode(currentNode.nextNodeID);
                if (nextNode != null)
                {
                    currentNode = nextNode;
                    DisplayCurrentLine();
                }
                else
                {
                    EndDialogue();
                }
            }
            else
            {
                EndDialogue();
            }
        }
        
        private DialogueNode FindNode(string nodeID)
        {
            foreach (DialogueNode node in dialogueTree)
            {
                if (node.speakerName == nodeID)
                {
                    return node;
                }
            }
            
            return null;
        }
        
        private void HideChoices()
        {
            foreach (UnityEngine.UI.Button button in choiceButtons)
            {
                button.gameObject.SetActive(false);
            }
        }
        
        public void EndDialogue()
        {
            isDialogueActive = false;
            currentNode = null;
            HideChoices();
            
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }
            
            if (OnDialogueEnded != null)
            {
                OnDialogueEnded();
            }
        }
        
        private void GrantItem(string itemID)
        {
            InventorySystem inventory = GetComponent<InventorySystem>();
            if (inventory != null)
            {
                inventory.AddItem(itemID, 1);
            }
        }
        
        private void StartQuest(string questID)
        {
            QuestJournal journal = GetComponent<QuestJournal>();
            if (journal != null)
            {
            }
        }
        
        private void ChangeReputation(int amount)
        {
            FactionManager faction = GetComponent<FactionManager>();
            if (faction != null)
            {
                faction.ModifyReputation(amount);
            }
        }
        
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }
        
        public void AddDialogueNode(DialogueNode node)
        {
            dialogueTree.Add(node);
        }
        
        public void ClearDialogues()
        {
            dialogueTree.Clear();
        }
        
        public event System.Action OnDialogueStarted;
        public event System.Action OnDialogueEnded;
    }
}
