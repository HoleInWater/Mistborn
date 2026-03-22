using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common physics operations
    /// </summary>
    public static class PhysicsExtension
    {
        /// <summary>
        /// Adds a force to a Rigidbody in the direction of a target
        /// </summary>
        public static void AddForceTowards(this Rigidbody rb, Transform target, float force)
        {
            if (rb == null || target == null) return;
            
            Vector3 direction = (target.position - rb.position).normalized;
            rb.AddForce(direction * force, ForceMode.Force);
        }

        /// <summary>
        /// Adds a force to a Rigidbody away from a target
        /// </summary>
        public static void AddForceAwayFrom(this Rigidbody rb, Transform target, float force)
        {
            if (rb == null || target == null) return;
            
            Vector3 direction = (rb.position - target.position).normalized;
            rb.AddForce(direction * force, ForceMode.Force);
        }

        /// <summary>
        /// Adds an upward force to a Rigidbody
        /// </summary>
        public static void AddUpwardForce(this Rigidbody rb, float force)
        {
            if (rb == null) return;
            rb.AddForce(Vector3.up * force, ForceMode.Force);
        }

        /// <summary>
        /// Adds a downward force to a Rigidbody
        /// </summary>
        public static void AddDownwardForce(this Rigidbody rb, float force)
        {
            if (rb == null) return;
            rb.AddForce(Vector3.down * force, ForceMode.Force);
        }

        /// <summary>
        /// Sets the velocity of a Rigidbody to move towards a target at a specific speed
        /// </summary>
        public static void SetVelocityTowards(this Rigidbody rb, Transform target, float speed)
        {
            if (rb == null || target == null) return;
            
            Vector3 direction = (target.position - rb.position).normalized;
            rb.velocity = direction * speed;
        }

        /// <summary>
        /// Applies an explosion force to a Rigidbody
        /// </summary>
        public static void AddExplosionForce(this Rigidbody rb, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f)
        {
            if (rb == null) return;
            rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier);
        }

        /// <summary>
        /// Checks if a Rigidbody is moving faster than a specified speed
        /// </summary>
        public static bool IsMovingFast(this Rigidbody rb, float speed)
        {
            if (rb == null) return false;
            return rb.velocity.magnitude > speed;
        }

        /// </// <summary>
        /// Checks if a Rigidbody is moving slower than a specified speed
        /// </summary>
        public static bool IsMovingSlow(this Rigidbody rb, float speed)
        {
            if (rb == null) return false;
            return rb.velocity.magnitude < speed;
        }

        /// <summary>
        /// Gets the speed of a Rigidbody
        /// </summary>
        public static float GetSpeed(this Rigidbody rb)
        {
            if (rb == null) return 0f;
            return rb.velocity.magnitude;
        }

        /// <summary>
        /// Resets the velocity and angular velocity of a Rigidbody
        /// </summary>
        public static void ResetVelocity(this Rigidbody rb)
        {
            if (rb == null) return;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Freezes a Rigidbody's position on specified axes
        /// </summary>
        public static void FreezePosition(this Rigidbody rb, bool x = true, bool y = true, bool z = true)
        {
            if (rb == null) return;
            
            RigidbodyConstraints constraints = rb.constraints;
            if (x) constraints |= RigidbodyConstraints.FreezePositionX;
            if (y) constraints |= RigidbodyConstraints.FreezePositionY;
            if (z) constraints |= RigidbodyConstraints.FreezePositionZ;
            
            rb.constraints = constraints;
        }

        /// <summary>
        /// Unfreezes a Rigidbody's position on specified axes
        /// </summary>
        public static void UnfreezePosition(this Rigidbody rb, bool x = true, bool y = true, bool z = true)
        {
            if (rb == null) return;
            
            RigidbodyConstraints constraints = rb.constraints;
            if (x) constraints &= ~RigidbodyConstraints.FreezePositionX;
            if (y) constraints &= ~RigidbodyConstraints.FreezePositionY;
            if (z) constraints &= ~RigidbodyConstraints.FreezePositionZ;
            
            rb.constraints = constraints;
        }

        /// <summary>
        /// Freezes a Rigidbody's rotation on specified axes
        /// </summary>
        public static void FreezeRotation(this Rigidbody rb, bool x = true, bool y = true, bool z = true)
        {
            if (rb == null) return;
            
            RigidbodyConstraints constraints = rb.constraints;
            if (x) constraints |= RigidbodyConstraints.FreezeRotationX;
            if (y) constraints |= RigidbodyConstraints.FreezeRotationY;
            if (z) constraints |= RigidbodyConstraints.FreezeRotationZ;
            
            rb.constraints = constraints;
        }

        /// <summary>
        /// Unfreezes a Rigidbody's rotation on specified axes
        /// </summary>
        public static void UnfreezeRotation(this Rigidbody rb, bool x = true, bool y = true, bool z = true)
        {
            if (rb == null) return;
            
            RigidbodyConstraints constraints = rb.constraints;
            if (x) constraints &= ~RigidbodyConstraints.FreezeRotationX;
            if (y) constraints &= ~RigidbodyConstraints.FreezeRotationY;
            if (z) constraints &= ~RigidbodyConstraints.FreezeRotationZ;
            
            rb.constraints = constraints;
        }

        /// <summary>
        /// Applies a force to avoid overlapping with another collider
        /// </summary>
        public static void ApplySeparationForce(this Rigidbody rb, Collider otherCollider, float separationDistance = 0.1f, float forceAmount = 10f)
        {
            if (rb == null || otherCollider == null) return;
                
            Vector3 closestPoint = otherCollider.ClosestPoint(rb.position);
            Vector3 direction = rb.position - closestPoint;
            float distance = direction.magnitude;
            
            if (distance < separationDistance && distance > 0f)
            {
                Vector3 forceDirection = direction.normalized;
                rb.AddForce(forceDirection * forceAmount, ForceMode.Force);
            }
        }

        /// <summary>
        /// Adds a force in a specified direction
        /// </summary>
        public static void AddForceInDirection(this Rigidbody rb, Vector3 direction, float force, ForceMode mode = ForceMode.Force)
        {
            if (rb == null) return;
            rb.AddForce(direction.normalized * force, mode);
        }

        /// <summary>
        /// Sets the velocity in a specified direction
        /// </summary>
        public static void SetVelocityInDirection(this Rigidbody rb, Vector3 direction, float speed)
        {
            if (rb == null) return;
            rb.velocity = direction.normalized * speed;
        }

        /// <summary>
        /// Adds an impulse force at a position (torque)
        /// </summary>
        public static void AddTorqueAtPosition(this Rigidbody rb, Vector3 force, Vector3 position)
        {
            if (rb == null) return;
            rb.AddForceAtPosition(force, position, ForceMode.Impulse);
        }

        /// <summary>
        /// Checks if a Rigidbody is grounded (simple check using raycast)
        /// </summary>
        public static bool IsGrounded(this Rigidbody rb, float checkDistance = 0.1f, LayerMask? groundLayer = null)
        {
            if (rb == null) return false;
            float radius = rb.GetComponent<Collider>().bounds.extents.y;
            Vector3 origin = rb.position + Vector3.up * radius;
            float distance = radius + checkDistance;
            LayerMask mask = groundLayer ?? Physics.DefaultRaycastLayers;
            return Physics.Raycast(origin, Vector3.down, distance, mask);
        }

        /// <summary>
        /// Gets the current momentum of a Rigidbody
        /// </summary>
        public static Vector3 GetMomentum(this Rigidbody rb)
        {
            if (rb == null) return Vector3.zero;
            return rb.velocity * rb.mass;
        }

        /// <summary>
        /// Applies a force to achieve a target velocity
        /// </summary>
        public static void AddForceToReachVelocity(this Rigidbody rb, Vector3 targetVelocity, float forceStrength)
        {
            if (rb == null) return;
            Vector3 velocityDiff = targetVelocity - rb.velocity;
            rb.AddForce(velocityDiff * forceStrength, ForceMode.Force);
        }
    }
}