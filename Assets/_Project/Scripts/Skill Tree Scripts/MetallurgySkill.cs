using UnityEngine;
using System.Collections.Generic;

// This attribute allows you to create new skill assets via the right-click menu
[CreateAssetMenu(fileName = "NewMetallurgySkill", menuName = "Skill Tree/Metallurgy Skill")]
public class MetallurgySkill : ScriptableObject 
{
    [Header("General Info")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Unlock Requirements")]
    public int skillPointCost;
    public List<MetallurgySkill> prerequisites; // Other skills that must be unlocked first

    [Header("Metallurgy Stats")]
    public int metalCost;
    public float cooldown;
    public float damage;

    [Header("State")]
    public bool isUnlocked;
    public bool isPassive;

    [Header("Metal Type")]
    public MetalType metalType;

    public enum MetalType
    {
        Steel,
        Iron,
        Pewter,
        Tin,
        Zinc,
        Brass,
        Copper,
        Bronze,
        Oraculum,
        Maloraculum,
        Gold,
        Electrum,
        Aluminum,
        Duralumin,
        Bendalloy,
        Cadmium,
        Chromium,
        Nicrosil
    }


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
    }
}
