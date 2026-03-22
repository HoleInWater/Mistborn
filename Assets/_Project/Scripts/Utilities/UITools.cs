using UnityEngine;
using UnityEngine.UI;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for common UI operations and helpers
    /// </summary>
    public static class UITools
    {
        /// <summary>
        /// Sets the alpha of a CanvasGroup
        /// </summary>
        public static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }

        /// <summary>
        /// Interacts with a CanvasGroup to make it interactable and visible
        /// </summary>
        public static void SetCanvasGroupActive(CanvasGroup canvasGroup, bool active)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = active ? 1f : 0f;
                canvasGroup.interactable = active;
                canvasGroup.blocksRaycasts = active;
            }
        }

        /// <summary>
        /// Sets the text of a UI Text component
        /// </summary>
        public static void SetText(UnityEngine.UI.Text textComponent, string text)
        {
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        /// <summary>
        /// Sets the text of a TMP_Text component (TextMeshPro)
        /// </summary>
        public static void SetText(TMPro.TMP_Text textComponent, string text)
        {
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        /// <summary>
        /// Fades a CanvasGroup over time
        /// </summary>
        public static void FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            if (canvasGroup != null)
            {
                // This would typically be used with a coroutine
                // For now, just set the target alpha immediately
                canvasGroup.alpha = targetAlpha;
            }
        }

        /// <summary>
        /// Enables or disables a Button
        /// </summary>
        public static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button != null)
            {
                button.interactable = enabled;
            }
        }

        /// <summary>
        /// Sets the visibility of a GameObject (UI element)
        /// </summary>
        public static void SetUIVisible(GameObject uiElement, bool visible)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(visible);
            }
        }

        /// <summary>
        /// Finds a UI component in the scene by name
        /// </summary>
        public static T FindUIComponent<T>(string name) where T : MonoBehaviour
        {
            T component = GameObject.Find(name)?.GetComponent<T>();
            if (component == null)
            {
                Debug.LogWarning($"UITools: Could not find UI component {typeof(T).Name} with name {name}");
            }
            return component;
        }

        /// <summary>
        /// Creates a simple UI panel with a background image
        /// </summary>
        public static GameObject CreateUIPanel(string name, Transform parent = null)
        {
            GameObject panel = new GameObject(name);
            panel.AddComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            panel.AddComponent<Image>();
            
            if (parent != null)
            {
                panel.transform.SetParent(parent, false);
            }
            
            return panel;
        }
    }
}
