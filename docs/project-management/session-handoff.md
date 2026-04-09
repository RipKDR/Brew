# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09 (Session 7)

---

## Current Milestone Focus

- Milestone: **Gate 5 — Soft Launch Readiness** — all code-side integration work complete. Awaiting Unity Editor + device testing.
- Goal: Import Firebase/IAP/Ad SDKs in Unity, run EditMode tests, create ScriptableObjects, pass Gate 5 criteria
- Source plan: `docs/production/mvp-build-plan.md`, meta plan at `.cursor/plans/brew_soft_launch_completion_4e5e47b0.plan.md`

## Current Branch + Baseline

- Branch: `master`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, `lint`, and `unity-tests` should pass

## Completed Since Last Handoff

### SDK Integration & Build Pipeline (This Session — Session 7)

**Firebase SDK Bridge Layer (5 new files):**
- `FirebaseAuthBridge.cs` — bridges `FirebaseAuthManager` to real Firebase Auth SDK (`#if FIREBASE_AUTH`)
- `FirebaseAnalyticsBridge.cs` — bridges `AnalyticsManager` to Firebase Analytics (`#if FIREBASE_ANALYTICS`)
- `FirebaseRemoteConfigBridge.cs` — bridges `RemoteConfigManager` to Firebase Remote Config (`#if FIREBASE_REMOTE_CONFIG`)
- `FirebaseCrashlyticsBridge.cs` — static bridge to Firebase Crashlytics (`#if FIREBASE_CRASHLYTICS`)
- `FirebaseCloudSaveBridge.cs` — bridges `CloudSaveManager` to Firestore with offline-first sync and conflict resolution (`#if FIREBASE_FIRESTORE`)
- Added `ApplyFirebaseAuthState` internal method to `FirebaseAuthManager` for SDK bridge

**Monetization SDK Bridge Layer (2 new files):**
- `UnityIAPBridge.cs` — implements `IDetailedStoreListener`, wires `IAPManager.Catalog` to Unity IAP (`#if UNITY_IAP`)
- `AdMobBridge.cs` — wraps AdMob rewarded + interstitial ad loading and display (`#if ADMOB`)

**Code Fixes:**
- `BoosterBarUI.cs` — removed hardcoded `5` for extra moves; now reads from `_extraMovesAmount` parameter set via `Initialize()`
- `Packages/manifest.json` — added `com.unity.mobile.notifications` 2.3.2 (was missing, referenced by `UnityNotificationScheduler`)
- Deleted stale `levels/` root directory and `Assets/_Project/Resources_moved/` (duplicates of canonical `Resources/Levels/`)

**CI/CD Pipeline:**
- `ci.yml` — added `unity-tests` job using `game-ci/unity-test-runner@v4` for EditMode tests; updated `validate-levels` to use canonical `Assets/_Project/Resources/Levels/`
- `build.yml` — replaced placeholder with real `game-ci/unity-builder@v4` builds (Android AAB + iOS); added `deploy-android` (Google Play internal), `deploy-ios` (TestFlight via Fastlane), and webhook notification

**Doc Fixes:**
- `CONTRIBUTING.md` — fixed Unity version from "Unity 2022 LTS" to "Unity 6 (6000.1 LTS)"
- `docs/economy/economy-model.md` — updated §3.3 (weekly event rewards) and §3.4 (Gem earning) to match `WeeklyEventConfigSO` implementation
- `docs/economy/monetization-plan.md` — marked `weekly_deal` SKU as post-MVP (not in current `IAPManager.Catalog`)

### Code Review Sprint (Prior Session — Session 6)

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

- None. All code-side integration work complete. Ready for Unity Editor and device testing.

## Next 3 Tasks

1. **Import Firebase Unity SDK in Unity Editor.** Add EDM4U + Firebase packages. Add scripting defines: `FIREBASE_AUTH`, `FIREBASE_ANALYTICS`, `FIREBASE_REMOTE_CONFIG`, `FIREBASE_CRASHLYTICS`, `FIREBASE_FIRESTORE`. Run EditMode tests. Fix any compilation errors.
2. **Create ScriptableObject assets in Unity.** `EconomyConfigSO`, `PotionShelfConfigSO`, `WorkshopConfigSO`, `DailyBrewConfigSO`, `WinStreakConfigSO`, `WeeklyEventConfigSO`, `NotificationConfigSO`. Assign all to `GameFlowController` inspector fields. Also add `UNITY_IAP` and `ADMOB` defines after importing those SDKs.
3. **Full playthrough on device.** Campaign levels 1-5, event flow, daily brew, workshop upgrade, streak reset on fail. Verify Firebase Analytics events appear in DebugView. Verify Crashlytics dashboard is active.

## Blockers / Risks

