# ArenaSurvivors simplification report

Completed 15 October 2026. Baseline: the clean working tree at `fdc653f` before this task. Every handwritten C# file was reviewed. Counts use physical lines, including comments and braces, and exclude `bin`, `obj`, and generated Designer files unless stated. The separate temporary QA harness is outside the game project.

## Size before and after

| Metric | Before | After | Reduction |
|---|---:|---:|---:|
| Handwritten C# files | 29 | 23 | 6 (20.69%) |
| Handwritten C# lines | 3,855 | 2,663 | 1,192 (30.92%) |
| Nonblank handwritten lines | 3,377 | 2,374 | 1,003 (29.70%) |
| Form1.cs lines | 227 | 220 | 7 |
| Largest handwritten file | 360 | 253 | 107 |
| All C# files, including both Designer files | 31 | 25 | 6 |
| All C# lines, including both Designer files | 4,897 | 3,702 | 1,195 |

The nonblank reduction confirms that this is an implementation reduction, not blank-line removal or minification. No new gameplay classes, dependencies, or frameworks were introduced. Form1 remains the coordinator; no replacement large class or extra partial file was created.

| Related source | Before | After |
|---|---:|---:|
| EnemyManager + EnemyBehavior | 527 | 198 |
| CombatSystem + ProjectileSystem | 437 | 241 |
| GameRenderer | 360 | 253 |
| GameSession | 280 | 237 |
| ScreenController | 235 | 110 |
| InputManager + GameInputController | 213 | 137 |
| AbilitySystem | 200 | 141 |
| LevelUpSelection + LevelUpController | 161 | 82 |
| DisplayManager + DisplayMode | 152 | 77 |
| WaveManager | 85 | 53 |
| HudController | 205 | 211 |

HUD grew six lines to fix control layout and sizing; the overall game still lost 1,192 handwritten lines.

Before, the largest files were GameRenderer (360), EnemyManager (354), GameSession (280), ProjectileSystem (267), ScreenController (235), and Form1 (227). After, they are GameRenderer (253), CombatSystem (241), GameSession (237), Form1 (220), HudController (211), and EnemyManager (198).

## Systems removed

- `Systems/Camera.cs`: removed Camera, the 5,000 x 3,500 GameWorld constants, following, view bounds, and screen-to-world conversion.
- `Systems/EnemyBehavior.cs`: removed the chase/windup/burst/strafe/retreat/recovery state machine, special attack timers, wandering, and locked charge directions.
- `Systems/ProjectileSystem.cs`: removed its separate object and optional argument plumbing. Actual shooting and swept collision now live together in CombatSystem.
- `Systems/InputManager.cs`: removed duplicate movement booleans and key tracking. GameInputController owns one pressed-key set and supplies movement to GameSession.
- `Systems/LevelUpSelection.cs`: replaced the separate selection model with a pending-level queue in GameSession and choices owned by the existing LevelUpController.
- `UI/DisplayMode.cs`: moved its small enum alongside DisplayManager, where it is used.
- Removed the AbilityDefinition and per-ability runtime classes, menu logo pulse and its Timer, reflection-based menu buffering, redundant hover handlers, off-screen culling, world border, AI windup tint/warnings, and the permanently disabled hitbox drawing path. The F3 Debug Menu remains.

## Single-screen arena

`Form1.ClientSize` supplies `GameSession.ArenaSize`. Drawing, movement, projectiles, pickups, spawning, teleportation, and mouse aiming all use those same client coordinates. The normal `PointToClient(Cursor.Position)` call handles the desktop cursor; gameplay performs no camera conversion.

Grass tiles cover the entire client area. A fresh Play or Respawn centers the player exactly. Movement and enemy placement clamp to the current arena. Resizing clamps the player, enemies, and retained bandages; removes projectiles and temporary effects outside the new area; and updates HUD placement. Empty minimize sizes are ignored. The Designer minimum window size keeps the existing controls usable; it does not define the arena's dimensions.

Windowed mode remembers its bounds, Maximized uses the normal Windows state, and Fullscreen uses the current monitor's bounds. Every mode still takes its playable size from ClientSize.

## Enemy AI and waves

All enemies now chase directly. A short hit stagger and small knockback remain. Body separation keeps contact to approximately one pixel so contact damage still works without enemies walking through the player. Alternate sides handle collisions at arena edges, and crowd pushes are capped per frame.

| Sprite/type | HP at level 1 | Speed per tick | Contact damage | Identity |
|---|---:|---:|---:|---|
| Zombie_1 / ZombieFast | 30 | 6.0 | 10 | Small, fast, low HP |
| Zombie_2 / ZombieHeavy | 80 | 3.6 | 20 | Large, slow, tough |
| Vampire_1 / Vampire | 110 | 6.6 | 30 | Fast, strong, later threat |

Existing 4% health scaling per player level and wave-based XP rewards remain.

Wave 1 requires three enemies; each subsequent wave adds two. Spawning uses a simple 650 ms interval. A whole enemy spawns on an arena edge, stays away from the player, and avoids overlapping existing bodies. A crowded edge retries without counting a failed spawn. Heavy zombies begin at wave 3; vampires begin at wave 10. The first spawn guarantees introduction of the stronger type. Remaining type selection uses simple probabilities. Clearing every required enemy starts a two-second break before the next wave.

## XP text cause and fix

