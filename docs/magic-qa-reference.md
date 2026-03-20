# MISTBORN ERA 1 — METALLIC ARTS Q&A REFERENCE
## For: Digital Video Game Implementation

*Reference document for all three Metallic Arts: Allomancy, Feruchemy, Hemalurgy*

---

## SECTION 1 — PHYSICAL METALS

---

### IRON | Allomancy: Ironpulling | Feruchemy: Weight | Hemalurgy: Steal Physical Powers

**Q: What can a Lurcher Pull, and what can they NOT Pull?**

A: Burning iron lets a Lurcher Pull on metal objects — coins, weapons, armor, anything made of or containing metal. They cannot Pull on living flesh or organic matter.

**GAME:** Tagging system for metal vs non-metal objects. What happens with mixed objects (wooden sword, metal pommel)?

---

**Q: Is there a range on Ironpulling?**

A: Yes, range exists. Scales with Allomancer power level and how much metal being burned. Flaring extends reach. Average Lurcher: few dozen meters.

**GAME:** Fixed stat, scaling resource, or upgradeable? Range indicator or keep vague?

---

**Q: What determines PLAYER vs OBJECT movement when Pulling?**

A: Relative mass. Lighter object moves more. Pull a coin — coin flies to you. Pull a wall bracket (infinite mass) — you fly to it.

**GAME:** Simulate actual mass differentials or simplified anchored/loose rules?

---

**Q: What does an ironmind store?**

A: Weight. Filling makes you literally lighter. Tapping makes you dramatically heavier.

**GAME:** Lighter = float further, pushed by wind. Heavier = break floors, resist knockback.

---

**Q: What does an iron spike steal?**

A: Allomantic strength in Physical metals — Ironpulling and Steelpushing. Must be driven through bicep.

---

### STEEL | Allomancy: Steelpushing | Feruchemy: Speed | Hemalurgy: Steal Physical Powers

**Q: How does a Coinshot "fly"?**

A: Push downward against coins on ground — equal-and-opposite reaction launches upward. Controlled ballistic movement, not true flight.

**GAME:** Infinite coins or limited economy? Urban environment design for traversal.

---

**Q: Range on Steelpushing? Push through walls?**

A: Range exists, scales with power. Can sense metals through walls via blue lines. No direct line-of-sight needed.

**GAME:** Blue lines as toggle or always-on? Handle visual clutter in metal-rich environments.

---

**Q: Why blue lines? What do they show?**

A: Allomancer's perceptual sense. Lines vary in thickness based on metal mass. Maps rooms through walls.

**GAME:** Built-in radar system. Design puzzles using blue lines only.

---

**Q: What does a steelmind store?**

A: Physical speed. Filling = sluggish. Tapping = superhuman velocity.

**GAME:** Slowed world or just faster movement? Affects hit detection.

---

### TIN | Allomancy: Enhanced Senses | Feruchemy: Sensory Acuity | Hemalurgy: Steal Senses

**Q: Downsides of enhanced senses?**

A: Enhanced senses can be weaponized. Loud noises agonizing, bright flashes blinding, strong smells overwhelming.

**GAME:** Enemies designed to counter tin — flashbangs, deafening bells, smoke bombs.

---

**Q: Selective sense enhancement?**

A: All five enhanced simultaneously by default. Skilled Tineyes can focus specific senses through practice.

**GAME:** Skill tree unlock for selective control.

---

**Q: What does a tinmind store?**

A: Sensory acuity — can choose which specific sense per tinmind. More precise than Allomantic tin.

**GAME:** Environmental puzzles requiring specific sensory combinations.

---

**Q: Tineye savant?**

A: Permanently heightened senses even without burning. Cannot turn off. Cautionary tale about overuse.

**GAME:** Risk-reward mechanic. Permanent drawbacks from over-reliance.

---

### PEWTER | Allomancy: Physical Enhancement | Feruchemy: Strength | Hemalurgy: Steal Physical Powers

**Q: What does pewter enhance beyond strength?**

A: Strength, speed, endurance, balance, AND pain tolerance simultaneously. Speeds healing of injuries sustained while burning.

**GAME:** Full combat mode — different animation states, altered movement weight.

---

**Q: What happens when Pewterarm runs out mid-fight?**

A: "Pewter drag" hits. All accumulated damage and exertion crashes back at once. Strategy: outlast and deplete.

