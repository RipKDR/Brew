# QA & Testing — Use Any Time

You are running a QA and testing pass on **Brew**, a mobile puzzle game at `T:\Brew`. This prompt can be used at any point during development — before a milestone gate, before a build submission, or whenever you suspect something is broken. It is a self-contained testing protocol.

---

## Step 1 — Read Source Documents

Read these files before running any tests:

1. `AGENTS.md` — project rules and architecture
2. `docs/testing/test-plan.md` — full test coverage requirements, test categories, what each test validates
3. `docs/testing/device-matrix.md` — P0 and P1 target devices with OS versions
4. `docs/testing/regression-checklist.md` — the 60-check pre-release regression list with pass/fail criteria
5. `docs/analytics/event-tracking-plan.md` — every analytics event that must fire (needed for Step 7)
6. `docs/project-management/session-handoff.md` — understand what was recently changed (recent changes are highest-risk)

---

## Step 2 — Run Unit Tests

Execute the Unity Test Framework unit tests:

```
Unity -batchmode -runTests -testPlatform EditMode -logFile test-results-unit.log
```

- These test pure C# game logic in `Scripts/Core/`: board state machine transitions, fusion rules, cascade resolution, scoring calculations, currency operations, level config validation
- All unit tests MUST pass. Any failure is a P0 blocker
- If tests fail, read the log, identify the failing test, read the test code and the code under test, fix the root cause
- Do not delete or skip failing tests. Fix the code, not the test

---

## Step 3 — Run Integration Tests

Execute integration tests in PlayMode:

```
Unity -batchmode -runTests -testPlatform PlayMode -logFile test-results-integration.log
```

- These test MonoBehaviour interactions: board initialization with TokenSpawner, input handling through InputHandler, visual state sync between core logic and presentation layer, scene transitions
- All integration tests MUST pass
- Integration test failures may indicate a wiring issue between `Scripts/Core/` and `Scripts/Presentation/` layers. Check that presenters are subscribing to the correct events

---

## Step 4 — Execute Regression Checklist

Open `docs/testing/regression-checklist.md` and execute every check manually on a real device (or emulator if device unavailable). The checklist covers these categories:

### Core Gameplay (checks ~1-15)
- Board loads correctly for each level config
- Tapping a cluster fuses tokens into an orb
- Orbs of the same color chain-fuse
- Cascade/gravity fills gaps correctly
- Brew triggers at the correct token threshold (6+)
- Recipe tracking updates accurately
- Win condition triggers when all recipes are complete
- Lose condition triggers when moves reach 0 with incomplete recipes
- Score calculation matches expected formula
- Board state machine never gets stuck (no frozen states)

### Tutorial (checks ~16-20)
- Tutorial starts on first launch
- Each tutorial step highlights the correct element
- Player can only tap what the tutorial directs
- Tutorial completes and unlocks free play
- Skipping tutorial (if available) doesn't break state

### Meta Systems (checks ~21-30)
- Workshop loads and displays current upgrade states
- Purchasing a workshop upgrade deducts Essence and shows animation
- Potion shelf displays correct collection state
- New potion unlock adds to shelf with animation
- Win streak increments on level completion
- Win streak resets on level failure
- Daily brew is available once per day and resets at midnight UTC
- Daily brew completion grants correct rewards

### Economy (checks ~31-40)
- Essence earned matches expected formula (stars × base × streak)
- Gems earned from correct sources (daily brew, shelf milestones, events)
- Currency cannot go below 0
- IAP purchase flow completes (sandbox/test mode)
- Receipt validation works
- No-Ads Pass removes interstitials
- Rewarded ads grant correct rewards
- Ad frequency cap is respected

### Firebase & Cloud (checks ~41-50)
- Anonymous auth creates account on first launch
- Player data syncs to Firestore
- Cloud save restores on new device
- Remote Config values override local defaults
- Analytics events fire (spot-check 10 key events in DebugView)
- Crashlytics captures a test crash

### UI/UX (checks ~51-55)
- All screens navigate correctly (no dead-end screens)
- Back button / swipe-back works on every screen
- UI scales correctly across tested device resolutions
- Safe areas respected (notch, home indicator)
- No text overflow or truncation in any language

### Edge Cases (checks ~56-60)
- App survives backgrounding during gameplay
- App survives kill and relaunch mid-level
- App handles network loss gracefully (offline mode)
- App handles interrupted IAP purchase
- App handles ad load failure (no ad available)

For each check, record: **Pass**, **Fail (with description)**, or **N/A (feature not yet built)**.

---

