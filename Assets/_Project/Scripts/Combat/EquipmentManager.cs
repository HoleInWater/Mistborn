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
    private Vector3       _heldLocalPos;
    private Vector3       _heldLocalRot;

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
            // worldPositionStays = false keeps local-space transform from the prefab,
            // then we override with the grip offsets below.
            _weaponInstance = Instantiate(data.prefab, attachPoint, false);

            // Always 90° on X so the weapon points forward along the hand bone.
            // Keep Y/Z from WeaponData so per-weapon tweaks still work.
            Vector3 rot = data.handRotationOffset;
            rot.x = 90f;

            _heldLocalPos = data.handPositionOffset;
            _heldLocalRot = rot;

            _weaponInstance.transform.localPosition    = _heldLocalPos;
            _weaponInstance.transform.localEulerAngles = _heldLocalRot;

            // Kinematic Rigidbody — follows animation, can push non-kinematic objects,
            // but is not dragged around by physics forces itself.
            // ContinuousSpeculative prevents the weapon from tunneling through geometry.
            foreach (var rb in _weaponInstance.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
            Rigidbody weaponRb = _weaponInstance.GetComponent<Rigidbody>()
                              ?? _weaponInstance.AddComponent<Rigidbody>();
            weaponRb.isKinematic = true;
            weaponRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            // Ignore collision with every player collider so it never blocks movement
            Collider[] playerCols = GetComponentsInChildren<Collider>();
            foreach (var wc in _weaponInstance.GetComponentsInChildren<Collider>(true))
            {
                wc.enabled = true;
                foreach (var pc in playerCols)
                    Physics.IgnoreCollision(wc, pc, true);
            }

            // Default layer so Allomancy raycasts can detect metal weapons
            SetLayerRecursive(_weaponInstance, 0);
        }

        Debug.Log($"[EquipmentManager] Equipped: {data.weaponName}  " +
                  $"dmg={data.damage} range={data.attackRange} speed={data.attackSpeed}/s");
        NotificationSystem.Instance?.ShowNotification($"Equipped: {data.weaponName}");
    }

    // Belt-and-suspenders: re-parent and re-seat the weapon every frame so physics,
    // animation, or late-running scripts can never knock it out of the hand.
    void LateUpdate()
    {
        if (_weaponInstance == null || rightHandBone == null) return;

        if (_weaponInstance.transform.parent != rightHandBone)
            _weaponInstance.transform.SetParent(rightHandBone, false);

        _weaponInstance.transform.localPosition    = _heldLocalPos;
        _weaponInstance.transform.localEulerAngles = _heldLocalRot;
    }

    public void UnequipWeapon()
    {
        if (_weaponInstance != null) Destroy(_weaponInstance);
        _weaponInstance = null;
        _equipped       = null;
    }

    /// <summary>
    /// Called by WeaponFollow when an external force (Allomancy, hard collision) pushes
    /// the weapon beyond the drop threshold. The weapon flies free as a physics object
    /// and becomes a pickup the player can retrieve.
    /// </summary>
    public void DropWeapon()
    {
        if (_weaponInstance == null || _equipped == null) return;

        // Detach from follow system
        WeaponFollow follow = _weaponInstance.GetComponent<WeaponFollow>();
        if (follow != null) Destroy(follow);

        // Let gravity take it
        Rigidbody rb = _weaponInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity    = true;
            rb.linearDamping = 0.3f;
        }

        // Add a WeaponPickup so the player can grab it off the floor
        if (_weaponInstance.GetComponent<WeaponPickup>() == null)
        {
            var pickup = _weaponInstance.AddComponent<WeaponPickup>();
            pickup.weaponData = _equipped;
        }

        _weaponInstance = null;
        _equipped       = null;

        NotificationSystem.Instance?.ShowNotification("Weapon knocked away!");
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
