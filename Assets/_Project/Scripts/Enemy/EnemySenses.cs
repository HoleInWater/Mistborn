using UnityEngine;

/// <summary>
/// Physics-based sight and hearing for enemy NPCs.
///
/// SIGHT — cone + line-of-sight raycast
///   Effective range scales with eye height using the horizon-distance
///   proportionality: D ∝ √(eyeHeight). A guard at 1.7 m eye height
///   achieves sightRangeMax; shorter/taller enemies scale accordingly.
///   A cone field-of-view (default 120°) and a blocking raycast are applied.
///   Night and heavy mist reduce range via DayNightCycle / WeatherGameplayIntegration.
///
/// HEARING — inverse-square sound intensity (acoustic physics)
///   Sound level at distance r:
///       L(r) = L_source − 20·log10(r / r_ref)          [r_ref = 1 m]
///   Player noise by movement state (source level at 1 m):
///       Still / crouched  ~20 dB
///       Walking           ~50 dB
///       Sprinting         ~65 dB
///       Combat / heavy    ~75 dB
///   Detection fires when L(r) ≥ hearingThresholdDB.
///
/// SUSPICION METER
///   Hearing builds suspicion slowly; direct sight builds it quickly.
///   The enemy alerts (CanDetectPlayer = true) once suspicion ≥ 1.
///   Suspicion decays when the player is neither seen nor heard.
/// </summary>
[RequireComponent(typeof(AIController))]
public class EnemySenses : MonoBehaviour
{
    // ── Sight ─────────────────────────────────────────────────────────────────

    [Header("Sight")]
    [Tooltip("Max sight range (m) for a guard at standard 1.7 m eye height.")]
    [Range(5f, 60f)] public float sightRangeMax = 25f;

    [Tooltip("Horizontal field of view in degrees (total cone width).")]
    [Range(30f, 360f)] public float fieldOfViewDegrees = 120f;

    [Tooltip("Eye height above the GameObject pivot (m). Scales effective range: taller = farther.")]
    [Range(0.5f, 3f)] public float eyeHeight = 1.7f;

    [Tooltip("Layer mask for geometry that blocks sight. Include Default, Environment, etc.")]
    public LayerMask sightBlockLayers = ~0;   // everything by default

    [Tooltip("Minimum sight range regardless of night/mist penalties.")]
    [Range(1f, 15f)] public float sightRangeMin = 5f;

    // ── Hearing ───────────────────────────────────────────────────────────────

    [Header("Hearing")]
    [Tooltip("Minimum dB level at the enemy's position needed to trigger a hearing alert.")]
    [Range(0f, 80f)] public float hearingThresholdDB = 35f;

    [Tooltip("Absolute max hearing radius — even a shout can't be heard beyond this.")]
    [Range(5f, 80f)] public float hearingRangeMax = 30f;

    [Tooltip("Sound source level (dB at 1 m) while player is still / not moving.")]
    public float noiseStillDB   = 20f;
    [Tooltip("Sound source level (dB at 1 m) while player is walking.")]
    public float noiseWalkingDB = 50f;
    [Tooltip("Sound source level (dB at 1 m) while player is sprinting.")]
    public float noiseSprintDB  = 65f;
    [Tooltip("Extra source level added during combat (swings, impacts).")]
    public float noiseCombatDB  = 75f;

    // ── Suspicion ─────────────────────────────────────────────────────────────

    [Header("Suspicion")]
    [Tooltip("Rate suspicion rises per second when the enemy hears the player.")]
    [Range(0.1f, 5f)] public float suspicionHearRate  = 0.5f;
    [Tooltip("Rate suspicion rises per second when the enemy directly sees the player.")]
    [Range(0.5f, 10f)] public float suspicionSightRate = 3f;
    [Tooltip("Rate suspicion decays per second when nothing is detected.")]
    [Range(0.05f, 2f)] public float suspicionDecayRate = 0.3f;

    // ── Public State ──────────────────────────────────────────────────────────

    /// <summary>True when suspicion has reached 1 (fully alerted).</summary>
    public bool CanDetectPlayer { get; private set; }

    /// <summary>True this frame if a line-of-sight check succeeded.</summary>
    public bool HasDirectSight  { get; private set; }

    /// <summary>True this frame if the hearing model detected a sound.</summary>
    public bool IsHearingPlayer { get; private set; }

    /// <summary>0–1 suspicion meter.</summary>
    public float Suspicion { get; private set; }

    // ── Private ───────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private BasicPlayerMove playerMove;
    private const float ReferenceDistance = 1f; // 1 m acoustic reference

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMove = playerObj.GetComponent<BasicPlayerMove>();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        HasDirectSight  = CheckSight();
        IsHearingPlayer = CheckHearing();

