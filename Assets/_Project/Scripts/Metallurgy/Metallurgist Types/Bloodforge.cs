using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bloodforge — the dark art of Ruin. Steals Metallurgic/Storecrafted abilities
/// by driving metal spikes through one person and into another.
/// Each spike steals a specific power but loses something in the transfer.
/// Spiked individuals are vulnerable to Ruin's influence.
///
/// Lore: Steel Sentinels have 11+ spikes. Bloodbrute have 4 spikes (formerly human).
/// The linchpin spike (in the back) holds an Sentinel together — remove it, instant death.
/// </summary>
public class Bloodforge : MonoBehaviour
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
        Steel,      // Steals Physical Metallurgic powers
        Iron,       // Steals Physical Storecrafted powers
        Pewter,     // Steals Physical Metallurgic powers (alternate)
        Tin,        // Steals Mental Metallurgic powers
        Bronze,     // Steals Mental Metallurgic powers
        Brass,      // Steals Mental Storecrafted powers
        Copper,     // Steals Mental Storecrafted powers (alternate)
        Zinc,       // Steals Mental Metallurgic powers
        Oraculum,      // Steals ANY power (god metal, most versatile)
        Gold        // Steals Storecrafted hybrid powers
    }

    public enum StolenPower
    {
        None,
        Metallurgy_Steel, Metallurgy_Iron, Metallurgy_Pewter, Metallurgy_Tin,
        Metallurgy_Zinc, Metallurgy_Brass, Metallurgy_Copper, Metallurgy_Bronze,
        Metallurgy_Oraculum,
        Storecraft_Steel, Storecraft_Iron, Storecraft_Pewter, Storecraft_Tin,
        Storecraft_Gold,
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
    public Metallurgist metallurgist;

    private int initialSpikeCount;

    void Start()
    {
        if (metallurgist == null) metallurgist = GetComponent<Metallurgist>();
        initialSpikeCount = spikes.Count;
        ApplyStolenPowers();
        UpdateRuinInfluence();
    }

    /// <summary>
    /// Apply all stolen powers from current spikes to the host.
    /// </summary>
    void ApplyStolenPowers()
    {
        if (metallurgist == null) return;

        foreach (var spike in spikes)
        {
            if (spike.stolenPower == StolenPower.None) continue;

            // Map stolen power to metal unlock
            MetallurgySkill.MetalType? metal = GetMetalFromPower(spike.stolenPower);
            if (metal.HasValue)
            {
                metallurgist.UnlockMetal(metal.Value);
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
        MetallurgySkill.MetalType? metal = GetMetalFromPower(spike.stolenPower);
        if (metal.HasValue && metallurgist != null)
        {
            // Only remove if no other spike grants the same power
            bool hasOtherSource = spikes.Exists(s => s.stolenPower == spike.stolenPower);
            if (!hasOtherSource)
                metallurgist.unlockedMetals[(int)metal.Value] = false;
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

        MetallurgySkill.MetalType? metal = GetMetalFromPower(spike.stolenPower);
        if (metal.HasValue && metallurgist != null)
            metallurgist.UnlockMetal(metal.Value);

    }

    MetallurgySkill.MetalType? GetMetalFromPower(StolenPower power)
    {
        switch (power)
        {
            case StolenPower.Metallurgy_Steel: return MetallurgySkill.MetalType.Steel;
            case StolenPower.Metallurgy_Iron: return MetallurgySkill.MetalType.Iron;
            case StolenPower.Metallurgy_Pewter: return MetallurgySkill.MetalType.Pewter;
            case StolenPower.Metallurgy_Tin: return MetallurgySkill.MetalType.Tin;
            case StolenPower.Metallurgy_Zinc: return MetallurgySkill.MetalType.Zinc;
            case StolenPower.Metallurgy_Brass: return MetallurgySkill.MetalType.Brass;
            case StolenPower.Metallurgy_Copper: return MetallurgySkill.MetalType.Copper;
            case StolenPower.Metallurgy_Bronze: return MetallurgySkill.MetalType.Bronze;
            case StolenPower.Metallurgy_Oraculum: return MetallurgySkill.MetalType.Oraculum;
            default: return null;
        }
    }

    // ── Public API ───────────────────────────────────────────────────────
    public int GetSpikeCount() => spikes.Count;
    public bool HasLinchpin() => spikes.Exists(s => s.isLinchpin);
    public float GetRuinInfluence() => ruinInfluence;
    public List<HemalurgicSpike> GetSpikes() => spikes;
}
