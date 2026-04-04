using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

#if UNITY_EDITOR
public class MetalWheelSetupWizard : EditorWindow
{
    [MenuItem("Ashwalker/Player/Generate Metal Wheel UI")]
    public static void GenerateWheelUI()
    {
        // 1. Create the Root Canvas
        GameObject rootCanvasGO = new GameObject("MetalWheelCanvas");
        Canvas canvas = rootCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = rootCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        rootCanvasGO.AddComponent<GraphicRaycaster>();
        CanvasGroup group = rootCanvasGO.AddComponent<CanvasGroup>();

        // 2. Attach our scripts
        MetalWheelController controller = rootCanvasGO.AddComponent<MetalWheelController>();
        rootCanvasGO.AddComponent<MetalWheelInputHandler>();
        rootCanvasGO.AddComponent<MetalWheelTimeManager>();
        rootCanvasGO.AddComponent<MetalWheelAudio>();

        // Link dependencies
        controller.wheelCanvasGroup = group;
        controller.timeManager = rootCanvasGO.GetComponent<MetalWheelTimeManager>();
        controller.inputHandler = rootCanvasGO.GetComponent<MetalWheelInputHandler>();
        controller.audioManager = rootCanvasGO.GetComponent<MetalWheelAudio>();

        // 3. Create Center UI
        GameObject centerGO = new GameObject("CenterElement");
        centerGO.transform.SetParent(rootCanvasGO.transform, false);
        Image centerImage = centerGO.AddComponent<Image>();
        centerImage.color = new Color(0, 0, 0, 0.65f); // Dark background
        centerImage.rectTransform.sizeDelta = new Vector2(80, 80);
        
        GameObject centerGlyphGO = new GameObject("CenterGlyph");
        centerGlyphGO.transform.SetParent(centerGO.transform, false);
        Image glyphImage = centerGlyphGO.AddComponent<Image>();
        glyphImage.rectTransform.sizeDelta = new Vector2(60, 60);

        GameObject centerTextGO = new GameObject("CenterText");
        centerTextGO.transform.SetParent(centerGO.transform, false);
        Text centerText = centerTextGO.AddComponent<Text>();
        centerText.alignment = TextAnchor.MiddleCenter;
        centerText.rectTransform.anchoredPosition = new Vector2(0, -50);
        
        controller.centerElement = centerImage.rectTransform;
        controller.centerGlyph = glyphImage;
        controller.centerMetalName = centerText;

        // 4. Create Slots Container
        GameObject slotsContainer = new GameObject("SlotsContainer");
        slotsContainer.transform.SetParent(rootCanvasGO.transform, false);
        controller.slotsContainer = slotsContainer.transform;

        // 5. Generate the Slot Prefab
        GameObject slotGO = new GameObject("MetalWheelSlotPrefab");
        Image hexOutline = slotGO.AddComponent<Image>(); // The base hexagon
        hexOutline.rectTransform.sizeDelta = new Vector2(48, 48);
        MetalWheelSlot slotScript = slotGO.AddComponent<MetalWheelSlot>();
        
        GameObject slotGlyphGO = new GameObject("Glyph");
        slotGlyphGO.transform.SetParent(slotGO.transform, false);
        Image miniGlyph = slotGlyphGO.AddComponent<Image>();
        miniGlyph.rectTransform.sizeDelta = new Vector2(30, 30);
        
        GameObject fuelArcGO = new GameObject("FuelArc");
        fuelArcGO.transform.SetParent(slotGO.transform, false);
        Image fuelArc = fuelArcGO.AddComponent<Image>();
        fuelArc.type = Image.Type.Filled;
        fuelArc.fillMethod = Image.FillMethod.Radial360; // Acts like a gauge
        
        slotScript.hexagonOutline = hexOutline;
        slotScript.glyphIcon = miniGlyph;
        slotScript.fuelArcIndicator = fuelArc;

        // Ensure Prefabs directory exists
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs")) AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/GUI")) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "GUI");

        string prefabPath = "Assets/_Project/Prefabs/GUI/MetalWheelSlot.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(slotGO, prefabPath);
        DestroyImmediate(slotGO);

        controller.slotPrefab = savedPrefab.GetComponent<MetalWheelSlot>();

        // 6. Pre-fill the 16 Metals as specified by the prompt
        controller.metalData = new List<MetalSlotData>
        {
            // PHYSICAL
            CreateData(MetallurgySkill.MetalType.Iron, MetalGroup.Physical, new Color(0.1f, 0.2f, 0.6f)),    // Deep Blue
            CreateData(MetallurgySkill.MetalType.Steel, MetalGroup.Physical, new Color(0.7f, 0.8f, 0.9f)),   // Bright Steel
            CreateData(MetallurgySkill.MetalType.Tin, MetalGroup.Physical, new Color(0.9f, 0.9f, 0.95f)),    // Silver-white
            CreateData(MetallurgySkill.MetalType.Pewter, MetalGroup.Physical, new Color(0.5f, 0.5f, 0.5f)),  // Warm Grey

            // MENTAL
            CreateData(MetallurgySkill.MetalType.Zinc, MetalGroup.Mental, new Color(1f, 0.95f, 0.2f)),       // Electric Yellow
            CreateData(MetallurgySkill.MetalType.Brass, MetalGroup.Mental, new Color(0.8f, 0.6f, 0.2f)),     // Warm Brass
            CreateData(MetallurgySkill.MetalType.Copper, MetalGroup.Mental, new Color(0.8f, 0.4f, 0.1f)),    // Burnt Orange
            CreateData(MetallurgySkill.MetalType.Bronze, MetalGroup.Mental, new Color(0.4f, 0.3f, 0.1f)),    // Dark Bronze

            // ENHANCEMENT
            CreateData(MetallurgySkill.MetalType.Aluminum, MetalGroup.Enhancement, new Color(0.85f, 0.85f, 0.85f)), // Flat Silver
            CreateData(MetallurgySkill.MetalType.Duralumin, MetalGroup.Enhancement, new Color(1f, 1f, 1f)),         // Brilliant White
            CreateData(MetallurgySkill.MetalType.Chromium, MetalGroup.Enhancement, new Color(0.8f, 0.9f, 0.9f)),    // Chrome
            CreateData(MetallurgySkill.MetalType.Nicrosil, MetalGroup.Enhancement, new Color(0.6f, 0.9f, 0.6f)),    // Pale Green

            // TEMPORAL
            CreateData(MetallurgySkill.MetalType.Gold, MetalGroup.Temporal, new Color(1f, 0.84f, 0f)),              // Deep Gold
            CreateData(MetallurgySkill.MetalType.Electrum, MetalGroup.Temporal, new Color(1f, 0.95f, 0.6f)),        // Light Gold
            CreateData(MetallurgySkill.MetalType.Cadmium, MetalGroup.Temporal, new Color(0.2f, 0.4f, 0.8f)),        // Cold Blue
            CreateData(MetallurgySkill.MetalType.Bendalloy, MetalGroup.Temporal, new Color(1f, 0.7f, 0.3f))         // Warm Amber
        };

        // Select the new UI root in editor so you can see it
        Selection.activeGameObject = rootCanvasGO;

        Debug.Log("<color=cyan><b>[MISTBORN] MetalWheelCanvas created successfully! Drag your Player's Metallurgist script into the inspector slot!</b></color>");
    }

    private static MetalSlotData CreateData(MetallurgySkill.MetalType type, MetalGroup group, Color color)
    {
        MetalSlotData data = new MetalSlotData();
        data.metalType = type;
        data.group = group;
        data.themeColor = color;
        return data;
    }
}
#endif
