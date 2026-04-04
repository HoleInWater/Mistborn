/* WeaponDurability.cs
 *
 * BotW-inspired weapon durability system.
 *
 * From GDD Section 6 (Combat System):
 *   "Potentially a durability system similar to breath of the wild,
 *    to encourage using different types of weapons."
 *
 * Every weapon has a durability value. Each hit reduces it. When it hits
 * zero, the weapon breaks (with a dramatic final hit that deals bonus damage).
 * This forces the player to improvise — grab enemy weapons, push metal
 * objects at enemies, use the environment. Fits the Ashwalker fantasy of
 * resourceful street fighting.
 *
 * Attach to any weapon GameObject (child of the player or enemy).
 */

using UnityEngine;
using System;

public class WeaponDurability : MonoBehaviour
{
    [Header("Durability")]
    public float maxDurability = 100f;
    public float currentDurability;
    [Tooltip("Durability lost per hit")]
    public float durabilityPerHit = 5f;
    [Tooltip("Durability lost per heavy attack")]
    public float durabilityPerHeavyHit = 12f;
    [Tooltip("Durability lost per block")]
    public float durabilityPerBlock = 8f;

    [Header("Breaking")]
    [Tooltip("Bonus damage multiplier on the final hit before breaking")]
    public float breakingBlowMultiplier = 2f;
    [Tooltip("Does the weapon shatter into physics fragments?")]
    public bool shatterOnBreak = true;
    public int shardCount = 4;

    [Header("Visual Feedback")]
    [Tooltip("Weapon starts flashing when durability is below this percentage")]
    [Range(0f, 0.5f)]
    public float warningThreshold = 0.2f;
    public float flashSpeed = 3f;

    [Header("Audio")]
    public AudioClip crackSound;
    public AudioClip breakSound;

    public event Action OnWeaponBroken;
    public event Action<float> OnDurabilityChanged; // 0-1 normalized

    private Renderer weaponRenderer;
    private Color originalColor;
    private bool isBroken;
    private bool isWarning;
    private float warningTimer;

    void Start()
    {
        currentDurability = maxDurability;
        weaponRenderer = GetComponentInChildren<Renderer>();
        if (weaponRenderer != null && weaponRenderer.material != null)
        {
            if (weaponRenderer.material.HasProperty("_BaseColor"))
                originalColor = weaponRenderer.material.GetColor("_BaseColor");
            else
                originalColor = weaponRenderer.material.color;
        }
    }

    void Update()
    {
        if (isBroken) return;

        // Warning flash when low durability
        float ratio = currentDurability / maxDurability;
        if (ratio <= warningThreshold && ratio > 0f)
        {
            if (!isWarning)
            {
                isWarning = true;
                // Play crack sound once
                if (crackSound != null)
                    AudioSource.PlayClipAtPoint(crackSound, transform.position);
            }

            warningTimer += Time.deltaTime;
            float flash = Mathf.Abs(Mathf.Sin(warningTimer * flashSpeed));
            if (weaponRenderer != null && weaponRenderer.material != null)
            {
                Color flashColor = Color.Lerp(originalColor, Color.red, flash * 0.4f);
                if (weaponRenderer.material.HasProperty("_BaseColor"))
                    weaponRenderer.material.SetColor("_BaseColor", flashColor);
                else
                    weaponRenderer.material.color = flashColor;
            }
        }
    }

    /// <summary>
    /// Called when the weapon hits something. Returns true if the weapon just broke.
    /// The final hit before breaking deals bonus damage (breaking blow).
    /// </summary>
    public bool TakeDurabilityHit(bool isHeavy = false)
    {
        if (isBroken) return true;

        float loss = isHeavy ? durabilityPerHeavyHit : durabilityPerHit;
        currentDurability = Mathf.Max(0f, currentDurability - loss);

        OnDurabilityChanged?.Invoke(currentDurability / maxDurability);

        if (currentDurability <= 0f)
        {
            BreakWeapon();
            return true;
        }

        return false;
    }

    /// <summary>Called when the weapon blocks an attack.</summary>
    public bool TakeBlockDurability()
    {
        if (isBroken) return true;

        currentDurability = Mathf.Max(0f, currentDurability - durabilityPerBlock);
        OnDurabilityChanged?.Invoke(currentDurability / maxDurability);

        if (currentDurability <= 0f)
        {
            BreakWeapon();
            return true;
        }

        return false;
    }

    /// <summary>Get the damage multiplier — bonus damage on the breaking blow.</summary>
    public float GetDamageMultiplier()
    {
        if (currentDurability <= 0f)
            return breakingBlowMultiplier; // breaking blow!
        return 1f;
    }

    void BreakWeapon()
    {
        isBroken = true;

        // Sound
        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        else
            SoundManager.Instance?.PlayImpactSound();

        // Camera shake
        CameraShakeManager.Instance?.Shake(0.2f, 0.15f);

        // Notification
        string weaponName = gameObject.name;
        var weaponData = GetComponentInParent<EquipmentManager>();
        if (weaponData != null && weaponData.Equipped != null)
            weaponName = weaponData.Equipped.weaponName;
        NotificationSystem.Instance?.ShowNotification($"{weaponName} broke!");

        // Shatter into fragments
        if (shatterOnBreak)
            SpawnShards();

        // Event
        OnWeaponBroken?.Invoke();

        // Unequip — the EquipmentManager should handle this
        var equipment = GetComponentInParent<EquipmentManager>();
        if (equipment != null)
            equipment.UnequipWeapon();

        // Destroy the weapon object
        Destroy(gameObject, 0.1f);
    }

    void SpawnShards()
    {
        for (int i = 0; i < shardCount; i++)
        {
            var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shard.name = "WeaponShard";
            shard.transform.position = transform.position + UnityEngine.Random.insideUnitSphere * 0.2f;
            shard.transform.localScale = new Vector3(
                UnityEngine.Random.Range(0.03f, 0.08f),
                UnityEngine.Random.Range(0.03f, 0.08f),
                UnityEngine.Random.Range(0.03f, 0.08f));
            shard.transform.rotation = UnityEngine.Random.rotation;

            // Color matches the weapon
            var shardRend = shard.GetComponent<Renderer>();
            if (shardRend != null && weaponRenderer != null)
                shardRend.material = weaponRenderer.material;

            // Physics — fly outward
            var rb = shard.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.AddForce(UnityEngine.Random.insideUnitSphere * 3f, ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.Impulse);

            // Metal layer — pushable/pullable shards!
            int metalLayer = LayerMask.NameToLayer("Metal");
            if (metalLayer >= 0) shard.layer = metalLayer;

            var target = shard.AddComponent<MetallurgicTarget>();
            target.canBePushed = true;
            target.canBePulled = true;

            // Self-destruct after 30 seconds
            Destroy(shard, 30f);
        }
    }

    /// <summary>Repair the weapon by a given amount.</summary>
    public void Repair(float amount)
    {
        if (isBroken) return;
        currentDurability = Mathf.Min(currentDurability + amount, maxDurability);
        OnDurabilityChanged?.Invoke(currentDurability / maxDurability);

        if (currentDurability / maxDurability > warningThreshold)
        {
            isWarning = false;
            // Reset color
            if (weaponRenderer != null && weaponRenderer.material != null)
            {
                if (weaponRenderer.material.HasProperty("_BaseColor"))
                    weaponRenderer.material.SetColor("_BaseColor", originalColor);
                else
                    weaponRenderer.material.color = originalColor;
            }
        }
    }

    public float GetDurabilityRatio() => maxDurability > 0f ? currentDurability / maxDurability : 0f;
    public bool IsBroken() => isBroken;
}
