---
description: Tune Brew economy values and validate balance. Use when adjusting currency rewards, costs, progression pacing, or monetization parameters.
---

# Skill: Brew Economy Tuning

## When to Use

- Adjusting Essence or Gem earn rates
- Changing booster costs or workshop upgrade prices
- Evaluating whether a proposed change breaks progression pacing
- Adding a new earn source or spend sink
- Validating monetization changes against ethical guardrails
- Modeling player economy at different engagement levels

## Economy Model Summary

### Essence (Soft Currency)

**Sources:**
| Source | Amount | Frequency |
|--------|--------|-----------|
| Level completion (1★) | 30 base × streak multiplier | Per level |
| Level completion (2★) | 50 base × streak multiplier | Per level |
| Level completion (3★) | 80 base × streak multiplier | Per level |
| Daily Brew | 100 (200 with ad doubler) | Daily |
| Daily Brew streak bonus | ×1.25 at 3-day, ×1.5 at 5-day, ×2.0 at 7-day | Daily (escalating) |
| Weekly Event levels | 50 each (7 levels = 350) | Weekly |
| Weekly Event 3-star bonus | 25 per level (175 max) | Weekly |
| Weekly Event completion bonus | 500 + 250 (all 3-star) | Weekly |
| Shelf milestones | 500 / 1,000 / 2,000 / 5,000 | One-time (4 tiers) |

**Win Streak Multipliers:**
| Streak | Multiplier |
|--------|-----------|
| 1 | 1.0× |
| 2 | 1.25× |
| 3 | 1.5× |
| 5 | 2.0× |
| 8 | 2.5× |
| 10+ | 3.0× (cap) |

**Sinks:**
| Sink | Cost |
|------|------|
| Workshop upgrades (12 tiers) | 50 → 12,000 (total: 36,350) |
| Shake booster | 50 Essence |
| Catalyst booster | 100 Essence |

### Gems (Premium Currency)

**Free Sources:**
| Source | Gems | Frequency |
|--------|------|-----------|
| Daily Brew | 5 | Daily |
| Daily Brew 3-day streak | +5 | Every 3 days |
| Daily Brew 5-day streak | +10 | Every 5 days |
| Daily Brew 7-day streak | +15 | Every 7 days |
| Shelf milestones | 25 / 50 / 100 / 250 | One-time (4 tiers) |
| Weekly Event (all 7 cleared) | 30 | Weekly |
| Weekly Event (all 7 at 3-star) | +20 | Weekly |
| IAP purchases | 50–1,500 per SKU | On purchase |

**Free Gem income estimate:** ~300–400 Gems/month for a highly engaged non-payer.

**Sinks:**
| Sink | Cost |
|------|------|
| Shake booster | 5 Gems |
| Catalyst booster | 10 Gems |
| Extra Moves +5 | 10 Gems |
| Streak protection | 5 Gems |

## Daily Economy Models

Use these models to project the impact of economy changes. Adjust the "base" values to match your proposed change, then compare the resulting daily/monthly totals and workshop completion timeline.

### Casual Player (5 levels/day)

Assumptions: avg 2.2 stars/level, avg 1.3× streak multiplier, Daily Brew completed.

| Source | Essence/Day | Gems/Day |
|--------|-------------|----------|
| 5 levels × 55 avg base × 1.3 streak | ~357 | 0 |
| Daily Brew | 100 | 5 |
| **Daily total** | **~457** | **~5** |
| **Monthly total (×30)** | **~13,700** | **~150** |

### Regular Player (10 levels/day)

Assumptions: avg 2.2 stars, avg 1.5× streak, Daily Brew + weekly event.

| Source | Essence/Day | Gems/Day |
|--------|-------------|----------|
| 10 levels × 55 avg base × 1.5 streak | ~825 | 0 |
| Daily Brew | 100 | 5 |
| Weekly Event (prorated) | ~182 | ~7 |
| **Daily total** | **~1,107** | **~12** |
| **Monthly total** | **~33,200** | **~360** |

### Hardcore Player (20 levels/day)

Assumptions: avg 2.5 stars, avg 2.0× streak, Daily Brew with ad doubler, weekly event.

| Source | Essence/Day | Gems/Day |
|--------|-------------|----------|
| 20 levels × 65 avg base × 2.0 streak | ~2,600 | 0 |
| Daily Brew (with doubler) | 200 | 5 |
| Weekly Event (prorated) | ~182 | ~7 |
| **Daily total** | **~2,982** | **~12** |
| **Monthly total** | **~89,460** | **~360** |

## Workshop Completion Guardrail

**Target: 30–45 days for the regular player (10 levels/day).**

Total workshop cost: **36,350 Essence**.

To calculate days-to-complete after an economy change:

```
net_daily_essence = daily_essence_earned - daily_essence_spent_on_boosters
days_to_complete = 36,350 / net_daily_essence
```

| Player Type | Net Daily Essence | Days to Complete |
|-------------|-------------------|-----------------|
| Casual (5/day) | ~407 | ~89 days |
| Regular (10/day) | ~1,007 | ~36 days |
| Hardcore (20/day) | ~2,882 | ~13 days |

**If your change moves the regular player outside 30–45 days, reconsider.** Faster than 30 days means players run out of content too quickly. Slower than 45 days means the workshop feels unachievable.

## How to Check If a Cost Change Breaks Pacing

1. Identify which earn rates or costs are changing.
2. Recalculate the daily model for each player tier (casual/regular/hardcore) using the new values.
3. Compute `days_to_complete_workshop` for each tier.
4. Verify the regular player is still within 30–45 days.
5. Verify the casual player completes within ~90 days (3 months is acceptable for casual).
6. Check that free Gem income still supports ~1–2 booster purchases per week for non-payers.

### Red flags:
- Regular player workshop completion > 50 days → earn rates too low or costs too high
- Regular player workshop completion < 25 days → earn rates too high (content drought)
- Free monthly Gem income < 200 → non-payers will feel starved
- Daily booster spending > 40% of daily income → boosters are too expensive relative to earnings

## Validating Monetization Changes Against Ethical Guardrails

Before shipping any monetization change, verify against these hard rules from `docs/economy/monetization-plan.md` §7:

- [ ] **No manufactured impossible levels.** Every level clearable without boosters by average-skill player ≥ 70% of the time.
- [ ] **No fake timers.** All countdowns reflect real deadlines.
- [ ] **No hidden mechanics.** All rules and odds are deterministic and documented.
- [ ] **No loot boxes or gacha.** All purchases deliver exactly what is shown.
- [ ] **No pay-to-win.** Boosters provide convenience, not exclusive power.
- [ ] **No forced ads.** Interstitials skippable after 5s. Rewarded ads are opt-in.
- [ ] **No dark patterns in purchase UI.** Close button is visible (44×44 pt minimum), always present, never delayed. No guilt modals.
- [ ] **Ad frequency cap enforced.** Max 5 rewarded/day. Max 1 interstitial per 3 level completions.
- [ ] **No spend gates on content.** All levels, Daily Brew, and events playable without spending.
- [ ] **Transparent Gem value.** Shop shows Gem price and USD equivalent.

### Additional self-audit:
- Can a non-payer complete 100% of Workshop and Potion Shelf through play alone?
- Does the change increase involuntary ad views?
- Would you be comfortable if a journalist described this mechanic in an article?

## Inflation Monitoring Thresholds

If these server-side metrics are breached, the economy needs rebalancing:

| Metric | Threshold | Action |
|--------|-----------|--------|
| Median Essence balance (D30+ players) | > 10,000 | Add Essence sink or increase booster consumption incentive |
| Median Gem balance (non-payers, D30+) | > 200 | Tighten free Gem earn rate |
| Workshop completion at D30 | > 60% of active players | Costs too low — add prestige tiers |
| Workshop completion at D45 | < 30% of active players | Costs too high — increase earn rates or add catch-up |

## Tuning Process

1. **Document the change** in `docs/economy/economy-model.md` before implementing.
2. **Model the impact** using the formulas above.
3. **Implement** via RemoteConfig parameter changes (no code change needed for value-only tweaks).
4. **Observe** for 7 days before making another adjustment.
5. **Modify at most 2–3 knobs per tuning pass** to isolate the effect.
6. **Never nerf rewards retroactively** for existing players. Apply only to new cohorts or new content.

## Key RemoteConfig Parameters

| Parameter | Default | Controls |
|-----------|---------|----------|
| `essence_per_star_1` / `_2` / `_3` | 50 / 100 / 200 | Base Essence earn rate |
| `gems_daily_brew` | 5 | Daily free Gem income |
| `gems_streak_day_3` / `_7` | 10 / 25 | Streak Gem rewards |
| `booster_cost_extra_moves` | 100 Essence | Booster Essence sink |
| `booster_gem_cost_extra_moves` | 10 Gems | Booster Gem sink |
| `interstitial_cooldown_seconds` | 120 | Ad pacing |
| `max_interstitials_per_session` | 5 | Ad cap |
| `rewarded_extra_moves_count` | 5 | Rewarded ad value |

Full parameter table: `docs/technical/data-model.md` §8.

## Reference

- Full economy model: `docs/economy/economy-model.md`
- Monetization plan and ethical guardrails: `docs/economy/monetization-plan.md`
- RemoteConfig schema: `docs/technical/data-model.md` §8
- KPI targets and thresholds: `docs/analytics/kpi-framework.md`
