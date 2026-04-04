using UnityEngine;

/// <summary>
/// Data container for a game objective.
/// </summary>
[CreateAssetMenu(fileName = "NewObjective", menuName = "Ashwalker/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveID;
    public string description;
    public bool isOptional;
}
