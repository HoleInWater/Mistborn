using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Hemalurgy — the dark art of Ruin. Steals Allomantic/Feruchemical abilities
/// by driving metal spikes through one person and into another.
/// Each spike steals a specific power but loses something in the transfer.
/// Spiked individuals are vulnerable to Ruin's influence.
///
/// Lore: Steel Inquisitors have 11+ spikes. Koloss have 4 spikes (formerly human).
/// The linchpin spike (in the back) holds an Inquisitor together — remove it, instant death.
/// </summary>
public class Hemalurgy : MonoBehaviour
{
    [System.Serializable]
    public class HemalurgicSpike
    {
        public string spikeId;
        public SpikeMetal metal;
        public StolenPower stolenPower;
        public float powerStrength;
        public float decayFactor;
        public bool isLinchpin;
        public Transform bindPoint;
    }

    public enum SpikeMetal
    {
        Steel,      // Steals Physical Allomantic powers
        Iron,       // Steals Physical Feruchemical powers
        Pewter,     // Steals Physical Allomantic powers (alternate)
        Tin,        // Steals Mental Allomantic powers
        Bronze,     // Steals Mental Allomantic powers
        Brass,      // Steals Mental Feruchemical powers
        Copper,     // Steals Mental Feruchemical powers (alternate)
        Zinc,       // Steals Mental Allomantic powers
        Atium,      // Steals ANY power (god metal, most versatile)
        Gold        // Steals Feruchemical hybrid powers
    }

    public enum StolenPower
    {
        None,
        Allomancy_Steel, Allomancy_Iron, Allomancy_Pewter, Allomancy_Tin,
        Allomancy_Zinc, Allomancy_Brass, Allomancy_Copper, Allomancy_Bronze,
        Allomancy_Atium,
        Feruchemy_Steel, Feruchemy_Iron, Feruchemy_Pewter, Feruchemy_Tin,
        Feruchemy_Gold,
        Strength, Speed, Senses, MentalFortitude
    }

    [Header("Spikes")]
    public List<HemalurgicSpike> spikes = new List<HemalurgicSpike>();

    [Header("Ruin's Influence")]
    [Range(0f, 1f)] public float ruinInfluence = 0f;
    [Tooltip("Each spike increases vulnerability to Ruin")]
    public float influencePerSpike = 0.1f;
    public float maxInfluence = 0.8f;

    [Header("References")]
    public Allomancer allomancer;

    private int initialSpikeCount;

    void Start()
    {
        if (allomancer == null) allomancer = GetComponent<Allomancer>();
        initialSpikeCount = spikes.Count;
        ApplyStolenPowers();
        UpdateRuinInfluence();
    }

    /// <summary>
    /// Apply all stolen powers from current spikes to the host.
    /// </summary>
    void ApplyStolenPowers()
    {
        if (allomancer == null) return;

        foreach (var spike in spikes)
        {
            if (spike.stolenPower == StolenPower.None) continue;

            // Map stolen power to metal unlock
            AllomancySkill.MetalType? metal = GetMetalFromPower(spike.stolenPower);
            if (metal.HasValue)
            {
                allomancer.UnlockMetal(metal.Value);
            }
        }
    }

    /// <summary>
    /// Remove a spike. If it's the linchpin, the host dies.
    /// Returns the removed spike's power info.
    /// </summary>
    public HemalurgicSpike RemoveSpike(string spikeId)
    {
        HemalurgicSpike spike = spikes.Find(s => s.spikeId == spikeId);
        if (spike == null) return null;

        spikes.Remove(spike);
        UpdateRuinInfluence();

        if (spike.isLinchpin)
        {
            OnLinchpinRemoved();
            return spike;
        }

        // Remove the stolen power
        AllomancySkill.MetalType? metal = GetMetalFromPower(spike.stolenPower);
        if (metal.HasValue && allomancer != null)
        {
            // Only remove if no other spike grants the same power
            bool hasOtherSource = spikes.Exists(s => s.stolenPower == spike.stolenPower);
            if (!hasOtherSource)
                allomancer.unlockedMetals[(int)metal.Value] = false;
        }

        return spike;
    }

    /// <summary>
    /// Remove the nearest non-linchpin spike. Used by player interaction.
    /// </summary>
    public HemalurgicSpike RemoveAnySpike()
    {
        // Prefer non-linchpin first
        HemalurgicSpike spike = spikes.Find(s => !s.isLinchpin);
        if (spike == null && spikes.Count > 0)
            spike = spikes[0]; // Last resort: linchpin

        return spike != null ? RemoveSpike(spike.spikeId) : null;
    }

    void OnLinchpinRemoved()
    {

        // Kill the host
        IDamageable health = GetComponent<IDamageable>();
        if (health != null)
            health.TakeDamage(99999f);

        // Trigger events
        EventManager.TriggerEvent("LinchpinRemoved");
        CameraShakeManager.Instance?.Shake(1f, 0.5f);
    }

    void UpdateRuinInfluence()
    {
        ruinInfluence = Mathf.Clamp(spikes.Count * influencePerSpike, 0f, maxInfluence);
    }

    /// <summary>
    /// Drive a new spike into the host, granting a stolen power.
    /// </summary>
    public void AddSpike(HemalurgicSpike spike)
    {
        spikes.Add(spike);
        UpdateRuinInfluence();

        AllomancySkill.MetalType? metal = GetMetalFromPower(spike.stolenPower);
        if (metal.HasValue && allomancer != null)
            allomancer.UnlockMetal(metal.Value);

    }

    AllomancySkill.MetalType? GetMetalFromPower(StolenPower power)
    {
        switch (power)
        {
            case StolenPower.Allomancy_Steel: return AllomancySkill.MetalType.Steel;
            case StolenPower.Allomancy_Iron: return AllomancySkill.MetalType.Iron;
            case StolenPower.Allomancy_Pewter: return AllomancySkill.MetalType.Pewter;
            case StolenPower.Allomancy_Tin: return AllomancySkill.MetalType.Tin;
            case StolenPower.Allomancy_Zinc: return AllomancySkill.MetalType.Zinc;
            case StolenPower.Allomancy_Brass: return AllomancySkill.MetalType.Brass;
            case StolenPower.Allomancy_Copper: return AllomancySkill.MetalType.Copper;
            case StolenPower.Allomancy_Bronze: return AllomancySkill.MetalType.Bronze;
            case StolenPower.Allomancy_Atium: return AllomancySkill.MetalType.Atium;
            default: return null;
        }
    }

    // ── Public API ───────────────────────────────────────────────────────
    public int GetSpikeCount() => spikes.Count;
    public bool HasLinchpin() => spikes.Exists(s => s.isLinchpin);
    public float GetRuinInfluence() => ruinInfluence;
    public List<HemalurgicSpike> GetSpikes() => spikes;
}
