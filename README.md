# Mistborn Era One — Project Overview

A third-person action game set in Brandon Sanderson's Mistborn universe. The core gameplay revolves around **Allomancy** — a hard magic system where characters ingest and "burn" metals to gain supernatural abilities.

Right now the focus is on getting the Steel and Iron mechanics feeling right. These are the "push/pull" metals that let you launch metal objects (or yourself) through the air. It's the foundation everything else builds on.

---

## Welcome, ThenBuzzard 👋

Here's where to start:

### 1. Read the Docs
- `docs/onboarding.md` — How to set up your environment
- `docs/allomancy-design.md` — How the magic system works
- `docs/magic-qa-reference.md` — All 16 metals explained

### 2. See What's Done
- `docs/CAN-DO.md` — All scripts and files we've created
- `docs/TEAM-TASKS.md` — What still needs doing
- `TODO.md` — The master checklist

### 3. Ask Questions
- `docs/PROJECT-REQUEST-SceneSetup-Kaderator.md` — What Kaderator is working on
- Check the team chat before editing scenes (Unity scenes don't merge well)

### 4. Jump In
- Pick something from `TODO.md` or `docs/TEAM-TASKS.md`
- Create a branch: `git checkout -b feature/your-feature-name`
- Make changes, commit, push, open a PR
- See `docs/git-guide.md` for the full workflow

### Need Help?
Ping the team. Don't sit on questions.

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

### Future Controls

| Action | Key | Status |
|--------|-----|--------|
| Pewter Enhancement | Q | Ready, needs hooking up |
| Tin Enhancement | E | Ready, needs hooking up |
| Steelpush Jump | Space + anchor below | Ready, needs hooking up |
| Coin Throw | F | Sprint 2 |

## Project Structure

```
Mistborn/
├── Assets/_Project/
│   ├── Scripts/
│   │   ├── Allomancy/     # Steel, Iron, Pewter, Tin abilities
│   │   ├── Player/        # Movement, camera, assisted jump
│   │   ├── Combat/        # Enemy base, enemy coinshot
│   │   ├── GUI/           # Metal reserve display
│   │   ├── World/         # Arena setup tools
│   │   ├── Utilities/     # Sound, save/load, constants
│   │   └── Physics/       # Allomancy force calculations
│   ├── Prefabs/           # Metal objects (coins, brackets)
│   ├── Scenes/            # Unity scenes
│   ├── Materials/         # Visual materials
│   ├── Models/           # 3D models
│   └── Animations/       # Character animations
├── docs/                  # Design docs, lore, guides
├── Packages/              # Unity packages
└── ProjectSettings/      # Unity settings
```

## Ready-to-Use Scripts

These are written and waiting to be hooked up:

| Script | What It Does |
|--------|--------------|
| `AllomancerController` | Manages metal reserves |
| `SteelPushAbility` | Push metal objects |
| `IronPullAbility` | Pull metal objects |
| `AllomanticTarget` | Marks objects as metal |
| `AllomanticSight` | Shows blue lines |
| `PlayerController` | WASD movement |
| `PlayerCamera` | Third-person camera |
| `MetalReserveUI` | Shows reserve bars |
| `EnemyBase` | Basic enemy AI |
| `EnemyCoinshot` | Enemy that uses push/pull |
| `PewterEnhancement` | Strength/speed boost |
| `TinEnhancement` | Enhanced senses |
| `SaveLoadSystem` | Save/load game state |
| `SoundManager` | Audio system |

## Common Issues

**"Scripts are missing errors"**
- Make sure you opened the ROOT folder, not just a scene
- Try: Assets → Reimport All

**"Scene looks empty"**
- The TestArena scene needs to be set up in Unity
- Run: Mistborn → Setup Test Arena

**"Camera is weird"**
- Press Escape to unlock the mouse cursor

## The Team

- Owner: HoleInWater
- Collaborator: thenbuzzard100@gmail.com

## License

This is a private collaborative project. All rights reserved.
