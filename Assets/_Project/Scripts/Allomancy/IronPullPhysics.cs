// ============================================================
// FILE: IronPullPhysics.cs  
// SYSTEM: Allomancy / Physics Reference
// STATUS: REFERENCE — From Invested project (MIT License)
// AUTHOR: austin-j-taylor (github.com/austin-j-taylor/Invested)
//
// PURPOSE:
//   Reference implementation of Iron pull physics.
//   Mirror of Steel push but attracts instead of repels.
//
// SOURCE: https://github.com/austin-j-taylor/Invested/blob/master/Assets/Scripts/Allomancy/IronPull.cs
// LICENSE: Apache 2.0 / MIT
//
// LORE NOTES:
//   - Same rules as Steel push (weight proportional, distance inverse)
//   - Pull direction is toward "center of self"
//   - Heavy anchors pull the player toward them
//
// TODO (Team):
//   - Review force values
//   - Test anchored pull feel
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;

namespace Mistborn.Allomancy
{
    public class IronPullPhysics : MonoBehaviour
    {
        [Header("Iron Pull Settings")]
        public float basePullForce = 500f;
        public float maxRange = 30f;
        public float minDistance = 1f;
        
        [Header("Weight Scaling")]
        public float playerWeight = 80f;
        public float forceMultiplier = 1f;
        
        public float CalculatePullForce(float targetMass, float distance, bool isAnchored)
        {
            if (isAnchored)
            {
                // Anchored targets pull the player toward them
                return basePullForce * forceMultiplier * 0.5f;
            }
            
            float massRatio = playerWeight / Mathf.Max(targetMass, 1f);
            float distanceFactor = 1f;
            
            if (distance > minDistance)
            {
                distanceFactor = 1f / distance;
            }
            
            return basePullForce * massRatio * distanceFactor * forceMultiplier;
        }
    }
}
