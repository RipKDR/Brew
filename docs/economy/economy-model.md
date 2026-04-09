# Economy Model — Brew

> Version 0.1 · MVP Scope · Last updated 2026-04-09

---

## 1. Currency Overview

Brew uses a strict two-currency economy. No additional tokens, tickets, or event currencies exist.

| Currency | Type | Primary Sources | Primary Sinks |
|----------|------|----------------|---------------|
| **Essence** | Soft (freely earned) | Level completion, Daily Brew, Weekly Event, Shelf milestones | Workshop upgrades, booster purchases |
| **Gems** | Premium (scarce) | IAP, Daily Brew, Shelf milestones, Weekly Event completion | Boosters (premium price), Extra Moves on fail, streak protection, cosmetic themes (future) |

---

## 2. Currency Flow Diagrams

### 2.1 Essence Sources & Sinks

```
SOURCES                                         SINKS
─────────────────────                           ─────────────────────
Level Completion (base)    ──┐                ┌── Workshop Upgrades (12 tiers, 36,350 total)
  × Star Rating            │                │
  × Win Streak Multiplier  ├──► ESSENCE ◄───┤
Daily Brew Reward          │    BALANCE     ├── Shake Booster (50 each)
Daily Brew Streak Bonus    │                ├── Catalyst Booster (100 each)
Weekly Event Level Rewards ─┤                │
Shelf Milestone Rewards    ─┘                └── (Future: cosmetic consumables)
```

### 2.2 Gem Sources & Sinks

```
SOURCES                                         SINKS
─────────────────────                           ─────────────────────
IAP Purchases             ──┐                ┌── Shake Booster (5 each)
Daily Brew Reward (5/day)   ├──► GEM    ◄───┤── Catalyst Booster (10 each)
Daily Brew Streak Bonus     │   BALANCE     ├── Extra Moves +5 (10 each)
Shelf Milestones (4 tiers) ─┤                ├── Streak Protection (5 each)
Weekly Event Completion    ─┘                └── (Future: workshop cosmetic themes)
```

---

## 3. Earning Rates

### 3.1 Essence Per Level (Main Campaign)

Base Essence earned depends on star rating:

| Star Rating | Base Essence |
|-------------|-------------|
| ★ (complete) | 30 |
| ★★ | 50 |
| ★★★ | 80 |

The Win Streak multiplier is applied after the base:

| Streak | Multiplier | ★ | ★★ | ★★★ |
|--------|-----------|---|-----|------|
| 1 | 1.0× | 30 | 50 | 80 |
| 2 | 1.25× | 37 | 62 | 100 |
| 3 | 1.5× | 45 | 75 | 120 |
| 5 | 2.0× | 60 | 100 | 160 |
| 8 | 2.5× | 75 | 125 | 200 |
| 10+ | 3.0× | 90 | 150 | 240 |

Values are rounded to the nearest whole number at display.

### 3.2 Essence from Daily Brew

| Source | Essence |
|--------|---------|
| Daily Brew completion | 100 |
| 2× reward ad (optional) | +100 (total 200) |
| 3-day streak bonus | ×1.25 (125 base) |
| 5-day streak bonus | ×1.5 (150 base) |
| 7-day streak bonus | ×2.0 (200 base) |

### 3.3 Essence from Weekly Event

| Source | Essence |
|--------|---------|
| Per event level (×7) | 75 each = 525 |
| 3-star bonus per level (×7) | 25 each = 175 |
| All-7 completion bonus | 500 |
| All-7 3-star bonus | 250 |
| **Event max** | **1,450** |

### 3.4 Gem Earning (Free Sources Only)

