# Mistborn Era One — Project Overview

A third-person action game set in Brandon Sanderson's Mistborn universe. The core gameplay revolves around **Allomancy** — a hard magic system where characters ingest and "burn" metals to gain supernatural abilities.

Right now the focus is on getting the Steel and Iron mechanics feeling right. These are the "push/pull" metals that let you launch metal objects (or yourself) through the air. It's the foundation everything else builds on.

## What We're Using

- **Engine**: Unity 2022 LTS
- **Language**: C#
- **Version Control**: Git + GitHub
- **Team Size**: Small collaborative team

## How to Get Started

1. Clone the repo
2. Open the project folder in Unity (not a scene — open the root folder)
3. Let Unity import everything (first time takes a few minutes)
4. Open `Assets/_Project/Scenes/TestArena.unity`
5. Hit Play

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
│   ├── UI/           # HUD elements
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
