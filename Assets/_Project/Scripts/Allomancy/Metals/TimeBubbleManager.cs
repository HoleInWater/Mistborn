using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages creation/destruction of Bendalloy (speed) and Cadmium (slow) time bubbles.
/// Uses PHYSICS-MATH-BOOK.md Section 9 for time dilation factors.
/// Cadmium: τ_slow ≈ 0.1 (10x slower). Bendalloy: τ_fast ≈ 10 (10x faster).
/// </summary>
public class TimeBubbleManager : MonoBehaviour
{
    public static TimeBubbleManager Instance { get; private set; }

    [Header("Bubble Limits")]
    public int maxSimultaneousBubbles = 3;

    private List<TimeBubble> activeBubbles = new List<TimeBubble>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Clean destroyed bubbles
        activeBubbles.RemoveAll(b => b == null);
    }

    public TimeBubble CreateBubble(Vector3 position, float radius, float timeScale, Color color, string name)
    {
        if (activeBubbles.Count >= maxSimultaneousBubbles) return null;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = Vector3.one * radius * 2f;

        // Transparent material
        Renderer r = go.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(color.r, color.g, color.b, 0f);
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        r.material = mat;

        TimeBubble bubble = go.AddComponent<TimeBubble>();
        bubble.timeScaleMultiplier = timeScale;

        activeBubbles.Add(bubble);
        return bubble;
    }

    /// <summary>
    /// Create a Bendalloy speed bubble (PHYSICS-MATH-BOOK.md Section 9: τ_fast ≈ 10).
    /// </summary>
    public TimeBubble CreateSpeedBubble(Vector3 position, float radius, float flareMultiplier = 1f)
    {
        float timeScale = AllomancyPhysicsFormulas.BENDALLOY_TAU * flareMultiplier;
        timeScale = Mathf.Min(timeScale, 20f); // Safety cap
        return CreateBubble(position, radius, timeScale,
            new Color(1f, 0.9f, 0.4f), "BendalloySpeedBubble");
    }

    /// <summary>
    /// Create a Cadmium slow bubble (PHYSICS-MATH-BOOK.md Section 9: τ_slow ≈ 0.1).
    /// </summary>
    public TimeBubble CreateSlowBubble(Vector3 position, float radius, float flareMultiplier = 1f)
    {
        float timeScale = AllomancyPhysicsFormulas.CADMIUM_TAU / flareMultiplier;
        timeScale = Mathf.Max(timeScale, 0.01f); // Safety cap
        return CreateBubble(position, radius, timeScale,
            new Color(0.2f, 0.4f, 1f), "CadmiumSlowBubble");
    }

    public void DestroyBubble(TimeBubble bubble)
    {
        if (bubble != null)
        {
            activeBubbles.Remove(bubble);
            bubble.Shutdown();
        }
    }

    public void DestroyAllBubbles()
    {
        foreach (var b in activeBubbles)
            if (b != null) b.Shutdown();
        activeBubbles.Clear();
    }

    public int GetActiveBubbleCount() => activeBubbles.Count;
}
