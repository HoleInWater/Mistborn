using UnityEngine;

/// <summary>
/// ScriptableObject defining a weapon's stats and visual.
/// Create via: right-click in Project → Ashwalker → Weapon Data
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Ashwalker/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName = "Unarmed";
    [TextArea] public string description;
    public Sprite icon;
    public WeaponType type = WeaponType.Unarmed;

    [Header("Combat Stats")]
    public float damage          = 15f;
    public float attackRange     = 2.5f;
    [Tooltip("Attacks per second — sets the light attack cooldown")]
    public float attackSpeed     = 2.5f;
    public float heavyMultiplier = 2.5f;
    public float knockbackForce      = 8f;
    public float heavyKnockbackForce = 20f;

    [Header("Metallurgy Interaction")]
    [Tooltip("Metal weapons can be Steel Pushed / Iron Pulled. Obsidian/wood cannot.")]
    public bool isMetal = true;
    [Tooltip("Mass in kg — heavier = harder to push away")]
    public float mass = 1.5f;

    [Header("Visual — weapon model prefab")]
    [Tooltip("Prefab instantiated and parented to the right hand bone on equip")]
    public GameObject prefab;
    public Vector3 handPositionOffset = Vector3.zero;
    public Vector3 handRotationOffset = Vector3.zero;

    [Header("Economy")]
    public int buyPrice  = 100;
    public int sellPrice = 50;

    /// <summary>Seconds between light attacks.</summary>
    public float AttackCooldown => attackSpeed > 0f ? 1f / attackSpeed : 0.4f;

    public enum WeaponType
    {
        Unarmed,     // fists — no prefab needed
        Dagger,      // fast, short range — Ember's signature
        Sword,       // balanced — nobleman's dueling blade
        Spear,       // long reach, slower swing
        Mace,        // high knockback, slower
        Axe,         // high damage, slow
        BloodbruteBlade, // stolen from a Bloodbrute — massive damage, very slow
    }
}
