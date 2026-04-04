# GDD Gap Analysis — What We Have vs What We Need

Based on `GDD_Ashwalker.docx` ("The Ashen Project" v1, 4/03/26)

## Status Key
- DONE = fully implemented
- PARTIAL = exists but incomplete
- MISSING = not built yet
- TBD = GDD hasn't decided yet

---

## 1. Game Overview
- High concept defined: "Cut through the mist like a glass dagger" — DONE (title sequence captures this)

## 2. Game Concept  
- Story: Darius's crew, Ember recruited, overthrow Ashen King — PARTIAL (story flags exist in GameFlowManager but no actual story content)
- Themes (Hope, Morality, Power) — MISSING (no morality system, no hope/despair tracking)

## 3. Genre & Design Pillars
- Third-person action-adventure with RPG elements — DONE
- Pillar 1 (Ashwalker Fantasy): Metallurgy feel — PARTIAL (physics done, animations need work)
- Pillar 2 (Grimdark Aesthetic): Art style — PARTIAL (title sequence has it, gameplay needs polish)
- Pillar 3 (Cinematic Narrative): Camera work, set pieces — PARTIAL (cutscene system exists but no content)

## 4. Target Audience
- the original author fans + action RPG players — understood, no code needed

## 5. Game Flow
- Core Loop: Combat → Explore → Story → Repeat — PARTIAL (systems exist, loop not tuned)
- Win: Mission Complete — MISSING (no mission complete system)
- Fail: Death → Checkpoint — DONE (checkpoint system + death UI exist)

## 6. Gameplay & Mechanics
### Steel/Iron Movement (GDD's biggest concern):
- Option A (Physics-based): DONE — our current system
- Option B (Scripted animation): NOT BUILT
- GDD says TBD — we went with Option A

### Combat:
- Daggers: DONE (weapon data exists)
- Canes: MISSING (no cane weapon)
- Coins: DONE (CoinPouch with shotgun, bounce, trail)
- Punches/Kicks: PARTIAL (PlayerCombat has light/heavy but no unarmed variant)
- BotW durability system: MISSING
- Improvisation emphasis: PARTIAL

### Abilities:
- Steel Push/Pull: DONE
- Pewter burning: DONE
- Tin enhanced senses: DONE
- Emotional metallurgy (Zinc/Brass): DONE
- Metal objects to push/pull: DONE
- "Pewter as stamina bar": PARTIAL (Pewter suppresses stamina drain but isn't THE stamina bar)

## 7. Game Progression
- Skill trees: DONE (MetallurgicSkillTree)
- Stat increases: DONE
- Gear/Equipment: DONE (EquipmentManager, WeaponData)
- World progression: PARTIAL (story flags, but no world state changes)
- Camp/Hub: MISSING (no crew hideout/base system)

## 8. Physics
- Character physics (dexterous, swift): DONE
- Push/Pull impact feel: PARTIAL (camera shake exists, needs more juice)
- Environmental physics: PARTIAL (no destructibles, no cloth sim)

## 9. Animation
- "Please fill in this stuff Lori!!!!!!" — PARTIAL (Playables system exists, needs content)

## 10. Interface
- HUD: DONE (health, metal bars, crosshair, quest tracker, minimap)
- Menu systems: DONE (pause, settings, save/load)
- Camera: DONE (third-person with collision)
- Control scheme: DONE (Keybinds.cs with full mapping)

## 11. AI
- Enemy AI: DONE (EnemyAI with states, 13 enemy types)
- Companion AI: MISSING
- World AI (NPC schedules): MISSING
- Difficulty scaling: PARTIAL (wave scaling exists)

## 12. Characters
- Protagonist (Ember): MISSING (no Ember-specific content, player is generic)
- Supporting cast (Darius, Tormund, Lysander, Grimshaw, Harlan): MISSING
- Antagonists (Ashen King, Sentinels): PARTIAL (Sentinel AI exists, Ashen King boss exists)

## 13. World & Level Design
- Cinderhold: PARTIAL (procedural city exists, no hand-crafted areas)
- Key regions: MISSING (no defined regions/districts)
- Level design philosophy: MISSING

## 14. Art Direction
- GDD references: heavy shadows, muted colors, purposeful lighting
- Current state: PARTIAL (DayNightCycle + ash, but needs artist work)

## 15. Audio
- Music: PARTIAL (main theme exists, SoundManager has crossfade)
- Sound design: PARTIAL (procedural audio, basic SFX)
- Voice acting: MISSING
- Ambient: PARTIAL (TitleAmbientAudio exists, gameplay ambient missing)

## 16. Cutscenes
- CutsceneManager: DONE (system exists)
- Actual cutscene content: MISSING

## 17. Target Hardware
- PC first — understood

## 18. Monetization
- Free demo + optional support — no code needed yet

---

## Priority Build List (from GDD)

### HIGH (core gameplay the GDD emphasizes):
1. Weapon durability system (BotW-style, GDD specifically mentions this)
2. Unarmed combat variant (punches, kicks — GDD lists these)
3. Cane weapon type (GDD specifically lists this)
4. Crew hideout/base (GDD mentions camp/hub)
5. Mission system with "Mission Complete" (GDD's win state)

### MEDIUM (GDD mentions but less detail):
6. Companion AI (allies in combat)
7. NPC schedules (world AI)
8. Destructible environment objects
9. Difficulty scaling improvements
10. Morality/choice system (GDD theme: Morality)

### LOW (GDD has placeholder text):
11. Character definitions (Ember, Darius, etc.)
12. Act/chapter structure
13. Cutscene content
14. Voice acting integration
