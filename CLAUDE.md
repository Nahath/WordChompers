# Word Chompers — Unity Project

## Project Overview
Educational word/letter game for ages 4–9, based on the MECC "Word Munchers" style.
Full spec lives at `C:\git\WordChompers\WordChompers.md`.
Style guide: `C:\git\WordChompers\StyleGuide.md`.
Art asset specs: `C:\git\WordChompers\ArtAssetsList.md`.
Audio asset specs: `C:\git\WordChompers\AudioAssetsList.md`.

## Unity Environment
- **Version:** 6000.3.13f1 (Unity 6)
- **Template:** 2D Core
- **Target resolution:** 1280×720, 16:9 landscape, fixed aspect ratio
- **Input system:** Legacy (NOT the new Input System package)
- **TextMeshPro:** 3.0.6 (already in manifest)
- **MCP bridge:** CoplayDev unity-mcp is installed — always check the Unity console through MCP before assuming code is correct

## Scene Structure
Two scenes, both must be in Build Settings (MainMenu index 0, Game index 1):
- `Assets/Scenes/MainMenu.unity` — title screen, character showcase, mode select buttons
- `Assets/Scenes/Game.unity` — all gameplay (reused for all levels, no per-level scenes)

## Persistent Singletons (DontDestroyOnLoad)
These live in the MainMenu scene and persist into Game:
- `GameManager` — state machine, lives, level, mode
- `AudioManager` — SFX + sequential spoken audio
- `GameDataLoader` — loads JSON from StreamingAssets

## Script Architecture
```
Assets/Scripts/
  Data/          CategoryData.cs, WordData.cs, LevelEntry.cs, GameDataLoader.cs
  Core/          GameManager.cs, AudioManager.cs, GameColors.cs
  Grid/          GridManager.cs, GridCell.cs
  Player/        PlayerController.cs
  Monsters/      MonsterBase.cs (abstract), 6 subclasses, MonsterSpawner.cs
  UI/            MainMenuUI.cs, GameplayUI.cs, PauseMenuUI.cs,
                 GameOverUI.cs, LevelCompleteUI.cs, GameCompleteUI.cs,
                 DPadButton.cs, ChompButton.cs
  GameSceneLoader.cs
```

## Key Singletons and How to Get Them
```csharp
GameManager.Instance
AudioManager.Instance
GameDataLoader.Instance
GridManager.Instance
PlayerController.Instance
MonsterSpawner.Instance
```

## GameManager State Machine
States: `MainMenu → Playing ↔ Paused`, `Playing → LevelComplete → Playing`,
`Playing → GameOver`, `Playing → GameComplete`

`GameManager.CanPlayerAct` — check this before processing any player input.

## Game Modes
`GameMode.ChompWords` — uses JSON word/category/level data
`GameMode.ChompLetters` — no data files needed, letters generated in code

## Data Files (StreamingAssets)
```
Assets/StreamingAssets/Data/
  categories.json   { "categories": [{ "name", "minimumDifficulty", "levelHeader" }] }
  words.json        { "words": [{ "word", "difficulty", "categories": [] }] }
  levels.json       { "levels": [{ "level", "difficulty" }] }
```
52 words across difficulties 1–5. 100 levels. 5 categories: Food, Animals, Reptiles, Mammals, Birds.

## Audio System
All clips in `Assets/Resources/Audio/` — loaded at runtime via `Resources.Load<AudioClip>`.
```
Audio/SFX/          sfx_player_move, sfx_player_chomp_valid, sfx_level_complete,
                    sfx_game_over, sfx_monster_eat, sfx_player_scream, sfx_menu_button_press
Audio/Spoken/       spoken_is_not_a, spoken_chomp_the_letter, spoken_you_chomped, spoken_only_chomp
Audio/Categories/   header_{name}, category_{name}   (lowercase, e.g. header_food)
Audio/Words/        word_{word}                       (lowercase, e.g. word_apple)
Audio/Letters/      letter_{a-z}
```
`AudioManager.PlaySFX("SFX/sfx_player_move")` — pass the path relative to `Audio/`.
`AudioManager.PlaySequence(string[])` — plays clips in order, waiting for each to finish.

## Animator States
Code calls `animator.Play("StateName")` directly — no parameters or transitions needed in the controller.
**Player states:** `Idle`, `IdleWord`, `Move`, `Chomp`, `Sick`
**Monster states:** `Idle`, `Move`, `Eat`
Idle/IdleWord/Idle(monster) should loop. Move/Chomp/Sick/Eat should not loop (hold last frame).

