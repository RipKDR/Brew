# Brew — Milestone Gates

**Version:** 1.0  
**Last updated:** 2026-04-09  
**Owner:** Game Designer / Producer  

---

## Gate Philosophy

Every milestone gate is a binary pass/fail decision point. The purpose is to catch fundamental problems early — before the team invests weeks of effort building on a broken foundation. Gates are not bureaucratic checkpoints; they are honest assessments of whether the project is on track.

**Decision authority:** The Game Designer / Producer (GP) owns the final go/no-go call for Gates 1–4. Gate 5 (soft launch readiness) requires agreement from GP + both Unity Developers.

---

## Gate 1 — Foundation (End of Week 2)

### Statement
> Can tap a cluster and see tokens fuse into an orb on screen.

### Pass Criteria

| #   | Criterion                                                                                                          | How to Verify                                                            |
| --- | ------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| 1.1 | Board generates with configurable dimensions (minimum: 6×6, 7×8, 8×8 tested).                                     | Change `BoardConfig` ScriptableObject values, verify board renders.      |
| 1.2 | 5 ingredient types spawn on the board with no pre-existing clusters ≥ 3.                                           | Generate 20 boards, visually inspect. Zero should have tap-ready groups. |
| 1.3 | Tapping a cluster of ≥ 3 same-type adjacent tokens removes them and spawns a Tier-1 orb at the tap position.       | Tap 10 valid clusters across 3 boards. All must resolve correctly.       |
| 1.4 | Tapping a cluster of < 3 tokens does nothing (no state change, no crash).                                          | Tap isolated tokens and pairs. Verify no response.                       |
| 1.5 | Gravity fills gaps after fusion — tokens above fall down, new tokens spawn at top.                                 | Fuse a cluster in the middle of the board. Verify column compaction.     |
| 1.6 | No crashes during 5 minutes of continuous tapping on iOS and Android.                                              | QA runs a 5-minute stress test on 1 device per platform.                 |
| 1.7 | Frame rate ≥ 30 fps during fusion animations on lowest-spec target device (iPhone SE 2nd gen or equivalent Android).| Profile with Unity Profiler or FPS counter overlay.                       |

### Evaluation Protocol
1. GP and QA each independently play 3 freshly-generated boards for 5 minutes.
2. GP verifies criteria 1.1–1.5 through structured test cases.
3. QA verifies 1.6–1.7 through device testing.
4. Both sign off on a shared checklist (Google Sheet or Notion page).

### Go/No-Go Decision
- **Pass:** All 7 criteria met. Proceed to Week 3.
- **Fail — minor (1–2 criteria failed):** 2-day rework window. UD-1 fixes. Re-evaluate on Wednesday of Week 3.
- **Fail — major (≥ 3 criteria failed or fundamental input/rendering broken):** Full-team retro. Identify root cause (architecture problem vs. implementation bug). Max 1-week rework. If still failing after rework, escalate: consider whether grid-based puzzle is the right mechanic.

---

## Gate 2 — Core Loop (End of Week 4)

### Statement
> Can play a complete level from start to win/lose with working fusions, chains, brews, and recipe tracking.

### Pass Criteria

| #   | Criterion                                                                                                           | How to Verify                                                             |
| --- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| 2.1 | Chain fusion works: adjacent same-type orbs auto-fuse into higher tiers after gravity settles.                       | Create a board state with two adjacent Tier-1 orbs. Verify auto-fusion.   |
| 2.2 | Brew triggers when an orb reaches the configured threshold tier. Vial UI updates to reflect brew progress.           | Play until a Tier-3 orb forms. Verify vial fills.                         |
| 2.3 | Recipe tracker accurately counts brewed potions against recipe targets. Level completes when all targets met.        | Complete a level with 2 recipe targets. Verify both register correctly.    |
| 2.4 | Move counter decrements on each player tap. Level fails when moves reach 0 with recipe incomplete.                  | Play a level, count taps manually, verify counter matches. Exhaust moves. |
| 2.5 | Cascades resolve correctly: gravity refill can trigger new auto-fusions, which trigger more gravity, up to cap of 20.| Set up a board where a cascade chain of ≥ 3 steps is likely. Observe.     |
| 2.6 | Level loads from JSON: different board sizes, ingredient sets, and recipes across ≥ 3 test levels.                   | Load levels 1, 5, and 10. Verify each has distinct configuration.         |
| 2.7 | Level complete screen shows star rating. Level fail screen shows retry option. Both transitions work without crash.   | Win a level, verify complete screen. Lose a level, verify fail screen.     |
| 2.8 | Level select screen shows correct lock/unlock state and star counts after completing levels.                         | Complete levels 1–3, return to level select. Verify stars and unlock.      |
| 2.9 | Progress persists across app restart (kill app, reopen, verify level progress is retained).                          | Complete level 5, force-quit app, relaunch, check level select.           |
| 2.10| No soft-locks during gameplay (board always has at least one valid move or triggers game-over).                      | Play 10 levels to completion. Zero soft-locks.                            |

