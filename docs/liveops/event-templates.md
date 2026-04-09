# Brew — Live-Ops Event Templates

> Owner: Game Design Lead · Production: Live-Ops Designer
> Last updated: 2026-04-09

---

## 1. Weekly Event Template

### Structure

Each weekly event consists of **7 event-specific levels** presented on a standalone event map. Players can access the event from the main map via a banner after completing main level 5.

| Property | Value |
|----------|-------|
| **Levels** | 7 (numbered E1–E7) |
| **Difficulty curve** | E1–E2: Easy, E3–E5: Medium, E6: Hard, E7: Boss |
| **Move budgets** | E1: 20, E2: 22, E3: 25, E4: 25, E5: 28, E6: 28, E7: 32 |
| **Recipes per level** | E1–E3: 2 potions, E4–E5: 3 potions, E6–E7: 3 potions (with themed ingredient) |
| **Board size** | 7×9 (same as main campaign) |
| **Ingredient pool** | 4 base + 1 themed ingredient (replaced via theme system) |
| **Boosters allowed** | Yes (same as main campaign) |

### Duration

- **Start:** Monday 00:00 UTC
- **End:** Sunday 23:59 UTC
- **Visibility window:** Event banner appears Saturday 20:00 UTC (teaser) through Sunday 23:59 UTC (results).
- **Grace period:** None. Unfinished levels are locked at event end.

### Rewards

| Milestone | Reward |
|-----------|--------|
| Each level completed (E1–E7) | 75 Essence per level |
| 3 levels completed | 100 bonus Essence |
| 5 levels completed | 10 Gems |
| All 7 levels completed | **Exclusive event potion** + 25 Gems + 250 Essence |
| 3-star on all 7 levels | Cosmetic badge (displayed on profile) |

Total possible per event: 775 Essence + 35 Gems + 1 exclusive potion + 1 badge.

### Theme System

Events reuse the same 7-level mechanical templates. Themes are applied as a **data overlay**, not new content. A theme swap requires zero engineering and zero new art assets.

#### Theme Layer Definition

```yaml
theme:
  id: "lunar_brew"
  display_name: "Lunar Brew"
  
  # Visual remapping
  color_palette:
    primary: "#C0C0FF"      # replaces default primary
    secondary: "#8080CC"    # replaces default secondary
    accent: "#FFFFFF"       # replaces default accent
    background_tint: "#1A1A3A"  # overlay on event map background
  
  # Ingredient swap (themed ingredient replaces slot 5)
  themed_ingredient:
    id: "moonstone"
    display_name: "Moonstone"
    sprite_variant: "ingredient_gem_silver"  # existing sprite, recolored
    color_remap: { hue_shift: -30, saturation: 0.8, brightness: 1.2 }
  
  # Recipe target swap (what potions are brewed)
  recipes:
    - id: "lunar_glow"
      display_name: "Lunar Glow Elixir"
      icon_variant: "potion_round_silver"
      ingredients_required: { moonstone: 3, dewdrop: 2 }
    - id: "starlight_tonic"
      display_name: "Starlight Tonic"
      icon_variant: "potion_tall_blue"
      ingredients_required: { moonstone: 2, emberpetal: 2, dewdrop: 1 }
  
  # Background
  background:
    base: "event_bg_default"
    tint_color: "#1A1A3A"
    tint_opacity: 0.6
  
  # Exclusive reward
  reward_potion:
    id: "lunar_glow"
    display_name: "Lunar Glow Elixir"
    shelf_slot: "event_exclusive"
```

#### What the theme system swaps (no new assets)

| Element | Method | Cost |
|---------|--------|------|
| Background mood | Color tint overlay on default event BG | 0 art hours |
| Ingredient appearance | HSV remap on existing ingredient sprite sheet | 0 art hours (shader) |
| Ingredient name | String swap in localization table | 0 art hours |
| Recipe targets | JSON config: ingredient counts → potion ID mapping | 0 art hours |
| Potion icon | Recolor existing potion bottle sprite | 0 art hours (shader) |
| Event banner text | Localization string + tint color | 0 art hours |
| Reward potion | References re-skinned potion from recipes | 0 art hours |

### Content Creation Checklist

**Time budget per event: < 2 hours of designer work.**

- [ ] **Choose theme** — pick color palette + ingredient name + 2 recipe names (15 min)
- [ ] **Fill theme YAML** — copy template, fill in 12 fields (15 min)
- [ ] **Adjust level difficulty** — review 7 template levels; tweak move budgets ±2 and recipe counts if needed for the ingredient mix (30 min)
- [ ] **Write event copy** — banner text, event description, reward descriptions (15 min)
- [ ] **Localization pass** — add 8–12 new strings to loc sheet for translation pipeline (10 min)
- [ ] **QA dry-run** — play E1, E4, E7 with theme applied; verify visuals, rewards, recipe completion (20 min)
- [ ] **Submit PR** — theme YAML + loc strings + any difficulty tweaks (5 min)

