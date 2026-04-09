---
description: QA validation checklist for Brew. Use before any PR merge or build submission.
---

# Skill: Brew QA Checklist

## When to Use

- Before merging any PR
- Before submitting a build to TestFlight or Google Play Internal Testing
- After completing a milestone (weeks 2, 4, 6, 8, 10)
- When validating a new level before it ships
- When verifying economy changes haven't broken balance

---

## 1. Pre-Merge Checklist

Run this checklist before every PR merge. Every item must pass.

### Build & Code Quality

- [ ] **Compiles clean.** Zero errors on both iOS and Android build targets.
- [ ] **Zero warnings.** No new compiler warnings introduced. Pre-existing warnings must not increase.
- [ ] **All tests pass.** Both EditMode (unit) and PlayMode (integration) test suites pass in CI.
- [ ] **No hardcoded economy values.** Grep for currency amounts, booster costs, or reward values in `.cs` files. All gameplay values must come from ScriptableObjects or RemoteConfig.
- [ ] **No new currencies.** Verify no new `CurrencyType` enum values, no new balance fields on player profile, no new currency-related classes.
- [ ] **No public fields.** All inspector-exposed fields use `[SerializeField] private`. No `public` fields on MonoBehaviours.
- [ ] **No `Instantiate`/`Destroy` in gameplay loop.** Object pooling is mandatory for tokens, orbs, particles.

### Architecture Compliance

- [ ] **State machine not bypassed.** Board state transitions follow the documented sequence: Idle → PlayerInput → Fusing → Cascading → Settling → CheckBrew → CheckWin → Idle. No state is skipped.
- [ ] **Game logic is pure C#.** New logic in `Scripts/Core/` has no MonoBehaviour or UnityEngine dependency (except `Mathf` or `Vector2Int` where appropriate).
- [ ] **Events used for decoupling.** No new direct references between manager classes. Inter-system communication uses C# events or the service locator.
- [ ] **Files in correct directories.** Board/fusion/scoring logic → `Core/`. Meta systems → `Meta/`. Save/sync → `Data/`. Ads/IAP/analytics → `Services/`. Visual/UI → `Presentation/`.

### Analytics

- [ ] **Events present for new features.** Every new player action, economy transaction, or progression milestone fires the corresponding event from the tracking plan.
- [ ] **Event names and parameters match tracking plan exactly.** All `snake_case`, correct types, correct parameter names.
- [ ] **Events tested in Firebase DebugView.** DebugView screenshot included in PR description (for PRs that add/modify analytics).

### Documentation

- [ ] **Spec changes documented.** If the PR implements something not in the existing docs, the relevant doc file is updated in the same PR.
- [ ] **Economy changes reflected in economy-model.md.** If currency earn/spend rates changed, the doc is updated first.

---

## 2. Pre-Build Checklist

Run this before submitting a build to TestFlight or Google Play Internal Testing.

### Performance

- [ ] **60 fps on target devices.** Profile on iPhone SE 2nd gen (A13) and Samsung Galaxy A14 (Helio G80). Sustained 60 fps during gameplay — including cascades and brew animations.
- [ ] **No frame spikes > 33ms.** Check Unity Profiler for allocation spikes, GC pauses, or shader compilation stalls during gameplay.
- [ ] **Peak memory < 350 MB.** Measure during a complex level with max particles and cascades.
- [ ] **Particle budget respected.** ≤ 3 active emitters simultaneously during gameplay.

### Size & Startup

- [ ] **Install size < 100 MB.** Check the final `.ipa` / `.apk` / `.aab` size. If over budget, check for uncompressed textures, unstripped debug symbols, or unnecessary plugins.
- [ ] **Cold start < 5 seconds.** Time from app launch to interactive main menu on target devices. Firebase SDK init must not block the main thread.
- [ ] **Textures use ASTC compression** on both platforms.
- [ ] **Audio encoded as Ogg Vorbis @ 96kbps** or lower.

### Stability

- [ ] **Zero crashes in recent test sessions.** Review Crashlytics dashboard for the last 48 hours of internal testing. Zero crash-free rate below 99.5%.
- [ ] **No ANR (Application Not Responding) on Android.** Verify no main-thread blocking operations (network, file I/O) exceed 5 seconds.
- [ ] **Graceful offline behavior.** Disconnect network and verify: gameplay works, saves persist, Remote Config uses cache, ad buttons hide, sync queues locally.

### Platform-Specific

- [ ] **iOS minimum version: iOS 16.0.** Verify in Xcode build settings.
- [ ] **Android minimum API level: 26 (Android 8.0).** Verify in `build.gradle` / Player Settings.
- [ ] **Notch / safe area handling.** UI elements don't clip behind notches, dynamic islands, or rounded corners. Test on iPhone 15, Samsung S24 (or equivalent).
- [ ] **Haptics respect device settings.** Haptic feedback is disabled when the player turns it off in Settings. Respects iOS system haptic setting.
- [ ] **Reduce Motion respected.** Screen shake and particle intensity are reduced when the OS accessibility setting is enabled.

---

## 3. Level Validation Checklist

Run this for every new level before it ships.

### Solvability

