# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-09-11

---

## Current Milestone Focus

- Milestone: **Gate 5 — Soft Launch Readiness**
- Goal: Stone blockers + deadlock recovery are code-complete and hardened (init order, analytics, CI stone rules, idempotent config generator). Remaining Gate 5 work needs Unity Editor (SDK import, ScriptableObject assets, EditMode test run) and device playthrough.
- Source plan: `docs/production/mvp-build-plan.md`

## Current Branch + Baseline

- Branch: `cursor/stone-blockers-deadlock-bbef`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, `lint`, and `unity-tests` should pass
- PR: https://github.com/RipKDR/Brew/pull/6 (draft)

## Completed Since Last Handoff

### Hardening Pass (This Session)

- **Stone init order:** `BoardPresenter` places stones **before** `PopulateBoard` so fill/cluster guarantees work around fixed stones; EditMode test covers preserve-stones + valid cluster.
- **Deadlock contract:** `OnDeadlockRecovered(bool)`; second `TryRecoverDeadlock` if first fails; `GameFlowController` wires event → `AnalyticsManager.LogBoardReshuffle`.
- **Stone clear feedback:** `OnStonesCleared`; brew paths notify after adjacent clear; top-row stones skipped in `PlaceStoneBlockers`.
- **Analytics:** `board_reshuffle` in `docs/analytics/event-tracking-plan.md` + `LogBoardReshuffle` + EditMode test.
- **CI / generator:** `validate_stone_placements` in `scripts/build/build_config.py` (no top-row, no orthogonal adjacency); default `--levels-dir` → `Assets/_Project/Resources/Levels`; safer stone generation + validation in `scripts/level-generator/generate_level.py`.
- **Config assets:** Idempotent `ConfigAssetGenerator` (Audio + Board included). Batchmode:
  `Unity -batchmode -quit -projectPath . -executeMethod Brew.Editor.ConfigAssetGenerator.GenerateAllConfigAssets`

### Stone Blockers + Deadlock Recovery (Prior On Branch)

**Docs / ADR (document-first):**
- Updated `docs/game-design/core-mechanic.md` §1.3 — added `STONE` cell type; ice/lock deferred
- Updated `docs/technical/data-model.md` — blocker schema matches shipped `{type,row,col}`; stone-only soft launch
- Updated `docs/game-design/level-design-framework.md` §12 — soft-launch stone-only note; ice marked post soft-launch
- Added ADR `docs/adr/0004-stone-only-blockers-soft-launch.md` + index row

**Core engine:**
- `CellContentType.Stone` + `CellContent.Stone` / `IsStone`
- `StoneClearer.ClearAdjacentStones` — orthogonal brew clear
- `CascadeResolver` — segment gravity around fixed stones; refill fills empties in every segment
- `TokenSpawner.IsDeadlocked` / `TryRecoverDeadlock` — reshuffle tokens (orbs/stones fixed), then regenerate tokens if needed
- `BoardPresenter` — stones before populate; clear on brew; deadlock after CheckWin; grey stone display
- `TokenView` — stone scale (1.0× cell)

**Content / tests:**
- `level_037.json` — non-adjacent stones; no top-row
- `Assets/Tests/EditMode/Core/StoneGameplayTests.cs`

## In Progress

- None in this agent session. Awaiting Unity Editor verification of EditMode suite.

## Next 3 Tasks

1. **Run EditMode tests in Unity Editor** (including `StoneGameplayTests` + new analytics test). Fix any compile/test failures.
2. **Create ScriptableObject assets** via menu `Brew/Generate Config Assets` or batchmode executeMethod above; assign to `GameFlowController`. Import Firebase/IAP/AdMob SDKs + scripting defines.
3. **Device playthrough** of levels with stones (16–40 subset): verify gravity, brew-clear, deadlock reshuffle UX; then Gate 5.5–5.15 checklist.

## Blockers / Risks

- Blocker: Full Unity EditMode suite + SO assignment still require Unity Editor
- Blocker: Firebase / Unity IAP / AdMob SDKs not imported (bridges compile as no-ops without defines)
- Risk: Ice from level-design framework remains unimplemented (intentional per ADR 0004)
- Risk: Deadlock reshuffle has no presentation juice yet (instant rebuild) — acceptable for soft launch correctness

## Decisions Recorded This Session

- Soft-launch blockers are **stone-only**, cleared on **adjacent brew** (framework §12)
- Stones must be placed **before** board populate so cluster guarantees remain valid
- Deadlock recovery does **not** add a new `BoardPhase`; fires `board_reshuffle` analytics
- CI rejects top-row and orthogonally adjacent stones

## Files Touched This Session (Hardening)

- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs`
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs`
- `Assets/_Project/Scripts/Core/Backend/AnalyticsManager.cs`
- `Assets/_Project/Scripts/Editor/ConfigAssetGenerator.cs`
- `Assets/Tests/EditMode/Core/StoneGameplayTests.cs`
- `Assets/Tests/EditMode/Core/Backend/AnalyticsManagerTests.cs`
- `docs/analytics/event-tracking-plan.md`
- `scripts/build/build_config.py`
- `scripts/level-generator/generate_level.py`
- Continuity: `session-handoff.md`, `weekly-focus.md`, `context-snapshot.md`, `agent-prompt.md`

## Handoff Checklist

- [x] Decision log updated (ADR 0004)
- [x] This handoff file updated
- [ ] Changelog updated (optional for this session)
- [x] Weekly focus updated
- [x] Context snapshot regenerated
- [x] ADR index validated
- [ ] CI/local Unity EditMode run (requires Unity Editor)
- [ ] ScriptableObject assets created and assigned (requires Unity Editor)
- [ ] Firebase SDK imported and scripting defines set (requires Unity Editor)
