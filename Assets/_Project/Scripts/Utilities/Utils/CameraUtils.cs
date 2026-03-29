using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for Camera operations
    /// </summary>
    public static class CameraUtils
    {
        /// <summary>
        /// Gets the main camera
        /// </summary>
        public static Camera GetMainCamera()
        {
            return Camera.main;
        }

        /// <summary>
        /// Checks if a point is visible by a camera
        /// </summary>
        public static bool IsVisible(Camera camera, Vector3 worldPoint)
        {
            if (camera == null) return false;
            Vector3 viewportPoint = camera.WorldToViewportPoint(worldPoint);
            return viewportPoint.z > 0f && 
                   viewportPoint.x >= 0f && viewportPoint.x <= 1f && 
                   viewportPoint.y >= 0f && viewportPoint.y <= 1f;
        }

        /// <summary>
        /// Checks if a bounding box is visible by a camera
        /// </summary>
        public static bool IsBoundsVisible(Camera camera, Bounds bounds)
        {
            if (camera == null) return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }

        /// <summary>
        /// Gets the screen position of a world point
        /// </summary>
        public static Vector3 WorldToScreen(Camera camera, Vector3 worldPoint)
        {
            if (camera == null) return Vector3.zero;
            return camera.WorldToScreenPoint(worldPoint);
        }

        /// <summary>
        /// Gets the world position from a screen point
        /// </summary>
        public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPoint, float distance)
        {
            if (camera == null) return Vector3.zero;
            return camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distance));
        }

        /// <summary>
        /// Gets the world position from a screen point with depth
        /// </summary>
        public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPoint)
        {
            if (camera == null) return Vector3.zero;
            Ray ray = camera.ScreenPointToRay(screenPoint);
            return ray.GetPoint(camera.nearClipPlane);
        }

        /// <summary>
        /// Gets the field of view at a specific distance
        /// </summary>
        public static float GetFOVAtDistance(Camera camera, float distance)
        {
            if (camera == null || camera.orthographic) return 0f;
            // For perspective cameras, FOV doesn't change with distance
            return camera.fieldOfView;
        }

        /// <summary>
        /// Gets the orthographic size at a specific distance (for perspective cameras)
        /// </summary>
        public static float GetOrthographicSizeAtDistance(Camera camera, float distance)
        {
            if (camera == null) return 0f;
            if (camera.orthographic) return camera.orthographicSize;
            
            // For perspective cameras, calculate what orthographic size would be at this distance
            float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            return Mathf.Tan(halfFov) * distance;
        }

        /// <summary>
        /// Gets the width of the camera view at a specific distance
        /// </summary>
        public static float GetViewWidthAtDistance(Camera camera, float distance)
        {
            if (camera == null) return 0f;
            if (camera.orthographic)
            {
                return camera.orthographicSize * 2f * camera.aspect;
            }
            else
            {
                float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                return Mathf.Tan(halfFov) * distance * 2f * camera.aspect;
            }
        }

        /// <summary>
        /// Gets the height of the camera view at a specific distance
        /// </summary>
        public static float GetViewHeightAtDistance(Camera camera, float distance)
        {
            if (camera == null) return 0f;
            if (camera.orthographic)
            {
                return camera.orthographicSize * 2f;
            }
            else
            {
                float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                return Mathf.Tan(halfFov) * distance * 2f;
            }
        }

        /// <summary>
        /// Gets the ray from a screen point
        /// </summary>
        public static Ray ScreenPointToRay(Camera camera, Vector3 screenPoint)
        {
            if (camera == null) return new Ray();
            return camera.ScreenPointToRay(screenPoint);
        }

        /// <summary>
        /// Gets the ray from the center of the camera
        /// </summary>
        public static Ray CenterRay(Camera camera)
        {
            if (camera == null) return new Ray();
            Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            return camera.ScreenPointToRay(center);
        }

        /// <summary>
        /// Gets the ray from the mouse position
        /// </summary>
        public static Ray MouseRay(Camera camera)
        {
            if (camera == null) return new Ray();
            return camera.ScreenPointToRay(Input.mousePosition);
        }

        /// <summary>
        /// Sets the camera to look at a target
        /// </summary>
        public static void LookAt(Camera camera, Vector3 target, Vector3? up = null)
        {
            if (camera == null) return;
            camera.transform.LookAt(target, up ?? Vector3.up);
        }

        /// <summary>
        /// Gets the corners of the camera's view at a specific distance
        /// </summary>
        public static Vector3[] GetViewCorners(Camera camera, float distance)
        {
            if (camera == null) return new Vector3[4];
            
            Vector3[] corners = new Vector3[4];
            if (camera.orthographic)
            {
                float halfHeight = camera.orthographicSize;
                float halfWidth = halfHeight * camera.aspect;
                Vector3 center = camera.transform.position + camera.transform.forward * distance;
                Vector3 right = camera.transform.right * halfWidth;
                Vector3 up = camera.transform.up * halfHeight;
                
                corners[0] = center - right - up; // bottom left
                corners[1] = center + right - up; // bottom right
                corners[2] = center + right + up; // top right
                corners[3] = center - right + up; // top left
            }
            else
            {
                float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float halfHeight = Mathf.Tan(halfFov) * distance;
                float halfWidth = halfHeight * camera.aspect;
                Vector3 center = camera.transform.position + camera.transform.forward * distance;
                Vector3 right = camera.transform.right * halfWidth;
                Vector3 up = camera.transform.up * halfHeight;
                
                corners[0] = center - right - up; // bottom left
                corners[1] = center + right - up; // bottom right
                corners[2] = center + right + up; // top right
                corners[3] = center - right + up; // top left
            }
            return corners;
        }

        /// <summary>
        /// Gets the bounds of the camera's view at a specific distance
        /// </summary>
        public static Bounds GetViewBounds(Camera camera, float distance)
        {
            Vector3[] corners = GetViewCorners(camera, distance);
            Vector3 min = corners[0];
            Vector3 max = corners[0];
            
            for (int i = 1; i < corners.Length; i++)
            {
                min = Vector3.Min(min, corners[i]);
                max = Vector3.Max(max, corners[i]);
            }
            
            return new Bounds((min + max) * 0.5f, max - min);
        }

        /// <summary>
        /// Checks if a point is behind the camera
        /// </summary>
        public static bool IsBehindCamera(Camera camera, Vector3 worldPoint)
        {
            if (camera == null) return false;
            Vector3 viewportPoint = camera.WorldToViewportPoint(worldPoint);
            return viewportPoint.z <= 0f;
        }

        /// <summary>
        /// Gets the distance from camera to a point (signed)
        /// </summary>
        public static float GetDistanceToCamera(Camera camera, Vector3 worldPoint)
        {
            if (camera == null) return float.MaxValue;
            return Vector3.Distance(camera.transform.position, worldPoint);
        }

        /// <summary>
        /// Gets the forward direction of the camera
        /// </summary>
        public static Vector3 Forward(Camera camera)
        {
            if (camera == null) return Vector3.forward;
            return camera.transform.forward;
        }

        /// <summary>
        /// Gets the right direction of the camera
        /// </summary>
        public static Vector3 Right(Camera camera)
        {
            if (camera == null) return Vector3.right;
            return camera.transform.right;
        }

        /// <summary>
        /// Gets the up direction of the camera
        /// </summary>
        public static Vector3 Up(Camera camera)
        {
            if (camera == null) return Vector3.up;
            return camera.transform.up;
        }
    }
}