**GAME:** Crash state with vulnerability window. Enemies who know to wait out pewter.

---

**Q: What does a pewtermind store?**

A: Physical strength. Filling = visible weakening. Tapping = muscles bulge to inhuman size.

**GAME:** Dynamic character model changes. Monster-looking player when tapping heavily.

---

## SECTION 2 — MENTAL METALS

---

### ZINC | Allomancy: Riot Emotions | Feruchemy: Mental Speed | Hemalurgy: Steal Mental Powers

**Q: What does Rioting do?**

A: Amplifies and inflames an emotion that ALREADY EXISTS. Cannot create emotions from nothing.

**GAME:** NPC emotion system. Push existing attitudes to extremes.

---

**Q: Targeted or area effect?**

A: Both. Unfocused = everyone in range. Focused = single person's specific emotion. Breeze-level precision possible.

**GAME:** Quick area effect vs slower precision interaction. Stealth implications.

---

**Q: What does a zincmind store?**

A: Mental speed. Filling = mentally slower, foggy. Tapping = extraordinarily fast thought processing.

**GAME:** Bullet-time planning windows. UI fog while filling.

---

### BRASS | Allomancy: Soothe Emotions | Feruchemy: Body Heat | Hemalurgy: Steal Mental Powers

**Q: What does Soothing do?**

A: Dampens and suppresses emotions. Skilled Soother can suppress ALL emotions except one.

**GAME:** Pacify enemies without combat. Precision isolation of loyalty.

---

**Q: Protection against emotional Allomancy?**

A: Aluminum blocks it. Coppercloud hides all Allomantic activity within. Another Allomancer burning brass/zinc can counteract.

**GAME:** Protection mechanics create counterplay depth.

---

**Q: What does a brassmind store?**

A: Body heat. Filling = physically cold. Tapping = unusual internal warmth.

**GAME:** Temperature system interaction. Melting ice, igniting flammables.

---

### COPPER | Allomancy: Coppercloud | Feruchemy: Memory | Hemalurgy: Steal Mental Powers

**Q: What does a coppercloud do?**

A: Sphere around Smoker that hides ALL Allomantic activity from Seekers. Does NOT hide the people themselves.

**GAME:** Placeable stealth field. Safe houses and preparation zones.

---

**Q: Strategic importance of a Smoker?**

A: Without one, every Allomancer in crew is detectable. Copper = difference between safe ops and capture.

**GAME:** Support role that enables aggressive Allomantic use.

---

**Q: What does a coppermind store?**

A: Memories. Perfectly and completely. Stored memories removed from active recall. Tap = perfect retrieval.

**GAME:** In-world journal/codex system. Memories as quest items.

---

**Q: Can one Feruchemist access another's coppermind?**

A: No. Fundamentally. A metalmind can only be accessed by who charged it.

**GAME:** Dead Feruchemist's metalminds = locked vaults.

---

### BRONZE | Allomancy: Detect Allomancy | Feruchemy: Wakefulness | Hemalurgy: Steal Mental Powers

**Q: What can a skilled Seeker detect?**

A: Unskilled = someone burning something. Skilled = which specific metal. Very skilled = multiple Allomancers simultaneously.

**GAME:** Enemy faction that punishes Allomantic use. Call in specific counters.

---

**Q: What blocks Bronze?**

A: Smoker burning copper. Coppercloud blinds Bronze detection for all within.

**GAME:** Rock-paper-scissors: Bronze detects, Copper blocks, powerful Seekers pierce Copper.

---

**Q: What does a bronzemind store?**

A: Wakefulness. Filling = drowsy. Tapping = stay alert far beyond human limits.

**GAME:** Immunity to sleep effects. 72-hour alertness possible.

---

## SECTION 3 — HIGHER METALS

---

### GOLD | Allomancy: Past Self Vision | Feruchemy: Health | Hemalurgy: Steal Feruchemical Powers

**Q: What does burning gold show?**

A: Vision of who you could have been — alternate versions based on different life choices. Psychologically unsettling.

**GAME:** Story device. Ghosts of roads not taken.

---

**Q: What does a goldmind store?**

A: Health and healing. Filling = sickly. Tapping = dramatically accelerated healing.

**GAME:** Pre-planning sustain system. Time weakened to build reserves.

---