---

## 2. First Four Event Themes

### Event 1: Lunar Brew (Week 1)

| Property | Value |
|----------|-------|
| **Theme** | Moon / Silver / Night sky |
| **Color palette** | Primary: `#C0C0FF`, Secondary: `#8080CC`, Accent: `#FFFFFF`, BG tint: `#1A1A3A` |
| **Themed ingredient** | **Moonstone** — silver gem, sprite `ingredient_gem` with hue_shift: −30 |
| **Recipe 1** | Lunar Glow Elixir — 3 Moonstone + 2 Dewdrop |
| **Recipe 2** | Starlight Tonic — 2 Moonstone + 2 Emberpetal + 1 Dewdrop |
| **Exclusive reward potion** | Lunar Glow Elixir |
| **Banner text** | "The moon is full — brew by its light!" |
| **Difficulty notes** | Standard template. No adjustments needed for Week 1 (player base is new). |

### Event 2: Ember Festival (Week 2)

| Property | Value |
|----------|-------|
| **Theme** | Fire / Gold / Warm glow |
| **Color palette** | Primary: `#FFD700`, Secondary: `#CC8800`, Accent: `#FF4500`, BG tint: `#3A1A00` |
| **Themed ingredient** | **Cindershard** — orange crystal, sprite `ingredient_gem` with hue_shift: +20, saturation: 1.2 |
| **Recipe 1** | Blazefire Draft — 3 Cindershard + 2 Emberpetal |
| **Recipe 2** | Molten Vigor — 2 Cindershard + 2 Thornroot + 1 Emberpetal |
| **Exclusive reward potion** | Blazefire Draft |
| **Banner text** | "The Festival of Embers has begun!" |
| **Difficulty notes** | E6–E7: reduce move budget by 1 each (players are slightly more skilled by Week 2). |

### Event 3: Frost Market (Week 3)

| Property | Value |
|----------|-------|
| **Theme** | Ice / Cyan / Winter market stalls |
| **Color palette** | Primary: `#00E5FF`, Secondary: `#0088AA`, Accent: `#E0FFFF`, BG tint: `#001A2A` |
| **Themed ingredient** | **Frostbloom** — icy blue flower, sprite `ingredient_flower` with hue_shift: −60, brightness: 1.3 |
| **Recipe 1** | Glacial Serum — 3 Frostbloom + 2 Dewdrop |
| **Recipe 2** | Chill Remedy — 2 Frostbloom + 2 Dewdrop + 1 Thornroot |
| **Exclusive reward potion** | Glacial Serum |
| **Banner text** | "The Frost Market opens its gates — barter with ice!" |
| **Difficulty notes** | Standard template. |

### Event 4: Bloom Season (Week 4)

| Property | Value |
|----------|-------|
| **Theme** | Spring / Pink / Flowers blooming |
| **Color palette** | Primary: `#FF69B4`, Secondary: `#CC3377`, Accent: `#FFFFAA`, BG tint: `#1A0A10` |
| **Themed ingredient** | **Petalvine** — pink petal, sprite `ingredient_leaf` with hue_shift: +45, saturation: 1.1 |
| **Recipe 1** | Blossom Nectar — 3 Petalvine + 2 Thornroot |
| **Recipe 2** | Spring Awakening — 2 Petalvine + 2 Emberpetal + 1 Dewdrop |
| **Exclusive reward potion** | Blossom Nectar |
| **Banner text** | "Spring has sprung — the garden overflows!" |
| **Difficulty notes** | E1–E2: increase move budget by 2 each (Bloom is positioned as a "relaxing" event). |

---

## 3. Daily Brew System

### Overview

One standalone challenge level available every day at 00:00 UTC. Designed as a quick, satisfying session anchor that rewards consistency.

### Level Parameters

| Property | Value |
|----------|-------|
| **Levels per day** | 1 |
| **Board size** | 7×9 (same as main) |
| **Ingredient pool** | 4 (day-of-week themed, see schedule below) |
| **Recipes** | Always exactly 2 potion targets |
| **Move budget** | 22 (fixed — between average and hard) |
| **Boosters** | Allowed |
| **Attempts** | Unlimited within the day (resets at next 00:00 UTC) |

### Difficulty Calibration

- Target completion rate: **65–75%** on first attempt (harder than average main level, easier than hard campaign levels).
- Aim for ~60% of players who attempt it to complete it within 2 tries.
- Move budget of 22 with 2 recipes requiring 4–5 fusions each provides the right tension.

### Day-of-Week Recipe Schedule

