# Week 9-10: Content & Polish

You are continuing work on **Brew**, a mobile puzzle game at `T:\Brew`. This prompt covers Weeks 9-10: tuning all 40 levels, implementing the weekly event system, optimizing performance, completing device testing, preparing app store assets, and producing the soft launch build.

---

## Prerequisites

Before starting this phase, verify the **Week 7-8 milestone** has PASSED. Run through `docs/production/milestone-gates.md` Gate 4 criteria:

- Full session loop works: play → earn → upgrade workshop → daily brew → streak
- IAP purchase flow completes on both platforms with server-side receipt validation
- Rewarded ads play and grant correct rewards at all 4 placements
- Analytics events fire correctly and appear in Firebase DebugView
- Cloud save works across devices
- Economy simulation confirms workshop completes in 30-45 days
- No currency exploits

If any of these fail, go back to the Week 7-8 prompt and fix them. Do not polish a game with broken monetization or leaking economy.

---

## Step 1 — Read Source Documents

Read these files before starting work:

1. `AGENTS.md` — project rules, architecture, naming conventions
2. `docs/game-design/level-design-framework.md` — level design principles, difficulty curve, pacing rules
3. `docs/liveops/event-templates.md` — weekly event system spec, theme swapping, reward structure
4. `docs/technical/infrastructure.md` — push notifications, CI/CD, build pipeline
5. `docs/testing/test-plan.md` — full test coverage requirements
6. `docs/testing/regression-checklist.md` — the 60-check pre-release regression list
7. `docs/testing/device-matrix.md` — P0 and P1 target devices with OS versions
8. `docs/creative/ua-creative-strategy.md` — app store assets, screenshot spec, ASO keywords
9. `docs/localization/localization-plan.md` — i18n approach and string extraction
10. `docs/project-management/session-handoff.md` — current state

---

## Step 2 — Week 9: Content Tuning & Events

### 2.1 Level Tuning Pass

This is the most important task of Week 9. Every level ships to players — each one must feel intentional.

- Play through all 40 levels in sequence on a real device. For each level, note:
  - Is it solvable within the move limit? (If not, it ships broken)
  - Does the move limit feel tight-but-fair, not punishing?
  - Does the ingredient distribution create interesting decisions?
  - Does the recipe target match the difficulty intent?
  - Is it fun? Would you play it again?

- Target completion rates (first attempt, no boosters):
  - **Breather levels** (every 5th: 5, 10, 15, 20, 25, 30, 35, 40): 90%+ completion rate. These exist to relax the player and reward progress
  - **Standard levels**: ~70% first-attempt completion rate
  - **Challenge levels** (levels just before breathers: 4, 9, 14, 19, 24, 29, 34, 39): 40-50% completion rate. Hard enough to create tension, not so hard they feel unfair

- Use `python scripts/level-generator/generate_level.py --level <N> --seed <S>` to regenerate any level that doesn't meet its target. Hand-tune parameters: `--moves`, `--ingredients`, `--recipes`, `--board-size`

- Milestone levels (10, 20, 30, 40) deserve extra attention. These should introduce a new mechanic or ingredient combination and feel like an event. Hand-tune these — do not rely purely on procedural generation

- Save tuned level configs to `Assets/Resources/Levels/`. Validate each with `python scripts/build/build_config.py --validate-levels`

### 2.2 Difficulty Curve Validation

After individual tuning, evaluate the full 40-level arc:

- Plot difficulty: estimated completion rate vs. level number. The curve should trend downward with periodic relief at breather levels (sawtooth pattern)
- No sudden spikes: if level N-1 is 80% and level N is 30%, that's a spike. Smooth it
- The first 5 levels (tutorial zone) should all be 95%+
- Levels 6-15 gradually introduce complexity
- Levels 16-30 are the mid-game — varied difficulty with consistent breathers
- Levels 31-40 are the hardest but still fair. Level 40 should feel like a satisfying finale, not a wall

### 2.3 Weekly Event Template

Reference: `docs/liveops/event-templates.md`.

