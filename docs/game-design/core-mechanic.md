# Brew — Core Mechanic Technical Specification

**Version:** 1.0.0
**Last Updated:** 2026-04-09
**Status:** Pre-production
**Audience:** Engineering, QA, and Technical Design

---

## 1. Grid Specification

### 1.1 Coordinate System

- Origin `(0, 0)` is the **top-left** cell of the grid.
- **Column** index increases left-to-right: `0` to `width - 1`.
- **Row** index increases top-to-bottom: `0` to `height - 1`.
- Gravity pulls cell contents toward **higher row indices** (downward on screen).

### 1.2 Default Dimensions

| Property | Default | Range |
|---|---|---|
| `grid_width` | 7 | 5–9 |
| `grid_height` | 9 | 5–11 |
| Total cells | 63 | 25–99 |

Dimensions are defined per level in the level config.

### 1.3 Cell Structure

Each cell at `(col, row)` holds exactly one of:

| Content | Description |
|---|---|
| `TOKEN` | A standard ingredient token with a `color` property. |
| `ORB` | A Brew Orb with `color` and `token_count` properties. |
| `EMPTY` | No content. Subject to gravity refill. |

There are no blocked/wall cells in MVP. The grid is always a full rectangle.

### 1.4 Adjacency

Two cells are **adjacent** if and only if they share an edge (orthogonal / Von Neumann neighborhood):

```
         (c, r-1)
(c-1, r)  [cell]  (c+1, r)
         (c, r+1)
```

Diagonal cells are **not** adjacent. Adjacency does not wrap around grid edges.

---

## 2. Token Types

### 2.1 Token Definitions

| ID | Name | Color | Hex | Shape Silhouette |
|---|---|---|---|---|
| `EMBER` | Ember | Red | `#E04040` | Flame / upward triangle |
| `FROST` | Frost | Blue | `#4080E0` | Snowflake / hexagon |
| `VINE` | Vine | Green | `#40B040` | Leaf / organic curve |
| `SUN` | Sun | Yellow | `#E0C020` | Star / radiating spokes |
| `SHADOW` | Shadow | Purple | `#8040C0` | Crescent / wisp |

### 2.2 Token States

```
IDLE ──tap──▶ FUSING ──complete──▶ (destroyed)
  │                                    
  └──gravity──▶ DROPPING ──land──▶ IDLE
  │
  └──spawn──▶ ENTERING ──land──▶ IDLE
```

| State | Duration | Interaction |
|---|---|---|
| `IDLE` | Indefinite | Tappable. Participates in cluster detection. |
| `FUSING` | 200ms | Not tappable. Animating toward orb center. |
| `DROPPING` | 50ms per row traveled + 30ms bounce | Not tappable. Moving downward due to gravity. |
| `ENTERING` | Same as `DROPPING` | Not tappable. Spawning from above grid top. |

### 2.3 Brew Orb States

```
CREATED ──check──▶ CHAIN_FUSING ──complete──▶ (merged into another orb)
   │                     │
   │                     └──token_count ≥ 6──▶ BREWING ──complete──▶ (destroyed)
   │
   └──settle──▶ ORB_IDLE
   │
   └──gravity──▶ ORB_DROPPING ──land──▶ ORB_IDLE ──adjacency check──▶ CHAIN_FUSING
```

| State | Duration | Interaction |
|---|---|---|
| `ORB_IDLE` | Indefinite | Not directly tappable. Participates in chain-fusion checks. |
| `CHAIN_FUSING` | 300ms | Animating merge into target orb cell. |
| `BREWING` | 1500ms total (600ms burst + 400ms transform + 500ms fly-to-vial) | Not interactive. Board paused. |
| `ORB_DROPPING` | Same timing as token drop | Not interactive. Subject to post-land adjacency check. |

---

## 3. Cluster Detection

### 3.1 Definition

A **cluster** is a maximal connected component of cells containing `TOKEN` content of the same color, connected by orthogonal adjacency.

### 3.2 Algorithm

Use flood-fill (BFS or DFS) from any seed cell:

```
function find_cluster(grid, col, row):
    color = grid[col][row].color
    visited = empty set
    queue = [(col, row)]
    cluster = []

    while queue is not empty:
        (c, r) = queue.pop()
        if (c, r) in visited: continue
        if out_of_bounds(c, r): continue
        if grid[c][r].type != TOKEN: continue
        if grid[c][r].color != color: continue

        visited.add((c, r))
        cluster.append((c, r))
        queue.push((c-1, r), (c+1, r), (c, r-1), (c, r+1))

    return cluster
```

### 3.3 Valid Cluster

A cluster is **valid** (tappable) if `len(cluster) >= 3`.

### 3.4 Board Cluster Map

On every board-stable state, precompute all clusters and cache:

- List of all clusters with size ≥ 3 (for highlight rendering and deadlock detection).
- Per-cell lookup: which cluster (if any) each cell belongs to.

Invalidate and recompute after every gravity + refill settle.

---

## 4. Player Fusion

### 4.1 Input Handling

1. Player taps cell `(tc, tr)`.
2. If `grid[tc][tr].type != TOKEN` → reject tap (no-op, shake animation).
3. Look up the cluster containing `(tc, tr)`.
4. If `cluster.size < 3` → reject tap (no-op, shake animation).
5. **Valid tap** — proceed to fusion.

### 4.2 Fusion Execution

Given valid tap at `(tc, tr)` with cluster `C` of color `K`:

```
1. Decrement move_counter by 1.
2. Set all cells in C to FUSING state (animation begins).
3. After 200ms (fusion animation complete):
   a. Remove all cells in C from grid (set to EMPTY).
   b. Place ORB at (tc, tr) with:
      - color = K
      - token_count = len(C)
      - state = CREATED
4. Enter Resolution Phase (Section 7).
```

### 4.3 Orb Placement Rule

The orb always appears at the cell the player tapped `(tc, tr)`. This is the core of the "positional strategy" — the player controls where the orb lands by choosing which cell in the cluster to tap.

---

## 5. Chain Fusion

### 5.1 Trigger Condition

Chain fusion is checked whenever an orb enters `ORB_IDLE` state at a cell (after creation, after gravity drop, or after a cascade creates a new orb).

### 5.2 Chain Detection

```
function find_chain_group(grid, col, row):
    orb = grid[col][row]  // must be ORB type
    color = orb.color
    visited = empty set
    queue = [(col, row)]
    group = []

    while queue is not empty:
        (c, r) = queue.pop()
        if (c, r) in visited: continue
        if out_of_bounds(c, r): continue
        if grid[c][r].type != ORB: continue
        if grid[c][r].color != color: continue

        visited.add((c, r))
        group.append((c, r))
        queue.push((c-1, r), (c+1, r), (c, r-1), (c, r+1))

    return group  // all connected same-color orbs
```

If `group.size >= 2`, a chain fusion triggers.

### 5.3 Chain Fusion Execution

Given a chain group of orbs `G` (size ≥ 2):

```
1. Determine survivor cell: the cell in G with the highest row index
   (bottom-most). Break ties by lowest column index (left-most).

2. Compute merged token_count = sum of token_count for all orbs in G.

3. Set all non-survivor orbs to CHAIN_FUSING state,
   animating toward the survivor cell (300ms).

4. After animation:
   a. Remove all non-survivor orbs (set cells to EMPTY).
   b. Update survivor orb: token_count = merged total.
   c. Increment chain_step_counter by 1 (for scoring).

5. If survivor.token_count >= BREW_THRESHOLD (6):
   a. Trigger Brew (Section 6).
   b. After brew completes, survivor cell becomes EMPTY.

6. Else: survivor enters ORB_IDLE.
   a. Re-check chain fusion at survivor cell
      (in case the merge moved it adjacent to another same-color orb — 
       this is rare but possible if orbs were in an L-shape).
```

### 5.4 Chain Step Counter

The `chain_step_counter` tracks how many chain-fusion merge events occur in a single resolution sequence (from one player tap to board-stable). It resets to 0 at the start of each player tap. Used for scoring multipliers.

---

## 6. Brewing

### 6.1 Brew Threshold

An orb brews when its `token_count >= 6`.

This can happen:

- Immediately on creation (player fuses a cluster of 6+ tokens).
- After chain fusion merges orbs to a combined count ≥ 6.

### 6.2 Brew Execution

