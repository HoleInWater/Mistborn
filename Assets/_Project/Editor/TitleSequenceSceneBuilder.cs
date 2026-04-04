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
using UnityEngine.Rendering.HighDefinition;
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

    // Building palette — BRIGHT for HDRP (tonemapping compresses the range)
    // These are ~2-3× brighter than Standard pipeline values
    static readonly Color COL_STONE_DARK  = new Color(0.35f, 0.30f, 0.25f);  // dark brown-grey
    static readonly Color COL_STONE_MED   = new Color(0.50f, 0.44f, 0.38f);  // warm brown
    static readonly Color COL_STONE_LIGHT = new Color(0.60f, 0.55f, 0.46f);  // light sandy
    static readonly Color COL_STONE_RED   = new Color(0.55f, 0.30f, 0.22f);  // reddish brick
    static readonly Color COL_STONE_GREY  = new Color(0.45f, 0.45f, 0.50f);  // cool grey
    static readonly Color COL_WOOD        = new Color(0.40f, 0.28f, 0.16f);  // dark wood
    static readonly Color COL_ROOF_SLATE  = new Color(0.35f, 0.38f, 0.42f);  // blue-grey slate
    static readonly Color COL_ROOF_TILE   = new Color(0.55f, 0.35f, 0.20f);  // clay tile
    static readonly Color COL_METAL       = new Color(0.55f, 0.55f, 0.62f);  // steel blue-grey
    static readonly Color COL_GROUND      = new Color(0.25f, 0.20f, 0.16f);  // dark ash earth
    static readonly Color COL_GROUND_LIGHT = new Color(0.32f, 0.27f, 0.22f); // lighter path
    static readonly Color COL_COBBLE      = new Color(0.38f, 0.35f, 0.30f);  // cobblestone
    static readonly Color COL_SKY         = new Color(0.05f, 0.04f, 0.06f);  // near-black sky (gradient sky volume handles this)
    static readonly Color COL_LANTERN     = new Color(1.0f, 0.60f, 0.20f);   // warm orange
    static readonly Color COL_WINDOW_WARM = new Color(0.95f, 0.65f, 0.25f);  // warm window glow
    static readonly Color COL_WINDOW_COOL = new Color(0.45f, 0.55f, 0.75f);  // cool window (tin?)
    static readonly Color COL_SPIRE       = new Color(0.25f, 0.23f, 0.28f);  // dark steel
    static readonly Color COL_SPIRE_TIP   = new Color(0.38f, 0.35f, 0.42f);  // lighter tips
    static readonly Color COL_ROCK        = new Color(0.35f, 0.30f, 0.25f);  // visible rocks
    static readonly Color COL_ROCK_DARK   = new Color(0.25f, 0.22f, 0.18f);  // darker rocks
    static readonly Color COL_ASH_GROUND  = new Color(0.28f, 0.26f, 0.24f);  // ash deposits

    [MenuItem("Mistborn/Scenes/Build Title Sequence Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Reset material caches
        _sourceMat = null;
        _matCache.Clear();
        _namedMats.Clear();
        _pendingAssignments.Clear();
        // Clean old generated materials and recreate fresh
        if (AssetDatabase.IsValidFolder("Assets/_Project/Materials/TitleSequence"))
            AssetDatabase.DeleteAsset("Assets/_Project/Materials/TitleSequence");
        AssetDatabase.Refresh();

        // ══════════════════════════════════════════════════════════════════
        // HDRP ENVIRONMENT — Sky, Exposure, Fog
        // HDRP renders BLACK without a sky volume. Lights use lux (not 0-1).
        // ══════════════════════════════════════════════════════════════════

        // Re-enable legacy fog as a fallback until HDRP Volume fog is set up.
        // This hides distant Z-fighting and adds atmospheric depth.
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.006f;
        RenderSettings.fogColor = new Color(0.06f, 0.06f, 0.08f);

        // HDRP sky + exposure Volume — essential or everything is black
        var volumeObj = new GameObject("HDRP_SkyVolume");
        var volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 0;
        // Profile needs to be created manually in the editor because
        // HDRP override types require assembly references we can't use here.
        // We'll log instructions.
        Debug.Log("[TitleSequenceBuilder] IMPORTANT — HDRP Sky Setup:");
        Debug.Log("  Select 'HDRP_SkyVolume' in Hierarchy → Add Override:");
        Debug.Log("  1. Visual Environment → Sky Type: Gradient, Ambient Mode: Static");
        Debug.Log("  2. Gradient Sky → Top=#050510, Middle=#0D0A08, Bottom=#1A1210");
        Debug.Log("  3. Exposure → Mode: Fixed, Fixed Exposure: 10");
        Debug.Log("  4. Fog → Enable: true, Base Height: 0, Max Height: 20, Mean Free Path: 40");
        Debug.Log("  If sky is STILL black, check Window → Rendering → HDRP Wizard");

        // ══════════════════════════════════════════════════════════════════
        // CAMERA
        // ══════════════════════════════════════════════════════════════════
        var camObj = new GameObject("TitleCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox; // HDRP needs Skybox, not SolidColor
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
        CreateGroundPlane(mistyField.transform, new Vector3(-8f, 0.12f, 15f), 8f, COL_ASH_GROUND);
        CreateGroundPlane(mistyField.transform, new Vector3(5f, 0.14f, 25f), 6f, COL_GROUND_LIGHT);
        CreateGroundPlane(mistyField.transform, new Vector3(-3f, 0.11f, 5f), 4f, COL_ASH_GROUND);

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
        sun.color = new Color(0.85f, 0.35f, 0.12f); // redder sun — ash-filtered
        sun.intensity = 15000f; // EV10 needs high lux
        SetupHDRPLight(sun, 15000f);
        sunObj.transform.rotation = Quaternion.Euler(15f, -30f, 0f);

        // Red fill light from the ashmount glow (lore: constant volcanic light)
        var redFill = new GameObject("RedFillLight");
        redFill.transform.SetParent(mistyField.transform);
        var redLight = redFill.AddComponent<Light>();
        redLight.type = LightType.Directional;
        redLight.color = new Color(0.9f, 0.2f, 0.05f); // deep red
        redLight.intensity = 5000f;
        SetupHDRPLight(redLight, 5000f);
        redFill.transform.rotation = Quaternion.Euler(5f, 10f, 0f); // low angle from ashmount direction

        // Ash particles
        var ashPS = CreateAshParticles(mistyField.transform, new Vector3(0f, 12f, 10f), 80f);

        // Mist particles
        // Mist layers — 2 is enough with fog handling the rest
        var mistPS = CreateMistParticles(mistyField.transform, new Vector3(0f, 0.5f, 8f), 40f);
        CreateMistParticles(mistyField.transform, new Vector3(0f, 2f, 20f), 35f);

        // Mist controller — makes mists roll in, pulse, and react
        mistyField.AddComponent<TitleMistController>();

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
        CreateAshPile(mistyField.transform, new Vector3(8f, 0.10f, 7f), 1.0f);

        // Broken fence line (posts + fallen rail)
        CreateFencePost(mistyField.transform, new Vector3(-3f, 0f, 6f));
        CreateFencePost(mistyField.transform, new Vector3(-3f, 0f, 8f));
        CreateFencePost(mistyField.transform, new Vector3(-3f, 0f, 10f));
        CreateFallenRail(mistyField.transform, new Vector3(-3f, 0.15f, 7f), 2.2f);

        // Dirt path (slightly lighter ground strip)
        CreateGroundPlane(mistyField.transform, new Vector3(1f, 0.15f, 12f), 1.5f, COL_GROUND_LIGHT);

        // Ember particles near ashmount (glowing orange specks in distance)
        CreateEmberParticles(mistyField.transform, new Vector3(0f, 8f, 100f));

        // Distant Luthadel silhouette on the horizon (city you're heading toward)
        CreateDistantCitySilhouette(mistyField.transform, new Vector3(0f, 0f, 70f));

        // Ruined cart on the roadside
        CreateRuinedCart(mistyField.transform, new Vector3(4f, 0f, 16f));

        // Second broken fence on the other side of the path
        CreateFencePost(mistyField.transform, new Vector3(5f, 0f, 10f));
        CreateFencePost(mistyField.transform, new Vector3(5f, 0f, 12f));
        CreateFallenRail(mistyField.transform, new Vector3(5f, 0.08f, 11f), 2.2f);

        // Scrub vegetation (dead, brown/grey, low to ground)
        CreateDeadShrub(mistyField.transform, new Vector3(-7f, 0f, 6f));
        CreateDeadShrub(mistyField.transform, new Vector3(9f, 0f, 14f));
        CreateDeadShrub(mistyField.transform, new Vector3(-1f, 0f, 22f));
        CreateDeadShrub(mistyField.transform, new Vector3(3f, 0f, 4f));

        // Collapsed stone ruin (old structure from before the Lord Ruler)
        CreateRuin(mistyField.transform, new Vector3(-10f, 0f, 20f));

        // Skaa shanty — lean-to shelters outside the city walls
        CreateSkaaShanty(mistyField.transform, new Vector3(8f, 0f, 28f));
        CreateSkaaShanty(mistyField.transform, new Vector3(12f, 0f, 30f));

        // Stray dog silhouette near the path
        CreateStrayAnimal(mistyField.transform, new Vector3(3f, 0f, 8f));

        // Scattered coins on the dirt path (a Mistborn passed through here)
        CreateScatteredCoins(mistyField.transform, new Vector3(1f, 0.02f, 11f));

        // Watchtower near the city wall (visible from the field — guards keep watch)
        CreateWatchtower(mistyField.transform, new Vector3(15f, 0f, 55f));

        // Skeleton / remains near the road (grim reminder — the Final Empire is cruel)
        CreateSkeleton(mistyField.transform, new Vector3(-2f, 0f, 18f));

        // Gibbet post (iron cage on a pole — executed criminal left as warning)
        CreateGibbetPost(mistyField.transform, new Vector3(6f, 0f, 25f));

        // Wagon wheel half-buried in ash (trade route long abandoned)
        CreateBuriedWheel(mistyField.transform, new Vector3(-6f, 0f, 12f));

        // Iron mile-marker post (metal — dangerous in a world of Allomancers)
        CreateMileMarker(mistyField.transform, new Vector3(2f, 0f, 9f));

        // Crow/raven perched on the gibbet (life finds a way)
        CreateBird(mistyField.transform, new Vector3(6.7f, 3.5f, 25f));

        // Ashmount glow at base (faint red-orange light on the horizon)
        var ashGlow = new GameObject("AshmountGlow");
        ashGlow.transform.SetParent(mistyField.transform);
        ashGlow.transform.position = new Vector3(0f, 3f, 120f);
        var ag = ashGlow.AddComponent<Light>();
        ag.type = LightType.Point;
        ag.color = new Color(1f, 0.3f, 0.05f);
        ag.intensity = 15000f; // EV10 — bright volcanic glow
        ag.range = 30f;

        // ══════════════════════════════════════════════════════════════════
        // PHASE 3: LUTHADEL STREETS
        // ══════════════════════════════════════════════════════════════════
        var luthadelGroup = new GameObject("LuthadelStreetsGroup");
        luthadelGroup.SetActive(false);

        // Street ground — cobblestone center, dirt edges
        CreateStreetGround(luthadelGroup.transform, new Vector3(0f, -0.05f, 0f), new Vector3(2f, 1f, 10f), COL_COBBLE);
        CreateStreetGround(luthadelGroup.transform, new Vector3(-2.5f, -0.15f, 0f), new Vector3(1f, 1f, 10f), COL_GROUND);
        CreateStreetGround(luthadelGroup.transform, new Vector3(2.5f, -0.15f, 0f), new Vector3(1f, 1f, 10f), COL_GROUND);
        // Ash deposits on street
        CreateAshPile(luthadelGroup.transform, new Vector3(-1f, 0.10f, 5f), 0.8f);
        CreateAshPile(luthadelGroup.transform, new Vector3(1.5f, 0.10f, -3f), 0.6f);

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

        // Awnings (flat tilted planes over some doors)
        CreateAwning(luthadelGroup.transform, new Vector3(-3.3f, 3.2f, -4f), true);
        CreateAwning(luthadelGroup.transform, new Vector3(3.3f, 2.8f, 6f), false);
        CreateAwning(luthadelGroup.transform, new Vector3(-3.2f, 3.0f, 11f), true);

        // Gutter / drain running down the center of the street
        CreateGutter(luthadelGroup.transform);

        // Skaa silhouettes — dark humanoid shapes huddled or walking
        CreateSkaaSilhouette(luthadelGroup.transform, new Vector3(-2.2f, 0f, -3f), true);
        CreateSkaaSilhouette(luthadelGroup.transform, new Vector3(1.8f, 0f, 8f), false);
        CreateSkaaSilhouette(luthadelGroup.transform, new Vector3(-1.5f, 0f, 15f), true);

        // Hanging sign (metal bracket + sign board)
        CreateHangingSign(luthadelGroup.transform, new Vector3(-3.2f, 5.5f, 0f), true);
        CreateHangingSign(luthadelGroup.transform, new Vector3(3.0f, 4.8f, 12f), false);

        // Archway between buildings (connects two buildings overhead)
        CreateArchway(luthadelGroup.transform, new Vector3(0f, 7f, -7f), 7f);
        CreateArchway(luthadelGroup.transform, new Vector3(0f, 8f, 13f), 6f);

        // Steps / stoops in front of some buildings
        CreateSteps(luthadelGroup.transform, new Vector3(-3.3f, 0f, -4f), true);
        CreateSteps(luthadelGroup.transform, new Vector3(3.0f, 0f, 14f), false);

        // Chimney smoke on taller buildings
        CreateSmokeParticles(luthadelGroup.transform, new Vector3(-6f, 12f, -4f));
        CreateSmokeParticles(luthadelGroup.transform, new Vector3(6.5f, 11f, 6f));
        CreateSmokeParticles(luthadelGroup.transform, new Vector3(-6f, 12f, 11f));

        // Wall-mounted torches (different from lanterns — more rustic, brighter)
        CreateWallTorch(luthadelGroup.transform, new Vector3(-3.3f, 2.5f, 2f), true);
        CreateWallTorch(luthadelGroup.transform, new Vector3(3.1f, 2.8f, 18f), false);

        // Guard patrol silhouette (standing, armored, with weapon shape)
        CreateGuardSilhouette(luthadelGroup.transform, new Vector3(1.2f, 0f, -5f));

        // More barrels and clutter near building edges
        CreateBarrel(luthadelGroup.transform, new Vector3(2.4f, 0f, -8f));
        CreateCrate(luthadelGroup.transform, new Vector3(-2.7f, 0f, 12f));
        CreateCrate(luthadelGroup.transform, new Vector3(-2.4f, 0f, 12.5f));

        // Puddle near a drain
        CreatePuddle(luthadelGroup.transform, new Vector3(-0.5f, 0.003f, 9f), 0.5f);
        CreatePuddle(luthadelGroup.transform, new Vector3(0.3f, 0.003f, -2f), 0.3f);

        // Clothesline between buildings (stretched cylinder)
        CreateClothesline(luthadelGroup.transform, new Vector3(0f, 5f, 4f), 7f);

        // Sewer grate (metal — important for Allomancy world-building)
        CreateSewerGrate(luthadelGroup.transform, new Vector3(0.5f, 0.01f, -1f));
        CreateSewerGrate(luthadelGroup.transform, new Vector3(-0.3f, 0.01f, 17f));

        // Market stall (collapsed for the night — tarps over tables)
        CreateMarketStall(luthadelGroup.transform, new Vector3(-2.5f, 0f, 7f), true);
        CreateMarketStall(luthadelGroup.transform, new Vector3(2.3f, 0f, -9f), false);

        // Noble carriage parked on the street (one of the keeps must be nearby)
        CreateNobleCarriage(luthadelGroup.transform, new Vector3(1.5f, 0f, 20f));

        // Scattered metal debris (nails, scrap — glints in the lantern light)
        CreateMetalDebris(luthadelGroup.transform, new Vector3(-1f, 0.01f, 1f));
        CreateMetalDebris(luthadelGroup.transform, new Vector3(0.8f, 0.01f, 11f));

        // Obligator silhouette (robed figure — distinct from guards and skaa)
        CreateObligatorSilhouette(luthadelGroup.transform, new Vector3(-1f, 0f, 19f));

        // Lord Ruler's banner on tall building (dark red with gold trim)
        CreateBanner(luthadelGroup.transform, new Vector3(-5.5f, 9f, 4f));
        CreateBanner(luthadelGroup.transform, new Vector3(6.5f, 12f, 6f));

        // Metal chains between buildings (world-building — metal is controlled)
        CreateChain(luthadelGroup.transform, new Vector3(-3.3f, 6f, -1f), new Vector3(3.3f, 5.5f, -1f));

        // Balcony on a noble building
        CreateBalcony(luthadelGroup.transform, new Vector3(3.2f, 5f, 6f), false);
        CreateBalcony(luthadelGroup.transform, new Vector3(-3.4f, 6f, 11f), true);

        // Stone well in a widened area
        CreateWell(luthadelGroup.transform, new Vector3(0f, 0f, -10f));

        // Gallows / stocks in a small square (oppressive regime)
        CreateGallows(luthadelGroup.transform, new Vector3(0f, 0f, 24f));

        // Sleeping skaa in a doorway
        CreateSleepingSkaa(luthadelGroup.transform, new Vector3(-3.2f, 0f, 9f));
        CreateSleepingSkaa(luthadelGroup.transform, new Vector3(2.8f, 0f, -7f));

        // Notice board on a wall (Lord Ruler's decree)
        CreateNoticeBoard(luthadelGroup.transform, new Vector3(-3.3f, 3f, 6f), true);

        // Drainage pipe on building facade
        CreateDrainPipe(luthadelGroup.transform, new Vector3(-3.4f, 0f, -4f), 10f);
        CreateDrainPipe(luthadelGroup.transform, new Vector3(3.2f, 0f, 14f), 8f);

        // Second guard patrol (pair walking together — more presence)
        CreateGuardSilhouette(luthadelGroup.transform, new Vector3(-0.8f, 0f, 22f));

        // Skaa worker carrying a sack
        CreateSkaaWorker(luthadelGroup.transform, new Vector3(0.5f, 0f, 13f));

        // Stray cat on a crate
        CreateStrayAnimal(luthadelGroup.transform, new Vector3(2.7f, 0.6f, 3.3f));

        // Alley entrance between buildings (dark recessed gap)
        CreateAlleyEntrance(luthadelGroup.transform, new Vector3(-3.3f, 0f, -8f), true);
        CreateAlleyEntrance(luthadelGroup.transform, new Vector3(3.1f, 0f, 9f), false);

        // Light drizzle particles (very subtle rain)
        CreateDrizzleParticles(luthadelGroup.transform, new Vector3(0f, 10f, 5f));

        // Ash sweep pile (skaa pushed ash to the side of the street)
        CreateAshPile(luthadelGroup.transform, new Vector3(-2.8f, 0.03f, 0f), 1.2f);
        CreateAshPile(luthadelGroup.transform, new Vector3(2.6f, 0.03f, 8f), 0.9f);

        // Iron gate between districts (metal bars — Allomancy hazard)
        CreateIronGate(luthadelGroup.transform, new Vector3(0f, 0f, -14f));

        // Wagon parked by the market stalls
        CreateParkedWagon(luthadelGroup.transform, new Vector3(-2f, 0f, -11f));

        // Kelsier's crew safehouse hint — building with a slightly ajar cellar door
        // and a faint blue glow from within (someone is burning tin inside)
        CreateSafehouseHint(luthadelGroup.transform, new Vector3(5.5f, 0f, -1f));

        // Skaa huddled around a small fire in an alley entrance
        CreateAlleyFire(luthadelGroup.transform, new Vector3(-3.8f, 0f, -8f));

        // Loose cobblestones / broken street section
        CreateBrokenStreet(luthadelGroup.transform, new Vector3(0.5f, 0f, 6f));

        // Rat running along the gutter (tiny silhouette)
        CreateRat(luthadelGroup.transform, new Vector3(0.2f, 0.02f, 4f));

        // Wind-blown debris particles (leaves, paper, ash clumps)
        CreateWindDebris(luthadelGroup.transform, new Vector3(0f, 1.5f, 5f));

        // Terrisman steward (tall, robed — distinctive from other silhouettes)
        CreateTerrismanSilhouette(luthadelGroup.transform, new Vector3(-1.5f, 0f, 17f));

        // Rain splash particles at puddle locations
        CreateRainSplash(luthadelGroup.transform, new Vector3(-0.5f, 0.01f, 9f));
        CreateRainSplash(luthadelGroup.transform, new Vector3(0.3f, 0.01f, -2f));

        // Light shaft from a lantern (volumetric cone of light hitting the street)
        CreateLightShaft(luthadelGroup.transform, new Vector3(-3f, 4.5f, 4f), -1f);
        CreateLightShaft(luthadelGroup.transform, new Vector3(3.5f, 4f, 10f), 1f);

        // Street particles — ash + one mist layer
        CreateAshParticles(luthadelGroup.transform, new Vector3(0f, 8f, 5f), 25f);
        CreateMistParticles(luthadelGroup.transform, new Vector3(0f, 0.5f, 5f), 15f);

        // Mist controller for the street too
        luthadelGroup.AddComponent<TitleMistController>();

        // Dim street light — slightly brighter so buildings are visible
        var streetSun = new GameObject("StreetAmbient");
        streetSun.transform.SetParent(luthadelGroup.transform);
        var sl = streetSun.AddComponent<Light>();
        sl.type = LightType.Directional;
        sl.color = new Color(0.6f, 0.25f, 0.12f); // reddish street light — ash-filtered
        sl.intensity = 10000f; // EV10 compensation
        SetupHDRPLight(sl, 10000f); // street ambient (EV10)
        streetSun.transform.rotation = Quaternion.Euler(35f, 15f, 0f);

        // ══════════════════════════════════════════════════════════════════
        // PHASE 4: KREDIK SHAW + CITY FROM ABOVE
        // ══════════════════════════════════════════════════════════════════
        var kredikGroup = new GameObject("KredikShawGroup");
        kredikGroup.SetActive(false);

        // Kredik Shaw — "Hill of a Thousand Spires"
        CreateKredikShaw(kredikGroup.transform, Vector3.zero);

        // Perimeter walls connecting the outer spires
        CreatePerimeterWall(kredikGroup.transform, Vector3.zero, 14f, 8);

        // Grand gate (south side)
        CreateGate(kredikGroup.transform, new Vector3(0f, 0f, 15f));

        // Inner courtyard ground (slightly raised, different color)
        var courtyard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        courtyard.name = "Courtyard";
        courtyard.transform.SetParent(kredikGroup.transform);
        courtyard.transform.position = new Vector3(0f, 0.4f, 0f);
        courtyard.transform.localScale = new Vector3(12f, 0.2f, 12f);
        ApplyColor(courtyard, new Color(0.13f, 0.11f, 0.10f));

        // Glowing windows on some spires (visible from the aerial view)
        CreateSpireWindowLights(kredikGroup.transform, Vector3.zero, 6f, 8);

        // City wall — ring around the outer edge of the city
        CreateCityWall(kredikGroup.transform, Vector3.zero, 65f, 16);

        // Roads radiating from Kredik Shaw (darker strips on the ground)
        for (int r = 0; r < 4; r++)
        {
            float angle = r * 90f + 45f;
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.SetParent(kredikGroup.transform);
            road.transform.position = new Vector3(0f, 0.15f, 0f);
            road.transform.localScale = new Vector3(2f, 0.1f, 120f);
            road.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            ApplyColor(road, new Color(0.07f, 0.06f, 0.06f));
        }

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

        // City lights — scattered point lights for windows from above (with flicker)
        for (int i = 0; i < 30; i++)
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
            var wf = ptLight.AddComponent<TitleLightFlicker>();
            wf.style = TitleLightFlicker.FlickerStyle.WindowGlow;
        }

        // Canals running through the city (Luthadel has canals)
        CreateCanal(kredikGroup.transform, new Vector3(0f, 0.1f, 0f), 0f, 130f);    // north-south
        CreateCanal(kredikGroup.transform, new Vector3(0f, 0.1f, 0f), 90f, 100f);   // east-west
        CreateCanal(kredikGroup.transform, new Vector3(20f, 0.1f, 20f), 45f, 60f);   // diagonal

        // Noble keeps — larger buildings with distinct shape (4 in the city)
        CreateNobleKeep(kredikGroup.transform, new Vector3(30f, 0f, -25f));
        CreateNobleKeep(kredikGroup.transform, new Vector3(-35f, 0f, 20f));
        CreateNobleKeep(kredikGroup.transform, new Vector3(-20f, 0f, -40f));
        CreateNobleKeep(kredikGroup.transform, new Vector3(25f, 0f, 35f));

        // Steel Inquisitor standing atop the central spire — ICONIC
        CreateInquisitorSilhouette(kredikGroup.transform, new Vector3(0f, 36f, 0f));

        // Mistborn silhouette crouching on a rooftop (visible from aerial)
        CreateRooftopMistborn(kredikGroup.transform, new Vector3(22f, 10f, -18f));

        // Bridges over the canals
        CreateCanalBridge(kredikGroup.transform, new Vector3(18f, 0.5f, 0f), 0f);
        CreateCanalBridge(kredikGroup.transform, new Vector3(-15f, 0.5f, 0f), 0f);
        CreateCanalBridge(kredikGroup.transform, new Vector3(0f, 0.5f, 22f), 90f);
        CreateCanalBridge(kredikGroup.transform, new Vector3(0f, 0.5f, -20f), 90f);

        // Dock/pier along one canal (cargo loading area)
        CreateDock(kredikGroup.transform, new Vector3(25f, 0f, 2f));

        // Steel Ministry building (distinctive, separate from noble keeps)
        CreateSteelMinistry(kredikGroup.transform, new Vector3(-15f, 0f, 30f));

        // Guards at the Kredik Shaw gate
        CreateGuardSilhouette(kredikGroup.transform, new Vector3(-2f, 0f, 15.5f));
        CreateGuardSilhouette(kredikGroup.transform, new Vector3(2f, 0f, 15.5f));

        // Height fog layers (visible from above — mist at different altitudes)
        CreateMistParticles(kredikGroup.transform, new Vector3(0f, 5f, 0f), 60f);
        CreateMistParticles(kredikGroup.transform, new Vector3(0f, 15f, 0f), 40f);

        // Garrison / barracks near the city wall
        CreateBarracks(kredikGroup.transform, new Vector3(45f, 0f, -30f));

        // Market square (open area with scattered stalls)
        CreateMarketSquare(kredikGroup.transform, new Vector3(-30f, 0f, 15f));

        // Skaa quarter — denser, shorter buildings, no lights
        CreateSkaaQuarter(kredikGroup.transform, new Vector3(35f, 0f, 30f));

        // Steeljumping Mistborn arc — a figure mid-flight between rooftops
        CreateSteeljumpArc(kredikGroup.transform, new Vector3(-15f, 12f, 15f), new Vector3(-8f, 18f, 10f), new Vector3(-2f, 8f, 6f));

        // Smoke from forges / foundries in the industrial district
        CreateSmokeParticles(kredikGroup.transform, new Vector3(-30f, 8f, -30f));
        CreateSmokeParticles(kredikGroup.transform, new Vector3(-25f, 6f, -35f));

        // A few bright Allomantic blue line flashes (foreshadowing)
        CreateAllomanticLineFlash(kredikGroup.transform, new Vector3(15f, 12f, 10f), new Vector3(20f, 5f, 15f));
        CreateAllomanticLineFlash(kredikGroup.transform, new Vector3(-10f, 15f, -8f), new Vector3(-12f, 3f, -5f));

        // Scadrial has NO MOON — light comes from ashmount glow and city fires
        var cityLight = new GameObject("AshmountGlow_City");
        cityLight.transform.SetParent(kredikGroup.transform);
        var cl = cityLight.AddComponent<Light>();
        cl.type = LightType.Directional;
        cl.color = new Color(0.7f, 0.2f, 0.08f); // deep red from volcanic glow
        cl.intensity = 10000f; // EV10 compensation
        SetupHDRPLight(cl, 10000f); // ashmount glow (no moon on Scadrial)
        cityLight.transform.rotation = Quaternion.Euler(30f, -20f, 0f); // low angle — horizon glow

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

        // Mistborn running silhouette — dark figure that sprints across before the wipe
        var mistbornObj = new GameObject("MistbornSilhouette");
        mistbornObj.transform.SetParent(canvasObj.transform, false);
        var mbRT = mistbornObj.AddComponent<RectTransform>();
        mbRT.anchorMin = new Vector2(0.5f, 0.15f);
        mbRT.anchorMax = new Vector2(0.5f, 0.15f);
        mbRT.pivot = new Vector2(0.5f, 0f);
        mbRT.sizeDelta = new Vector2(120f, 280f);
        mbRT.anchoredPosition = new Vector2(-1920f, 0f); // starts off-screen
        mistbornObj.SetActive(false);

        // Body of the silhouette (dark capsule shape)
        var mbBody = new GameObject("Body");
        mbBody.transform.SetParent(mistbornObj.transform, false);
        var mbBodyImg = mbBody.AddComponent<Image>();
        mbBodyImg.color = new Color(0.02f, 0.02f, 0.03f, 0.95f);
        mbBodyImg.raycastTarget = false;
        var mbBodyRT = mbBody.GetComponent<RectTransform>();
        mbBodyRT.anchorMin = new Vector2(0.5f, 0.2f);
        mbBodyRT.anchorMax = new Vector2(0.5f, 0.8f);
        mbBodyRT.offsetMin = new Vector2(-20f, 0f);
        mbBodyRT.offsetMax = new Vector2(20f, 0f);

        // Mistcloak tassels trailing behind the figure
        for (int t = 0; t < 8; t++)
        {
            var trail = new GameObject($"CloakTrail_{t}");
            trail.transform.SetParent(mistbornObj.transform, false);
            var trailImg = trail.AddComponent<Image>();
            trailImg.color = new Color(0.03f, 0.03f, 0.04f, Random.Range(0.5f, 0.9f));
            trailImg.raycastTarget = false;
            var trailRT = trail.GetComponent<RectTransform>();
            trailRT.anchorMin = new Vector2(0f, 0f);
            trailRT.anchorMax = new Vector2(0f, 0f);
            trailRT.pivot = new Vector2(1f, 0.5f);
            float ty = Random.Range(40f, 200f);
            float tw = Random.Range(40f, 150f);
            float th = Random.Range(8f, 30f);
            trailRT.anchoredPosition = new Vector2(-10f, ty);
            trailRT.sizeDelta = new Vector2(tw, th);
            trailRT.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 10f));
        }

        // Mistcloak wipe panel — wide dark panel with ragged edge, starts off-screen
        var wipeObj = new GameObject("MistcloakWipePanel");
        wipeObj.transform.SetParent(canvasObj.transform, false);
        var wipeRT = wipeObj.AddComponent<RectTransform>();
        wipeRT.anchorMin = new Vector2(0.5f, 0f);
        wipeRT.anchorMax = new Vector2(0.5f, 1f);
        wipeRT.pivot = new Vector2(0.5f, 0.5f);
        wipeRT.sizeDelta = new Vector2(3840f, 0f); // 2x screen width to cover fully
        wipeRT.anchoredPosition = new Vector2(-3840f, 0f); // starts way off-screen left
        var wipeImg = wipeObj.AddComponent<Image>();
        wipeImg.color = new Color(0.02f, 0.02f, 0.03f, 1f); // near-black mistcloak
        wipeImg.raycastTarget = false;
        wipeObj.SetActive(false);

        // Tassels — ragged strips on the trailing edge of the wipe
        for (int t = 0; t < 12; t++)
        {
            var tassel = new GameObject($"Tassel_{t}");
            tassel.transform.SetParent(wipeObj.transform, false);
            var tasselImg = tassel.AddComponent<Image>();
            tasselImg.color = new Color(0.03f, 0.03f, 0.04f, Random.Range(0.7f, 1f));
            tasselImg.raycastTarget = false;
            var tasselRT = tassel.GetComponent<RectTransform>();
            tasselRT.anchorMin = new Vector2(1f, 0f);
            tasselRT.anchorMax = new Vector2(1f, 0f);
            tasselRT.pivot = new Vector2(0f, 0f);
            float ty = Random.Range(0f, 1080f);
            float tw = Random.Range(80f, 250f);
            float th = Random.Range(30f, 120f);
            tasselRT.anchoredPosition = new Vector2(0f, ty);
            tasselRT.sizeDelta = new Vector2(tw, th);
            tasselRT.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f));
        }

        // Vignette overlay (dark edges for cinematic feel)
        var vigObj = new GameObject("VignetteOverlay");
        vigObj.transform.SetParent(canvasObj.transform, false);
        var vigImg = vigObj.AddComponent<Image>();
        vigImg.color = new Color(0f, 0f, 0f, 0.4f);
        vigImg.raycastTarget = false;
        StretchFill(vigObj.GetComponent<RectTransform>());
        // The vignette works best with a radial gradient sprite — for now a subtle
        // semi-transparent black overlay softens the edges. Replace with a proper
        // vignette texture later.

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
        tsc.postTitleHold       = 8f; // Let title sit before mistcloak transition

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

        // Ambient audio (assign clips in Inspector — wind, dripping, bells, etc.)
        var ambAudio = manager.AddComponent<TitleAmbientAudio>();
        tsc.ambientAudio          = ambAudio;
        tsc.mistbornSilhouette    = mbRT;
        tsc.mistcloakWipePanel    = wipeRT;
        tsc.nextSceneName         = "MainMenu";

        tsc.creditLines = new List<TitleSequenceController.CreditLine>
        {
            new TitleSequenceController.CreditLine { time = 28f, text = "Music by Malakai Probert" },
            new TitleSequenceController.CreditLine { time = 35f, text = "Based on the novels by\nBrandon Sanderson" },
            new TitleSequenceController.CreditLine { time = 42f, text = "Produced by\nCrimson Blade Interactive" },
            new TitleSequenceController.CreditLine { time = 49f, text = "Creative Director\nLandon Adams" },
            new TitleSequenceController.CreditLine { time = 55f, text = "Crimson Blade Interactive\nproudly presents" },
        };

        manager.AddComponent<SceneBootstrap>();

        // ══════════════════════════════════════════════════════════════════
        // APPLY MATERIALS — save all .mat assets, then re-assign every
        // renderer from the saved asset so Unity serializes the reference.
        // ══════════════════════════════════════════════════════════════════
        // ══════════════════════════════════════════════════════════════════
        // FIX ALL LIGHTS — ensure every Light has HDAdditionalLightData
        // HDRP completely ignores Light.intensity without this component.
        // ══════════════════════════════════════════════════════════════════
        int lightFixCount = 0;
        foreach (var light in Object.FindObjectsOfType<Light>())
        {
            var hd = light.GetComponent<HDAdditionalLightData>();
            if (hd == null)
            {
                hd = light.gameObject.AddComponent<HDAdditionalLightData>();
                lightFixCount++;
            }
            // Transfer the value we set on Light.intensity to the HD component
            // then reset Light.intensity to 1 so they don't multiply
            float savedIntensity = light.intensity;
            light.intensity = 1f;
            hd.intensity = savedIntensity;
            hd.lightUnit = light.type == LightType.Directional ? LightUnit.Lux : LightUnit.Lumen;
            EditorUtility.SetDirty(light.gameObject);
        }
        Debug.Log($"[TitleSequenceBuilder] Fixed {lightFixCount} lights with HDAdditionalLightData, intensity transferred to HDRP");

        // ══════════════════════════════════════════════════════════════════
        // FIX PARTICLE RENDERERS — HDRP needs a specific particle material
        // Default particle material = pink on HDRP. Must use HDRP/Particles shader.
        // ══════════════════════════════════════════════════════════════════
        Material particleMat = null;

        // Step 1: Try to find HDRP particle shader and create a proper material
        string particleMatPath = "Assets/_Project/Materials/TitleSequence/HDRP_Particle.mat";
        particleMat = AssetDatabase.LoadAssetAtPath<Material>(particleMatPath);

        if (particleMat == null)
        {
            // Try HDRP particle shaders
            string[] particleShaderNames = {
                "HDRP/Particles/Lit",
                "HDRP/Particles/Unlit",
                "Particles/Standard Unlit",
                "Universal Render Pipeline/Particles/Unlit",
            };

            Shader particleShader = null;
            foreach (var sn in particleShaderNames)
            {
                particleShader = Shader.Find(sn);
                if (particleShader != null)
                {
                    Debug.Log($"[TitleSequenceBuilder] Found particle shader: {sn}");
                    break;
                }
            }

            if (particleShader != null)
            {
                particleMat = new Material(particleShader);
                particleMat.name = "HDRP_Particle";
                particleMat.color = Color.white;
                if (particleMat.HasProperty("_BaseColor"))
                    particleMat.SetColor("_BaseColor", Color.white);

                EnsureMatFolder("");
                AssetDatabase.CreateAsset(particleMat, particleMatPath);
                Debug.Log("[TitleSequenceBuilder] Created HDRP particle material");
            }
            else
            {
                // Last resort: search for ANY material with "Particle" in its shader name
                string[] allMatGuids = AssetDatabase.FindAssets("t:Material");
                foreach (var guid in allMatGuids)
                {
                    var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                    if (m != null && m.shader != null && m.shader.name.Contains("Particle"))
                    {
                        particleMat = m;
                        Debug.Log($"[TitleSequenceBuilder] Found existing particle material: {AssetDatabase.GUIDToAssetPath(guid)}");
                        break;
                    }
                }
            }
        }

        if (particleMat == null)
            Debug.LogWarning("[TitleSequenceBuilder] Could not find or create HDRP particle material! Particles will be pink.");

        // Temporarily activate ALL scene groups so we can find their particle renderers.
        // FindObjectsOfType only finds active objects — inactive groups get missed.
        var inactiveGroups = new List<GameObject>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    inactiveGroups.Add(child.gameObject);
                }
            }
        }

        // Apply to ALL particle system renderers (now including previously inactive ones)
        int particleFixCount = 0;
        foreach (var psr in Object.FindObjectsOfType<ParticleSystemRenderer>())
        {
            if (particleMat != null)
            {
                psr.sharedMaterial = particleMat;
                EditorUtility.SetDirty(psr);
                particleFixCount++;
            }
        }

        // Re-deactivate the groups that were inactive
        foreach (var go in inactiveGroups)
        {
            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }

        Debug.Log($"[TitleSequenceBuilder] Applied particle material to {particleFixCount} particle renderers (activated {inactiveGroups.Count} inactive objects to find them)");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int appliedCount = 0;
        foreach (var pair in _pendingAssignments)
        {
            if (pair.Item1 == null || pair.Item2 == null) continue;

            // Re-load the material from disk to get the persisted asset reference
            string assetPath = AssetDatabase.GetAssetPath(pair.Item2);
            if (!string.IsNullOrEmpty(assetPath))
            {
                Material saved = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (saved != null)
                {
                    pair.Item1.sharedMaterial = saved;
                    EditorUtility.SetDirty(pair.Item1);
                    EditorUtility.SetDirty(pair.Item1.gameObject);
                    appliedCount++;
                }
            }
        }
        Debug.Log($"[TitleSequenceBuilder] Re-applied {appliedCount} materials ({_namedMats.Count} unique) to renderers");

        // ══════════════════════════════════════════════════════════════════
        // SAVE SCENE
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
        body.AddComponent<TitleObjectSway>().swayType = TitleObjectSway.SwayType.Lantern;

        // Point light with flicker
        var lightObj = new GameObject("LanternLight");
        lightObj.transform.SetParent(parent);
        lightObj.transform.position = body.transform.position;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = COL_LANTERN;
        light.intensity = 8000f; // EV10 lumens
        light.range = 8f;
        var flicker = lightObj.AddComponent<TitleLightFlicker>();
        flicker.style = TitleLightFlicker.FlickerStyle.Lantern;
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
    // FIELD DETAIL BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static void CreateFencePost(Transform parent, Vector3 pos)
    {
        var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "FencePost";
        post.transform.SetParent(parent);
        post.transform.position = pos + new Vector3(0f, 0.5f, 0f);
        post.transform.localScale = new Vector3(0.06f, 0.5f, 0.06f);
        post.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), 0f, Random.Range(-8f, 8f));
        ApplyColor(post, COL_WOOD);
    }

    static void CreateFallenRail(Transform parent, Vector3 pos, float length)
    {
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rail.name = "FallenRail";
        rail.transform.SetParent(parent);
        rail.transform.position = pos;
        rail.transform.localScale = new Vector3(0.03f, length * 0.5f, 0.03f);
        rail.transform.rotation = Quaternion.Euler(85f, Random.Range(-20f, 20f), 0f);
        ApplyColor(rail, new Color(COL_WOOD.r * 0.8f, COL_WOOD.g * 0.8f, COL_WOOD.b * 0.8f));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // STREET DETAIL BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static void CreateAwning(Transform parent, Vector3 pos, bool leftSide)
    {
        var awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        awning.name = "Awning";
        awning.transform.SetParent(parent);
        awning.transform.position = pos;
        awning.transform.localScale = new Vector3(1.5f, 0.04f, 1.0f);
        float tilt = leftSide ? 8f : -8f;
        awning.transform.rotation = Quaternion.Euler(0f, 0f, tilt);

        Color[] awningColors = {
            new Color(0.35f, 0.15f, 0.10f), // faded red
            new Color(0.20f, 0.18f, 0.28f), // dusty purple
            new Color(0.30f, 0.25f, 0.15f), // tan canvas
        };
        ApplyColor(awning, awningColors[Random.Range(0, awningColors.Length)]);
    }

    static void CreateGutter(Transform parent)
    {
        // Shallow channel down center of street
        var gutter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gutter.name = "Gutter";
        gutter.transform.SetParent(parent);
        gutter.transform.position = new Vector3(0f, -0.03f, 5f);
        gutter.transform.localScale = new Vector3(0.3f, 0.06f, 40f);
        ApplyColor(gutter, new Color(0.08f, 0.07f, 0.06f)); // darker than street

        // Puddle (reflective-ish flat disc)
        var puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        puddle.name = "Puddle";
        puddle.transform.SetParent(parent);
        puddle.transform.position = new Vector3(0.1f, 0.005f, 2f);
        puddle.transform.localScale = new Vector3(0.6f, 0.005f, 0.4f);
        ApplyColor(puddle, new Color(0.08f, 0.10f, 0.14f)); // dark blue-ish reflection
    }

    static void CreateSkaaSilhouette(Transform parent, Vector3 pos, bool crouching)
    {
        var skaa = new GameObject("SkaaSilhouette");
        skaa.transform.SetParent(parent);
        skaa.transform.position = pos;

        Color skaaColor = new Color(0.06f, 0.05f, 0.05f); // near-black silhouette

        // Body
        float bodyHeight = crouching ? 0.6f : 1.2f;
        float bodyY = crouching ? 0.3f : 0.6f;
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(skaa.transform);
        body.transform.localPosition = new Vector3(0f, bodyY, 0f);
        body.transform.localScale = new Vector3(0.3f, bodyHeight * 0.5f, 0.2f);
        if (crouching) body.transform.rotation = Quaternion.Euler(30f, Random.Range(-20f, 20f), 0f);
        ApplyColor(body, skaaColor);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(skaa.transform);
        float headY = crouching ? 0.7f : 1.35f;
        head.transform.localPosition = new Vector3(0f, headY, crouching ? 0.15f : 0f);
        head.transform.localScale = new Vector3(0.18f, 0.2f, 0.18f);
        ApplyColor(head, skaaColor);

        // Cloak/shawl (flattened cube draped)
        if (!crouching)
        {
            var cloak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloak.name = "Cloak";
            cloak.transform.SetParent(skaa.transform);
            cloak.transform.localPosition = new Vector3(0f, 0.7f, -0.05f);
            cloak.transform.localScale = new Vector3(0.45f, 0.8f, 0.08f);
            ApplyColor(cloak, new Color(0.10f, 0.08f, 0.07f)); // slightly lighter than body
        }
    }

    static void CreateHangingSign(Transform parent, Vector3 pos, bool leftSide)
    {
        // Metal bracket
        var bracket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bracket.name = "SignBracket";
        bracket.transform.SetParent(parent);
        bracket.transform.position = pos;
        bracket.transform.localScale = new Vector3(0.03f, 0.4f, 0.03f);
        bracket.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(bracket, COL_METAL);

        // Sign board
        float signX = leftSide ? pos.x + 0.5f : pos.x - 0.5f;
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "SignBoard";
        sign.transform.SetParent(parent);
        sign.transform.position = new Vector3(signX, pos.y - 0.3f, pos.z);
        sign.transform.localScale = new Vector3(0.6f, 0.4f, 0.04f);
        sign.transform.rotation = Quaternion.Euler(0f, Random.Range(-5f, 5f), Random.Range(-3f, 3f));
        ApplyColor(sign, new Color(0.22f, 0.15f, 0.08f)); // weathered wood
        sign.AddComponent<TitleObjectSway>().swayType = TitleObjectSway.SwayType.HangingSign;
    }

    static void CreateTerrismanSilhouette(Transform parent, Vector3 pos)
    {
        var terris = new GameObject("TerrismanSilhouette");
        terris.transform.SetParent(parent);
        terris.transform.position = pos;

        Color col = new Color(0.06f, 0.05f, 0.05f);

        // Tall body (Terrisman are described as tall)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(terris.transform);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.3f, 0.9f, 0.22f);
        ApplyColor(body, col);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(terris.transform);
        head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
        head.transform.localScale = new Vector3(0.18f, 0.2f, 0.18f);
        ApplyColor(head, col);

        // Distinctive V-shaped robes (wider at the bottom — Terris style)
        var robeBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        robeBottom.name = "RobeSkirt";
        robeBottom.transform.SetParent(terris.transform);
        robeBottom.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        robeBottom.transform.localScale = new Vector3(0.55f, 0.5f, 0.35f);
        ApplyColor(robeBottom, new Color(0.10f, 0.08f, 0.07f));

        // Terris earring (small metallic sphere — stores Feruchemical charge)
        var earring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        earring.name = "Earring";
        earring.transform.SetParent(terris.transform);
        earring.transform.localPosition = new Vector3(0.1f, 1.82f, 0f);
        earring.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        ApplyColor(earring, COL_METAL);

        // Arm bracelets (metalminds — Feruchemical storage)
        for (int arm = -1; arm <= 1; arm += 2)
        {
            var bracelet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bracelet.name = "Metalmind";
            bracelet.transform.SetParent(terris.transform);
            bracelet.transform.localPosition = new Vector3(arm * 0.2f, 1.1f, 0f);
            bracelet.transform.localScale = new Vector3(0.08f, 0.015f, 0.08f);
            ApplyColor(bracelet, COL_METAL);
        }
    }

    static void CreateRainSplash(Transform parent, Vector3 pos)
    {
        var obj = new GameObject("RainSplash");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
        main.startColor = new Color(0.5f, 0.55f, 0.6f, 0.3f);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;
        var em = ps.emission;
        em.rateOverTime = 12f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        // Splash upward
        shape.rotation = new Vector3(-90f, 0f, 0f);
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.3f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    static void CreateLightShaft(Transform parent, Vector3 pos, float direction)
    {
        // Cone of light visible in the mist/dust (approximated with a stretched transparent cube)
        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "LightShaft";
        shaft.transform.SetParent(parent);
        shaft.transform.position = pos + new Vector3(direction * 0.8f, -1.5f, 0f);
        shaft.transform.localScale = new Vector3(1.2f, 3f, 0.8f);
        shaft.transform.rotation = Quaternion.Euler(0f, 0f, direction > 0 ? -15f : 15f);
        // Very faint warm color — the light cone itself
        ApplyColor(shaft, new Color(0.9f, 0.6f, 0.2f, 0.03f));

        // Dust motes in the light beam
        var dustObj = new GameObject("LightDust");
        dustObj.transform.SetParent(parent);
        dustObj.transform.position = pos + new Vector3(direction * 0.5f, -1f, 0f);
        var ps = dustObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.025f);
        main.startColor = new Color(0.9f, 0.7f, 0.4f, 0.4f);
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = 5f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.8f, 2f, 0.5f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.05f;
        noise.frequency = 0.3f;
        noise.octaveCount = 1;
    }

    static void CreateSafehouseHint(Transform parent, Vector3 pos)
    {
        // A cellar door slightly ajar at the base of a building
        var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "CellarDoor";
        door.transform.SetParent(parent);
        door.transform.position = pos + new Vector3(0f, 0.2f, -0.3f);
        door.transform.localScale = new Vector3(0.6f, 0.04f, 0.5f);
        door.transform.rotation = Quaternion.Euler(-20f, 0f, 0f); // slightly open
        ApplyColor(door, new Color(COL_WOOD.r * 0.7f, COL_WOOD.g * 0.7f, COL_WOOD.b * 0.7f));

        // Faint blue glow from inside (someone is burning tin)
        var glow = new GameObject("TinGlow");
        glow.transform.SetParent(parent);
        glow.transform.position = pos + new Vector3(0f, -0.1f, -0.3f);
        var gl = glow.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = new Color(0.3f, 0.5f, 1f); // Allomantic blue
        gl.intensity = 0.4f;
        gl.range = 2f;
        glow.AddComponent<TitleLightFlicker>().style = TitleLightFlicker.FlickerStyle.WindowGlow;
    }

    static void CreateAlleyFire(Transform parent, Vector3 pos)
    {
        // Small campfire (emissive sphere + light)
        var fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fire.name = "AlleyFire";
        fire.transform.SetParent(parent);
        fire.transform.position = pos + new Vector3(0f, 0.1f, 0f);
        fire.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
        ApplyEmissive(fire, new Color(1f, 0.4f, 0.05f));

        var fireLight = new GameObject("FireLight");
        fireLight.transform.SetParent(parent);
        fireLight.transform.position = pos + new Vector3(0f, 0.2f, 0f);
        var fl = fireLight.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.color = new Color(1f, 0.5f, 0.1f);
        fl.intensity = 1.5f;
        fl.range = 4f;
        fireLight.AddComponent<TitleLightFlicker>().style = TitleLightFlicker.FlickerStyle.Torch;

        // 2 huddled skaa around it
        CreateSkaaSilhouette(parent, pos + new Vector3(-0.3f, 0f, 0.2f), true);
        CreateSkaaSilhouette(parent, pos + new Vector3(0.3f, 0f, 0.15f), true);
    }

    static void CreateBrokenStreet(Transform parent, Vector3 pos)
    {
        // Missing cobblestones — a dark hole with scattered stones around it
        var hole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hole.name = "BrokenStreetHole";
        hole.transform.SetParent(parent);
        hole.transform.position = pos + new Vector3(0f, -0.03f, 0f);
        hole.transform.localScale = new Vector3(0.8f, 0.04f, 0.6f);
        ApplyColor(hole, new Color(0.04f, 0.04f, 0.03f)); // dark earth underneath

        // Loose cobblestones around the edge
        for (int i = 0; i < 5; i++)
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "LooseCobble";
            stone.transform.SetParent(parent);
            float s = Random.Range(0.06f, 0.12f);
            stone.transform.position = pos + new Vector3(Random.Range(-0.5f, 0.5f), s * 0.5f, Random.Range(-0.4f, 0.4f));
            stone.transform.localScale = new Vector3(s, s * 0.6f, s);
            stone.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 90f), Random.Range(-10f, 10f));
            ApplyColor(stone, Color.Lerp(COL_COBBLE, COL_STONE_DARK, Random.Range(0f, 1f)));
        }
    }

    static void CreateRat(Transform parent, Vector3 pos)
    {
        var rat = new GameObject("Rat");
        rat.transform.SetParent(parent);
        rat.transform.position = pos;

        Color col = new Color(0.12f, 0.10f, 0.08f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(rat.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.025f, 0.015f, 0.04f);
        body.transform.rotation = Quaternion.Euler(0f, Random.Range(-30f, 30f), 90f);
        ApplyColor(body, col);

        // Tail (thin cylinder)
        var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tail.name = "Tail";
        tail.transform.SetParent(rat.transform);
        tail.transform.localPosition = new Vector3(-0.04f, 0.005f, 0f);
        tail.transform.localScale = new Vector3(0.004f, 0.03f, 0.004f);
        tail.transform.rotation = Quaternion.Euler(0f, 0f, 70f);
        ApplyColor(tail, new Color(0.15f, 0.12f, 0.10f));
    }

    static void CreateWindDebris(Transform parent, Vector3 pos)
    {
        var obj = new GameObject("WindDebris");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.25f, 0.22f, 0.18f, 0.5f),
            new Color(0.35f, 0.30f, 0.25f, 0.7f)
        );
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.05f;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        var em = ps.emission;
        em.rateOverTime = 3f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(8f, 0.5f, 20f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.4f;
        noise.scrollSpeed = 0.3f;
        noise.octaveCount = 2;
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
    }

    static void CreateBarracks(Transform parent, Vector3 pos)
    {
        // Long rectangular building — military garrison
        var main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "Barracks";
        main.transform.SetParent(parent);
        main.transform.position = pos + new Vector3(0f, 3f, 0f);
        main.transform.localScale = new Vector3(15f, 6f, 6f);
        ApplyColor(main, COL_STONE_GREY);

        // Roof
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "BarracksRoof";
        roof.transform.SetParent(main.transform, false);
        roof.transform.localPosition = new Vector3(0f, 0.53f, 0f);
        roof.transform.localScale = new Vector3(1.05f, 0.05f, 1.1f);
        ApplyColor(roof, COL_ROOF_SLATE);

        // Yard fence
        for (int i = -3; i <= 3; i++)
        {
            var fencePost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fencePost.name = "YardPost";
            fencePost.transform.SetParent(parent);
            fencePost.transform.position = pos + new Vector3(i * 2.5f, 1f, 5f);
            fencePost.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            ApplyColor(fencePost, COL_WOOD);
        }

        // Training dummy in the yard
        var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        dummy.name = "TrainingDummy";
        dummy.transform.SetParent(parent);
        dummy.transform.position = pos + new Vector3(3f, 1f, 7f);
        dummy.transform.localScale = new Vector3(0.3f, 0.8f, 0.2f);
        ApplyColor(dummy, COL_WOOD);
    }

    static void CreateMarketSquare(Transform parent, Vector3 pos)
    {
        // Open square — lighter ground
        var square = GameObject.CreatePrimitive(PrimitiveType.Cube);
        square.name = "MarketSquare";
        square.transform.SetParent(parent);
        square.transform.position = pos + new Vector3(0f, 0.15f, 0f);
        square.transform.localScale = new Vector3(12f, 0.1f, 12f);
        ApplyColor(square, COL_COBBLE);

        // Fountain in the center (dried up — water is scarce)
        var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fountain.name = "Fountain";
        fountain.transform.SetParent(parent);
        fountain.transform.position = pos + new Vector3(0f, 0.6f, 0f);
        fountain.transform.localScale = new Vector3(2f, 0.6f, 2f);
        ApplyColor(fountain, COL_STONE_LIGHT);

        // Scattered stalls around the edges
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 stallPos = pos + new Vector3(Mathf.Cos(angle) * 4.5f, 0f, Mathf.Sin(angle) * 4.5f);
            var stall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stall.name = "MarketStall";
            stall.transform.SetParent(parent);
            stall.transform.position = stallPos + new Vector3(0f, 0.8f, 0f);
            stall.transform.localScale = new Vector3(2f, 1.6f, 1.5f);
            stall.transform.rotation = Quaternion.Euler(0f, i * 60f, 0f);
            Color[] stallColors = { COL_WOOD, COL_STONE_RED, COL_STONE_MED };
            ApplyColor(stall, stallColors[i % stallColors.Length]);
        }
    }

    static void CreateSkaaQuarter(Transform parent, Vector3 pos)
    {
        // Dense cluster of small, short buildings — no lights, oppressive
        for (int i = 0; i < 15; i++)
        {
            float x = pos.x + Random.Range(-8f, 8f);
            float z = pos.z + Random.Range(-8f, 8f);
            float h = Random.Range(2f, 5f); // shorter than noble buildings
            float w = Random.Range(2f, 4f);
            float d = Random.Range(2f, 4f);

            var hovel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hovel.name = "SkaaHovel";
            hovel.transform.SetParent(parent);
            hovel.transform.position = new Vector3(x, h * 0.5f, z);
            hovel.transform.localScale = new Vector3(w, h, d);
            hovel.transform.rotation = Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f);
            // Darker, dirtier colors — no maintenance, no wealth
            ApplyColor(hovel, new Color(
                Random.Range(0.10f, 0.16f),
                Random.Range(0.08f, 0.14f),
                Random.Range(0.07f, 0.12f)));
        }
    }

    static void CreateBuriedWheel(Transform parent, Vector3 pos)
    {
        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "BuriedWheel";
        wheel.transform.SetParent(parent);
        wheel.transform.position = pos + new Vector3(0f, 0.15f, 0f);
        wheel.transform.localScale = new Vector3(0.7f, 0.04f, 0.7f);
        wheel.transform.rotation = Quaternion.Euler(65f, Random.Range(0f, 90f), 0f);
        ApplyColor(wheel, new Color(COL_WOOD.r * 0.6f, COL_WOOD.g * 0.6f, COL_WOOD.b * 0.6f));
    }

    static void CreateMileMarker(Transform parent, Vector3 pos)
    {
        // Iron post marking distance to Luthadel
        var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = "MileMarker";
        post.transform.SetParent(parent);
        post.transform.position = pos + new Vector3(0f, 0.5f, 0f);
        post.transform.localScale = new Vector3(0.1f, 1f, 0.08f);
        post.transform.rotation = Quaternion.Euler(0f, Random.Range(-10f, 10f), Random.Range(-3f, 3f));
        ApplyColor(post, COL_METAL);

        // Sign plate
        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "MarkerPlate";
        plate.transform.SetParent(post.transform, false);
        plate.transform.localPosition = new Vector3(0f, 0.35f, 0.5f);
        plate.transform.localScale = new Vector3(3f, 2f, 0.2f);
        ApplyColor(plate, new Color(COL_METAL.r - 0.05f, COL_METAL.g - 0.05f, COL_METAL.b - 0.03f));
    }

    static void CreateBird(Transform parent, Vector3 pos)
    {
        var bird = new GameObject("Bird");
        bird.transform.SetParent(parent);
        bird.transform.position = pos;

        Color col = new Color(0.05f, 0.05f, 0.06f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(bird.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.08f, 0.06f, 0.1f);
        ApplyColor(body, col);

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(bird.transform);
        head.transform.localPosition = new Vector3(0f, 0.035f, 0.05f);
        head.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        ApplyColor(head, col);

        // Beak
        var beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beak.name = "Beak";
        beak.transform.SetParent(bird.transform);
        beak.transform.localPosition = new Vector3(0f, 0.03f, 0.08f);
        beak.transform.localScale = new Vector3(0.01f, 0.008f, 0.025f);
        ApplyColor(beak, new Color(0.20f, 0.15f, 0.05f));
    }

    static void CreateIronGate(Transform parent, Vector3 pos)
    {
        // Two stone pillars with iron gate between them
        for (int side = -1; side <= 1; side += 2)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "GatePillar";
            pillar.transform.SetParent(parent);
            pillar.transform.position = pos + new Vector3(side * 2f, 2f, 0f);
            pillar.transform.localScale = new Vector3(0.8f, 4f, 0.8f);
            ApplyColor(pillar, COL_STONE_GREY);
        }

        // Iron bars
        for (int i = -3; i <= 3; i++)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.name = "GateBar";
            bar.transform.SetParent(parent);
            bar.transform.position = pos + new Vector3(i * 0.4f, 2f, 0f);
            bar.transform.localScale = new Vector3(0.04f, 2f, 0.04f);
            ApplyColor(bar, COL_METAL);
        }

        // Horizontal crossbar
        var crossbar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crossbar.name = "GateCrossbar";
        crossbar.transform.SetParent(parent);
        crossbar.transform.position = pos + new Vector3(0f, 3f, 0f);
        crossbar.transform.localScale = new Vector3(0.05f, 2.2f, 0.05f);
        crossbar.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(crossbar, COL_METAL);

        // Arch over the gate
        var arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arch.name = "GateArch";
        arch.transform.SetParent(parent);
        arch.transform.position = pos + new Vector3(0f, 4.2f, 0f);
        arch.transform.localScale = new Vector3(5f, 0.6f, 0.8f);
        ApplyColor(arch, COL_STONE_MED);
    }

    static void CreateParkedWagon(Transform parent, Vector3 pos)
    {
        // Flat bed wagon with wheels
        var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bed.name = "WagonBed";
        bed.transform.SetParent(parent);
        bed.transform.position = pos + new Vector3(0f, 0.5f, 0f);
        bed.transform.localScale = new Vector3(1.0f, 0.08f, 2.0f);
        ApplyColor(bed, COL_WOOD);

        // Side walls
        for (int side = -1; side <= 1; side += 2)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "WagonSide";
            wall.transform.SetParent(parent);
            wall.transform.position = pos + new Vector3(side * 0.5f, 0.7f, 0f);
            wall.transform.localScale = new Vector3(0.04f, 0.4f, 2.0f);
            ApplyColor(wall, new Color(COL_WOOD.r * 0.85f, COL_WOOD.g * 0.85f, COL_WOOD.b * 0.85f));
        }

        // Wheels
        for (int wz = -1; wz <= 1; wz += 2)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "WagonWheel";
            wheel.transform.SetParent(parent);
            wheel.transform.position = pos + new Vector3(0.55f, 0.3f, wz * 0.7f);
            wheel.transform.localScale = new Vector3(0.5f, 0.04f, 0.5f);
            wheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            ApplyColor(wheel, COL_WOOD);
        }

        // Some cargo on the wagon
        CreateCrate(parent, pos + new Vector3(0f, 0.55f, 0.3f));
        CreateBarrel(parent, pos + new Vector3(-0.2f, 0.55f, -0.5f));
    }

    static void CreateSkaaShanty(Transform parent, Vector3 pos)
    {
        // Lean-to shelter — pole + slanted board + tarp
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "ShantyPole";
        pole.transform.SetParent(parent);
        pole.transform.position = pos + new Vector3(0f, 0.8f, 0f);
        pole.transform.localScale = new Vector3(0.05f, 0.8f, 0.05f);
        ApplyColor(pole, COL_WOOD);

        var pole2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole2.name = "ShantyPole2";
        pole2.transform.SetParent(parent);
        pole2.transform.position = pos + new Vector3(1.2f, 0.4f, 0f);
        pole2.transform.localScale = new Vector3(0.05f, 0.4f, 0.05f);
        ApplyColor(pole2, COL_WOOD);

        // Slanted roof panel
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "ShantyRoof";
        roof.transform.SetParent(parent);
        roof.transform.position = pos + new Vector3(0.6f, 0.9f, 0f);
        roof.transform.localScale = new Vector3(1.5f, 0.03f, 1.2f);
        roof.transform.rotation = Quaternion.Euler(0f, Random.Range(-10f, 10f), -20f);

        Color[] roofColors = {
            new Color(0.22f, 0.18f, 0.12f),
            new Color(0.18f, 0.15f, 0.18f),
            new Color(0.20f, 0.16f, 0.10f),
        };
        ApplyColor(roof, roofColors[Random.Range(0, roofColors.Length)]);

        // Blanket / sleeping roll underneath
        var blanket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blanket.name = "Blanket";
        blanket.transform.SetParent(parent);
        blanket.transform.position = pos + new Vector3(0.4f, 0.05f, 0f);
        blanket.transform.localScale = new Vector3(0.8f, 0.06f, 0.5f);
        ApplyColor(blanket, new Color(0.22f, 0.18f, 0.15f));
    }

    static void CreateStrayAnimal(Transform parent, Vector3 pos)
    {
        var animal = new GameObject("StrayAnimal");
        animal.transform.SetParent(parent);
        animal.transform.position = pos;

        Color col = new Color(0.12f, 0.10f, 0.08f);

        // Body (elongated capsule)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(animal.transform);
        body.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        body.transform.localScale = new Vector3(0.12f, 0.12f, 0.2f);
        body.transform.rotation = Quaternion.Euler(0f, Random.Range(-30f, 30f), 90f);
        ApplyColor(body, col);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(animal.transform);
        head.transform.localPosition = new Vector3(0.2f, 0.22f, 0f);
        head.transform.localScale = new Vector3(0.1f, 0.08f, 0.08f);
        ApplyColor(head, col);
    }

    static void CreateObligatorSilhouette(Transform parent, Vector3 pos)
    {
        var obligator = new GameObject("ObligatorSilhouette");
        obligator.transform.SetParent(parent);
        obligator.transform.position = pos;

        Color col = new Color(0.05f, 0.04f, 0.04f);

        // Tall robed body (wider at bottom — robes)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(obligator.transform);
        body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        body.transform.localScale = new Vector3(0.35f, 0.8f, 0.25f);
        ApplyColor(body, col);

        // Head (bald — obligators shave their heads)
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(obligator.transform);
        head.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        head.transform.localScale = new Vector3(0.18f, 0.2f, 0.18f);
        ApplyColor(head, new Color(0.08f, 0.07f, 0.06f)); // slightly lighter (skin visible)

        // Robe hem (wider cube at feet)
        var hem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hem.name = "RobeHem";
        hem.transform.SetParent(obligator.transform);
        hem.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        hem.transform.localScale = new Vector3(0.5f, 0.3f, 0.35f);
        ApplyColor(hem, col);

        // Tattoo lines (thin bright strips on the face — the obligator markings)
        for (int t = 0; t < 3; t++)
        {
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "TattooLine";
            line.transform.SetParent(obligator.transform);
            line.transform.localPosition = new Vector3(0.09f, 1.63f + t * 0.04f, -0.02f + t * 0.02f);
            line.transform.localScale = new Vector3(0.005f, 0.008f, 0.05f);
            ApplyColor(line, new Color(0.15f, 0.08f, 0.08f)); // red-brown ink
        }
    }

    static void CreateAlleyEntrance(Transform parent, Vector3 pos, bool leftSide)
    {
        // Dark recessed gap between buildings
        float dir = leftSide ? -1f : 1f;
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "AlleyFloor";
        floor.transform.SetParent(parent);
        floor.transform.position = pos + new Vector3(dir * 1.5f, 0f, 0f);
        floor.transform.localScale = new Vector3(2f, 0.02f, 2.5f);
        ApplyColor(floor, new Color(0.05f, 0.04f, 0.04f)); // very dark — can't see in

        // Shadow / darkness indicator (thin dark cube at the entrance)
        var shadow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shadow.name = "AlleyShadow";
        shadow.transform.SetParent(parent);
        shadow.transform.position = pos + new Vector3(dir * 0.3f, 2f, 0f);
        shadow.transform.localScale = new Vector3(0.6f, 4f, 2.5f);
        ApplyColor(shadow, new Color(0.03f, 0.03f, 0.03f));
    }

    static void CreateInquisitorSilhouette(Transform parent, Vector3 pos)
    {
        // The most terrifying figure in the Final Empire — standing atop Kredik Shaw
        var inq = new GameObject("InquisitorSilhouette");
        inq.transform.SetParent(parent);
        inq.transform.position = pos;

        Color col = new Color(0.03f, 0.03f, 0.04f);

        // Tall, gaunt body
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(inq.transform);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.3f, 0.9f, 0.2f);
        ApplyColor(body, col);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(inq.transform);
        head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
        head.transform.localScale = new Vector3(0.2f, 0.22f, 0.2f);
        ApplyColor(head, col);

        // Spike eyes — two small emissive cylinders where the eyes should be
        for (int side = -1; side <= 1; side += 2)
        {
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spike.name = "EyeSpike";
            spike.transform.SetParent(inq.transform);
            spike.transform.localPosition = new Vector3(side * 0.04f, 1.87f, 0.09f);
            spike.transform.localScale = new Vector3(0.015f, 0.12f, 0.015f);
            spike.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ApplyColor(spike, COL_METAL);
        }

        // Long robes / coat trailing
        var coat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        coat.name = "Coat";
        coat.transform.SetParent(inq.transform);
        coat.transform.localPosition = new Vector3(0f, 0.5f, -0.15f);
        coat.transform.localScale = new Vector3(0.4f, 1.2f, 0.08f);
        coat.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
        ApplyColor(coat, new Color(0.04f, 0.04f, 0.05f));

        // Axe in hand
        var axeHandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axeHandle.name = "AxeHandle";
        axeHandle.transform.SetParent(inq.transform);
        axeHandle.transform.localPosition = new Vector3(0.22f, 0.9f, 0.05f);
        axeHandle.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
        axeHandle.transform.rotation = Quaternion.Euler(0f, 0f, 15f);
        ApplyColor(axeHandle, COL_WOOD);

        var axeHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
        axeHead.name = "AxeHead";
        axeHead.transform.SetParent(axeHandle.transform, false);
        axeHead.transform.localPosition = new Vector3(0.3f, 0.9f, 0f);
        axeHead.transform.localScale = new Vector3(8f, 3f, 1f);
        ApplyColor(axeHead, COL_METAL);
    }

    static void CreateCanalBridge(Transform parent, Vector3 pos, float angleDeg)
    {
        var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "CanalBridge";
        bridge.transform.SetParent(parent);
        bridge.transform.position = pos + new Vector3(0f, 0.8f, 0f);
        bridge.transform.localScale = new Vector3(5f, 0.4f, 3.5f);
        bridge.transform.rotation = Quaternion.Euler(0f, angleDeg, 0f);
        ApplyColor(bridge, COL_STONE_MED);

        // Bridge railings
        for (int side = -1; side <= 1; side += 2)
        {
            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "BridgeRail";
            rail.transform.SetParent(bridge.transform, false);
            rail.transform.localPosition = new Vector3(0f, 1.2f, side * 0.45f);
            rail.transform.localScale = new Vector3(0.95f, 0.5f, 0.04f);
            ApplyColor(rail, COL_METAL);
        }
    }

    static void CreateDock(Transform parent, Vector3 pos)
    {
        // Wooden platform extending over canal
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "DockPlatform";
        platform.transform.SetParent(parent);
        platform.transform.position = pos + new Vector3(0f, 0.3f, 0f);
        platform.transform.localScale = new Vector3(6f, 0.15f, 3f);
        ApplyColor(platform, COL_WOOD);

        // Support posts
        for (int i = 0; i < 4; i++)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "DockPost";
            post.transform.SetParent(parent);
            float px = pos.x + (i % 2 == 0 ? -2.5f : 2.5f);
            float pz = pos.z + (i < 2 ? -1f : 1f);
            post.transform.position = new Vector3(px, 0.4f, pz);
            post.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
            ApplyColor(post, new Color(COL_WOOD.r * 0.8f, COL_WOOD.g * 0.8f, COL_WOOD.b * 0.8f));
        }

        // Crates on the dock
        CreateCrate(parent, pos + new Vector3(-1f, 0.38f, 0.5f));
        CreateCrate(parent, pos + new Vector3(-0.5f, 0.38f, 0.2f));
        CreateBarrel(parent, pos + new Vector3(1.5f, 0.38f, -0.3f));
    }

    static void CreateRuin(Transform parent, Vector3 pos)
    {
        // Collapsed wall sections at different angles
        var wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall1.name = "RuinWall";
        wall1.transform.SetParent(parent);
        wall1.transform.position = pos + new Vector3(0f, 0.8f, 0f);
        wall1.transform.localScale = new Vector3(3f, 1.6f, 0.3f);
        wall1.transform.rotation = Quaternion.Euler(0f, 20f, 5f);
        ApplyColor(wall1, COL_STONE_GREY);

        var wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall2.name = "RuinWallFallen";
        wall2.transform.SetParent(parent);
        wall2.transform.position = pos + new Vector3(1.5f, 0.2f, 0.5f);
        wall2.transform.localScale = new Vector3(2f, 0.3f, 1.5f);
        wall2.transform.rotation = Quaternion.Euler(0f, -10f, 0f);
        ApplyColor(wall2, COL_STONE_MED);

        // Rubble (scattered small cubes)
        for (int i = 0; i < 6; i++)
        {
            var rubble = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rubble.name = "Rubble";
            rubble.transform.SetParent(parent);
            float s = Random.Range(0.1f, 0.3f);
            rubble.transform.position = pos + new Vector3(Random.Range(-2f, 2f), s * 0.5f, Random.Range(-1f, 2f));
            rubble.transform.localScale = new Vector3(s, s, s * Random.Range(0.5f, 1.5f));
            rubble.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 90f), Random.Range(-15f, 15f));
            ApplyColor(rubble, Color.Lerp(COL_STONE_GREY, COL_STONE_DARK, Random.Range(0f, 1f)));
        }
    }

    static void CreateScatteredCoins(Transform parent, Vector3 pos)
    {
        // Small metal discs — hints that a Mistborn was here
        for (int i = 0; i < 5; i++)
        {
            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.SetParent(parent);
            coin.transform.position = pos + new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            coin.transform.localScale = new Vector3(0.04f, 0.003f, 0.04f);
            // Copper clips — slightly shiny
            ApplyColor(coin, new Color(0.45f, 0.30f, 0.15f)); // copper color
        }
    }

    static void CreateSewerGrate(Transform parent, Vector3 pos)
    {
        var grate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grate.name = "SewerGrate";
        grate.transform.SetParent(parent);
        grate.transform.position = pos;
        grate.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);
        ApplyColor(grate, COL_METAL);

        // Bars across the grate
        for (int i = 0; i < 4; i++)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "GrateBar";
            bar.transform.SetParent(grate.transform, false);
            bar.transform.localPosition = new Vector3(0f, 0.5f, -0.35f + i * 0.23f);
            bar.transform.localScale = new Vector3(0.95f, 0.5f, 0.06f);
            ApplyColor(bar, new Color(0.22f, 0.22f, 0.25f));
        }
    }

    static void CreateMarketStall(Transform parent, Vector3 pos, bool leftSide)
    {
        // Table
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "StallTable";
        table.transform.SetParent(parent);
        table.transform.position = pos + new Vector3(0f, 0.5f, 0f);
        table.transform.localScale = new Vector3(1.5f, 0.06f, 0.8f);
        ApplyColor(table, COL_WOOD);

        // Legs
        for (int lx = -1; lx <= 1; lx += 2)
        {
            for (int lz = -1; lz <= 1; lz += 2)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "StallLeg";
                leg.transform.SetParent(parent);
                leg.transform.position = pos + new Vector3(lx * 0.6f, 0.25f, lz * 0.3f);
                leg.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);
                ApplyColor(leg, COL_WOOD);
            }
        }

        // Tarp draped over (closed for the night)
        var tarp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tarp.name = "StallTarp";
        tarp.transform.SetParent(parent);
        tarp.transform.position = pos + new Vector3(0f, 0.6f, 0f);
        tarp.transform.localScale = new Vector3(1.7f, 0.02f, 1.0f);
        tarp.transform.rotation = Quaternion.Euler(0f, 0f, leftSide ? 5f : -5f);
        Color[] tarpColors = {
            new Color(0.28f, 0.20f, 0.12f),
            new Color(0.18f, 0.16f, 0.22f),
            new Color(0.25f, 0.22f, 0.18f),
        };
        ApplyColor(tarp, tarpColors[Random.Range(0, tarpColors.Length)]);
    }

    static void CreateNobleCarriage(Transform parent, Vector3 pos)
    {
        // Carriage body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "CarriageBody";
        body.transform.SetParent(parent);
        body.transform.position = pos + new Vector3(0f, 1f, 0f);
        body.transform.localScale = new Vector3(1.5f, 1.2f, 2.5f);
        ApplyColor(body, new Color(0.15f, 0.08f, 0.05f)); // dark polished wood

        // Roof
        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "CarriageRoof";
        roof.transform.SetParent(body.transform, false);
        roof.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        roof.transform.localScale = new Vector3(1.1f, 0.08f, 1.05f);
        ApplyColor(roof, new Color(0.10f, 0.06f, 0.04f));

        // Wheels (4)
        for (int wx = -1; wx <= 1; wx += 2)
        {
            for (int wz = -1; wz <= 1; wz += 2)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = "CarriageWheel";
                wheel.transform.SetParent(parent);
                float wSize = wz > 0 ? 0.45f : 0.35f; // back wheels bigger
                wheel.transform.position = pos + new Vector3(wx * 0.8f, wSize, wz * 0.9f);
                wheel.transform.localScale = new Vector3(wSize * 2f, 0.06f, wSize * 2f);
                wheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                ApplyColor(wheel, COL_WOOD);
            }
        }

        // Gold trim (thin strip along the middle — noble wealth)
        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "CarriageTrim";
        trim.transform.SetParent(body.transform, false);
        trim.transform.localPosition = new Vector3(0.51f, 0f, 0f);
        trim.transform.localScale = new Vector3(0.02f, 0.15f, 0.9f);
        ApplyColor(trim, new Color(0.50f, 0.40f, 0.15f)); // gold

        var trim2 = Object.Instantiate(trim, body.transform);
        trim2.transform.localPosition = new Vector3(-0.51f, 0f, 0f);
    }

    static void CreateMetalDebris(Transform parent, Vector3 pos)
    {
        // Nails, scraps, bent metal — important in a world where metal = power
        for (int i = 0; i < 4; i++)
        {
            PrimitiveType type = Random.Range(0, 2) == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube;
            var piece = GameObject.CreatePrimitive(type);
            piece.name = "MetalScrap";
            piece.transform.SetParent(parent);
            float s = Random.Range(0.02f, 0.08f);
            piece.transform.position = pos + new Vector3(Random.Range(-0.3f, 0.3f), s * 0.5f, Random.Range(-0.3f, 0.3f));
            piece.transform.localScale = type == PrimitiveType.Cylinder
                ? new Vector3(s * 0.3f, s * 2f, s * 0.3f)
                : new Vector3(s, s * 0.3f, s * 0.5f);
            piece.transform.rotation = Quaternion.Euler(Random.Range(-45f, 45f), Random.Range(0f, 180f), Random.Range(-30f, 30f));
            ApplyColor(piece, new Color(
                COL_METAL.r + Random.Range(-0.05f, 0.05f),
                COL_METAL.g + Random.Range(-0.05f, 0.05f),
                COL_METAL.b + Random.Range(-0.03f, 0.05f)));
        }
    }

    static void CreateRuinedCart(Transform parent, Vector3 pos)
    {
        // Cart bed (flat box, tilted — one wheel broken)
        var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bed.name = "CartBed";
        bed.transform.SetParent(parent);
        bed.transform.position = pos + new Vector3(0f, 0.3f, 0f);
        bed.transform.localScale = new Vector3(1.2f, 0.1f, 2f);
        bed.transform.rotation = Quaternion.Euler(0f, 15f, 8f); // tilted
        ApplyColor(bed, COL_WOOD);

        // Remaining wheel
        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "CartWheel";
        wheel.transform.SetParent(parent);
        wheel.transform.position = pos + new Vector3(-0.6f, 0.35f, 0.5f);
        wheel.transform.localScale = new Vector3(0.6f, 0.04f, 0.6f);
        wheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(wheel, new Color(COL_WOOD.r * 0.7f, COL_WOOD.g * 0.7f, COL_WOOD.b * 0.7f));

        // Broken axle
        var axle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axle.name = "CartAxle";
        axle.transform.SetParent(parent);
        axle.transform.position = pos + new Vector3(0f, 0.15f, 0.5f);
        axle.transform.localScale = new Vector3(0.05f, 0.7f, 0.05f);
        axle.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(axle, COL_METAL);
    }

    static void CreateDeadShrub(Transform parent, Vector3 pos)
    {
        // Cluster of thin sticks poking up
        for (int i = 0; i < 4; i++)
        {
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick.name = "Twig";
            stick.transform.SetParent(parent);
            float h = Random.Range(0.3f, 0.7f);
            stick.transform.position = pos + new Vector3(Random.Range(-0.15f, 0.15f), h * 0.5f, Random.Range(-0.15f, 0.15f));
            stick.transform.localScale = new Vector3(0.02f, h * 0.5f, 0.02f);
            stick.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
            ApplyColor(stick, new Color(0.16f, 0.12f, 0.08f));
        }
    }

    static void CreateWallTorch(Transform parent, Vector3 pos, bool leftSide)
    {
        // Torch holder (metal bracket)
        var holder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        holder.name = "TorchHolder";
        holder.transform.SetParent(parent);
        holder.transform.position = pos;
        holder.transform.localScale = new Vector3(0.04f, 0.2f, 0.04f);
        holder.transform.rotation = Quaternion.Euler(0f, 0f, leftSide ? -45f : 45f);
        ApplyColor(holder, COL_METAL);

        // Torch head (emissive)
        float headX = leftSide ? pos.x + 0.2f : pos.x - 0.2f;
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "TorchFlame";
        head.transform.SetParent(parent);
        head.transform.position = new Vector3(headX, pos.y + 0.15f, pos.z);
        head.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
        ApplyEmissive(head, new Color(1f, 0.5f, 0.1f));

        // Light with flicker
        var torchLight = new GameObject("TorchLight");
        torchLight.transform.SetParent(parent);
        torchLight.transform.position = head.transform.position;
        var tl = torchLight.AddComponent<Light>();
        tl.type = LightType.Point;
        tl.color = new Color(1f, 0.6f, 0.2f);
        tl.intensity = 2f;
        tl.range = 6f;
        var tf = torchLight.AddComponent<TitleLightFlicker>();
        tf.style = TitleLightFlicker.FlickerStyle.Torch;
    }

    static void CreateGuardSilhouette(Transform parent, Vector3 pos)
    {
        var guard = new GameObject("GuardSilhouette");
        guard.transform.SetParent(parent);
        guard.transform.position = pos;

        Color guardColor = new Color(0.08f, 0.07f, 0.06f);

        // Body (wider than skaa — armored)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(guard.transform);
        body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        body.transform.localScale = new Vector3(0.4f, 0.7f, 0.25f);
        ApplyColor(body, guardColor);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(guard.transform);
        head.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        head.transform.localScale = new Vector3(0.2f, 0.22f, 0.2f);
        ApplyColor(head, guardColor);

        // Helmet crest
        var crest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crest.name = "Crest";
        crest.transform.SetParent(guard.transform);
        crest.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        crest.transform.localScale = new Vector3(0.03f, 0.15f, 0.2f);
        ApplyColor(crest, COL_METAL);

        // Spear (long thin cylinder)
        var spear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spear.name = "Spear";
        spear.transform.SetParent(guard.transform);
        spear.transform.localPosition = new Vector3(0.25f, 1f, 0f);
        spear.transform.localScale = new Vector3(0.025f, 1.2f, 0.025f);
        ApplyColor(spear, COL_METAL);

        // Spear tip
        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "SpearTip";
        tip.transform.SetParent(spear.transform, false);
        tip.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        tip.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
        ApplyColor(tip, new Color(0.30f, 0.30f, 0.33f));
    }

    static void CreatePuddle(Transform parent, Vector3 pos, float size)
    {
        var puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        puddle.name = "Puddle";
        puddle.transform.SetParent(parent);
        puddle.transform.position = pos;
        puddle.transform.localScale = new Vector3(size, 0.003f, size * Random.Range(0.6f, 1f));
        ApplyColor(puddle, new Color(0.06f, 0.08f, 0.12f));
    }

    static void CreateClothesline(Transform parent, Vector3 pos, float span)
    {
        // Line (thin cylinder stretched across)
        var line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        line.name = "Clothesline";
        line.transform.SetParent(parent);
        line.transform.position = pos;
        line.transform.localScale = new Vector3(0.01f, span * 0.5f, 0.01f);
        line.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(line, new Color(0.20f, 0.18f, 0.14f));

        // Hanging cloth pieces
        for (int i = 0; i < 3; i++)
        {
            var cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = "Cloth";
            cloth.transform.SetParent(parent);
            float cx = pos.x + Random.Range(-2f, 2f);
            cloth.transform.position = new Vector3(cx, pos.y - Random.Range(0.3f, 0.8f), pos.z + Random.Range(-0.1f, 0.1f));
            cloth.transform.localScale = new Vector3(0.5f, Random.Range(0.4f, 0.7f), 0.02f);
            cloth.transform.rotation = Quaternion.Euler(0f, Random.Range(-10f, 10f), Random.Range(-5f, 5f));

            Color[] clothColors = {
                new Color(0.30f, 0.25f, 0.20f), // brown rag
                new Color(0.22f, 0.22f, 0.25f), // grey
                new Color(0.20f, 0.15f, 0.10f), // dark tan
            };
            ApplyColor(cloth, clothColors[Random.Range(0, clothColors.Length)]);
            cloth.AddComponent<TitleObjectSway>().swayType = TitleObjectSway.SwayType.Cloth;
        }
    }

    static void CreateRooftopMistborn(Transform parent, Vector3 pos)
    {
        var mb = new GameObject("RooftopMistborn");
        mb.transform.SetParent(parent);
        mb.transform.position = pos;

        Color mbColor = new Color(0.04f, 0.04f, 0.05f);

        // Crouching body
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(mb.transform);
        body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        body.transform.localScale = new Vector3(0.3f, 0.4f, 0.25f);
        body.transform.rotation = Quaternion.Euler(25f, -30f, 0f);
        ApplyColor(body, mbColor);

        // Head (hooded)
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(mb.transform);
        head.transform.localPosition = new Vector3(0f, 0.75f, 0.1f);
        head.transform.localScale = new Vector3(0.2f, 0.18f, 0.2f);
        ApplyColor(head, mbColor);

        // Mistcloak draping behind (multiple flat pieces)
        for (int t = 0; t < 5; t++)
        {
            var tassel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tassel.name = "CloakTassel";
            tassel.transform.SetParent(mb.transform);
            tassel.transform.localPosition = new Vector3(
                Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.5f), -0.3f + t * -0.15f);
            tassel.transform.localScale = new Vector3(0.15f, Random.Range(0.3f, 0.6f), 0.02f);
            tassel.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(-15f, 15f), Random.Range(-5f, 5f));
            ApplyColor(tassel, new Color(mbColor.r + 0.02f, mbColor.g + 0.02f, mbColor.b + 0.02f));
        }
    }

    static void CreateAllomanticLineFlash(Transform parent, Vector3 from, Vector3 to)
    {
        // A thin blue line between two points — hints at Allomancy in the city
        var lineObj = new GameObject("AllomanticLine");
        lineObj.transform.SetParent(parent);
        lineObj.transform.position = from;
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPositions(new[] { from, to });
        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f;
        lr.startColor = new Color(0.3f, 0.55f, 1f, 0.6f);
        lr.endColor = new Color(0.3f, 0.55f, 1f, 0.1f);
        lr.useWorldSpace = true;

        // Try to assign a working material
        var mat = CreateSavedMaterial(new Color(0.3f, 0.55f, 1f, 0.6f), "AllomLine");
        lr.material = mat;
    }

    static void CreateArchway(Transform parent, Vector3 pos, float span)
    {
        // Horizontal beam connecting buildings across the street
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "Archway";
        beam.transform.SetParent(parent);
        beam.transform.position = pos;
        beam.transform.localScale = new Vector3(span, 0.8f, 1.5f);
        ApplyColor(beam, COL_STONE_GREY);

        // Arch underside detail (slightly different shade)
        var underside = GameObject.CreatePrimitive(PrimitiveType.Cube);
        underside.name = "ArchUnderside";
        underside.transform.SetParent(beam.transform, false);
        underside.transform.localPosition = new Vector3(0f, -0.4f, 0f);
        underside.transform.localScale = new Vector3(0.95f, 0.3f, 0.9f);
        ApplyColor(underside, COL_STONE_DARK);
    }

    static void CreateSteps(Transform parent, Vector3 pos, bool leftSide)
    {
        float dir = leftSide ? 1f : -1f;
        for (int s = 0; s < 3; s++)
        {
            var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "Step";
            step.transform.SetParent(parent);
            step.transform.position = pos + new Vector3(dir * s * 0.25f, s * 0.12f, 0f);
            step.transform.localScale = new Vector3(0.8f, 0.12f, 1.2f);
            ApplyColor(step, Color.Lerp(COL_COBBLE, COL_STONE_MED, 0.5f));
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // KREDIK SHAW DETAIL BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    static void CreatePerimeterWall(Transform parent, Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * Mathf.PI * 2f / segments;
            float angle2 = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0f, Mathf.Sin(angle1) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0f, Mathf.Sin(angle2) * radius);
            Vector3 mid = (p1 + p2) * 0.5f;
            float length = Vector3.Distance(p1, p2);
            float angle = Mathf.Atan2(p2.x - p1.x, p2.z - p1.z) * Mathf.Rad2Deg;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "PerimeterWall";
            wall.transform.SetParent(parent);
            wall.transform.position = mid + new Vector3(0f, 3f, 0f);
            wall.transform.localScale = new Vector3(0.5f, 6f, length);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            ApplyColor(wall, new Color(0.12f, 0.11f, 0.13f));

            // Battlement on top
            var battlement = GameObject.CreatePrimitive(PrimitiveType.Cube);
            battlement.name = "Battlement";
            battlement.transform.SetParent(wall.transform, false);
            battlement.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            battlement.transform.localScale = new Vector3(1.3f, 0.08f, 1.0f);
            ApplyColor(battlement, COL_METAL);
        }
    }

    static void CreateGate(Transform parent, Vector3 pos)
    {
        // Two pillars
        for (int side = -1; side <= 1; side += 2)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "GatePillar";
            pillar.transform.SetParent(parent);
            pillar.transform.position = pos + new Vector3(side * 2.5f, 5f, 0f);
            pillar.transform.localScale = new Vector3(1.5f, 10f, 1.5f);
            ApplyColor(pillar, new Color(0.10f, 0.09f, 0.11f));

            // Pillar cap
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "PillarCap";
            cap.transform.SetParent(pillar.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            cap.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);
            ApplyColor(cap, COL_SPIRE_TIP);
        }

        // Arch (stretched cube across the top)
        var arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arch.name = "GateArch";
        arch.transform.SetParent(parent);
        arch.transform.position = pos + new Vector3(0f, 9.5f, 0f);
        arch.transform.localScale = new Vector3(7f, 1.5f, 1f);
        ApplyColor(arch, new Color(0.11f, 0.10f, 0.12f));

        // Gate doors (dark metal)
        var doorL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorL.name = "GateDoorL";
        doorL.transform.SetParent(parent);
        doorL.transform.position = pos + new Vector3(-1.2f, 4f, 0.2f);
        doorL.transform.localScale = new Vector3(2.2f, 8f, 0.15f);
        ApplyColor(doorL, new Color(0.08f, 0.08f, 0.10f));

        var doorR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorR.name = "GateDoorR";
        doorR.transform.SetParent(parent);
        doorR.transform.position = pos + new Vector3(1.2f, 4f, 0.2f);
        doorR.transform.localScale = new Vector3(2.2f, 8f, 0.15f);
        ApplyColor(doorR, new Color(0.08f, 0.08f, 0.10f));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PARTICLE BUILDERS
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates ash particle system with physics-accurate parameters.
    ///
    /// From PHYSICS-MATH-BOOK Section 16:
    ///   Terminal velocity v_t = sqrt(4gd(ρ_p-ρ_f) / (3 C_d ρ_f))
    ///   Fine ash (d=0.1mm): v_t ≈ 0.26 m/s → stays airborne for hours
    ///   Coarse ash (d=2mm):  v_t ≈ 3.6 m/s  → falls like heavy snow
    ///
    /// We simulate a MIX of fine + coarse particles:
    ///   gravityModifier = 0.03–0.15 (mapped from v_t in Unity's gravity scale)
    ///   startSize = 0.015–0.08 (fine singles to small aggregates, Section 16 fractal)
    ///   Noise simulates irregular sphericity (ψ ≈ 0.6–0.8 for volcanic ash)
    ///   causing erratic drift instead of straight-line falling.
    /// </summary>
    static ParticleSystem CreateAshParticles(Transform parent, Vector3 pos, float spread)
    {
        var obj = new GameObject("AshParticles");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        // Lifetime: fine ash stays airborne much longer than coarse
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 14f);
        // Speed: slight initial lateral velocity from eruption plume
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        // Size: mix of fine singles (0.015) and fractal aggregates (0.08)
        // From Section 16: d_agg = d_0 × (N/k_f)^(1/D_f), small aggregates ~5-10× primary
        main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.08f);
        // Color: grey-brown with slight variation
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.30f, 0.27f, 0.23f, 0.6f),  // lighter fine ash
            new Color(0.45f, 0.40f, 0.33f, 0.9f)    // darker coarse fragments
        );
        main.maxParticles = 800;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        // Gravity: mapped from terminal velocity range
        // v_t(fine)=0.26m/s, v_t(coarse)=3.6m/s → gravityMod 0.03-0.15 gives good visual
        main.gravityModifier = new ParticleSystem.MinMaxCurve(0.03f, 0.15f);
        // Rotation: ash tumbles as it falls (irregular shape)
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var em = ps.emission;
        em.rateOverTime = 60f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spread, 0.5f, spread);

        // Noise simulates irregular drag from low sphericity (ψ ≈ 0.6-0.8)
        // Jagged ash fragments tumble and drift erratically, not fall straight
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;           // erratic drift amount
        noise.frequency = 0.6f;           // turbulence frequency
        noise.scrollSpeed = 0.25f;        // wind variation over time
        noise.octaveCount = 2;            // layered turbulence
        noise.damping = true;             // smaller particles affected more

        // Rotation over lifetime: continuous tumbling (low sphericity)
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-1f, 1f);

        // Size over lifetime: slight shrink as ash breaks apart mid-air
        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.7f, 0.9f), new Keyframe(1f, 0.6f)));

        // Color over lifetime: slight fade near end of life
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var ashGrad = new Gradient();
        ashGrad.SetKeys(
            new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 0.8f),
                new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 1f)
            },
            new[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.1f),
                new GradientAlphaKey(0.7f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(ashGrad);

        return ps;
    }

    /// <summary>
    /// Creates Scadrial's mist particle system.
    ///
    /// Lore: the mists are Preservation's power made manifest. They come every night,
    /// rolling in at dusk and retreating at dawn. They cling to the ground, swirl
    /// around objects, and are thick enough to limit visibility to a few dozen meters.
    /// Allomancers can "feel" the mists — they're not normal fog.
    ///
    /// Visually: thick, slow-moving tendrils that coalesce and dissipate.
    /// Slightly luminous — they have a faint inner light (Preservation's investiture).
    /// </summary>
    static ParticleSystem CreateMistParticles(Transform parent, Vector3 pos, float spread)
    {
        var obj = new GameObject("MistParticles");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(12f, 20f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.12f);
        // Moderate size — visible but not screen-filling squares
        main.startSize = new ParticleSystem.MinMaxCurve(4f, 10f);
        // Subtle opacity — you can see through them, they add atmosphere
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.70f, 0.73f, 0.80f, 0.08f),  // very transparent
            new Color(0.85f, 0.85f, 0.90f, 0.15f)    // slightly more visible
        );
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var em = ps.emission;
        em.rateOverTime = 4f; // Gentle mist, not a wall of squares
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spread, 1.5f, spread);

        // Mist flows and swirls — slow, organic, supernatural
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 0.15f;       // very slow, large-scale movement
        noise.scrollSpeed = 0.06f;     // gradual wind change
        noise.octaveCount = 2;
        noise.damping = true;

        // Slow rotation — mist tendrils curl
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

        // Grow slightly as they spread out, then shrink as they dissipate
        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f), new Keyframe(0.3f, 1.0f),
            new Keyframe(0.7f, 1.1f), new Keyframe(1f, 0.3f)));

        // Fade: appear gradually, hold, fade slowly
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.75f, 0.78f, 0.85f), 0f),
                new GradientColorKey(Color.white, 0.4f),
                new GradientColorKey(new Color(0.70f, 0.72f, 0.80f), 1f)
            },
            new[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.6f, 0.15f),
                new GradientAlphaKey(0.5f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // Render as horizontal billboards — flat fog planes, not upright squares
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            renderer.minParticleSize = 0f;
            renderer.maxParticleSize = 1f;
        }

        return ps;
    }

    static void CreateCanal(Transform parent, Vector3 center, float angleDeg, float length)
    {
        var canal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canal.name = "Canal";
        canal.transform.SetParent(parent);
        canal.transform.position = center + new Vector3(0f, -0.3f, 0f);
        canal.transform.localScale = new Vector3(3f, 0.8f, length);
        canal.transform.rotation = Quaternion.Euler(0f, angleDeg, 0f);
        ApplyColor(canal, new Color(0.04f, 0.06f, 0.10f)); // dark water

        // Canal walls
        for (int side = -1; side <= 1; side += 2)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "CanalWall";
            wall.transform.SetParent(canal.transform, false);
            wall.transform.localPosition = new Vector3(side * 0.55f, 0.7f, 0f);
            wall.transform.localScale = new Vector3(0.08f, 0.6f, 1f);
            ApplyColor(wall, COL_STONE_GREY);
        }
    }

    static void CreateNobleKeep(Transform parent, Vector3 pos)
    {
        // Main building — taller and wider than commoner buildings
        var main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "NobleKeep";
        main.transform.SetParent(parent);
        float h = Random.Range(14f, 20f);
        main.transform.position = pos + new Vector3(0f, h * 0.5f, 0f);
        main.transform.localScale = new Vector3(10f, h, 8f);
        ApplyColor(main, COL_STONE_LIGHT);

        // Tower on one corner
        var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tower.name = "KeepTower";
        tower.transform.SetParent(parent);
        float th = h + Random.Range(4f, 8f);
        tower.transform.position = pos + new Vector3(4f, th * 0.5f, 3f);
        tower.transform.localScale = new Vector3(2f, th * 0.5f, 2f);
        ApplyColor(tower, COL_STONE_MED);

        // Spire on the tower
        var spire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spire.name = "KeepSpire";
        spire.transform.SetParent(tower.transform, false);
        spire.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        spire.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
        ApplyColor(spire, COL_METAL);

        // Courtyard wall around the keep
        for (int i = 0; i < 4; i++)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "KeepWall";
            wall.transform.SetParent(parent);
            float angle = i * 90f;
            Vector3 wallPos = pos + Quaternion.Euler(0, angle, 0) * new Vector3(0f, 2f, 8f);
            wall.transform.position = wallPos;
            wall.transform.localScale = new Vector3(16f, 4f, 0.5f);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            ApplyColor(wall, COL_STONE_DARK);
        }

        // Window light
        var keepLight = new GameObject("KeepLight");
        keepLight.transform.SetParent(parent);
        keepLight.transform.position = pos + new Vector3(0f, h * 0.7f, 0f);
        var kl = keepLight.AddComponent<Light>();
        kl.type = LightType.Point;
        kl.color = COL_WINDOW_WARM;
        kl.intensity = 1.5f;
        kl.range = 15f;
    }

    static void CreateDistantCitySilhouette(Transform parent, Vector3 center)
    {
        // Row of dark building silhouettes on the horizon
        for (int i = -8; i <= 8; i++)
        {
            float x = center.x + i * 4f + Random.Range(-1f, 1f);
            float h = Random.Range(2f, 7f);
            if (Mathf.Abs(i) <= 1) h = Random.Range(6f, 10f); // taller center (Kredik Shaw hint)

            var bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "DistantBuilding";
            bldg.transform.SetParent(parent);
            bldg.transform.position = new Vector3(x, h * 0.5f, center.z);
            bldg.transform.localScale = new Vector3(Random.Range(2f, 4f), h, Random.Range(1f, 3f));
            // Very dark — almost blends with fog, just barely visible
            ApplyColor(bldg, new Color(0.06f, 0.05f, 0.06f));
        }

        // Central spire hint (Kredik Shaw from a distance)
        var spireHint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spireHint.name = "DistantKredikHint";
        spireHint.transform.SetParent(parent);
        spireHint.transform.position = new Vector3(center.x, 7f, center.z);
        spireHint.transform.localScale = new Vector3(0.3f, 7f, 0.3f);
        ApplyColor(spireHint, new Color(0.05f, 0.04f, 0.06f));
    }

    static void CreateSpireWindowLights(Transform parent, Vector3 center, float ringRadius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * ringRadius, Random.Range(8f, 20f), Mathf.Sin(angle) * ringRadius);
            var light = new GameObject("SpireLight");
            light.transform.SetParent(parent);
            light.transform.position = pos;
            var pl = light.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = COL_WINDOW_WARM;
            pl.intensity = Random.Range(0.3f, 0.8f);
            pl.range = Random.Range(3f, 6f);
        }
    }

    static void CreateCityWall(Transform parent, Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * Mathf.PI * 2f / segments;
            float angle2 = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0f, Mathf.Sin(angle1) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0f, Mathf.Sin(angle2) * radius);
            Vector3 mid = (p1 + p2) * 0.5f;
            float length = Vector3.Distance(p1, p2);
            float angle = Mathf.Atan2(p2.x - p1.x, p2.z - p1.z) * Mathf.Rad2Deg;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "CityWall";
            wall.transform.SetParent(parent);
            wall.transform.position = mid + new Vector3(0f, 4f, 0f);
            wall.transform.localScale = new Vector3(1f, 8f, length);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            ApplyColor(wall, new Color(0.16f, 0.14f, 0.13f));

            // Tower at each corner
            if (i % 4 == 0)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = "WallTower";
                tower.transform.SetParent(parent);
                tower.transform.position = p1 + new Vector3(0f, 6f, 0f);
                tower.transform.localScale = new Vector3(2.5f, 6f, 2.5f);
                ApplyColor(tower, new Color(0.14f, 0.12f, 0.12f));
            }
        }
    }

    static void CreateSmokeParticles(Transform parent, Vector3 pos)
    {
        var obj = new GameObject("ChimneySmoke");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startColor = new Color(0.25f, 0.23f, 0.20f, 0.3f);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.03f; // rises
        var em = ps.emission;
        em.rateOverTime = 4f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.3f, 0.28f, 0.25f), 0f), new GradientColorKey(new Color(0.15f, 0.15f, 0.15f), 1f) },
            new[] { new GradientAlphaKey(0.3f, 0f), new GradientAlphaKey(0.15f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f), new Keyframe(1f, 2f)));
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.5f;
        noise.octaveCount = 1;
    }

    static void CreateWatchtower(Transform parent, Vector3 pos)
    {
        // Tall cylindrical tower with lookout platform
        var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tower.name = "Watchtower";
        tower.transform.SetParent(parent);
        tower.transform.position = pos + new Vector3(0f, 6f, 0f);
        tower.transform.localScale = new Vector3(1.5f, 6f, 1.5f);
        ApplyColor(tower, COL_STONE_GREY);

        // Lookout platform
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "LookoutPlatform";
        platform.transform.SetParent(parent);
        platform.transform.position = pos + new Vector3(0f, 12.2f, 0f);
        platform.transform.localScale = new Vector3(2.2f, 0.15f, 2.2f);
        ApplyColor(platform, COL_STONE_MED);

        // Torch at top
        var torchLight = new GameObject("WatchtowerTorch");
        torchLight.transform.SetParent(parent);
        torchLight.transform.position = pos + new Vector3(0f, 13f, 0f);
        var wl = torchLight.AddComponent<Light>();
        wl.type = LightType.Point;
        wl.color = COL_LANTERN;
        wl.intensity = 3f;
        wl.range = 15f;
        torchLight.AddComponent<TitleLightFlicker>().style = TitleLightFlicker.FlickerStyle.Torch;
    }

    static void CreateSkeleton(Transform parent, Vector3 pos)
    {
        Color boneColor = new Color(0.35f, 0.32f, 0.28f);

        // Ribcage (flattened sphere)
        var ribs = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ribs.name = "Skeleton";
        ribs.transform.SetParent(parent);
        ribs.transform.position = pos + new Vector3(0f, 0.08f, 0f);
        ribs.transform.localScale = new Vector3(0.25f, 0.08f, 0.15f);
        ribs.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        ApplyColor(ribs, boneColor);

        // Skull
        var skull = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        skull.name = "Skull";
        skull.transform.SetParent(parent);
        skull.transform.position = pos + new Vector3(0.2f, 0.06f, 0f);
        skull.transform.localScale = new Vector3(0.1f, 0.08f, 0.08f);
        ApplyColor(skull, boneColor);
    }

    static void CreateGibbetPost(Transform parent, Vector3 pos)
    {
        // Tall post with iron cage at top
        var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "GibbetPost";
        post.transform.SetParent(parent);
        post.transform.position = pos + new Vector3(0f, 2f, 0f);
        post.transform.localScale = new Vector3(0.08f, 2f, 0.08f);
        ApplyColor(post, COL_WOOD);

        // Cross beam
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "GibbetBeam";
        beam.transform.SetParent(parent);
        beam.transform.position = pos + new Vector3(0.4f, 3.8f, 0f);
        beam.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
        beam.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(beam, COL_WOOD);

        // Iron cage (wireframe approximation — cube with visible edges)
        var cage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cage.name = "GibbetCage";
        cage.transform.SetParent(parent);
        cage.transform.position = pos + new Vector3(0.7f, 3.2f, 0f);
        cage.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
        ApplyColor(cage, COL_METAL);
        cage.AddComponent<TitleObjectSway>().swayType = TitleObjectSway.SwayType.HangingSign;
    }

    static void CreateGallows(Transform parent, Vector3 pos)
    {
        // Execution platform
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "GallowsPlatform";
        platform.transform.SetParent(parent);
        platform.transform.position = pos + new Vector3(0f, 0.3f, 0f);
        platform.transform.localScale = new Vector3(3f, 0.6f, 2f);
        ApplyColor(platform, COL_WOOD);

        // Upright posts
        for (int side = -1; side <= 1; side += 2)
        {
            var upright = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            upright.name = "GallowsUpright";
            upright.transform.SetParent(parent);
            upright.transform.position = pos + new Vector3(side * 0.8f, 2.5f, -0.5f);
            upright.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
            ApplyColor(upright, COL_WOOD);
        }

        // Crossbeam
        var crossbeam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crossbeam.name = "GallowsBeam";
        crossbeam.transform.SetParent(parent);
        crossbeam.transform.position = pos + new Vector3(0f, 4.5f, -0.5f);
        crossbeam.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
        crossbeam.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(crossbeam, COL_WOOD);

        // Noose (thin cylinder hanging down)
        var rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "GallowsRope";
        rope.transform.SetParent(parent);
        rope.transform.position = pos + new Vector3(0f, 3.5f, -0.5f);
        rope.transform.localScale = new Vector3(0.02f, 0.8f, 0.02f);
        ApplyColor(rope, new Color(0.28f, 0.22f, 0.14f));

        // Steps up to the platform
        for (int s = 0; s < 3; s++)
        {
            var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "GallowsStep";
            step.transform.SetParent(parent);
            step.transform.position = pos + new Vector3(0f, s * 0.1f, 0.7f + s * 0.3f);
            step.transform.localScale = new Vector3(1.5f, 0.1f, 0.3f);
            ApplyColor(step, COL_WOOD);
        }
    }

    static void CreateSleepingSkaa(Transform parent, Vector3 pos)
    {
        var skaa = new GameObject("SleepingSkaa");
        skaa.transform.SetParent(parent);
        skaa.transform.position = pos;

        Color col = new Color(0.08f, 0.07f, 0.06f);

        // Body curled on the ground
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(skaa.transform);
        body.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        body.transform.localScale = new Vector3(0.25f, 0.12f, 0.35f);
        body.transform.rotation = Quaternion.Euler(0f, Random.Range(-20f, 20f), 90f);
        ApplyColor(body, col);

        // Blanket / rag over them
        var blanket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blanket.name = "Blanket";
        blanket.transform.SetParent(skaa.transform);
        blanket.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        blanket.transform.localScale = new Vector3(0.6f, 0.03f, 0.4f);
        ApplyColor(blanket, new Color(0.15f, 0.12f, 0.10f));
    }

    static void CreateNoticeBoard(Transform parent, Vector3 pos, bool leftSide)
    {
        // Wooden board mounted on wall
        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "NoticeBoard";
        board.transform.SetParent(parent);
        board.transform.position = pos;
        board.transform.localScale = new Vector3(0.06f, 0.6f, 0.8f);
        ApplyColor(board, COL_WOOD);

        // Paper notices (lighter rectangles on the board)
        for (int n = 0; n < 3; n++)
        {
            var notice = GameObject.CreatePrimitive(PrimitiveType.Cube);
            notice.name = "Notice";
            notice.transform.SetParent(board.transform, false);
            notice.transform.localPosition = new Vector3(
                leftSide ? 0.55f : -0.55f,
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f));
            notice.transform.localScale = new Vector3(0.1f, 0.35f, 0.25f);
            notice.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-5f, 5f));
            ApplyColor(notice, new Color(0.40f, 0.36f, 0.28f)); // aged paper
        }
    }

    static void CreateDrainPipe(Transform parent, Vector3 pos, float height)
    {
        var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pipe.name = "DrainPipe";
        pipe.transform.SetParent(parent);
        pipe.transform.position = pos + new Vector3(0f, height * 0.5f, 0f);
        pipe.transform.localScale = new Vector3(0.08f, height * 0.5f, 0.08f);
        ApplyColor(pipe, COL_METAL);

        // Elbow at bottom
        var elbow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        elbow.name = "DrainElbow";
        elbow.transform.SetParent(parent);
        elbow.transform.position = pos + new Vector3(0f, 0.15f, 0.08f);
        elbow.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        ApplyColor(elbow, COL_METAL);
    }

    static void CreateSkaaWorker(Transform parent, Vector3 pos)
    {
        var worker = new GameObject("SkaaWorker");
        worker.transform.SetParent(parent);
        worker.transform.position = pos;

        Color col = new Color(0.07f, 0.06f, 0.05f);

        // Hunched body (carrying heavy load)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(worker.transform);
        body.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        body.transform.localScale = new Vector3(0.3f, 0.6f, 0.22f);
        body.transform.rotation = Quaternion.Euler(15f, Random.Range(-10f, 10f), 0f);
        ApplyColor(body, col);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(worker.transform);
        head.transform.localPosition = new Vector3(0f, 1.25f, 0.1f);
        head.transform.localScale = new Vector3(0.17f, 0.18f, 0.17f);
        ApplyColor(head, col);

        // Sack on back
        var sack = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sack.name = "Sack";
        sack.transform.SetParent(worker.transform);
        sack.transform.localPosition = new Vector3(0f, 1.0f, -0.2f);
        sack.transform.localScale = new Vector3(0.3f, 0.25f, 0.25f);
        ApplyColor(sack, new Color(0.20f, 0.17f, 0.12f));
    }

    static void CreateSteelMinistry(Transform parent, Vector3 pos)
    {
        // Distinctive tall building — the Canton of Inquisition headquarters
        var main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "SteelMinistry";
        main.transform.SetParent(parent);
        float h = 22f;
        main.transform.position = pos + new Vector3(0f, h * 0.5f, 0f);
        main.transform.localScale = new Vector3(12f, h, 10f);
        ApplyColor(main, COL_STONE_DARK);

        // Central spire (taller than the building, thinner)
        var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spire.name = "MinistrySpire";
        spire.transform.SetParent(parent);
        spire.transform.position = pos + new Vector3(0f, h + 6f, 0f);
        spire.transform.localScale = new Vector3(1.5f, 6f, 1.5f);
        ApplyColor(spire, COL_SPIRE);

        // Spire tip
        var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "MinistrySpireTip";
        tip.transform.SetParent(spire.transform, false);
        tip.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        tip.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
        ApplyColor(tip, COL_SPIRE_TIP);

        // Iron symbol on front (cube representing the Steel Ministry emblem)
        var emblem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emblem.name = "MinistryEmblem";
        emblem.transform.SetParent(parent);
        emblem.transform.position = pos + new Vector3(0f, h * 0.7f, 5.05f);
        emblem.transform.localScale = new Vector3(2f, 2f, 0.15f);
        ApplyColor(emblem, COL_METAL);

        // Steps leading up
        for (int s = 0; s < 5; s++)
        {
            var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "MinistryStep";
            step.transform.SetParent(parent);
            step.transform.position = pos + new Vector3(0f, s * 0.2f, 5.5f + s * 0.5f);
            step.transform.localScale = new Vector3(8f - s * 0.5f, 0.2f, 0.5f);
            ApplyColor(step, COL_STONE_MED);
        }

        // Bright window glow (Inquisitors never sleep)
        var glow = new GameObject("MinistryGlow");
        glow.transform.SetParent(parent);
        glow.transform.position = pos + new Vector3(0f, h * 0.6f, 0f);
        var gl = glow.AddComponent<Light>();
        gl.type = LightType.Point;
        gl.color = new Color(0.8f, 0.4f, 0.15f);
        gl.intensity = 2f;
        gl.range = 12f;
        glow.AddComponent<TitleLightFlicker>().style = TitleLightFlicker.FlickerStyle.WindowGlow;
    }

    static void CreateBanner(Transform parent, Vector3 pos)
    {
        // Pole (metal — this is controlled by the Lord Ruler)
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "BannerPole";
        pole.transform.SetParent(parent);
        pole.transform.position = pos + new Vector3(0f, 1f, 0f);
        pole.transform.localScale = new Vector3(0.04f, 1f, 0.04f);
        ApplyColor(pole, COL_METAL);

        // Banner cloth (dark red — the Lord Ruler's colors)
        var cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cloth.name = "BannerCloth";
        cloth.transform.SetParent(parent);
        cloth.transform.position = pos + new Vector3(0f, 0.5f, 0.15f);
        cloth.transform.localScale = new Vector3(0.03f, 1.5f, 0.8f);
        cloth.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-3f, 3f));
        ApplyColor(cloth, new Color(0.35f, 0.08f, 0.05f)); // dark blood red
        cloth.AddComponent<TitleObjectSway>().swayType = TitleObjectSway.SwayType.Banner;

        // Gold trim strip
        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "BannerTrim";
        trim.transform.SetParent(cloth.transform, false);
        trim.transform.localPosition = new Vector3(0f, -0.48f, 0f);
        trim.transform.localScale = new Vector3(1.1f, 0.05f, 1.05f);
        ApplyColor(trim, new Color(0.50f, 0.40f, 0.12f)); // gold
    }

    static void CreateChain(Transform parent, Vector3 from, Vector3 to)
    {
        // Chain = series of small links between two points
        Vector3 dir = to - from;
        float length = dir.magnitude;
        int links = Mathf.Max(3, Mathf.FloorToInt(length / 0.3f));
        float sag = length * 0.08f; // catenary sag

        for (int i = 0; i <= links; i++)
        {
            float t = (float)i / links;
            Vector3 pos = Vector3.Lerp(from, to, t);
            // Catenary sag (parabolic approximation)
            float sagAmount = 4f * sag * t * (1f - t);
            pos.y -= sagAmount;

            var link = GameObject.CreatePrimitive(PrimitiveType.Cube);
            link.name = "ChainLink";
            link.transform.SetParent(parent);
            link.transform.position = pos;
            link.transform.localScale = new Vector3(0.04f, 0.06f, 0.04f);
            link.transform.rotation = Quaternion.Euler(0f, i * 45f, i % 2 == 0 ? 0f : 90f);
            ApplyColor(link, COL_METAL);
        }
    }

    static void CreateBalcony(Transform parent, Vector3 pos, bool leftSide)
    {
        float dir = leftSide ? 1f : -1f;

        // Platform
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "BalconyPlatform";
        platform.transform.SetParent(parent);
        platform.transform.position = pos + new Vector3(dir * 0.5f, 0f, 0f);
        platform.transform.localScale = new Vector3(1.2f, 0.1f, 1.5f);
        ApplyColor(platform, COL_STONE_MED);

        // Railing (metal)
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = "BalconyRail";
        rail.transform.SetParent(parent);
        rail.transform.position = pos + new Vector3(dir * 1.1f, 0.4f, 0f);
        rail.transform.localScale = new Vector3(0.05f, 0.8f, 1.5f);
        ApplyColor(rail, COL_METAL);

        // Support brackets
        var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bracket.name = "BalconyBracket";
        bracket.transform.SetParent(parent);
        bracket.transform.position = pos + new Vector3(dir * 0.3f, -0.3f, 0f);
        bracket.transform.localScale = new Vector3(0.8f, 0.08f, 0.08f);
        bracket.transform.rotation = Quaternion.Euler(0f, 0f, leftSide ? -30f : 30f);
        ApplyColor(bracket, COL_METAL);
    }

    static void CreateWell(Transform parent, Vector3 pos)
    {
        // Circular stone wall
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wall.name = "WellWall";
        wall.transform.SetParent(parent);
        wall.transform.position = pos + new Vector3(0f, 0.4f, 0f);
        wall.transform.localScale = new Vector3(1.0f, 0.4f, 1.0f);
        ApplyColor(wall, COL_STONE_GREY);

        // Inner void (darker cylinder, slightly smaller)
        var inner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        inner.name = "WellInner";
        inner.transform.SetParent(parent);
        inner.transform.position = pos + new Vector3(0f, 0.45f, 0f);
        inner.transform.localScale = new Vector3(0.8f, 0.45f, 0.8f);
        ApplyColor(inner, new Color(0.03f, 0.03f, 0.04f)); // very dark — deep hole

        // Roof structure (two posts + crossbeam + bucket rope)
        for (int side = -1; side <= 1; side += 2)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "WellPost";
            post.transform.SetParent(parent);
            post.transform.position = pos + new Vector3(side * 0.45f, 1.2f, 0f);
            post.transform.localScale = new Vector3(0.06f, 0.8f, 0.06f);
            ApplyColor(post, COL_WOOD);
        }

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "WellBeam";
        beam.transform.SetParent(parent);
        beam.transform.position = pos + new Vector3(0f, 2f, 0f);
        beam.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
        beam.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ApplyColor(beam, COL_WOOD);

        // Bucket (small cube dangling)
        var bucket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bucket.name = "WellBucket";
        bucket.transform.SetParent(parent);
        bucket.transform.position = pos + new Vector3(0.1f, 1.3f, 0f);
        bucket.transform.localScale = new Vector3(0.15f, 0.18f, 0.15f);
        ApplyColor(bucket, new Color(COL_WOOD.r * 0.7f, COL_WOOD.g * 0.7f, COL_WOOD.b * 0.7f));
    }

    static void CreateSteeljumpArc(Transform parent, Vector3 start, Vector3 apex, Vector3 end)
    {
        // A Mistborn mid-steeljump — visualized as a figure + blue line trail
        // positioned at the apex of the arc

        var mb = new GameObject("SteeljumpingMistborn");
        mb.transform.SetParent(parent);
        mb.transform.position = apex;

        Color col = new Color(0.04f, 0.04f, 0.05f);

        // Body in flight pose (leaning forward)
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(mb.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.25f, 0.6f, 0.2f);
        body.transform.rotation = Quaternion.Euler(45f, 0f, 0f); // leaning forward in flight
        ApplyColor(body, col);

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(mb.transform);
        head.transform.localPosition = new Vector3(0f, 0.4f, 0.3f);
        head.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
        ApplyColor(head, col);

        // Mistcloak streaming behind
        for (int t = 0; t < 6; t++)
        {
            var tassel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tassel.name = "CloakTrail";
            tassel.transform.SetParent(mb.transform);
            tassel.transform.localPosition = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.3f, 0.1f),
                -0.4f - t * 0.2f);
            tassel.transform.localScale = new Vector3(
                0.1f + Random.Range(0f, 0.1f),
                Random.Range(0.2f, 0.5f),
                0.02f);
            tassel.transform.rotation = Quaternion.Euler(
                Random.Range(-20f, -5f), Random.Range(-10f, 10f), Random.Range(-5f, 5f));
            ApplyColor(tassel, new Color(col.r + 0.02f, col.g + 0.02f, col.b + 0.02f));
        }

        // Blue Allomantic line from the figure down to the push point (start = coin/anchor)
        var lineObj = new GameObject("SteeljumpLine");
        lineObj.transform.SetParent(parent);
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPositions(new[] { apex, start });
        lr.startWidth = 0.06f;
        lr.endWidth = 0.02f;
        lr.startColor = new Color(0.3f, 0.55f, 1f, 0.7f);
        lr.endColor = new Color(0.3f, 0.55f, 1f, 0.15f);
        lr.useWorldSpace = true;
        lr.material = CreateSavedMaterial(new Color(0.3f, 0.55f, 1f, 0.5f), "SteelLine");
    }

    static void CreateDrizzleParticles(Transform parent, Vector3 pos)
    {
        var obj = new GameObject("DrizzleParticles");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.02f);
        main.startColor = new Color(0.5f, 0.5f, 0.55f, 0.25f);
        main.maxParticles = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 1f;
        var em = ps.emission;
        em.rateOverTime = 80f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(15f, 0.1f, 30f);
        // Stretch particles to look like rain streaks
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.1f;
            renderer.lengthScale = 3f;
        }
    }

    static void CreateEmberParticles(Transform parent, Vector3 pos)
    {
        var obj = new GameObject("EmberParticles");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        var ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor = new Color(1f, 0.4f, 0.05f, 0.9f); // bright orange
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f; // float upward
        var em = ps.emission;
        em.rateOverTime = 15f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 2f, 10f);
        // Fade out over lifetime
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0.0f), 0.7f),
                new GradientColorKey(new Color(0.3f, 0.05f, 0.0f), 1f)
            },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.6f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.3f;
        noise.octaveCount = 2;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════════════════════

    // ═════════════════════════════════════════════════════════════════════════
    // ═════════════════════════════════════════════════════════════════════════
    // HDRP LIGHT HELPER
    //
    // HDRP ignores Light.intensity — it uses HDAdditionalLightData.intensity.
    // Every light must have this component or it renders at default (near zero).
    // ═════════════════════════════════════════════════════════════════════════

    static void SetupHDRPLight(Light light, float intensityLuxOrLumens)
    {
        var hd = light.GetComponent<HDAdditionalLightData>();
        if (hd == null) hd = light.gameObject.AddComponent<HDAdditionalLightData>();

        // HDRP reads from HDAdditionalLightData.intensity (same pattern as SkyController.cs)
        light.intensity = 1f;
        hd.intensity = intensityLuxOrLumens;
        hd.lightUnit = light.type == LightType.Directional ? LightUnit.Lux : LightUnit.Lumen;
    }

    // MATERIAL SYSTEM — Clone from existing HDRP material, save to disk
    //
    // Clones Ground(Temp).mat (known working HDRP Lit material) and sets
    // _BaseColor. Saved to organized subfolders under Materials/TitleSequence/.
    // Applied via sharedMaterial — no runtime components, no pink.
    // ═════════════════════════════════════════════════════════════════════════

    private static Material _sourceMat;
    private static Dictionary<string, Material> _matCache = new Dictionary<string, Material>();
    // Track every (renderer → material) assignment so we can re-apply after SaveAssets
    private static List<System.Tuple<Renderer, Material>> _pendingAssignments = new List<System.Tuple<Renderer, Material>>();

    static readonly string MAT_ROOT = "Assets/_Project/Materials/TitleSequence";

    static void EnsureMatFolder(string subfolder)
    {
        string[] parts = ($"Assets/_Project/Materials/TitleSequence/{subfolder}").Split('/');
        string current = "";
        for (int i = 0; i < parts.Length; i++)
        {
            string parent = current;
            current = i == 0 ? parts[0] : current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(current))
                AssetDatabase.CreateFolder(
                    string.IsNullOrEmpty(parent) ? parts[0] : parent,
                    parts[i]);
        }
    }

    static Material GetSourceMaterial()
    {
        if (_sourceMat != null) return _sourceMat;

        // Load a known working HDRP material from the project
        string[] candidates = {
            "Assets/_Project/Materials/Ground(Temp).mat",
            "Assets/_Project/Materials/Metal.mat",
            "Assets/_Project/Materials/Wood.mat",
            "Assets/_Project/Materials/White.mat",
            "Assets/_Project/Materials/Obsidian.mat",
            "Assets/_Project/Materials/Ground(Temp) 1.mat",
            "Assets/_Project/Materials/Player(Temp).mat",
        };
        foreach (var p in candidates)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null) { _sourceMat = m; return m; }
        }

        // Fallback: search for any .mat
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/_Project/Materials" });
        foreach (var guid in guids)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (m != null) { _sourceMat = m; return m; }
        }

        Debug.LogError("[TitleSequenceBuilder] No source material found! Create any material in Assets/_Project/Materials/ first.");
        return null;
    }

    static Material GetOrCreateMaterial(string subfolder, string name, Color color, bool emissive = false)
    {
        string path = $"{MAT_ROOT}/{subfolder}/{name}.mat";

        // Check cache first
        if (_matCache.TryGetValue(path, out Material cached)) return cached;

        // Check if already exists on disk
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            _matCache[path] = existing;
            return existing;
        }

        // Clone from source
        var source = GetSourceMaterial();
        if (source == null) return null;

        EnsureMatFolder(subfolder);

        var mat = new Material(source);
        mat.name = name;

        // Set color — skip HasProperty checks, just set everything directly.
        // HDRP materials have all these properties but HasProperty can be unreliable
        // when the material was just cloned.
        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.1f);
        mat.SetFloat("_Metallic", 0f);

        // Kill emission on ALL materials (source has _EmissionColor = white)
        mat.DisableKeyword("_EMISSION");
        mat.SetColor("_EmissiveColor", Color.black);
        mat.SetColor("_EmissiveColorLDR", Color.black);
        mat.SetColor("_EmissionColor", Color.black);
        mat.SetFloat("_UseEmissiveIntensity", 0f);
        mat.SetFloat("_EmissiveIntensity", 0f);
        mat.SetFloat("_AlbedoAffectEmissive", 0f);

        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissiveColor", color);
            mat.SetColor("_EmissiveColorLDR", color);
            mat.SetColor("_EmissionColor", color);
            // Keep UseEmissiveIntensity OFF — raw color is enough
        }

        AssetDatabase.CreateAsset(mat, path);
        _matCache[path] = mat;
        return mat;
    }

    // Pre-create all named materials so they're organized and reusable
    static Dictionary<string, Material> _namedMats = new Dictionary<string, Material>();

    static Material Mat(string subfolder, string name, Color color, bool emissive = false)
    {
        string key = $"{subfolder}/{name}";
        if (_namedMats.TryGetValue(key, out Material m)) return m;
        m = GetOrCreateMaterial(subfolder, name, color, emissive);
        if (m != null) _namedMats[key] = m;
        return m;
    }

    static void ApplyColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;

        string hex = ColorUtility.ToHtmlStringRGB(color);
        string subfolder = CategorizeColor(color);
        Material mat = Mat(subfolder, $"Col_{hex}", color);
        if (mat != null)
        {
            rend.sharedMaterial = mat;
            _pendingAssignments.Add(new System.Tuple<Renderer, Material>(rend, mat));
        }
    }

    static void ApplyEmissive(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;

        Color bright = color * 2.5f;
        bright.a = 1f;
        string hex = ColorUtility.ToHtmlStringRGB(bright);
        Material mat = Mat("Emissive", $"Emit_{hex}", bright, true);
        if (mat != null)
        {
            rend.sharedMaterial = mat;
            _pendingAssignments.Add(new System.Tuple<Renderer, Material>(rend, mat));
        }
    }

    static Material CreateSavedMaterial(Color color, string label)
    {
        // For LineRenderers
        string hex = ColorUtility.ToHtmlStringRGB(color);
        return Mat("Lines", $"{label}_{hex}", color);
    }

    static string CategorizeColor(Color c)
    {
        float brightness = (c.r + c.g + c.b) / 3f;
        if (brightness < 0.08f) return "Silhouettes";
        if (c.b > c.r * 1.3f && c.b > c.g * 1.3f) return "Water";
        if (c.r > c.g * 1.4f && c.r > c.b * 1.4f) return "Warm";
        if (Mathf.Abs(c.r - c.g) < 0.05f && Mathf.Abs(c.g - c.b) < 0.05f) return "Stone";
        if (c.r > 0.3f && c.g > 0.2f && c.b < 0.15f) return "Wood";
        return "General";
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

        // Logo image slot — starts disabled (no white square). Enable and assign
        // sprite when actual logo art is ready.
        var imgObj = new GameObject("LogoImage");
        imgObj.transform.SetParent(obj.transform, false);
        var img = imgObj.AddComponent<Image>();
        img.preserveAspect = true;
        var imgRT = imgObj.GetComponent<RectTransform>();
        imgRT.anchorMin = imgRT.anchorMax = imgRT.pivot = new Vector2(0.5f, 0.5f);
        imgRT.anchoredPosition = new Vector2(0f, 30f);
        imgRT.sizeDelta = new Vector2(200f, 200f);
        imgObj.SetActive(false); // Hidden until logo art is added

        // Text — visible now as placeholder
        var tmp = CreateTMP(obj.transform, "LogoText", text, fontSize,
            COL_TEXT, TextAlignmentOptions.Center);
        var rt = tmp.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
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