Each day features a fixed pair of ingredient themes. This creates weekly rhythm and lets players anticipate what's coming.

| Day | Ingredient Pair | Recipe Theme | Example Recipes |
|-----|----------------|-------------|-----------------|
| Monday | Emberpetal + Frostbloom | Fire & Ice | Ember Frost Brew (3 Ember + 2 Frost), Thermal Shock (2 Frost + 2 Ember + 1 Dewdrop) |
| Tuesday | Thornroot + Sunpetal | Vine & Sun | Solarvine Elixir (3 Thorn + 2 Sun), Morning Dew (2 Sun + 2 Thorn + 1 Dewdrop) |
| Wednesday | Dewdrop + Emberpetal | Water & Fire | Steam Distillate (3 Dew + 2 Ember), Vapor Brew (2 Ember + 2 Dew + 1 Thornroot) |
| Thursday | Frostbloom + Thornroot | Ice & Earth | Permafrost Salve (3 Frost + 2 Thorn), Frozen Root (2 Thorn + 2 Frost + 1 Sunpetal) |
| Friday | Sunpetal + Dewdrop | Sun & Water | Daylight Tonic (3 Sun + 2 Dew), Solar Rain (2 Dew + 2 Sun + 1 Emberpetal) |
| Saturday | Emberpetal + Thornroot | Fire & Earth | Magma Cordial (3 Ember + 2 Thorn), Lava Root (2 Thorn + 2 Ember + 1 Frostbloom) |
| Sunday | All four base | Wildcard | Alchemist's Choice (2 of each base pair, random selection from recipe pool) |

### Rewards

| Condition | Reward |
|-----------|--------|
| Daily Brew completed | 200 Essence + 5 Gems |
| 7 consecutive daily brews (streak bonus) | +100 Essence bonus on the 7th day |
| 14 consecutive daily brews | +200 Essence bonus + 10 Gems |
| 30 consecutive daily brews | +500 Essence bonus + 25 Gems + exclusive "Master Brewer" badge |

Streak resets to 0 if a day is missed. Partial credit: none.

### Level Generation

Daily Brew levels are **procedurally generated** using the level generator with deterministic parameters:

```
seed = SHA256(date_string + "daily_brew" + season_id)[:8]
  → parsed as uint32, used as PRNG seed

parameters:
  board_width: 7
  board_height: 9
  ingredient_count: 4
  ingredient_ids: [day_of_week_pair + 2 random from base pool]
  recipe_count: 2
  recipe_difficulty: "medium_hard"  # fusions_required: 4–5 per recipe
  move_budget: 22
  min_solutions: 3   # generator must verify ≥ 3 winning paths exist
  max_solutions: 20  # reject boards that are trivially solvable
```

The generator runs nightly at 22:00 UTC (2 hours before the daily reset). It produces the next 7 days of daily brews and caches them. If generation fails validation (< 3 solutions), it increments the seed suffix and retries (max 10 attempts).

### Daily Brew Availability

- Unlocks after completing main level 3.
- Available 00:00–23:59 UTC each day.
- If the player completes the brew, the tile shows a checkmark for the rest of the day.
- If the player fails, they can retry unlimited times within the day.

---

## 4. Event Analytics Integration

All events track via the `event_*` analytics events defined in `docs/analytics/event-tracking-plan.md`. Additionally:

| Metric | How Tracked | Target |
|--------|------------|--------|
| Event participation rate | `event_start` / DAU during event window | ≥ 35% |
| Event completion rate | `event_complete` / `event_start` | ≥ 45% |
| Daily Brew participation | `daily_brew_complete` / DAU | ≥ 20% |
| Daily Brew streak distribution | `streak_milestone.streak_count` histogram | Median ≥ 3 days |
| Event revenue uplift | ARPDAU during event week vs non-event baseline | ≥ +5% |

---

## 5. Event Content Pipeline

### Production Timeline

| Milestone | Deadline (relative to event start) |
|-----------|-----------------------------------|
| Theme selection & approval | −21 days (3 weeks before) |
| Theme YAML complete | −14 days |
| Localization strings submitted | −12 days |
| Translations returned | −7 days |
| QA pass complete | −5 days |
| Remote Config staged | −3 days |
| Go-live (automatic via server clock) | Day 0 (Monday 00:00 UTC) |

**Minimum buffer: 2 weeks of events should always be fully produced and staged ahead of the current week.**

### Approval Chain

1. Designer creates theme YAML → self-QA (play E1, E4, E7).
2. Design Lead reviews theme for variety vs. recent events (no color repeat within 3 weeks).
3. Analytics Lead confirms reward values are within economy guardrails.
4. PR merged → auto-deployed to staging environment.
5. Live-Ops Lead gives final go/no-go 48 hours before event start.
