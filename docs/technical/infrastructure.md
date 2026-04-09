# Brew — Infrastructure & DevOps Plan

> **Version:** 0.1.0-draft · **Last updated:** 2026-04-09
> **Status:** Pre-production · MVP scope

---

## 1. Firebase Project Setup

### 1.1 Environments

| Environment | Firebase Project | Purpose | Access |
|---|---|---|---|
| **dev** | `brew-dev` | Local development, rapid iteration, throwaway data | All engineers |
| **staging** | `brew-staging` | QA, playtesting, TestFlight/Internal Testing builds | Engineers + QA + PM |
| **production** | `brew-prod` | Live players | Deploy via CI only; console access restricted to leads |

Each environment is a separate Firebase project with its own:
- Firestore database
- Auth user pool
- Remote Config values
- Analytics property
- Cloud Functions deployment
- Crashlytics instance
- Cloud Storage bucket

### 1.2 Firebase Services Enabled

| Service | Dev | Staging | Prod |
|---|---|---|---|
| Authentication (Anonymous + Google + Apple) | ✅ | ✅ | ✅ |
| Cloud Firestore | ✅ | ✅ | ✅ |
| Cloud Functions (2nd gen) | ✅ | ✅ | ✅ |
| Remote Config | ✅ | ✅ | ✅ |
| Firebase Analytics (GA4) | ❌ | ✅ | ✅ |
| Crashlytics | ❌ | ✅ | ✅ |
| Cloud Messaging (FCM) | ❌ | ✅ | ✅ |
| Cloud Storage | ✅ | ✅ | ✅ |
| App Check (DeviceCheck / Play Integrity) | ❌ | ❌ | ✅ |

### 1.3 Firebase Config in Unity

The Unity project includes three `google-services.json` (Android) and `GoogleService-Info.plist` (iOS) files, one per environment. The active config is selected by a build preprocessor based on a `BUILD_ENV` scripting define symbol:

```
Assets/
  StreamingAssets/
    Firebase/
      dev/
        google-services.json
        GoogleService-Info.plist
      staging/
        google-services.json
        GoogleService-Info.plist
      prod/
        google-services.json
        GoogleService-Info.plist
```

A custom `FirebaseConfigSwitcher` editor script copies the correct files to the expected locations before build.

---

## 2. CI/CD Pipeline

### 2.1 Toolchain

| Tool | Role |
|---|---|
| **GitHub** | Source control, PR reviews, issue tracking |
| **GitHub Actions** | CI orchestration |
| **GameCI** | Unity Docker images for headless builds (`game-ci/unity-builder`) |
| **Fastlane** | iOS code signing (match), TestFlight upload, Google Play Internal Testing upload |
| **Firebase CLI** | Deploy Cloud Functions, Firestore rules, Remote Config |

### 2.2 Pipeline Stages

```
Code Push (PR or merge to main)
    │
    ├─► [1] Lint & Static Analysis
    │       - C# code style check (dotnet format)
    │       - Unity YAML merge conflict detector
    │
    ├─► [2] Unit Tests
    │       - NUnit EditMode tests (Core/ logic)
    │       - Run via Unity Test Framework in headless mode
    │       - Fail build on any test failure
    │
    ├─► [3] Build
    │       - iOS build (Xcode project export via Unity)
    │       - Android build (AAB via Unity)
    │       - Environment: staging (for PR/main), prod (for release tags)
    │       - Build number auto-incremented from CI run number
    │
    ├─► [4] Integration Tests (optional, staging only)
    │       - PlayMode tests on simulator
    │       - Firebase Test Lab (Android) — smoke test on 3 devices
    │
    ├─► [5] Deploy
    │       - iOS: Fastlane → TestFlight (staging) or App Store Connect (prod)
    │       - Android: Fastlane → Google Play Internal Testing (staging) or Production (prod)
    │       - Cloud Functions: firebase deploy --only functions (on merge to main)
    │       - Firestore Rules: firebase deploy --only firestore:rules (on merge to main)
    │
    └─► [6] Notify
            - Slack/Discord notification with build status, download link, changelog
```

### 2.3 GitHub Actions Workflow (Summary)

