using UnityEngine;

/// <summary>
/// Interface for any object the player can interact with (doors, levers, items, etc).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player interacts with this object.
    /// </summary>
    /// <param name="player">The player GameObject triggering the interaction.</param>
    void Interact(GameObject player);

    /// <summary>
    /// Returns the text prompt to show on the HUD (e.g. "Press [F] to Open").
    /// </summary>
    string GetInteractionPrompt();

    /// <summary>
    /// Returns true if the object is currently interactable.
    /// </summary>
    bool CanInteract();
}