```
1. Orb enters BREWING state.
2. Brew Burst animation at orb cell (600ms):
   - Expanding particle ring in orb color.
   - Screen shake (subtle, optional — respects reduce-motion setting).
3. Transform animation (400ms):
   - Orb morphs into completed potion bottle icon.
4. Fly-to-vial animation (500ms):
   - Potion bottle arcs from cell position to the HUD recipe vial.
   - If this brew matches a recipe color that still needs filling:
     a. Vial fills with liquid (color-tinted).
     b. Recipe counter for that color decrements.
   - If this brew does NOT match any unfilled recipe color:
     a. Potion bottle pops with a smaller celebration.
     b. No vial fills.
5. Orb cell becomes EMPTY.
6. Award brew score (200 for target color, 100 for non-target).
7. Check win condition:
   - If all recipe vials are filled → queue WIN (after current resolution completes).
```

### 6.3 Multiple Simultaneous Brews

If multiple orbs reach brew threshold in the same resolution step (e.g., two separate chain fusions resolve simultaneously), process them sequentially:

- **Order:** Lowest row first (bottom of screen). Break ties by lowest column (left-most).
- Each brew animation plays fully before the next begins.
- All brews complete before gravity runs for the empty cells they leave behind.

---

## 7. Resolution Phase

The Resolution Phase is the deterministic sequence of events following a player tap (or cascade auto-fuse). No player input is accepted during resolution.

### 7.1 Resolution Pipeline

```
function resolve(grid, trigger_cell, is_player_move):
    cascade_wave = 0

    // Step 1: Chain fusion at trigger cell
    chain_fuse_all(grid)

    loop:
        // Step 2: Gravity
        apply_gravity(grid)

        // Step 3: Refill
        refill_top(grid)

        // Step 4: Settle
        wait_for_settle(grid)

        // Step 5: Post-settle chain check
        // (Orbs may have become adjacent after dropping)
        chain_fuse_all(grid)

        // Step 6: Cascade detection
        cascades = find_all_valid_clusters(grid)
        if cascades is empty:
            break  // board is stable

        // Step 7: Auto-fuse cascades
        cascade_wave += 1
        for each cluster in cascades (process bottom-to-top, left-to-right):
            auto_fuse(grid, cluster, cascade_wave)

        // Step 8: Chain fusion on newly created orbs
        chain_fuse_all(grid)

        // Loop back to Step 2 (gravity for newly emptied cells)

    // Resolution complete
    check_win_condition()
    check_deadlock()
    return_control_to_player()
```

### 7.2 `chain_fuse_all`

Scan the entire grid for any orb that is adjacent to a same-color orb. Group all connected same-color orbs (using the algorithm from Section 5.2). For each group of size ≥ 2, execute chain fusion (Section 5.3). Process groups bottom-to-top, left-to-right. After all groups are processed, re-scan — new adjacencies may have formed due to merges. Repeat until no adjacent same-color orb pairs remain.

### 7.3 `auto_fuse`

For a cascade cluster `C` of tokens with color `K`:

```
1. Determine orb placement: the cell in C with the highest row index
   (bottom-most). Break ties by lowest column index (left-most).

2. Set all cells in C to FUSING state (200ms animation).

3. After animation:
   a. Remove all cells in C (set to EMPTY).
   b. Place ORB at determined cell with:
      - color = K
      - token_count = len(C)

4. If token_count >= 6: trigger brew immediately.
```

### 7.4 Cascade Wave Counter

The `cascade_wave` counter tracks how many gravity-refill-cascade cycles occur in a single resolution. It resets to 0 at the start of each player tap. Used for scoring multipliers.

### 7.5 Resolution Timing Budget

| Phase | Duration | Notes |
|---|---|---|
| Fusion animation | 200ms | Tokens streaking toward orb center. |
| Chain fusion animation | 300ms per step | Sequential if multiple steps. |
| Brew animation | 1500ms | Per brew. Sequential if multiple brews. |
| Gravity drop | 50ms per row + 30ms bounce | Simultaneous across columns. |
| Refill drop | Same as gravity | Simultaneous. |
| Cascade fusion animation | 200ms | Same as player fusion animation. |
| Settle buffer | 50ms | Safety pause to sync state. |