```yaml
# .github/workflows/build.yml
name: Build & Deploy

on:
  push:
    branches: [main]
    tags: ['v*']
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    container:
      image: unityci/editor:2024.3.x-base  # Unity 6 LTS
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true
      - uses: game-ci/unity-test-runner@v4
        with:
          testMode: editmode
          projectPath: .

  build-android:
    needs: test
    runs-on: ubuntu-latest
    container:
      image: unityci/editor:2024.3.x-android
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true
      - uses: game-ci/unity-builder@v4
        with:
          targetPlatform: Android
          buildMethod: BuildScript.BuildAndroid
      - uses: r0adkll/upload-google-play@v1
        if: github.ref == 'refs/heads/main'
        with:
          track: internal

  build-ios:
    needs: test
    runs-on: macos-latest  # Required for Xcode
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true
      - uses: game-ci/unity-builder@v4
        with:
          targetPlatform: iOS
          buildMethod: BuildScript.BuildIOS
      - name: Fastlane TestFlight
        if: github.ref == 'refs/heads/main'
        run: |
          cd build/iOS
          fastlane ios beta
        env:
          MATCH_PASSWORD: ${{ secrets.MATCH_PASSWORD }}
          FASTLANE_APPLE_APPLICATION_SPECIFIC_PASSWORD: ${{ secrets.FASTLANE_APP_PASSWORD }}

  deploy-firebase:
    needs: test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: FirebaseExtended/action-hosting-deploy@v0
      - run: |
          npm ci --prefix functions
          npx firebase deploy --only functions,firestore:rules --project brew-staging
        env:
          FIREBASE_TOKEN: ${{ secrets.FIREBASE_TOKEN }}
```

### 2.4 Branch Strategy

| Branch | Purpose | Deploys To |
|---|---|---|
| `main` | Integration branch; always buildable | Staging (auto) |
| `feature/*` | Feature development | PR builds only (no deploy) |
| `release/v*` | Release candidate | Staging for QA sign-off |
| `v*.*.*` (tag) | Production release | Production |
| `hotfix/*` | Emergency fixes | Cherry-picked to main + release |

---

## 3. App Store Submission Checklist

### iOS (App Store Connect)

- [ ] App icon: 1024×1024 (no alpha, no rounded corners)
- [ ] Screenshots: iPhone 6.7" (1290×2796), iPhone 6.5" (1242×2688), iPad 12.9" (2048×2732)
- [ ] App Preview video (optional, recommended): 15–30 seconds
- [ ] Privacy Nutrition Labels: configured for Analytics, Advertising (AdMob), Purchases
- [ ] App Privacy Policy URL: hosted and linked
- [ ] Age Rating: complete questionnaire (likely 4+, no objectionable content)
- [ ] In-App Purchases: all IAP products created and submitted for review
- [ ] Sign in with Apple: implemented (required since we offer Google sign-in)
- [ ] ATT (App Tracking Transparency): prompt implemented before AdMob init
- [ ] SKAdNetwork: all ad network IDs added to Info.plist
- [ ] Minimum deployment target: iOS 16.0
- [ ] Bitcode: disabled (Unity 6 does not support)
- [ ] `NSUserTrackingUsageDescription` set in Info.plist
- [ ] Export compliance: HTTPS-only exemption (ITSAppUsesNonExemptEncryption = NO)
- [ ] TestFlight: internal + external beta tested; at least 1 external beta round before submission
- [ ] Review notes: include test account credentials if needed, description of IAP flow

### Android (Google Play Console)

- [ ] App icon: 512×512 PNG
- [ ] Feature graphic: 1024×500
- [ ] Screenshots: phone (min 2), tablet 7" and 10" (if targeting tablets)
- [ ] Content rating: IARC questionnaire completed
- [ ] Data safety form: completed (analytics, ads, authentication data disclosed)
- [ ] Target API level: API 35 (Android 15) — required by Google Play in 2026
- [ ] App Bundle (AAB): signed with Play App Signing
- [ ] In-app products: all products created in Play Console
- [ ] Ads declaration: "Yes, my app contains ads"
- [ ] Closed testing: at least 20 testers with 14 days of testing before production
- [ ] Store listing: short description (80 chars), full description (4000 chars), category (Puzzle)
- [ ] Privacy policy URL: linked in store listing and in-app settings

