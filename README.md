# Mistborn Era One — Unity Game

A third-person action RPG built in Unity, set in Brandon Sanderson's Mistborn universe. The core gameplay loop is **Allomancy** — a hard magic system where Mistborn ingest metals and "burn" them for supernatural abilities.

The current focus is on getting all 18 metals feeling correct before expanding into full level design, story, and enemy variety.

---

## Current Priorities

| # | Task | Status |
|---|------|--------|
| 1 | Tin Allomancy — full 5-sense implementation | In Progress |
| 2 | Particle effects — metal burn/flare VFX prefabs | Pending |
| 3 | Animation Controllers — character state machines | Pending |
| 4 | Enemy polish — Koloss, Inquisitor, Hazekiller | Pending |
| 5 | Level design — playable Luthadel environment | Pending |
| 6 | Audio system integration | Pending |
| 7 | Save/Load system | Pending |

See `TODO.md` for the full breakdown.

---

## What's Working

- **Steel Push / Iron Pull** — physics-based push/pull with anchor detection, flaring, lore-accurate force math
- **2-Metal Selection HUD** — bottom-right bars showing primary/secondary metal reserves + proportional ring indicator
- **Allomantic Sight** — Tab to toggle blue lines to all metal objects in range
- **Flaring** — Left Ctrl or Shift+Push/Pull, synced across burn state
- **Metal Wheel** — radial menu for selecting metals
- **Enemy AI** — patrol, chase, flee with NavMesh
- **Time Bubbles** — Bendalloy (speed) and Cadmium (slow)
- **Auto Setup Player** — `Mistborn > Auto Setup Player` editor tool to wire up a player GameObject without manual scene work

---

## Getting Started

### Prerequisites

- Unity **6000.4 LTS** (Hub will prompt you to install it)
- Git

### Clone and Open

```bash
git clone https://github.com/HoleInWater/Mistborn.git
cd Mistborn
```

Open Unity Hub → **Add project from disk** → select the `Mistborn` folder → open with Unity 6000.4 LTS.

Unity will import everything on first open (a few minutes).

### Open the Scene

Project window → `Assets/_Project/Scenes/` → double-click `Scene 1.unity`

### Play

Hit the Play button (or `Ctrl+P`). Use the controls below.

---

## Controls

| Action | Input |
|--------|-------|
| Move | WASD |
| Sprint | Left Shift |
| Jump | Space |
| Iron Pull | Hold LMB |
| Steel Push | Hold RMB |
| Allomantic Sight | Tab |
| Burn Toggle | B |
| Flare | Left Ctrl (while burning) |
| Swap Metal (Primary/Secondary) | Left Alt |
| Metal Selection Wheel | Scroll Wheel (opens radial menu) |

**How Push/Pull works:**
- Light objects (coins) fly toward/away from you
- Heavy anchored objects (wall brackets) launch *you* in the opposite direction
- Force scales with distance: closer = stronger (lore accurate)

---

## Working on the Project

### Branching

```bash
git checkout -b feature/your-feature-name
```

### Committing

```bash
git add <specific-files>
git commit -m "Short description of what changed and why"
git push origin feature/your-feature-name
```

Then open a Pull Request on GitHub. **Don't push directly to master without a PR unless it's a critical fix.**

### Scene Coordination

Unity scene files don't merge. Check with the team before editing `Scene 1.unity`. If two people need to modify the scene, coordinate who goes first.

### Auto Setup Player

If the Player GameObject in a scene is missing components, run `Mistborn > Auto Setup Player` from the Unity menu bar. It scans all scripts tagged with `[PlayerComponent]` and lets you add them in one click with full Undo support.

---

## Project Structure

```
Assets/_Project/
├── Scripts/
│   ├── Allomancy/
│   │   ├── Allomancer Types/   # Allomancer.cs, AllomanticTarget, Feruchemist
│   │   ├── Metals/             # One script per metal (SteelPush, IronPull, Tin, etc.)
│   │   ├── Dependancies/       # MetalReserve, MetalSelector, FlareManager
│   │   └── Coins/              # CoinPouch, CoinPhysics, trajectory preview
│   ├── Player/                 # Movement, camera, stamina, parkour
│   ├── Combat/                 # Health, damage, melee
│   ├── Enemy/                  # EnemyAI, KolossAI, SteelInquisitorAI, LordRulerBoss
│   ├── UI/                     # HUD, menus, dialogue, tutorial
│   └── World/                  # Spawners, checkpoints, interactions
├── Scenes/                     # Scene 1.unity
├── Prefabs/                    # Metal objects, enemies
└── Materials/
Assets/UI Toolkit/
├── HUD.uxml                    # HUD layout
└── HUD.uss                     # HUD styles
docs/                           # Design docs, lore, physics handbook
```

---

## Reference

- **Lore / physics rules:** `docs/PHYSICS-MATH-BOOK.md`
- **Allomancy design:** `docs/allomancy-design.md`
- **Onboarding:** `docs/onboarding.md`
- **Git workflow:** `docs/git-guide.md`

---

## Common Issues

**"Scripts are missing" errors after clone**
→ Make sure you opened the root `Mistborn/` folder in Unity, not a subfolder. Try Assets → Reimport All.

**Scene looks empty or wrong**
→ Ensure you have `Scene 1.unity` open (`Assets/_Project/Scenes/`).

**Player missing components in scene**
→ Use `Mistborn > Auto Setup Player` from the menu bar.

**Pulled new code and Unity is broken**
→ Close Unity, delete `Library/` and `Temp/`, reopen Unity. It will rebuild.

---

## Team

- Owner: HoleInWater
- Contributors: Kelsier's Crew
