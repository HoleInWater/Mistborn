using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.UI
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [System.Serializable]
        public class TutorialStep
        {
            public string stepId;
            public string title;
            [TextArea(2, 4)]
            public string instruction;
            public string[] highlightObjects;
            public KeyCode? requiredKey;
            public float duration;
            public bool waitForKey;
            public TutorialStep[] nextSteps;
        }

        [Header("Tutorial Data")]
        [SerializeField] private TutorialStep[] m_tutorialSteps;

        [Header("UI")]
        [SerializeField] private GameObject m_tutorialPanel;
        [SerializeField] private UnityEngine.UI.Text m_titleText;
        [SerializeField] private UnityEngine.UI.Text m_instructionText;
        [SerializeField] private UnityEngine.UI.Image m_keyPromptImage;
        [SerializeField] private UnityEngine.UI.Button m_skipButton;
        [SerializeField] private UnityEngine.UI.Button m_nextButton;

        [Header("Settings")]
        [SerializeField] private bool m_canSkip = true;
        [SerializeField] private bool m_showOnStart = false;

        private Queue<TutorialStep> m_tutorialQueue = new Queue<TutorialStep>();
        private TutorialStep m_currentStep;
        private float m_stepTimer;
        private bool m_isRunning;
        private bool m_waitingForKey;
        private HashSet<string> m_completedSteps = new HashSet<string>();

        public event System.Action<TutorialStep> OnStepStart;
        public event System.Action<TutorialStep> OnStepComplete;
        public event System.Action OnTutorialComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (m_tutorialPanel != null)
                m_tutorialPanel.SetActive(false);

            if (m_nextButton != null)
                m_nextButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (m_showOnStart && m_tutorialSteps != null && m_tutorialSteps.Length > 0)
            {
                StartTutorial();
            }
        }

        private void Update()
        {
            if (!m_isRunning || m_currentStep == null) return;

            if (m_waitingForKey && m_currentStep.requiredKey.HasValue)
            {
                if (Input.GetKeyDown(m_currentStep.requiredKey.Value))
                {
                    CompleteCurrentStep();
                }
            }
            else
            {
                m_stepTimer -= Time.deltaTime;
                if (m_stepTimer <= 0 && !m_waitingForKey)
                {
                    CompleteCurrentStep();
                }
            }

            UpdateHighlightObjects();
        }

        public void StartTutorial()
        {
            if (m_tutorialSteps == null || m_tutorialSteps.Length == 0) return;

            m_tutorialQueue.Clear();
            foreach (TutorialStep step in m_tutorialSteps)
            {
                if (!m_completedSteps.Contains(step.stepId))
                {
                    m_tutorialQueue.Enqueue(step);
                }
            }

            m_isRunning = true;

            if (m_tutorialPanel != null)
                m_tutorialPanel.SetActive(true);

            ShowNextStep();
        }

        public void StartTutorial(string tutorialId)
        {
            TutorialStep[] steps = GetTutorialSteps(tutorialId);
            if (steps == null || steps.Length == 0) return;

            m_tutorialQueue.Clear();
            foreach (TutorialStep step in steps)
            {
                if (!m_completedSteps.Contains(step.stepId))
                {
                    m_tutorialQueue.Enqueue(step);
                }
            }

            m_isRunning = true;

            if (m_tutorialPanel != null)
                m_tutorialPanel.SetActive(true);

            ShowNextStep();
        }

        private TutorialStep[] GetTutorialSteps(string tutorialId)
        {
            List<TutorialStep> steps = new List<TutorialStep>();
            foreach (TutorialStep step in m_tutorialSteps)
            {
                if (step.stepId.StartsWith(tutorialId))
                {
                    steps.Add(step);
                }
            }
            return steps.ToArray();
        }

        private void ShowNextStep()
        {
            if (m_tutorialQueue.Count == 0)
            {
                EndTutorial();
                return;
            }

            m_currentStep = m_tutorialQueue.Dequeue();
            m_stepTimer = m_currentStep.duration > 0 ? m_currentStep.duration : 5f;
            m_waitingForKey = m_currentStep.waitForKey && m_currentStep.requiredKey.HasValue;

            UpdateUI();
            OnStepStart?.Invoke(m_currentStep);
        }

        private void UpdateUI()
        {
            if (m_titleText != null)
                m_titleText.text = m_currentStep.title;

            if (m_instructionText != null)
                m_instructionText.text = m_currentStep.instruction;

            if (m_nextButton != null)
                m_nextButton.gameObject.SetActive(!m_waitingForKey && m_currentStep.duration <= 0);

            if (m_skipButton != null)
                m_skipButton.gameObject.SetActive(m_canSkip);
        }

        private void CompleteCurrentStep()
        {
            m_completedSteps.Add(m_currentStep.stepId);
            OnStepComplete?.Invoke(m_currentStep);

            if (m_currentStep.nextSteps != null && m_currentStep.nextSteps.Length > 0)
            {
                foreach (TutorialStep next in m_currentStep.nextSteps)
                {
                    if (!m_completedSteps.Contains(next.stepId))
                    {
                        m_tutorialQueue.Enqueue(next);
                    }
                }
            }

            ShowNextStep();
        }

        private void UpdateHighlightObjects()
        {
            if (m_currentStep?.highlightObjects == null) return;

            foreach (string objName in m_currentStep.highlightObjects)
            {
                GameObject obj = GameObject.Find(objName);
                if (obj != null)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
                        renderer.material.color = Color.Lerp(Color.white, Color.yellow, pulse);
                    }
                }
            }
        }

        public void SkipTutorial()
        {
            if (!m_canSkip) return;
            EndTutorial();
        }

        public void NextStep()
        {
            if (m_currentStep == null || m_waitingForKey) return;
            CompleteCurrentStep();
        }

        private void EndTutorial()
        {
            m_isRunning = false;
            m_currentStep = null;

            if (m_tutorialPanel != null)
                m_tutorialPanel.SetActive(false);

            OnTutorialComplete?.Invoke();
        }

        public bool IsRunning()
        {
            return m_isRunning;
        }

        public TutorialStep GetCurrentStep()
        {
            return m_currentStep;
        }

        public bool IsStepCompleted(string stepId)
        {
            return m_completedSteps.Contains(stepId);
        }

        public void ResetTutorial()
        {
            m_completedSteps.Clear();
            m_tutorialQueue.Clear();
            EndTutorial();
        }
    }

    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private string m_tutorialId;
        [SerializeField] private bool m_oneTimeOnly = true;
        [SerializeField] private bool m_autoStart = true;

        private bool m_hasTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (m_oneTimeOnly && m_hasTriggered) return;

            m_hasTriggered = true;

            if (m_autoStart)
            {
                TutorialManager.Instance?.StartTutorial(m_tutorialId);
            }
        }

        public void TriggerTutorial()
        {
            if (m_oneTimeOnly && m_hasTriggered) return;

            m_hasTriggered = true;
            TutorialManager.Instance?.StartTutorial(m_tutorialId);
        }
    }
}
