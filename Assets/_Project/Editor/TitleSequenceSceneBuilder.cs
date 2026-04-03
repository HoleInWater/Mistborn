/* TitleSequenceSceneBuilder.cs
 *
 * Mistborn → Scenes → Build Title Sequence Scene
 *
 * Builds EVERYTHING for the title intro:
 *   Phase 1: Misty ash field with rolling terrain, particles, fog, dim sun
 *   Phase 2: Company logo overlays (text placeholders)
 *   Phase 3: Procedural Luthadel street with buildings, lanterns, ash
 *   Phase 4: Procedural Kredik Shaw spires + aerial city block-out
 *   Phase 5: MISTBORN title in Allomantic blue lines
 *   Camera controller with animated dolly/orbit for each phase
 *   Audio wired to MistbornTitleTheme
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TitleSequenceSceneBuilder
{
    static readonly Color COL_TEXT    = new Color(0.91f, 0.84f, 0.72f, 1f);
    static readonly Color COL_TITLE  = new Color(0.27f, 0.53f, 1f, 0.6f);
    static readonly Color COL_CREDIT = new Color(0.91f, 0.84f, 0.72f, 0.9f);
    static readonly Color COL_ASH_PARTICLE = new Color(0.45f, 0.40f, 0.35f, 0.8f);
    static readonly Color COL_MIST   = new Color(0.75f, 0.75f, 0.80f, 0.2f);

    // Building palette — brighter values with more contrast between them
    static readonly Color COL_STONE_DARK  = new Color(0.18f, 0.15f, 0.13f);  // dark brown-grey
    static readonly Color COL_STONE_MED   = new Color(0.30f, 0.26f, 0.22f);  // warm brown
    static readonly Color COL_STONE_LIGHT = new Color(0.38f, 0.34f, 0.28f);  // light sandy
    static readonly Color COL_STONE_RED   = new Color(0.32f, 0.18f, 0.14f);  // reddish brick
    static readonly Color COL_STONE_GREY  = new Color(0.28f, 0.28f, 0.30f);  // cool grey
    static readonly Color COL_WOOD        = new Color(0.25f, 0.18f, 0.10f);  // dark wood
    static readonly Color COL_ROOF_SLATE  = new Color(0.20f, 0.22f, 0.25f);  // blue-grey slate
    static readonly Color COL_ROOF_TILE   = new Color(0.35f, 0.20f, 0.12f);  // clay tile
    static readonly Color COL_METAL       = new Color(0.35f, 0.35f, 0.40f);  // steel blue-grey
    static readonly Color COL_GROUND      = new Color(0.12f, 0.10f, 0.08f);  // dark ash earth
    static readonly Color COL_GROUND_LIGHT = new Color(0.18f, 0.15f, 0.12f); // lighter path
    static readonly Color COL_COBBLE      = new Color(0.22f, 0.20f, 0.18f);  // cobblestone
    static readonly Color COL_SKY         = new Color(0.05f, 0.04f, 0.06f);  // near-black sky
    static readonly Color COL_LANTERN     = new Color(1.0f, 0.55f, 0.15f);   // warm orange
    static readonly Color COL_WINDOW_WARM = new Color(0.9f, 0.6f, 0.2f);     // warm window glow
    static readonly Color COL_WINDOW_COOL = new Color(0.4f, 0.5f, 0.7f);     // cool window (tin?)
    static readonly Color COL_SPIRE       = new Color(0.14f, 0.13f, 0.16f);  // dark steel
    static readonly Color COL_SPIRE_TIP   = new Color(0.22f, 0.20f, 0.25f);  // lighter tips
    static readonly Color COL_ROCK        = new Color(0.20f, 0.18f, 0.15f);  // visible rocks
    static readonly Color COL_ROCK_DARK   = new Color(0.14f, 0.12f, 0.10f);  // darker rocks
    static readonly Color COL_ASH_GROUND  = new Color(0.15f, 0.14f, 0.13f);  // ash deposits

    [MenuItem("Mistborn/Scenes/Build Title Sequence Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Global render settings
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.025f;
        RenderSettings.fogColor = new Color(0.06f, 0.06f, 0.08f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.07f);

        // ══════════════════════════════════════════════════════════════════
        // CAMERA
        // ══════════════════════════════════════════════════════════════════
        var camObj = new GameObject("TitleCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = COL_SKY;
        cam.fieldOfView = 55f;
        cam.farClipPlane = 500f;
        camObj.AddComponent<AudioListener>();
        var camCtrl = camObj.AddComponent<TitleCameraController>();

        // ══════════════════════════════════════════════════════════════════
        // PHASE 1: MISTY ASH FIELD
        // ══════════════════════════════════════════════════════════════════
        var mistyField = new GameObject("MistyFieldScene");

        // Ground — main field + ash deposits for color variation
        CreateGroundPlane(mistyField.transform, Vector3.zero, 50f, COL_GROUND);
        CreateGroundPlane(mistyField.transform, new Vector3(-8f, 0.01f, 15f), 8f, COL_ASH_GROUND);
        CreateGroundPlane(mistyField.transform, new Vector3(5f, 0.01f, 25f), 6f, COL_GROUND_LIGHT);
        CreateGroundPlane(mistyField.transform, new Vector3(-3f, 0.01f, 5f), 4f, COL_ASH_GROUND);

        // Distant horizon hills — varied colors for depth
        CreateHill(mistyField.transform, new Vector3(-30f, 2f, 80f), new Vector3(25f, 5f, 4f), new Color(0.10f, 0.09f, 0.11f));
        CreateHill(mistyField.transform, new Vector3(15f, 1.5f, 90f), new Vector3(30f, 4f, 3f), new Color(0.08f, 0.07f, 0.09f));
        CreateHill(mistyField.transform, new Vector3(-10f, 3f, 100f), new Vector3(40f, 7f, 5f), new Color(0.06f, 0.06f, 0.08f));
        CreateHill(mistyField.transform, new Vector3(40f, 2.5f, 85f), new Vector3(20f, 6f, 4f), new Color(0.09f, 0.08f, 0.10f));
        // Ashmount silhouette — taller, reddish tint (active volcano glow at base)
        CreateHill(mistyField.transform, new Vector3(0f, 6f, 120f), new Vector3(15f, 14f, 8f), new Color(0.12f, 0.08f, 0.06f));

        // Dim sun
        var sunObj = new GameObject("DimSun");
        sunObj.transform.SetParent(mistyField.transform);
        var sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(0.70f, 0.30f, 0.12f);
        sun.intensity = 0.35f;
        sunObj.transform.rotation = Quaternion.Euler(15f, -30f, 0f);

        // Ash particles
        var ashPS = CreateAshParticles(mistyField.transform, new Vector3(0f, 12f, 10f), 80f);

        // Mist particles
        var mistPS = CreateMistParticles(mistyField.transform, new Vector3(0f, 0.3f, 8f), 40f);

        // Scattered rocks — varied colors and sizes for visual interest
        CreateRock(mistyField.transform, new Vector3(-5f, 0.2f, 8f), 0.6f, COL_ROCK);
        CreateRock(mistyField.transform, new Vector3(3f, 0.12f, 12f), 0.35f, COL_ROCK_DARK);
        CreateRock(mistyField.transform, new Vector3(-8f, 0.25f, 15f), 0.8f, COL_ROCK);
        CreateRock(mistyField.transform, new Vector3(7f, 0.15f, 6f), 0.3f, COL_ROCK_DARK);
        CreateRock(mistyField.transform, new Vector3(-2f, 0.22f, 20f), 0.55f, COL_ROCK);
        CreateRock(mistyField.transform, new Vector3(10f, 0.18f, 18f), 0.4f, COL_STONE_MED);
        CreateRock(mistyField.transform, new Vector3(-12f, 0.3f, 10f), 1.0f, COL_ROCK_DARK);
        CreateRock(mistyField.transform, new Vector3(1f, 0.1f, 3f), 0.2f, COL_ROCK);

        // Dead tree stumps (tall thin cylinders)
        CreateStump(mistyField.transform, new Vector3(-4f, 0f, 14f), 1.5f);
        CreateStump(mistyField.transform, new Vector3(6f, 0f, 22f), 2.0f);
        CreateStump(mistyField.transform, new Vector3(-9f, 0f, 9f), 1.2f);

        // Ash piles (flat stretched spheres, lighter color)
        CreateAshPile(mistyField.transform, new Vector3(2f, 0.05f, 10f), 1.5f);
        CreateAshPile(mistyField.transform, new Vector3(-6f, 0.05f, 18f), 2.0f);
        CreateAshPile(mistyField.transform, new Vector3(8f, 0.03f, 7f), 1.0f);

        // ══════════════════════════════════════════════════════════════════
        // PHASE 3: LUTHADEL STREETS
        // ══════════════════════════════════════════════════════════════════
        var luthadelGroup = new GameObject("LuthadelStreetsGroup");
        luthadelGroup.SetActive(false);

        // Street ground — cobblestone center, dirt edges
        CreateStreetGround(luthadelGroup.transform, Vector3.zero, new Vector3(2f, 1f, 10f), COL_COBBLE);
        CreateStreetGround(luthadelGroup.transform, new Vector3(-2.5f, -0.01f, 0f), new Vector3(1f, 1f, 10f), COL_GROUND);
        CreateStreetGround(luthadelGroup.transform, new Vector3(2.5f, -0.01f, 0f), new Vector3(1f, 1f, 10f), COL_GROUND);
        // Ash deposits on street
        CreateAshPile(luthadelGroup.transform, new Vector3(-1f, 0.02f, 5f), 0.8f);
        CreateAshPile(luthadelGroup.transform, new Vector3(1.5f, 0.02f, -3f), 0.6f);

        // Buildings — left side (each unique color and shape)
        CreateBuilding(luthadelGroup.transform, new Vector3(-6f, 0f, -12f), new Vector3(5f, 8f, 6f), COL_STONE_DARK, COL_ROOF_SLATE);
        CreateBuilding(luthadelGroup.transform, new Vector3(-6.5f, 0f, -4f), new Vector3(6f, 10f, 7f), COL_STONE_RED, COL_ROOF_TILE);
        CreateBuilding(luthadelGroup.transform, new Vector3(-5.5f, 0f, 4f), new Vector3(4.5f, 7f, 6f), COL_STONE_MED, COL_ROOF_SLATE);
        CreateBuilding(luthadelGroup.transform, new Vector3(-6f, 0f, 11f), new Vector3(5.5f, 12f, 5f), COL_STONE_LIGHT, COL_ROOF_TILE);
        CreateBuilding(luthadelGroup.transform, new Vector3(-7f, 0f, 18f), new Vector3(6f, 9f, 7f), COL_STONE_GREY, COL_ROOF_SLATE);

        // Buildings — right side (different colors from left)
        CreateBuilding(luthadelGroup.transform, new Vector3(6f, 0f, -10f), new Vector3(5f, 9f, 8f), COL_STONE_MED, COL_ROOF_TILE);
        CreateBuilding(luthadelGroup.transform, new Vector3(5.5f, 0f, -1f), new Vector3(4f, 6f, 5f), COL_STONE_LIGHT, COL_ROOF_SLATE);
        CreateBuilding(luthadelGroup.transform, new Vector3(6.5f, 0f, 6f), new Vector3(6f, 11f, 6f), COL_STONE_RED, COL_ROOF_TILE);
        CreateBuilding(luthadelGroup.transform, new Vector3(5f, 0f, 14f), new Vector3(5f, 8f, 7f), COL_STONE_GREY, COL_ROOF_SLATE);
        CreateBuilding(luthadelGroup.transform, new Vector3(6f, 0f, 22f), new Vector3(5.5f, 10f, 5f), COL_STONE_DARK, COL_ROOF_TILE);

        // Back-row buildings (visible above front buildings, different heights)
        CreateBuilding(luthadelGroup.transform, new Vector3(-12f, 0f, -8f), new Vector3(5f, 14f, 6f), COL_STONE_GREY, COL_ROOF_SLATE);
        CreateBuilding(luthadelGroup.transform, new Vector3(-11f, 0f, 7f), new Vector3(4f, 16f, 5f), COL_STONE_DARK, COL_ROOF_TILE);
        CreateBuilding(luthadelGroup.transform, new Vector3(11f, 0f, -5f), new Vector3(5f, 13f, 7f), COL_STONE_RED, COL_ROOF_SLATE);
        CreateBuilding(luthadelGroup.transform, new Vector3(12f, 0f, 10f), new Vector3(4.5f, 15f, 5f), COL_STONE_MED, COL_ROOF_TILE);

        // Lanterns — more of them, staggered
        CreateLantern(luthadelGroup.transform, new Vector3(-3.2f, 4f, -8f));
        CreateLantern(luthadelGroup.transform, new Vector3(3.2f, 3.5f, -2f));
        CreateLantern(luthadelGroup.transform, new Vector3(-3f, 4.5f, 4f));
        CreateLantern(luthadelGroup.transform, new Vector3(3.5f, 4f, 10f));
        CreateLantern(luthadelGroup.transform, new Vector3(-3.3f, 3.8f, 16f));
        CreateLantern(luthadelGroup.transform, new Vector3(3.0f, 4.2f, 20f));

        // Street clutter — barrels, crates
        CreateBarrel(luthadelGroup.transform, new Vector3(-2.8f, 0f, -6f));
        CreateBarrel(luthadelGroup.transform, new Vector3(-2.5f, 0f, -5.5f));
        CreateCrate(luthadelGroup.transform, new Vector3(2.6f, 0f, 3f));
        CreateCrate(luthadelGroup.transform, new Vector3(2.9f, 0f, 3.5f));
        CreateCrate(luthadelGroup.transform, new Vector3(2.7f, 0.6f, 3.2f)); // stacked

        // Street particles
        CreateAshParticles(luthadelGroup.transform, new Vector3(0f, 8f, 5f), 25f);
        CreateMistParticles(luthadelGroup.transform, new Vector3(0f, 0.2f, 5f), 15f);

        // Dim street light — slightly brighter so buildings are visible
        var streetSun = new GameObject("StreetAmbient");
        streetSun.transform.SetParent(luthadelGroup.transform);
        var sl = streetSun.AddComponent<Light>();
        sl.type = LightType.Directional;
        sl.color = new Color(0.35f, 0.25f, 0.18f);
        sl.intensity = 0.25f;
        streetSun.transform.rotation = Quaternion.Euler(35f, 15f, 0f);

        // ══════════════════════════════════════════════════════════════════
        // PHASE 4: KREDIK SHAW + CITY FROM ABOVE
        // ══════════════════════════════════════════════════════════════════
        var kredikGroup = new GameObject("KredikShawGroup");
        kredikGroup.SetActive(false);

        // Kredik Shaw — "Hill of a Thousand Spires"
        CreateKredikShaw(kredikGroup.transform, Vector3.zero);

        // Surrounding city blocks (seen from above)
        float blockSpacing = 18f;
        for (int bx = -3; bx <= 3; bx++)
        {
            for (int bz = -3; bz <= 3; bz++)
            {
                // Skip the center where Kredik Shaw sits
                if (Mathf.Abs(bx) <= 1 && Mathf.Abs(bz) <= 1) continue;

                Vector3 blockCenter = new Vector3(bx * blockSpacing, 0f, bz * blockSpacing);
                CreateCityBlock(kredikGroup.transform, blockCenter);
            }
        }

        // City ground — varied patches
        var cityGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        cityGround.name = "CityGround";
        cityGround.transform.SetParent(kredikGroup.transform);
        cityGround.transform.localScale = new Vector3(30f, 1f, 30f);
        ApplyColor(cityGround, new Color(0.10f, 0.08f, 0.07f));

        // Mist rolling through streets from above
        var cityMist = CreateMistParticles(kredikGroup.transform, new Vector3(0f, 2f, 0f), 80f);
        var cm = cityMist.main;
        cm.startSize = new ParticleSystem.MinMaxCurve(5f, 15f);

        // Overhead ash
        CreateAshParticles(kredikGroup.transform, new Vector3(0f, 70f, 0f), 120f);

        // City lights — scattered point lights for windows from above
        for (int i = 0; i < 20; i++)
        {
            var ptLight = new GameObject("CityWindowLight");
            ptLight.transform.SetParent(kredikGroup.transform);
            ptLight.transform.position = new Vector3(
                Random.Range(-50f, 50f), Random.Range(3f, 8f), Random.Range(-50f, 50f));
            var pl = ptLight.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = Color.Lerp(COL_WINDOW_WARM, COL_LANTERN, Random.Range(0f, 1f));
            pl.intensity = Random.Range(0.5f, 1.5f);
            pl.range = Random.Range(5f, 12f);
        }

        // Moonlight — brighter so city is visible from above
        var cityLight = new GameObject("CityMoonlight");
        cityLight.transform.SetParent(kredikGroup.transform);
        var cl = cityLight.AddComponent<Light>();
        cl.type = LightType.Directional;
        cl.color = new Color(0.25f, 0.25f, 0.35f);
        cl.intensity = 0.2f;
        cityLight.transform.rotation = Quaternion.Euler(55f, -20f, 0f);

        // ══════════════════════════════════════════════════════════════════
        // UI CANVAS
        // ══════════════════════════════════════════════════════════════════
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

        // Black overlay
        var blackCG = CreateOverlay(canvasObj.transform, "BlackOverlay", Color.black, 1f);

        // Crimson Blade logo
        var cbLogoCG = CreateLogoGroup(canvasObj.transform, "CrimsonBladeLogoGroup",
            "CRIMSON BLADE\nINTERACTIVE", 48);

        // Sanderson logo
        var sLogoCG = CreateLogoGroup(canvasObj.transform, "SandersonLogoGroup",
            "DRAGONSTEEL\nENTERTAINMENT", 42);

        // Credit text
        var creditTMP = CreateTMP(canvasObj.transform, "CreditText", "", 30,
            COL_CREDIT, TextAlignmentOptions.Center);
        var creditRT = creditTMP.GetComponent<RectTransform>();
        creditRT.anchorMin = new Vector2(0.5f, 0.35f);
        creditRT.anchorMax = new Vector2(0.5f, 0.35f);
        creditRT.sizeDelta = new Vector2(900f, 80f);
        var creditCG = creditTMP.gameObject.AddComponent<CanvasGroup>();
        creditCG.alpha = 0f;

        // Title group
        var titleGroup = new GameObject("TitleGroup");
        titleGroup.transform.SetParent(canvasObj.transform, false);
        StretchFill(titleGroup.AddComponent<RectTransform>());
        var titleCG = titleGroup.AddComponent<CanvasGroup>();
        titleCG.alpha = 0f;

        var titleTMP = CreateTMP(titleGroup.transform, "TitleText", "MISTBORN", 120,
            COL_TITLE, TextAlignmentOptions.Center);
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.characterSpacing = 25f;
        var trt = titleTMP.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(1200f, 200f);
        trt.anchoredPosition = new Vector2(0f, 50f);

        var titleRenderer = titleGroup.AddComponent<AllomanticTitleRenderer>();
        titleRenderer.titleText = titleTMP;
        titleRenderer.titleString = "MISTBORN";

        var subTMP = CreateTMP(titleGroup.transform, "SubtitleText", "THE FINAL EMPIRE", 28,
            new Color(COL_TEXT.r, COL_TEXT.g, COL_TEXT.b, 0f), TextAlignmentOptions.Center);
        subTMP.characterSpacing = 15f;
        var srt = subTMP.GetComponent<RectTransform>();
        srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(800f, 50f);
        srt.anchoredPosition = new Vector2(0f, -60f);
        titleRenderer.subtitleText = subTMP;
        titleRenderer.subtitleString = "THE FINAL EMPIRE";

        // Skip hint
        var skipTMP = CreateTMP(canvasObj.transform, "SkipHint", "Press ESC to skip", 16,
            new Color(1f, 1f, 1f, 0.2f), TextAlignmentOptions.Center);
        var skrt = skipTMP.GetComponent<RectTransform>();
        skrt.anchorMin = new Vector2(0.5f, 0f);
        skrt.anchorMax = new Vector2(0.5f, 0f);
        skrt.pivot = new Vector2(0.5f, 0f);
        skrt.anchoredPosition = new Vector2(0f, 15f);
        skrt.sizeDelta = new Vector2(400f, 30f);

        // Black overlay last (renders on top)
        blackCG.transform.SetAsLastSibling();

        // ══════════════════════════════════════════════════════════════════
        // MANAGER + WIRING
        // ══════════════════════════════════════════════════════════════════
        var manager = new GameObject("TitleManager");
        var audioSrc = manager.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;

        var tsc = manager.AddComponent<TitleSequenceController>();
        tsc.musicSource = audioSrc;
        tsc.musicVolume = 1f;

        // Find audio
        tsc.mainThemeClip = FindAudioClip("MistbornTitleTheme")
                         ?? FindAudioClip("Mistborn Title theme");

        // Timing
        tsc.fadeInDuration      = 9f;
        tsc.logoStartTime       = 9f;
        tsc.logoDuration        = 6f;
        tsc.logoFadeSpeed       = 1.2f;
        tsc.streetsStartTime    = 28f;
        tsc.kredikShawStartTime = 45f;
        tsc.titleDropTime       = 60f;
        tsc.titleDrawDuration   = 3f;
        tsc.postTitleHold       = 5f;

        // References
        tsc.blackOverlay          = blackCG.GetComponent<CanvasGroup>();
        tsc.mistyFieldScene       = mistyField;
        tsc.ashParticles          = ashPS;
        tsc.mistParticles         = mistPS;
        tsc.crimsonBladeLogoGroup = cbLogoCG.GetComponent<CanvasGroup>();
        tsc.sandersonLogoGroup    = sLogoCG.GetComponent<CanvasGroup>();
        tsc.luthadelStreetsGroup   = luthadelGroup;
        tsc.kredikShawGroup       = kredikGroup;
        tsc.titleGroup            = titleCG;
        tsc.creditText            = creditTMP;
        tsc.creditTextGroup       = creditCG;
        tsc.cameraController      = camCtrl;
        tsc.nextSceneName         = "MainMenu";

        tsc.creditLines = new List<TitleSequenceController.CreditLine>
        {
            new TitleSequenceController.CreditLine { time = 28f, text = "Music by Malakei" },
            new TitleSequenceController.CreditLine { time = 35f, text = "Based on the novels by\nBrandon Sanderson" },
            new TitleSequenceController.CreditLine { time = 42f, text = "Produced by\nCrimson Blade Interactive" },
            new TitleSequenceController.CreditLine { time = 49f, text = "Creative Director\nLandon Adams" },
            new TitleSequenceController.CreditLine { time = 55f, text = "Crimson Blade Interactive\nproudly presents" },
        };

        manager.AddComponent<SceneBootstrap>();

        // ══════════════════════════════════════════════════════════════════
        // SAVE
        // ══════════════════════════════════════════════════════════════════
        EditorSceneManager.MarkSceneDirty(scene);
        string scenePath = "Assets/_Project/Scenes/TitleSequence.unity";
        System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AddSceneToBuild(scenePath, 0);

        string audioMsg = tsc.mainThemeClip != null
            ? $"Audio assigned: {tsc.mainThemeClip.name}"
            : "Audio NOT FOUND — drag your mp3 into TitleManager → Main Theme Clip";

        EditorUtility.DisplayDialog("Title Sequence Built",
            $"Saved to: {scenePath}\n{audioMsg}\n\n" +
            "BUILT:\n" +
            "• Misty ash field with terrain, hills, rocks, ash + mist particles\n" +
            "• Luthadel street with 10 buildings, 4 lanterns, ash, fog\n" +
            "• Kredik Shaw with 20+ spires + surrounding city blocks\n" +
            "• Camera controller (field dolly → street dolly → aerial orbit)\n" +
            "• All UI: logos, credits, MISTBORN title, subtitle, skip hint\n" +
            "• TitleSequenceController fully wired\n\n" +
            "NEEDS ARTIST:\n" +
            "• Replace grey-box buildings with textured models\n" +
            "• Replace Kredik Shaw primitives with proper spire model\n" +
            "• Add logo images (Crimson Blade, Sanderson/Dragonsteel)\n" +
            "• Custom Mistborn font for the title\n" +
            "• Ground/wall textures, skybox\n" +
            "• Post-processing (bloom for title glow)",
            "OK");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ENVIRONMENT BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static void CreateGroundPlane(Transform parent, Vector3 pos, float scale, Color color)
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Ground";
        plane.transform.SetParent(parent);
        plane.transform.position = pos;
        plane.transform.localScale = new Vector3(scale, 1f, scale);
        ApplyColor(plane, color);
    }

    static void CreateHill(Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        var hill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hill.name = "Hill";
        hill.transform.SetParent(parent);
        hill.transform.position = pos;
        hill.transform.localScale = scale;
        ApplyColor(hill, color);
    }

    static void CreateRock(Transform parent, Vector3 pos, float size, Color color)
    {
        var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.SetParent(parent);
        rock.transform.position = pos;
        rock.transform.localScale = new Vector3(size, size * 0.6f, size * Random.Range(0.7f, 1.3f));
        rock.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
        ApplyColor(rock, color);
    }

    static void CreateStump(Transform parent, Vector3 pos, float height)
    {
        var stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stump.name = "DeadTreeStump";
        stump.transform.SetParent(parent);
        stump.transform.position = pos + new Vector3(0f, height * 0.5f, 0f);
        stump.transform.localScale = new Vector3(0.15f, height * 0.5f, 0.15f);
        stump.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), 0f, Random.Range(-8f, 8f));
        ApplyColor(stump, COL_WOOD);
    }

    static void CreateAshPile(Transform parent, Vector3 pos, float size)
    {
        var pile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pile.name = "AshPile";
        pile.transform.SetParent(parent);
        pile.transform.position = pos;
        pile.transform.localScale = new Vector3(size, size * 0.15f, size * Random.Range(0.8f, 1.2f));
        ApplyColor(pile, COL_ASH_GROUND);
    }

    static void CreateStreetGround(Transform parent, Vector3 offset, Vector3 scale, Color color)
    {
        var street = GameObject.CreatePrimitive(PrimitiveType.Plane);
        street.name = "StreetGround";
        street.transform.SetParent(parent);
        street.transform.position = offset;
        street.transform.localScale = scale;
        ApplyColor(street, color);
    }

    static void CreateBarrel(Transform parent, Vector3 pos)
    {
        var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        barrel.transform.SetParent(parent);
        barrel.transform.position = pos + new Vector3(0f, 0.4f, 0f);
        barrel.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
        ApplyColor(barrel, COL_WOOD);
    }

    static void CreateCrate(Transform parent, Vector3 pos)
    {
        var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Crate";
        crate.transform.SetParent(parent);
        crate.transform.position = pos + new Vector3(0f, 0.3f, 0f);
        crate.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        crate.transform.rotation = Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f);
        ApplyColor(crate, new Color(COL_WOOD.r + 0.05f, COL_WOOD.g + 0.03f, COL_WOOD.b));
    }

    static void CreateBuilding(Transform parent, Vector3 pos, Vector3 size, Color wallColor, Color roofColor)
    {
        // Main body
        var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bldg.name = "Building";
        bldg.transform.SetParent(parent);
        bldg.transform.position = pos + new Vector3(0f, size.y * 0.5f, 0f);
        bldg.transform.localScale = size;
        bldg.transform.rotation = Quaternion.Euler(0f, Random.Range(-3f, 3f), 0f);
        ApplyColor(bldg, wallColor);

        // Roof — flat slab on top, distinct color
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(bldg.transform, false);
        roof.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        roof.transform.localScale = new Vector3(1.05f, 0.06f, 1.05f);
        ApplyColor(roof, roofColor);

        // Trim / ledge near the top — different shade from wall
        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "Trim";
        trim.transform.SetParent(bldg.transform, false);
        trim.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        trim.transform.localScale = new Vector3(1.02f, 0.03f, 1.02f);
        Color trimColor = Color.Lerp(wallColor, roofColor, 0.5f);
        ApplyColor(trim, trimColor);

        // Windows — varied warm/cool glow
        int windowRows = Mathf.FloorToInt(size.y / 2.5f);
        for (int w = 0; w < windowRows; w++)
        {
            float wy = pos.y + 2f + w * 2.5f;
            if (wy > pos.y + size.y - 1f) break;

            // Street-facing windows
            float wx = pos.x > 0 ? pos.x - size.x * 0.5f + 0.05f : pos.x + size.x * 0.5f - 0.05f;

            // 2 windows per row at different z positions
            for (int wz = 0; wz < 2; wz++)
            {
                float zOff = pos.z + (wz == 0 ? -size.z * 0.2f : size.z * 0.2f);
                var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = "Window";
                win.transform.SetParent(bldg.transform, true);
                win.transform.position = new Vector3(wx, wy, zOff);
                win.transform.localScale = new Vector3(0.08f, 0.7f, 0.4f);

                // Some windows warm, some cool, some dark (unlit)
                float roll = Random.Range(0f, 1f);
                if (roll < 0.4f)
                    ApplyEmissive(win, COL_WINDOW_WARM * Random.Range(0.2f, 0.5f));
                else if (roll < 0.6f)
                    ApplyEmissive(win, COL_WINDOW_COOL * 0.3f);
                else
                    ApplyColor(win, new Color(0.05f, 0.05f, 0.05f)); // dark / shuttered
            }
        }

        // Door on ground floor (darker rectangle)
        float doorX = pos.x > 0 ? pos.x - size.x * 0.5f + 0.05f : pos.x + size.x * 0.5f - 0.05f;
        var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(bldg.transform, true);
        door.transform.position = new Vector3(doorX, pos.y + 0.9f, pos.z);
        door.transform.localScale = new Vector3(0.08f, 1.6f, 0.7f);
        ApplyColor(door, new Color(COL_WOOD.r * 0.7f, COL_WOOD.g * 0.7f, COL_WOOD.b * 0.7f));
    }

    static void CreateLantern(Transform parent, Vector3 pos)
    {
        // Bracket (metal rod)
        var bracket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bracket.name = "LanternBracket";
        bracket.transform.SetParent(parent);
        bracket.transform.position = pos;
        bracket.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
        bracket.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(bracket, COL_METAL);

        // Lantern body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "LanternBody";
        body.transform.SetParent(parent);
        body.transform.position = pos + new Vector3(pos.x > 0 ? -0.4f : 0.4f, -0.2f, 0f);
        body.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
        ApplyEmissive(body, COL_LANTERN * 0.5f);

        // Point light
        var lightObj = new GameObject("LanternLight");
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = body.transform.position;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = COL_LANTERN;
        light.intensity = 1.5f;
        light.range = 8f;
    }

    static void CreateKredikShaw(Transform parent, Vector3 center)
    {
        // Central tower — tallest spire
        CreateSpire(parent, center + new Vector3(0f, 0f, 0f), 2f, 35f);

        // Ring of major spires
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            float r = 6f;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            float height = Random.Range(18f, 28f);
            float radius = Random.Range(1f, 1.8f);
            CreateSpire(parent, pos, radius, height);
        }

        // Outer ring of smaller spires
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f + 0.15f;
            float r = 11f;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            float height = Random.Range(10f, 18f);
            float radius = Random.Range(0.6f, 1.2f);
            CreateSpire(parent, pos, radius, height);
        }

        // Base platform
        var basePlat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlat.name = "KredikShawBase";
        basePlat.transform.SetParent(parent);
        basePlat.transform.position = center + new Vector3(0f, 1.5f, 0f);
        basePlat.transform.localScale = new Vector3(16f, 1.5f, 16f);
        ApplyColor(basePlat, COL_SPIRE);
    }

    static void CreateSpire(Transform parent, Vector3 pos, float radius, float height)
    {
        // Vary the spire color slightly so they're not all identical
        Color spireCol = new Color(
            COL_SPIRE.r + Random.Range(-0.02f, 0.03f),
            COL_SPIRE.g + Random.Range(-0.02f, 0.03f),
            COL_SPIRE.b + Random.Range(-0.01f, 0.04f));

        // Cylinder body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Spire";
        body.transform.SetParent(parent);
        body.transform.position = pos + new Vector3(0f, height * 0.5f, 0f);
        body.transform.localScale = new Vector3(radius, height * 0.5f, radius);
        ApplyColor(body, spireCol);

        // Cone tip (stretched sphere) — lighter
        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "SpireTip";
        tip.transform.SetParent(body.transform, false);
        tip.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        tip.transform.localScale = new Vector3(0.5f, 1.8f, 0.5f);
        ApplyColor(tip, COL_SPIRE_TIP);

        // Mid-ring detail (architectural band)
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "SpireRing";
        ring.transform.SetParent(body.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        ring.transform.localScale = new Vector3(1.15f, 0.02f, 1.15f);
        ApplyColor(ring, COL_METAL);

        // Slight random lean
        body.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));
    }

    // Color palette for city blocks seen from above
    static readonly Color[] CITY_COLORS = {
        new Color(0.18f, 0.15f, 0.13f),  // dark brown
        new Color(0.24f, 0.20f, 0.16f),  // medium brown
        new Color(0.30f, 0.26f, 0.22f),  // light brown
        new Color(0.28f, 0.18f, 0.14f),  // reddish
        new Color(0.22f, 0.22f, 0.25f),  // cool grey
        new Color(0.16f, 0.14f, 0.12f),  // very dark
        new Color(0.20f, 0.18f, 0.20f),  // purple-grey
    };

    static readonly Color[] CITY_ROOF_COLORS = {
        new Color(0.20f, 0.22f, 0.25f),  // slate
        new Color(0.30f, 0.18f, 0.10f),  // clay
        new Color(0.15f, 0.15f, 0.18f),  // dark slate
        new Color(0.25f, 0.22f, 0.18f),  // tan
    };

    static void CreateCityBlock(Transform parent, Vector3 center)
    {
        int count = Random.Range(3, 7);
        for (int i = 0; i < count; i++)
        {
            float x = center.x + Random.Range(-6f, 6f);
            float z = center.z + Random.Range(-6f, 6f);
            float h = Random.Range(3f, 10f);
            float w = Random.Range(3f, 7f);
            float d = Random.Range(3f, 7f);

            var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "CityBuilding";
            bldg.transform.SetParent(parent);
            bldg.transform.position = new Vector3(x, h * 0.5f, z);
            bldg.transform.localScale = new Vector3(w, h, d);
            bldg.transform.rotation = Quaternion.Euler(0f, Random.Range(-8f, 8f), 0f);

            ApplyColor(bldg, CITY_COLORS[Random.Range(0, CITY_COLORS.Length)]);

            // Roof slab
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "CityRoof";
            roof.transform.SetParent(bldg.transform, false);
            roof.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            roof.transform.localScale = new Vector3(1.03f, 0.04f, 1.03f);
            ApplyColor(roof, CITY_ROOF_COLORS[Random.Range(0, CITY_ROOF_COLORS.Length)]);

            // Random window light (30% chance per building)
            if (Random.Range(0f, 1f) < 0.3f)
            {
                var winLight = new GameObject("WindowGlow");
                winLight.transform.SetParent(bldg.transform, false);
                winLight.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f), 0.5f);
                var wl = winLight.AddComponent<Light>();
                wl.type = LightType.Point;
                wl.color = COL_WINDOW_WARM;
                wl.intensity = 0.4f;
                wl.range = 3f;
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PARTICLE BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static ParticleSystem CreateAshParticles(Transform parent, Vector3 pos, float spread)
    {
        var obj = new GameObject("AshParticles");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 10f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = COL_ASH_PARTICLE;
        main.maxParticles = 600;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.12f;
        var em = ps.emission;
        em.rateOverTime = 50f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spread, 0.1f, spread);
        // Wind via noise module instead of velocity curves (avoids mode mismatch)
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;
        noise.octaveCount = 2;
        return ps;
    }

    static ParticleSystem CreateMistParticles(Transform parent, Vector3 pos, float spread)
    {
        var obj = new GameObject("MistParticles");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 14f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startColor = COL_MIST;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = 3f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spread, 0.3f, spread);
        // Gentle drift via noise (avoids velocity curve mode mismatch)
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.1f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.1f;
        noise.octaveCount = 1;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
        return ps;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════════════════════

    // Cache the default pipeline material by grabbing it from a temp primitive.
    // This is the ONLY reliable way to get a working material on any pipeline
    // (Built-in, URP, HDRP) because Unity auto-assigns the correct one.
    private static Material _baseMat;
    static Material GetBaseMaterial()
    {
        if (_baseMat != null) return _baseMat;
        var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _baseMat = new Material(temp.GetComponent<Renderer>().sharedMaterial);
        Object.DestroyImmediate(temp);
        return _baseMat;
    }

    static void ApplyColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var mat = new Material(GetBaseMaterial());
        // Set every known color property so it works on any pipeline
        mat.color = color;
        if (mat.HasProperty("_BaseColor"))   mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))       mat.SetColor("_Color", color);
        if (mat.HasProperty("_Smoothness"))  mat.SetFloat("_Smoothness", 0.05f);
        if (mat.HasProperty("_Glossiness"))  mat.SetFloat("_Glossiness", 0.05f);
        if (mat.HasProperty("_Metallic"))    mat.SetFloat("_Metallic", 0f);
        rend.sharedMaterial = mat;
    }

    static void ApplyEmissive(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        var mat = new Material(GetBaseMaterial());
        Color bright = color * 2.5f;
        bright.a = 1f;
        mat.color = bright;
        if (mat.HasProperty("_BaseColor"))   mat.SetColor("_BaseColor", bright);
        if (mat.HasProperty("_Color"))       mat.SetColor("_Color", bright);
        if (mat.HasProperty("_Smoothness"))  mat.SetFloat("_Smoothness", 0f);
        if (mat.HasProperty("_Glossiness"))  mat.SetFloat("_Glossiness", 0f);
        // Emission
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", bright);
        if (mat.HasProperty("_EmissiveColor"))
            mat.SetColor("_EmissiveColor", bright);
        if (mat.HasProperty("_EmissiveIntensity"))
            mat.SetFloat("_EmissiveIntensity", 3f);
        if (mat.HasProperty("_UseEmissiveIntensity"))
            mat.SetFloat("_UseEmissiveIntensity", 1f);
        rend.sharedMaterial = mat;
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

    static GameObject CreateOverlay(Transform parent, string name, Color color, float alpha)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.color = color;
        StretchFill(obj.GetComponent<RectTransform>());
        var cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
        return obj;
    }

    static GameObject CreateLogoGroup(Transform parent, string name, string text, float fontSize)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        StretchFill(obj.AddComponent<RectTransform>());
        var cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Logo image placeholder (white square — assign your actual logo PNG here)
        var imgObj = new GameObject("LogoImage");
        imgObj.transform.SetParent(obj.transform, false);
        var img = imgObj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.9f);
        var imgRT = imgObj.GetComponent<RectTransform>();
        imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0.5f, 0.5f);
        imgRT.anchoredPosition = new Vector2(0f, 30f);
        imgRT.sizeDelta = new Vector2(200f, 200f);
        // Set to "None" sprite — designer replaces with actual logo
        img.sprite = null;
        img.preserveAspect = true;

        // Text below the image
        var tmp = CreateTMP(obj.transform, "LogoText", text, fontSize,
            COL_TEXT, TextAlignmentOptions.Center);
        var rt = tmp.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -100f);
        rt.sizeDelta = new Vector2(700f, 100f);

        return obj;
    }

    static void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static AudioClip FindAudioClip(string search)
    {
        string[] guids = AssetDatabase.FindAssets(search + " t:AudioClip");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
    }

    static void AddSceneToBuild(string path, int index)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(s => s.path == path);
        var entry = new EditorBuildSettingsScene(path, true);
        if (index >= scenes.Count) scenes.Add(entry);
        else scenes.Insert(index, entry);
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
