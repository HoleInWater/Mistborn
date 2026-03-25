using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    public AudioMixer audioMixer;

    [Header("Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;
    public float ambientVolume = 0.5f;

    private Dictionary<string, AudioClip> soundCache = new Dictionary<string, AudioClip>();
    private AudioClip currentMusic;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null) return;

        currentMusic = clip;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * volumeScale);
    }

    public void PlaySoundAt(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeScale);
    }

    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        if (ambientSource == null) return;
        ambientSource.clip = clip;
        ambientSource.loop = loop;
        ambientSource.volume = ambientVolume * masterVolume;
        ambientSource.Play();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void FadeMusicOut(float duration)
    {
        StartCoroutine(FadeOut(musicSource, duration));
    }

    System.Collections.IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        source.Stop();
    }
}

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healthRegenRate = 5f;
    public float regenDelay = 3f;

    [Header("References")]
    public Allomancer allomancer;
    public UnityEngine.UI.Image healthBar;
    public Text healthText;

    private float timeSinceDamage = 0f;
    private bool isDead = false;

    public static PlayerHealth Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        timeSinceDamage += Time.deltaTime;

        if (timeSinceDamage >= regenDelay && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        timeSinceDamage = 0f;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    void Die()
    {
        isDead = true;
        Debug.Log("[PLAYER] Died!");
        GetComponent<BasicPlayerMove>().enabled = false;
        GetComponent<PlayerCombat>().enabled = false;

        EventManager.TriggerEvent("PlayerDied");
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        }
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    public bool IsDead() => isDead;
}

public interface IDamageable
{
    void TakeDamage(float damage);
    void ApplyKnockback(Vector3 direction);
}

public class DamageFlash : MonoBehaviour
{
    public Material flashMaterial;
    public float flashDuration = 0.2f;

    private Material originalMaterial;
    private Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    System.Collections.IEnumerator Flash()
    {
        foreach (Renderer r in renderers)
        {
            originalMaterial = r.material;
            r.material = flashMaterial;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (Renderer r in renderers)
        {
            if (r != null && originalMaterial != null)
                r.material = originalMaterial;
        }
    }
}