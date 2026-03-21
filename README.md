# Mistborn Era One — Project Overview

A third-person action game set in Brandon Sanderson's Mistborn universe. The core gameplay revolves around **Allomancy** — a hard magic system where characters ingest and "burn" metals to gain supernatural abilities.

Right now the focus is on getting the Steel and Iron mechanics feeling right. These are the "push/pull" metals that let you launch metal objects (or yourself) through the air. It's the foundation everything else builds on.

---

## Welcome, Kelsier's Crew 👋

Here's where to start:

### 1. Read the Docs
- `docs/onboarding.md` — How to set up your environment
- `docs/allomancy-design.md` — How the magic system works
- `docs/magic-qa-reference.md` — All 16 metals explained
- `docs/mistborn-era-one-lore.md` — Comprehensive lore reference
- `docs/game-design-reference.md` — Quick game design reference

### 2. See What's Done
- `docs/CAN-DO.md` — All scripts and files we've created
- `docs/TEAM-TASKS.md` — What still needs doing
- `docs/TODO.md` — The master checklist

### 3. Ask Questions
- Check the team chat before editing scenes (Unity scenes don't merge well)
- Coordinate with team members on any Unity scene work

### 4. Jump In
- Pick something from `docs/TODO.md` or `docs/TEAM-TASKS.md`
- Create a branch: `git checkout -b feature/your-feature-name`
- Make changes, commit, push, open a PR
- See `docs/git-guide.md` for the full workflow

### Need Help?
Ping the team. Don't sit on questions.

---

## What We're Using

- **Engine**: Unity 2022 LTS
- **Language**: C#
- **Version Control**: Git + GitHub
- **Team Size**: Small collaborative team

---

## About the .git Folder

When you clone the repo, you'll see folders like `Assets`, `docs`, and a hidden folder called `.git`. **This is important:**

| What's in `.git` | Why You Shouldn't Touch It |
|-----------------|--------------------------|
| All your commit history | This IS your project history |
| Branch information | Where branches are stored |
| Settings for this repo | Your identity, remote URL, etc. |

**DO NOT:**
- Delete the `.git` folder
- Edit files inside `.git`
- Move the `.git` folder
- Commit changes inside `.git`

The `.git` folder is what makes this folder a Git repository. Without it, Git doesn't know this is a tracked project. Everything else (Assets, docs, Scripts) is just regular files.

**If you accidentally deleted `.git`:**
```bash
git init
git remote add origin https://github.com/HoleInWater/Mistborn.git
git pull origin master
```
This restores it.

For more on how Git works, see `docs/git-guide.md`.

---

## Understanding .gitignore — Why Some Files Don't Sync

Unity generates **huge** cache files (500MB+) that rebuild automatically. These should **NOT** be synced:

| Folder | What It Is | Why We Exclude It |
|--------|------------|-------------------|
| `Library/` | Asset cache, compiled data | Rebuilds automatically, huge, machine-specific |
| `Temp/` | Temporary Unity files | Disposable, regenerated constantly |
| `Obj/` | Build intermediates | Generated during compilation |
| `Logs/` | Unity error logs | Not relevant to the project |
| `UserSettings/` | Your personal editor preferences | Everyone has different settings |
| `.vs/` `.vscode/` | IDE cache files | IDE-specific, causes merge conflicts |

### The .gitignore File

This file tells Git **what to ignore**. When you see something NOT in the repo:

1. **Check if it's in `.gitignore`** — If yes, that's intentional
2. **Don't force-add it** with `git add -f`
3. **Don't commit build outputs** like `.dll` or `.exe` files

### When You Clone a Fresh Repo

```bash
# After cloning, Unity needs to rebuild the Library folder
# Just open the project in Unity and wait for import to finish
```

**First-time clone important steps:**
```bash
# 1. Clone the repo
git clone https://github.com/HoleInWater/Mistborn.git

# 2. OPEN THE FOLDER IN UNITY FIRST
# Unity will rebuild Library/ automatically

# 3. If you have errors after pulling new changes:
#    - Close Unity
#    - DELETE the Library folder
#    - Reopen Unity (it will rebuild)

# 4. To ensure a clean state (if asked by team):
#    - Close Unity
#    - Delete Library/, Temp/, Obj/
#    - Reopen Unity
```

### Why This Matters for Teamwork

| Problem | Cause | Solution |
|---------|-------|----------|
| Git changes don't appear in Unity | Library cache is stale | Delete `Library/`, let Unity rebuild |
| Merge conflicts in binary files | Tracked `.dll` or `.asset` files | Remove them from git, add to `.gitignore` |
| Repo is huge (>1GB) | Tracked cache files | Fix `.gitignore`, remove cached files |

### Files We DO Track

```
Assets/_Project/    ← All your scripts, scenes, prefabs, models
docs/               ← Design documents, lore, guides
Packages/          ← Unity package manifest (not the packages themselves)
ProjectSettings/   ← Core Unity settings (keep minimal)
```

### Files We DON'T Track

```
Library/           ← Asset database cache
Temp/              ← Temporary files
obj/               ← Build intermediates
.vs/               ← VS cache
UserSettings/      ← Personal settings
```

---

## How to Get Started

### Step 1: Clone the Repo

First, make sure you have Git installed on your computer. If not, download it from https://git-scm.com/

**Option A: Using Git Bash / Terminal**
```bash
# Navigate to where you want the project folder
cd ~/Documents/Projects

# Clone the repository
git clone https://github.com/HoleInWater/Mistborn.git

# Go into the project folder
cd Mistborn
```

**Option B: Using GitHub Desktop**
1. Download GitHub Desktop from https://desktop.github.com/
2. Sign in with your GitHub account
3. Click "Clone a repository from the Internet"
4. Find "HoleInWater/Mistborn"
5. Choose where to save it on your computer
6. Click "Clone"

**Option C: Using VS Code**
1. Open VS Code
2. Press Ctrl+Shift+P (or Cmd+Shift+P on Mac)
3. Type "Git: Clone"
4. Paste: `https://github.com/HoleInWater/Mistborn.git`
5. Choose a folder to save it
6. Click "Open" when done

---

### Step 2: Open in Unity

1. Open Unity Hub
2. Click "Add"
3. Click "Add project from disk"
4. Navigate to where you saved the project
5. Select the **Mistborn folder** (the root folder, NOT a scene file)
6. Click "Open"
7. Select Unity install "6000.4 LTS"
8. Select "Set Default"

Unity will import everything — this takes a few minutes the first time.

---

### Step 3: Open the Test Scene

1. In Unity, find the Project window
2. Navigate to `Assets/_Project/Scenes/`
3. Double-click `TestArena.unity`
4. The scene opens in the Editor

---

### Step 4: Run the Game

1. Hit the **Play** button at the top of Unity (or press Ctrl+P)
2. Use WASD to move, mouse to look
3. Click left/right mouse to pull/push metal objects

---

## First Time Git Setup

Before you start working, set up your identity:

```bash
git config --global user.name "YourUsername"
git config --global user.email "your@email.com"
```

**Need help understanding Git?** See `docs/git-guide.md` for a full explanation of branches, commits, pushing, and pulling.

### Quick Version

Create a branch for your work:
```bash
git checkout -b feature/your-feature-name
```

After making changes:
```bash
git add .
git commit -m "[SYSTEM] What you changed"
git push origin feature/your-feature-name
```

Then open a Pull Request on GitHub.

---

## Controls

| Action | Key | What It Does |
|--------|-----|--------------|
| Move | WASD | Walk around |
| Sprint | Left Shift | Run faster |
| Jump | Space | Jump / Vault |
| Iron Pull | Mouse Left Click (hold) | Pull metal toward you |
| Steel Push | Mouse Right Click (hold) | Push metal away |
| Allomantic Sight | Tab | See blue lines to all metal |

### How It Works

- **Iron Pull (Left Click):** Hold to pull metal toward you. Light objects (coins) fly to you. Heavy anchored objects (wall brackets) pull YOU toward them instead.

- **Steel Push (Right Click):** Hold to push metal away. Light objects fly away. Heavy anchored objects push YOU in the opposite direction.

- **Allomantic Sight (Tab):** Toggle to see blue lines pointing to all metal within range. Thicker lines = heavier metal.

---

## Project Structure

```
Mistborn/
├── Assets/_Project/
│   ├── Scripts/
│   │   ├── Allomancy/       # All 16 metal abilities
│   │   ├── Allies/          # Companion AI
│   │   ├── Audio/           # Sound management
│   │   ├── Combat/          # Player combat, stealth
│   │   ├── Effects/          # VFX, screen effects
│   │   ├── Enemy/           # 12 enemy types
│   │   ├── Managers/        # Game state, events
│   │   ├── Player/           # Movement, camera, skills
│   │   ├── Quests/           # Quest system
│   │   ├── UI/               # HUD, menus, dialogue
│   │   ├── Utilities/        # Managers, helpers
│   │   └── World/            # Spawners, interactions
│   ├── Prefabs/             # Metal objects, enemies
│   ├── Scenes/              # Unity scenes
│   ├── Materials/            # Visual materials
│   ├── Models/             # 3D models
│   └── Animations/          # Character animations
├── docs/                     # Design docs, guides, lore
├── Packages/                # Unity packages
└── ProjectSettings/         # Unity settings
```

---

## Scripts Overview

### Allomancy (16 Metals)
| Metal | Key | Effect |
|-------|-----|--------|
| Steel | RMB | Push metals away |
| Iron | LMB | Pull metals toward |
| Pewter | Q | Strength, speed, healing |
| Tin | E | Enhanced senses |
| Zinc | Z | Riot emotions |
| Brass | X | Soothe emotions |
| Copper | C | Hide Allomantic pulses |
| Bronze | V | Detect Allomantic pulses |
| Atium | T | See enemy futures |
| Malatium | Y | See future selves |
| Gold | G | See past selves |
| Electrum | 5 | See your futures |
| Aluminum | F | Purge all metals |
| Duralumin | R | Mega metal burst |
| Bendalloy | 8 | Speed time bubble |
| Cadmium | 9 | Slow time bubble |

### Enemies (12 Types)
- **Guard** - Basic patrol enemy
- **Coinshot** - Enemy Allomancer (push)
- **Seeker** - Detects player burning metals
- **Koloss** - Charge attack, ground slam
- **Steel Inquisitor** - Boss with 3 phases
- **Kandra** - Shapeshifter, disguises
- **Noble Guard** - Armored, shield block
- **Mist Spirit** - Phasing ghost
- **Pewter Armsmaster** - Combat arts
- **Lurcher** - Pulls metals
- **Thug** - Tactical fighter
- **Mist Spirit** - Ghost enemy

### Core Systems
- Quest system with objectives
- Save/Load system
- Inventory system
- Achievement system
- Skill tree
- Minimap with markers
- Loading screen
- Tutorial system
- Dialogue manager
- Stealth detection
- Lock-on targeting
- Combat combo system

---

## Common Issues

**"Scripts are missing errors"**
- Make sure you opened the ROOT folder, not just a scene
- Try: Assets → Reimport All

**"Scene looks empty"**
- The TestArena scene needs to be set up in Unity
- Run: Mistborn → Setup Test Arena (menu item)

**"Camera is weird"**
- Press Escape to unlock the mouse cursor

---

## The Team

- Owner: HoleInWater
- Contributors: The Mistborn Game Team

## License

This is a private collaborative project. All rights reserved.