        UpdateSuspicion();
    }

    // ── Sight ─────────────────────────────────────────────────────────────────

    private bool CheckSight()
    {
        Vector3 eyePos    = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = playerTransform.position + Vector3.up * 1f; // aim at player torso
        Vector3 toPlayer  = targetPos - eyePos;
        float   distance  = toPlayer.magnitude;

        float effectiveRange = EffectiveSightRange();
        if (distance > effectiveRange) return false;

        // Cone check
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > fieldOfViewDegrees * 0.5f) return false;

        // Line-of-sight raycast
        if (Physics.Raycast(eyePos, toPlayer.normalized, distance, sightBlockLayers,
                            QueryTriggerInteraction.Ignore))
            return false; // blocked by geometry

        return true;
    }

    /// <summary>
    /// Effective sight range scaled by eye height (horizon proportionality)
    /// and reduced by night / mist conditions.
    /// </summary>
    private float EffectiveSightRange()
    {
        // D ∝ √(eyeHeight). Normalised so 1.7 m → sightRangeMax.
        float heightScale = Mathf.Sqrt(eyeHeight / 1.7f);
        float range = sightRangeMax * heightScale;

        // Night / mist reduction
        float envScale = EnvironmentVisibilityScale();
        range *= envScale;

        return Mathf.Max(range, sightRangeMin);
    }

    private float EnvironmentVisibilityScale()
    {
        float scale = 1f;

        if (DayNightCycle.Instance != null)
        {
            float hour = DayNightCycle.Instance.GetHour();
            // Full visibility 8–18h; ramps down to 0.25 at night
            if (hour < 6f || hour > 20f)       scale *= 0.25f;
            else if (hour < 8f || hour > 18f)  scale *= Mathf.Lerp(0.25f, 1f,
                                                    hour < 8f ? (hour - 6f) / 2f : (20f - hour) / 2f);
        }

        if (WeatherGameplayIntegration.Instance != null)
        {
            // Heavy mist halves sight range
            float mist = WeatherGameplayIntegration.Instance.mistIntensity;
            scale *= Mathf.Lerp(1f, 0.5f, mist);
        }

        return scale;
    }

    // ── Hearing ───────────────────────────────────────────────────────────────

    private bool CheckHearing()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance > hearingRangeMax) return false;

        float sourceDB = PlayerNoiseLevelDB();

        // Inverse-square law: L(r) = L_source − 20·log10(r / r_ref)
        float levelAtEnemy = sourceDB - 20f * Mathf.Log10(Mathf.Max(distance, ReferenceDistance));

        return levelAtEnemy >= hearingThresholdDB;
    }

    /// <summary>
    /// Returns the sound source level (dB at 1 m) based on player movement speed.
    /// Uses BasicPlayerMove.GetCurrentSpeed() and IsSprinting() if available.
    /// </summary>
    private float PlayerNoiseLevelDB()
    {
        if (playerMove == null) return noiseWalkingDB;

        float speed = playerMove.GetCurrentSpeed();

        if (speed < 0.3f) return noiseStillDB;
        if (playerMove.IsSprinting()) return noiseSprintDB;

        // Linearly interpolate walk → sprint range between 0.3 and sprint threshold (~6 m/s)
        return Mathf.Lerp(noiseWalkingDB, noiseSprintDB, Mathf.InverseLerp(0.3f, 6f, speed));
    }

    // ── Suspicion ─────────────────────────────────────────────────────────────

    private void UpdateSuspicion()
    {
        if (HasDirectSight)
            Suspicion += suspicionSightRate * Time.deltaTime;
        else if (IsHearingPlayer)
            Suspicion += suspicionHearRate * Time.deltaTime;
        else
            Suspicion -= suspicionDecayRate * Time.deltaTime;

        Suspicion = Mathf.Clamp01(Suspicion);
        CanDetectPlayer = Suspicion >= 1f;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        // Sight cone — green when alert, yellow when suspicious, white at rest
        Color coneColor = CanDetectPlayer ? Color.green
                        : Suspicion > 0.1f ? Color.yellow
                        : new Color(1f, 1f, 1f, 0.3f);
        Gizmos.color = coneColor;

        float range = Application.isPlaying ? EffectiveSightRange() : sightRangeMax;
        float halfFOV = fieldOfViewDegrees * 0.5f * Mathf.Deg2Rad;

        Vector3 leftEdge  = Quaternion.Euler(0, -fieldOfViewDegrees * 0.5f, 0) * transform.forward * range;
        Vector3 rightEdge = Quaternion.Euler(0,  fieldOfViewDegrees * 0.5f, 0) * transform.forward * range;
        Gizmos.DrawLine(eyePos, eyePos + leftEdge);
        Gizmos.DrawLine(eyePos, eyePos + rightEdge);
        Gizmos.DrawWireSphere(eyePos, range * 0.05f); // eye marker

        // Hearing ring — blue
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, hearingRangeMax);

        // Suspicion bar above head (drawn as a line segment)
        if (Application.isPlaying && Suspicion > 0f)
        {
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, Suspicion);
            Vector3 barBase = transform.position + Vector3.up * (eyeHeight + 0.3f);
            Gizmos.DrawLine(barBase, barBase + Vector3.right * Suspicion);
        }
    }
#endif
}
