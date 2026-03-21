using UnityEngine;

namespace Mistborn.Allomancy
{
    public class IronPullPhysics : MonoBehaviour
    {
        [Header("Pull Settings")]
        [SerializeField] private float basePullForce = 500f;
        [SerializeField] private float maxRange = 30f;
        [SerializeField] private float minDistance = 1f;
        
        [Header("Weight Scaling")]
        [SerializeField] private float playerWeight = 80f;
        [SerializeField] private float forceMultiplier = 1f;

        public float CalculatePullForce(float targetMass, float distance, bool isAnchored)
        {
            if (isAnchored)
            {
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
