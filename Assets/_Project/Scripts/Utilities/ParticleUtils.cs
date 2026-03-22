using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for handling common particle system operations
    /// </summary>
    public class ParticleUtils : MonoBehaviour
    {
        /// <summary>
        /// Spawns a particle system at a position and destroys it after its duration
        /// </summary>
        public static ParticleSystem SpawnAtPoint(ParticleSystem prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            
            ParticleSystem instance = Object.Instantiate(prefab, position, rotation);
            instance.Play();
            
            // Destroy after duration
            Object.Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
            return instance;
        }

        /// <summary>
        /// Spawns a particle system at a position and destroys it after its duration
        /// </summary>
        public static ParticleSystem SpawnAtPoint(ParticleSystem prefab, Vector3 position)
        {
            return SpawnAtPoint(prefab, position, Quaternion.identity);
        }

        /// <summary>
        /// Spawns a particle system at a transform's position and destroys it after its duration
        /// </summary>
        public static ParticleSystem SpawnAtTransform(ParticleSystem prefab, Transform transform)
        {
            if (transform == null) return null;
            return SpawnAtPoint(prefab, transform.position, transform.rotation);
        }

        /// <summary>
        /// Spawns a particle system and parents it to a transform
        /// </summary>
        public static ParticleSystem SpawnAttached(ParticleSystem prefab, Transform parent, bool worldPositionStays = true)
        {
            if (prefab == null) return null;
            
            ParticleSystem instance = Object.Instantiate(prefab, parent, worldPositionStays);
            instance.Play();
            
            // Destroy after duration
            Object.Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
            return instance;
        }

        /// <summary>
        /// Changes the color of all particles in a particle system
        /// </summary>
        public static void SetParticleColor(ParticleSystem particleSystem, Color color)
        {
            if (particleSystem == null) return;
            
            var main = particleSystem.main;
            main.startColor = color;
        }

        /// <summary>
        /// Changes the size of all particles in a particle system
        /// </summary>
        public static void SetParticleSize(ParticleSystem particleSystem, float size)
        {
            if (particleSystem == null) return;
            
            var main = particleSystem.main;
            main.startSize = size;
        }

        /// <summary>
        /// Emits a burst of particles from a particle system
        /// </summary>
        public static void Emit(ParticleSystem particleSystem, int count)
        {
            if (particleSystem == null) return;
            particleSystem.Emit(count);
        }

        /// <summary>
        /// Stops a particle system and clears its particles
        /// </summary>
        public static void StopAndClear(ParticleSystem particleSystem)
        {
            if (particleSystem == null) return;
            particleSystem.Stop();
            particleSystem.Clear();
        }

        /// <summary>
        /// Checks if a particle system is currently playing
        /// </summary>
        public static bool IsPlaying(ParticleSystem particleSystem)
        {
            return particleSystem != null && particleSystem.IsPlaying();
        }

        /// <summary>
        /// Gets the total emission rate of a particle system
        /// </summary>
        public static float GetEmissionRate(ParticleSystem particleSystem)
        {
            if (particleSystem == null) return 0f;
            
            var emission = particleSystem.emission;
            if (emission.enabled)
            {
                return emission.rateOverTime.constant;
            }
            return 0f;
        }

        /// <summary>
        /// Sets the emission rate of a particle system
        /// </summary>
        public static void SetEmissionRate(ParticleSystem particleSystem, float rate)
        {
            if (particleSystem == null) return;
            
            var emission = particleSystem.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(rate);
        }

        /// <summary>
        /// Creates a particle system from a prefab and returns it without playing
        /// </summary>
        public static ParticleSystem InstantiateParticleSystem(ParticleSystem prefab, Transform parent = null, bool worldPositionStays = true)
        {
            if (prefab == null) return null;
            
            return Object.Instantiate(prefab, parent, worldPositionStays);
        }
    }
}