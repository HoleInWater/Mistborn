# NPC Behavior System — Design Document & TODO

*"I'm not designing combat AI. I'm designing a society reacting to power."*

**Status:** Fully designed, not yet implemented. Saved here to resume later.

---

## Core Thesis

A utility-based AI system where NPCs continuously score four behavioral states:
- **Fight** — commit to engagement
- **Flight** — escape / retreat
- **Freeze** — momentary shock / lockup
- **Flop** — surrender / give up

The highest score determines current behavior. Scores shift dynamically as the situation evolves.

---

## Input Variables (6 Categories)

### 1. Threat Assessment
- Player power level, active abilities, recent displays
- Distance to threat, line of sight
- Weapon type, perceived lethality

### 2. Self Assessment
- Health, skill level, equipment quality
- Courage stat, training type
- Current stamina / readiness

### 3. Social Context
- Ally count, ally strength
- Group morale (shared value across squad)
- Leadership presence (captain/noble nearby boosts Fight)

### 4. Environment
- Escape routes available (doors, alleys, rooftops)
- Cover positions
- Terrain advantages
- Nearby metal objects (Metallurgy risk — metal = danger for NPCs who know about Metallurgists)

### 5. Memory / Recent Events
- Witnessed deaths (ally or enemy)
- Prior Metallurgy exposure (first time vs. seasoned)
- Past escape attempts (success → higher Flight, failure → lower Flight)
- Time since last traumatic event (decay toward baseline)

### 6. Belief System / World Model
- Perceived power hierarchy ("nobles are invincible" vs "they bleed too")
- Agency bias ("my actions matter" vs "nothing changes")
- Risk interpretation (same threat → different weight based on worldview)

---

## Key Insight: Belief ≠ Memory

Two NPCs can witness the same event and interpret it differently based on their belief system.

```
Memory: "I saw a Ashwalker kill three guards"
  → Noble Guard belief: "Ashwalker are powerful, but I'm trained for this"  (Fight stays)
  → Passive Lowborn belief: "The world is more dangerous than I thought"       (Flop rises)
  → Rebel Lowborn belief:  "If they can fight back, maybe we all can"          (Fight rises)
```

Memory feeds belief. Belief modifies how memory is interpreted. This creates a feedback loop over time.

---

## Ashwalker-Specific NPC Archetypes

| Archetype | Fight | Flight | Freeze | Flop | Notes |
|---|---|---|---|---|---|
| Passive Lowborn | Very Low | Low | Medium | High | Learned helplessness from Ashen Dominion oppression |
| Rebel Lowborn | Conditional | Medium | Low | Low | Swing character — Fight if conditions favor it |
| Witness Lowborn | Medium | Medium | Low | Low | Saw resistance succeed; faster Freeze recovery |
| Noble Guard | High | Low | Low | Delayed | Trained, knows Metallurgists exist |
| Untrained Civilian | Very Low | Medium | Very High | High | Reality collapse on Metallurgy reveal |
| Prelate | Low | Medium | Low | Low | Authority figure; Freeze → summon Sentinel |
| Thieving Crew Member | Medium | High | Low | Low | Trained for escape; Flight is preferred |

---

## Architecture: Hybrid Event + Tick System

### Event-Driven (Immediate)
- Ally death → instant morale penalty, Freeze spike for nearby NPCs
- Metallurgy reveal → Freeze spike for NPCs who haven't seen it before
- Taking damage → Fight spike (cornered) or Flight spike (outmatched)
- Player enters area → Threat Assessment recalculation

### Tick-Based (~5-10 Hz)
- Freeze decay (recovers over seconds)
- Morale drift (trends toward group average)
- Agency rebuild (slowly rises if no new threats)
- Belief update (long timescale — changes over minutes/missions)

### Freeze Resolution
```
When Freeze decays below threshold:
  if perceived_agency > 0.3 → resolve to Flight (can still act)
  if perceived_agency < 0.3 → resolve to Flop  (nothing matters)
  if allies_fighting > 2    → resolve to Fight  (social pressure)
```

---

## LOD (Level of Detail) Tiers

| Tier | Range | System | Tick Rate | Notes |
|---|---|---|---|---|
| Tier 1 (Focus) | Near / engaging | Full system, all 6 variable categories | 10 Hz | NPCs in combat or direct interaction |
| Tier 2 (Nearby) | Medium range | Simplified variables, scripted tendencies | 3 Hz | Crowd NPCs who might react |
| Tier 3 (Background) | Far / ambient | Simple state machine, predefined reactions | 0.5 Hz | Visual storytelling only |

