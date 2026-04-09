# Brew — Level Design Framework

## 1. Level Anatomy

Every level is defined by the following parameters:

| Parameter | Type | Description |
|---|---|---|
| `grid_width` | int (3–7) | Columns |
| `grid_height` | int (3–8) | Rows |
| `grid_mask` | bool[][] | Which cells are active (enables irregular shapes) |
| `ingredient_pool` | enum[] | Which ingredient types can spawn (subset of 5) |
| `recipe_targets` | {color, count}[] | Potions the player must brew to win |
| `move_limit` | int | Total taps allowed |
| `blockers` | {type, positions}[] | Stone and Ice tile placements |
| `star_thresholds` | {two_star, three_star} | Moves remaining for 2★ and 3★ |
| `is_hand_tuned` | bool | Whether the level is hand-crafted or procedurally generated |

A level is **won** when all recipe targets are fulfilled. A level is **lost** when the move counter hits zero with unfulfilled targets.

---

## 2. Difficulty Curve Philosophy

The 40 MVP levels follow a **staircase-with-landings** curve: difficulty increases in discrete steps, with every 5th level acting as a recovery ("breathing room") level.

```
Difficulty
  ▲
  │            ╱‾‾‾╲       ╱‾‾‾╲       ╱‾‾‾╲
  │        ╱‾‾╱     ╲ ╱‾‾‾╱     ╲ ╱‾‾‾╱     ╲ ╱‾‾‾
  │    ╱‾‾╱   ▽       ╱   ▽       ╱   ▽       ╱
  │╱‾‾╱   5   10  15  20  25  30  35  40
  └──────────────────────────────────────────▶ Level
           ▽ = breathing room level
```

### Phases

| Phase | Levels | Theme | Focus |
|---|---|---|---|
| **Tutorial** | 1–5 | Learn core mechanics | Fusing, orbs, chaining, recipes |
| **Foundation** | 6–10 | Solidify understanding | Grid grows, 4th color introduced |
| **Expansion** | 11–20 | Complexity ramp | 5th color, multi-target recipes, irregular boards |
| **Depth** | 21–30 | Blockers & strategy | Stone + Ice tiles, tight move budgets |
| **Mastery** | 31–40 | Full difficulty | All systems active, complex recipes, constrained boards |

---

## 3. Difficulty Levers

Levers are introduced in a strict sequence. A new lever is never introduced at the same time as another new lever — each gets its own "spotlight" level.

### Lever Introduction Order

| Priority | Lever | First Appearance | Description |
|---|---|---|---|
| 1 | **Grid size** | Level 1 | Start 3×3, grow to 7×8 by endgame |
| 2 | **Ingredient colors** | Level 1 | Start with 2, add 3rd at L3, 4th at L8, 5th at L13 |
| 3 | **Move budget** | Level 4 | Generous early, tightens gradually |
| 4 | **Recipe complexity** | Level 4 | Single-color → 2-color → 3-color targets |
| 5 | **Board shape** | Level 11 | Remove cells to create L-shapes, plus-shapes, etc. |
| 6 | **Stone blockers** | Level 16 | Must brew adjacent to clear |
| 7 | **Ice blockers** | Level 22 | Locks token in place for 1 turn after adjacent fusion |

### How Each Lever Affects Difficulty

**Grid size** — Smaller grids have fewer possible clusters, demanding precise play. Larger grids create more options but more chaos (harder to plan).

**Ingredient colors** — More colors = smaller average cluster size = fewer fusion opportunities per color. Going from 3 to 4 colors is roughly a 25% reduction in same-color density.

**Move budget** — The primary fine-tuning dial. Solvability testing determines the minimum moves to complete a recipe; the budget is set as a multiplier:
- Tutorial: 3.0× minimum
- Foundation: 2.0× minimum
- Expansion: 1.6× minimum
- Depth: 1.4× minimum
- Mastery: 1.2× minimum

**Recipe complexity** — Each additional recipe target roughly doubles the planning required. Multi-color recipes force the player to divide attention.

