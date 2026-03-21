# Unity UI Setup Guide

## Creating the Metal HUD

### 1. Create Canvas
1. Right-click in Hierarchy → UI → Canvas
2. Set Canvas Scaler:
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080
   - **Match**: 0.5 (halfway)

### 2. Create Metal Reserve Panel
1. Right-click Canvas → UI → Panel
2. Name it `MetalReservePanel`
3. Anchor to bottom-left
4. Set position: X = 200, Y = 100
5. Width = 250, Height = 150

### 3. Create Steel Reserve UI
1. Right-click MetalReservePanel → UI → Slider
2. Name it `SteelSlider`
3. Anchor to top-left of panel
4. Position: X = 10, Y = -20
5. Width = 100, Height = 20

### 4. Create Steel Text
1. Right-click SteelSlider → UI → Text
2. Name it `SteelText`
3. Anchor to right of slider
4. Position: X = 110, Y = 0
5. Text: "100"
6. Font Size: 24
7. Color: White

### 5. Create Iron Reserve UI
1. Right-click MetalReservePanel → UI → Slider
2. Name it `IronSlider`
3. Anchor below Steel slider
4. Position: X = 10, Y = -60

### 6. Create Iron Text
1. Right-click IronSlider → UI → Text
2. Name it `IronText`
3. Anchor to right of slider
4. Position: X = 110, Y = 0
5. Text: "100"

---

## Connecting UI to Script

### MetalHUD Script
1. Select MetalReservePanel
2. Add Component: `MetalHUD`
3. Drag these from Hierarchy to Inspector:

| Inspector Field | Drag From Hierarchy |
|----------------|---------------------|
| Allomancer Controller | (leave empty - auto-finds) |
| Steel Fill Image | SteelSlider → Fill |
| Steel Text | SteelText |
| Steel Burning Indicator | SteelSlider (or create Image) |
| Iron Fill Image | IronSlider → Fill |
| Iron Text | IronText |
| Iron Burning Indicator | IronSlider (or create Image) |

### MetalReserveUI Script (Alternative)
1. Select MetalReservePanel
2. Add Component: `MetalReserveUI`
3. Drag Sliders and Text objects to their fields
4. Set Colors:
   - **Steel Color**: (0.3, 0.5, 1.0) - Blue
   - **Iron Color**: (0.6, 0.6, 0.6) - Grey
   - **Burning Color**: (0.2, 0.8, 1.0) - Cyan

---

## Creating Health Bar

### 1. Create Health Panel
1. Right-click Canvas → UI → Panel
2. Name it `HealthPanel`
3. Anchor to top-center
4. Height = 30

### 2. Create Health Fill
1. Right-click HealthPanel → UI → Slider
2. Name it `HealthSlider`
3. Set Min Value: 0, Max Value: 100
4. Remove handle: destroy child "Handle Slide Area"

### 3. Set Fill Color
1. Select HealthSlider → Fill
2. Image: Solid Color
3. Color: Green (0, 1, 0)

---

## Creating Pause Menu

### 1. Create Pause Panel
1. Right-click Canvas → UI → Panel
2. Name it `PauseMenu`
3. Make sure Anchor is center-stretched
4. Set Active to: **OFF** (start hidden)

### 2. Create Resume Button
1. Right-click PauseMenu → UI → Button
2. Name it `ResumeButton`
3. Anchor: Center
4. Position: Y = 50
5. Text: "Resume"

### 3. Create Other Buttons
1. Copy ResumeButton (Ctrl+D)
2. Move down for each button:
   - Options (Y = -10)
   - Controls (Y = -70)
   - Main Menu (Y = -130)
   - Quit (Y = -190)

### 4. Connect to Script
1. Select Canvas
2. Add Component: `PauseMenu`
3. Drag PauseMenu panel to Main Pause Panel
4. Drag each button to its field

---

## Tips

- **Canvas always on top**: UI renders last (on top of 3D)
- **Raycast Target**: Disable on background images for click-through
- **Sorting Order**: Use for layered UI elements
- **Preserve Aspect**: Check on images that should scale uniformly
- **UI Layer**: Make sure Camera has "UI" layer enabled

---

## Common Issues

**UI not visible?**
- Check if Canvas GameObject is active
- Check if Panel/Image has alpha > 0
- Check Canvas render mode (Screen Space - Overlay recommended)

**UI stretches weirdly?**
- Use Anchor presets (Alt+click in Rect Transform)
- Set Canvas Scaler to "Scale With Screen Size"

**Text looks blurry?**
- Increase Dynamic Pixels Per UGUI in Canvas
- Use higher resolution textures
