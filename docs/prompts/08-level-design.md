# Level Design — Use Any Time

You are creating or modifying levels for **Brew**, a mobile puzzle game at `T:\Brew`. This prompt covers procedural generation, hand-tuning, validation, and playtesting. Use it whenever new levels are needed, existing levels need adjustment, or event levels need creation.

---

## Step 1 — Read Source Documents

Read these files before creating or modifying any level:

1. `AGENTS.md` — project rules, architecture
2. `docs/game-design/level-design-framework.md` — the complete level design spec: board sizes, ingredient rules, recipe types, move budget formulas, difficulty tiers, pacing philosophy, and the milestone level design language
3. `docs/game-design/meta-systems.md` — how levels connect to potion shelf, workshop, and events (certain levels unlock specific potions)
4. `docs/liveops/event-templates.md` — event level requirements: themed ingredients, event-specific recipes, event reward potions

If `.cursor/skills/brew-level-design.md` exists, read it too — it contains learned design heuristics from previous sessions.

---

## Step 2 — Determine Scope

Before generating anything, determine what levels you're working on:

### New Campaign Levels
- Which level numbers? (e.g., levels 21-30)
- What difficulty tier should they target? Check the framework:
  - Levels 1-5: Tutorial zone (95%+ completion, heavily guided)
  - Levels 6-15: Early game (70-85% completion, introduce mechanics gradually)
  - Levels 16-30: Mid game (50-70% completion, full mechanic variety)
  - Levels 31-40: Late game (40-60% completion, mastery-level challenge)

### Modifications to Existing Levels
- Which levels are being modified and why? (too hard, too easy, broken, unfun)
- What specific parameter needs adjustment? (move limit, ingredient count, recipe targets, board size)

### Event Levels
- Which event template? (reference `docs/liveops/event-templates.md`)
- How many levels? (standard is 7 per weekly event)
- Event levels follow different rules: moderate difficulty, themed ingredients, event-specific potion rewards

---

## Step 3 — Generate Levels

Use the procedural level generator for initial level creation:

```
python scripts/level-generator/generate_level.py --level <N> --difficulty <TIER> --seed <S>
```

Parameters:
- `--level <N>`: Level number (1-40 for campaign, event_1 through event_7 for events)
- `--difficulty <TIER>`: One of `tutorial`, `easy`, `medium`, `hard`, `challenge`
- `--seed <S>`: Random seed for reproducibility. Use the level number as default seed. If the result isn't good, increment the seed and regenerate
- `--board-width <W>`: Board width (default from framework: 7 for most levels)
- `--board-height <H>`: Board height (default from framework: 9 for most levels)
- `--ingredients <N>`: Number of ingredient types (3-5, increases with difficulty)
- `--moves <N>`: Move budget override (if not set, calculated by difficulty formula)
- `--recipes <spec>`: Recipe specification (e.g., `"red:3,blue:2"` means brew 3 red potions and 2 blue)

The generator outputs a JSON level config file. Review the output before accepting it.

### Generation Rules

Follow these constraints from the level design framework:

1. **Ingredient count by tier:**
   - Tutorial/Easy: 3 ingredients
   - Medium: 4 ingredients
   - Hard/Challenge: 4-5 ingredients

2. **Board size:**
   - Standard: 7×9
   - Small (tight constraint levels): 6×8
   - Large (breather/spectacle levels): 8×10

3. **Move budget:**
   - Tutorial: generous (50%+ surplus over minimum solve)
   - Easy: comfortable (30-40% surplus)
   - Medium: fair (15-25% surplus)
   - Hard: tight (5-15% surplus)
   - Challenge: very tight (0-10% surplus)

4. **Recipe complexity by tier:**
   - Tutorial: 1 recipe, 1 potion type
   - Easy: 1-2 recipes, 1-2 potion types
   - Medium: 2-3 recipes, 2-3 potion types
   - Hard: 2-3 recipes, 3 potion types
   - Challenge: 3 recipes, 3-4 potion types

---

## Step 4 — Hand-Tune Milestone Levels

Milestone levels (every 5th level, especially 10, 20, 30, 40) require special attention. Do not use raw procedural output for these. They should feel like mini-events:

### Level 10 — First Milestone
- Introduces the 4th ingredient for the first time
- Slightly larger board (8×10) to give breathing room with the new mechanic
- Generous move budget — this is a celebration, not a wall
- On completion, unlocks a unique potion for the shelf

### Level 20 — Midpoint Milestone
- Introduces the 5th ingredient
- Complex recipes requiring all ingredient types
- Medium-tight move budget — player should feel tested but capable
- Workshop unlock tied to this level (from meta-systems.md)

### Level 30 — Late Game Gate
- Board returns to standard size (7×9) but with 5 ingredients and demanding recipes
- This is the hardest "standard" level in the game
- Move budget is tight but fair
- Players who pass this can handle anything in levels 31-39