**Board shape** — Irregular shapes create dead zones where tokens pile up unpredictably. Narrow columns reduce cascade opportunities.

**Stone blockers** — Consume space on the board, reducing playable area. Require the player to brew *near* them rather than optimizing freely.

**Ice blockers** — Create timing puzzles. The player must sequence fusions to "thaw" iced tokens before they can participate in clusters.

---

## 4. Level Pacing — Element Introduction Schedule

| Level | New Element Introduced | Notes |
|---|---|---|
| 1 | Fusing, Brew Orb | Tutorial: 2 colors, tiny grid |
| 2 | Positional strategy | Tutorial: orb placement matters |
| 3 | 3rd color (Vine), chaining | Tutorial: chain-fusion mechanic |
| 4 | Recipes, move limits | Tutorial: first real goal |
| 5 | Unguided play | Tutorial: all basics, no handholding (breathing room) |
| 6 | 5×6 grid | Larger playfield |
| 7 | 2-target recipe | "Brew 1 Red and 1 Green" |
| 8 | 4th color (Sun) | Yellow joins the pool |
| 9 | Tighter moves | First level where budget feels constraining |
| 10 | Milestone level (breathing room) | Hand-tuned, celebratory, generous budget |
| 11 | Irregular board shape | First masked cells |
| 12 | 6×6 grid | Larger arena |
| 13 | 5th color (Shadow) | Purple joins — all 5 now available |
| 14 | 3-target recipe | "Brew 1 Red, 1 Blue, 1 Green" |
| 15 | Milestone level (breathing room) | Hand-tuned recovery |
| 16 | Stone blockers (2 tiles) | First blocker type |
| 17 | Stone blockers (4 tiles) | More stones, non-symmetric placement |
| 18 | Irregular board + stones | Combined difficulty |
| 19 | 3-target recipe + stones | Combined difficulty |
| 20 | Milestone level (breathing room) | Hand-tuned, no blockers, generous |
| 21 | 6×7 grid | Larger board with all 5 colors |
| 22 | Ice blockers (2 tiles) | Second blocker type |
| 23 | Ice + stone combined | Both blockers on one board |
| 24 | Quantity recipes | "Brew 2 Red" — must brew same color twice |
| 25 | Milestone level (breathing room) | Hand-tuned recovery |
| 26 | 7×7 grid, tight moves | Near-maximum board with pressure |
| 27 | Complex recipe + both blockers | 3 targets + stones + ice |
| 28 | Narrow irregular board | Tall and thin (3×8) — limits horizontal clustering |
| 29 | Highest move pressure yet | Budget at 1.3× minimum |
| 30 | Milestone level (breathing room) | Hand-tuned, satisfying to solve |
| 31–34 | Escalating combinations | Mix all levers, increasing pressure |
| 35 | Milestone level (breathing room) | Hand-tuned recovery |
| 36–39 | Peak difficulty | All 5 colors, both blockers, irregular boards, 3-target recipes, tight budgets |
| 40 | Final milestone | Hand-tuned capstone level, challenging but fair |

---

## 5. Breathing Room Levels

Every 5th level (5, 10, 15, 20, 25, 30, 35) is intentionally easier than the surrounding levels. Purpose:

- **Emotional recovery** after a difficulty spike
- **Confidence reinforcement** — the player feels mastery
- **Retention checkpoint** — players who reach a breathing room level are less likely to churn
- **Mechanic consolidation** — lets the player practice recent mechanics without pressure

Design rules for breathing room levels:
- Move budget at 2.5× minimum or higher
- No new mechanics introduced
- Recipe has fewer targets than the player has recently seen
- Board shape is simple (rectangular, no irregular masks)
- No blockers (or minimal blockers if the player has seen them)

---

## 6. Procedural vs Hand-Crafted Levels

### Hand-Tuned Levels (Milestone Levels)

Levels **1, 5, 10, 15, 20, 25, 30, 35, 40** are hand-designed. These levels:
- Have curated initial board states
- Are playtested for specific emotional beats
- Serve as quality gates for the experience
- Tutorial levels (1–5) are all hand-tuned

### Procedurally Generated Levels

