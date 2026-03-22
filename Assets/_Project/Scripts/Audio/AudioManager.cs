using UnityEngine;
using System.Collections.Generic;

namespace MistbornGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource voiceSource;
        
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;
        [Range(0f, 1f)] public float voiceVolume = 1f;
        
        [Header("Audio Categories")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Music")]
        [SerializeField] private AudioClip defaultMusic;
        [SerializeField] private AudioClip combatMusic;
        [SerializeField] private AudioClip stealthMusic;
        [SerializeField] private AudioClip bossMusic;
        
        [Header("Ambient")]
        [SerializeField] private AudioClip cityAmbient;
        [SerializeField] private AudioClip caveAmbient;
        [SerializeField] private AudioClip forestAmbient;
        
        [Header("Settings")]
        [SerializeField] private bool persistBetweenScenes = true;
        
        private Dictionary<string, AudioClip> audioLibrary = new Dictionary<string, AudioClip>();
        private AudioClip currentMusic;
        private bool isMuted = false;
        
        public static AudioManager instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                
                if (persistBetweenScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeAudioSources();
            LoadVolumeSettings();
        }
        
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
            
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.loop = false;
                voiceSource.playOnAwake = false;
            }
            
            UpdateVolumes();
        }
        
        private void Update()
        {
            UpdateVolumes();
        }
        
        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume * (isMuted ? 0 : 1);
            
            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume * (isMuted ? 0 : 1);
            
            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume * (isMuted ? 0 : 1);
            
            if (voiceSource != null)
                voiceSource.volume = voiceVolume * masterVolume * (isMuted ? 0 : 1);
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource == null)
                return;
            
            currentMusic = clip;
            musicSource.clip = clip;
            musicSource.Play();
        }
        
        public void PlayMusic(string musicId)
        {
            AudioClip clip = GetAudioClip(musicId);
            if (clip != null)
            {
                PlayMusic(clip);
            }
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                currentMusic = null;
            }
        }
        
        public void CrossFadeMusic(AudioClip newClip, float duration = 2f)
        {
            if (newClip == null || musicSource == null)
                return;
            
            StartCoroutine(CrossFadeCoroutine(newClip, duration));
        }
        
        private System.Collections.IEnumerator CrossFadeCoroutine(AudioClip newClip, float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / (duration / 2));
                yield return null;
            }
            
            musicSource.clip = newClip;
            musicSource.Play();
            
            elapsed = 0f;
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0, startVolume, elapsed / (duration / 2));
                yield return null;
            }
        }
        
        public void SetMusicForState(MusicState state)
        {
            switch (state)
            {
                case MusicState.Normal:
                    if (defaultMusic != null)
                        PlayMusic(defaultMusic);
                    break;
                case MusicState.Combat:
                    if (combatMusic != null)
                        CrossFadeMusic(combatMusic);
                    break;
                case MusicState.Stealth:
                    if (stealthMusic != null)
                        CrossFadeMusic(stealthMusic);
                    break;
                case MusicState.Boss:
                    if (bossMusic != null)
                        CrossFadeMusic(bossMusic);
                    break;
            }
        }
        
        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null || ambientSource == null)
                return;
            
            ambientSource.clip = clip;
            ambientSource.Play();
        }
        
        public void SetAmbientForLocation(LocationAmbient location)
        {
            switch (location)
            {
                case LocationAmbient.City:
                    if (cityAmbient != null)
                        PlayAmbient(cityAmbient);
                    break;
                case LocationAmbient.Cave:
                    if (caveAmbient != null)
                        PlayAmbient(caveAmbient);
                    break;
                case LocationAmbient.Forest:
                    if (forestAmbient != null)
                        PlayAmbient(forestAmbient);
                    break;
            }
        }
        
        public void StopAmbient()
        {
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }
        }
        
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
                return;
            
            sfxSource.PlayOneShot(clip);
        }
        
        public void PlaySFX(string sfxId)
        {
            AudioClip clip = GetAudioClip(sfxId);
            if (clip != null)
            {
                PlaySFX(clip);
            }
        }
        
        public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
        {
            if (clip == null)
                return;
            
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
        }
        
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null || voiceSource == null)
                return;
            
            voiceSource.clip = clip;
            voiceSource.Play();
        }
        
        public void StopVoice()
        {
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }
        }
        
        public AudioClip GetAudioClip(string clipId)
        {
            if (audioLibrary.ContainsKey(clipId))
            {
                return audioLibrary[clipId];
            }
            
            AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipId}");
            if (clip != null)
            {
                audioLibrary[clipId] = clip;
            }
            
            return clip;
        }
        
        public void AddToLibrary(string id, AudioClip clip)
        {
            if (!audioLibrary.ContainsKey(id))
            {
                audioLibrary[id] = clip;
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            UpdateVolumes();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            UpdateVolumes();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            UpdateVolumes();
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            UpdateVolumes();
        }
        
        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
            UpdateVolumes();
        }
        
        public void Mute()
        {
            isMuted = true;
            UpdateVolumes();
        }
        
        public void Unmute()
        {
            isMuted = false;
            UpdateVolumes();
        }
        
        public void ToggleMute()
        {
            isMuted = !isMuted;
            UpdateVolumes();
        }
        
        private void LoadVolumeSettings()
        {
            if (PlayerPrefs.HasKey("MasterVolume"))
                masterVolume = PlayerPrefs.GetFloat("MasterVolume");
            
            if (PlayerPrefs.HasKey("MusicVolume"))
                musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            
            if (PlayerPrefs.HasKey("SFXVolume"))
                sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            
            if (PlayerPrefs.HasKey("AmbientVolume"))
                ambientVolume = PlayerPrefs.GetFloat("AmbientVolume");
            
            if (PlayerPrefs.HasKey("VoiceVolume"))
                voiceVolume = PlayerPrefs.GetFloat("VoiceVolume");
            
            UpdateVolumes();
        }
        
        public AudioClip GetCurrentMusic()
        {
            return currentMusic;
        }
        
        public float GetMasterVolume()
        {
            return masterVolume;
        }
        
        public bool IsMuted()
        {
            return isMuted;
        }
    }
    
    public enum MusicState
    {
        Normal,
        Combat,
        Stealth,
        Boss
    }
    
    public enum LocationAmbient
    {
        City,
        Cave,
        Forest
    }
    
    [System.Serializable]
    public class AudioMixer
    {
        public void SetFloat(string name, float value)
        {
        }
        
        public float GetFloat(string name)
        {
            return 1f;
        }
    }
}
