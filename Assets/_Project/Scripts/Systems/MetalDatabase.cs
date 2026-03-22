using UnityEngine;

public class MetalDatabase : MonoBehaviour
{
    public static MetalDatabase Instance { get; private set; }

    [Header("Metal Information")]
    public MetalInfo[] metals;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public MetalInfo GetMetalInfo(MetalType type)
    {
        foreach (var metal in metals)
        {
            if (metal.type == type) return metal;
        }
        return null;
    }

    public MetalCategory GetCategory(MetalType type)
    {
        switch (type)
        {
            case MetalType.Iron:
            case MetalType.Steel:
            case MetalType.Tin:
            case MetalType.Pewter:
                return MetalCategory.Physical;
            case MetalType.Zinc:
            case MetalType.Brass:
            case MetalType.Copper:
            case MetalType.Bronze:
                return MetalCategory.Mental;
            case MetalType.Aluminum:
            case MetalType.Duralumin:
            case MetalType.Chromium:
            case MetalType.Nicrosil:
                return MetalCategory.Enhancement;
            case MetalType.Gold:
            case MetalType.Electrum:
            case MetalType.Cadmium:
            case MetalType.Bendalloy:
                return MetalCategory.Temporal;
            case MetalType.Atium:
            case MetalType.Malatium:
            case MetalType.Lerasium:
            case MetalType.Harmonium:
            case MetalType.Nalatium:
                return MetalCategory.GodMetal;
            default:
                return MetalCategory.Physical;
        }
    }
}

[System.Serializable]
public class MetalInfo
{
    public MetalType type;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public float baseCost = 10f;
    public float baseCooldown = 1f;
    public float baseRange = 100f;
    public MetalCategory category;
}