All other levels are generated using the procedural system. The generator takes the level's parameters (from the table in §8) and produces a valid initial board state.

---

## 7. Procedural Level Generation

### Input Parameters

The generator receives: `grid_size`, `grid_mask`, `ingredient_pool`, `recipe_targets`, `move_limit`, `blocker_placements`.

### Generation Algorithm

**Step 1 — Populate grid**
Fill all active cells with random ingredients from the pool. Use weighted distribution:
- Recipe-target colors: 25% each (split evenly)
- Non-target colors: fill remaining probability equally
- Minimum 15% for any active color to prevent starvation

**Step 2 — Guarantee minimum clusters**
Scan the board for clusters of 3+. If fewer than `floor(move_limit / 3)` clusters exist, swap tokens to create clusters. Prioritize creating clusters of recipe-target colors.

Target cluster counts by phase:
- Tutorial/Foundation: ≥ 5 clusters visible at start
- Expansion: ≥ 4 clusters
- Depth/Mastery: ≥ 3 clusters

**Step 3 — Place blockers**
Position Stone and Ice tiles according to level spec. Blockers must not:
- Occupy the same cell
- Block more than 30% of any single column (prevents softlock from gravity starvation)
- Create a fully blocked row (no tokens can pass through)

**Step 4 — Solvability heuristic**
Run a greedy simulation (100 iterations):
1. Pick the largest cluster of any recipe-target color
2. Fuse it
3. Apply gravity, fill from top
4. Repeat until recipe complete or moves exhausted

Pass criteria: ≥ 60% of simulations complete the recipe within `move_limit`.

If the board fails solvability, re-run from Step 1 (max 50 attempts before flagging for manual review).

**Step 5 — Star threshold calibration**
Run 1000 greedy simulations. Set thresholds:
- 2★: moves remaining ≥ median remaining moves across successful runs
- 3★: moves remaining ≥ 75th percentile remaining moves

### Seed Control

Every procedural level stores its generation seed. This enables:
- Deterministic regeneration for debugging
- A/B testing of alternative boards for the same level parameters
- Consistent experience across devices

---

## 8. Difficulty Parameters — All 40 MVP Levels

### Levels 1–10: Tutorial & Foundation

| Lv | Grid | Colors | Moves | Recipe | Blockers | Stars (2★/3★) | Type | Notes |
|---|---|---|---|---|---|---|---|---|
| 1 | 3×3 | 2 (Ember, Frost) | 15 | Brew 1 Red | — | 10/12 remaining | Hand | Tutorial: learn fusing |
| 2 | 4×4 | 2 (Ember, Frost) | 12 | Brew 1 Blue | — | 6/9 remaining | Hand | Tutorial: positional choice |
| 3 | 4×5 | 3 (Ember, Frost, Vine) | 14 | Brew 1 Green | — | 7/10 remaining | Hand | Tutorial: chains |
| 4 | 5×5 | 3 | 12 | Brew 1 Red | — | 5/8 remaining | Hand | Tutorial: recipes + moves |
| 5 | 5×6 | 3 | 14 | Brew 1 Red, 1 Blue | — | 6/9 remaining | Hand | Tutorial: unguided (breathing) |
| 6 | 5×6 | 3 | 12 | Brew 1 Red, 1 Green | — | 5/7 remaining | Proc | Larger grid |
| 7 | 5×6 | 3 | 14 | Brew 1 Red, 1 Green | — | 5/8 remaining | Proc | 2-target practice |
| 8 | 5×7 | 4 (+Sun) | 16 | Brew 1 Yellow | — | 7/10 remaining | Proc | 4th color intro |
| 9 | 5×7 | 4 | 12 | Brew 1 Red, 1 Yellow | — | 4/6 remaining | Proc | Tighter budget |
| 10 | 5×6 | 3 | 18 | Brew 1 Blue | — | 10/14 remaining | Hand | Milestone (breathing) |

### Levels 11–20: Expansion

