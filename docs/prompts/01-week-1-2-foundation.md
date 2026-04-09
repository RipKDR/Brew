# Week 1-2: Foundation Build Prompt

You are building **Brew**, a mobile puzzle game for iOS and Android. The project lives at `T:\Brew`. This is the FOUNDATION phase — the first code being written. Documentation, specs, level JSONs (40 files in `levels/`), and four starter C# scripts exist, but no working gameplay yet.

Before doing anything, read these files in order:

1. `AGENTS.md` — project rules, architecture, code conventions, common mistakes
2. `docs/game-design/GDD.md` — full game design document
3. `docs/game-design/core-mechanic.md` — the technical bible for fusion/chain/brew/cascade rules
4. `docs/sdk/unity-architecture.md` — module structure and dependency rules
5. `docs/sdk/state-machine-spec.md` — board state machine (every state, transition, and timing)
6. `docs/production/mvp-build-plan.md` — the full 10-week build plan with task breakdown
7. `docs/project-management/session-handoff.md` — current status and what's next

Also read all files in `.cursor/rules/` for coding standards.

**Tech stack:** Unity 6 (6000.1 LTS), C# 11, Firebase backend (later phases). Target: iOS 15+ and Android 8.0+ (API 26), portrait only.

---

## Existing Code

Four starter scripts already exist. Read all of them before writing new code:

- `Assets/Scripts/Data/BoardConfigSO.cs` — ScriptableObject for board dimensions (width, height, cell size, spacing). Clamps width/height to 5-11. Namespace: `Brew.Data`.
- `Assets/Scripts/Core/BoardManager.cs` — MonoBehaviour that generates an NxM grid of `CellData` structs. Has `GenerateBoard()`, `TryGetCell()`, `TrySetIngredient()`. Defines `IngredientType` enum (Ember, Frost, Vine, Sun, Shadow) and `CellData` struct. Currently fills all cells with Ember only — needs TokenSpawner integration. Namespace: `Brew.Core`.
- `Assets/Scripts/Core/TokenSpawner.cs` — MonoBehaviour that populates the board with random ingredients while preventing horizontal/vertical 3-in-a-row at spawn. Has `PopulateInitialBoard()`. Namespace: `Brew.Core`.
- `Assets/Scripts/Input/InputHandler.cs` — MonoBehaviour with flood-fill `FindCluster()` using BFS and `OnValidClusterTapped` event. Detects tap via `Input.GetMouseButtonDown(0)`, converts screen-to-world, fires event if cluster size >= `_minClusterSize` (3). Namespace: `Brew.Input`.

---

## Week 1 Deliverables

Complete these tasks in order. Reference the specific doc sections noted.

### 1.1 — Unity Project Setup