---

## 4. Remote Config Workflow

### 4.1 Making Economy Changes

1. **Author:** PM drafts parameter changes in a shared spreadsheet (parameter name, old value, new value, rationale).
2. **Review:** Lead engineer reviews for sanity (no negative values, no divide-by-zero, no breaking schema changes).
3. **Apply to staging:** PM or engineer updates values in Firebase Console (`brew-staging` project) → Remote Config → Publish.
4. **Validate:** QA launches staging build, force-fetches config (dev menu button), verifies new values in-game.
5. **Promote to production:** After QA sign-off, same values applied to `brew-prod` Remote Config → Publish.
6. **Monitor:** Check Analytics dashboards for 24 hours post-change. Look for anomalies in economy flow, ad engagement, retention.

### 4.2 Pushing a New Event

1. **Design event:** theme, 9 levels, reward tiers, schedule, art assets.
2. **Build levels:** Level designer creates levels in Unity editor tool, exports as JSON.
3. **Upload assets:** Event art uploaded to Cloud Storage (`gs://brew-prod/events/{event_id}/`).
4. **Upload levels:** Level JSON uploaded to Cloud Storage.
5. **Configure Remote Config:** Add event JSON to `active_events` parameter in Remote Config. Set start/end dates.
6. **Test on staging:** Manually set device clock or use a staging override to simulate event dates.
7. **Publish to production:** Publish Remote Config. Client picks up on next foreground (within 1 hour due to cache TTL; can set shorter TTL for event launches).
8. **Monitor:** Event participation rate, completion rate, reward claim rate via Analytics events.

### 4.3 Emergency Kill Switches

| Parameter | Effect |
|---|---|
| `maintenance_mode = true` | Shows maintenance dialog, blocks all gameplay. Use for critical server issues. |
| `feature_events_enabled = false` | Hides event UI. Use if an event has a critical bug. |
| `force_update_min_version = "1.1.0"` | Forces users on older versions to update. Use after a critical client-side fix. |
| `ads_enabled = false` | Disables all ad placements. Use if ad SDK causes crashes. |

---

## 5. Analytics Event Catalog

### 5.1 Automatically Tracked (Firebase SDK)

| Event | Description |
|---|---|
| `first_open` | First app launch after install |
| `session_start` | App foregrounded (new session) |
| `screen_view` | Screen transitions (auto-tracked if using Firebase screen reporting) |
| `app_update` | First launch after update |
| `os_update` | First launch after OS update |
| `in_app_purchase` | IAP completed (auto-tracked by Firebase) |
| `ad_impression` | Ad displayed (auto-tracked by AdMob) |

### 5.2 Custom Events — Core Gameplay

| Event | Parameters | Trigger |
|---|---|---|
| `level_start` | `level_id`, `level_type`, `moves_available`, `boosters_selected[]`, `attempt_number` | Player starts a level |
| `level_complete` | `level_id`, `level_type`, `stars`, `score`, `moves_used`, `moves_remaining`, `duration_seconds`, `potions_brewed_count`, `boosters_used[]`, `ads_watched_count`, `attempt_number`, `is_first_completion` | Player completes a level |
| `level_fail` | `level_id`, `level_type`, `score`, `moves_used`, `duration_seconds`, `potions_remaining`, `boosters_used[]`, `ads_watched_count`, `attempt_number` | Player fails (runs out of moves) |
| `level_quit` | `level_id`, `moves_used`, `duration_seconds` | Player quits mid-level |
| `level_retry` | `level_id`, `attempt_number` | Player retries after fail |
| `fusion_performed` | `level_id`, `color`, `cluster_size`, `resulting_tier`, `cascade_depth` | Player taps a valid cluster |
| `potion_brewed` | `level_id`, `potion_id`, `is_first_ever` | A potion is brewed during gameplay |

### 5.3 Custom Events — Meta

