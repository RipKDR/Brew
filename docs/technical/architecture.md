# Brew — Technical Architecture

> **Version:** 0.1.0-draft · **Last updated:** 2026-04-09
> **Status:** Pre-production · MVP scope (8–10 week build)

---

## 1. Tech Stack

| Layer | Technology | Version | Rationale |
|---|---|---|---|
| **Engine** | Unity | 6000.1 LTS (Unity 6.1) | Dominant mobile puzzle engine. Royal Match, Toon Blast, Candy Crush, Homescapes all ship on Unity. Cross-platform iOS/Android from one codebase. C# is productive; talent pool is large. Unity 6 LTS gives Burst, DOTS-optional, and improved mobile GPU perf. |
| **Language** | C# | 11 (via Unity 6) | First-class Unity support. async/await, pattern matching, spans. |
| **Backend** | Firebase | latest BOM | Single platform covers Auth, Firestore, Cloud Functions, Remote Config, Analytics, Crashlytics, Cloud Messaging. Fastest path for a ≤8-person team. Generous free tier covers MVP traffic. |
| **Auth** | Firebase Authentication | — | Anonymous auth on first launch (zero-friction onboarding), upgrade to email/Google/Apple sign-in later. Handles device transfer. |
| **Database** | Cloud Firestore | — | Real-time sync, offline persistence built in, document model maps cleanly to player profiles. Firestore Security Rules enforce per-player access. |
| **Functions** | Cloud Functions for Firebase | 2nd gen (Node 20) | Server-side IAP receipt verification, leaderboard writes, anti-cheat checks, event scheduling. 2nd gen gives Cloud Run concurrency + longer timeouts. |
| **Remote Config** | Firebase Remote Config | — | Economy tuning, feature flags, event definitions, ad frequency caps — all changeable without app update. Conditions by platform, country, user segment. |
| **Analytics** | Firebase Analytics (GA4) | — | Free, unlimited events, automatic screen tracking, audiences, funnels. Custom events for economy, retention, and ad engagement. Optional Amplitude or Mixpanel added post-MVP for cohort/funnel depth if needed. |
| **Crash Reporting** | Firebase Crashlytics | — | Native + managed crash reporting. Integrates with Unity via Firebase Unity SDK. Breadcrumb logs for game state at crash time. |
| **Ads** | Google AdMob + mediation | — | Rewarded video + interstitial. Mediation via AppLovin MAX (or IronSource) to waterfall/bid across networks: Meta Audience Network, Unity Ads, Vungle, Pangle. AdMob is primary; MAX maximizes eCPM. |
| **IAP** | Unity IAP | 4.x | Unified API for App Store + Google Play. Handles receipt generation; server-side verification via Cloud Functions. |
| **Push** | Firebase Cloud Messaging | — | Re-engagement notifications: daily brew reminder, event start/end, streak-at-risk. Topic-based + scheduled via Cloud Functions. |
| **CI/CD** | GitHub Actions + Fastlane | — | GitHub Actions runs Unity build (via GameCI Docker images). Fastlane handles code signing, TestFlight upload, Google Play Internal Testing upload. Unity Cloud Build is fallback if GameCI gives trouble. |
| **Source Control** | Git + GitHub | — | Monorepo. Git LFS for textures, audio, prefabs. |
| **Art Pipeline** | Spine / Lottie + TextMeshPro | — | Spine for character/potion animations (small file size, runtime blending). Lottie for UI micro-animations. TextMeshPro for all text rendering. |

---

