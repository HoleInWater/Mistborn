/* AllomanticAbilityData.cs
 *
 * ScriptableObject defining an individual Allomantic ability with all
 * its parameters. Used by the skill tree and ability system.
 *
 * Each metal can have multiple abilities (e.g., Steel has Push, Coin Shot,
 * Steel Bubble, Steeljump). This defines one ability's stats.
 */

using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Mistborn/Allomantic Ability")]
public class AllomanticAbilityData : ScriptableObject
{
    [Header("Identity")]
    public string abilityName;
    public string description;
    public AllomancySkill.MetalType requiredMetal;
    public Sprite icon;

    [Header("Activation")]
    public AbilityActivationType activationType;
    public KeyCode defaultKey;
    public float cooldown;
    public float duration; // 0 = instant or toggle

    [Header("Cost")]
    public float metalCostPerUse;
    public float metalCostPerSecond; // for sustained abilities
    public float staminaCost;

    [Header("Scaling")]
    public float basePower = 1f;
    public float flareMultiplier = 2.5f;
    public float skillScaling = 0.1f; // per skill level

    [Header("Requirements")]
    public int requiredSkillLevel;
    public string[] prerequisiteAbilities;

    [Header("Lore")]
    [TextArea(2, 4)]
    public string loreDescription;

    public float GetScaledPower(int skillLevel, float flareMult)
    {
        return basePower * (1f + skillLevel * skillScaling) * flareMult;
    }
}

public enum AbilityActivationType
{
    Press,          // Single press
    Hold,           // Hold key
    Toggle,         // Press to start, press again to stop
    Passive,        // Always active while burning
    Charged,        // Hold to charge, release to fire
    Instant         // One-shot, no duration
}
