# Monetization Plan — Brew

> Version 0.1 · MVP Scope · Last updated 2026-04-09

---

## 1. Monetization Philosophy

Brew monetizes through **convenience, recovery, and cosmetics** — never through access gating. There is no energy or lives system. Every player can play unlimited levels for free, forever. Paying players get comfort and acceleration. Non-paying players get the same content, slightly slower.

---

## 2. In-App Purchase Catalog

### 2.1 Gem Packs (Permanent)

| SKU | Price (USD) | Gems | Gems/$ | Bonus Badge |
|-----|-------------|------|--------|-------------|
| gem_50 | $0.99 | 50 | 50.5 | — |
| gem_300 | $4.99 | 300 | 60.1 | "Best Value" (at 300+) |
| gem_700 | $9.99 | 700 | 70.1 | "Most Popular" |
| gem_1500 | $19.99 | 1,500 | 75.0 | "Best Deal" |

Higher tiers offer progressively better Gem-per-dollar ratios (up to ~48% more value at the top tier vs. the base tier). This rewards commitment without punishing small spenders.

### 2.2 Starter Bundle (One-Time)

| SKU | Price | Contents | Limit |
|-----|-------|----------|-------|
| starter_bundle | $1.99 | 50 Gems + 500 Essence + 3 Catalysts | Once per account |

Surfaced after the player completes level 5. Appears as a modal offer with a 24-hour countdown display (the countdown is real — after 24 hours the offer moves to the shop permanently at the same price, but loses the prominent modal placement). The bundle provides approximately one day's worth of regular-player earnings, giving a meaningful but not game-breaking head start.

### 2.3 No-Ads Pass (One-Time)

| SKU | Price | Effect | Limit |
|-----|-------|--------|-------|
| no_ads | $4.99 | Permanently removes interstitial ads | Once per account |

The No-Ads Pass removes **only interstitial ads**. Rewarded ads remain available as opt-in choices (the player can still choose to watch an ad for +3 moves, 2× rewards, etc.). This ensures rewarded ad revenue is not eliminated by the pass.

### 2.4 Weekly Deal (Post-MVP — Not in Current Catalog)

> **Status:** Planned for post-soft-launch. The `weekly_deal` SKU is **not** in the current `IAPManager.Catalog`. Implement after validating IAP conversion during soft launch.

| SKU | Price | Contents | Limit |
|-----|-------|----------|-------|
| weekly_deal | $2.99 | Rotating: see below | Once per week |

Weekly Deal rotates on a 4-week cycle:

| Week | Contents |
|------|----------|
| 1 | 100 Gems + 1,000 Essence |
| 2 | 75 Gems + 5 Shakes + 3 Catalysts |
| 3 | 150 Gems + 500 Essence |
| 4 | 100 Gems + 3 Catalysts + 2 Extra Moves |

Weekly Deal appears in the shop with a "Weekly Deal" badge and a countdown timer showing time remaining. The timer is real and synchronized to the weekly reset (Monday 00:00 UTC).

---

## 3. Rewarded Ad Placements

### 3.1 Placement Map

| # | Placement | Trigger | Reward | Frequency Cap |
|---|-----------|---------|--------|---------------|
| 1 | **Fail Recovery** | Player runs out of moves | +3 moves, resume level | 1× per level attempt |
| 2 | **Double Potion Reward** | Level completion results screen | 2× Essence earned for that level | 1× per level completion |
| 3 | **Free Daily Booster** | Booster shop screen, daily reset | 1 free Shake booster | 1× per day (resets 00:00 UTC) |
| 4 | **Streak Protection** | Fail modal when streak ≥ 2 | Preserves Win Streak | 2× per day (resets 00:00 UTC) |
| 5 | **Daily Brew Doubler** | Daily Brew completion screen | 2× Daily Brew Essence | 1× per Daily Brew |

### 3.2 Session Frequency Cap

