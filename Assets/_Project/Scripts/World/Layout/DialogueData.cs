using UnityEngine;

/// <summary>
/// Data container for a single dialogue sequence.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Mistborn/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    [TextArea(3, 10)]
    public string[] lines;
}
