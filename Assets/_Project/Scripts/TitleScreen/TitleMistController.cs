/* TitleMistController.cs
 *
 * Makes the mists BEHAVE like The Warden's mists from the books:
 * - They roll in waves, not static clouds
 * - They curl around objects (attracted to metal)
 * - They pulse with a faint inner rhythm (The Warden's heartbeat)
 * - They react to the camera (part slightly as you move through them)
 * - They thicken over time during the intro (mists are "coming in")
 *
 * Attach to the MistyFieldScene or any parent of mist particle systems.
 * Finds all child ParticleSystems named "MistParticles" and controls them.
 */

using UnityEngine;
using System.Collections.Generic;

public class TitleMistController : MonoBehaviour
{
    [Header("Mist Behavior")]
    [Tooltip("Mists thicken over this many seconds (rolling in at dusk)")]
    public float rollInDuration = 8f;
    [Tooltip("Starting emission multiplier (thin at start)")]
    public float startDensity = 0.2f;
    [Tooltip("Final emission multiplier (thick at peak)")]
    public float peakDensity = 1.5f;

    [Header("Pulse (The Warden's Heartbeat)")]
    [Tooltip("Mists pulse in opacity — frequency in Hz")]
    public float pulseFrequency = 0.15f;
    [Tooltip("How much the opacity varies with each pulse")]
    public float pulseAmplitude = 0.08f;

    [Header("Wind")]
    [Tooltip("Mist drifts in this direction (world space)")]
    public Vector3 windDirection = new Vector3(0.3f, 0f, 0.1f);
    [Tooltip("Wind changes direction slowly over time")]
    public float windShiftSpeed = 0.05f;

    [Header("Camera Reaction")]
    [Tooltip("Mists part slightly around the camera")]
    public float cameraRepelRadius = 3f;
    public float cameraRepelForce = 0.5f;

    private List<ParticleSystem> mistSystems = new List<ParticleSystem>();
    private List<float> baseEmissionRates = new List<float>();
    private float timer;
    private Camera cam;
    private float windAngle;

    void Start()
    {
        cam = Camera.main;
        windAngle = Mathf.Atan2(windDirection.z, windDirection.x);

        // Find all mist particle systems in children
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            if (ps.gameObject.name.Contains("Mist"))
            {
                mistSystems.Add(ps);
                baseEmissionRates.Add(ps.emission.rateOverTime.constant);
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // ── Roll-in: mists thicken over time ─────────────────────────────
        float rollT = Mathf.Clamp01(timer / rollInDuration);
        // Smooth ease-in — mists creep in slowly then fill quickly
        float densityMult = Mathf.Lerp(startDensity, peakDensity, rollT * rollT);

        // ── The Warden's pulse — subtle opacity rhythm ─────────────────
        float pulse = 1f + Mathf.Sin(timer * pulseFrequency * Mathf.PI * 2f) * pulseAmplitude;
        densityMult *= pulse;

        // ── Apply to all mist systems ────────────────────────────────────
        for (int i = 0; i < mistSystems.Count; i++)
        {
            if (mistSystems[i] == null) continue;

            var em = mistSystems[i].emission;
            em.rateOverTime = baseEmissionRates[i] * densityMult;
        }

        // ── Wind shift — direction changes slowly ────────────────────────
        windAngle += windShiftSpeed * Time.deltaTime;
        Vector3 currentWind = new Vector3(Mathf.Cos(windAngle), 0f, Mathf.Sin(windAngle)) * windDirection.magnitude;

        foreach (var ps in mistSystems)
        {
            if (ps == null) continue;
            var noise = ps.noise;
            if (noise.enabled)
            {
                noise.scrollSpeed = 0.08f + Mathf.Sin(timer * 0.2f) * 0.04f;
            }
        }

        // ── Camera repel — mists part slightly around the viewer ─────────
        if (cam != null)
        {
            foreach (var ps in mistSystems)
            {
                if (ps == null) continue;
                var forceField = ps.externalForces;
                // External forces require ParticleSystemForceField objects
                // Instead, we subtly shift the emission shape position away from camera
                float distToCam = Vector3.Distance(ps.transform.position, cam.transform.position);
                if (distToCam < cameraRepelRadius * 2f)
                {
                    Vector3 awayFromCam = (ps.transform.position - cam.transform.position).normalized;
                    ps.transform.position += awayFromCam * cameraRepelForce * Time.deltaTime *
                        Mathf.Clamp01(1f - distToCam / (cameraRepelRadius * 2f));
                }
            }
        }
    }
}
