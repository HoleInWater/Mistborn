using UnityEngine;

namespace MistbornGame.Utilities
{
    public class LayerUtils : MonoBehaviour
    {
        /// <summary>
        /// Gets a layer index by name
        /// </summary>
        public static int GetLayerIndex(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }

        /// <summary>
        /// Gets a layer mask by name
        /// </summary>
        public static int GetLayerMask(string layerName)
        {
            return 1 << GetLayerIndex(layerName);
        }

        /// <summary>
        /// Gets a layer mask by multiple layer names
        /// </summary>
        public static int GetLayerMask(params string[] layerNames)
        {
            int mask = 0;
            foreach (string layerName in layerNames)
            {
                mask |= GetLayerMask(layerName);
            }
            return mask;
        }

        /// <summary>
        /// Checks if a GameObject is in a specific layer index
        /// </summary>
        public static bool IsInLayer(GameObject go, int layer)
        {
            if (go == null) return false;
            return go.layer == layer;
        }

        /// <summary>
        /// Checks if a GameObject is in any of the specified layers (Bitmask)
        /// </summary>
        public static bool IsInLayerMask(GameObject go, int layerMask)
        {
            if (go == null) return false;
            return ((1 << go.layer) & layerMask) != 0;
        }

        /// <summary>
        /// Sets the layer of a GameObject and optionally its children
        /// </summary>
        public static void SetLayer(GameObject go, int layer, bool includeChildren = false)
        {
            if (go == null) return;
            go.layer = layer;
            
            if (includeChildren && go.transform != null)
            {
                foreach (Transform child in go.transform)
                {
                    SetLayer(child.gameObject, layer, true);
                }
            }
        }

        /// <summary>
        /// Toggles a layer in a layer mask
        /// </summary>
        public static int ToggleLayerInMask(int mask, int layer)
        {
            int layerBit = 1 << layer;
            return (mask & layerBit) != 0 ? mask & ~layerBit : mask | layerBit;
        }

        /// <summary>
        /// Checks if a layer is set in a layer mask
        /// </summary>
        public static bool IsLayerSetInMask(int mask, int layer)
        {
            return (mask & (1 << layer)) != 0;
        }
    }
}
