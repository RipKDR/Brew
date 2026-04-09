# Test Plan — Brew

> Version 1.0 · Pre-production · Last updated 2026-04-09
> Audience: Engineering, QA

---

## 1. Unit Test Coverage (C# / Unity Test Framework)

All unit tests run headless via Unity Test Runner in EditMode unless noted otherwise. Tests use deterministic seeded RNG (`seed = 42` unless a test requires randomness). Grid helpers create boards from string layouts for readability:

```
// "E" = Ember, "F" = Frost, "V" = Vine, "S" = Sun, "X" = Shadow, "." = Empty, "e3" = Ember orb (token_count 3)
var grid = GridBuilder.FromLayout(7, 9, @"
  E E F F V V S
  E E F S V X X
  ...
");
```

---

### 1.1 Core Mechanic Tests

#### 1.1.1 Cluster Detection

| # | Test | Setup | Expected |
|---|------|-------|----------|
| CD-01 | Valid cluster — exactly 3 | 3 same-color tokens in an L-shape | `find_cluster` returns 3 cells, cluster is valid |
| CD-02 | Valid cluster — large group | 8 connected same-color tokens | `find_cluster` returns all 8, cluster is valid |
| CD-03 | Invalid cluster — 2 adjacent | 2 adjacent same-color tokens, no third neighbor | Cluster size = 2, cluster is **not** valid |
| CD-04 | Invalid cluster — isolated token | Single token surrounded by other colors | Cluster size = 1, not valid |
| CD-05 | Diagonal not counted | Two same-color tokens at `(2,2)` and `(3,3)` only | Each forms cluster of size 1; no valid cluster |
| CD-06 | Edge cluster | 3+ same-color tokens along left column `(0, y)` | Cluster detected correctly, no out-of-bounds access |
| CD-07 | Corner cluster | 3 same-color tokens meeting at `(0,0)` corner | Cluster valid, adjacency limited to 2 directions |
| CD-08 | Full-row cluster | 7 same-color tokens spanning entire row | Single cluster of size 7 |
| CD-09 | Two separate same-color clusters | Two disconnected groups of 3, same color | `find_all_valid_clusters` returns 2 distinct clusters |
| CD-10 | Mixed-color adjacency | Alternating colors in checkerboard | No valid clusters found |
| CD-11 | Cluster does not include orbs | 3 tokens adjacent to same-color orb | Cluster contains only the 3 tokens, not the orb |
| CD-12 | Board cluster map cache | Modify one cell after cache built | Cache invalidated, recompute returns updated clusters |

#### 1.1.2 Fusion

| # | Test | Setup | Expected |
|---|------|-------|----------|
| FU-01 | Tokens merge to tapped position | Tap center of a 4-token cluster | Orb created at tapped cell; other 3 cells become `EMPTY` |
| FU-02 | Orb has correct token_count | Fuse a cluster of 5 | `orb.token_count == 5` |
| FU-03 | Orb has correct color | Fuse Frost cluster | `orb.color == FROST` |
| FU-04 | Move decremented on valid tap | `move_counter = 15`, fuse valid cluster | `move_counter == 14` |
| FU-05 | Move not decremented on invalid tap | Tap a 2-token cluster | `move_counter` unchanged |
| FU-06 | Move not decremented on orb tap | Tap cell containing an orb | `move_counter` unchanged |
| FU-07 | Move not decremented on empty tap | Tap an empty cell | `move_counter` unchanged |

#### 1.1.3 Chain Fusion

| # | Test | Setup | Expected |
|---|------|-------|----------|
| CH-01 | Two adjacent same-color orbs chain | Orb A (token_count 3) adjacent to Orb B (token_count 2), same color | Survivor orb at bottom-most cell with `token_count == 5` |
| CH-02 | Chain does not trigger for different colors | Ember orb adjacent to Frost orb | No chain fusion, both orbs remain |
| CH-03 | Three orbs in L-shape merge to one | 3 same-color orbs in L arrangement | Single survivor with summed `token_count` |
| CH-04 | Survivor selection — bottom-most wins | Orbs at `(3,2)` and `(3,5)` | Survivor at `(3,5)` (higher row index) |
| CH-05 | Survivor selection — tie-break left-most | Orbs at `(4,7)` and `(2,7)` | Survivor at `(2,7)` (lower column index) |
| CH-06 | Multi-step chain resolves in order | Orb A chains into Orb B, result lands adjacent to Orb C (same color) | Two chain steps fire sequentially; `chain_step_counter == 2` |
| CH-07 | Chain does not consume a move | Player fuses cluster, resulting orb chains | `move_counter` decremented exactly once (the player tap) |