NPCs promote/demote between tiers dynamically based on distance and relevance.
Entering Tier 1 triggers full variable initialization from archetype defaults.

---

## The Hidden Variable: Perceived Agency

"Do my actions still matter?"

```
High agency (> 0.7)  → Flight preferred (I can still act, I choose to leave)
Medium agency (0.3–0.7) → Fight or Flight depending on context
Low agency (< 0.3)   → Flop (nothing I do matters, give up)
Freeze              = the moment agency is being recalculated
```

Agency is the bridge between Freeze and its resolution. It answers "what now?"

---

## Belief Update Feedback Loop (TO DESIGN)

This is the core unsolved design problem. How does witnessing resistance change
an NPC's world model?

### Proposed Model (not yet finalized)

```
belief_shift = event_impact × interpretation_weight × repetition_factor

Where:
  event_impact:         how significant the event is (ally death = high, rumor = low)
  interpretation_weight: archetype-dependent (Rebel Lowborn weight resistance +, Passive Lowborn -)
  repetition_factor:    diminishing returns on same event, increasing returns on pattern
```

### Open Questions
- How fast should beliefs change? (One dramatic event vs gradual shift)
- Can beliefs regress? (A Witness Lowborn who sees resistance crushed — does hope decay?)
- Do beliefs propagate socially? (NPC tells others what they saw → belief infection)
- How to model "tipping points"? (Belief crosses threshold → permanent archetype shift)

---

## Long-Term Emergent Behaviors

The belief system should allow simulation of:

### Indoctrination
- Lowborn who never question the system
- High Flop baseline, belief: "this is how the world works"
- Extremely resistant to belief change (high interpretation_weight against hope)

### Hope Spreading
- One witnessed act of resistance updates beliefs across connected NPCs
- Social network: who talks to whom? (family, crew, neighborhood)
- Information degrades as it passes through the network (rumor → myth)

### Radicalization
- Repeated exposure to resistance shifts Fight weight upward over time
- Eventually crosses threshold → archetype shift from Passive to Rebel
- Irreversible past a certain point (can't unsee what you've seen)

### Crowd Dynamics
- Individual Flop NPCs can collectively shift to Fight if group morale spikes
- One brave NPC fighting back can cascade through a crowd
- One dramatic loss can cascade Flop through an entire district

---

## Implementation TODO

### Phase 1: Data Structures
- [ ] Define `NPCBehaviorProfile` ScriptableObject (archetype defaults)
- [ ] Define `NPCBehaviorState` runtime class (current scores, memory, belief)
- [ ] Define `BehaviorEvent` struct (type, impact, source, timestamp)
- [ ] Define `BeliefModel` struct (agency, power hierarchy, risk interpretation)

### Phase 2: Score Calculator
- [ ] Implement utility scoring for Fight/Flight/Freeze/Flop
- [ ] Wire input variables (Threat, Self, Social, Environment, Memory, Belief)
- [ ] Implement event-driven score spikes
- [ ] Implement tick-based decay and recovery

### Phase 3: Behavior Execution
- [ ] Wire highest-score state to EnemyAI state machine
- [ ] Implement Fight behavior (attack, pursue)
- [ ] Implement Flight behavior (pathfind to escape, run)
- [ ] Implement Freeze behavior (stop, stare, vulnerability window)
- [ ] Implement Flop behavior (surrender animation, drop weapons, cower)

### Phase 4: LOD System
- [ ] Implement Tier 1/2/3 with promotion/demotion
- [ ] Distance-based tier calculation
- [ ] Relevance-based promotion (targeted by player, near combat)

### Phase 5: Belief Update Loop
- [ ] Design belief shift formula (see open questions above)
- [ ] Implement social propagation (NPC-to-NPC belief transfer)
- [ ] Implement archetype threshold shifts
- [ ] Test emergent behaviors (crowd panic, hope cascade, radicalization)

### Phase 6: Metallurgy Integration
- [ ] Metal proximity awareness (NPCs avoid standing near metal if they know about Metallurgists)
- [ ] Metallurgy reveal event (first-time → massive Freeze spike)
- [ ] Sentinel presence modifier (overrides all behavior → extreme Flop or extreme Fight)

---

## Resume Point

When ready to implement, start with:
1. Design the belief update feedback loop (how does witnessing resistance change an NPC's world model?)
2. Scaffold the data structures (`NPCBehaviorProfile`, `NPCBehaviorState`, `BehaviorEvent`)
3. Implement the score calculator with a single archetype (Noble Guard — simplest to test)

---

*Saved: April 2026 — Resume when core gameplay loop is stable.*
