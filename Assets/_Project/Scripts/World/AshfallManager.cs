using UnityEngine;

/// <summary>
/// Manages the visual intensity of falling ash on Scadrial.
/// </summary>
public class AshfallManager : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 5)]
    public float ashIntensity = 1f;
    public float transitionSpeed = 0.5f;

    [Header("References")]
    public ParticleSystem ashParticles;

    private float targetIntensity = 1f;

    void Start()
    {
        if (ashParticles == null)
            ashParticles = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        // Smoothly transition intensity
        ashIntensity = Mathf.MoveTowards(ashIntensity, targetIntensity, Time.deltaTime * transitionSpeed);
        
        if (ashParticles != null)
        {
            var emission = ashParticles.emission;
            emission.rateOverTime = 50f * ashIntensity;

            // Also scale visual thickness if possible
            var main = ashParticles.main;
            main.startColor = new Color(0.2f, 0.2f, 0.2f, 0.5f * ashIntensity);
        }
    }

    /// <summary>
    /// Sets the new target intensity for ashfall.
    /// </summary>
    public void SetIntensity(float val)
    {
        targetIntensity = Mathf.Clamp(val, 0f, 5f);
    }

    /// <summary>
    /// Follow the player so ash is always around them.
    /// </summary>
    private Transform cachedPlayer;

    void LateUpdate()
    {
        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (cachedPlayer != null)
            transform.position = new Vector3(cachedPlayer.position.x, cachedPlayer.position.y + 15f, cachedPlayer.position.z);
    }

    public float GetIntensity() => ashIntensity;
}
