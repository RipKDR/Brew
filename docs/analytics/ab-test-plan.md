# Brew — A/B Test Plan (First 90 Days)

> Owner: Product Lead + Analytics Lead
> Platform: Firebase Remote Config + custom assignment service
> Assignment: device-level, sticky, mutually exclusive within the same system (tests targeting different systems may overlap)
> Last updated: 2026-04-09

---

## General Testing Rules

1. **Minimum sample:** Unless noted otherwise, each variant needs ≥ 2,000 unique users who reach the test exposure point.
2. **Statistical significance:** p < 0.05, two-tailed. Use sequential testing (alpha-spending) to allow early stopping if effect is large.
3. **Guardrail metrics:** Every test monitors D1 retention, D7 retention, and ARPDAU as guardrails. If any guardrail degrades by > 2 pp (retention) or > 15% (ARPDAU) with p < 0.10, pause the test.
4. **Rollout:** Winning variants roll out to 100% via Remote Config; losing variants are archived with a post-mortem.
5. **Interaction policy:** Tests 1 and 2 may run concurrently (different systems). Tests 3, 4, 5, 6 are staggered to avoid confounding.

---

## Test 1: Tutorial Length

| Field | Detail |
|-------|--------|
| **ID** | `AB_TUTORIAL_LENGTH` |
| **Launch window** | Soft-launch Day 1 – Day 21 |
| **Hypothesis** | A shorter 3-level tutorial reduces early drop-off and improves D1 retention by ≥ 3 pp without harming tutorial comprehension (measured by level 5 completion rate). |
| **Exposure point** | First app launch (new installs only) |

### Variants

| Variant | Description |
|---------|-------------|
| **Control (A)** | 5-level tutorial: (1) Tap a cluster, (2) Fuse into orb, (3) Chain two orbs, (4) Complete a recipe, (5) Use a booster. Full hand-holding with coach marks on every step. |
| **Treatment (B)** | 3-level tutorial: (1) Tap a cluster → auto-fuse, (2) Chain orbs → brew potion, (3) Free play with subtle hints. Booster tutorial deferred to level 6 tooltip. |

### Metrics

| Role | Metric | How measured |
|------|--------|-------------|
| **Primary** | D1 retention | `users_active_on(install_date+1) / installs` per variant |
| **Secondary** | Tutorial completion rate | `tutorial_complete / tutorial_start` |
| **Secondary** | Level 5 completion rate | `level_complete(5) / installs` — tests if shorter tutorial causes confusion later |
| **Secondary** | Time to complete tutorial (seconds) | `tutorial_complete.timestamp − tutorial_start.timestamp` |
| **Guardrail** | D7 retention | Must not degrade > 2 pp |
| **Guardrail** | Booster usage rate (levels 6–10) | Checks if deferred booster tutorial reduces booster awareness |

### Sample Size & Duration

- MDE (minimum detectable effect): 3 pp on D1 (40% → 43%).
- Required per variant: ~2,500 new installs.
- Total: 5,000 new installs.
- Estimated duration: 14 days of enrollment + 7 days of D7 observation = **21 days**.

### Decision Criteria

| Outcome | Action |
|---------|--------|
| B improves D1 ≥ 3 pp, level 5 completion ≥ control | Ship B. Retire 5-level tutorial. |
| B improves D1 but level 5 completion drops > 5 pp | Iterate: try 4-level variant with booster step retained. |
| No significant difference | Keep A (5-level). Document and move on. |
| B degrades D1 or D7 | Kill B immediately. |

---

## Test 2: Move Budget Generosity

| Field | Detail |
|-------|--------|
| **ID** | `AB_MOVE_BUDGET` |
| **Launch window** | Soft-launch Day 1 – Day 28 |
| **Hypothesis** | Increasing move budgets by 20% across all levels improves level completion rate and D7 retention by reducing frustration, but may reduce rewarded-ad opt-in rate (fewer fail-screen ads). Net effect on ARPDAU is the key trade-off. |
| **Exposure point** | First `level_start` (new installs only, persists for lifetime) |

