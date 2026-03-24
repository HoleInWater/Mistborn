using UnityEngine;

/// <summary>
/// Provides visual and auditory feedback when a metal is burned.
/// Standardized for all 18 metals in the Mistborn RPG foundation.
/// </summary>
public class MetalBurnEffect : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem burnParticles;
    public Light burnLight;
    public Color burnColor = new Color(0.8f, 0.8f, 1f, 1f);

    [Header("Audio Effects")]
    public AudioSource burnAudioSource;
    public AudioClip burnLoopClip;

    private Allomancer allomancer;
    private FlareManager flareManager;

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
        flareManager = GetComponentInParent<FlareManager>();

        if (burnParticles != null) burnParticles.Stop();
        if (burnLight != null) burnLight.enabled = false;
        if (burnAudioSource != null && burnLoopClip != null)
        {
            burnAudioSource.clip = burnLoopClip;
            burnAudioSource.loop = true;
            burnAudioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (allomancer == null) return;

        bool isBurning = allomancer.IsBurning();
        float intensity = 1f;

        if (flareManager != null && flareManager.IsFlaring)
        {
            intensity = flareManager.FlareIntensity;
        }

        if (isBurning)
        {
            if (burnParticles != null && !burnParticles.isPlaying) burnParticles.Play();
            if (burnLight != null)
            {
                burnLight.enabled = true;
                burnLight.intensity = 2f * intensity;
                burnLight.color = burnColor;
            }
            if (burnAudioSource != null && !burnAudioSource.isPlaying) burnAudioSource.Play();
        }
        else
        {
            if (burnParticles != null && burnParticles.isPlaying) burnParticles.Stop();
            if (burnLight != null) burnLight.enabled = false;
            if (burnAudioSource != null && burnAudioSource.isPlaying) burnAudioSource.Stop();
        }
    }
}
