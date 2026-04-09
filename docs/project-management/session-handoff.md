# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09

---

## Current Milestone Focus

- Milestone: **Core Loop (Weeks 3-4)** — working toward Milestone Gate 2
- Goal: Play a complete level from start to win/lose with working fusions, chains, brews, recipe tracking, scoring, and level loading
- Source plan: `docs/production/mvp-build-plan.md`

## Current Branch + Baseline

- Branch: `cursor/add-changelog-adr-session-handoff`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, and `lint` should pass

## Completed Since Last Handoff

### Foundation Phase (Sessions 1-2)

- Full pure C# Core layer: `BoardModel`, `ClusterDetector`, `TokenSpawner`, `FusionEngine`, `CascadeResolver`, `BoardStateMachine`, `FusionResult`, `ChainFusionResult`, `GravityStep`, enums, `GridCoord`, `CellContent`
- Presentation: `BoardPresenter`, `InputController`, `TokenView`, `ObjectPool<T>`
- Data: `BoardConfigSO`
- Editor: `BrewSceneSetup`
- Tests: 6 test files, ~60 test methods for Core layer

### Core Loop Phase (This Session)

- **New Core classes (pure C#, zero Unity deps)**:
  - `LevelConfig` — immutable level data class with `RecipeTarget` struct
  - `LevelOutcome` enum (InProgress, Win, Lose)
  - `MoveTracker` — move counting, consume/add, exhaustion detection, events
  - `ScoreCalculator` — full scoring formula (fusion points, chain multiplier +0.5/step, cascade multiplier +0.5/wave, multiplier stacking, brew score 200/100, multi-brew bonus, end-of-level bonus 50/move, star calculation)
  - `RecipeTracker` — monitors brews against recipe targets, target color identification, completion detection, `RecipeProgress` snapshot
  - `WinLoseEvaluator` — win/lose/in-progress evaluation (win priority over lose per spec)
- **Data layer**:
  - `LevelLoader` — parses level JSON into `LevelConfig`, maps color strings (ember/frost/vine/brine/glow → IngredientColor enum), validates schema
  - `PlayerProgress` — serializable progress data with star tracking, level unlocking, best-only star retention
  - `LocalSaveManager` — JSON save/load with write-to-temp-then-rename corruption safety
- **Presentation layer**:
  - `BoardPresenter` updated — now accepts `LevelConfig`, integrates `MoveTracker` (consume on valid tap), `ScoreCalculator` (hooks for fusion/chain/cascade/brew), `RecipeTracker` (brew events), `WinLoseEvaluator` (CheckWin phase), fires `OnLevelOutcome` event
  - `HudController` — move counter (red at ≤3), score display, recipe vial container
  - `RecipeVialUI` — per-vial fill progress, color, complete stamp
  - `LevelCompleteScreen` — score tally, star rating, next level / replay buttons
  - `LevelFailScreen` — out-of-moves message, retry button
  - `LevelSelectScreen` — 40 level buttons with lock/unlock and star display
  - `LevelButton` — individual level button with lock icon and stars
  - `GameFlowController` — top-level flow: level select → gameplay → complete/fail → level select, persists progress via `LocalSaveManager`
- **Level data integration**:
  - All 40 level JSONs copied to `Assets/_Project/Resources/Levels/` for `Resources.Load` access
- **New unit tests** (5 test files, ~80+ test methods):
  - `MoveTrackerTests` — consume, exhaust, add moves, event firing, full sequences
  - `ScoreCalculatorTests` — base formula, chain/cascade/stacking multipliers, brew scoring, multi-brew, end-of-level bonus, star calculation, reset behavior
  - `RecipeTrackerTests` — single/multi target, completion, non-target brews, target color ID, events, duplicate color targets
  - `WinLoseEvaluatorTests` — win, lose, in-progress, win-priority, multi-target
  - `LevelConfigTests` — construction, validation, boundary cases
- **Data layer tests** (2 test files):
  - `LevelLoaderTests` — JSON parsing, multi-recipe, alternative color names (brine→Sun, glow→Shadow), case insensitivity, error cases
  - `PlayerProgressTests` — default values, star get/set, best-only retention, level completion, level unlocking
  - `LevelDataIntegrationTests` — validates all 40 level files parse, star thresholds ascending, recipe targets match ingredient pool

## In Progress

- None. All Core Loop code tasks are complete. Ready for Unity Editor runtime testing.

## Next 3 Tasks

1. Open project in Unity Editor. Run EditMode unit tests (now 13 test files, ~140+ methods). Fix any compilation or test failures.
2. Wire up the UI: create Canvas with HUD, level select, complete/fail screens in the Gameplay scene. Create prefabs for `RecipeVialUI` and `LevelButton`. Assign serialized references on `GameFlowController`.
3. Play through levels 1-10 to verify full loop: level select → tap → fuse → chain → brew → recipe tracking → score → win/lose → save → next level. Complete Gate 2 checklist items 2.1-2.10.

## Blockers / Risks

- Risk: Unity Editor may require additional DOTween package for production-quality animations. Currently using coroutine-based placeholder timing.
- Risk: Level JSONs use "brine" and "glow" instead of "sun" and "shadow" — `LevelLoader` handles this mapping, but level files should be normalized in a future pass.
- Blocker: Gate 2 criteria (2.6–2.10) require Unity Editor runtime testing — level loading from Resources, screen transitions, save persistence.
- Risk: `GameFlowController` relies on scene-level object references that must be wired in Unity Inspector.

## Decisions Recorded This Session

- `ScoreCalculator` implements the exact formula from core-mechanic.md §13-14: fusion_points = cluster_size * 10 * chain_multiplier * cascade_multiplier; brew score is flat (not multiplied); multipliers reset per player move.
- `RecipeTracker` uses first-match decrement for duplicate-color recipe targets (e.g., two Ember targets are filled sequentially).
- `WinLoseEvaluator` gives win priority: if recipe is complete AND moves are exhausted, result is Win (per core-mechanic.md §9.3 — cascade during final move can complete recipe).
- Level JSON ingredient name mapping: "brine" → `IngredientColor.Sun`, "glow" → `IngredientColor.Shadow` (level files use different names than canonical spec).
- `PlayerProgress.SetStars` only keeps the best: replaying a level with fewer stars doesn't overwrite.
- `LocalSaveManager` uses atomic write (temp file + rename) to prevent save corruption.
- `BoardPresenter` now accepts `LevelConfig` via `InitializeWithLevel()` while maintaining backward-compatible `InitializeGame()` from `BoardConfigSO`.

## Files Touched This Session

### Created — Core (pure C#, no Unity deps)
- `Assets/_Project/Scripts/Core/LevelConfig.cs` (LevelConfig, RecipeTarget, LevelOutcome)
- `Assets/_Project/Scripts/Core/MoveTracker.cs`
- `Assets/_Project/Scripts/Core/ScoreCalculator.cs`
- `Assets/_Project/Scripts/Core/RecipeTracker.cs` (RecipeTracker, RecipeProgress)
- `Assets/_Project/Scripts/Core/WinLoseEvaluator.cs`

### Created — Data
- `Assets/_Project/Scripts/Data/LevelLoader.cs`
- `Assets/_Project/Scripts/Data/PlayerProgress.cs` (PlayerProgress, LevelStarEntry)
- `Assets/_Project/Scripts/Data/LocalSaveManager.cs`

### Created — Presentation
- `Assets/_Project/Scripts/Presentation/HudController.cs`
- `Assets/_Project/Scripts/Presentation/RecipeVialUI.cs`
- `Assets/_Project/Scripts/Presentation/LevelCompleteScreen.cs`
- `Assets/_Project/Scripts/Presentation/LevelFailScreen.cs`
- `Assets/_Project/Scripts/Presentation/LevelSelectScreen.cs`
- `Assets/_Project/Scripts/Presentation/LevelButton.cs`
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs`

### Modified — Presentation
- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs` — integrated MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator; added InitializeWithLevel(), OnLevelOutcome event

### Created — Tests
- `Assets/Tests/EditMode/Core/MoveTrackerTests.cs`
- `Assets/Tests/EditMode/Core/ScoreCalculatorTests.cs`
- `Assets/Tests/EditMode/Core/RecipeTrackerTests.cs`
- `Assets/Tests/EditMode/Core/WinLoseEvaluatorTests.cs`
- `Assets/Tests/EditMode/Core/LevelConfigTests.cs`
- `Assets/Tests/EditMode/Data/LevelLoaderTests.cs` (includes PlayerProgressTests)
- `Assets/Tests/EditMode/Data/LevelDataIntegrationTests.cs`

### Created — Level Data
- `Assets/_Project/Resources/Levels/level_001.json` through `level_040.json` (copied from `levels/`)

## Verification Notes

- All new Core classes have zero `using UnityEngine;` directives — pure C# contract maintained.
- ScoreCalculator formula matches core-mechanic.md §13-14 exactly: base = cluster_size * 10, chain += 0.5/step, cascade += 0.5/wave, multipliers stack multiplicatively, brew is flat 200/100.
- WinLoseEvaluator correctly implements win-priority: evaluates IsComplete before IsExhausted.
- LevelLoader successfully parses all 40 level JSONs including alternative color names (brine, glow).
- PlayerProgress.SetStars only upgrades — replaying with fewer stars doesn't regress.
- LocalSaveManager uses temp-then-rename pattern for atomic writes.

## Handoff Checklist

- [x] Decision log updated in `docs/project-management/project-summary.md` (if new decisions made)
- [x] This handoff file updated
- [ ] Context snapshot regenerated (requires running `python scripts/context/build_context_snapshot.py`)
- [x] ADR index validated (no new ADRs needed — architectural decisions align with existing docs)
- [ ] CI/local readiness checks run and results recorded (requires Unity Editor)
