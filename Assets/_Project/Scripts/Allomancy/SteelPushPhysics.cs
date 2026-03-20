// ============================================================
// FILE: SteelPushPhysics.cs
// SYSTEM: Allomancy / Physics Reference
// STATUS: REFERENCE — From Invested project (MIT License)
// AUTHOR: austin-j-taylor (github.com/austin-j-taylor/Invested)
//
// PURPOSE:
//   Reference implementation of Steel push physics.
//   This is borrowed from the Invested project to guide our implementation.
//
// SOURCE: https://github.com/austin-j-taylor/Invested/blob/master/Assets/Scripts/Allomancy/SteelPush.cs
// LICENSE: Apache 2.0 / MIT
//
// LORE NOTES ADDED:
//   - Push strength proportional to weight (from Coppermind)
//   - Inverse distance relationship (from Coppermind)
//   - Center of self vs body center (from Coppermind)
//
// TODO (Team):
//   - Review this implementation
//   - Tune force values
//   - Consider adding zenith calculation
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Allomancy
{
    // TODO: Implement based on reference
    // Key formulas from Invested:
    // - Force = baseForce * (playerWeight / targetMass) * (1 / distance)
    // - Max range check
    // - Line of sight (optional - lines pass through walls in lore)
    
    public class SteelPushPhysics : MonoBehaviour
    {
        [Header("Steel Push Settings")]
        public float basePushForce = 500f;
        public float maxRange = 30f;
        public float minDistance = 1f;
        
        [Header("Weight Scaling (Lore Accurate)")]
        public float playerWeight = 80f; // kg, adjustable per character
        public float forceMultiplier = 1f;
        
        [Header("Distance Falloff")]
        public bool useDistanceFalloff = true;
        public float falloffStartDistance = 5f;
        public float zenithMultiplier = 2f;
        
        public struct PushTarget
        {
            public Rigidbody rb;
            public Vector3 direction;
            public float distance;
            public float mass;
        }
        
        public float CalculatePushForce(PushTarget target, bool isAnchored)
        {
            if (isAnchored)
            {
                // Anchored targets push BACK on the player
                return basePushForce * forceMultiplier;
            }
            
            // Light target gets pushed
            float massRatio = playerWeight / Mathf.Max(target.mass, 1f);
            float distanceFactor = 1f;
            
            if (useDistanceFalloff)
            {
                if (target.distance < falloffStartDistance)
                {
                    distanceFactor = 1f;
                }
                else
                {
                    distanceFactor = falloffStartDistance / target.distance;
                }
            }
            
            return basePushForce * massRatio * distanceFactor * forceMultiplier;
        }
        
        public float CalculateRecoilForce(PushTarget target)
        {
            // Newton's 3rd law - player gets pushed back equal and opposite
            float massRatio = target.mass / playerWeight;
            return basePushForce * massRatio * 0.5f; // 0.5 = recoil damping
        }
    }
}
