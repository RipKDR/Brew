# Meta Systems — Brew

> Version 0.1 · MVP Scope · Last updated 2026-04-09

---

## 1. Potion Shelf

### 1.1 Overview

The Potion Shelf is a visual collection screen showing every unique potion the player has brewed. Each potion corresponds to a specific level's recipe. Completing a level fills that potion's slot on the shelf. The shelf doubles as a progress map and a milestone reward track.

### 1.2 Potion Count (MVP)

| World | Levels | Potions |
|-------|--------|---------|
| 1 — Ember Glen | 20 | 20 |
| 2 — Frost Hollow | 20 | 20 |
| 3 — Vine Thicket | 20 | 20 |
| 4 — Sun Ridge | 20 | 20 |
| 5 — Shadow Deep | 20 | 20 |
| **Total** | **100** | **100** |

Each level has exactly one recipe, so completing a level for the first time always unlocks one new potion. Replaying a level does not create a duplicate.

### 1.3 Shelf Layout

- The shelf is displayed as a single scrollable screen divided into 5 **shelves** (one per world).
- Each shelf holds 20 potion bottles arranged in two rows of 10.
- Locked potions appear as dark silhouettes with a lock icon and the level number.
- Unlocked potions render as filled, colored bottles with a subtle idle shimmer.
- Tapping an unlocked potion opens a detail card: potion name, recipe ingredients, date brewed, star rating achieved.

### 1.4 Milestone Rewards

Milestones trigger at cumulative shelf completion thresholds. Each milestone plays a full-screen celebration animation and delivers rewards to the player's inbox.

| Milestone | Potions Required | Essence Reward | Gem Reward | Bonus |
|-----------|-----------------|----------------|------------|-------|
| 25% | 25 | 500 | 25 | Unlock "Apprentice" player title |
| 50% | 50 | 1,000 | 50 | Unlock "Brewer" player title + shelf frame skin |
| 75% | 75 | 2,000 | 100 | Unlock "Alchemist" player title + workshop theme variant |
| 100% | 100 | 5,000 | 250 | Unlock "Master Brewer" title + golden shelf frame + exclusive potion glow effect |

### 1.5 Visual Presentation

- Background: a warm wooden shelf with candlelight ambient glow.
- As potions fill in, the scene gains light — empty shelves are dim, full shelves radiate color.
- Milestone thresholds are marked with a ribbon/banner on the shelf edge showing progress (e.g., "25/100").
- A small Essence and Gem icon next to the next milestone shows the upcoming reward.

---

## 2. Workshop

### 2.1 Overview

The Workshop is a single-room scene the player gradually restores through Essence spending. It begins as a dusty, abandoned alchemist's lab. Each upgrade reveals a new decoration, piece of equipment, or visual feature. The workshop serves as the player's home screen and primary Essence sink.

### 2.2 Upgrade States

The workshop has 12 upgrade states. Costs follow an exponential curve: early upgrades are cheap to create an immediate sense of progress; later upgrades require sustained play.

| # | Upgrade Name | Essence Cost | Cumulative Cost | Visual Change |
|---|-------------|-------------|-----------------|---------------|
| 1 | Sweep the Floor | 50 | 50 | Dust and cobwebs cleared. Stone floor visible. |
| 2 | Light the Hearth | 100 | 150 | Fireplace ignites, warm orange light fills room. Ambient crackling SFX. |
| 3 | Repair the Workbench | 200 | 350 | Broken table restored. Mortar and pestle appear on surface. |
| 4 | Hang the Shelves | 400 | 750 | Wooden shelves mount to the back wall. Empty bottles line them. |
| 5 | Install the Cauldron | 600 | 1,350 | Large bubbling cauldron appears center-room. Steam particles. |
| 6 | Stock the Herb Rack | 1,000 | 2,350 | Dried herbs and ingredient bundles hang from the ceiling. |
| 7 | Place the Star Map | 1,500 | 3,850 | Celestial chart unfurls on the wall. Constellation twinkle at night. |
| 8 | Add the Crystal Array | 2,500 | 6,350 | Five colored crystals (one per ingredient type) glow on the windowsill. |
| 9 | Build the Distillery | 4,000 | 10,350 | Copper distillation apparatus with flowing liquid tubes. |
| 10 | Enchant the Windows | 6,000 | 16,350 | Plain windows transform to stained glass. Colored light shafts. |
| 11 | Summon the Familiar | 8,000 | 24,350 | A small animated companion (owl/cat/fox — chosen randomly) appears and idles. |
| 12 | Master's Flourish | 12,000 | 36,350 | The entire room gets a golden-hour lighting pass, floating magical particles, and the familiar gains a sparkle trail. Final state. |

### 2.3 Progression Curve

