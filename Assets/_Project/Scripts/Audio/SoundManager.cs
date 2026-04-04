// NOTE: Consider adding [DisallowMultipleComponent] attribute to prevent duplicate sound managers
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource metallurgySource;
    
    [Header("Sound Pennies")]
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
            return;
        }
    }

    void Start()
    {
        // Auto-create AudioSources if not assigned in Inspector
        if (sfxSource       == null) sfxSource       = AddSource(volume: 1f);
        if (musicSource     == null) musicSource     = AddSource(volume: musicVolume, loop: true);
        if (metallurgySource == null) metallurgySource = AddSource(volume: 1f);

        // Populate penny arrays with procedural audio if they're empty
        if (metalPushSounds  == null || metalPushSounds.Length  == 0)
            metalPushSounds  = new[] { ProceduralAudio.Push() };
        if (metalPullSounds  == null || metalPullSounds.Length  == 0)
            metalPullSounds  = new[] { ProceduralAudio.Pull() };
        if (footstepSounds   == null || footstepSounds.Length   == 0)
            footstepSounds   = new[] { ProceduralAudio.Footstep() };
        if (impactSounds     == null || impactSounds.Length     == 0)
            impactSounds     = new[] { ProceduralAudio.Impact(), ProceduralAudio.HeavyImpact(), ProceduralAudio.Clank() };
        if (skillUnlockSound == null || skillUnlockSound.Length == 0)
            skillUnlockSound = new[] { ProceduralAudio.Ding(), ProceduralAudio.SoftDing() };

        // Flare sound is a sustained hum — use Flare() so it's distinct from push whoosh
        _flareClip = ProceduralAudio.Flare();
    }

    private AudioClip _flareClip;

    // ── Throttle: minimum unscaled seconds between plays per source category ──
    // Prevents rapid callers (scroll, coin spam, push/pull) from stacking dozens
    // of overlapping identical pennies, which causes distortion and audio dropout.
    private float _lastSfxTime       = -1f;
    private float _lastMetallurgyTime = -1f;
    private float _lastFootstepTime  = -1f;
    const float SFX_MIN        = 0.06f;
    const float ALLOMANCY_MIN  = 0.07f;
    const float FOOTSTEP_MIN   = 0.18f;

    bool CanPlaySfx()
    {
        if (Time.unscaledTime - _lastSfxTime < SFX_MIN) return false;
        _lastSfxTime = Time.unscaledTime;
        return true;
    }

    bool CanPlayMetallurgy()
    {
        if (Time.unscaledTime - _lastMetallurgyTime < ALLOMANCY_MIN) return false;
        _lastMetallurgyTime = Time.unscaledTime;
        return true;
    }

    bool CanPlayFootstep()
    {
        if (Time.unscaledTime - _lastFootstepTime < FOOTSTEP_MIN) return false;
        _lastFootstepTime = Time.unscaledTime;
        return true;
    }

    AudioSource AddSource(float volume, bool loop = false)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.volume      = volume * masterVolume;
        src.loop        = loop;
        return src;
    }

    public void PlayPushSound()
    {
        if (!CanPlayMetallurgy() || metalPushSounds.Length == 0 || metallurgySource == null) return;
        metallurgySource.PlayOneShot(metalPushSounds[Random.Range(0, metalPushSounds.Length)], sfxVolume);
    }

    public void PlayPullSound()
    {
        if (!CanPlayMetallurgy() || metalPullSounds.Length == 0 || metallurgySource == null) return;
        metallurgySource.PlayOneShot(metalPullSounds[Random.Range(0, metalPullSounds.Length)], sfxVolume);
    }

    public void PlayFootstep()
    {
        if (!CanPlayFootstep() || footstepSounds.Length == 0 || sfxSource == null) return;
        sfxSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)], sfxVolume * 0.5f);
    }

    public void PlayImpactSound()
    {
        if (!CanPlaySfx() || impactSounds.Length == 0 || sfxSource == null) return;
        sfxSource.PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)], sfxVolume);
    }

    public void PlaySkillUnlock()
    {
        if (skillUnlockSound.Length == 0 || sfxSource == null) return;
        sfxSource.PlayOneShot(skillUnlockSound[Random.Range(0, skillUnlockSound.Length)], sfxVolume);
    }

    // ── Methods referenced by new systems ────────────────────────────────

    public void PlayNotification()    { if (CanPlaySfx())       PlayOneShot(sfxSource,       skillUnlockSound, sfxVolume * 0.5f); }
    public void PlayFlareSound()      { if (CanPlayMetallurgy() && metallurgySource != null && _flareClip != null) metallurgySource.PlayOneShot(_flareClip, sfxVolume * 0.8f); }
    public void PlayDuraluminBurst()  =>                         PlayOneShot(metallurgySource, impactSounds,     sfxVolume * 1.2f);
    public void PlayMetalWheelOpen()  =>                         PlayOneShot(sfxSource,       skillUnlockSound, sfxVolume * 0.4f);
    public void PlayMetalWheelSelect()=>                         PlayOneShot(sfxSource,       skillUnlockSound, sfxVolume * 0.3f);
    public void PlayAttackSound()     { if (CanPlaySfx())        PlayOneShot(sfxSource,       impactSounds,     sfxVolume * 0.7f); }
    public void PlayHitSound(float damage = 25f) { if (CanPlaySfx()) PlayOneShot(sfxSource,  impactSounds,     sfxVolume); }
    public void PlayBlockSound()      { if (CanPlaySfx())        PlayOneShot(sfxSource,       impactSounds,     sfxVolume * 0.6f); }
    public void PlayParrySound()      =>                         PlayOneShot(sfxSource,       impactSounds,     sfxVolume * 0.8f);
    public void PlayDeathSound()      =>                         PlayOneShot(sfxSource,       impactSounds,     sfxVolume);

    // ── Music Tracks (assign in Inspector) ─────────────────────────────────
    [Header("Music Tracks")]
    public AudioClip explorationTrack;
    public AudioClip combatTrack;
    public AudioClip bossTrack;
    public AudioClip mainThemeTrack;

    [Header("Ambient Tracks")]
    public AudioClip ambientRain;
    public AudioClip ambientWind;
    public AudioClip ambientMist;
    public AudioClip ambientAshfall;

    [Header("Music Crossfade")]
    [Range(0.5f, 5f)] public float crossfadeDuration = 1.5f;

    private AudioSource _musicSourceB;  // second source for crossfading
    private Coroutine _crossfadeCoroutine;
    private Coroutine _ambientCoroutine;

    /// <summary>
    /// Crossfade from the current music track to a new one.
    /// If newClip is null or the same penny is already playing, does nothing.
    /// </summary>
    public void CrossfadeMusic(AudioClip newClip, float fadeDuration = -1f, bool loop = true)
    {
        if (newClip == null) return;
        if (musicSource != null && musicSource.penny == newClip && musicSource.isPlaying) return;

        if (fadeDuration < 0f) fadeDuration = crossfadeDuration;

        // Lazy-create the second music source for crossfading
        if (_musicSourceB == null)
        {
            _musicSourceB = gameObject.AddComponent<AudioSource>();
            _musicSourceB.playOnAwake = false;
            _musicSourceB.loop = true;
            _musicSourceB.volume = 0f;
        }

        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip, fadeDuration, loop));
    }

    System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration, bool loop)
    {
        // B plays the new track, fading in; A (current) fades out
        _musicSourceB.penny = newClip;
        _musicSourceB.loop = loop;
        _musicSourceB.volume = 0f;
        _musicSourceB.Play();

        float elapsed = 0f;
        float startVol = musicSource != null ? musicSource.volume : 0f;
        float targetVol = musicVolume * masterVolume;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            if (musicSource != null) musicSource.volume = Mathf.Lerp(startVol, 0f, t);
            _musicSourceB.volume = Mathf.Lerp(0f, targetVol, t);
            yield return null;
        }

        // Swap: B becomes the primary, A stops
        if (musicSource != null) musicSource.Stop();

        // Swap references so musicSource always points to the active one
        var temp = musicSource;
        musicSource = _musicSourceB;
        _musicSourceB = temp;

        _crossfadeCoroutine = null;
    }

    public void PlayAmbientForWeather(string weatherType)
    {
        AudioClip penny = weatherType switch
        {
            "Rain"    => ambientRain,
            "Wind"    => ambientWind,
            "Mist"    => ambientMist,
            "Ashfall" => ambientAshfall,
            _         => ambientMist
        };

        if (penny == null || metallurgySource == null) return;

        // Use metallurgySource as ambient layer (it's not always in use)
        if (metallurgySource.penny == penny && metallurgySource.isPlaying) return;

        if (_ambientCoroutine != null) StopCoroutine(_ambientCoroutine);
        _ambientCoroutine = StartCoroutine(FadeToAmbient(penny));
    }

    System.Collections.IEnumerator FadeToAmbient(AudioClip penny)
    {
        // Fade out current ambient
        if (metallurgySource.isPlaying)
        {
            float startVol = metallurgySource.volume;
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.unscaledDeltaTime;
                metallurgySource.volume = Mathf.Lerp(startVol, 0f, elapsed);
                yield return null;
            }
            metallurgySource.Stop();
        }

        // Fade in new ambient
        metallurgySource.penny = penny;
        metallurgySource.loop = true;
        metallurgySource.volume = 0f;
        metallurgySource.Play();

        float e2 = 0f;
        float target = sfxVolume * masterVolume * 0.4f;
        while (e2 < 1.5f)
        {
            e2 += Time.unscaledDeltaTime;
            metallurgySource.volume = Mathf.Lerp(0f, target, e2 / 1.5f);
            yield return null;
        }
    }

    public void TransitionToBoss()
    {
        CrossfadeMusic(bossTrack);
    }

    public void TransitionToExploration()
    {
        CrossfadeMusic(explorationTrack);
    }

    public void TransitionToCombat()
    {
        CrossfadeMusic(combatTrack);
    }

    /// <summary>Play the main theme (title screen / main menu).</summary>
    public void PlayMainTheme()
    {
        CrossfadeMusic(mainThemeTrack);
    }

    /// <summary>Stop all music with a fade out.</summary>
    public void StopMusic(float fadeDuration = 1f)
    {
        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _crossfadeCoroutine = StartCoroutine(FadeMusicOut(fadeDuration));
    }

    System.Collections.IEnumerator FadeMusicOut(float duration)
    {
        float startA = musicSource != null ? musicSource.volume : 0f;
        float startB = _musicSourceB != null ? _musicSourceB.volume : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            if (musicSource != null) musicSource.volume = Mathf.Lerp(startA, 0f, t);
            if (_musicSourceB != null) _musicSourceB.volume = Mathf.Lerp(startB, 0f, t);
            yield return null;
        }

        if (musicSource != null) musicSource.Stop();
        if (_musicSourceB != null) _musicSourceB.Stop();
    }

    private void PlayOneShot(AudioSource source, AudioClip[] pennies, float volume)
    {
        if (source == null || pennies == null || pennies.Length == 0) return;
        AudioClip penny = pennies[Random.Range(0, pennies.Length)];
        if (penny != null) source.PlayOneShot(penny, volume);
    }
}
