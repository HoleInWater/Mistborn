# Ashwalker Era One — Master TODO

> Last updated: 2026-03-28

---

## Current Script Priorities

1. **Metals** — metallurgy metals and works
2. **Particle Effects** — metal burn/flare VFX prefabs for all active metals
3. **Finsih Animation Controllers** — character animation state machines (movement, combat, metallurgy)
4. **Enemy Polish** — Bloodbrute, IronSentinel, Metalhunter need testing and tuning
5. **Level Design** — first playable Cinderhold city block (vertical traversal focus)

---

## What's Done

### Foundation
- [x] Project structure, GitHub repo, Unity 6000.4 LTS setup
- [x] `Ashwalker > Auto Setup Player` editor tool — reflection-based, zero merge conflicts
- [x] `[PlayerComponent]` attribute system — components declare themselves, no shared file to edit

### Player
- [x] Basic movement (WASD, sprint, jump)
- [x] Third-person camera
- [x] Player stamina system
- [ ] Dodge roll
- [ ] Crouch
- [ ] Vault/jump
- [ ] Wall run (script exists)
- [ ] Fall damage

### Metallurgy Core
- [x] `Metallurgist.cs` — metal reserve management, burn toggle (left ctrl key), metal draining
- [x] `MetallurgicTarget.cs` — marks objects as pushable/pullable
- [x] `MetalSelector.cs` — 2-metal primary/secondary system, swap key (Left Alt), instant HUD sync
- [x] `MetalReserve.cs` / `MetalRingVisual.cs` — 2-bar HUD + proportional arc ring (bottom-right)
- [x] `FlareManager.cs` — Scroll while burning
- [x] `MetallurgicSight.cs` — Tab toggles blue lines to all metal in range
- [x] `RadialMetalMenu.cs` — scroll wheel opens metal selection wheel
- [x] `MetalLineRenderer.cs` — blue line visuals to metals

### Steel & Iron (Physical Metals)
- [x] `SteelPush.cs` — physics push, anchor detection, lore-accurate 1/r force, flight mechanics
- [x] `IronPull.cs` — physics pull, anchor detection, burn state sync fixed
- [x] Flaring works for Steel and Iron (force ×2, drain ×3)
- [ ] Coin pouch and trajectory preview

### Other Metals (Scripts exist — needs full testing/polish)
- [ ] `Pewter.cs` — strength/speed enhancement
- [x] `Tin.cs` — script exists, full 5-sense implementation incomplete ← **PRIORITY**
- [ ] `Zinc.cs` — riot emotions
- [ ] `Brass.cs` — soothe emotions
- [ ] `Copper.cs` — hide metallurgic pulses
- [ ] `Bronze.cs` — detect burning
- [ ] `Aluminum.cs` — purge own metals
- [ ] `Duralumin.cs` — mega burst
- [ ] `Chromium.cs` — leech others' metals
- [ ] `Nicrosil.cs` — amplify others
- [ ] `Bendalloy.cs` + `TimeBubble.cs` — speed time bubble
- [ ] `Cadmium.cs` — slow time bubble
- [ ] `Oraculum.cs` — see enemy futures (stub, unimplemented)
- [ ] `Revelum.cs` — see future selves (stub, unimplemented)
- [ ] `Gold.cs` — see past self (stub, unimplemented)
- [ ] `Electrum.cs` — see your own future (stub, unimplemented)

### Combat
- [ ] `IDamageable` interface
- [x] Player health system
- [ ] Player combat (melee)
- [ ] Enemy health bars (UI)
- [ ] Damage numbers UI

### Enemies
- [ ] `EnemyAI.cs` — patrol, chase, flee, NavMesh (crash fix applied)
- [ ] `BloodbruteAI.cs` — charge attack, ground slam
- [ ] `IronSentinelAI.cs` — multi-phase boss
- [ ] `AshenKingBoss.cs` — final boss
- [ ] `MetalhunterAI.cs` — elite enemy
- [ ] `EnemySeeker.cs` — detects metallurgy
- [ ] All enemies need playtesting and tuning against the actual player

### HUD / UI
- [x] Health and Stamina bars (top-left, UIToolkit)
- [x] 2-metal bars with active/secondary highlight (bottom-right)
- [x] Proportional arc ring indicator (bottom-right corner)
- [ ] Pause menu
- [ ] Dialogue manager (script)
- [ ] Tutorial system (script)
- [ ] Achievement system (script)
- [ ] Minimap system (script)
- [ ] All UI systems need scene hookup and playtesting

---

## In Progress / Up Next

### Particle Effects
- [ ] Metal burn particle (emitter around player when burning)
- [ ] Steel Push impact flash on target
- [ ] Iron Pull trail on pulled objects
- [ ] Flare screen vignette/pulse (placeholder exists, needs actual VFX prefab)
- [ ] Pewter body glow while active
- [ ] Time bubble edge effect (Bendalloy/Cadmium)

### Finish Animation Controllers
- [ ] Player — idle, walk, run, jump, fall, land
- [ ] Player — push/pull stance and follow-through
- [ ] Player — flare reaction
- [ ] Enemy — patrol idle, alert, attack, death
- [ ] Animator Controller assets need creating in Unity Editor

### Level Design
- [ ] City block with rooftops and vertical paths for Steel/Iron traversal
- [ ] Metal objects placed for traversal (brackets, railings, coins)
- [ ] Lighting — ash/soot atmosphere, mist effects
- [ ] Enemy patrol routes set up in scene
- [ ] At least one indoor area (keep interior)

---

## Notes

- **Lore accuracy is non-negotiable.** Physics formulas are in `docs/PHYSICS-MATH-BOOK.md`. Mental/temporal metals cannot flare. Check `docs/metallurgy-design.md` before implementing any metal.
- **Scene coordination:** Don't edit `Scene 1.unity` without checking with the team first. Binary files don't merge.
- **Branching:** Every feature gets its own branch. PR → review → merge to master.
- **Auto Setup Player:** `Ashwalker > Auto Setup Player` in Unity Editor. Use it instead of manually adding components.
