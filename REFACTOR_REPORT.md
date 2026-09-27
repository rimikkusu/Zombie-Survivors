# Refactor report

Completed 2026-09-27. Comparison is against the source snapshot taken before this refactor, including the earlier gameplay changes already in the worktree. Git HEAD was not used as the baseline. No gameplay values were intentionally changed.

## 1–4. Files, lines, removal, reduction

| Metric | Before | After | Reduction |
|---|---:|---:|---:|
| C# source files | 21 | 21 | 0 |
| Physical C# lines | 4,833 | 4,495 | 338 (6.99%) |
| Nonblank C# lines | 4,221 | 3,948 | 273 (6.47%) |
| Physical lines excluding Designer files | 3,816 | 3,478 | 338 (8.86%) |

Counts include Form1.Designer.cs and Resources.Designer.cs, exclude bin/obj, and retain normal readable formatting. Nonblank counts include comments and braces; they are provided so blank-line removal cannot inflate the result. The 1,017 lines in the two generated Designer files are unchanged.

### Requested file counts

| File | Before | After | Removed |
|---|---:|---:|---:|
| Form1.cs | 1366 | 1261 | 105 |
| Rendering/GameRenderer.cs | 384 | 330 | 54 |
| Systems/CombatSystem.cs | 441 | 413 | 28 |
| Systems/EnemyManager.cs | 544 | 504 | 40 |

### Largest ten files before and after

| Rank | Before file | Lines | After file | Lines |
|---:|---|---:|---|---:|
| 1 | Form1.cs | 1366 | Form1.cs | 1261 |
| 2 | Form1.Designer.cs | 884 | Form1.Designer.cs | 884 |
| 3 | Systems/EnemyManager.cs | 544 | Systems/EnemyManager.cs | 504 |
| 4 | Systems/CombatSystem.cs | 441 | Systems/CombatSystem.cs | 413 |
| 5 | Rendering/GameRenderer.cs | 384 | Rendering/GameRenderer.cs | 330 |
| 6 | Entities/Player.cs | 211 | Systems/AbilitySystem.cs | 176 |
| 7 | Systems/AbilitySystem.cs | 160 | Entities/Player.cs | 170 |
| 8 | Properties/Resources.Designer.cs | 133 | Properties/Resources.Designer.cs | 133 |
| 9 | Systems/UpgradeSystem.cs | 128 | Systems/UpgradeSystem.cs | 111 |
| 10 | Systems/InputManager.cs | 119 | Systems/WaveManager.cs | 83 |

## 5–7. Deleted, merged, and added files

- No C# source files were deleted, merged, or added by this refactor.
- MajorAbilityType moved from UpgradeSystem.cs into AbilitySystem.cs, keeping the same namespace.
- AbilityDefinition was added inside AbilitySystem.cs; no framework or dependency was introduced.
- This report is the only new repository file from the refactor. DeathParticle.cs already existed in the initial snapshot as an untracked file and was preserved.
- Verification harnesses and the pre-refactor snapshot remain under the local temporary directory, outside the game project.

## 8. Largest duplication removed

- Fast and Heavy zombie windup, burst, cooldown, and recovery handling now share UpdateZombie. Their distinct values, Fast drift, and Heavy stationary recovery remain explicit.
- Upgrade titles, effects, magnitudes, and stack text use one definition table.
- Ability titles, descriptions, durations, and cooldowns use one definition table.
- Player reset uses the default PlayerStats initialization instead of repeating every default assignment.
- Repeated single-shot/multishot calls, level-up button setup, XP presentation, control centering, debug wave cleanup, and menu animation reset were consolidated.

## 9. Architecture simplifications

Form1 still owns state and coordinates the systems and Designer UI. Combat, enemies, progression, abilities, camera, input, and rendering retain their separate responsibilities. EnemyStats now also owns knockback and stagger settings. Enemy world clamping is shared by movement and combat. No service layer, generic behavior framework, or global utility collection was added.

## 10. Performance and allocation improvements

- Enemy separation reuses its movement-budget array, growing only when needed.
- Wave spawning reuses a callback created once per form.
- HUD formatting is skipped when the relevant values or display versions have not changed.
- Upgrade HUD comparisons avoid per-tick LINQ, enumeration arrays, object arrays, and repeated collection rebuilding.
- Spawn-spacing and teleport-clearance checks use direct loops instead of capturing LINQ predicates.
- Rendering computes the visible world bounds once per draw and reuses drawing resources.
- Button hover handlers share static delegates instead of capturing each button in closures.

These are verified code-path improvements; no FPS or whole-game allocation benchmark is claimed.

## 11. Dead code removed

Removed the unused Bullet.Speed and BandageDropChance constants, UpgradeStacks accessor, stored mouse coordinates and mouse-button flag, SetMouse and its MouseMove subscription, Camera.Reset wrapper, duplicated PlayerStats.Reset assignments, duplicate FormClosed cleanup subscription, and obsolete ability timing/runtime duplication. Resource references were checked: all seven game images remain in use.

## 12. Rendering

