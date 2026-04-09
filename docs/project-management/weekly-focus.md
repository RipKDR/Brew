# Brew — Weekly Focus

> Short-horizon operational focus for the current week.
> Update this document at least once per week and whenever priorities shift.
> Week of: 2026-04-09

---

## Primary Outcome

Week 9-10 code systems complete. Weekly event system, push notifications, CurrencyFormatter, and level templates in place. Ready for Unity Editor verification and Gate 5 preparation.

## Must Complete This Week

- Run all EditMode unit tests in Unity Editor (27 test files, ~300+ methods). Fix compilation issues.
- Create ScriptableObject assets for all config SOs (Economy, PotionShelf, Workshop, WinStreak, DailyBrew, WeeklyEvent, Notification).
- Verify weekly event flow end-to-end: start event → play levels → claim milestones → event expiry.
- Verify notification scheduling logic with mock scheduler.
- Begin level 36-40 tuning pass per difficulty framework.

## Secondary Work

- Implement `INotificationScheduler` platform adapters for iOS and Android (requires Unity Mobile Notifications package).
- Implement event theme overlay in level loader (ingredient swapping from Remote Config).
- Begin app store asset preparation (icon, screenshots).

## Risks to Watch

- Firebase SDK not yet imported — backend managers are pure C# abstractions.
- Unity IAP and AdMob SDKs not yet configured — managers are logic-only stubs.
- NotificationManager needs platform-specific implementations before device testing.
- Event theme overlay system needs level loader modifications.

## Owners

- Product/Engineering: Level tuning, event theme design, app store assets
- Engineering: Unity Editor testing, SDK integration, platform notification adapters