The project root is `T:\Brew`. The `Assets/`, `Packages/`, and `ProjectSettings/` directories should already exist as scaffolding. If they don't, create a Unity 6 (6000.1 LTS) project here. Organize the folder structure per `AGENTS.md` §Project Structure:

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/           # BoardModel, FusionEngine, CascadeResolver
│   │   ├── Meta/           # (empty for now)
│   │   ├── Data/           # BoardConfigSO, LevelCatalog, JsonLoader
│   │   ├── Services/       # (empty for now)
│   │   ├── Presentation/   # BoardPresenter, TokenView
│   │   └── Utilities/      # ObjectPool, Extensions, ServiceLocator
│   ├── Prefabs/Tokens/     # Token prefab variants
│   ├── ScriptableObjects/
│   │   ├── Levels/
│   │   └── Config/         # BoardConfig asset
│   ├── Art/Sprites/
│   └── Scenes/             # Boot, Gameplay
├── Tests/
│   ├── EditMode/
│   └── PlayMode/
```

NOTE: The existing starter scripts are at `Assets/Scripts/` (not under `_Project/`). You may either move them into the `_Project/` structure or keep them in place and build the new architecture alongside — but be consistent. If you restructure, update all namespaces accordingly. The architecture doc (`docs/sdk/unity-architecture.md`) uses `Assets/Scripts/` paths, while `AGENTS.md` uses `Assets/_Project/Scripts/`. Pick one structure and stick with it. Document your choice.

Configure `.gitignore` — one already exists at the project root.

### 1.2 — Build Pipeline

Configure iOS Xcode export and Android Gradle export. Verify clean builds on both platforms from a scene containing only a camera. This validates the project settings are correct before any gameplay code is added.

### 1.3 — CI Pipeline

A GitHub Actions workflow already exists at `.github/workflows/ci.yml`. It currently validates level JSONs, economy pacing, docs, and context. Extend it (or create a separate workflow) to include Unity build validation when gameplay code exists. Use GameCI Docker images for Unity builds. For now, ensure the existing CI jobs pass.

### 1.4 — BoardManager Enhancement

The existing `BoardManager.cs` generates a grid but fills every cell with Ember. Enhance it:

- Integrate with `TokenSpawner` so `GenerateBoard()` calls `PopulateInitialBoard()` (or refactor so the spawner is called externally after generation).
- Add cell content types beyond just ingredient tokens — the grid needs to support `TOKEN`, `ORB`, and `EMPTY` states per `docs/game-design/core-mechanic.md` §1.3.
- Make the grid configurable via `BoardConfigSO` (already wired via `[SerializeField]`).
- The board origin `(0,0)` is top-left. Column increases left-to-right, row increases top-to-bottom. Gravity pulls toward higher row indices. See `core-mechanic.md` §1.1.

### 1.5 — TokenSpawner Enhancement

The existing `TokenSpawner.cs` prevents horizontal/vertical 3-in-a-row but doesn't use the full flood-fill cluster check from the spec. Enhance it:

- After initial population, run a full cluster detection pass (BFS flood fill per `core-mechanic.md` §3.2). If any cluster of 3+ exists, recolor random cells in that cluster until no clusters remain.
- Also enforce: no cluster larger than 5 at generation (per `core-mechanic.md` §10.4 — prevents accidental free brews).
- Ensure at least 3 valid clusters (size >= 3) exist on the generated board (per `core-mechanic.md` §10.2).
- Support weighted spawn probabilities from level config (`spawn_weights` — see level JSON files in `levels/` for the `ingredient_pool` field).

### 1.6 — InputHandler Enhancement

The existing `InputHandler.cs` has working flood-fill cluster detection. Enhance it:

- Only accept input when the board state machine is in `WaitingForInput` state (stub this check for now if the state machine isn't built yet).
- On valid tap: fire `OnValidClusterTapped` with the cluster cells AND the tap position (the tap position determines where the orb spawns — this is the core "positional strategy" per `core-mechanic.md` §4.3).
- On invalid tap (cluster < 3, or tapping an orb/empty cell): fire a rejection event for UI shake feedback.
- Add input debounce: after a tap is accepted, disable input until the state machine returns to `WaitingForInput`. See `state-machine-spec.md` §WaitingForInput.

### 1.7 — Placeholder Art

Create 5 colored circle sprites for the 5 ingredient types. Colors from `core-mechanic.md` §2.1:

- Ember: `#E04040` (red)
- Frost: `#4080E0` (blue)
- Vine: `#40B040` (green)
- Sun: `#E0C020` (yellow)
- Shadow: `#8040C0` (purple)

64x64 px PNGs. Place in `Assets/_Project/Art/Sprites/Tokens/` (or equivalent). Create a prefab for each token type in `Assets/_Project/Prefabs/Tokens/`.

### 1.8 — Level JSON Loading

40 level JSON files already exist in `levels/` (level_001.json through level_040.json). The schema is documented in `docs/technical/data-model.md` §3. Each level defines:

