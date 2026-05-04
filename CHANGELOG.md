# Changelog

All notable changes to Brew are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), with project-specific sections for context and documentation integrity.

## [Unreleased]

### Fixed — Session 9 Double-Check Pass (2026-05-04)

- **Economy CI regression**: `simulate_economy.py` had two template-tag corruptions in working tree (lines 84/106) and a missing `total_levels_played = 0` initialisation — `validate-economy` job now passes clean
- **Version format**: `ProjectSettings.asset bundleVersion` was `1.0` (not `major.minor.patch`) — changed to `1.0.0`; `validate-levels` now exits 0
- **iOS export path**: `build.yml` referenced `ExportOptions.plist` from inside `build/iOS/` directory — changed to `$GITHUB_WORKSPACE/ExportOptions.plist`
- **Workshop cost rebalance**: Tiers 6-12 reduced so non-payer completes workshop in ~40 days (target 30-45); old total 36,350 → 28,350. `WorkshopConfigSO.cs` defaults and `economy-model.md` updated consistently
- **Editor utility committed**: `ConfigAssetGenerator.cs` (`Brew > Generate Config Assets` menu) was untracked — now committed; generates all Gate 5 ScriptableObject assets in one click

### Added — Session 8 Workspace Hardening (2026-05-04)

- **`level_001.json`**: Level 1 was missing from `Assets/_Project/Resources/Levels/`; all 40 levels now present and schema-valid
- **`CLAUDE.md`**: Project-level CLAUDE.md added for AI session continuity and hard-rule enforcement
- **`Fastfile` + `ExportOptions.plist`**: Required iOS build infrastructure that `build.yml` deploy-ios job references
- **`GameFlowControllerTests.cs`**: 12 EditMode tests covering reward calculation, streak pipeline, ad caps, currency mutation, level-cap guard
- **`BoardStateMachineIntegrationTests.cs`**: 10 integration tests for full board state machine cycle (Idle→…→CheckWin→Idle) with deterministic seeded board
- **CI improvements**: `validate-levels` and `validate-economy` step summaries added; `check-secrets` job warns on missing `UNITY_LICENSE`/`ANDROID_KEYSTORE_BASE64`/`ASC_KEY`

### Fixed — Session 8 Workspace Hardening

- **`BrewSceneSetup.cs`**: Restored 4 missing `using` directives removed in uncommitted working-tree diff

### Fixed — Week 10 Code Review Sprint

- **P0: AnalyticsManager compilation errors**: `LogEventStart` now accepts `(string eventId, string eventName)` and `LogEventComplete` accepts `(string eventId, int completedLevelCount)` — signatures match all call sites in `GameFlowController`
- **P1: Streak exploit**: Streak now correctly resets on level fail — immediate reset when protection panel is not shown, reset via Retry button when protection was not purchased
- **P1: Hardcoded economy values**: `GameFlowController.InitializeSystems()` now reads from config ScriptableObjects (`PotionShelfConfigSO`, `WorkshopConfigSO`, `DailyBrewConfigSO`, `WinStreakConfigSO`) with fallbacks
- **P1: Magic numbers replaced**: Remaining-move bonus, extra-moves-from-ad, ad daily cap, interstitial frequency now driven by `EconomyConfigSO`
- **P2: BoosterManager default charges**: Changed from `int.MaxValue` (infinite) to `0` — boosters must be explicitly granted via save data
- **P2: BoosterManager.ActivateExtraMoves**: Removed default parameter — callers must pass explicit amount from config
- **P2: Tutorial check**: `GameFlowController` now uses `LevelConfig.IsTutorial` property instead of hardcoded `LevelId <= 5`
- **P2: Level cap**: `AdvanceToNextLevel` uses `LevelLoader.GetMaxLevelId()` (cached scan of Resources/) instead of hardcoded `40`
- **P2: Workshop analytics cost**: Now reads from `WorkshopConfigSO` instead of duplicating the cost array
- **P2: CloudSaveManager.PlayerSaveData**: Converted 12 public fields to auto-properties per AGENTS.md conventions

### Added — Week 10 Code Review Sprint

