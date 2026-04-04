/* TitlePostProcessingSetup.cs
 *
 * Editor tool: Mistborn → Scenes → Add Post-Processing to Current Scene
 *
 * Adds an HDRP Volume with Mistborn-appropriate post-processing:
 *   - Bloom (makes blue Allomantic lines and lanterns glow)
 *   - Vignette (darkened edges for cinematic feel)
 *   - Color Adjustments (desaturated, slightly warm — ash world)
 *   - Film Grain (subtle, adds texture)
 *   - Fog (volumetric if available)
 *   - Exposure (locked, dark scene)
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class TitlePostProcessingSetup
{
    [MenuItem("Mistborn/Scenes/Add Post-Processing to Current Scene")]
    public static void Setup()
    {
        // Check if a Volume already exists
        var existing = Object.FindObjectOfType<Volume>();
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Post-Processing",
                "A Volume already exists in the scene. Replace it?", "Replace", "Cancel"))
                return;
            Object.DestroyImmediate(existing.gameObject);
        }

        // Create Volume GameObject
        var volumeObj = new GameObject("PostProcessing_Volume");
        var volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;

        // Create profile asset
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Settings"))
            AssetDatabase.CreateFolder("Assets/_Project", "Settings");

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        string profilePath = "Assets/_Project/Settings/TitleSequence_PostProcessing.asset";
        AssetDatabase.CreateAsset(profile, profilePath);
        volume.sharedProfile = profile;

        // We can't directly add HDRP overrides from code without referencing
        // the HDRP assembly, so we'll set it up as much as possible and
        // log instructions for the manual steps.

        Debug.Log("[PostProcessing] Volume created. Now add overrides manually:");
        Debug.Log("  1. Select 'PostProcessing_Volume' in Hierarchy");
        Debug.Log("  2. In Inspector, click 'Add Override' on the Volume component");
        Debug.Log("  3. Add these overrides with these values:");
        Debug.Log("");
        Debug.Log("  BLOOM:");
        Debug.Log("    Intensity: 0.3");
        Debug.Log("    Threshold: 0.8");
        Debug.Log("    Scatter: 0.7");
        Debug.Log("    Tint: #4488FF (Allomantic blue — makes steel lines glow)");
        Debug.Log("");
        Debug.Log("  VIGNETTE:");
        Debug.Log("    Intensity: 0.35");
        Debug.Log("    Smoothness: 0.4");
        Debug.Log("    Color: Black");
        Debug.Log("");
        Debug.Log("  COLOR ADJUSTMENTS:");
        Debug.Log("    Post Exposure: -0.5 (darker overall)");
        Debug.Log("    Contrast: 15");
        Debug.Log("    Saturation: -20 (desaturated ash world)");
        Debug.Log("    Color Filter: #FFE8D0 (warm sepia tint from ash-filtered light)");
        Debug.Log("");
        Debug.Log("  FILM GRAIN:");
        Debug.Log("    Type: Medium");
        Debug.Log("    Intensity: 0.15");
        Debug.Log("");
        Debug.Log("  EXPOSURE:");
        Debug.Log("    Mode: Fixed");
        Debug.Log("    Fixed Exposure: 8.5");
        Debug.Log("");
        Debug.Log("  FOG (if using HDRP Volumetric Fog):");
        Debug.Log("    Enable Volumetric Fog: true");
        Debug.Log("    Fog Attenuation Distance: 50");
        Debug.Log("    Base Height: 0");
        Debug.Log("    Max Height: 15");
        Debug.Log("    Color: #0D0D14 (dark blue-black)");

        EditorUtility.DisplayDialog("Post-Processing Volume Created",
            "A Volume with an empty profile has been added to the scene.\n\n" +
            "To finish setup:\n" +
            "1. Select 'PostProcessing_Volume' in the Hierarchy\n" +
            "2. Click 'Add Override' in the Inspector\n" +
            "3. Add: Bloom, Vignette, Color Adjustments, Film Grain, Exposure\n" +
            "4. See Console log for exact values to enter\n\n" +
            "Profile saved to:\n" + profilePath,
            "OK");

        Selection.activeGameObject = volumeObj;
    }
}
#endif
