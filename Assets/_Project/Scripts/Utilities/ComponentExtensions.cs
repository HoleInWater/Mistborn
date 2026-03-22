using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Extension methods for Component class
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Gets a component of type T from the same GameObject, or adds one if missing
        /// </summary>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (component == null) return null;
            T found = component.GetComponent<T>();
            if (found == null)
            {
                found = component.gameObject.AddComponent<T>();
            }
            return found;
        }

        /// <summary>
        /// Gets a component of type T from the parent GameObject
        /// </summary>
        public static T GetComponentInParent<T>(this Component component) where T : Component
        {
            if (component == null) return null;
            return component.GetComponentInParent<T>();
        }

        /// <summary>
        /// Gets a component of type T from the children, including inactive
        /// </summary>
        public static T GetComponentInChildrenIncludingInactive<T>(this Component component) where T : Component
        {
            if (component == null) return null;
            return component.GetComponentInChildren<T>(true);
        }

        /// <summary>
        /// Checks if a component of type T exists on the same GameObject
        /// </summary>
        public static bool HasComponent<T>(this Component component) where T : Component
        {
            if (component == null) return false;
            return component.GetComponent<T>() != null;
        }

        /// <summary>
        /// Removes a component of type T from the same GameObject
        /// </summary>
        public static bool RemoveComponent<T>(this Component component) where T : Component
        {
            if (component == null) return false;
            T found = component.GetComponent<T>();
            if (found != null)
            {
                Object.Destroy(found);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the GameObject of a component
        /// </summary>
        public static GameObject GameObject(this Component component)
        {
            return component != null ? component.gameObject : null;
        }

        /// <summary>
        /// Gets the Transform of a component
        /// </summary>
        public static Transform Transform(this Component component)
        {
            return component != null ? component.transform : null;
        }

        /// <summary>
        /// Sets the active state of the GameObject
        /// </summary>
        public static void SetActive(this Component component, bool active)
        {
            if (component != null)
            {
                component.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Toggles the active state of the GameObject
        /// </summary>
        public static void ToggleActive(this Component component)
        {
            if (component != null)
            {
                component.gameObject.SetActive(!component.gameObject.activeSelf);
            }
        }

        /// <summary>
        /// Gets the distance to another component's GameObject
        /// </summary>
        public static float DistanceTo(this Component component, Component other)
        {
            if (component == null || other == null) return float.MaxValue;
            return Vector3.Distance(component.transform.position, other.transform.position);
        }

        /// <summary>
        /// Gets the direction to another component's GameObject
        /// </summary>
        public static Vector3 DirectionTo(this Component component, Component other)
        {
            if (component == null || other == null) return Vector3.zero;
            return (other.transform.position - component.transform.position).normalized;
        }

        /// <summary>
        /// Checks if this component is within range of another
        /// </summary>
        public static bool IsWithinRange(this Component component, Component other, float range)
        {
            return DistanceTo(component, other) <= range;
        }

        /// <summary>
        /// Duplicates the GameObject of this component
        /// </summary>
        public static GameObject Duplicate(this Component component, string newName = null)
        {
            if (component == null) return null;
            return component.gameObject.Duplicate(newName);
        }

        /// <summary>
        /// Logs a debug message with the component's name
        /// </summary>
        public static void Log(this Component component, object message)
        {
            if (component != null)
            {
                Debug.Log($"[{component.name}] {message}", component);
            }
        }

        /// <summary>
        /// Logs a warning message with the component's name
        /// </summary>
        public static void LogWarning(this Component component, object message)
        {
            if (component != null)
            {
                Debug.LogWarning($"[{component.name}] {message}", component);
            }
        }

        /// <summary>
        /// Logs an error message with the component's name
        /// </summary>
        public static void LogError(this Component component, object message)
        {
            if (component != null)
            {
                Debug.LogError($"[{component.name}] {message}", component);
            }
        }
    }
}
