using UnityEngine;
using System.Collections;

/// <summary>
/// Briefly flashes the enemy's renderers red when they take damage.
/// Uses MaterialPropertyBlock so the shared material is never modified.
/// Works with both Standard, URP/Lit, and HDRP/Lit shaders.
/// </summary>
public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor  = new Color(1f, 0.1f, 0.1f, 1f);
    public float flashDuration = 0.12f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock;
    private Coroutine activeFlash;

    // Common property names across Standard / URP / HDRP shaders
    private static readonly int ColorProp    = Shader.PropertyToID("_BaseColor");   // URP / HDRP
    private static readonly int ColorPropStd = Shader.PropertyToID("_Color");       // Standard
    private static readonly int EmissionProp = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Called by EnemyAI.TakeDamage() to trigger the red flash.
    /// Safe to call while a previous flash is still running — restarts it.
    /// </summary>
    public void Flash()
    {
        if (renderers == null || renderers.Length == 0) return;
        if (activeFlash != null) StopCoroutine(activeFlash);
        activeFlash = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        SetColor(flashColor);
        yield return new WaitForSecondsRealtime(flashDuration);
        ClearColor();
        activeFlash = null;
    }

    private void SetColor(Color color)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor(ColorProp, color);
            propBlock.SetColor(ColorPropStd, color);
            propBlock.SetColor(EmissionProp, color * 0.5f);
            r.SetPropertyBlock(propBlock);
        }
    }

    private void ClearColor()
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
    }
}