- **Config SO fields**: `GameFlowController` now has `[SerializeField]` references for `PotionShelfConfigSO`, `WorkshopConfigSO`, `DailyBrewConfigSO`, `WinStreakConfigSO`, and `SettingsPanel`
- **EconomyConfigSO fields**: Added `RewardedAdDailyCap`, `InterstitialFrequency`, `RemainingMoveBonusPerMove`, `ExtraMovesFromAd`
- **SettingsPanel**: Added privacy policy button (opens URL), credits panel toggle, restore purchases button with `OnRestorePurchasesRequested` event
- **LevelLoader.GetMaxLevelId()**: Cached method that scans Resources/Levels/ for level count
- **Soft launch plan**: `docs/production/soft-launch-plan.md` — target markets, budget, metrics, kill criteria, decision checkpoints
- **Store listing**: `marketing/store-listing/store-listing.md` — app name, description, keywords, promotional text

### Changed — Week 10 Code Review Sprint

- **Levels 36-40 rebalanced** as proper challenge levels: all now have 3-4 recipe targets, 36-45 moves (down from 18-82), 1-4 stone blockers. L39 is the hardest (4 recipes, 8 brews, 38 moves, 4 blockers). L40 is a satisfying 9×10 finale (3 recipes, 45 moves)
- **BoosterManagerTests**: Updated for new default-zero charges and explicit `ActivateExtraMoves` amount parameter
- Synced root `levels/` directory with `Assets/_Project/Resources/Levels/` for levels 36-40
- `TutorialLevelData.IsTutorialLevel()` marked `[Obsolete]` — use `LevelConfig.IsTutorial` instead
- Updated `project-summary.md` milestone table and weekly focus

### Added — Integration Wiring (Gate 5 prep)

- **Event Theme Overlay**: `LevelLoader.LoadEventLevel(json, themedIngredientId)` resolves "themed" placeholder in event level JSONs to a concrete ingredient from Remote Config; falls back to Shadow
- **Notification Platform Adapters**: `UnityNotificationScheduler` (compile-guarded iOS/Android) and `NullNotificationScheduler` (Editor/unsupported fallback) implementing `INotificationScheduler`
- **Event Level Flow**: `GameFlowController.StartEventLevel()` loads event templates, `HandleEventLevelWin()` grants per-level Essence + milestone rewards via `CurrencyManager`
- **RemoteConfig Sync**: 48 additional key constants in `RemoteConfigDefaults.cs` (workshop costs, shelf milestones, daily brew streak bonuses, IAP amounts) — now 68 total matching JSON
- **Event Integration Tests**: `EventLevelLoaderTests` (11 tests: theme resolution, fallback, metadata, key count sync)
- **AdManager Tests**: 11 new tests covering events, tutorial path, constructor validation, placement edge cases
- **NotificationManager Tests**: 5 new tests covering EventEndingSoon event, next-day scheduling, null guard, custom params
- **CurrencyFormatter Tests**: 5 new boundary tests at exact tier transitions

### Changed — Integration Wiring

- `GameFlowController`: constructs `WeeklyEventManager` + `RemoteConfigManager` + `NotificationManager`; event level loading path; notification scheduling after wins and on level select
- `WeeklyEventView`: added `RefreshState(EventState)` for banner/level-select/results visibility management
- `LevelLoader`: extended `RawLevelData` with `grid_mask`, `blocker_placements`, `tutorial_steps` for event JSON compatibility
- `AGENTS.md`: updated phase to "Implementation — Content & Polish", fixed path references to `_Project/Scripts/`
- `project-summary.md`: milestone table updated (Milestones 1-4 Complete, Milestone 5 In Progress)

### Added — Content & Polish (Gate 5 prep)

