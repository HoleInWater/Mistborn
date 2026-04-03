/* TitleCameraController.cs
 *
 * Handles all camera movement during the title sequence:
 *   Phase 1: Slow push forward across the misty field
 *   Phase 3: Dolly through Luthadel streets
 *   Phase 4: Aerial orbit around Kredik Shaw
 *
 * Controlled by TitleSequenceController — call SetPhase() to transition.
 */

using UnityEngine;

public class TitleCameraController : MonoBehaviour
{
    public enum Phase { MistyField, LuthadelStreets, KredikShawAerial, TitleHold }

    [Header("Phase 1 — Misty Field")]
    public Vector3 fieldStartPos    = new Vector3(0f, 2.5f, -12f);
    public Vector3 fieldEndPos      = new Vector3(0f, 2f, -4f);
    public Vector3 fieldLookAt      = new Vector3(0f, 1f, 20f);
    public float   fieldDuration    = 28f;

    [Header("Phase 3 — Luthadel Streets")]
    public Vector3 streetStartPos   = new Vector3(0f, 3f, -15f);
    public Vector3 streetEndPos     = new Vector3(0f, 2.5f, 15f);
    public Vector3 streetLookOffset = new Vector3(0f, 2f, 5f);
    public float   streetDuration   = 17f;

    [Header("Phase 4 — Kredik Shaw Aerial")]
    public Vector3 aerialCenter     = new Vector3(0f, 0f, 0f);
    public float   aerialHeight     = 60f;
    public float   aerialRadius     = 40f;
    public float   aerialOrbitSpeed = 0.03f;
    public float   aerialTilt       = 35f;

    [Header("Phase 5 — Title Hold")]
    public Vector3 titleHoldPos     = new Vector3(0f, 30f, -20f);
    public Vector3 titleHoldLookAt  = new Vector3(0f, 0f, 0f);

    [Header("Camera Breathing")]
    [Tooltip("Subtle sway to make the camera feel handheld/alive")]
    public float breathAmplitude   = 0.03f;
    public float breathFrequency   = 0.4f;
    public float breathRotAmount   = 0.15f;

    private Phase currentPhase = Phase.MistyField;
    private float phaseTimer;
    private float orbitAngle;
    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;
    private bool transitioning;
    private float transitionTime;
    private float transitionDuration = 1.5f;

    void Start()
    {
        transform.position = fieldStartPos;
        transform.LookAt(fieldLookAt);
    }

    public void SetPhase(Phase phase)
    {
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        transitioning = true;
        transitionTime = 0f;
        currentPhase = phase;
        phaseTimer = 0f;
    }

    void Update()
    {
        phaseTimer += Time.deltaTime;

        if (transitioning)
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / transitionDuration);
            t = t * t * (3f - 2f * t); // smoothstep

            Vector3 targetPos = GetPhaseStartPosition();
            Quaternion targetRot = GetPhaseStartRotation();

            transform.position = Vector3.Lerp(transitionStartPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(transitionStartRot, targetRot, t);

            if (t >= 1f) transitioning = false;
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
                transform.position += new Vector3(0f, Mathf.Sin(phaseTimer * 0.3f) * 0.002f, 0f);
                break;
        }

        // Camera breathing — subtle sway on all phases
        ApplyBreathing();
    }

    void UpdateMistyField()
    {
        float t = Mathf.Clamp01(phaseTimer / fieldDuration);
        t = t * t * (3f - 2f * t); // smoothstep for slow start, slow end
        transform.position = Vector3.Lerp(fieldStartPos, fieldEndPos, t);

        // Gentle look-at with slight vertical bob
        Vector3 look = fieldLookAt + new Vector3(0f, Mathf.Sin(phaseTimer * 0.5f) * 0.1f, 0f);
        transform.LookAt(look);
    }

    void UpdateStreets()
    {
        float t = Mathf.Clamp01(phaseTimer / streetDuration);
        t = t * t * (3f - 2f * t);
        transform.position = Vector3.Lerp(streetStartPos, streetEndPos, t);
        transform.LookAt(transform.position + streetLookOffset);
    }

    void UpdateAerial()
    {
        orbitAngle += aerialOrbitSpeed * Time.deltaTime;
        float x = aerialCenter.x + Mathf.Cos(orbitAngle) * aerialRadius;
        float z = aerialCenter.z + Mathf.Sin(orbitAngle) * aerialRadius;
        transform.position = new Vector3(x, aerialHeight, z);
        transform.LookAt(aerialCenter);
        transform.rotation *= Quaternion.Euler(aerialTilt - 90f, 0f, 0f);

        // Correct: look down at the city at an angle
        Vector3 lookDir = (aerialCenter - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(aerialTilt, 0f, 0f);
    }

    void ApplyBreathing()
    {
        float t = Time.time;
        // Organic multi-frequency sway
        float bx = Mathf.Sin(t * breathFrequency * 1.0f) * breathAmplitude
                  + Mathf.Sin(t * breathFrequency * 2.3f) * breathAmplitude * 0.3f;
        float by = Mathf.Sin(t * breathFrequency * 0.7f) * breathAmplitude * 0.6f
                  + Mathf.Cos(t * breathFrequency * 1.8f) * breathAmplitude * 0.2f;

        transform.position += new Vector3(bx, by, 0f);
        transform.rotation *= Quaternion.Euler(
            Mathf.Sin(t * breathFrequency * 0.5f) * breathRotAmount,
            Mathf.Sin(t * breathFrequency * 0.3f) * breathRotAmount * 0.5f,
            0f);
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
            case Phase.TitleHold:       return titleHoldPos;
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
                return Quaternion.LookRotation(aerialCenter - startPos);
            case Phase.TitleHold:
                return Quaternion.LookRotation(titleHoldLookAt - titleHoldPos);
            default: return transform.rotation;
        }
    }
}
