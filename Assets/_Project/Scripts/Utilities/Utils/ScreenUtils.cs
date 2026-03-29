using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for screen and resolution operations
    /// </summary>
    public static class ScreenUtils
    {
        /// <summary>
        /// Gets the screen width in pixels
        /// </summary>
        public static int GetWidth()
        {
            return Screen.width;
        }

        /// <summary>
        /// Gets the screen height in pixels
        /// </summary>
        public static int GetHeight()
        {
            return Screen.height;
        }

        /// <summary>
        /// Gets the screen resolution as a Vector2
        /// </summary>
        public static Vector2 GetResolution()
        {
            return new Vector2(Screen.width, Screen.height);
        }

        /// <summary>
        /// Gets the screen aspect ratio
        /// </summary>
        public static float GetAspectRatio()
        {
            return (float)Screen.width / Screen.height;
        }

        /// <summary>
        /// Gets the center of the screen in pixels
        /// </summary>
        public static Vector2 GetCenter()
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        /// <summary>
        /// Converts screen pixels to normalized coordinates (0-1)
        /// </summary>
        public static Vector2 ToNormalized(Vector2 screenPoint)
        {
            return new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
        }

        /// <summary>
        /// Converts normalized coordinates (0-1) to screen pixels
        /// </summary>
        public static Vector2 FromNormalized(Vector2 normalizedPoint)
        {
            return new Vector2(normalizedPoint.x * Screen.width, normalizedPoint.y * Screen.height);
        }

        /// <summary>
        /// Checks if a screen point is within the screen bounds
        /// </summary>
        public static bool IsOnScreen(Vector2 screenPoint)
        {
            return screenPoint.x >= 0 && screenPoint.x <= Screen.width && 
                   screenPoint.y >= 0 && screenPoint.y <= Screen.height;
        }

        /// <summary>
        /// Gets the DPI (dots per inch) of the screen
        /// </summary>
        public static float GetDPI()
        {
            return Screen.dpi;
        }

        /// <summary>
        /// Gets the pixel density (pixels per unit based on DPI)
        /// </summary>
        public static float GetPixelDensity()
        {
            if (Screen.dpi <= 0f) return 1f;
            return Screen.dpi / 160f; // Android baseline is 160 DPI
        }

        /// <summary>
        /// Gets the orientation of the screen
        /// </summary>
        public static ScreenOrientation GetOrientation()
        {
            return Screen.orientation;
        }

        /// <summary>
        /// Sets the screen to fullscreen mode
        /// </summary>
        public static void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
        /// Sets the screen resolution
        /// </summary>
        public static void SetResolution(int width, int height, bool fullscreen)
        {
            Screen.SetResolution(width, height, fullscreen);
        }

        /// <summary>
        /// Sets the screen resolution with refresh rate
        /// </summary>
        public static void SetResolution(int width, int height, bool fullscreen, int refreshRate)
        {
            Screen.SetResolution(width, height, fullscreen, refreshRate);
        }

        /// <summary>
        /// Gets the current refresh rate
        /// </summary>
        public static int GetRefreshRate()
        {
            return Screen.currentResolution.refreshRate;
        }

        /// <summary>
        /// Gets the current resolution
        /// </summary>
        public static Resolution GetResolutionObject()
        {
            return Screen.currentResolution;
        }

        /// <summary>
        /// Gets all available resolutions
        /// </summary>
        public static Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions;
        }

        /// <summary>
        /// Calculates the best matching resolution from available resolutions
        /// </summary>
        public static Resolution GetBestMatchingResolution(int targetWidth, int targetHeight)
        {
            Resolution[] resolutions = GetAvailableResolutions();
            if (resolutions.Length == 0) return Screen.currentResolution;
            
            Resolution best = resolutions[0];
            int bestDiff = Mathf.Abs(best.width - targetWidth) + Mathf.Abs(best.height - targetHeight);
            
            for (int i = 1; i < resolutions.Length; i++)
            {
                int diff = Mathf.Abs(resolutions[i].width - targetWidth) + Mathf.Abs(resolutions[i].height - targetHeight);
                if (diff < bestDiff)
                {
                    best = resolutions[i];
                    bestDiff = diff;
                }
            }
            
            return best;
        }

        /// <summary>
        /// Gets the safe area of the screen (for notches, etc.)
        /// </summary>
        public static Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        /// <summary>
        /// Checks if the screen has a notch or other cutout
        /// </summary>
        public static bool HasCutout()
        {
            Rect safeArea = GetSafeArea();
            return safeArea.x > 0 || safeArea.y > 0 || 
                   safeArea.width < Screen.width || safeArea.height < Screen.height;
        }

        /// <summary>
        /// Converts a world position to screen position
        /// </summary>
        public static Vector2 WorldToScreen(Vector3 worldPosition, Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return Vector2.zero;
            
            Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);
            return new Vector2(screenPos.x, screenPos.y);
        }

        /// <summary>
        /// Converts a screen position to world position at a specific distance
        /// </summary>
        public static Vector3 ScreenToWorld(Vector2 screenPosition, float distance, Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return Vector3.zero;
            
            return camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distance));
        }

        /// <summary>
        /// Gets the mouse position in screen coordinates
        /// </summary>
        public static Vector2 GetMousePosition()
        {
            return Input.mousePosition;
        }

        /// <summary>
        /// Gets the mouse position in normalized coordinates
        /// </summary>
        public static Vector2 GetMousePositionNormalized()
        {
            return ToNormalized(GetMousePosition());
        }

        /// <summary>
        /// Checks if mouse is over a UI element
        /// </summary>
        public static bool IsMouseOverUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
}
