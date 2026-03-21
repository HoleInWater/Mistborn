using UnityEngine;
using System.Collections.Generic;

// This attribute allows you to create new skill assets via the right-click menu
[CreateAssetMenu(fileName = "NewMagicSkill", menuName = "Skill Tree/Magic Skill")]
public class MagicSkill : ScriptableObject 
{
    [Header("General Info")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Unlock Requirements")]
    public int skillPointCost;
    public List<MagicSkill> prerequisites; // Other skills that must be unlocked first

    [Header("Magic Stats")]
    public int manaCost;
    public float cooldown;
    public float damage;

    [Header("State")]
    public bool isUnlocked; // Tracks if the player has bought this skill
