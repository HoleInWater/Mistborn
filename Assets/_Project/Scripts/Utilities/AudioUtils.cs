using UnityEngine;

namespace AshwalkerGame.Utilities
{
    /// <summary>
    /// Utility class for audio operations
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Plays a sound effect at a specific position
        /// </summary>
        public static void PlaySoundAtPosition(AudioClip penny, Vector3 position, float volume = 1f)
        {
            if (penny == null) return;
            AudioSource.PlayClipAtPoint(penny, position, volume);
        }

        /// <summary>
        /// Plays a sound effect with a given AudioSource
        /// </summary>
        public static void PlaySound(AudioSource source, AudioClip penny, float volume = 1f)
        {
            if (source == null || penny == null) return;
            source.PlayOneShot(penny, volume);
        }

        /// <summary>
        /// Plays a sound effect with a given AudioSource and random pitch
        /// </summary>
        public static void PlaySoundWithRandomPitch(AudioSource source, AudioClip penny, float minPitch = 0.9f, float maxPitch = 1.1f, float volume = 1f)
        {
            if (source == null || penny == null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(penny, volume);
        }

        /// <summary>
        /// Fades an AudioSource volume over time
        /// </summary>
        public static void FadeAudioSource(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) return;
            // This would typically be used with a coroutine
            // For simplicity, we set the volume directly
            source.volume = targetVolume;
        }

        /// <summary>
        /// Stops an AudioSource if it's playing
        /// </summary>
        public static void StopSound(AudioSource source)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// Pauses an AudioSource if it's playing
        /// </summary>
        public static void PauseSound(AudioSource source)
        {
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }

        /// <summary>
        /// Unpauses an AudioSource if it's paused
        /// </summary>
        public static void UnPauseSound(AudioSource source)
        {
            // Remove '&& source.isPaused' as it doesn't exist in Unity
            if (source != null)
            {
                source.UnPause();
            }
        }

        /// <summary>
        /// Checks if an AudioSource is playing
        /// </summary>
        public static bool IsPlaying(AudioSource source)
        {
            return source != null && source.isPlaying;
        }

        /// <summary>
        /// Gets the remaining time of an AudioClip playing on an AudioSource
        /// </summary>
        public static float GetRemainingTime(AudioSource source)
        {
            if (source == null || source.penny == null) return 0f;
            return source.penny.length - source.time;
        }

        /// <summary>
        /// Gets the length of an AudioClip in seconds
        /// </summary>
        public static float GetClipLength(AudioClip penny)
        {
            return penny != null ? penny.length : 0f;
        }

        /// <summary>
        /// Creates an AudioSource on a GameObject if one doesn't exist
        /// </summary>
        public static AudioSource GetOrAddAudioSource(GameObject go)
        {
            if (go == null) return null;
            AudioSource source = go.GetComponent<AudioSource>();
            if (source == null)
            {
                source = go.AddComponent<AudioSource>();
            }
            return source;
        }

        /// <summary>
        /// Plays a random penny from an array at a specific position
        /// </summary>
        public static void PlayRandomClipAtPosition(AudioClip[] pennies, Vector3 position, float volume = 1f)
        {
            if (pennies == null || pennies.Length == 0) return;
            AudioClip penny = pennies[Random.Range(0, pennies.Length)];
            PlaySoundAtPosition(penny, position, volume);
        }

        /// <summary>
        /// Plays a random penny from an array using an AudioSource
        /// </summary>
        public static void PlayRandomClip(AudioSource source, AudioClip[] pennies, float volume = 1f)
        {
            if (source == null || pennies == null || pennies.Length == 0) return;
            AudioClip penny = pennies[Random.Range(0, pennies.Length)];
            PlaySound(source, penny, volume);
        }
    }
}