### Evaluation Protocol
1. GP plays levels 1–10 start to finish without developer assistance.
2. GP deliberately tests edge cases: exhaust moves, achieve cascade chains, quit mid-level and resume.
3. QA runs the same 10 levels on 2 iOS + 2 Android devices.
4. UD-1 reviews cascade cap behavior by logging cascade depth to console.

### Go/No-Go Decision
- **Pass:** All 10 criteria met. Proceed to Week 5.
- **Fail — minor (1–3 criteria failed, none are 2.1–2.3):** 2-day rework. Focus on the failed criteria. Re-evaluate Wednesday of Week 5.
- **Fail — major (2.1, 2.2, or 2.3 failed, OR ≥ 4 criteria failed):** The core loop is broken. Stop all art and content work. Full team focuses on core mechanic repair for up to 1 week. If chain/brew mechanic cannot be made to work reliably, reconsider the core mechanic design.

---

## Gate 3 — Feel and Juice (End of Week 6)

### Statement
> The game FEELS good. A non-team-member can pick it up, complete the tutorial, and want to play more.

**This is the most critical gate.** A mechanically correct game that doesn't feel good will not retain players. This gate has the highest bar and the most structured evaluation.

### Pass Criteria

| #   | Criterion                                                                                                              | How to Verify                                                                         |
| --- | ---------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| 3.1 | Tutorial: 5 tutorial levels are implemented. Each teaches one concept (tap, chain, brew, booster, recipe).             | GP plays through tutorial cold (as if first time). Verify teaching sequence is clear.  |
| 3.2 | Tutorial completion: ≥ 4 out of 5 external playtesters complete all 5 tutorial levels without asking for help.          | External playtest (6.10 from build plan). Record completion rate.                      |
| 3.3 | Continued play: ≥ 3 out of 5 external playtesters voluntarily continue playing past the tutorial (at least 2 more levels) without prompting. | Observe playtest. Note when testers stop. Do not suggest they continue.    |
| 3.4 | Art integration: all placeholder art is replaced. Tokens, orbs, vials, HUD elements use final or near-final art.       | Visual audit by ART and GP. No colored circles or programmer art visible.              |
| 3.5 | Particles and effects: fusion sparkles, brew bubbles, cascade trails, and level-complete celebration all play correctly.| Play 3 levels. Verify all particle triggers fire on correct events.                    |
| 3.6 | Sound: all core SFX (tap, fuse, chain, brew, win, lose) are integrated and play at appropriate volume levels.          | Play with sound on. Verify no missing SFX, no ear-piercing volume spikes.              |
| 3.7 | Haptics: fusion, chain, and brew events trigger appropriate haptic feedback on supported devices.                       | Test on iPhone (Taptic Engine) and Android device with vibration motor.                 |
| 3.8 | All 3 boosters function correctly and are accessible from the booster bar during gameplay.                             | Use each booster in a level. Verify effect, charge decrement, and visual feedback.      |
| 3.9 | No crashes during 15 minutes of continuous play on iOS and Android.                                                    | QA runs extended play session on 2 devices per platform.                                |
| 3.10| Qualitative: at least 1 external playtester spontaneously expresses positive sentiment ("that was satisfying," smiles, etc.) during chain or brew events. | GP observes playtester reactions. Document verbatim quotes.               |

### Evaluation Protocol — External Playtest
1. Recruit 5 people who have never seen the game. Mix of casual and mid-core mobile gamers. Age 18–45.
2. Hand them the device with the game open. Say only: "This is a puzzle game we're working on. Please play it."
3. GP observes silently. Do not answer questions, offer hints, or explain mechanics. Take notes on:
   - Where they tap first
   - Whether they understand the tutorial instructions
   - Where they hesitate or look confused
   - Whether they smile, frown, or show frustration
   - When they stop playing and why