**Maximum 5 rewarded ads per calendar day per player.** The cap spans all placements combined.

When the cap is reached, all rewarded ad buttons grey out and show "Available tomorrow" text. Gem alternatives remain functional (e.g., streak protection can still be purchased with 5 Gems after the ad cap is hit).

### 3.3 Rewarded Ad UX Rules

1. Every rewarded ad interaction is **opt-in**. No forced viewing.
2. A clear "Watch Ad" button is presented alongside the Gem cost alternative. The player always has a non-ad option.
3. If the ad network fails to fill (no ad available), the button hides gracefully — no error state shown.
4. Ad viewing does not interrupt gameplay. All rewarded ad prompts occur between levels or at explicit decision points (fail, completion).
5. Rewarded ads are 15–30 seconds. The reward is granted immediately upon ad completion.

---

## 4. Interstitial Ad Placement

### 4.1 Trigger Rule

An interstitial ad is shown **after every 3rd level completion**, counting only successes. Fails do not increment the counter.

- Counter persists across sessions (stored locally).
- Counter resets to 0 at the start of each calendar day (00:00 UTC).
- **No interstitial is shown after a failed level.** This prevents frustration stacking.
- **No interstitial is shown during Weekly Event levels.** Events are premium-feeling experiences; ads would undermine that.

### 4.2 Interstitial Format

- Skippable after **5 seconds**.
- Maximum duration: 30 seconds if not skipped.
- Full-screen, with a visible countdown timer and an "X" close button that appears at the 5-second mark.

### 4.3 No-Ads Pass Interaction

Players who purchase the No-Ads Pass ($4.99):

- **Interstitials:** Permanently removed. Never shown.
- **Rewarded ads:** Still available and opt-in. The "Watch Ad" buttons remain visible and functional.
- **Interstitial counter:** Disabled entirely. No interstitial logic runs.

---

## 5. ARPDAU Modeling

### 5.1 Target Range

**Target ARPDAU: $0.05–$0.15**

This positions Brew in line with casual puzzle games in the global mass-market tier (comparable to games like Tile Match, Bubble Pop, etc.).

### 5.2 Revenue Composition Model

Assumptions for a mature (D30+) player base:

| Revenue Stream | % of DAU Engaging | Revenue/User/Day | % of ARPDAU |
|----------------|-------------------|-------------------|-------------|
| Interstitial ads | 100% of non-pass holders (~98%) | $0.015 | 20% |
| Rewarded ads | 65% of DAU | $0.025 | 33% |
| IAP (Gem packs) | 1.5% of DAU | $0.025 | 33% |
| IAP (Bundles/Weekly Deal) | 0.5% of DAU | $0.008 | 11% |
| No-Ads Pass | 1.2% lifetime purchasers (amortized) | $0.002 | 3% |
| **Total ARPDAU** | | **~$0.075** | **100%** |

### 5.3 Revenue Split

| Source | Share |
|--------|-------|
| Ad revenue (interstitial + rewarded) | ~53% |
| IAP revenue (Gems + bundles + No-Ads) | ~47% |

The ad-heavy split is expected for a mass-market casual game with no energy system. IAP share grows if the game attracts a dedicated core audience.

### 5.4 Scenario Analysis

| Scenario | DAU | ARPDAU | Monthly Revenue |
|----------|-----|--------|-----------------|
| Soft launch (conservative) | 5,000 | $0.05 | $7,500 |
| Post-launch (3 months) | 50,000 | $0.08 | $120,000 |
| Scaled (12 months, with UA) | 200,000 | $0.10 | $600,000 |
| Optimistic ceiling | 500,000 | $0.12 | $1,800,000 |

---

## 6. Conversion Funnel

### 6.1 Expected Conversion Rates

