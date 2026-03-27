using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the display and flow of dialogue in the UI.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;

    private Queue<string> sentenceQueue = new Queue<string>();
    private bool isTyping = false;
    private string currentLine = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData data)
    {
        if (dialoguePanel == null) return;

        dialoguePanel.SetActive(true);
        nameText.text = data.speakerName;

        sentenceQueue.Clear();
        foreach (string line in data.lines)
        {
            sentenceQueue.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            // Finish line instantly
            StopAllCoroutines();
            dialogueText.text = currentLine;
            isTyping = false;
            return;
        }

        if (sentenceQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = sentenceQueue.Dequeue();
        StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // Advance on Space or Left Click if panel is active
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            DisplayNextSentence();
        }
    }
}