| Event | Parameters | Trigger |
|---|---|---|
| `workshop_upgrade_purchased` | `upgrade_id`, `cost_essence`, `upgrade_index` | Player buys workshop upgrade |
| `workshop_upgrade_viewed` | `upgrade_id`, `cost_essence`, `can_afford` | Player views next upgrade (intent signal) |
| `potion_shelf_opened` | `total_potions_collected`, `total_potions_available` | Player opens collection screen |
| `daily_brew_started` | `streak_count` | Player starts daily brew level |
| `daily_brew_completed` | `streak_count`, `stars`, `reward_essence`, `reward_gems` | Player completes daily brew |
| `streak_milestone` | `streak_count`, `reward_type`, `reward_amount` | Player hits streak milestone (3, 7, 14, etc.) |
| `streak_broken` | `previous_streak`, `last_active_date` | Streak resets due to inactivity |
| `event_entered` | `event_id`, `levels_available` | Player opens event screen |
| `event_level_completed` | `event_id`, `event_level_index`, `stars` | Player completes an event level |
| `event_reward_claimed` | `event_id`, `reward_index`, `reward_type`, `reward_amount` | Player claims event reward |

### 5.4 Custom Events — Economy

| Event | Parameters | Trigger |
|---|---|---|
| `currency_earned` | `currency` ("essence" / "gems"), `amount`, `source` ("level_complete" / "daily_brew" / "streak" / "event" / "iap" / "ad_reward"), `balance_after` | Any currency increase |
| `currency_spent` | `currency`, `amount`, `item` ("booster_x" / "workshop_upgrade_x"), `balance_after` | Any currency decrease |
| `booster_used` | `booster_id`, `context` ("pre_game" / "in_game"), `level_id` | Booster consumed |
| `booster_purchased` | `booster_id`, `cost_type` ("essence" / "gems"), `cost_amount` | Booster bought with currency |

### 5.5 Custom Events — Monetization

| Event | Parameters | Trigger |
|---|---|---|
| `ad_rewarded_offered` | `placement` ("extra_moves" / "post_fail_retry" / "double_reward"), `level_id` | Rewarded ad option shown |
| `ad_rewarded_started` | `placement`, `level_id`, `ad_network` | Player starts watching rewarded ad |
| `ad_rewarded_completed` | `placement`, `level_id`, `ad_network`, `reward_type`, `reward_amount` | Rewarded ad fully watched |
| `ad_rewarded_skipped` | `placement`, `level_id` | Player declines rewarded ad |
| `ad_interstitial_shown` | `placement` ("level_end" / "map_transition"), `level_id`, `ad_network` | Interstitial displayed |
| `ad_interstitial_closed` | `placement`, `duration_viewed_seconds` | Player closes interstitial |
| `iap_store_opened` | `source` ("gem_shop" / "booster_upsell" / "out_of_gems_prompt") | Player opens IAP store UI |
| `iap_product_viewed` | `product_id`, `price_usd` | Player views a product |
| `iap_purchase_started` | `product_id`, `price_usd` | Player initiates purchase |
| `iap_purchase_completed` | `product_id`, `price_usd`, `gems_received`, `transaction_id` | Purchase verified and credited |
| `iap_purchase_failed` | `product_id`, `error_code`, `error_message` | Purchase failed |
| `iap_purchase_restored` | `product_id` | Purchase restored (non-consumable, if any) |

### 5.6 Custom Events — System

| Event | Parameters | Trigger |
|---|---|---|
| `sync_completed` | `direction` ("push" / "pull" / "merge"), `items_synced`, `conflicts_resolved`, `duration_ms` | Cloud sync completes |
| `sync_failed` | `error_code`, `error_message`, `retry_count` | Cloud sync fails |
| `remote_config_fetched` | `success`, `config_version`, `stale` (bool) | Remote Config fetch attempt |
| `push_token_registered` | `platform` | FCM token registered |
| `push_notification_received` | `notification_type`, `campaign_id` | Push notification received while app is open |
| `push_notification_opened` | `notification_type`, `campaign_id` | App opened from push notification |
| `deep_link_opened` | `link_type`, `destination` | Deep link / universal link handled |
| `tutorial_step_completed` | `step_index`, `step_type`, `level_id` | Player completes a tutorial step |
| `tutorial_completed` | `total_duration_seconds` | Player finishes all tutorial levels |
| `force_update_shown` | `current_version`, `required_version` | Force-update dialog displayed |
| `error_non_fatal` | `error_type`, `error_message`, `context` | Non-fatal exception logged |

### 5.7 User Properties (Firebase Analytics)

