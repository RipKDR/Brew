# Brew — Context Snapshot

> Generated: 2026-04-09 (Session 5)
> Project summary updated: 2026-04-09
> Session handoff updated: 2026-04-09

## Quick Resume

- Branch: `cursor/add-changelog-adr-session-handoff`
- Canonical handoff: `docs/project-management/session-handoff.md`
- ADR index: `docs/adr/README.md`
- Changelog: `CHANGELOG.md`

## Current Milestone Focus

- Milestone: **Meta & Economy (Weeks 7-8)** — code complete, working toward Milestone Gate 4
- Goal: Full meta-progression, dual-currency economy, IAP, ads, Firebase backend abstractions
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/04-week-7-8-meta-and-economy.md`

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (24 test files, ~255+ methods). Fix any compilation or test failures.
2. Create ScriptableObject assets (EconomyConfigSO, PotionShelfConfigSO, WorkshopConfigSO, WinStreakConfigSO, DailyBrewConfigSO). Assign to GameFlowController.
3. Play full session loop: complete level → earn Essence → open workshop → purchase upgrade → complete daily brew → verify potion shelf → test IAP stubs. Complete Gate 4 checklist.

## Blockers / Risks

- Risk: Firebase SDK not yet imported — backend managers are pure C# abstractions awaiting SDK wiring.
- Risk: Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs.
- Risk: Gate 3 runtime verification still pending (external playtest + Unity Editor testing).
- Blocker: Gate 4 requires IAP sandbox testing on both platforms.

## Decisions This Session

- CurrencyManager is pure C# — zero Unity dependencies, testable in EditMode.
- All economy values from EconomyConfigSO or RemoteConfigManager — zero hardcoded values.
- Offline-first cloud save via CloudSaveManager (ADR 0002).
- Remote Config as single config source at runtime (ADR 0003).
- WorkshopManager enforces sequential purchases only.
- DailyBrewManager uses UTC dates for consistency.
- AdManager uses Func<bool> callback for No-Ads Pass — decoupled from IAPManager.
- IAPManager product catalog is static readonly.
- AnalyticsManager maintains in-memory event log for debugging.

## Code Inventory

### Core Layer (pure C#, zero Unity deps) — 32 files

- Enums: IngredientColor, CellContentType, BoardPhase, LevelOutcome, BoosterType, CurrencyType, AdPlacement
- Data types: CellContent, GridCoord, RecipeTarget, LevelConfig, RecipeProgress, FusionResult, ChainFusionResult, GravityStep, TutorialLevelData, IAPProduct, PotionShelfMilestoneDefinition, DailyStreakBonusDefinition, PlayerSaveData
- Logic: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver
- Game systems: MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, BoosterManager
- Economy: CurrencyManager, RewardCalculator, IAPManager
- Meta: PotionShelfManager, WorkshopManager, WinStreakTracker, DailyBrewManager
- Ads: AdManager
- Backend: FirebaseAuthManager, CloudSaveManager, RemoteConfigManager, AnalyticsManager

### Data Layer — 14 files

- BoardConfigSO, AudioConfigSO, ScreenShakeConfigSO
- EconomyConfigSO, RemoteConfigDefaults
- PotionShelfConfigSO, WorkshopConfigSO, WinStreakConfigSO, DailyBrewConfigSO
- SfxId, LevelLoader, PlayerProgress, LocalSaveManager, SettingsManager

### Presentation Layer — 28 files

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

### Utilities — 1 file

- ObjectPool

### Editor — 1 file

- BrewSceneSetup

### Tests — 24 files, ~255+ methods

- Core: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver, MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, LevelConfig, BoosterManager
- Economy: CurrencyManager, RewardCalculator, IAPManager
- Meta: PotionShelfManager, WorkshopManager, WinStreakTracker, DailyBrewManager
- Ads: AdManager
- Backend: CloudSaveManager, RemoteConfigManager
- Data: LevelLoader, LevelDataIntegration

## ADR Summary

| Number | Title | Status | Date |
| ------ | ----- | ------ | ---- |
| 0001 | Canonical Unity Version | accepted | 2026-04-09 |
| 0002 | Offline-First Cloud Save | accepted | 2026-04-09 |
| 0003 | Remote Config as Config Source | accepted | 2026-04-09 |

## Recent Git Commits

- 11230eb 2026-04-09 Add Presentation layer for meta/economy systems and wire into GameFlowController
- 751122d 2026-04-09 Fix XML doc cref to use fully qualified BoosterManager reference
- 1e2aabf 2026-04-09 Add Meta/Economy/Backend/Ads systems and fix 15 bugs from code review
- c50e16e 2026-04-09 Update project docs to reflect Week 7-8 phase start
- f18aff9 2026-04-09 Add remaining meta files, project settings, and doc updates