### Variants

| Variant | Description |
|---------|-------------|
| **Control (A)** | Baseline move budgets as designed (level-specific, typically 15–30 moves). |
| **Treatment (B)** | All move budgets multiplied by 1.2 (rounded up). E.g., 25 → 30, 15 → 18. |

### Metrics

| Role | Metric | How measured |
|------|--------|-------------|
| **Primary** | Level completion rate (aggregate, levels 1–40) | `level_complete / level_start` |
| **Secondary** | D7 retention | Cohort retention |
| **Secondary** | Level retry rate after fail | `retry_within_5min / level_fail` |
| **Secondary** | Rewarded ad opt-in rate (fail_extra_moves) | `rewarded_ad_watched(fail) / rewarded_ad_offered(fail)` |
| **Secondary** | ARPDAU | Combined ad + IAP revenue / DAU |
| **Guardrail** | Session length | Must not drop below 4 min median |

### Sample Size & Duration

- MDE: 5 pp on completion rate (70% → 75%).
- Required per variant: ~2,000 new installs reaching level 5+.
- Total: ~5,000 installs (accounting for funnel drop before level 5).
- Estimated duration: 14 days enrollment + 14 days D14 observation = **28 days**.

### Decision Criteria

| Outcome | Action |
|---------|--------|
| B improves completion ≥ 5 pp AND ARPDAU neutral or positive | Ship B. |
| B improves completion but ARPDAU drops > 10% | Reject B. The ad-revenue loss outweighs engagement gains. Consider a smaller +10% variant. |
| B improves completion but retry rate drops > 10 pp | Reject B. Levels are now too easy; engagement signal is weakened. |
| No significant difference in completion | Keep A. Moves budget is already well-tuned. |

---

## Test 3: Rewarded Ad Reward Amount

| Field | Detail |
|-------|--------|
| **ID** | `AB_AD_REWARD_MOVES` |
| **Launch window** | Day 15 – Day 42 (starts after Test 1 enrolls enough users) |
| **Hypothesis** | Offering +5 extra moves (vs +3) on the fail screen increases rewarded-ad opt-in rate by ≥ 10 pp, which increases ad revenue per DAU despite lowering subsequent fail frequency. |
| **Exposure point** | First `level_fail` event |

### Variants

| Variant | Description |
|---------|-------------|
| **Control (A)** | Fail screen offers rewarded ad for +3 extra moves. |
| **Treatment (B)** | Fail screen offers rewarded ad for +5 extra moves. |

### Metrics

| Role | Metric | How measured |
|------|--------|-------------|
| **Primary** | Rewarded ad opt-in rate (fail placement) | `rewarded_ad_watched(fail) / rewarded_ad_offered(fail)` |
| **Secondary** | Level completion rate after watching ad | `level_complete_after_ad / rewarded_ad_watched(fail)` |
| **Secondary** | Rewarded ad views per DAU | `rewarded_ad_watched / DAU` |
| **Secondary** | Ad revenue per DAU | `ad_revenue / DAU` |
| **Guardrail** | Level retry rate | Must not drop (more continues = fewer retries, but net play should stay up) |
| **Guardrail** | D7 retention | Must not degrade |

### Sample Size & Duration