| Property | Type | Description |
|---|---|---|
| `player_level` | int | Current level number (updated on each level complete) |
| `total_levels_completed` | int | Lifetime levels completed |
| `paying_user` | bool | Has made at least one IAP |
| `total_spend_usd` | float | Lifetime IAP spend |
| `gems_balance` | int | Current gem balance (updated periodically) |
| `streak_count` | int | Current streak |
| `days_since_install` | int | Calculated from `first_open` |
| `potions_collected_count` | int | Total unique potions |
| `workshop_level` | int | `current_upgrade_index` |
| `ad_engagement_tier` | string | "none" / "low" / "medium" / "high" based on ad watches in last 7 days |

---

## 6. Crash Reporting (Firebase Crashlytics)

### 6.1 Setup

- Firebase Crashlytics Unity SDK integrated via Firebase Unity package.
- dSYM upload automated in CI (Fastlane `upload_symbols_to_crashlytics`).
- Android mapping file uploaded via Gradle Crashlytics plugin.

### 6.2 Custom Keys

Set on each gameplay session to aid crash diagnosis:

| Key | Value | When Set |
|---|---|---|
| `level_id` | Current level ID | Level start |
| `moves_remaining` | Remaining moves | Each move |
| `board_state_hash` | MD5 of board state array | Each move |
| `session_duration_s` | Seconds since app foreground | Periodic (every 30s) |
| `last_screen` | Current UI screen name | Screen transitions |
| `remote_config_version` | Config fetch timestamp | On config fetch |
| `network_status` | "online" / "offline" | Connectivity changes |

### 6.3 Non-Fatal Exception Logging

Log non-fatal exceptions for:
- Ad load failures (`AdLoadException`)
- Cloud sync failures (`SyncException`)
- Remote Config fetch failures
- JSON parse errors (level data, remote config)
- IAP initialization failures
- Asset bundle download failures

### 6.4 Alerting

- Firebase Console → Crashlytics → Alerts: enabled for new crash clusters and regressions.
- Velocity alert: trigger if crash-free users drops below 99.5% in 1 hour.
- Integrate with Slack/Discord webhook for real-time crash notifications.

---

## 7. Push Notification Strategy

### 7.1 Notification Types

| Type | Trigger | Timing | Content | Frequency Cap |
|---|---|---|---|---|
| **Daily Brew Reminder** | Player hasn't completed today's daily brew | 18:00 local time | "Your daily brew is ready! Don't break your {streak_count}-day streak." | 1/day, not sent if already completed |
| **Streak at Risk** | Player has active streak but hasn't played today | 20:00 local time | "Your {streak_count}-day streak ends at midnight! Play now to keep it going." | 1/day, only if streak ≥ 3 |
| **Event Starting** | Event start_date reached | Event start time | "Spooky Brews event is live! Brew haunted potions for exclusive rewards." | 1 per event |
| **Event Ending Soon** | 24 hours before event end | 24h before end | "Last chance! The Spooky Brews event ends tomorrow." | 1 per event |
| **Lapsed Player** | Player hasn't opened app in 3 days | Day 3, 10:00 local | "Your cauldron is getting cold! Come back and brew something amazing." | 1 per lapse period |
| **New Content** | New level pack or feature released | On publish | "50 new levels just dropped! Continue your journey." | Max 1/week |
| **Win-back** | Player hasn't opened app in 7+ days | Day 7, 14, 28 | "We miss you! Here's a free Color Bomb to get you back in the brew." | Max 1/week, stop after day 28 |

### 7.2 Implementation

- **Topics:** `all_users`, `platform_ios`, `platform_android`, `streak_active`, `event_{id}_participants`.
- **Scheduled sends:** Cloud Function triggered by Cloud Scheduler (cron). Function queries eligible users and sends via FCM Admin SDK.
- **Local notifications:** Daily brew reminder and streak-at-risk can also be scheduled as local notifications (iOS `UNUserNotificationCenter`, Android `AlarmManager`) for reliability when offline.
- **Opt-out:** In-app settings toggle (`notifications_enabled`). Respected both locally (cancel local notifications) and server-side (unsubscribe from FCM topics).

### 7.3 Rules

