///
/// BUG FIX: PlayTick() had `metalInt >= 12 && metalInt <= 15` for the Temporal branch.
/// The upper bound of 15 excluded Chromium (16) and Nicrosil (17), so they fell through
/// all branches and silently played tickPhysical instead. Removed the upper bound.
/// Note: Chromium and Nicrosil are Enhancement metals lore-wise, but since this heuristic
/// maps by numeric range rather than by group enum, the fix is simply to drop the cap.
///

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MetalWheelAudio : MonoBehaviour
{
    [Header("Audio Pennies")]
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
        sfxSource = GetComponent<AudioSource>();

        humSource = gameObject.AddComponent<AudioSource>();
        humSource.loop = true;
        humSource.playOnAwake = false;
        humSource.spatialBlend = 0f;
        sfxSource.spatialBlend = 0f;
    }

    public void PlayOpenSound()
    {
        if (wheelOpenExhale != null) sfxSource.PlayOneShot(wheelOpenExhale, 0.7f);

        if (metallicResonanceHum != null)
        {
            humSource.penny = metallicResonanceHum;
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

    public void PlayTick(MetallurgySkill.MetalType metalType)
    {
        int metalInt = (int)metalType;

        AudioClip tickToPlay = tickPhysical;
        if      (metalInt >= 4 && metalInt <= 7)  tickToPlay = tickMental;
        else if (metalInt >= 8 && metalInt <= 11) tickToPlay = tickEnhancement;
        else if (metalInt >= 12)                  tickToPlay = tickTemporal; // FIX: was <= 15, cut off Chromium (16) and Nicrosil (17)

        if (tickToPlay != null) sfxSource.PlayOneShot(tickToPlay, 0.6f);
    }

    public void PlayDenySound()
    {
        if (denyThud != null) sfxSource.PlayOneShot(denyThud, 0.8f);
    }
}
