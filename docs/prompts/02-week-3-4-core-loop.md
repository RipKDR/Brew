# Brew — Week 3-4: Core Loop Implementation

You are continuing the build of **Brew**, a mobile puzzle game (Unity 6 / C#). This prompt covers Weeks 3-4: wiring the complete core gameplay loop — recipes, win/lose conditions, level progression, scoring, blockers, and the event bus.

---

## Before You Start

1. Read `docs/prompts/00-context-recovery.md` to restore full project context (architecture, code conventions, folder structure, state machine).
2. Read `AGENTS.md` — the project-wide agent guide. Follow every convention it specifies.
3. Read `docs/game-design/core-mechanic.md` — the mechanical bible. Sections 5 (chain fusion), 6 (brew rules), 13 (scoring), and 14 (multipliers) are critical this phase.
4. Read `docs/sdk/state-machine-spec.md` — the board state machine. Every gameplay transition must follow this sequence exactly: `Idle → WaitingForInput → ProcessingFusion → Cascading → Settling → CheckingBrew → CheckingWinLose → Idle/Complete/Failed`.
5. Read `docs/sdk/event-bus-design.md` — the typed event bus you will implement this phase.
6. Read `docs/production/milestone-gates.md`, Gate 2 — these are the exact pass/fail criteria you must satisfy.

**Prerequisite:** Week 1-2 milestone (Gate 1) has PASSED. Fusion, gravity, chain fusion, brew detection, and the state machine all work with placeholder art. If any Gate 1 criterion is failing, fix it before proceeding.

---

## Week 3 Deliverables

### 1. Recipe System

Implement `RecipeTracker` that reads recipe targets from the level JSON. A recipe target is a list of potion types with required counts (e.g., `"recipes": [{"type": "EMBER", "count": 2}, {"type": "FROST", "count": 1}]`). When a brew event fires (orb reaches `token_count >= 6` per `core-mechanic.md` §6.1), decrement the matching recipe target. The `RecipeTracker` lives in `Scripts/Core/` as pure C# — no MonoBehaviour dependency.

Display recipe progress via **Potion Vials** on the screen edge. One vial per recipe target. Visual states: empty → filling (proportional to brews completed) → full. The vial UI belongs in `Scripts/Presentation/`. Use a smooth lerp for fill animation.

### 2. Win Condition

All recipe targets met → level complete. Transition to `Complete` state per the state machine spec. Show a level results screen with star rating based on remaining moves. Star thresholds come from the level JSON (`star_thresholds: [moves_for_3, moves_for_2]` — 1 star is always awarded on completion). The `WinLoseEvaluator` in `Scripts/Core/` makes the determination; the presentation layer shows the result.

### 3. Lose Condition

Moves exhausted with incomplete recipe → level failed. Transition to `Failed` state. Show a near-miss UI: calculate how close the player was (e.g., "You were 2 fusions away from Ember Potion!"). This motivates retry. The near-miss calculation lives in `WinLoseEvaluator`.

### 4. Fail Recovery (Placeholder)

After the fail screen, show a "+3 Moves" button. For now this is a free tap (no ad integration yet — that comes in Weeks 7-8). When tapped, add 3 moves, transition back to `WaitingForInput`, and let the player continue. Reference `docs/economy/monetization-plan.md` for placement rules — fail recovery is offered once per level attempt.

### 5. Level Progression

Build a level select screen: scrollable grid showing levels 1-40. Locked levels show a padlock. Unlocked levels show earned stars (0-3). Sequential unlock: completing level N unlocks level N+1. Implement `LevelCatalog` in `Scripts/Data/` that loads level JSON files from the `levels/` directory (or ScriptableObjects from `ScriptableObjects/Levels/`). Validate schema on load — reject malformed levels with a clear error log.

### 6. Scoring System

Implement `ScoreCalculator` in `Scripts/Core/` with the formula from `core-mechanic.md`:

- Base: `cluster_size × 10` per fusion
- Chain multiplier: `+0.5` per chain step (§14.1)
- Cascade multiplier: `+0.5` per cascade wave (§14.2)
- Brew bonus: 200 points for target potion, 100 for non-target (§6.2)
- Remaining move bonus: `50 × remaining_moves` at level complete (§13.2)

Multipliers stack. Display running score in the HUD.

---

## Week 4 Deliverables

### 1. Deadlock Detection

After every settle phase, check if any valid cluster (≥ 3 adjacent same-color tokens) exists on the board. If none exist, trigger an auto-shuffle: randomize all token positions, guaranteeing at least one valid cluster after shuffle. The shuffle costs 0 moves. Reference `core-mechanic.md` edge cases section. This is critical — a board with no valid moves and remaining moves left is a softlock.

### 2. Blocker Tiles

Implement two blocker types per `docs/game-design/level-design-framework.md`:

- **Stone:** Occupies a cell. Cannot be fused. Cleared when an adjacent brew occurs. Stone cells do not participate in gravity (tokens cannot fall through them). Render with a distinct stone texture.
- **Ice:** Locks a token in place for 1 turn after an adjacent fusion occurs. The iced token cannot be part of a cluster while locked. After 1 player move passes, the ice cracks and the token returns to normal. Visual: frosted overlay on the token.

Blockers are defined in the level JSON. Ensure the state machine handles blocker interactions correctly — especially that Stone clearing and Ice thawing happen at the right phase (during `Settling`).

### 3. Star Display

Level results screen shows 1, 2, or 3 stars based on `star_thresholds` from the level JSON. Animate stars appearing sequentially (0.3s delay between each). 3-star completion should feel celebratory (larger animation, more particles — placeholder for now, polish comes in Weeks 5-6).

### 4. Level Results Screen

Show: stars earned, potions brewed (list with icons), final score, Essence earned (placeholder amounts: 1-star = 10, 2-star = 20, 3-star = 35). Buttons: "Next Level" (if won) or "Retry" (if lost). Replaying a level for more stars is always available from level select.

### 5. Event Bus Implementation

Implement the typed event bus per `docs/sdk/event-bus-design.md` exactly. Key events to fire this phase:

- `OnFusionComplete` — cluster fused into orb (includes cluster size, color, position)
- `OnChainFusion` — orbs auto-fused (includes chain step count, resulting token_count)
- `OnBrewComplete` — orb brewed into potion (includes potion type, whether it was a recipe target)
- `OnCascadeWave` — cascade triggered (includes wave number)
- `OnLevelComplete` — level won (includes stars, score, potions brewed)
- `OnLevelFailed` — level lost (includes remaining recipe targets, near-miss info)
- `OnMoveUsed` — move counter decremented (includes remaining moves)

All events are structs implementing `IGameEvent`. Zero-allocation. Main-thread only. Systems subscribe to events rather than holding direct references to each other.

### 6. Level Integration

Ensure all 40 level JSONs from the `levels/` directory (or ScriptableObjects) load without errors. Each level must be playable from start to win or lose. Verify that board dimensions, ingredient pools, recipe targets, move limits, blocker placements, and star thresholds are all read correctly. Log a warning for any level that cannot generate a solvable opening board after 100 attempts.

---

## Milestone Gate 2 — Pass/Fail Criteria

Reference `docs/production/milestone-gates.md`, Gate 2 for the authoritative list. Summary:

- Can play a complete level from start to win or lose
- Chain fusion works correctly (adjacent same-type orbs auto-fuse after gravity)
- Brew triggers at configured threshold, vial UI updates
- Recipe tracking accurately counts brewed potions against targets
- Move counter works, level fails at 0 moves with incomplete recipe
- Cascades resolve correctly up to cap of 20
- Levels load from JSON with distinct configurations
- Level complete screen shows star rating; fail screen shows retry
- Level select shows correct lock/unlock state and star counts
- Progress persists across app restart
- No softlocks during gameplay (deadlock detection + shuffle works)
- Blockers (Stone, Ice) function as specified
- All 40 levels load without errors
- Event bus fires correct events at correct times
- Scoring formula matches spec

---

## What NOT to Do

- No meta systems (Workshop, Potion Shelf, Streaks, Daily Brew)
- No real monetization (no ads, no IAP — only the placeholder +3 moves button)
- No analytics integration
- No art or sound polish (placeholder art is fine)
- No Firebase integration
- No cloud save (local save only this phase)

Stay focused on the core loop. If a player can pick up the game, play a level, win or lose, see their score, and move to the next level — this phase is complete.