| Metric | Target Rate | Notes |
|--------|-------------|-------|
| **Rewarded ad viewers** (% of DAU) | 60–70% | High because ads are tied to tangible, immediate rewards |
| **Any IAP purchasers** (% of installers, lifetime) | 3–5% | Standard for casual puzzle; driven by Starter Bundle and gem_50 |
| **No-Ads Pass purchasers** (% of installers, lifetime) | 1–2% | Lower because Brew has no energy system — ad frequency is moderate |
| **Repeat IAP purchasers** (% of first-time buyers) | 25–35% | Weekly Deal drives repeat purchases |
| **Whale segment** (spends $50+ lifetime, % of installers) | 0.3–0.5% | Capped naturally by limited Gem sinks in MVP |

### 6.2 Purchase Triggers by Lifecycle Stage

| Stage | Days | Most Likely First Purchase | Trigger |
|-------|------|---------------------------|---------|
| Onboarding | D0–D1 | Starter Bundle ($1.99) | Modal offer after level 5; low price, high perceived value |
| Early engagement | D2–D7 | gem_50 ($0.99) or No-Ads Pass ($4.99) | First interstitial frustration; first difficult level requiring Extra Moves |
| Mid engagement | D7–D14 | Weekly Deal ($2.99) | Workshop progress stalls; player wants Essence boost |
| Committed player | D14–D30 | gem_300 or gem_700 ($4.99–$9.99) | Desire for faster workshop completion or booster stockpile |
| Late stage | D30+ | gem_1500 ($19.99) | Completionist drive; cosmetic themes (post-MVP) |

### 6.3 First Purchase Optimization

- The **Starter Bundle** is priced at $1.99 to minimize first-purchase friction. Research shows the first IAP is the hardest; once a player has purchased, repeat purchase likelihood increases 3–5×.
- The offer appears at level 5 (typically 5–8 minutes into the game) — early enough that the player has experienced the core loop but late enough to understand what the currencies do.
- The bundle contents are worth ~$2.50 at gem_50 rates, creating a genuine deal.

---

## 7. Ethical Monetization Principles

### 7.1 Hard Rules

These rules are non-negotiable. Violations are treated as bugs.

| # | Rule | Rationale |
|---|------|-----------|
| 1 | **No manufactured impossible levels.** Every level must be clearable without boosters by an average-skill player at least 70% of the time with optimal play. | Players must trust that failure is fair, not engineered. |
| 2 | **No fake countdown timers.** If a timer is displayed (e.g., Weekly Deal countdown), it must reflect a real deadline. The offer must actually expire or change when the timer hits zero. | Fake urgency is deceptive and erodes trust. |
| 3 | **No hidden mechanics.** Game rules, odds, and reward amounts must be deterministic and visible to the player. If a mechanic has randomness (e.g., board generation), the system is documented in help screens. | Players should never feel cheated by invisible systems. |
| 4 | **No loot boxes or gacha.** All purchases deliver exactly what is advertised. No randomized IAP rewards. Gem packs give a fixed Gem count. Bundles list exact contents. | Randomized monetization is exploitative and increasingly regulated. |
| 5 | **No pay-to-win boosters.** Boosters provide convenience (board shuffle, color conversion, extra moves). No booster gives a permanent stat increase or unlocks a level that is otherwise impossible. | Paying must not create a separate, superior game. |
| 6 | **No forced ads.** Interstitial ads are the only non-opt-in ad format, and they are skippable after 5 seconds. Rewarded ads are always opt-in. | Respect for player time is non-negotiable. |
| 7 | **No dark patterns in purchase flows.** The "X" / close button on any purchase modal must be clearly visible (minimum 44×44 pt tap target), always present, and never delayed. No "Are you sure you want to miss this deal?" guilt modals. | Purchase decisions must be free from manipulation. |
| 8 | **Ad frequency cap enforced.** Maximum 5 rewarded ads per day. Maximum 1 interstitial per 3 level completions. These caps cannot be raised by server config without a client update and review. | Prevents ad spam even under revenue pressure. |
| 9 | **No spend gates on content.** All 100 campaign levels, all Daily Brews, and all Weekly Events are playable without spending real money or Gems. | The full game is free. Monetization is layered on top. |
| 10 | **Transparent Gem value.** The shop always shows the Gem price and the equivalent USD value (e.g., "10 Gems ≈ $0.20 based on gem_50 rate"). Players can make informed spending decisions. | Obfuscating value behind premium currency is a dark pattern. |

