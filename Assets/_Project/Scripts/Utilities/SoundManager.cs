// ============================================================
// FILE: SoundManager.cs
// SYSTEM: Utilities
// STATUS: READY TO USE
// AUTHOR: 
//
// PURPOSE:
//   Central audio manager for all game sounds.
//   Handles SFX, music, and ambient audio.
//
// TODO:
//   - Add actual audio files
//   - Set up audio mixer
//   - Add 3D spatial audio for some sounds
//
// LAST UPDATED: 2026-03-20
// ============================================================

using UnityEngine;
using System.Collections.Generic;

namespace Mistborn.Utilities
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        
        [Header("Audio Sources")]
        public AudioSource sfxSource;
        public AudioSource musicSource;
        public AudioSource ambientSource;
        
        [Header("Audio Clips - Allomancy")]
        public AudioClip steelPushSound;
        public AudioClip ironPullSound;
        public AudioClip metalDepletedSound;
        public AudioClip blueLineActivateSound;
        
        [Header("Audio Clips - Combat")]
        public AudioClip playerHitSound;
        public AudioClip enemyHitSound;
        public AudioClip coinThrowSound;
        public AudioClip metalClangSound;
        
        [Header("Audio Clips - UI")]
        public AudioClip menuSelectSound;
        public AudioClip menuConfirmSound;
        public AudioClip saveLoadSound;
        
        [Header("Audio Clips - Ambient")]
        public AudioClip ashWindAmbient;
        public AudioClip cityAmbient;
        
        [Header("Settings")]
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 0.7f;
        
        private Dictionary<string, AudioClip> soundLibrary;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                BuildSoundLibrary();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeAudioSources()
        {
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
            }
            
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("Music Source");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("Ambient Source");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
            }
        }
        
        private void BuildSoundLibrary()
        {
            soundLibrary = new Dictionary<string, AudioClip>
            {
                { "steelPush", steelPushSound },
                { "ironPull", ironPullSound },
                { "metalDepleted", metalDepletedSound },
                { "blueLineActivate", blueLineActivateSound },
                { "playerHit", playerHitSound },
                { "enemyHit", enemyHitSound },
                { "coinThrow", coinThrowSound },
                { "metalClang", metalClangSound },
                { "menuSelect", menuSelectSound },
                { "menuConfirm", menuConfirmSound },
                { "saveLoad", saveLoadSound }
            };
        }
        
        public void PlaySound(string soundName)
        {
            if (soundLibrary.ContainsKey(soundName) && soundLibrary[soundName] != null)
            {
                sfxSource.PlayOneShot(soundLibrary[soundName], sfxVolume * masterVolume);
            }
        }
        
        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
            }
        }
        
        public void PlayMusic(AudioClip clip, float fadeIn = 0f)
        {
            if (clip == null) return;
            
            musicSource.clip = clip;
            musicSource.volume = 0;
            musicSource.Play();
            
            if (fadeIn > 0)
            {
                StartCoroutine(FadeInMusic(fadeIn));
            }
            else
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }
        
        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;
            
            ambientSource.clip = clip;
            ambientSource.volume = masterVolume * 0.5f;
            ambientSource.Play();
        }
        
        public void StopMusic(float fadeOut = 0f)
        {
            if (fadeOut > 0)
            {
                StartCoroutine(FadeOutMusic(fadeOut));
            }
            else
            {
                musicSource.Stop();
            }
        }
        
        private System.Collections.IEnumerator FadeInMusic(float duration)
        {
            float targetVolume = musicVolume * masterVolume;
            float startVolume = 0;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }
            
            musicSource.volume = targetVolume;
        }
        
        private System.Collections.IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
                yield return null;
            }
            
            musicSource.Stop();
        }
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume;
            ambientSource.volume = masterVolume * 0.5f;
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume;
        }
    }
}
