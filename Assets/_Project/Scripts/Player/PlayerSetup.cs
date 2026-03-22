// PlayerSetup.cs
// Automatically adds missing components to the Player at runtime.
// This ensures PlayerController, PlayerCamera, and AllomanticSight exist.
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        // Find the Player GameObject (tagged "Player")
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("PlayerSetup: No GameObject tagged 'Player' found!");
            return;
        }

        // Ensure PlayerCamera component exists
        PlayerCamera playerCamera = player.GetComponent<PlayerCamera>();
        if (playerCamera == null)
        {
            playerCamera = player.AddComponent<PlayerCamera>();
            Debug.Log("Added PlayerCamera component to Player");
        }

        // Ensure AllomanticSight component exists
        AllomanticSight allomanticSight = player.GetComponent<AllomanticSight>();
        if (allomanticSight == null)
        {
            allomanticSight = player.AddComponent<AllomanticSight>();
            Debug.Log("Added AllomanticSight component to Player");
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

        // Assign metalLayer for AllomanticSight (should be set in inspector, but we can default to everything)
        if (allomanticSight != null && allomanticSight.metalLayer == 0)
        {
            // Default to all layers (should be configured in inspector)
            allomanticSight.metalLayer = ~0; // Everything
            Debug.Log("AllomanticSight metalLayer defaulted to all layers");
        }

        // Assign playerCamera reference in AllomanticSight if not set
        if (allomanticSight != null && allomanticSight.playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                allomanticSight.playerCamera = mainCamera;
            }
        }
    }
}