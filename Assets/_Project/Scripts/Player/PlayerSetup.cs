/* PlayerSetup.cs
 * 
 * PURPOSE:
 * This script automatically adds missing required components to the Player GameObject at runtime.
 * It ensures the Player has a PlayerController (BasicPlayerMove), PlayerCamera, MetallurgicSight,
 * and other essential components for the game to function properly.
 * 
 * HOW IT WORKS:
 * Uses the [RuntimeInitializeOnLoadMethod] attribute to run once after the scene loads.
 * Finds the Player by tag, then checks for each required component and adds it if missing.
 * It also copies camera references from the BasicPlayerMove script to the PlayerCamera script
 * to ensure proper camera setup.
 * 
 * KEY FEATURES:
 * - Automatically adds PlayerCamera and MetallurgicSight components if missing
 * - Copies camera transform/pivot references from existing PlayerController
 * - Sets default values for MetallurgicSight metal detection layer
 * - Assigns the main camera to MetallurgicSight if not already set
 * 
 * IMPORTANT NOTES:
 * This script is designed to be a one-time setup. Once components are added, they remain.
 * The Player must be tagged as "Player" in the Unity Editor for this script to find it.
 * Some default values are set, but should be configured in the Inspector for best results.
 */

using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    // This method runs automatically after the scene loads (due to the attribute above)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        // Find the Player GameObject (tagged "Player")
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            return;
        }

        // Ensure PlayerCamera component exists
        PlayerCamera playerCamera = player.GetComponent<PlayerCamera>();
        if (playerCamera == null)
        {
            playerCamera = player.AddComponent<PlayerCamera>();
        }

        // Ensure MetallurgicSight component exists
        MetallurgicSight metallurgicSight = player.GetComponent<MetallurgicSight>();
        if (metallurgicSight == null)
        {
            metallurgicSight = player.AddComponent<MetallurgicSight>();
        }

        // Copy camera references from BasicPlayerMove if they are assigned
        BasicPlayerMove playerMove = player.GetComponent<BasicPlayerMove>();
        if (playerMove != null && playerCamera != null)
        {
            // Since fields are public, we can access them directly
            if (playerCamera.cameraTransform == null && playerMove.cameraTransform != null)
            {
                playerCamera.cameraTransform = playerMove.cameraTransform;
            }
            if (playerCamera.cameraPivot == null && playerMove.cameraPivot != null)
            {
                playerCamera.cameraPivot = playerMove.cameraPivot;
            }
            if (playerCamera.collisionLayers == 0 && playerMove.collisionLayers != 0)
            {
                playerCamera.collisionLayers = playerMove.collisionLayers;
            }
        }

        // Assign metalLayer for MetallurgicSight (should be set in inspector, but we can default to everything)
        if (metallurgicSight != null && metallurgicSight.metalLayer == 0)
        {
            // Default to all layers (should be configured in inspector)
            metallurgicSight.metalLayer = ~0; // Everything
        }

        // Assign playerCamera reference in MetallurgicSight if not set
        if (metallurgicSight != null && metallurgicSight.playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                metallurgicSight.playerCamera = mainCamera;
            }
        }
    }
}