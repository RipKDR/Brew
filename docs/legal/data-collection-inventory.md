# Data Collection Inventory — Brew

> **⚠️ TEMPLATE — VERIFY BEFORE USE**
> This inventory must be verified against actual app implementation before completing Apple Privacy Nutrition Labels and Google Play Data Safety forms. Update this document whenever data collection practices change.

**Last Updated:** [DATE]

---

## Overview

This document catalogs every data element collected by Brew, its purpose, destination, retention period, and user control mechanisms. It also maps each element to Apple App Privacy and Google Play Data Safety categories for store submission.

---

## 1. Device Information

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| Device model (e.g., iPhone 15, Pixel 8) | Crash diagnostics, performance optimization, compatibility analysis | Firebase Crashlytics, Firebase Analytics | 26 months (Analytics), 90 days (Crashlytics) | Cannot opt out — essential for app functionality. Can request full data deletion. |
| Operating system and version (e.g., iOS 18.2, Android 15) | Crash diagnostics, compatibility testing, feature support decisions | Firebase Crashlytics, Firebase Analytics | 26 months (Analytics), 90 days (Crashlytics) | Cannot opt out — essential for app functionality. Can request full data deletion. |
| Screen resolution and display scale | UI rendering optimization, layout analytics | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Language and locale settings | Localization, regional analytics | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| App version and build number | Crash diagnostics, version-specific issue tracking | Firebase Crashlytics, Firebase Analytics | 26 months (Analytics), 90 days (Crashlytics) | Cannot opt out. Can request full data deletion. |
| Firebase anonymous user ID | User session linkage, progress persistence | Firebase Auth, Firebase Analytics | Until account deletion or data deletion request | User can delete account via in-app settings or support request. |

**Apple Privacy Category:** Device ID and similar identifiers — *Data Not Linked to You* (anonymous ID), collected for *App Functionality* and *Analytics*.

**Google Play Data Safety:** Device or other IDs → *Collected*, *Not shared*, used for *App functionality* and *Analytics*.

---

## 2. App Usage Data

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| Session start timestamp | Usage pattern analysis, daily active user metrics | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Session end timestamp | Session duration calculation, engagement metrics | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Session duration | Engagement analysis, game balance tuning | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Levels opened/started | Content engagement analysis, funnel tracking | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Time spent per level | Difficulty analysis, game balance tuning | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Menu/screen navigation events | UX flow analysis, UI improvement | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| App foreground/background events | Session boundary tracking | Firebase Analytics | 26 months | Cannot opt out (automatically collected by Firebase SDK). Can request full data deletion. |
| First open date | Cohort analysis, retention metrics | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |

**Apple Privacy Category:** Usage Data → *Product Interaction* — *Data Not Linked to You*, collected for *Analytics*.

**Google Play Data Safety:** App activity → *App interactions* → *Collected*, *Not shared*, used for *Analytics*.

---

## 3. Gameplay Data

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| Level completion results (win/lose) | Difficulty balancing, progression analysis | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Score per level | Leaderboard potential (future), game balance | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Moves made per level | Puzzle difficulty calibration | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Fusions performed (type, count) | Core mechanic engagement analysis | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Brews created (type, count) | Core mechanic engagement analysis | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Boosters used (type, count, context) | Economy balance, booster effectiveness | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Level retries | Difficulty analysis, frustration detection | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Tutorial completion status | Onboarding funnel analysis | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Player progression (current level, stars, achievements) | Game state persistence, progression analysis | Firebase (Firestore/RTDB), Firebase Analytics | Duration of account existence + 30 days | User can delete account. Can request full data deletion. |

**Apple Privacy Category:** Usage Data → *Product Interaction* — *Data Not Linked to You*, collected for *Analytics* and *App Functionality*.

**Google Play Data Safety:** App activity → *App interactions*, *In-app search history* (N/A) → *Collected*, *Not shared*, used for *Analytics* and *App functionality*.

---

## 4. Economy Data

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| Virtual currency balance (Gems) | Game state persistence, economy analysis | Firebase (Firestore/RTDB), Firebase Analytics | Duration of account existence + 30 days | User can delete account. Balance has no real-world value. |
| Currency earned (source: gameplay, ads, purchases) | Economy balance analysis, monetization analytics | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Currency spent (destination: boosters, retries) | Economy drain analysis, feature pricing | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Ad reward events (rewarded ad completed, reward granted) | Ad monetization analysis, reward economy tuning | Firebase Analytics, AdMob | 26 months (Analytics), per AdMob retention policy | Can stop viewing rewarded ads (voluntary). Can request full data deletion. |
| No-Ads pass status | Ad display logic, purchase verification | Firebase (Firestore/RTDB) | Duration of account existence + 30 days | Non-consumable purchase; can request data deletion but purchase is restorable via Apple/Google. |

