using UnityEngine;

namespace Mistborn.Allomancy.Physics
{
    public static class AllomancyPhysicsFormulas
    {
        public const float BASE_RANGE = 30f;
        public const float FALLOFF_START_DISTANCE = 15f;
        public const float MAX_RANGE = 75f;
        public const float ZENITH_MULTIPLIER = 1.5f;

        public static float CalculatePushForce(
            float allomancerWeight,
            float targetMass,
            float distance,
            bool isAnchored,
            bool isFlaring = false)
        {
            if (distance > MAX_RANGE) return 0f;
            if (targetMass <= 0f) return 0f;

            float force = BASE_RANGE;

            float massRatio = allomancerWeight / targetMass;
            force *= massRatio;

            float distanceFactor = CalculateDistanceFactor(distance);
            force *= distanceFactor;

            if (isAnchored)
            {
                force *= (targetMass / allomancerWeight) * 0.5f;
            }

            if (isFlaring)
            {
                force *= 2f;
            }

            return Mathf.Max(force, 0f);
        }

        public static float CalculatePullForce(
            float allomancerWeight,
            float targetMass,
            float distance,
            bool isAnchored,
            bool isFlaring = false)
        {
            return CalculatePushForce(allomancerWeight, targetMass, distance, isAnchored, isFlaring);
        }

        public static float CalculateRecoilForce(
            float pushForce,
            float allomancerWeight,
            float targetMass)
        {
            float massRatio = targetMass / allomancerWeight;
            return pushForce * massRatio * 0.5f;
        }

        public static float CalculateDistanceFactor(float distance)
        {
            if (distance <= FALLOFF_START_DISTANCE)
            {
                return 1f;
            }

            float falloffRange = distance - FALLOFF_START_DISTANCE;
            float maxFalloffRange = MAX_RANGE - FALLOFF_START_DISTANCE;

            float falloff = 1f - (falloffRange / maxFalloffRange);
            falloff = Mathf.Clamp01(falloff);

            return falloff * falloff;
        }

        public static float CalculateZenithHeight(
            float launchForce,
            float gravity = -20f)
        {
            float velocity = launchForce;
            float height = (velocity * velocity) / (2f * Mathf.Abs(gravity));
            return height * ZENITH_MULTIPLIER;
        }

        public static bool IsInRange(float distance, float targetMass)
        {
            float massBonus = Mathf.Log(targetMass + 1f) * 2f;
            return distance <= (MAX_RANGE + massBonus);
        }

        public static Vector3 GetPushDirection(Vector3 allomancerPosition, Vector3 targetPosition)
        {
            return (targetPosition - allomancerPosition).normalized;
        }

        public static Vector3 GetPullDirection(Vector3 allomancerPosition, Vector3 targetPosition)
        {
            return (allomancerPosition - targetPosition).normalized;
        }
    }
}