- **Weekly Event System**: `WeeklyEventManager` (pure C#, event lifecycle state machine, 7-level completion tracking, milestone rewards at 3/5/all levels), `MilestoneDefinition` data class, `WeeklyEventConfigSO` (level count, reward amounts, move budgets, board size), `WeeklyEventView` (event banner, level select, results, timer countdown)
- **Push Notifications**: `NotificationManager` (pure C#, `INotificationScheduler` interface, 3 triggers: Daily Brew reminder, Streak at Risk, Event Ending Soon), `NotificationConfigSO` (caps, cooldowns, notification copy), frequency cap (1/day), permission cooldown (7 days)
- **CurrencyFormatter**: Static utility for compact currency display (exact < 1K, K suffix 1K-999K, M suffix 1M+), integrated into `WalletUI`
- **Event Level Templates**: 7 event level templates in `levels/events/` (E1-E2 easy, E3-E5 medium, E6 hard, E7 boss) with 7x9 board, themed ingredient slot
- **Analytics Expansion**: 6 new methods in `AnalyticsManager`: `LogEventStart`, `LogEventLevelComplete`, `LogEventComplete`, `LogEventRewardClaimed`, `LogNotificationScheduled`, `LogNotificationOpened`
- **Remote Config**: 13 new keys for event system (event_active, event_id, event_name, event_end_timestamp, theme colors, ingredient ID, reward values)
- **Tests**: `WeeklyEventManagerTests` (~18 methods), `NotificationManagerTests` (~15 methods), `CurrencyFormatterTests` (~12 methods)

### Changed — Content & Polish

- Updated `GameFlowController`: added `WeeklyEventView` SerializeField and initialization
- Updated `BrewSceneSetup`: creates WeeklyEvent screen in meta screens, wires to GameFlowController
- Updated `SettingsPanel`: added notifications toggle
- Updated `SettingsManager`: added `NotificationsEnabled` property (default true)
- Updated `WalletUI`: uses `CurrencyFormatter.Format()` for compact balance display

### Added — Meta & Economy (Gate 4 prep)

- **Economy Foundation**: `CurrencyType` enum, `CurrencyManager` (pure C#, Add/Spend with events, balance-never-negative), `RewardCalculator` (star × streak multiplier), `EconomyConfigSO` (all tuning values from economy-model.md)
- **Remote Config**: `RemoteConfigDefaults` static class, `config/firebase/remote-config-defaults.json` (56 keys), `RemoteConfigManager` (default + server merge, 12h stale check)
- **Potion Shelf**: `PotionShelfManager` (pure C#, brewed potion tracking, 4 milestone tiers with rewards), `PotionShelfConfigSO`, `PotionShelfView` (scrollable grid, milestone bar)
- **Workshop**: `WorkshopManager` (pure C#, 12 sequential upgrades 50–12,000 Essence), `WorkshopConfigSO`, `WorkshopView` (decoration toggle, purchase flow)
- **Win Streak**: `WinStreakTracker` (pure C#, 6-tier multiplier 1.0×–3.0×), `WinStreakConfigSO`
- **Daily Brew**: `DailyBrewManager` (pure C#, UTC-date tracking, streak bonuses at 3/5/7 days), `DailyBrewConfigSO`, `DailyBrewUI` (availability indicator, streak display)
- **IAP**: `IAPProduct` data class, `IAPManager` (6-product catalog: 4 Gem Packs, Starter Bundle, No-Ads Pass; purchase fulfillment via CurrencyManager)
- **Ads**: `AdPlacement` enum, `AdManager` (daily cap 5, per-placement limits, interstitial every 3rd win, No-Ads Pass suppression)
- **Firebase Backend**: `FirebaseAuthManager` (anonymous auth abstraction), `CloudSaveManager` (offline-first PlayerSaveData with dirty/synced flags), `AnalyticsManager` (facade for 18+ event types per event-tracking-plan.md)
- **Economy UI**: `WalletUI` (real-time balance display), `StoreUI` (IAP product listing), `BoosterShopUI` (Essence + Gem purchase paths)
- **Tests**: `CurrencyManagerTests`, `RewardCalculatorTests`, `IAPManagerTests`, `AdManagerTests`, `PotionShelfManagerTests`, `WorkshopManagerTests`, `WinStreakTrackerTests`, `DailyBrewManagerTests`, `CloudSaveManagerTests`, `RemoteConfigManagerTests` (~100+ new test methods)

### Changed — Meta & Economy

- Updated `GameFlowController`: full meta/economy/ad/analytics integration — Essence rewards on win, streak tracking, potion shelf unlock, milestone rewards, interstitial triggers, ad-for-moves, streak protection, daily brew flow
- Updated `LevelCompleteScreen`: Essence earned display, streak multiplier breakdown, double-reward ad button, new potion reveal
- Updated `LevelFailScreen`: watch-ad-for-moves button, streak protection panel (ad + gem options, dismiss)
- Updated `HudController`: wallet display (Essence + Gems text), streak flame indicator with multiplier text
- Updated `BrewSceneSetup`: creates meta screen GameObjects (PotionShelf, Workshop, DailyBrew, Store, BoosterShop, Wallet), wires new serialized references

### Previously Added — Feel & Juice (Gate 3 prep)

- **Booster System**: `BoosterType`, `BoosterManager` (pure C#), `BoosterBarUI`, `BoosterSlotUI` with full Shake/Catalyst/ExtraMoves logic per core-mechanic.md §15
- **Audio System**: `AudioManager` singleton (8 SFX + 4 music AudioSources, priority voice-stealing, round-robin variants, pitch variation), `AudioConfigSO`, `SfxId` enum (16 SFX types), `AdaptiveAudioController` (tension stems at ≤5 moves, critical state at 1 move, brew ducking)
- **Haptic System**: `HapticManager` (static, platform-abstracted iOS/Android, 5 feedback types: Light/Medium/Heavy/Success/Selection)
- **Screen Shake**: `ScreenShakeConfigSO` (4 presets), `ScreenShakeController` (Perlin noise, stacking, chain length scaling)
- **Particle System**: `ParticleManager` (pooled, max 5 concurrent, 6 effect types: FusionSparkles/CascadeTrail/BrewBubbles/BrewBurst/LevelCompleteConfetti/ChainIndicator)
- **Token Animation**: `TokenAnimator` (idle shimmer, selected pulse, fusing rush with squash-and-stretch)
- **Brew Animation**: `BrewAnimationController` (1.8s 4-phase sequence: Ignition→Eruption→Formation→Celebration per art-direction.md)
- **Tutorial System**: `TutorialLevelData` (pure C#, 5 hand-tuned levels per tutorial-flow.md), `TutorialController` (step-by-step flow, input gating, hint system), `TutorialOverlay` (dim/spotlight/glow ring/finger indicator)
- **Settings**: `SettingsManager` (PlayerPrefs-backed), `SettingsPanel` (volume sliders, haptics/shake toggles)
- **Scene Setup**: Enhanced `BrewSceneSetup` with `Brew > Create Gameplay Scene` menu, full UI hierarchy creation, `RecipeVialUI.prefab` and `LevelButton.prefab` generation
- **Tests**: `BoosterManagerTests` (15 tests covering charges, shake, catalyst, extra moves)

### Changed

- Updated `BoardPresenter`: integrated boosters, brew animation, particle/haptic/shake triggers, tutorial controller, correct art-direction.md color palette (#E05A3A, #6FB8D9, #6DAF5E, #F2C745, #6B4E9B)
- Updated `HudController`: animated move counter with pulse (red at ≤5, critical at 1), smoothstep score tween
- Updated `TokenView`: exposes SpriteRenderer/TokenAnimator, auto-starts idle shimmer
- Updated `BrewSceneSetup`: generates full Canvas hierarchy, prefabs, GameFlowController wiring, EditorBuildSettings

### Previously Added

- Added canonical session continuity doc at `docs/project-management/session-handoff.md`.
- Added ADR system scaffolding:
  - `docs/adr/README.md`
  - `docs/adr/0001-canonical-unity-version.md`

### Previously Changed

- Updated CI required-doc checks in `.github/workflows/ci.yml` to match actual repository paths.
- Added context continuity CI job (`validate-context`) in `.github/workflows/ci.yml`.
- Aligned canonical engine version to Unity 6 (6000.1 LTS) across:
  - `docs/project-management/project-summary.md`
  - `docs/onboarding/new-team-member-guide.md`
  - `README.md`