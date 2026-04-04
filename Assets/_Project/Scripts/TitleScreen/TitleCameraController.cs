/* TitleCameraController.cs
 *
 * Camera movement for the title sequence:
 *   Phase 1 (MistyField): Slow push forward across the ash field
 *   Phase 3 (LuthadelStreets): Dolly through streets, ends pushing into a doorway
 *   Phase 4 (KredikShawAerial): Aerial orbit around Kredik Shaw, descending
 *   Phase 5 (TitleHold): CONTINUES orbiting Kredik Shaw (no barrel roll, no static hold)
 */

using UnityEngine;

public class TitleCameraController : MonoBehaviour
{
    public enum Phase { MistyField, LuthadelStreets, KredikShawAerial, TitleHold }

    [Header("Phase 1 -- Misty Field")]
    public Vector3 fieldStartPos    = new Vector3(0f, 2.5f, -12f);
    public Vector3 fieldEndPos      = new Vector3(0f, 2f, -4f);
    public Vector3 fieldLookAt      = new Vector3(0f, 1f, 20f);
    public float   fieldDuration    = 28f;

    [Header("Phase 3 -- Luthadel Streets")]
    public Vector3 streetStartPos   = new Vector3(0f, 3f, -15f);
    public Vector3 streetEndPos     = new Vector3(0f, 2.5f, 15f);
    public Vector3 streetLookOffset = new Vector3(0f, 2f, 5f);
    public float   streetDuration   = 17f;
    [Tooltip("Final position — camera pushes into a building doorway for clean transition")]
    public Vector3 streetExitPos    = new Vector3(-3f, 2f, 18f);
    public float   streetExitTime   = 2f;

    [Header("Phase 4 -- Kredik Shaw Aerial")]
    public Vector3 aerialCenter     = new Vector3(0f, 0f, 0f);
    public float   aerialHeight     = 60f;
    public float   aerialRadius     = 40f;
    public float   aerialOrbitSpeed = 0.03f;

    [Header("Title Hold -- dramatic reveal position")]
    [Tooltip("Orbit angle (radians) where the camera should be when title drops")]
    public float   titleRevealAngle  = 2.5f; // facing Kredik Shaw from a dramatic angle
    public float   titleZoomStart    = 55f;
    public float   titleZoomEnd      = 42f;
    public float   titleZoomDuration = 12f;

    [Header("Camera Smoothing")]
    [Tooltip("How smoothly the camera follows its target path (lower = smoother)")]
    public float positionSmoothTime = 0.5f;
    public float rotationSmoothSpeed = 3f;

    [Header("Camera Breathing")]
    public float breathAmplitude   = 0.015f;
    public float breathFrequency   = 0.25f;

    private Phase currentPhase = Phase.MistyField;
    private float phaseTimer;
    private float orbitAngle;
    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;
    private bool transitioning;
    private float transitionTime;
    private float transitionDuration = 3f;
    private Camera cam;
    private Vector3 _smoothVelocity; // for SmoothDamp

    void Start()
    {
        cam = GetComponent<Camera>();
        transform.position = fieldStartPos;
        transform.LookAt(fieldLookAt);
    }

    public void SetPhase(Phase phase)
    {
        // TitleHold: continue orbiting but smoothly rotate to the reveal angle
        if (phase == Phase.TitleHold)
        {
            currentPhase = phase;
            phaseTimer = 0f;
            Vector3 dir = transform.position - aerialCenter;
            orbitAngle = Mathf.Atan2(dir.z, dir.x);
            // Don't set transitioning — just let UpdateAerial keep running
            // The orbit will naturally reach the title reveal angle
            return;
        }

        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        transitioning = true;
        transitionTime = 0f;
        currentPhase = phase;
    }

    void Update()
    {
        if (!transitioning)
            phaseTimer += Time.deltaTime;

        if (transitioning)
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / transitionDuration);
            // Cubic ease-in-out
            t = t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

            Vector3 targetPos = GetPhaseStartPosition();
            Quaternion targetRot = GetPhaseStartRotation();

            transform.position = Vector3.Lerp(transitionStartPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(transitionStartRot, targetRot, t);

            if (t >= 1f)
            {
                transitioning = false;
                phaseTimer = 0f;
            }
            return;
        }

