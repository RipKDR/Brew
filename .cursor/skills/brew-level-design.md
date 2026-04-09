---
description: Design and validate Brew puzzle levels. Use when creating new levels, adjusting difficulty, or debugging level solvability.
---

# Skill: Brew Level Design

## When to Use

- Creating a new level (main campaign, daily, or event)
- Adjusting difficulty of an existing level
- Debugging a level that players report as unsolvable or trivially easy
- Creating event-themed level variants
- Validating a level before it ships

## Level JSON Format

Every level is defined as JSON matching this schema (full spec: `docs/technical/data-model.md` §3):

```json
{
  "level_id": 16,
  "level_type": "normal",
  "grid_width": 6,
  "grid_height": 6,
  "grid_mask": [[1,1,1,1,1,1], ...],
  "ingredient_pool": ["ember", "frost", "vine", "sun"],
  "ingredient_weights": {"ember": 1.0, "frost": 1.0, "vine": 0.8, "sun": 0.6},
  "recipe_targets": [
    {"potion_id": "healing_draught", "count": 2},
    {"potion_id": "fire_elixir", "count": 1}
  ],
  "move_limit": 14,
  "blocker_placements": [
    {"type": "stone", "x": 1, "y": 2, "hp": 1},
    {"type": "ice", "x": 4, "y": 3, "hp": 1}
  ],
  "star_thresholds": [1000, 3000, 6000],
  "seed_mode": "random",
  "fixed_seed": null,
  "is_tutorial": false,
  "tutorial_steps": [],
  "initial_board": null
}
```

Key fields:
- `grid_mask`: 2D array of 1s and 0s. 1 = active cell, 0 = blocked. Enables irregular board shapes.
- `ingredient_pool`: Subset of the 5 token types (`ember`, `frost`, `vine`, `sun`, `shadow`).
- `ingredient_weights`: Spawn probability. Higher weight = more frequent. Omit for uniform distribution.
- `recipe_targets`: Potions the player must brew to win. Each entry has a `potion_id` (matches a color) and `count`.
- `blocker_placements`: Pre-placed Stone and Ice blockers with HP values.
- `star_thresholds`: Score needed for 1-star, 2-star, 3-star.
- `initial_board`: If non-null, the exact starting token layout. Used for hand-tuned levels.

## Difficulty Parameter Ranges

| Parameter | Easy | Medium | Hard | Extreme |
|-----------|------|--------|------|---------|
| Grid size | 3×3 to 5×5 | 5×6 to 6×6 | 6×7 to 7×7 | 7×8+ or narrow (3×8) |
| Colors | 2–3 | 3–4 | 4–5 | 5 |
| Move budget | ≥ 2.5× minimum | 2.0× minimum | 1.4–1.6× minimum | 1.2× minimum |
| Recipe targets | 1 (single color) | 1–2 | 2–3 | 3–4 or quantity (brew 2+ of same) |
| Stone blockers | 0 | 0–2 | 2–4 | 4–6 |
| Ice blockers | 0 | 0 | 0–2 | 2–4 |
| Board shape | Rectangular | Rectangular | Irregular (L, plus) | Complex (cross, diamond, hourglass) |

## How Each Lever Affects Difficulty

- **Grid size:** Smaller grids → fewer possible clusters → demands precision. Larger grids → more options but more chaos.
- **Colors:** Each additional color reduces same-color density by ~20–25%. Going from 3 to 5 colors roughly halves the average cluster frequency per color.
- **Move budget:** The primary fine-tuning dial. Set as a multiplier of the minimum moves needed to complete the recipe. See the multiplier table by phase in `docs/game-design/level-design-framework.md` §3.
- **Recipe complexity:** Each additional target roughly doubles the planning load. Quantity recipes (brew 2+ of same color) require sustained focus on one color.
- **Board shape:** Irregular shapes create dead zones and restrict cascade flow. Narrow columns limit horizontal clustering.
- **Stone blockers:** Consume board space and force brews near them to clear. Never place in top row or adjacent to another stone.
- **Ice blockers:** Lock tokens until an adjacent fusion thaws them. Create timing puzzles. Never on more than 2 tokens of the same color.

## Solvability Validation

A level must be validated as solvable before shipping. Use this heuristic:

### Quick Check (minimum viable)

For each recipe target color, verify:
```
initial_tokens_of_color + expected_refills_of_color >= recipe_target_count × brew_threshold × 2
```