- **Upgrade 1–3** (350 Essence total): achievable in the first 1–2 sessions. Immediate gratification.
- **Upgrade 4–6** (2,000 Essence): days 2–5 of regular play.
- **Upgrade 7–9** (8,000 Essence): days 6–18 of regular play.
- **Upgrade 10–12** (26,000 Essence): days 19–40 of regular play.
- **Total cost to fully restore: 36,350 Essence.**
- **Target: 30–45 days for an average player completing 8–12 levels/day.**

### 2.4 Workshop Interactions

- The workshop is the main menu background. The player sees their progress every time they open the app.
- Tapping an upgradeable object highlights it with a pulsing glow and shows the cost. If affordable, a "Restore" button appears.
- After purchasing, a 2-second transformation animation plays showing the before/after.
- Each upgrade unlocks a one-time "share" prompt (screenshot card with the new room state).

---

## 3. Daily Brew

### 3.1 Overview

A single challenge level that refreshes every 24 hours at 00:00 UTC. The Daily Brew uses the standard puzzle mechanics but with a curated recipe that may include ingredient combinations not seen in the main campaign. It is available to any player who has completed level 5.

### 3.2 Timing

- **Refresh:** 00:00 UTC daily.
- **Availability window:** 24 hours. If the player does not complete it, it expires and is replaced.
- **Attempt limit:** Unlimited attempts. The player can retry until they clear it or the window closes.
- **No Essence cost to enter.** Free to play.

### 3.3 Difficulty

Daily Brews cycle through three difficulty tiers on a fixed weekly pattern:

| Day | Difficulty | Move Count | Target Potions |
|-----|-----------|------------|----------------|
| Mon | Easy | 25 | 2 |
| Tue | Medium | 22 | 3 |
| Wed | Medium | 22 | 3 |
| Thu | Hard | 18 | 4 |
| Fri | Easy | 25 | 2 |
| Sat | Medium | 22 | 3 |
| Sun | Hard | 18 | 4 |

### 3.4 Rewards

| Reward | Amount |
|--------|--------|
| Essence (base) | 100 |
| Gems (base) | 5 |
| Bonus: 2× potion reward (rewarded ad) | Doubles Essence to 200 |

### 3.5 Daily Brew Streak

Completing Daily Brews on consecutive calendar days builds a Daily Brew Streak independent of the Win Streak.

| Consecutive Days | Bonus Multiplier on Essence | Gem Bonus |
|------------------|-----------------------------|-----------|
| 1 | 1.0× | +0 |
| 2 | 1.0× | +0 |
| 3 | 1.25× | +5 |
| 5 | 1.5× | +10 |
| 7 | 2.0× | +15 |

The streak resets if the player misses a day. The 7-day cycle then restarts. There is no streak protection for Daily Brew streaks.

---

## 4. Win Streak

### 4.1 Overview

The Win Streak tracks consecutive level completions in the main campaign. It multiplies the Essence reward earned at the end of each level. The streak resets on any level failure.

### 4.2 Multiplier Table

| Consecutive Wins | Essence Multiplier | UI Indicator |
|-------------------|--------------------|--------------|
| 1 | 1.0× | No badge |
| 2 | 1.25× | Bronze flame |
| 3 | 1.5× | Silver flame |
| 5 | 2.0× | Gold flame |
| 8 | 2.5× | Platinum flame |
| 10+ | 3.0× (cap) | Diamond flame |

The multiplier applies only to the base Essence reward for the level. It does not multiply Gem rewards, Daily Brew rewards, or event rewards.

### 4.3 Streak Protection

When a player fails a level while holding a streak ≥ 2:

1. A modal appears: "Your streak is at risk!"
2. **Option A:** Watch a rewarded ad → streak preserved, player retries the level with the same streak.
3. **Option B:** Spend 5 Gems → streak preserved, same as above.
4. **Option C:** Dismiss → streak resets to 0.

Protection can be used a maximum of **2 times per day** (resets at 00:00 UTC). This prevents indefinite ad-farming.

### 4.4 UI Presentation

- A flame icon sits in the top-left of the puzzle screen, showing the current streak count.
- At streaks of 3+, the flame animates with increasing intensity.
- On level completion, the results screen shows: Base Essence × Streak Multiplier = Total Essence, with the multiplier highlighted.
- On streak reset, a brief "streak broken" animation plays (flame extinguishes).

---

## 5. Weekly Event

### 5.1 Overview

A themed set of 7 special levels available for one week (Monday 00:00 UTC through Sunday 23:59 UTC). Events use re-skinned ingredients and unique recipes. All events share the same structural template; only art assets, color palettes, and tuning parameters change.

### 5.2 Template Structure

Each weekly event contains:

- **7 levels**, progressing in difficulty.
- **A themed ingredient set:** 5 ingredients with event-specific art and names (same gameplay behavior as the base set).
- **An exclusive potion:** brewed by completing all 7 levels. Added permanently to the Potion Shelf in a special "Event" section.
- **Entry requirement:** Player must have completed main campaign level 10.

### 5.3 Level Difficulty Curve

