/* TitleObjectSway.cs
 *
 * Gently sways objects — banners fluttering, signs creaking, chains swinging,
 * lanterns swinging in the wind. Adds life to the static scene.
 *
 * Attach to any object that should move subtly.
 */

using UnityEngine;

[ExecuteAlways]
public class TitleObjectSway : MonoBehaviour
{
    public enum SwayType { Banner, HangingSign, Chain, Lantern, Cloth, Pendulum }

    public SwayType swayType = SwayType.Banner;

    [Header("Override (0 = auto from type)")]
    public float swayAmount;
    public float swaySpeed;
    public Vector3 swayAxis = Vector3.forward;

    private float _seed;
    private Vector3 _basePosition;
    private Quaternion _baseRotation;

    void OnEnable()
    {
        _seed = Random.Range(0f, 100f);
        _basePosition = transform.localPosition;
        _baseRotation = transform.localRotation;

        if (swayAmount <= 0f)
        {
            switch (swayType)
            {
                case SwayType.Banner:
                    swayAmount = 5f;
                    swaySpeed = 0.8f;
                    swayAxis = Vector3.forward;
                    break;
                case SwayType.HangingSign:
                    swayAmount = 3f;
                    swaySpeed = 0.5f;
                    swayAxis = Vector3.forward;
                    break;
                case SwayType.Chain:
                    swayAmount = 2f;
                    swaySpeed = 0.4f;
                    swayAxis = Vector3.right;
                    break;
                case SwayType.Lantern:
                    swayAmount = 2.5f;
                    swaySpeed = 0.6f;
                    swayAxis = new Vector3(1f, 0f, 0.5f).normalized;
                    break;
                case SwayType.Cloth:
                    swayAmount = 4f;
                    swaySpeed = 1.0f;
                    swayAxis = Vector3.forward;
                    break;
                case SwayType.Pendulum:
                    // Physical pendulum: T = 2π√(I / mgd)
                    // For a gibbet cage: I ≈ 0.5 kg·m², m ≈ 5kg, d ≈ 0.3m
                    // T ≈ 2π√(0.5 / (5 × 9.81 × 0.3)) ≈ 1.16s → speed ≈ 5.4 rad/s
                    swayAmount = 6f;
                    swaySpeed = 0.85f; // 1/T ≈ 0.86 Hz
                    swayAxis = Vector3.forward;
                    break;
            }
        }
    }

    void Update()
    {
        float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;

        // Multi-frequency for organic feel
        float sway = Mathf.Sin(t * swaySpeed + _seed) * 0.6f
                    + Mathf.Sin(t * swaySpeed * 2.1f + _seed * 1.7f) * 0.3f
                    + Mathf.Sin(t * swaySpeed * 0.4f + _seed * 3.2f) * 0.1f;

        transform.localRotation = _baseRotation * Quaternion.AngleAxis(sway * swayAmount, swayAxis);

        // Slight position bob for hanging objects
        if (swayType == SwayType.Lantern || swayType == SwayType.HangingSign)
        {
            float bob = Mathf.Sin(t * swaySpeed * 0.7f + _seed) * 0.005f;
            transform.localPosition = _basePosition + new Vector3(0f, bob, 0f);
        }
    }
}
