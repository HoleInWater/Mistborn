# What We've Built — Project Status

*Here's what's been created for the project so far.*

---

## Scripts (Ready to Use)

| File | Status | What It Does |
|------|--------|--------------|
| `Scripts/Allomancy/AllomanticMetal.cs` | ✅ Done | Enum of all 16 metals |
| `Scripts/Allomancy/MetalReserve.cs` | ✅ Done | Metal reserve data class |
| `Scripts/Allomancy/AllomancerController.cs` | ✅ Done | Core system, handles reserves |
| `Scripts/Allomancy/SteelPushAbility.cs` | ✅ Done | Physics-based push |
| `Scripts/Allomancy/IronPullAbility.cs` | ✅ Done | Physics-based pull |
| `Scripts/Allomancy/AllomanticTarget.cs` | ✅ Done | Marks metal objects |
| `Scripts/Allomancy/AllomanticSight.cs` | ✅ Done | Blue lines (Debug.DrawLine) |
| `Scripts/Allomancy/BlueLineRenderer.cs` | ✅ Done | LineRenderer VFX version |
| `Scripts/Allomancy/Physics/AllomancyPhysicsFormulas.cs` | ✅ Done | Force calculations |
| `Scripts/Allomancy/CoinPouch.cs` | ✅ Done | Sprint 2 ready |
| `Scripts/Allomancy/PewterEnhancement.cs` | ✅ Done | Strength/speed boost |
| `Scripts/Allomancy/TinEnhancement.cs` | ✅ Done | Enhanced senses |
| `Scripts/Player/PlayerController.cs` | ✅ Done | WASD movement, jump |
| `Scripts/Player/PlayerCamera.cs` | ✅ Done | Third-person follow |
| `Scripts/Player/SteelpushAssistedJump.cs` | ✅ Done | Super-jump off anchors |
| `Scripts/Player/AllomancerAnimationController.cs` | ✅ Done | Animation bridge |
| `Scripts/Combat/PlayerHealth.cs` | ✅ Done | Health system |
| `Scripts/Combat/EnemyBase.cs` | ✅ Done | Basic enemy AI |
| `Scripts/Combat/EnemyGuard.cs` | ✅ Done | Soldier with patrol |
| `Scripts/Combat/EnemyCoinshot.cs` | ✅ Done | Enemy that uses push/pull |
| `Scripts/Combat/EnemySeeker.cs` | ✅ Done | Detects Allomancers |
| `Scripts/UI/MetalHUD.cs` | ✅ Done | Basic HUD |
| `Scripts/UI/MetalReserveUI.cs` | ✅ Done | Better bar display |
| `Scripts/World/TestArenaSetup.cs` | ✅ Done | Editor tool for setup |
| `Scripts/Utilities/AllomancyConstants.cs` | ✅ Done | Tuning values |
| `Scripts/Utilities/SaveLoadSystem.cs` | ✅ Done | Save/load game |
| `Scripts/Utilities/SoundManager.cs` | ✅ Done | Audio system |
| `Scripts/Utilities/GameManager.cs` | ✅ Done | Pause, checkpoints |

---

## Documentation

| File | What It Is |
|------|------------|
| `README.md` | Main project overview |
| `CONTRIBUTING.md` | How to contribute |
| `TODO.md` | Master checklist |
| `docs/onboarding.md` | How to get started |
| `docs/architecture.md` | How the code fits together |
| `docs/allomancy-design.md` | How the magic works |
| `docs/lore-notes.md` | Lore from the books |
| `docs/game-design.md` | Elevator pitch, pillars |
| `docs/combat-system.md` | How combat works |
| `docs/progression-system.md` | Leveling up |
| `docs/steelpush-range.md` | Range specs |
| `docs/scene-setup.md` | How to set up scenes |
| `docs/team-playbook.md` | How we work together |
| `docs/hud-design.md` | HUD layout |
| `docs/environment-design.md` | Luthadel city design |
| `docs/animation-spec.md` | Animation needs |
| `docs/magic-qa-reference.md` | All 16 metals Q&A |
| `docs/git-guide.md` | Git basics |
| `docs/TEAM-TASKS.md` | What's left to do |
| `docs/PROJECT-REQUEST-SceneSetup-Kaderator.md` | Scene setup request |

---

## What Still Needs Doing

See `docs/TEAM-TASKS.md` for the full list. Quick version:

**Unity Scene Work (Needs someone with Unity)**
- Setting up the TestArena scene
- Placing metal objects in the world
- Hooking up scripts to game objects
- Testing that things work

**Art & Assets (Needs artists)**
- 3D character model
- Environment models
- Textures and materials
- Particle effects

**Audio (Needs sound work)**
- Sound effects
- Music
- Ambient audio

**Playtesting (Needs testers)**
- Does the push force feel right?
- Is the camera smooth?
- Are there bugs?

---

## How to Request More Code or Docs

Just ask. Examples:
- "Can you write an enemy script for me?"
- "I need a spec for the stealth system"
- "Write up what audio we need for sprint 2"

---

## Need Help?

Check `docs/TEAM-TASKS.md` or ping the team.
