# Board State Machine Specification

> Brew — `BoardStateMachine` (GameFlow module)
> Governs all board behavior from level start to level end.
> Single authority for "what the board is doing right now."

---

## State Diagram

```
                              ┌──────────────────────────────────────────────────────┐
                              │                                                      │
                              ▼                                                      │
┌─────────┐    ┌──────────────────┐    ┌──────────────────┐    ┌────────────┐       │
│  Idle   │───▶│ WaitingForInput  │───▶│ ProcessingFusion │───▶│ Cascading  │       │
└─────────┘    └──────────────────┘    └──────────────────┘    └─────┬──────┘       │
     ▲                                                               │              │
     │                                                               ▼              │
     │                                        ┌────────────┐   ┌──────────┐        │
     │                                        │ auto-chain │◀──│ Settling │        │
     │                                        │   found    │   └────┬─────┘        │
     │                                        └──────┬─────┘        │              │
     │                                               │         no chain            │
     │                                               ▼              │              │
     │                                   ┌──────────────────┐       │              │
     │                                   │ ProcessingFusion │◀──────┘              │
     │                                   │   (auto-chain)   │                      │
     │                                   └────────┬─────────┘                      │
     │                                            │                                │
     │                                            ▼                                │
     │                                      ┌────────────┐                         │
     │                                      │ Cascading  │─── (loops until stable) │
     │                                      └────────────┘                         │
     │                                                                             │
     │         ┌─────────────────┐    ┌────────────────┐                           │
     │         │ CheckingBrew    │◀───│   Settling     │◀──────────────────────────┘
     │         └────────┬────────┘    └────────────────┘
     │                  │
     │                  ▼
     │         ┌─────────────────┐
     │         │ CheckingWinLose │
     │         └────────┬────────┘
     │                  │
     │        ┌─────────┼─────────┐
     │        ▼         ▼         ▼
     │    ┌───────┐ ┌────────┐ ┌──────────────┐
     └────│ Idle  │ │Complete│ │   Failed     │
          └───────┘ └────────┘ └──────────────┘

                    ┌────────┐
          (any) ───▶│ Paused │───▶ (previous state)
                    └────────┘
```

---

## States

### `Idle`

The resting state between moves. The board is stable, all animations have completed, and the system is ready for the next action.

| Aspect | Detail |
|---|---|
| **Board activity** | None. Board is static. |
| **Valid transitions** | → `WaitingForInput` (automatic, immediate — `Idle` exists as a logical boundary for event firing but transitions instantly) |
| **Player input** | Not yet accepted. Transition to `WaitingForInput` enables input. |
| **Animations/VFX** | Idle shimmer on tokens (ambient). Orbs pulse gently. |
| **Events on entry** | None. |
| **Events on exit** | None. |
| **Duration** | Instantaneous (0 frames). |

In practice, `Idle` and `WaitingForInput` are often experienced as a single state. `Idle` exists as a clean re-entry point after `CheckingWinLose` determines play continues.

---

### `WaitingForInput`

The board is accepting player taps. This is the only state where player interaction with the board is processed.

| Aspect | Detail |
|---|---|
| **Board activity** | Listening for touch/click input on the board grid. Highlighting valid clusters on hover (optional visual feedback). |
| **Valid transitions** | → `ProcessingFusion` (player taps a cell belonging to a valid cluster of ≥ `min_cluster_size` tokens) |
| **Player input** | **Accepted.** Tap on valid cluster → begin fusion. Tap on invalid cell (single token, empty cell) → ignored with subtle shake feedback. Booster activation → handled by `BoosterPanel`, may modify board then transition to `ProcessingFusion`. |
| **Animations/VFX** | Idle token shimmer. Selected cluster highlights on tap-and-hold. |
| **Events on entry** | None. |
| **Events on exit** | `OnMoveUsed` (decrement remaining moves). |
| **Duration** | Indefinite — waits for player. |

**Input debounce:** After a tap is accepted, input is disabled until the state machine returns to `WaitingForInput` again. This prevents double-taps during transition frames.

---

### `ProcessingFusion`

Tokens in the tapped cluster are animating toward the fusion point (center of cluster or existing orb position). At the end, tokens are consumed and an orb is created or grown.