| Source | Gems | Frequency |
|--------|------|-----------|
| Daily Brew (base) | 5 | Daily |
| Daily Brew 3-day streak | +5 | Every 3rd consecutive day |
| Daily Brew 5-day streak | +10 | Every 5th consecutive day |
| Daily Brew 7-day streak | +15 | Every 7th consecutive day |
| Shelf milestone — 25% | 25 | One-time |
| Shelf milestone — 50% | 50 | One-time |
| Shelf milestone — 75% | 100 | One-time |
| Shelf milestone — 100% | 250 | One-time |
| Weekly Event (all 7 cleared) | 30 | Weekly |
| Weekly Event (all 7 at 3-star) | +20 | Weekly |

**Monthly free Gem income (active player):** ~5/day × 30 = 150 (Daily Brew base) + ~30 (streak bonuses) + ~120–200 (events) + milestone one-offs = **~300–400 Gems/month** for a highly engaged non-payer.

---

## 4. Spending Rates

### 4.1 Workshop Upgrade Costs

| # | Upgrade | Cost | Cumulative |
|---|---------|------|-----------|
| 1 | Sweep the Floor | 50 | 50 |
| 2 | Light the Hearth | 100 | 150 |
| 3 | Repair the Workbench | 200 | 350 |
| 4 | Hang the Shelves | 400 | 750 |
| 5 | Install the Cauldron | 600 | 1,350 |
| 6 | Stock the Herb Rack | 1,000 | 2,350 |
| 7 | Place the Star Map | 1,500 | 3,850 |
| 8 | Add the Crystal Array | 2,500 | 6,350 |
| 9 | Build the Distillery | 4,000 | 10,350 |
| 10 | Enchant the Windows | 6,000 | 16,350 |
| 11 | Summon the Familiar | 8,000 | 24,350 |
| 12 | Master's Flourish | 12,000 | 36,350 |

### 4.2 Booster Costs

| Booster | Essence Price | Gem Price | Effect |
|---------|--------------|-----------|--------|
| Shake | 50 | 5 | Randomizes the board layout once |
| Catalyst | 100 | 10 | Converts a chosen token into any adjacent color |
| Extra Moves +5 | Free 1×/day (ad) | 10 | Adds 5 moves to the current level |

### 4.3 Other Gem Sinks

| Sink | Cost |
|------|------|
| Streak protection (per use) | 5 Gems |
| Extra Moves +5 (after free daily use) | 10 Gems |

---

## 5. Session Economics — Daily Earning Scenarios

Assumptions: average star rating of 2.2 stars/level, average Win Streak multiplier of 1.3× (casual players break streaks frequently), Daily Brew completed.

### 5.1 Casual Player — 5 Levels/Day

| Source | Essence | Gems |
|--------|---------|------|
| 5 levels × 55 base × 1.3 streak | 357 | 0 |
| Daily Brew | 100 | 5 |
| **Daily total** | **~457** | **~5** |
| **Weekly total (×7)** | **~3,200** | **~35** |
| **Monthly total (×30)** | **~13,700** | **~150** |

### 5.2 Regular Player — 10 Levels/Day

| Source | Essence | Gems |
|--------|---------|------|
| 10 levels × 55 base × 1.5 streak | 825 | 0 |
| Daily Brew | 100 | 5 |
| Weekly Event (prorated daily) | ~182 | ~7 |
| **Daily total** | **~1,107** | **~12** |
| **Weekly total** | **~7,750** | **~84** |
| **Monthly total** | **~33,200** | **~360** |

### 5.3 Hardcore Player — 20 Levels/Day

| Source | Essence | Gems |
|--------|---------|------|
| 20 levels × 65 base × 2.0 streak | 2,600 | 0 |
| Daily Brew (with ad doubler) | 200 | 5 |
| Weekly Event (prorated daily) | ~182 | ~7 |
| **Daily total** | **~2,982** | **~12** |
| **Weekly total** | **~20,875** | **~84** |
| **Monthly total** | **~89,460** | **~360** |

---

## 6. Economy Pacing

### 6.1 Workshop Completion Timeline

Total Workshop cost: **36,350 Essence.**

