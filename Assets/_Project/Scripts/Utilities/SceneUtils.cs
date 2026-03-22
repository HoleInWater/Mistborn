using UnityEngine;
using UnityEngine.SceneManagement;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for scene management
    /// </summary>
    public static class SceneUtils
    {
        /// <summary>
        /// Loads a scene by name
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneUtils: Scene name is null or empty");
                return;
            }
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Loads a scene by build index
        /// </summary>
        public static void LoadScene(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"SceneUtils: Invalid build index {buildIndex}");
                return;
            }
            SceneManager.LoadScene(buildIndex);
        }

        /// <summary>
        /// Loads a scene asynchronously
        /// </summary>
        public static AsyncOperation LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneUtils: Scene name is null or empty");
                return null;
            }
            return SceneManager.LoadSceneAsync(sceneName);
        }

        /// <summary>
        /// Loads a scene asynchronously by build index
        /// </summary>
        public static AsyncOperation LoadSceneAsync(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"SceneUtils: Invalid build index {buildIndex}");
                return null;
            }
            return SceneManager.LoadSceneAsync(buildIndex);
        }

        /// <summary>
        /// Gets the currently active scene
        /// </summary>
        public static Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        /// <summary>
        /// Gets the name of the currently active scene
        /// </summary>
        public static string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Gets the build index of the currently active scene
        /// </summary>
        public static int GetActiveSceneBuildIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        /// <summary>
        /// Checks if a scene is loaded
        /// </summary>
        public static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        /// <summary>
        /// Gets the number of loaded scenes
        /// </summary>
        public static int GetLoadedSceneCount()
        {
            return SceneManager.loadedSceneCount;
        }

        /// <summary>
        /// Gets the total number of scenes in build settings
        /// </summary>
        public static int GetSceneCountInBuildSettings()
        {
            return SceneManager.sceneCountInBuildSettings;
        }

        /// <summary>
        /// Adds a scene additively (doesn't unload current scene)
        /// </summary>
        public static void LoadSceneAdditive(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneUtils: Scene name is null or empty");
                return;
            }
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Unloads a scene
        /// </summary>
        public static void UnloadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneUtils: Scene name is null or empty");
                return;
            }
            SceneManager.UnloadSceneAsync(sceneName);
        }

        /// <summary>
        /// Reloads the current scene
        /// </summary>
        public static void ReloadCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        /// <summary>
        /// Gets the path of a scene by its build index
        /// </summary>
        public static string GetScenePathByBuildIndex(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"SceneUtils: Invalid build index {buildIndex}");
                return string.Empty;
            }
            return SceneManager.GetSceneByBuildIndex(buildIndex).path;
        }
    }
}
