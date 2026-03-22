using UnityEngine;

namespace MistbornGame.Utilities
{
    public class MathUtils : MonoBehaviour
    {
        /// <summary>
        /// Clamps an angle between -180 and 180 degrees
        /// </summary>
        public static float ClampAngle(float angle)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, -180f, 180f);
        }

        /// <summary>
        /// Returns the shortest angle difference between two angles
        /// </summary>
        public static float DeltaAngle(float current, float target)
        {
            float delta = (target - current) % 360f;
            if (delta > 180f)
                delta -= 360f;
            if (delta <= -180f)
                delta += 360f;
            return delta;
        }

        /// <summary>
        /// Linearly maps a value from one range to another
        /// </summary>
        public static float Map(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Returns true if a value is approximately within a range
        /// </summary>
        public static bool IsInRange(float value, float min, float max, float tolerance = 0.001f)
        {
            return value >= min - tolerance && value <= max + tolerance;
        }

        /// <summary>
        /// Returns a random point on the surface of a sphere
        /// </summary>
        public static Vector3 RandomPointOnSphere(float radius)
        {
            Vector3 randomPoint = Random.insideUnitSphere.normalized * radius;
            return randomPoint;
        }

        /// <summary>
        /// Returns a random point inside a torus (donut shape)
        /// </summary>
        public static Vector3 RandomPointInTorus(float majorRadius, float minorRadius)
        {
            Vector2 randomPoint = Random.insideUnitCircle * minorRadius;
            float angle = Random.value * 360f;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * majorRadius;
            return offset + new Vector3(randomPoint.x, 0f, randomPoint.y);
        }
    }
}