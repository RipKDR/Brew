# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09

---

## Current Milestone Focus

- Milestone: **Content & Polish (Weeks 9-10)** — code review sprint complete, working toward Milestone Gate 5
- Goal: Unity Editor testing, ScriptableObject asset creation, Gate 5 verification
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/05-week-9-10-content-and-polish.md`

## Current Branch + Baseline

- Branch: `cursor/add-changelog-adr-session-handoff`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, and `lint` should pass

## Completed Since Last Handoff

### Code Review Sprint (This Session — Session 6)

**P0 Fixes (Compilation):**
- Fixed `AnalyticsManager.LogEventStart` signature: now `(string eventId, string eventName)` — was `(string eventId)`
- Fixed `AnalyticsManager.LogEventComplete` signature: now `(string eventId, int completedLevelCount)` — was `(string eventId)`

**P1 Fixes (Logic):**
- Closed streak exploit: streak now resets on fail when protection panel is not shown, AND on retry when protection was not purchased
- Removed all hardcoded economy values from `GameFlowController.InitializeSystems()` — now reads from config SOs with fallbacks
- Tutorial check uses `LevelConfig.IsTutorial` instead of `LevelId <= 5`
- Level cap uses `LevelLoader.GetMaxLevelId()` instead of hardcoded `40`
- Remaining-move bonus reads from `EconomyConfigSO.RemainingMoveBonusPerMove` instead of `* 50`
- Extra moves from ad reads from `EconomyConfigSO.ExtraMovesFromAd` instead of hardcoded `3`

**P2 Fixes (Quality):**
- `CloudSaveManager.PlayerSaveData` — 12 public fields → auto-properties
- `BoosterManager` — default charges from `int.MaxValue` → `0`; removed default parameter from `ActivateExtraMoves`
- `GameFlowController` — added `[SerializeField]` for `PotionShelfConfigSO`, `WorkshopConfigSO`, `DailyBrewConfigSO`, `WinStreakConfigSO`, `SettingsPanel`
- Workshop analytics cost reads from `WorkshopConfigSO` instead of duplicated cost array
- `TutorialLevelData.IsTutorialLevel()` marked `[Obsolete]`

**SettingsPanel Completion:**
- Added privacy policy button (`Application.OpenURL`)
- Added credits panel toggle
- Added restore purchases button with `OnRestorePurchasesRequested` event
- Wired `SettingsPanel.Initialize(IAPManager)` in `GameFlowController`

**Level Rebalancing:**
- Levels 36-40 rebalanced as proper challenge levels: 3-4 recipe targets, 36-45 moves, 1-4 blockers
- L39 is peak difficulty (4 recipes, 8 brews, 38 moves, 4 blockers)
- L40 is satisfying finale (9×10, 3 recipes, 45 moves, 1 blocker)
- Synced root `levels/` directory

**New Documents:**
- `docs/production/soft-launch-plan.md` — complete soft launch plan with markets, budget, metrics, kill criteria
- `marketing/store-listing/store-listing.md` — app name, description, keywords per UA creative strategy

**Config Additions:**
- `EconomyConfigSO`: added `RewardedAdDailyCap`, `InterstitialFrequency`, `RemainingMoveBonusPerMove`, `ExtraMovesFromAd`
- `LevelLoader.GetMaxLevelId()` — cached scan of level Resources

**Test Updates:**
- `BoosterManagerTests` — updated for zero-default charges and explicit `ActivateExtraMoves` amount

### Prior Sessions (1-5)

See previous handoff for complete history of Foundation, Core Loop, Feel & Juice, Meta & Economy, Content & Polish, and Integration Wiring phases.

## In Progress

- None. Code review sprint complete. Ready for Unity Editor testing.

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (28+ test files, ~350+ methods). Fix any compilation or test failures.
2. Create ScriptableObject assets in Unity: `EconomyConfigSO`, `PotionShelfConfigSO`, `WorkshopConfigSO`, `DailyBrewConfigSO`, `WinStreakConfigSO`, `WeeklyEventConfigSO`, `NotificationConfigSO`. Assign all to `GameFlowController` inspector fields.
3. Play full session: campaign levels 1-5 → event flow → daily brew → workshop upgrade → verify streak reset on fail. Complete Gate 5 checklist items.

## Blockers / Risks

- Risk: Firebase SDK not yet imported — backend managers are pure C# abstractions
- Risk: Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs
- Risk: `BoardPresenter.ActivateExtraMoves()` and `BoosterBarUI` still hardcode `5` for extra moves instead of reading from config
- Risk: `levels/` root directory is a stale copy of `Resources/Levels/` — needs consolidation or deletion
- Risk: `economy-model.md` §3.3-3.4 (weekly event rewards) does not match implementation — doc needs updating
- Risk: `monetization-plan.md` mentions `weekly_deal` SKU not in `IAPManager.Catalog`
- Blocker: Gate 5 requires Unity Editor runtime testing, device matrix testing, and app store asset creation

## Decisions Recorded This Session (Session 6)

- BoosterManager defaults to 0 charges — infinite charges were a development convenience that leaked into production code. Boosters must be explicitly granted.
- `ActivateExtraMoves` requires explicit `amount` parameter — no default, callers must read from config. Prevents silent drift between config and hardcoded values.
- Streak resets on fail unless player explicitly pays for protection (ad or gems) — the "Retry" path now resets streak if protection was not purchased, closing the exploit.
- `LevelLoader.GetMaxLevelId()` is cached — avoids loading all level TextAssets on every `AdvanceToNextLevel` call.
- Level 40 uses 9×10 grid (wider than 36-39) — gives the finale a distinctive, generous feel while staying within spec limits.
- Config SO fallbacks remain as defensive code — removed after SO assets are created in Unity Editor.

## Files Touched This Session (Session 6)

### Modified — Core
- `Assets/_Project/Scripts/Core/Backend/AnalyticsManager.cs` — fixed `LogEventStart` and `LogEventComplete` signatures
- `Assets/_Project/Scripts/Core/Backend/CloudSaveManager.cs` — `PlayerSaveData` fields → properties
- `Assets/_Project/Scripts/Core/BoosterManager.cs` — default charges to 0, removed default parameter
- `Assets/_Project/Scripts/Core/TutorialLevelData.cs` — marked `IsTutorialLevel()` obsolete

### Modified — Data
- `Assets/_Project/Scripts/Data/Economy/EconomyConfigSO.cs` — added 4 config fields
- `Assets/_Project/Scripts/Data/LevelLoader.cs` — added cached `GetMaxLevelId()`

### Modified — Presentation
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs` — config SO wiring, streak exploit fix, magic number removal, settings panel wiring
- `Assets/_Project/Scripts/Presentation/SettingsPanel.cs` — added privacy, credits, restore purchases
- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs` — `ActivateExtraMoves` requires amount
- `Assets/_Project/Scripts/Presentation/BoosterBarUI.cs` — explicit amount parameter

### Modified — Tests
- `Assets/Tests/EditMode/Core/BoosterManagerTests.cs` — updated for new defaults and API

### Modified — Level Data
- `Assets/_Project/Resources/Levels/level_036.json` through `level_040.json` — rebalanced
- `levels/level_036.json` through `levels/level_040.json` — synced

### Created — Docs
- `docs/production/soft-launch-plan.md`
- `marketing/store-listing/store-listing.md`

### Modified — Docs
- `CHANGELOG.md`
- `docs/project-management/session-handoff.md` (this file)
- `docs/project-management/project-summary.md`
- `docs/project-management/weekly-focus.md`

## Handoff Checklist

- [x] Decision log updated
- [x] This handoff file updated
- [x] Changelog updated
- [x] Weekly focus updated
- [ ] Context snapshot regenerated
- [ ] ADR index validated — no new ADRs this session
- [ ] CI/local readiness checks run and results recorded (requires Unity Editor)
- [ ] ScriptableObject assets created and assigned (requires Unity Editor)