#### 1.1.4 Brew Detection

| # | Test | Setup | Expected |
|---|------|-------|----------|
| BR-01 | Orb at threshold brews | Orb with `token_count = 6` | `BREWING` state triggered |
| BR-02 | Orb above threshold brews | Orb with `token_count = 9` | `BREWING` state triggered |
| BR-03 | Orb below threshold does not brew | Orb with `token_count = 5` | No brew; orb enters `ORB_IDLE` |
| BR-04 | Chain pushes orb to threshold | Orb A (3) chains with Orb B (3) → merged = 6 | Brew triggered on survivor |
| BR-05 | Direct 6-token cluster brews immediately | Fuse cluster of 6 tokens | Orb created with `token_count = 6`, immediately brews |
| BR-06 | Target-color brew awards 200 points | Brew matching recipe color | `resolution_score` includes +200 |
| BR-07 | Non-target brew awards 100 points | Brew of color not in recipe | `resolution_score` includes +100 |

#### 1.1.5 Gravity

| # | Test | Setup | Expected |
|---|------|-------|----------|
| GR-01 | Single gap filled | Remove one token from column mid-height | Token above drops one row; no floating tokens |
| GR-02 | Multiple gaps compacted | Remove 3 non-adjacent tokens from one column | All remaining tokens settle to bottom, gaps at top |
| GR-03 | Orbs fall under gravity | Orb above empty cells | Orb drops same as tokens |
| GR-04 | Columns independent | Gap in column 2 only | Only column 2 tokens move; other columns untouched |
| GR-05 | Refill from top | 3 empty cells at top after gravity | 3 new tokens spawned in rows 0–2 |
| GR-06 | No floating tokens | Random board after gravity + refill | Assert every column: no `EMPTY` cell exists below a non-empty cell |

#### 1.1.6 Cascade

| # | Test | Setup | Expected |
|---|------|-------|----------|
| CA-01 | Cascade triggers on new cluster | After gravity, 3+ same-color tokens become adjacent | Auto-fuse triggers without consuming a move |
| CA-02 | Multi-wave cascade | Cascade wave 1 creates gaps → gravity → new cluster forms | Second cascade wave fires; `cascade_wave_counter == 2` |
| CA-03 | Cascade orb placement | Auto-fused cascade cluster | Orb placed at bottom-most, then left-most cell of cluster |
| CA-04 | Cascade does not consume moves | 3 cascade waves complete | `move_counter` unchanged from pre-cascade value |

#### 1.1.7 Scoring

| # | Test | Setup | Expected |
|---|------|-------|----------|
| SC-01 | Base fusion score | Fuse cluster of 4 | `fusion_points = 4 × 10 = 40` |
| SC-02 | Chain multiplier applied | 1 chain step, then fusion of 3 | Second fusion: `30 × 1.5 = 45` |
| SC-03 | Cascade multiplier applied | 1 cascade wave, then auto-fuse of 5 | `50 × 1.5 = 75` |
| SC-04 | Multipliers stack multiplicatively | 2 chain steps + 1 cascade wave, fusion of 3 | `30 × 2.0 × 1.5 = 90` |
| SC-05 | Remaining moves bonus on win | Win with 4 moves remaining | `level_score` includes `4 × 50 = 200` bonus |
| SC-06 | No remaining moves bonus on lose | Lose with 0 moves | No bonus applied |
| SC-07 | Star thresholds — 1 star | Score at minimum pass | `star_rating == 1` |
| SC-08 | Star thresholds — 3 stars | Score exceeds 3-star threshold | `star_rating == 3` |

#### 1.1.8 Deadlock Detection

