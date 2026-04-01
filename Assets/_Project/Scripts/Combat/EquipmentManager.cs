using UnityEngine;

/// <summary>
/// Manages the player's equipped weapon.
/// - Attaches weapon prefab to the right hand bone on equip
/// - Exposes stat overrides consumed by PlayerCombat
/// - Auto-added by PlayerAutoSetup via [PlayerComponent]
/// </summary>
[PlayerComponent("Combat", order: 8)]
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Starting Weapon")]
    [Tooltip("Assign a WeaponData asset. Leave blank to start unarmed.")]
    public WeaponData startingWeapon;

    [Header("Hand Bone (auto-found if blank)")]
    [Tooltip("Right hand bone for weapon attachment. Auto-found from humanoid Animator.")]
    public Transform rightHandBone;

    private WeaponData    _equipped;
    private GameObject    _weaponInstance;

    public WeaponData Equipped  => _equipped;
    public bool       HasWeapon => _equipped != null && _equipped.type != WeaponData.WeaponType.Unarmed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find right hand bone from humanoid rig
        if (rightHandBone == null)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.isHuman)
                rightHandBone = anim.GetBoneTransform(HumanBodyBones.RightHand);

            if (rightHandBone == null)
                Debug.LogWarning("[EquipmentManager] Could not find right hand bone — " +
                                 "weapon prefab won't attach. Assign it manually in the Inspector.");
        }

        if (startingWeapon != null)
            EquipWeapon(startingWeapon);
        else
            Debug.Log("[EquipmentManager] No starting weapon — player is unarmed.");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void EquipWeapon(WeaponData data)
    {
        if (data == null) return;
        UnequipWeapon();
        _equipped = data;

        if (data.prefab != null && rightHandBone != null)
        {
            _weaponInstance = Instantiate(data.prefab, rightHandBone);
            _weaponInstance.transform.localPosition    = data.handPositionOffset;
            _weaponInstance.transform.localEulerAngles = data.handRotationOffset;
        }

        Debug.Log($"[EquipmentManager] Equipped: {data.weaponName}  " +
                  $"dmg={data.damage} range={data.attackRange} speed={data.attackSpeed}/s");
        NotificationSystem.Instance?.ShowNotification($"Equipped: {data.weaponName}");
    }

    public void UnequipWeapon()
    {
        if (_weaponInstance != null) Destroy(_weaponInstance);
        _weaponInstance = null;
        _equipped       = null;
    }

    // ── Stat providers — PlayerCombat calls these ─────────────────────────────

    public float GetDamage(float fallback)               => HasWeapon ? _equipped.damage               : fallback;
    public float GetRange(float fallback)                => HasWeapon ? _equipped.attackRange           : fallback;
    public float GetAttackCooldown(float fallback)       => HasWeapon ? _equipped.AttackCooldown        : fallback;
    public float GetHeavyMultiplier(float fallback)      => HasWeapon ? _equipped.heavyMultiplier       : fallback;
    public float GetKnockback(float fallback)            => HasWeapon ? _equipped.knockbackForce        : fallback;
    public float GetHeavyKnockback(float fallback)       => HasWeapon ? _equipped.heavyKnockbackForce   : fallback;
    public bool  IsWeaponMetal()                         => !HasWeapon || _equipped.isMetal;
}
