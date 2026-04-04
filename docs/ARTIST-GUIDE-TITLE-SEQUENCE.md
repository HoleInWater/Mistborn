# Artist Guide — Title Sequence Assets

This document lists every asset needed for the title sequence, organized by phase. The scene is built procedurally via **Ashwalker → Scenes → Build Title Sequence Scene** — grey-box geometry, lights, particles, cameras, and UI are all auto-generated. Your job is to replace the grey boxes with real art.

All assets go in `Assets/_Project/Art/TitleSequence/` (create subfolders as needed).

---

## How to Replace Grey-Box Objects

1. Run **Ashwalker → Scenes → Build Title Sequence Scene** to generate the layout
2. Open `Assets/_Project/Scenes/TitleSequence.unity`
3. Find the grey-box object in the Hierarchy (e.g., `MistyFieldScene → Hill`)
4. Select it, then either:
   - **Swap the mesh**: drag your model into the MeshFilter component
   - **Replace entirely**: delete the primitive, drop your prefab in the same position
5. Materials you create should use **HDRP/Lit** shader (our pipeline is HDRP)
6. Save the scene — the builder won't overwrite your changes unless you rebuild

---

## Global Settings

| Setting | Value |
|---|---|
| Render Pipeline | HDRP |
| Reference Resolution | 1920 × 1080 |
| World Scale | 1 Unity unit = 0.762 m = 2.5 ft |
| Art Style | Dark, gritty, ash-covered. No bright colors. Everything is muted, weathered, oppressive. |
| Time of Day | Night / pre-dawn. Very little direct sunlight. Mostly ambient + point lights. |
| Color Palette | Browns, dark greys, muted reds, steel blue-grey. The only bright color is orange (torches/lanterns) and blue (Metallurgic lines in the title). |

---

## Phase 1 — Misty Ash Field (0–9 seconds)

*Camera slowly pushes forward across a desolate field outside Cinderhold. Black fades in to reveal this.*

### Terrain / Ground

