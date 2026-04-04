using UnityEngine;

/// <summary>
/// Loot drop that spawns on enemy death. Player picks up by walking over it.
/// Grants metal reserves, coins, health, or XP based on type.
/// </summary>
public class LootDrop : MonoBehaviour
{
    [Header("Loot Settings")]
    public LootType lootType;
    public int minAmount = 1;
    public int maxAmount = 5;
    public float pickupRadius = 1.5f;
    public float lifetime = 30f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    [Header("Metal Loot")]
    public MetallurgySkill.MetalType metalType = MetallurgySkill.MetalType.Steel;

    [Header("References")]
    public GameObject pickupEffect;

    public enum LootType { MetalVial, Coin, HealthPotion, SkillPoint, Crown }

    private float spawnY;
    private float spawnTime;

    void Start()
    {
        spawnY = transform.position.y;
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Bob up and down
        float bob = Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, spawnY + 0.5f + bob, transform.position.z);

        // Spin slowly
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PickUp(other.gameObject);
    }

    void PickUp(GameObject player)
    {
        int amount = Random.Range(minAmount, maxAmount + 1);

        switch (lootType)
        {
            case LootType.MetalVial:
                MetalVialSystem vials = player.GetComponent<MetalVialSystem>();
                if (vials != null) vials.AddVial(metalType, amount * 20f, 1f);
                else
                {
                    Metallurgist allo = player.GetComponent<Metallurgist>();
                    if (allo != null) allo.RefillMetal(metalType, amount * 20f);
                }
                NotificationSystem.Instance?.ShowPickup($"{metalType} Vial x{amount}");
                break;

            case LootType.Coin:
                CoinPouch pouch = player.GetComponent<CoinPouch>();
                if (pouch != null) pouch.AddCoins(amount * 5);
                NotificationSystem.Instance?.ShowPickup($"Coins x{amount * 5}");
                break;

            case LootType.HealthPotion:
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null) hp.Heal(amount * 25f);
                NotificationSystem.Instance?.ShowPickup($"Health +{amount * 25}");
                break;

            case LootType.SkillPoint:
                PlayerExperience xp = player.GetComponent<PlayerExperience>();
                if (xp != null) xp.AddXP(amount * 50f);
                NotificationSystem.Instance?.ShowPickup($"XP +{amount * 50}");
                break;

            case LootType.Crown:
                ShopSystem shop = FindObjectOfType<ShopSystem>();
                if (shop != null) shop.AddBoxings(amount * 10);
                NotificationSystem.Instance?.ShowPickup($"Crowns +{amount * 10}");
                break;
        }

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        SoundManager.Instance?.PlayNotification();
        Destroy(gameObject);
    }
}
