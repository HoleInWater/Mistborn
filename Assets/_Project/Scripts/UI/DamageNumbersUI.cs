using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Floating damage numbers that pop up when enemies or the player take damage.
/// Object-pooled for performance. Color-coded by damage type.
/// </summary>
public class DamageNumbersUI : MonoBehaviour
{
    public static DamageNumbersUI Instance { get; private set; }

    [Header("Settings")]
    public GameObject damageNumberPrefab;
    public Canvas worldCanvas;
    public int poolSize = 30;
    public float floatSpeed = 1.5f;
    public float floatHeight = 1f;
    public float lifetime = 1f;
    public float scalePunch = 1.3f;

    [Header("Colors")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.yellow;
    public Color allomancyDamageColor = new Color(0.3f, 0.5f, 1f);
    public Color healColor = Color.green;
    public Color pewterDamageColor = new Color(0.8f, 0.2f, 0.2f);

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        if (damageNumberPrefab == null || worldCanvas == null) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(damageNumberPrefab, worldCanvas.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void ShowDamage(Vector3 worldPos, float amount, DamageType type = DamageType.Normal)
    {
        GameObject obj = GetFromPool();
        if (obj == null) return;

        obj.transform.position = worldPos + Vector3.up * 1.5f;
        obj.SetActive(true);

        Text text = obj.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = type == DamageType.Heal
                ? $"+{Mathf.RoundToInt(amount)}"
                : $"{Mathf.RoundToInt(amount)}";

            text.color = GetColor(type);
            text.fontSize = amount > 50 ? 28 : (amount > 25 ? 22 : 18);
        }

        StartCoroutine(AnimateNumber(obj));
    }

    IEnumerator AnimateNumber(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = startPos + Vector3.up * floatHeight;

        // Scale punch
        obj.transform.localScale = Vector3.one * scalePunch;

        float elapsed = 0f;
        Text text = obj.GetComponentInChildren<Text>();
        Color startColor = text != null ? text.color : Color.white;

        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime;

            // Float up
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Scale down
            obj.transform.localScale = Vector3.Lerp(Vector3.one * scalePunch, Vector3.one * 0.5f, t);

            // Fade out
            if (text != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                text.color = c;
            }

            // Billboard — face camera
            if (Camera.main != null)
                obj.transform.rotation = Camera.main.transform.rotation;

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReturnToPool(obj);
    }

    GameObject GetFromPool()
    {
        if (pool.Count > 0) return pool.Dequeue();
        return null;
    }

    void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    Color GetColor(DamageType type)
    {
        switch (type)
        {
            case DamageType.Critical: return criticalDamageColor;
            case DamageType.Allomancy: return allomancyDamageColor;
            case DamageType.Pewter: return pewterDamageColor;
            case DamageType.Heal: return healColor;
            default: return normalDamageColor;
        }
    }

    public enum DamageType { Normal, Critical, Allomancy, Pewter, Heal }
}