| Asset | Description | Priority |
|---|---|---|
| **Ground terrain** | Replace the flat plane with sculpted terrain or a landscape mesh. Rolling, uneven ash-covered earth. No grass — everything is dead. Cracked, dry, grey-brown dirt with ash deposits. | HIGH |
| **Ground texture** | Tiling material: ash-covered dirt. Diffuse + normal map. Color: dark brown-grey (#1A1612). Roughness: 0.9. | HIGH |
| **Ash deposit texture** | Lighter grey patches on the ground where ash has accumulated. Can be a decal or second UV layer. | MEDIUM |
| **Dirt path texture** | Slightly lighter, compacted earth for the path the camera follows. | LOW |

### Skybox

| Asset | Description | Priority |
|---|---|---|
| **Custom skybox** | Dark overcast sky. Heavy ash clouds, no stars, no blue. Sepia/dark red tint near horizon (ash-filtered sunlight). The red sun is barely visible through the haze, low on the horizon. Use an HDRI or 6-sided cubemap. | HIGH |

### Environment Objects

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **Rocks** | Weathered volcanic rocks, dark grey-brown. 3–4 variants at different sizes (0.2m to 1m). Low-poly is fine — they're mostly silhouettes. | `Rock` | MEDIUM |
| **Dead tree stumps** | Charred, twisted stumps. No leaves, no branches above 1m. Black/dark brown wood. | `DeadTreeStump` | MEDIUM |
| **Dead shrubs** | Clusters of dead sticks/twigs poking from the ground. Dry, brittle, brown. | `Twig` | LOW |
| **Broken fence** | Weathered wooden fence posts (tilted, some fallen). Dark wood, splintering. | `FencePost`, `FallenRail` | LOW |
| **Ruined cart** | Abandoned wooden cart, one wheel broken, tilted. Rotting wood, rusted iron axle. | `CartBed`, `CartWheel`, `CartAxle` | LOW |
| **Collapsed ruin** | Remains of an old stone structure. 2 partial walls at angles, rubble scattered around. Grey stone, crumbling mortar. | `RuinWall`, `Rubble` | MEDIUM |
| **Lowborn shanties** | Lean-to shelters made of scrap wood and cloth. Poles, slanted roof panels, ragged blankets. These are where lowborn live outside the city. | `ShantyPole`, `ShantyRoof`, `Blanket` | MEDIUM |
| **Scattered coins** | 5 small copper discs on the ground (Ashwalker calling card). Copper color, slightly tarnished. Only ~2cm diameter. | `Coin` | LOW |

### Distant Silhouettes

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **Distant hills / mountains** | Dark silhouette shapes on the horizon. Barely visible through fog. Multiple overlapping ridgelines at different distances. | `Hill` | LOW |
| **Ashmount** | Taller volcanic mountain silhouette, center-back of the horizon. Faint red-orange glow at the base (lava). Ash plume rising from the peak (use the existing ember particle system). | `Hill` (tallest one at z=120) | MEDIUM |
| **Distant Cinderhold** | City silhouette on the horizon (z=70). Row of dark building shapes with one tall spire in the center (Thornspire hint). Very dark — almost blends into the fog. | `DistantBuilding`, `DistantThornspireHint` | LOW |

### Particle Systems (ALREADY BUILT — tweak only)

| System | What It Does | Tweak If Needed |
|---|---|---|
| `AshParticles` | Grey-brown flakes drifting down with wind noise | Assign a small ash flake texture for better look |
| `MistParticles` | Large translucent clouds at ground level, fading in/out | Assign a soft cloud/smoke texture |
| `EmberParticles` | Glowing orange specks near the ashmount, floating upward | Looks good with default particle, but a spark texture helps |

---

## Phase 2 — Company Logos (~9 seconds)

*Logos appear over the misty field (field stays visible behind them).*

| Asset | Description | Where to Put It | Priority |
|---|---|---|---|
| **Crimson Blade Interactive logo** | Studio logo. PNG with transparency, minimum 512×512. Will be displayed as a UI Image centered on screen. | `CrimsonBladeLogoGroup → LogoImage` (enable the GameObject, assign sprite) | HIGH |
| **the original IP holder logo** | the original author's company logo. PNG with transparency. Only if/when approved. | `the original authorLogoGroup → LogoImage` (enable the GameObject, assign sprite) | HIGH (when approved) |
| **Logo animations** (optional) | If you want the logos to animate in (scale up, particle reveal, etc.), create Animator Controllers with a "Play" trigger. Assign to the `Animator` field on the controller. | `crimsonBladeLogoAnimator` / `sandersonLogoAnimator` | LOW |

---

## Phase 3 — Cinderhold Streets (~28 seconds)

*Camera dolly pushes through a dark Cinderhold street. Credits roll.*

### Buildings

| Asset | Description | Count Needed | Priority |
|---|---|---|---|
| **Stone building — dark** | Dark grey-brown stone facade. 2-3 story. Small windows, heavy door. Ash stains running down from the roof. | 3–4 variants | HIGH |
| **Stone building — medium** | Warmer brown stone. Slightly different window layout. | 2–3 variants | HIGH |
| **Stone building — reddish** | Brick-like reddish stone. Noble district building — slightly nicer. | 1–2 variants | MEDIUM |
| **Stone building — grey** | Cool grey stone. Taller, institutional look (Prelate offices?). | 1–2 variants | MEDIUM |
| **Slate roof** | Blue-grey slate tiles. Flat or slightly angled. Modular piece that sits on top of buildings. | 1 modular piece | HIGH |
| **Clay tile roof** | Reddish-brown clay tiles. Different from slate for variety. | 1 modular piece | HIGH |
| **Window (lit)** | Small recessed window with warm orange glow inside. Can be a simple emissive plane. | Reuse | MEDIUM |
| **Window (dark)** | Shuttered window. Dark wood shutters closed. | Reuse | LOW |
| **Door** | Heavy wooden door, dark stained. Iron hinges and handle. | 1–2 variants | MEDIUM |

### Street Elements

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **Cobblestone ground** | Tiling cobblestone texture for the street center. Worn, uneven, ash in the gaps. | `StreetGround` (center) | HIGH |
| **Dirt ground** | Tiling texture for the street edges. Muddy, ashy. | `StreetGround` (sides) | HIGH |
| **Metal lantern** | Wrought iron bracket + glass/metal lantern body. Hangs from building walls. Point light inside (already placed). | `LanternBracket`, `LanternBody` | HIGH |
| **Wall torch** | Metal bracket holder + burning torch head. More rustic than lanterns. | `TorchHolder`, `TorchFlame` | MEDIUM |
| **Barrel** | Wooden barrel, iron bands. Dark wood, weathered. | `Barrel` | MEDIUM |
| **Crate** | Wooden shipping crate, slightly lighter wood than barrels. | `Crate` | MEDIUM |
| **Market stall** | Wooden table with legs + draped tarp/canvas on top (closed for night). | `StallTable`, `StallTarp` | LOW |
| **Hanging sign** | Metal bracket + wooden sign board. Weathered, text unreadable from distance. | `SignBracket`, `SignBoard` | LOW |
| **Sewer grate** | Iron grate with bars, set into the street. Metal — important for Metallurgy world. | `SewerGrate`, `GrateBar` | LOW |
| **Archway** | Stone bridge/archway connecting buildings overhead. Heavy stone. | `Archway` | MEDIUM |
| **Clothesline** | Thin rope strung between buildings with ragged cloth pieces hanging. | `Clothesline`, `Cloth` | LOW |
| **Puddle** | Small reflective water puddle in the street gutter. Can be a simple reflective plane. | `Puddle` | LOW |
| **Stone steps** | 3-step stone stoop in front of building doors. | `Step` | LOW |
| **Awning** | Cloth canopy over a door. Faded colors (red, purple, tan). | `Awning` | LOW |
| **Metal debris** | Scattered nails, scraps, bent metal on the ground. Tiny — just glints in torchlight. | `MetalScrap` | LOW |

### Characters (Silhouettes)

These are dark silhouettes — they don't need faces, detailed clothing, or animation. Just recognizable shapes.

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **Lowborn (crouching)** | Huddled figure, knees drawn up, head down. Ragged clothing. | `LowbornSilhouette` | MEDIUM |
| **Lowborn (standing)** | Thin figure in a rough cloak, slightly hunched posture. | `LowbornSilhouette` | MEDIUM |
| **Guard** | Wider armored figure, helmet with crest, holding a spear upright. | `GuardSilhouette` | MEDIUM |
| **Prelate** | Tall robed figure, bald head, visible tattoo lines on face. Distinctive from guards and lowborn. | `PrelateSilhouette` | MEDIUM |
| **Noble carriage** | Dark polished wooden carriage with gold trim, 4 wheels. Parked on the street. | `CarriageBody`, `CarriageWheel` | LOW |
| **Stray animals** | Small dog or cat silhouette. Very simple — just body + head shapes. | `StrayAnimal` | LOW |

### Particle Systems (ALREADY BUILT)

| System | Texture Needed |
|---|---|
| `AshParticles` | Small ash flake (2–4px white sprite with soft edges) |
| `MistParticles` | Soft cloud/smoke puff (64×64 soft white circle) |
| `ChimneySmoke` | Same smoke puff, or a wispy variant |
| `DrizzleParticles` | Thin rain streak (1×8px white line) |

---

## Phase 4 — Thornspire Aerial Pan (~45 seconds)

*Camera orbits above Cinderhold at night, centered on Thornspire.*

### Thornspire (The Hill of a Thousand Spires)

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **Central spire** | Tallest tower, ~35m. Dark steel/stone, smooth tapering to a sharp point. The Ashen King's personal tower. | `Spire` (tallest) | HIGH |
| **Major spires (ring of 8)** | 18–28m tall. Varied heights. Dark metal with architectural detail bands. Slight lean for character. | `Spire` (inner ring) | HIGH |
| **Minor spires (ring of 12)** | 10–18m. Thinner, shorter. Same material family but more weathered. | `Spire` (outer ring) | HIGH |
| **Base platform** | Large stone/metal platform all spires sit on. ~16m radius. Dark, imposing. | `ThornspireBase` | MEDIUM |
| **Perimeter wall** | Connecting walls between outer spires with battlements on top. Dark stone + metal caps. | `PerimeterWall`, `Battlement` | MEDIUM |
| **Grand gate** | Two massive pillars with sphere caps, stone arch overhead, dark iron doors. Facing south. | `GatePillar`, `GateArch`, `GateDoorL/R` | MEDIUM |
| **Courtyard** | Flat stone courtyard inside the walls. Slightly different color/texture from surroundings. | `Courtyard` | LOW |

### Iconic Silhouettes

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **Iron Sentinel** | Standing on top of the central spire. Tall, gaunt, long coat trailing. Axe in hand. TWO METAL SPIKES through the eyes (most important detail). This is THE shot of the intro. | `SentinelSilhouette` | HIGH |
| **Ashwalker on rooftop** | Crouching figure on a city rooftop. Hooded, ashcloak tassels draping behind. Visible from aerial if you look carefully. | `RooftopAshwalker` | MEDIUM |

### City Layout

| Asset | Description | Grey-box Name | Priority |
|---|---|---|---|
| **City buildings** | Simple block-out buildings of varied heights (3–10m). 7 different wall colors, 4 roof colors. ~150 total. Keep low-poly — they're seen from far above. | `CityBuilding`, `CityRoof` | MEDIUM |
| **City wall** | Ring wall around the outer city edge. 8m tall, 1m thick. Corner towers (cylindrical, taller). | `CityWall`, `WallTower` | MEDIUM |
| **Noble keeps** | 4 larger buildings with corner tower + spire + courtyard walls. Represent the Great Houses. | `NobleKeep`, `KeepTower` | LOW |
| **Canals** | 3 water channels cutting through the city. Dark water surface, stone retaining walls. | `Canal`, `CanalWall` | MEDIUM |
| **Canal bridges** | Stone bridges with metal railings crossing the canals. | `CanalBridge`, `BridgeRail` | LOW |
| **Dock/pier** | Wooden platform over a canal. Cargo crates, barrel. | `DockPlatform`, `DockPost` | LOW |
| **Roads** | 4 dark strips radiating from Thornspire. Visible from above as darker ground. | `Road` | LOW |

### Particle Systems (ALREADY BUILT)

| System | Texture Needed |
|---|---|
| `AshParticles` (high altitude) | Same ash flake as Phase 1 |
| `MistParticles` (seen from above) | Larger, softer cloud puffs rolling through streets |
| `ChimneySmoke` (foundries) | Wispy dark smoke |

### Metallurgic Line Flashes

Two brief blue lines flash between rooftops during this phase — they're `LineRenderer` objects. If they look wrong, assign a simple additive blue material. Color: `#4D8CFF` with alpha 0.6.

---

## Phase 5 — MISTBORN Title (Rock Drop)

| Asset | Description | Priority |
|---|---|---|
| **Custom font** | Angular serif font for "MISTBORN". Trajan Pro, Cinzel, or a custom Ashwalker-style typeface. Import the .ttf/.otf, then **Window → TextMeshPro → Font Asset Creator** to generate the TMP font asset. Assign to `TitleGroup → TitleText`. | HIGH |
| **Post-processing volume** | Add a Volume to the scene with Bloom (intensity 0.5–1.0) so the blue title text actually glows. Also add Vignette (intensity 0.3) for darkened edges. | HIGH |
| **Blue line particles** (optional) | Thin blue particle trails radiating from the title after it's drawn. Like Metallurgic steel lines shooting outward. | LOW |

---

## Ashcloak Wipe Transition (After Title)

A dark silhouette runs across the screen, followed by a black panel with ragged tassels.

| Asset | Description | Priority |
|---|---|---|
| **Ashwalker run cycle** (optional) | If you want the silhouette to actually animate (legs moving, cloak flowing), create a sprite sheet or UI animation. Currently it's a static dark shape that slides across. | LOW |
| **Ashcloak tassel texture** | Ragged torn cloth strips. Currently solid black UI rectangles. A proper texture with torn edges would look much better. Transparent PNG, dark grey/black, ~256×64 per strip. | MEDIUM |

---

## Texture Specifications

| Texture | Resolution | Format | Notes |
|---|---|---|---|
| Ground (dirt/ash) | 1024×1024 | PNG, diffuse + normal | Tiling, seamless edges |
| Cobblestone | 1024×1024 | PNG, diffuse + normal | Worn, uneven, ash in gaps |
| Stone wall | 512×512 | PNG, diffuse + normal | 3–4 color variants |
| Wood | 512×512 | PNG, diffuse + normal | Dark, weathered, grain visible |
| Slate roof | 512×512 | PNG, diffuse + normal | Blue-grey, overlapping tiles |
| Clay roof | 512×512 | PNG, diffuse + normal | Reddish-brown, curved tiles |
| Metal (iron/steel) | 512×512 | PNG, diffuse + normal + metallic | Dark steel, some rust |
| Skybox | 2048×2048 per face or HDRI | HDR/EXR | Dark ash sky, red horizon |
| Particle (ash) | 32×32 | PNG with alpha | Soft white flake |
| Particle (smoke) | 64×64 | PNG with alpha | Soft circle, feathered edge |
| Particle (rain) | 4×32 | PNG with alpha | Thin white vertical streak |
| Logo (Crimson Blade) | 512×512+ | PNG with alpha | Your studio logo |
| Font | .ttf or .otf | — | Serif/angular style |

---

## File Organization

```
Assets/_Project/Art/TitleSequence/
├── Textures/
│   ├── Ground_Ash_D.png        (diffuse)
│   ├── Ground_Ash_N.png        (normal)
│   ├── Cobblestone_D.png
│   ├── Cobblestone_N.png
│   ├── StoneWall_Dark_D.png
│   ├── StoneWall_Med_D.png
│   ├── StoneWall_Red_D.png
│   ├── Wood_D.png
│   ├── Metal_D.png
│   ├── Metal_M.png             (metallic)
│   ├── Slate_D.png
│   ├── ClayTile_D.png
│   └── Skybox/
│       └── AshSky_HDRI.exr
├── Models/
│   ├── Building_Dark.fbx
│   ├── Building_Med.fbx
│   ├── Building_Red.fbx
│   ├── Thornspire_Spire.fbx
│   ├── Thornspire_Base.fbx
│   ├── Lantern.fbx
│   ├── Barrel.fbx
│   ├── Crate.fbx
│   └── RuinedCart.fbx
├── Characters/
│   ├── Silhouette_Lowborn.fbx
│   ├── Silhouette_Guard.fbx
│   ├── Silhouette_Prelate.fbx
│   ├── Silhouette_Sentinel.fbx
│   └── Silhouette_Ashwalker.fbx
├── Particles/
│   ├── AshFlake.png
│   ├── SmokePuff.png
│   └── RainStreak.png
├── UI/
│   ├── CrimsonBladeLogo.png
│   ├── the original IP holderLogo.png
│   └── AshcloakTassel.png
└── Fonts/
    └── AshwalkerTitle.ttf
```

---

## Priority Summary

**Do these first (biggest visual impact):**
1. Skybox (removes black sky, sets the mood)
2. Ground texture (removes flat grey ground)
3. Custom font for MISTBORN title
4. Crimson Blade logo
5. Post-processing volume (Bloom + Vignette)
6. Cobblestone street texture

**Do these next (building the world):**
7. Stone wall textures (3–4 variants)
8. Thornspire spire model
9. Lantern model
10. Roof textures (slate + clay)
11. Particle textures (ash, smoke, rain)
12. Iron Sentinel silhouette model

**Polish (when everything else is done):**
13. Character silhouette models
14. Carriage, cart, market stall details
15. Metal debris, coins
16. Ashcloak tassel textures
17. Logo animations
18. Blue line particles for title

---

*Last updated: April 2026*
*Scene builder: Ashwalker → Scenes → Build Title Sequence Scene*