        switch (currentPhase)
        {
            case Phase.MistyField:
                UpdateMistyField();
                break;
            case Phase.LuthadelStreets:
                UpdateStreets();
                break;
            case Phase.KredikShawAerial:
                UpdateAerial();
                break;
            case Phase.TitleHold:
                // Keep orbiting Kredik Shaw — same as aerial but with slow zoom
                UpdateAerial();
                if (cam != null)
                {
                    float zoomT = Mathf.Clamp01(phaseTimer / titleZoomDuration);
                    cam.fieldOfView = Mathf.Lerp(titleZoomStart, titleZoomEnd, zoomT);
                }
                break;
        }

        // Subtle breathing — only when not transitioning, and ONLY position (no rotation)
        if (!transitioning)
            ApplyBreathing();
    }

    void UpdateMistyField()
    {
        float t = Mathf.Clamp01(phaseTimer / fieldDuration);
        t = t * t * (3f - 2f * t);

        Vector3 targetPos = Vector3.Lerp(fieldStartPos, fieldEndPos, t);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _smoothVelocity, positionSmoothTime);

        Vector3 look = fieldLookAt + new Vector3(0f, Mathf.Sin(phaseTimer * 0.4f) * 0.03f, 0f);
        SmoothLookAt(look);
    }

    void UpdateStreets()
    {
        float t = Mathf.Clamp01(phaseTimer / streetDuration);
        t = t * t * (3f - 2f * t);

        Vector3 targetPos = Vector3.Lerp(streetStartPos, streetEndPos, t);

        // Near the end, veer toward a building doorway for clean transition
        float exitT = Mathf.Clamp01((phaseTimer - (streetDuration - streetExitTime)) / streetExitTime);
        if (exitT > 0f)
        {
            float eased = exitT * exitT;
            targetPos = Vector3.Lerp(targetPos, streetExitPos, eased);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _smoothVelocity, positionSmoothTime);

        // Gentle forward look only — no side scanning
        Vector3 lookTarget = transform.position + streetLookOffset;
        if (exitT > 0f)
            lookTarget = Vector3.Lerp(lookTarget, streetExitPos + Vector3.forward * 2f, exitT);

        SmoothLookAt(lookTarget);
    }

    void UpdateAerial()
    {
        orbitAngle += aerialOrbitSpeed * Time.deltaTime;

        float descentT = Mathf.Clamp01(phaseTimer / 25f);
        float currentHeight = Mathf.Lerp(aerialHeight, aerialHeight * 0.65f, descentT);
        float currentRadius = Mathf.Lerp(aerialRadius, aerialRadius * 0.7f, descentT);

        float x = aerialCenter.x + Mathf.Cos(orbitAngle) * currentRadius;
        float z = aerialCenter.z + Mathf.Sin(orbitAngle) * currentRadius;
        Vector3 targetPos = new Vector3(x, currentHeight, z);

        // Smooth damp instead of hard set — no snapping
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _smoothVelocity, positionSmoothTime * 0.3f);

        Vector3 lookTarget = aerialCenter + new Vector3(0f, 10f, 0f);
        SmoothLookAt(lookTarget);
    }

    /// <summary>Smoothly rotate toward a look target instead of snapping.</summary>
    void SmoothLookAt(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);
    }

    void ApplyBreathing()
    {
        float t = Time.time;
        float bx = Mathf.Sin(t * breathFrequency) * breathAmplitude;
        float by = Mathf.Sin(t * breathFrequency * 0.7f) * breathAmplitude * 0.5f;
        transform.position += new Vector3(bx, by, 0f);
    }

    Vector3 GetPhaseStartPosition()
    {
        switch (currentPhase)
        {
            case Phase.MistyField:      return fieldStartPos;
            case Phase.LuthadelStreets: return streetStartPos;
            case Phase.KredikShawAerial:
                return new Vector3(
                    aerialCenter.x + aerialRadius,
                    aerialHeight,
                    aerialCenter.z);
            case Phase.TitleHold:
                // Start from wherever the aerial camera currently is
                return transform.position;
            default: return transform.position;
        }
    }

    Quaternion GetPhaseStartRotation()
    {
        switch (currentPhase)
        {
            case Phase.MistyField:
                return Quaternion.LookRotation(fieldLookAt - fieldStartPos);
            case Phase.LuthadelStreets:
                return Quaternion.LookRotation(streetLookOffset);
            case Phase.KredikShawAerial:
                Vector3 startPos = GetPhaseStartPosition();
                return Quaternion.LookRotation((aerialCenter + Vector3.up * 10f) - startPos);
            case Phase.TitleHold:
                return transform.rotation; // continue from current
            default: return transform.rotation;
        }
    }
}
