# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09

---

## Current Milestone Focus

- Milestone: **Content & Polish (Weeks 9-10)** — working toward Milestone Gate 5
- Goal: Weekly event system, push notifications, level tuning, CurrencyFormatter utility, settings expansion, soft launch preparation
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/05-week-9-10-content-and-polish.md`

## Current Branch + Baseline

- Branch: `cursor/add-changelog-adr-session-handoff`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, and `lint` should pass

## Completed Since Last Handoff

### Foundation Phase (Sessions 1-2)

- Full pure C# Core layer: `BoardModel`, `ClusterDetector`, `TokenSpawner`, `FusionEngine`, `CascadeResolver`, `BoardStateMachine`, `FusionResult`, `ChainFusionResult`, `GravityStep`, enums, `GridCoord`, `CellContent`
- Presentation: `BoardPresenter`, `InputController`, `TokenView`, `ObjectPool<T>`
- Data: `BoardConfigSO`
- Editor: `BrewSceneSetup`
- Tests: 6 test files, ~60 test methods for Core layer

### Core Loop Phase (Session 3)

- New Core classes: `LevelConfig`, `MoveTracker`, `ScoreCalculator`, `RecipeTracker`, `WinLoseEvaluator`
- Data: `LevelLoader`, `PlayerProgress`, `LocalSaveManager`
- Presentation: `HudController`, `RecipeVialUI`, `LevelCompleteScreen`, `LevelFailScreen`, `LevelSelectScreen`, `LevelButton`, `GameFlowController`
- Tests: 7 additional test files, ~80+ methods
- Level data: 40 level JSONs in `Assets/_Project/Resources/Levels/`

### Gate 2 Closure + Feel & Juice Phase (Session 4)

- Booster System, Audio System, Haptic System, Screen Shake, Particle System, Token Animation, Brew Animation
- Tutorial System, Settings, Scene Setup enhancements
- 15 booster tests, updated BoardPresenter/HudController/TokenView

### Meta & Economy Phase (This Session)

**Economy Foundation:**

- `CurrencyType` enum (Essence, Gems) — pure C#
- `CurrencyManager` — single source of truth for all currency operations, Add/Spend with events, balance-never-negative guard
- `RewardCalculator` — star rating × streak multiplier calculation, values injected via constructor
- `EconomyConfigSO` — ScriptableObject with all economy tuning values from economy-model.md
- `RemoteConfigDefaults` — static class mirroring `config/firebase/remote-config-defaults.json`
- `config/firebase/remote-config-defaults.json` — 56 Remote Config keys with defaults

**Meta Systems (pure C#):**

- `PotionShelfManager` — tracks brewed potions by level ID, 4 milestone thresholds (25/50/75/100) with Essence + Gem rewards
- `WorkshopManager` — 12 sequential upgrades (50–12,000 Essence), spends via CurrencyManager
- `WinStreakTracker` — consecutive wins, 6-tier multiplier lookup (1.0×–3.0×)
- `DailyBrewManager` — UTC-date seeded challenge, 100 Essence + 5 Gems base reward, streak bonuses at 3/5/7 days
- Config SOs: `PotionShelfConfigSO`, `WorkshopConfigSO`, `WinStreakConfigSO`, `DailyBrewConfigSO`

**Monetization (pure C#):**

- `AdPlacement` enum — 6 placement types
- `AdManager` — daily rewarded cap (5), per-placement limits (FreeDailyBooster 1×/day, StreakProtection 2×/day), interstitial every 3rd win, No-Ads Pass check
- `IAPProduct` — product data class with type, price, gem/essence amounts
- `IAPManager` — 6-product catalog (4 Gem Packs, Starter Bundle, No-Ads Pass), purchase fulfillment via CurrencyManager, non-consumable ownership tracking

**Firebase Backend Abstractions (pure C#):**

- `FirebaseAuthManager` — anonymous auth state, account linking placeholder
- `CloudSaveManager` — offline-first PlayerSaveData with dirty/synced flags
- `RemoteConfigManager` — default + server value merge, GetInt/GetFloat/GetBool/GetString, 12h stale check
- `AnalyticsManager` — facade for all 18+ event types from event-tracking-plan.md, event log for debugging

**Presentation Layer:**

- `PotionShelfView` — scrollable grid with locked/unlocked states, milestone progress bar
- `WorkshopView` — upgrade display, purchase button, decoration toggle, completion badge
- `DailyBrewUI` — availability indicator, streak count, button wiring
- `WalletUI` — real-time Essence + Gem display via CurrencyManager events
- `StoreUI` — IAP product listing with purchase buttons
- `BoosterShopUI` — purchase boosters with Essence or Gems via CurrencyManager
- Updated `LevelCompleteScreen` — Essence earned display, streak multiplier breakdown, double-reward ad button, new potion reveal
- Updated `LevelFailScreen` — watch-ad-for-moves button, streak protection panel (ad + gem options)
- Updated `HudController` — wallet display (Essence/Gems), streak flame indicator
- Updated `GameFlowController` — full meta/economy/ad/analytics integration, milestone rewards, interstitial triggers
- Updated `BrewSceneSetup` — creates meta screen GameObjects in scene hierarchy

**Tests (8 new files, ~100+ methods):**

- `CurrencyManagerTests`, `RewardCalculatorTests`
- `PotionShelfManagerTests`, `WorkshopManagerTests`, `WinStreakTrackerTests`, `DailyBrewManagerTests`
- `AdManagerTests`, `IAPManagerTests`
- `CloudSaveManagerTests`, `RemoteConfigManagerTests`

### Content & Polish Phase (This Session)

**Weekly Event System:**

- `WeeklyEventManager` — pure C# event lifecycle (NotStarted/Active/Completed/Expired), 7-level completion tracking, milestone rewards at 3/5/all levels, config-driven reward amounts
- `MilestoneDefinition` — data class for event milestone reward definitions
- `WeeklyEventConfigSO` — ScriptableObject with level count, Essence per level, milestone rewards, move budgets, board dimensions
- `WeeklyEventView` — MonoBehaviour with event banner, level select, results panels, timer countdown, level button states
- `WeeklyEventManagerTests` — 18+ test methods covering lifecycle, completion, milestones, expiry, feature flags
- 7 event level templates in `levels/events/` (E1-E7 difficulty progression: easy→medium→hard→boss)
- Remote Config keys: `event_active`, `event_id`, `event_name`, `event_end_timestamp`, `event_potion_id`, theme colors, ingredient ID, per-level/milestone reward values

**Push Notification System:**

- `NotificationManager` — pure C# with `INotificationScheduler` interface abstraction, 3 notification triggers (Daily Brew, Streak at Risk, Event Ending Soon), daily frequency cap, permission cooldown tracking
- `NotificationConfigSO` — ScriptableObject with max per day, cooldown days, hour offsets, notification titles/bodies
- `NotificationManagerTests` — 15+ test methods covering all trigger logic, caps, permissions
- Updated `SettingsPanel` with notifications toggle
- Updated `SettingsManager` with `NotificationsEnabled` property

**CurrencyFormatter Utility:**

- `CurrencyFormatter` — pure C# static formatter (< 1K exact, 1K-9.9K decimal+K, 10K-999K integer+K, 1M+ M suffix)
- `CurrencyFormatterTests` — 12+ test methods covering all ranges and edge cases
- Updated `WalletUI` to use `CurrencyFormatter.Format()` instead of `ToString("N0")`

**Analytics Expansion:**

- Added 6 new analytics methods to `AnalyticsManager`: `LogEventStart`, `LogEventLevelComplete`, `LogEventComplete`, `LogEventRewardClaimed`, `LogNotificationScheduled`, `LogNotificationOpened`

**Integration:**

- Updated `GameFlowController` with `WeeklyEventView` SerializeField and initialization
- Updated `BrewSceneSetup` to create WeeklyEvent screen in meta screens and wire to GameFlowController

### Integration Wiring Phase (Session 5)

**Event System Wiring:**

- `LevelLoader.LoadEventLevel(json, themedIngredientId)` — theme overlay resolves "themed" placeholder to concrete `IngredientColor` from Remote Config; falls back to Shadow if ID is empty
- Extended `RawLevelData` with `grid_mask`, `blocker_placements`, `tutorial_steps` fields for event JSON compatibility
- `GameFlowController` now constructs `WeeklyEventManager` with config from `WeeklyEventConfigSO` (or spec-matching defaults)
- `GameFlowController.TryStartWeeklyEvent()` reads `event_active`/`event_id`/`event_name`/`event_end_timestamp`/`event_potion_id` from `RemoteConfigManager`
- `GameFlowController.StartEventLevel(int)` loads event templates with theme resolution, wires gameplay
- `HandleEventLevelWin()` grants per-level Essence, claims milestones, fires analytics
- `WeeklyEventView.RefreshState(EventState)` manages banner/level-select/results visibility

**Notification Integration:**

- `NullNotificationScheduler` — no-op for Editor/unsupported platforms (prevents crashes)
- `UnityNotificationScheduler` — compile-guarded iOS/Android implementation using Unity Mobile Notifications
- `GameFlowController` constructs `NotificationManager` with platform-detected scheduler
- Notification triggers: streak-at-risk after win, event-ending-soon after win, daily-brew-reminder on level select
- All triggers gated behind `SettingsManager.NotificationsEnabled`

**RemoteConfigDefaults Sync:**

- Added 48 missing key constants and dictionary entries to `RemoteConfigDefaults.cs` (workshop costs, shelf milestones, daily brew streak bonuses, IAP amounts, event config)
- Total: 68 keys matching `config/firebase/remote-config-defaults.json` exactly

**Test Coverage Hardening:**

- `AdManagerTests` — 11 new tests: events, tutorial path, constructor validation, extra placements, interstitial no-ops
- `NotificationManagerTests` — 5 new tests: EventEndingSoon event, next-day scheduling, null constructor, custom params
- `CurrencyFormatterTests` — 5 new boundary tests at exact tier transitions (999/1000, 9999/10000, etc.)
- `EventLevelLoaderTests` — 11 new tests: theme resolution, fallback, metadata preservation, RemoteConfigDefaults key count sync

## In Progress

- None. Integration wiring complete. Ready for Unity Editor testing and Gate 5 preparation.

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (now 28+ test files, ~350+ methods including new event/notification/formatter tests). Fix any compilation or test failures.
2. Create ScriptableObject assets in Unity: `WeeklyEventConfigSO`, `NotificationConfigSO`, `EconomyConfigSO`, and any missing config SOs. Assign to `GameFlowController`.
3. Play full session including event flow: set `event_active=true` in Remote Config → play event levels → claim milestone rewards → verify notification scheduling. Complete Gate 5 checklist items.

## Blockers / Risks

- Risk: Firebase SDK not yet imported — FirebaseAuthManager, CloudSaveManager, RemoteConfigManager are abstractions that need SDK wiring.
- Risk: Unity IAP package not yet configured — IAPManager purchase flow needs SDK integration for real store transactions.
- Risk: AdMob SDK not yet integrated — AdManager is pure logic; ad display requires SDK callbacks.
- Risk: Gate 3 and Gate 4 runtime verification still pending (external playtest + Unity Editor testing).
- Risk: Gate 5 requires full device matrix testing, performance profiling, and app store asset creation.
- Risk: Unity Mobile Notifications package not yet imported — `UnityNotificationScheduler` compile-guarded but untested on device.
- Blocker: Cloud save Firestore integration requires Firebase project setup and authentication flow.
- Resolved: ~~Event level templates need theme overlay system~~ — `LevelLoader.LoadEventLevel()` now resolves "themed" slots via Remote Config ingredient ID.
- Resolved: ~~INotificationScheduler platform implementations missing~~ — `UnityNotificationScheduler` and `NullNotificationScheduler` created.
- Resolved: ~~WeeklyEventManager not wired in GameFlowController~~ — fully constructed, event level loading path added.
- Resolved: ~~RemoteConfigDefaults out of sync with JSON~~ — all 68 keys now mirrored.

## Decisions Recorded This Session (Session 5)

- Theme overlay uses parameter-based resolution (not static ColorMap) — `LoadEventLevel` accepts `themedIngredientId`, falls back to "shadow" on empty/null.
- `GameFlowController` constructs `RemoteConfigManager` directly with `RemoteConfigDefaults.GetAll()` — no Firebase SDK dependency for local defaults.
- `UnityNotificationScheduler` uses `#if` compile guards — no runtime platform detection, zero overhead on unsupported platforms.
- Event level wins do NOT affect win streak or interstitial counter — event flow is separate from main campaign progression.
- Notification scheduling is gated behind `SettingsManager.NotificationsEnabled` — respects user opt-out preference.

