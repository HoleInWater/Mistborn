using UnityEngine;

/// <summary>
/// An NPC that can be spoken to.
/// </summary>
public class NPCInteractable : MonoBehaviour, IInteractable
{
    public string npcName = "Mistfallen";
    public DialogueData dialogue;

    public void Interact(GameObject player)
    {
        if (dialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }

    public string GetInteractionPrompt()
    {
        return $"Press [F] to talk to {npcName}";
    }

    public bool CanInteract() => dialogue != null;
}
