# Regression Test Checklist — Brew

> Version 1.0 · Run before every release build
> Estimated execution time: 2–3 hours (one QA tester)
> Last updated: 2026-04-09

---

## How to Use

1. Create a copy of this checklist for each release build.
2. Record build number, date, tester name, and device at the top.
3. Execute each check in order. Mark **PASS**, **FAIL**, or **SKIP** (with reason).
4. Any FAIL in Sections 1–4 is a **release blocker** (S0/S1). Escalate immediately.
5. FAILs in Sections 5–7 are evaluated case-by-case with the lead.

**Build:** _____________ **Date:** _____________ **Tester:** _____________ **Device:** _____________

---

## 1. Core Mechanic (15 checks)


| #    | Check                             | Steps                                                                                                | Expected Result                                                                                                                                             | Status |
| ---- | --------------------------------- | ---------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| R-01 | Valid cluster tap                 | Start any level. Tap a cluster of 3+ same-color tokens.                                              | Tokens fuse toward tapped cell. Orb appears at tapped position. Move counter decrements by 1.                                                               | ☐      |
| R-02 | Invalid cluster tap               | Tap a group of only 2 same-color tokens.                                                             | Shake animation plays. No fusion. Move counter unchanged.                                                                                                   | ☐      |
| R-03 | Orb tap rejected                  | Tap directly on an existing orb.                                                                     | Subtle shake. No action. Move counter unchanged.                                                                                                            | ☐      |
| R-04 | Chain fusion triggers             | Create an orb adjacent to another same-color orb.                                                    | Orbs merge into survivor (bottom-most, then left-most). Combined `token_count` is correct. No move consumed.                                                | ☐      |
| R-05 | Brew at threshold                 | Create or chain-merge an orb to `token_count ≥ 6`.                                                   | Brew animation plays (burst → transform → fly-to-vial). If target color, recipe counter decrements.                                                         | ☐      |
| R-06 | Orb below threshold does not brew | Create orb with `token_count = 4`. No same-color orb adjacent.                                       | Orb sits on board in `ORB_IDLE`. No brew animation.                                                                                                         | ☐      |
| R-07 | Gravity fills gaps                | Fuse a cluster in the middle of the board.                                                           | Tokens above the gap drop down. Empty cells filled from top with new tokens. No floating tokens.                                                            | ☐      |
| R-08 | Cascade auto-fuse                 | After gravity + refill, a new cluster of 3+ forms.                                                   | Auto-fuse triggers without consuming a move. Orb placed at bottom-most cell of the cascade cluster.                                                         | ☐      |
| R-09 | Win condition                     | Complete all recipe targets.                                                                         | Resolution finishes. Win screen displays: score, star rating, Essence earned, streak updated.                                                               | ☐      |
| R-10 | Lose condition                    | Exhaust all moves without completing recipe (non-near-miss).                                         | Fail screen displays. No rewards granted. "Try Again" button functional.                                                                                    | ☐      |
| R-11 | Near-miss recovery                | Fail a level where near-miss heuristic returns true.                                                 | Recovery offer modal appears (watch ad or spend Gems for +5 moves). Accepting resumes gameplay.                                                             | ☐      |
| R-12 | Deadlock reshuffle                | Reach a board state with no valid clusters (use Shake booster to help reproduce, or use debug seed). | "Reshuffling..." overlay appears. Board randomizes tokens (orbs stay). At least one valid cluster exists after. No move consumed.                           | ☐      |
| R-13 | Shake booster                     | Activate Shake booster during a level.                                                               | Board tokens randomize. Orbs unchanged. At least one valid cluster present. No cascade triggers. No move consumed. Booster inventory decrements.            | ☐      |
| R-14 | Catalyst booster                  | Activate Catalyst on a single token.                                                                 | Token converts to orb with `token_count = cluster_size`. Resolution begins (chain check, gravity, cascade). Move counter decrements by 1. Booster consumed. | ☐      |
| R-15 | Extra Moves booster               | Fail a level → activate Extra Moves.                                                                 | `move_counter += 5`. Fail screen dismisses. Gameplay resumes. Booster marked as used for this attempt.                                                      | ☐      |


---

