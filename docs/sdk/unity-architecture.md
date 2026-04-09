# Unity Project Architecture

> Brew — Unity 2022 LTS, C# (.NET Standard 2.1)
> Modular architecture with strict dependency rules and event-driven communication.

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           — Board, tokens, orbs, fusion engine, cascade, brew detection
│   ├── GameFlow/       — Level loading, state machine, win/lose, scoring
│   ├── Meta/           — Workshop, potion shelf, streaks, daily brew
│   ├── Economy/        — Currency manager, transaction log, reward distribution
│   ├── UI/             — HUD, menus, popups, transitions, tutorial overlays
│   ├── Services/       — Firebase wrapper, ads manager, IAP handler, analytics
│   ├── Data/           — ScriptableObjects, JSON loaders, remote config cache
│   ├── Audio/          — Sound manager, SFX triggers, music controller
│   ├── VFX/            — Particle controllers, screen effects, juice system
│   └── Utils/          — Object pool, math helpers, extensions, coroutine utils
├── Prefabs/
│   ├── Tokens/         — Token prefab variants per element type
│   ├── Orbs/           — Orb prefab variants per element type
│   ├── UI/             — Reusable UI prefab components
│   ├── VFX/            — Particle system prefabs
│   └── Boosters/       — Booster activation effect prefabs
├── Art/
│   ├── Sprites/        — Token, orb, background, UI sprite atlases
│   ├── Animations/     — Animator controllers and clips
│   └── Materials/      — Shader materials, UI materials
├── Audio/
│   ├── SFX/            — Sound effect clips
│   └── Music/          — Background music tracks
├── Data/
│   ├── Levels/         — Level definition JSON files
│   ├── Configs/        — ScriptableObject config assets
│   └── Defaults/       — default_config.json (Remote Config fallback)
├── Scenes/
│   ├── Boot.unity      — Initialization, service setup, auth
│   ├── Meta.unity      — Main menu, workshop, potion shelf, map
│   └── Gameplay.unity  — Board, HUD, level play
├── Plugins/
│   ├── Firebase/       — Firebase SDK native plugins
│   ├── Ads/            — Ad mediation SDK
│   └── IAP/            — Unity IAP plugin
└── Editor/
    ├── LevelEditor/    — Custom level design inspector tools
    ├── ConfigEditor/   — ScriptableObject inspectors
    └── BuildTools/     — Build pipeline scripts
```

---

## Dependency Rules

```
                    ┌─────────┐
                    │  Utils  │  ← No dependencies. Pure utility.
                    └────▲────┘
                         │
                    ┌────┴────┐
                    │  Core   │  ← Depends only on Utils.
                    └────▲────┘
                         │
              ┌──────────┼──────────┐
              │          │          │
         ┌────┴────┐ ┌──┴───┐ ┌───┴───┐
         │GameFlow │ │ Data │ │ Audio │
         └────▲────┘ └──▲───┘ └───▲───┘
              │         │         │
         ┌────┴────┐    │         │
         │Economy  │────┘         │
         └────▲────┘              │
              │                   │
         ┌────┴────┐              │
         │  Meta   │──────────────┘
         └────▲────┘
              │
         ┌────┴────┐
         │Services │  ← Injected into any module that needs backend access.
         └────▲────┘
              │
         ┌────┴────┐
         │   VFX   │  ← Subscribes to Core events. No direct Core dependency.
         └─────────┘

         ┌─────────┐
         │   UI    │  ← Depends on NOTHING directly. Communicates entirely via EventBus.
         └─────────┘
