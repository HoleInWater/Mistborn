/* HitstopManager.cs
 *
 * Manages hitstop (brief time freeze on impact) and time slow effects.
 * Called by combat systems to create impactful hits.
 *
 * Types:
 *   - Light hit:  50ms freeze at timeScale 0.1
 *   - Heavy hit:  100ms freeze at timeScale 0.05
 *   - Parry:      80ms freeze at timeScale 0.0 (full stop)
 *   - Pewter slam: 150ms at 0.1 (dramatic landing)
 *   - Duralumin:  200ms at 0.02 (massive burst)
 */

using UnityEngine;
using System.Collections;

public class HitstopManager : MonoBehaviour
{
    public static HitstopManager Instance { get; private set; }

    [Header("Settings")]
    public float lightHitDuration = 0.05f;
    public float heavyHitDuration = 0.1f;
    public float parryDuration = 0.08f;
    public float slamDuration = 0.15f;
    public float duraluminDuration = 0.2f;

    private Coroutine _activeHitstop;
    private bool _inHitstop;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void LightHit()   => DoHitstop(lightHitDuration, 0.1f);
    public void HeavyHit()   => DoHitstop(heavyHitDuration, 0.05f);
    public void ParryHit()   => DoHitstop(parryDuration, 0.0f);
    public void PewterSlam() => DoHitstop(slamDuration, 0.1f);
    public void DuraluminBurst() => DoHitstop(duraluminDuration, 0.02f);

    public void DoHitstop(float duration, float timeScale)
    {
        if (_inHitstop) return; // Don't stack hitstops
        if (_activeHitstop != null) StopCoroutine(_activeHitstop);
        _activeHitstop = StartCoroutine(HitstopCoroutine(duration, timeScale));
    }

    IEnumerator HitstopCoroutine(float duration, float targetScale)
    {
        _inHitstop = true;
        float originalScale = Time.timeScale;

        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * targetScale;

        // Wait using unscaled time (so the hitstop actually lasts the right duration)
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalScale;
        Time.fixedDeltaTime = 0.02f * originalScale;
        _inHitstop = false;
        _activeHitstop = null;
    }

    public bool InHitstop => _inHitstop;
}