- Create `WeeklyEventManager.cs` in `Scripts/Core/LiveOps/` — manages event lifecycle, level loading, reward distribution
- Create `WeeklyEventView.cs` in `Scripts/Presentation/LiveOps/` — event banner on main menu, event level select, event results
- Event structure: 7 themed levels, playable during the event window (7 days). Completion earns an exclusive potion (added to shelf) + Gem bonus
- Theme swapping via Remote Config: event name, color palette reference, ingredient skin overrides, potion reward ID. The code should not know about specific themes — it reads config and applies
- Implement the first event: **"Lunar Brew"**
  - 7 event levels with moonlight color palette
  - Exclusive potion: "Moonlight Elixir"
  - Reward: 50 Gems for completing all 7 levels
  - Deploy by setting Remote Config keys: `event_active=true`, `event_id=lunar_brew`, `event_name=Lunar Brew`, `event_end_timestamp=<unix>`, `event_potion_id=moonlight_elixir`
- Fire events: `event_start`, `event_level_complete`, `event_complete`, `event_reward_claimed`
- Feature-flagged: `weekly_events_enabled` Remote Config key. If false, hide event UI entirely

### 2.4 Push Notifications

Reference: `docs/technical/infrastructure.md` → Push Notifications section.

- Integrate Firebase Cloud Messaging (FCM)
- Create `NotificationManager.cs` in `Scripts/Core/Services/`
- Notification triggers (scheduled locally, not server-push for MVP):
  1. **Daily Brew available** — fire at midnight UTC + 1 hour if today's Daily Brew hasn't been completed
  2. **Streak about to expire** — fire at 8 PM local time if player hasn't played today and has an active streak
  3. **Event ending soon** — fire 24 hours before event end if player hasn't completed all event levels
- Frequency cap: maximum 1 notification per day. Never stack multiple notifications
- Respect OS notification permissions. If user denies, do not prompt again for 7 days
- Fire `notification_scheduled`, `notification_opened` events

### 2.5 Bug Fixing

- Review all known issues from `docs/project-management/session-handoff.md` blockers section
- Review any crash reports from testing sessions
- Prioritize P0 (blockers) and P1 (critical) issues. P2 and P3 can ship with soft launch if necessary
- Run the full test suite: `Unity -batchmode -runTests -testPlatform PlayMode -logFile test-results.log`
- Fix any new test failures

---

## Step 3 — Week 10: Polish & Submission Prep

### 3.1 Performance Optimization

Reference: `docs/technical/performance-budget.md` (if it exists) or these targets:

- **Target devices:** iPhone SE 2nd gen (A13), Samsung Galaxy A14 (Helio G80)
- **Frame rate:** Stable 60 FPS during gameplay, no drops below 45 FPS during cascades
- **Load time:** Cold start to main menu under 4 seconds. Level load under 1.5 seconds
- **Memory:** Under 300 MB RAM during gameplay on low-end devices
- **Battery:** Less than 8% drain per 30-minute session
- **Build size:** Under 150 MB download, under 250 MB installed

Optimization checklist:
1. Profile with Unity Profiler on target devices. Identify top 5 CPU and GPU bottlenecks
2. Object pool warmup: ensure `TokenPool`, `OrbPool`, `ParticlePool` pre-warm during loading, not during gameplay
3. Texture atlas: all UI elements in one atlas, all token/orb sprites in one atlas. Max 2048x2048
4. Draw calls: target under 50 draw calls during gameplay. Use `SpriteRenderer` batching, minimize material count
5. Audio: compress all SFX to Vorbis. Stream music, don't load into memory
6. Garbage collection: zero-alloc gameplay loop. Profile with Deep Profiler to find allocations
7. Shader warmup: pre-warm all shaders during loading screen

### 3.2 Device Testing

Reference: `docs/testing/device-matrix.md`.

- Test on every P0 device (must ship without issues) and every P1 device (should ship without major issues)
- Check for each device:
  - Layout renders correctly at device resolution and aspect ratio
  - Safe area insets are respected (notch, home indicator, camera cutout)
  - Touch input registers correctly, no dead zones near edges
  - Text is readable at device DPI
  - Animations play smoothly
  - Audio plays correctly
  - App survives backgrounding and foregrounding
  - App survives phone call interruption
  - App survives low memory warning

### 3.3 Regression Testing

Reference: `docs/testing/regression-checklist.md`.

Execute every check on the regression list. There are 60 checks across these categories:
- Core gameplay (board, fusion, brew, cascade, win/lose)
- Tutorial flow
- Meta systems (workshop, shelf, streaks, daily brew)
- Economy (earning, spending, IAP, ads)
- Firebase (auth, cloud save, remote config, analytics)
- UI/UX (navigation, transitions, responsive layout)
- Performance (frame rate, memory, load times)
- Edge cases (offline mode, interrupted purchase, app kill during save)

**All 60 checks must pass.** Any failure is a blocker for soft launch. Log failures with severity ratings and fix before proceeding.