### Prior Session Decisions (Weeks 9-10)

- `WeeklyEventManager` is pure C# with `Func<bool>` feature flag — event system can be entirely disabled via Remote Config.
- Event milestone rewards are injected via `MilestoneDefinition` array, not hardcoded — all reward values come from `WeeklyEventConfigSO`.
- `NotificationManager` uses `INotificationScheduler` interface — platform-specific notification scheduling is fully abstracted for testability.
- Notification frequency cap is 1 per day with 7-day re-prompt cooldown — prevents notification fatigue.
- `CurrencyFormatter` is a centralized static utility — all currency display flows through one format function, no duplication across UI scripts.
- Event level templates use "themed" ingredient placeholder — the theme overlay system will swap this at runtime via Remote Config.
- 13 new Remote Config keys added for event system — all event behavior is server-configurable without app update.

### Prior Session Decisions (Weeks 7-8)

- `CurrencyManager` is pure C# with zero Unity dependencies — enables EditMode testing of all economy logic.
- All economy values flow through `EconomyConfigSO` or `RemoteConfigDefaults` — zero hardcoded values in game logic.
- `WorkshopManager` enforces sequential purchases only — no skipping upgrades.
- `DailyBrewManager` uses UTC dates for consistency — no time zone manipulation possible.
- `CloudSaveManager` is offline-first with dirty/synced flags — local state always available, server sync is best-effort.
- `RemoteConfigManager` merges server values on top of defaults — app always has valid config even without network.
- `AdManager` uses callback-based No-Ads Pass check (`Func<bool>`) — decoupled from IAPManager implementation.
- `IAPManager` product catalog is static readonly — product definitions don't change at runtime.
- `AnalyticsManager` maintains an in-memory event log for debugging — events are inspectable without Firebase DebugView.
- `BrewSceneSetup` now creates all meta screen GameObjects — workshop, potion shelf, daily brew, store, booster shop, wallet, weekly event.

