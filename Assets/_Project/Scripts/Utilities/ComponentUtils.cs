using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common component operations
    /// </summary>
    public static class ComponentUtils
    {
        /// <summary>
        /// Tries to get a component of type T from a GameObject. If not found, adds it.
        /// </summary>
        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Gets all components of type T in the children of a GameObject, including inactive ones.
        /// </summary>
        public static T[] GetComponentsInChildrenIncludingInactive<T>(GameObject go) where T : Component
        {
            if (go == null) return new T[0];
            return go.GetComponentsInChildren<T>(true);
        }

        /// <summary>
        /// Gets all components of type T in the parents of a GameObject.
        /// </summary>
        public static T[] GetComponentsInParents<T>(GameObject go) where T : Component
        {
            if (go == null) return new T[0];
            return go.GetComponentsInParent<T>();
        }

        /// <summary>
        /// Removes a component of type T from a GameObject if it exists.
        /// </summary>
        public static bool RemoveComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return false;
            
            T component = go.GetComponent<T>();
            if (component != null)
            {
                Object.Destroy(component);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a GameObject has a component of type T.
        /// </summary>
        public static bool HasComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return false;
            return go.GetComponent<T>() != null;
        }

        /// <summary>
        /// Gets the first component of type T in the children of a GameObject.
        /// </summary>
        public static T GetComponentInChildren<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            return go.GetComponentInChildren<T>();
        }

        /// <summary>
        /// Gets the first component of type T in the parents of a GameObject.
        /// </summary>
        public static T GetComponentInParent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            return go.GetComponentInParent<T>();
        }

        /// <summary>
        /// Creates a copy of a component and adds it to a GameObject.
        /// </summary>
        public static T CopyComponent<T>(T component, GameObject destination) where T : Component
        {
            if (component == null || destination == null) return null;
            
            Type type = component.GetType();
            if (destination.GetComponent(type) != null)
                return destination.GetComponent(type) as T;
            
            return destination.AddComponent(type) as T;
        }

        /// <summary>
        /// Swaps two components of the same type between two GameObjects.
        /// </summary>
        public static void SwapComponents<T>(GameObject go1, GameObject go2) where T : Component
        {
            if (go1 == null || go2 == null) return;
            
            T comp1 = go1.GetComponent<T>();
            T comp2 = go2.GetComponent<T>();
            
            // Remove both components
            if (comp1 != null) Object.Destroy(comp1);
            if (comp2 != null) Object.Destroy(comp2);
            
            // Add them swapped
            if (comp2 != null) go1.AddComponent(comp2.GetType());
            if (comp1 != null) go2.AddComponent(comp1.GetType());
        }

        /// <summary>
        /// Gets all components of type T that are active and enabled in the children of a GameObject.
        /// </summary>
        public static T[] GetActiveComponentsInChildren<T>(GameObject go) where T : Component
        {
            if (go == null) return new T[0];
            return go.GetComponentsInChildren<T>(true).Where(c => c.enabled && c.gameObject.activeInHierarchy).ToArray();
        }

        /// <summary>
        /// Initializes a component with default values if it's null.
        /// </summary>
        public static T InitializeComponent<T>(ref T component) where T : Component, new()
        {
            if (component == null)
            {
                component = new T();
            }
            return component;
        }

        /// <summary>
        /// Creates a component and adds it to a GameObject, returning the component.
        /// </summary>
        public static T CreateAndAddComponent<T>(GameObject go) where T : Component, new()
        {
            if (go == null) return null;
            T component = go.AddComponent<T>();
            return component;
        }
    }
}