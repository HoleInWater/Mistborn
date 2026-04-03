/* TitleSequenceMaterialOverride.cs
 *
 * Applies color to objects via MaterialPropertyBlock — works on ANY render pipeline
 * (HDRP, URP, Standard) without creating new materials or causing pink.
 *
 * MaterialPropertyBlock overrides rendering properties at the renderer level
 * without touching the material reference. The base material stays as the
 * pipeline default (which always renders correctly), and we just override
 * the color on top.
 *
 * Attached automatically by TitleSequenceSceneBuilder.ApplyColor().
 */

using UnityEngine;

[ExecuteAlways]
public class TitleSequenceMaterialOverride : MonoBehaviour
{
    public Color overrideColor = Color.white;
    public bool isEmissive = false;
    public Color emissiveColor = Color.black;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    void OnEnable()
    {
        Apply();
    }

    void Start()
    {
        Apply();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update in editor when Inspector values change
        Apply();
    }

    void Update()
    {
        // Keep applying in editor so Scene view shows correct colors
        if (!Application.isPlaying)
            Apply();
    }
#endif

    public void Apply()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_renderer == null) return;

        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_mpb);

        // Set color on every known property name across all pipelines
        _mpb.SetColor("_BaseColor", overrideColor);    // HDRP + URP
        _mpb.SetColor("_Color", overrideColor);         // Standard
        _mpb.SetColor("_UnlitColor", overrideColor);    // Unlit shaders

        if (isEmissive)
        {
            _mpb.SetColor("_EmissiveColor", emissiveColor);   // HDRP
            _mpb.SetColor("_EmissionColor", emissiveColor);   // Standard + URP
        }

        _renderer.SetPropertyBlock(_mpb);
    }
}
