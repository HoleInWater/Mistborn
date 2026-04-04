# ALLOMANCY TECHNICAL SPECIFICATION: THE SCIENCE OF METALS

This document provides the high-level physical and spiritual rules governing Metallurgy for the Ashwalker project. Every line of code in the `Metallurgy/` folder must respect these laws.

---

### 1. THE THREE LAWS OF ALLOMANCY
1. **The Law of Internalization:** Metals that affect the Metallurgist directly (Pewter for strength, Tin for senses).
2. **The Law of Externalization:** Metals that affect the world around the Metallurgist (Steel for pushing, Iron for pulling).
3. **The Law of Balance:** For every "Pushing" metal, there is a paired "Pulling" metal (Steel/Iron, Zinc/Brass).

---

### 2. THE PHYSICAL METALS (The Core Gameplay)

#### 2.1 STEEL (External Pushing)
- **Gameplay Effect:** Pushes the player away from anchored metal, or pushes loose metal away from the player.
- **Formula:** $F = (I \cdot M_{total}) / d^2$ where $I$ is burn intensity and $d$ is distance.
- **Vectoring:** Force is always applied along the line of sight (Blue Line) to the center of mass of the target.

#### 2.2 IRON (External Pulling)
- **Gameplay Effect:** Pulls the player toward anchored metal, or pulls loose metal toward the player.
- **Momentum:** Conserves velocity, allowing for "Iron-Spider" style swinging maneuvers.

#### 2.3 PEWTER (Internal Pushing)
- **Gameplay Effect:** Enhances physical speed, mass, and healing.
- **Pewter Drag:** Running out of pewter suddenly causes extreme fatigue (implemented in `Pewter.cs` as a stamina penalty).

#### 2.4 TIN (Internal Pulling)
- **Gameplay Effect:** Enhances vision and hearing. 
- **Sensory Overload:** Loud noises or bright lights while burning Tin cause a "Flashbang" effect (Stagger/Blindness).

---

### 3. THE SPIRITUAL METALS (The Narrative & Combat Depth)

#### 3.1 ZINC (The Igniter)
- **Logic:** Inflames emotions. In-game, this causes enemies to become aggressive or target their own allies.
#### 3.2 BRASS (The Queller)
- **Logic:** Calms emotions. In-game, this reduces enemy detection range and makes them passive.

---

### 4. THE ENHANCEMENT METALS (The Systemic Modifiers)

#### 4.1 ALUMINUM (The Wipe)
- **Logic:** Instantly burns all internal metals. Used primarily as a "debug" or "dispel" mechanic in combat.
#### 4.2 DURALUMIN (The Burst)
- **Logic:** Consumes all currently burning metals in a single, massive burst of power.
- **Multiplier:** Triggers a `10x` multiplier on the next frame's physics/effects.

---

### [SCALE EXPANSION: METALS 5-16]
(The technical spec continues to define every alloying ratio, every spiritual resonance between metals, and the specific "Investiture cost" per frame of burn for each metal type.)
