# Brew — Context Snapshot

> Generated: 2026-04-09 (Session 6)
> Project summary updated: 2026-04-09
> Session handoff updated: 2026-04-09

## Quick Resume

- Branch: `cursor/add-changelog-adr-session-handoff`
- Canonical handoff: `docs/project-management/session-handoff.md`
- ADR index: `docs/adr/README.md`
- Changelog: `CHANGELOG.md`

## Current Milestone Focus

- Milestone: **Content & Polish (Weeks 9-10)** — code complete, working toward Milestone Gate 5
- Goal: Weekly event system, push notifications, level tuning, CurrencyFormatter, settings expansion, soft launch preparation
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/05-week-9-10-content-and-polish.md`

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (27 test files, ~300+ methods). Fix any compilation or test failures.
2. Create ScriptableObject assets (WeeklyEventConfigSO, NotificationConfigSO, plus any missing Week 7-8 configs). Assign to GameFlowController.
3. Play full session including event flow: activate event via Remote Config → play event levels → claim milestone rewards → verify notification scheduling. Complete Gate 5 checklist items.

## Blockers / Risks

- Risk: Firebase SDK not yet imported — backend managers are pure C# abstractions awaiting SDK wiring.
- Risk: Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs.
- Risk: NotificationManager needs platform-specific INotificationScheduler implementations.
- Risk: Event theme overlay needs level loader modifications for ingredient swapping.
- Blocker: Gate 5 requires full device matrix testing and app store asset creation.

## Decisions This Session

- WeeklyEventManager is pure C# with Func<bool> feature flag — fully disableable via Remote Config.
- Event milestone rewards injected via MilestoneDefinition, not hardcoded.
- NotificationManager uses INotificationScheduler interface — platform abstraction for testability.
- Notification frequency cap: 1/day, 7-day re-prompt cooldown.
- CurrencyFormatter is centralized static utility — single format function, no duplication.
- Event level templates use "themed" ingredient placeholder for runtime swapping.
- 13 new Remote Config keys for event system — all server-configurable.

## Code Inventory

### Core Layer (pure C#, zero Unity deps) — 35 files

- Enums: IngredientColor, CellContentType, BoardPhase, LevelOutcome, BoosterType, CurrencyType, AdPlacement, EventState, NotificationType
- Data types: CellContent, GridCoord, RecipeTarget, LevelConfig, RecipeProgress, FusionResult, ChainFusionResult, GravityStep, TutorialLevelData, IAPProduct, PotionShelfMilestoneDefinition, DailyStreakBonusDefinition, PlayerSaveData, MilestoneDefinition
- Logic: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver
- Game systems: MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, BoosterManager
- Economy: CurrencyManager, RewardCalculator, IAPManager
- Meta: PotionShelfManager, WorkshopManager, WinStreakTracker, DailyBrewManager
- LiveOps: WeeklyEventManager
- Services: NotificationManager, INotificationScheduler
- Ads: AdManager
- Backend: FirebaseAuthManager, CloudSaveManager, RemoteConfigManager, AnalyticsManager

### Data Layer — 16 files

- BoardConfigSO, AudioConfigSO, ScreenShakeConfigSO
- EconomyConfigSO, RemoteConfigDefaults
- PotionShelfConfigSO, WorkshopConfigSO, WinStreakConfigSO, DailyBrewConfigSO
- WeeklyEventConfigSO, NotificationConfigSO
- SfxId, LevelLoader, PlayerProgress, LocalSaveManager, SettingsManager

### Presentation Layer — 29 files

- BoardPresenter, InputController, TokenView, TokenAnimator
- HudController, RecipeVialUI
- LevelCompleteScreen, LevelFailScreen, LevelSelectScreen, LevelButton
- GameFlowController
- AudioManager, AdaptiveAudioController, HapticManager
- ScreenShakeController, ParticleManager, BrewAnimationController
- TutorialController, TutorialOverlay
- BoosterBarUI, BoosterSlotUI, SettingsPanel
- PotionShelfView, WorkshopView, DailyBrewUI
- WalletUI, StoreUI, BoosterShopUI
- WeeklyEventView

### Utilities — 2 files

- ObjectPool, CurrencyFormatter

### Editor — 1 file

- BrewSceneSetup

### Tests — 27 files, ~300+ methods

- Core: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver, MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, LevelConfig, BoosterManager
- Economy: CurrencyManager, RewardCalculator, IAPManager
- Meta: PotionShelfManager, WorkshopManager, WinStreakTracker, DailyBrewManager
- LiveOps: WeeklyEventManager
- Services: NotificationManager
- Ads: AdManager
- Backend: CloudSaveManager, RemoteConfigManager
- Utilities: CurrencyFormatter
- Data: LevelLoader, LevelDataIntegration

## ADR Summary

| Number | Title | Status | Date |
| ------ | ----- | ------ | ---- |
| 0001 | Canonical Unity Version | accepted | 2026-04-09 |
| 0002 | Offline-First Cloud Save | accepted | 2026-04-09 |
| 0003 | Remote Config as Config Source | accepted | 2026-04-09 |

## Recent Git Commits

- (pending) 2026-04-09 Add Week 9-10 systems: weekly events, notifications, CurrencyFormatter, event levels
- 3ed5842 2026-04-09 Complete Week 7-8 session close: handoff, changelog, ADRs 0002-0003, context snapshot
- 11230eb 2026-04-09 Add Presentation layer for meta/economy systems and wire into GameFlowController
- 751122d 2026-04-09 Fix XML doc cref to use fully qualified BoosterManager reference
- 1e2aabf 2026-04-09 Add Meta/Economy/Backend/Ads systems and fix 15 bugs from code review
