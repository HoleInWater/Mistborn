using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar that floats above an enemy.
/// Shows briefly after taking damage, fades when full or dead.
///
/// AUTO-CREATE: if no Canvas / Image references are assigned in the Inspector
/// (e.g. when spawned by EnemyFactory), Start() builds the full Canvas +
/// background + fill Image hierarchy at runtime — no prefab setup needed.
///
/// Placement: attach this script to a child GameObject of the enemy (e.g.
/// "HealthBar" child at localPosition (0, 2.5, 0)). GetComponentInParent
/// finds EnemyHealth automatically.
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI References (auto-created if null)")]
    public Image healthFill;
    public Image healthBackground;
    public Canvas worldCanvas;

    [Header("Display Settings")]
    public float showDuration  = 3f;
    public float fadeSpeed     = 2f;
    public Vector3 offset      = new Vector3(0, 0.4f, 0); // offset relative to this object
    public bool alwaysShow     = false;

    [Header("Colors")]
    public Color fullHealthColor = new Color(0.25f, 0.85f, 0.25f);
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor  = new Color(0.9f, 0.15f, 0.15f);

    // ── Private ───────────────────────────────────────────────────────────────

    private EnemyHealth enemyHealth;
    private float lastDamageTime = -99f;
    private float currentAlpha   = 0f;
    private float lastHealthPct  = 1f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();

        if (worldCanvas == null || healthFill == null)
            BuildRuntimeCanvas();

        // worldCamera may not be available yet — retry in LateUpdate
        if (worldCanvas != null && Camera.main != null)
            worldCanvas.worldCamera = Camera.main;

        SetAlpha(0f);
    }

    void LateUpdate()
    {
        if (enemyHealth == null) return;

        // Assign camera if it wasn't ready at Start
        if (worldCanvas != null && worldCanvas.worldCamera == null && Camera.main != null)
            worldCanvas.worldCamera = Camera.main;

        float maxHp = enemyHealth.GetMaxHealth();
        if (maxHp <= 0f) return;

        float hpPct = Mathf.Clamp01(enemyHealth.GetCurrentHealth() / maxHp);

        // Detect damage event
        if (hpPct < lastHealthPct - 0.005f)
        {
            lastDamageTime = Time.time;
            currentAlpha   = 1f;
        }
        lastHealthPct = hpPct;

        // Update fill
        if (healthFill != null)
        {
            healthFill.fillAmount = hpPct;
            Color barColor = hpPct > 0.6f ? fullHealthColor
                           : hpPct > 0.3f ? halfHealthColor
                           : lowHealthColor;
            barColor.a = currentAlpha;
            healthFill.color = barColor;
        }

        // Fade logic
        if (!alwaysShow)
        {
            if (Time.time - lastDamageTime > showDuration)
                currentAlpha = Mathf.Max(0f, currentAlpha - fadeSpeed * Time.deltaTime);
        }
        else
        {
            currentAlpha = (hpPct < 1f && !enemyHealth.isDead) ? 1f : 0f;
        }

        SetAlpha(currentAlpha);

        // Billboard — rotate to face camera
        if (Camera.main != null)
        {
            Vector3 lookDir = transform.position - Camera.main.transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // Track above enemy
        if (transform.parent != null)
            transform.position = transform.parent.position + offset;
    }

    // ── Canvas auto-creation ──────────────────────────────────────────────────

    void BuildRuntimeCanvas()
    {
        // Canvas — world space, 0.8m wide × 0.1m tall
        // (100 × 12 canvas units at 0.01 local scale = 1m × 0.12m)
        GameObject canvasObj = new GameObject("WorldCanvas");
        canvasObj.transform.SetParent(transform, false);

        worldCanvas            = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRT  = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta      = new Vector2(100f, 12f);
        canvasRT.localPosition  = Vector3.zero;
        canvasRT.localScale     = Vector3.one * 0.01f;
        canvasRT.localRotation  = Quaternion.identity;

        // Dark background panel
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);

        healthBackground       = bgObj.AddComponent<Image>();
        healthBackground.color = new Color(0.05f, 0.05f, 0.05f, 0.7f);

        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Health fill — horizontal fill type so it shrinks from the right
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform, false);

        healthFill             = fillObj.AddComponent<Image>();
        healthFill.color       = fullHealthColor;
        healthFill.type        = Image.Type.Filled;
        healthFill.fillMethod  = Image.FillMethod.Horizontal;
        healthFill.fillOrigin  = (int)Image.OriginHorizontal.Left;
        healthFill.fillAmount  = 1f;

        RectTransform fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(1.5f, 1.5f);
        fillRT.offsetMax = new Vector2(-1.5f, -1.5f);
    }

    // ── Alpha helper ──────────────────────────────────────────────────────────

    void SetAlpha(float alpha)
    {
        if (healthBackground != null)
        {
            Color c = healthBackground.color;
            c.a = alpha * 0.7f;
            healthBackground.color = c;
        }
        // Fill alpha is handled inline with the color in LateUpdate
    }
}
