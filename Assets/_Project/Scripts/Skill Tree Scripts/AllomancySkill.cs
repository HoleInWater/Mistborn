using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAllomancySkill", menuName = "Skill Tree/Allomancy Skill")]
public class AllomancySkill : ScriptableObject 
{
    [Header("General Info")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Unlock Requirements")]
    public int skillPointCost;
    public List<AllomancySkill> prerequisites;

    [Header("Allomancy Stats")]
    public int metalCost;
    public float cooldown;
    public float damage;

    [Header("State")]
    public bool isUnlocked;
    public bool isPassive;

    [Header("Metal Type")]
    public MetalType metalType;

    public bool CanUnlock(int availablePoints, float[] metalReserves)
    {
        if (isUnlocked) return false;
        if (skillPointCost > availablePoints) return false;
        if (metalReserves[(int)metalType] < metalCost) return false;
        
        foreach (var prereq in prerequisites)
        {
            if (!prereq.isUnlocked) return false;
        }
        return true;
    }

    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log($"Unlocked skill: {skillName}");
    }
}
