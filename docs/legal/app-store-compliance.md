# App Store Submission Compliance Checklist — Brew

> **⚠️ TEMPLATE — VERIFY BEFORE SUBMISSION**
> This checklist reflects requirements as understood at time of drafting. App store policies change frequently. Verify all items against current Apple App Store Review Guidelines and Google Play Developer Program Policies before each submission.

**Last Updated:** [DATE]

---

## Apple App Store (App Store Review Guidelines)

### Content & Age Rating

- [ ] **Age Rating:** Select **12+** in App Store Connect. Brew is a puzzle game with optional in-app purchases but no simulated gambling, loot boxes, horror, violence, or mature content. Confirm via Apple's age rating questionnaire that "Frequent/Intense" categories are all answered "No" except "Infrequent/Mild Simulated Gambling" if applicable — Brew has no gambling mechanics, so this should be "None."
- [ ] **Content:** No objectionable, offensive, or illegal content. No pornography, hate speech, or discriminatory material.
- [ ] **No User-Generated Content:** Brew MVP has no chat, messaging, friend lists, profiles, or user-generated content. No moderation system required for MVP.
- [ ] **Screenshots & Previews:** All App Store screenshots and app preview videos accurately represent actual gameplay. No misleading imagery, no features shown that are not available.

### In-App Purchases

- [ ] **Apple IAP Exclusive:** All digital purchases (Gem packs at $0.99, $2.99, $4.99, $9.99, $19.99 and No-Ads pass at $4.99) use Apple's In-App Purchase system exclusively. No links, buttons, or messaging directing users to external payment mechanisms.
- [ ] **IAP Metadata:** Each IAP product has a clear name, description, and price in App Store Connect matching the in-game presentation.
- [ ] **Restore Purchases:** Implement a "Restore Purchases" button accessible from settings for non-consumable purchases (No-Ads pass). Required by Apple.
- [ ] **No Subscriptions (MVP):** No auto-renewing subscriptions in MVP. If subscriptions are added later, comply with Guidelines 3.1.2 (auto-renewable subscription rules).

### Privacy

- [ ] **Privacy Policy URL:** A publicly accessible Privacy Policy URL is provided in App Store Connect.
- [ ] **Privacy Nutrition Labels:** Accurately complete the App Privacy section in App Store Connect declaring:
  - **Data Used to Track You:** Advertising identifiers (IDFA) — only if ATT consent granted.
  - **Data Linked to You:** Purchase history (if account linked via email).
  - **Data Not Linked to You:** Analytics data, crash data, device identifiers, usage data, gameplay data.
  - See `data-collection-inventory.md` for complete mapping to Apple Privacy categories.
- [ ] **App Tracking Transparency (ATT):** Implement the ATT permission prompt (`ATTrackingManager.requestTrackingAuthorization`) before accessing IDFA for ad personalization. Include a clear purpose string in `Info.plist` under `NSUserTrackingUsageDescription` explaining why tracking permission is requested.
- [ ] **Purpose Strings:** Include all required `Info.plist` usage description strings for any permissions requested. Brew should not request camera, microphone, location, contacts, or other sensitive permissions.

### Authentication

- [ ] **Sign in with Apple:** Not required for Brew MVP. Sign in with Apple is only required when the app offers third-party social login (Facebook, Google, etc.). Brew uses anonymous auth with optional email linking — no third-party social login, so Sign in with Apple is not required.
- [ ] **Anonymous Auth:** Anonymous authentication as default login is permitted. No account creation is forced on the user.

### Performance & Functionality

- [ ] **Stability:** App must not crash during App Review testing. Test thoroughly on supported devices.
- [ ] **System Events:** App responds correctly to system events: incoming calls, memory warnings, low battery, Do Not Disturb, orientation lock.
- [ ] **Offline Functionality:** Core puzzle gameplay functions without an internet connection. Online features (ads, purchases, analytics) degrade gracefully when offline.
- [ ] **Minimum Functionality:** Game provides meaningful entertainment value. Not a thin wrapper around a web view or trivial functionality.
- [ ] **Launch Performance:** App launches within a reasonable time. No excessively long loading screens.
- [ ] **IPv6 Compatibility:** App works on IPv6-only networks (Apple requirement).

### Device & OS Requirements

- [ ] **Minimum iOS Version:** Declare minimum supported iOS version (recommended: iOS 16.0+ for 2026 submissions).
- [ ] **Required Device Capabilities:** Declare in `Info.plist` via `UIRequiredDeviceCapabilities`. For Brew, this likely includes only `arm64`.
- [ ] **Supported Devices:** Test on both iPhone and iPad if the app is universal. If iPhone-only, declare accordingly.
- [ ] **Dark Mode / Dynamic Type:** Recommended but not strictly required. Consider supporting system appearance settings.

### Additional Apple Requirements

