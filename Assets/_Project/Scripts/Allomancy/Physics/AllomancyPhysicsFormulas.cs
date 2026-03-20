// ============================================================
// FILE: AllomancyPhysicsFormulas.cs
// SYSTEM: Allomancy / Physics
// STATUS: IMPLEMENTED
// AUTHOR: 
//
// PURPOSE:
//   Centralized physics calculations for Allomancy.
//   Based on book lore, community analysis, and reference to Invested project.
//
// LORE SOURCES:
//   - Coppermind Wiki: Steel, Iron articles
//   - 17th Shard physics discussions
//   - Mistborn Adventure Game
//
// FORMULA NOTES:
//   Force ∝ (AllomancerWeight / TargetMass) × (1 / Distance²)
//   Zenith point = max height reachable
//   Range cap = ~100 paces (~75m)
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy.Physics
{
    public static class AllomancyPhysicsFormulas
    {
        // Base values
        public const float BASE_RANGE = 30f; // Game balance (below max ~75m)
        public const float FALLOFF_START_DISTANCE = 15f;
        public const float MAX_RANGE = 75f; // Lore accurate max
        public const float ZENITH_MULTIPLIER = 1.5f; // Max height boost from single push
        
        // Force calculations
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
            
            // Mass ratio (heavier you = stronger push)
            float massRatio = allomancerWeight / targetMass;
            force *= massRatio;
            
            // Distance falloff
            float distanceFactor = CalculateDistanceFactor(distance);
            force *= distanceFactor;
            
            // Anchored targets push YOU
            if (isAnchored)
            {
                force *= (targetMass / allomancerWeight) * 0.5f;
            }
            
            // Flaring boosts force but drains faster
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
            // Same as push but toward player
            return CalculatePushForce(allomancerWeight, targetMass, distance, isAnchored, isFlaring);
        }
        
        public static float CalculateRecoilForce(
            float pushForce,
            float allomancerWeight,
            float targetMass)
        {
            // Newton's 3rd law - equal and opposite reaction
            float massRatio = targetMass / allomancerWeight;
            return pushForce * massRatio * 0.5f; // 0.5 = recoil damping
        }
        
        public static float CalculateDistanceFactor(float distance)
        {
            if (distance <= FALLOFF_START_DISTANCE)
            {
                return 1f;
            }
            
            // Inverse square falloff after threshold
            float falloffRange = distance - FALLOFF_START_DISTANCE;
            float maxFalloffRange = MAX_RANGE - FALLOFF_START_DISTANCE;
            
            float falloff = 1f - (falloffRange / maxFalloffRange);
            falloff = Mathf.Clamp01(falloff);
            
            // Square the falloff for sharper cutoff
            return falloff * falloff;
        }
        
        public static float CalculateZenithHeight(
            float launchForce,
            float gravity = -20f)
        {
            // Physics: v² = 2gh, so h = v²/2g
            float velocity = launchForce;
            float height = (velocity * velocity) / (2f * Mathf.Abs(gravity));
            return height * ZENITH_MULTIPLIER;
        }
        
        public static bool IsInRange(float distance, float targetMass)
        {
            // Heavier targets have extended range
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
