# Device Testing Matrix — Brew

> Version 1.0 · Pre-production · Last updated 2026-04-09
> Audience: QA, Engineering, Production

---

## 1. iOS Devices (Minimum iOS 15)


| Device            | iOS Version | Screen Size | Resolution  | Priority                    | Notes                                                                                                                                    |
| ----------------- | ----------- | ----------- | ----------- | --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| iPhone SE 2nd gen | 15.x        | 4.7"        | 750 × 1334  | **P0 — Performance floor**  | Smallest supported screen. A10 Fusion chip. Must hold 60 fps during cascade animations. Touch targets must be reachable at minimum size. |
| iPhone 11         | 16.x        | 6.1"        | 828 × 1792  | **P0 — Mass market**        | High-volume device in target demographics. A13 Bionic. Notch safe-area handling.                                                         |
| iPhone 13         | 17.x        | 6.1"        | 1170 × 2532 | **P1 — Current mainstream** | A15 chip. Standard notch. Validates mid-tier performance.                                                                                |
| iPhone 15         | 18.x        | 6.1"        | 1179 × 2556 | **P1 — Latest**             | A16 chip. Dynamic Island — verify no UI overlap with island cutout.                                                                      |
| iPhone 15 Pro Max | 18.x        | 6.7"        | 1290 × 2796 | **P2 — Large screen**       | Max screen size. Verify grid and HUD layout scale correctly. Dynamic Island + ProMotion 120 Hz.                                          |
| iPad Air 5th gen  | 17.x        | 10.9"       | 1640 × 2360 | **P2 — Tablet**             | M1 chip. Portrait tablet layout must be playable — verify grid centering, touch target sizes, and HUD positioning.                       |


### iOS-Specific Checks

- Notch / Dynamic Island safe area insets respected on all screens
- Home indicator bar does not overlap bottom HUD elements
- Status bar hidden during gameplay; visible on menus
- App Tracking Transparency (ATT) prompt displays correctly before ad SDK initializes
- StoreKit 2 IAP flow completes for all products
- Game Center disabled (not used) — no spurious prompts

---

## 2. Android Devices (Minimum API 26 / Android 8.0)


| Device                | Android Version | RAM  | Chipset            | Priority                   | Notes                                                                                                                                         |
| --------------------- | --------------- | ---- | ------------------ | -------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| Samsung Galaxy A14    | 13              | 4 GB | Exynos 850         | **P0 — Performance floor** | Low-end mass-market device. Heaviest install base in emerging markets. Must not drop below 30 fps; target 60 fps. Monitor thermal throttling. |
| Samsung Galaxy S21    | 14              | 8 GB | Snapdragon 888     | **P0 — Mid-range**         | High-volume mid-range. Validates performance on 2–3 year old flagship silicon. Punch-hole camera safe area.                                   |
| Xiaomi Redmi Note 12  | 13              | 4 GB | Snapdragon 4 Gen 1 | **P1 — Emerging markets**  | MIUI-specific quirks (notification handling, background kill policies). Validates budget chipset.                                             |
| Google Pixel 7a       | 14              | 8 GB | Tensor G2          | **P1 — Stock Android**     | Reference Android experience. Validates behavior without OEM overlays.                                                                        |
| Samsung Galaxy S24    | 15              | 8 GB | Snapdragon 8 Gen 3 | **P2 — Flagship**          | Latest flagship. Validates forward compatibility and 120 Hz support.                                                                          |
| Samsung Galaxy Tab A9 | 14              | 4 GB | MediaTek Helio G99 | **P2 — Tablet**            | Budget tablet. Portrait tablet layout, larger touch targets, HUD scaling.                                                                     |


### Android-Specific Checks

- Notch / punch-hole / camera cutout safe area insets respected (all variants)
- Navigation bar (gesture vs. 3-button) does not overlap gameplay
- Split-screen / freeform multi-window: game displays correctly or gracefully blocks
- Battery optimization (Doze mode): game resumes correctly after deep sleep
- OEM background kill policies (Xiaomi, Samsung, Huawei): push notifications still arrive; save state persists
- Google Play Billing Library v6+ IAP flow completes for all products
- Android App Bundle (AAB) asset delivery: all assets load on first launch

---

## 3. Test Protocol Per Device

Execute the following protocol on every device in the matrix. Record results in the shared QA tracker.

### 3.1 Installation & Cold Start


| #     | Check           | Target                     | Method                                                  |
| ----- | --------------- | -------------------------- | ------------------------------------------------------- |
| TP-01 | Install size    | < 100 MB (download)        | Check Play Store / App Store listing size               |
| TP-02 | On-device size  | < 200 MB (installed)       | Settings → App Info after install                       |
| TP-03 | Cold start time | < 5 seconds to interactive | Stopwatch from tap-to-launch to first interactive frame |
| TP-04 | Warm start time | < 2 seconds                | Background app, reopen                                  |