- `level_id`, `grid_width`, `grid_height`, `grid_mask`, `ingredient_pool`, `recipe_targets` (array of `{ingredient, count}`), `move_limit`, `blocker_placements`, `star_thresholds` (array of 3 ints), `is_tutorial`, `tutorial_steps`

Build a `LevelLoader` or `LevelCatalog` class that:

- Parses level JSON files into a C# data structure (plain C# class, not MonoBehaviour).
- Provides `GetLevel(int levelId)` to retrieve a parsed level config.
- Validates that loaded values are within expected ranges (grid 5-9 wide, 5-11 tall, move_limit > 0, etc.).

---

## Week 2 Deliverables

### 2.1 — Basic Fusion

When the player taps a valid cluster (>= 3 same-type tokens): remove all tokens in the cluster, spawn a Tier-1 orb at the tapped cell position. The orb has `color = cluster color` and `token_count = cluster.size`. Reference `core-mechanic.md` §4.2 for the exact execution steps.

The move counter decrements by 1 at the START of the fusion animation (immediately on valid tap). See `core-mechanic.md` §9.1-9.3.

### 2.2 — Gravity System

After fusion removes tokens, gravity fills gaps. Process columns independently, left-to-right:

- Non-empty cells (tokens AND orbs) fall to fill gaps below them.
- After gravity, refill empty cells at the top of each column with new random tokens.
- Reference `core-mechanic.md` §8.1-8.4 for the exact algorithm.

### 2.3 — Fusion Animation

Tokens in the fusing cluster tween toward the tap point over 0.15s with ease-in. The orb scales up from 0 with 0.2s elastic ease. Use DOTween (preferred) or LeanTween. Add DOTween to the project via Unity Package Manager or NuGet.

Reference timing: `core-mechanic.md` §7.5 gives fusion animation at 200ms.

### 2.4 — Gravity Animation

Tokens drop with acceleration. Timing: 50ms per row traveled + 30ms bounce at landing (per `core-mechanic.md` §8.5). All drops in all columns begin simultaneously. The column with the longest drop determines when the grid is settled.

### 2.5 — Board State Machine

Implement `BoardStateMachine` per `docs/sdk/state-machine-spec.md`. States:

```
Idle → WaitingForInput → ProcessingFusion → Cascading → Settling → CheckingBrew → CheckingWinLose → Idle
```

Key rules:

- Only `WaitingForInput` accepts player input.
- `ProcessingFusion` plays the fusion animation (0.3s per the state machine spec, though core-mechanic says 200ms — use the state machine spec's 0.3s as it includes visual padding).
- `Cascading` runs gravity + refill.
- `Settling` scans for auto-chain clusters. If found → back to `ProcessingFusion`. If none → `CheckingBrew`.
- `CheckingBrew` checks if any orb has `token_count >= 6` (brew threshold from `core-mechanic.md` §6.1). If so, brew it (placeholder VFX for now). If brewed orb leaves a gap → back to `Cascading`.
- `CheckingWinLose` checks: recipe complete → win; moves == 0 → fail; otherwise → `Idle` → `WaitingForInput`.
- `Paused` is a stack-push state (remembers previous state, returns to it on resume).

Implement transition validation: only allow legal transitions. Log errors for invalid transitions in development builds.

### 2.6 — Chain Fusion Detection

After settling (gravity complete, board stable), check if any orb is adjacent to another same-color orb. If so, chain-fuse them per `core-mechanic.md` §5:

- Find all connected same-color orbs via BFS.
- Survivor = bottom-most orb; break ties by left-most.
- Survivor's `token_count` = sum of all merged orbs' `token_count`.
- Non-survivor orbs animate toward survivor (300ms) then are removed.
- After chain fusion, if survivor's `token_count >= 6`, trigger brew.

Also check: after gravity settles, do any new clusters of 3+ same-color tokens exist (cascade auto-fuse)? If so, auto-fuse them (orb placement = bottom-most, then left-most cell in the cluster — per `core-mechanic.md` §7.3). Then re-run gravity. Loop until board is stable.

### 2.7 — Basic Brew Detection

When any orb reaches `token_count >= 6`: trigger brew. For now, use placeholder VFX (particle burst at the orb cell). Remove the orb, set the cell to empty, award score (200 for target recipe color, 100 for non-target — per `core-mechanic.md` §6.2). After brew, gravity fills the gap.

### 2.8 — Move Counter

Implement the move counter per `core-mechanic.md` §9:

- Decrement by 1 on each player-initiated fusion (NOT on chain fusions, cascades, brews, or reshuffles).
- Display as integer in a simple HUD text element.
- When counter reaches 0 and no resolution is in progress, the level fails.
- If counter reaches 0 during a resolution, let the resolution complete fully before showing fail.

---

## Milestone Gate 1 Checklist (End of Week 2)

Before moving to Week 3-4, ALL of these must pass. Reference: `docs/production/milestone-gates.md` §Gate 1.

- Board generates with configurable dimensions via BoardConfigSO (test: 6x6, 7x8, 8x8)
- 5 ingredient types spawn with no pre-existing clusters >= 3
- Tapping a cluster of >= 3 same-type tokens removes them and spawns an orb at tap position
- Tapping a cluster of < 3 tokens or an orb/empty cell does nothing (no crash)
- Gravity fills gaps — tokens above fall down, new tokens spawn at top
- Chain fusions trigger automatically when same-color orbs become adjacent
- Cascade auto-fusions trigger when gravity creates new 3+ clusters
- Brew triggers when an orb reaches token_count >= 6
- Board state machine transitions correctly through all states
- Move counter decrements on player taps only
- No crashes during 10 consecutive level plays
- Frame rate >= 30 fps during fusion animations on lowest-spec target device

---

## What NOT To Do

- Do NOT add any meta systems (workshop, potion shelf, streaks, daily brew)
- Do NOT add any monetization (IAP, ads)
- Do NOT add analytics events yet
- Do NOT add sound or music yet
- Do NOT add haptic feedback yet
- Do NOT deviate from the board state machine spec — implement it exactly as documented
- Do NOT hardcode grid sizes, token counts, brew thresholds, or any tunable values — use ScriptableObjects
- Do NOT use MonoBehaviours for game logic — board model, fusion engine, cascade resolver, and scoring are pure C# classes (MonoBehaviours only for Unity lifecycle hooks and presentation)
- Do NOT use `Instantiate`/`Destroy` in the gameplay loop — set up object pooling for tokens and orbs from the start

---

## Key References Quick-Access


| Topic                                                            | File                                          |
| ---------------------------------------------------------------- | --------------------------------------------- |
| Grid, tokens, clusters, fusion, chains, brews, cascades, scoring | `docs/game-design/core-mechanic.md`           |
| Board state machine (every state and transition)                 | `docs/sdk/state-machine-spec.md`              |
| Module structure and dependency rules                            | `docs/sdk/unity-architecture.md`              |
| Level JSON schema                                                | `docs/technical/data-model.md` §3             |
| Build plan task breakdown                                        | `docs/production/mvp-build-plan.md` §Week 1-2 |
| Milestone gate criteria                                          | `docs/production/milestone-gates.md` §Gate 1  |
| Architecture layers and folder structure                         | `AGENTS.md` §Project Structure                |
| Code conventions and naming                                      | `AGENTS.md` §Code Conventions                 |
| Common mistakes                                                  | `AGENTS.md` §Common Mistakes to Avoid         |


---

## At End of Session

Update `docs/project-management/session-handoff.md` with:

- What was completed this session
- What's still in progress
- Next 3 tasks for the next session
- Any blockers discovered
- Files created or modified

Run `python scripts/context/build_context_snapshot.py --project-root .` to regenerate the context snapshot.