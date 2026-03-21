using UnityEngine;
using System.Collections.Generic;
using Mistborn.Allomancy;

namespace Mistborn.Audio
{
    public class AllomancyAudioManager : MonoBehaviour
    {
        public static AllomancyAudioManager Instance { get; private set; }

        [System.Serializable]
        public class SoundEntry
        {
            public string soundId;
            public AudioClip[] clips;
            public float volume = 1f;
            public float pitchVariance = 0.1f;
        }

        [Header("Metal Burns")]
        [SerializeField] private AudioClip m_steelPushSound;
        [SerializeField] private AudioClip m_ironPullSound;
        [SerializeField] private AudioClip m_pewterBurnSound;
        [SerializeField] private AudioClip m_tinBurnSound;
        [SerializeField] private AudioClip m_copperBurnSound;
        [SerializeField] private AudioClip m_bronzeBurnSound;

        [Header("Combat")]
        [SerializeField] private AudioClip[] m_meleeHitSounds;
        [SerializeField] private AudioClip[] m_playerHurtSounds;
        [SerializeField] private AudioClip[] m_enemyDeathSounds;
        [SerializeField] private AudioClip[] m_blockSounds;

        [Header("Ambient")]
        [SerializeField] private AudioClip m_mistAmbient;
        [SerializeField] private AudioClip m_windHowl;
        [SerializeField] private AudioClip m_ashFall;

        [Header("Metal Pickups")]
        [SerializeField] private AudioClip m_coinPickup;
        [SerializeField] private AudioClip m_metalPickup;
        [SerializeField] private AudioSource m_oneshotSource;

        private Dictionary<string, AudioSource> m_loopingSources = new Dictionary<string, AudioSource>();
        private AudioSource m_ambientSource;
        private AudioSource m_musicSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupAmbientSource();
        }

        private void SetupAmbientSource()
        {
            m_ambientSource = gameObject.AddComponent<AudioSource>();
            m_ambientSource.loop = true;
            m_ambientSource.volume = 0.3f;
            m_ambientSource.playOnAwake = false;

            m_musicSource = gameObject.AddComponent<AudioSource>();
            m_musicSource.loop = true;
            m_musicSource.volume = 0.5f;
            m_musicSource.playOnAwake = false;
        }

        public void PlaySteelPush(Vector3 position)
        {
            PlaySoundAtPosition(m_steelPushSound, position, 0.5f);
        }

        public void PlayIronPull(Vector3 position)
        {
            PlaySoundAtPosition(m_ironPullSound, position, 0.5f);
        }

        public void PlayMetalBurn(AllomanticMetal metal)
        {
            AudioClip clip = metal switch
            {
                AllomanticMetal.Steel => m_steelPushSound,
                AllomanticMetal.Iron => m_ironPullSound,
                AllomanticMetal.Pewter => m_pewterBurnSound,
                AllomanticMetal.Tin => m_tinBurnSound,
                AllomanticMetal.Copper => m_copperBurnSound,
                AllomanticMetal.Bronze => m_bronzeBurnSound,
                _ => null
            };

            if (clip != null)
                PlaySound(clip, 0.3f);
        }

        public void PlayMeleeHit()
        {
            PlayRandomSound(m_meleeHitSounds, 0.7f);
        }

        public void PlayPlayerHurt()
        {
            PlayRandomSound(m_playerHurtSounds, 0.6f);
        }

        public void PlayEnemyDeath()
        {
            PlayRandomSound(m_enemyDeathSounds, 0.8f);
        }

        public void PlayBlock()
        {
            PlayRandomSound(m_blockSounds, 0.5f);
        }

        public void PlayCoinPickup(Vector3 position)
        {
            PlaySoundAtPosition(m_coinPickup, position, 0.4f);
        }

        public void PlayMetalPickup(Vector3 position)
        {
            PlaySoundAtPosition(m_metalPickup, position, 0.5f);
        }

        public void StartMistAmbient()
        {
            if (m_ambientSource != null && m_mistAmbient != null)
            {
                m_ambientSource.clip = m_mistAmbient;
                m_ambientSource.Play();
            }
        }

        public void StopMistAmbient()
        {
            m_ambientSource?.Stop();
        }

        public void PlayMusic(AudioClip music)
        {
            if (m_musicSource != null && music != null)
            {
                m_musicSource.clip = music;
                m_musicSource.Play();
            }
        }

        public void StopMusic()
        {
            m_musicSource?.Stop();
        }

        public void SetAmbientVolume(float volume)
        {
            if (m_ambientSource != null)
                m_ambientSource.volume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            if (m_musicSource != null)
                m_musicSource.volume = Mathf.Clamp01(volume);
        }

        public void FadeOutAmbient(float duration = 2f)
        {
            StartCoroutine(FadeAudio(m_ambientSource, 0f, duration));
        }

        public void FadeInAmbient(float duration = 2f)
        {
            m_ambientSource.volume = 0f;
            StartCoroutine(FadeAudio(m_ambientSource, 0.3f, duration));
        }

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = m_oneshotSource;
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            source.PlayOneShot(clip, volume);
        }

        private void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = new GameObject("Sound").AddComponent<AudioSource>();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 1f;
            source.maxDistance = 25f;
            source.Play();
            Destroy(source.gameObject, clip.length + 0.5f);
        }

        private void PlayRandomSound(AudioClip[] clips, float volume = 1f)
        {
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            PlaySound(clip, volume);
        }

        private System.Collections.IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            source.volume = targetVolume;
        }

        public AudioSource CreateLoop(string loopId, AudioClip clip, float volume = 1f)
        {
            if (m_loopingSources.ContainsKey(loopId))
            {
                m_loopingSources[loopId].Stop();
                Destroy(m_loopingSources[loopId].gameObject);
            }

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = volume;
            source.Play();

            m_loopingSources[loopId] = source;
            return source;
        }

        public void StopLoop(string loopId)
        {
            if (m_loopingSources.TryGetValue(loopId, out AudioSource source))
            {
                source.Stop();
                Destroy(source);
                m_loopingSources.Remove(loopId);
            }
        }

        public void SetLoopVolume(string loopId, float volume)
        {
            if (m_loopingSources.TryGetValue(loopId, out AudioSource source))
            {
                source.volume = volume;
            }
        }
    }
}