## Files Touched This Session (Session 5 — Integration Wiring)

### Created — Core/Services

- `Assets/_Project/Scripts/Core/Services/NullNotificationScheduler.cs`
- `Assets/_Project/Scripts/Core/Services/UnityNotificationScheduler.cs`

### Created — Tests

- `Assets/Tests/EditMode/Data/EventLevelLoaderTests.cs`

### Modified — Data

- `Assets/_Project/Scripts/Data/LevelLoader.cs` — added `LoadEventLevel()` theme overlay, extended `RawLevelData`
- `Assets/_Project/Scripts/Data/Economy/RemoteConfigDefaults.cs` — added 48 missing key constants + dictionary entries (68 total)

### Modified — Presentation

- `Assets/_Project/Scripts/Presentation/GameFlowController.cs` — constructed WeeklyEventManager/RemoteConfigManager/NotificationManager, added event level flow, notification scheduling
- `Assets/_Project/Scripts/Presentation/LiveOps/WeeklyEventView.cs` — added `RefreshState(EventState)` method

### Modified — Tests

- `Assets/Tests/EditMode/Core/Ads/AdManagerTests.cs` — 11 new tests
- `Assets/Tests/EditMode/Core/Services/NotificationManagerTests.cs` — 5 new tests
- `Assets/Tests/EditMode/Utilities/CurrencyFormatterTests.cs` — 5 new boundary tests

