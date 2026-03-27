using UnityEngine;

/// <summary>
/// Visual trail effect when pushing or pulling — shows a blue energy line
/// from player chest to the target metal during the impulse.
/// Fades out quickly after the push/pull ends.
/// </summary>
public class PushPullTrail : MonoBehaviour
{
    public static PushPullTrail Instance { get; private set; }

    [Header("Settings")]
    public float trailDuration = 0.3f;
    public float trailWidth = 0.04f;

    [Header("Colors")]
    public Color pushColor = new Color(0.3f, 0.5f, 1f, 0.7f);
    public Color pullColor = new Color(0.2f, 0.8f, 1f, 0.7f);

    private LineRenderer pushLine;
    private LineRenderer pullLine;
    private float pushTimer;
    private float pullTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        pushLine = CreateLine("PushTrail", pushColor);
        pullLine = CreateLine("PullTrail", pullColor);
    }

    LineRenderer CreateLine(string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.startWidth = trailWidth;
        lr.endWidth = trailWidth * 0.3f;
        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        lr.material = new Material(shader);
        lr.startColor = color;
        lr.endColor = color * 0.5f;
        lr.positionCount = 0;
        lr.useWorldSpace = true;
        return lr;
    }

    void Update()
    {
        // Fade push trail
        if (pushTimer > 0f)
        {
            pushTimer -= Time.deltaTime;
            float alpha = pushTimer / trailDuration;
            Color c = pushColor;
            c.a *= alpha;
            pushLine.startColor = c;
            pushLine.endColor = c * 0.5f;
            pushLine.startWidth = trailWidth * alpha;

            if (pushTimer <= 0f) pushLine.positionCount = 0;
        }

        // Fade pull trail
        if (pullTimer > 0f)
        {
            pullTimer -= Time.deltaTime;
            float alpha = pullTimer / trailDuration;
            Color c = pullColor;
            c.a *= alpha;
            pullLine.startColor = c;
            pullLine.endColor = c * 0.5f;
            pullLine.startWidth = trailWidth * alpha;

            if (pullTimer <= 0f) pullLine.positionCount = 0;
        }
    }

    /// <summary>
    /// Show a push trail from player to target. Called by SteelPush.
    /// </summary>
    public void ShowPushTrail(Vector3 from, Vector3 to)
    {
        if (pushLine == null) return;
        pushLine.positionCount = 2;
        pushLine.SetPosition(0, from);
        pushLine.SetPosition(1, to);
        pushLine.startColor = pushColor;
        pushLine.endColor = pushColor * 0.5f;
        pushLine.startWidth = trailWidth;
        pushTimer = trailDuration;
    }

    /// <summary>
    /// Show a pull trail from target to player. Called by IronPull.
    /// </summary>
    public void ShowPullTrail(Vector3 from, Vector3 to)
    {
        if (pullLine == null) return;
        pullLine.positionCount = 2;
        pullLine.SetPosition(0, from);
        pullLine.SetPosition(1, to);
        pullLine.startColor = pullColor;
        pullLine.endColor = pullColor * 0.5f;
        pullLine.startWidth = trailWidth;
        pullTimer = trailDuration;
    }
}
