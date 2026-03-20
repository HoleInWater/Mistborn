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

### Need Help?
Ping the team. Don't sit on questions.

## What We're Using

- **Engine**: Unity 2022 LTS
- **Language**: C#
- **Version Control**: Git + GitHub
- **Team Size**: Small collaborative team

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
2. Click "Open" 
3. Navigate to where you saved the project
4. Select the **Mistborn folder** (the root folder, NOT a scene file)
5. Click "Open"

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

| Action | Key |
|--------|-----|
| Move | WASD |
| Sprint | Left Shift |
| Jump | Space |
| Iron Pull (left) | Mouse Left Click |
| Steel Push (right) | Mouse Right Click |
| Allomantic Sight | Tab |

## Project Structure

```
Assets/_Project/
├── Scripts/          # All C# code
│   ├── Allomancy/    # Magic system
│   ├── Player/       # Character & camera
│   ├── Combat/       # Fighting mechanics
│   ├── GUI/          # HUD elements
│   └── Utilities/    # Helpers & constants
├── Prefabs/          # Reusable game objects
├── Scenes/           # Unity scenes
└── Materials/        # Visual materials
```

## The Team

- Owner: HoleInWater
- Collaborator: thenbuzzard100@gmail.com

## License

This is a private collaborative project. All rights reserved.
