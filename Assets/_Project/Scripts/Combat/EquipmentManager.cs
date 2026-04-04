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
    private Transform     _attachPoint;   // cached: rightHandBone if found, else transform

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

        }

        if (startingWeapon != null)
            EquipWeapon(startingWeapon);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void EquipWeapon(WeaponData data)
    {
        if (data == null) return;
        UnequipWeapon();
        _equipped = data;

        // Cache attach point — used here and in LateUpdate
        _attachPoint = rightHandBone != null ? rightHandBone : transform;

        if (data.prefab != null)
        {
            _weaponInstance = Instantiate(data.prefab, _attachPoint, false);

            // _heldLocalRot is now a fine-tune correction applied on top of the
            // forward-facing world rotation set in LateUpdate — not raw Euler angles.
            // Zero means no correction needed (weapon tip already faces +Z on the mesh).
            _heldLocalRot = data.handRotationOffset;
            _heldLocalPos = (data.handPositionOffset == Vector3.zero)
                ? new Vector3(0f, 0f, 0.1f)   // default: push grip slightly forward
                : data.handPositionOffset;

            _weaponInstance.transform.localPosition = _heldLocalPos;

            // Kinematic Rigidbody — follows animation, can push non-kinematic objects.
            // Discrete mode: ContinuousSpeculative jitters kinematic bodies in animated rigs.
            foreach (var rb in _weaponInstance.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            Rigidbody weaponRb = _weaponInstance.GetComponent<Rigidbody>()
                              ?? _weaponInstance.AddComponent<Rigidbody>();
            weaponRb.isKinematic = true;
            weaponRb.collisionDetectionMode = CollisionDetectionMode.Discrete;

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

        NotificationSystem.Instance?.ShowNotification($"Equipped: {data.weaponName}");
    }

    void LateUpdate()
    {
        if (_weaponInstance == null || _attachPoint == null) return;

        if (_weaponInstance.transform.parent != _attachPoint)
            _weaponInstance.transform.SetParent(_attachPoint, false);

        // Position: hand-bone local space
        _weaponInstance.transform.localPosition = _heldLocalPos;

        // Rotation: point the weapon's +Z (tip) in the player's facing direction.
        // _heldLocalRot is a mesh-specific correction on top — zero by default.
        // This keeps the blade pointing forward regardless of hand bone orientation,
        // eliminating body intersection caused by Euler guesswork.
        _weaponInstance.transform.rotation =
            Quaternion.LookRotation(transform.forward, transform.up)
            * Quaternion.Euler(_heldLocalRot);
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

        // Let gravity take it — must disable kinematic first or gravity has no effect
        Rigidbody rb = _weaponInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic            = false;
            rb.useGravity             = true;
            rb.linearDamping          = 0.3f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Detach from the hand so it's no longer re-seated each LateUpdate
        _weaponInstance.transform.SetParent(null);

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