| Aspect | Detail |
|---|---|
| **Board activity** | Tokens in the cluster lerp toward the orb position. Tokens shrink and fade. Orb grows (scale punch). |
| **Valid transitions** | → `Cascading` (fusion animation completes, gaps now exist on the board) |
| **Player input** | **Ignored.** All taps are swallowed during fusion. |
| **Animations/VFX** | Token merge animation (0.3s ease-in-out). Element-colored particle trails on moving tokens. Orb scale punch on creation/growth. `FusionVFX` burst at orb position. |
| **Events on entry** | `OnFusionStarted { Position, Element, TokenCount }` |
| **Events on exit** | `OnFusionComplete { OrbPosition, Element, OrbSize, TokensConsumed, IsChain }` |
| **Duration** | 0.3 seconds (configurable via `fusion_animation_duration` constant). |

**Auto-chain variant:** When this state is entered from `Settling` (auto-chain), `IsChain = true` in the exit event, and `OnChainFusion` fires additionally with the chain depth.

---

### `Cascading`

Gravity is applied. Tokens above gaps fall down. New tokens spawn at the top of each column and fall into place.

| Aspect | Detail |
|---|---|
| **Board activity** | Column-by-column, tokens shift down to fill gaps. New tokens are instantiated at the top of columns with gaps and fall. Fall speed is `cascade_settle_delay_ms` per row (100ms default = 10 rows/sec). |
| **Valid transitions** | → `Settling` (all tokens have reached their target positions and no tokens are in motion) |
| **Player input** | **Ignored.** |
| **Animations/VFX** | Tokens fall with eased motion (fast start, slight bounce on landing). `CascadeVFX` trail effects on falling tokens. Speed lines on columns with large drops. |
| **Events on entry** | `OnCascadeStarted { GapCount, ColumnsAffected }` |
| **Events on exit** | `OnCascadeComplete { TokensDropped, TokensSpawned, HasAutoChain }` |
| **Duration** | Variable. Depends on the number of rows tokens must fall. Typical: 0.2–0.8s. Maximum (entire column empty): ~0.9s for a 9-row board. |

**Cascade timing:** All columns cascade simultaneously. Within a column, tokens fall one row at a time with `cascade_settle_delay_ms` between each row step. This creates a satisfying "rain" visual rather than instant snapping.

---

### `Settling`

The board is fully populated (no gaps). The system scans for auto-chain opportunities — new clusters formed by tokens that fell into place.

| Aspect | Detail |
|---|---|
| **Board activity** | `FusionEngine` scans the entire board for valid clusters. |
| **Valid transitions** | → `ProcessingFusion` (at least one valid auto-chain cluster found — process the largest one) |
| | → `CheckingBrew` (no auto-chain clusters — board is truly stable) |
| **Player input** | **Ignored.** |
| **Animations/VFX** | Brief pause (1–2 frames) for visual breathing room between cascade and next action. If auto-chain found, the cluster briefly highlights before transitioning to `ProcessingFusion`. |
| **Events on entry** | None. |
| **Events on exit** | `OnChainFusion` (if auto-chain found, with incremented `ChainDepth`). |
| **Duration** | 1–2 frames for scan (~16–33ms). If auto-chain found, 0.15s highlight before transitioning. |

**Auto-chain selection:** If multiple auto-chain clusters exist, the largest cluster is processed first. Ties are broken by bottom-left priority (lowest row first, then leftmost column). After that fusion cascades, `Settling` runs again and may find more chains.

**Chain depth tracking:** A counter increments each time `Settling` → `ProcessingFusion` occurs without returning to `WaitingForInput`. The chain depth resets when the state machine returns to `WaitingForInput`. Chain depth drives the score multiplier: `base_score × (1 + 0.25 × chain_depth)`.

---

### `CheckingBrew`

The board is stable and no auto-chains remain. The system checks if any orb has reached the `brew_threshold`.

| Aspect | Detail |
|---|---|
| **Board activity** | `BrewDetector` iterates all orbs on the board. Any orb with `Size >= brew_threshold` triggers a brew. |
| **Valid transitions** | → `CheckingBrew` again (if multiple orbs brew — process one, then re-check) |
| | → `CheckingWinLose` (no more orbs at threshold) |
| **Player input** | **Ignored.** |
| **Animations/VFX** | Per brew: orb glows intensely (0.2s), then erupts into potion particles (0.5s). Potion icon flies to recipe tracker in HUD. If multiple brews, each plays sequentially with 0.3s gap. |
| **Events on entry** | `OnBrewStarted { OrbPosition, Element, PotionId }` (per brew) |
| **Events on exit** | `OnBrewComplete { PotionId, Element, TotalBrewedThisLevel, RequiredBrews, RecipeFulfilled }` (per brew) |
| **Duration** | 0.5s per brew + 0.3s gap between sequential brews. Zero brews = instantaneous transition to `CheckingWinLose`. |