| # | Test | Setup | Expected |
|---|------|-------|----------|
| DL-01 | Deadlocked board detected | Board with no cluster ≥ 3, moves remaining | `is_deadlocked() == true` |
| DL-02 | Non-deadlocked board passes | Board with at least one valid cluster | `is_deadlocked() == false` |
| DL-03 | Reshuffle produces valid board | Force deadlock, then reshuffle | Post-reshuffle board has ≥ 1 valid cluster |
| DL-04 | Reshuffle preserves orb positions | Board with 2 orbs, deadlocked tokens | After reshuffle, orbs remain at original cells |

#### 1.1.9 Move Counting

| # | Test | Setup | Expected |
|---|------|-------|----------|
| MV-01 | Player fusion costs 1 move | Valid tap | `move_counter -= 1` |
| MV-02 | Catalyst costs 1 move | Activate Catalyst on a token | `move_counter -= 1` |
| MV-03 | Auto-chain is free | Chain fusion triggered by player move | No additional move cost |
| MV-04 | Cascade is free | Gravity triggers cascade cluster | No additional move cost |
| MV-05 | Shake booster is free | Activate Shake | `move_counter` unchanged |
| MV-06 | Invalid tap is free | Tap on 2-token cluster | `move_counter` unchanged |
| MV-07 | Reshuffle is free | Deadlock triggers reshuffle | `move_counter` unchanged |

#### 1.1.10 Recipe Tracking

| # | Test | Setup | Expected |
|---|------|-------|----------|
| RT-01 | Target brew decrements recipe counter | Recipe needs 2 Ember brews; brew 1 Ember | Ember recipe counter: 2 → 1 |
| RT-02 | Non-target brew does not decrement | Recipe needs Ember; brew Frost | Ember counter unchanged, Frost counter unchanged |
| RT-03 | Recipe counter does not go negative | Brew 3 Ember when recipe needs 2 | Counter stays at 0, third brew awards non-target score |
| RT-04 | Multiple recipe colors tracked independently | Recipe: 2 Ember + 1 Frost; brew 1 Ember | Ember: 2→1, Frost remains 1 |

#### 1.1.11 Win / Lose Conditions

| # | Test | Setup | Expected |
|---|------|-------|----------|
| WL-01 | All targets met = WIN | Brew final required potion | Game state → `WIN` after resolution completes |
| WL-02 | Win mid-resolution | Final brew happens in cascade wave 2, wave 3 still pending | Resolution completes fully, then WIN |
| WL-03 | Moves exhausted, recipe incomplete = LOSE | `move_counter == 0`, recipe not done | Game state → `FAIL_CHECK` |
| WL-04 | Last move completes recipe = WIN | Final move fuses the cluster that brews the last potion | WIN, not LOSE (resolution completes before fail check) |
| WL-05 | Near-miss triggers recovery offer | Fail + near-miss heuristic returns true | `OFFER_RECOVERY` state entered |

#### 1.1.12 Booster Effects

| # | Test | Setup | Expected |
|---|------|-------|----------|
| BO-01 | Shake randomizes token positions | Pre-shake: board state A | Post-shake: token colors redistributed, orbs untouched |
| BO-02 | Shake guarantees valid cluster | Worst-case board | Post-shake has ≥ 1 valid cluster |
| BO-03 | Shake does not trigger cascades | Shake creates new cluster of 4 | No auto-fuse; player must tap |
| BO-04 | Catalyst converts token to orb | Target cell has Ember token, cluster size 1 | Ember orb with `token_count = 1` at target cell |
| BO-05 | Catalyst on larger cluster | Target cell in a 4-token cluster | Orb with `token_count = 4`; resolution begins |
| BO-06 | Catalyst rejected on orb cell | Target cell is an orb | No-op, booster not consumed |
| BO-07 | Extra Moves adds 5 | `move_counter = 0`, activate Extra Moves | `move_counter == 5` |
| BO-08 | Extra Moves limited to once per attempt | Activate Extra Moves, then try again | Second activation rejected |

---

### 1.2 Economy Tests