The root cause was the Designer hierarchy: `lblLevel` and `lblXp` were children of `pnlSettings`, while the XP bar belonged to the Form. Hiding Settings hid both labels, regardless of their own Visible settings or BringToFront calls.

The existing Designer-created labels now belong directly to the Form beside `pnlXpBackground`. They keep Bottom/Left anchors, use AutoSize to prevent text clipping, and sit above the bar. Only `pnlXpFill` belongs inside the bar background, so it cannot cover either label. Fill starts at X = 0 and uses its parent's current client width instead of a width cached before scaling. Level and current/required XP refresh from Player after progression, selection, and reset. XP is still shown by Labels and Panels; no XP UI is manually painted.

HUD positioning uses actual control bounds to avoid compact-window overlaps. Active ability text sizes to its content. Debug information also uses AutoSize, and the Debug panel stays within the client area.

## Features retained and intentional changes

Retained: WASD movement, client-coordinate aiming, manual fire and cooldown, point-blank shooting, swept bullets, critical hits, hit flashes, recoil/muzzle feedback, knockback, red killing numbers, delayed floating XP notifications, a six-particle death burst, HP/contact invulnerability, XP/levels, all seven additive upgrades, all six milestone abilities, acquisition-order 1-9/numpad slots, active durations/cooldowns, 10% bandage drops and +20 HP pickups, Main Menu, Settings, Pause, Game Over, Respawn, and every F3 Debug control.

No requested core gameplay feature was removed. Intentional changes are the visible arena, direct-chase AI, simpler spawn composition/timing, removal of the logo pulse/AI warnings/world border, and native ComboBox drawing in Settings. Teleport still travels at most 450 pixels and now tries shorter jumps if its destination is occupied. Shots at a cursor inside the gun's reach keep firing forward instead of reversing.

All 50 existing Designer controls remain in InitializeComponent with their existing names and event handlers. Designer changes are limited to the XP hierarchy/sizing/text, Debug text sizing, minimum window size, and deletion of the retired animation Timer. All 16 original image/resource/generated-resource files checked against their starting hashes are unchanged. No artwork or resources were deleted.

## Build and verification

Built after the arena/AI conversion, after controller simplification, after combat/ability simplification, and after HUD fixes. Final builds:

```powershell
dotnet build 'E:\Vampire Survivors\Vampire Survivors.slnx' -c Debug -v minimal --no-restore
dotnet build 'E:\Vampire Survivors\Vampire Survivors.slnx' -c Release -v minimal --no-restore
```

Both succeeded with **0 warnings and 0 errors**. `git diff --check` passed.

The final separate STA WinForms harness passed **2,501 runtime assertions**, covering:

- Exact initial center and boundaries at four sizes, shrinking, retained pickups, transient small sizes, and minimize handling.
- Eight-direction fire, near-cursor shots, point-blank obstruction, cooldown, nearest swept hits at high bullet speed, enemies behind the player, baseline time-to-kill, kill/XP feedback, and death particles.
- Chase speeds/contact damage for all three types, repeated no-pass-through checks, corner collisions, edge spawning and clearance, all ten waves at three arena sizes, stronger-type introduction, failed spawn retries, and intermissions.
- Leftover XP and queued levels, all seven additive upgrades/caps, critical hits, all six abilities, acquisition order, expiry/cooldowns, teleport clearance, forcefield, shockwave XP, multishot, Last Stand, pickup behavior, and full reset.
- A scripted normal-input combat run completed five waves and reached wave 6, level 13, with 40/110 HP. It used ordinary movement/fire, offered upgrades and abilities, and no Debug/stat cheats. This is a sample playability check, not proof for every random run.
- A 10,000-kill bandage sample produced **1,017 drops (10.17%)**; the production probability remains exactly 0.10.
- Actual Form1 instances at host DPI 120: all three display modes, client size matching the arena, Designer hierarchy, XP values/fill/visibility/anchors/Z-order/text bounds, queued upgrade buttons, milestone choice, number/numpad input, active ability text, pause/settings/resume, Debug tools/toggles, contact death, Respawn, menu return, unchanged control count, and disposal. Additional explicit control-scaling checks passed.

Rendered Form1 frames were visually inspected for the minimum window, normal window, simultaneous abilities, scaling, and Debug. The screenshots use actual Designer controls. Visual Studio's interactive Designer and a human-controlled play session were not opened; Designer compatibility is supported by the preserved control definitions/event wiring and actual Form1 construction, rather than a claimed manual Designer check.

Local verification artifacts:

- `E:\Vampire Survivors\TestResults\runtime-results-full.txt`
- `E:\Vampire Survivors\TestResults\after-counts.json`
- `E:\Vampire Survivors\TestResults\arena-minimum-window.png`
- `E:\Vampire Survivors\TestResults\arena-abilities.png`
- `E:\Vampire Survivors\TestResults\arena-scaled.png`
- `E:\Vampire Survivors\TestResults\arena-debug.png`

TestResults is ignored by the existing .gitignore. The QA source/project and original metric/hash snapshots are saved under `$env:TEMP\ArenaSurvivors-simplification-20261015`. Re-run locally after building Debug:

```powershell
dotnet run --project (Join-Path $env:TEMP 'ArenaSurvivors-simplification-20261015\ArenaChecks.csproj') -c Release
```

The harness executes assertions against the current Debug game assembly; Release was independently built successfully. These are automated model/WinForms checks and rendered-layout inspection, not a claim of exhaustive manual playtesting or an FPS benchmark.
