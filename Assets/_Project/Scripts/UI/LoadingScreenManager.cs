using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace Mistborn.UI
{
    public class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private Image m_progressBar;
        [SerializeField] private Text m_progressText;
        [SerializeField] private Text m_sceneNameText;
        [SerializeField] private Image m_tipIcon;
        [SerializeField] private Text m_tipText;

        [Header("Settings")]
        [SerializeField] private float m_minLoadTime = 1f;
        [SerializeField] private bool m_showTips = true;
        [SerializeField] private string[] m_tips;

        [Header("Effects")]
        [SerializeField] private CanvasGroup m_canvasGroup;
        [SerializeField] private float m_fadeSpeed = 2f;

        private bool m_isLoading;
        private AsyncOperation m_asyncOperation;
        private float m_progress;
        private CanvasGroup m_currentCanvas;

        public event Action OnLoadComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (m_loadingPanel != null)
                m_loadingPanel.SetActive(false);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneRoutine(sceneIndex));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            m_isLoading = true;
            ShowLoadingScreen(sceneName);

            float startTime = Time.time;
            m_asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            m_asyncOperation.allowSceneActivation = false;

            while (!m_asyncOperation.isDone)
            {
                UpdateProgress(m_asyncOperation.progress);
                yield return null;
            }

            FinalizeLoad(startTime);
        }

        private IEnumerator LoadSceneRoutine(int sceneIndex)
        {
            m_isLoading = true;
            ShowLoadingScreen($"Scene {sceneIndex}");

            float startTime = Time.time;
            m_asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            m_asyncOperation.allowSceneActivation = false;

            while (!m_asyncOperation.isDone)
            {
                UpdateProgress(m_asyncOperation.progress);
                yield return null;
            }

            FinalizeLoad(startTime);
        }

        private void ShowLoadingScreen(string sceneName)
        {
            if (m_loadingPanel != null)
                m_loadingPanel.SetActive(true);

            if (m_sceneNameText != null)
                m_sceneNameText.text = $"Loading: {sceneName}";

            if (m_tipText != null && m_showTips && m_tips.Length > 0)
            {
                m_tipText.text = m_tips[UnityEngine.Random.Range(0, m_tips.Length)];
            }

            m_progress = 0f;
            UpdateProgressUI();

            StartCoroutine(FadeInRoutine());
        }

        private void UpdateProgress(float progress)
        {
            m_progress = Mathf.Lerp(m_progress, progress, Time.deltaTime * 5f);
            UpdateProgressUI();
        }

        private void UpdateProgressUI()
        {
            if (m_progressBar != null)
                m_progressBar.fillAmount = m_progress;

            if (m_progressText != null)
                m_progressText.text = $"{Mathf.RoundToInt(m_progress * 100)}%";
        }

        private void FinalizeLoad(float startTime)
        {
            float elapsed = Time.time - startTime;
            if (elapsed < m_minLoadTime)
            {
                StartCoroutine(WaitForMinTime(m_minLoadTime - elapsed));
            }
            else
            {
                ActivateScene();
            }
        }

        private IEnumerator WaitForMinTime(float waitTime)
        {
            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                elapsed += Time.deltaTime;
                m_progress = Mathf.Lerp(m_progress, 1f, elapsed / waitTime);
                UpdateProgressUI();
                yield return null;
            }

            ActivateScene();
        }

        private void ActivateScene()
        {
            m_asyncOperation.allowSceneActivation = true;
            OnLoadComplete?.Invoke();

            StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            if (m_canvasGroup == null) yield break;

            m_canvasGroup.alpha = 0f;
            while (m_canvasGroup.alpha < 1f)
            {
                m_canvasGroup.alpha += Time.deltaTime * m_fadeSpeed;
                yield return null;
            }
            m_canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutRoutine()
        {
            if (m_canvasGroup == null)
            {
                if (m_loadingPanel != null)
                    m_loadingPanel.SetActive(false);
                yield break;
            }

            while (m_canvasGroup.alpha > 0f)
            {
                m_canvasGroup.alpha -= Time.deltaTime * m_fadeSpeed;
                yield return null;
            }

            m_canvasGroup.alpha = 0f;
            if (m_loadingPanel != null)
                m_loadingPanel.SetActive(false);
        }

        public bool IsLoading()
        {
            return m_isLoading;
        }

        public static void Load(string sceneName)
        {
            Instance?.LoadScene(sceneName);
        }

        public static void Load(int sceneIndex)
        {
            Instance?.LoadScene(sceneIndex);
        }
    }
}