**Maximum resolution time** for a complex cascade (e.g., 5 cascade waves, 3 chain steps, 2 brews): approximately 8–10 seconds. If resolution exceeds 15 seconds, force-resolve remaining cascades instantly (skip animation) to prevent player frustration.

---

## 8. Gravity & Refill

### 8.1 Gravity Rules

Process columns independently, left-to-right:

```
function apply_gravity(grid, col):
    write_row = grid_height - 1  // bottom of column

    // Pass 1: pull all non-empty cells to bottom
    for read_row from (grid_height - 1) down to 0:
        cell = grid[col][read_row]
        if cell.type != EMPTY:
            if read_row != write_row:
                grid[col][write_row] = cell
                grid[col][read_row] = EMPTY
                cell.state = DROPPING
                cell.drop_distance = write_row - read_row
            write_row -= 1

    // Remaining cells (0 to write_row) are EMPTY — filled in refill step.
```

### 8.2 What Falls

Both `TOKEN` and `ORB` cell contents are subject to gravity. Orbs fall just like tokens.

### 8.3 Refill Rules

After gravity, for each column:

```
function refill(grid, col):
    for row from 0 to (grid_height - 1):
        if grid[col][row].type == EMPTY:
            new_token = generate_token(level.spawn_weights)
            grid[col][row] = new_token
            new_token.state = ENTERING
            new_token.enter_distance = row + 1  // drops from above grid
```

Wait: this doesn't correctly refill only the top. After gravity, all EMPTY cells are at the top of the column (rows 0 to `write_row`). Refill should only target those:

```
function refill(grid, col):
    for row from 0 to (grid_height - 1):
        if grid[col][row].type == EMPTY:
            new_token = generate_token(level.spawn_weights)
            grid[col][row] = new_token
            new_token.state = ENTERING
            new_token.enter_from = -(count_of_empties_above_including_self)
        else:
            break  // no more empties in this column (they're all at top)
```

### 8.4 Token Generation

```
function generate_token(spawn_weights):
    // spawn_weights: dict of {color: probability}
    // Example: {EMBER: 0.2, FROST: 0.2, VINE: 0.2, SUN: 0.2, SHADOW: 0.2}

    roll = random(0.0, 1.0)
    cumulative = 0.0
    for each (color, weight) in spawn_weights:
        cumulative += weight
        if roll <= cumulative:
            return Token(color)
```

RNG is seeded per level attempt for reproducibility in replay/debug. Seed = `hash(player_id, level_id, attempt_number, timestamp)`.

### 8.5 Drop Animation Timing

```
drop_duration_ms = (drop_distance * 50) + 30  // 30ms bounce at end
```

All drops in all columns begin simultaneously. The column with the longest drop determines when the grid is settled.

---

## 9. Move Counting

### 9.1 Actions That Consume a Move

| Action | Moves Consumed |
|---|---|
| Player taps a valid cluster (3+ tokens) | 1 |
| Player uses Catalyst booster on a token | 1 |

### 9.2 Actions That Do NOT Consume a Move

| Action |
|---|
| Chain fusion (auto-triggered) |
| Cascade auto-fuse (gravity-triggered cluster) |
| Brew trigger |
| Board reshuffle (deadlock recovery) |
| Shake booster activation |
| Invalid tap (rejected — no cluster or wrong target) |

### 9.3 Move Counter Behavior

- Decremented at the **start** of the fusion animation (immediately on valid tap).
- Displayed as integer in HUD. Pulses red when ≤ 3 remaining.
- When counter reaches 0 and no resolution is in progress, trigger fail state.
- If counter reaches 0 during a resolution sequence, the resolution completes fully before the fail screen appears. (The in-progress cascade may still complete the recipe — this feels fair and exciting.)

---

## 10. Board Generation

### 10.1 Initial Board Generation

```
function generate_board(level_config):
    grid = empty grid of (width × height)

    // Fill every cell with a random token
    for col in 0..(width-1):
        for row in 0..(height-1):
            grid[col][row] = generate_token(level_config.spawn_weights)

    // Validate: at least MIN_VALID_CLUSTERS exist
    clusters = find_all_valid_clusters(grid)
    attempts = 0

    while len(clusters) < MIN_VALID_CLUSTERS:
        // Regenerate random cells to break up dead zones
        reshuffle(grid)
        clusters = find_all_valid_clusters(grid)
        attempts += 1
        if attempts > 100:
            // Full regeneration as fallback
            regenerate_all(grid, level_config.spawn_weights)
            break

    return grid
```

