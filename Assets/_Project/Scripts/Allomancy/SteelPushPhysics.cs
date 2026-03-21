using UnityEngine;

namespace Mistborn.Allomancy
{
    public class SteelPushPhysics : MonoBehaviour
    {
        [Header("Push Settings")]
        [SerializeField] private float basePushForce = 500f;
        [SerializeField] private float maxRange = 30f;
        [SerializeField] private float minDistance = 1f;
        
        [Header("Weight Scaling")]
        [SerializeField] private float playerWeight = 80f;
        [SerializeField] private float forceMultiplier = 1f;
        
        [Header("Distance Falloff")]
        [SerializeField] private bool useDistanceFalloff = true;
        [SerializeField] private float falloffStartDistance = 5f;
        
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
                return basePushForce * forceMultiplier;
            }
            
            float massRatio = playerWeight / Mathf.Max(target.mass, 1f);
            float distanceFactor = 1f;
            
            if (useDistanceFalloff && target.distance > falloffStartDistance)
            {
                distanceFactor = falloffStartDistance / target.distance;
            }
            
            return basePushForce * massRatio * distanceFactor * forceMultiplier;
        }
        
        public float CalculateRecoilForce(PushTarget target)
        {
            float massRatio = target.mass / playerWeight;
            return basePushForce * massRatio * 0.5f;
        }
    }
}