4. After they stop (or after 15 minutes), ask three questions:
   - "What did you think?"
   - "Was anything confusing?"
   - "Would you play this again?"
5. Compile results. Gate passes if criteria 3.1–3.10 are met.

### Go/No-Go Decision
- **Pass:** All 10 criteria met. Proceed to Week 7.
- **Fail — feel issues (3.2, 3.3, or 3.10 failed):** This is serious. The game doesn't feel fun. Hold all meta/economy work. Spend up to 1 week diagnosing and fixing:
  - Review playtest footage/notes. Identify the specific moments players disengage.
  - Adjust animation timing, particle intensity, SFX responsiveness.
  - Simplify tutorial if players are confused.
  - Re-run external playtest with 3 new testers.
  - If second playtest still fails criteria 3.2/3.3: **Pivot meeting.** Evaluate whether the core mechanic (cluster tap → fuse → brew) is inherently satisfying enough. Consider mechanic redesign.
- **Fail — technical (3.4–3.9 failed but 3.2/3.3/3.10 passed):** 3-day fix window. These are integration issues, not design problems.

---

## Gate 4 — Meta and Economy (End of Week 8)

### Statement
> Full session loop works: play levels → earn currency → upgrade workshop → return for daily brew → maintain streak.

### Pass Criteria

| #   | Criterion                                                                                                             | How to Verify                                                                       |
| --- | --------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| 4.1 | Potion Shelf displays all brewed potions accurately. Unbrewed potions are locked/greyed.                              | Brew 3 different potion types. Verify shelf updates correctly.                       |
| 4.2 | Workshop renders with cauldron. Upgrade from Tier 1 → Tier 2 works: costs Essence, shows animation, applies bonus.    | Earn Essence, purchase upgrade. Verify cost deducted, visual changes, bonus active.  |
| 4.3 | Daily Brew: unique level available once per day. Completing it grants bonus Essence. Calendar shows streak.            | Complete daily brew. Advance clock 24 hours. Verify new daily brew available.        |
| 4.4 | Win Streak: counter increments on consecutive wins, resets on fail. Streak bonuses grant at correct thresholds.        | Win 3 consecutive levels. Verify streak bonus at 3. Fail a level. Verify reset.      |
| 4.5 | Essence earned from level completions matches design spreadsheet values (±10% tolerance).                             | Complete 5 levels, record Essence earned. Compare to spreadsheet.                    |
| 4.6 | Gems can be purchased via IAP (sandbox/test environment). Purchase grants correct Gem amount. Restore purchases works.| Make test purchase on iOS sandbox and Google Play test track.                         |
| 4.7 | Rewarded ad (fail recovery): after level fail, watching ad grants +5 moves and level continues.                       | Fail a level, watch test ad, verify moves granted and gameplay resumes.               |
| 4.8 | Rewarded ad (2× reward): after daily brew completion, watching ad doubles Essence reward.                             | Complete daily brew, watch ad, verify double Essence credit.                          |
| 4.9 | Rewarded ad (streak protection): after fail with streak ≥ 3, watching ad preserves streak.                            | Build streak to 3, fail level, watch ad, verify streak preserved.                    |
| 4.10| Interstitial ads appear after every 3rd level win (not after fails). No interstitial within 30 s of another.          | Win 6 levels in a row. Verify interstitials after levels 3 and 6 only.               |
| 4.11| Firebase auth works (anonymous login on first launch, Google Sign-In optional).                                       | Fresh install: verify anonymous auth. Sign in with Google. Verify profile creation.  |
| 4.12| Cloud save: complete levels on Device A, sign in on Device B, verify progress syncs.                                  | Use two test devices. Complete levels on one, verify sync on other.                   |
| 4.13| Analytics events fire correctly for: `level_start`, `level_complete`, `level_fail`, `iap_purchase`, `ad_watched`.     | Play through a session. Check Firebase Analytics debug view for event logs.           |
| 4.14| Full session loop test: play 5 levels → earn Essence → upgrade cauldron → complete daily brew → maintain 3-win streak → see all systems interact. | GP plays a complete session. No system fails silently or contradicts another.   |

