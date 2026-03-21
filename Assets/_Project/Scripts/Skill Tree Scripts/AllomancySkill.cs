using UnityEngine;
using System.Collections.Generic;

// This attribute allows you to create new skill assets via the right-click menu
[CreateAssetMenu(fileName = "NewAllomancySkill", menuName = "Skill Tree/Allomancy Skill")]
public class AllomancySkill : ScriptableObject 
{
    [Header("General Info")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Unlock Requirements")]
    public int skillPointCost;
    public List<AllomancySkill> prerequisites; // Other skills that must be unlocked first

    [Header("Allomancy Stats")]
    public int manaCost;
    public float cooldown;
    public float damage;

    [Header("State")]
    public bool isUnlocked; // Tracks if the player has bought this skill
