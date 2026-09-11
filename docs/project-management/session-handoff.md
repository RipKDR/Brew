# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-09-11

---

## Current Milestone Focus

- Milestone: **Gate 5 — Soft Launch Readiness**
- Goal: Stone blockers + deadlock recovery are now code-complete. Remaining Gate 5 work needs Unity Editor (SDK import, ScriptableObject assets, EditMode test run) and device playthrough.
- Source plan: `docs/production/mvp-build-plan.md`

## Current Branch + Baseline

- Branch: `cursor/stone-blockers-deadlock-bbef`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, `lint`, and `unity-tests` should pass

## Completed Since Last Handoff

### Stone Blockers + Deadlock Recovery (This Session)

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
- `BoardPresenter` — places stones from `LevelConfig.BlockerPlacements` after populate; clears stones on both brew paths; recovers deadlock after CheckWin when still in progress; grey stone display color
- `TokenView` — stone scale (1.0× cell)

**Content fix:**
- `level_037.json` — separated adjacent stones at (2,3)/(2,4); moved second stone to (4,6)

**Tests:**
- `Assets/Tests/EditMode/Core/StoneGameplayTests.cs` — placement, gravity around stones, refill above/below stones, adjacent clear, cluster ignore, deadlock detect/recover

### Prior Session Summary

SDK bridge layer, monetization bridges, CI/CD game-ci wiring, and Gate 5 code review fixes remain as previously handed off. Unity Editor still required for SDK import and SO asset creation.

## In Progress

- None in this agent session. Awaiting Unity Editor verification of new EditMode tests.

## Next 3 Tasks

1. **Run EditMode tests in Unity Editor** (including `StoneGameplayTests`). Fix any compile/test failures from stone/deadlock changes.
2. **Create ScriptableObject assets** via `Brew/Generate Config Assets` and assign to `GameFlowController`. Import Firebase/IAP/AdMob SDKs + scripting defines.
3. **Device playthrough** of levels with stones (16–40 subset): verify gravity, brew-clear, deadlock reshuffle UX; then Gate 5.5–5.15 checklist.

## Blockers / Risks

- Blocker: Full Unity EditMode suite + SO assignment still require Unity Editor
- Blocker: Firebase / Unity IAP / AdMob SDKs not imported (bridges compile as no-ops without defines)
- Risk: Ice from level-design framework remains unimplemented (intentional per ADR 0004)
- Risk: Deadlock reshuffle has no presentation juice yet (instant rebuild) — acceptable for soft launch correctness

## Decisions Recorded This Session

- Soft-launch blockers are **stone-only**, cleared on **adjacent brew** (framework §12), not HP/fusion damage from older data-model draft
- Shipped JSON `{type,row,col}` is canonical; `hp` / `x` / `y` deferred
- Deadlock recovery does **not** add a new `BoardPhase`; it runs as CheckWin → Idle side path when outcome is still in progress
- Adjacent stones in level 37 violated placement rules — content fixed rather than relaxing the rule

## Files Touched This Session

### Docs
- `docs/game-design/core-mechanic.md`
- `docs/game-design/level-design-framework.md`
- `docs/technical/data-model.md`
- `docs/adr/0004-stone-only-blockers-soft-launch.md`
- `docs/adr/README.md`
- `docs/project-management/session-handoff.md`
- `docs/project-management/weekly-focus.md`
- `docs/project-management/context-snapshot.md`

### Core / Presentation
- `Assets/_Project/Scripts/Core/CellContentType.cs`
- `Assets/_Project/Scripts/Core/CellContent.cs`
- `Assets/_Project/Scripts/Core/StoneClearer.cs`
- `Assets/_Project/Scripts/Core/CascadeResolver.cs`
- `Assets/_Project/Scripts/Core/TokenSpawner.cs`
- `Assets/_Project/Scripts/Core/LevelConfig.cs`
- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs`
- `Assets/_Project/Scripts/Presentation/TokenView.cs`

### Content / Tests
- `Assets/_Project/Resources/Levels/level_037.json`
- `Assets/Tests/EditMode/Core/StoneGameplayTests.cs`

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
