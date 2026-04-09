# Event Bus Design

> Brew — Internal Messaging System
> Typed event bus using C# generics. Zero-allocation struct events. Main-thread only.

---

## Architecture

### Core Design

The event bus is a static, type-keyed publish/subscribe system. Each event type `T` has its own subscriber list, ensuring type safety at compile time and avoiding string-based event names.

```csharp
public interface IGameEvent { }

public static class EventBus<T> where T : struct, IGameEvent
{
    private static readonly List<Action<T>> _subscribers = new();
    private static bool _isPublishing;

    public static void Subscribe(Action<T> handler)
    {
        _subscribers.Add(handler);
    }

    public static void Unsubscribe(Action<T> handler)
    {
        _subscribers.Remove(handler);
    }

    public static void Publish(T evt)
    {
        Debug.Assert(!_isPublishing, $"Recursive publish of {typeof(T).Name}");
        _isPublishing = true;
        for (int i = 0; i < _subscribers.Count; i++)
        {
            try
            {
                _subscribers[i].Invoke(evt);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        _isPublishing = false;
    }

    public static void Clear()
    {
        _subscribers.Clear();
    }
}
```

### Design Decisions

| Decision | Rationale |
|---|---|
| **Struct events** | Avoid GC allocation. Events fire frequently (every fusion, cascade tick). Heap allocations would cause frame hitches. |
| **Static generic class** | Each `EventBus<T>` gets its own static subscriber list via C# generic specialization. No dictionary lookup, no boxing. |
| **Main thread only** | Unity API is not thread-safe. All events fire on the main thread. Services that receive async callbacks must marshal to main thread before publishing. |
| **No event inheritance** | Events are flat structs, not class hierarchies. Subscribing to a base event does not capture derived events. This is intentional — it prevents ambiguous dispatch. |
| **Exception isolation** | A throwing subscriber does not prevent subsequent subscribers from receiving the event. Exceptions are logged. |
| **No priority/ordering** | Subscribers fire in registration order. If ordering matters, the architectural solution is to split into two events (e.g., `OnFusionStarted` before `OnFusionComplete`), not to add priority. |

### Lifecycle

- **Subscribe** in `OnEnable()` or initialization.
- **Unsubscribe** in `OnDisable()` or teardown.
- **Clear** all subscribers on scene unload via `EventBus<T>.Clear()` to prevent stale references.
- A static helper `EventBusCleaner.ClearAll()` iterates all known event types via reflection (editor/debug only) or an explicit registration list (release builds).

---

## Event Catalog

### Core Gameplay Events

#### `OnFusionStarted`

Fires when a valid token cluster is tapped and the fusion animation begins.

```csharp
public struct OnFusionStarted : IGameEvent
{
    public Vector2Int Position;       // Grid position of the tap
    public ElementType Element;       // Element type being fused
    public int TokenCount;            // Number of tokens in the cluster
}
```

| Field | Type | Description |
|---|---|---|
| `Position` | `Vector2Int` | Grid cell where the player tapped |
| `Element` | `ElementType` | Fire, Water, Earth, Air, or Spirit |
| `TokenCount` | `int` | Size of the cluster being fused |

**When:** Player taps a valid cluster. Before tokens begin moving.
**Subscribers:** `VFX.FusionVFX` (start anticipation particles), `Audio.GameplaySFXTrigger` (play fusion start SFX).

---

#### `OnFusionComplete`

Fires after tokens have merged into an orb (or an existing orb has grown).

```csharp
public struct OnFusionComplete : IGameEvent
{
    public Vector2Int OrbPosition;
    public ElementType Element;
    public int OrbSize;               // Total fusions accumulated in this orb
    public int TokensConsumed;        // Tokens consumed in this specific fusion
    public bool IsChain;              // True if this fusion resulted from a cascade (not player tap)
}
```

**When:** Fusion animation completes. Orb is now on the board.
**Subscribers:** `GameFlow.ScoreTracker` (add score), `VFX.FusionVFX` (burst particles), `Audio.GameplaySFXTrigger` (play fusion complete SFX), `UI.HUDController` (update score display).

---

#### `OnChainFusion`

Fires when a cascade causes an automatic fusion (no player input).

```csharp
public struct OnChainFusion : IGameEvent
{
    public int ChainDepth;            // 1 = first auto-fusion, 2 = second, etc.
    public Vector2Int Position;
    public ElementType Element;
    public int TokenCount;
    public float ScoreMultiplier;     // Increases with chain depth
}
```