| Player Type | Daily Essence (Net after ~1 booster/day) | Days to Complete Workshop |
|-------------|----------------------------------------|--------------------------|
| Casual (5 levels/day) | ~407 net | ~89 days |
| Regular (10 levels/day) | ~1,007 net | ~36 days |
| Hardcore (20 levels/day) | ~2,882 net | ~13 days |

**Target: 30–45 days for the regular player.** The regular player at 10 levels/day completes in ~36 days, centered in the target range.

Casual players complete in ~3 months. This is acceptable — casual players have lower completion expectations, and workshop progress remains visible throughout.

### 6.2 Booster Spending Assumptions

Average booster consumption per session:

| Player Type | Boosters/Day | Essence Spent on Boosters/Day |
|-------------|-------------|------------------------------|
| Casual | 0.5–1 | ~50 |
| Regular | 1–2 | ~100 |
| Hardcore | 1–2 | ~100 |

These deductions are already factored into the "net" column in section 6.1.

---

## 7. Payer vs. Non-Payer Experience

### 7.1 Non-Payer Progression

- **Levels:** Unlimited access. No energy gates. A non-payer plays every level and completes the game identically to a payer.
- **Boosters:** Earned through Essence. A regular non-payer can afford ~1 booster per day without impacting workshop progress significantly.
- **Recovery:** On fail, the non-payer watches a rewarded ad for +5 moves (once per fail). This is free.
- **Workshop:** Completes in ~36 days at 10 levels/day. Fully achievable.
- **Shelf:** Same completion rate as payers. No potions are locked behind payment.
- **Gems:** Earns ~300–400/month from free sources. Enough for ~30–40 booster purchases or ~6–8 streak protections per month.

### 7.2 Payer Progression

- **Starter Bundle ($1.99):** 50 Gems + 500 Essence + 3 Catalysts provides a head start equivalent to ~1 day of regular play.
- **Gem Packs:** A $4.99 purchase (300 Gems) funds ~30 Catalysts or 60 Shakes — roughly 2–4 weeks of booster use.
- **No-Ads Pass ($4.99):** Removes interstitial ads. Rewarded ads remain optional. Quality-of-life improvement.
- **Workshop speed:** A payer who buys the Starter Bundle and one $4.99 Gem Pack can convert Gems → boosters, conserve Essence, and finish the workshop ~5–10 days faster than a non-payer.
- **The payer advantage is convenience and comfort, not content access.**

### 7.3 Experience Parity Principles

1. A non-payer must be able to reach 100% Potion Shelf and 100% Workshop completion through play alone.
2. No level should be designed to require paid boosters to clear at a reasonable skill level.
3. Paying accelerates progression but does not unlock exclusive gameplay content (exclusive potions from events are free to earn).
4. The Gem earn rate for non-payers must be high enough that they can afford streak protection and occasional boosters without feeling starved.

---

## 8. Inflation Prevention

### 8.1 Problem Statement

Once a player completes all Workshop upgrades and all Shelf milestones, Essence loses its purpose. Gems become less useful after all boosters are purchased. If unchecked, currency accumulates with nothing to spend it on, devaluing rewards.

### 8.2 Mitigation Strategies

| Strategy | Mechanism | Scope |
|----------|-----------|-------|
| **Workshop Season Themes** | After completing the base workshop, release cosmetic theme variants (Frost Workshop, Shadow Workshop) at equivalent or higher cost. New visual set, same structure. | Post-MVP, every 2–3 months |
| **Booster Consumption** | Boosters are consumable and single-use. An active player burns 50–200 Essence/day on boosters, creating a perpetual soft drain. | MVP |
| **Escalating Event Difficulty** | Later weekly events have harder level 6–7 configurations, encouraging booster use. | MVP, tuned over time |
| **Prestige Decorations** | After workshop 12/12, release "prestige" items: purely cosmetic, very expensive (5,000–15,000 Essence each). Animated workshop pets, rare potion shelf frames. | Post-MVP |
| **No Essence-to-Gem Conversion** | Players cannot convert Essence to Gems. This prevents Essence stockpiles from undermining Gem scarcity. | MVP |
| **Gem Sink Expansion** | Future releases add cosmetic purchases (workshop themes, potion bottle skins, board backgrounds) priced in Gems to maintain demand. | Post-MVP |

