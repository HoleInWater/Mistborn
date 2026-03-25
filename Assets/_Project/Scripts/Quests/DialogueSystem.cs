using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class DialogueNode
{
    public string nodeId;
    [TextArea] public string text;
    public string speakerName;
    public Sprite speakerPortrait;

    [Header("Responses")]
    public List<DialogueResponse> responses;

    [Header("Events")]
    public string triggerEvent;
    public string setFlag;
    public bool requiresFlag;

    [Header("Next")]
    public string nextNodeId;
    public bool isEndNode;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseText;
    public string nextNodeId;
    public string requiredFlag;
    public string setFlagOnSelect;
    public string questToAdd;
    public string conditionScript;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/New Dialogue")]
public class Dialogue : ScriptableObject
{
    public string dialogueId;
    public string startNodeId;
    public List<DialogueNode> nodes;

    public DialogueNode GetNode(string nodeId)
    {
        return nodes.FirstOrDefault(n => n.nodeId == nodeId);
    }
}

public class BranchingDialogueManager : MonoBehaviour
{
    public static BranchingDialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public UnityEngine.UI.Text speakerNameText;
    public UnityEngine.UI.Text dialogueText;
    public UnityEngine.UI.Image speakerPortrait;
    public Transform responseContainer;
    public GameObject responseButtonPrefab;

    [Header("State")]
    public Dialogue currentDialogue;
    public DialogueNode currentNode;
    public bool isDialogueActive = false;
    public string conversationPartner;

    private Dictionary<string, Dialogue> loadedDialogues = new Dictionary<string, Dialogue>();
    private Dictionary<string, bool> dialogueFlags = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    public void StartDialogue(Dialogue dialogue, string partner)
    {
        if (dialogue == null) return;

        currentDialogue = dialogue;
        conversationPartner = partner;
        isDialogueActive = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        ShowNode(dialogue.startNodeId);
    }

    public void ShowNode(string nodeId)
    {
        currentNode = currentDialogue.GetNode(nodeId);
        if (currentNode == null) return;

        if (!string.IsNullOrEmpty(currentNode.setFlag))
        {
            dialogueFlags[currentNode.setFlag] = true;
        }

        if (!string.IsNullOrEmpty(currentNode.triggerEvent))
        {
            EventManager.TriggerEvent(currentNode.triggerEvent);
        }

        UpdateDialogueUI();

        if (currentNode.isEndNode)
        {
            Invoke("EndDialogue", 1f);
        }
    }

    void UpdateDialogueUI()
    {
        if (speakerNameText != null && currentNode != null)
            speakerNameText.text = currentNode.speakerName;

        if (dialogueText != null && currentNode != null)
            dialogueText.text = currentNode.text;

        if (speakerPortrait != null && currentNode != null)
            speakerPortrait.sprite = currentNode.speakerPortrait;

        ClearResponses();

        if (currentNode.responses != null && currentNode.responses.Count > 0)
        {
            foreach (var response in currentNode.responses)
            {
                if (IsResponseAvailable(response))
                {
                    CreateResponseButton(response);
                }
            }
        }
        else if (!currentNode.isEndNode && !string.IsNullOrEmpty(currentNode.nextNodeId))
        {
            CreateContinueButton();
        }
    }

    bool IsResponseAvailable(DialogueResponse response)
    {
        if (!string.IsNullOrEmpty(response.requiredFlag))
        {
            if (!dialogueFlags.ContainsKey(response.requiredFlag) || !dialogueFlags[response.requiredFlag])
                return false;
        }
        return true;
    }

    void CreateResponseButton(DialogueResponse response)
    {
        if (responseButtonPrefab == null || responseContainer == null) return;

        GameObject btn = Instantiate(responseButtonPrefab, responseContainer);
        UnityEngine.UI.Text btnText = btn.GetComponentInChildren<UnityEngine.UI.Text>();
        if (btnText != null) btnText.text = response.responseText;

        btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnResponseSelected(response));
    }

    void CreateContinueButton()
    {
        if (responseButtonPrefab == null || responseContainer == null) return;

        GameObject btn = Instantiate(responseButtonPrefab, responseContainer);
        UnityEngine.UI.Text btnText = btn.GetComponentInChildren<UnityEngine.UI.Text>();
        if (btnText != null) btnText.text = "Continue...";

        btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ShowNode(currentNode.nextNodeId));
    }

    void OnResponseSelected(DialogueResponse response)
    {
        if (!string.IsNullOrEmpty(response.setFlagOnSelect))
        {
            dialogueFlags[response.setFlagOnSelect] = true;
        }

        if (!string.IsNullOrEmpty(response.questToAdd))
        {
            QuestManager.Instance.AddQuestById(response.questToAdd);
        }

        ShowNode(response.nextNodeId);
    }

    void ClearResponses()
    {
        if (responseContainer == null) return;

        foreach (Transform child in responseContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        currentDialogue = null;
        currentNode = null;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public bool HasFlag(string flagName)
    {
        return dialogueFlags.ContainsKey(flagName) && dialogueFlags[flagName];
    }

    public void SetFlag(string flagName, bool value)
    {
        dialogueFlags[flagName] = value;
    }

    public void LoadDialogue(Dialogue dialogue)
    {
        if (dialogue != null)
        {
            loadedDialogues[dialogue.dialogueId] = dialogue;
        }
    }
}