### 3.2 Performance


| #     | Check                              | Target                         | Method                                                |
| ----- | ---------------------------------- | ------------------------------ | ----------------------------------------------------- |
| TP-05 | Frame rate — idle board            | Stable 60 fps                  | Unity Profiler or on-device FPS overlay               |
| TP-06 | Frame rate — cascade animation     | ≥ 55 fps (no perceptible drop) | Trigger 3-wave cascade, monitor profiler              |
| TP-07 | Frame rate — brew animation        | ≥ 50 fps                       | Trigger brew with particle effects, monitor profiler  |
| TP-08 | Frame drops during gravity refill  | No frame below 45 fps          | Full-column refill (7 tokens dropping simultaneously) |
| TP-09 | Memory usage — launch              | < 150 MB                       | Profiler snapshot after main menu loaded              |
| TP-10 | Memory usage — 20-level session    | < 250 MB, no upward trend      | Profile memory at level 1, 10, 20; verify no leak     |
| TP-11 | Memory — no leaks on level restart | Stable after 5 restarts        | GC allocation profiler shows no growth                |


### 3.3 Battery & Thermal


| #     | Check                          | Target                                      | Method                                                                        |
| ----- | ------------------------------ | ------------------------------------------- | ----------------------------------------------------------------------------- |
| TP-12 | Battery drain — 30-min session | < 8% drain (full charge, mid brightness)    | Record battery % before and after 30 min continuous play                      |
| TP-13 | Thermal throttling             | No thermal warning; device warm but not hot | Play 30 min, check for throttling indicators in profiler and device skin temp |
| TP-14 | Background battery drain       | < 0.5% over 1 hour backgrounded             | Background the app for 1 hour, measure drain                                  |


### 3.4 Display & Layout


| #     | Check                                       | Target                                                         | Method                                        |
| ----- | ------------------------------------------- | -------------------------------------------------------------- | --------------------------------------------- |
| TP-15 | Safe area — notch/punch-hole/Dynamic Island | No UI elements obscured by cutout                              | Visual inspection on all screens              |
| TP-16 | Grid fits screen                            | Full 7×9 grid visible without scrolling                        | Visual inspection; no clipping                |
| TP-17 | HUD readability                             | Score, moves, recipe vials all legible                         | Visual inspection at arm's length             |
| TP-18 | Touch targets                               | All interactive elements ≥ 44×44 pt (iOS) / 48×48 dp (Android) | Measure with layout inspector                 |
| TP-19 | Tablet layout                               | Grid centered; no excessive empty space; HUD repositioned      | Visual inspection on iPad Air / Galaxy Tab A9 |


### 3.5 Input & Responsiveness


| #     | Check                     | Target                                             | Method                                                                    |
| ----- | ------------------------- | -------------------------------------------------- | ------------------------------------------------------------------------- |
| TP-20 | Tap-to-response latency   | < 100 ms from tap to visual feedback               | High-speed camera or touch latency profiler                               |
| TP-21 | Tap accuracy              | Correct cell registers on grid edges and corners   | Tap each corner and edge cell, verify correct cluster highlights          |
| TP-22 | Multi-touch rejection     | Only single touches register during gameplay       | Two-finger tap on grid, verify only one fusion (or no-op)                 |
| TP-23 | Accidental scroll vs. tap | Grid does not scroll; taps always register as taps | Slight drag gesture on grid cell, verify it registers as tap (not scroll) |


---

## 4. Priority Definitions


| Priority | Definition                                                                                       | Release Gate                                                        |
| -------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------- |
| **P0**   | Must pass on these devices before any release build. Performance floor and mass-market coverage. | Hard gate — build cannot ship if P0 devices fail any TP check.      |
| **P1**   | Must pass before public launch. Covers current mainstream and regional variants.                 | Soft gate — can ship with documented minor issues and a patch plan. |
| **P2**   | Should pass. Covers edge cases (large screen, tablet, flagship).                                 | Advisory — document issues, fix in next update cycle.               |


---

## 5. Device Lab Management

### 5.1 Acquisition Priority

Purchase or procure devices in priority order. P0 devices must be physically available in the QA lab before soft launch.

### 5.2 OS Version Updates

- P0 devices: pin to the specified OS version. Do not update until QA validates on the new version.
- P1/P2 devices: update to latest OS within 2 weeks of release. Run regression checklist after update.

### 5.3 Cloud Device Testing

Supplement physical devices with cloud device farms (Firebase Test Lab, BrowserStack) for:

- Automated smoke tests on broader device matrix (50+ models)
- Screenshot comparison across screen sizes
- Crash detection on devices not physically available

---

*End of Device Testing Matrix.*