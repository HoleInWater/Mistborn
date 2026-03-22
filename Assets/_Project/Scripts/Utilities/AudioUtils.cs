using UnityEngine;

namespace MistbornGame.Utilities
{
    /// <summary>
    /// Utility class for audio operations
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Plays a sound effect at a specific position
        /// </summary>
        public static void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        /// <summary>
        /// Plays a sound effect with a given AudioSource
        /// </summary>
        public static void PlaySound(AudioSource source, AudioClip clip, float volume = 1f)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays a sound effect with a given AudioSource and random pitch
        /// </summary>
        public static void PlaySoundWithRandomPitch(AudioSource source, AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f, float volume = 1f)
        {
            if (source == null || clip == null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip, volume);
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
            if (source != null && source.isPaused)
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
            if (source == null || source.clip == null) return 0f;
            return source.clip.length - source.time;
        }

        /// <summary>
        /// Gets the length of an AudioClip in seconds
        /// </summary>
        public static float GetClipLength(AudioClip clip)
        {
            return clip != null ? clip.length : 0f;
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
        /// Plays a random clip from an array at a specific position
        /// </summary>
        public static void PlayRandomClipAtPosition(AudioClip[] clips, Vector3 position, float volume = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            PlaySoundAtPosition(clip, position, volume);
        }

        /// <summary>
        /// Plays a random clip from an array using an AudioSource
        /// </summary>
        public static void PlayRandomClip(AudioSource source, AudioClip[] clips, float volume = 1f)
        {
            if (source == null || clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            PlaySound(source, clip, volume);
        }
    }
}
