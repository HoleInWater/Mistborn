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
    static readonly Color COL_ASH_PARTICLE = new Color(0.35f, 0.32f, 0.28f, 0.7f);
    static readonly Color COL_MIST   = new Color(0.7f, 0.72f, 0.75f, 0.15f);

    // Building palette
    static readonly Color COL_STONE_DARK  = new Color(0.12f, 0.11f, 0.10f);
    static readonly Color COL_STONE_MED   = new Color(0.18f, 0.16f, 0.14f);
    static readonly Color COL_STONE_LIGHT = new Color(0.22f, 0.20f, 0.17f);
    static readonly Color COL_METAL       = new Color(0.25f, 0.25f, 0.28f);
    static readonly Color COL_GROUND      = new Color(0.08f, 0.07f, 0.06f);
    static readonly Color COL_SKY         = new Color(0.04f, 0.03f, 0.05f);
    static readonly Color COL_LANTERN     = new Color(0.9f, 0.45f, 0.1f);
    static readonly Color COL_SPIRE       = new Color(0.10f, 0.10f, 0.12f);

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

        // Ground — multiple overlapping planes for depth
        CreateGroundPlane(mistyField.transform, Vector3.zero, 50f);
        CreateGroundPlane(mistyField.transform, new Vector3(0, -0.02f, 30f), 40f);

        // Distant horizon hills (stretched cubes as silhouettes)
        CreateHill(mistyField.transform, new Vector3(-30f, 2f, 80f), new Vector3(25f, 5f, 4f));
        CreateHill(mistyField.transform, new Vector3(15f, 1.5f, 90f), new Vector3(30f, 4f, 3f));
        CreateHill(mistyField.transform, new Vector3(-10f, 3f, 100f), new Vector3(40f, 7f, 5f));
        CreateHill(mistyField.transform, new Vector3(40f, 2.5f, 85f), new Vector3(20f, 6f, 4f));

        // Dim sun
        var sunObj = new GameObject("DimSun");
        sunObj.transform.SetParent(mistyField.transform);
        var sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(0.65f, 0.25f, 0.1f);
        sun.intensity = 0.25f;
        sunObj.transform.rotation = Quaternion.Euler(20f, -25f, 0f);

        // Ash particles
        var ashPS = CreateAshParticles(mistyField.transform, new Vector3(0f, 12f, 10f), 80f);

        // Mist particles
        var mistPS = CreateMistParticles(mistyField.transform, new Vector3(0f, 0.3f, 8f), 40f);

        // Scattered dead objects (rocks / stumps)
        CreateRock(mistyField.transform, new Vector3(-5f, 0.15f, 8f), 0.5f);
        CreateRock(mistyField.transform, new Vector3(3f, 0.1f, 12f), 0.3f);
        CreateRock(mistyField.transform, new Vector3(-8f, 0.2f, 15f), 0.7f);
        CreateRock(mistyField.transform, new Vector3(7f, 0.12f, 6f), 0.25f);
        CreateRock(mistyField.transform, new Vector3(-2f, 0.18f, 20f), 0.45f);

        // ══════════════════════════════════════════════════════════════════
        // PHASE 3: LUTHADEL STREETS
        // ══════════════════════════════════════════════════════════════════
        var luthadelGroup = new GameObject("LuthadelStreetsGroup");
        luthadelGroup.SetActive(false);

        // Street ground
        CreateStreetGround(luthadelGroup.transform);

        // Buildings — left side
        CreateBuilding(luthadelGroup.transform, new Vector3(-6f, 0f, -12f), new Vector3(5f, 8f, 6f), COL_STONE_DARK);
        CreateBuilding(luthadelGroup.transform, new Vector3(-6.5f, 0f, -4f), new Vector3(6f, 10f, 7f), COL_STONE_MED);
        CreateBuilding(luthadelGroup.transform, new Vector3(-5.5f, 0f, 4f), new Vector3(4.5f, 7f, 6f), COL_STONE_DARK);
        CreateBuilding(luthadelGroup.transform, new Vector3(-6f, 0f, 11f), new Vector3(5.5f, 12f, 5f), COL_STONE_LIGHT);
        CreateBuilding(luthadelGroup.transform, new Vector3(-7f, 0f, 18f), new Vector3(6f, 9f, 7f), COL_STONE_MED);

        // Buildings — right side
        CreateBuilding(luthadelGroup.transform, new Vector3(6f, 0f, -10f), new Vector3(5f, 9f, 8f), COL_STONE_MED);
        CreateBuilding(luthadelGroup.transform, new Vector3(5.5f, 0f, -1f), new Vector3(4f, 6f, 5f), COL_STONE_DARK);
        CreateBuilding(luthadelGroup.transform, new Vector3(6.5f, 0f, 6f), new Vector3(6f, 11f, 6f), COL_STONE_LIGHT);
        CreateBuilding(luthadelGroup.transform, new Vector3(5f, 0f, 14f), new Vector3(5f, 8f, 7f), COL_STONE_DARK);
        CreateBuilding(luthadelGroup.transform, new Vector3(6f, 0f, 22f), new Vector3(5.5f, 10f, 5f), COL_STONE_MED);

        // Lanterns on walls
        CreateLantern(luthadelGroup.transform, new Vector3(-3.2f, 4f, -8f));
        CreateLantern(luthadelGroup.transform, new Vector3(3.2f, 3.5f, 0f));
        CreateLantern(luthadelGroup.transform, new Vector3(-3f, 4.5f, 8f));
        CreateLantern(luthadelGroup.transform, new Vector3(3.5f, 4f, 16f));

        // Street ash
        CreateAshParticles(luthadelGroup.transform, new Vector3(0f, 8f, 5f), 25f);
        CreateMistParticles(luthadelGroup.transform, new Vector3(0f, 0.2f, 5f), 15f);

        // Dim street light
        var streetSun = new GameObject("StreetAmbient");
        streetSun.transform.SetParent(luthadelGroup.transform);
        var sl = streetSun.AddComponent<Light>();
        sl.type = LightType.Directional;
        sl.color = new Color(0.3f, 0.2f, 0.15f);
        sl.intensity = 0.15f;
        streetSun.transform.rotation = Quaternion.Euler(40f, 10f, 0f);

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

        // City ground
        var cityGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        cityGround.name = "CityGround";
        cityGround.transform.SetParent(kredikGroup.transform);
        cityGround.transform.localScale = new Vector3(30f, 1f, 30f);
        ApplyColor(cityGround, new Color(0.06f, 0.05f, 0.05f));

        // Mist rolling through streets from above
        var cityMist = CreateMistParticles(kredikGroup.transform, new Vector3(0f, 2f, 0f), 80f);
        var cm = cityMist.main;
        cm.startSize = new ParticleSystem.MinMaxCurve(5f, 15f);

        // Overhead ash
        CreateAshParticles(kredikGroup.transform, new Vector3(0f, 70f, 0f), 120f);

        // Very dim overhead light
        var cityLight = new GameObject("CityMoonlight");
        cityLight.transform.SetParent(kredikGroup.transform);
        var cl = cityLight.AddComponent<Light>();
        cl.type = LightType.Directional;
        cl.color = new Color(0.2f, 0.2f, 0.3f);
        cl.intensity = 0.1f;
        cityLight.transform.rotation = Quaternion.Euler(60f, -20f, 0f);

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

    static void CreateGroundPlane(Transform parent, Vector3 pos, float scale)
    {
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Ground";
        plane.transform.SetParent(parent);
        plane.transform.position = pos;
        plane.transform.localScale = new Vector3(scale, 1f, scale);
        ApplyColor(plane, COL_GROUND);
    }

    static void CreateHill(Transform parent, Vector3 pos, Vector3 scale)
    {
        var hill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hill.name = "Hill";
        hill.transform.SetParent(parent);
        hill.transform.position = pos;
        hill.transform.localScale = scale;
        ApplyColor(hill, new Color(COL_GROUND.r + 0.02f, COL_GROUND.g + 0.02f, COL_GROUND.b + 0.01f));
    }

    static void CreateRock(Transform parent, Vector3 pos, float size)
    {
        var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.SetParent(parent);
        rock.transform.position = pos;
        rock.transform.localScale = new Vector3(size, size * 0.6f, size);
        rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));
        ApplyColor(rock, new Color(0.12f, 0.11f, 0.10f));
    }

    static void CreateStreetGround(Transform parent)
    {
        var street = GameObject.CreatePrimitive(PrimitiveType.Plane);
        street.name = "StreetGround";
        street.transform.SetParent(parent);
        street.transform.position = Vector3.zero;
        street.transform.localScale = new Vector3(4f, 1f, 10f);
        ApplyColor(street, new Color(0.10f, 0.09f, 0.08f));
    }

    static void CreateBuilding(Transform parent, Vector3 pos, Vector3 size, Color color)
    {
        var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bldg.name = "Building";
        bldg.transform.SetParent(parent);
        bldg.transform.position = pos + new Vector3(0f, size.y * 0.5f, 0f);
        bldg.transform.localScale = size;
        bldg.transform.rotation = Quaternion.Euler(0f, Random.Range(-3f, 3f), 0f);
        ApplyColor(bldg, color);

        // Window glow (small emissive cube recessed into the face)
        int windowCount = Mathf.FloorToInt(size.y / 2.5f);
        for (int w = 0; w < windowCount; w++)
        {
            float wy = pos.y + 2f + w * 2.5f;
            if (wy > pos.y + size.y - 1f) break;

            // Only add windows facing the street (inner face)
            float wx = pos.x > 0 ? pos.x - size.x * 0.5f + 0.05f : pos.x + size.x * 0.5f - 0.05f;

            var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
            win.name = "Window";
            win.transform.SetParent(bldg.transform, true);
            win.transform.position = new Vector3(wx, wy, pos.z + Random.Range(-1f, 1f));
            win.transform.localScale = new Vector3(0.1f, 0.8f, 0.5f);
            ApplyEmissive(win, COL_LANTERN * 0.3f);
        }
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
        // Cylinder body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Spire";
        body.transform.SetParent(parent);
        body.transform.position = pos + new Vector3(0f, height * 0.5f, 0f);
        body.transform.localScale = new Vector3(radius, height * 0.5f, radius);
        ApplyColor(body, COL_SPIRE);

        // Cone tip (stretched sphere)
        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "SpireTip";
        tip.transform.SetParent(body.transform, false);
        tip.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        tip.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
        ApplyColor(tip, new Color(COL_SPIRE.r + 0.03f, COL_SPIRE.g + 0.03f, COL_SPIRE.b + 0.04f));

        // Slight random lean
        body.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(0f, 360f), Random.Range(-2f, 2f));
    }

    static void CreateCityBlock(Transform parent, Vector3 center)
    {
        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
        {
            float x = center.x + Random.Range(-6f, 6f);
            float z = center.z + Random.Range(-6f, 6f);
            float h = Random.Range(3f, 9f);
            float w = Random.Range(3f, 7f);
            float d = Random.Range(3f, 7f);

            var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "CityBuilding";
            bldg.transform.SetParent(parent);
            bldg.transform.position = new Vector3(x, h * 0.5f, z);
            bldg.transform.localScale = new Vector3(w, h, d);
            bldg.transform.rotation = Quaternion.Euler(0f, Random.Range(-5f, 5f), 0f);

            Color c = Color.Lerp(COL_STONE_DARK, COL_STONE_MED, Random.Range(0f, 1f));
            ApplyColor(bldg, c);
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

        var tmp = CreateTMP(obj.transform, "LogoText", text, fontSize,
            COL_TEXT, TextAlignmentOptions.Center);
        var rt = tmp.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700f, 160f);

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
