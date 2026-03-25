using UnityEngine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float shakeIntensity = 0f;
    private float shakeDecay = 1f;
    private bool isShaking = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
    }

    void LateUpdate()
    {
        if (!isShaking) return;

        if (shakeIntensity > 0)
        {
            Vector3 offset = Random.insideUnitSphere * shakeIntensity;
            transform.localPosition = originalPosition + offset;

            shakeIntensity -= shakeDecay * Time.deltaTime;
            if (shakeIntensity <= 0)
            {
                StopShake();
            }
        }
    }

    public void Shake(float duration, float intensity)
    {
        if (!isShaking)
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }

        shakeIntensity = intensity;
        shakeDecay = intensity / duration;
        isShaking = true;
    }

    public void ShakeExplosion(float intensity = 0.3f)
    {
        Shake(0.5f, intensity);
    }

    public void ShakeImpact(float intensity = 0.15f)
    {
        Shake(0.2f, intensity);
    }

    public void ShakeContinuous(float intensity)
    {
        shakeIntensity = intensity;
        shakeDecay = 0;
        isShaking = true;
    }

    public void StopShake()
    {
        isShaking = false;
        shakeIntensity = 0;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    public bool IsShaking() => isShaking;
}

public class CameraEffects : MonoBehaviour
{
    [Header("Vignette")]
    public float vignetteIntensity = 0f;
    public Color vignetteColor = Color.black;
    public float vignetteTransitionSpeed = 2f;

    [Header("Field of View")]
    public float normalFOV = 60f;
    public float sprintFOV = 70f;
    public float flyingFOV = 75f;
    public float allomancyFOV = 65f;
    public float fovTransitionSpeed = 5f;

    [Header("Motion Blur")]
    public float motionBlurIntensity = 0f;
    public float maxMotionBlur = 0.3f;

    [Header("Bloom")]
    public float bloomIntensity = 1f;
    public Color bloomColor = Color.white;

    [Header("Chromatic Aberration")]
    public float chromaticAberration = 0f;
    public float chromaticTransitionSpeed = 3f;

    [Header("References")]
    public Camera playerCamera;
    public UnityEngine.Rendering.Volume postProcessVolume;

    private float currentFOV;
    private float targetFOV;
    private bool isAllomancyActive = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = GetComponent<Camera>();
        currentFOV = normalFOV;
        targetFOV = normalFOV;
    }

    void Update()
    {
        UpdateFOV();
        UpdateVignette();
        UpdateMotionBlur();
    }

    void UpdateFOV()
    {
        if (playerCamera == null) return;

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovTransitionSpeed);
        playerCamera.fieldOfView = currentFOV;
    }

    void UpdateVignette()
    {
        // Would update post-process vignette here
    }

    void UpdateMotionBlur()
    {
        Rigidbody playerRb = GetComponent<Rigidbody>();
        if (playerRb == null) return;

        float velocity = playerRb.linearVelocity.magnitude;
        motionBlurIntensity = Mathf.Clamp01(velocity / 20f) * maxMotionBlur;
    }

    public void SetFOV(float fov)
    {
        targetFOV = fov;
    }

    public void SetSprintFOV()
    {
        targetFOV = sprintFOV;
    }

    public void SetNormalFOV()
    {
        targetFOV = normalFOV;
    }

    public void SetFlyingFOV()
    {
        targetFOV = flyingFOV;
    }

    public void SetAllomancyFOV()
    {
        targetFOV = allomancyFOV;
        isAllomancyActive = true;
    }

    public void ClearAllomancyFOV()
    {
        targetFOV = normalFOV;
        isAllomancyActive = false;
    }

    public void SetVignette(float intensity, Color color)
    {
        vignetteIntensity = intensity;
        vignetteColor = color;
    }

    public void PulseVignette(float duration, Color color)
    {
        StartCoroutine(VignettePulse(duration, color));
    }

    IEnumerator VignettePulse(float duration, Color color)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            vignetteIntensity = Mathf.PingPong(elapsed * 4f, 0.5f);
            vignetteColor = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        vignetteIntensity = 0;
    }

    public void SetChromaticAberration(float intensity)
    {
        chromaticAberration = intensity;
    }

    public void PulseChromaticAberration(float duration)
    {
        StartCoroutine(ChromaticPulse(duration));
    }

    IEnumerator ChromaticPulse(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            chromaticAberration = Mathf.PingPong(elapsed * 3f, 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        chromaticAberration = 0;
    }
}

public class ScreenEffects : MonoBehaviour
{
    [Header("Screen Damage")]
    public UnityEngine.UI.Image damageOverlay;
    public Color damageColor = new Color(1, 0, 0, 0.3f);
    public float damageFadeSpeed = 2f;
    private float damageAlpha = 0;

    [Header("Heal Effect")]
    public UnityEngine.UI.Image healOverlay;
    public Color healColor = new Color(0, 1, 0, 0.2f);

    [Header("Death Effect")]
    public UnityEngine.UI.Image deathOverlay;
    public Color deathColor = new Color(0.1f, 0.1f, 0.2f, 1f);

    [Header("Screen Flash")]
    public UnityEngine.UI.Image flashOverlay;
    public float flashDuration = 0.1f;

    void Update()
    {
        UpdateDamageOverlay();
    }

    void UpdateDamageOverlay()
    {
        if (damageOverlay == null) return;

        if (damageAlpha > 0)
        {
            damageAlpha -= damageFadeSpeed * Time.deltaTime;
            damageOverlay.color = new Color(damageColor.r, damageColor.g, damageColor.b, damageAlpha);
        }
    }

    public void ShowDamageEffect(float amount)
    {
        if (damageOverlay == null) return;

        damageAlpha = Mathf.Clamp01(amount / 50f);
        damageOverlay.color = new Color(damageColor.r, damageColor.g, damageColor.b, damageAlpha);
    }

    public void ShowHealEffect()
    {
        if (healOverlay == null) return;

        StartCoroutine(FlashEffect(healOverlay, healColor, 0.3f));
    }

    public void ShowDeathEffect()
    {
        if (deathOverlay == null) return;

        StartCoroutine(FadeInEffect(deathOverlay, deathColor, 1f));
    }

    public void ShowScreenFlash(Color color)
    {
        if (flashOverlay == null) return;

        StartCoroutine(FlashEffect(flashOverlay, color, flashDuration));
    }

    IEnumerator FlashEffect(UnityEngine.UI.Image overlay, Color color, float duration)
    {
        overlay.color = color;
        yield return new WaitForSeconds(duration);

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / duration);
            overlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        overlay.color = Color.clear;
    }

    IEnumerator FadeInEffect(UnityEngine.UI.Image overlay, Color color, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / duration);
            overlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }

    public void HideDeathEffect()
    {
        if (deathOverlay != null)
            deathOverlay.color = Color.clear;
    }
}