**Multiple simultaneous brews:** If three orbs all hit threshold in the same cascade, they brew sequentially (not simultaneously) so the player can register each one. Order: top-to-bottom, left-to-right. Each brew removes the orb and the resulting gap does NOT trigger a new cascade — the board waits until all brews complete, then the gaps cascade together (transition back through `Cascading` → `Settling` → `CheckingBrew` → `CheckingWinLose`).

**Correction:** After all brews in `CheckingBrew` complete, if any orbs were removed (creating gaps), the state machine transitions to `Cascading` (not directly to `CheckingWinLose`). This ensures the board is stable before win/lose evaluation.

---

### `CheckingWinLose`

Evaluates whether the level has been won, lost, or continues.

| Aspect | Detail |
|---|---|
| **Board activity** | None. Pure logic evaluation. |
| **Valid transitions** | → `LevelComplete` (recipe fully satisfied) |
| | → `LevelFailed` (moves exhausted AND recipe not satisfied) |
| | → `Idle` (moves remain AND recipe not yet satisfied — play continues) |
| **Player input** | **Ignored.** |
| **Animations/VFX** | None. |
| **Events on entry** | None. |
| **Events on exit** | None (the destination state fires its own entry events). |
| **Duration** | Instantaneous (single frame). |

**Win condition:** `RecipeTracker.IsComplete == true`. All required potions have been brewed the required number of times.

**Lose condition:** `RemainingMoves == 0 && !RecipeTracker.IsComplete`.

**Continue condition:** `RemainingMoves > 0 && !RecipeTracker.IsComplete`.

**Edge case — win with zero moves:** If the player completes the recipe on their last move, it's a win (win is checked before loss).

---

### `LevelComplete`

Terminal state. The level has been won.

| Aspect | Detail |
|---|---|
| **Board activity** | Board freezes. Remaining tokens play a celebration animation (pop sequentially from center outward). |
| **Valid transitions** | → (none — level ends. `LevelManager` handles transition back to Meta scene.) |
| **Player input** | Limited — player can tap to skip celebration animation. UI buttons appear after animation (Continue, Replay). |
| **Animations/VFX** | Star reveal animation (1–3 stars fly in sequentially, 0.3s each). Score tally animation. Confetti particles. Screen-wide glow. |
| **Events on entry** | `OnLevelComplete { LevelId, Stars, FinalScore, MovesRemaining, MovesUsed, Duration, IsNewBest, IsDailyBrew, IsEvent }` |
| **Events on exit** | None. |
| **Duration** | 2–3s celebration → reward popup → player-dismissed. |

**Star calculation:**
- 1 star: recipe complete (baseline).
- 2 stars: `score >= star_2_threshold` (defined per level).
- 3 stars: `score >= star_3_threshold` (defined per level).

---

### `LevelFailed`

Terminal state. Moves are exhausted without completing the recipe.

| Aspect | Detail |
|---|---|
| **Board activity** | Board dims. Remaining tokens desaturate. |
| **Valid transitions** | → (none — level ends. Fail popup offers options.) |
| **Player input** | Fail popup buttons: **Watch Ad** (+5 moves, returns to `WaitingForInput`), **Retry** (restart level), **Quit** (return to Meta). |
| **Animations/VFX** | Board desaturation (0.3s). Gentle downward particle drift. Sad audio sting. |
| **Events on entry** | `OnLevelFailed { LevelId, FinalScore, MovesUsed, Duration, BrewsRemaining, CanWatchAdForMoves }` |
| **Events on exit** | None. |
| **Duration** | Until player makes a choice. |

**Ad-for-moves flow:** If the player watches a rewarded ad, the state machine transitions:
`LevelFailed` → `WaitingForInput` (with `MovesRemaining += rewarded_ad_extra_moves_count`). This is the only case where a terminal state transitions back to gameplay. `OnMoveUsed` is NOT re-fired; the fail popup simply closes and the board reactivates.

**Retry flow:** Full board reset. `LevelManager.LoadLevel()` is called again. Boosters used in the failed attempt are NOT refunded (they were consumed when activated).

---

### `Paused`

Overlay state. The game is paused (player opened pause menu, app backgrounded, or modal popup is blocking).

| Aspect | Detail |
|---|---|
| **Board activity** | Frozen. All animations pause (`Time.timeScale = 0` or animation pause). |
| **Valid transitions** | → (previous state — resumes exactly where it left off) |
| **Player input** | Only pause menu UI is interactive. Board taps ignored. |
| **Animations/VFX** | Dim overlay. Board blur (optional). Pause menu slides in. |
| **Events on entry** | None (pause is transparent to game logic). |
| **Events on exit** | None. |
| **Duration** | Until player unpauses or app returns to foreground. |