Cached bandage brushes and outline, world boundary pen, warning pen, forcefield pen, muzzle brush, and the mutable brush used for particles and combat text. Existing fonts and image attributes remain cached. One tint factory replaces three repeated matrix definitions. Combat-text styles are expressed in a compact switch. All owned cached resources are disposed; shared resource bitmaps and stock brushes are not disposed. Optional hitbox-debug drawing remains available, with its short-lived pens scoped to that debug path.

Six deterministic rendered frames matched the baseline exactly, covering normal/critical flashes, enemy windup, recoil and muzzle flash, all combat-text types, bandages, particles, forcefield, and the world boundary.

## 13. Form1

Consolidated HUD visibility, five centering calculations, level-up choice button handling, debug wave actions, XP presentation, and shooting calls. Cached wave/debug/ability/upgrade HUD values reduce work in the update loop. Preserved the original control visibility order and foreground order, including WinForms behavior when controls first become visible. Unchanged upgrade-list text does not rebuild the list.

## 14. Enemy system

Shared zombie transitions and spawn-attempt code. Preserved the original first 80 constrained spawn attempts plus 200 fallback attempts, random-call order, type selection, movement, collision order, forcefield logic, and separation limits. Vampire behavior stays separate because its strafe and retreat rules differ. Knockback, stagger duration, and stagger speed factor now live alongside the other enemy stats.

## 15. Upgrade system

One small data table supplies upgrade names, descriptions, amounts, and stack display. The application switch remains explicit. Upgrade and ability choices share the same unique random-pick algorithm. Version changes allow HUD refreshes only when necessary.

## 16. Ability system

Ability definitions own title, descriptive effect, cooldown, and duration. Runtime stores only the remaining duration and cooldown. Display version changes are tracked independently of frame frequency. Acquisition-order slots, 1–9/numpad input, active durations, post-effect cooldowns, and debug cooldown behavior are preserved.

## 17. Reset and state handling

Play, Respawn, and returning to the main menu continue using RestartGame. Player defaults have one source. Wave reset and advancement share StartWave. The existing mutually exclusive GameState enum remains; debug toggles remain independent and survive reset. Health UI cache assignment has one location. Reset still clears bullets, enemies, bandages, combat text, particles, progression, and ability ownership.

## 18. Code deliberately retained

- Designer layout and generated resource accessors were left byte-for-byte unchanged.
- Distinct Vampire behavior and special ability actions remain explicit.
- Swept bullet intersection and muzzle obstruction retain their original iteration/tie behavior and floating-point math.
- Collision/contact rules, forcefield edge handling, and screen/world transforms remain readable.
- Distance calculations were retained where changing boundary rounding or normalization would risk altering behavior.
- Small entity types remain separate; combining them would obscure their different lifetimes and rules.
- Debug functionality and hitbox visualization were retained.

## 19–20. Builds and warnings

Built after each major stage and again after the second review. Final commands:

```powershell
dotnet build '.\Vampire Survivors.slnx' -c Release
dotnet build '.\Vampire Survivors.slnx' -c Debug
```

Both succeeded with **0 errors and 0 warnings**. Target framework remains .NET 10 Windows. No packages were added.

## 21. Designer compatibility

Form1.Designer.cs and Resources.Designer.cs are byte-identical to the initial snapshot. InitializeComponent, control declarations, Controls.Add relationships, event wiring, and the generated disposal pattern are preserved. Actual Form1 instances were created and exercised in STA WinForms harnesses. Visual Studio's visual Designer was not opened, so this is source-preservation and runtime verification rather than a manual Designer session.

## 22. Runtime and smoke results

- **46 combat checks passed**, including enemy attack cycles, windups, locked charge/dash directions, stagger, knockback, point-blank obstruction, swept projectiles, hit feedback, particles, and scripted clearable encounters at waves 1, 3, 5, and 10.
- **109 progression/UI checks passed**, including enemy data and XP scaling, upgrades and queued level-ups, milestone abilities, acquisition-order hotkeys, cooldowns, pause, teleport, forcefield, multishot, HUD, bandages, F3/debug controls, infinite-health/XP/cooldown options, wave tools, and reset.
- **48,961 deterministic trace records were byte-identical** between the pre-refactor snapshot and final code. These cover AI states, repeated upgrade applications, 8,000 ability timer updates, movement/input/aiming, six rendered-image hashes, control bounds/visibility/text/order, menu/settings/play/pause/resume/game-over/respawn, all display modes, and Quit disposal.
- The unchanged-HUD test temporarily enables list selection in the harness to detect unwanted Items.Clear calls; the game's SelectionMode.None configuration remains untouched.
- Bandage probability remains exactly `Random.Shared.NextDouble() < 0.10`; the 10,000-kill distribution check passed. Healing remains +20 HP, full-health pickups stay on the ground, and reset clears pickups.

The earlier progression harness had expectations predating the combat-polish pass. Its copy was updated to the saved baseline values (HP 30/80/110, 4% health scaling, 10% drops) and renamed camera API before running; the game values were not altered to satisfy old tests.

The second whole-code review additionally consolidated XP presentation and clamping, removed a repeated wave HUD update, cached the spawn callback, and checked control order and unchanged-list refresh behavior. This is automated runtime/smoke verification, not a claim of exhaustive manual playtesting or proof for every random playthrough.
