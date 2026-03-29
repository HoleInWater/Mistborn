using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Metal vial system — Mistborn carry vials of dissolved metals to replenish reserves.
/// Lore: Each vial contains specific metal flakes suspended in alcohol.
/// Drinking a vial restores that metal's reserve. Metal purity matters — impure metals
/// cause illness (detected by Tin-enhanced taste).
/// </summary>
[PlayerComponent("Allomancy Support", order: 40)]
public class MetalVialSystem : MonoBehaviour
{
    [System.Serializable]
    public class MetalVial
    {
        public string vialId;
        public AllomancySkill.MetalType metalType;
        public float metalAmount = 50f;
        public float purity = 1f; // 0-1, impure metals cause damage
        public int quantity = 1;
    }

    [Header("Vial Inventory")]
    public List<MetalVial> vials = new List<MetalVial>();
    public int maxVials = 10;

    [Header("Drinking")]
    public float drinkDuration = 1f;
    public float impurityDamage = 20f;
    public float impurityThreshold = 0.7f;
    public KeyCode drinkKey = KeyCode.X;

    [Header("References")]
    public Allomancer allomancer;
    public Animator animator;

    private bool isDrinking = false;
    private float drinkTimer;

    void Start()
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        if (animator == null) animator = GetComponent<Animator>();

        // Start with a basic vial set
        if (vials.Count == 0)
            PopulateStarterVials();
    }

    void Update()
    {
        if (isDrinking)
        {
            drinkTimer -= Time.deltaTime;
            if (drinkTimer <= 0f) isDrinking = false;
            return;
        }

        // Drink vial for current metal
        if (Input.GetKeyDown(drinkKey) && allomancer != null)
        {
            DrinkVialForMetal(allomancer.GetCurrentMetal());
        }
    }

    void PopulateStarterVials()
    {
        AddVial(AllomancySkill.MetalType.Steel, 80f, 1f, 3);
        AddVial(AllomancySkill.MetalType.Iron, 80f, 1f, 3);
        AddVial(AllomancySkill.MetalType.Pewter, 80f, 1f, 2);
        AddVial(AllomancySkill.MetalType.Tin, 60f, 1f, 2);
    }

    public void AddVial(AllomancySkill.MetalType metal, float amount, float purity, int qty = 1)
    {
        MetalVial existing = vials.Find(v => v.metalType == metal && Mathf.Abs(v.purity - purity) < 0.01f);
        if (existing != null)
        {
            existing.quantity += qty;
            return;
        }

        if (vials.Count >= maxVials) return;

        vials.Add(new MetalVial
        {
            vialId = $"vial_{metal}_{Random.Range(1000, 9999)}",
            metalType = metal,
            metalAmount = amount,
            purity = purity,
            quantity = qty
        });
    }

    public bool DrinkVialForMetal(AllomancySkill.MetalType metal)
    {
        MetalVial vial = vials.Find(v => v.metalType == metal && v.quantity > 0);
        if (vial == null)
        {
            NotificationSystem.Instance?.ShowNotification($"No {metal} vials remaining!");
            return false;
        }

        return DrinkVial(vial);
    }

    public bool DrinkVial(MetalVial vial)
    {
        if (vial == null || vial.quantity <= 0 || isDrinking) return false;

        isDrinking = true;
        drinkTimer = drinkDuration;
        vial.quantity--;

        // Replenish metal reserve
        if (allomancer != null)
            allomancer.RefillMetal(vial.metalType, vial.metalAmount);

        // Check purity — impure metals cause damage
        if (vial.purity < impurityThreshold)
        {
            float damage = impurityDamage * (1f - vial.purity);
            IDamageable health = GetComponent<IDamageable>();
            health?.TakeDamage(damage);
            CameraShakeManager.Instance?.Shake(0.5f, 0.3f);
            NotificationSystem.Instance?.ShowNotification($"Impure {vial.metalType}! Took {damage:F0} damage.");

            // Tin-enhanced taste can detect this
            TutorialSystem.Instance?.ShowTip("impure_metal",
                "That metal tasted wrong! Use Tin to detect impurities before drinking.");
        }
        else
        {
            NotificationSystem.Instance?.ShowNotification($"Drank {vial.metalType} vial. Reserve restored.");
        }

        animator?.SetTrigger("Drink");

        // Remove empty vials
        if (vial.quantity <= 0)
            vials.Remove(vial);

        return true;
    }

    /// <summary>
    /// Drink ALL vials at once (pre-combat preparation).
    /// </summary>
    public void DrinkAllVials()
    {
        List<MetalVial> toDrink = new List<MetalVial>(vials);
        foreach (var vial in toDrink)
        {
            if (allomancer != null)
                allomancer.RefillMetal(vial.metalType, vial.metalAmount * vial.quantity);
            vial.quantity = 0;
        }
        vials.RemoveAll(v => v.quantity <= 0);
        NotificationSystem.Instance?.ShowNotification("Drank all metal vials!");
    }

    public int GetVialCount(AllomancySkill.MetalType metal)
    {
        MetalVial vial = vials.Find(v => v.metalType == metal);
        return vial != null ? vial.quantity : 0;
    }

    public int GetTotalVialCount()
    {
        int total = 0;
        foreach (var v in vials) total += v.quantity;
        return total;
    }

    public bool HasVial(AllomancySkill.MetalType metal)
    {
        return vials.Exists(v => v.metalType == metal && v.quantity > 0);
    }
}