### 8.3 Monitoring Triggers

Implement server-side tracking for:

| Metric | Threshold | Action |
|--------|-----------|--------|
| Median Essence balance (D30+ players) | > 10,000 | Introduce new Essence sink or increase booster consumption incentive |
| Median Gem balance (non-payers, D30+) | > 200 | Review free Gem earn rate; consider tightening Daily Brew Gem rewards slightly |
| Workshop completion rate at D30 | > 60% of active players | Costs may be too low; consider adding prestige tiers sooner |
| Workshop completion rate at D45 | < 30% of active players | Costs may be too high; consider increasing Essence earn rates or adding a catch-up bonus |

---

## 9. Balancing Principles & Tuning Knobs

### 9.1 Core Principles

1. **Earn-before-spend.** The player should always earn enough to feel rewarded before encountering a meaningful spending decision.
2. **Visible progress every session.** Even a 5-minute session must move at least one progress bar (workshop, shelf, or streak).
3. **No dead zones.** At no point in the player lifecycle should all sinks be exhausted with no path to new content.
4. **Proportional reward.** Harder levels and higher star ratings must always yield meaningfully more Essence than easier alternatives. The reward curve must never flatten.
5. **Gem scarcity is sacred.** Gems must always feel valuable. Free Gem income should be enough to taste the premium experience (1–2 purchases/week) but not enough to replace IAP entirely.

### 9.2 Tuning Knobs

These are the parameters designers can adjust without code changes (server-configurable or in level data):

| Knob | Current Value | Range | Impact |
|------|--------------|-------|--------|
| Base Essence per star (★/★★/★★★) | 30 / 50 / 80 | 10–200 | Controls overall earn rate and workshop pacing |
| Win Streak multiplier cap | 3.0× | 1.5×–5.0× | Controls reward ceiling for skilled/persistent players |
| Win Streak thresholds | 1/2/3/5/8/10 | Any integers | Controls how quickly multiplier ramps |
| Workshop upgrade costs (per tier) | See table | ±50% each | Controls workshop completion timeline |
| Daily Brew Essence | 100 | 50–300 | Controls daily retention incentive strength |
| Daily Brew Gems | 5 | 1–15 | Controls free Gem flow rate |
| Event Essence (total) | 1,450 max | 500–2,500 | Controls weekly supplemental income |
| Event Gems (total) | 50 max | 20–100 | Controls weekly Gem income for engaged players |
| Booster Essence cost (Shake/Catalyst) | 50 / 100 | 25–200 | Controls Essence drain rate |
| Booster Gem cost (Shake/Catalyst) | 5 / 10 | 2–25 | Controls Gem drain rate |
| Streak protection Gem cost | 5 | 3–15 | Controls Gem drain for streak-invested players |
| Shelf milestone Essence rewards | 500/1K/2K/5K | ±50% | Controls one-time Essence injections |
| Shelf milestone Gem rewards | 25/50/100/250 | ±50% | Controls one-time Gem injections |

### 9.3 Tuning Process

1. **Soft launch (2–4 weeks):** Run with baseline values. Collect D1/D7/D14/D30 retention, median Essence balance, median Gem balance, workshop progress percentiles, booster purchase frequency, and IAP conversion.
2. **Analyze:** Compare actual earn/spend rates to modeled values. Identify if workshop pacing is too fast or slow.
3. **Adjust:** Modify no more than 2–3 knobs per tuning pass. Wait 7 days between passes to observe impact.
4. **Principle:** Never nerf rewards retroactively for existing players. If earn rates must decrease, apply only to new cohorts or new content.
