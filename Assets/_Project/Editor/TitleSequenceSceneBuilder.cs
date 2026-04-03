/* TitleSequenceSceneBuilder.cs
 *
 * Editor tool: Mistborn → Scenes → Build Title Sequence Scene
 *
 * Creates the complete TitleSequence scene with all UI, cameras, particles,
 * audio, and the TitleSequenceController fully wired. One click.
 *
 * SEQUENCE (from design prompt):
 *   0–9s       Black fades to misty field with ash falling in distance
 *   ~9s        Percussion → Crimson Blade Interactive logo (+ Sanderson if approved)
 *   ~28s       Drums → cut to Luthadel streets + rolling credits
 *   First drop → Kredik Shaw aerial pan + "proudly presents"
 *   Rock drop  → MISTBORN title in blue Allomantic lines
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class TitleSequenceSceneBuilder
{
    // Colors
    static readonly Color COL_TEXT       = new Color(0.91f, 0.84f, 0.72f, 1f);   // #E8D5B7 parchment
    static readonly Color COL_TITLE     = new Color(0.27f, 0.53f, 1f, 0.6f);     // #4488FF allomantic blue
    static readonly Color COL_TITLE_BRIGHT = new Color(0.27f, 0.53f, 1f, 0.85f);
    static readonly Color COL_CREDIT    = new Color(0.91f, 0.84f, 0.72f, 0.9f);
    static readonly Color COL_ASH       = new Color(0.35f, 0.32f, 0.28f, 0.7f);  // grey-brown ash
    static readonly Color COL_MIST      = new Color(0.7f, 0.72f, 0.75f, 0.15f);  // pale mist
    static readonly Color COL_GROUND    = new Color(0.08f, 0.07f, 0.06f, 1f);    // dark ash ground
    static readonly Color COL_SKY       = new Color(0.04f, 0.03f, 0.05f, 1f);    // near-black sky

    [MenuItem("Mistborn/Scenes/Build Title Sequence Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ══════════════════════════════════════════════════════════════════
        // 3D ENVIRONMENT — Misty field with ash
        // ══════════════════════════════════════════════════════════════════

        // Main Camera
        var camObj = new GameObject("TitleCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = COL_SKY;
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;
        camObj.AddComponent<AudioListener>();
        camObj.transform.position = new Vector3(0f, 2f, -8f);
        camObj.transform.rotation = Quaternion.Euler(5f, 0f, 0f);

        // Ground plane — dark ash-covered field
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "AshField";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(20f, 1f, 20f);
        var groundRend = ground.GetComponent<Renderer>();
        var groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = COL_GROUND;
        groundMat.SetFloat("_Glossiness", 0f);
        groundRend.material = groundMat;

        // Fog / atmosphere
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.04f;
        RenderSettings.fogColor = new Color(0.08f, 0.08f, 0.1f, 1f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.06f, 0.08f, 1f);

        // Dim directional light — faint red sun through ash
        var lightObj = new GameObject("DimSun");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.7f, 0.3f, 0.15f, 1f);
        light.intensity = 0.3f;
        lightObj.transform.rotation = Quaternion.Euler(25f, -30f, 0f);

        // ── Misty Field parent (Phase 1 environment) ─────────────────────
        var mistyField = new GameObject("MistyFieldScene");
        ground.transform.SetParent(mistyField.transform);
        lightObj.transform.SetParent(mistyField.transform);

        // Ash particle system
        var ashObj = new GameObject("AshParticles");
        ashObj.transform.SetParent(mistyField.transform);
        ashObj.transform.position = new Vector3(0f, 15f, 10f);
        var ashPS = ashObj.AddComponent<ParticleSystem>();
        var ashMain = ashPS.main;
        ashMain.startLifetime = 8f;
        ashMain.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        ashMain.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        ashMain.startColor = COL_ASH;
        ashMain.maxParticles = 500;
        ashMain.simulationSpace = ParticleSystemSimulationSpace.World;
        ashMain.gravityModifier = 0.15f;
        var ashEmission = ashPS.emission;
        ashEmission.rateOverTime = 60f;
        var ashShape = ashPS.shape;
        ashShape.shapeType = ParticleSystemShapeType.Box;
        ashShape.scale = new Vector3(30f, 0.1f, 30f);
        var ashVel = ashPS.velocityOverLifetime;
        ashVel.enabled = true;
        ashVel.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        ashVel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        // Mist particle system — ground-level fog drifting
        var mistObj = new GameObject("MistParticles");
        mistObj.transform.SetParent(mistyField.transform);
        mistObj.transform.position = new Vector3(0f, 0.5f, 5f);
        var mistPS = mistObj.AddComponent<ParticleSystem>();
        var mistMain = mistPS.main;
        mistMain.startLifetime = 12f;
        mistMain.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        mistMain.startSize = new ParticleSystem.MinMaxCurve(3f, 8f);
        mistMain.startColor = COL_MIST;
        mistMain.maxParticles = 40;
        mistMain.simulationSpace = ParticleSystemSimulationSpace.World;
        var mistEmission = mistPS.emission;
        mistEmission.rateOverTime = 4f;
        var mistShape = mistPS.shape;
        mistShape.shapeType = ParticleSystemShapeType.Box;
        mistShape.scale = new Vector3(30f, 0.5f, 20f);
        // Mist drifts slowly
        var mistVel = mistPS.velocityOverLifetime;
        mistVel.enabled = true;
        mistVel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        mistVel.z = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        // Fade in and out
        var mistCol = mistPS.colorOverLifetime;
        mistCol.enabled = true;
        var mistGrad = new Gradient();
        mistGrad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.6f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        mistCol.color = new ParticleSystem.MinMaxGradient(mistGrad);

        // ── Placeholder scene groups (fill with real environments later) ─
        var luthadelGroup = new GameObject("LuthadelStreetsGroup");
        luthadelGroup.SetActive(false);

        var kredikGroup = new GameObject("KredikShawGroup");
        kredikGroup.SetActive(false);

        // ══════════════════════════════════════════════════════════════════
        // UI CANVAS
        // ══════════════════════════════════════════════════════════════════

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // ── Black Overlay (full screen, starts opaque) ───────────────────
        var blackObj = new GameObject("BlackOverlay");
        blackObj.transform.SetParent(canvasObj.transform, false);
        var blackImg = blackObj.AddComponent<Image>();
        blackImg.color = Color.black;
        StretchFill(blackObj.GetComponent<RectTransform>());
        var blackCG = blackObj.AddComponent<CanvasGroup>();
        blackCG.alpha = 1f;

        // ── Crimson Blade Logo Group ─────────────────────────────────────
        var cbLogoGroup = new GameObject("CrimsonBladeLogoGroup");
        cbLogoGroup.transform.SetParent(canvasObj.transform, false);
        StretchFill(cbLogoGroup.AddComponent<RectTransform>());
        var cbLogoCG = cbLogoGroup.AddComponent<CanvasGroup>();
        cbLogoCG.alpha = 0f;

        // Logo text placeholder (replace with actual logo image later)
        var cbText = CreateTMP(cbLogoGroup.transform, "LogoText",
            "CRIMSON BLADE\nINTERACTIVE", 48, COL_TEXT, TextAlignmentOptions.Center);
        CenterFill(cbText.GetComponent<RectTransform>(), 600f, 150f);

        // ── Sanderson Logo Group ─────────────────────────────────────────
        var sLogoGroup = new GameObject("SandersonLogoGroup");
        sLogoGroup.transform.SetParent(canvasObj.transform, false);
        StretchFill(sLogoGroup.AddComponent<RectTransform>());
        var sLogoCG = sLogoGroup.AddComponent<CanvasGroup>();
        sLogoCG.alpha = 0f;

        var sText = CreateTMP(sLogoGroup.transform, "SandersonLogoText",
            "DRAGONSTEEL\nENTERTAINMENT", 42, COL_TEXT, TextAlignmentOptions.Center);
        CenterFill(sText.GetComponent<RectTransform>(), 600f, 130f);

        // ── Credit Text ──────────────────────────────────────────────────
        var creditObj = new GameObject("CreditText");
        creditObj.transform.SetParent(canvasObj.transform, false);
        var creditRT = creditObj.AddComponent<RectTransform>();
        creditRT.anchorMin = new Vector2(0.5f, 0.35f);
        creditRT.anchorMax = new Vector2(0.5f, 0.35f);
        creditRT.pivot = new Vector2(0.5f, 0.5f);
        creditRT.sizeDelta = new Vector2(900f, 80f);
        creditRT.anchoredPosition = Vector2.zero;
        var creditTMP = creditObj.AddComponent<TextMeshProUGUI>();
        creditTMP.text = "";
        creditTMP.fontSize = 30;
        creditTMP.color = COL_CREDIT;
        creditTMP.alignment = TextAlignmentOptions.Center;
        creditTMP.fontStyle = FontStyles.Normal;
        var creditCG = creditObj.AddComponent<CanvasGroup>();
        creditCG.alpha = 0f;

        // ── Title Group (MISTBORN in blue lines) ─────────────────────────
        var titleGroup = new GameObject("TitleGroup");
        titleGroup.transform.SetParent(canvasObj.transform, false);
        StretchFill(titleGroup.AddComponent<RectTransform>());
        var titleCG = titleGroup.AddComponent<CanvasGroup>();
        titleCG.alpha = 0f;

        // Title text
        var titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleGroup.transform, false);
        var titleRT = titleTextObj.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.sizeDelta = new Vector2(1200f, 200f);
        titleRT.anchoredPosition = new Vector2(0f, 50f);
        var titleTMP = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "MISTBORN";
        titleTMP.fontSize = 120;
        titleTMP.color = COL_TITLE;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.characterSpacing = 25f;

        // AllomanticTitleRenderer
        var titleRenderer = titleGroup.AddComponent<AllomanticTitleRenderer>();
        titleRenderer.titleText = titleTMP;
        titleRenderer.titleString = "MISTBORN";

        // Subtitle (optional, below MISTBORN)
        var subObj = new GameObject("SubtitleText");
        subObj.transform.SetParent(titleGroup.transform, false);
        var subRT = subObj.AddComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.5f, 0.5f);
        subRT.anchorMax = new Vector2(0.5f, 0.5f);
        subRT.pivot = new Vector2(0.5f, 0.5f);
        subRT.sizeDelta = new Vector2(800f, 50f);
        subRT.anchoredPosition = new Vector2(0f, -60f);
        var subTMP = subObj.AddComponent<TextMeshProUGUI>();
        subTMP.text = "THE FINAL EMPIRE";
        subTMP.fontSize = 28;
        subTMP.color = new Color(COL_TEXT.r, COL_TEXT.g, COL_TEXT.b, 0f);
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.characterSpacing = 15f;
        titleRenderer.subtitleText = subTMP;
        titleRenderer.subtitleString = "THE FINAL EMPIRE";

        // ── Skip Hint (bottom of screen) ─────────────────────────────────
        var skipObj = new GameObject("SkipHint");
        skipObj.transform.SetParent(canvasObj.transform, false);
        var skipRT = skipObj.AddComponent<RectTransform>();
        skipRT.anchorMin = new Vector2(0.5f, 0f);
        skipRT.anchorMax = new Vector2(0.5f, 0f);
        skipRT.pivot = new Vector2(0.5f, 0f);
        skipRT.anchoredPosition = new Vector2(0f, 20f);
        skipRT.sizeDelta = new Vector2(400f, 30f);
        var skipTMP = skipObj.AddComponent<TextMeshProUGUI>();
        skipTMP.text = "Press ESC to skip";
        skipTMP.fontSize = 16;
        skipTMP.color = new Color(1f, 1f, 1f, 0.25f);
        skipTMP.alignment = TextAlignmentOptions.Center;

        // Make sure BlackOverlay renders on top of everything
        blackObj.transform.SetAsLastSibling();

        // ══════════════════════════════════════════════════════════════════
        // MANAGER + AUDIO
        // ══════════════════════════════════════════════════════════════════

        var manager = new GameObject("TitleManager");
        var audioSrc = manager.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.loop = false;

        var tsc = manager.AddComponent<TitleSequenceController>();

        // Wire audio
        tsc.musicSource = audioSrc;
        tsc.musicVolume = 1f;

        // Try to find and assign the title theme
        string[] guids = AssetDatabase.FindAssets("MistbornTitleTheme t:AudioClip");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            tsc.mainThemeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        if (tsc.mainThemeClip == null)
        {
            // Try alternate names
            guids = AssetDatabase.FindAssets("Mistborn Title theme t:AudioClip");
            if (guids.Length > 0)
                tsc.mainThemeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        // Wire timing
        tsc.fadeInDuration     = 9f;
        tsc.logoStartTime      = 9f;
        tsc.logoDuration       = 6f;
        tsc.logoFadeSpeed      = 1.2f;
        tsc.streetsStartTime   = 28f;
        tsc.kredikShawStartTime = 45f;
        tsc.titleDropTime      = 60f;
        tsc.titleDrawDuration  = 3f;
        tsc.postTitleHold      = 5f;

        // Wire references
        tsc.blackOverlay         = blackCG;
        tsc.mistyFieldScene      = mistyField;
        tsc.ashParticles         = ashPS;
        tsc.mistParticles        = mistPS;
        tsc.crimsonBladeLogoGroup = cbLogoCG;
        tsc.sandersonLogoGroup   = sLogoCG;
        tsc.luthadelStreetsGroup  = luthadelGroup;
        tsc.kredikShawGroup      = kredikGroup;
        tsc.titleGroup           = titleCG;
        tsc.creditText           = creditTMP;
        tsc.creditTextGroup      = creditCG;
        tsc.nextSceneName        = "MainMenu";

        // Pre-populated credit lines
        tsc.creditLines = new System.Collections.Generic.List<TitleSequenceController.CreditLine>
        {
            new TitleSequenceController.CreditLine { time = 28f,  text = "Music by Malakei" },
            new TitleSequenceController.CreditLine { time = 33f,  text = "Based on the novels by\nBrandon Sanderson" },
            new TitleSequenceController.CreditLine { time = 38f,  text = "Produced by\nCrimson Blade Interactive" },
            new TitleSequenceController.CreditLine { time = 46f,  text = "Creative Director\nLandon Adams" },
            new TitleSequenceController.CreditLine { time = 52f,  text = "Crimson Blade Interactive\nproudly presents" },
        };

        // SceneBootstrap
        manager.AddComponent<SceneBootstrap>();

        // ══════════════════════════════════════════════════════════════════
        // SAVE
        // ══════════════════════════════════════════════════════════════════

        EditorSceneManager.MarkSceneDirty(scene);

        string scenePath = "Assets/_Project/Scenes/TitleSequence.unity";
        System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);

        AddSceneToBuild(scenePath, 0);

        Debug.Log($"[TitleSequenceSceneBuilder] Scene created at {scenePath}");

        string audioStatus = tsc.mainThemeClip != null
            ? $"Audio: {tsc.mainThemeClip.name} assigned"
            : "Audio: NOT FOUND — drag MistbornTitleTheme.mp3 into Music Source → Main Theme Clip";

        EditorUtility.DisplayDialog("Title Sequence Built",
            $"TitleSequence scene created at:\n{scenePath}\n\n" +
            $"{audioStatus}\n\n" +
            "Includes:\n" +
            "• Misty ash field with particles + fog\n" +
            "• Black overlay fade (0–9s)\n" +
            "• Crimson Blade + Sanderson logo placeholders\n" +
            "• Credit text with 5 pre-timed lines\n" +
            "• MISTBORN title with AllomanticTitleRenderer\n" +
            "• \"THE FINAL EMPIRE\" subtitle\n" +
            "• Skip hint + ESC/Space support\n" +
            "• All timing wired to TitleSequenceController\n\n" +
            "Replace logo text with actual logo images when ready.\n" +
            "Add Luthadel/Kredik Shaw environments when modeled.",
            "OK");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════════════════════

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

    static void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void CenterFill(RectTransform rt, float w, float h)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
    }

    static void AddSceneToBuild(string scenePath, int targetIndex)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Remove if already exists
        scenes.RemoveAll(s => s.path == scenePath);

        // Insert at target index
        var entry = new EditorBuildSettingsScene(scenePath, true);
        if (targetIndex >= scenes.Count)
            scenes.Add(entry);
        else
            scenes.Insert(targetIndex, entry);

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
