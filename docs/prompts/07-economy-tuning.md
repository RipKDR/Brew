# Economy Tuning — Use Any Time

You are tuning the economy for **Brew**, a mobile puzzle game at `T:\Brew`. This prompt is used whenever economy pacing feels off, monetization needs adjustment, or you want to verify that currency flows are healthy. It is a repeatable, data-driven tuning workflow.

---

## Step 1 — Read Source Documents

Read these files before making any changes:

1. `AGENTS.md` — project rules. Pay special attention to: "Two currencies only" and "No hardcoded values"
2. `.cursor/rules/economy-guard.mdc` — economy-specific coding rules
3. `docs/economy/economy-model.md` — the authoritative spec for currency flows, earning rates, cost tables, pacing targets, and the reasoning behind each value
4. `docs/economy/monetization-plan.md` — IAP catalog, ad reward values, ethical guardrails, acceptable monetization boundaries
5. `config/firebase/remote-config-defaults.json` — the current live values for all economy parameters

If `.cursor/skills/brew-economy-tuning.md` exists, read it too — it contains learned tuning heuristics from previous sessions.

---

## Step 2 — Understand Current State

Before changing anything, understand what the economy looks like right now.

### 2.1 Read Current Config Values

Open `config/firebase/remote-config-defaults.json` and extract these key values:

**Earning rates:**
- `essence_per_level_1star` — Essence earned for 1-star completion
- `essence_per_level_2star` — Essence earned for 2-star completion
- `essence_per_level_3star` — Essence earned for 3-star completion
- `daily_brew_essence_reward` — Essence from Daily Brew
- `daily_brew_gem_reward` — Gems from Daily Brew
- `streak_multipliers` — array of multipliers by streak count

**Costs:**
- Workshop upgrade costs (array or individual keys per upgrade)
- Booster costs (Essence and Gem prices for each booster type)
- Extra moves cost (Gems per +3 moves)

**Ad rewards:**
- Essence/Gem rewards for each rewarded ad placement

**IAP values:**
- Currency grants per Gem pack
- Starter Bundle contents

Record these values. You'll compare against simulation output.

### 2.2 Run Economy Simulation

```
python scripts/economy-sim/simulate_economy.py
```

The simulation models a player profile over time. It should output:
- Days to complete workshop at various play rates (5, 10, 15 levels/day)
- Total Essence and Gems earned per day at each play rate
- Gem surplus or deficit (free gem income minus casual spending)
- Monetization pressure index (how often a player hits a currency wall)

If the simulation script accepts parameters, run it with the default player profile first:
```
python scripts/economy-sim/simulate_economy.py --levels-per-day 10 --days 60 --use-daily-brew true --watch-ads 2
```

---

## Step 3 — Evaluate Against Targets

Compare simulation output against the targets defined in `docs/economy/economy-model.md`:

### 3.1 Workshop Pacing

**Target:** Workshop completes in 30-45 days for a player completing 10 levels/day who does the Daily Brew and watches ~2 rewarded ads/day.

- If workshop completes in **under 25 days**: player runs out of goals too fast. Increase workshop costs or reduce earning rates
- If workshop completes in **25-30 days**: slightly fast but acceptable for soft launch. Monitor
- If workshop completes in **30-45 days**: ideal range. No changes needed
- If workshop completes in **45-60 days**: slightly slow. Players may lose motivation. Reduce costs or increase earning rates
- If workshop completes in **over 60 days**: too slow. Players will churn before finishing. This is a P1 issue

### 3.2 Gem Sufficiency

**Target:** Free gem sources (Daily Brew + shelf milestones + event rewards) should cover casual booster usage (~1 booster every 2-3 days) without requiring IAP.

- Calculate daily free gem income: `daily_brew_gem_reward` + amortized shelf milestone gems + amortized event gems
- Calculate daily gem spend for casual play: average booster cost in gems ÷ 2.5 (one booster every 2.5 days)
- If daily income > daily spend: gem balance grows over time. Good — player feels rewarded for free play
- If daily income ≈ daily spend: break-even. Acceptable but watch for frustration
- If daily income < daily spend: gem deficit. Player must either stop using boosters or buy gems. This is acceptable monetization pressure IF boosters are never required to complete levels. If any level requires a booster, this is a P0 issue

### 3.3 Star Distribution Impact

Verify that the gap between 1-star and 3-star rewards is motivating but not punishing:
- 3-star reward should be 2-2.5x of 1-star reward (currently 120 vs 50 = 2.4x). This is good
- A player who consistently gets 1 star should still complete the workshop, just slower (verify with simulation: `--star-rate 1.0`)
- A player who consistently gets 3 stars should feel noticeably faster progression

### 3.4 Win Streak Impact

Verify the streak multiplier creates a meaningful but not broken acceleration:
- Max streak (3x) should feel like a significant reward for skilled play
- A player at max streak earning 3 stars gets: 120 × 3 = 360 Essence per level. Run the simulation with this ceiling and verify workshop doesn't complete in under 20 days
- Streak reset on failure should feel like a setback, not a catastrophe. The difference between 3x and 1x is important but not crushing