### ELECTRUM | Allomancy: Own Future Shadow

**Q: What does electrum show vs atium?**

A: Electrum = your own future actions. Atium = others' future actions. Electrum counters atium.

**GAME:** Nullification mechanic. Brief "phantom self" preview.

---

### ATIUM | Allomancy: See Others' Futures | Hemalurgy: Steal Feruchemical Powers

**Q: What does atium show?**

A: Ghostly images of what every nearby person/object is about to do. Cognitively overwhelming with multiple opponents. Burns extremely fast.

**GAME:** Short duration, limited pool, ghost images of enemy movements.

---

**Q: Counters to atium?**

A: Burning electrum, multiple simultaneous attacks, waiting them out, pure randomness/instinct.

**GAME:** Swarm enemies counter atium. Discovery mechanic before atium boss.

---

### ALUMINUM | Allomancy: Wipe Own Metals | Feruchemy: Identity | Hemalurgy: Steal Identity

**Q: What does burning aluminum do?**

A: Instantly destroys all other metals in stomach. No power gained. Protects against Nicroburst. Aluminum nulls external emotional Allomancy.

**GAME:** Emergency panic button. Aluminum-lined equipment reduces emotional damage.

---

### DURALUMIN | Allomancy: Burst Current Metal | Hemalurgy: Steal Identity

**Q: What does duralumin do?**

A: Massive instantaneous burst of current metal's power — consumes all at once. One-shot nuclear option. Leaves you completely defenseless after.

**GAME:** Ultimate ability. Commit to one metal, massive payoff, long vulnerability window.

---

### CHROMIUM/NICROSIL | Allomancy: Drain/Burst Others' Metals | Hemalurgy: Steal Enhancement Powers

**Q: What do chromium and nicrosil do?**

A: Chromium = drain someone's metal reserves on touch. Nicrosil = burst someone's metals immediately. Anti-Allomancer weapons requiring contact.

**GAME:** Grappler enemy archetype. High-risk high-reward close-range.

---

### CADMIUM/BENDALLOY | Allomancy: Slow/Speed Time Bubbles | Feruchemy: Breath/Nutrition

**Q: What do cadmium and bendalloy do?**

A: Cadmium = slow time bubble. Bendalloy = speed time bubble. KEY: Allomancer always inside their own bubble. Cannot affect enemies without being in it.

**GAME:** Position-commitment abilities. Deflection at bubble boundary.

---

## SECTION 4 — HEMALURGY

---

**Q: What determines what a spike steals?**

A: BOTH metal type AND placement on body. Wrong location = different/diminished power.

**GAME:** Full simulation or abstracted? Placement map as discoverable lore.

---

**Q: What is always lost in Hemalurgy transfer?**

A: Power is always diminished. End-negative. Recipient gets less than donor had.

**GAME:** Hemalurgy always yields diminishing returns.

---

**Q: What does Ruin exploit?**

A: Spikes tear Spiritweb, creating gap. Gives Ruin access to influence and control.

**GAME:** Corruption/Influence system. Accumulated usage affects control.

---

**Q: Steel Inquisitor spikes?**

A: ~10 spikes. Eye-spikes typically grant steel/iron. Metal-detection sight, not normal vision.

**GAME:** Multiple spiked powers = feels like multiple Allomancers in one enemy.

---

## SECTION 5 — CROSS-SYSTEM

---

**Q: Which system connects to which Shard?**

A: Allomancy = Preservation (end-positive). Hemalurgy = Ruin (end-negative). Feruchemy = Balance (end-neutral).

**GAME:** Thematic backbone. Allomancy = explosive, Hemalurgy = costly, Feruchemy = balanced.

---

**Q: What is Compounding?**

A: Twinborn uses same metal for both Allomantic and Feruchemy. Store attribute, burn charged metalmind, supercharged output.

**GAME:** Late-game power fantasy. Gate behind progression. Consider whether available to players at all.

---

## REFERENCE: Community Q&A Summary

**Range:** ~100 paces (~75m) max from Mistborn Adventure Game
**Aluminum planes:** Immune to push/pull (they're specifically aluminum)
**Zenith point:** Max height per push, drops more metal to go higher
**Line of sight:** Not needed — blue lines pass through walls
**Savants:** Permanent enhancement/damage from overuse
**Identity:** Cannot be accessed by others' metalminds