| Lv | Grid | Colors | Moves | Recipe | Blockers | Stars (2★/3★) | Type | Notes |
|---|---|---|---|---|---|---|---|---|
| 11 | 5×6 (L-shape) | 4 | 14 | Brew 1 Red, 1 Blue | — | 5/8 remaining | Proc | First irregular board |
| 12 | 6×6 | 4 | 14 | Brew 1 Yellow, 1 Green | — | 5/7 remaining | Proc | Larger arena |
| 13 | 6×6 | 5 (+Shadow) | 16 | Brew 1 Purple | — | 7/10 remaining | Proc | 5th color intro |
| 14 | 6×6 | 5 | 16 | Brew 1 Red, 1 Blue, 1 Green | — | 5/8 remaining | Proc | First 3-target |
| 15 | 5×6 | 3 | 18 | Brew 1 Green, 1 Blue | — | 8/12 remaining | Hand | Milestone (breathing) |
| 16 | 6×6 | 4 | 14 | Brew 1 Red, 1 Blue | 2 Stone | 4/7 remaining | Proc | Stone blocker intro |
| 17 | 6×7 | 4 | 16 | Brew 1 Yellow, 1 Green | 4 Stone | 5/8 remaining | Proc | More stones |
| 18 | 6×6 (plus-shape) | 4 | 14 | Brew 1 Red, 1 Blue | 3 Stone | 4/6 remaining | Proc | Irregular + stones |
| 19 | 6×7 | 5 | 16 | Brew 1 Red, 1 Blue, 1 Purple | 3 Stone | 4/7 remaining | Proc | 3-target + stones |
| 20 | 5×6 | 3 | 20 | Brew 1 Red | — | 12/16 remaining | Hand | Milestone (breathing) |

### Levels 21–30: Depth

| Lv | Grid | Colors | Moves | Recipe | Blockers | Stars (2★/3★) | Type | Notes |
|---|---|---|---|---|---|---|---|---|
| 21 | 6×7 | 5 | 16 | Brew 1 Red, 1 Green, 1 Purple | 2 Stone | 4/7 remaining | Proc | Larger board, all colors |
| 22 | 6×7 | 4 | 14 | Brew 1 Blue, 1 Yellow | 2 Ice | 4/6 remaining | Proc | Ice blocker intro |
| 23 | 6×7 | 5 | 16 | Brew 1 Red, 1 Blue | 2 Stone, 2 Ice | 4/7 remaining | Proc | Both blockers |
| 24 | 6×6 | 4 | 16 | Brew 2 Red | 2 Stone | 5/8 remaining | Proc | Quantity recipe |
| 25 | 5×7 | 3 | 18 | Brew 1 Blue, 1 Green | — | 8/12 remaining | Hand | Milestone (breathing) |
| 26 | 7×7 | 5 | 14 | Brew 1 Red, 1 Blue, 1 Green | 3 Stone | 3/5 remaining | Proc | Max grid, tight |
| 27 | 6×7 | 5 | 16 | Brew 1 Red, 1 Purple, 1 Yellow | 2 Stone, 2 Ice | 4/6 remaining | Proc | Full complexity |
| 28 | 3×8 | 4 | 14 | Brew 1 Blue, 1 Yellow | 2 Ice | 4/6 remaining | Proc | Narrow board |
| 29 | 7×7 | 5 | 12 | Brew 2 Red, 1 Blue | 3 Stone, 1 Ice | 2/4 remaining | Proc | Tightest budget yet |
| 30 | 6×6 | 4 | 20 | Brew 1 Red, 1 Blue | — | 10/14 remaining | Hand | Milestone (breathing) |

### Levels 31–40: Mastery

