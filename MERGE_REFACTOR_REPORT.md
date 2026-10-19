# ArenaSurvivors file merge and readability refactor

Completed 15 October 2026. This report compares against the 23-file, single-screen version present at the start of this request, including the preceding uncommitted simplification. It does not use Git HEAD or the older large-world version as the baseline. The existing changes and historical reports were preserved.

## 1. Files and code size

Handwritten counts include comments, braces, and normal readable formatting. They exclude `bin`, `obj`, the two generated Designer files, and the separate temporary QA projects.

| Metric | Before | After | Difference |
|---|---:|---:|---:|
| Handwritten C# files | 23 | 12 | 11 fewer (47.83%) |
| Handwritten physical lines | 2,663 | 2,622 | 41 fewer (1.54%) |
| Nonblank handwritten lines | 2,374 | 2,323 | 51 fewer (2.15%) |
| Form1.cs lines | 220 | 208 | 12 fewer |
| All C# files including generated files | 25 | 14 | 11 fewer |
| All C# lines including generated files | 3,702 | 3,661 | 41 fewer |

The extra line reduction is modest: the main change is grouping related existing classes into sensible files while preserving gameplay. Shared methods also remove actual duplicated work. Formatting was kept readable, with blank lines between the grouped classes. No gameplay logic was relocated into the QA projects.

The largest resulting file is Enemies.cs at 375 lines, containing enemy settings, Enemy, EnemyManager, and WaveManager. No single giant replacement class was introduced. Form1 stays a 208-line coordinator.

## 2. Files merged

| New file | Original files | Contents |
|---|---|---|
| Game/Entities.cs | Entities/Player.cs, PlayerStats.cs, Bullet.cs, Bandage.cs, DamageNumber.cs, DeathParticle.cs | Player and stats, bullets, pickups, floating text, death particles |
| Game/Enemies.cs | Entities/EnemyStats.cs, Enemy.cs, Systems/EnemyManager.cs, WaveManager.cs | Enemy types/settings, bodies, chase/collision/spawning, waves |
| Game/Progression.cs | Systems/UpgradeSystem.cs, AbilitySystem.cs | Upgrade choices/stacks, ability ownership/timers/definitions |
| UI/GameUI.cs | UI/HudController.cs, LevelUpController.cs | HUD and level-up choices |
| UI/Menus.cs | UI/ScreenController.cs, DisplayManager.cs | Menu state/placement and display modes |

These classes keep their original namespaces, accessibility, public names, and public member signatures. Related classes share files without introducing new inheritance or a framework.

## 3. Files moved or renamed

| Previous path | Final path |
|---|---|
| Systems/GameSession.cs | Game/GameSession.cs |
| Systems/CombatSystem.cs | Game/Combat.cs |
| Rendering/GameRenderer.cs | Game/GameRenderer.cs |
| UI/GameInputController.cs | UI/Input.cs |
| UI/DebugController.cs | UI/DebugMenu.cs |

All 21 retired original source paths from these tables were removed after their definitions were merged or moved. No duplicate original copies remain in the compiled project. Empty Entities, Systems, and Rendering directories were removed. No gameplay class or feature was deleted in this round.

## 4. Final folder structure

```text
Vampire Survivors/
├── Form1.cs                    208 lines
├── Form1.Designer.cs           unchanged generated UI
├── Form1.resx                  unchanged
├── Program.cs                  17 lines, unchanged
├── Vampire Survivors.csproj    unchanged
├── Game/
│   ├── GameSession.cs          237 lines
│   ├── Entities.cs             336 lines
│   ├── Combat.cs               243 lines
│   ├── Enemies.cs              375 lines
│   ├── Progression.cs          250 lines
│   └── GameRenderer.cs         246 lines
├── UI/
│   ├── GameUI.cs               288 lines
│   ├── Menus.cs                186 lines
│   ├── Input.cs                138 lines
│   └── DebugMenu.cs             98 lines
├── Properties/                 unchanged resources and generated accessor
├── Resources/                  unchanged artwork
└── Sprites/                    unchanged artwork
```

The SDK project includes the new C# paths automatically, so no manual compile list or project dependency was added. Form1.cs, its generated partial file, and its event handler names remain in their original locations.

## 5. Simpler code

- HP and XP fills now share one short UpdateBar method with the same clamping and width calculation. The text labels remain separate Designer controls.
- Player/enemy sprite drawing shares DrawSprite, including the existing cached flash tints. The drawing order, resources, rotations, effects, and resulting pixels are preserved.
- The upgrade sidebar fills the existing ListBox directly when its version changes. It no longer builds an intermediate string list and copies it into the control.
- Debug XP updates use the existing positive-XP/dead-player guards directly, without saving and comparing redundant old values.
- Form1 calls the existing HUD and level-up methods directly where its private forwarding wrappers added no work. Every Designer-connected event handler is preserved.
- Bullet path differences and the player's contact hitbox are calculated once per search instead of once per enemy.

No new helper class, interface, package, factory, dependency injection layer, event system, or generic framework was added. Existing short classes remain ordinary classes in the grouped files.

## 6. Comments

Short comments explain the relationship between run entities, ownership of wave counts versus spawn positions, enum-indexed ability timers, why held keys must not toggle actions repeatedly, shared flash tints, bar fill ownership, menu visibility, and the segment clipping used for fast bullets. Existing useful explanations of shooting, contact damage, XP leftovers, queued choices, reset, and wave timing remain.

