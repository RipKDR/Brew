# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09

---

## Current Milestone Focus

- Milestone: **Meta & Economy (Weeks 7-8)** — working toward Milestone Gate 4
- Goal: Full meta-progression (potion shelf, workshop, streaks, daily brew), dual-currency economy, IAP, ads, Firebase backend abstractions, analytics facade
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/04-week-7-8-meta-and-economy.md`

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

## In Progress

- None. All Week 7-8 code is complete. Ready for Unity Editor testing and Gate 4 preparation.

## Next 3 Tasks

1. Open project in Unity Editor. Run all EditMode unit tests (now 24 test files, ~255+ methods). Fix any compilation or test failures.
2. Create EconomyConfigSO, PotionShelfConfigSO, WorkshopConfigSO, WinStreakConfigSO, DailyBrewConfigSO assets in Unity. Assign to GameFlowController.
3. Play full session loop: complete level → earn Essence → open workshop → purchase upgrade → complete daily brew → verify streak multiplier → verify potion shelf updates. Complete Gate 4 checklist items 4.1–4.14.

## Blockers / Risks

- Risk: Firebase SDK not yet imported — FirebaseAuthManager, CloudSaveManager, RemoteConfigManager are abstractions that need SDK wiring.
- Risk: Unity IAP package not yet configured — IAPManager purchase flow needs SDK integration for real store transactions.
- Risk: AdMob SDK not yet integrated — AdManager is pure logic; ad display requires SDK callbacks.
- Risk: Gate 3 runtime verification still pending (external playtest + Unity Editor testing).
- Risk: Gate 4 requires IAP sandbox testing on both iOS and Android — needs Apple Developer + Google Play Console setup.
- Blocker: Cloud save Firestore integration requires Firebase project setup and authentication flow.

## Decisions Recorded This Session

- `CurrencyManager` is pure C# with zero Unity dependencies — enables EditMode testing of all economy logic.
- All economy values flow through `EconomyConfigSO` or `RemoteConfigDefaults` — zero hardcoded values in game logic.
- `WorkshopManager` enforces sequential purchases only — no skipping upgrades.
- `DailyBrewManager` uses UTC dates for consistency — no time zone manipulation possible.
- `CloudSaveManager` is offline-first with dirty/synced flags — local state always available, server sync is best-effort.
- `RemoteConfigManager` merges server values on top of defaults — app always has valid config even without network.
- `AdManager` uses callback-based No-Ads Pass check (`Func<bool>`) — decoupled from IAPManager implementation.
- `IAPManager` product catalog is static readonly — product definitions don't change at runtime.
- `AnalyticsManager` maintains an in-memory event log for debugging — events are inspectable without Firebase DebugView.
- `BrewSceneSetup` now creates all meta screen GameObjects — workshop, potion shelf, daily brew, store, booster shop, wallet.

## Files Touched This Session

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

- All new Core classes (Economy, Meta, Ads, Backend) have zero `using UnityEngine;` — pure C# contract maintained.
- CurrencyManager.Spend returns false on insufficient balance; balance never goes negative per economy-model.md.
- RewardCalculator streak multiplier tiers match economy-model.md exactly: 1.0/1.25/1.5/2.0/2.5/3.0 at streaks 1/2/3/5/8/10+.
- Workshop costs match economy-model.md: 50, 100, 200, 400, 600, 1000, 1500, 2500, 4000, 6000, 8000, 12000 (cumulative 36,350).
- PotionShelfManager milestones match economy-model.md: 25%=500E+25G, 50%=1000E+50G, 75%=2000E+100G, 100%=5000E+250G.
- DailyBrewManager streak bonuses match economy-model.md: 3-day=1.25x+5G, 5-day=1.5x+10G, 7-day=2.0x+15G.
- IAPManager catalog matches monetization-plan.md: 4 gem packs, starter bundle ($1.99, 50G+500E+3 Catalysts, max level 15), no-ads pass ($4.99).
- AdManager daily cap (5), interstitial frequency (3), per-placement limits all match monetization-plan.md.
- Remote Config defaults JSON contains all 56 keys matching economy-model.md and monetization-plan.md values.

## Handoff Checklist

- Decision log updated
- This handoff file updated
- Context snapshot regenerated
- ADR index validated — 2 new ADRs recorded (0002 offline-first cloud save, 0003 Remote Config as config source)
- CI/local readiness checks run and results recorded (requires Unity Editor)
- ScriptableObject assets created and assigned (requires Unity Editor)
