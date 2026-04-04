/* FactionSystem.cs
 *
 * Tracks the player's reputation with all factions in the Ashen Dominion.
 *
 * Factions (Era 1):
 *   - Darius's Crew (the rebellion)
 *   - House Venture (most powerful noble house)
 *   - House Hasting
 *   - House Elariel
 *   - House Lekal
 *   - House Tekiel
 *   - The Iron Clergy (Prelates + Sentinels)
 *   - The Lowborn Underground (broader rebellion network)
 *   - The Diocese of Inquisition
 *   - Independent Merchants
 *
 * Reputation ranges from -100 (hostile) to +100 (allied).
 * Actions in the game shift reputation with multiple factions simultaneously.
 * Example: stealing from House Venture decreases Venture rep but increases
 * Lowborn Underground rep.
 *
 * Faction reputation affects:
 *   - NPC dialogue options
 *   - Quest availability
 *   - Shop prices
 *   - Guard behavior (attack on sight vs. ignore)
 *   - Story branching
 */

using UnityEngine;
using System;
using System.Collections.Generic;

public class FactionSystem : MonoBehaviour
{
    public static FactionSystem Instance { get; private set; }

    [Header("Factions")]
    public List<Faction> factions = new List<Faction>();

    public event Action<string, float, float> OnReputationChanged; // factionId, oldRep, newRep

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (factions.Count == 0)
            InitializeDefaultFactions();
    }

    void InitializeDefaultFactions()
    {
        factions = new List<Faction>
        {
            new Faction
            {
                factionId = "dariuss_crew",
                displayName = "Darius's Crew",
                description = "The rebellion's inner circle. Darius, Tormund, Lysander, Grimshaw, Harlan, and the rest. They plan to overthrow the Ashen King.",
                reputation = 20f, // start slightly positive — you're recruited
                hostileThreshold = -50f,
                alliedThreshold = 60f,
                color = new Color(0.3f, 0.5f, 1f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "lowborn_underground", modifier = 0.5f },
                    new FactionRelationship { factionId = "steel_ministry", modifier = -0.8f },
                    new FactionRelationship { factionId = "house_venture", modifier = -0.6f },
                }
            },

            new Faction
            {
                factionId = "house_venture",
                displayName = "House Venture",
                description = "The most powerful noble house in Cinderhold. Led by Lord Strafen Ashford. Controls much of the city's economy and has the ear of the Ashen King.",
                reputation = -10f,
                hostileThreshold = -40f,
                alliedThreshold = 50f,
                color = new Color(0.8f, 0.6f, 0.1f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "dariuss_crew", modifier = -0.7f },
                    new FactionRelationship { factionId = "steel_ministry", modifier = 0.3f },
                    new FactionRelationship { factionId = "house_hasting", modifier = -0.3f },
                    new FactionRelationship { factionId = "house_elariel", modifier = -0.4f },
                }
            },

            new Faction
            {
                factionId = "house_hasting",
                displayName = "House Hasting",
                description = "A powerful noble house, rivals of House Venture. Known for their Metallurgists and military strength.",
                reputation = 0f,
                hostileThreshold = -40f,
                alliedThreshold = 50f,
                color = new Color(0.6f, 0.2f, 0.2f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "house_venture", modifier = -0.4f },
                    new FactionRelationship { factionId = "house_elariel", modifier = 0.2f },
                }
            },

            new Faction
            {
                factionId = "house_elariel",
                displayName = "House Elariel",
                description = "A noble house known for its spy network and information trade. Shan Elariel is a Ashwalker.",
                reputation = 0f,
                hostileThreshold = -40f,
                alliedThreshold = 50f,
                color = new Color(0.2f, 0.5f, 0.3f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "house_venture", modifier = -0.5f },
                    new FactionRelationship { factionId = "house_hasting", modifier = 0.2f },
                }
            },

            new Faction
            {
                factionId = "house_lekal",
                displayName = "House Lekal",
                description = "A mid-tier noble house with strong Valdris connections. More moderate than most houses.",
                reputation = 5f,
                hostileThreshold = -40f,
                alliedThreshold = 50f,
                color = new Color(0.4f, 0.4f, 0.6f),
                relationships = new List<FactionRelationship>()
            },

            new Faction
            {
                factionId = "house_tekiel",
                displayName = "House Tekiel",
                description = "A noble house focused on trade and commerce. Controls shipping along the Canal.",
                reputation = 0f,
                hostileThreshold = -40f,
                alliedThreshold = 50f,
                color = new Color(0.5f, 0.4f, 0.2f),
                relationships = new List<FactionRelationship>()
            },

            new Faction
            {
                factionId = "steel_ministry",
                displayName = "The Iron Clergy",
                description = "The Ashen King's bureaucracy and enforcement arm. Prelates track all noble contracts. The Diocese of Inquisition hunts Metallurgists.",
                reputation = -30f,
                hostileThreshold = -20f, // they're suspicious of everyone
                alliedThreshold = 70f,   // nearly impossible to befriend
                color = new Color(0.5f, 0.15f, 0.15f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "dariuss_crew", modifier = -1f },
                    new FactionRelationship { factionId = "lowborn_underground", modifier = -1f },
                    new FactionRelationship { factionId = "canton_inquisition", modifier = 0.8f },
                }
            },

            new Faction
            {
                factionId = "canton_inquisition",
                displayName = "The Diocese of Inquisition",
                description = "The most feared branch of the Iron Clergy. Iron Sentinels answer only to the Ashen King. They hunt and kill rogue Metallurgists.",
                reputation = -50f,
                hostileThreshold = -10f, // hostile to almost everyone
                alliedThreshold = 90f,   // effectively impossible
                color = new Color(0.3f, 0.05f, 0.05f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "steel_ministry", modifier = 0.5f },
                    new FactionRelationship { factionId = "dariuss_crew", modifier = -1f },
                }
            },

            new Faction
            {
                factionId = "lowborn_underground",
                displayName = "Lowborn Underground",
                description = "The broader network of lowborn resistance. Safe houses, smuggling routes, and information networks. Darius's crew is the tip of the spear, but the Underground is the body.",
                reputation = 10f,
                hostileThreshold = -60f,
                alliedThreshold = 40f,
                color = new Color(0.4f, 0.35f, 0.3f),
                relationships = new List<FactionRelationship>
                {
                    new FactionRelationship { factionId = "dariuss_crew", modifier = 0.7f },
                    new FactionRelationship { factionId = "steel_ministry", modifier = -0.8f },
                }
            },

            new Faction
            {
                factionId = "merchants",
                displayName = "Independent Merchants",
                description = "Traders, shopkeepers, and craftsmen who serve both nobles and lowborn. Neutral by necessity — they sell to whoever pays.",
                reputation = 0f,
                hostileThreshold = -70f,
                alliedThreshold = 30f,
                color = new Color(0.6f, 0.5f, 0.3f),
                relationships = new List<FactionRelationship>()
            },
        };
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public float GetReputation(string factionId)
    {
        var faction = factions.Find(f => f.factionId == factionId);
        return faction != null ? faction.reputation : 0f;
    }

    public FactionStanding GetStanding(string factionId)
    {
        var faction = factions.Find(f => f.factionId == factionId);
        if (faction == null) return FactionStanding.Neutral;

        if (faction.reputation <= faction.hostileThreshold) return FactionStanding.Hostile;
        if (faction.reputation >= faction.alliedThreshold)  return FactionStanding.Allied;
        if (faction.reputation > 20f) return FactionStanding.Friendly;
        if (faction.reputation < -20f) return FactionStanding.Unfriendly;
        return FactionStanding.Neutral;
    }

    /// <summary>
    /// Change reputation with a faction. Also propagates to related factions.
    /// Example: +10 with Darius's Crew → +5 with Lowborn Underground, -8 with Iron Clergy
    /// </summary>
    public void ChangeReputation(string factionId, float amount, bool propagate = true)
    {
        var faction = factions.Find(f => f.factionId == factionId);
        if (faction == null) return;

        float oldRep = faction.reputation;
        faction.reputation = Mathf.Clamp(faction.reputation + amount, -100f, 100f);

        OnReputationChanged?.Invoke(factionId, oldRep, faction.reputation);

        // Check for standing change notifications
        FactionStanding oldStanding = GetStandingFromValue(oldRep, faction);
        FactionStanding newStanding = GetStandingFromValue(faction.reputation, faction);
        if (oldStanding != newStanding)
        {
            NotificationSystem.Instance?.ShowNotification(
                $"{faction.displayName}: now {newStanding}");
        }

        // Propagate to related factions
        if (propagate)
        {
            foreach (var rel in faction.relationships)
            {
                float propagatedAmount = amount * rel.modifier;
                if (Mathf.Abs(propagatedAmount) > 0.1f)
                    ChangeReputation(rel.factionId, propagatedAmount, false); // don't cascade
            }
        }
    }

    FactionStanding GetStandingFromValue(float rep, Faction faction)
    {
        if (rep <= faction.hostileThreshold) return FactionStanding.Hostile;
        if (rep >= faction.alliedThreshold)  return FactionStanding.Allied;
        if (rep > 20f) return FactionStanding.Friendly;
        if (rep < -20f) return FactionStanding.Unfriendly;
        return FactionStanding.Neutral;
    }

    /// <summary>Get the shop price multiplier based on faction reputation.</summary>
    public float GetPriceMultiplier(string factionId)
    {
        float rep = GetReputation(factionId);
        // -100 rep = 2x prices, 0 = 1x, +100 = 0.5x
        return Mathf.Lerp(2f, 0.5f, (rep + 100f) / 200f);
    }

    /// <summary>Should guards from this faction attack on sight?</summary>
    public bool IsHostile(string factionId)
    {
        return GetStanding(factionId) == FactionStanding.Hostile;
    }

    /// <summary>Can the player access this faction's quests?</summary>
    public bool CanAccessQuests(string factionId)
    {
        var standing = GetStanding(factionId);
        return standing == FactionStanding.Neutral
            || standing == FactionStanding.Friendly
            || standing == FactionStanding.Allied;
    }

    // ── Save/Load ────────────────────────────────────────────────────────────

    public Dictionary<string, float> GetSaveData()
    {
        var data = new Dictionary<string, float>();
        foreach (var f in factions)
            data[f.factionId] = f.reputation;
        return data;
    }

    public void LoadSaveData(Dictionary<string, float> data)
    {
        foreach (var kvp in data)
        {
            var faction = factions.Find(f => f.factionId == kvp.Key);
            if (faction != null) faction.reputation = kvp.Value;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DATA STRUCTURES
// ═══════════════════════════════════════════════════════════════════════════

[System.Serializable]
public class Faction
{
    public string factionId;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public float reputation;
    public float hostileThreshold;
    public float alliedThreshold;
    public Color color;
    public List<FactionRelationship> relationships;
}

[System.Serializable]
public class FactionRelationship
{
    [Tooltip("The related faction ID")]
    public string factionId;
    [Tooltip("How much reputation change propagates (-1 to 1). Positive = allied, negative = opposed")]
    public float modifier;
}

public enum FactionStanding
{
    Hostile,     // -100 to hostile threshold — attack on sight
    Unfriendly,  // hostile threshold to -20 — won't help, may refuse service
    Neutral,     // -20 to +20 — default interaction
    Friendly,    // +20 to allied threshold — better prices, more quests
    Allied       // allied threshold to +100 — full access, special content
}