| Lv | Grid | Colors | Moves | Recipe | Blockers | Stars (2★/3★) | Type | Notes |
|---|---|---|---|---|---|---|---|---|
| 31 | 7×7 | 5 | 14 | Brew 1 Red, 1 Blue, 1 Green | 3 Stone, 2 Ice | 3/5 remaining | Proc | Full loadout |
| 32 | 6×7 (T-shape) | 5 | 14 | Brew 2 Blue, 1 Purple | 2 Stone, 2 Ice | 3/5 remaining | Proc | Irregular + all blockers |
| 33 | 7×8 | 5 | 16 | Brew 1 Red, 1 Blue, 1 Green | 4 Stone, 2 Ice | 3/6 remaining | Proc | Max grid |
| 34 | 6×6 (cross-shape) | 5 | 12 | Brew 2 Red, 1 Purple | 3 Stone, 3 Ice | 2/4 remaining | Proc | Constrained + tight |
| 35 | 6×6 | 4 | 18 | Brew 1 Red, 1 Yellow | 2 Stone | 8/12 remaining | Hand | Milestone (breathing) |
| 36 | 7×7 (diamond) | 5 | 14 | Brew 1 Red, 1 Blue, 1 Yellow | 4 Stone, 2 Ice | 2/5 remaining | Proc | Diamond shape |
| 37 | 7×8 | 5 | 14 | Brew 2 Green, 1 Purple | 3 Stone, 3 Ice | 2/4 remaining | Proc | High complexity |
| 38 | 6×7 (hourglass) | 5 | 12 | Brew 1 Red, 1 Blue, 1 Green | 4 Stone, 3 Ice | 1/3 remaining | Proc | Hourglass shape |
| 39 | 7×8 | 5 | 12 | Brew 2 Red, 1 Blue, 1 Purple | 4 Stone, 4 Ice | 1/3 remaining | Proc | Peak difficulty |
| 40 | 7×7 | 5 | 16 | Brew 1 Red, 1 Blue, 1 Green, 1 Yellow | 3 Stone, 2 Ice | 3/6 remaining | Hand | Capstone milestone |

---

## 9. Star Rating System

Stars reward efficient play and drive replay.

| Stars | Condition | Player Signal |
|---|---|---|
| ★☆☆ | Complete all recipe targets | "You did it" |
| ★★☆ | Complete with ≥ N moves remaining | "Well done" |
| ★★★ | Complete with ≥ M moves remaining (M > N) | "Perfect brew" |

### Threshold Calibration

For hand-tuned levels, thresholds are set by the designer after playtesting.

For procedural levels, thresholds are derived from simulation (see §7 Step 5). General guidelines:
- 2★ should be achievable by ~60% of players who complete the level
- 3★ should be achievable by ~20% of players on first attempt

### Star Gate (Future)

Stars can gate world unlocks in future updates (e.g., "Collect 30 stars to unlock World 2"). Not implemented in MVP but star data must be persisted.

---

## 10. Level Fail State

When the player runs out of moves with incomplete recipe targets:

### Fail Screen Content

```
┌──────────────────────────────────┐
│                                  │
│   So close!                      │
│                                  │
│   You were 2 fusions away from   │
│   completing Ember Elixir!       │
│                                  │
│   ┌─────────────────────────┐    │
│   │  +3 Moves  ▶  Continue  │    │
│   └─────────────────────────┘    │
│                                  │
│       ↻ Retry       ✕ Exit       │
│                                  │
└──────────────────────────────────┘
```

### Logic

1. Calculate `remaining_fusions`: how many more brews are needed to satisfy all remaining recipe targets
2. Display: *"You were {remaining_fusions} fusion(s) away from completing {Potion Name}!"*
3. Offer **+3 Moves** (costs 1 Extra Moves booster, or triggers IAP prompt if none owned)
4. **Retry** restarts the level with the same board seed
5. **Exit** returns to level select

### Design Intent

- The "X fusions away" message creates a near-miss effect that motivates retry
- +3 moves is deliberately small — it converts players who were genuinely close, but doesn't rescue lost causes
- Showing the potion name personalizes the failure ("you almost had the *Frost Tonic*!")

---

## 11. Board Shape Masks — Reference Catalog

Shapes used across levels 11–40:

