# Game Design Document — Mistborn Era One

*Work in progress. This document defines the game we're building.*

---

## Elevator Pitch

A third-person action game where you play as a Mistborn — someone who can burn all 16 metals to gain supernatural powers. The core loop is using Steel and Iron to push and pull metal objects, letting you soar through the ash-filled streets of Luthadel.

Think: Assassin's Creed movement meets Jet Set Radio with Mistborn magic.

---

## The Elevator Pitch (Long Version)

In a world ruled by the Lord Ruler, where ash falls endlessly from a grey sky and the mists come every night, you are one of the few who can burn metals. Not just any metals — all of them.

But this game starts simple. Just Steel and Iron. Push and Pull.

Push coins at enemies. Push off metal rooftops and fly. Pull yourself toward metal anchors and swing through the city. The basics of Allomancy are already a playground of movement and combat.

We'll add more metals later. For now: push, pull, fly.

---

## Core Pillars

### 1. Movement as Combat
Every movement option is also a combat option. Flying into an enemy is damage. Pulling a metal weapon from their hands is disarming. Push off an enemy's metal armor to launch them.

### 2. Physics-Based Freedom
The Allomancy system is based on real physics (with mistborn twists). Momentum matters. Weight matters. Position matters. Learn the physics, master the movement.

### 3. The Ash-Soaked City
Luthadel is vertical, metal-rich, and oppressive. The environment should make you want to climb, jump, and fly through it.

### 4. One Mistborn Against the Empire
You're powerful but not invincible. Early game is about skill expression and traversal, not power fantasy.

---

## Game Modes (Planned)

### Campaign (Eventually)
Story mode following Era 1 events. Work with/rebet the crew. Fight the Lord Ruler.

### Arena (Sprint 1 Focus)
Simple arena with metal objects. Test all the push/pull mechanics. No enemies yet — just you and the physics.

### Survival (Sprint 2)
Endless waves of enemies. Use your limited metal reserves wisely.

### Stealth Challenges (Future)
Get from A to B without being seen. Use allomancy creatively.

---

## Player Character

### Starting Abilities
- Steel Push (right click)
- Iron Pull (left click)
- Allomantic Sight (Tab) — see blue lines

### Movement
- Walk (WASD)
- Sprint (Shift)
- Jump (Space)
- *Steelpush-boosted jump (future)*

### No Weapons
The player doesn't use weapons initially. All combat is through Allomancy.

---

## Enemies (Future)

### Skaa Soldier
- Basic melee
- No Allomancy
- Metal armor (can be pushed)

### Steel Inquisitor
- Has iron spikes (can use Allomancy!)
- Red glowing eyes
- Extremely dangerous

### Koloss
- Huge, strong
- Metal spikes through body
- Dumb but deadly

---

## Controls

| Action | Key | Notes |
|--------|-----|-------|
| Move | WASD | Standard third-person |
| Look | Mouse | Free camera |
| Sprint | Left Shift | Hold to run |
| Jump | Space | Basic jump |
| Iron Pull | Mouse Left | Hold to pull |
| Steel Push | Mouse Right | Hold to push |
| Allomantic Sight | Tab | Toggle blue lines |

---

## Scope Per Sprint

### Sprint 1 — Foundation
- Basic third-person movement
- Steel/Iron push/pull physics
- Blue lines (Allomantic Sight)
- Test arena with metal objects
- Metal reserve HUD

### Sprint 2 — Combat
- Basic enemies
- Coin pouch (unlimited coins to push)
- Steelpush-assisted jumping
- LineRenderer VFX upgrade

### Sprint 3 — World
- Luthadel city environment
- Vertical traversal
- Multiple metal types
- Wall-running with Allomancy

### Sprint 4 — Story (Future)
- Campaign intro
- Kelsier's crew as NPCs
- Lord Ruler encounter

---

## Questions for the Team

1. **Should we start with unlimited metal reserves or limited?**
   - Unlimited = focus on movement skill
   - Limited = focus on resource management

2. **How punishing should the game be?**
   - Roguelike deaths = high stakes
   - Checkpoint saves = accessible

3. **First target platform?**
   - PC only to start
   - Console ports later

4. **Visual style?**
   - Gritty ash-covered aesthetic
   - More stylized/colorful
   - Dark souls-esque

---

## References

- **Movement**: Assassin's Creed, Dying Light, Mirror's Edge
- **Combat Feel**: Warframe, DOOM
- **Allomancy Implementation**: Invested (github.com/austin-j-taylor/Invested)
- **Lore**: Coppermind wiki, official Mistborn books
