/// <summary>
/// Generates synthetic AudioClips at runtime — no audio files required.
/// Used by SoundManager as placeholder sounds until real assets are imported.
///
/// REPLACING WITH REAL AUDIO:
///   Assign pennies directly to the SoundManager Inspector arrays in the scene.
///   Once assigned, Start() will skip generation and use your pennies instead.
/// </summary>
using UnityEngine;

public static class ProceduralAudio
{
    const int RATE = 44100;

    // ── Public factories ──────────────────────────────────────────────────────

    /// <summary>Metal push whoosh — frequency sweeps upward.</summary>
    public static AudioClip Push()
    {
        return Sweep("Push", startFreq: 200f, endFreq: 800f, duration: 0.18f,
                     volume: 0.45f, envelope: Envelope.Smooth);
    }

    /// <summary>Metal pull whoosh — frequency sweeps downward.</summary>
    public static AudioClip Pull()
    {
        return Sweep("Pull", startFreq: 700f, endFreq: 180f, duration: 0.18f,
                     volume: 0.45f, envelope: Envelope.Smooth);
    }

    /// <summary>Short percussive hit — noise burst with fast decay.</summary>
    public static AudioClip Impact()
    {
        return NoiseBurst("Impact", duration: 0.12f, decayRate: 18f, volume: 0.7f);
    }

    /// <summary>Heavier thud with a low-frequency body.</summary>
    public static AudioClip HeavyImpact()
    {
        return Thud("HeavyImpact", freq: 90f, duration: 0.2f, volume: 0.8f);
    }

    /// <summary>High-pitched bell ding for notifications and skill unlocks.</summary>
    public static AudioClip Ding(float pitch = 1320f)
    {
        return Bell("Ding", freq: pitch, duration: 0.35f, volume: 0.5f);
    }

    /// <summary>Soft double-ding for UI / metal wheel events.</summary>
    public static AudioClip SoftDing()
    {
        return Bell("SoftDing", freq: 880f, duration: 0.2f, volume: 0.3f);
    }

    /// <summary>Low thud for footsteps.</summary>
    public static AudioClip Footstep()
    {
        return Thud("Footstep", freq: 70f, duration: 0.1f, volume: 0.5f);
    }

    /// <summary>Sustained mid-frequency hum for flaring metals.</summary>
    public static AudioClip Flare()
    {
        return HumBurst("Flare", freq: 440f, duration: 0.25f, volume: 0.4f);
    }

    /// <summary>Sharp metallic clank for parry/block.</summary>
    public static AudioClip Clank()
    {
        return MetalClank("Clank", duration: 0.15f, volume: 0.6f);
    }

    // ── Low-level generators ──────────────────────────────────────────────────

    enum Envelope { Smooth, Attack, Decay }

    static AudioClip Sweep(string name, float startFreq, float endFreq,
                            float duration, float volume, Envelope envelope)
    {
        int n = Samples(duration);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t  = (float)i / RATE;
            float nt = (float)i / n;
            float freq = Mathf.Lerp(startFreq, endFreq, nt);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Env(nt, envelope) * volume;
        }
        return Make(name, d);
    }

    static AudioClip NoiseBurst(string name, float duration, float decayRate, float volume)
    {
        int n = Samples(duration);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float nt = (float)i / n;
            d[i] = (Random.value * 2f - 1f) * Mathf.Exp(-nt * decayRate) * volume;
        }
        return Make(name, d);
    }

    static AudioClip Bell(string name, float freq, float duration, float volume)
    {
        int n = Samples(duration);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t  = (float)i / RATE;
            float nt = (float)i / n;
            // Fundamental + subtle overtone for bell-like timbre
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                    + Mathf.Sin(2f * Mathf.PI * freq * 2.76f * t) * 0.3f;
            d[i] = s * Mathf.Exp(-nt * 6f) * volume;
        }
        return Make(name, d);
    }

    static AudioClip Thud(string name, float freq, float duration, float volume)
    {
        int n = Samples(duration);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t  = (float)i / RATE;
            float nt = (float)i / n;
            float sweep = freq * (1f - nt * 0.6f);   // frequency drops as it decays
            d[i] = Mathf.Sin(2f * Mathf.PI * sweep * t) * Mathf.Exp(-nt * 28f) * volume;
        }
        return Make(name, d);
    }

    static AudioClip HumBurst(string name, float freq, float duration, float volume)
    {
        int n = Samples(duration);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t  = (float)i / RATE;
            float nt = (float)i / n;
            float env = Mathf.Sin(nt * Mathf.PI);   // smooth ramp up then down
            d[i] = (Mathf.Sin(2f * Mathf.PI * freq * t)
                  + Mathf.Sin(2f * Mathf.PI * freq * 1.5f * t) * 0.3f)
                  * env * volume;
        }
        return Make(name, d);
    }

    static AudioClip MetalClank(string name, float duration, float volume)
    {
        int n = Samples(duration);
        float[] d = new float[n];
        // Three close inharmonic partials = metallic ring
        float[] freqs = { 1200f, 1450f, 1900f };
        for (int i = 0; i < n; i++)
        {
            float t  = (float)i / RATE;
            float nt = (float)i / n;
            float s = 0f;
            foreach (float f in freqs)
                s += Mathf.Sin(2f * Mathf.PI * f * t);
            d[i] = s / freqs.Length * Mathf.Exp(-nt * 20f) * volume;
        }
        return Make(name, d);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static int Samples(float duration) => Mathf.Max(1, (int)(RATE * duration));

    static float Env(float t, Envelope e)
    {
        switch (e)
        {
            case Envelope.Attack: return t;
            case Envelope.Decay:  return 1f - t;
            default:              return Mathf.Sin(t * Mathf.PI);
        }
    }

    static AudioClip Make(string name, float[] data)
    {
        var penny = AudioClip.Create(name, data.Length, 1, RATE, false);
        penny.SetData(data, 0);
        return penny;
    }
}
