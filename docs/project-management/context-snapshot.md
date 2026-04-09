# Brew — Context Snapshot

> Generated: 2026-04-09 (Session 7 — Gate 5 Code Sprint)
> Project summary updated: 2026-04-09
> Session handoff updated: 2026-04-09

## Quick Resume

- Branch: `cursor/add-changelog-adr-session-handoff`
- Canonical handoff: `docs/project-management/session-handoff.md`
- ADR index: `docs/adr/README.md`
- Changelog: `CHANGELOG.md`

## Current Milestone Focus

- Milestone: **Content & Polish (Weeks 9-10)** — Gate 5 code sprint complete, working toward Milestone Gate 5
- Goal: Unity Editor testing, ScriptableObject asset creation, blocker gameplay implementation, Gate 5 verification
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/05-week-9-10-content-and-polish.md`

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (now 31+ test files, ~400+ methods). Fix any compilation or test failures.
2. Create ScriptableObject assets (EconomyConfigSO, PotionShelfConfigSO, WorkshopConfigSO, DailyBrewConfigSO, WinStreakConfigSO, WeeklyEventConfigSO, NotificationConfigSO). Assign to GameFlowController.
3. Implement blocker gameplay in the board engine: add `Blocker` to `CellContentType`, handle in `BoardModel`/`ClusterDetector`/`CascadeResolver`/`BoardPresenter` — levels 36-40 now carry blocker data but gameplay logic is pending.

## Blockers / Risks

- Blocker: Blocker gameplay NOT yet implemented — `LevelConfig.BlockerPlacements` data preserved but `CellContentType` has no `Blocker` variant; levels 36-40 play without blockers
- Risk: Firebase SDK not yet imported — backend managers are pure C# abstractions
- Risk: Unity IAP and AdMob SDKs not yet configured
- Risk: `BoardPresenter.ActivateExtraMoves` no longer has a default parameter — unknown callers must be updated
- Blocker: Gate 5 requires Unity Editor runtime testing

## Decisions This Session

- Blocker data preserved in `LevelConfig` but gameplay deferred — avoids cascading changes to board engine this close to Gate 5
- Extra moves kept at 5 (code authoritative); documentation updated to match
- Weekly event per-level essence aligned to 75 (code), doc updated from 50
- `localHour` renamed to `utcHour` — parameter was always computed from UTC dates

## Code Inventory

### Core Layer (pure C#, zero Unity deps) — 36 files

- Enums: IngredientColor, CellContentType, BoardPhase, LevelOutcome, BoosterType, CurrencyType, AdPlacement, EventState, NotificationType
- Data types: CellContent, GridCoord, RecipeTarget, LevelConfig (updated: +BlockerPlacement, +BlockerPlacements), RecipeProgress, FusionResult, ChainFusionResult, GravityStep, TutorialLevelData, IAPProduct, PotionShelfMilestoneDefinition, DailyStreakBonusDefinition, PlayerSaveData, MilestoneDefinition
- Logic: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver
- Game systems: MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, BoosterManager
- Economy: CurrencyManager, RewardCalculator, IAPManager (updated: +weekly_deal SKU)
- Meta: PotionShelfManager, WorkshopManager, WinStreakTracker, DailyBrewManager
- LiveOps: WeeklyEventManager
- Services: NotificationManager (updated: utcHour param), INotificationScheduler
- Ads: AdManager
- Backend: FirebaseAuthManager, CloudSaveManager, RemoteConfigManager, AnalyticsManager

### Data Layer — 16 files

- BoardConfigSO, AudioConfigSO, ScreenShakeConfigSO
- EconomyConfigSO, RemoteConfigDefaults
- PotionShelfConfigSO, WorkshopConfigSO, WinStreakConfigSO, DailyBrewConfigSO
- WeeklyEventConfigSO (updated: +3 event economy fields), NotificationConfigSO
- SfxId, LevelLoader (updated: +blocker mapping), PlayerProgress, LocalSaveManager, SettingsManager

### Presentation Layer — 29 files

- BoardPresenter (updated: explicit extra-moves amount), InputController, TokenView, TokenAnimator
- HudController, RecipeVialUI
- LevelCompleteScreen, LevelFailScreen, LevelSelectScreen, LevelButton
- GameFlowController (updated: +_levelStartTimeUtc, duration tracking)
- AudioManager, AdaptiveAudioController, HapticManager
- ScreenShakeController, ParticleManager, BrewAnimationController
- TutorialController, TutorialOverlay
- BoosterBarUI (updated: config-driven extra-moves amount), BoosterSlotUI, SettingsPanel
- PotionShelfView, WorkshopView, DailyBrewUI
- WalletUI, StoreUI, BoosterShopUI
- WeeklyEventView

### Tests — 31 files, ~400+ methods

- New: AnalyticsManagerTests (12), FirebaseAuthManagerTests (9), TutorialLevelDataTests (20)
- Updated: IAPManagerTests (+3 tests, catalog count assertion), LevelLoaderTests (+2 blocker tests)
- Prior: BoardModelTests, ClusterDetectorTests, TokenSpawnerTests, FusionEngineTests, CascadeResolverTests, MoveTrackerTests, ScoreCalculatorTests, RecipeTrackerTests, WinLoseEvaluatorTests, BoosterManagerTests, CurrencyManagerTests, RewardCalculatorTests, AdManagerTests, PotionShelfManagerTests, WorkshopManagerTests, WinStreakTrackerTests, DailyBrewManagerTests, CloudSaveManagerTests, RemoteConfigManagerTests, WeeklyEventManagerTests, NotificationManagerTests, CurrencyFormatterTests, EventLevelLoaderTests, LevelConfigTests, BoardStateMachineTests, BoardPresenterTests

## ADR Summary

| Number | Title | Status | Date |
| ------ | ----- | ------ | ---- |
| 0001 | Canonical Unity Version | accepted | 2026-04-09 |
| 0002 | Offline-First Cloud Save | accepted | 2026-04-09 |
| 0003 | Remote Config as Config Source | accepted | 2026-04-09 |

## Recent Git Commits

- (pending) 2026-04-09 Session 7 — Gate 5 code sprint: P1 bug fixes, blocker data, weekly_deal, test coverage, doc reconciliation
- (pending) 2026-04-09 Week 10 code review sprint: fix P0/P1 bugs, rebalance levels 36-40, complete settings, soft launch prep
- 3a197a9 2026-04-09 Wire event system integration, notification adapters, and harden test coverage for Gate 5
- 722664d 2026-04-09 Add Week 9-10 systems: weekly events, notifications, CurrencyFormatter, event level templates
