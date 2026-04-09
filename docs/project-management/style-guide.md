# Brew — C# & Project Style Guide

> Canonical coding conventions for the Brew Unity project.
> All contributors must follow this guide. PRs that violate these conventions will be rejected in review.
> Last updated: 2026-04-09

---

## 1. Naming Conventions

| Element | Convention | Examples |
|---------|-----------|----------|
| Classes / Structs | PascalCase | `BoardManager`, `FusionEngine`, `BrewOrb`, `CascadeResolver` |
| Public methods | PascalCase | `GetCluster()`, `TryFuse()`, `StartBrew()` |
| Public properties | PascalCase | `TokenCount`, `CurrentState`, `IsBrewReady` |
| Private fields | _camelCase | `_currentState`, `_tokenPool`, `_cascadeDepth` |
| Local variables | camelCase | `clusterSize`, `targetRecipe`, `adjacentCells` |
| Method parameters | camelCase | `gridWidth`, `orbColor`, `moveCount` |
| Constants | UPPER_SNAKE_CASE | `MAX_GRID_WIDTH`, `MIN_CLUSTER_SIZE`, `CASCADE_DEPTH_LIMIT` |
| Static readonly fields | PascalCase | `DefaultBoardSize`, `EmptyCell` |
| Interfaces | I-prefixed PascalCase | `IPoolable`, `IFusable`, `IBoardElement` |
| Enums (type) | PascalCase | `BoardState`, `IngredientType`, `BoosterKind` |
| Enum values | PascalCase | `BoardState.WaitingForInput`, `IngredientType.Ember` |
| Events / delegates | On-prefixed PascalCase | `OnFusionComplete`, `OnBrewStarted`, `OnLevelFailed` |
| ScriptableObjects | SO-suffixed PascalCase | `LevelConfigSO`, `IngredientDefinitionSO`, `BoardConfigSO` |
| Namespaces | PascalCase, match folder | `Brew.Core`, `Brew.UI`, `Brew.Meta`, `Brew.Services` |
| Test classes | ClassUnderTest + Tests | `FusionEngineTests`, `CascadeResolverTests` |
| Test methods | MethodUnderTest_Condition_ExpectedResult | `TryFuse_ClusterTooSmall_ReturnsFalse` |

### Naming Rules

- Boolean properties and fields use `is`, `has`, `can`, or `should` prefixes: `IsBrewReady`, `_hasSettled`, `CanFuse`.
- Avoid abbreviations except universally understood ones (`UI`, `ID`, `IO`, `SO`). Write `position` not `pos`, `manager` not `mgr`.
- Collection variables use plural nouns: `tokens`, `adjacentCells`, `activeOrbs`.
- Callback parameters use the `on` prefix: `onComplete`, `onFusionFinished`.

---

## 2. File Organization

### One Class Per File

Each `.cs` file contains exactly one public class, struct, or interface. The file name matches the type name.

**Exceptions:** small related types may share a file when they are tightly coupled and under ~20 lines total. Examples:
- An enum used exclusively by the class in the same file.
- A struct used as a return type only by the class in the same file.

### Folder Structure Matches Namespaces

```
Assets/Scripts/
├── Core/                    # Brew.Core
│   ├── BoardManager.cs
│   ├── FusionEngine.cs
│   ├── CascadeResolver.cs
│   ├── GravitySystem.cs
│   └── TokenSpawner.cs
├── Data/                    # Brew.Data
│   ├── LevelConfigSO.cs
│   ├── IngredientDefinitionSO.cs
│   └── BoardConfigSO.cs
├── Meta/                    # Brew.Meta
│   ├── PotionShelf.cs
│   ├── Workshop.cs
│   └── StreakTracker.cs
├── UI/                      # Brew.UI
│   ├── HUD.cs
│   ├── PotionVial.cs
│   └── LevelCompletePopup.cs
├── Services/                # Brew.Services
│   ├── AnalyticsService.cs
│   ├── AdService.cs
│   ├── IAPService.cs
│   └── RemoteConfigService.cs
├── Input/                   # Brew.Input
│   └── InputHandler.cs
├── Audio/                   # Brew.Audio
│   └── AudioManager.cs
└── Utils/                   # Brew.Utils
    ├── ObjectPool.cs
    └── GridHelper.cs
```

### File Layout Within a Class