## 7. Gameplay preservation

No gameplay values, enemy AI, wave formulas, random probabilities, XP rewards, upgrade amounts, ability durations/cooldowns, collision thresholds, animation/effect timing, or rendering settings were intentionally changed. No currently working visual effect was removed.

The single-screen arena, movement/aiming, manual and close-range shooting, three enemy types, chase/separation/contact damage, waves, health/resistance, XP/levels, additive upgrades, acquisition-order ability keys, bandages at 10%, floating text, death/hit/shot effects, all menus/settings/display modes, Pause, Game Over, Respawn, Debug, and all HUD elements remain.

## 8. Builds

Built after merging gameplay types, after moving/grouping UI and the remaining gameplay files, and after the small readability changes. Final solution builds:

```powershell
dotnet build 'E:\Vampire Survivors\Vampire Survivors.slnx' -c Debug -v minimal --no-restore
dotnet build 'E:\Vampire Survivors\Vampire Survivors.slnx' -c Release -v minimal --no-restore
```

Both succeeded with **0 warnings and 0 errors**. `git diff --check` passed.

## 9. Runtime and before/after verification

A full copy of the starting project was saved and independently built before editing. Separate STA WinForms QA runs used that baseline assembly and the final Debug game assembly; their loaded assembly copies were checked against the respective source assembly hashes.

- **2,501 runtime assertions passed on both versions.** These run actual GameSession and Form1 objects, not just source checks.
- Tested movement/bounds/resizing/minimize, eight-direction shots and cooldown, close-range obstruction and near-cursor firing, high-speed nearest bullet hits, all enemy types and contact/corner collisions, ten waves at three arena sizes, XP/queued levels, all seven upgrades, all six abilities/timers/hotkeys, forcefield/teleport/shockwave/multishot, bandages/healing, and resets.
- Actual Form1 tests exercised Main Menu, Settings, all display modes, XP hierarchy/text/fill/anchors/Z-order, upgrade and milestone buttons, active ability text, pause/resume/settings return, Debug tools/toggles, contact death, Respawn, menu return, and disposal.
- A seeded normal-input sample cleared five waves on both versions with identical results: wave 6, level 13, 18/120 HP, 2,726 simulated frames. It used normal movement/fire and offered upgrades/abilities without Debug/stat cheats.
- The seeded 10,000-kill sample produced the same **1,025 bandages** on both versions. The production chance remains 0.10.
- **All 15,068 before/after trace records matched byte-for-byte.** They include per-frame player/stats/entity/effect state, positions/health/XP, random spawn/reward/choice results, queued levels, upgrade stacks, ability timers, wave/intermission timing, control hierarchy/bounds/text/visibility/anchors/Z-order, and eight raw-pixel render hashes.
- **All five rendered UI screenshots matched byte-for-byte:** windowed, minimum window, simultaneous abilities, scaled controls, and Debug. The final minimum-window and ability frames were also visually inspected.
- The sorted public type/member signatures matched exactly.

Only the QA processes seed the runtime random generator to make comparisons repeatable. Production randomness was not changed. The comparisons are evidence for the exercised paths; they are not a claim of exhaustive proof for every possible input or random playthrough.

## 10. WinForms Designer

Form1.Designer.cs and Properties/Resources.Designer.cs are **byte-identical to the starting snapshot**. So are the resource files, artwork, Program.cs, and project files: all 20 protected files checked were unchanged. InitializeComponent, all 50 original controls, their names/parents, event connections, Form1's public parameterless constructor, and the generated disposal pattern remain intact. No Toolbox control was dynamically recreated.

Actual Form1 instances constructed and exercised all 50 controls successfully at host DPI 120, with additional scaling checks. Visual Studio's interactive `Form1.cs [Design]` surface was not opened, so an interactive Designer edit/save session is not claimed. Compatibility evidence is the unchanged generated definitions plus successful native WinForms construction and control/event tests.

## 11. Remaining verification limits and artifacts

No human-controlled play session, interactive Visual Studio Designer session, or FPS/allocation benchmark was performed. All reported runtime results are automated model/controller/WinForms tests and rendered-output comparisons. No requested feature was knowingly left broken or removed.

Evidence is under `E:\Vampire Survivors\TestResults\Merge` (already ignored by the repository's .gitignore):

- before-counts.json and after-counts.json
- baseline-runtime.txt and after-runtime.txt
- comparison.json
- before/behavior.jsonl and after/behavior.jsonl
- before/public-api.txt and after/public-api.txt
- before/ and after/ UI screenshots

The baseline project, source hashes, and QA source remain under `C:\Users\Eduard\AppData\Local\Temp\ArenaSurvivors-merge-20261015-100429`. The QA projects are outside the shipped game. Re-run the final checks after building Debug:

```powershell
$env:ARENA_QA_OUTPUT = 'E:\Vampire Survivors\TestResults\Merge\after'
dotnet run --project 'C:\Users\Eduard\AppData\Local\Temp\ArenaSurvivors-merge-20261015-100429\qa-after\ArenaChecks.csproj' -c Release '-p:GameAssembly=E:\Vampire Survivors\Vampire Survivors\bin\Debug\net10.0-windows\Vampire Survivors.dll'
```

The runtime harness checks the final Debug assembly. Release was independently built successfully.
