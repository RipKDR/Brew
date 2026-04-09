# AGENTS.md — Brew

> Read this file before making any changes to this project.

## Project Identity

**Brew** is a mobile puzzle game for iOS and Android. Players tap clusters of ingredients to fuse them into growing orbs, chain orbs together, and brew potions to fill recipes — all before running out of moves.

- **Engine:** Unity 6 (6000.1 LTS), C# 11
- **Backend:** Firebase (Auth, Firestore, Cloud Functions, Remote Config, Analytics, Crashlytics, Cloud Messaging)
- **Ads:** AdMob + AppLovin MAX mediation
- **IAP:** Unity IAP
- **CI/CD:** GitHub Actions + Fastlane
- **Phase:** Pre-production — documentation complete, implementation begins per `docs/production/mvp-build-plan.md`

---

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER                                          │
│  UI (uGUI) · Animations (DOTween/Spine) · Particles · Audio │
├──────────────────────────────────────────────────────────────┤
│  GAME LOGIC LAYER  (pure C# — no MonoBehaviour dependency)   │
│  BoardModel · FusionEngine · CascadeResolver ·               │
│  MoveTracker · ScoreCalculator · WinLoseEvaluator            │
├──────────────────────────────────────────────────────────────┤
│  META LAYER                                                  │
│  PotionShelf · Workshop · Streaks · DailyBrew · Events ·     │
│  BoosterInventory · Economy (Essence / Gems)                 │
├──────────────────────────────────────────────────────────────┤
│  DATA LAYER                                                  │
│  LocalSaveManager · CloudSyncManager · RemoteConfigCache ·   │
│  LevelCatalog · Asset Bundles (Addressables)                 │
├──────────────────────────────────────────────────────────────┤
│  SERVICES LAYER                                              │
│  AdService · IAPService · AnalyticsService · AuthService ·   │
│  PushService · CrashReporting                                │
└──────────────────────────────────────────────────────────────┘
```

Game logic (BoardModel, FusionEngine, CascadeResolver, ScoreCalculator, WinLoseEvaluator) is pure C# with zero Unity dependencies. A MonoBehaviour `BoardPresenter` drives the visual layer. All services are accessed through a lightweight service locator with interface-based dependencies.

---

## Non-Negotiable Design Decisions

### Core Mechanic: Tap-Cluster-Fuse — NOT match-3

Brew is a **building** game, not a clearing game. Tokens fuse into orbs, orbs chain-fuse, orbs brew into potions when they accumulate 6+ tokens. The orb appears at the cell the player tapped (positional strategy). Do not implement swap-based matching, line-drawing, or any match-3 variant. See `docs/game-design/core-mechanic.md` for exact fusion rules, chain logic, cascade resolution, and the full state machine.

### Economy: Exactly 2 Currencies


| Currency    | Type             | Purpose                                                           |
| ----------- | ---------------- | ----------------------------------------------------------------- |
| **Essence** | Soft (free)      | Level rewards, Daily Brew → Workshop upgrades, boosters           |
| **Gems**    | Premium (scarce) | IAP, Daily Brew, milestones → Premium boosters, streak protection |


**Never add a third currency.** No tickets, no energy, no event tokens, no seasonal coins. The two-currency model is intentional and documented in `docs/economy/economy-model.md`.

### Levels: JSON-Driven, Never Hardcoded

Levels are data — defined as ScriptableObjects in Unity and exportable as JSON. Level parameters include grid dimensions, grid mask, ingredient pool, recipe targets, move limit, blockers, and star thresholds. Schema is in `docs/technical/data-model.md` §3. Event levels are delivered via Remote Config / Cloud Storage without an app update.

### Economy Values: Server-Configurable via Remote Config

Every tunable value — Essence per star, Gem earn rates, booster costs, workshop upgrade costs, ad frequency caps, event rewards — lives in Firebase Remote Config. Never hardcode economy numbers.

### Performance Targets


| Metric       | Requirement                                                |
| ------------ | ---------------------------------------------------------- |
| Frame rate   | 60 fps sustained on iPhone SE 2nd gen (A13) and Galaxy A14 |
| Install size | < 100 MB initial download                                  |
| Cold start   | < 5 seconds to interactive menu                            |
| Memory       | < 350 MB peak during gameplay                              |
| Network      | Fully playable offline; sync payloads < 5 KB               |


### Ethical Monetization

No dark patterns. No manufactured impossible levels. No fake timers. No loot boxes. No forced ads (interstitials are skippable after 5s). No pay-to-win. No spend gates on content. See `docs/economy/monetization-plan.md` §7 for the full list of hard rules.

---

## Project Structure (Unity)

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/               # BoardModel, FusionEngine, CascadeResolver, ScoreCalculator, WinLoseEvaluator
│   │   ├── Meta/               # PotionShelf, Workshop, Streaks, DailyBrew, Economy
│   │   ├── Data/               # SaveManager, CloudSync, RemoteConfig, LevelCatalog
│   │   ├── Services/           # AdService, IAPService, AnalyticsService, AuthService
│   │   ├── Presentation/       # BoardPresenter, TokenView, UIManager, screen controllers
│   │   └── Utilities/          # Extensions, ObjectPool, ServiceLocator
│   ├── Prefabs/
│   │   ├── Tokens/
│   │   ├── Blockers/
│   │   ├── UI/
│   │   └── VFX/
│   ├── ScriptableObjects/
│   │   ├── Levels/             # Level definitions (one SO per level)
│   │   ├── Potions/            # Potion definitions
│   │   └── Config/             # Economy config, board config, ingredient definitions
│   ├── Art/                    # Sprites, Spine, UI art
│   ├── Audio/                  # Music, SFX
│   └── Scenes/                 # Boot, MainMenu, Gameplay, Workshop
├── Plugins/                    # Firebase SDK, AdMob, Unity IAP, DOTween
├── AddressableAssetsData/
└── Tests/
    ├── EditMode/               # Pure C# unit tests for Core/ (no Unity dependency)
    └── PlayMode/               # Integration tests for presentation
```

### Where to Put Things


| What you're adding                 | Where it goes                               |
| ---------------------------------- | ------------------------------------------- |
| Board/fusion/cascade/scoring logic | `Scripts/Core/` (pure C#, no MonoBehaviour) |
| Workshop, potion shelf, streaks    | `Scripts/Meta/`                             |
| Save/load, cloud sync, config      | `Scripts/Data/`                             |
| Ads, IAP, analytics, auth          | `Scripts/Services/`                         |
| Anything that touches GameObjects  | `Scripts/Presentation/`                     |
| Shared helpers, pools, extensions  | `Scripts/Utilities/`                        |
| Configuration data                 | `ScriptableObjects/Config/`                 |
| Level definitions                  | `ScriptableObjects/Levels/`                 |
| Unit tests for game logic          | `Tests/EditMode/`                           |
| Integration/UI tests               | `Tests/PlayMode/`                           |


---

## Code Conventions

### Naming


| Element                | Convention | Example                           |
| ---------------------- | ---------- | --------------------------------- |
| Public method/property | PascalCase | `CalculateScore()`, `MoveCount`   |
| Private field          | _camelCase | `_currentLevel`, `_boardModel`    |
| Parameter / local      | camelCase  | `clusterSize`, `targetCell`       |
| Interface              | I-prefix   | `IAnalyticsService`, `IAdService` |
| Constant               | PascalCase | `BrewThreshold = 6`               |
| Enum value             | PascalCase | `CellState.Idle`                  |


No Hungarian notation. No `m`_ prefix. No public fields — use properties or `[SerializeField]` private fields.

### Unity Patterns

- **MonoBehaviour only for Unity lifecycle hooks.** Game logic lives in plain C# classes. MonoBehaviours call into logic classes, not the other way around.
- **ScriptableObjects for all config.** Board configs, level data, economy values, ingredient definitions, potion definitions — all ScriptableObjects.
- **Events for decoupling.** Use C# `event`/`Action` or UnityEvents. Never direct references between managers (e.g., `EconomyManager` should not hold a reference to `WorkshopManager`).
- **Object pooling.** Mandatory for tokens, orbs, particles. Never `Instantiate`/`Destroy` in the gameplay loop. Use the project's `ObjectPool<T>` utility.
- **Async/await via UniTask** over coroutines for sequenced logic. Coroutines only for simple animation timing when UniTask is overkill.
- `**[SerializeField]`** for inspector-exposed private fields. Never expose fields as `public`.

### Board State Machine

The board follows a strict state sequence. **Never bypass states.**

```
Idle → PlayerInput → Fusing → Cascading → Settling → CheckBrew → CheckWin → Idle
```

Full state diagram with substates is in `docs/technical/architecture.md` §2.2.

---

## Testing Expectations

### Unit Tests (EditMode)

Required coverage for all game logic in `Scripts/Core/`:

- **Fusion:** Cluster detection (BFS flood-fill), valid cluster threshold (≥ 3), orb placement at tapped cell, token_count calculation
- **Chain fusion:** Adjacent same-color orb detection, survivor selection (bottom-most, then left-most), merged token_count accumulation
- **Brewing:** Threshold trigger at token_count ≥ 6, recipe target decrement, score award (200 target / 100 non-target)
- **Cascading:** Gravity, refill, auto-fuse detection, cascade wave counting
- **Scoring:** Base formula (cluster_size × 10), chain multiplier (+0.5 per step), cascade multiplier (+0.5 per wave), multiplier stacking, remaining-move bonus
- **Win/Lose:** Recipe completion check, move exhaustion check, near-miss detection

### Integration Tests (PlayMode)

- Economy flows: earn → display → spend → balance update → sync
- Booster activation and effect verification
- Analytics event firing on key actions

---

## What to Read Before Making Changes

### Before touching any code:

1. `docs/game-design/GDD.md` — understand the game
2. `docs/game-design/core-mechanic.md` — understand the rules (this is the bible)

### By domain:


| Domain                                  | Read first                                                                |
| --------------------------------------- | ------------------------------------------------------------------------- |
| Board logic, fusion, cascades           | `docs/game-design/core-mechanic.md`                                       |
| Level design, difficulty                | `docs/game-design/level-design-framework.md`                              |
| Tutorial flow                           | `docs/game-design/tutorial-flow.md`                                       |
| Meta systems (shelf, workshop, streaks) | `docs/game-design/meta-systems.md`                                        |
| Economy, currency, spending             | `docs/economy/economy-model.md`                                           |
| Monetization, IAP, ads                  | `docs/economy/monetization-plan.md`                                       |
| Architecture, state machine, layers     | `docs/technical/architecture.md`                                          |
| Data schemas, JSON formats              | `docs/technical/data-model.md`                                            |
| Firebase, CI/CD, infra                  | `docs/technical/infrastructure.md`                                        |
| Analytics events                        | `docs/analytics/event-tracking-plan.md`                                   |
| KPI targets                             | `docs/analytics/kpi-framework.md`                                         |
| A/B tests                               | `docs/analytics/ab-test-plan.md`                                          |
| Build plan, milestones                  | `docs/production/mvp-build-plan.md`, `docs/production/milestone-gates.md` |
| Art, visuals                            | `docs/creative/art-direction.md`                                          |
| Sound, haptics                          | `docs/creative/sound-design.md`                                           |
| Live-ops, events                        | `docs/liveops/event-templates.md`, `docs/liveops/content-calendar.md`     |


---

## Common Mistakes to Avoid

1. **Adding a currency.** There are exactly two: Essence and Gems. Do not add energy, tickets, event tokens, or any other currency.
2. **Hardcoding economy values.** Every currency amount, cost, reward, and cap must come from RemoteConfig or ScriptableObjects. If you're typing a number that represents a currency amount into a `.cs` file, you're doing it wrong.
3. **Breaking the board state machine sequence.** The states (Idle → Input → Fusing → Cascading → Settling → CheckBrew → CheckWin → Idle) must be traversed in order. Never skip states, never process input during resolution, never check win before cascades complete.
4. **Skipping analytics events.** Every gameplay action, economy transaction, and meta event must fire the corresponding event from `docs/analytics/event-tracking-plan.md`. Missing analytics is a blocking bug.
5. **Using MonoBehaviour for game logic.** Board logic, fusion, cascading, scoring — all pure C#. MonoBehaviour is only for Unity lifecycle hooks and presentation.
6. **Instantiate/Destroy in gameplay loop.** All tokens, orbs, and particles must be object-pooled. Allocation during gameplay causes frame spikes.
7. **Direct references between managers.** Use events or the service locator. `EconomyManager` should not `GetComponent<WorkshopManager>()`.
8. **Magic numbers.** If a value controls gameplay behavior, it belongs in a ScriptableObject or RemoteConfig. `if (clusterSize >= 6)` is wrong; `if (clusterSize >= _brewConfig.BrewThreshold)` is right.
9. **Implementing match-3 mechanics.** This is NOT a match-3 game. No swapping, no line-matching, no board-clearing. The core mechanic is tap-cluster-fuse-brew.
10. **Crashing on Firebase errors.** All network operations must degrade gracefully. If Remote Config fetch fails, use cached values. If Firestore write fails, queue locally. If ad load fails, hide the ad button. Never show an error dialog to the player for backend failures.
11. **Changing economy without updating docs.** The rule is: update `docs/economy/economy-model.md` FIRST, then implement. Documents and code stay in sync at all times.
12. **Deviating from the documented game design.** If a proposed change contradicts any spec in `docs/`, stop and ask a human before implementing. The documentation is the source of truth.

---

## Key Constants (Reference)


| Constant                   | Value                                      | Source                   |
| -------------------------- | ------------------------------------------ | ------------------------ |
| Brew threshold             | 6 tokens                                   | `core-mechanic.md` §6.1  |
| Min cluster size           | 3 tokens                                   | `core-mechanic.md` §3.3  |
| Chain survivor rule        | Bottom-most, then left-most                | `core-mechanic.md` §5.3  |
| Grid size range            | 5–9 wide × 5–11 tall                       | `core-mechanic.md` §1.2  |
| Token types                | 5 (Ember, Frost, Vine, Sun, Shadow)        | `core-mechanic.md` §2.1  |
| Score per fusion           | cluster_size × 10                          | `core-mechanic.md` §13.1 |
| Chain multiplier           | +0.5 per step                              | `core-mechanic.md` §14.1 |
| Cascade multiplier         | +0.5 per wave                              | `core-mechanic.md` §14.2 |
| Brew score (target)        | 200                                        | `core-mechanic.md` §6.2  |
| Brew score (non-target)    | 100                                        | `core-mechanic.md` §6.2  |
| Remaining move bonus       | 50 per move                                | `core-mechanic.md` §13.2 |
| Workshop total cost        | 36,350 Essence                             | `economy-model.md` §4.1  |
| Workshop upgrades          | 12 tiers                                   | `economy-model.md` §4.1  |
| Workshop target completion | 30–45 days (regular player, 10 levels/day) | `economy-model.md` §6.1  |


