# Brew — Weekly Focus

> Short-horizon operational focus for the current week.
> Update this document at least once per week and whenever priorities shift.
> Week of: 2026-05-04

---

## Primary Outcome

Gate 5 code sprint complete. P1 bug fixes (hardcoded extra moves, level duration tracking, blocker data preservation) resolved. 46 new tests added (AnalyticsManager, FirebaseAuthManager, TutorialLevelData, plus blockers and IAP). Doc/code reconciliation done for economy-model.md and monetization-plan.md. Ready for Unity Editor verification, blocker gameplay implementation, and Gate 5 checkpoint.

## Must Complete This Week

- [x] Deep code review of all C# source files (28+ files audited)
- [x] Fix P0 compilation errors (AnalyticsManager signature mismatches)
- [x] Fix P1 logic bugs (streak exploit, hardcoded economy values, magic numbers)
- [x] Rebalance levels 36-40 as proper challenge levels (3-4 recipes, tight moves)
- [x] Complete SettingsPanel (privacy policy, credits, restore purchases)
- [x] Economy sanity check against spec documents
- [x] Create soft launch plan document
- [x] Write app store description and keywords
- [x] Remove hardcoded extra moves from BoardPresenter and BoosterBarUI
- [x] Add blocker data parsing to LevelLoader/LevelConfig
- [x] Add weekly_deal SKU to IAPManager
- [x] Add weekly event economy fields to WeeklyEventConfigSO
- [x] Fix level duration tracking (was always 0)
- [x] Rename NotificationManager localHour → utcHour
- [x] Reconcile economy-model.md and monetization-plan.md with code
- [x] Add test coverage for AnalyticsManager, FirebaseAuthManager, TutorialLevelData
- Run all EditMode unit tests in Unity Editor (31+ files, ~400+ methods) — fix any failures
- Create ScriptableObject assets and assign to GameFlowController
- Implement blocker gameplay in board engine (CellContentType.Blocker, BoardModel, ClusterDetector, CascadeResolver, BoardPresenter)

## Secondary Work

- Begin app store asset preparation (icon, screenshots)
- Implement `INotificationScheduler` platform adapters for iOS and Android
- Full play session: campaign levels 1-5 → event flow → daily brew → workshop → verify streak reset on fail
- Complete Gate 5 checklist items

## Risks to Watch

- Blocker gameplay NOT yet implemented — LevelConfig preserves data but CellContentType has no Blocker variant; levels 36-40 play without blockers
- Firebase SDK not yet imported — backend managers are pure C# abstractions
- Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs
- NotificationManager needs platform-specific implementations before device testing
- BoardPresenter.ActivateExtraMoves no longer has a default parameter — any unknown callers must be updated

## Owners

- Product/Engineering: Blocker gameplay design, event theme design, app store assets
- Engineering: Unity Editor testing, SDK integration, blocker board engine work, platform notification adapters