```
L-Shape (L11)        Plus (L18)         T-Shape (L32)
■ ■ ■ ■ ■ ·         · ■ ■ ■ ■ ·       ■ ■ ■ ■ ■ ■
■ ■ ■ ■ ■ ·         ■ ■ ■ ■ ■ ■       · ■ ■ ■ ■ ·
■ ■ ■ ■ ■ ·         ■ ■ ■ ■ ■ ■       · ■ ■ ■ ■ ·
■ ■ ■ ■ ■ ■         ■ ■ ■ ■ ■ ■       · ■ ■ ■ ■ ·
■ ■ ■ ■ ■ ■         · ■ ■ ■ ■ ·       · ■ ■ ■ ■ ·
■ ■ ■ ■ ■ ■                            · ■ ■ ■ ■ ·
                                        · ■ ■ ■ ■ ·

Cross (L34)          Diamond (L36)      Hourglass (L38)
· · ■ ■ · ·         · · · ■ · · ·     ■ ■ ■ ■ ■ ■
· ■ ■ ■ ■ ·         · · ■ ■ ■ · ·     · ■ ■ ■ ■ ·
■ ■ ■ ■ ■ ■         · ■ ■ ■ ■ ■ ·     · · ■ ■ · ·
■ ■ ■ ■ ■ ■         ■ ■ ■ ■ ■ ■ ■     · · ■ ■ · ·
· ■ ■ ■ ■ ·         · ■ ■ ■ ■ ■ ·     · ■ ■ ■ ■ ·
· · ■ ■ · ·         · · ■ ■ ■ · ·     ■ ■ ■ ■ ■ ■
                     · · · ■ · · ·     ■ ■ ■ ■ ■ ■
```

`■` = active cell, `·` = masked (inactive)

---

## 12. Blocker Tile Specifications

### Stone

- **Visual**: Grey rock filling the cell
- **Behavior**: Occupies a cell permanently until cleared. Tokens cannot occupy this cell. Tokens fall around it (gravity skips stone cells).
- **Clearing**: When any Brew Orb adjacent (4-directional) to a Stone tile triggers a brew, the Stone is destroyed. One brew clears one Stone.
- **Placement rules**: Never in the top row (would block token entry). Never adjacent to another Stone (prevents impossible clusters). Maximum 4 per level in Expansion, 6 in Mastery.

### Ice

- **Visual**: Frosted crystal overlay on a normal token
- **Behavior**: The token underneath is visible but locked — it cannot participate in cluster detection or be tapped. After any fusion occurs in an adjacent cell (4-directional), the ice cracks and is removed at the start of the next turn. The token is then free.
- **Placement rules**: Never on more than 2 tokens of the same color (prevents recipe-color starvation). Never in corners (too hard to reach). Maximum 4 per level.

---

## 13. Booster Integration with Level Design

Boosters are balanced against level difficulty:

| Booster | Effect | Design Constraint |
|---|---|---|
| **Shake** | Randomizes all token positions (preserves types) | Must re-run cluster guarantee after shuffle |
| **Catalyst** | Auto-fuses the largest cluster on the board | Level budget assumes 0 Catalyst uses; Catalyst is pure bonus |
| **Extra Moves** | +5 moves added to remaining count | Star thresholds are NOT recalculated when boosters are used |

Levels should be completable without boosters. Boosters are an accessibility valve, not a requirement.

---

## 14. Level Data Schema (Implementation Reference)

```json
{
  "level_id": 16,
  "grid_width": 6,
  "grid_height": 6,
  "grid_mask": [
    [1,1,1,1,1,1],
    [1,1,1,1,1,1],
    [1,1,1,1,1,1],
    [1,1,1,1,1,1],
    [1,1,1,1,1,1],
    [1,1,1,1,1,1]
  ],
  "ingredient_pool": ["ember", "frost", "vine", "sun"],
  "recipe": [
    {"color": "ember", "count": 1},
    {"color": "frost", "count": 1}
  ],
  "move_limit": 14,
  "blockers": [
    {"type": "stone", "row": 2, "col": 1},
    {"type": "stone", "row": 3, "col": 4}
  ],
  "star_thresholds": {"two_star": 4, "three_star": 7},
  "seed": null,
  "hand_tuned": false,
  "initial_board": null
}
```

For hand-tuned levels, `initial_board` contains the exact token layout and `seed` is null. For procedural levels, `seed` is set and `initial_board` is null (generated at build time or on first load).