**When:** During cascade resolution, when fallen tokens form a new valid cluster.
**Subscribers:** `GameFlow.ScoreTracker` (apply chain multiplier), `VFX.ChainVFX` (escalating effects), `Audio.GameplaySFXTrigger` (pitch-shifted combo SFX), `UI.HUDController` (show combo counter).

---

#### `OnBrewStarted`

Fires when an orb reaches the brew threshold and begins the brewing animation.

```csharp
public struct OnBrewStarted : IGameEvent
{
    public Vector2Int OrbPosition;
    public ElementType Element;
    public string PotionId;           // Which potion this brew contributes to
}
```

**When:** `BrewDetector` identifies an orb at threshold. Before brew animation plays.
**Subscribers:** `VFX.BrewVFX` (start brew particles and cauldron glow), `Audio.GameplaySFXTrigger` (play brew start SFX).

---

#### `OnBrewComplete`

Fires after the brew animation finishes and the potion is registered.

```csharp
public struct OnBrewComplete : IGameEvent
{
    public string PotionId;
    public ElementType Element;
    public int TotalBrewedThisLevel;  // Running count of brews for this potion in the level
    public int RequiredBrews;         // How many brews the recipe needs
    public bool RecipeFulfilled;      // True if this brew completed the recipe requirement
}
```

**When:** Brew animation completes. Orb is removed from board.
**Subscribers:** `GameFlow.RecipeTracker` (update recipe progress), `Meta.PotionShelf` (add to inventory), `VFX.BrewVFX` (celebration particles), `Audio.GameplaySFXTrigger` (brew complete SFX), `UI.HUDController` (update recipe display).

---

#### `OnCascadeStarted`

Fires when gravity begins pulling tokens down after a fusion removes tokens from the board.

```csharp
public struct OnCascadeStarted : IGameEvent
{
    public int GapCount;              // Number of empty cells that need filling
    public int ColumnsAffected;       // Number of columns with gaps
}
```

**When:** Fusion completes and gaps exist on the board.
**Subscribers:** `VFX.CascadeVFX` (enable trail effects on falling tokens), `Audio.GameplaySFXTrigger` (play cascade ambient SFX).

---

#### `OnCascadeComplete`

Fires when all tokens have settled and new tokens have spawned.

```csharp
public struct OnCascadeComplete : IGameEvent
{
    public int TokensDropped;         // Number of existing tokens that fell
    public int TokensSpawned;         // Number of new tokens created at top
    public bool HasAutoChain;         // True if the new board state has a valid cluster (will auto-fuse)
}
```

**When:** Board is stable after gravity and token spawning.
**Subscribers:** `GameFlow.BoardStateMachine` (transition to next state), `VFX.CascadeVFX` (disable trail effects).

---

#### `OnMoveUsed`

Fires every time a player move is consumed.

```csharp
public struct OnMoveUsed : IGameEvent
{
    public int MovesRemaining;
    public int MovesTotal;            // Includes any purchased extra moves
    public int MoveNumber;            // Sequential move counter (1-indexed)
}
```

**When:** After a player-initiated fusion begins (not auto-chain fusions).
**Subscribers:** `UI.HUDController` (update move counter), `GameFlow.BoardStateMachine` (track for win/lose check).

---

#### `OnLevelComplete`

Fires when the level's recipe is fully satisfied.

```csharp
public struct OnLevelComplete : IGameEvent
{
    public string LevelId;
    public int Stars;                 // 1-3
    public int FinalScore;
    public int MovesRemaining;
    public int MovesUsed;
    public float Duration;            // Seconds
    public bool IsNewBest;            // True if star rating improved
    public bool IsDailyBrew;
    public bool IsEvent;
}
```

**When:** `RecipeTracker` reports all brew requirements met and `StarCalculator` computes rating.
**Subscribers:** `Meta.StreakManager` (increment streak), `Meta.EventManager` (update event progress), `Services.FirebaseService` (submit result), `Services.AnalyticsService` (log event), `UI.RewardPopup` (show reward animation), `Audio.GameplaySFXTrigger` (victory jingle), `VFX` (celebration effects).

---

#### `OnLevelFailed`

Fires when all moves are exhausted without completing the recipe.

