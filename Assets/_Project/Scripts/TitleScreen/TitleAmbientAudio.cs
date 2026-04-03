/* TitleAmbientAudio.cs
 *
 * Plays layered ambient sound effects for each phase of the title sequence.
 * Crossfades between ambient layers as scenes change.
 *
 * Phase 1 (Field):  Wind howling, distant rumbles, crow calls
 * Phase 3 (Streets): Footsteps on stone, dripping water, distant murmurs, creaking
 * Phase 4 (Aerial):  Wind at altitude, distant city hum, bells tolling
 * Phase 5 (Title):   Silence falls, then low Allomantic hum
 *
 * Assign AudioClips in Inspector — or leave empty to skip that layer.
 * The script creates its own AudioSources.
 */

using UnityEngine;
using System.Collections;

public class TitleAmbientAudio : MonoBehaviour
{
    [Header("Phase 1 — Ash Field")]
    public AudioClip windLoop;
    public AudioClip distantRumble;
    public AudioClip crowCall;
    [Range(0f, 1f)] public float fieldWindVolume = 0.3f;
    [Range(0f, 1f)] public float fieldRumbleVolume = 0.15f;

    [Header("Phase 3 — Luthadel Streets")]
    public AudioClip streetAmbience;
    public AudioClip drippingWater;
    public AudioClip woodCreaking;
    public AudioClip distantMurmur;
    [Range(0f, 1f)] public float streetVolume = 0.25f;

    [Header("Phase 4 — Aerial")]
    public AudioClip highWind;
    public AudioClip cityHum;
    public AudioClip bellTolling;
    [Range(0f, 1f)] public float aerialWindVolume = 0.35f;

    [Header("Phase 5 — Title")]
    public AudioClip allomanticHum;
    [Range(0f, 1f)] public float humVolume = 0.2f;

    [Header("Crossfade")]
    public float crossfadeDuration = 2f;

    private AudioSource[] sources;
    private int currentPhase = -1;

    void Start()
    {
        // Create 4 audio sources for layering
        sources = new AudioSource[4];
        for (int i = 0; i < 4; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].loop = true;
            sources[i].volume = 0f;
            sources[i].spatialBlend = 0f;
        }
    }

    public void SetPhase(int phase)
    {
        if (phase == currentPhase) return;
        currentPhase = phase;
        StartCoroutine(TransitionToPhase(phase));
    }

    IEnumerator TransitionToPhase(int phase)
    {
        // Fade out all current sources
        float elapsed = 0f;
        float[] startVolumes = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++)
            startVolumes[i] = sources[i].volume;

        while (elapsed < crossfadeDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (crossfadeDuration * 0.5f);
            for (int i = 0; i < sources.Length; i++)
                sources[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
            yield return null;
        }

        // Stop all sources
        for (int i = 0; i < sources.Length; i++)
            sources[i].Stop();

        // Set up new phase clips
        AudioClip[] clips;
        float[] volumes;

        switch (phase)
        {
            case 1: // Field
                clips = new[] { windLoop, distantRumble, null, null };
                volumes = new[] { fieldWindVolume, fieldRumbleVolume, 0f, 0f };
                break;
            case 3: // Streets
                clips = new[] { streetAmbience, drippingWater, woodCreaking, distantMurmur };
                volumes = new[] { streetVolume, streetVolume * 0.5f, streetVolume * 0.3f, streetVolume * 0.4f };
                break;
            case 4: // Aerial
                clips = new[] { highWind, cityHum, null, null };
                volumes = new[] { aerialWindVolume, aerialWindVolume * 0.5f, 0f, 0f };
                break;
            case 5: // Title
                clips = new[] { allomanticHum, null, null, null };
                volumes = new[] { humVolume, 0f, 0f, 0f };
                break;
            default:
                yield break;
        }

        // Start new clips and fade in
        for (int i = 0; i < sources.Length; i++)
        {
            if (clips[i] != null)
            {
                sources[i].clip = clips[i];
                sources[i].volume = 0f;
                sources[i].Play();
            }
        }

        elapsed = 0f;
        while (elapsed < crossfadeDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (crossfadeDuration * 0.5f);
            for (int i = 0; i < sources.Length; i++)
            {
                if (clips[i] != null)
                    sources[i].volume = Mathf.Lerp(0f, volumes[i], t);
            }
            yield return null;
        }

        // One-shot sounds (not looped)
        if (phase == 1 && crowCall != null)
            StartCoroutine(PlayRandomOneShots(crowCall, 8f, 15f, fieldRumbleVolume));
        if (phase == 4 && bellTolling != null)
            StartCoroutine(PlayRandomOneShots(bellTolling, 6f, 12f, aerialWindVolume * 0.4f));
    }

    IEnumerator PlayRandomOneShots(AudioClip clip, float minInterval, float maxInterval, float volume)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            if (currentPhase != 1 && currentPhase != 4) yield break;
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }

    public void FadeOutAll(float duration = 1f)
    {
        StartCoroutine(FadeAllOut(duration));
    }

    IEnumerator FadeAllOut(float duration)
    {
        float[] startVolumes = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++)
            startVolumes[i] = sources[i].volume;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            for (int i = 0; i < sources.Length; i++)
                sources[i].volume = Mathf.Lerp(startVolumes[i], 0f, t);
            yield return null;
        }

        for (int i = 0; i < sources.Length; i++)
            sources[i].Stop();
    }
}