## 2. Economy (10 checks)


| #    | Check                           | Steps                                                                                   | Expected Result                                                                                                 | Status |
| ---- | ------------------------------- | --------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- | ------ |
| R-16 | Essence earned on win           | Complete a level with ★★ rating.                                                        | Earn 50 Essence (base). Verify balance incremented on results screen and in profile.                            | ☐      |
| R-17 | Streak multiplier on Essence    | Win 3 levels in a row (streak = 3, multiplier = 1.5×). Complete another level with ★★★. | Earn `80 × 1.5 = 120` Essence. Breakdown shown on results screen.                                               | ☐      |
| R-18 | Workshop upgrade purchase       | Navigate to Workshop. Tap an available upgrade. Confirm purchase.                       | Essence deducted. Upgrade animation plays. Workshop visual updates. `upgrade_level` increments.                 | ☐      |
| R-19 | Insufficient funds — Essence    | With balance below upgrade cost, tap the upgrade.                                       | "Not enough Essence" message. Purchase button disabled or grayed. Balance unchanged.                            | ☐      |
| R-20 | Booster purchase with Gems      | Go to booster shop. Buy Catalyst for 10 Gems.                                           | Gem balance decrements by 10. Catalyst count increments by 1.                                                   | ☐      |
| R-21 | Insufficient funds — Gems       | With < 10 Gems, attempt to buy 10-Gem booster.                                          | Purchase rejected. Prompt to buy more Gems (IAP screen) or dismiss.                                             | ☐      |
| R-22 | Currency never negative         | Attempt rapid repeated purchases (tap buy button 5× fast).                              | Only affordable purchases complete. Balance never drops below 0.                                                | ☐      |
| R-23 | IAP — Gem pack purchase         | Initiate purchase of "$4.99 = 300 Gems" via App Store / Play Store.                     | Payment flow completes. 300 Gems credited. Receipt validated server-side. `iap_purchase` event logged.          | ☐      |
| R-24 | IAP — purchase restore          | Uninstall and reinstall app. Tap "Restore Purchases."                                   | Previously purchased No-Ads Pass (or other non-consumable) restored. Consumable Gem balance synced from server. | ☐      |
| R-25 | IAP — failed/cancelled purchase | Start IAP flow → cancel before confirming.                                              | No Gems credited. No error crash. App returns to normal state.                                                  | ☐      |


---

## 3. Meta Systems (10 checks)


| #    | Check                              | Steps                                                               | Expected Result                                                                               | Status |
| ---- | ---------------------------------- | ------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | ------ |
| R-26 | Potion Shelf — new potion          | Complete a new level for the first time. Open Potion Shelf.         | New potion bottle appears in correct slot. Slot transitions from silhouette to filled bottle. | ☐      |
| R-27 | Potion Shelf — replay no duplicate | Replay and complete an already-cleared level. Open shelf.           | Potion count unchanged. No duplicate entry.                                                   | ☐      |
| R-28 | Shelf milestone reward             | Reach a milestone threshold (e.g., 25 potions).                     | Celebration animation plays. Essence + Gems credited. Player title unlocked (check profile).  | ☐      |
| R-29 | Workshop sequential enforcement    | Attempt to purchase upgrade #4 when #3 is not yet bought.           | Purchase unavailable. Only the next sequential upgrade is interactive.                        | ☐      |
| R-30 | Win streak increment               | Win 2 levels consecutively. Check streak counter.                   | Streak = 2. Bronze flame badge visible. Multiplier = 1.25×.                                   | ☐      |
| R-31 | Win streak reset                   | Fail a level without using protection. Check streak.                | Streak = 0. Flame badge gone. Multiplier = 1.0×.                                              | ☐      |
| R-32 | Streak protection — ad             | Fail at streak ≥ 2. Choose "Watch ad" protection.                   | Ad plays. Streak preserved. Retry level with same streak value.                               | ☐      |
| R-33 | Streak protection — Gems           | Fail at streak ≥ 2. Choose Gem protection (5 Gems).                 | 5 Gems deducted. Streak preserved.                                                            | ☐      |
| R-34 | Daily Brew — complete and lock     | Complete the Daily Brew. Attempt to re-enter.                       | Entry locked. "Completed" badge shown. Timer displays time until next Daily Brew.             | ☐      |
| R-35 | Daily Brew — streak                | Complete Daily Brew on 3 consecutive days (use clock mock or wait). | `daily_brew_streak = 3`. 1.25× Essence multiplier applied. +5 bonus Gems on day 3.            | ☐      |


