# Word Chompers Style Guide

## Target Audience
Ages 4-9, educational game focused on literacy and letter/word recognition.

---

## Fonts

### Primary Font: Headings & Game Title
**Font:** Bubblegum Sans (Google Fonts - Free)
**Fallback:** Poppins Bold
**Usage:** Game title "Word Chompers", level headers, "Game Over" text
**Characteristics:** Playful, rounded, whimsical but highly readable

### Secondary Font: Grid Words & UI Text
**Font:** Andika (Google Fonts - Free, designed for literacy/beginning readers)
**Fallback:** Open Sans
**Usage:** Words in grid cells, letters in grid cells, button labels, level indicators
**Characteristics:** Clear letterforms, tall x-height, designed specifically for children learning to read

### Font Sizing Guidelines
| Element | Size (relative to screen height) |
|---------|----------------------------------|
| Game Title | 8-10% |
| Level Header ("Eat All Words...") | 4-5% |
| Level Number | 2.5% |
| Grid Cell Words | 3-4% (scale down for longer words) |
| Grid Cell Letters | 5-6% |
| Button Text | 3.5% |
| Lives Display | 2% |

### Typography Rules
- Never use ALL CAPS for words children need to read
- Minimum font weight: Regular (400) - avoid thin fonts
- Letter spacing: Slightly increased (+5%) for better readability
- Line height: 1.4x font size minimum

---

## Color Palette

### Primary Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Sunshine Yellow | #FFD93D | 255, 217, 61 | Game title, success states, positive feedback |
| Sky Blue | #6BCB77 | 107, 203, 119 | Calm UI elements, Squiggler monster placeholder |
| Playful Purple | #9B5DE5 | 155, 93, 229 | Buttons, interactive elements, Blagwerr monster placeholder |
| Coral Orange | #FF6B6B | 255, 107, 107 | "Chomp" button, attention elements, Gorbler monster placeholder |

### Secondary Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Soft Teal | #4ECDC4 | 78, 205, 196 | Secondary UI, Gallumpher monster placeholder |
| Cream White | #FFF8E7 | 255, 248, 231 | Grid cell text, UI text on dark backgrounds |
| Deep Navy | #2C3E50 | 44, 62, 80 | Main Menu background, Zabyss monster placeholder |
| Soft Gray | #95A5A6 | 149, 165, 166 | Disabled states, subtle UI elements |

### Functional Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Success Green | #27AE60 | 39, 174, 96 | Correct word eaten, positive feedback |
| Error Red | #E74C3C | 231, 76, 60 | Wrong word eaten, life lost |
| Player Highlight | #F39C12 | 243, 156, 18 | Reserved — not currently used for cell highlighting |

---

## Background Colors

