using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allomancy HUD — shows metal reserve bars, active metal indicator, flare state.
/// Rendered as a compact vertical bar stack on the left side of screen.
/// Updates throttled to 10fps for performance.
/// </summary>
[PlayerComponent("UI", order: 10)]
public class AllomancyHUD : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform barContainer;
    public GameObject metalBarPrefab;
    public Text activeMetalText;
    public Text flareStatusText;
    public Text tinStateText;

    [Header("Colors")]
    public Color steelColor = new Color(0.3f, 0.5f, 1f);
    public Color ironColor = new Color(0.2f, 0.8f, 1f);
    public Color pewterColor = new Color(0.8f, 0.2f, 0.2f);
    public Color tinColor = new Color(1f, 1f, 0.5f);
    public Color zincColor = new Color(1f, 0.5f, 0f);
    public Color brassColor = new Color(0.2f, 0.9f, 0.5f);
    public Color copperColor   = new Color(0.2f, 0.8f, 0.2f);
    public Color bronzeColor   = new Color(0.8f, 0.3f, 0.8f);
    // Upper 8 — God Metals and Alloys
    public Color atiumColor    = new Color(0.55f, 0.55f, 0.70f); // dark silver-blue
    public Color malatiumColor = new Color(0.50f, 0.28f, 0.62f); // deep violet
    public Color goldColor     = new Color(1.0f,  0.80f, 0.10f); // warm gold
    public Color electrumColor = new Color(0.90f, 0.90f, 0.40f); // pale electrum
    public Color aluminumColor = new Color(0.90f, 0.92f, 0.96f); // bright silver-white
    public Color duraluminColor= new Color(0.45f, 0.75f, 1.00f); // silver-blue
    public Color bendalloyColor= new Color(0.90f, 0.58f, 0.18f); // amber
    public Color cadmiumColor  = new Color(0.20f, 0.72f, 0.70f); // teal
    public Color defaultColor  = Color.white;

    [Header("Settings")]
    public float barWidth = 8f;
    public float barHeight = 40f;
    public float barSpacing = 2f;

    private Allomancer allomancer;
    private Tin tinComponent;
    private Image[] metalBars;
    private Text[] metalLabels;
    private float updateTimer;
    private const float UPDATE_INTERVAL = 0.1f;

    private static readonly string[] metalNames = {
        "Stl", "Irn", "Pew", "Tin", "Znc", "Brs", "Cop", "Brz",
        "Ati", "Mal", "Gld", "Elc", "Alm", "Dur", "Ben", "Cad"
    };

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            allomancer = player.GetComponent<Allomancer>();
            tinComponent = player.GetComponent<Tin>();
        }

        CreateBars();
    }

    void CreateBars()
    {
        if (barContainer == null || allomancer == null) return;

        metalBars = new Image[16];
        metalLabels = new Text[16];

        for (int i = 0; i < 16; i++)
        {
            if (metalBarPrefab != null)
            {
                GameObject bar = Instantiate(metalBarPrefab, barContainer);
                metalBars[i] = bar.GetComponentInChildren<Image>();
            }
            else
            {
                // Create simple bar at runtime
                GameObject barObj = new GameObject($"MetalBar_{metalNames[i]}");
                barObj.transform.SetParent(barContainer, false);

                RectTransform rt = barObj.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(barWidth, barHeight);
                rt.anchoredPosition = new Vector2(i * (barWidth + barSpacing), 0);

                // Background
                Image bg = barObj.AddComponent<Image>();
                bg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

                // Fill bar (child)
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(barObj.transform, false);
                RectTransform fillRt = fillObj.AddComponent<RectTransform>();
                fillRt.anchorMin = new Vector2(0, 0);
                fillRt.anchorMax = new Vector2(1, 1);
                fillRt.offsetMin = Vector2.zero;
                fillRt.offsetMax = Vector2.zero;
                fillRt.pivot = new Vector2(0.5f, 0);

                Image fill = fillObj.AddComponent<Image>();
                fill.color = GetMetalColor(i);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Vertical;
                fill.fillAmount = 1f;
                metalBars[i] = fill;

                // Label
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(barObj.transform, false);
                RectTransform labelRt = labelObj.AddComponent<RectTransform>();
                labelRt.anchoredPosition = new Vector2(0, -12);
                labelRt.sizeDelta = new Vector2(barWidth + 4, 10);

                Text label = labelObj.AddComponent<Text>();
                label.text = metalNames[i];
                label.fontSize = 7;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                metalLabels[i] = label;
            }
        }
    }

    void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer > 0f || allomancer == null) return;
        updateTimer = UPDATE_INTERVAL;

        for (int i = 0; i < 16 && i < metalBars.Length && i < allomancer.metalReserves.Length; i++)
        {
            if (metalBars[i] == null) continue;
            float reserve = allomancer.metalReserves[i];
            metalBars[i].fillAmount = reserve / 100f;

            // Pulse low reserves
            if (reserve < 20f && reserve > 0f)
            {
                float pulse = Mathf.Sin(Time.time * 5f) * 0.3f + 0.7f;
                Color c = GetMetalColor(i);
                metalBars[i].color = c * pulse;
            }
        }

        // Active metal text
        if (activeMetalText != null)
        {
            var metal = allomancer.GetCurrentMetal();
            activeMetalText.text = metal.ToString();
        }

        // Flare status
        if (flareStatusText != null)
        {
            if (FlareManager.Instance != null && FlareManager.Instance.IsFlaring)
                flareStatusText.text = $"FLARING x{FlareManager.Instance.FlareMultiplier:F1}";
            else
                flareStatusText.text = "";
        }

        // Tin state indicator
        if (tinStateText != null)
        {
            if (tinComponent != null && tinComponent.CurrentState != Tin.TinState.Off)
                tinStateText.text = tinComponent.CurrentState == Tin.TinState.Flaring
                    ? "TIN: FLARING" : "TIN: BURNING";
            else
                tinStateText.text = "";
        }
    }

    Color GetMetalColor(int index)
    {
        switch (index)
        {
            case 0: return steelColor;
            case 1: return ironColor;
            case 2: return pewterColor;
            case 3: return tinColor;
            case 4: return zincColor;
            case 5: return brassColor;
            case 6:  return copperColor;
            case 7:  return bronzeColor;
            case 8:  return atiumColor;
            case 9:  return malatiumColor;
            case 10: return goldColor;
            case 11: return electrumColor;
            case 12: return aluminumColor;
            case 13: return duraluminColor;
            case 14: return bendalloyColor;
            case 15: return cadmiumColor;
            default: return defaultColor;
        }
    }
}
