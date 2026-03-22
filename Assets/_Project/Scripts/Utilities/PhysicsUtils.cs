using UnityEngine;

namespace MistbornGame.Utilities
{
    public static class PhysicsUtils
    {
        /// <summary>
        /// Applies an explosion force to all rigidbodies in a radius
        /// </summary>
        public static void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Impulse)
        {
            Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, mode);
                }
            }
        }

        /// <summary>
        /// Checks if there's clear line of sight between two points
        /// </summary>
        public static bool HasLineOfSight(Vector3 from, Vector3 to, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            return !Physics.Raycast(from, (to - from).normalized, out RaycastHit hit, maxDistance, layerMask) || hit.distance >= Vector3.Distance(from, to);
        }

        /// <summary>
        /// Casts a ray and returns the first hit, or null if nothing was hit
        /// </summary>
        public static RaycastHit? Raycast(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, layerMask))
            {
                return hit;
            }
            return null;
        }

        /// <summary>
        /// Casts a ray and returns all hits, sorted by distance
        /// </summary>
        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, layerMask);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            return hits;
        }

        /// <summary>
        /// Applies a force to avoid overlapping with another collider
        /// </summary>
        public static void ApplySeparationForce(Rigidbody rb, Collider otherCollider, float separationDistance = 0.1f, float forceAmount = 10f)
        {
            if (rb == null || otherCollider == null)
                return;
                
            Vector3 closestPoint = otherCollider.ClosestPoint(rb.position);
            Vector3 direction = rb.position - closestPoint;
            float distance = direction.magnitude;
            
            if (distance < separationDistance && distance > 0f)
            {
                Vector3 forceDirection = direction.normalized;
                rb.AddForce(forceDirection * forceAmount, ForceMode.Force);
            }
        }
    }
}