---

## 4. Monetization (8 checks)


| #    | Check                                | Steps                                                          | Expected Result                                                                                    | Status |
| ---- | ------------------------------------ | -------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- | ------ |
| R-36 | Rewarded ad — level fail Extra Moves | Fail a level. Tap "Watch Ad for +5 Moves."                     | Ad loads and plays. On completion: +5 moves granted, gameplay resumes, `ad_rewarded` event fired.  | ☐      |
| R-37 | Rewarded ad — Daily Brew doubler     | Complete Daily Brew. Tap "Watch Ad to Double Reward."          | Ad plays. Essence doubles (100 → 200). `ad_rewarded` event with `reward_type = daily_brew_double`. | ☐      |
| R-38 | Rewarded ad — streak protection      | Fail at streak ≥ 2. Choose "Watch Ad" to protect.              | Ad plays. Streak preserved on successful completion.                                               | ☐      |
| R-39 | Ad failure — network error           | Disable network → trigger rewarded ad.                         | Graceful error: "Ad not available. Try again later." No crash. Fallback options still accessible.  | ☐      |
| R-40 | No-Ads Pass active                   | Purchase No-Ads Pass. Play 5 levels.                           | No interstitial ads appear between levels. Rewarded ads still available as opt-in.                 | ☐      |
| R-41 | IAP — Starter Bundle                 | Purchase Starter Bundle ($1.99).                               | 50 Gems + 500 Essence + 3 Catalysts credited. Bundle removed from store (one-time purchase).       | ☐      |
| R-42 | IAP — Gem pack                       | Purchase any Gem pack.                                         | Correct Gem amount credited. Transaction receipt validated. Balance updates in real-time.          | ☐      |
| R-43 | Streak protection daily limit        | Use streak protection 2× in one day. Fail again at streak ≥ 2. | Third protection unavailable. Only "Dismiss" option shown. Streak resets.                          | ☐      |


---

## 5. Analytics (5 checks)


| #    | Check                       | Steps                                                                       | Expected Result                                                                                                                          | Status |
| ---- | --------------------------- | --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| R-44 | Level complete event        | Complete any level. Check Firebase Analytics debug view.                    | `level_complete` event fires with params: `level_id`, `star_rating`, `score`, `moves_remaining`, `duration_seconds`.                     | ☐      |
| R-45 | Level fail event            | Fail any level. Check debug view.                                           | `level_fail` event fires with: `level_id`, `moves_used`, `recipe_remaining`, `near_miss` (bool).                                         | ☐      |
| R-46 | Currency transaction events | Earn Essence (win level) and spend Essence (buy booster). Check debug view. | `essence_earned` and `essence_spent` events fire with correct `amount`, `source`/`item_id`, `balance_after`.                             | ☐      |
| R-47 | IAP revenue tracking        | Complete a real or sandbox IAP. Check debug view.                           | `iap_purchase` event fires with `product_id`, `price_usd`, `currency`, `receipt_valid` (bool). Revenue attributed in Firebase dashboard. | ☐      |
| R-48 | Session tracking            | Launch app, play 3 levels, close app. Check debug view.                     | `session_start` fires on launch. `session_end` fires on close with `session_duration_seconds`, `levels_played`.                          | ☐      |


---

## 6. Performance (5 checks)