| Screen | Background |
|--------|------------|
| Main Menu | Solid: Deep Navy (#2C3E50) |
| Gameplay | Solid: Black — applies both outside and inside the grid |
| Overlay panels | Semi-transparent black (Game Over: 85%, Level Complete: 80%, Pause: 85%) |

---

## Grid

### Grid Cells
- **Background:** Black (matches overall game background — no separate cell fill color)
- **Text:** Cream White (#FFF8E7) — rendered by TextMeshPro
- **Player occupied cell:** No separate color — player sprite indicates position
- **Borders:** Handled entirely by the GridLines shader (see below)

### Grid Lines
Procedural shader (`Assets/Shaders/GridLines.shader`) renders lines as half-cylinders with a dome at intersections. No art assets required.

| Property | Value |
|----------|-------|
| Line width | 8px (logical canvas pixels) |
| Base color (edge) | #330D59 — dark purple |
| Crest color (centre highlight) | #B373F2 — light purple |
| Cross-section profile | Circular (sqrt gradient) — cylindrical on straight segments, hemispherical dome at intersections |

---

## UI Components

### Buttons
- **Background:** Playful Purple (#9B5DE5)
- **Text:** Cream White (#FFF8E7)
- **Pressed State:** 10% darker (aspirational — requires sprite or shader)
- **Hover State:** 5% lighter (aspirational)

### Chomp Button (mobile only)
- **Background:** Coral Orange (#FF6B6B)
- **Text:** Cream White (#FFF8E7)
- **Shape:** Circle (aspirational — requires sprite)

### D-Pad (mobile only)
- **Background:** White at 25% opacity
- **Border Radius:** aspirational

### Lives Display
- Life icons are small player sprite instances in a horizontal row
- **Lost life:** 50% opacity grayscale (aspirational)

---

## Contrast

| Combination | Ratio |
|-------------|-------|
| Cream White (#FFF8E7) on Black | ~21:1 |
| Deep Navy (#2C3E50) on Cream White (#FFF8E7) | ~10.5:1 |

All text meets WCAG AA minimum (4.5:1).

---

## Monster Placeholder Colors
These are flat-color placeholders used until sprite assets are ready.
Final monster designs should have distinct silhouettes readable in grayscale.

| Monster | Placeholder Color | Unlocks |
|---------|------------------|---------|
| Squiggler | Sky Blue (#6BCB77) | Level 1 |
| Gorbler | Coral Orange (#FF6B6B) | Level 1 |
| Scaredy | Sunshine Yellow (#FFD93D) | Level 1 |
| Blagwerr | Playful Purple (#9B5DE5) | Level 15 |
| Gallumpher | Soft Teal (#4ECDC4) | Level 25 |
| Zabyss | Deep Navy (#2C3E50) | Level 35 |

---

## Player Character

- Rendered as a UI Image (Canvas/Screen Space Overlay) — not a world-space sprite
- Image `color` is white so the sprite renders at its natural colors
- Size: 80×80 logical pixels
- Animated via Animator Controller (`Assets/Animations/MuncherAnimator.controller`)

### Animator States
Code calls `animator.Play("StateName")` directly — no transitions or parameters needed in the controller.

| State | Loop | Trigger |
|-------|------|---------|
| Idle | Yes | Default; standing on empty cell |
| IdleWord | Yes | Standing on a cell with a word |
| Move | No | Each step taken |
| Chomp | No | Chomp action |
| Sick | No | Wrong chomp |

---

## Unity Implementation Notes

### Color Definition
```csharp
public static class GameColors
{
    public static Color SunshineYellow  = new Color(1f,     0.851f, 0.239f);
    public static Color SkyBlue         = new Color(0.42f,  0.796f, 0.467f);
    public static Color PlayfulPurple   = new Color(0.608f, 0.365f, 0.898f);
    public static Color CoralOrange     = new Color(1f,     0.42f,  0.42f);
    public static Color SoftTeal        = new Color(0.306f, 0.804f, 0.769f);
    public static Color CreamWhite      = new Color(1f,     0.973f, 0.906f);
    public static Color DeepNavy        = new Color(0.173f, 0.243f, 0.314f);
    public static Color SoftGray        = new Color(0.584f, 0.647f, 0.651f);
    public static Color SuccessGreen    = new Color(0.153f, 0.682f, 0.376f);
    public static Color ErrorRed        = new Color(0.906f, 0.298f, 0.235f);
    public static Color PlayerHighlight = new Color(0.953f, 0.612f, 0.071f);
    public static Color EmptyCell       = new Color(0.941f, 0.902f, 0.827f);
}
```

### Font Import
```
1. Download Bubblegum Sans and Andika from Google Fonts
2. Import .ttf files into Assets/Fonts/
3. Create TextMeshPro font assets for each
4. Set Atlas Resolution to 2048x2048 for clarity at all sizes
```

### Scene Architecture
- All gameplay elements (grid, player, monsters) are Unity UI components on a Screen Space - Overlay Canvas
- Reference resolution: 1280×720, CanvasScaler ScaleWithScreenSize, match 0.5
- Grid: 6×6, each cell 160×100 logical pixels, total 960×600, centered with 80px header above and 40px lives strip below
