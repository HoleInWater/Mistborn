using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central audio manager for the Mistborn game.
/// Handles SFX, music, ambient, Allomancy sounds, and UI audio.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    public static SoundManager Instance { get; private set; }

    // ── Audio Sources ────────────────────────────────────────────────────
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource allomancySource;
    public AudioSource ambientSource;
    public AudioSource uiSource;

    // ── Allomancy Sound Clips ────────────────────────────────────────────
    [Header("Allomancy Sounds")]
    public AudioClip[] metalPushSounds;
    public AudioClip[] metalPullSounds;
    public AudioClip[] metalBurnStartSounds;
    public AudioClip[] metalBurnLoopSounds;
    public AudioClip[] flareSounds;
    public AudioClip[] duraluminBurstSounds;
    public AudioClip[] timeBubbleSounds;
    public AudioClip[] atiumActivateSounds;
    public AudioClip[] copperCloudSounds;
    public AudioClip[] bronzeSeekSounds;

    // ── Combat Sound Clips ───────────────────────────────────────────────
    [Header("Combat Sounds")]
    public AudioClip[] swordSwingSounds;
    public AudioClip[] swordHitSounds;
    public AudioClip[] blockSounds;
    public AudioClip[] parrySounds;
    public AudioClip[] dodgeSounds;
    public AudioClip[] impactSounds;
    public AudioClip[] coinHitSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] criticalHitSounds;

    // ── Movement Sound Clips ─────────────────────────────────────────────
    [Header("Movement Sounds")]
    public AudioClip[] footstepSounds;
    public AudioClip[] footstepMetalSounds;
    public AudioClip[] landingSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] slideSounds;
    public AudioClip[] wallRunSounds;
    public AudioClip[] vaultSounds;
    public AudioClip[] grappleSounds;

    // ── Environmental Sound Clips ────────────────────────────────────────
    [Header("Environmental Sounds")]
    public AudioClip[] mistAmbienceSounds;
    public AudioClip[] ashFallSounds;
    public AudioClip[] windSounds;
    public AudioClip[] rainSounds;
    public AudioClip[] thunderSounds;
    public AudioClip[] fireAmbienceSounds;

    // ── UI Sound Clips ───────────────────────────────────────────────────
    [Header("UI Sounds")]
    public AudioClip[] menuClickSounds;
    public AudioClip[] menuHoverSounds;
    public AudioClip[] skillUnlockSounds;
    public AudioClip[] questStartSounds;
    public AudioClip[] questCompleteSounds;
    public AudioClip[] notificationSounds;
    public AudioClip[] metalWheelOpenSounds;
    public AudioClip[] metalWheelSelectSounds;

    // ── Music ────────────────────────────────────────────────────────────
    [Header("Music Tracks")]
    public AudioClip[] explorationTracks;
    public AudioClip[] combatTracks;
    public AudioClip[] stealthTracks;
    public AudioClip[] bosseTracks;
    public AudioClip[] menuTracks;

    // ── Volume Settings ──────────────────────────────────────────────────
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float ambientVolume = 0.4f;
    [Range(0f, 1f)] public float allomancyVolume = 0.7f;
    [Range(0f, 1f)] public float uiVolume = 0.6f;

    [Header("Music Settings")]
    public float musicCrossfadeDuration = 2f;
    public bool shuffleMusic = true;

    // ── State ────────────────────────────────────────────────────────────
    private MusicState currentMusicState = MusicState.Exploration;
    private int currentTrackIndex = 0;
    private Coroutine musicCoroutine;
    private Coroutine crossfadeCoroutine;
    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();

    public enum MusicState { Exploration, Combat, Stealth, Boss, Menu, Silent }

    // ── Lifecycle ────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyVolumeSettings();
        PlayMusicForState(MusicState.Exploration);
    }

    void EnsureAudioSources()
    {
        if (sfxSource == null) sfxSource = CreateAudioSource("SFX");
        if (musicSource == null) musicSource = CreateAudioSource("Music", true);
        if (allomancySource == null) allomancySource = CreateAudioSource("Allomancy");
        if (ambientSource == null) ambientSource = CreateAudioSource("Ambient", true);
        if (uiSource == null) uiSource = CreateAudioSource("UI");
    }

    AudioSource CreateAudioSource(string name, bool loop = false)
    {
        GameObject go = new GameObject($"AudioSource_{name}");
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }

    // ── Volume ───────────────────────────────────────────────────────────

    public void ApplyVolumeSettings()
    {
        if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
        if (allomancySource != null) allomancySource.volume = allomancyVolume * masterVolume;
        if (ambientSource != null) ambientSource.volume = ambientVolume * masterVolume;
        if (uiSource != null) uiSource.volume = uiVolume * masterVolume;
    }

    public void SetMasterVolume(float vol) { masterVolume = Mathf.Clamp01(vol); ApplyVolumeSettings(); }
    public void SetSFXVolume(float vol) { sfxVolume = Mathf.Clamp01(vol); ApplyVolumeSettings(); }
    public void SetMusicVolume(float vol) { musicVolume = Mathf.Clamp01(vol); ApplyVolumeSettings(); }
    public void SetAmbientVolume(float vol) { ambientVolume = Mathf.Clamp01(vol); ApplyVolumeSettings(); }

    // ── Allomancy Sounds ─────────────────────────────────────────────────

    public void PlayPushSound()
    {
        PlayRandomClip(allomancySource, metalPushSounds, allomancyVolume);
    }

    public void PlayPullSound()
    {
        PlayRandomClip(allomancySource, metalPullSounds, allomancyVolume);
    }

    public void PlayMetalBurnStart(AllomancySkill.MetalType metal)
    {
        PlayRandomClip(allomancySource, metalBurnStartSounds, allomancyVolume * 0.6f);
    }

    public void PlayFlareSound()
    {
        PlayRandomClip(allomancySource, flareSounds, allomancyVolume);
    }

    public void PlayDuraluminBurst()
    {
        PlayRandomClip(allomancySource, duraluminBurstSounds, allomancyVolume * 1.2f);
    }

    public void PlayTimeBubble()
    {
        PlayRandomClip(allomancySource, timeBubbleSounds, allomancyVolume);
    }

    public void PlayAtiumActivate()
    {
        PlayRandomClip(allomancySource, atiumActivateSounds, allomancyVolume);
    }

    // ── Combat Sounds ────────────────────────────────────────────────────

    public void PlayAttackSound()
    {
        PlayRandomClipCooldown("attack", sfxSource, swordSwingSounds, sfxVolume, 0.15f);
    }

    public void PlayHitSound(float damage = 25f)
    {
        if (damage > 50f)
            PlayRandomClip(sfxSource, criticalHitSounds, sfxVolume * 1.2f);
        else
            PlayRandomClip(sfxSource, swordHitSounds, sfxVolume);
    }

    public void PlayBlockSound()
    {
        PlayRandomClip(sfxSource, blockSounds, sfxVolume);
    }

    public void PlayParrySound()
    {
        PlayRandomClip(sfxSource, parrySounds, sfxVolume);
    }

    public void PlayImpactSound()
    {
        PlayRandomClip(sfxSource, impactSounds, sfxVolume);
    }

    public void PlayDeathSound()
    {
        PlayRandomClip(sfxSource, deathSounds, sfxVolume);
    }

    // ── Movement Sounds ──────────────────────────────────────────────────

    public void PlayFootstep(bool onMetal = false)
    {
        AudioClip[] clips = onMetal ? footstepMetalSounds : footstepSounds;
        PlayRandomClipCooldown("footstep", sfxSource, clips, sfxVolume * 0.5f, 0.2f);
    }

    public void PlayLanding()
    {
        PlayRandomClip(sfxSource, landingSounds, sfxVolume * 0.8f);
    }

    public void PlayJump()
    {
        PlayRandomClip(sfxSource, jumpSounds, sfxVolume * 0.5f);
    }

    public void PlaySlide()
    {
        PlayRandomClip(sfxSource, slideSounds, sfxVolume * 0.6f);
    }

    public void PlayWallRun()
    {
        PlayRandomClipCooldown("wallrun", sfxSource, wallRunSounds, sfxVolume * 0.5f, 0.3f);
    }

    // ── UI Sounds ────────────────────────────────────────────────────────

    public void PlayMenuClick()
    {
        PlayRandomClip(uiSource, menuClickSounds, uiVolume);
    }

    public void PlaySkillUnlock()
    {
        PlayRandomClip(uiSource, skillUnlockSounds, uiVolume);
    }

    public void PlayQuestStart()
    {
        PlayRandomClip(uiSource, questStartSounds, uiVolume);
    }

    public void PlayQuestComplete()
    {
        PlayRandomClip(uiSource, questCompleteSounds, uiVolume);
    }

    public void PlayNotification()
    {
        PlayRandomClip(uiSource, notificationSounds, uiVolume * 0.5f);
    }

    public void PlayMetalWheelOpen()
    {
        PlayRandomClip(uiSource, metalWheelOpenSounds, uiVolume);
    }

    public void PlayMetalWheelSelect()
    {
        PlayRandomClip(uiSource, metalWheelSelectSounds, uiVolume * 0.7f);
    }

    // ── Music System ─────────────────────────────────────────────────────

    public void PlayMusicForState(MusicState state)
    {
        if (state == currentMusicState && musicSource.isPlaying) return;
        currentMusicState = state;

        AudioClip[] tracks = GetTracksForState(state);
        if (tracks == null || tracks.Length == 0) return;

        int index = shuffleMusic ? Random.Range(0, tracks.Length) : 0;
        currentTrackIndex = index;

        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(CrossfadeToTrack(tracks[index]));
    }

    public void TransitionToCombat()
    {
        PlayMusicForState(MusicState.Combat);
    }

    public void TransitionToExploration()
    {
        PlayMusicForState(MusicState.Exploration);
    }

    public void TransitionToStealth()
    {
        PlayMusicForState(MusicState.Stealth);
    }

    public void TransitionToBoss()
    {
        PlayMusicForState(MusicState.Boss);
    }

    AudioClip[] GetTracksForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.Exploration: return explorationTracks;
            case MusicState.Combat: return combatTracks;
            case MusicState.Stealth: return stealthTracks;
            case MusicState.Boss: return bosseTracks;
            case MusicState.Menu: return menuTracks;
            case MusicState.Silent: return null;
            default: return explorationTracks;
        }
    }

    IEnumerator CrossfadeToTrack(AudioClip newTrack)
    {
        if (newTrack == null) yield break;

        float targetVol = musicVolume * masterVolume;

        // Fade out
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            float elapsed = 0f;
            while (elapsed < musicCrossfadeDuration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (musicCrossfadeDuration * 0.5f));
                yield return null;
            }
        }

        // Switch track
        musicSource.clip = newTrack;
        musicSource.Play();

        // Fade in
        float fadeElapsed = 0f;
        while (fadeElapsed < musicCrossfadeDuration * 0.5f)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVol, fadeElapsed / (musicCrossfadeDuration * 0.5f));
            yield return null;
        }

        musicSource.volume = targetVol;

        // Auto-advance to next track when this one ends
        if (musicCoroutine != null) StopCoroutine(musicCoroutine);
        musicCoroutine = StartCoroutine(WaitForTrackEnd());
    }

    IEnumerator WaitForTrackEnd()
    {
        while (musicSource.isPlaying)
            yield return null;

        // Play next track in current state
        AudioClip[] tracks = GetTracksForState(currentMusicState);
        if (tracks != null && tracks.Length > 0)
        {
            currentTrackIndex = shuffleMusic
                ? Random.Range(0, tracks.Length)
                : (currentTrackIndex + 1) % tracks.Length;

            crossfadeCoroutine = StartCoroutine(CrossfadeToTrack(tracks[currentTrackIndex]));
        }
    }

    // ── Ambient Sound ────────────────────────────────────────────────────

    public void SetAmbientSound(AudioClip clip)
    {
        if (ambientSource == null || clip == null) return;
        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void PlayAmbientForWeather(string weatherType)
    {
        AudioClip[] clips = null;
        switch (weatherType)
        {
            case "Mist": clips = mistAmbienceSounds; break;
            case "Ash": clips = ashFallSounds; break;
            case "Rain": clips = rainSounds; break;
            case "Storm":
                clips = windSounds;
                // Also play thunder occasionally
                if (thunderSounds != null && thunderSounds.Length > 0)
                    PlayRandomClip(sfxSource, thunderSounds, sfxVolume * 0.8f);
                break;
            case "Clear": clips = windSounds; break;
        }

        if (clips != null && clips.Length > 0)
            SetAmbientSound(clips[Random.Range(0, clips.Length)]);
    }

    public void StopAmbient()
    {
        if (ambientSource != null) ambientSource.Stop();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    void PlayRandomClip(AudioSource source, AudioClip[] clips, float volume)
    {
        if (source == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            source.PlayOneShot(clip, volume * masterVolume);
    }

    void PlayRandomClipCooldown(string id, AudioSource source, AudioClip[] clips, float volume, float cooldown)
    {
        if (soundCooldowns.ContainsKey(id) && Time.time < soundCooldowns[id]) return;
        soundCooldowns[id] = Time.time + cooldown;
        PlayRandomClip(source, clips, volume);
    }

    /// <summary>
    /// Play a one-shot sound at a world position (3D spatialized).
    /// </summary>
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume * masterVolume);
    }
}