## Step 5 — Profile Performance

Profile on the two target low-end devices (or closest available):
- **iOS:** iPhone SE 2nd gen (A13 Bionic)
- **Android:** Samsung Galaxy A14 (MediaTek Helio G80)

Measure and record:
| Metric | Target | Actual iOS | Actual Android |
|---|---|---|---|
| Gameplay FPS | 60 stable | | |
| FPS during cascade | ≥45 | | |
| Cold start to menu | <4s | | |
| Level load time | <1.5s | | |
| RAM during gameplay | <300 MB | | |
| Build download size | <150 MB | | |

If any metric misses the target, log it as a P1 performance issue with specifics (which scene, which moment, measured value).

Use Unity Profiler connected to the device for FPS and memory. Use a stopwatch or screen recording for load times.

---

## Step 6 — Verify Analytics Events

Reference: `docs/analytics/event-tracking-plan.md`.

1. Enable Firebase DebugView on a test device (`adb shell setprop debug.firebase.analytics.app com.yourcompany.brew` on Android, or enable Analytics Debug mode in Xcode scheme)
2. Play a session that covers all features: complete a level, fail a level, use a booster, earn currency, spend currency, watch a rewarded ad, complete daily brew, upgrade workshop, unlock a potion
3. Check DebugView for each event. For every event in the tracking plan, verify:
   - Event name matches exactly (case-sensitive)
   - All required parameters are present
   - Parameter values are correct types (string, int, float)
   - No duplicate events (one action should fire one event)
4. List any missing or incorrect events as P1 bugs

---

## Step 7 — Check Economy Integrity

1. Read current economy config from `config/firebase/remote-config-defaults.json`
2. Run the economy simulation:

```
python scripts/economy-sim/simulate_economy.py
```

3. Verify output against targets from `docs/economy/economy-model.md`:
   - Workshop completes in 30-45 days for a player doing 10 levels/day
   - Gem income from free sources covers ~1 booster every 2-3 days
   - No currency can be earned faster than intended (check for multiplication exploits)
   - Spending Essence on workshop at the intended pace leaves enough for occasional booster purchases

4. If simulation fails targets, DO NOT adjust code. Flag it and reference the `07-economy-tuning.md` prompt for the proper tuning workflow.

---

## Step 8 — Validate All 40 Levels

For each level 1 through 40:
1. Load the level — verify it loads without errors
2. Confirm the board size, ingredient count, move limit, and recipe targets match the level config in `Assets/Resources/Levels/`
3. Play the level to completion (use debug tools if available to verify solvability quickly)
4. Confirm win triggers correctly and rewards are granted
5. Confirm the level feels appropriate for its position in the difficulty curve

Flag any level that:
- Fails to load (P0)
- Is unsolvable within the move limit (P0)
- Has incorrect config (P1)
- Feels significantly too easy or too hard for its position (P2)

---

## Step 9 — Memory Leak Test

Run a 30-minute continuous play session on a real device:

1. Start the app, note initial memory usage
2. Play levels continuously — complete some, fail some, use boosters, watch ads, visit workshop, visit shelf
3. Record memory usage every 5 minutes
4. After 30 minutes, memory should be within 10% of the 5-minute reading

If memory grows continuously, there's a leak. Common causes:
- Objects not returned to pool
- Event listeners not unsubscribed
- Textures loaded but never released
- Coroutines started but never stopped
- References held to destroyed GameObjects

Profile with Unity Memory Profiler to identify the source. Log as P1.

---

## Step 10 — Report Findings

Compile all findings into a structured report. For each issue found:

```
[SEVERITY] Summary
- Category: (Gameplay / Meta / Economy / Firebase / UI / Performance / Edge Case)
- Steps to Reproduce: (exact steps)
- Expected: (what should happen)
- Actual: (what does happen)
- Device/OS: (where observed)
- Screenshot/Log: (if available)
```

Severity ratings:
- **P0 — Blocker:** Crash, data loss, broken core loop, can't complete a level, security exploit. Must fix before any release.
- **P1 — Critical:** Feature broken but workaround exists, performance below target, analytics event missing, economy exploit. Must fix before soft launch.
- **P2 — Major:** Visual glitch, minor UX issue, non-critical feature broken. Should fix before global launch.
- **P3 — Minor:** Cosmetic issue, typo, animation not quite right. Fix when convenient.

Save the report to `docs/testing/qa-report-<date>.md` (e.g., `qa-report-2026-04-09.md`).

Update `docs/project-management/session-handoff.md` with a summary of findings and the count of issues by severity.
