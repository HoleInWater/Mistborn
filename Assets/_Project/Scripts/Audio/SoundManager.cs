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
}