## 2. Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                         │
│  UI Toolkit (uGUI) · Animations · Particles · Camera · Audio   │
├─────────────────────────────────────────────────────────────────┤
│                       GAME LOGIC LAYER                          │
│  Board State Machine · Fusion Engine · Cascade Resolver ·       │
│  Move Tracker · Score Calculator · Win/Lose Evaluator           │
├─────────────────────────────────────────────────────────────────┤
│                         META LAYER                              │
│  Potion Shelf · Workshop · Streaks · Daily Brew · Events ·     │
│  Booster Inventory · Economy (Essence/Gems)                     │
├─────────────────────────────────────────────────────────────────┤
│                         DATA LAYER                              │
│  Local Save Manager · Cloud Sync · Remote Config Cache ·        │
│  Level Catalog · Asset Bundles                                  │
├─────────────────────────────────────────────────────────────────┤
│                       SERVICES LAYER                            │
│  Ads · IAP · Analytics · Push Notifications · Auth ·            │
│  Crash Reporting · Deep Links                                   │
└─────────────────────────────────────────────────────────────────┘
```

### 2.1 Presentation Layer

Responsible for everything the player sees and hears.

| Component | Implementation |
|---|---|
| **Board View** | Grid of `TokenView` MonoBehaviours. Object-pooled; tokens are recycled, not instantiated/destroyed per move. SpriteRenderer for tokens, shader-based glow for fusion highlight. |
| **Animations** | DOTween for all tweens (fall, merge, shake, score pop). Spine runtime for potion-brew celebration and workshop characters. |
| **Particles** | VFX Graph or legacy Particle System for fusion sparkles, cascade trails, brew eruption. Budget: ≤3 active emitters at peak. |
| **Camera** | Orthographic, fixed. Subtle screen-shake on cascade chains (configurable intensity). |
| **UI** | uGUI canvases: HUD (moves, score, objectives), Pause overlay, Win/Lose dialogs, Map screen, Workshop, Potion Shelf, Settings. All UI driven by a `UIManager` with a screen-stack (push/pop). |
| **Audio** | AudioManager singleton with pooled AudioSources. Separate mixer groups for SFX / Music / UI. Music cross-fades between map and gameplay. |

### 2.2 Game Logic Layer

Pure C# where possible — no MonoBehaviour dependency. This makes the logic unit-testable outside Unity.

| Component | Responsibility |
|---|---|
| **BoardModel** | 2D array of `Cell` structs. Each cell holds a `Token` (color, size/tier) or a `Blocker`. Immutable per-step; new board state is computed, then applied. |
| **FusionEngine** | On tap: flood-fill to find connected cluster of same-color tokens. If cluster ≥ 3, fuse into orb. Orb tier grows with cluster size. Tier 4+ triggers "brew" (potion created). |
| **CascadeResolver** | After fusion: gravity pulls tokens down. Empty cells filled from top (random draw from level's ingredient pool). Repeat until no auto-matches exist. |
| **MoveTracker** | Decrements remaining moves on each player tap. Evaluates booster usage. |
| **ScoreCalculator** | Points per fusion (base × tier multiplier × cascade-chain bonus). Star thresholds from level definition. |
| **WinLoseEvaluator** | Win: all recipe targets met. Lose: moves exhausted before win. Returns `LevelResult`. |

**Board State Machine:**

```
         ┌──────────┐
         │   Idle   │◄────────────────────────────┐
         └────┬─────┘                              │
              │ player taps cluster                │
         ┌────▼──────────┐                         │
         │ Player Input  │                         │
         └────┬──────────┘                         │
              │ valid cluster found                │
         ┌────▼─────┐                              │
         │  Fusing   │  (merge animation)          │
         └────┬─────┘                              │
              │ tokens removed / orb placed        │
         ┌────▼──────────┐                         │
         │  Cascading    │  (gravity + refill)     │
         └────┬──────────┘                         │
              │ board settled                      │
         ┌────▼──────────┐                         │
         │   Settling    │  (animations finish)    │
         └────┬──────────┘                         │
              │                                    │
         ┌────▼──────────┐     no                  │
         │  Check Brew   │──────────┐              │
         └────┬──────────┘          │              │
              │ brew triggered      │              │
         ┌────▼──────────┐    ┌─────▼──────────┐   │
         │ Brew Anim     │    │ Check Win/Lose │───┘
         └────┬──────────┘    └────┬───────────┘
              │                    │
              └────────────────────┘
                      │
               ┌──────▼──────┐
               │  Result     │ → LevelResult written to save
               └─────────────┘
