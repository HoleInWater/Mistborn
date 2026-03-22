using UnityEngine;
using System.Collections;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common game object activation/deactivation patterns
    /// </summary>
    public static class ActivationUtils
    {
        /// <summary>
        /// Safely sets a GameObject's active state
        /// </summary>
        public static void SetActiveSafe(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        /// <summary>
        /// Toggles a GameObject's active state
        /// </summary>
        public static void ToggleActive(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(!go.activeSelf);
            }
        }

        /// <summary>
        /// Activates a GameObject after a delay
        /// </summary>
        public static IEnumerator ActivateAfterDelay(GameObject go, float delay)
        {
            if (go == null) yield break;
            
            yield return new WaitForSeconds(delay);
            go.SetActive(true);
        }

        /// <summary>
        /// Deactivates a GameObject after a delay
        /// </summary>
        public static IEnumerator DeactivateAfterDelay(GameObject go, float delay)
        {
            if (go == null) yield break;
            
            yield return new WaitForSeconds(delay);
            go.SetActive(false);
        }

        /// <summary>
        /// Gets all child objects with a specific component type
        /// </summary>
        public static T[] GetChildrenWithComponent<T>(Transform parent) where T : Component
        {
            if (parent == null)
                return new T[0];
                
            return parent.GetComponentsInChildren<T>(true); // true includes inactive
        }

        /// <summary>
        /// Gets all child objects with a specific tag
        /// </summary>
        public static GameObject[] GetChildrenWithTag(Transform parent, string tag)
        {
            if (parent == null || string.IsNullOrEmpty(tag))
                return new GameObject[0];
                
            System.Collections.Generic.List<GameObject> result = new System.Collections.Generic.List<GameObject>();
            
            foreach (Transform child in parent)
            {
                if (child.CompareTag(tag))
                {
                    result.Add(child.gameObject);
                }
            }
            
            return result.ToArray();
        }

        /// <summary>
        /// Checks if a GameObject is visible to a camera
        /// </summary>
        public static bool IsVisibleFromCamera(GameObject go, Camera camera)
        {
            if (go == null || camera == null)
                return false;
                
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, go.GetComponent<Collider>().bounds);
        }

        /// <summary>
        /// Creates a simple fade effect on a CanvasGroup
        /// </summary>
        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            if (canvasGroup == null) yield break;
            
            float startAlpha = canvasGroup.alpha;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
        }
    }
}
