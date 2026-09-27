# Form1 coordinator refactor

Comparison baseline: a copy of the complete C# source immediately before this extraction, after the preceding general refactor. Counts are physical lines in `.cs` files, excluding `bin` and `obj`. The two generated Designer files are included in project totals and excluded from the handwritten-file ranking.

## Requested results

| Item | Result |
|---|---|
| 1. Form1.cs before | 1,261 lines |
| 2. Form1.cs after | 225 lines |
| 3. Form1 reduction | 1,036 lines, **82.16%** |
| 4. Logic moved from Form1 | Run state, reset, movement, pickups, shooting, ability actions, wave updates, level-up selection, HUD, screen visibility/layout, input, debug actions, and display modes |
| 5. New classes | `GameSession`, `LevelUpSelection`, `AbilityActions`, `EnemyBehavior`, `ProjectileSystem`, `HudController`, `ScreenController`, `DisplayManager`, `LevelUpController`, `DebugController`, `GameInputController` |
| 6. Old classes/files deleted | None. Former Form1 implementations and gameplay fields were removed from Form1; the generated Designer files remain. |
| 7. Largest handwritten file | `Systems/EnemyManager.cs`, 347 lines. Next is `Rendering/GameRenderer.cs`, 338 lines. |
| 8. Total C# source lines | 4,495 before → 4,736 after (**241 more; 5.36% increase**). Handwritten source: 3,478 → 3,719. The requested reduction in total lines was **not met in this round**. The immediately preceding refactor went from 4,833 to 4,495, so the combined two-round total is still 97 lines below that earlier baseline. |
| 9. Duplication removed | All gameplay lists and systems now have one owner in `GameSession`; start, respawn, and return-to-menu paths share its reset. Screen visibility transitions live in `ScreenController`, and the HUD and level-up presentation each have one implementation. The old Form1 copies were removed, rather than retained in partial files. Unused imports were removed. |
| 10. Build | `dotnet build 'Vampire Survivors.slnx' -c Release -v minimal` and `-c Debug -v minimal`: **0 warnings, 0 errors** each. Builds were also run after the extraction stages. |
| 11. Designer | `Form1.Designer.cs` (884 lines) and `Properties/Resources.Designer.cs` (133 lines) are byte identical to the immediate baseline. Runtime checks instantiated the form and confirmed existing controls are Designer-owned. |
| 12. Runtime smoke test | Automated WinForms runtime checks passed **110 progression/UI** and **46 combat** assertions, including a targeted shockwave level-up/HUD ordering check. A separate before/after differential trace matched on **48,961 deterministic records**, covering movement, enemy behavior, abilities, UI state and layout, display modes, rendering, reset, and disposal. No manual play session was performed. |

The total-line increase comes from explicit controller boundaries and constructor wiring. I kept normal formatting and separate methods instead of compressing code to force a lower count. There is no second large Form1 partial file or replacement 800-line class.

## Handwritten C# files after refactor, largest first

| Lines | File |
|---:|---|
| 347 | `Systems/EnemyManager.cs` |
| 338 | `Rendering/GameRenderer.cs` |
| 262 | `Systems/ProjectileSystem.cs` |
| 235 | `UI/ScreenController.cs` |
| 225 | `Form1.cs` |
| 205 | `UI/HudController.cs` |
| 196 | `Systems/GameSession.cs` |
| 176 | `Systems/AbilitySystem.cs` |
| 170 | `Entities/Player.cs` |
| 165 | `Systems/EnemyBehavior.cs` |
| 163 | `Systems/CombatSystem.cs` |
| 143 | `UI/DisplayManager.cs` |
| 113 | `UI/GameInputController.cs` |
| 111 | `Systems/UpgradeSystem.cs` |
| 110 | `UI/DebugController.cs` |
| 88 | `Systems/AbilityActions.cs` |
| 83 | `Systems/WaveManager.cs` |
| 81 | `Entities/Enemy.cs` |
| 81 | `Systems/LevelUpSelection.cs` |
| 80 | `Systems/InputManager.cs` |
| 78 | `UI/LevelUpController.cs` |
| 60 | `Entities/PlayerStats.cs` |
| 40 | `Entities/EnemyStats.cs` |
| 35 | `Systems/Camera.cs` |
| 29 | `Entities/DamageNumber.cs` |
| 27 | `Entities/Bullet.cs` |
| 26 | `Entities/DeathParticle.cs` |
| 26 | `Entities/Bandage.cs` |
| 17 | `Program.cs` |
| 9 | `UI/DisplayMode.cs` |

Handwritten total: **30 files, 3,719 lines**. Generated Designer total: **2 files, 1,017 lines**. Combined C# total: **32 files, 4,736 lines**.

## Responsibility boundaries

- `Form1` creates the systems, coordinates the 16 ms tick, forwards paint/input/button events, switches basic run state, and disposes the timer and renderer. The tick is 19 physical lines including its declaration and braces; event handlers are short forwarders.
- `GameSession` owns the player, camera, enemies, bullets, bandages, combat text, death particles, input, gameplay systems, and the run reset/update/shoot/ability entry points.
- `ProjectileSystem` owns projectile creation and swept/muzzle collision. `CombatSystem` owns damage, contact damage, combat text, and particle updates. `EnemyBehavior` owns the AI state transitions while `EnemyManager` keeps spawning, spacing, and forcefield placement.
- `HudController`, `ScreenController`, `LevelUpController`, `DebugController`, `GameInputController`, and `DisplayManager` own their respective WinForms presentation or input behavior using the existing Designer controls.

The current bandage drop expression remains `Random.Shared.NextDouble() < 0.10`; the healing, pickup, and reset behavior was preserved by the runtime checks.
