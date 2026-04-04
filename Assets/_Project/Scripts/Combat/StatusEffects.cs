using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages active buffs and debuffs on any entity (player or enemy).
/// Metallurgy metals apply status effects: Pewter=StrengthBuff, Tin=SensoryOverload,
/// Zinc=RiotDebuff, Brass=SootheDebuff, etc.
/// </summary>
[PlayerComponent("Combat", order: 60)]
public class StatusEffects : MonoBehaviour
{
    [System.Serializable]
    public class StatusEffect
    {
        public string id;
        public string displayName;
        public EffectType type;
        public float duration;
        public float elapsed;
        public float magnitude;
        public bool isStackable;
        public int stacks;

        public float RemainingTime => Mathf.Max(0, duration - elapsed);
        public float NormalizedTime => duration > 0 ? elapsed / duration : 1f;
        public bool IsExpired => elapsed >= duration;
    }

    public enum EffectType
    {
        // Buffs
        StrengthBuff, SpeedBuff, DamageResistance, Regeneration,
        SenseEnhancement, Invisibility, TimeAcceleration,
        // Debuffs
        Slow, Weakness, DamageOverTime, SensoryOverload,
        Fear, Confusion, Burning, Frozen, Stunned,
        // Metallurgy-specific
        RiotInfluence, SootheInfluence, CopperHidden, BronzeRevealed,
        OraculumSight, DuraluminPrimed, CompoundingActive
    }

    [Header("Active Effects")]
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    [Header("Immunity")]
    public List<EffectType> immunities = new List<EffectType>();

    public System.Action<StatusEffect> OnEffectApplied;
    public System.Action<StatusEffect> OnEffectRemoved;

    private BasicPlayerMove playerMove;
    private PlayerHealth playerHealth;

    void Start()
    {
        playerMove = GetComponent<BasicPlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            effect.elapsed += Time.deltaTime;

            ApplyTickEffect(effect);

            if (effect.IsExpired)
            {
                RemoveEffectInternal(effect);
                activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Apply a new status effect. Stacks or refreshes if already active.
    /// </summary>
    public void ApplyEffect(string id, string name, EffectType type, float duration, float magnitude = 1f, bool stackable = false)
    {
        if (immunities.Contains(type)) return;

        StatusEffect existing = activeEffects.Find(e => e.id == id);
        if (existing != null)
        {
            if (stackable)
            {
                existing.stacks++;
                existing.magnitude += magnitude * 0.5f;
            }
            existing.elapsed = 0f; // Refresh duration
            existing.duration = duration;
            return;
        }

        StatusEffect effect = new StatusEffect
        {
            id = id,
            displayName = name,
            type = type,
            duration = duration,
            elapsed = 0f,
            magnitude = magnitude,
            isStackable = stackable,
            stacks = 1
        };

        activeEffects.Add(effect);
        ApplyOneShotEffect(effect);
        OnEffectApplied?.Invoke(effect);
        NotificationSystem.Instance?.ShowNotification($"{name} applied");
    }

    /// <summary>
    /// Remove a specific effect by ID.
    /// </summary>
    public void RemoveEffect(string id)
    {
        StatusEffect effect = activeEffects.Find(e => e.id == id);
        if (effect != null)
        {
            RemoveEffectInternal(effect);
            activeEffects.Remove(effect);
        }
    }

    public void ClearAllEffects()
    {
        foreach (var e in activeEffects) RemoveEffectInternal(e);
        activeEffects.Clear();
    }

    // ── Effect Application ───────────────────────────────────────────────

    void ApplyOneShotEffect(StatusEffect effect)
    {
        switch (effect.type)
        {
            case EffectType.SpeedBuff:
                if (playerMove != null) playerMove.externalSpeedMultiplier *= (1f + effect.magnitude * 0.5f);
                break;
            case EffectType.Slow:
                if (playerMove != null) playerMove.externalSpeedMultiplier *= (1f - effect.magnitude * 0.3f);
                break;
            case EffectType.Stunned:
                if (playerMove != null) playerMove.enabled = false;
                break;
        }
    }

    void ApplyTickEffect(StatusEffect effect)
    {
        switch (effect.type)
        {
            case EffectType.DamageOverTime:
                playerHealth?.TakeDamage(effect.magnitude * Time.deltaTime);
                break;
            case EffectType.Regeneration:
                playerHealth?.Heal(effect.magnitude * Time.deltaTime);
                break;
            case EffectType.Burning:
                playerHealth?.TakeDamage(effect.magnitude * 2f * Time.deltaTime);
                break;
        }
    }

    void RemoveEffectInternal(StatusEffect effect)
    {
        switch (effect.type)
        {
            case EffectType.SpeedBuff:
            case EffectType.Slow:
                if (playerMove != null) playerMove.externalSpeedMultiplier = 1f;
                break;
            case EffectType.Stunned:
                if (playerMove != null) playerMove.enabled = true;
                break;
        }
        OnEffectRemoved?.Invoke(effect);
    }

    // ── Queries ──────────────────────────────────────────────────────────

    public bool HasEffect(string id) => activeEffects.Exists(e => e.id == id);
    public bool HasEffectType(EffectType type) => activeEffects.Exists(e => e.type == type);
    public StatusEffect GetEffect(string id) => activeEffects.Find(e => e.id == id);
    public int GetActiveEffectCount() => activeEffects.Count;

    // ── Convenience: Metallurgy Effects ───────────────────────────────────

    public void ApplyPewterBuff(float duration) =>
        ApplyEffect("pewter_strength", "Pewter Strength", EffectType.StrengthBuff, duration, 2f);
    public void ApplyTinOverload(float intensity) =>
        ApplyEffect("tin_overload", "Sensory Overload", EffectType.SensoryOverload, 3f, intensity);
    public void ApplyRiot(float duration) =>
        ApplyEffect("zinc_riot", "Rioted", EffectType.RiotInfluence, duration, 1.5f);
    public void ApplySoothe(float duration) =>
        ApplyEffect("brass_soothe", "Soothed", EffectType.SootheInfluence, duration, 1f);
    public void ApplyOraculumSight(float duration) =>
        ApplyEffect("oraculum_sight", "Oraculum Vision", EffectType.OraculumSight, duration, 1f);
}