```csharp
public struct OnLevelFailed : IGameEvent
{
    public string LevelId;
    public int FinalScore;
    public int MovesUsed;
    public float Duration;
    public int BrewsRemaining;        // How many more brews the recipe needed
    public bool CanWatchAdForMoves;   // True if ad-for-moves option available
}
```

**When:** `MovesRemaining` reaches 0 and recipe is incomplete (after final cascade resolves).
**Subscribers:** `Meta.StreakManager` (reset streak, check protection), `Services.AnalyticsService` (log event), `UI` (show fail popup with retry/ad/quit options), `Audio.GameplaySFXTrigger` (fail jingle).

---

### Economy Events

#### `OnCurrencyChanged`

```csharp
public struct OnCurrencyChanged : IGameEvent
{
    public CurrencyType Currency;     // Essence or Gems
    public int PreviousAmount;
    public int NewAmount;
    public int Delta;                 // Positive = gained, negative = spent
    public string Reason;             // "level_reward", "booster_purchase", "iap", etc.
}
```

**When:** Any currency mutation via `CurrencyManager.Debit()` or `.Credit()`.
**Subscribers:** `UI.HUDController` (update currency display), `UI` (animate coin/gem counter).

---

#### `OnPurchaseComplete`

```csharp
public struct OnPurchaseComplete : IGameEvent
{
    public string ProductId;          // Booster ID, upgrade ID, or IAP product
    public CurrencyType CurrencyUsed;
    public int AmountSpent;
    public PurchaseSource Source;     // InGame, Workshop, Store
}
```

**When:** A purchase transaction succeeds (booster bought, upgrade purchased, IAP validated).
**Subscribers:** `Services.AnalyticsService`, `Services.FirebaseService` (trigger sync).

---

#### `OnRewardGranted`

```csharp
public struct OnRewardGranted : IGameEvent
{
    public RewardSource Source;       // LevelComplete, DailyBrew, EventTier, AdReward
    public int EssenceGranted;
    public int GemsGranted;
    public string[] ItemsGranted;    // Potion IDs, booster IDs
    public float MultiplierApplied;  // Streak multiplier (1.0 if none)
}
```

**When:** `RewardDistributor.GrantReward()` applies a reward bundle.
**Subscribers:** `UI.RewardPopup` (animate rewards), `Audio` (reward SFX).

---

#### `OnBoosterUsed`

```csharp
public struct OnBoosterUsed : IGameEvent
{
    public string BoosterId;         // "shake", "catalyst", "transmute"
    public int RemainingCount;       // Boosters of this type left
    public string LevelId;
}
```

**When:** Player activates a booster during gameplay.
**Subscribers:** `Core.Board` (apply booster effect), `VFX` (booster activation visuals), `Audio` (booster SFX), `UI.BoosterPanel` (update count), `Services.AnalyticsService`.

---

### Meta Events

#### `OnWorkshopUpgraded`

```csharp
public struct OnWorkshopUpgraded : IGameEvent
{
    public WorkshopStation Station;   // Cauldron, Flask, or Mortar
    public int PreviousLevel;
    public int NewLevel;
    public int EssenceSpent;
}
```

**When:** Player successfully upgrades a workshop station.
**Subscribers:** `UI` (play upgrade animation, update workshop display), `Audio` (upgrade SFX), `Services.AnalyticsService`.

---

#### `OnPotionCollected`

```csharp
public struct OnPotionCollected : IGameEvent
{
    public string PotionId;
    public int NewQuantity;           // Total of this potion type after collection
}
```

**When:** A brew completes and the potion is added to the shelf.
**Subscribers:** `UI` (potion shelf animation), `Audio` (collection SFX).

---

#### `OnStreakChanged`

```csharp
public struct OnStreakChanged : IGameEvent
{
    public int PreviousStreak;
    public int NewStreak;
    public float PreviousMultiplier;
    public float NewMultiplier;
    public bool ProtectionUsed;       // True if streak protection saved the streak
}
```

**When:** Win streak increments (level win), resets (level loss), or is protected.
**Subscribers:** `UI` (update streak display, show protection popup if used), `Audio` (streak SFX).

---

#### `OnDailyBrewAvailable`

```csharp
public struct OnDailyBrewAvailable : IGameEvent
{
    public string Date;               // "2026-04-09"
    public bool AlreadyCompleted;     // True if player already did today's brew
}
```