- [ ] **Level is solvable.** Run 100+ greedy simulations. ≥ 60% must complete the recipe within `move_limit`.
- [ ] **No softlocks.** Verify no board configuration where recipe-target tokens are permanently inaccessible.
- [ ] **Blockers don't create impossible states.** No blocker occupies > 30% of any column. No fully blocked row.
- [ ] **No initial cluster ≥ 6.** The board generator ensures no "free brew" on first load (max initial cluster = 5).
- [ ] **At least 3 valid clusters at start.** `MIN_VALID_CLUSTERS` = 3 is enforced on board generation.

### Star Thresholds

- [ ] **2-star achievable by ~60% of completing players.** Based on simulation median.
- [ ] **3-star achievable by ~20% of completing players on first attempt.** Based on simulation 75th percentile.
- [ ] **Star thresholds are set as moves-remaining values** (for hand-tuned levels) or score values (procedural levels).

### Move Count

- [ ] **Move count playtested.** Manually play the level 3+ times. Verify it feels fair — challenging but not impossible without boosters.
- [ ] **Move budget multiplier matches phase.** Tutorial ≥ 3.0×, Foundation ≥ 2.0×, Expansion ≥ 1.6×, Depth ≥ 1.4×, Mastery ≥ 1.2×.

### Pacing

- [ ] **Breathing room levels (every 5th) are genuinely easy.** Budget ≥ 2.5× minimum, no new mechanics, simple board, minimal blockers.
- [ ] **No two new mechanics on the same level.** Each new element gets its own spotlight level.
- [ ] **Difficulty fits the staircase curve.** Level difficulty increases within each phase and drops at each breathing room level.

### Edge Cases

- [ ] **Deadlock recovery works.** If no valid clusters exist, the reshuffle mechanic triggers correctly.
- [ ] **Cascade resolution completes.** Complex cascades (5+ waves) resolve without hanging. Resolution > 15 seconds force-completes.
- [ ] **Multiple brews in one resolution handled correctly.** Brews process sequentially (bottom-first, then left-first). Each independently fills recipe vials.

---

## 4. Economy Validation Checklist

Run this when any economy-related change is made.

### Currency Balance

- [ ] **Earn rates match the economy model.** Calculate daily Essence/Gem income for regular player (10 levels/day) and verify it matches `docs/economy/economy-model.md` §5.
- [ ] **Workshop completion target: 30–45 days (regular player).** Recalculate `36,350 / net_daily_essence`. Must be in range.
- [ ] **No infinite earn loops.** Verify no action sequence grants unbounded currency (e.g., earn → spend → refund → earn cycle).
- [ ] **Balances never go negative.** All spend operations validate `balance >= cost` before executing.
- [ ] **Server is authority for currencies.** On sync conflict, server balance wins. Offline earnings tracked as deltas, not absolutes.

### Transactions

- [ ] **Every earn fires `currency_earn`.** With `currency_type`, `amount`, `source`, `balance_after`.
- [ ] **Every spend fires `currency_spend`.** With `currency_type`, `amount`, `sink`, `item_id`, `balance_after`.
- [ ] **IAP receipts server-validated.** No reward granted without Cloud Function verification.
- [ ] **Rewarded ad rewards capped.** Daily cap (default 5) enforced. Cooldown (default 30s) enforced.

### Monetization Ethics

- [ ] **No new spend gates on content.** All levels, Daily Brew, events playable without spending.
- [ ] **No loot boxes or randomized rewards.** All purchases deliver exactly what's shown.
- [ ] **Close/dismiss button visible on all purchase modals.** Minimum 44×44 pt tap target, never delayed.
- [ ] **Timers are real.** Any countdown reflects an actual deadline.

---

## 5. Analytics Validation Checklist

Run this when analytics code is added or modified.

- [ ] **All events fire on golden path.** Walk through: launch → tutorial → play level → win → earn essence → use booster → workshop upgrade → daily brew. Verify each step fires its event in DebugView.
- [ ] **Events fire on failure paths.** Level fail, IAP cancel, ad decline, network error — verify failure events fire.
- [ ] **Parameter types are correct.** Integers are integers, strings are lowercase strings, no accidental type mismatches.
- [ ] **`session_id` attached to all events.** Verify in DebugView that every event within a session shares the same `session_id`.
- [ ] **Offline events queue and send.** Disconnect, trigger events, reconnect, verify events appear in DebugView with original timestamps.
- [ ] **No duplicate events.** Verify each action fires its event exactly once (e.g., `level_complete` doesn't fire twice on a single win).

---

## Quick Reference: Kill Criteria

From `docs/analytics/kpi-framework.md` — if these metrics fall below the threshold, the feature/build has a blocking issue:

| Metric | Target | No-Go |
|--------|--------|-------|
| Tutorial Completion | ≥ 85% | < 70% |
| D1 Retention | ≥ 40% | < 32% |
| D7 Retention | ≥ 15% | < 10% |
| Level Retry Rate | ≥ 40% | < 25% |
| CPI (iOS, Meta) | < $2.50 | > $4.00 |
| Crash-Free Rate | ≥ 99.5% | < 99% |
| Cold Start Time | < 5s | > 8s |
| Install Size | < 100 MB | > 120 MB |
