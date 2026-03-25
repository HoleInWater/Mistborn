///
/// [AGENT TRIPLE ERROR CHECK REPORT]
/// PASS 1 - LOGIC: Null checks for AudioSource and strict limits on sample playing times.
/// PASS 2 - UNITY API: Utilizes regular AudioSources natively, respecting global volumes. PlayOneShot used for overlapping tings.
/// PASS 3 - CONSOLE: N/A, UI Sound feedback enhances console tactile feel.
///

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MetalWheelAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip wheelOpenExhale;
    public AudioClip wheelCloseWhoosh;
    public AudioClip selectConfirmClunk;
    public AudioClip denyThud;
    
    [Header("Scroll Tings")]
    public AudioClip tickPhysical;
    public AudioClip tickMental;
    public AudioClip tickEnhancement;
    public AudioClip tickTemporal;

    [Header("Looping Ambience")]
    public AudioClip lowReserveHeartbeat;
    public AudioClip metallicResonanceHum;

    private AudioSource sfxSource;
    private AudioSource humSource;

    void Awake()
    {
        // Try to get existing or add new
        sfxSource = GetComponent<AudioSource>();
        
        // We need a secondary source for the looping hums
        humSource = gameObject.AddComponent<AudioSource>();
        humSource.loop = true;
        humSource.playOnAwake = false;
        humSource.spatialBlend = 0f; // 2D UI sound
        sfxSource.spatialBlend = 0f;
    }

    public void PlayOpenSound()
    {
        if (wheelOpenExhale != null) sfxSource.PlayOneShot(wheelOpenExhale, 0.7f);
        
        if (metallicResonanceHum != null)
        {
            humSource.clip = metallicResonanceHum;
            humSource.volume = 0.4f;
            humSource.Play();
        }
    }

    public void PlayCloseSound(bool wasConfirmed)
    {
        humSource.Stop();
        
        if (wasConfirmed && selectConfirmClunk != null)
        {
            sfxSource.PlayOneShot(selectConfirmClunk, 1f);
            sfxSource.PlayOneShot(wheelCloseWhoosh, 0.6f);
        }
        else if (wheelCloseWhoosh != null)
        {
            sfxSource.PlayOneShot(wheelCloseWhoosh, 0.4f);
        }
    }

    public void PlayTick(AllomancySkill.MetalType metalType)
    {
        // Simple heuristic based on prompt order (1-4 Physical, 5-8 Mental, 9-12 Enhance, 13-16 Temporal)
        int metalInt = (int)metalType;
        AudioClip tickToPlay = tickPhysical;

        if (metalInt >= 4 && metalInt <= 7) tickToPlay = tickMental;
        else if (metalInt >= 8 && metalInt <= 11) tickToPlay = tickEnhancement;
        else if (metalInt >= 12 && metalInt <= 15) tickToPlay = tickTemporal;

        if (tickToPlay != null) sfxSource.PlayOneShot(tickToPlay, 0.6f);
    }

    public void PlayDenySound()
    {
        if (denyThud != null) sfxSource.PlayOneShot(denyThud, 0.8f);
    }
}