**Implementation:** `Paused` is implemented as a state stack push rather than a standard transition. The state machine remembers the state it was in before pause and returns to it. `Time.timeScale` is set to 0 during pause (all `Time.deltaTime`-based animations freeze). Coroutines using `WaitForSecondsRealtime` continue running (used for pause menu animations).

---

## Typical Move Flow

A complete player move from tap to next input opportunity:

```
Frame   Time    State               Action
─────   ─────   ──────────────────  ────────────────────────────────
  0     0.000   WaitingForInput     Player taps cell (3,5). Cluster of 4 Fire tokens detected.
  1     0.000   ProcessingFusion    OnMoveUsed fires. OnFusionStarted fires. Tokens begin
                                    merge animation toward (3,5).
  9     0.300   ProcessingFusion    Animation complete. Tokens removed, Fire Orb created at
                                    (3,5) with Size=4. OnFusionComplete fires.
 10     0.316   Cascading           OnCascadeStarted fires. 4 gaps in column 3,4,5.
                                    Tokens fall. New tokens spawn at top.
 16     0.516   Cascading           Tokens land. OnCascadeComplete fires.
 17     0.533   Settling            Board scanned. Auto-chain found: 3 Water tokens at
                                    (2,3), (3,3), (4,3) formed by cascade.
 18     0.550   ProcessingFusion    OnChainFusion fires (ChainDepth=1). OnFusionStarted fires.
                                    Auto-fusion animation begins.
 27     0.850   ProcessingFusion    Animation complete. Water Orb created. OnFusionComplete
                                    fires (IsChain=true).
 28     0.866   Cascading           OnCascadeStarted. 3 new gaps. Cascade resolves.
 33     1.066   Cascading           OnCascadeComplete. No auto-chain found.
 34     1.083   Settling            Board scanned. No clusters.
 35     1.100   CheckingBrew        Fire Orb at (3,5) has Size=4. Not at brew_threshold (7).
                                    No brews.
 36     1.116   CheckingWinLose     Moves remaining: 17. Recipe not complete. Continue.
 37     1.133   Idle                Immediate transition.
 38     1.133   WaitingForInput     Ready for next player tap.
```

**Total elapsed time for one move:** ~1.1 seconds (with one auto-chain). Simple moves without chains: ~0.6 seconds.

---

## Edge Cases

### Player Taps During Cascade

**Behavior:** Ignored. The state machine is in `Cascading` or `ProcessingFusion`, neither of which accepts input. The tap is silently discarded.

**Visual feedback:** None — the board is clearly animating, providing implicit feedback that input isn't accepted.

**Technical detail:** The input system checks `BoardStateMachine.CurrentState == WaitingForInput` before forwarding taps to `MoveProcessor`.

---

### Multiple Simultaneous Brews

**Scenario:** A cascade results in two orbs reaching `brew_threshold` at the same time.

**Behavior:** Brews are processed sequentially, not simultaneously.

```
CheckingBrew → Brew Orb A (0.5s) → 0.3s gap → Brew Orb B (0.5s) → gaps exist →
Cascading → Settling → CheckingBrew (verify no more brews) → CheckingWinLose
```

**Rationale:** Sequential brews let the player register each potion contribution to the recipe. Simultaneous brews would be visually overwhelming and make recipe progress hard to track.

**Order:** Top-to-bottom, left-to-right scan of the board.

---

### Board Deadlock (No Valid Clusters)

**Scenario:** After a cascade settles, no group of ≥ `min_cluster_size` adjacent same-type tokens exists anywhere on the board. The player has no valid move.

**Behavior:** Auto-shuffle.

```
Settling (no valid moves detected) → Board shuffles (0.5s animation) → Settling (re-check)
```

**Details:**
- `BoardValidator.HasValidMoves()` is called during `Settling` when no auto-chains are found.
- If `false`, `Board.Shuffle()` randomizes token positions with a guaranteed-valid-move result.
- Shuffle animation: all tokens fly to random board positions simultaneously (0.5s).
- Shuffle costs **0 moves** — it's not the player's fault the board deadlocked.
- If the board deadlocks 3 times in a single level, a free Shake booster is granted automatically (anti-frustration mechanic controlled by Remote Config).
- `OnBoardShuffled` event fires with `{ ShuffleCount }`.

---

