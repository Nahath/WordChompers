# Art Assets List

All character sprites are designed to fit within one grid cell. Based on a 1280×720 screen with a 6×6 grid, cells are approximately 160×100px. Sprites should be authored at **128×128px per frame** (square, power-of-two) and scaled in Unity to fit the cell. Spritesheets should arrange all frames for a single animation in a single horizontal row.

Animation frame counts and durations are specified per animation. Playback rate = frames ÷ duration. Animations marked **loops** play continuously; all others play once and hold the final frame until a new animation is triggered.

---

## Player Character

One spritesheet per animation (5 total).

| Animation | Frames | Duration | Loops | Notes |
|---|---|---|---|---|
| Idle - Mouth Closed | 8 | 2000ms | Yes | Used when the player's current cell has no word/letter |
| Idle - Mouth Open | 8 | 2000ms | Yes | Used when the player's current cell has a word/letter; mouth visually frames the right 75% of the cell where the word is displayed |
| Move | 4 | 80ms | No | Quick step cycle |
| Eat (valid chomp) | 4 | 120ms | No | Munching/swallowing motion |
| Sick | 5 | 150ms | No | Character looks ill; played on penalty (wrong chomp or monster encounter) |

**Design notes for player:**
- The left 25% of the cell is the player's body area; the right 75% is where words/letters appear, framed by the player's open mouth in the Idle (Mouth Open) animation.
- The player's mouth should visually open around the word area, not cover it.

---

## Monster Characters

Each of the 6 monster types needs its own spritesheet set. All share the same animation names and frame structure. Speed differences (Gorbler, Gallumpher, Zabyss) are handled by adjusting Unity playback rate — the spritesheets themselves are the same number of frames.

| Animation | Frames | Duration | Loops | Notes |
|---|---|---|---|---|
| Idle | 8 | 2000ms | Yes | |
| Move | 4 | 80ms | No | Gorbler plays this 30% faster; Gallumpher and Zabyss play it 40% faster |
| Eat | 5 | 180ms | No | Used when monster eats the player or another monster |

### Monster Types (one full spritesheet set each):

1. **Squiggler**
2. **Gorbler**
3. **Blagwerr**
4. **Scaredy**
5. **Gallumpher**
6. **Zabyss**

**Design note for all monsters:** When a monster is partially off the edge of the board (entering or exiting), only the portion within the grid is visible. Sprites should be designed so partial clipping at any edge still reads clearly.

---

## UI Elements

### Gear / Settings Icon
- 1 static image
- Displayed in the top-right corner of the game screen
- Recommended size: **48×48px**

### D-Pad (touchscreen only)
- 5 separate images: Neutral, Up Pressed, Down Pressed, Left Pressed, Right Pressed
- Or a single image with 4 directional arrow regions that visually depress on tap
- Recommended overall size: **160×160px**

### Chomp Button (touchscreen only)
- 2 images: Normal, Pressed
- Round, red button with the label "Chomp" (label can be rendered as UI text on top)
- Recommended size: **100×100px**

### Life Indicator
- 1 static image — a small version of the player character in a neutral pose (can reuse frame 1 of the player's Idle Mouth Closed animation, scaled down)
- Displayed once per extra life remaining, in a row below the grid
- Recommended size: **48×48px**

### Main Menu Buttons
Two buttons, each with 2 states (Normal, Pressed/Highlighted):
- **"1. Chomp Letters"** button
- **"2. Chomp Words"** button
- Recommended size: approximately **300×70px** each

### Pause / Options Menu Panel
- 1 opaque (non-transparent) rectangular panel, centered on screen, overlaid on top of the game
- Should be large enough to contain a volume slider and two buttons
- Recommended size: **500×300px**
- No border decoration required beyond the panel itself (styling is up to the artist)

### Volume Slider
- Slider track: 1 static image (horizontal bar), approximately **300×20px**
- Slider handle: 1 static image (draggable knob), approximately **30×30px**

### Options Menu Buttons
Two buttons, each with 2 states (Normal, Pressed/Highlighted):
- **"Return to Game"**
- **"Quit"**
- Recommended size: approximately **220×55px** each

---

## Background / Grid

### Game Board Background
- 1 static image, full screen: **1280×720px**
- Covers the entire screen; grid area, header area, and life display area all sit on top of this

### Grid Cell Tile
- 1 static image used for each of the 36 cells
- Recommended size: **160×100px** (to be tiled 6×6 by Unity)
- Should be a simple, readable background that contrasts with text

---

## Effects

### Fireworks Particle Sprite
- Used as the particle texture in Unity's Particle System for the game-completion animation (triggered when all 100 levels are cleared)
- 1 small sprite image: a simple bright star, sparkle, or burst shape
- Recommended size: **32×32px**
- The Particle System itself will handle motion, color variation, and quantity — no animation frames needed for this sprite