### 3.4 App Store Preparation

Reference: `docs/creative/ua-creative-strategy.md` → ASO section and Screenshot Guide.

- **App Icon:** 1024x1024 PNG, no alpha. Follow art direction from `docs/creative/art-direction.md`. Export to `marketing/app-icon/`
- **Screenshots:** 5 screenshots per device class (6.7" iPhone, 6.5" iPhone, iPad, Android phone, Android tablet). Capture actual gameplay with overlaid marketing text. Export to `marketing/screenshots/`
- **App Preview Video:** 30-second gameplay video showing: tutorial → satisfying cascade → brew → potion complete → workshop upgrade. Capture on iPhone 15 Pro or equivalent. Export to `marketing/video/`
- **App Store Description:** title, subtitle, description, keywords — pull from `docs/creative/ua-creative-strategy.md` ASO keywords. Write to `marketing/store-listing/`
- **Category:** Games → Puzzle
- **Age Rating:** 4+ (no objectionable content). If ads are shown, disclose

### 3.5 Localization Prep

Reference: `docs/localization/localization-plan.md`.

- Install Unity Localization package if not already present
- Extract ALL player-facing strings into a String Table asset. Reference `docs/localization/string-keys.md` for the key naming convention
- Verify every UI element uses a localized string reference, not a hardcoded string
- English is the default locale. All strings must be complete in English
- Create placeholder locale entries for the planned translation languages (from localization plan) but do not translate yet — translations are post-MVP
- Test that switching locale doesn't break any UI layout (text overflow, truncation)

### 3.6 Soft Launch Build

- Create release builds:
  - **iOS:** Archive in Xcode, upload to App Store Connect, create TestFlight build
  - **Android:** Generate signed AAB, upload to Google Play Console Internal Testing track
- Build configuration:
  - IL2CPP scripting backend (both platforms)
  - Code stripping enabled
  - Firebase production project (not development)
  - AdMob production ad unit IDs (not test IDs)
  - Analytics in production mode (not DebugView)
  - Crashlytics enabled
- Verify builds on real devices:
  - iOS: install via TestFlight on at least 2 devices (different screen sizes)
  - Android: install via Internal Testing on at least 2 devices
  - Play through 5 levels, complete a purchase (sandbox), watch a rewarded ad, open workshop
  - Confirm no crashes, no ANRs, no layout issues

---

## Step 4 — Milestone Gate (Week 9-10)

Before proceeding to soft launch, verify ALL of the following. Check against `docs/production/milestone-gates.md` Gate 5:

### Level Quality
- [ ] All 40 levels are playable and completable without boosters
- [ ] Breather levels (every 5th) feel relaxing — 90%+ completion rate estimate
- [ ] Challenge levels feel hard but fair — 40-50% completion rate estimate
- [ ] Difficulty curve is smooth with no sudden spikes
- [ ] Milestone levels (10, 20, 30, 40) feel like events

### Performance
- [ ] 60 FPS stable on iPhone SE 2nd gen
- [ ] 60 FPS stable on Samsung Galaxy A14
- [ ] Cold start under 4 seconds on both
- [ ] Memory under 300 MB during gameplay
- [ ] Build size under 150 MB

### Regression
- [ ] All 60 regression checks pass (documented in test results)

### Events
- [ ] Weekly event template loads and plays correctly
- [ ] Event levels are accessible during active event
- [ ] Event rewards are granted correctly
- [ ] Event can be toggled via Remote Config

### Store Assets
- [ ] App icon exported at 1024x1024
- [ ] 5 screenshots per device class ready
- [ ] App preview video captured and edited
- [ ] Store description and keywords written

### Build Stability
- [ ] Release build runs for 30+ minutes without crash on both platforms
- [ ] No ANRs on Android
- [ ] No memory leaks (memory stable during extended play)

### Ready for Submission
- [ ] TestFlight build uploaded and installable
- [ ] Internal Testing build uploaded and installable
- [ ] Both builds use production Firebase, production ad units, Crashlytics enabled

---

## Step 5 — Session Completion

Before ending this session, follow the session completion protocol from `00-context-recovery.md` Step 6:

1. Update `docs/project-management/session-handoff.md`
2. Update `docs/project-management/project-summary.md` Decision Log if needed
3. Run `python scripts/context/build_context_snapshot.py --project-root .`
4. Log any new ADRs in `docs/adr/`

If the milestone gate passes, note in the handoff that the project is ready for `09-soft-launch-prep.md`.
