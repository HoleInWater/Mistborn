using UnityEngine;

/// <summary>
/// Environmental hazards: lava, spikes, mist zones, ash clouds.
/// Each hazard damages or debuffs the player on contact.
/// </summary>
public class EnvironmentalHazards : MonoBehaviour
{
    public enum HazardType { Lava, Spikes, DeepMist, AshCloud, PoisonGas, Acid }

    [Header("Settings")]
    public HazardType hazardType = HazardType.Lava;
    public float damagePerSecond = 10f;
    public float debuffDuration = 3f;
    public bool instantKill = false;

    [Header("Effects")]
    public GameObject hazardEffectPrefab;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (instantKill)
        {
            IDamageable hp = other.GetComponentInParent<IDamageable>();
            hp?.TakeDamage(99999f);
            return;
        }

        IDamageable health = other.GetComponentInParent<IDamageable>();
        if (health != null)
            health.TakeDamage(damagePerSecond * Time.deltaTime);

        ApplyHazardEffect(other.gameObject);
    }

    void ApplyHazardEffect(GameObject player)
    {
        switch (hazardType)
        {
            case HazardType.DeepMist:
                // Reduce visibility, increase Tin overload
                CameraShakeManager.Instance?.Shake(0.1f, 0.03f);
                break;
            case HazardType.AshCloud:
                // Slow player
                BasicPlayerMove pm = player.GetComponent<BasicPlayerMove>();
                if (pm != null) pm.externalSpeedMultiplier = 0.7f;
                break;
            case HazardType.PoisonGas:
                // DOT + screen tint
                CameraShakeManager.Instance?.Shake(0.2f, 0.05f);
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Remove debuffs
        BasicPlayerMove pm = other.GetComponent<BasicPlayerMove>();
        if (pm != null) pm.externalSpeedMultiplier = 1f;
    }
}
