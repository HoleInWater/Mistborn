using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AllomancyTutorial : MonoBehaviour
{
    [Header("Tutorial Steps")]
    public TutorialStep[] tutorialSteps;
    public int currentStep = 0;
    
    [Header("UI Elements")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image tutorialImage;
    public Button nextButton;
    public Button skipButton;
    public Slider progressSlider;
    
    [Header("Settings")]
    public bool showOnStart = true;
    public bool trackProgress = true;
    public string progressKey = "TutorialProgress";
    
    private bool tutorialActive = false;
    
    void Start()
    {
        if (showOnStart && tutorialSteps.Length > 0)
        {
            LoadProgress();
            
            if (currentStep < tutorialSteps.Length)
            {
                StartTutorial();
            }
        }
        
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStep);
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }
    }
    
    void StartTutorial()
    {
        tutorialActive = true;
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
        
        ShowStep(currentStep);
    }
    
    void ShowStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialSteps.Length)
            return;
        
        TutorialStep step = tutorialSteps[stepIndex];
        
        if (titleText != null)
        {
            titleText.text = step.title;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = step.description;
        }
        
        if (tutorialImage != null && step.image != null)
        {
            tutorialImage.sprite = step.image;
            tutorialImage.gameObject.SetActive(true);
        }
        else if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }
        
        if (progressSlider != null)
        {
            progressSlider.value = (float)(stepIndex + 1) / tutorialSteps.Length;
        }
        
        step.onStepShown?.Invoke();
    }
    
    public void NextStep()
    {
        if (currentStep < tutorialSteps.Length - 1)
        {
            tutorialSteps[currentStep].onStepCompleted?.Invoke();
            currentStep++;
            ShowStep(currentStep);
            SaveProgress();
        }
        else
        {
            CompleteTutorial();
        }
    }
    
    void SkipTutorial()
    {
        CompleteTutorial();
    }
    
    void CompleteTutorial()
    {
        tutorialActive = false;
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        if (trackProgress)
        {
            PlayerPrefs.SetInt(progressKey, tutorialSteps.Length);
            PlayerPrefs.Save();
        }
        
        Debug.Log("Tutorial completed!");
    }
    
    void SaveProgress()
    {
        if (trackProgress)
        {
            PlayerPrefs.SetInt(progressKey, currentStep);
            PlayerPrefs.Save();
        }
    }
    
    void LoadProgress()
    {
        if (trackProgress && PlayerPrefs.HasKey(progressKey))
        {
            currentStep = PlayerPrefs.GetInt(progressKey);
        }
    }
    
    public void ResetTutorial()
    {
        currentStep = 0;
        PlayerPrefs.DeleteKey(progressKey);
        
        if (tutorialSteps.Length > 0)
        {
            StartTutorial();
        }
    }
    
    public bool IsTutorialComplete()
    {
        return currentStep >= tutorialSteps.Length;
    }
    
    public int GetCurrentStep()
    {
        return currentStep;
    }
    
    public int GetTotalSteps()
    {
        return tutorialSteps.Length;
    }
}

[System.Serializable]
public class TutorialStep
{
    public string title;
    [TextArea(2, 5)]
    public string description;
    public Sprite image;
    public UnityEngine.Events.UnityEvent onStepShown;
    public UnityEngine.Events.UnityEvent onStepCompleted;
}

public class MistbornTutorialManager : MonoBehaviour
{
    [Header("Tutorial References")]
    public AllomancyTutorial allomancyTutorial;
    public CombatTutorial combatTutorial;
    public MovementTutorial movementTutorial;
    
    [Header("Progress")]
    public bool[] tutorialCompleted;
    
    void Start()
    {
        LoadTutorialProgress();
    }
    
    void LoadTutorialProgress()
    {
        int savedProgress = PlayerPrefs.GetInt("TutorialManagerProgress", 0);
        
        tutorialCompleted = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            tutorialCompleted[i] = (savedProgress & (1 << i)) != 0;
        }
    }
    
    public void MarkTutorialComplete(int tutorialIndex)
    {
        if (tutorialIndex < tutorialCompleted.Length)
        {
            tutorialCompleted[tutorialIndex] = true;
            SaveTutorialProgress();
        }
    }
    
    void SaveTutorialProgress()
    {
        int progress = 0;
        for (int i = 0; i < tutorialCompleted.Length; i++)
        {
            if (tutorialCompleted[i])
            {
                progress |= (1 << i);
            }
        }
        PlayerPrefs.SetInt("TutorialManagerProgress", progress);
        PlayerPrefs.Save();
    }
    
    public bool IsAllTutorialsComplete()
    {
        foreach (bool complete in tutorialCompleted)
        {
            if (!complete) return false;
        }
        return true;
    }
}

public class CombatTutorial : MonoBehaviour
{
    public void StartCombatTutorial()
    {
        Debug.Log("Starting Combat Tutorial");
    }
}

public class MovementTutorial : MonoBehaviour
{
    public void StartMovementTutorial()
    {
        Debug.Log("Starting Movement Tutorial");
    }
}