### Evaluation Protocol
1. GP performs the full session loop test (4.14) on a fresh install.
2. UD-1 verifies Firebase integration (4.11–4.13) using Firebase console debug view.
3. QA verifies monetization flows (4.6–4.10) on both platforms using sandbox/test environments.
4. GP cross-checks currency values against economy spreadsheet.

### Go/No-Go Decision
- **Pass:** All 14 criteria met. Proceed to Week 9.
- **Fail — monetization issues (4.6–4.10):** 3-day fix window. These are integration issues with ad/IAP SDKs. Do not let SDK bugs block content work — content team (GP, ART) continues into Week 9 tasks while devs fix.
- **Fail — economy issues (4.5 off by > 20%, or 4.14 reveals broken economy flow):** GP re-tunes economy spreadsheet. Devs update values via Remote Config (no code change needed if Remote Config is working). 2-day retune cycle.
- **Fail — backend issues (4.11–4.13):** UD-1 has 3-day fix window. Cloud save and analytics are important but not launch-blocking for soft launch if they can be fixed in Week 9.

---

## Gate 5 — Soft Launch Readiness (End of Week 10)

### Statement
> Build is stable, performant, and ready for soft launch.

### Pass Criteria

| #    | Criterion                                                                                                   | How to Verify                                                                   |
| ---- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| 5.1  | Zero P0 bugs (crash, data loss, progress corruption).                                                       | QA bug tracker shows 0 open P0 issues.                                          |
| 5.2  | ≤ 3 open P1 bugs, each with documented workaround.                                                         | QA bug tracker review. Each P1 has workaround noted.                            |
| 5.3  | All 40 levels are completable. Each level tested by ≥ 2 people.                                             | Playtest sign-off spreadsheet: level # × tester name × completed (Y/N).         |
| 5.4  | Tutorial completion rate ≥ 85% in internal testing (simulated by having 10+ people play from fresh install).| Track completions across team + friends/family testers.                          |
| 5.5  | 60 fps during normal gameplay on mid-tier devices. ≥ 30 fps on lowest-spec targets during cascades.         | Unity Profiler on 4 devices spanning spec range.                                |
| 5.6  | Cold start time < 3 s on mid-tier devices.                                                                  | Time from tap-to-first-frame on 4 devices. Average must be < 3 s.              |
| 5.7  | Install size < 100 MB (download size, not install footprint).                                               | Check IPA/APK/AAB size after build.                                             |
| 5.8  | Peak RAM < 200 MB on lowest-spec Android target.                                                            | Unity Profiler memory snapshot during peak gameplay (large cascade).             |
| 5.9  | Crash rate < 1% across all internal testing sessions (tracked via Firebase Crashlytics).                    | Crashlytics dashboard after ≥ 100 sessions of internal testing.                 |
| 5.10 | IAP works in production sandbox on both platforms.                                                           | Successful test purchase + restore on iOS TestFlight and Google internal track.  |
| 5.11 | Ads display correctly (rewarded + interstitial) on both platforms with real ad units (test mode off).        | QA verifies ad display on 2 devices per platform.                               |
| 5.12 | Cloud save round-trips successfully between 2 devices.                                                      | Sign in on Device A, play, sign in on Device B, verify sync.                    |
| 5.13 | App store assets complete: icon, 5 screenshots per platform, description, keywords, preview video.          | GP reviews all assets against App Store / Google Play requirements.              |
| 5.14 | Remote Config is live with tunable values: ad frequency, economy multipliers, feature flags.                | Change a value in Firebase RC console. Verify client picks it up within 1 hour.  |
| 5.15 | Analytics events verified in Firebase console for all critical events.                                      | Run a full session. Check Firebase DebugView for event completeness.             |

### Evaluation Protocol
1. QA presents bug tracker summary: P0/P1/P2 counts, closure rates.
2. UD-1 presents performance data: FPS, memory, startup time across device matrix.
3. GP presents playtest data: level completion rates, tutorial completion, qualitative feedback.
4. Team reviews app store assets together.
5. **All three decision-makers (GP, UD-1, UD-2) must agree on go/no-go.**

