/* MetalDatabase.cs
 *
 * Complete database of all 16 Metallurgic metals with their properties,
 * burn rates, lore descriptions, and gameplay parameters.
 *
 * This is the single source of truth for metal data. All metal scripts
 * should reference this instead of hardcoding values.
 *
 * Sources: Ashwalker Adventure Game (MAG), Coppermind, PHYSICS-MATH-BOOK.md
 */

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MetalDatabase", menuName = "Ashwalker/Metal Database")]
public class MetalDatabase : ScriptableObject
{
    public static MetalDatabase Instance { get; private set; }

    [Header("Metal Definitions")]
    public List<MetalDefinition> metals = new List<MetalDefinition>();

    void OnEnable()
    {
        Instance = this;
        if (metals.Count == 0)
            PopulateDefaults();
    }

    public MetalDefinition GetMetal(string name)
    {
        return metals.Find(m => m.metalName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
    }

    public MetalDefinition GetMetal(MetallurgySkill.MetalType type)
    {
        return metals.Find(m => m.metalType == type);
    }

    public List<MetalDefinition> GetMetalsByCategory(MetalCategory category)
    {
        return metals.FindAll(m => m.category == category);
    }

    void PopulateDefaults()
    {
        metals = new List<MetalDefinition>
        {
            // ═══════════════════════════════════════════════════════════════
            // PHYSICAL METALS (External/Internal)
            // ═══════════════════════════════════════════════════════════════

            new MetalDefinition
            {
                metalName = "Steel",
                metalType = MetallurgySkill.MetalType.Steel,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Iron + Carbon",
                density = MetallurgyPhysicsFormulas.DENSITY_STEEL,
                magBurnDuration = MetallurgyConstants.SteelBurnDuration,
                drainRate = MetallurgyConstants.SteelDrainRate,
                isInstant = false,
                sparkbloodName = "Launcher",
                description = "Push on nearby metals. Launch coins as projectiles, deflect incoming metal, fly by pushing off anchored metals below.",
                loreNote = "Steel Pushing is the most versatile offensive Metallurgic ability. The force acts along the line between the Metallurgist's chest and the metal's center of mass.",
                gameplayTip = "Push off coins on the ground to launch yourself upward. Push off heavy anchored metals (lampposts, steel beams) for maximum height.",
                lineColor = new Color(0.3f, 0.5f, 1f, 0.8f),
                hudColor = new Color(0.4f, 0.6f, 1f),
            },

            new MetalDefinition
            {
                metalName = "Iron",
                metalType = MetallurgySkill.MetalType.Iron,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_IRON,
                magBurnDuration = MetallurgyConstants.IronBurnDuration,
                drainRate = MetallurgyConstants.IronDrainRate,
                isInstant = false,
                sparkbloodName = "Hauler",
                description = "Pull on nearby metals. Yank weapons from enemies' hands, reel yourself toward anchored metals, catch metal objects mid-air.",
                loreNote = "Iron Pulling follows the same physics as Steel Pushing but in reverse. The Hauler moves toward the metal if it's heavier, or the metal flies to the Hauler if it's lighter.",
                gameplayTip = "Pull yourself toward overhead metal anchors for fast traversal. Pull enemy weapons to disarm them.",
                lineColor = new Color(0.3f, 0.5f, 1f, 0.6f),
                hudColor = new Color(0.5f, 0.5f, 0.8f),
            },

            new MetalDefinition
            {
                metalName = "Pewter",
                metalType = MetallurgySkill.MetalType.Pewter,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Tin + Lead",
                density = MetallurgyPhysicsFormulas.DENSITY_PEWTER,
                magBurnDuration = MetallurgyConstants.PewterBurnDuration,
                drainRate = MetallurgyConstants.PewterDrainRate,
                isInstant = false,
                sparkbloodName = "Thug (Ironhide)",
                description = "Enhance physical abilities. Increased strength, speed, endurance, and durability. Survive falls and injuries that would kill a normal person.",
                loreNote = "Pewter burning suppresses pain and fatigue but doesn't eliminate them. When Pewter runs out, all suppressed fatigue hits at once — the 'Pewter drag crash' can be lethal.",
                gameplayTip = "Burn Pewter before big falls. The superhero landing deals AOE damage to nearby enemies. Don't let your Pewter run out mid-fight.",
                lineColor = new Color(0.6f, 0.6f, 0.6f, 0.5f),
                hudColor = new Color(0.7f, 0.7f, 0.7f),
            },

            new MetalDefinition
            {
                metalName = "Tin",
                metalType = MetallurgySkill.MetalType.Tin,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_TIN,
                magBurnDuration = MetallurgyConstants.TinBurnDuration,
                drainRate = MetallurgyConstants.TinDrainRate,
                isInstant = false,
                sparkbloodName = "Keensense",
                description = "Enhance all five senses. See in the dark, hear whispers from far away, detect danger before it arrives. Risk sensory overload from loud noises or bright lights.",
                loreNote = "Tin enhancement is proportional to burn rate. Flaring Tin gives near-superhuman perception but makes the Metallurgist vulnerable to sensory overload — a flashbang can incapacitate a Keensense.",
                gameplayTip = "Use Tin to scout ahead and detect enemies through walls (Metallurgic sight). Be careful around explosions and bright lights.",
                lineColor = new Color(0.8f, 0.8f, 0.9f, 0.5f),
                hudColor = new Color(0.8f, 0.85f, 0.9f),
            },

            // ═══════════════════════════════════════════════════════════════
            // MENTAL METALS (External/Internal)
            // ═══════════════════════════════════════════════════════════════

            new MetalDefinition
            {
                metalName = "Zinc",
                metalType = MetallurgySkill.MetalType.Zinc,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_ZINC,
                magBurnDuration = MetallurgyConstants.ZincBurnDuration,
                drainRate = MetallurgyConstants.ZincDrainRate,
                isInstant = false,
                sparkbloodName = "Igniter",
                description = "Riot (inflame) the emotions of others. Enrage enemies into reckless attacks, incite crowds to violence, amplify fear or courage.",
                loreNote = "Rioting doesn't create emotions — it amplifies existing ones. A calm person can have their anger stoked, but a truly happy person can't be made to feel rage from nothing.",
                gameplayTip = "Riot an enemy's aggression to make them attack recklessly (lower defense). Riot fear to make them flee.",
                lineColor = new Color(1f, 0.8f, 0.3f, 0.6f),
                hudColor = new Color(1f, 0.85f, 0.4f),
            },

            new MetalDefinition
            {
                metalName = "Brass",
                metalType = MetallurgySkill.MetalType.Brass,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Copper + Zinc",
                density = MetallurgyPhysicsFormulas.DENSITY_BRASS,
                magBurnDuration = MetallurgyConstants.BrassBurnDuration,
                drainRate = MetallurgyConstants.BrassDrainRate,
                isInstant = false,
                sparkbloodName = "Queller",
                description = "Soothe (dampen) the emotions of others. Calm aggressive enemies, pacify guards, remove fear from allies, manipulate negotiations.",
                loreNote = "Soothing is subtle — a skilled Queller can calm a guard without them even noticing. Less skilled Quellers leave their targets feeling unnaturally numb.",
                gameplayTip = "Soothe a guard's suspicion to sneak past. Soothe an ally's fear to boost their morale in combat.",
                lineColor = new Color(0.8f, 0.6f, 0.2f, 0.6f),
                hudColor = new Color(0.85f, 0.7f, 0.3f),
            },

            new MetalDefinition
            {
                metalName = "Copper",
                metalType = MetallurgySkill.MetalType.Copper,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_COPPER,
                magBurnDuration = MetallurgyConstants.CopperBurnDuration,
                drainRate = MetallurgyConstants.CopperDrainRate,
                isInstant = false,
                sparkbloodName = "Smoker (Coppercloud)",
                description = "Create a coppercloud that hides Metallurgic pulses from Bronze seekers. Allies within the cloud are invisible to Metallurgic detection.",
                loreNote = "A Coppercloud is essential for any crew — without one, a Bronze Seeker can detect every Metallurgist in the area. Grimshaw runs Darius's crew's coppercloud.",
                gameplayTip = "Burn Copper when using other metals near enemies who might have Bronze seekers. Protects the whole team.",
                lineColor = new Color(0.6f, 0.4f, 0.2f, 0.5f),
                hudColor = new Color(0.7f, 0.5f, 0.3f),
            },

            new MetalDefinition
            {
                metalName = "Bronze",
                metalType = MetallurgySkill.MetalType.Bronze,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Copper + Tin",
                density = MetallurgyPhysicsFormulas.DENSITY_BRONZE,
                magBurnDuration = MetallurgyConstants.BronzeBurnDuration,
                drainRate = MetallurgyConstants.BronzeDrainRate,
                isInstant = false,
                sparkbloodName = "Seeker",
                description = "Detect other Metallurgists who are burning metals. See their Metallurgic pulses as vibrations, identify which metal they're burning.",
                loreNote = "Bronze seeking can identify the specific metal being burned by the 'flavor' of the pulse. Physical metals pulse differently from mental metals. A coppercloud blocks this entirely.",
                gameplayTip = "Use Bronze to detect hidden Metallurgists. Each metal type produces a unique pulse pattern.",
                lineColor = new Color(0.7f, 0.4f, 0.1f, 0.6f),
                hudColor = new Color(0.75f, 0.5f, 0.2f),
            },

            // ═══════════════════════════════════════════════════════════════
            // TEMPORAL METALS (External/Internal)
            // ═══════════════════════════════════════════════════════════════

            new MetalDefinition
            {
                metalName = "Gold",
                metalType = MetallurgySkill.MetalType.Gold,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_GOLD,
                magBurnDuration = MetallurgyConstants.GoldBurnDuration,
                drainRate = MetallurgyConstants.GoldDrainRate,
                isInstant = false,
                sparkbloodName = "Augur",
                description = "See a vision of your past self — who you might have been if you'd made different choices. Deeply unsettling, rarely useful in combat.",
                loreNote = "Gold burning shows a 'gold shadow' — an alternate version of yourself. Most Metallurgists find the experience deeply disturbing and avoid burning Gold.",
                gameplayTip = "Gold has limited combat use but can reveal story information about your character's past.",
                lineColor = new Color(1f, 0.8f, 0.0f, 0.6f),
                hudColor = new Color(1f, 0.85f, 0.1f),
            },

            new MetalDefinition
            {
                metalName = "Electrum",
                metalType = MetallurgySkill.MetalType.Electrum,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Gold + Silver",
                density = MetallurgyPhysicsFormulas.DENSITY_ELECTRUM,
                magBurnDuration = MetallurgyConstants.ElectrumBurnDuration,
                drainRate = MetallurgyConstants.ElectrumDrainRate,
                isInstant = false,
                sparkbloodName = "Oracle",
                description = "See shadows of your own possible futures. Creates multiple future-self images showing different outcomes of your next few seconds.",
                loreNote = "Electrum is the 'poor man's Oraculum' — it counters Oraculum by flooding the Oraculum-burner's vision with false shadows. This is how Ember defeats Zane.",
                gameplayTip = "Burn Electrum to counter enemy Oraculum users. Also provides a brief dodge window in combat.",
                lineColor = new Color(0.9f, 0.9f, 0.7f, 0.6f),
                hudColor = new Color(0.95f, 0.95f, 0.75f),
            },

            new MetalDefinition
            {
                metalName = "Cadmium",
                metalType = MetallurgySkill.MetalType.Cadmium,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_CADMIUM,
                magBurnDuration = MetallurgyConstants.CadmiumBurnDuration,
                drainRate = MetallurgyConstants.CadmiumDrainRate,
                isInstant = false,
                sparkbloodName = "Pulser",
                description = "Create a bubble of slowed time around yourself. Everything inside the bubble moves slowly relative to the outside world.",
                loreNote = "Cadmium bubbles are spherical and have a sharp boundary. Objects crossing the boundary experience a sudden time shift. Useful for waiting out sieges or skipping forward in time.",
                gameplayTip = "Use Cadmium to slow enemies inside the bubble while you act at normal speed outside it.",
                lineColor = new Color(0.5f, 0.5f, 0.8f, 0.6f),
                hudColor = new Color(0.6f, 0.6f, 0.85f),
            },

            new MetalDefinition
            {
                metalName = "Bendalloy",
                metalType = MetallurgySkill.MetalType.Bendalloy,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Cadmium + Lead + Tin + Bismuth",
                density = MetallurgyPhysicsFormulas.DENSITY_BENDALLOY,
                magBurnDuration = MetallurgyConstants.BendalloyBurnDuration,
                drainRate = MetallurgyConstants.BendalloyDrainRate,
                isInstant = false,
                sparkbloodName = "Slider",
                description = "Create a bubble of accelerated time around yourself. Everything inside the bubble moves faster relative to the outside world.",
                loreNote = "Bendalloy is extremely rare and expensive. Wayne uses it constantly in Era 2, creating speed bubbles for rapid combat and quick conversations.",
                gameplayTip = "Pop a speed bubble to get multiple attacks in while enemies outside are frozen. Great for healing or reloading mid-fight.",
                lineColor = new Color(0.3f, 0.8f, 0.5f, 0.6f),
                hudColor = new Color(0.4f, 0.85f, 0.55f),
            },

            // ═══════════════════════════════════════════════════════════════
            // ENHANCEMENT METALS (External/Internal)
            // ═══════════════════════════════════════════════════════════════

            new MetalDefinition
            {
                metalName = "Aluminum",
                metalType = MetallurgySkill.MetalType.Aluminum,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_ALUMINUM,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                sparkbloodName = "Aluminum Gnat",
                description = "Instantly purge ALL of your metal reserves. Wipes every metal you're currently storing. Mostly useless for a Ashwalker.",
                loreNote = "Aluminum has no Metallurgic use for a Sparkblood — they can only burn away their own Aluminum, gaining nothing. As a Ashwalker ability it's a last resort to prevent an enemy from stealing your metals via Chromium.",
                gameplayTip = "Almost never useful. Only burn if an enemy Leecher (Chromium) is about to touch you and you'd rather lose everything than let them drain you.",
                lineColor = new Color(0.9f, 0.9f, 0.9f, 0.4f),
                hudColor = new Color(0.9f, 0.9f, 0.9f),
            },

            new MetalDefinition
            {
                metalName = "Duralumin",
                metalType = MetallurgySkill.MetalType.Duralumin,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Aluminum + Copper",
                density = MetallurgyPhysicsFormulas.DENSITY_DURALUMIN,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                sparkbloodName = "Duralumin Gnat",
                description = "Burn your entire reserve of another metal in one massive burst. Enormously amplified effect but instantly exhausts the paired metal.",
                loreNote = "Duralumin + Steel = one coin fired at insane velocity. Duralumin + Pewter = brief moment of incredible strength. The cost is losing ALL of the paired metal at once.",
                gameplayTip = "Pair with Steel for a devastating ranged attack. Pair with Pewter for an emergency power boost. Always have backup vials.",
                lineColor = new Color(0.85f, 0.85f, 0.9f, 0.5f),
                hudColor = new Color(0.85f, 0.85f, 0.95f),
            },

            new MetalDefinition
            {
                metalName = "Chromium",
                metalType = MetallurgySkill.MetalType.Chromium,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = MetallurgyPhysicsFormulas.DENSITY_CHROMIUM,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                sparkbloodName = "Leecher",
                description = "Strip another Metallurgist's metal reserves by touching them. Drains all their metals on contact.",
                loreNote = "Chromium requires physical contact. In combat, this means getting dangerously close to another Metallurgist. Extremely effective against Ashwalker who rely on multiple metals.",
                gameplayTip = "Touch an enemy Metallurgist to drain all their metals. High risk, high reward — you have to get close.",
                lineColor = new Color(0.5f, 0.5f, 0.55f, 0.5f),
                hudColor = new Color(0.6f, 0.6f, 0.65f),
            },

            new MetalDefinition
            {
                metalName = "Nicrosil",
                metalType = MetallurgySkill.MetalType.Nicrosil,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Chromium + Nickel + Silicon",
                density = MetallurgyPhysicsFormulas.DENSITY_NICROSIL,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                sparkbloodName = "Nicroburst",
                description = "Supercharge another Metallurgist's current burn by touching them. Their active metal fires at Duralumin-level intensity.",
                loreNote = "Nicrosil is the external version of Duralumin. Instead of boosting your own metals, you boost someone else's. Can be used offensively — supercharging an enemy's Tin causes sensory overload.",
                gameplayTip = "Touch an ally to supercharge their active metal. Or touch an enemy burning Tin to overload their senses.",
                lineColor = new Color(0.6f, 0.6f, 0.65f, 0.5f),
                hudColor = new Color(0.65f, 0.65f, 0.7f),
            },
        };
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DATA STRUCTURES
// ═══════════════════════════════════════════════════════════════════════════

[System.Serializable]
public class MetalDefinition
{
    [Header("Identity")]
    public string metalName;
    public MetallurgySkill.MetalType metalType;
    public MetalCategory category;
    public MetalQuadrant quadrant;
    public string alloyOf;
    public string sparkbloodName;

    [Header("Physical Properties")]
    public float density;              // kg/m3

    [Header("Burn Rates (MAG Canon)")]
    public float magBurnDuration;      // seconds at full reserve
    public float drainRate;            // reserve units per second
    public bool isInstant;             // Aluminum, Duralumin, Chromium, Nicrosil

    [Header("Descriptions")]
    [TextArea(2, 4)] public string description;
    [TextArea(2, 4)] public string loreNote;
    [TextArea(1, 3)] public string gameplayTip;

    [Header("Visual")]
    public Color lineColor;            // Metallurgic sight line color
    public Color hudColor;             // Metal reserve bar color
}

public enum MetalCategory
{
    Physical,
    Mental,
    Temporal,
    Enhancement
}

public enum MetalQuadrant
{
    InternalPush,
    InternalPull,
    ExternalPush,
    ExternalPull
}
