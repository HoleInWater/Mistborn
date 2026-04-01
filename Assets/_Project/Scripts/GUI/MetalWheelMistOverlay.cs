using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fullscreen mist overlay shown whenever the Metal Wheel is open.
/// Fades in a dark background and drifting mist ribbons so the game
/// world is obscured — you can't exploit frozen time to scout enemies.
///
/// Self-building: no prefab or scene setup required.
/// Auto-created by SceneBootstrap; call Show() / Hide() from MetalWheelController.
///
/// SORTING ORDER: sits at 90 — above game HUD (minimap at 100 is a uGUI overlay,
/// so this will be BELOW the minimap when both are visible, which is intentional).
/// Metal Wheel canvas must be sorted ABOVE 90 in the Inspector.
/// </summary>
public class MetalWheelMistOverlay : MonoBehaviour
{
    public static MetalWheelMistOverlay Instance { get; private set; }

    [Header("Background")]
    public Color  backgroundColor = new Color(0.02f, 0.02f, 0.06f, 1f);
    public float  backgroundAlpha = 0.84f;

    [Header("Mist")]
    public int   mistLayerCount = 10;
    public Color mistColor      = new Color(0.60f, 0.72f, 0.88f, 1f);

    [Header("Animation")]
    public float fadeSpeed = 8f;   // alpha units / unscaled second

    // ── Runtime state ─────────────────────────────────────────────────────────

    private Canvas          _canvas;
    private Image           _background;
    private Image[]         _mistImages;
    private RectTransform[] _mistRTs;
    private float[]         _mistSpeeds;
    private float[]         _mistBaseAlpha;
    private float           _targetAlpha;
    private float           _currentAlpha;

    const float MIST_STRIP_WIDTH = 2800f;   // px — wider than any reasonable resolution

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Build();
    }

    void Build()
    {
        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasObj = new GameObject("MistOverlayCanvas");
        DontDestroyOnLoad(canvasObj);
        canvasObj.transform.SetParent(transform, false);

        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 90;          // above HUD, below metal wheel canvas

        canvasObj.AddComponent<CanvasScaler>();
        // No GraphicRaycaster — overlay must not absorb pointer events

        // ── Dark background ──────────────────────────────────────────────────
        var bgObj = new GameObject("BG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        _background       = bgObj.AddComponent<Image>();
        _background.color = new Color(backgroundColor.r, backgroundColor.g,
                                      backgroundColor.b, 0f);
        var bgRT          = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin    = Vector2.zero;
        bgRT.anchorMax    = Vector2.one;
        bgRT.offsetMin    = bgRT.offsetMax = Vector2.zero;

        // ── Mist ribbons ─────────────────────────────────────────────────────
        _mistImages    = new Image[mistLayerCount];
        _mistRTs       = new RectTransform[mistLayerCount];
        _mistSpeeds    = new float[mistLayerCount];
        _mistBaseAlpha = new float[mistLayerCount];

        for (int i = 0; i < mistLayerCount; i++)
        {
            var obj = new GameObject($"Mist{i}");
            obj.transform.SetParent(canvasObj.transform, false);

            var img = obj.AddComponent<Image>();
            img.color = new Color(mistColor.r, mistColor.g, mistColor.b, 0f);
            _mistImages[i] = img;

            // Spread ribbons evenly vertically with randomised band heights
            float yFrac  = (i + 0.5f) / mistLayerCount + Random.Range(-0.04f, 0.04f);
            float hFrac  = Random.Range(0.04f, 0.20f);
            yFrac        = Mathf.Clamp01(yFrac);

            var rt           = obj.GetComponent<RectTransform>();
            rt.anchorMin     = new Vector2(0f, Mathf.Clamp01(yFrac - hFrac * 0.5f));
            rt.anchorMax     = new Vector2(0f, Mathf.Clamp01(yFrac + hFrac * 0.5f));
            rt.pivot         = new Vector2(0f, 0.5f);
            rt.sizeDelta     = new Vector2(MIST_STRIP_WIDTH, 0f);
            rt.anchoredPosition = new Vector2(Random.Range(-MIST_STRIP_WIDTH, 0f), 0f);

            _mistRTs[i]       = rt;
            _mistSpeeds[i]    = Random.Range(20f, 85f) * (Random.value > 0.25f ? 1f : -1f);
            _mistBaseAlpha[i] = Random.Range(0.04f, 0.14f);
        }

        _canvas.gameObject.SetActive(false);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha,
                                           Time.unscaledDeltaTime * fadeSpeed);

        // Dark background
        var bg = _background.color;
        bg.a   = _currentAlpha * backgroundAlpha;
        _background.color = bg;

        // Drift and pulse each ribbon
        float screenW = Screen.width;
        for (int i = 0; i < mistLayerCount; i++)
        {
            if (_mistRTs[i] == null) continue;

            // Translate
            var pos = _mistRTs[i].anchoredPosition;
            pos.x  += _mistSpeeds[i] * Time.unscaledDeltaTime;

            if (_mistSpeeds[i] > 0f && pos.x > screenW)           pos.x = -MIST_STRIP_WIDTH;
            else if (_mistSpeeds[i] < 0f && pos.x < -MIST_STRIP_WIDTH) pos.x = screenW;

            _mistRTs[i].anchoredPosition = pos;

            // Gentle alpha pulse so each ribbon breathes independently
            float pulse = _mistBaseAlpha[i]
                        * (0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 0.45f + i * 1.05f));

            var c = _mistImages[i].color;
            c.a   = _currentAlpha * pulse;
            _mistImages[i].color = c;
        }

        // Deactivate canvas once fully faded out (saves rendering cost)
        if (_currentAlpha <= 0f && _targetAlpha == 0f)
            _canvas.gameObject.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()
    {
        _canvas.gameObject.SetActive(true);
        _targetAlpha = 1f;
    }

    public void Hide() => _targetAlpha = 0f;
}