**When:** App launch or midnight rollover when daily brew feature is enabled.
**Subscribers:** `UI` (show daily brew badge/notification dot), `Audio` (notification ping).

---

#### `OnEventStarted` / `OnEventEnded`

```csharp
public struct OnEventStarted : IGameEvent
{
    public string EventId;
    public string Title;
    public string EndsAt;             // ISO 8601
}

public struct OnEventEnded : IGameEvent
{
    public string EventId;
    public int LevelsCompleted;
    public int HighestTierReached;
}
```

**When:** Event becomes active (on fetch) or timer expires.
**Subscribers:** `UI` (show/hide event tab, event banner animation), `Audio` (event theme music).

---

### UI Events

#### `OnScreenTransition`

```csharp
public struct OnScreenTransition : IGameEvent
{
    public string FromScreen;         // "Meta", "Gameplay", "Workshop"
    public string ToScreen;
    public TransitionType Type;       // Crossfade, SlideLeft, SlideRight
}
```

**When:** Screen navigation occurs.
**Subscribers:** `Audio.MusicController` (crossfade music tracks), `VFX` (transition effects).

---

#### `OnPopupShown` / `OnPopupDismissed`

```csharp
public struct OnPopupShown : IGameEvent
{
    public string PopupId;            // "reward", "fail", "settings", "booster_select"
    public bool BlocksInput;          // True if popup is modal
}

public struct OnPopupDismissed : IGameEvent
{
    public string PopupId;
    public string DismissReason;      // "button", "background_tap", "timeout"
}
```

**When:** `PopupManager` shows or hides a popup.
**Subscribers:** `Audio` (popup SFX), `GameFlow.BoardStateMachine` (pause/resume if modal).

---

#### `OnTutorialStepComplete`

```csharp
public struct OnTutorialStepComplete : IGameEvent
{
    public string TutorialId;         // "first_fusion", "first_brew", "boosters"
    public int StepIndex;
    public int TotalSteps;
    public bool TutorialFinished;     // True if this was the last step
}
```

**When:** Player completes a tutorial step (taps through instruction, performs required action).
**Subscribers:** `Services.AnalyticsService` (funnel tracking), `UI.TutorialOverlay` (advance to next step or dismiss).

---

## Performance Guidelines

### Allocation-Free Publishing

All events are `struct` types. Publishing involves no heap allocation:

```csharp
// Zero allocation — struct is stack-allocated
EventBus<OnFusionComplete>.Publish(new OnFusionComplete
{
    OrbPosition = orbPos,
    Element = element,
    OrbSize = orb.Size,
    TokensConsumed = cluster.Count,
    IsChain = false
});
```

Avoid putting reference types (strings, arrays, lists) in events when possible. Use `string` only for IDs that are already interned. For collections, prefer fixed-size fields or publish multiple events.

### Subscriber Cost Budget

| Event Category | Target Budget | Rationale |
|---|---|---|
| Core gameplay | < 0.5ms total per event | Fires during active gameplay; affects frame time |
| Economy | < 1ms total | Fires infrequently; can tolerate more work |
| Meta | < 2ms total | Fires between levels; no frame-time pressure |
| UI | < 1ms total | Should only trigger UI state changes, not heavy computation |

If a subscriber needs to do heavy work (e.g., saving to disk, making a network call), it should enqueue the work onto a coroutine or Task and return immediately.

---

## Debug Tooling

### Event Logger

In development builds, an `EventLogger` subscribes to all event types and logs them to the console with timestamps and payloads:

```
[EventBus] 14:30:01.234 OnFusionStarted { Position=(3,5), Element=Fire, TokenCount=4 }
[EventBus] 14:30:01.567 OnFusionComplete { OrbPosition=(3,5), Element=Fire, OrbSize=4, IsChain=false }
```

Toggled via a debug menu flag or Remote Config key `debug_event_logging`.

### Event Visualizer (Editor)

An editor window shows:
- All registered event types and their current subscriber counts.
- Live event feed during Play Mode.
- Click an event to see its subscriber call stack.
- Highlight events that took > 1ms to dispatch (performance warning).

### Dead Subscriber Detection

In debug builds, `EventBus<T>.Publish()` checks if any subscriber target is a destroyed `MonoBehaviour`. If detected, it logs a warning and auto-removes the subscriber. This catches missing `Unsubscribe` calls.
