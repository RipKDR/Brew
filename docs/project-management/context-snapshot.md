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

- Milestone: **Meta & Economy (Weeks 7-8)** — working toward Milestone Gate 4
- Goal: Build meta-progression (potion shelf, workshop, daily brew, win streaks), full economy (dual currency, IAP, ads), and Firebase backend
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/04-week-7-8-meta-and-economy.md`

## Next 3 Tasks

1. Build economy foundation: CurrencyManager (Essence + Gems), RewardCalculator, EconomyConfigSO with all tuning values from economy-model.md.
2. Build meta-progression Core classes: PotionShelfManager, WorkshopManager, WinStreakTracker, DailyBrewManager — all pure C# with config SOs.
3. Build monetization + backend: IAPManager, AdManager, FirebaseAuthManager, CloudSaveManager, RemoteConfigManager, AnalyticsManager.

## Blockers / Risks

- Risk: Firebase SDK not yet imported into Unity project — backend managers use interface abstractions so they compile without SDK.
- Risk: Unity IAP package not yet configured — IAPManager uses abstracted purchase flow.
- Risk: Gate 3 runtime verification still pending (external playtest + Unity Editor testing).
- Risk: Ad SDK (AdMob) not yet integrated — AdManager uses stub callbacks.

## Decisions This Session

- CurrencyManager is pure C# with event-driven notifications — no MonoBehaviour dependency.
- All economy values come from ScriptableObject configs or Remote Config — zero hardcoded values in code.
- Offline-first cloud save: write local immediately, sync to Firestore when connected.
- Remote Config as single source of truth for all tunable values at runtime.

## Code Inventory

### Core Layer (pure C#, zero Unity deps) — 21 files

- Enums: IngredientColor, CellContentType, BoardPhase, LevelOutcome, BoosterType
- Data types: CellContent, GridCoord, RecipeTarget, LevelConfig, RecipeProgress, FusionResult, ChainFusionResult, GravityStep, TutorialLevelData
- Logic: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver
- Game systems: MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, BoosterManager

### Data Layer — 8 files

- BoardConfigSO, AudioConfigSO, ScreenShakeConfigSO (ScriptableObjects)
- SfxId (enum)
- LevelLoader (JSON parser)
- PlayerProgress + LevelStarEntry (save data)
- LocalSaveManager (persistence)
- SettingsManager (PlayerPrefs-backed)

### Presentation Layer — 22 files

- BoardPresenter, InputController, TokenView, TokenAnimator
- HudController, RecipeVialUI
- LevelCompleteScreen, LevelFailScreen
- LevelSelectScreen, LevelButton
- GameFlowController
- AudioManager, AdaptiveAudioController
- HapticManager
- ScreenShakeController, ParticleManager
- BrewAnimationController
- TutorialController, TutorialOverlay
- BoosterBarUI, BoosterSlotUI
- SettingsPanel

### Utilities — 1 file

- ObjectPool

### Editor — 1 file

- BrewSceneSetup

### Tests — 14 files, ~155+ methods

- Core: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver, MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, LevelConfig, BoosterManager
- Data: LevelLoader, LevelDataIntegration

## ADR Summary


| Number | Title                   | Status   | Date       |
| ------ | ----------------------- | -------- | ---------- |
| 0001   | Canonical Unity Version | accepted | 2026-04-09 |


## Changelog (Unreleased)

### Added — Feel & Juice (Gate 3 prep)

- Booster System: BoosterType, BoosterManager, BoosterBarUI, BoosterSlotUI
- Audio System: AudioManager, AudioConfigSO, SfxId, AdaptiveAudioController
- Haptic System: HapticManager
- Screen Shake: ScreenShakeConfigSO, ScreenShakeController
- Particle System: ParticleManager
- Token Animation: TokenAnimator
- Brew Animation: BrewAnimationController
- Tutorial System: TutorialLevelData, TutorialController, TutorialOverlay
- Settings: SettingsManager, SettingsPanel
- Scene Setup: Enhanced BrewSceneSetup
- Tests: BoosterManagerTests

### Previously Added

- Core Loop: MoveTracker, ScoreCalculator, RecipeTracker, WinLoseEvaluator, LevelConfig, LevelLoader, PlayerProgress, LocalSaveManager, full UI screen set, 40 level JSONs
- Foundation: BoardModel, ClusterDetector, TokenSpawner, BoardStateMachine, FusionEngine, CascadeResolver, BoardPresenter, InputController, TokenView, ObjectPool, BoardConfigSO

## Recent Git Commits

- f18aff9 2026-04-09 Add remaining meta files, project settings, and doc updates
- 268a255 2026-04-09 Implement Feel & Juice systems for Gate 3 prep
- 339fde3 2026-04-09 Fix formatting in Firebase config README
- 23767d5 2026-04-09 Add Adaptive Performance settings, Timeline config
- 83a15d5 2026-04-09 Implement Foundation + Core Loop gameplay systems (Gate 1-2)