### 10.2 `MIN_VALID_CLUSTERS`

Minimum number of valid (size ≥ 3) clusters that must exist on the board after generation: **3**.

This ensures the player has meaningful choices on their first move.

### 10.3 No Pre-Placed Orbs

The initial board contains only tokens. No orbs exist at level start.

### 10.4 Anti-Frustration: No Immediate Brews

The board generator must ensure that no single cluster of 6+ same-colored tokens exists at generation. Maximum initial cluster size: 5. This prevents an accidental "free brew" before the player has done anything.

```
function validate_no_large_clusters(grid):
    for each cluster in find_all_clusters(grid):
        if cluster.size > 5:
            // Break up cluster: recolor one random cell in the cluster
            random_cell = random_choice(cluster.cells)
            new_color = random_choice(level_config.token_types, exclude=cluster.color)
            grid[random_cell.col][random_cell.row].color = new_color
            // Re-validate recursively
            validate_no_large_clusters(grid)
```

---

## 11. Deadlock Detection & Recovery

### 11.1 Deadlock Definition

The board is deadlocked when:

1. No cluster of 3+ same-colored tokens exists.
2. The player has moves remaining.
3. No orbs are in a chain-fusible state.

### 11.2 Detection Timing

Check for deadlock after every board-stable state (end of resolution phase).

### 11.3 Recovery: Reshuffle

```
function reshuffle(grid):
    // Collect all token positions and all orb positions separately
    tokens = [(col, row, grid[col][row]) for all cells where type == TOKEN]
    orbs = [(col, row, grid[col][row]) for all cells where type == ORB]

    // Shuffle only the token list
    shuffled_colors = shuffle([t.color for t in tokens])

    // Reassign colors to token positions
    for i, (col, row, _) in enumerate(tokens):
        grid[col][row].color = shuffled_colors[i]

    // Orbs do NOT move during reshuffle

    // Validate that at least one valid cluster exists
    if len(find_all_valid_clusters(grid)) == 0:
        // Retry reshuffle (up to 10 times)
        reshuffle(grid)
```

### 11.4 Reshuffle Limits

- Maximum 10 consecutive reshuffles before full board regeneration (excluding orbs).
- Full regeneration: remove all tokens, regenerate random tokens, keep orbs in place.

### 11.5 Reshuffle UX

- Display "Reshuffling..." overlay (500ms).
- Tokens scatter and reassemble animation (300ms).
- No move consumed.
- If reshuffle creates cascade clusters, they auto-resolve normally.

---

## 12. Edge Cases

### 12.1 Two Orbs Created Simultaneously (Cascade)

When a cascade wave contains multiple valid clusters, they are processed sequentially:

- **Order:** Bottom-most cluster first (highest row index of the cluster's bottom-most cell). Ties broken by left-most (lowest column index).
- Each cluster fuses into an orb before the next is processed.
- After each orb creation, chain fusion is checked immediately.
- This means earlier orbs in the cascade may chain-fuse with later orbs.

### 12.2 Orb Blocks a Token's Drop Path

Orbs occupy cells and fall with gravity just like tokens. An orb cannot "block" a token differently — they simply stack in the column with normal gravity rules. An orb above a token falls on top of it (orb in lower row index). The relative order of orbs and tokens in a column is preserved during gravity; only empty gaps are closed.

### 12.3 Player Taps an Orb

Tapping a cell containing an orb is a no-op (reject with subtle shake animation). Orbs cannot be directly manipulated by the player.

### 12.4 Brew During Cascade

If a cascade auto-fuse creates an orb with `token_count >= 6`, it brews immediately following the standard brew animation. The cascade pauses during the brew animation, then resumes gravity + refill for the emptied cell.

### 12.5 Multiple Brews in One Resolution

Processed sequentially per Section 6.3. Each brew can independently fill a recipe vial. If the recipe is completed mid-resolution (e.g., a cascade brews the final required potion), the resolution still completes fully (remaining cascades resolve, remaining brews trigger). The win screen appears after full resolution.

### 12.6 Chain Fusion Creates an L-Shape or T-Shape Group

Three or more orbs in a non-linear arrangement all merge into the bottom-left survivor per the standard rule (highest row, then lowest column). The merged orb inherits the sum of all `token_count` values. Only one survivor remains.

### 12.7 Orb Adjacent to Two Different-Colored Orbs

No interaction. Chain fusion only triggers between same-colored orbs. Different-colored orbs adjacent to each other are inert.

### 12.8 Catalyst Booster on Isolated Token

The Catalyst booster allows targeting a single token with no adjacent same-color neighbors. It creates an orb with `token_count = 1`. This orb behaves normally — it can chain-fuse with other orbs of the same color and needs to accumulate 5 more tokens (via chain fusion) to brew. This is intentionally weak as a standalone action but strategically useful for positioning.

### 12.9 Recipe Requires More Brews Than Board Can Support

Level design should prevent this, but as a safety net: if the total tokens of a required color on the board + expected refills (estimated via spawn weights × expected empty cells) is insufficient, the level config is flagged during QA validation. At runtime, no special handling — the player simply fails the level, which is an acceptable outcome.

### 12.10 Simultaneous Chain Fusions in Separate Board Regions

If two separate groups of same-color orbs each qualify for chain fusion in different parts of the board, they are independent. Both resolve simultaneously (separate animations in parallel). If they are the same color but not connected, they do NOT merge — chain fusion requires adjacency.

---

## 13. Scoring Formula

### 13.1 Per-Resolution Scoring

A "resolution" is everything that happens from a player tap (or cascade trigger) through to the next board-stable state.

```
resolution_score = 0
chain_multiplier = 1.0
cascade_multiplier = 1.0

// For each fusion event in the resolution:
on_fusion(cluster_size):
    fusion_points = cluster_size * 10
    resolution_score += fusion_points * chain_multiplier * cascade_multiplier

on_chain_step():
    chain_multiplier += 0.5
    // chain step 1: multiplier = 1.5
    // chain step 2: multiplier = 2.0
    // chain step 3: multiplier = 2.5

on_cascade_wave():
    cascade_multiplier += 0.5
    // cascade wave 1: multiplier = 1.5
    // cascade wave 2: multiplier = 2.0

on_brew(is_target_color):
    if is_target_color:
        resolution_score += 200  // flat, not multiplied
    else:
        resolution_score += 100  // flat, not multiplied

on_multi_brew(brew_count_this_resolution):
    if brew_count_this_resolution > 1:
        resolution_score += (brew_count_this_resolution - 1) * 100
```

### 13.2 End-of-Level Scoring

```
level_score = sum of all resolution_scores
level_score += remaining_moves * 50  // only on win
```

### 13.3 Score Display

- Fusion points pop up from the fusion location as floating text.
- Multiplier text (e.g., "×2.0!") appears when chain or cascade multiplier exceeds 1.0.
- Brew points pop up from the recipe vial.
- End screen shows: total score, breakdown (fusions, brews, bonus), star rating.

### 13.4 Score Ranges (Informational)

For a typical World 3 level (7×9 grid, 18 moves, 2 target brews):

| Rating | Estimated Score |
|---|---|
| Fail | 200–600 |
| 1 star | 700–1200 |
| 2 stars | 1200–2000 |
| 3 stars | 2000–3500 |

These ranges are tuning targets, not hard rules.

---

## 14. Combo & Chain Multiplier Details

### 14.1 Chain Multiplier

Tracks chain-fusion events within a single resolution.

| Chain Steps | Multiplier | Applied To |
|---|---|---|
| 0 | 1.0× | All fusion points |
| 1 | 1.5× | Fusion points after this step |
| 2 | 2.0× | " |
| 3 | 2.5× | " |
| N | 1.0 + (N × 0.5) | " |

**Key:** The multiplier increases **after** the chain step occurs. So the first player fusion is always at 1.0×. The first chain merge boosts subsequent events to 1.5×.

### 14.2 Cascade Multiplier

Tracks cascade waves (gravity-triggered auto-fusions).

| Cascade Waves | Multiplier | Applied To |
|---|---|---|
| 0 | 1.0× | All fusion points |
| 1 | 1.5× | Cascade fusion points and beyond |
| 2 | 2.0× | " |
| N | 1.0 + (N × 0.5) | " |

### 14.3 Multiplier Stacking

Chain and cascade multipliers **multiply together**:

```
effective_multiplier = chain_multiplier * cascade_multiplier
```

Example: 2 chain steps (2.0×) + 1 cascade wave (1.5×) = 3.0× on subsequent fusion points.

### 14.4 Multiplier Reset

Both multipliers reset to 1.0 at the start of each player move (tap). They accumulate only within a single resolution sequence.

### 14.5 Combo Visual Feedback

| Effective Multiplier | Feedback |
|---|---|
| 1.5×–2.0× | "Nice!" floating text |
| 2.5×–3.5× | "Great!!" floating text + screen flash |
| 4.0×+ | "INCREDIBLE!!!" floating text + screen shake + particle burst |

---

## 15. Booster Mechanics (Technical)

### 15.1 Shake

```
function activate_shake(grid):
    // Collect all TOKEN cells (not ORBs, not EMPTY)
    token_cells = [(c, r) for all cells where type == TOKEN]
    colors = [grid[c][r].color for (c, r) in token_cells]

    // Shuffle colors
    shuffle(colors)

    // Reassign
    for i, (c, r) in enumerate(token_cells):
        grid[c][r].color = colors[i]

    // Guarantee at least one valid cluster
    if find_all_valid_clusters(grid).size == 0:
        activate_shake(grid)  // retry (max 10 recursions, then force-place a cluster)

    // Animation: tokens scatter and reassemble (500ms)
    // Does NOT consume a move
    // Does NOT trigger cascades (board is not re-checked for auto-fuse after shake)
```

### 15.2 Catalyst

```
function activate_catalyst(grid, target_col, target_row):
    cell = grid[target_col][target_row]
    if cell.type != TOKEN:
        reject()  // can only target tokens, not orbs or empty cells
        return

    // Find the cluster containing this cell (any size, including 1)
    cluster = find_cluster(grid, target_col, target_row)

    // Fuse the cluster into an orb at the target cell
    for (c, r) in cluster:
        grid[c][r] = EMPTY
    grid[target_col][target_row] = ORB(color=cell.color, token_count=cluster.size)

    // Consume 1 move
    move_counter -= 1

    // Enter resolution phase (chain fusion, gravity, cascades)
    resolve(grid, target_col, target_row, is_player_move=true)
```

### 15.3 Extra Moves

```
function activate_extra_moves():
    move_counter += 5
    // Can be activated at any time, including the fail screen
    // Limited to once per level attempt
    // If activated on fail screen: dismiss fail screen, return to gameplay
```

---

## 16. Near-Miss Detection

### 16.1 Algorithm

Run on level fail (move counter reaches 0, recipe incomplete):

```
function is_near_miss(grid, recipe_remaining):
    // Simulate up to 3 optimal moves on the current board state
    // Use a simple greedy heuristic (not full tree search):

    simulated_grid = deep_copy(grid)
    simulated_recipe = deep_copy(recipe_remaining)

    for move in 1..3:
        // Find the fusion that is most likely to progress the recipe:
        best_cluster = null
        best_score = -1

        for each valid cluster C in simulated_grid:
            score = evaluate_cluster(C, simulated_recipe)
            if score > best_score:
                best_score = score
                best_cluster = C

        if best_cluster is null:
            break  // no valid moves

        // Execute the fusion + resolution
        simulate_fusion(simulated_grid, best_cluster)
        update_recipe(simulated_recipe, brews_this_resolution)

        if simulated_recipe is fully complete:
            return true  // near miss confirmed

    return false  // not a near miss
```

### 16.2 `evaluate_cluster` Heuristic

```
function evaluate_cluster(cluster, recipe_remaining):
    color = cluster.color
    score = 0

    // Prefer recipe colors
    if color in recipe_remaining.needed_colors:
        score += 100

    // Prefer larger clusters (closer to brew threshold)
    score += cluster.size * 10

    // Prefer clusters that create orbs adjacent to existing same-color orbs
    // (would trigger chain fusion)
    orb_proximity = count_adjacent_same_color_orbs(cluster.optimal_tap_cell, color)
    score += orb_proximity * 50

    return score
```

### 16.3 Server Validation

Near-miss determination must be validated server-side before offering the +3 moves prompt. The client sends the board state snapshot; the server runs the heuristic and returns `{near_miss: true/false}`. This prevents client-side manipulation.

**Latency budget:** Response within 500ms. If server is unreachable, default to `near_miss = false` (don't offer the prompt).

---

## 17. State Machine Summary

```
LEVEL_START
    │
    ▼
PLAYER_TURN ◄───────────────────────────────┐
    │                                        │
    ├─ valid tap ──▶ FUSION ──▶ RESOLUTION ──┤
    │                                        │
    ├─ deadlock ──▶ RESHUFFLE ──────────────►┤
    │                                        │
    ├─ moves == 0 ──▶ FAIL_CHECK            │
    │                    │                   │
    │                    ├─ near_miss ──▶ OFFER_RECOVERY
    │                    │                   │
    │                    │                   ├─ accepted ──▶ PLAYER_TURN
    │                    │                   │
    │                    │                   └─ declined ──▶ FAIL
    │                    │
    │                    └─ not near_miss ──▶ FAIL
    │
    └─ recipe complete ──▶ WIN

RESOLUTION
    │
    ├─ chain_fuse_all()
    ├─ gravity()
    ├─ refill()
    ├─ settle()
    ├─ chain_fuse_all()  (post-gravity)
    ├─ cascade_detect()
    │     │
    │     ├─ cascades found ──▶ auto_fuse ──▶ (loop back to gravity)
    │     │
    │     └─ no cascades ──▶ STABLE
    │
    └─ STABLE ──▶ check win ──▶ check deadlock ──▶ return to PLAYER_TURN
```

---

## 18. Data Structures (Reference)

### 18.1 Grid

```
Grid {
    width: int
    height: int
    cells: Cell[width][height]
}
```

### 18.2 Cell

```
Cell {
    col: int
    row: int
    content: TOKEN | ORB | EMPTY
    color: ColorID | null       // null if EMPTY
    token_count: int | null     // only for ORB
    state: CellState
}
```

### 18.3 Level Config

```
LevelConfig {
    level_id: int
    grid_width: int
    grid_height: int
    move_limit: int
    recipe: Recipe[]
    token_types: ColorID[]
    spawn_weights: Map<ColorID, float>
    boosters_allowed: BoosterType[]
    par_moves: int
}

Recipe {
    color: ColorID
    count: int
}
```

### 18.4 Resolution Context

```
ResolutionContext {
    chain_step_counter: int     // resets per player move
    cascade_wave_counter: int   // resets per player move
    brews_this_resolution: int  // count of brews in current resolution
    chain_multiplier: float     // 1.0 + (chain_steps * 0.5)
    cascade_multiplier: float   // 1.0 + (cascade_waves * 0.5)
    resolution_score: int       // accumulated this resolution
}
```

---

## 19. Determinism & Replay

### 19.1 RNG Seeding

All random operations (token generation, reshuffle) use a seeded PRNG:

```
seed = hash(player_id + level_id + attempt_number + start_timestamp)
```

This allows:

- Server-side replay validation (anti-cheat).
- Bug reproduction with deterministic board states.
- Replay recording (store only seed + player taps + timings).

### 19.2 Replay Format

```
Replay {
    seed: int64
    level_id: int
    actions: Action[]
}

Action {
    type: TAP | BOOSTER_SHAKE | BOOSTER_CATALYST | BOOSTER_EXTRA_MOVES
    col: int | null     // for TAP and CATALYST
    row: int | null     // for TAP and CATALYST
    timestamp_ms: int   // milliseconds since level start
}
```

---

## 20. Performance Budgets

### 20.1 Computation

| Operation | Budget |
|---|---|
| Cluster detection (full board scan) | < 1ms |
| Gravity + refill (all columns) | < 1ms |
| Chain fusion check (full board) | < 1ms |
| Near-miss heuristic (3-move simulation) | < 50ms |
| Full resolution (worst case: 5 cascade waves) | < 10ms computation (animation time is separate) |

### 20.2 Memory

| Item | Budget |
|---|---|
| Grid state | ~1 KB (99 cells × ~10 bytes) |
| Cluster cache | ~2 KB |
| Resolution context | ~100 bytes |
| Replay buffer (200-move level) | ~5 KB |

---

*End of Core Mechanic Technical Specification.*
