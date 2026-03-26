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
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData data)
    {
<<<<<<< HEAD
        if (dialoguePanel == null) return;

        dialoguePanel.SetActive(true);
        nameText.text = data.speakerName;

        sentenceQueue.Clear();
        foreach (string line in data.lines)
        {
            sentenceQueue.Enqueue(line);
=======
        if (dialoguePanel == null || data == null) return;

        dialoguePanel.SetActive(true);
        if (nameText != null) nameText.text = data.speakerName ?? "";

        sentenceQueue.Clear();
        if (data.lines != null)
        {
            foreach (string line in data.lines)
                sentenceQueue.Enqueue(line);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
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
<<<<<<< HEAD
            yield return new WaitForSeconds(typingSpeed);
=======
            yield return new WaitForSecondsRealtime(typingSpeed);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        }

        isTyping = false;
    }

    void EndDialogue()
    {
<<<<<<< HEAD
        dialoguePanel.SetActive(false);
=======
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    }

    void Update()
    {
<<<<<<< HEAD
        // Advance on Space or Left Click if panel is active
        if (dialoguePanel.activeInHierarchy && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
=======
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy &&
            (Input.GetKeyDown(Keybinds.Jump) || Input.GetMouseButtonDown(0)))
>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
        {
            DisplayNextSentence();
        }
    }
}
