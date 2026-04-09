# Brew — Context Snapshot

> Generated: 2026-04-09 (Session 6 — Code Review Sprint)
> Project summary updated: 2026-04-09
> Session handoff updated: 2026-04-09

## Quick Resume

- Branch: `cursor/add-changelog-adr-session-handoff`
- Canonical handoff: `docs/project-management/session-handoff.md`
- ADR index: `docs/adr/README.md`
- Changelog: `CHANGELOG.md`

## Current Milestone Focus

- Milestone: **Content & Polish (Weeks 9-10)** — code review sprint complete, working toward Milestone Gate 5
- Goal: Unity Editor testing, ScriptableObject asset creation, Gate 5 verification
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/05-week-9-10-content-and-polish.md`

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (28+ test files, ~350+ methods). Fix any compilation or test failures.
2. Create ScriptableObject assets (EconomyConfigSO, PotionShelfConfigSO, WorkshopConfigSO, DailyBrewConfigSO, WinStreakConfigSO, WeeklyEventConfigSO, NotificationConfigSO). Assign to GameFlowController.
3. Play full session: campaign levels → event flow → daily brew → workshop → verify streak on fail. Complete Gate 5 checklist.

## Blockers / Risks

- Risk: Firebase SDK not yet imported — backend managers are pure C# abstractions
- Risk: Unity IAP and AdMob SDKs not yet configured
- Risk: BoardPresenter/BoosterBarUI hardcode extra-moves amount (5) instead of config
- Risk: economy-model.md §3.3-3.4 needs updating to match implementation
- Blocker: Gate 5 requires Unity Editor runtime testing

## Decisions This Session

- BoosterManager defaults to 0 charges (was int.MaxValue) — must be explicitly granted
- ActivateExtraMoves requires explicit amount parameter — no hidden defaults
- Streak resets on fail unless player pays for protection — Retry path now resets streak
- LevelLoader.GetMaxLevelId() cached to avoid repeated resource loading
- Level 40 uses 9×10 grid for distinctive finale feel
- Config SO fallbacks are defensive-only — to be removed after Unity asset creation

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
- EconomyConfigSO (updated: +4 fields), RemoteConfigDefaults
- PotionShelfConfigSO, WorkshopConfigSO, WinStreakConfigSO, DailyBrewConfigSO
- WeeklyEventConfigSO, NotificationConfigSO
- SfxId, LevelLoader (updated: +GetMaxLevelId), PlayerProgress, LocalSaveManager, SettingsManager

### Presentation Layer — 29 files

- BoardPresenter (updated), InputController, TokenView, TokenAnimator
- HudController, RecipeVialUI
- LevelCompleteScreen, LevelFailScreen, LevelSelectScreen, LevelButton
- GameFlowController (major update: config SO wiring, streak fix, settings)
- AudioManager, AdaptiveAudioController, HapticManager
- ScreenShakeController, ParticleManager, BrewAnimationController
- TutorialController, TutorialOverlay
- BoosterBarUI (updated), BoosterSlotUI, SettingsPanel (updated: +3 buttons)
- PotionShelfView, WorkshopView, DailyBrewUI
- WalletUI, StoreUI, BoosterShopUI
- WeeklyEventView

### Tests — 28 files, ~350+ methods

All prior test files plus updates to BoosterManagerTests

## ADR Summary

| Number | Title | Status | Date |
| ------ | ----- | ------ | ---- |
| 0001 | Canonical Unity Version | accepted | 2026-04-09 |
| 0002 | Offline-First Cloud Save | accepted | 2026-04-09 |
| 0003 | Remote Config as Config Source | accepted | 2026-04-09 |

## Recent Git Commits

- (pending) 2026-04-09 Week 10 code review sprint: fix P0/P1 bugs, rebalance levels 36-40, complete settings, soft launch prep
- 3a197a9 2026-04-09 Wire event system integration, notification adapters, and harden test coverage for Gate 5
- 722664d 2026-04-09 Add Week 9-10 systems: weekly events, notifications, CurrencyFormatter, event level templates