### 7.2 Self-Audit Checklist

Before any monetization change ships, the team reviews the following:

- [ ] Can a non-paying player still complete this level/event/feature?
- [ ] Does this change increase the number of involuntary ad views?
- [ ] Does the purchase UI have a clearly visible dismiss/close option?
- [ ] Is the timer/countdown real?
- [ ] Does the purchase deliver exactly what is shown — no randomization?
- [ ] Would we be comfortable if a journalist described this mechanic in an article?

### 7.3 Age & Spending Protections

- If the device reports the player is under 18 (via platform age gate or parental controls), IAP purchase prompts are hidden and only restored with parental confirmation.
- No ad personalization for users who have not consented to tracking (ATT on iOS, consent prompt on Android).
- Monthly spend cap notification: if a player spends more than $50 in a rolling 30-day window, a one-time informational modal appears: "You've spent $X this month. Need to manage your budget? Visit Settings > Purchase History." This is informational only — it does not block purchases.

---

## 8. Ad Network & Mediation

### 8.1 Recommended Stack

| Component | Recommended | Rationale |
|-----------|-------------|-----------|
| Mediation | AppLovin MAX or ironSource LevelPlay | High fill rates for casual games; unified auction |
| Rewarded video | AdMob, Unity Ads, Meta Audience Network | Broad demand; high eCPM for rewarded |
| Interstitial | AdMob, Pangle, Meta Audience Network | Coverage in global markets |
| Waterfall strategy | Hybrid (bidding + waterfall) | Maximize eCPM with real-time bidding where supported |

### 8.2 eCPM Assumptions

| Format | Region | Estimated eCPM |
|--------|--------|----------------|
| Rewarded video | US/CA/EU | $10–$18 |
| Rewarded video | LATAM/SEA | $3–$7 |
| Rewarded video | Global blended | $6–$10 |
| Interstitial | US/CA/EU | $6–$12 |
| Interstitial | LATAM/SEA | $1.50–$4 |
| Interstitial | Global blended | $3–$6 |

### 8.3 Fill Rate Targets

- Rewarded video: ≥ 95% fill rate. If fill drops below 90%, add a backup network.
- Interstitial: ≥ 98% fill rate. Interstitial demand is typically abundant.
- If an ad fails to load, the player should never see a broken state. The ad button hides, and the flow continues normally.

---

## 9. Revenue KPI Dashboard

Track these metrics daily from launch:

| Metric | Definition | Target |
|--------|-----------|--------|
| ARPDAU | Total revenue ÷ DAU | $0.05–$0.15 |
| ARPPU | Revenue ÷ paying users | $5–$15 |
| IAP conversion (lifetime) | Unique payers ÷ unique installers | 3–5% |
| Rewarded ad engagement | % of DAU watching ≥ 1 rewarded ad | 60–70% |
| Avg rewarded ads/user/day | Total rewarded views ÷ DAU | 1.5–2.5 |
| Interstitial impressions/user/day | Total interstitial views ÷ DAU | 2–4 |
| No-Ads Pass attach rate | Pass purchasers ÷ installers (lifetime) | 1–2% |
| D7 IAP rate | % of D7-retained users who made any purchase | 1–2% |
| Starter Bundle conversion | Bundle purchases ÷ players who saw the offer | 8–12% |
| Weekly Deal conversion | Deal purchases ÷ eligible players (D7+) | 3–6% |
| Ad revenue per DAU | Ad revenue ÷ DAU | $0.03–$0.06 |
| IAP revenue per DAU | IAP revenue ÷ DAU | $0.02–$0.09 |
