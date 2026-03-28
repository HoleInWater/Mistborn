# Mistborn Era One — Master TODO

> Last updated: 2026-03-28

---

## Current Script Priorities

1. **Tin Allomancy** — full 5-sense implementation (sight, sound, smell, taste, touch ranges)
2. **Particle Effects** — metal burn/flare VFX prefabs for all active metals
3. **Finsih Animation Controllers** — character animation state machines (movement, combat, allomancy)
4. **Enemy Polish** — Koloss, SteelInquisitor, Hazekiller need testing and tuning
5. **Level Design** — first playable Luthadel city block (vertical traversal focus)

---

## What's Done

### Foundation
- [x] Project structure, GitHub repo, Unity 6000.4 LTS setup
- [x] `Mistborn > Auto Setup Player` editor tool — reflection-based, zero merge conflicts
- [x] `[PlayerComponent]` attribute system — components declare themselves, no shared file to edit

### Player
- [x] Basic movement (WASD, sprint, jump)
- [x] Third-person camera
- [x] Player stamina system
- [x] Dodge roll
- [x] Crouch
- [x] Vault/jump
- [x] Wall run (script exists)
- [x] Fall damage

### Allomancy Core
- [x] `Allomancer.cs` — metal reserve management, burn toggle (B key), metal draining
- [x] `AllomanticTarget.cs` — marks objects as pushable/pullable
- [x] `MetalSelector.cs` — 2-metal primary/secondary system, swap key (Left Alt), instant HUD sync
- [x] `MetalReserve.cs` / `MetalRingVisual.cs` — 2-bar HUD + proportional arc ring (bottom-right)
- [x] `FlareManager.cs` — flaring (Left Ctrl), synced with B key burn toggle
- [x] `AllomanticSight.cs` — Tab toggles blue lines to all metal in range
- [x] `RadialMetalMenu.cs` — scroll wheel opens metal selection wheel
- [x] `MetalLineRenderer.cs` — blue line visuals to metals

### Steel & Iron (Physical Metals)
- [x] `SteelPush.cs` — physics push, anchor detection, lore-accurate 1/r force, flight mechanics
- [x] `IronPull.cs` — physics pull, anchor detection, burn state sync fixed
- [x] Flaring works for Steel and Iron (force ×2, drain ×3)
- [x] Coin pouch and trajectory preview

### Other Metals (Scripts exist — needs full testing/polish)
- [x] `Pewter.cs` — strength/speed enhancement
- [ ] `Tin.cs` — script exists, full 5-sense implementation incomplete ← **PRIORITY**
- [x] `Zinc.cs` — riot emotions
- [x] `Brass.cs` — soothe emotions
- [x] `Copper.cs` — hide allomantic pulses
- [x] `Bronze.cs` — detect burning
- [x] `Aluminum.cs` — purge own metals
- [x] `Duralumin.cs` — mega burst
- [x] `Chromium.cs` — leech others' metals
- [x] `Nicrosil.cs` — amplify others
- [x] `Bendalloy.cs` + `TimeBubble.cs` — speed time bubble
- [x] `Cadmium.cs` — slow time bubble
- [ ] `Atium.cs` — see enemy futures (stub, unimplemented)
- [ ] `Malatium.cs` — see future selves (stub, unimplemented)
- [ ] `Gold.cs` — see past self (stub, unimplemented)
- [ ] `Electrum.cs` — see your own future (stub, unimplemented)

### Combat
- [x] `IDamageable` interface
- [x] Player health system
- [x] Player combat (melee)
- [x] Enemy health bars (UI)
- [x] Damage numbers UI

### Enemies
- [x] `EnemyAI.cs` — patrol, chase, flee, NavMesh (crash fix applied)
- [x] `KolossAI.cs` — charge attack, ground slam
- [x] `SteelInquisitorAI.cs` — multi-phase boss
- [x] `LordRulerBoss.cs` — final boss
- [x] `HazekillerAI.cs` — elite enemy
- [x] `EnemySeeker.cs` — detects allomancy
- [ ] All enemies need playtesting and tuning against the actual player

### HUD / UI
- [x] Health and Stamina bars (top-left, UIToolkit)
- [x] 2-metal bars with active/secondary highlight (bottom-right)
- [x] Proportional arc ring indicator (bottom-right corner)
- [x] Pause menu
- [x] Dialogue manager (script)
- [x] Tutorial system (script)
- [x] Achievement system (script)
- [x] Minimap system (script)
- [ ] All UI systems need scene hookup and playtesting

---

## In Progress / Up Next

### Tin Allomancy
- [ ] Enhanced sight range and clarity (highlight interactable objects)
- [ ] Enhanced hearing (ambient audio range, footstep detection radius)
- [ ] Sensitivity downside — bright lights cause pain, loud sounds stun
- [ ] Tin Savant risk: prolonged flaring damages player
- [ ] Visual post-processing effect when Tin is active

### Particle Effects
- [ ] Metal burn particle (emitter around player when burning)
- [ ] Steel Push impact flash on target
- [ ] Iron Pull trail on pulled objects
- [ ] Flare screen vignette/pulse (placeholder exists, needs actual VFX prefab)
- [ ] Pewter body glow while active
- [ ] Time bubble edge effect (Bendalloy/Cadmium)

### Animation Controllers
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

## Backlog (Phase 3+)

### Enemy Variety
- [ ] Noble Guard — armored, shield block
- [ ] Coinshot enemy — ranged Allomancer
- [ ] Kandra — shapeshifter, disguises as NPCs
- [ ] Mist Spirit — phasing ghost

### World & Story
- [ ] Dialogue system scene hookup (DialogueManager.cs exists)
- [ ] Quest system
- [ ] Faction system (nobility vs skaa)
- [ ] NPC interactions

### Polish
- [ ] Screen shake on heavy impact
- [ ] Camera collision
- [ ] Camera lag (cinematic feel)
- [ ] Coyote time (0.15s after edge)
- [ ] Jump buffering (0.2s before landing)
- [ ] Air control while flying via push/pull

### Systems
- [ ] Save/Load scene hookup (SaveLoadSystem.cs exists)
- [ ] Audio — sound files, mixer, 3D spatial audio
- [ ] Skill tree scene hookup
- [ ] Metal pickup/vial system scene hookup
- [ ] Performance optimization (object pooling audit, LODs)

### Future (Post-MVP)
- [ ] Feruchemy system
- [ ] Compounding (burn a Feruchemical charge as an Allomantic metal)
- [ ] Multiplayer
- [ ] Character customization
- [ ] Full Luthadel map (multiple districts)

---

## Notes

- **Lore accuracy is non-negotiable.** Physics formulas are in `docs/PHYSICS-MATH-BOOK.md`. Mental/temporal metals cannot flare. Check `docs/allomancy-design.md` before implementing any metal.
- **Scene coordination:** Don't edit `Scene 1.unity` without checking with the team first. Binary files don't merge.
- **Branching:** Every feature gets its own branch. PR → review → merge to master.
- **Auto Setup Player:** `Mistborn > Auto Setup Player` in Unity Editor. Use it instead of manually adding components.