### Booster Activation During WaitingForInput

**Scenario:** Player activates a booster instead of tapping a cluster.

**Behavior depends on booster type:**

| Booster | Effect | State Transition |
|---|---|---|
| **Shake** | Randomizes all token positions (not orbs). | `WaitingForInput` → shake animation (0.5s) → `Settling` → `CheckingBrew` → `CheckingWinLose` → `Idle` → `WaitingForInput`. Costs 0 moves. |
| **Catalyst** | Player selects an orb; it gains +3 size toward brew threshold. | `WaitingForInput` → player taps orb → catalyst animation (0.4s) → `CheckingBrew` → `CheckingWinLose` → `Idle` → `WaitingForInput`. Costs 0 moves. |
| **Transmute** | Player selects a 3×3 area; all tokens in it convert to a chosen element. | `WaitingForInput` → player selects area → transmute animation (0.4s) → `Settling` → (may trigger auto-chains) → `CheckingBrew` → `CheckingWinLose` → `Idle` → `WaitingForInput`. Costs 0 moves. |

Boosters never consume a move. `OnBoosterUsed` fires before the effect animation. `OnMoveUsed` does NOT fire.

---

### Extra Moves Purchased Mid-Level

**Scenario:** Player is in `LevelFailed`, watches a rewarded ad or spends gems for +5 moves.

**Behavior:**

```
LevelFailed → (ad completes or gems deducted) → WaitingForInput
```

- `MovesRemaining` increases by `rewarded_ad_extra_moves_count` (default 5).
- `OnCurrencyChanged` fires if gems were spent.
- The board state is preserved exactly as it was when the player ran out of moves. No shuffle, no re-deal.
- Streak is NOT retroactively saved — the `OnLevelFailed` event already fired. If the player subsequently wins, `OnLevelComplete` fires and streak logic processes normally (the fail is treated as if it never happened for streak purposes).

---

### App Backgrounding Mid-Cascade

**Scenario:** The player switches to another app while the board is cascading.

**Behavior:**

```
(any state) → Paused (app loses focus) → (previous state) (app regains focus)
```

- `OnApplicationPause(true)` triggers `Paused`.
- All animations and timers freeze.
- On resume, the state machine continues from exactly where it left off.
- If the app is killed while paused, the level attempt is abandoned. On next launch, the player is returned to the Meta scene. No moves or boosters are refunded.

---

### Win on Final Move

**Scenario:** Player completes the recipe on their very last move.

**Behavior:** Normal win flow. `CheckingWinLose` checks recipe completion BEFORE checking moves remaining. Recipe complete = win, regardless of moves left (even if zero).

`MovesRemaining` is 0 in the `OnLevelComplete` event, which means the player still earns stars purely based on score thresholds — having 0 remaining moves does not penalize star rating.

---

## State Machine Implementation

```csharp
public class BoardStateMachine
{
    public BoardState CurrentState { get; private set; }
    private BoardState _stateBeforePause;
    private int _chainDepth;

    public bool AcceptsInput => CurrentState == BoardState.WaitingForInput;

    public void TransitionTo(BoardState newState)
    {
        var previous = CurrentState;
        OnExitState(previous);
        CurrentState = newState;
        OnEnterState(newState, previous);
    }

    public void Pause()
    {
        if (CurrentState == BoardState.Paused) return;
        _stateBeforePause = CurrentState;
        CurrentState = BoardState.Paused;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (CurrentState != BoardState.Paused) return;
        Time.timeScale = 1f;
        CurrentState = _stateBeforePause;
    }
}
```

**Transition validation:** `TransitionTo()` asserts that the requested transition is valid for the current state. Invalid transitions log an error and are rejected in development builds. In release builds, the assertion is stripped but the transition is still rejected to prevent undefined behavior.

---

## State Timing Summary

| State | Typical Duration | Fixed/Variable |
|---|---|---|
| Idle | 0 frames | Fixed |
| WaitingForInput | Indefinite | Player-controlled |
| ProcessingFusion | 0.3s | Fixed (configurable) |
| Cascading | 0.2–0.9s | Variable (depends on gap count and column height) |
| Settling | 16–33ms (1–2 frames) | Fixed (scan only) |
| CheckingBrew | 0s (no brew) to 1.3s+ (multiple brews) | Variable |
| CheckingWinLose | 1 frame | Fixed |
| LevelComplete | 2–3s auto, then player-dismissed | Semi-fixed |
| LevelFailed | Player-dismissed | Indefinite |
| Paused | Player-dismissed or app-resumed | Indefinite |
