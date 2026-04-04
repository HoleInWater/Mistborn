/* MetalDatabase.cs
 *
 * Complete database of all 16 Allomantic metals with their properties,
 * burn rates, lore descriptions, and gameplay parameters.
 *
 * This is the single source of truth for metal data. All metal scripts
 * should reference this instead of hardcoding values.
 *
 * Sources: Mistborn Adventure Game (MAG), Coppermind, PHYSICS-MATH-BOOK.md
 */

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MetalDatabase", menuName = "Mistborn/Metal Database")]
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

    public MetalDefinition GetMetal(AllomancySkill.MetalType type)
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
                metalType = AllomancySkill.MetalType.Steel,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Iron + Carbon",
                density = AllomancyPhysicsFormulas.DENSITY_STEEL,
                magBurnDuration = AllomancyConstants.SteelBurnDuration,
                drainRate = AllomancyConstants.SteelDrainRate,
                isInstant = false,
                mistingName = "Coinshot",
                description = "Push on nearby metals. Launch coins as projectiles, deflect incoming metal, fly by pushing off anchored metals below.",
                loreNote = "Steel Pushing is the most versatile offensive Allomantic ability. The force acts along the line between the Allomancer's chest and the metal's center of mass.",
                gameplayTip = "Push off coins on the ground to launch yourself upward. Push off heavy anchored metals (lampposts, steel beams) for maximum height.",
                lineColor = new Color(0.3f, 0.5f, 1f, 0.8f),
                hudColor = new Color(0.4f, 0.6f, 1f),
            },

            new MetalDefinition
            {
                metalName = "Iron",
                metalType = AllomancySkill.MetalType.Iron,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_IRON,
                magBurnDuration = AllomancyConstants.IronBurnDuration,
                drainRate = AllomancyConstants.IronDrainRate,
                isInstant = false,
                mistingName = "Lurcher",
                description = "Pull on nearby metals. Yank weapons from enemies' hands, reel yourself toward anchored metals, catch metal objects mid-air.",
                loreNote = "Iron Pulling follows the same physics as Steel Pushing but in reverse. The Lurcher moves toward the metal if it's heavier, or the metal flies to the Lurcher if it's lighter.",
                gameplayTip = "Pull yourself toward overhead metal anchors for fast traversal. Pull enemy weapons to disarm them.",
                lineColor = new Color(0.3f, 0.5f, 1f, 0.6f),
                hudColor = new Color(0.5f, 0.5f, 0.8f),
            },

            new MetalDefinition
            {
                metalName = "Pewter",
                metalType = AllomancySkill.MetalType.Pewter,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Tin + Lead",
                density = AllomancyPhysicsFormulas.DENSITY_PEWTER,
                magBurnDuration = AllomancyConstants.PewterBurnDuration,
                drainRate = AllomancyConstants.PewterDrainRate,
                isInstant = false,
                mistingName = "Thug (Pewterarm)",
                description = "Enhance physical abilities. Increased strength, speed, endurance, and durability. Survive falls and injuries that would kill a normal person.",
                loreNote = "Pewter burning suppresses pain and fatigue but doesn't eliminate them. When Pewter runs out, all suppressed fatigue hits at once — the 'Pewter drag crash' can be lethal.",
                gameplayTip = "Burn Pewter before big falls. The superhero landing deals AOE damage to nearby enemies. Don't let your Pewter run out mid-fight.",
                lineColor = new Color(0.6f, 0.6f, 0.6f, 0.5f),
                hudColor = new Color(0.7f, 0.7f, 0.7f),
            },

            new MetalDefinition
            {
                metalName = "Tin",
                metalType = AllomancySkill.MetalType.Tin,
                category = MetalCategory.Physical,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_TIN,
                magBurnDuration = AllomancyConstants.TinBurnDuration,
                drainRate = AllomancyConstants.TinDrainRate,
                isInstant = false,
                mistingName = "Tineye",
                description = "Enhance all five senses. See in the dark, hear whispers from far away, detect danger before it arrives. Risk sensory overload from loud noises or bright lights.",
                loreNote = "Tin enhancement is proportional to burn rate. Flaring Tin gives near-superhuman perception but makes the Allomancer vulnerable to sensory overload — a flashbang can incapacitate a Tineye.",
                gameplayTip = "Use Tin to scout ahead and detect enemies through walls (Allomantic sight). Be careful around explosions and bright lights.",
                lineColor = new Color(0.8f, 0.8f, 0.9f, 0.5f),
                hudColor = new Color(0.8f, 0.85f, 0.9f),
            },

            // ═══════════════════════════════════════════════════════════════
            // MENTAL METALS (External/Internal)
            // ═══════════════════════════════════════════════════════════════

            new MetalDefinition
            {
                metalName = "Zinc",
                metalType = AllomancySkill.MetalType.Zinc,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_ZINC,
                magBurnDuration = AllomancyConstants.ZincBurnDuration,
                drainRate = AllomancyConstants.ZincDrainRate,
                isInstant = false,
                mistingName = "Rioter",
                description = "Riot (inflame) the emotions of others. Enrage enemies into reckless attacks, incite crowds to violence, amplify fear or courage.",
                loreNote = "Rioting doesn't create emotions — it amplifies existing ones. A calm person can have their anger stoked, but a truly happy person can't be made to feel rage from nothing.",
                gameplayTip = "Riot an enemy's aggression to make them attack recklessly (lower defense). Riot fear to make them flee.",
                lineColor = new Color(1f, 0.8f, 0.3f, 0.6f),
                hudColor = new Color(1f, 0.85f, 0.4f),
            },

            new MetalDefinition
            {
                metalName = "Brass",
                metalType = AllomancySkill.MetalType.Brass,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Copper + Zinc",
                density = AllomancyPhysicsFormulas.DENSITY_BRASS,
                magBurnDuration = AllomancyConstants.BrassBurnDuration,
                drainRate = AllomancyConstants.BrassDrainRate,
                isInstant = false,
                mistingName = "Soother",
                description = "Soothe (dampen) the emotions of others. Calm aggressive enemies, pacify guards, remove fear from allies, manipulate negotiations.",
                loreNote = "Soothing is subtle — a skilled Soother can calm a guard without them even noticing. Less skilled Soothers leave their targets feeling unnaturally numb.",
                gameplayTip = "Soothe a guard's suspicion to sneak past. Soothe an ally's fear to boost their morale in combat.",
                lineColor = new Color(0.8f, 0.6f, 0.2f, 0.6f),
                hudColor = new Color(0.85f, 0.7f, 0.3f),
            },

            new MetalDefinition
            {
                metalName = "Copper",
                metalType = AllomancySkill.MetalType.Copper,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_COPPER,
                magBurnDuration = AllomancyConstants.CopperBurnDuration,
                drainRate = AllomancyConstants.CopperDrainRate,
                isInstant = false,
                mistingName = "Smoker (Coppercloud)",
                description = "Create a coppercloud that hides Allomantic pulses from Bronze seekers. Allies within the cloud are invisible to Allomantic detection.",
                loreNote = "A Coppercloud is essential for any crew — without one, a Bronze Seeker can detect every Allomancer in the area. Clubs runs Kelsier's crew's coppercloud.",
                gameplayTip = "Burn Copper when using other metals near enemies who might have Bronze seekers. Protects the whole team.",
                lineColor = new Color(0.6f, 0.4f, 0.2f, 0.5f),
                hudColor = new Color(0.7f, 0.5f, 0.3f),
            },

            new MetalDefinition
            {
                metalName = "Bronze",
                metalType = AllomancySkill.MetalType.Bronze,
                category = MetalCategory.Mental,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Copper + Tin",
                density = AllomancyPhysicsFormulas.DENSITY_BRONZE,
                magBurnDuration = AllomancyConstants.BronzeBurnDuration,
                drainRate = AllomancyConstants.BronzeDrainRate,
                isInstant = false,
                mistingName = "Seeker",
                description = "Detect other Allomancers who are burning metals. See their Allomantic pulses as vibrations, identify which metal they're burning.",
                loreNote = "Bronze seeking can identify the specific metal being burned by the 'flavor' of the pulse. Physical metals pulse differently from mental metals. A coppercloud blocks this entirely.",
                gameplayTip = "Use Bronze to detect hidden Allomancers. Each metal type produces a unique pulse pattern.",
                lineColor = new Color(0.7f, 0.4f, 0.1f, 0.6f),
                hudColor = new Color(0.75f, 0.5f, 0.2f),
            },

            // ═══════════════════════════════════════════════════════════════
            // TEMPORAL METALS (External/Internal)
            // ═══════════════════════════════════════════════════════════════

            new MetalDefinition
            {
                metalName = "Gold",
                metalType = AllomancySkill.MetalType.Gold,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_GOLD,
                magBurnDuration = AllomancyConstants.GoldBurnDuration,
                drainRate = AllomancyConstants.GoldDrainRate,
                isInstant = false,
                mistingName = "Augur",
                description = "See a vision of your past self — who you might have been if you'd made different choices. Deeply unsettling, rarely useful in combat.",
                loreNote = "Gold burning shows a 'gold shadow' — an alternate version of yourself. Most Allomancers find the experience deeply disturbing and avoid burning Gold.",
                gameplayTip = "Gold has limited combat use but can reveal story information about your character's past.",
                lineColor = new Color(1f, 0.8f, 0.0f, 0.6f),
                hudColor = new Color(1f, 0.85f, 0.1f),
            },

            new MetalDefinition
            {
                metalName = "Electrum",
                metalType = AllomancySkill.MetalType.Electrum,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Gold + Silver",
                density = AllomancyPhysicsFormulas.DENSITY_ELECTRUM,
                magBurnDuration = AllomancyConstants.ElectrumBurnDuration,
                drainRate = AllomancyConstants.ElectrumDrainRate,
                isInstant = false,
                mistingName = "Oracle",
                description = "See shadows of your own possible futures. Creates multiple future-self images showing different outcomes of your next few seconds.",
                loreNote = "Electrum is the 'poor man's Atium' — it counters Atium by flooding the Atium-burner's vision with false shadows. This is how Vin defeats Zane.",
                gameplayTip = "Burn Electrum to counter enemy Atium users. Also provides a brief dodge window in combat.",
                lineColor = new Color(0.9f, 0.9f, 0.7f, 0.6f),
                hudColor = new Color(0.95f, 0.95f, 0.75f),
            },

            new MetalDefinition
            {
                metalName = "Cadmium",
                metalType = AllomancySkill.MetalType.Cadmium,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_CADMIUM,
                magBurnDuration = AllomancyConstants.CadmiumBurnDuration,
                drainRate = AllomancyConstants.CadmiumDrainRate,
                isInstant = false,
                mistingName = "Pulser",
                description = "Create a bubble of slowed time around yourself. Everything inside the bubble moves slowly relative to the outside world.",
                loreNote = "Cadmium bubbles are spherical and have a sharp boundary. Objects crossing the boundary experience a sudden time shift. Useful for waiting out sieges or skipping forward in time.",
                gameplayTip = "Use Cadmium to slow enemies inside the bubble while you act at normal speed outside it.",
                lineColor = new Color(0.5f, 0.5f, 0.8f, 0.6f),
                hudColor = new Color(0.6f, 0.6f, 0.85f),
            },

            new MetalDefinition
            {
                metalName = "Bendalloy",
                metalType = AllomancySkill.MetalType.Bendalloy,
                category = MetalCategory.Temporal,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Cadmium + Lead + Tin + Bismuth",
                density = AllomancyPhysicsFormulas.DENSITY_BENDALLOY,
                magBurnDuration = AllomancyConstants.BendalloyBurnDuration,
                drainRate = AllomancyConstants.BendalloyDrainRate,
                isInstant = false,
                mistingName = "Slider",
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
                metalType = AllomancySkill.MetalType.Aluminum,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.InternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_ALUMINUM,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                mistingName = "Aluminum Gnat",
                description = "Instantly purge ALL of your metal reserves. Wipes every metal you're currently storing. Mostly useless for a Mistborn.",
                loreNote = "Aluminum has no Allomantic use for a Misting — they can only burn away their own Aluminum, gaining nothing. As a Mistborn ability it's a last resort to prevent an enemy from stealing your metals via Chromium.",
                gameplayTip = "Almost never useful. Only burn if an enemy Leecher (Chromium) is about to touch you and you'd rather lose everything than let them drain you.",
                lineColor = new Color(0.9f, 0.9f, 0.9f, 0.4f),
                hudColor = new Color(0.9f, 0.9f, 0.9f),
            },

            new MetalDefinition
            {
                metalName = "Duralumin",
                metalType = AllomancySkill.MetalType.Duralumin,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.InternalPush,
                alloyOf = "Aluminum + Copper",
                density = AllomancyPhysicsFormulas.DENSITY_DURALUMIN,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                mistingName = "Duralumin Gnat",
                description = "Burn your entire reserve of another metal in one massive burst. Enormously amplified effect but instantly exhausts the paired metal.",
                loreNote = "Duralumin + Steel = one coin fired at insane velocity. Duralumin + Pewter = brief moment of incredible strength. The cost is losing ALL of the paired metal at once.",
                gameplayTip = "Pair with Steel for a devastating ranged attack. Pair with Pewter for an emergency power boost. Always have backup vials.",
                lineColor = new Color(0.85f, 0.85f, 0.9f, 0.5f),
                hudColor = new Color(0.85f, 0.85f, 0.95f),
            },

            new MetalDefinition
            {
                metalName = "Chromium",
                metalType = AllomancySkill.MetalType.Chromium,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.ExternalPull,
                alloyOf = "Pure element",
                density = AllomancyPhysicsFormulas.DENSITY_CHROMIUM,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                mistingName = "Leecher",
                description = "Strip another Allomancer's metal reserves by touching them. Drains all their metals on contact.",
                loreNote = "Chromium requires physical contact. In combat, this means getting dangerously close to another Allomancer. Extremely effective against Mistborn who rely on multiple metals.",
                gameplayTip = "Touch an enemy Allomancer to drain all their metals. High risk, high reward — you have to get close.",
                lineColor = new Color(0.5f, 0.5f, 0.55f, 0.5f),
                hudColor = new Color(0.6f, 0.6f, 0.65f),
            },

            new MetalDefinition
            {
                metalName = "Nicrosil",
                metalType = AllomancySkill.MetalType.Nicrosil,
                category = MetalCategory.Enhancement,
                quadrant = MetalQuadrant.ExternalPush,
                alloyOf = "Chromium + Nickel + Silicon",
                density = AllomancyPhysicsFormulas.DENSITY_NICROSIL,
                magBurnDuration = 0f,
                drainRate = 0f,
                isInstant = true,
                mistingName = "Nicroburst",
                description = "Supercharge another Allomancer's current burn by touching them. Their active metal fires at Duralumin-level intensity.",
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
    public AllomancySkill.MetalType metalType;
    public MetalCategory category;
    public MetalQuadrant quadrant;
    public string alloyOf;
    public string mistingName;

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
    public Color lineColor;            // Allomantic sight line color
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
