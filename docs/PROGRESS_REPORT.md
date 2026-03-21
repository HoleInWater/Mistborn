# Mistborn Era One — Progress Report
**Date:** March 20, 2026  
**Team:** HoleInWater, Tanning, Garrett, Adam

---

## Executive Summary

We've established the **complete design foundation** for a Mistborn Era One Unity game featuring Allomancy mechanics. All documentation is done, project structure is set up, and we're ready to implement.

---

## What We've Accomplished

### ✅ Documentation (100% Complete)
- **32 design documents** covering:
  - All 16 metals with physics specs
  - Combat system design
  - Enemy AI designs (5 types)
  - Environment/lore (Luthadel, The Final Empire setting)
  - Story outline
  - Player controls and HUD
  - Team workflow guides

### ✅ Project Foundation (100% Complete)
- Unity 2022 LTS project created
- Git repository with proper `.gitignore` (excludes 500MB+ of cache files)
- Unity-ready folder structure (Scripts, Prefabs, Scenes, Materials, VFX, etc.)
- GitHub workflow documented

### ✅ Era 1 Lore Verification
- All content verified against Mistborn Era 1 books
- Era 2 references removed (Wax, Wayne, etc.)

---

## What's Ready to Build

### Allomancy System
All 16 metals documented with:
- Physics calculations for push/pull force
- Range formulas (inverse square with cutoff)
- Reserve drain rates
- Visual effects (blue lines, glow)
- Balance values

### Player Controller
- WASD + sprint + jump
- Third-person camera
- Steel Push (RMB) / Iron Pull (LMB)
- Allomantic Sight (Tab)

### 5 Enemy Types Designed
1. **Skaa Soldier** — Basic patrol
2. **Noble Guard** — Armored, shield block
3. **Steel Inquisitor** — Boss with 3 phases
4. **Koloss** — Hemalurgic brute
5. **Kandra** — Shapeshifter

---

## Next Steps

| Priority | Task | Who |
|----------|------|-----|
| 1 | Add scripts in Unity | Team |
| 2 | Set up TestArena scene | Unity dev |
| 3 | Add metal object prefabs | Art team |
| 4 | Create player character | 3D artist |
| 5 | Add particle effects | VFX artist |

---

## Repository

**GitHub:** https://github.com/HoleInWater/Mistborn

---

## Key Documents

- `README.md` — Main overview
- `docs/onboarding.md` — How to get started
- `docs/allomancy-design.md` — How Allomancy works
- `docs/magic-qa-reference.md` — All 16 metals
- `docs/mistborn-era-one-lore.md` — Full lore reference
- `docs/TODO.md` — Master checklist

---

## Team Roles

| Member | Focus |
|--------|-------|
| HoleInWater | DevOps, Documentation |
| Tanning | Unity setup, VS Code integration |
| Garrett | UI (MetalHUD, Controls) |
| Adam | Map development (Scadrial) |

---

**Status: DESIGN COMPLETE — READY TO BUILD**
