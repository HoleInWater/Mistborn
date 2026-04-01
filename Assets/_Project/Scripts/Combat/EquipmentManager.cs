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
    [Tooltip("Weapon the player holds at spawn. " +
             "To remove: right-click the field in the Inspector and choose 'Set to None', or just clear it.")]
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
        // Auto-find right hand bone — humanoid rig first, then name search fallback
        if (rightHandBone == null)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.isHuman)
                rightHandBone = anim.GetBoneTransform(HumanBodyBones.RightHand);

            // Fallback: search all transforms by common hand bone names (works on Generic rigs)
            if (rightHandBone == null)
                rightHandBone = FindBoneByName(transform,
                    "RightHand", "Hand_R", "hand_r", "mixamorig:RightHand",
                    "Bip01_R_Hand", "RHand", "Right Hand");

            if (rightHandBone == null)
                Debug.LogWarning("[EquipmentManager] Could not find right hand bone — " +
                                 "assign it manually in the Inspector.");
            else
                Debug.Log($"[EquipmentManager] Hand bone found: {rightHandBone.name}");
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

        // Use hand bone if found; fall back to player root so weapon always attaches somewhere
        Transform attachPoint = rightHandBone != null ? rightHandBone : transform;
        Debug.Log($"[EquipmentManager] Attaching '{data.weaponName}' to '{attachPoint.name}'");

        if (data.prefab != null)
        {
            _weaponInstance = Instantiate(data.prefab, attachPoint);
            _weaponInstance.transform.localPosition    = data.handPositionOffset;
            _weaponInstance.transform.localEulerAngles = data.handRotationOffset;

            // Make all existing Rigidbodies kinematic so they move with the animation
            foreach (var rb in _weaponInstance.GetComponentsInChildren<Rigidbody>(true))
                rb.isKinematic = true;

            // Add a root kinematic Rigidbody if none exists — lets the weapon push
            // dynamic objects in the world while still following the hand animation.
            Rigidbody weaponRb = _weaponInstance.GetComponent<Rigidbody>();
            if (weaponRb == null)
            {
                weaponRb = _weaponInstance.AddComponent<Rigidbody>();
                weaponRb.isKinematic = true;
            }

            // Ignore collision between every weapon collider and every player collider
            // so the weapon never blocks the player's movement or deforms their capsule.
            Collider[] playerCols = GetComponentsInChildren<Collider>();
            foreach (var wc in _weaponInstance.GetComponentsInChildren<Collider>(true))
            {
                wc.enabled = true;
                foreach (var pc in playerCols)
                    Physics.IgnoreCollision(wc, pc, true);
            }

            // Keep on Ignore Raycast so OverlapSphere attack detection skips the weapon mesh
            SetLayerRecursive(_weaponInstance, LayerMask.NameToLayer("Ignore Raycast"));
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

    static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    static Transform FindBoneByName(Transform root, params string[] names)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            foreach (string n in names)
                if (t.name.Equals(n, System.StringComparison.OrdinalIgnoreCase))
                    return t;
        return null;
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
