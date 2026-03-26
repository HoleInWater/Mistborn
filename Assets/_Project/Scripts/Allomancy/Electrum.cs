using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the Electrum Allomancy ability (Oracle).
/// Lore: Shows multiple "Future Shadows" of the player to prevent Atium-strikes.
/// </summary>
public class Electrum : MonoBehaviour
{
    [Header("Settings")]
    public int shadowCount = 3;
    public float shadowSpread = 1.5f;
    public float ghostAlpha = 0.3f;
    public Color electrumShadowColor = new Color(0.8f, 1f, 1f, 1f);

    private Allomancer allomancer;
    private List<GhostRenderer> shadows = new List<GhostRenderer>();
    private bool isBurning = false;

    void Start()
    {
        allomancer = GetComponentInParent<Allomancer>();
    }

    void Update()
    {
        bool wasBurning = isBurning;
        isBurning = allomancer != null && allomancer.IsBurning() && allomancer.GetCurrentMetal() == AllomancySkill.MetalType.Electrum;

        if (isBurning)
        {
            if (shadows.Count == 0) CreateShadows();
            UpdateShadows();
        }
        else if (wasBurning)
        {
            DestroyShadows();
        }
    }

    void CreateShadows()
    {
        for (int i = 0; i < shadowCount; i++)
        {
            GameObject go = new GameObject("ElectrumShadow_" + i);
            GhostRenderer gr = go.AddComponent<GhostRenderer>();
            gr.SetupGhost(gameObject, electrumShadowColor, ghostAlpha);
            shadows.Add(gr);
        }
    }

    void UpdateShadows()
    {
        Rigidbody rb = GetComponentInParent<Rigidbody>();
        Vector3 vel = rb != null ? rb.linearVelocity : transform.forward * 2f;

        for (int i = 0; i < shadows.Count; i++)
        {
            // Each shadow predicts a slightly different movement path
            float spreadTime = 0.3f + (i * 0.2f);
            Vector3 predictedPos = transform.position + (vel * spreadTime);
            
            // Add some lateral spread for "multiple futures" effect
            predictedPos += transform.right * Mathf.Sin(Time.time * 5f + i) * shadowSpread;

            shadows[i].UpdateTransform(predictedPos, transform.rotation);
        }
    }

    void DestroyShadows()
    {
        foreach (var ghost in shadows)
        {
            if (ghost != null) Destroy(ghost.gameObject);
        }
        shadows.Clear();
    }

    void OnDestroy() => DestroyShadows();
}
