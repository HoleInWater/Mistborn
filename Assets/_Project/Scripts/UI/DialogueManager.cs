using UnityEngine;
using System;
using System.Collections.Generic;

namespace Mistborn.UI
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Mistborn/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        [Serializable]
        public class DialogueLine
        {
            [TextArea(2, 4)]
            public string text;
            public float duration = 3f;
        }

        [Serializable]
        public class DialogueEntry
        {
            public string speakerName;
            public Sprite portrait;
            public List<DialogueLine> lines = new List<DialogueLine>();
        }

        [Header("Dialogue Info")]
        public string dialogueId;
        [TextArea(1, 2)]
        public string description;
        public List<DialogueEntry> entries = new List<DialogueEntry>();
        public bool canSkip = true;
        public bool autoAdvance = true;
    }

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject m_dialoguePanel;
        [SerializeField] private UnityEngine.UI.Text m_speakerText;
        [SerializeField] private UnityEngine.UI.Text m_dialogueText;
        [SerializeField] private UnityEngine.UI.Image m_portraitImage;
        [SerializeField] private UnityEngine.UI.Text m_continuePrompt;

        [Header("Settings")]
        [SerializeField] private float m_typeSpeed = 0.05f;
        [SerializeField] private AudioClip m_textBeep;

        private Queue<Dialogue> m_dialogueQueue = new Queue<Dialogue>();
        private Dialogue m_currentDialogue;
        private int m_currentEntryIndex;
        private int m_currentLineIndex;
        private bool m_isTyping;
        private bool m_isPlaying;
        private string m_currentText;
        private float m_lineTimer;
        private AudioSource m_audioSource;

        public event System.Action OnDialogueStart;
        public event System.Action OnDialogueEnd;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            m_audioSource = GetComponent<AudioSource>();
            if (m_dialoguePanel != null)
                m_dialoguePanel.SetActive(false);
        }

        private void Update()
        {
            if (!m_isPlaying) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                HandleInput();
            }

            if (m_isTyping)
            {
                return;
            }

            m_lineTimer -= Time.deltaTime;
            if (m_lineTimer <= 0 && m_currentDialogue.autoAdvance)
            {
                NextLine();
            }
        }

        public void StartDialogue(Dialogue dialogue)
        {
            if (dialogue == null) return;

            m_dialogueQueue.Enqueue(dialogue);
            if (!m_isPlaying)
            {
                PlayNextDialogue();
            }
        }

        private void PlayNextDialogue()
        {
            if (m_dialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            m_currentDialogue = m_dialogueQueue.Dequeue();
            m_currentEntryIndex = 0;
            m_currentLineIndex = 0;
            m_isPlaying = true;

            if (m_dialoguePanel != null)
                m_dialoguePanel.SetActive(true);

            OnDialogueStart?.Invoke();
            ShowCurrentLine();
        }

        private void HandleInput()
        {
            if (m_isTyping)
            {
                CompleteTyping();
            }
            else
            {
                NextLine();
            }
        }

        private void ShowCurrentLine()
        {
            if (m_currentDialogue == null) return;

            if (m_currentEntryIndex >= m_currentDialogue.entries.Count)
            {
                PlayNextDialogue();
                return;
            }

            Dialogue.DialogueEntry entry = m_currentDialogue.entries[m_currentEntryIndex];
            if (m_currentLineIndex >= entry.lines.Count)
            {
                m_currentEntryIndex++;
                m_currentLineIndex = 0;
                ShowCurrentLine();
                return;
            }

            Dialogue.DialogueLine line = entry.lines[m_currentLineIndex];

            if (m_speakerText != null)
                m_speakerText.text = entry.speakerName;

            if (m_portraitImage != null && entry.portrait != null)
                m_portraitImage.sprite = entry.portrait;

            StartTypingEffect(line.text);
            m_lineTimer = line.duration;

            if (m_continuePrompt != null)
                m_continuePrompt.enabled = !m_currentDialogue.autoAdvance;
        }

        private void StartTypingEffect(string text)
        {
            m_currentText = text;
            m_dialogueText.text = "";
            m_isTyping = true;
            StartCoroutine(TypeTextRoutine(text));
        }

        private System.Collections.IEnumerator TypeTextRoutine(string text)
        {
            foreach (char c in text)
            {
                m_dialogueText.text += c;

                if (m_textBeep != null && char.IsLetterOrDigit(c))
                {
                    m_audioSource?.PlayOneShot(m_textBeep, 0.1f);
                }

                yield return new WaitForSeconds(m_typeSpeed);
            }

            m_isTyping = false;
        }

        private void CompleteTyping()
        {
            StopAllCoroutines();
            m_dialogueText.text = m_currentText;
            m_isTyping = false;
        }

        private void NextLine()
        {
            if (m_currentDialogue == null) return;

            Dialogue.DialogueEntry entry = m_currentDialogue.entries[m_currentEntryIndex];
            m_currentLineIndex++;

            if (m_currentLineIndex >= entry.lines.Count)
            {
                m_currentEntryIndex++;
                m_currentLineIndex = 0;
            }

            ShowCurrentLine();
        }

        private void EndDialogue()
        {
            m_isPlaying = false;
            m_currentDialogue = null;

            if (m_dialoguePanel != null)
                m_dialoguePanel.SetActive(false);

            OnDialogueEnd?.Invoke();
        }

        public void SkipDialogue()
        {
            if (!m_currentDialogue?.canSkip ?? true) return;
            EndDialogue();
        }

        public bool IsPlaying()
        {
            return m_isPlaying;
        }

        public void ClearQueue()
        {
            m_dialogueQueue.Clear();
        }
    }
}