| # | Test | Setup | Expected |
|---|------|-------|----------|
| EC-01 | Correct Essence per star | Complete level with ★★ rating, streak = 1 | Earn 50 Essence |
| EC-02 | Streak multiplier applied | Complete level with ★★★, streak = 5 (2.0×) | Earn `80 × 2.0 = 160` Essence |
| EC-03 | Streak multiplier capped at 3.0× | Streak = 15 | Multiplier = 3.0× (not higher) |
| EC-04 | Workshop upgrade deducts correctly | Balance = 1000, upgrade cost = 600 | Post-purchase balance = 400 |
| EC-05 | Insufficient Essence rejected | Balance = 300, upgrade cost = 600 | Purchase rejected, balance unchanged |
| EC-06 | Gem IAP grants correct amount | Purchase "$4.99 = 300 Gems" pack | `gem_balance += 300` |
| EC-07 | Booster purchase deducts Gems | Buy Catalyst for 10 Gems | `gem_balance -= 10`, Catalyst inventory +1 |
| EC-08 | Currency never negative — Essence | Attempt to spend more Essence than balance | Transaction rejected |
| EC-09 | Currency never negative — Gems | Attempt to spend more Gems than balance | Transaction rejected |
| EC-10 | Analytics event on earn | Complete level, earn Essence | `essence_earned` analytics event fired with correct amount |
| EC-11 | Analytics event on spend | Purchase workshop upgrade | `essence_spent` analytics event fired with item ID and cost |
| EC-12 | Gem transaction logged | Buy booster with Gems | `gem_spent` event fired with `item_id`, `amount`, `balance_after` |

---

### 1.3 Meta Tests

#### 1.3.1 Potion Collection

| # | Test | Setup | Expected |
|---|------|-------|----------|
| PC-01 | First brew adds potion to shelf | Complete level 12 for the first time | Potion slot 12 unlocked on shelf |
| PC-02 | Replay does not duplicate | Replay and complete level 12 again | Potion count unchanged; slot already filled |
| PC-03 | Shelf milestone triggers at threshold | Reach 25 potions collected | 25% milestone fires: 500 Essence + 25 Gems awarded |
| PC-04 | Event potion counts toward milestones | Complete weekly event, earn exclusive potion | Shelf total increments, milestone progress updated |

#### 1.3.2 Workshop

| # | Test | Setup | Expected |
|---|------|-------|----------|
| WK-01 | Upgrades apply in sequence | Attempt upgrade #3 before completing #2 | Rejected; sequential order enforced |
| WK-02 | Upgrade costs match config | Query cost for upgrade #5 | Returns 600 Essence |
| WK-03 | Visual state updates | Purchase upgrade #2 (Light the Hearth) | Workshop `current_upgrade_level == 2` |
| WK-04 | Final upgrade (12/12) completes workshop | Purchase upgrade #12 | `workshop_complete == true` |

#### 1.3.3 Win Streak

| # | Test | Setup | Expected |
|---|------|-------|----------|
| WS-01 | Streak increments on win | Win 3 levels consecutively | `win_streak == 3` |
| WS-02 | Streak resets on fail | Streak = 5, fail a level, decline protection | `win_streak == 0` |
| WS-03 | Multiplier caps at 3.0× | Streak = 10+ | `streak_multiplier == 3.0` |
| WS-04 | Streak protection preserves streak | Streak = 4, fail, use Gem protection (5 Gems) | `win_streak == 4`, `gem_balance -= 5` |
| WS-05 | Protection limited to 2×/day | Use protection twice, fail again | Third protection unavailable |

#### 1.3.4 Daily Brew

| # | Test | Setup | Expected |
|---|------|-------|----------|
| DB-01 | Available once per day | Complete Daily Brew, try to enter again | Entry locked; shows "Completed" badge |
| DB-02 | Resets at 00:00 UTC | Advance mock clock past midnight | New Daily Brew available |
| DB-03 | Correct reward amounts | Complete Monday (Easy) Daily Brew | 100 Essence + 5 Gems |
| DB-04 | Daily streak tracks consecutive days | Complete Daily Brew 3 days running | `daily_brew_streak == 3`, 1.25× Essence multiplier + 5 bonus Gems |
| DB-05 | Streak resets on missed day | Streak = 4, skip a day | `daily_brew_streak == 0` |

---

## 2. Integration Tests

Integration tests run in PlayMode with mocked Firebase services and a local save file.