```

### 2.3 Meta Layer

| System | Description |
|---|---|
| **Potion Shelf** | Collection screen. Each potion has a recipe (e.g., "3× Red Orb tier-3 + 2× Blue Orb tier-2"). Brewed potions populate the shelf with unlock animation. Progress persisted per-potion. |
| **Workshop** | Sequential restoration: each upgrade costs Essence, unlocks cosmetic area + gameplay reward (booster slot, new ingredient, etc.). Linear progression; index-based. |
| **Streaks** | Consecutive-day login streak. Rewards escalate on days 1-7, then cycle. `streak_count` resets if >36 hours since last completion. |
| **Daily Brew** | One special level per day. Refreshes at 00:00 UTC. Rewards Essence + Gems. Tracked by date string. |
| **Events** | Time-limited event with themed levels and exclusive potion rewards. Defined entirely in Remote Config — no code change to add an event. |
| **Economy** | Two currencies: Essence (soft, earned from gameplay) and Gems (premium, earned sparingly or purchased). All earn/spend rates live in Remote Config. |
| **Boosters** | Pre-game and in-game boosters (e.g., "Extra 5 Moves", "Color Bomb", "Row Clear"). Inventory stored locally, costs defined in Remote Config. |

### 2.4 Data Layer

| Component | Description |
|---|---|
| **LocalSaveManager** | Writes player state to `Application.persistentDataPath` as encrypted JSON after every move and on every meta-state change. Uses AES-256 with a device-derived key (not secure against determined attackers — server is authority for currencies). |
| **CloudSyncManager** | On app foreground, connectivity change, and level complete: push local state to Firestore `players/{uid}`. Pull on login. Conflict resolution: server wins for currencies; client wins for level progress (highest stars kept). |
| **RemoteConfigCache** | Fetches Remote Config on app start with 1-hour cache TTL. Falls back to last-fetched values or baked-in defaults. Config includes economy values, event definitions, feature flags, ad caps. |
| **LevelCatalog** | Levels stored as `ScriptableObject` assets in Unity (baked into build) and optionally as JSON downloaded from Cloud Storage for post-ship content. Level ID is the lookup key. |
| **Asset Bundles** | Event-specific art (backgrounds, token skins) delivered via Unity Addressables from Cloud Storage. Downloaded on-demand when event starts; cached locally. |

### 2.5 Services Layer

| Service | Integration |
|---|---|
| **Ads** | `AdService` wraps AdMob SDK. Exposes `ShowRewarded(placement, callback)` and `ShowInterstitial(placement)`. Preloads next ad on completion. Frequency capping enforced by Remote Config (`interstitial_cooldown_seconds`, `max_interstitials_per_session`). |
| **IAP** | `IAPService` wraps Unity IAP. Products defined in Remote Config + store dashboards. On purchase: receipt sent to Cloud Function for server-side verification; on success, currencies credited both locally and in Firestore. |
| **Analytics** | `AnalyticsService` façade. Calls Firebase Analytics under the hood. Every public method is a typed event (`LogLevelStart`, `LogLevelComplete`, `LogAdWatched`, `LogPurchase`, etc.). See analytics event catalog in `infrastructure.md`. |
| **Push** | Firebase Cloud Messaging. Token registered on first launch. Topics: `all_users`, `platform_ios`, `platform_android`, `event_{id}`. Scheduled sends via Cloud Functions (daily brew reminder 18:00 local, streak-at-risk 20:00 local). |
| **Auth** | Anonymous on first launch. Optional link to Google/Apple sign-in for cross-device. `AuthService` exposes `CurrentUser`, `LinkAccount()`, `SignOut()`. |
| **Crash Reporting** | Crashlytics auto-initialized. Custom keys set on each level start: `level_id`, `moves_remaining`, `board_state_hash`. Non-fatal exceptions logged for ad load failures and sync failures. |

---

## 3. Key Architectural Decisions

### 3.1 Level Data as ScriptableObjects + JSON

Levels are authored in a Unity Editor tool (grid painter) and serialized as `ScriptableObject` assets. Each level is also exportable as JSON for:

- **Remote level packs:** new levels pushed via Cloud Storage + Remote Config pointer, downloaded by Addressables. No app update needed.
- **Event levels:** event config includes level JSON inline or references a Cloud Storage URL.
- **QA tooling:** JSON levels can be loaded in a headless test runner for automated solve verification.

Schema: see `data-model.md § Level Definition`.

### 3.2 Remote Config for Economy Tuning

Every tunable value lives in Remote Config, never hard-coded:

- Essence earned per star (1★ / 2★ / 3★)
- Gem earn rates (daily brew, streak milestones)
- Booster costs (Essence and Gem prices)
- Workshop upgrade costs (per index)
- IAP product values (Gems per dollar tier)
- Ad frequency caps
- Event reward tables

This allows the game economy to be rebalanced by a product manager in the Firebase console without engineering involvement or app update.

### 3.3 Offline-First Design

1. **Every move** writes board state + meta state to local save (synchronous, <2ms on target devices).
2. On **level complete**, result is appended to a local `pending_sync` queue.
3. When **connectivity is detected**, `CloudSyncManager` flushes the queue to Firestore in a batched write.
4. If the player opens the game on a **second device**, the server profile is pulled and merged:
   - **Currencies:** server value wins (prevents duplication exploits).
   - **Level progress:** max(local, server) per level (player keeps best stars).
   - **Potion collection:** union of local and server sets.
   - **Workshop:** max(local_index, server_index).
5. **Remote Config** is fetched on foreground; if fetch fails, cached values are used. If no cache exists, baked-in defaults apply.

### 3.4 Event System via Remote Config

An event is a JSON blob in Remote Config:

```json
{
  "event_id": "halloween_2026",
  "theme_name": "Spooky Brews",
  "start_date": "2026-10-25T00:00:00Z",
  "end_date": "2026-11-01T23:59:59Z",
  "levels": ["evt_hw_01", "evt_hw_02", "evt_hw_03"],
  "rewards": [
    { "threshold": 3, "type": "essence", "amount": 500 },
    { "threshold": 6, "type": "potion", "potion_id": "spooky_elixir" },
    { "threshold": 9, "type": "gems", "amount": 50 }
  ],
  "display_config": {
    "banner_asset": "events/halloween_2026/banner",
    "color_primary": "#FF6B00",
    "color_secondary": "#1A0A2E"
  }
}
```

The client reads this on foreground, shows/hides the event UI accordingly, downloads event assets via Addressables, and tracks event progress locally. No code change required to launch a new event.

### 3.5 Dependency Injection / Service Locator

All services (`AdService`, `IAPService`, `AnalyticsService`, `AuthService`, `CloudSyncManager`, `RemoteConfigCache`) are registered in a lightweight service locator at app startup. Game logic depends on interfaces, not concrete implementations. This allows:

- Swapping real ads for a `MockAdService` in editor/testing.
- Running game logic unit tests with a `MockAnalyticsService`.
- Toggling Firebase between dev/staging/prod via build config.

### 3.6 Board Logic Separation

`BoardModel`, `FusionEngine`, `CascadeResolver`, `ScoreCalculator`, and `WinLoseEvaluator` are pure C# classes with zero Unity dependencies. They operate on data structs (`Cell`, `Token`, `Blocker`, `FusionResult`, `CascadeStep`). The `BoardPresenter` (MonoBehaviour) reads the computed steps and drives animations.

Benefits:
- **Unit testable:** NUnit tests run in CI without Unity editor.
- **Deterministic replay:** given the same RNG seed + move sequence, the board resolves identically. Useful for QA, anti-cheat, and replay sharing.
- **Server validation (future):** the same logic can run in a Cloud Function to verify submitted scores.

---

## 4. Performance Targets

| Metric | Target | How |
|---|---|---|
| **Frame rate** | 60 fps sustained on iPhone SE 2nd gen (A13) and Samsung Galaxy A14 (Helio G80, 2GB RAM) | Object pooling, shader simplicity (no PBR), sprite atlasing, aggressive particle budgets, Burst-compiled cascade solver if needed. |
| **Install size** | < 100 MB (initial download) | Texture compression (ASTC on both platforms), audio as Ogg Vorbis @ 96kbps, Addressables for event assets (downloaded on-demand). |
| **Cold start** | < 5 seconds to interactive menu | Minimal scene load: splash → main menu. Firebase SDK init on background thread. Remote Config fetch is non-blocking (use cache). Deferred loading for workshop/shelf scenes. |
| **Memory** | < 350 MB peak (gameplay) | Sprite atlasing (one atlas per screen context), texture streaming for workshop/shelf, pool all particles and tokens. |
| **Battery** | < 8% per 30-minute session | Target 60fps but drop to 30fps when on low-battery mode. Avoid continuous GPU overdraw; limit shader complexity. |
| **Network** | Playable on 2G (50 kbps) | All gameplay offline. Sync payloads < 5 KB. Remote Config fetch < 2 KB. Ad preload is opportunistic. |

---

## 5. Project Structure (Unity)

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/               # BoardModel, FusionEngine, CascadeResolver, etc.
│   │   ├── Meta/               # PotionShelf, Workshop, Streaks, Economy
│   │   ├── Data/               # SaveManager, CloudSync, RemoteConfig, LevelCatalog
│   │   ├── Services/           # AdService, IAPService, AnalyticsService, AuthService
│   │   ├── Presentation/       # BoardPresenter, TokenView, UIManager, screens
│   │   └── Utilities/          # Extensions, ObjectPool, ServiceLocator
│   ├── Prefabs/
│   │   ├── Tokens/
│   │   ├── Blockers/
│   │   ├── UI/
│   │   └── VFX/
│   ├── ScriptableObjects/
│   │   ├── Levels/
│   │   ├── Potions/
│   │   └── Config/
│   ├── Art/
│   │   ├── Sprites/
│   │   ├── Spine/
│   │   └── UI/
│   ├── Audio/
│   │   ├── Music/
│   │   └── SFX/
│   └── Scenes/
│       ├── Boot.unity
│       ├── MainMenu.unity
│       ├── Gameplay.unity
│       └── Workshop.unity
├── Plugins/                    # Firebase SDK, AdMob, Unity IAP, DOTween
├── AddressableAssetsData/
└── Tests/
    ├── EditMode/               # Pure C# unit tests for Core/
    └── PlayMode/               # Integration tests for presentation
```

