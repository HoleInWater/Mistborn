using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerName;
    public TextMeshProUGUI dialogueText;
    public Image speakerPortrait;
    public Button continueButton;
    public GameObject nameplatePanel;
    
    [Header("Options")]
    public GameObject optionsPanel;
    public Button[] optionButtons;
    
    [Header("Settings")]
    public float textSpeed = 0.05f;
    public bool skipOnClick = true;
    public AudioClip textSound;
    public bool showNameplate = true;
    
    [Header("Current Dialogue")]
    private DialogueNode currentNode;
    private bool isTyping = false;
    private string fullText = "";
    private int currentCharIndex = 0;
    
    private static DialogueSystem instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }
    
    void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (isTyping && skipOnClick)
            {
                CompleteText();
            }
            else
            {
                OnContinueClicked();
            }
        }
    }
    
    public void StartDialogue(DialogueNode startNode)
    {
        if (startNode == null) return;
        
        currentNode = startNode;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        ShowNode(currentNode);
    }
    
    void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }
        
        if (speakerName != null)
        {
            speakerName.text = node.speakerName;
        }
        
        if (speakerPortrait != null && node.portrait != null)
        {
            speakerPortrait.sprite = node.portrait;
            speakerPortrait.enabled = true;
        }
        
        if (nameplatePanel != null)
        {
            nameplatePanel.SetActive(showNameplate && !string.IsNullOrEmpty(node.speakerName));
        }
        
        if (node.dialogueOptions != null && node.dialogueOptions.Length > 0)
        {
            ShowOptions(node.dialogueOptions);
        }
        else
        {
            HideOptions();
            StartTypingEffect(node.dialogueText);
        }
    }
    
    void StartTypingEffect(string text)
    {
        fullText = text;
        currentCharIndex = 0;
        isTyping = true;
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        
        StopAllCoroutines();
        StartCoroutine(TypeText());
    }
    
    System.Collections.IEnumerator TypeText()
    {
        while (currentCharIndex < fullText.Length)
        {
            currentCharIndex++;
            dialogueText.text = fullText.Substring(0, currentCharIndex);
            
            if (textSound != null && currentCharIndex % 3 == 0)
            {
                AudioSource.PlayClipAtPoint(textSound, Vector3.zero);
            }
            
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
    }
    
    void CompleteText()
    {
        StopAllCoroutines();
        currentCharIndex = fullText.Length;
        dialogueText.text = fullText;
        isTyping = false;
    }
    
    void OnContinueClicked()
    {
        if (isTyping) return;
        
        if (currentNode.dialogueOptions != null && currentNode.dialogueOptions.Length > 0)
        {
            return;
        }
        
        AdvanceDialogue();
    }
    
    void AdvanceDialogue()
    {
        if (currentNode.nextNode != null)
        {
            ShowNode(currentNode.nextNode);
        }
        else
        {
            EndDialogue();
        }
    }
    
    public void SelectOption(int optionIndex)
    {
        if (currentNode == null || currentNode.dialogueOptions == null)
            return;
        
        if (optionIndex >= currentNode.dialogueOptions.Length)
            return;
        
        DialogueOption option = currentNode.dialogueOptions[optionIndex];
        
        if (option.requiredQuest != null)
        {
            QuestManager questManager = FindObjectOfType<QuestManager>();
            Quest quest = questManager?.GetQuest(option.requiredQuest.questId);
            
            if (quest == null || !quest.isComplete)
            {
                Debug.Log($"Quest requirement not met: {option.requiredQuest.questId}");
                return;
            }
        }
        
        if (option.grantQuest != null)
        {
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager?.AcceptQuest(option.grantQuest);
        }
        
        if (option.setFlag != null)
        {
            PlayerPrefs.SetInt(option.setFlag, 1);
        }
        
        if (option.nextNode != null)
        {
            HideOptions();
            ShowNode(option.nextNode);
        }
        else
        {
            EndDialogue();
        }
    }
    
    void ShowOptions(DialogueOption[] options)
    {
        if (optionsPanel == null) return;
        
        optionsPanel.SetActive(true);
        
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                TextMeshProUGUI buttonText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = options[i].optionText;
                }
                
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    void HideOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }
    
    public void EndDialogue()
    {
        currentNode = null;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        HideOptions();
    }
    
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}

[System.Serializable]
public class DialogueNode
{
    public string speakerName;
    public Sprite portrait;
    [TextArea(2, 5)]
    public string dialogueText;
    public DialogueOption[] dialogueOptions;
    public DialogueNode nextNode;
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public DialogueNode nextNode;
    public Quest requiredQuest;
    public Quest grantQuest;
    public string setFlag;
}

public class NPCDialogue : MonoBehaviour
{
    public DialogueNode[] dialogueNodes;
    public bool interactOnCollision = false;
    public bool showInteractionPrompt = true;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("Interaction UI")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    private bool playerInRange = false;
    private int currentNodeIndex = 0;
    
    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (promptText != null)
        {
            promptText.text = $"Press {interactKey} to talk";
        }
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            StartDialogue();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!interactOnCollision) return;
        
        if (other.CompareTag("Player"))
        {
            StartDialogue();
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            if (interactionPrompt != null && !DialogueSystem.instance.IsDialogueActive())
            {
                interactionPrompt.SetActive(showInteractionPrompt);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    void StartDialogue()
    {
        if (dialogueNodes == null || dialogueNodes.Length == 0) return;
        
        DialogueSystem dialogue = DialogueSystem.instance;
        if (dialogue != null)
        {
            dialogue.StartDialogue(dialogueNodes[currentNodeIndex]);
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    public void SetDialogueNode(int index)
    {
        if (index >= 0 && index < dialogueNodes.Length)
        {
            currentNodeIndex = index;
        }
    }
}
