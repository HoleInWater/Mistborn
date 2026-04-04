/* TitleCameraController.cs
 *
 * Camera for the title sequence. DESIGN RULE:
 *   The camera does NOT move positions except during fade-to-black transitions.
 *   Within each phase, the camera is essentially LOCKED in place with only
 *   very subtle drift (breathing). No panning, no dollying, no orbiting
 *   within a phase — just a still cinematic shot.
 *
 *   Phase 1 (Field): Static wide shot of the misty field. Tiny drift.
 *   Phase 3 (Streets): Static shot down the street. Tiny drift.
 *   Phase 4 (Kredik Shaw): Slow orbit around Kredik Shaw. This is the only
 *     phase with real movement — aerial establishing shot.
 *   Phase 5 (Title): Continues the slow orbit with FOV zoom.
 */

using UnityEngine;

public class TitleCameraController : MonoBehaviour
{
    public enum Phase { MistyField, LuthadelStreets, KredikShawAerial, TitleHold }

    [Header("Phase 1 -- Misty Field (static shot)")]
    public Vector3 fieldPosition = new Vector3(0f, 2.5f, -8f);
    public Vector3 fieldLookAt   = new Vector3(0f, 1.5f, 30f);

    [Header("Phase 3 -- Luthadel Streets (static shot)")]
    public Vector3 streetPosition = new Vector3(0f, 3f, -5f);
    public Vector3 streetLookAt   = new Vector3(0f, 2.5f, 20f);

    [Header("Phase 4 -- Kredik Shaw (slow orbit)")]
    public Vector3 aerialCenter     = new Vector3(0f, 0f, 0f);
    public float   aerialHeight     = 55f;
    public float   aerialRadius     = 35f;
    public float   aerialOrbitSpeed = 0.025f;

    [Header("Phase 5 -- Title Hold")]
    public float   titleZoomStart    = 55f;
    public float   titleZoomEnd      = 42f;
    public float   titleZoomDuration = 12f;

    [Header("Subtle Drift (all phases)")]
    public float driftAmount = 0.008f;
    public float driftSpeed  = 0.15f;

    private Phase currentPhase = Phase.MistyField;
    private float phaseTimer;
    private float orbitAngle;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        transform.position = fieldPosition;
        transform.LookAt(fieldLookAt);
    }

    public void SetPhase(Phase phase)
    {
        currentPhase = phase;
        phaseTimer = 0f;

        // Snap to the new phase position immediately.
        // This is called DURING a fade-to-black, so the player can't see the snap.
        switch (phase)
        {
            case Phase.MistyField:
                transform.position = fieldPosition;
                transform.LookAt(fieldLookAt);
                break;

            case Phase.LuthadelStreets:
                transform.position = streetPosition;
                transform.LookAt(streetLookAt);
                break;

            case Phase.KredikShawAerial:
                orbitAngle = 0f;
                UpdateAerialPosition();
                break;

            case Phase.TitleHold:
                // Continue from current aerial position — no snap
                Vector3 dir = transform.position - aerialCenter;
                orbitAngle = Mathf.Atan2(dir.z, dir.x);
                break;
        }
    }

    void Update()
    {
        phaseTimer += Time.deltaTime;

        switch (currentPhase)
        {
            case Phase.MistyField:
                // Static shot with tiny drift
                ApplyDrift(fieldPosition, fieldLookAt);
                break;

            case Phase.LuthadelStreets:
                // Static shot with tiny drift
                ApplyDrift(streetPosition, streetLookAt);
                break;

            case Phase.KredikShawAerial:
                // Slow orbit — the only phase with real movement
                orbitAngle += aerialOrbitSpeed * Time.deltaTime;
                UpdateAerialPosition();
                break;

            case Phase.TitleHold:
                // Continue orbiting + slow zoom
                orbitAngle += aerialOrbitSpeed * Time.deltaTime;
                UpdateAerialPosition();
                if (cam != null)
                {
                    float t = Mathf.Clamp01(phaseTimer / titleZoomDuration);
                    cam.fieldOfView = Mathf.Lerp(titleZoomStart, titleZoomEnd, t);
                }
                break;
        }
    }

    void UpdateAerialPosition()
    {
        // Slow descent over time
        float descentT = Mathf.Clamp01(phaseTimer / 30f);
        float h = Mathf.Lerp(aerialHeight, aerialHeight * 0.7f, descentT);
        float r = Mathf.Lerp(aerialRadius, aerialRadius * 0.75f, descentT);

        float x = aerialCenter.x + Mathf.Cos(orbitAngle) * r;
        float z = aerialCenter.z + Mathf.Sin(orbitAngle) * r;
        transform.position = new Vector3(x, h, z);
        transform.LookAt(aerialCenter + new Vector3(0f, 10f, 0f));
    }

    /// <summary>
    /// Extremely subtle position drift. The camera stays essentially still
    /// but has a tiny organic sway so it doesn't feel 100% locked.
    /// </summary>
    void ApplyDrift(Vector3 basePos, Vector3 lookAt)
    {
        float t = Time.time;
        float dx = Mathf.Sin(t * driftSpeed) * driftAmount;
        float dy = Mathf.Sin(t * driftSpeed * 0.7f) * driftAmount * 0.5f;

        transform.position = basePos + new Vector3(dx, dy, 0f);
        transform.LookAt(lookAt);
    }
}