```csharp
// 1. Using statements (System first, then Unity, then project)
using System;
using System.Collections.Generic;
using UnityEngine;
using Brew.Data;

namespace Brew.Core
{
    // 2. XML doc summary
    /// <summary>
    /// Resolves chain-fusion sequences after gravity settles.
    /// </summary>
    public class CascadeResolver : MonoBehaviour
    {
        // 3. Constants
        private const int CASCADE_DEPTH_LIMIT = 20;

        // 4. Static fields

        // 5. Serialized fields
        [SerializeField] private BoardManager _boardManager;

        // 6. Private fields
        private int _currentDepth;

        // 7. Events
        public event Action<int> OnCascadeComplete;

        // 8. Properties
        public bool IsCascading => _currentDepth > 0;

        // 9. Unity lifecycle (Awake, OnEnable, Start, Update, OnDisable, OnDestroy)

        // 10. Public methods

        // 11. Private methods
    }
}
```

---

## 3. Code Patterns

### Dependency Injection

Inject dependencies via constructor or `[SerializeField]`. Never use `Find()`, `FindObjectOfType()`, or `GetComponent()` in `Update()`.

```csharp
// Correct: injected via inspector
[SerializeField] private FusionEngine _fusionEngine;

// Correct: resolved once in Awake
private InputHandler _inputHandler;
private void Awake() => _inputHandler = GetComponent<InputHandler>();

// Wrong: resolved every frame
private void Update()
{
    var engine = FindObjectOfType<FusionEngine>(); // Never do this
}
```

### Object Pooling

All frequently created and destroyed objects must use pooling. Tokens, orbs, particles, and UI popups are pooled. Never call `Instantiate`/`Destroy` in hot paths.

```csharp
public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}
```

### ScriptableObjects for Data

All data that a designer might tune — grid dimensions, move budgets, economy values, ingredient definitions, brew thresholds — must live in ScriptableObjects or Firebase Remote Config. Never hardcode tunable values.

```csharp
// Correct
[SerializeField] private LevelConfigSO _levelConfig;
int moveLimit = _levelConfig.MoveLimit;

// Wrong
int moveLimit = 25;
```

### Events for Cross-Module Communication

Modules communicate through events, not direct references to unrelated systems. The board logic layer does not know about the UI layer.

```csharp
// In FusionEngine
public event Action<FusionResult> OnFusionComplete;

// In HUD (subscribed in OnEnable, unsubscribed in OnDisable)
private void HandleFusionComplete(FusionResult result)
{
    _moveCounter.UpdateDisplay(result.MovesRemaining);
}
```

### State Machine for Board Flow

Board game flow is controlled by a state machine with explicit states and transitions. Never use if-else chains or boolean flags to track game state.

```csharp
public enum BoardState
{
    Initializing,
    WaitingForInput,
    Fusing,
    Cascading,
    Settling,
    Brewing,
    CheckingWin,
    LevelComplete,
    LevelFailed
}
```

Transitions are deterministic and driven by events. Each state has clear entry, tick, and exit logic.

### Async/Await Over Coroutines

Prefer `async`/`await` (via UniTask) over coroutines for asynchronous operations. Coroutines are acceptable for simple animation sequences tightly coupled to MonoBehaviour lifecycle.

```csharp
// Preferred: UniTask
public async UniTask PlayFusionSequence(FusionResult result)
{
    await AnimateTokensToOrb(result.Tokens, result.OrbPosition);
    await AnimateOrbSpawn(result.OrbPosition);
    OnFusionAnimationComplete?.Invoke();
}

// Acceptable for simple MonoBehaviour-bound sequences
private IEnumerator FlashHighlight()
{
    _renderer.color = Color.white;
    yield return new WaitForSeconds(0.1f);
    _renderer.color = _originalColor;
}
```

### Null Safety

- Use `TryGet` patterns over null checks where possible.
- Use `??` and `??=` for fallback values.
- Never silently swallow null — log a warning at minimum.
- Unity objects: use `== null` (not `is null`) due to Unity's custom null equality.

---

## 4. Comments Policy

### Do Not Comment

- Obvious operations: `// increment counter`, `// return result`, `// loop through tokens`
- What the code does when naming already conveys it
- Change history: that is what git is for

### Do Comment

- **Why** a non-obvious algorithm or approach was chosen
- Edge cases and boundary conditions that are not self-evident
- Threshold values that look like magic numbers (reference the design doc)
- Workarounds for known Unity or SDK bugs (include issue link)
- Performance-critical sections explaining why a specific optimization exists

### XML Documentation

All public APIs must have XML doc summaries. Parameters and return values documented when non-obvious.

```csharp
/// <summary>
/// Attempts to fuse the cluster containing the token at the given position.
/// Returns null if the cluster is smaller than MIN_CLUSTER_SIZE.
/// </summary>
/// <param name="tapPosition">Grid coordinates of the tapped cell.</param>
/// <returns>FusionResult if successful, null otherwise.</returns>
public FusionResult TryFuse(Vector2Int tapPosition)
```