Where:
- `brew_threshold` = 6 (tokens needed to brew)
- `expected_refills` ≈ (move_limit × avg_tokens_consumed_per_move) × spawn_weight_for_color
- The ×2 factor accounts for clustering inefficiency (not all tokens form useful clusters)

### Simulation Check (recommended)

Run 100+ greedy simulations:
1. On each iteration, pick the largest cluster of any recipe-target color
2. Fuse it (place orb at bottom-most cell of cluster)
3. Apply gravity, refill, resolve cascades
4. Repeat until recipe complete or moves exhausted

**Pass criteria:** ≥ 60% of simulations complete the recipe within `move_limit`.

If the pass rate is below 60%, the level is too hard. If above 90%, it may be too easy (unless it's a breathing room level).

### Softlock Check

Verify there is no board configuration where:
- All remaining tokens of a recipe-target color are permanently inaccessible (blocked by stones with no adjacent brew path)
- Blockers consume > 30% of any single column (prevents gravity starvation)
- A fully blocked row exists (no tokens can pass through)

## Adjusting Difficulty

### To make a level harder:
1. Reduce `move_limit` (most impactful, least disruptive)
2. Add a color to `ingredient_pool`
3. Add recipe targets (e.g., 2-target → 3-target)
4. Add stone or ice blockers
5. Shrink or irregularize the grid (mask cells)
6. Add quantity to recipe targets (e.g., "Brew 2 Red" instead of "Brew 1 Red")

### To make a level easier:
1. Increase `move_limit`
2. Remove a color from `ingredient_pool`
3. Reduce recipe targets
4. Remove blockers
5. Expand the grid or make it rectangular
6. Increase spawn weights for recipe-target colors

### Fine-tuning order:
Always adjust `move_limit` first — it's the cheapest change. Only modify structural elements (grid, colors, blockers) if move_limit alone can't achieve the target difficulty.

## Level Pacing Rules

The 40 MVP levels follow a staircase-with-landings curve:

- **Every 5th level is a breathing room level** (5, 10, 15, 20, 25, 30, 35). These must be:
  - Move budget ≥ 2.5× minimum
  - No new mechanics introduced
  - Fewer recipe targets than recent levels
  - Simple rectangular board
  - No or minimal blockers
- **New mechanics get a spotlight level** — never introduce two new elements on the same level.
- **Difficulty increases between landings**, then drops at each breathing room level.

### Phase pacing:

| Phase | Levels | Move budget multiplier | Max colors | Max blockers |
|-------|--------|----------------------|------------|-------------|
| Tutorial | 1–5 | ≥ 3.0× | 3 | 0 |
| Foundation | 6–10 | ≥ 2.0× | 4 | 0 |
| Expansion | 11–20 | ≥ 1.6× | 5 | 4 Stone |
| Depth | 21–30 | ≥ 1.4× | 5 | 3 Stone + 2 Ice |
| Mastery | 31–40 | ≥ 1.2× | 5 | 4 Stone + 4 Ice |

## Creating Event-Themed Levels

Event levels reuse the core mechanics but with themed presentation:

1. **Keep the same JSON schema.** Event levels use `level_type: "event"`.
2. **Swap ingredient names/colors** in display config (e.g., "Ember" → "Pumpkin Spice" for Halloween). The underlying `ingredient_pool` enum values stay the same — only the visual mapping changes.
3. **Adjust recipe targets** to use event-specific potion IDs (defined in the event's Remote Config entry).
4. **Set `seed_mode: "fixed"`** so all players get the same board. Use a specific `fixed_seed` value.
5. **Target 7 levels per event** (matching the weekly event structure). Difficulty should follow a compressed version of the main curve: levels 1–2 easy, 3–5 medium, 6 hard, 7 climax.
6. **Event levels should be completable without boosters** by a player who has reached level 10+ in the main campaign.

## Reference

- Full level parameter table (all 40 MVP levels): `docs/game-design/level-design-framework.md` §8
- Board shape mask catalog: `docs/game-design/level-design-framework.md` §11
- Blocker specifications: `docs/game-design/level-design-framework.md` §12
- Level JSON schema: `docs/technical/data-model.md` §3
- Core mechanic rules (fusion, chains, cascades, brew threshold): `docs/game-design/core-mechanic.md`
