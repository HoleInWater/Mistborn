# 🌑 MISTBORN ERA ONE — MASTER TODO

## ✅ DONE
- [x] GitHub repository setup
- [x] Unity project structure
- [x] AllomanticMetal enum (all 16 metals)
- [x] MetalReserve data class
- [x] AllomancerController core system
- [x] SteelPushAbility (physics push)
- [x] IronPullAbility (physics pull)
- [x] AllomanticTarget component
- [x] AllomanticSight (blue lines)
- [x] PlayerController (movement, jump, sprint)
- [x] PlayerCamera (third-person follow)
- [x] MetalHUD (reserve display)
- [x] AllomancyConstants

## 🔧 IN PROGRESS
- [ ] TestArena scene setup
- [ ] Metal object prefabs (coins, brackets)

## 📋 BACKLOG — SPRINT 1 (Foundation)
- [ ] Third-person player controller fine-tuning
- [ ] Basic placeholder character model
- [ ] Camera collision with geometry
- [ ] Metal object prefabs:
  - [ ] Metal coin (small, light, pushable)
  - [ ] Metal bracket (wall-mounted, anchored)
  - [ ] Metal railing (floor, for jumping off)
- [ ] HUD hookup in TestArena scene
- [ ] Playtesting push/pull force balance

## 📋 BACKLOG — SPRINT 2 (Combat)
- [ ] Enemy AI placeholder
- [ ] Coin pouch mechanic (shoot coins with Steel)
- [ ] Anchored Steelpush (launch self upward)
- [ ] Ironpull grab and throw
- [ ] LineRenderer VFX upgrade for blue lines

## 📋 BACKLOG — SPRINT 3 (World)
- [ ] City block environment (placeholder geometry)
- [ ] Vertical traversal mechanics
- [ ] Metal-rich environment design

## 💡 IDEAS / FUTURE
- [ ] Add remaining 14 metals (Pewter, Tin, Brass, etc.)
- [ ] Mistborn vs Misting character select
- [ ] Skaa vs Noble faction system
- [ ] Atium implementation
- [ ] Boss encounters with Allomantic enemies

---

## ❓ TEAM DECISIONS NEEDED

### Core Mechanics
1. **Push strength formula** — should we use weight-proportional force like in the books?
   - Lore: Push strength ∝ Allomancer's physical weight
   - Bigger Mistborn = stronger push

2. **Distance falloff** — how quickly does push strength decrease with distance?
   - Lore: Inversely proportional to distance until zenith point

3. **Flaring** — burning metal faster for more power?
   - Currently: Constant burn rate
   - Could add: Hold key for max power, drains faster

4. **Metal in bodies** — should we implement the rule that metal inside people can't be pushed?
   - Lore accurate but complex to implement

### UI/UX
5. **HUD layout** — where should metal reserves display?
6. **Camera FOV** — default zoom level for allomantic flight?
7. **Control scheme** — are the current bindings good?

### Scope
8. **Starting metal amounts** — how much Steel/Iron to start with?
9. **Reserve regeneration** — passive regen or fixed amounts?
10. **Coin pouch** — how many coins can player carry?

### Third-Person Feel
11. **Jump assist** — smooth landing or precise?
12. **Air control** — how much can player steer while flying?

---

## 📚 REFERENCE PROJECTS

### Invested (austin-j-taylor/Invested)
A complete Mistborn Unity game with working Allomancy physics.
- GitHub: https://github.com/austin-j-taylor/Invested
- Has: Working steel/iron physics, blue line VFX, multiple levels
- We can reference their physics calculations

### What we should borrow from Invested:
- Physics force calculations
- Blue line rendering (they use Volumetric Lines asset)
- Momentum conservation for flight

---

## 📖 LORE ACCURACY NOTES

### Steel (Coinshot)
- Pushes from "center of self" (not just body center)
- Push strength ∝ user's physical weight
- Push strength ∝ 1/distance to target
- Metal *inside* a person cannot be pushed (even invested metal)
- Blue lines exist on Spiritual Realm (can pass through walls!)
- Can identify specific metals/colors with steel lines
- Can push on specific *parts* of metal objects
- Can create steel bubble to deflect projectiles

### Iron (Lurcher)  
- Pulls metal toward "center of self"
- Same weight-proportional strength rules
- Same distance falloff rules

### Key Gameplay Implications
1. Heavier Allomancer = stronger push
2. Get closer for stronger push
3. Use anchored metal below to fly up
4. Blue lines reveal metal even through walls

---

## 🔗 COMMUNITY RESOURCES

- Coppermind Wiki: https://coppermind.net/wiki/Allomancy
- 17th Shard Forums: https://www.17thshard.com
- Arcanum (WoB): https://wob.coppermind.net
- Brandon Sanderson's FAQ: https://faq.brandonsanderson.com
