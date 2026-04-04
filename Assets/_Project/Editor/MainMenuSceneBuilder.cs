/* MainMenuSceneBuilder.cs
 *
 * Editor tool: Tools → Ashwalker → Build Main Menu Scene
 *
 * Creates a complete, correctly-laid-out MainMenu scene with:
 *   - Canvas (1920×1080 scale), EventSystem
 *   - Background (solid dark), mist particles
 *   - MainPanel with title + 6 styled buttons in vertical layout
 *   - SettingsPanel with audio sliders, quality/resolution dropdowns, fullscreen toggle
 *   - LoadGamePanel placeholder with back button
 *   - CreditsPanel placeholder with back button
 *   - FadeOverlay (black, alpha 0, CanvasGroup)
 *   - MenuManager with MainMenuController + SceneBootstrap wired
 *
 * Run from: top menu → Tools → Ashwalker → Build Main Menu Scene
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class MainMenuSceneBuilder
{
    // ── Ashwalker color palette ───────────────────────────────────────────────
    static readonly Color COL_BG           = new Color(0.04f, 0.04f, 0.08f, 1f);       // #0A0A14
    static readonly Color COL_PANEL_BG     = new Color(0.04f, 0.04f, 0.08f, 0.92f);
    static readonly Color COL_BUTTON       = new Color(0.10f, 0.10f, 0.18f, 1f);       // #1A1A2E
    static readonly Color COL_BUTTON_HOVER = new Color(0.09f, 0.13f, 0.24f, 1f);       // #16213E
    static readonly Color COL_BUTTON_PRESS = new Color(0.06f, 0.20f, 0.38f, 1f);       // #0F3460
    static readonly Color COL_TEXT         = new Color(0.91f, 0.84f, 0.72f, 1f);        // #E8D5B7
    static readonly Color COL_TITLE        = new Color(0.27f, 0.53f, 1f, 0.85f);        // #4488FF
    static readonly Color COL_DIM          = new Color(0.91f, 0.84f, 0.72f, 0.4f);

    const float REF_W = 1920f;
    const float REF_H = 1080f;

    [MenuItem("Ashwalker/Scenes/Build Main Menu Scene")]
    public static void BuildMainMenuScene()
    {
        // Confirm if current scene has unsaved changes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // New scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Remove default light/camera — Canvas handles rendering
        foreach (var go in Object.FindObjectsOfType<Light>()) Object.DestroyImmediate(go.gameObject);

        // Keep the Main Camera for now (some UI setups need it)
        Camera mainCam = Camera.main;
        if (mainCam != null) mainCam.backgroundColor = COL_BG;

        // ── EventSystem ──────────────────────────────────────────────────
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ── Canvas ───────────────────────────────────────────────────────
        GameObject canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_W, REF_H);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ── Background ───────────────────────────────────────────────────
        var bg = CreateUIElement<RawImage>(canvasObj.transform, "Background");
        StretchFill(bg.rectTransform);
        bg.color = COL_BG;

        // ── MainPanel ────────────────────────────────────────────────────
        var mainPanel = CreatePanel(canvasObj.transform, "MainPanel", true);

        // Title text
        var titleTMP = CreateTMP(mainPanel.transform, "TitleText", "MISTBORN",
            72, COL_TITLE, TextAlignmentOptions.Center);
        var titleRT = titleTMP.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -120f);
        titleRT.sizeDelta = new Vector2(800f, 100f);

        // Button group
        var btnGroup = new GameObject("ButtonGroup");
        btnGroup.transform.SetParent(mainPanel.transform, false);
        var btnGroupRT = btnGroup.AddComponent<RectTransform>();
        btnGroupRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnGroupRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnGroupRT.pivot = new Vector2(0.5f, 0.5f);
        btnGroupRT.anchoredPosition = new Vector2(0f, -30f);
        btnGroupRT.sizeDelta = new Vector2(320f, 420f);

        var vlg = btnGroup.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var newGameBtn    = CreateMenuButton(btnGroup.transform, "NewGameButton",    "New Game");
        var continueBtn   = CreateMenuButton(btnGroup.transform, "ContinueButton",   "Continue");
        var loadGameBtn   = CreateMenuButton(btnGroup.transform, "LoadGameButton",   "Load Game");
        var settingsBtn   = CreateMenuButton(btnGroup.transform, "SettingsButton",   "Settings");
        var creditsBtn    = CreateMenuButton(btnGroup.transform, "CreditsButton",    "Credits");
        var quitBtn       = CreateMenuButton(btnGroup.transform, "QuitButton",       "Quit");

        // Version text
        var verTMP = CreateTMP(mainPanel.transform, "VersionText", "v0.1 Alpha",
            16, COL_DIM, TextAlignmentOptions.BottomRight);
        var verRT = verTMP.GetComponent<RectTransform>();
        verRT.anchorMin = new Vector2(1f, 0f);
        verRT.anchorMax = new Vector2(1f, 0f);
        verRT.pivot = new Vector2(1f, 0f);
        verRT.anchoredPosition = new Vector2(-20f, 15f);
        verRT.sizeDelta = new Vector2(200f, 30f);

        // ── SettingsPanel ────────────────────────────────────────────────
        var settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", false);
        BuildSettingsPanel(settingsPanel);

        // ── LoadGamePanel ────────────────────────────────────────────────
        var loadPanel = CreatePanel(canvasObj.transform, "LoadGamePanel", false);
        BuildPlaceholderPanel(loadPanel, "Load Game", "Save slots will appear here.");

        // ── CreditsPanel ─────────────────────────────────────────────────
        var creditsPanel = CreatePanel(canvasObj.transform, "CreditsPanel", false);
        BuildCreditsPanel(creditsPanel);

        // ── FadeOverlay ──────────────────────────────────────────────────
        var fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(canvasObj.transform, false);
        var fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = Color.black;
        StretchFill(fadeObj.GetComponent<RectTransform>());
        var fadeCG = fadeObj.AddComponent<CanvasGroup>();
        fadeCG.alpha = 0f;
        fadeCG.blocksRaycasts = false;

        // ── MenuManager ──────────────────────────────────────────────────
        var manager = new GameObject("MenuManager");
        var mmc = manager.AddComponent<MainMenuController>();

        mmc.mainPanel     = mainPanel;
        mmc.settingsPanel = settingsPanel;
        mmc.loadGamePanel = loadPanel;
        mmc.creditsPanel  = creditsPanel;

        mmc.newGameButton  = newGameBtn;
        mmc.continueButton = continueBtn;
        mmc.loadGameButton = loadGameBtn;
        mmc.settingsButton = settingsBtn;
        mmc.creditsButton  = creditsBtn;
        mmc.quitButton     = quitBtn;

        mmc.fadeOverlay    = fadeCG;
        mmc.gameplayScene  = "Cinderhold";

        // Add SceneBootstrap
        manager.AddComponent<SceneBootstrap>();

        // ── Wire Settings Panel component ────────────────────────────────
        var msp = settingsPanel.GetComponent<MainMenuSettingsPanel>();
        if (msp != null)
        {
            msp.masterVolumeSlider = settingsPanel.transform.Find("MasterVolumeSlider")?.GetComponent<Slider>();
            msp.musicVolumeSlider  = settingsPanel.transform.Find("MusicVolumeSlider")?.GetComponent<Slider>();
            msp.sfxVolumeSlider    = settingsPanel.transform.Find("SFXVolumeSlider")?.GetComponent<Slider>();
            msp.masterValueText    = settingsPanel.transform.Find("MasterValueText")?.GetComponent<TextMeshProUGUI>();
            msp.musicValueText     = settingsPanel.transform.Find("MusicValueText")?.GetComponent<TextMeshProUGUI>();
            msp.sfxValueText       = settingsPanel.transform.Find("SFXValueText")?.GetComponent<TextMeshProUGUI>();
            msp.qualityDropdown    = settingsPanel.transform.Find("QualityDropdown")?.GetComponent<TMP_Dropdown>();
            msp.resolutionDropdown = settingsPanel.transform.Find("ResolutionDropdown")?.GetComponent<TMP_Dropdown>();
            msp.fullscreenToggle   = settingsPanel.transform.Find("FullscreenToggle")?.GetComponent<Toggle>();
            msp.backButton         = settingsPanel.transform.Find("BackButton")?.GetComponent<Button>();
        }

        // ── Save ─────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);

        string path = "Assets/_Project/Scenes/MainMenu.unity";
        System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
        EditorSceneManager.SaveScene(scene, path);

        // Add to build settings if not already there
        AddSceneToBuild(path);

        Debug.Log($"[MainMenuSceneBuilder] MainMenu scene created and saved to {path}");
        EditorUtility.DisplayDialog("Main Menu Built",
            "MainMenu scene created at:\n" + path +
            "\n\nAll panels, buttons, and components are wired." +
            "\n\nAssign the title theme audio penny to SoundManager → Main Theme Track at runtime.",
            "OK");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PANEL BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static void BuildSettingsPanel(GameObject panel)
    {
        // Background
        var bgImg = panel.AddComponent<Image>();
        bgImg.color = COL_PANEL_BG;
        StretchFill(panel.GetComponent<RectTransform>());

        // Header
        var header = CreateTMP(panel.transform, "HeaderText", "Settings",
            42, COL_TEXT, TextAlignmentOptions.Center);
        SetAnchored(header, 0.5f, 1f, 0f, -60f, 400f, 60f);

        // Audio sliders
        float sliderY = -160f;
        CreateVolumeSlider(panel.transform, "MasterVolumeSlider", "Master Volume", "MasterValueText", 1f, ref sliderY);
        CreateVolumeSlider(panel.transform, "MusicVolumeSlider",  "Music",         "MusicValueText",  0.5f, ref sliderY);
        CreateVolumeSlider(panel.transform, "SFXVolumeSlider",    "SFX",           "SFXValueText",    1f, ref sliderY);

        // Quality dropdown
        sliderY -= 30f;
        CreateLabeledDropdown(panel.transform, "QualityDropdown", "Graphics Quality", ref sliderY);

        // Resolution dropdown
        CreateLabeledDropdown(panel.transform, "ResolutionDropdown", "Resolution", ref sliderY);

        // Fullscreen toggle
        sliderY -= 20f;
        CreateFullscreenToggle(panel.transform, ref sliderY);

        // Back button
        var backBtn = CreateMenuButton(panel.transform, "BackButton", "Back");
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0.5f, 0f);
        backRT.anchorMax = new Vector2(0.5f, 0f);
        backRT.pivot = new Vector2(0.5f, 0f);
        backRT.anchoredPosition = new Vector2(0f, 40f);
        backRT.sizeDelta = new Vector2(200f, 50f);

        // Add the settings component
        panel.AddComponent<MainMenuSettingsPanel>();
    }

    static void BuildPlaceholderPanel(GameObject panel, string title, string placeholder)
    {
        var bgImg = panel.AddComponent<Image>();
        bgImg.color = COL_PANEL_BG;
        StretchFill(panel.GetComponent<RectTransform>());

        var header = CreateTMP(panel.transform, "HeaderText", title,
            42, COL_TEXT, TextAlignmentOptions.Center);
        SetAnchored(header, 0.5f, 1f, 0f, -60f, 400f, 60f);

        var body = CreateTMP(panel.transform, "PlaceholderText", placeholder,
            22, COL_DIM, TextAlignmentOptions.Center);
        SetAnchored(body, 0.5f, 0.5f, 0f, 0f, 600f, 40f);

        var backBtn = CreateMenuButton(panel.transform, "BackButton", "Back");
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0.5f, 0f);
        backRT.anchorMax = new Vector2(0.5f, 0f);
        backRT.pivot = new Vector2(0.5f, 0f);
        backRT.anchoredPosition = new Vector2(0f, 40f);
        backRT.sizeDelta = new Vector2(200f, 50f);
    }

    static void BuildCreditsPanel(GameObject panel)
    {
        var bgImg = panel.AddComponent<Image>();
        bgImg.color = COL_PANEL_BG;
        StretchFill(panel.GetComponent<RectTransform>());

        var header = CreateTMP(panel.transform, "HeaderText", "Credits",
            42, COL_TEXT, TextAlignmentOptions.Center);
        SetAnchored(header, 0.5f, 1f, 0f, -60f, 400f, 60f);

        string creditsText =
            "MISTBORN\n" +
            "Based on the novels by the original author\n\n" +
            "Creative Director\nLandon Adams\n\n" +
            "Music by\nMalakai Probert\n\n" +
            "Developed by\nCrimson Blade Interactive\n\n" +
            "Built with Unity";

        var body = CreateTMP(panel.transform, "CreditsText", creditsText,
            22, COL_TEXT, TextAlignmentOptions.Center);
        SetAnchored(body, 0.5f, 0.5f, 0f, 30f, 600f, 400f);

        var backBtn = CreateMenuButton(panel.transform, "BackButton", "Back");
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0.5f, 0f);
        backRT.anchorMax = new Vector2(0.5f, 0f);
        backRT.pivot = new Vector2(0.5f, 0f);
        backRT.anchoredPosition = new Vector2(0f, 40f);
        backRT.sizeDelta = new Vector2(200f, 50f);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // UI ELEMENT FACTORIES
    // ═════════════════════════════════════════════════════════════════════════

    static Button CreateMenuButton(Transform parent, string name, string label)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        var img = btnObj.AddComponent<Image>();
        img.color = COL_BUTTON;

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = COL_BUTTON;
        colors.highlightedColor = COL_BUTTON_HOVER;
        colors.pressedColor     = COL_BUTTON_PRESS;
        colors.selectedColor    = COL_BUTTON_HOVER;
        colors.disabledColor    = new Color(COL_BUTTON.r, COL_BUTTON.g, COL_BUTTON.b, 0.3f);
        colors.fadeDuration     = 0.1f;
        btn.colors = colors;

        var rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 50f);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = COL_TEXT;
        tmp.alignment = TextAlignmentOptions.Center;
        StretchFill(textObj.GetComponent<RectTransform>());

        return btn;
    }

    static void CreateVolumeSlider(Transform parent, string name, string label, string valueName, float defaultVal, ref float y)
    {
        // Label
        var lbl = CreateTMP(parent, name + "Label", label, 20, COL_TEXT, TextAlignmentOptions.MidlineRight);
        SetAnchored(lbl, 0.5f, 0.5f, -220f, y, 160f, 30f);

        // Slider (built manually since DefaultControls needs a Resources reference)
        var sliderObj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderObj.name = name;
        sliderObj.transform.SetParent(parent, false);
        var sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = sliderRT.anchorMax = sliderRT.pivot = new Vector2(0.5f, 0.5f);
        sliderRT.anchoredPosition = new Vector2(30f, y);
        sliderRT.sizeDelta = new Vector2(300f, 20f);

        var slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultVal;

        // Style the slider colors
        var fillArea = sliderObj.transform.Find("Fill Area/Fill");
        if (fillArea != null) fillArea.GetComponent<Image>().color = COL_TITLE;
        var bgArea = sliderObj.transform.Find("Background");
        if (bgArea != null) bgArea.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Value text
        var val = CreateTMP(parent, valueName, $"{Mathf.RoundToInt(defaultVal * 100)}%",
            18, COL_DIM, TextAlignmentOptions.MidlineLeft);
        SetAnchored(val, 0.5f, 0.5f, 220f, y, 70f, 30f);

        y -= 60f;
    }

    static void CreateLabeledDropdown(Transform parent, string name, string label, ref float y)
    {
        var lbl = CreateTMP(parent, name + "Label", label, 20, COL_TEXT, TextAlignmentOptions.MidlineRight);
        SetAnchored(lbl, 0.5f, 0.5f, -220f, y, 160f, 30f);

        var ddObj = DefaultControls.CreateDropdown(new DefaultControls.Resources());
        // Replace legacy Text with TMP — destroy existing, add TMP dropdown
        Object.DestroyImmediate(ddObj.GetComponent<Dropdown>());
        Object.DestroyImmediate(ddObj);

        // Create TMP Dropdown manually
        ddObj = new GameObject(name);
        ddObj.transform.SetParent(parent, false);
        var ddImg = ddObj.AddComponent<Image>();
        ddImg.color = COL_BUTTON;
        var ddRT = ddObj.GetComponent<RectTransform>();
        ddRT.anchorMin = ddRT.anchorMax = ddRT.pivot = new Vector2(0.5f, 0.5f);
        ddRT.anchoredPosition = new Vector2(30f, y);
        ddRT.sizeDelta = new Vector2(300f, 35f);

        var dd = ddObj.AddComponent<TMP_Dropdown>();

        // Caption text
        var capObj = new GameObject("Label");
        capObj.transform.SetParent(ddObj.transform, false);
        var capTMP = capObj.AddComponent<TextMeshProUGUI>();
        capTMP.text = "Select...";
        capTMP.fontSize = 18;
        capTMP.color = COL_TEXT;
        capTMP.alignment = TextAlignmentOptions.MidlineLeft;
        StretchFill(capObj.GetComponent<RectTransform>(), 10f);
        dd.captionText = capTMP;

        y -= 55f;
    }

    static void CreateFullscreenToggle(Transform parent, ref float y)
    {
        var togObj = DefaultControls.CreateToggle(new DefaultControls.Resources());
        togObj.name = "FullscreenToggle";
        togObj.transform.SetParent(parent, false);
        var togRT = togObj.GetComponent<RectTransform>();
        togRT.anchorMin = togRT.anchorMax = togRT.pivot = new Vector2(0.5f, 0.5f);
        togRT.anchoredPosition = new Vector2(30f, y);
        togRT.sizeDelta = new Vector2(300f, 30f);

        var toggle = togObj.GetComponent<Toggle>();
        toggle.isOn = true;

        // Replace legacy label text
        var legacyText = togObj.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            var textGO = legacyText.gameObject;
            Object.DestroyImmediate(legacyText);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Fullscreen";
            tmp.fontSize = 20;
            tmp.color = COL_TEXT;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
        }

        y -= 50f;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════════════════════

    static GameObject CreatePanel(Transform parent, string name, bool active)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        StretchFill(rt);
        panel.SetActive(active);
        return panel;
    }

    static T CreateUIElement<T>(Transform parent, string name) where T : Graphic
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<T>();
    }

    static TextMeshProUGUI CreateTMP(Transform parent, string name, string text,
        float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }

    static void StretchFill(RectTransform rt, float padding = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    static void SetAnchored(Component c, float ax, float ay, float px, float py, float w, float h)
    {
        var rt = c.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(ax, ay);
        rt.anchoredPosition = new Vector2(px, py);
        rt.sizeDelta = new Vector2(w, h);
    }

    static void AddSceneToBuild(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == scenePath) return; // already added

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[MainMenuSceneBuilder] Added {scenePath} to Build Settings");
    }
}
#endif