### Modified — Docs

- `AGENTS.md` — updated phase, fixed path references
- `docs/project-management/project-summary.md` — updated milestone table
- `docs/project-management/session-handoff.md` — this file

---

## Files Touched Prior Session (Week 9-10)

### Created — Core/LiveOps (pure C#)

- `Assets/_Project/Scripts/Core/LiveOps/WeeklyEventManager.cs`

### Created — Core/Services (pure C#)

- `Assets/_Project/Scripts/Core/Services/NotificationManager.cs`

### Created — Utilities (pure C#)

- `Assets/_Project/Scripts/Utilities/CurrencyFormatter.cs`

### Created — Data

- `Assets/_Project/Scripts/Data/LiveOps/WeeklyEventConfigSO.cs`
- `Assets/_Project/Scripts/Data/Services/NotificationConfigSO.cs`

### Created — Presentation

- `Assets/_Project/Scripts/Presentation/LiveOps/WeeklyEventView.cs`

### Created — Tests

- `Assets/Tests/EditMode/Core/LiveOps/WeeklyEventManagerTests.cs`
- `Assets/Tests/EditMode/Core/Services/NotificationManagerTests.cs`
- `Assets/Tests/EditMode/Utilities/CurrencyFormatterTests.cs`

### Created — Level Data