- MDE: 10 pp on opt-in rate (60% → 70%).
- Required per variant: ~1,500 users reaching first fail.
- Total: ~4,000 installs (accounting for users who don't fail before churning).
- Estimated duration: 14 days enrollment + 14 days observation = **28 days**.

### Decision Criteria

| Outcome | Action |
|---------|--------|
| B opt-in ≥ 10 pp higher AND ad rev/DAU increases | Ship B. |
| B opt-in higher but ad rev/DAU flat or negative (fewer subsequent fails = fewer total ad views) | Evaluate LTV impact. If D7 improves ≥ 1 pp, ship B (retention value offsets). Otherwise keep A. |
| No significant difference | Keep A. The marginal moves don't change the decision. |

---

## Test 4: Win Streak Multiplier

| Field | Detail |
|-------|--------|
| **ID** | `AB_STREAK_MULTIPLIER` |
| **Launch window** | Day 22 – Day 52 |
| **Hypothesis** | A steeper streak multiplier (1x/2x/3x/4x/5x) increases session length and D7 retention by making streaks more rewarding, encouraging players to push for "just one more level." |
| **Exposure point** | First session with a streak ≥ 2 |

### Variants

| Variant | Description |
|---------|-------------|
| **Control (A)** | Streak multiplier: 1×, 1.5×, 2×, 2.5×, 3× (for streaks 1–5+). Applied to Essence earned on level completion. |
| **Treatment (B)** | Streak multiplier: 1×, 2×, 3×, 4×, 5× (for streaks 1–5+). Same application. |

### Metrics

| Role | Metric | How measured |
|------|--------|-------------|
| **Primary** | Average session length (minutes) | `MEDIAN(session_duration)` |
| **Secondary** | D7 retention | Cohort retention |
| **Secondary** | Levels per session | `level_complete_count / session_count` |
| **Secondary** | Streak average length | `AVG(streak_count)` for streaks ≥ 2 |
| **Guardrail** | Essence balance inflation | P90 balance must not exceed 2× control's P90 within 14 days |
| **Guardrail** | Payer conversion | Must not drop (generous free Essence could suppress IAP) |

### Sample Size & Duration

- MDE: 1.0 min increase in median session length (6 min → 7 min).
- Required per variant: ~2,000 users with ≥ 1 streak event.
- Total: ~5,500 installs (streak exposure starts ~level 3).
- Estimated duration: 14 days enrollment + 14 days observation = **28 days** + 2 days buffer = **30 days**.

### Decision Criteria

| Outcome | Action |
|---------|--------|
| B increases session length ≥ 1 min AND D7 ≥ control | Ship B. |
| B increases session length but Essence inflation triggers guardrail | Adjust economy sinks (raise workshop costs) then re-test. |
| B increases session length but payer conversion drops > 0.5 pp | Reject B. Free currency is cannibalizing IAP. |
| No significant difference | Keep A. The psychological difference is insufficient. |

---

## Test 5: Workshop Upgrade Costs

| Field | Detail |
|-------|--------|
| **ID** | `AB_WORKSHOP_COST` |
| **Launch window** | Day 30 – Day 65 |
| **Hypothesis** | Reducing workshop upgrade costs by 20% increases workshop progression speed, driving higher D30 retention (more invested players) and higher payer conversion (players who engage with workshop are more likely to spend on Gems to accelerate). |
| **Exposure point** | First `workshop_upgrade` event |

### Variants

| Variant | Description |
|---------|-------------|
| **Control (A)** | Baseline workshop costs (e.g., Tier 1: 300 Essence, Tier 2: 500 Essence, Tier 3: 800 Essence + 50 Gems, etc.) |
| **Treatment (B)** | All workshop costs reduced by 20% (rounded to nearest 10). Tier 1: 240 Essence, Tier 2: 400 Essence, Tier 3: 640 Essence + 40 Gems, etc. |

### Metrics

| Role | Metric | How measured |
|------|--------|-------------|
| **Primary** | D30 retention | Cohort retention at day 30 |
| **Secondary** | Payer conversion rate | `payers / users` within 30 days |
| **Secondary** | Workshop stage distribution at D14 and D30 | `PERCENTILE(workshop_stage)` |
| **Secondary** | Gem spend rate | `currency_spend(gem) / DAU` |
| **Guardrail** | Essence balance distribution | Faster spending should not deplete balances below playability |
| **Guardrail** | Level completion rate | Must remain stable (workshop benefits should not trivialize levels) |

### Sample Size & Duration

- MDE: 2 pp on D30 retention (7% → 9%).
- Required per variant: ~3,000 installs (need to wait 30 days for D30 data).
- Total: ~7,500 installs (accounting for churn before workshop unlock ~level 8).
- Estimated duration: 21 days enrollment + 30 days D30 observation = **~51 days** (with some overlap). Runs through Day 65 from soft-launch start.

### Decision Criteria

| Outcome | Action |
|---------|--------|
| B improves D30 ≥ 2 pp | Ship B. |
| B improves D30 but payer conversion drops | Mixed signal. Check ARPPU: if spenders spend more despite fewer conversions, ship B. Otherwise keep A. |
| B has no effect on D30 but workshop engagement up | Ship B anyway if engagement increase is ≥ 20% more upgrades. Workshop engagement has intrinsic retention value. |
| No significant difference | Keep A. Costs are appropriately tuned. |

---

## Test 6: Interstitial Frequency

| Field | Detail |
|-------|--------|
| **ID** | `AB_INTERSTITIAL_FREQ` |
| **Launch window** | Day 30 – Day 58 |
| **Hypothesis** | Showing interstitials every 5 levels (vs every 3) improves D7 retention by ≥ 2 pp due to reduced ad fatigue, but reduces ad revenue. The retention gain must yield higher LTV to justify the revenue loss. |
| **Exposure point** | First interstitial trigger point (after level 3 or level 5, depending on variant) |

### Variants

| Variant | Description |
|---------|-------------|
| **Control (A)** | Interstitial shown after every 3rd level completion (levels 3, 6, 9, 12 …). Cap: 5 per session. |
| **Treatment (B)** | Interstitial shown after every 5th level completion (levels 5, 10, 15, 20 …). Cap: 3 per session. |

### Metrics

| Role | Metric | How measured |
|------|--------|-------------|
| **Primary** | D7 retention | Cohort retention |
| **Secondary** | Ad revenue per DAU | `interstitial_revenue / DAU` |
| **Secondary** | Session count per day | `sessions / DAU` |
| **Secondary** | Session length | `MEDIAN(session_duration)` |
| **Secondary** | Levels per session | `level_complete / sessions` |
| **Guardrail** | Total revenue (ARPDAU) | Must not drop > 10% |

### Sample Size & Duration

- MDE: 2 pp on D7 retention (15% → 17%).
- Required per variant: ~3,000 users reaching the interstitial exposure point.
- Total: ~7,000 installs.
- Estimated duration: 14 days enrollment + 14 days D7+D14 observation = **28 days**.

### Decision Criteria

| Outcome | Action |
|---------|--------|
| B improves D7 ≥ 2 pp AND ARPDAU drop ≤ 5% | Ship B. Retention value exceeds short-term ad loss. |
| B improves D7 ≥ 2 pp but ARPDAU drops 5–15% | Run LTV projection: if 30-day LTV of B > A, ship B. Otherwise keep A. |
| B improves D7 but ARPDAU drops > 15% | Reject B. Revenue impact is too severe. |
| No significant D7 difference | Keep A (higher revenue, same retention). |

---

## Test Calendar Overview

```
Day:  1         14        21        28        35        42        52        58        65        90
      |----------|---------|---------|---------|---------|---------|---------|---------|---------|
T1    [===== enroll =====][= observe =]
T2    [=========== enroll ===========][======== observe ========]
T3                 [=========== enroll ===========][======== observe ========]
T4                              [============ enroll ============][========= observe =========]
T5                                        [=============== enroll ===============][==== observe (D30) ====]
T6                                        [=========== enroll ===========][=== observe ===]
```

Concurrent tests at any point: max 3 (by design, covering different game systems).

---

## Post-Test Artifacts

For every completed test, produce:
1. **Results summary** (1 page): variant performance table, statistical significance, recommendation.
2. **Detailed analysis** (in analytics notebook): segment breakdowns (platform, country, install cohort), interaction effects with other active tests.
3. **Decision log entry** in `docs/analytics/ab-test-log.md`: date, test ID, decision, rationale, who approved.
4. **Remote Config update** PR: ship winner or archive.