**Apple Privacy Category:** Usage Data → *Product Interaction* — *Data Not Linked to You*, collected for *Analytics* and *App Functionality*. Purchase History (see Section 7).

**Google Play Data Safety:** App activity → *App interactions* → *Collected*, *Not shared*, used for *Analytics* and *App functionality*.

---

## 5. Crash and Performance Data

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| Stack traces | Crash diagnosis and fix prioritization | Firebase Crashlytics | 90 days | Cannot opt out — essential for app stability. Can request full data deletion. |
| Exception type and message | Crash categorization | Firebase Crashlytics | 90 days | Cannot opt out. Can request full data deletion. |
| Device state at crash (memory usage, battery, disk, orientation) | Crash context analysis, environment-specific issue identification | Firebase Crashlytics | 90 days | Cannot opt out. Can request full data deletion. |
| App state at crash (current screen, recent actions) | Crash reproduction assistance | Firebase Crashlytics | 90 days | Cannot opt out. Can request full data deletion. |
| Thread and process information | Multi-threading issue diagnosis | Firebase Crashlytics | 90 days | Cannot opt out. Can request full data deletion. |
| Non-fatal error logs | Proactive issue detection before they cause crashes | Firebase Crashlytics | 90 days | Cannot opt out. Can request full data deletion. |
| Performance traces (app start time, screen render time, network latency) | Performance monitoring and optimization | Firebase Performance Monitoring (if enabled) | 90 days | Cannot opt out. Can request full data deletion. |

**Apple Privacy Category:** Diagnostics → *Crash Data*, *Performance Data* — *Data Not Linked to You*, collected for *App Functionality*.

**Google Play Data Safety:** App performance → *Crash logs*, *Diagnostics* → *Collected*, *Not shared*, used for *App functionality*.

---

## 6. Advertising Data

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| IDFA (iOS) | Ad personalization, attribution, frequency capping | AdMob (Google) | Per AdMob/Google retention policy | **Full control.** Requires ATT consent on iOS. User can deny tracking, reset IDFA, or enable "Limit Ad Tracking" in device settings. |
| GAID (Android) | Ad personalization, attribution, frequency capping | AdMob (Google) | Per AdMob/Google retention policy | **Full control.** User can opt out of ads personalization or reset advertising ID in device settings. |
| Ad impression events (ad unit ID, ad format, timestamp) | Ad revenue analytics, fill rate optimization | AdMob (Google), Firebase Analytics | 26 months (Analytics), per AdMob retention policy | Cannot opt out of impression counting. Can opt out of personalized ads. |
| Ad click events | Ad performance analysis, invalid click detection | AdMob (Google) | Per AdMob retention policy | Cannot opt out of click tracking. Can opt out of personalized ads. |
| Ad load/failure events | Ad SDK diagnostics, fill rate monitoring | Firebase Analytics | 26 months | Cannot opt out. Can request full data deletion. |
| Rewarded ad completion status | Reward grant verification | AdMob (Google), Firebase Analytics | 26 months (Analytics), per AdMob retention policy | Rewarded ads are voluntary. User can choose not to watch. |

**Apple Privacy Category:** Identifiers → *Device ID* (IDFA) — *Data Used to Track You* (only if ATT granted), collected for *Third-Party Advertising*. Usage Data → *Advertising Data* — *Data Not Linked to You*, collected for *Third-Party Advertising*.

**Google Play Data Safety:** Device or other IDs → *Collected*, *Shared* with AdMob for *Advertising*. User consent required for personalized ads.

---

## 7. Purchase Data

| Data Element | Purpose | Destination | Retention | User Control |
|---|---|---|---|---|
| Transaction ID | Purchase verification, duplicate prevention, support | Apple App Store / Google Play (primary), Firebase (receipt validation) | Per Apple/Google retention + 3–7 years for tax/legal compliance | Managed by Apple/Google. User can request purchase history from Apple/Google directly. |
| Product ID (e.g., `gems_pack_1`, `no_ads_pass`) | Purchase fulfillment, inventory management | Apple App Store / Google Play, Firebase | Duration of account existence + legal retention period | Cannot opt out — required for purchase fulfillment. |
| Purchase price and currency | Revenue analytics, regional pricing analysis | Apple App Store / Google Play (primary), Firebase Analytics (aggregate) | Per Apple/Google retention + 3–7 years for tax compliance | Managed by Apple/Google. |
| Purchase timestamp | Purchase history, support, analytics | Apple App Store / Google Play, Firebase Analytics | Per Apple/Google retention + legal retention period | Managed by Apple/Google. |
| Purchase status (completed, refunded, pending) | Purchase fulfillment, refund handling | Apple App Store / Google Play, Firebase | Duration of account existence | Managed by Apple/Google. |
| Receipt / purchase token | Server-side purchase validation, fraud prevention | Apple App Store / Google Play (validation endpoint), Firebase (temporary) | Validated and discarded; not stored long-term beyond validation result | Managed by Apple/Google. |

