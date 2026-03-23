using UnityEngine;

/// <summary>
/// Marks an object as a source of sensory input for Allomancers burning Tin.
/// Used to trigger sensory overload.
/// </summary>
public class SensorySource : MonoBehaviour
{
    public static System.Collections.Generic.List<SensorySource> ActiveSources = new System.Collections.Generic.List<SensorySource>();

    public enum SourceType
    {
        BrightLight,
        LoudNoise
    }

    [Header("Settings")]
    public SourceType type = SourceType.BrightLight;
    
    [Tooltip("Intensity of the sensory input (0-1)")]
    [Range(0f, 1f)]
    public float intensity = 0.5f;

    [Tooltip("Maximum distance at which this source affects an Allomancer")]
    public float radius = 10f;

    [Tooltip("How much the effect falls off over distance (1 = linear)")]
    public float falloff = 1f;

    private void OnEnable() => ActiveSources.Add(this);
    private void OnDisable() => ActiveSources.Remove(this);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = type == SourceType.BrightLight ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