### 2.1 Full Level Playthrough

| # | Test | Procedure | Verification |
|---|------|-----------|-------------|
| IT-01 | Win path | Load level 5 JSON config → inject board seed → simulate taps to complete all recipe targets → resolution completes | Game state = `WIN`; star rating calculated; Essence + Gems added to balance; potion slot filled |
| IT-02 | Lose path | Load level 5 → simulate suboptimal taps until `move_counter == 0` without completing recipe | Game state = `LOSE`; no rewards; near-miss check executed |
| IT-03 | Level config integrity | Load every level JSON (1–100) | All parse successfully; `grid_width`/`grid_height` within range; `move_limit > 0`; recipe colors exist in `token_types`; `spawn_weights` sum to 1.0 |

### 2.2 Economy Flow

| # | Test | Procedure | Verification |
|---|------|-----------|-------------|
| IT-04 | Play → earn → spend loop | Complete level (earn 80 Essence) → purchase Shake booster (50 Essence) → verify balance | Balance = initial + 80 - 50; booster inventory incremented |
| IT-05 | Workshop upgrade flow | Earn enough Essence → purchase upgrade → check workshop state → check balance | `upgrade_level` incremented, Essence deducted, analytics event fired |
| IT-06 | Insufficient funds flow | Set balance to 30 → attempt upgrade costing 50 | Purchase rejected; UI shows "Not enough Essence"; balance unchanged |

### 2.3 Ad Flow

| # | Test | Procedure | Verification |
|---|------|-----------|-------------|
| IT-07 | Rewarded ad → Extra Moves | Fail level → mock rewarded ad callback (success) | `move_counter += 5`; fail screen dismissed; gameplay resumes; `ad_rewarded` analytics event fired with `reward_type = extra_moves` |
| IT-08 | Rewarded ad — user cancels | Fail level → mock rewarded ad callback (cancelled) | No extra moves; fail screen remains; `ad_cancelled` event fired |
| IT-09 | Daily Brew ad doubler | Complete Daily Brew → watch rewarded ad | Essence reward doubled (100 → 200); `ad_rewarded` event with `reward_type = daily_brew_double` |

### 2.4 Save / Load

| # | Test | Procedure | Verification |
|---|------|-----------|-------------|
| IT-10 | Mid-level save/restore | Start level 8, make 3 moves → serialize game state → clear memory → deserialize | Board state matches: same grid, same `move_counter`, same recipe progress, same score |
| IT-11 | Cross-session persistence | Complete level, earn rewards, upgrade workshop → "quit" → reload from save | Currency balances correct; workshop level correct; potion shelf accurate; win streak intact |
| IT-12 | Corrupted save recovery | Load intentionally corrupted save file | Game resets to last known good checkpoint; no crash; error logged to analytics |

### 2.5 Remote Config

| # | Test | Procedure | Verification |
|---|------|-----------|-------------|
| IT-13 | Config value propagation | Set `booster_shake_essence_cost = 75` in mock Remote Config → client fetches | Shake booster costs 75 Essence in UI and transaction logic |
| IT-14 | Config fallback | Remote Config fetch fails (timeout) | Client uses cached/default values; no crash; `remote_config_fetch_failed` event fired |
| IT-15 | A/B test variant applied | Assign player to variant B (`star_1_essence = 40`) | Level completion awards 40 Essence for ★ (not default 30) |

---

## 3. Manual Test Procedures

### 3.1 First-Time User Experience (FTUE)

**Precondition:** Fresh install on test device. No prior save data. No account linked.

| Step | Action | Verify |
|------|--------|--------|
| 1 | Launch app | Splash screen → loading → tutorial begins within 5 seconds |
| 2 | Tutorial level 1 | Hand prompt points to valid cluster. Tap fuses correctly. Instructional overlay explains cluster + fusion |
| 3 | Tutorial — orb chain | Second tap positions orb adjacent to auto-placed orb. Chain fusion demonstrated |
| 4 | Tutorial — brew | Chain reaches threshold. Brew animation plays. Recipe vial fills. "You brewed a potion!" overlay |
| 5 | Tutorial — win | Complete recipe. Win screen shows: score, star rating, Essence earned |
| 6 | Tutorial — workshop | Redirect to workshop. "Sweep the Floor" upgrade highlighted. Player purchases (free/gifted) |
| 7 | Level 2–5 | Play through with minimal guidance overlays. No blockers, no crashes |
| 8 | Post-level-5 | Daily Brew unlocked notification. Workshop accessible from main menu. Potion Shelf shows 5 filled slots |

