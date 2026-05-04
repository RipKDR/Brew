# Brew — Full-Scale Workspace Review & Improvement

**Date:** 2026-05-04  
**Author:** OpenCode AI  
**Status:** Approved  
**Branch:** cursor/audio-config-interface-extraction → master  

---

## Problem Statement

The workspace is at Gate 5 (Soft Launch Readiness) with several gaps that either block the gate or degrade developer/AI productivity:

1. **Compile error** — `BrewSceneSetup.cs` on current branch is missing 4 `using` directives removed in an uncommitted diff
2. **Missing level** — Only 39 of 40 levels exist (`level_001.json` absent); Gate 5 criterion 5.3 requires all 40 completable
3. **Stale docs** — `context-snapshot.md`, `session-handoff.md`, and `weekly-focus.md` are 25 days old; CI `validate-context` has a 14-day freshness window
4. **Missing infra** — No project-level `CLAUDE.md`; no `Fastfile` or `ExportOptions.plist` (both referenced in `build.yml` but absent)
5. **CI gaps** — No dependency caching, no secret presence checks, no coverage reporting
6. **Test coverage** — `GameFlowController` (the main system orchestrator) has zero tests; no integration test for the board state machine full cycle

---

## Architecture

No new architecture is introduced. All changes are additive or corrective within the existing layers.

---

## Implementation Phases

### Phase A — Ship Blockers

#### A1 — Fix `BrewSceneSetup.cs`
- Restore the 4 `using` directives removed in the current uncommitted diff:
  - `using UnityEditor;`
  - `using UnityEditor.SceneManagement;`
  - `using UnityEngine;`
  - `using UnityEngine.SceneManagement;`
- These are required for `EditorSceneManager`, `EditorUtility`, `GameObject`, `Scene` references in the file
- File: `Assets/_Project/Scripts/Editor/BrewSceneSetup.cs`

#### A2 — Generate `level_001.json`
- `Assets/_Project/Resources/Levels/level_001.json` is absent (levels 002–040 exist)
- Level 1 is the tutorial entry point — it must be the simplest possible board
- Spec: 5×6 grid, 3 ingredient types (Ember/Frost/Vine), 1 recipe target (1 brew), 20 moves, 0 blockers, star thresholds [1 brew, 1 brew + 10 score, 1 brew + 5 moves remaining]
- Must pass the CI level validator (`scripts/build/build_config.py`)
- Add corresponding `.meta` stub so Unity doesn't generate a random GUID on import

#### A3 — Freshen Session Docs
- `docs/project-management/context-snapshot.md` — regenerate using `scripts/context/build_context_snapshot.py`
- `docs/project-management/session-handoff.md` — update "Last updated" date and "Current Branch" to reflect current state
- `docs/project-management/weekly-focus.md` — update "Week of" date and task checklist to reflect completed items

---

### Phase B — Infrastructure Completeness

#### B1 — Project-Level `CLAUDE.md`
- Lives at `T:/Brew/CLAUDE.md` (repo root, not user-global)
- Purpose: gives Claude Code project-specific context on first load without reading all of `AGENTS.md`
- Contents:
  - One-paragraph game description
  - Pointer to `AGENTS.md` as the authoritative conventions file
  - Pointer to `docs/project-management/session-handoff.md` for current state
  - Key hard rules (no third currency, no match-3, no hardcoded economy values)
  - Current milestone + branch

#### B2 — `Fastfile` + `ExportOptions.plist`
- `Fastfile` lives at repo root (Fastlane convention); defines a `pilot_upload` lane used by `build.yml`
- `ExportOptions.plist` lives at repo root; defines iOS export method (`app-store`), signing style (`manual`), placeholder bundle ID
- Both files contain project-neutral defaults; actual secrets come from CI environment variables already defined in `build.yml`

#### B3 — CI Improvements
Three targeted additions to `ci.yml`:
1. **pip cache** — already present on `validate-levels`; add to `validate-economy` job (currently missing)
2. **Secret presence check** — new job `check-secrets` that warns (non-blocking) if `UNITY_LICENSE`, `ANDROID_KEYSTORE_BASE64`, or `ASC_KEY` are absent; prevents silent build failure with unhelpful errors
3. **Job summary** — add `>> $GITHUB_STEP_SUMMARY` output to `validate-levels` and `validate-economy` so results appear in the PR summary panel

---

### Phase C — Test Coverage

#### C1 — `GameFlowControllerTests`
- File: `Assets/Tests/EditMode/Presentation/GameFlowControllerTests.cs`
- `GameFlowController` is a `MonoBehaviour` so tests use `EditMode` with `[UnityTest]` or pure-logic extraction
- Focus: test the pure-logic paths accessible without a real scene — `InitializeSystems()` default fallback values, `AdvanceToNextLevel()` cap behavior, `HandleLevelWin()` Essence calculation
- Minimum 8 test methods covering the documented P1 fixes from Session 6

#### C2 — Board State Machine Integration Test
- File: `Assets/Tests/EditMode/Core/BoardStateMachineIntegrationTests.cs`
- Tests the full `Idle → PlayerInput → Fusing → Cascading → Settling → CheckBrew → CheckWin → Idle` cycle
- Uses a seeded 5×5 board, injects a known cluster tap, and asserts each phase transition fires in order
- Verifies win detection triggers after final recipe brew, move exhaustion triggers lose

---

## Testing Strategy

- A1: verified by `dotnet build` or Unity compile (no runtime test needed — it's a using directive)
- A2: verified by `scripts/build/build_config.py --level-range 1-40`
- A3: verified by `scripts/context/validate_context.py`
- B1: no automated test — content review
- B2: verified by syntax check (`xmllint ExportOptions.plist`, `bundle exec fastlane lanes`)
- B3: verified by running CI on a test branch
- C1/C2: verified by Unity EditMode test runner (same job already in `ci.yml`)

---

## Files Changed

| File | Change |
|------|--------|
| `Assets/_Project/Scripts/Editor/BrewSceneSetup.cs` | Restore 4 missing `using` directives |
| `Assets/_Project/Resources/Levels/level_001.json` | New — tutorial entry level |
| `Assets/_Project/Resources/Levels/level_001.json.meta` | New — Unity meta stub |
| `docs/project-management/context-snapshot.md` | Regenerate |
| `docs/project-management/session-handoff.md` | Update date + branch |
| `docs/project-management/weekly-focus.md` | Update week + checklist |
| `CLAUDE.md` | New — project-level Claude context |
| `Fastfile` | New — Fastlane lane definition |
| `ExportOptions.plist` | New — iOS export config |
| `.github/workflows/ci.yml` | Add pip cache, secret check, job summaries |
| `Assets/Tests/EditMode/Presentation/GameFlowControllerTests.cs` | New — 8+ test methods |
| `Assets/Tests/EditMode/Core/BoardStateMachineIntegrationTests.cs` | New — full cycle integration test |

---

## Out of Scope

- Flutter subdirectory (`T:/Brew/flutter/`) — separate project, separate concern
- Addressables configuration — requires Unity Editor
- ScriptableObject asset creation — requires Unity Editor
- Firebase SDK import — requires Unity Editor
- App store asset creation (icon, screenshots) — art task
