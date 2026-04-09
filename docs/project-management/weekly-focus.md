# Brew — Weekly Focus

> Short-horizon operational focus for the current week.
> Update this document at least once per week and whenever priorities shift.
> Week of: 2026-04-09

---

## Primary Outcome

Week 10 code review and bug-fix sprint complete. All P0/P1 issues resolved. Levels 36-40 rebalanced as challenge levels. Soft launch plan and store listing prepared. Ready for Unity Editor verification and Gate 5 checkpoint.

## Must Complete This Week

- [x] Deep code review of all C# source files (28+ files audited)
- [x] Fix P0 compilation errors (AnalyticsManager signature mismatches)
- [x] Fix P1 logic bugs (streak exploit, hardcoded economy values, magic numbers)
- [x] Rebalance levels 36-40 as proper challenge levels (3-4 recipes, tight moves)
- [x] Complete SettingsPanel (privacy policy, credits, restore purchases)
- [x] Economy sanity check against spec documents
- [x] Create soft launch plan document
- [x] Write app store description and keywords
- Run all EditMode unit tests in Unity Editor — fix any remaining failures
- Create ScriptableObject assets and assign to GameFlowController
- Verify weekly event flow end-to-end in Unity Editor

## Secondary Work

- Implement `INotificationScheduler` platform adapters for iOS and Android
- Begin app store asset preparation (icon, screenshots)
- Sync `levels/` root directory with `Resources/Levels/` to eliminate stale copies

## Risks to Watch

- Firebase SDK not yet imported — backend managers are pure C# abstractions
- Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs
- NotificationManager needs platform-specific implementations before device testing
- BoosterBarUI and BoardPresenter still hardcode extra-moves amount (5) instead of reading from config

## Owners

- Product/Engineering: Level tuning complete, event theme design, app store assets
- Engineering: Unity Editor testing, SDK integration, platform notification adapters
