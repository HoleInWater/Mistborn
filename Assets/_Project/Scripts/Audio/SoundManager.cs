// NOTE: Consider adding [DisallowMultipleComponent] attribute to prevent duplicate sound managers
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource allomancySource;
    
    [Header("Sound Clips")]
    public AudioClip[] metalPushSounds;
    public AudioClip[] metalPullSounds;
    public AudioClip[] footstepSounds;
    public AudioClip[] impactSounds;
    public AudioClip[] skillUnlockSound;
    
    [Header("Settings")]
    // NOTE: Consider adding [Range(0f, 1f)] attribute for masterVolume
    public float masterVolume = 1f;
    // NOTE: Consider adding [Range(0f, 1f)] attribute for sfxVolume
    public float sfxVolume = 1f;
    // NOTE: Consider adding [Range(0f, 1f)] attribute for musicVolume
    public float musicVolume = 0.5f;
    
    public static SoundManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayPushSound()
    {
        if (metalPushSounds.Length > 0 && allomancySource != null)
        {
            AudioClip clip = metalPushSounds[Random.Range(0, metalPushSounds.Length)];
            allomancySource.PlayOneShot(clip, sfxVolume);
        }
    }
    
    public void PlayPullSound()
    {
        if (metalPullSounds.Length > 0 && allomancySource != null)
        {
            AudioClip clip = metalPullSounds[Random.Range(0, metalPullSounds.Length)];
            allomancySource.PlayOneShot(clip, sfxVolume);
        }
    }
    
    public void PlayFootstep()
    {
        if (footstepSounds.Length > 0 && sfxSource != null)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            sfxSource.PlayOneShot(clip, sfxVolume * 0.5f);
        }
    }
    
    public void PlayImpactSound()
    {
        if (impactSounds.Length > 0 && sfxSource != null)
        {
            AudioClip clip = impactSounds[Random.Range(0, impactSounds.Length)];
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
    
    public void PlaySkillUnlock()
    {
        if (skillUnlockSound.Length > 0 && sfxSource != null)
        {
            AudioClip clip = skillUnlockSound[0];
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // ── Methods referenced by new systems ────────────────────────────────

    public void PlayNotification() => PlayOneShot(sfxSource, skillUnlockSound, sfxVolume * 0.5f);
    public void PlayFlareSound() => PlayOneShot(allomancySource, metalPushSounds, sfxVolume * 0.8f);
    public void PlayDuraluminBurst() => PlayOneShot(allomancySource, impactSounds, sfxVolume * 1.2f);
    public void PlayMetalWheelOpen() => PlayOneShot(sfxSource, skillUnlockSound, sfxVolume * 0.4f);
    public void PlayMetalWheelSelect() => PlayOneShot(sfxSource, skillUnlockSound, sfxVolume * 0.3f);
    public void PlayAttackSound() => PlayOneShot(sfxSource, impactSounds, sfxVolume * 0.7f);
    public void PlayHitSound(float damage = 25f) => PlayOneShot(sfxSource, impactSounds, sfxVolume);
    public void PlayBlockSound() => PlayOneShot(sfxSource, impactSounds, sfxVolume * 0.6f);
    public void PlayParrySound() => PlayOneShot(sfxSource, impactSounds, sfxVolume * 0.8f);
    public void PlayDeathSound() => PlayOneShot(sfxSource, impactSounds, sfxVolume);

    public void PlayAmbientForWeather(string weatherType)
    {
        // Placeholder — plays footstep as ambient until proper weather clips are assigned
    }

    public void TransitionToBoss()
    {
        // Placeholder — would crossfade to boss music track
    }

    public void TransitionToExploration() { }
    public void TransitionToCombat() { }

<<<<<<< HEAD
=======
    /// <summary>Apply music volume to the music AudioSource.</summary>
    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        if (musicSource != null) musicSource.volume = v;
    }

    /// <summary>Apply SFX volume; individual PlayOneShot calls already use sfxVolume.</summary>
    public void SetSFXVolume(float v)
    {
        sfxVolume = v;
    }

>>>>>>> 7daa366c60caed24ce0c1046ca4c50300c733d1a
    private void PlayOneShot(AudioSource source, AudioClip[] clips, float volume)
    {
        if (source == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null) source.PlayOneShot(clip, volume);
    }
}
