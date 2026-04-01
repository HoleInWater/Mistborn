using UnityEngine;

/// <summary>
/// Placed on every weapon visual while it is held by an EnemyAI.
/// SteelPush / IronPull read this to disarm the enemy when enough force is applied.
/// </summary>
public class HeldWeaponMarker : MonoBehaviour
{
    public EnemyAI owner;

    [Tooltip("Minimum push/pull force magnitude needed to disarm the enemy.")]
    public float disarmForceThreshold = 12f;

    /// <summary>
    /// Called by SteelPush / IronPull immediately after applying a force.
    /// If the force exceeds the threshold, the weapon is ripped from the enemy's hand.
    /// </summary>
    public void TryDisarm(float forceMagnitude)
    {
        if (owner == null) return;
        if (forceMagnitude >= disarmForceThreshold)
            owner.DisarmWeapon();
    }
}