## Monster Types and Unlock Levels
| Class | Unlocks | Speed |
|---|---|---|
| SquigglerMonster | Level 1 | 1× |
| GorblerMonster | Level 1 | 1/0.7× (30% faster) |
| ScaredyMonster | Level 1 | 1× |
| BlagwerrMonster | Level 15 | 1× |
| GallumpherMonster | Level 25 | 1/0.6× (40% faster, extends Squiggler) |
| ZabyssMonster | Level 35 | 1/0.6× (40% faster, extends Blagwerr) |

## Grid
6×6 = 36 cells. `GridManager` instantiates `GridCell` prefabs into a `GridLayoutGroup` (6 cols, 160×100 cell size).
Player position: `(row, col)` — (0,0) is top-left. Off-board is row -1/6 or col -1/6 (for monster entry).
`GridManager.GetCellCenter(row, col)` returns canvas-space position, works for off-board values (extrapolates).

## Player Movement
- 200ms cooldown between moves
- Movement: Arrow keys / WASD / gamepad axis / touch D-pad
- Chomp: Space / Fire1 / touch Chomp button
- Multiple held directions → random choice each cooldown tick

## Monster Spawning
- Spawn interval: `20 - (level / 18)` seconds, minimum 5s
- Spawn positions: non-corner edge cells, Manhattan distance ≥ 4 from player
- Monster move interval: `2 * 0.8^floor(level/10)` seconds (slows every 10 levels)

## Canvas / UI Layout (Screen Space - Overlay, 1280×720 reference)
```
Header area:    top ~80px  — level number (top-left) + level header (top-center)
Grid area:      middle     — GridPanel (GridLayoutGroup)
Lives area:     bottom     — HorizontalLayoutGroup of life icons
Gear button:    top-right  — opens pause menu
DPad:           left of grid (mobile only)
Chomp button:   right of grid (mobile only)
Overlays:       PauseMenu, GameOver, LevelComplete, GameComplete panels (inactive by default)
```

## Color Palette (from GameColors static class)
CreamWhite `#FFF8E7`, DeepNavy `#2C3E50`, PlayfulPurple `#9B5DE5`, CoralOrange `#FF6B6B`,
SkyBlue `#6BCB77`, SoftTeal `#4ECDC4`, SunshineYellow `#FFD93D`, PlayerHighlight `#F39C12`

## Fonts
- Headings / title: **Bubblegum Sans** (Google Fonts, free) → `Assets/Fonts/`
- Grid words / UI text: **Andika** (Google Fonts, free) → `Assets/Fonts/`
- Create TMP Font Assets at 2048×2048 atlas resolution

## Unity MCP Setup (copy this section to any new Unity project)

At the start of every session in a Unity project, verify all three of the following are in place.
If any are missing, offer to fix them — all steps can be done without user action except restarting Claude Code at the end.

### Check 1 — Package installed
`Packages/manifest.json` must contain:
```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"
```
If missing, add it to the `dependencies` object. Unity will auto-install on next load.

### Check 2 — Auto-start script present
`Assets/Editor/McpAutoStart.cs` must exist with this exact content:
```csharp
using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class McpAutoStart
{
    static McpAutoStart()
    {
        if (Application.isBatchMode) return;
        EditorApplication.delayCall += StartMcpBridge;
    }

    private static async void StartMcpBridge()
    {
        try
        {
            if (MCPServiceLocator.Bridge.IsRunning) return;

            if (!MCPServiceLocator.Server.IsLocalHttpServerReachable())
            {
                bool started = MCPServiceLocator.Server.StartLocalHttpServer(quiet: true);
                if (!started) return;

                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(500);
                    if (MCPServiceLocator.Server.IsLocalHttpServerReachable()) break;
                }
            }

            await MCPServiceLocator.Bridge.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[McpAutoStart] Failed to start MCP bridge: {ex.Message}");
        }
    }
}
```
If missing, create it. Unity will recompile and the MCP server will start automatically.

### Check 3 — MCP server registered with Claude Code
Run `claude mcp list` and verify `unityMCP` appears pointing to `http://localhost:8080/mcp`.
If missing, run:
```
claude mcp add --transport http unityMCP http://localhost:8080/mcp
```

### After any fixes
If anything was missing and was just fixed, tell the user: **"Please restart Claude Code to connect to the Unity MCP server."**

### Startup order (every session)
Unity must be open and the MCP server running before Claude Code starts.
The `McpAutoStart` script handles starting the server automatically when Unity loads.

### Prerequisites (one-time, machine-wide)
- `uv` must be installed: `powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"`
- Verify with `uv --version`

## Coding Conventions
- MonoBehaviours: one class per file, filename matches class name exactly
- Singleton pattern: `public static T Instance { get; private set; }` + Awake check
- Never use `Find()` at runtime — use registered references or static Instance
- Prefer events (`System.Action`) over direct method calls between managers
- All serialized inspector fields use `[SerializeField] private`
- No comments unless the WHY is non-obvious