- [ ] **HTTPS:** All network communication uses HTTPS/TLS. No insecure HTTP connections.
- [ ] **App Transport Security:** ATS is enabled. Any exceptions are justified and documented.
- [ ] **Bitcode / Symbol Upload:** Upload dSYM files for crash symbolication.
- [ ] **Review Notes:** Provide clear review notes to Apple's review team explaining the game, how to test IAP (sandbox credentials if needed), and any special configuration.

---

## Google Play Store (Developer Program Policies)

### Target API Level & Technical

- [ ] **Target SDK Version:** Meet Google Play's current target API level requirement. For 2026, this is API level 34 (Android 14) or higher. Verify against [Google's current requirements](https://developer.android.com/google/play/requirements/target-sdk).
- [ ] **64-bit Support:** App includes 64-bit native libraries (arm64-v8a). 32-bit-only apps are not accepted.
- [ ] **App Bundle:** Submit as Android App Bundle (.aab), not APK.

### Content Rating

- [ ] **IARC Questionnaire:** Complete the International Age Rating Coalition questionnaire in Google Play Console. Answer accurately regarding:
  - Violence: None / Mild (puzzle game — no violence).
  - Sexuality: None.
  - Language: None.
  - Controlled Substances: None.
  - Gambling: None (no loot boxes, no random paid rewards, no gacha).
  - User Interaction: None (no social features, no chat, no UGC).
  - In-App Purchases: Yes (disclose clearly).
- [ ] **Expected Rating:** "Everyone" or "Everyone 10+" depending on IARC assessment. The 13+ internal target is a business decision; the store rating may differ based on content.

### Data Safety

- [ ] **Data Safety Section:** Complete Google Play's Data Safety form in Play Console:
  - **Data Collected:** Device identifiers, app activity (usage, gameplay), app performance (crash logs), financial (purchase history via Google Play).
  - **Data Shared:** Advertising data shared with AdMob for ad serving.
  - **Security Practices:** Data encrypted in transit, users can request data deletion.
  - See `data-collection-inventory.md` for complete mapping to Google Play Data Safety categories.
- [ ] **Privacy Policy URL:** A publicly accessible Privacy Policy URL is provided in the Play Console store listing. Required for all apps that collect user data.

### Advertising

- [ ] **AdMob Compliance:** Ads served via Google AdMob in compliance with AdMob policies.
- [ ] **No Deceptive Ads:** Ad placements do not mimic UI elements, do not trick users into clicking, and are clearly distinguishable from game content.
- [ ] **Interstitial Timing:** Interstitial ads are not shown immediately on app launch or during gameplay actions where accidental clicks are likely. Displayed at natural transition points (level completion, returning to menu).
- [ ] **Rewarded Ads:** Rewarded ads are opt-in only. Users are not forced to watch ads to continue core gameplay.
- [ ] **Ad Content:** Request only age-appropriate ads via AdMob content rating settings.

### In-App Purchases

- [ ] **Google Play Billing:** All digital purchases use Google Play Billing Library. No alternative billing mechanisms for digital goods.
- [ ] **Clear Pricing:** All IAP prices are clearly displayed before purchase confirmation.
- [ ] **No Misleading IAP:** IAP descriptions accurately describe what the user receives.

### Families Policy

- [ ] **Families Self-Certification:** Evaluate whether Brew needs to opt into Google's Families program. Since Brew targets 13+ and does not primarily target children, opting into the Families program is likely unnecessary. However, if the content rating results in "Everyone," review Families policy requirements.
- [ ] **Ads in Apps for Families:** If opted into Families, ads must use Google Play certified ad SDKs. AdMob qualifies, but additional restrictions apply.
- [ ] **Neutral Age Screen:** If implementing an age gate, it must be neutral (not suggest the "correct" answer).

### Permissions

- [ ] **Minimal Permissions:** Brew should request no dangerous permissions. Verify that the manifest does not include unnecessary permissions:
  - No `CAMERA`
  - No `RECORD_AUDIO`
  - No `ACCESS_FINE_LOCATION` / `ACCESS_COARSE_LOCATION`
  - No `READ_CONTACTS`
  - No `READ_PHONE_STATE`
  - `INTERNET` — required (for ads, analytics, auth)
  - `ACCESS_NETWORK_STATE` — acceptable (for connectivity checks)
  - `AD_ID` — required for GAID access; must be declared in manifest and Data Safety section
- [ ] **Permission Rationale:** If any permissions beyond the above are required, provide clear rationale in Play Console.

### Store Listing

- [ ] **Accurate Description:** Store listing description accurately represents the game. No misleading claims.
- [ ] **Screenshots:** Screenshots show actual gameplay, not renders or mockups.
- [ ] **Feature Graphic:** 1024x500 feature graphic provided.
- [ ] **Category:** Select appropriate category (Games → Puzzle).
- [ ] **Contact Information:** Developer email and (optionally) website and phone number provided.

---

## Cross-Platform Compliance

### Legal Documents

- [ ] **Privacy Policy Hosted:** Privacy policy is hosted at a stable, publicly accessible URL (not behind authentication). URL works on both mobile browsers and desktop.
- [ ] **Terms of Service Hosted:** Terms of service are hosted at a stable, publicly accessible URL.
- [ ] **In-App Access:** Both Privacy Policy and Terms of Service are accessible from within the game (e.g., Settings screen) via tappable links.
- [ ] **Legal Review:** Both documents have been reviewed by qualified legal counsel before publication.

### GDPR Compliance (European Union)

- [ ] **Consent Flow:** Implement a consent management flow for EU/EEA users before collecting data for ad personalization. Must provide genuine choice (not a dark pattern).
- [ ] **Consent Record:** Record and store user consent status. Respect consent withdrawal.
- [ ] **Data Processing Agreements:** Ensure Data Processing Agreements are in place with Firebase/Google and any other data processors.
- [ ] **Right to Erasure:** Implement mechanism for users to request data deletion. Respond within 30 days.
- [ ] **Legitimate Interest Assessment:** Document legitimate interest assessment for non-consent-based processing (crash reporting, fraud prevention).

### COPPA Compliance (United States)

- [ ] **Age Gate (if applicable):** If any mechanism collects age, implement a neutral age gate that does not encourage users to misrepresent their age.
- [ ] **No Known Children Under 13:** Confirm that the app does not knowingly collect personal information from children under 13. Firebase Auth anonymous tokens do not constitute "personal information" under COPPA in most interpretations, but verify with counsel.
- [ ] **Mixed Audience Considerations:** If the app could attract users under 13 despite the 13+ target, consider implementing age-gated data collection restrictions.
- [ ] **AdMob Child-Directed Settings:** If any portion of the audience may include children, configure AdMob with appropriate child-directed treatment tags.

### Accessibility

- [ ] **VoiceOver (iOS):** Core menu elements, buttons, and navigation are accessible via VoiceOver. Include `accessibilityLabel` on all interactive elements.
- [ ] **TalkBack (Android):** Core menu elements, buttons, and navigation are accessible via TalkBack. Include `contentDescription` on all interactive elements.
- [ ] **Touch Target Size:** All interactive elements meet minimum touch target size (44x44 pt on iOS, 48x48 dp on Android).
- [ ] **Color Contrast:** Text and essential UI elements meet WCAG 2.1 AA contrast ratios (4.5:1 for normal text, 3:1 for large text).
- [ ] **No Color-Only Information:** Game mechanics do not rely solely on color to convey information. Provide shape, pattern, or label alternatives.

### Localization

- [ ] **English (en):** Full localization — all UI, store listing, legal documents.
- [ ] **Spanish (es):** Store listing and core UI (recommended for global launch).
- [ ] **Portuguese (pt-BR):** Store listing and core UI (recommended for global launch).
- [ ] **French (fr):** Store listing and core UI (recommended for global launch).
- [ ] **German (de):** Store listing and core UI (recommended for global launch).
- [ ] **Japanese (ja):** Store listing and core UI (recommended for global launch).
- [ ] **Korean (ko):** Store listing and core UI (recommended for global launch).
- [ ] **RTL Support:** If Arabic, Hebrew, or other RTL languages are added later, verify layout mirroring.

### Additional Cross-Platform

- [ ] **Age Rating Consistency:** Ensure age ratings on both stores are consistent and appropriate.
- [ ] **IAP Parity:** Ensure IAP product offerings and prices are consistent across platforms (accounting for platform-specific pricing tiers).
- [ ] **Feature Parity:** Core game features are consistent across iOS and Android versions.
- [ ] **Data Deletion Mechanism:** Provide a clear method for users to request account and data deletion, accessible from within the app and from external channels (email).

---

## Pre-Submission Final Checks

- [ ] All placeholder values (`[COMPANY NAME]`, `[CONTACT EMAIL]`, `[DATE]`, etc.) replaced in legal documents.
- [ ] Privacy policy and terms of service reviewed by legal counsel.
- [ ] All IAP products created and tested in sandbox/test environments on both platforms.
- [ ] ATT prompt tested on iOS (appears before IDFA access, app functions correctly if denied).
- [ ] GDPR consent flow tested (EU users see consent prompt, non-EU users do not, consent can be changed in settings).
- [ ] App tested on oldest supported OS versions on both platforms.
- [ ] App tested on variety of screen sizes (small phones, large phones, tablets if supported).
- [ ] Crash-free rate > 99% verified via Firebase Crashlytics.
- [ ] All analytics events firing correctly and appearing in Firebase Analytics.
- [ ] Store listing assets prepared (screenshots, descriptions, feature graphic, app icon) for all target languages.
- [ ] Build numbers and version codes incremented appropriately.

---

> **⚠️ TEMPLATE REMINDER**
> App store policies are updated regularly. Before each submission, review the latest versions of Apple's [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) and Google's [Developer Program Policies](https://play.google.com/about/developer-content-policy/). This checklist does not constitute legal or compliance advice.
