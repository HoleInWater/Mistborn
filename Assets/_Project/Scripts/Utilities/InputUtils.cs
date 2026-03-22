using UnityEngine;

namespace MistbornGame.Utilities
{
    public static class InputUtils
    {
        /// <summary>
        /// Returns true if the key was pressed this frame (ignoring if held down)
        /// </summary>
        public static bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        /// <summary>
        /// Returns true if the key is released this frame
        /// </summary>
        public static bool GetKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        /// <summary>
        /// Returns true while the key is held down
        /// </summary>
        public static bool GetKey(KeyCode key)
        {
            return Input.GetKey(key);
        }

        /// <summary>
        /// Returns the axis value for the given axis name (for analog input)
        /// </summary>
        public static float GetAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        /// <summary>
        /// Returns the raw axis value (without smoothing) for the given axis name
        /// </summary>
        public static float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        /// <summary>
        /// Returns true if any mouse button is clicked
        /// </summary>
        public static bool GetMouseButtonDown()
        {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        }

        /// <summary>
        /// Returns true if the specified mouse button is clicked
        /// </summary>
        public static bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// Returns the current mouse position in screen coordinates
        /// </summary>
        public static Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }

        /// <summary>
        /// Returns the current mouse position in world coordinates (on the given plane)
        /// </summary>
        public static Vector3 GetMouseWorldPosition(float z = 0f)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = z;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// Returns the direction from the main camera to the mouse position in world space
        /// </summary>
        public static Vector3 GetMouseDirection()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
            return (worldPoint - Camera.main.transform.position).normalized;
        }
    }
}