| Event Level | Moves | Target Potions | Star 1 | Star 2 | Star 3 |
|-------------|-------|----------------|--------|--------|--------|
| 1 | 28 | 2 | Complete | ≤ 22 moves | ≤ 18 moves |
| 2 | 25 | 2 | Complete | ≤ 20 moves | ≤ 16 moves |
| 3 | 25 | 3 | Complete | ≤ 20 moves | ≤ 16 moves |
| 4 | 22 | 3 | Complete | ≤ 18 moves | ≤ 14 moves |
| 5 | 22 | 4 | Complete | ≤ 18 moves | ≤ 14 moves |
| 6 | 20 | 4 | Complete | ≤ 16 moves | ≤ 12 moves |
| 7 | 18 | 5 | Complete | ≤ 14 moves | ≤ 10 moves |

### 5.4 Reward Structure

| Trigger | Essence | Gems |
|---------|---------|------|
| Complete event level (each) | 50 | 0 |
| 3-star an event level (each) | +25 bonus | 0 |
| Complete all 7 levels | 500 bonus | 30 |
| 3-star all 7 levels | 250 bonus | 20 |
| **Maximum event total** | **1,275** | **50** |

Completing all 7 levels also awards the **exclusive event potion** for the Potion Shelf.

### 5.5 Theme Swapping & Content Production

Events are built from a template. Creating a new event requires:

| Asset | Spec | Production Time (Est.) |
|-------|------|------------------------|
| 5 ingredient token sprites | 128×128 px, PNG with alpha | 2–3 hours (artist) |
| 5 ingredient idle animations | 4-frame loop, 12 fps | 2–3 hours (artist) |
| Event banner art | 1080×600 px, used on event entry screen | 1–2 hours (artist) |
| Exclusive potion bottle sprite | 256×256 px, matches theme palette | 1 hour (artist) |
| 7 level configurations | JSON: board size, move count, targets, ingredient distribution | 2–4 hours (designer) |
| Color palette override | 5 hex values for UI accent recoloring | 15 minutes (designer) |
| Localized event name + description | ~30 words | 30 minutes (writer) + localization pipeline |

**Total per event: ~1–2 person-days.** A single artist + designer pair can produce events on a 2-week pipeline, staying 1 week ahead.

### 5.6 Example Event Themes (First Month)

| Week | Theme Name | Ingredient Skin | Palette |
|------|-----------|----------------|---------|
| 1 | Lunar Brew | Moon dust, Starlight, Tide, Eclipse, Nebula | Silver / deep blue |
| 2 | Blossom Brew | Petal, Pollen, Dew, Thorn, Root | Pink / spring green |
| 3 | Storm Brew | Lightning, Hail, Gust, Thunder, Drizzle | Electric yellow / dark gray |
| 4 | Harvest Brew | Wheat, Apple, Pumpkin, Cinnamon, Honey | Warm orange / brown |

---

## 6. Inter-System Connections

### 6.1 Dependency Map

```
Main Campaign Levels
  ├─► Potion Shelf (each first-time clear fills a slot)
  │     └─► Milestone Rewards → Essence + Gems
  ├─► Win Streak (consecutive clears multiply Essence)
  │     └─► Streak Protection (ad / Gem sink)
  └─► Essence Earned
        └─► Workshop Upgrades (primary Essence sink)

Daily Brew
  ├─► Essence + Gems (direct reward)
  └─► Daily Brew Streak (bonus multiplier)

Weekly Event
  ├─► Essence + Gems (level + completion rewards)
  ├─► Exclusive Potion → Potion Shelf (event section)
  └─► Event potions count toward shelf milestones
```

### 6.2 System Reference Rules

- **Potion Shelf** never gates gameplay. It is read-only progression display + reward milestones.
- **Workshop** is purely cosmetic. No gameplay advantage from upgrades. No workshop level gates content.
- **Win Streak** multiplier applies only to main campaign Essence. It does not affect Daily Brew or Event Essence.
- **Daily Brew** is self-contained. Its streak is independent of the Win Streak.
- **Weekly Events** award potions that count toward Potion Shelf milestones, creating a cross-system reward loop.
- **Boosters** are purchased with Essence or Gems. They are consumed in any mode (campaign, daily, event). This makes Essence valuable across all systems.

### 6.3 Player Motivation Loop

```
Play Level → Earn Essence + Fill Potion Slot
    │                │
    │                ▼
    │         Shelf Milestone? → Bonus Essence + Gems
    │
    ▼
Spend Essence → Workshop Upgrade → Visual Reward → Motivation to Earn More
    │
    ▼
Win Streak → Higher Multiplier → More Essence Per Level → Faster Workshop Progress
    │
    ▼
Daily Brew → Bonus Essence/Gems + Daily Habit Formation
    │
    ▼
Weekly Event → Exclusive Potion + Gems → Shelf Completionism + Premium Currency
```