---

## 6. Build Matrix

| Platform | Min OS | Target Device Floor | Scripting Backend | Graphics API |
|---|---|---|---|---|
| **iOS** | iOS 16.0 | iPhone SE 2nd gen (A13, 3GB) | IL2CPP | Metal |
| **Android** | API 26 (Android 8.0) | Samsung Galaxy A14 (Helio G80, 2GB) | IL2CPP | Vulkan (primary), OpenGL ES 3.0 (fallback) |

---

## 7. Third-Party Dependencies

| Package | Purpose | License |
|---|---|---|
| Firebase Unity SDK | Auth, Firestore, Functions, Remote Config, Analytics, Crashlytics, Messaging | Apache 2.0 |
| Google Mobile Ads Unity Plugin | AdMob + mediation | Apache 2.0 |
| AppLovin MAX SDK | Ad mediation | Commercial (free) |
| Unity IAP | In-app purchases | Unity license |
| DOTween Pro | Tweening library | Commercial ($15) |
| Spine Unity Runtime | Skeletal animation | Spine license (per seat) |
| TextMeshPro | Text rendering | Included in Unity |
| Newtonsoft.Json (Unity package) | JSON serialization | MIT |
| NaughtyAttributes | Editor QoL | MIT |

---

## 8. Risk Register

| Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|
| Firebase SDK bloat increases install size past 100MB | Medium | Medium | Measure after integration. Strip unused Firebase modules. Use deferred init. |
| AdMob mediation integration delays | Medium | Medium | Start with AdMob-only; add mediation adapters in week 6+. |
| Offline/online sync edge cases cause currency duplication | High | Medium | Server is currency authority. Client cannot increase currencies without server ack (except offline earn, which is reconciled on sync with server-side validation). |
| Cascade/fusion performance on low-end Android | High | Low | Board logic is pure C# — Burst-compile if needed. Profile on Galaxy A14 weekly from week 3. |
| Unity 6 LTS bugs on specific devices | Medium | Low | Lock to a specific patch version. Test on BrowserStack/Firebase Test Lab device farm. |