- Never send more than 2 push notifications per day.
- Never send between 22:00 and 08:00 local time.
- Stop all re-engagement pushes if player has been inactive > 28 days (avoid spam complaints).
- All notification content must be localizable (store templates in Remote Config keyed by language).

---

## 8. Security Considerations

### 8.1 Client-Side Economy Validation

The client tracks currencies locally for offline play, but the client is **never trusted** as the source of truth for economy:

- Currency balances are stored in Firestore and maintained server-side.
- Offline earnings are recorded as **deltas** (not absolute values) in a pending sync queue: `{ "type": "essence_earned", "amount": 200, "source": "level_complete", "level_id": 48, "timestamp": "..." }`.
- On sync, a Cloud Function processes the queue: validates each delta against the corresponding `level_result`, checks that the level was actually completable, and applies credits.
- If a delta is invalid (e.g., claims 1000 Essence for a level that awards max 200), it is rejected and logged for fraud review.

### 8.2 Server-Side IAP Receipt Verification

1. Client completes purchase → receives receipt from App Store / Google Play.
2. Client sends receipt to Cloud Function `verifyPurchase`.
3. Cloud Function calls Apple `verifyReceipt` endpoint (or App Store Server API v2) / Google Play Developer API `purchases.products.get`.
4. On valid receipt: credit Gems to Firestore, return success.
5. On invalid receipt: reject, log as potential fraud.
6. Client never credits Gems locally without server confirmation. If offline during purchase (rare edge case), receipt is queued and verified on next connectivity.

### 8.3 Anti-Cheat