```

### Hard Rules

1. **Core has ZERO dependencies** on any game module (only Utils).
2. **GameFlow depends on Core** — it orchestrates Core systems but never exposes Core internals upward.
3. **Meta depends on Economy** — workshop upgrades and rewards flow through Economy.
4. **UI depends on nothing directly.** All data flows through the EventBus. UI subscribes to events and dispatches user-intent events. No `using Brew.Core;` in UI code.
5. **Services are injected**, never accessed via static singletons. Modules receive service interfaces (`IFirebaseService`, `IAdsService`) through constructor or `[Inject]` attribute.
6. **Data is read-only at runtime.** ScriptableObjects and JSON configs are loaded once and cached. Mutations go through Economy or Services.
7. **VFX and Audio subscribe to events.** They react to gameplay — they never drive it.

---

## Module Specifications

### Core

**Purpose:** Pure gameplay logic. The board, tokens, orbs, fusion detection, cascade simulation, and brew evaluation. This module could run headless in a unit test with no Unity scene.

| Class | Responsibility |
|---|---|
| `Board` | 2D grid of cells. Manages token placement, neighbor queries, and spatial lookups. |
| `Token` | Represents a single element token on the board. Has element type, grid position, and visual state. |
| `Orb` | Formed by fusing tokens. Tracks element type, size (fusion count), and brew progress. |
| `FusionEngine` | Detects valid clusters (≥ `min_cluster_size` adjacent same-type tokens). Executes fusion: removes tokens, creates or grows orb. |
| `CascadeResolver` | After fusion, applies gravity (tokens fall to fill gaps), spawns new tokens at top, and repeats until stable. |
| `BrewDetector` | Checks if any orb has reached `brew_threshold`. Triggers brew (potion creation). |
| `BoardValidator` | Scans board for valid moves. If none exist, triggers auto-shuffle. |
| `MoveProcessor` | Accepts a player tap (grid coordinate), validates it, and initiates the fusion→cascade→brew pipeline. |

**Dependencies:** `Utils` only.

**Depends on Core:** `GameFlow`, `VFX` (via events), `Audio` (via events).

**Public Interface:**

```csharp
// Board queries
Cell GetCell(int x, int y);
List<Token> GetClusterAt(int x, int y);
bool HasValidMoves();

// Actions
FusionResult ProcessTap(Vector2Int position);
void Shuffle();
void Reset(LevelDefinition level);

// Read-only state
int RemainingMoves { get; }
BoardState CurrentState { get; }
```

**Events Published:** `OnFusionStarted`, `OnFusionComplete`, `OnChainFusion`, `OnBrewStarted`, `OnBrewComplete`, `OnCascadeStarted`, `OnCascadeComplete`, `OnBoardShuffled`.

**Events Subscribed:** None. Core is event-source only.

---

### GameFlow

**Purpose:** Level lifecycle management. Loads level definitions, runs the board state machine, tracks scoring, and determines win/lose conditions.

| Class | Responsibility |
|---|---|
| `LevelManager` | Loads `LevelDefinition` from Data, initializes Board, tracks level progress. |
| `BoardStateMachine` | Governs board state transitions (see state-machine-spec.md). The authoritative controller of "what the board is doing right now." |
| `ScoreTracker` | Accumulates score during a level. Applies multipliers (cascade bonuses, streak multipliers). |
| `RecipeTracker` | Monitors brew events against the level's recipe. Determines when the recipe is fulfilled (win condition). |
| `StarCalculator` | Given final score and level star thresholds, computes 1–3 star rating. |
| `LevelResultBuilder` | Assembles the `LevelResult` payload for `submitLevelResult` API call. |

**Dependencies:** `Core`, `Data`.

**Depends on GameFlow:** `Meta` (for level progression), `UI` (via events).

**Public Interface:**

```csharp
void LoadLevel(string levelId);
void StartLevel();
void PauseLevel();
void ResumeLevel();
void RequestExtraMoves();
LevelState GetCurrentState();
```

**Events Published:** `OnLevelLoaded`, `OnLevelStarted`, `OnLevelComplete`, `OnLevelFailed`, `OnMoveUsed`, `OnScoreChanged`, `OnRecipeProgress`.

**Events Subscribed:** `OnFusionComplete` (update score), `OnBrewComplete` (update recipe), `OnCascadeComplete` (check win/lose).

---

### Meta

**Purpose:** Metagame systems that exist outside individual levels. Workshop upgrades, potion collection, win streaks, daily brew, and event progress.

| Class | Responsibility |
|---|---|
| `WorkshopManager` | Manages upgrade state for cauldron, flask, and mortar. Validates upgrade costs against Economy. |
| `PotionShelf` | Inventory of brewed potions. Tracks quantities. Read by UI for display. |
| `StreakManager` | Tracks consecutive win count. Applies streak multipliers from Remote Config. Handles streak protection. |
| `DailyBrewManager` | Checks if today's daily brew is available. Tracks completion history. |
| `EventManager` | Tracks player progress within the active event. Manages reward tier claims. |

**Dependencies:** `Economy`, `Data`, `Services` (injected).

**Depends on Meta:** `UI` (via events).

**Public Interface:**

```csharp
// Workshop
bool CanUpgrade(WorkshopStation station);
void PurchaseUpgrade(WorkshopStation station);
int GetStationLevel(WorkshopStation station);

