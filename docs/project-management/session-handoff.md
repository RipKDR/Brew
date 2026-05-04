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

**P1 Bug Fixes:**
- Removed hardcoded extra moves (=5) from `BoardPresenter.ActivateExtraMoves` — now requires explicit amount parameter
- `BoosterBarUI` now accepts `extraMovesAmount` via `Initialize()` instead of hardcoded literal 5
- Renamed `NotificationManager.ScheduleStreakAtRisk` parameter from `localHour` to `utcHour` with XML doc clarifying UTC semantics
- Added level duration tracking: `GameFlowController` now records `_levelStartTimeUtc` on level/event start and passes real elapsed seconds to `LogLevelComplete` and `LogLevelFail` (was always 0)

**P2 Data Gaps Fixed:**
- Added `BlockerPlacement` struct and `BlockerPlacements` property to `LevelConfig` — level JSON `blocker_placements` are now parsed and preserved (previously silently dropped)
- `LevelLoader.ConvertToLevelConfig` now maps `blocker_placements` from JSON to `LevelConfig.BlockerPlacements`
- Added `weekly_deal` SKU to `IAPManager.Catalog` ($2.99, consumable, 150 gems + 500 essence + 1 booster) matching `monetization-plan.md` spec
- Added weekly event economy fields to `WeeklyEventConfigSO`: `ThreeStarBonusEssence` (25), `CompletionGems` (30), `AllThreeStarGems` (20) matching `economy-model.md` §3.3-3.4

**Test Coverage Added:**
- Created `AnalyticsManagerTests.cs` — 12 tests covering all log methods, event accumulation, OnEventLogged callback, null parameter handling
- Created `FirebaseAuthManagerTests.cs` — 9 tests covering auth lifecycle, GUID generation, event callbacks, sign-in/sign-out cycles
- Created `TutorialLevelDataTests.cs` — 20 tests covering all 5 tutorial levels, board setup validation, step types, obsolete API
- Added 2 blocker-related tests to `LevelLoaderTests.cs`
- Added 3 tests to `IAPManagerTests.cs` for weekly_deal SKU (catalog count, purchase grant, re-purchase)
- Updated `Catalog_HasSixEntries` test to `Catalog_HasSevenEntries`

**Doc/Code Reconciliation:**
- Updated `monetization-plan.md` fail recovery from +3 to +5 moves (matching `EconomyConfigSO.ExtraMovesFromAd`)
- Updated `economy-model.md` §3.3 weekly event per-level essence from 50 to 75 (matching `WeeklyEventConfigSO.EssencePerLevel`), event max from 1,275 to 1,450
- Updated `economy-model.md` §7.1 non-payer recovery from +3 to +5 moves
- Updated `economy-model.md` §9.2 tuning knobs event essence total from 1,275 to 1,450

**Cleanup:**
- Added `README.md` to `levels/` marking it as authoring mirror of `Resources/Levels/`

### Prior Sessions (1-6)

See previous handoff for complete history of Foundation, Core Loop, Feel & Juice, Meta & Economy, Content & Polish, Integration Wiring, and Code Review Sprint phases.

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

- Blocker data preserved in `LevelConfig` but gameplay deferred — avoids cascading changes to board engine this close to Gate 5
- Extra moves kept at 5 (code authoritative); documentation updated to match
- Weekly event per-level essence aligned to 75 (code), doc updated from 50
- `localHour` renamed to `utcHour` — parameter was always computed from UTC dates

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
- `Assets/_Project/Scripts/Core/LevelConfig.cs` — added `BlockerPlacement` struct and `BlockerPlacements` property
- `Assets/_Project/Scripts/Core/Economy/IAPManager.cs` — added `weekly_deal` SKU
- `Assets/_Project/Scripts/Core/Services/NotificationManager.cs` — renamed `localHour` to `utcHour`

### Modified — Presentation
- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs` — removed default param from `ActivateExtraMoves`
- `Assets/_Project/Scripts/Presentation/BoosterBarUI.cs` — added `_extraMovesAmount` field, config injection via `Initialize()`
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs` — added `_levelStartTimeUtc`, duration tracking

### Modified — Data
- `Assets/_Project/Scripts/Data/LevelLoader.cs` — blocker placement mapping in `ConvertToLevelConfig`
- `Assets/_Project/Scripts/Data/LiveOps/WeeklyEventConfigSO.cs` — added 3 event economy fields

### Created — Tests
- `Assets/Tests/EditMode/Core/Backend/AnalyticsManagerTests.cs` — 12 tests
- `Assets/Tests/EditMode/Core/Backend/FirebaseAuthManagerTests.cs` — 9 tests
- `Assets/Tests/EditMode/Core/TutorialLevelDataTests.cs` — 20 tests

### Modified — Tests
- `Assets/Tests/EditMode/Core/Economy/IAPManagerTests.cs` — 3 new tests, updated catalog count assertion
- `Assets/Tests/EditMode/Data/LevelLoaderTests.cs` — 2 new blocker tests

### Modified — Docs
- `docs/economy/monetization-plan.md` — fail recovery +3 → +5 moves
- `docs/economy/economy-model.md` — §3.3, §7.1, §9.2 updated to match code
- `CHANGELOG.md`
- `docs/project-management/session-handoff.md` (this file)
- `docs/project-management/weekly-focus.md`
- `docs/project-management/context-snapshot.md`

### Created — Docs
- `levels/README.md` — authoring mirror notice

### Created — Level Data
- `Assets/_Project/Resources/Levels/level_001.json` — was missing from Resources (only existed in authoring mirror `levels/`)

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