### Go/No-Go Decision
- **Pass:** All 15 criteria met. Submit to soft launch markets.
- **Fail — P0 bugs remain:** No launch until P0 count is zero. Max 3-day fix sprint. If P0s persist beyond 3 days, investigate systemic stability issues.
- **Fail — performance (5.5–5.8):** Implement quality tiers (reduce particles, lower resolution on low-end). 2-day fix window.
- **Fail — store assets (5.13):** ART has 2-day window to complete. Does not block build submission but blocks store listing going live.
- **Fail — monetization (5.10–5.11):** Critical for soft launch revenue data. Max 3-day fix. If IAP cannot be fixed, launch without IAP (ads only) to still gather retention data.

---

## Soft Launch Gate — Post-Launch Criteria

### Soft Launch Parameters
- **Markets:** Canada, Australia, New Zealand
- **Duration:** 4–8 weeks
- **UA spend:** $500–$1,000 (Meta Ads, Google UAC)
- **Sample size target:** ≥ 2,000 installs for statistically meaningful retention data

### Success Thresholds (Must Meet All)

| Metric                     | Target   | Minimum Acceptable | Kill Threshold |
| -------------------------- | -------- | ------------------- | -------------- |
| D1 Retention               | ≥ 45%    | ≥ 40%               | < 32%          |
| D7 Retention               | ≥ 18%    | ≥ 15%               | < 10%          |
| D30 Retention              | ≥ 8%     | ≥ 6%                | < 4%           |
| Tutorial Completion Rate   | ≥ 90%    | ≥ 85%               | < 70%          |
| Crash Rate                 | < 0.5%   | < 1%                | > 2%           |
| CPI (Meta, broad targeting)| < $1.50  | < $2.50             | > $4.00        |
| Avg Session Length          | ≥ 8 min  | ≥ 5 min             | < 3 min        |
| Sessions per Day (D1 users)| ≥ 2.0    | ≥ 1.5               | < 1.2          |

### Kill Criteria — When to Abandon

Abandon the project if **any two** of the following are true after 2 weeks of soft launch with ≥ 1,000 installs:

1. **D1 retention < 32%.** Players don't come back the next day. The core loop isn't compelling enough.
2. **D7 retention < 10%.** Even players who come back on day 1 churn by day 7. No long-term engagement hook.
3. **Tutorial completion < 70%.** Players can't figure out how to play. Fundamental UX/design failure.
4. **CPI > $4.00 on Meta (broad targeting).** The creative/concept doesn't appeal to a broad enough audience at a viable cost.

**Kill process:** GP presents data to team. 1-day discussion. If kill criteria are met, document learnings, archive the project, and move on. Do not spend additional weeks trying to "fix" a fundamentally broken game.

### Post-Soft-Launch Decision Framework

#### Week 2 Check-in (≥ 1,000 installs)
- **Review:** D1 retention, tutorial completion, crash rate, CPI.
- **Decision:** Continue / Kill / Pivot.
- **Actions if continuing:** Identify top 3 drop-off points in the funnel. Plan targeted fixes.

#### Week 4 Check-in (≥ 2,000 installs)
- **Review:** D1, D7 retention, session metrics, IAP conversion rate, ARPDAU.
- **Decision:** Continue with current approach / Major pivot (new onboarding, different economy) / Kill.
- **Actions if continuing:** Begin planning content updates (new levels, events). Estimate D30 from D7 cohort curve.

#### Week 8 Check-in (≥ 3,000 installs, D30 data available)
- **Review:** D30 retention, LTV projection, LTV/CPI ratio (must be > 1.5× for viable scaling).
- **Decision:** Scale UA / Iterate more / Kill.
- **Green light to scale:** D1 ≥ 40%, D7 ≥ 15%, D30 ≥ 6%, LTV/CPI > 2×, crash rate < 0.5%.
- **Actions if scaling:** Increase UA budget 5×. Begin development of next content batch (levels 41–80). Plan global launch.

---

## Appendix: Gate Checklist Template

Use this template for each gate review meeting:

```
GATE [#] REVIEW — [Date]
========================

Attendees: [names]

CRITERIA REVIEW:
[ ] Criterion X.1 — PASS / FAIL (notes: ___)
[ ] Criterion X.2 — PASS / FAIL (notes: ___)
...

OVERALL: PASS / FAIL

If FAIL:
- Failed criteria: ___
- Root cause: ___
- Rework plan: ___
- Rework deadline: ___
- Re-evaluation date: ___

Decision maker sign-off: ___
```
