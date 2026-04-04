using UnityEngine;

/// <summary>
/// Attached to an equipped weapon instance by EquipmentManager.
/// Follows the hand bone each physics step via Rigidbody.MovePosition so the
/// weapon is a real physics object (hits walls, pushes barrels, reacts to
/// Metallurgic forces) while still staying in Ember's hand under normal conditions.
///
/// If an external force drives velocity above DropVelocityThreshold (e.g. a
/// Steel Push), the weapon is released and flies free.
/// </summary>
public class WeaponFollow : MonoBehaviour
{
    [HideInInspector] public Transform       target;           // hand bone
    [HideInInspector] public Vector3         positionOffset;
    [HideInInspector] public Vector3         rotationOffset;
    [HideInInspector] public EquipmentManager owner;

    [Tooltip("Velocity (m/s) at which an external push drops the weapon from the hand.")]
    public float dropVelocityThreshold = 4f;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // If a force (Metallurgy, collision) drove velocity above threshold, drop it
        if (_rb.linearVelocity.magnitude > dropVelocityThreshold)
        {
            owner?.DropWeapon();
            return;
        }

        // Track the hand bone — weapon stays in grip under normal conditions
        Vector3    worldPos = target.TransformPoint(positionOffset);
        Quaternion worldRot = target.rotation * Quaternion.Euler(rotationOffset);

        _rb.MovePosition(worldPos);
        _rb.MoveRotation(worldRot);
    }
}