---

## Step 4 — Make Adjustments (If Needed)

If any evaluation in Step 3 is outside its acceptable range, follow this exact sequence:

### 4.1 Update the Spec First

Open `docs/economy/economy-model.md` and update the relevant values. Add a note in the document explaining WHY the change was made:

```
### Revision — [Date]
Changed workshop_upgrade_cost_tier_3 from 800 to 650.
Reason: simulation showed 52-day workshop completion at 10 levels/day, 
exceeding the 45-day ceiling. Reducing mid-tier costs to bring pacing 
into the 35-40 day range.
```

The spec document is the source of truth. Never change config without updating the spec.

### 4.2 Update Remote Config Defaults

Open `config/firebase/remote-config-defaults.json` and update the corresponding keys to match the new spec values.

### 4.3 Update ScriptableObjects (If Applicable)

If the changed values are also in ScriptableObject assets (e.g., `EconomyConfig.asset`, `WorkshopConfig.asset`), update those to match. ScriptableObjects serve as offline fallback defaults.

### 4.4 Update Code Only If Necessary

Code changes should rarely be needed for economy tuning — if values are properly externalized. If you find a hardcoded value in a `.cs` file, that's a bug. Extract it to config first, then tune the config.

### 4.5 Re-Run Simulation

```
python scripts/economy-sim/simulate_economy.py
```

Verify the adjusted values bring all metrics into their target ranges. If not, iterate Steps 4.1-4.5 until they do. Limit yourself to 3 iterations — if you can't converge, the issue may be structural (e.g., too many workshop upgrades, not enough level content) and needs a design discussion.

---

## Step 5 — Ethical Guardrail Checks

Reference: `docs/economy/monetization-plan.md` → Ethical Guardrails section.

These are hard constraints that must never be violated, regardless of tuning:

### 5.1 No Level Requires Boosters

Play every level without using any boosters. Every level must be completable with skill alone. If you find a level that seems to require a booster:
- First verify it's actually solvable (try 5 different approaches)
- If unsolvable without boosters, increase the move limit or reduce the recipe target. Reference `08-level-design.md` for the level tuning process
- A level can be hard (40% first-attempt completion) — it just can't be impossible

### 5.2 No Manufactured Near-Misses

The game must not artificially manipulate the board to create "almost won" scenarios that push toward booster purchases. Verify:
- Token spawning is truly random (seeded PRNG, not weighted toward failure)
- No code adjusts difficulty based on player spending behavior
- No code reduces token variety when the player is close to winning

### 5.3 Spending Caps Are Reasonable

- No single IAP over $19.99 in the MVP
- No "loot box" mechanics (every purchase gives exactly what's advertised)
- No purchases that provide gameplay advantages unavailable through free play (pay-to-win). Boosters purchasable with Gems are also earnable for free
- Children's considerations: if the game might be played by minors, verify compliance with COPPA/GDPR-K. See `docs/legal/app-store-compliance.md`

### 5.4 Ad Pressure Is Reasonable

- Rewarded ads are always optional — player chooses to watch
- Interstitials are infrequent (every 3rd level win, not every level)
- No-Ads Pass exists as an affordable option ($4.99)
- No ad plays during gameplay (only between levels or on menu)
- Total session ad time for a non-paying player should average under 2 minutes per 30-minute session

---

## Step 6 — Document Changes

After tuning is complete:

1. Verify `docs/economy/economy-model.md` reflects all current values and the reasoning behind recent changes
2. Verify `config/firebase/remote-config-defaults.json` matches the spec
3. Run the simulation one final time and save the output:

```
python scripts/economy-sim/simulate_economy.py --output docs/economy/simulation-results-<date>.md
```

4. Update `docs/project-management/session-handoff.md` with a summary of what was tuned and why
5. If `.cursor/skills/brew-economy-tuning.md` exists, add any new heuristics or lessons learned (e.g., "Reducing workshop costs is more effective than increasing earning rates for pacing adjustments")

---

## Quick Reference: Tuning Levers

| Lever | Effect | Side Effects |
|---|---|---|
| Increase Essence per level | Faster workshop progression | Reduces monetization pressure, may shorten retention |
| Decrease Essence per level | Slower workshop, more IAP pressure | May frustrate free players |
| Increase workshop costs | Longer progression, more IAP pressure | May feel grindy |
| Decrease workshop costs | Faster progression, less IAP need | May run out of goals too fast |
| Increase streak multiplier | Rewards skill, widens gap between players | May make workshop too fast for skilled players |
| Increase daily brew reward | Rewards daily engagement | May make gems too abundant |
| Increase ad reward | More incentive to watch ads | May reduce IAP if ads become "good enough" |
| Adjust star thresholds | Changes how easy/hard 3-star is | Affects both earnings AND player satisfaction |