### Level 40 — Finale
- The biggest, most satisfying level
- Large board (8×10), all 5 ingredients, multi-recipe challenge
- Move budget is tight but should feel achievable with good play
- On completion, a special "Grand Brew" potion unlocks with a unique animation
- This should feel like a culmination, not a grind. If it's frustrating, it's wrong

### Breather Levels (5, 15, 25, 35)
- Easy difficulty regardless of where they fall in the sequence
- Smaller recipe requirements, generous moves
- Purpose: let the player relax, feel successful, rebuild confidence after harder levels
- These are as important as challenge levels — they prevent fatigue

---

## Step 5 — Validate Levels

After generating or modifying any level, run validation:

```
python scripts/build/build_config.py --validate-levels
```

This script should check:
- JSON schema validity of each level config
- All referenced ingredient IDs exist in the ingredient database
- All referenced recipe IDs exist in the recipe database
- Move limit is positive and within sane bounds (10-60)
- Board dimensions are within supported range
- Ingredient count matches the number of unique ingredients in recipes
- No duplicate level numbers

Fix any validation errors before proceeding.

---

## Step 6 — Playtest

For every new or modified level, playtest it yourself (or have the agent simulate a playtest):

### Playtest Checklist

For each level:

1. **Solvability:** Can you complete the level within the move limit without boosters? If you can't solve it in 5 attempts, the move limit may be too tight or the recipe too demanding. Adjust
2. **Fun factor:** Is it fun? Does it create interesting decisions? "Tap the only available cluster" is not interesting. Multiple valid moves should exist at each turn
3. **Difficulty match:** Does the difficulty feel right for this level's position? A level 8 that plays like a level 30 is wrong
4. **Pacing flow:** Play 3 levels in sequence around the modified level. Does the transition feel smooth? No sudden spikes or drops
5. **Cascade opportunity:** Do cascades happen organically? Cascades are the most satisfying moment — levels should create conditions for them. If a level never cascades, the board may be too small or ingredients too few
6. **Booster relevance:** Would any of the 3 booster types be useful here? Good levels make boosters tempting but not required
7. **Star distribution:** Is 3-star achievable with efficient play? Is 1-star achievable with sloppy play? The gap should feel like skill expression

### Event Level Playtesting

Event levels have additional considerations:
- Theme visuals (ingredient skins, color palette) apply correctly
- Event recipes reference event-specific potion types
- Difficulty is moderate — events should be accessible to retain engagement, not gatekeep rewards
- All 7 event levels can be completed in 2-3 sessions (20-30 minutes total)

---

## Step 7 — Event Level Creation

When creating levels for a weekly event (reference `docs/liveops/event-templates.md`):

1. Read the event template to understand: theme name, ingredient skin mappings, event potion type, reward structure

2. Generate 7 event levels with the following difficulty curve:
   - Event levels 1-2: Easy (warm up, introduce theme)
   - Event levels 3-5: Medium (core challenge)
   - Event level 6: Hard (climax)
   - Event level 7: Easy-medium (victory lap, satisfying finale)

3. Use the generator with the event flag:
```
python scripts/level-generator/generate_level.py --level event_1 --difficulty easy --event-theme <THEME_ID>
```

4. Event levels should showcase the theme — if the theme is "Lunar Brew," the board should feel distinct from campaign levels through ingredient variety and recipe combinations, not just reskins

5. Save event level configs to `Assets/Resources/Events/<event_id>/`. These are loaded dynamically when the event is active via Remote Config

---

## Step 8 — Save and Document

1. Save all level configs to the appropriate directory:
   - Campaign levels: `Assets/Resources/Levels/level_<NN>.json`
   - Event levels: `Assets/Resources/Events/<event_id>/event_level_<N>.json`

2. Run validation one more time:
```
python scripts/build/build_config.py --validate-levels
```

3. Update `docs/project-management/session-handoff.md` with which levels were created/modified and any notable design decisions

4. If `.cursor/skills/brew-level-design.md` exists, add any new heuristics discovered during this session (e.g., "5-ingredient levels need at least 20 moves to feel fair," "8×10 boards cascade more naturally than 7×9")

---

## Quick Reference: Level Parameters

| Parameter | Tutorial | Easy | Medium | Hard | Challenge |
|---|---|---|---|---|---|
| Ingredients | 3 | 3 | 4 | 4-5 | 4-5 |
| Board Size | 7×9 | 7×9 | 7×9 | 7×9 or 6×8 | 7×9 or 6×8 |
| Recipes | 1 | 1-2 | 2-3 | 2-3 | 3 |
| Move Surplus | 50%+ | 30-40% | 15-25% | 5-15% | 0-10% |
| Target Completion | 95%+ | 80-90% | 60-75% | 45-60% | 35-50% |
| Cascade Frequency | High | High | Medium | Medium | Low-Medium |