| #    | Check                          | Steps                                                                                 | Expected Result                                       | Status |
| ---- | ------------------------------ | ------------------------------------------------------------------------------------- | ----------------------------------------------------- | ------ |
| R-49 | Cold start time                | Force-stop app. Launch from home screen. Time to first interactive frame.             | < 5 seconds on P0 devices. < 3 seconds on P1+.        | ☐      |
| R-50 | Frame rate — cascade           | Trigger a 3+ wave cascade (use debug seed if needed). Monitor FPS.                    | ≥ 55 fps sustained on P0 devices. No visible stutter. | ☐      |
| R-51 | Memory — 10-level session      | Play 10 levels. Monitor memory in profiler at levels 1, 5, 10.                        | Memory < 250 MB. No upward trend (leak).              | ☐      |
| R-52 | Battery drain — 15 min session | Play continuously for 15 minutes on a P0 device. Record battery %.                    | < 5% drain at mid brightness.                         | ☐      |
| R-53 | Loading time between levels    | Complete a level, measure time from results screen dismiss to next level interactive. | < 2 seconds. Loading indicator shown if > 1 second.   | ☐      |


---

## 7. Platform-Specific

### 7.1 iOS (5 checks)


| #    | Check                         | Steps                                           | Expected Result                                                                                                                           | Status |
| ---- | ----------------------------- | ----------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| R-54 | Safe area — iPhone with notch | Play a level on iPhone 11 or later.             | No UI elements hidden behind notch or Dynamic Island. Home indicator does not overlap bottom HUD.                                         | ☐      |
| R-55 | App Tracking Transparency     | Fresh install on iOS 14.5+. Launch app.         | ATT prompt appears before any ad network initialization. Selecting "Ask App Not to Track" does not break ads (contextual ads still load). | ☐      |
| R-56 | Background/resume — iOS       | Start a level. Press Home. Wait 30 sec. Reopen. | Game resumes at exact board state. No duplicate resolution. Audio resumes.                                                                | ☐      |
| R-57 | StoreKit IAP flow             | Initiate Gem purchase in sandbox.               | Payment sheet appears. Transaction completes. Gems credited. Works on both StoreKit 1 and StoreKit 2 paths.                               | ☐      |
| R-58 | iPad layout                   | Open app on iPad Air.                           | Portrait layout. Grid centered. No stretched sprites. HUD elements properly positioned. Touch targets meet 44 pt minimum.                 | ☐      |


### 7.2 Android (5 checks)


| #    | Check                         | Steps                                                        | Expected Result                                                                                                            | Status |
| ---- | ----------------------------- | ------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------- | ------ |
| R-59 | Safe area — punch-hole camera | Play a level on Samsung Galaxy S21.                          | No UI elements behind punch-hole cutout. Status bar area respected.                                                        | ☐      |
| R-60 | Navigation bar compatibility  | Test with 3-button nav and gesture nav (change in Settings). | Game area does not overlap nav bar in either mode. Back gesture does not exit mid-level (shows pause menu).                | ☐      |
| R-61 | Background/resume — Android   | Start a level. Press Home. Wait 30 sec. Reopen.              | Game resumes. If system killed the process (low memory), app restores from saved state on relaunch.                        | ☐      |
| R-62 | Google Play Billing           | Initiate Gem purchase in test mode.                          | Billing flow completes. Gems credited. Purchase acknowledged (prevents phantom purchases).                                 | ☐      |
| R-63 | Permissions                   | Fresh install. Play through first 10 levels.                 | No unnecessary permission prompts (no camera, microphone, location). Only network + ad identifiers (handled via manifest). | ☐      |


---

## Summary


| Section              | Checks              | Estimated Time |
| -------------------- | ------------------- | -------------- |
| 1. Core Mechanic     | 15                  | 35–45 min      |
| 2. Economy           | 10                  | 20–25 min      |
| 3. Meta Systems      | 10                  | 20–25 min      |
| 4. Monetization      | 8                   | 20–25 min      |
| 5. Analytics         | 5                   | 10–15 min      |
| 6. Performance       | 5                   | 15–20 min      |
| 7. Platform-Specific | 10 (5 per platform) | 20–25 min      |
| **Total**            | **63**              | **~2.5 hours** |


---

## Sign-Off


| Role             | Name | Date | Verdict            |
| ---------------- | ---- | ---- | ------------------ |
| QA Tester        |      |      | PASS / FAIL        |
| QA Lead          |      |      | APPROVED / BLOCKED |
| Engineering Lead |      |      | APPROVED / BLOCKED |


**Release decision:** ☐ SHIP    ☐ HOLD (list blockers below)

**Blocker notes:**

---

*End of Regression Test Checklist.*