- `levels/events/event_template_01.json` through `event_template_07.json` (7 event level templates)

### Modified

- `Assets/_Project/Scripts/Core/Backend/AnalyticsManager.cs` — added 6 analytics methods (event + notification)
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs` — WeeklyEventView field + initialization
- `Assets/_Project/Scripts/Editor/BrewSceneSetup.cs` — WeeklyEvent screen creation + wiring
- `Assets/_Project/Scripts/Presentation/SettingsPanel.cs` — notifications toggle
- `Assets/_Project/Scripts/Data/SettingsManager.cs` — NotificationsEnabled property
- `Assets/_Project/Scripts/Presentation/Economy/WalletUI.cs` — CurrencyFormatter integration
- `config/firebase/remote-config-defaults.json` — 13 new event Remote Config keys

---

## Files Touched Previous Sessions (Weeks 1-8)

### Created — Core/Economy (pure C#)

- `Assets/_Project/Scripts/Core/Economy/CurrencyType.cs`
- `Assets/_Project/Scripts/Core/Economy/CurrencyManager.cs`
- `Assets/_Project/Scripts/Core/Economy/RewardCalculator.cs`
- `Assets/_Project/Scripts/Core/Economy/IAPProduct.cs`
- `Assets/_Project/Scripts/Core/Economy/IAPManager.cs`

### Created — Core/Meta (pure C#)

- `Assets/_Project/Scripts/Core/Meta/PotionShelfManager.cs`
- `Assets/_Project/Scripts/Core/Meta/WorkshopManager.cs`
- `Assets/_Project/Scripts/Core/Meta/WinStreakTracker.cs`
- `Assets/_Project/Scripts/Core/Meta/DailyBrewManager.cs`

### Created — Core/Ads (pure C#)

- `Assets/_Project/Scripts/Core/Ads/AdPlacement.cs`
- `Assets/_Project/Scripts/Core/Ads/AdManager.cs`

### Created — Core/Backend (pure C#)

- `Assets/_Project/Scripts/Core/Backend/FirebaseAuthManager.cs`
- `Assets/_Project/Scripts/Core/Backend/CloudSaveManager.cs`
- `Assets/_Project/Scripts/Core/Backend/RemoteConfigManager.cs`
- `Assets/_Project/Scripts/Core/Backend/AnalyticsManager.cs`

### Created — Data

- `Assets/_Project/Scripts/Data/Economy/EconomyConfigSO.cs`
- `Assets/_Project/Scripts/Data/Economy/RemoteConfigDefaults.cs`
- `Assets/_Project/Scripts/Data/Meta/PotionShelfConfigSO.cs`
- `Assets/_Project/Scripts/Data/Meta/WorkshopConfigSO.cs`
- `Assets/_Project/Scripts/Data/Meta/WinStreakConfigSO.cs`
- `Assets/_Project/Scripts/Data/Meta/DailyBrewConfigSO.cs`

### Created — Presentation

- `Assets/_Project/Scripts/Presentation/Meta/PotionShelfView.cs`
- `Assets/_Project/Scripts/Presentation/Meta/WorkshopView.cs`
- `Assets/_Project/Scripts/Presentation/Meta/DailyBrewUI.cs`
- `Assets/_Project/Scripts/Presentation/Economy/WalletUI.cs`
- `Assets/_Project/Scripts/Presentation/Economy/StoreUI.cs`
- `Assets/_Project/Scripts/Presentation/Economy/BoosterShopUI.cs`

### Created — Tests

- `Assets/Tests/EditMode/Core/Economy/CurrencyManagerTests.cs`
- `Assets/Tests/EditMode/Core/Economy/RewardCalculatorTests.cs`
- `Assets/Tests/EditMode/Core/Economy/IAPManagerTests.cs`
- `Assets/Tests/EditMode/Core/Meta/PotionShelfManagerTests.cs`
- `Assets/Tests/EditMode/Core/Meta/WorkshopManagerTests.cs`
- `Assets/Tests/EditMode/Core/Meta/WinStreakTrackerTests.cs`
- `Assets/Tests/EditMode/Core/Meta/DailyBrewManagerTests.cs`
- `Assets/Tests/EditMode/Core/Ads/AdManagerTests.cs`
- `Assets/Tests/EditMode/Core/Backend/CloudSaveManagerTests.cs`
- `Assets/Tests/EditMode/Core/Backend/RemoteConfigManagerTests.cs`

### Created — Config

- `config/firebase/remote-config-defaults.json`

### Modified — Presentation

- `Assets/_Project/Scripts/Presentation/GameFlowController.cs` — full meta/economy/ad/analytics wiring
- `Assets/_Project/Scripts/Presentation/HudController.cs` — wallet display, streak indicator
- `Assets/_Project/Scripts/Presentation/LevelCompleteScreen.cs` — economy breakdown, double-reward ad, potion reveal
- `Assets/_Project/Scripts/Presentation/LevelFailScreen.cs` — ad-for-moves, streak protection panel

### Modified — Editor

- `Assets/_Project/Scripts/Editor/BrewSceneSetup.cs` — CreateMetaScreens, updated CreateGameFlowController with meta references

## Verification Notes

### Week 9-10 Verification

- All new Core classes (LiveOps, Services) have zero `using UnityEngine;` — pure C# contract maintained.
- `CurrencyFormatter` has zero Unity dependencies — pure static utility.
- `WeeklyEventManager` reward amounts match event-templates.md: 75 Essence/level, 100E at 3 levels, 10G at 5 levels, 250E+25G at all 7 (total 775E + 35G).
- Event level templates follow event-templates.md spec: E1-E2 easy (20/22 moves, 2 recipes), E3-E5 medium (25/28 moves, 2-3 recipes), E6 hard (28 moves, 3 recipes + blockers), E7 boss (32 moves, 3 recipes + mixed blockers).
- Remote Config defaults JSON now contains 69 keys (56 original + 13 event keys).
- `NotificationManager` daily cap (1) and re-prompt cooldown (7 days) match spec.
- `SettingsManager.NotificationsEnabled` defaults to true (opt-out model).
- `WalletUI` now uses `CurrencyFormatter.Format()` for compact display.

### Week 7-8 Verification (Prior)

- All prior Core classes (Economy, Meta, Ads, Backend) have zero `using UnityEngine;` — pure C# contract maintained.
- CurrencyManager.Spend returns false on insufficient balance; balance never goes negative per economy-model.md.
- RewardCalculator streak multiplier tiers match economy-model.md exactly: 1.0/1.25/1.5/2.0/2.5/3.0 at streaks 1/2/3/5/8/10+.
- Workshop costs match economy-model.md: 50, 100, 200, 400, 600, 1000, 1500, 2500, 4000, 6000, 8000, 12000 (cumulative 36,350).
- PotionShelfManager milestones match economy-model.md: 25%=500E+25G, 50%=1000E+50G, 75%=2000E+100G, 100%=5000E+250G.
- DailyBrewManager streak bonuses match economy-model.md: 3-day=1.25x+5G, 5-day=1.5x+10G, 7-day=2.0x+15G.
- IAPManager catalog matches monetization-plan.md: 4 gem packs, starter bundle ($1.99, 50G+500E+3 Catalysts, max level 15), no-ads pass ($4.99).
- AdManager daily cap (5), interstitial frequency (3), per-placement limits all match monetization-plan.md.
- Remote Config defaults JSON contains all keys matching economy-model.md and monetization-plan.md values.

## Handoff Checklist

- [x] Decision log updated
- [x] This handoff file updated
- [x] Context snapshot regenerated
- [x] Changelog updated
- [x] Weekly focus updated
- [ ] ADR index validated — no new ADRs this session (event system uses existing patterns)
- [ ] CI/local readiness checks run and results recorded (requires Unity Editor)
- [ ] ScriptableObject assets created and assigned (requires Unity Editor): WeeklyEventConfigSO, NotificationConfigSO
