using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar that floats above enemies.
/// Shows when enemy takes damage, fades when full or dead.
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    public Image healthFill;
    public Image healthBackground;
    public Canvas worldCanvas;

    [Header("Settings")]
    public float showDuration = 3f;
    public float fadeSpeed = 2f;
    public Vector3 offset = new Vector3(0, 2.2f, 0);
    public bool alwaysShow = false;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    private EnemyHealth enemyHealth;
    private float lastDamageTime;
    private float currentAlpha = 0f;
    private float lastHealthPercent = 1f;

    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();

        if (worldCanvas == null)
            worldCanvas = GetComponentInChildren<Canvas>();

        if (worldCanvas != null)
            worldCanvas.worldCamera = Camera.main;

        SetAlpha(0f);
    }

    void LateUpdate()
    {
        if (enemyHealth == null) return;

        float hpPercent = enemyHealth.GetCurrentHealth() / enemyHealth.GetMaxHealth();

        // Detect damage
        if (hpPercent < lastHealthPercent - 0.01f)
        {
            lastDamageTime = Time.time;
            currentAlpha = 1f;
        }
        lastHealthPercent = hpPercent;

        // Update fill
        if (healthFill != null)
        {
            healthFill.fillAmount = hpPercent;
            healthFill.color = hpPercent > 0.6f ? fullHealthColor
                             : hpPercent > 0.3f ? halfHealthColor
                             : lowHealthColor;
        }

        // Fade logic
        if (!alwaysShow)
        {
            if (Time.time - lastDamageTime > showDuration)
                currentAlpha = Mathf.Max(0f, currentAlpha - fadeSpeed * Time.deltaTime);
        }
        else
        {
            currentAlpha = hpPercent < 1f ? 1f : 0f;
        }

        SetAlpha(currentAlpha);

        // Billboard
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;

        // Position above enemy
        if (transform.parent != null)
            transform.position = transform.parent.position + offset;
    }

    void SetAlpha(float alpha)
    {
        if (healthFill != null)
        {
            Color c = healthFill.color;
            c.a = alpha;
            healthFill.color = c;
        }
        if (healthBackground != null)
        {
            Color c = healthBackground.color;
            c.a = alpha * 0.5f;
            healthBackground.color = c;
        }
    }
}
