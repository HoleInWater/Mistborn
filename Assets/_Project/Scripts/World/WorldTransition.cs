using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public string targetScene;
    public Vector3 spawnPosition;
    public float transitionDuration = 1f;
    public bool saveProgress = true;
    
    [Header("Visual")]
    public GameObject transitionEffect;
    public string[] requiredKeys;
    
    [Header("Audio")]
    public AudioClip transitionSound;
    
    private bool isTransitioning = false;
    private CanvasGroup fadeCanvas;
    private GameObject fadeObject;
    
    void Start()
    {
        CreateFadeCanvas();
    }
    
    void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        fadeObject = new GameObject("Fade");
        fadeObject.transform.SetParent(canvasObj.transform);
        
        fadeObject.AddComponent<RectTransform>().anchoredPosition = Vector2.zero;
        fadeObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        
        fadeCanvas = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvas.alpha = 0f;
        
        UnityEngine.UI.Image img = fadeObject.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            if (CanTransition())
            {
                StartTransition();
            }
        }
    }
    
    bool CanTransition()
    {
        if (requiredKeys == null || requiredKeys.Length == 0)
            return true;
        
        foreach (string key in requiredKeys)
        {
            if (!PlayerPrefs.HasKey(key))
                return false;
        }
        return true;
    }
    
    public void StartTransition()
    {
        if (isTransitioning) return;
        
        StartCoroutine(TransitionSequence());
    }
    
    System.Collections.IEnumerator TransitionSequence()
    {
        isTransitioning = true;
        
        if (transitionEffect != null)
        {
            transitionEffect.SetActive(true);
        }
        
        if (transitionSound != null)
        {
            AudioSource.PlayClipAtPoint(transitionSound, transform.position);
        }
        
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = elapsed / transitionDuration;
            yield return null;
        }
        
        fadeCanvas.alpha = 1f;
        
        if (saveProgress)
        {
            SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
            saveSystem?.SaveGame();
        }
        
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
            PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
            if (player != null)
            {
                player.transform.position = spawnPosition;
            }
        }
        
        elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = 1f - (elapsed / transitionDuration);
            yield return null;
        }
        
        fadeCanvas.alpha = 0f;
        isTransitioning = false;
        
        if (transitionEffect != null)
        {
            transitionEffect.SetActive(false);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}

public class MistbornSceneManager : MonoBehaviour
{
    [Header("Scene Management")]
    public string[] mainStoryScenes;
    public string[] sideQuestScenes;
    public string tutorialScene;
    
    [Header("Current Progress")]
    public int currentStoryIndex = 0;
    public bool[] completedSideQuests;
    
    [Header("Settings")]
    public bool autoSaveOnSceneChange = true;
    public float sceneLoadDelay = 0.5f;
    
    private static MistbornSceneManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Loaded scene: {scene.name}");
        
        if (autoSaveOnSceneChange)
        {
            SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
            saveSystem?.SaveGame();
        }
    }
    
    public void LoadNextStoryScene()
    {
        if (currentStoryIndex < mainStoryScenes.Length - 1)
        {
            currentStoryIndex++;
            LoadScene(mainStoryScenes[currentStoryIndex]);
        }
        else
        {
            Debug.LogWarning("No more story scenes!");
        }
    }
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(DelayedLoadScene(sceneName));
    }
    
    System.Collections.IEnumerator DelayedLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(sceneName);
    }
    
    public void CompleteSideQuest(int questIndex)
    {
        if (questIndex < completedSideQuests.Length)
        {
            completedSideQuests[questIndex] = true;
            Debug.Log($"Side quest {questIndex} completed!");
        }
    }
    
    public bool IsSideQuestComplete(int questIndex)
    {
        if (questIndex < completedSideQuests.Length)
            return completedSideQuests[questIndex];
        return false;
    }
    
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    public void RestartCurrentScene()
    {
        LoadScene(GetCurrentSceneName());
    }
    
    public void LoadTutorial()
    {
        LoadScene(tutorialScene);
    }
    
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        LoadScene("MainMenu");
    }
}