---

## 5. Formatting

| Rule | Value |
|------|-------|
| Indentation | 4 spaces (no tabs) |
| Max line length | 120 characters (soft limit) |
| Braces | Allman style (opening brace on new line) |
| Single-line members | Expression-bodied syntax allowed for one-liners |
| Trailing whitespace | None |
| End-of-file newline | Required |
| Blank lines between members | One |
| `using` directives | Inside namespace: no. Top of file: yes. |

```csharp
// Allman braces
public void ProcessFusion(FusionResult result)
{
    if (result == null)
    {
        Debug.LogWarning("Null fusion result");
        return;
    }

    ApplyFusion(result);
}

// Expression-bodied single-liners
public int TokenCount => _tokens.Count;
public bool IsEmpty => _content == CellContent.Empty;
```

---

## 6. Error Handling

- Use exceptions for programmer errors (invalid state, null arguments that should never be null).
- Use return values (`bool TryX`, nullable returns, Result types) for expected failure cases (no valid cluster, move limit reached).
- Log warnings with `Debug.LogWarning` for recoverable unexpected states.
- Log errors with `Debug.LogError` for states that indicate a bug but do not crash.
- Never use try/catch as flow control.

---

## 7. Git Conventions

### Branch Naming

| Prefix | Use |
|--------|-----|
| `feature/` | New functionality (e.g., `feature/chain-fusion`) |
| `fix/` | Bug fixes (e.g., `fix/gravity-gap-on-edge`) |
| `docs/` | Documentation changes (e.g., `docs/update-economy-model`) |
| `refactor/` | Code restructuring with no behavior change |
| `chore/` | Build, CI, tooling changes |

### Commit Messages

- Imperative mood: "Add cascade resolver" not "Added cascade resolver"
- First line: <72 characters, summarizes the change
- Body (optional): explains why, not what
- Reference issue number if applicable: `Fixes #42`

```
Add cascade depth limit to prevent infinite loops

The recursive chain resolution could theoretically loop if gravity
created a cycle. Cap at 20 iterations per the core-mechanic spec.

Refs: docs/game-design/core-mechanic.md §5.3
```

### Pull Requests

- Must pass CI (build + lint + tests)
- Must have at least 1 approving review
- Must reference the relevant doc if the change affects game design, economy, or architecture
- Squash-merge to `main` with a clean commit message
- Delete branch after merge

### Git LFS

Binary assets are tracked via Git LFS. These extensions are managed:

```
*.png, *.jpg, *.psd, *.tga
*.wav, *.mp3, *.ogg
*.fbx, *.obj
*.prefab, *.unity, *.asset
*.dll
```

### .gitignore

The project `.gitignore` covers standard Unity ignores (`Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`), IDE files (`.vs/`, `.idea/`), and OS files (`.DS_Store`, `Thumbs.db`).

---

## 8. Testing

- Unit tests live alongside source in a `Tests/` folder mirroring the main structure.
- Test naming: `MethodUnderTest_Condition_ExpectedResult`.
- Use Unity Test Framework (NUnit) for edit-mode tests, play-mode tests for integration.
- Mock external dependencies (Firebase, ads, IAP) in tests.
- Every public method in the Core and Meta namespaces must have test coverage.
- Tests run in CI on every push.

---

## 9. Analytics Integration

Every feature must fire the corresponding analytics events defined in the [Event Tracking Plan](../analytics/event-tracking-plan.md). Events are fired through the `AnalyticsService` abstraction — never directly via the Firebase SDK.

```csharp
// Correct: abstracted
_analyticsService.LogFusionComplete(clusterSize, orbTier, movesRemaining);

// Wrong: direct SDK call
FirebaseAnalytics.LogEvent("fusion_complete", ...);
```

This abstraction enables swapping analytics providers without touching game logic.

---

## 10. Performance Guidelines

| Rule | Rationale |
|------|-----------|
| No allocations in Update/FixedUpdate | GC spikes cause frame drops on mobile |
| Pool all frequently instantiated objects | Avoid GC pressure from Instantiate/Destroy |
| Cache component references in Awake | GetComponent is expensive per-frame |
| Use `struct` over `class` for small value types | Avoid heap allocations for grid coordinates, cell data |
| Sprite atlasing for all UI and token art | Reduce draw calls |
| Target 60 fps on mid-range devices, 30 fps floor on min-spec | iPhone SE 2nd gen / Android API 26 equivalent |
| Profile weekly with Unity Profiler | Catch regressions before they compound |
