using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for debug drawing and logging
    /// </summary>
    public static class DebugUtils
    {
        /// <summary>
        /// Draws a line between two points
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = false)
        {
            Debug.DrawLine(start, end, color, duration, depthTest);
        }

        /// <summary>
        /// Draws a line with thickness
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float width = 1f, float duration = 0f)
        {
            // Unity's Debug.DrawLine doesn't support width, but we can use Debug.DrawRay for approximation
            Vector3 direction = end - start;
            float length = direction.magnitude;
            Debug.DrawRay(start, direction.normalized, color, duration, false);
            // For thicker lines, you might want to use Gizmos or a custom solution
        }

        /// <summary>
        /// Draws a ray from a point
        /// </summary>
        public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 0f, bool depthTest = false)
        {
            Debug.DrawRay(origin, direction, color, duration, depthTest);
        }

        /// <summary>
        /// Draws a sphere at a position
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f, bool depthTest = false)
        {
            // Draw lines approximating a sphere
            float deltaTheta = 0.1f;
            float deltaPhi = 0.1f;
            
            for (float theta = 0; theta < Mathf.PI * 2; theta += deltaTheta)
            {
                for (float phi = 0; phi < Mathf.PI; phi += deltaPhi)
                {
                    Vector3 start = center + SphericalToCartesian(radius, theta, phi);
                    Vector3 end = center + SphericalToCartesian(radius, theta + deltaTheta, phi);
                    Debug.DrawLine(start, end, color, duration, depthTest);
                    
                    end = center + SphericalToCartesian(radius, theta, phi + deltaPhi);
                    Debug.DrawLine(start, end, color, duration, depthTest);
                }
            }
        }

        private static Vector3 SphericalToCartesian(float radius, float theta, float phi)
        {
            float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = radius * Mathf.Cos(phi);
            float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Draws a cube at a position
        /// </summary>
        public static void DrawCube(Vector3 center, Vector3 size, Color color, float duration = 0f, bool depthTest = false)
        {
            Vector3 halfSize = size * 0.5f;
            
            // Draw 12 edges
            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            corners[1] = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            corners[2] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            corners[3] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            corners[4] = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            corners[5] = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            corners[6] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            corners[7] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            
            // Bottom square
            Debug.DrawLine(corners[0], corners[1], color, duration, depthTest);
            Debug.DrawLine(corners[1], corners[5], color, duration, depthTest);
            Debug.DrawLine(corners[5], corners[4], color, duration, depthTest);
            Debug.DrawLine(corners[4], corners[0], color, duration, depthTest);
            
            // Top square
            Debug.DrawLine(corners[2], corners[3], color, duration, depthTest);
            Debug.DrawLine(corners[3], corners[7], color, duration, depthTest);
            Debug.DrawLine(corners[7], corners[6], color, duration, depthTest);
            Debug.DrawLine(corners[6], corners[2], color, duration, depthTest);
            
            // Vertical edges
            Debug.DrawLine(corners[0], corners[2], color, duration, depthTest);
            Debug.DrawLine(corners[1], corners[3], color, duration, depthTest);
            Debug.DrawLine(corners[4], corners[6], color, duration, depthTest);
            Debug.DrawLine(corners[5], corners[7], color, duration, depthTest);
        }

        /// <summary>
        /// Draws a circle at a position
        /// </summary>
        public static void DrawCircle(Vector3 center, float radius, Color color, float duration = 0f, bool depthTest = false, int segments = 32)
        {
            float deltaTheta = (2f * Mathf.PI) / segments;
            float theta = 0f;
            
            Vector3 start = center + new Vector3(Mathf.Cos(theta) * radius, 0f, Mathf.Sin(theta) * radius);
            for (int i = 0; i < segments; i++)
            {
                theta += deltaTheta;
                Vector3 end = center + new Vector3(Mathf.Cos(theta) * radius, 0f, Mathf.Sin(theta) * radius);
                Debug.DrawLine(start, end, color, duration, depthTest);
                start = end;
            }
        }

        /// <summary>
        /// Draws a circle on a plane (with normal)
        /// </summary>
        public static void DrawCircle(Vector3 center, float radius, Color color, Vector3 normal, float duration = 0f, bool depthTest = false, int segments = 32)
        {
            normal.Normalize();
            Vector3 tangent = Vector3.Cross(normal, Vector3.right).normalized;
            if (tangent.magnitude < 0.001f)
            {
                tangent = Vector3.Cross(normal, Vector3.forward).normalized;
            }
            Vector3 binormal = Vector3.Cross(normal, tangent).normalized;
            
            float deltaTheta = (2f * Mathf.PI) / segments;
            float theta = 0f;
            
            Vector3 start = center + (tangent * Mathf.Cos(theta) + binormal * Mathf.Sin(theta)) * radius;
            for (int i = 0; i < segments; i++)
            {
                theta += deltaTheta;
                Vector3 end = center + (tangent * Mathf.Cos(theta) + binormal * Mathf.Sin(theta)) * radius;
                Debug.DrawLine(start, end, color, duration, depthTest);
                start = end;
            }
        }

        /// <summary>
        /// Draws a triangle
        /// </summary>
        public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color, float duration = 0f, bool depthTest = false)
        {
            Debug.DrawLine(a, b, color, duration, depthTest);
            Debug.DrawLine(b, c, color, duration, depthTest);
            Debug.DrawLine(c, a, color, duration, depthTest);
        }

        /// <summary>
        /// Draws a cone
        /// </summary>
        public static void DrawCone(Vector3 apex, Vector3 direction, float height, float angle, Color color, float duration = 0f, bool depthTest = false, int segments = 16)
        {
            direction.Normalize();
            Vector3 baseCenter = apex + direction * height;
            float baseRadius = Mathf.Tan(angle * Mathf.Deg2Rad) * height;
            
            // Draw base circle
            DrawCircle(baseCenter, baseRadius, color, direction, duration, depthTest, segments);
            
            // Draw lines from apex to base circle
            float deltaTheta = (2f * Mathf.PI) / segments;
            for (int i = 0; i < segments; i++)
            {
                float theta = i * deltaTheta;
                Vector3 pointOnCircle = baseCenter + (Vector3.Cross(direction, Vector3.right).normalized * Mathf.Cos(theta) + Vector3.Cross(direction, Vector3.forward).normalized * Mathf.Sin(theta)) * baseRadius;
                Debug.DrawLine(apex, pointOnCircle, color, duration, depthTest);
            }
        }

        /// <summary>
        /// Draws an arrow from start to end
        /// </summary>
        public static void DrawArrow(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = false, float arrowHeadLength = 0.2f, float arrowHeadAngle = 20f)
        {
            Debug.DrawLine(start, end, color, duration, depthTest);
            
            Vector3 direction = end - start;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            if (right.magnitude < 0.001f)
            {
                right = Vector3.Cross(direction, Vector3.forward).normalized;
            }
            Vector3 up = Vector3.Cross(right, direction).normalized;
            
            // Draw arrow head
            Vector3 arrowHeadEnd1 = end - direction.normalized * arrowHeadLength;
            Vector3 arrowHeadEnd2 = end - direction.normalized * arrowHeadLength + up * arrowHeadLength * Mathf.Tan(arrowHeadAngle * Mathf.Deg2Rad);
            Vector3 arrowHeadEnd3 = end - direction.normalized * arrowHeadLength - up * arrowHeadLength * Mathf.Tan(arrowHeadAngle * Mathf.Deg2Rad);
            
            Debug.DrawLine(end, arrowHeadEnd1, color, duration, depthTest);
            Debug.DrawLine(end, arrowHeadEnd2, color, duration, depthTest);
            Debug.DrawLine(end, arrowHeadEnd3, color, duration, depthTest);
        }

        /// <summary>
        /// Logs a message with a timestamp
        /// </summary>
        public static void LogWithTimestamp(string message)
        {
            Debug.Log($"[{Time.time:F2}] {message}");
        }

        /// <summary>
        /// Logs a warning with a timestamp
        /// </summary>
        public static void LogWarningWithTimestamp(string message)
        {
            Debug.LogWarning($"[{Time.time:F2}] {message}");
        }

        /// <summary>
        /// Logs an error with a timestamp
        /// </summary>
        public static void LogErrorWithTimestamp(string message)
        {
            Debug.LogError($"[{Time.time:F2}] {message}");
        }

        /// <summary>
        /// Logs a message with a specific color using HTML tags (Unity console)
        /// </summary>
        public static void LogColored(string message, string color)
        {
            Debug.Log($"<color=\"{color}\">{message}</color>");
        }

        /// <summary>
        /// Logs a message with a specific size using HTML tags (Unity console)
        /// </summary>
        public static void LogSized(string message, int size)
        {
            Debug.Log($"<size={size}>{message}</size>");
        }

        /// <summary>
        /// Logs a bold message using HTML tags (Unity console)
        /// </summary>
        public static void LogBold(string message)
        {
            Debug.Log($"<b>{message}</b>");
        }

        /// <summary>
        /// Logs an italic message using HTML tags (Unity console)
        /// </summary>
        public static void LogItalic(string message)
        {
            Debug.Log($"<i>{message}</i>");
        }

        /// <summary>
        /// Draws a coordinate axes at a position
        /// </summary>
        public static void DrawAxes(Vector3 origin, float length = 1f, float duration = 0f, bool depthTest = false)
        {
            Debug.DrawRay(origin, Vector3.right * length, Color.red, duration, depthTest);
            Debug.DrawRay(origin, Vector3.up * length, Color.green, duration, depthTest);
            Debug.DrawRay(origin, Vector3.forward * length, Color.blue, duration, depthTest);
        }

        /// <summary>
        /// Draws a grid on the XY plane
        /// </summary>
        public static void DrawGrid(Vector3 origin, int xCells, int yCells, float cellSize, Color color, float duration = 0f, bool depthTest = false)
        {
            for (int x = 0; x <= xCells; x++)
            {
                Vector3 start = origin + new Vector3(x * cellSize, 0f, 0f);
                Vector3 end = start + new Vector3(0f, yCells * cellSize, 0f);
                Debug.DrawLine(start, end, color, duration, depthTest);
            }
            
            for (int y = 0; y <= yCells; y++)
            {
                Vector3 start = origin + new Vector3(0f, y * cellSize, 0f);
                Vector3 end = start + new Vector3(xCells * cellSize, 0f, 0f);
                Debug.DrawLine(start, end, color, duration, depthTest);
            }
        }
    }
}
