using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks the state of all active mission objectives.
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    private List<ObjectiveData> activeObjectives = new List<ObjectiveData>();
    private HashSet<string> completedObjectiveIDs = new HashSet<string>();

    public delegate void ObjectiveUpdate();
    public event ObjectiveUpdate OnObjectivesChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void AddObjective(ObjectiveData objective)
    {
        if (!activeObjectives.Contains(objective))
        {
            activeObjectives.Add(objective);
            OnObjectivesChanged?.Invoke();
            Debug.Log($"[OBJECTIVE] New Task: {objective.description}");
        }
    }

    public void CompleteObjective(string objectiveID)
    {
        ObjectiveData found = activeObjectives.Find(o => o.objectiveID == objectiveID);
        if (found != null)
        {
            activeObjectives.Remove(found);
            completedObjectiveIDs.Add(objectiveID);
            OnObjectivesChanged?.Invoke();
            Debug.Log($"[OBJECTIVE] Completed: {found.description}");
        }
    }

    public List<ObjectiveData> GetActiveObjectives() => activeObjectives;
    public bool IsCompleted(string id) => completedObjectiveIDs.Contains(id);
}