- Blocker: Firebase SDK must be imported in Unity Editor before bridge classes compile with real implementations
- Blocker: Unity IAP and AdMob SDKs must be imported before `UnityIAPBridge` / `AdMobBridge` compile with real implementations
- Blocker: Gate 5 requires device matrix testing (8+ devices), app store asset creation (icon, screenshots, video)
- Risk: `BoardPresenter.ActivateExtraMoves()` still has default parameter `amount = 5` — callers should pass `_economyConfig.ExtraMovesFromAd`
- Risk: Firebase EDM4U may conflict with existing packages — test in a clean branch first

## Decisions Recorded This Session (Session 7)

- Firebase SDK integration uses a bridge pattern: pure C# managers in Core remain unchanged; bridge classes in `Core/Backend/` use `#if` preprocessor directives so they compile both with and without SDKs imported. Define symbols (`FIREBASE_AUTH`, etc.) enable the real SDK paths.
- Unity IAP and AdMob use the same bridge pattern (`#if UNITY_IAP`, `#if ADMOB`).
- Stale `levels/` root directory deleted; CI updated to use canonical `Assets/_Project/Resources/Levels/`. Single source of truth for level data.
- `BoosterBarUI.Initialize()` accepts an `extraMovesAmount` parameter instead of hardcoding `5`. Default value preserves backward compatibility until SO is wired.
- Build pipeline uses `game-ci/unity-builder@v4` for both Android (AAB) and iOS builds, with Fastlane for TestFlight upload and `upload-google-play` for Play Console internal track.
- `weekly_deal` SKU deferred to post-soft-launch per monetization plan update.

## Decisions Recorded Session 6

- BoosterManager defaults to 0 charges — infinite charges were a development convenience that leaked into production code. Boosters must be explicitly granted.
- `ActivateExtraMoves` requires explicit `amount` parameter — no default, callers must read from config. Prevents silent drift between config and hardcoded values.
- Streak resets on fail unless player explicitly pays for protection (ad or gems) — the "Retry" path now resets streak if protection was not purchased, closing the exploit.
- `LevelLoader.GetMaxLevelId()` is cached — avoids loading all level TextAssets on every `AdvanceToNextLevel` call.
- Level 40 uses 9×10 grid (wider than 36-39) — gives the finale a distinctive, generous feel while staying within spec limits.
- Config SO fallbacks remain as defensive code — removed after SO assets are created in Unity Editor.

## Files Touched This Session (Session 7)

### Created — Firebase Bridges
- `Assets/_Project/Scripts/Core/Backend/FirebaseAuthBridge.cs`
- `Assets/_Project/Scripts/Core/Backend/FirebaseAnalyticsBridge.cs`
- `Assets/_Project/Scripts/Core/Backend/FirebaseRemoteConfigBridge.cs`
- `Assets/_Project/Scripts/Core/Backend/FirebaseCrashlyticsBridge.cs`
- `Assets/_Project/Scripts/Core/Backend/FirebaseCloudSaveBridge.cs`

### Created — Monetization Bridges
- `Assets/_Project/Scripts/Core/Economy/UnityIAPBridge.cs`
- `Assets/_Project/Scripts/Core/Ads/AdMobBridge.cs`

### Modified — Core
- `Assets/_Project/Scripts/Core/Backend/FirebaseAuthManager.cs` — added `ApplyFirebaseAuthState` internal method
- `Assets/_Project/Scripts/Presentation/BoosterBarUI.cs` — removed hardcoded `5`, added `_extraMovesAmount` field and `extraMovesAmount` parameter to `Initialize()`

### Modified — Config
- `Packages/manifest.json` — added `com.unity.mobile.notifications` 2.3.2

### Modified — CI/CD
- `.github/workflows/ci.yml` — added `unity-tests` job, updated levels dir path
- `.github/workflows/build.yml` — replaced placeholders with game-ci/unity-builder builds + Fastlane deploy

### Modified — Docs
- `CONTRIBUTING.md` — fixed Unity version
- `docs/economy/economy-model.md` — updated §3.3, §3.4
- `docs/economy/monetization-plan.md` — marked `weekly_deal` as post-MVP
- `docs/project-management/session-handoff.md` (this file)

### Deleted
- `levels/` root directory (40 JSON files — stale duplicates)
- `Assets/_Project/Resources_moved/` (stale migration artifact)

## Files Touched Session 6

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

- [x] Decision log updated (Session 7)
- [x] This handoff file updated (Session 7)
- [ ] Changelog updated
- [ ] Weekly focus updated
- [ ] Context snapshot regenerated
- [ ] ADR index validated — no new ADRs this session
- [ ] CI/local readiness checks run and results recorded (requires Unity Editor)
- [ ] ScriptableObject assets created and assigned (requires Unity Editor)
- [ ] Firebase SDK imported and scripting defines set (requires Unity Editor)
- [ ] Unity IAP + AdMob SDKs imported (requires Unity Editor)