| Threat | Mitigation |
|---|---|
| **Memory editing (GameGuardian, etc.)** | Client-side obfuscation via IL2CPP. Server is currency authority. No client-only currency writes accepted without validation. |
| **Replay attacks (submit same level result twice)** | Level results have unique `timestamp` + `level_id` composite. Cloud Function checks for duplicates within a time window. |
| **Time manipulation (clock forward for streaks/dailies)** | Server timestamps used for streak and daily brew validation. `streak_last_date` compared against server time, not client time. |
| **Fake level completions** | Level results include `moves_used`, `score`, `potions_brewed`, `duration_seconds`. Cloud Function sanity-checks against level definition (e.g., score can't exceed theoretical max for that level). |
| **Leaderboard manipulation** | Leaderboards (if added) are write-only via Cloud Functions. Scores validated against level results. |
| **Network interception (MITM)** | All Firebase communication is TLS. App Check (DeviceCheck on iOS, Play Integrity on Android) validates requests come from genuine app instances. |

### 8.4 Firebase App Check

Enabled in production:
- iOS: DeviceCheck attestation provider.
- Android: Play Integrity attestation provider.
- Cloud Functions and Firestore configured to require valid App Check tokens.
- Prevents API abuse from spoofed clients, bots, or emulators.

### 8.5 Data Privacy

- Player data is stored in Firestore under `players/{uid}` — scoped to authenticated user only.
- Account deletion: Cloud Function `deleteAccount` removes all Firestore data, Auth record, and Cloud Storage files for the user. Required for App Store compliance.
- COPPA: if targeting under-13, disable personalized ads and limit data collection. For MVP, target 13+ to simplify compliance.
- GDPR: privacy policy discloses all data collected. Consent dialog shown to EU users before analytics/ads initialization (managed via Google UMP SDK).

---

## 9. Cloud Functions Inventory

| Function | Trigger | Purpose |
|---|---|---|
| `verifyPurchase` | HTTPS callable | Validates IAP receipts against App Store / Google Play APIs. Credits currencies on success. |
| `syncPlayerState` | HTTPS callable | Processes offline sync queue. Validates currency deltas. Merges player state. |
| `submitLeaderboardScore` | HTTPS callable | Validates and writes score to leaderboard collection (post-MVP). |
| `sendDailyBrewReminder` | Cloud Scheduler (daily 17:50 UTC) | Queries eligible players, sends FCM notifications at 18:00 local. |
| `sendStreakAtRisk` | Cloud Scheduler (daily 19:50 UTC) | Queries players with active streaks who haven't played, sends FCM at 20:00 local. |
| `sendEventNotifications` | Cloud Scheduler (hourly) | Checks for events starting/ending within the next hour, sends relevant FCM. |
| `sendLapsedPlayerWinback` | Cloud Scheduler (daily 09:50 UTC) | Queries lapsed players (3d, 7d, 14d, 28d), sends re-engagement FCM. |
| `deleteAccount` | HTTPS callable | Deletes all player data from Firestore, Auth, and Cloud Storage. |
| `onPlayerCreated` | Auth `onCreate` trigger | Initializes player profile document in Firestore with defaults. |

---

## 10. Monitoring & Observability

### 10.1 Dashboards

| Dashboard | Platform | Metrics |
|---|---|---|
| **Retention** | Firebase Analytics | D1, D3, D7, D14, D30 retention rates. Cohorted by install date. |
| **Economy** | Firebase Analytics + BigQuery | Essence/Gem earn rates vs spend rates. Sink/source balance. |
| **Monetization** | AdMob + App Store Connect / Play Console | ARPDAU, ARPU, eCPM, rewarded ad opt-in rate, IAP conversion. |
| **Engagement** | Firebase Analytics | DAU, sessions/day, avg session length, levels played/session. |
| **Funnel** | Firebase Analytics | Install → Tutorial complete → Level 5 → Level 10 → Level 20 → IAP. |
| **Stability** | Crashlytics | Crash-free user %, crash clusters, ANR rate (Android). |
| **Performance** | Firebase Performance Monitoring | App start time, screen rendering time, network latency for sync calls. |

### 10.2 Alerts

| Alert | Condition | Channel |
|---|---|---|
| Crash-free users < 99.5% | 1-hour window | Slack #brew-alerts |
| Cloud Function error rate > 5% | 5-minute window | Slack #brew-alerts |
| Firestore read/write quota > 80% | Daily check | Email to leads |
| Revenue anomaly (±30% from 7-day avg) | Daily | Slack #brew-revenue |
| D1 retention drops below 35% | Weekly cohort | Email to PM + leads |

---

## 11. Scalability Notes (Post-MVP)

### 11.1 Firestore Scaling

- MVP traffic (0–50K DAU): well within Firestore free tier and default quotas.
- At 100K+ DAU: monitor read/write operations. Level results subcollection grows linearly — consider TTL (auto-delete results older than 90 days) or archive to BigQuery.
- At 500K+ DAU: evaluate Firestore usage costs. Consider moving high-read data (leaderboards, event rankings) to Realtime Database or a Redis cache.

### 11.2 Cloud Functions Scaling

- 2nd gen Cloud Functions (Cloud Run) auto-scale to 1000 instances by default.
- `verifyPurchase` and `syncPlayerState` are the highest-traffic functions. Profile latency and set minimum instances (1–3) to avoid cold starts.
- Move to a dedicated Cloud Run service if function execution time or memory needs exceed Cloud Functions limits.

### 11.3 Content Delivery

- Event assets served from Cloud Storage with Firebase Hosting CDN for edge caching.
- At scale, consider Unity Addressables + custom CDN (CloudFront / Fastly) for lower latency and cost.

### 11.4 Data Pipeline

- Export Firebase Analytics to BigQuery (free, built-in) for custom SQL analysis.
- Post-MVP: Amplitude or Mixpanel for self-serve product analytics if PM needs exceed Firebase Analytics' funnel/cohort depth.
- Data warehouse: BigQuery is the natural choice given the Firebase ecosystem.

### 11.5 Feature Additions Requiring Infrastructure Changes

| Feature | Infrastructure Impact |
|---|---|
| **Guilds / Social** | New Firestore collections (`guilds/{id}`, `guild_members`). Real-time listeners for chat (consider Realtime Database for chat). |
| **Leaderboards** | Firestore collection with Cloud Function writes. At scale, move to Redis sorted sets or a dedicated leaderboard service. |
| **Live tournaments** | Cloud Functions for matchmaking and scoring. Consider Cloud Run for WebSocket support if real-time is needed. |
| **A/B testing** | Firebase Remote Config supports A/B test experiments natively. Use with Firebase Analytics goals. |
| **Content creation tools** | Level editor web app (React) hosted on Firebase Hosting. Writes level JSON to Cloud Storage. Accessible to designers without Unity. |
| **Localization** | String tables in Remote Config or Cloud Storage JSON. Client fetches based on `settings.language`. |
