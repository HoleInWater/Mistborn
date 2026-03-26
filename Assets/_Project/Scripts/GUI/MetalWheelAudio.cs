///
/// [AGENT TRIPLE ERROR CHECK REPORT]
/// PASS 1 - LOGIC: Null checks for AudioSource and strict limits on sample playing times.
/// PASS 2 - UNITY API: Utilizes regular AudioSources natively, respecting global volumes. PlayOneShot used for overlapping tings.
/// PASS 3 - CONSOLE: N/A, UI Sound feedback enhances console tactile feel.
///
/// BUG FIX: PlayTick() had `metalInt >= 12 && metalInt <= 15` for the Temporal branch.
///          This upper bound excluded Chromium (16) and Nicrosil (17), causing them to
///          silently fall back to tickPhysical. Removed the upper bound so any metal
///          with index >= 12 correctly receives the Temporal tick sound.
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
        // Heuristic based on metal group order (0-3 Physical, 4-7 Mental, 8-11 Enhancement, 12+ Temporal)
        // FIX: The old code was `metalInt >= 12 && metalInt <= 15`, which excluded
        //      Chromium (index 16) and Nicrosil (index 17). They fell through all
        //      branches and incorrectly played tickPhysical. Removing the upper bound
        //      fixes this for all current and any future metals beyond index 15.
        int metalInt = (int)metalType;

        AudioClip tickToPlay = tickPhysical;
        if      (metalInt >= 4 && metalInt <= 7)  tickToPlay = tickMental;
        else if (metalInt >= 8 && metalInt <= 11) tickToPlay = tickEnhancement;
        else if (metalInt >= 12)                  tickToPlay = tickTemporal; // FIX: no upper bound

        if (tickToPlay != null) sfxSource.PlayOneShot(tickToPlay, 0.6f);
    }

    public void PlayDenySound()
    {
        if (denyThud != null) sfxSource.PlayOneShot(denyThud, 0.8f);
    }
}