// Streaks
int CurrentStreak { get; }
float CurrentMultiplier { get; }

// Daily
bool IsDailyBrewAvailable { get; }
```

**Events Published:** `OnWorkshopUpgraded`, `OnPotionCollected`, `OnStreakChanged`, `OnDailyBrewAvailable`, `OnEventStarted`, `OnEventEnded`, `OnEventProgress`.

**Events Subscribed:** `OnLevelComplete` (update streak, event progress), `OnBrewComplete` (add potion to shelf).

---

### Economy

**Purpose:** Single source of truth for currency balances and all transactions. Every essence/gem change goes through Economy — no module modifies currency directly.

| Class | Responsibility |
|---|---|
| `CurrencyManager` | Holds current balances for essence and gems. All mutations are transactional (debit/credit with reason). |
| `TransactionLog` | Append-only log of every currency change. Used for debugging and anti-cheat. Periodically synced to server. |
| `RewardDistributor` | Given a `RewardBundle` (from level completion, event tier, daily brew), applies it atomically: currency + items. |
| `PurchaseValidator` | Checks if a purchase (booster, upgrade, extra moves) is affordable. Returns success/insufficient funds. |

**Dependencies:** `Data` (for cost lookups from Remote Config cache).

**Depends on Economy:** `Meta`, `GameFlow` (via RewardDistributor).

**Public Interface:**

```csharp
int Essence { get; }
int Gems { get; }
bool CanAfford(CurrencyType type, int amount);
TransactionResult Debit(CurrencyType type, int amount, string reason);
TransactionResult Credit(CurrencyType type, int amount, string reason);
void GrantReward(RewardBundle reward);
```

**Events Published:** `OnCurrencyChanged`, `OnPurchaseComplete`, `OnRewardGranted`, `OnInsufficientFunds`.

**Events Subscribed:** None. Economy is called imperatively by other modules.

---

### UI

**Purpose:** All visual interface elements. HUD during gameplay, menus, popups, transitions, and tutorial overlays. UI is a pure consumer of events and a publisher of user-intent events.

| Class | Responsibility |
|---|---|
| `HUDController` | In-game display: move counter, score, recipe progress, booster buttons. |
| `MenuController` | Main menu navigation: play, workshop, potion shelf, settings, daily brew. |
| `PopupManager` | Modal popup lifecycle: show, queue, dismiss. Handles stacking. |
| `ScreenTransition` | Crossfade/slide transitions between scenes and major UI states. |
| `TutorialOverlay` | Step-by-step tutorial system with highlight masks and instruction text. |
| `SettingsPanel` | Audio, haptics, notification toggles. Reads/writes local preferences. |
| `BoosterPanel` | Booster selection and activation UI. Shows costs, quantities, and cooldowns. |
| `RewardPopup` | Animated reward reveal (coins flying, stars appearing). |

**Dependencies:** None (receives all data via EventBus).

**Depends on UI:** Nothing depends on UI.

**Public Interface:** UI exposes no public API. Other modules interact with it exclusively through events.

**Events Published:** `OnPlayButtonPressed`, `OnBoosterSelected`, `OnPauseRequested`, `OnResumeRequested`, `OnExtraMovesRequested`, `OnPopupDismissed`, `OnTutorialStepComplete`, `OnSettingsChanged`.

**Events Subscribed:** `OnMoveUsed`, `OnScoreChanged`, `OnRecipeProgress`, `OnCurrencyChanged`, `OnLevelComplete`, `OnLevelFailed`, `OnStreakChanged`, `OnDailyBrewAvailable`, `OnWorkshopUpgraded`, `OnBrewComplete`, `OnFusionComplete`, `OnScreenTransition`.

---

### Services

**Purpose:** Abstracted wrappers around third-party SDKs. Every external dependency is behind an interface so modules never reference Firebase, AdMob, or Unity IAP directly.

| Class | Responsibility |
|---|---|
| `FirebaseService` : `IFirebaseService` | Wraps Firebase Auth, Firestore, Cloud Functions, and Remote Config. Exposes typed methods like `SubmitLevelResult(LevelResult)`. |
| `AdsManager` : `IAdsService` | Manages rewarded and interstitial ads. Tracks session ad count, enforces cooldowns. Exposes `ShowRewardedAd(callback)`. |
| `IAPHandler` : `IIAPService` | Wraps Unity IAP. Handles purchase flow, receipt forwarding to `validateIAPReceipt`, and purchase restoration. |
| `AnalyticsService` : `IAnalyticsService` | Wraps Firebase Analytics. Exposes typed event methods like `LogLevelComplete(levelId, stars, score)`. |
| `CrashlyticsService` : `ICrashlyticsService` | Wraps Firebase Crashlytics. Logs non-fatal errors, sets user properties. |
| `PushNotificationService` : `IPushService` | Wraps Firebase Cloud Messaging. Handles token registration and notification display. |

**Dependencies:** Firebase SDK, Ad SDK, IAP SDK (via Plugins/).

**Depends on Services:** Any module that needs backend access receives a service interface via injection.

**Public Interface (example):**

```csharp
public interface IFirebaseService
{
    Task<bool> SignInAnonymously();
    Task<PlayerState> SyncPlayerState(PlayerState clientState);
    Task<LevelResult> SubmitLevelResult(LevelResultPayload payload);
    Task<DailyBrewLevel> GetDailyBrew();
    Task<List<GameEvent>> GetActiveEvents();
    Task<RemoteConfig> GetRemoteConfig(string clientVersion, string platform);
    Task<IAPValidationResult> ValidateIAPReceipt(IAPReceiptPayload receipt);
}
```

**Events Published:** `OnAuthStateChanged`, `OnSyncComplete`, `OnRemoteConfigUpdated`, `OnPushNotificationReceived`.

**Events Subscribed:** `OnLevelComplete` (trigger auto-sync), `OnPurchaseComplete` (trigger sync).

---

### Data

**Purpose:** Static data definitions and runtime configuration cache. Level definitions, ScriptableObject configs, and the local copy of Remote Config values.

| Class | Responsibility |
|---|---|
| `LevelDefinition` (ScriptableObject) | Board dimensions, move limit, recipe, token distribution, star thresholds, seed. |
| `LevelCatalog` | Registry of all levels. Supports lookup by ID, chapter, and level number. |
| `RemoteConfigCache` | Local cache of Remote Config key-values. Provides typed getters with fallback defaults. |
| `GameConfig` (ScriptableObject) | Compile-time constants that never change via Remote Config (e.g., max board size, supported element types). |
| `JsonLoader` | Utility for loading and parsing JSON level files from `Data/Levels/`. |

**Dependencies:** `Utils`.

**Depends on Data:** `Core` (reads `LevelDefinition`), `Economy` (reads cost values), `Meta` (reads streak multipliers), `GameFlow` (reads level catalog).

**Public Interface:**

```csharp
LevelDefinition GetLevel(string levelId);
T GetConfigValue<T>(string key, T defaultValue);
void UpdateRemoteConfig(Dictionary<string, object> newValues);
```

**Events Published:** `OnRemoteConfigUpdated` (after cache refresh).

**Events Subscribed:** None.

---

### Audio

**Purpose:** All sound playback. A single `SoundManager` controls SFX and music. Individual trigger classes map game events to audio clips.

| Class | Responsibility |
|---|---|
| `SoundManager` | AudioSource pool management. Plays SFX (one-shot, overlapping) and music (crossfade). Volume/mute controls. |
| `GameplaySFXTrigger` | Subscribes to Core events (`OnFusionComplete`, `OnBrewComplete`, `OnCascadeStarted`) and plays corresponding clips. |
| `UISFXTrigger` | Subscribes to UI events (`OnPopupShown`, `OnScreenTransition`) and plays UI sound effects. |
| `MusicController` | Manages background music tracks. Crossfades between menu music and level music. |

**Dependencies:** `Utils` (object pool for AudioSources).

**Depends on Audio:** Nothing.

**Events Published:** None.

**Events Subscribed:** `OnFusionComplete`, `OnBrewComplete`, `OnCascadeStarted`, `OnCascadeComplete`, `OnLevelComplete`, `OnLevelFailed`, `OnPopupShown`, `OnScreenTransition`, `OnSettingsChanged`.

---

### VFX

**Purpose:** Particle effects, screen shake, token animations, and "juice" — all the visual feedback that makes the game feel alive.

| Class | Responsibility |
|---|---|
| `FusionVFX` | Particle burst when tokens merge into an orb. Color-matched to element type. |
| `BrewVFX` | Full-screen celebration when a potion brews. Cauldron bubbles, steam, glow. |
| `CascadeVFX` | Trail effects on falling tokens. Speed lines during rapid cascades. |
| `ChainVFX` | Escalating intensity effects for chain fusions (combo counter, screen shake). |
| `ScreenShake` | Camera shake controller. Intensity scales with fusion chain length. |
| `JuiceSystem` | Coordinates micro-animations: token squash/stretch, anticipation frames, overshoot on landing. |

**Dependencies:** `Utils` (object pool for particle systems).

**Depends on VFX:** Nothing.

**Events Published:** None.

**Events Subscribed:** `OnFusionStarted`, `OnFusionComplete`, `OnChainFusion`, `OnBrewStarted`, `OnBrewComplete`, `OnCascadeStarted`, `OnCascadeComplete`, `OnLevelComplete`.

---

### Utils

**Purpose:** Shared utilities with zero game-specific knowledge. These are pure tools.

| Class | Responsibility |
|---|---|
| `ObjectPool<T>` | Generic object pooling. Used for tokens, particles, audio sources. |
| `MathHelpers` | Grid math, hex/rect coordinate conversions, interpolation curves. |
| `Extensions` | C# extension methods for `Transform`, `Vector2Int`, `IEnumerable`, etc. |
| `CoroutineRunner` | MonoBehaviour-free coroutine execution. Singleton lifecycle helper. |
| `SafeAreaHelper` | Notch/cutout detection and UI safe area adjustment. |
| `DeviceInfo` | Platform detection, memory tier, GPU tier — used for quality settings. |

**Dependencies:** None.

**Depends on Utils:** Every module.

---

## Initialization Order

Boot scene (`Boot.unity`) initializes modules in this sequence:

```
1. Utils          — ObjectPool warm-up, device detection
2. Data           — Load embedded default_config.json, level catalog
3. Services       — Firebase init, auth (anonymous sign-in), fetch Remote Config
4. Data           — Update RemoteConfigCache with server values
5. Economy        — Initialize currency balances from last sync / default
6. Audio          — AudioSource pool, load clip references
7. VFX            — Particle pool warm-up
8. Core           — (No init needed — created per-level)
9. GameFlow       — (No init needed — created per-level)
10. Meta          — Load workshop state, streak state, daily brew check
11. UI            — Transition to Meta scene
```

---

## Dependency Injection

The project uses a lightweight DI container (Zenject/VContainer or custom) to bind interfaces to implementations at startup.

```csharp
Container.Bind<IFirebaseService>().To<FirebaseService>().AsSingle();
Container.Bind<IAdsService>().To<AdsManager>().AsSingle();
Container.Bind<IIAPService>().To<IAPHandler>().AsSingle();
Container.Bind<IAnalyticsService>().To<AnalyticsService>().AsSingle();
Container.Bind<ICrashlyticsService>().To<CrashlyticsService>().AsSingle();
```

For unit tests, mock implementations are injected:

```csharp
Container.Bind<IFirebaseService>().To<MockFirebaseService>().AsSingle();
```

This allows Core and GameFlow to be tested without any Firebase dependency.

---

## Scene Architecture

| Scene | Contents | Loaded When |
|---|---|---|
| `Boot` | ServiceLocator setup, Firebase init, auth, initial sync | App launch (never unloaded) |
| `Meta` | Main menu, workshop, potion shelf, world map, settings | After boot; returns here after levels |
| `Gameplay` | Board, HUD, boosters, level logic | Additive load on level start; unloaded on level end |

`Boot` is a persistent scene (loaded once, never destroyed). `Meta` and `Gameplay` swap via additive loading with crossfade transitions.