**Important:** We do not collect, process, or store credit card numbers, bank account details, or other payment instruments. All payment processing is handled entirely by Apple or Google.

**Apple Privacy Category:** Purchases → *Purchase History* — *Data Linked to You* (if account is linked via email) or *Data Not Linked to You* (if anonymous), collected for *App Functionality*.

**Google Play Data Safety:** Financial info → *Purchase history* → *Collected*, *Not shared* (handled by Google Play), used for *App functionality*.

---

## Summary: Apple Privacy Nutrition Label Mapping

| Apple Privacy Category | Data Types | Linked to User? | Tracking? | Purpose |
|---|---|---|---|---|
| Identifiers | Device ID (Firebase anonymous ID) | Not Linked | No | App Functionality, Analytics |
| Identifiers | Device ID (IDFA) | Not Linked | Yes (if ATT granted) | Third-Party Advertising |
| Usage Data | Product Interaction (gameplay, sessions, economy) | Not Linked | No | Analytics, App Functionality |
| Usage Data | Advertising Data (impressions, clicks) | Not Linked | No | Third-Party Advertising |
| Diagnostics | Crash Data | Not Linked | No | App Functionality |
| Diagnostics | Performance Data | Not Linked | No | App Functionality |
| Purchases | Purchase History | Linked (if email linked) / Not Linked (if anonymous) | No | App Functionality |

---

## Summary: Google Play Data Safety Mapping

| Google Play Category | Data Type | Collected? | Shared? | Purpose | Required? | Optional? |
|---|---|---|---|---|---|---|
| Device or other IDs | Device identifiers, GAID | Yes | Yes (GAID with AdMob) | Advertising, Analytics, App functionality | Device IDs required; GAID optional (user can opt out) | GAID is optional |
| App activity | App interactions, in-game actions | Yes | No | Analytics, App functionality | Required | — |
| App performance | Crash logs, diagnostics | Yes | No | App functionality | Required | — |
| Financial info | Purchase history | Yes | No (handled by Google Play) | App functionality | Required for purchases | — |

---

## Data Flow Diagram (Textual)

```
User Device
├── Firebase Auth → Anonymous/email user ID → Firebase servers (Google Cloud)
├── Firebase Analytics → All usage, gameplay, economy events → Firebase servers (Google Cloud)
├── Firebase Crashlytics → Crash data, device state → Firebase servers (Google Cloud)
├── AdMob SDK → IDFA/GAID (if consented), ad events → Google ad servers
├── Apple IAP / Google Play Billing → Purchase data → Apple/Google servers
└── Local storage only → Game state, preferences, consent status → Device only (not transmitted)
```

---

## Data Deletion Process

When a user requests data deletion:

1. **Firebase Auth:** Delete user record (anonymous or email-linked). This removes the user identifier.
2. **Firebase Analytics:** User-level data deletion is initiated via Firebase's data deletion API. Analytics data associated with the user ID is marked for deletion. Aggregate/anonymized data may persist.
3. **Firebase Crashlytics:** Crash data associated with the user's device is not individually identifiable after account deletion (linked via anonymous ID). No additional action required beyond auth deletion.
4. **AdMob:** Advertising data linked to IDFA/GAID is managed by Google. User can reset their advertising ID on-device. Google's data retention policies apply.
5. **Apple/Google Purchase Data:** Purchase transaction records are maintained by Apple and Google per their policies and legal requirements. We cannot delete records held by Apple or Google on the user's behalf.
6. **Local Device Data:** User should uninstall the app to remove all locally stored data.

**Target completion time:** Within 30 days of verified request, per GDPR requirements.

---

## Change Log

| Date | Change | Author |
|---|---|---|
| [DATE] | Initial inventory created | [NAME] |

---

> **⚠️ TEMPLATE REMINDER**
> This inventory must be verified against the actual implementation of the app. Any changes to SDKs, third-party services, or data collection practices must be reflected here and in the corresponding Apple Privacy Nutrition Labels and Google Play Data Safety sections. This document does not constitute legal advice.
