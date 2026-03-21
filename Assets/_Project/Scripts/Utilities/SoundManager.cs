using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Utilities
{
    /// <summary>
    /// Central audio manager for SFX, music, and ambient audio.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource m_sfxSource;
        [SerializeField] private AudioSource m_musicSource;
        [SerializeField] private AudioSource m_ambientSource;
        
        [Header("Allomancy Sounds")]
        [SerializeField] private AudioClip m_steelPushSound;
        [SerializeField] private AudioClip m_ironPullSound;
        [SerializeField] private AudioClip m_metalDepletedSound;
        
        [Header("Combat Sounds")]
        [SerializeField] private AudioClip m_playerHitSound;
        [SerializeField] private AudioClip m_enemyHitSound;
        [SerializeField] private AudioClip m_coinThrowSound;
        
        [Header("UI Sounds")]
        [SerializeField] private AudioClip m_menuSelectSound;
        [SerializeField] private AudioClip m_menuConfirmSound;
        
        [Header("Ambient")]
        [SerializeField] private AudioClip m_ashWindAmbient;
        
        [Header("Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float m_masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float m_sfxVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float m_musicVolume = 0.7f;

        private Dictionary<string, AudioClip> m_soundLibrary;

        public float masterVolume => m_masterVolume;
        public float sfxVolume => m_sfxVolume;
        public float musicVolume => m_musicVolume;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupAudioSources();
                BuildSoundLibrary();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupAudioSources()
        {
            m_sfxSource = CreateAudioSource("SFX Source", false);
            m_musicSource = CreateAudioSource("Music Source", true);
            m_ambientSource = CreateAudioSource("Ambient Source", true);
        }

        private AudioSource CreateAudioSource(string name, bool loop)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.loop = loop;
            return source;
        }

        private void BuildSoundLibrary()
        {
            m_soundLibrary = new Dictionary<string, AudioClip>
            {
                { "steelPush", m_steelPushSound },
                { "ironPull", m_ironPullSound },
                { "metalDepleted", m_metalDepletedSound },
                { "playerHit", m_playerHitSound },
                { "enemyHit", m_enemyHitSound },
                { "coinThrow", m_coinThrowSound },
                { "menuSelect", m_menuSelectSound },
                { "menuConfirm", m_menuConfirmSound }
            };
        }

        public void PlaySound(string soundName)
        {
            if (m_soundLibrary.TryGetValue(soundName, out AudioClip clip) && clip != null)
            {
                m_sfxSource.PlayOneShot(clip, m_sfxVolume * m_masterVolume);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                m_sfxSource.PlayOneShot(clip, m_sfxVolume * m_masterVolume);
            }
        }

        public void PlayMusic(AudioClip clip, float fadeIn = 0f)
        {
            if (clip == null) return;
            
            m_musicSource.clip = clip;
            m_musicSource.volume = 0;
            m_musicSource.Play();
            
            if (fadeIn > 0)
            {
                StartCoroutine(FadeInRoutine(m_musicSource, fadeIn, m_musicVolume * m_masterVolume));
            }
            else
            {
                m_musicSource.volume = m_musicVolume * m_masterVolume;
            }
        }

        public void StopMusic(float fadeOut = 0f)
        {
            if (fadeOut > 0)
            {
                StartCoroutine(FadeOutRoutine(m_musicSource, fadeOut));
            }
            else
            {
                m_musicSource.Stop();
            }
        }

        private System.Collections.IEnumerator FadeInRoutine(AudioSource source, float duration, float targetVolume)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            source.volume = targetVolume;
        }

        private System.Collections.IEnumerator FadeOutRoutine(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            source.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            m_masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            m_sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            m_musicVolume = Mathf.Clamp01(volume);
            m_musicSource.volume = m_musicVolume * m_masterVolume;
        }

        private void UpdateVolumes()
        {
            m_musicSource.volume = m_musicVolume * m_masterVolume;
            m_ambientSource.volume = m_masterVolume * 0.5f;
        }
    }
}
