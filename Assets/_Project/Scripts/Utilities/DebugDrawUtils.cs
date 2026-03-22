using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for drawing debug shapes and lines in the scene view
    /// </summary>
    public static class DebugDrawUtils
    {
        /// <summary>
        /// Draws a circle in the scene view
        /// </summary>
        public static void DrawCircle(Vector3 center, float radius, Color color, int segments = 32)
        {
            if (segments < 3) segments = 3;
            float angleStep = 2f * Mathf.PI / segments;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0f) * radius, 0f, Mathf.Sin(0f) * radius);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Debug.DrawLine(prevPoint, point, color);
                prevPoint = point;
            }
        }

        /// <summary>
        /// Draws a wire sphere in the scene view
        /// </summary>
        public static void DrawWireSphere(Vector3 center, float radius, Color color, int latitudeLines = 16, int longitudeLines = 16)
        {
            // Latitude lines
            for (int i = 0; i <= latitudeLines; i++)
            {
                float lat = Mathf.PI * (-0.5f + (float)i / latitudeLines);
                float radiusAtLat = radius * Mathf.Cos(lat);
                float y = radius * Mathf.Sin(lat);
                Vector3 prevPoint = center + new Vector3(0f, y, radiusAtLat);
                for (int j = 1; j <= longitudeLines; j++)
                {
                    float lon = 2f * Mathf.PI * (float)j / longitudeLines;
                    float x = radiusAtLat * Mathf.Cos(lon);
                    float z = radiusAtLat * Mathf.Sin(lon);
                    Vector3 point = center + new Vector3(x, y, z);
                    Debug.DrawLine(prevPoint, point, color);
                    prevPoint = point;
                }
                // Close the loop
                Debug.DrawLine(prevPoint, center + new Vector3(0f, y, radiusAtLat), color);
            }

            // Longitude lines
            for (int i = 0; i <= longitudeLines; i++)
            {
                float lon = 2f * Mathf.PI * (float)i / longitudeLines;
                Vector3 prevPoint = center + new Vector3(radius * Mathf.Cos(lon), 0f, radius * Mathf.Sin(lon));
                for (int j = 1; j <= latitudeLines; j++)
                {
                    float lat = Mathf.PI * (-0.5f + (float)j / latitudeLines);
                    float radiusAtLat = radius * Mathf.Cos(lat);
                    float y = radius * Mathf.Sin(lat);
                    float x = radiusAtLat * Mathf.Cos(lon);
                    float z = radiusAtLat * Mathf.Sin(lon);
                    Vector3 point = center + new Vector3(x, y, z);
                    Debug.DrawLine(prevPoint, point, color);
                    prevPoint = point;
                }
                // Close the loop
                Debug.DrawLine(prevPoint, center + new Vector3(radius * Mathf.Cos(lon), 0f, radius * Mathf.Sin(lon)), color);
            }
        }

        /// <summary>
        /// Draws a wire cube in the scene view
        /// </summary>
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color)
        {
            Vector3 halfSize = size * 0.5f;
            Vector3[] points = new Vector3[8]
            {
                center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                center + new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                center + new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                center + new Vector3(halfSize.x, halfSize.y, halfSize.z),
                center + new Vector3(-halfSize.x, halfSize.y, halfSize.z)
            };

            // Draw the edges
            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);
            Debug.DrawLine(points[4], points[5], color);
            Debug.DrawLine(points[5], points[6], color);
            Debug.DrawLine(points[6], points[7], color);
            Debug.DrawLine(points[7], points[4], color);
            Debug.DrawLine(points[0], points[4], color);
            Debug.DrawLine(points[1], points[5], color);
            Debug.DrawLine(points[2], points[6], color);
            Debug.DrawLine(points[3], points[7], color);
        }

        /// <summary>
        /// Draws a wire capsule in the scene view
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, float height, float radius, Color color, int segments = 16)
        {
            if (height < 2f * radius) height = 2f * radius;
            Vector3 top = center + Vector3.up * (height * 0.5f - radius);
            Vector3 bottom = center - Vector3.up * (height * 0.5f - radius);

            // Draw the top and bottom spheres
            DrawWireSphere(top, radius, color, segments, segments);
            DrawWireSphere(bottom, radius, color, segments, segments);

            // Draw the middle cylinder
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * Mathf.PI * (float)i / segments;
                float x = radius * Mathf.Cos(angle);
                float z = radius * Mathf.Sin(angle);
                Vector3 point1 = center + new Vector3(x, height * 0.5f, z);
                Vector3 point2 = center + new Vector3(x, -height * 0.5f, z);
                Debug.DrawLine(point1, point2, color);
            }
        }

        /// <summary>
        /// Draws a line with an arrow at the end
        /// </summary>
        public static void DrawArrow(Vector3 from, Vector3 to, Color color, float arrowHeadSize = 0.1f, float arrowHeadAngle = 20f)
        {
            Debug.DrawLine(from, to, color);

            Vector3 direction = (to - from).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            if (perpendicular.sqrMagnitude == 0f)
                perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;

            Vector3 arrowDir1 = Quaternion.AngleAxis(arrowHeadAngle, perpendicular) * direction;
            Vector3 arrowDir2 = Quaternion.AngleAxis(-arrowHeadAngle, perpendicular) * direction;

            Vector3 arrowHead1 = to - arrowDir1 * arrowHeadSize;
            Vector3 arrowHead2 = to - arrowDir2 * arrowHeadSize;

            Debug.DrawLine(to, arrowHead1, color);
            Debug.DrawLine(to, arrowHead2, color);
        }

        /// <summary>
        /// Draws a cross at a position
        /// </summary>
        public static void DrawCross(Vector3 center, float size, Color color)
        {
            Vector3 from = center - Vector3.one * size * 0.5f;
            Vector3 to = center + Vector3.one * size * 0.5f;
            Debug.DrawLine(from, to, color);
            from = center - new Vector3(size * 0.5f, -size * 0.5f, size * 0.5f);
            to = center + new Vector3(size * 0.5f, -size * 0.5f, size * 0.5f);
            Debug.DrawLine(from, to, color);
            from = center - new Vector3(size * 0.5f, size * 0.5f, -size * 0.5f);
            to = center + new Vector3(size * 0.5f, size * 0.5f, -size * 0.5f);
            Debug.DrawLine(from, to, color);
        }

        /// <summary>
        /// Draws a rectangle in the scene view
        /// </summary>
        public static void DrawRectangle(Vector3 center, Vector2 size, Color color, float angle = 0f)
        {
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 halfSize = new Vector3(size.x * 0.5f, 0f, size.y * 0.5f);
            Vector3[] points = new Vector3[4]
            {
                center + rotation * new Vector3(-halfSize.x, 0f, -halfSize.z),
                center + rotation * new Vector3(halfSize.x, 0f, -halfSize.z),
                center + rotation * new Vector3(halfSize.x, 0f, halfSize.z),
                center + rotation * new Vector3(-halfSize.x, 0f, halfSize.z)
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);
        }

        /// <summary>
        /// Draws a label in the scene view
        /// </summary>
        public static void DrawLabel(Vector3 worldPosition, string text, Color color, float size = 12f)
        {
            // Note: This requires Handles from UnityEditor, which is not available in runtime builds
            // This is for editor-only debugging
#if UNITY_EDITOR
            UnityEditor.Handles.Label(worldPosition, text, new GUIStyle() { normal = { textColor = color }, fontSize = Mathf.RoundToInt(size) });
#endif
        }

        /// <summary>
        /// Draws a grid in the XZ plane
        /// </summary>
        public static void DrawGrid(Vector3 center, float size, int divisions, Color color)
        {
            float halfSize = size * 0.5f;
            float step = size / divisions;

            // Vertical lines
            for (int i = 0; i <= divisions; i++)
            {
                float x = -halfSize + i * step;
                Vector3 from = center + new Vector3(x, 0f, -halfSize);
                Vector3 to = center + new Vector3(x, 0f, halfSize);
                Debug.DrawLine(from, to, color);
            }

            // Horizontal lines
            for (int i = 0; i <= divisions; i++)
            {
                float z = -halfSize + i * step;
                Vector3 from = center + new Vector3(-halfSize, 0f, z);
                Vector3 to = center + new Vector3(halfSize, 0f, z);
                Debug.DrawLine(from, to, color);
            }
        }
    }
}