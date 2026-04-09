# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09

---

## Current Milestone Focus

- Milestone: **Content & Polish (Weeks 9-10)** — Gate 5 code sprint complete, working toward Milestone Gate 5
- Goal: Unity Editor testing, ScriptableObject asset creation, blocker gameplay implementation, Gate 5 verification
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/05-week-9-10-content-and-polish.md`

## Current Branch + Baseline

- Branch: `cursor/add-changelog-adr-session-handoff`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, and `lint` should pass

## Completed Since Last Handoff

### Gate 5 Code Sprint (This Session — Session 7)

**P1 Bug Fixes:**
- Removed hardcoded extra moves (=5) from `BoardPresenter.ActivateExtraMoves` — now requires explicit amount parameter
- `BoosterBarUI` now accepts `extraMovesAmount` via `Initialize()` instead of hardcoded literal 5
- Renamed `NotificationManager.ScheduleStreakAtRisk` parameter from `localHour` to `utcHour` with XML doc clarifying UTC semantics
- Added level duration tracking: `GameFlowController` now records `_levelStartTimeUtc` on level/event start and passes real elapsed seconds to `LogLevelComplete` and `LogLevelFail` (was always 0)

**P2 Data Gaps Fixed:**
- Added `BlockerPlacement` struct and `BlockerPlacements` property to `LevelConfig` — level JSON `blocker_placements` are now parsed and preserved (previously silently dropped)
- `LevelLoader.ConvertToLevelConfig` now maps `blocker_placements` from JSON to `LevelConfig.BlockerPlacements`
- Added `weekly_deal` SKU to `IAPManager.Catalog` ($2.99, consumable, 150 gems + 500 essence + 1 booster) matching `monetization-plan.md` spec
- Added weekly event economy fields to `WeeklyEventConfigSO`: `ThreeStarBonusEssence` (25), `CompletionGems` (30), `AllThreeStarGems` (20) matching `economy-model.md` §3.3-3.4

**Test Coverage Added:**
- Created `AnalyticsManagerTests.cs` — 12 tests covering all log methods, event accumulation, OnEventLogged callback, null parameter handling
- Created `FirebaseAuthManagerTests.cs` — 9 tests covering auth lifecycle, GUID generation, event callbacks, sign-in/sign-out cycles
- Created `TutorialLevelDataTests.cs` — 20 tests covering all 5 tutorial levels, board setup validation, step types, obsolete API
- Added 2 blocker-related tests to `LevelLoaderTests.cs`
- Added 3 tests to `IAPManagerTests.cs` for weekly_deal SKU (catalog count, purchase grant, re-purchase)
- Updated `Catalog_HasSixEntries` test to `Catalog_HasSevenEntries`

**Doc/Code Reconciliation:**
- Updated `monetization-plan.md` fail recovery from +3 to +5 moves (matching `EconomyConfigSO.ExtraMovesFromAd`)
- Updated `economy-model.md` §3.3 weekly event per-level essence from 50 to 75 (matching `WeeklyEventConfigSO.EssencePerLevel`), event max from 1,275 to 1,450
- Updated `economy-model.md` §7.1 non-payer recovery from +3 to +5 moves
- Updated `economy-model.md` §9.2 tuning knobs event essence total from 1,275 to 1,450

**Cleanup:**
- Added `README.md` to `levels/` marking it as authoring mirror of `Resources/Levels/`

### Prior Sessions (1-6)

See previous handoff for complete history of Foundation, Core Loop, Feel & Juice, Meta & Economy, Content & Polish, Integration Wiring, and Code Review Sprint phases.

## In Progress

- None. Gate 5 code sprint complete. Ready for Unity Editor testing.

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (now 31+ test files, ~400+ methods). Fix any compilation or test failures.
2. Create ScriptableObject assets in Unity: `EconomyConfigSO`, `PotionShelfConfigSO`, `WorkshopConfigSO`, `DailyBrewConfigSO`, `WinStreakConfigSO`, `WeeklyEventConfigSO`, `NotificationConfigSO`. Assign all to `GameFlowController` inspector fields.
3. Implement blocker gameplay in the board engine: add `Blocker` to `CellContentType`, handle in `BoardModel`/`ClusterDetector`/`CascadeResolver`/`BoardPresenter` — levels 36-40 now carry blocker data but gameplay logic is pending.

## Blockers / Risks

- Blocker: Blocker gameplay NOT yet implemented — `LevelConfig.BlockerPlacements` data is preserved from JSON but `CellContentType` has no `Blocker` variant; levels 36-40 play as if they have no blockers
- Risk: Firebase SDK not yet imported — backend managers are pure C# abstractions
- Risk: Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs
- Risk: `BoardPresenter.ActivateExtraMoves` no longer has a default parameter — any unknown callers must be updated
- Blocker: Gate 5 requires Unity Editor runtime testing, device matrix testing, and app store asset creation

## Decisions Recorded This Session (Session 7)

- Blocker data preserved in `LevelConfig` but gameplay deferred — avoids cascading changes to board engine this close to Gate 5
- Extra moves kept at 5 (code authoritative); documentation updated to match
- Weekly event per-level essence aligned to 75 (code), doc updated from 50
- `localHour` renamed to `utcHour` — parameter was always computed from UTC dates

## Files Touched This Session (Session 7)

### Modified — Core
- `Assets/_Project/Scripts/Core/LevelConfig.cs` — added `BlockerPlacement` struct and `BlockerPlacements` property
- `Assets/_Project/Scripts/Core/Economy/IAPManager.cs` — added `weekly_deal` SKU
- `Assets/_Project/Scripts/Core/Services/NotificationManager.cs` — renamed `localHour` to `utcHour`

### Modified — Presentation
- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs` — removed default param from `ActivateExtraMoves`
- `Assets/_Project/Scripts/Presentation/BoosterBarUI.cs` — added `_extraMovesAmount` field, config injection via `Initialize()`
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs` — added `_levelStartTimeUtc`, duration tracking

### Modified — Data
- `Assets/_Project/Scripts/Data/LevelLoader.cs` — blocker placement mapping in `ConvertToLevelConfig`
- `Assets/_Project/Scripts/Data/LiveOps/WeeklyEventConfigSO.cs` — added 3 event economy fields

### Created — Tests
- `Assets/Tests/EditMode/Core/Backend/AnalyticsManagerTests.cs` — 12 tests
- `Assets/Tests/EditMode/Core/Backend/FirebaseAuthManagerTests.cs` — 9 tests
- `Assets/Tests/EditMode/Core/TutorialLevelDataTests.cs` — 20 tests

### Modified — Tests
- `Assets/Tests/EditMode/Core/Economy/IAPManagerTests.cs` — 3 new tests, updated catalog count assertion
- `Assets/Tests/EditMode/Data/LevelLoaderTests.cs` — 2 new blocker tests

### Modified — Docs
- `docs/economy/monetization-plan.md` — fail recovery +3 → +5 moves
- `docs/economy/economy-model.md` — §3.3, §7.1, §9.2 updated to match code
- `CHANGELOG.md`
- `docs/project-management/session-handoff.md` (this file)
- `docs/project-management/weekly-focus.md`
- `docs/project-management/context-snapshot.md`

### Created — Docs
- `levels/README.md` — authoring mirror notice

### Created — Level Data
- `Assets/_Project/Resources/Levels/level_001.json` — was missing from Resources (only existed in authoring mirror `levels/`)

## Handoff Checklist

- [x] Decision log updated
- [x] This handoff file updated
- [x] Changelog updated
- [x] Weekly focus updated
- [x] Context snapshot regenerated
- [ ] ADR index validated — no new ADRs this session
- [ ] CI/local readiness checks run and results recorded (requires Unity Editor)
- [ ] ScriptableObject assets created and assigned (requires Unity Editor)