**Pass criteria:** All 8 steps complete without error, crash, or confusing UX gap.

### 3.2 Session Loop (10-Level Stress Test)

**Precondition:** Player account at level 20+. Moderate currency balance.

| Step | Action | Verify |
|------|--------|--------|
| 1 | Play 10 consecutive levels (mix of new + replay) | No crashes or ANRs across the session |
| 2 | Monitor win streak | Streak counter increments/resets correctly; multiplier badge updates |
| 3 | Earn and spend | Buy 1 booster mid-session, use it | Transaction correct, booster consumed |
| 4 | Complete Daily Brew if available | Reward granted, streak updated |
| 5 | Check Potion Shelf | New potions appear; shelf scrolls smoothly |
| 6 | Check Workshop | Upgrade available if balance sufficient; visual state current |
| 7 | Session duration | Total ~20–30 minutes | No memory leak (monitor via profiler); frame rate stable |

### 3.3 Edge Cases

| Scenario | Steps | Expected |
|----------|-------|----------|
| **Airplane mode gameplay** | Enable airplane mode → launch app → play cached levels → complete level | Gameplay works offline; rewards cached locally; sync when connection restored |
| **Mid-level background/resume** | Start level → press Home → wait 30 seconds → reopen app | Level state restored exactly; no duplicate moves; timer (if any) paused |
| **Background for 5+ minutes** | Start level → background for 5 minutes → resume | Session continues or graceful reconnect; no crash |
| **Low memory warning** | Simulate low-memory condition (Xcode/Android Studio tools) | App reduces texture cache; no crash; gameplay continues |
| **Interrupting phone call** | Mid-level, receive phone call → answer → hang up → return | Game paused during call; resume to intact board state |
| **Notification during gameplay** | Receive push notification banner mid-level | Game does not pause (notification does not obscure critical UI); taps register correctly |
| **Rapid tap spam** | Tap valid cluster 10× in rapid succession | Only first tap registers; subsequent taps ignored during resolution |
| **Double-tap same cell** | Tap valid cell, immediately tap again | Single fusion; no duplicate move decrement |
| **Orientation lock** | Rotate device to landscape | Game remains portrait-locked; no layout break |
| **Time zone change** | Complete Daily Brew → change device timezone → check availability | Daily Brew still locked (server uses UTC, not device time) |
| **Force kill and relaunch** | Mid-level force kill (swipe away) → relaunch | Game offers to resume in-progress level or start fresh |

---

## 4. Test Infrastructure

### 4.1 CI Pipeline

- **On every PR:** Unit tests (EditMode) run via Unity Test Runner CLI on build server. Must pass 100%.
- **Nightly:** Full integration test suite (PlayMode) against staging Firebase project. Report emailed to team.
- **Pre-release:** Manual test procedures (Section 3) executed on P0 devices from the device matrix.

### 4.2 Test Data

- **Level configs:** Committed to `Assets/Resources/Levels/`. Validated by IT-03.
- **Mock Firebase:** Local emulator for Auth, Firestore, Remote Config, Analytics.
- **Seed library:** Curated set of board seeds that reproduce specific scenarios (deadlock, multi-cascade, near-miss).

### 4.3 Bug Severity Definitions

| Severity | Definition | Release Impact |
|----------|-----------|----------------|
| S0 — Blocker | Crash, data loss, currency exploit, infinite loop | Cannot ship. Fix immediately. |
| S1 — Critical | Incorrect rewards, broken progression, soft lock | Cannot ship. Fix within 24 hours. |
| S2 — Major | Visual glitch during core mechanic, wrong animation, UI overlap | Ship if isolated; fix in next patch. |
| S3 — Minor | Cosmetic issues, typos, minor animation jank | Ship; fix when convenient. |

---

*End of Test Plan.*
