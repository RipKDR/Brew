# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-04-09

---

## Current Milestone Focus

- Milestone: **Feel & Juice (Weeks 5-6)** — working toward Milestone Gate 3
- Goal: Art integration, particles, SFX, haptics, tutorial, boosters, polish — the game must FEEL good
- Source plan: `docs/production/mvp-build-plan.md`, `docs/prompts/03-week-5-6-feel-and-juice.md`

## Current Branch + Baseline

- Branch: `cursor/add-changelog-adr-session-handoff`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, and `lint` should pass

## Completed Since Last Handoff

### Foundation Phase (Sessions 1-2)

- Full pure C# Core layer: `BoardModel`, `ClusterDetector`, `TokenSpawner`, `FusionEngine`, `CascadeResolver`, `BoardStateMachine`, `FusionResult`, `ChainFusionResult`, `GravityStep`, enums, `GridCoord`, `CellContent`
- Presentation: `BoardPresenter`, `InputController`, `TokenView`, `ObjectPool<T>`
- Data: `BoardConfigSO`
- Editor: `BrewSceneSetup`
- Tests: 6 test files, ~60 test methods for Core layer

### Core Loop Phase (Session 3)

- New Core classes: `LevelConfig`, `MoveTracker`, `ScoreCalculator`, `RecipeTracker`, `WinLoseEvaluator`
- Data: `LevelLoader`, `PlayerProgress`, `LocalSaveManager`
- Presentation: `HudController`, `RecipeVialUI`, `LevelCompleteScreen`, `LevelFailScreen`, `LevelSelectScreen`, `LevelButton`, `GameFlowController`
- Tests: 7 additional test files, ~80+ methods
- Level data: 40 level JSONs in `Assets/_Project/Resources/Levels/`

### Gate 2 Closure + Feel & Juice Phase (This Session)

**Phase 1 — Scene Wiring (Gate 2 closure)**:

- Enhanced `BrewSceneSetup` editor script with full scene hierarchy creation
  - `Brew > Create Gameplay Scene` creates scene file at `Assets/_Project/Scenes/Gameplay.unity` and registers it in EditorBuildSettings
  - `Brew > Setup Gameplay Scene` creates: Camera, Canvas (1080×1920 CanvasScaler), HUD panel, LevelSelectScreen (ScrollView + GridLayoutGroup), LevelCompleteScreen, LevelFailScreen, BoosterBar, GameplayPanel, GameFlowController with all serialized refs wired, EventSystem
- Created UI prefabs programmatically: `RecipeVialUI.prefab`, `LevelButton.prefab` at `Assets/_Project/Prefabs/UI/`

**Phase 2 — Booster System**:

- `BoosterType` enum (Shake, Catalyst, ExtraMoves) — pure C#
- `BoosterManager` — pure C#, charge management, Fisher-Yates shuffle for Shake, Catalyst converts token→orb(count=1), ExtraMoves with one-use-per-attempt guard, events
- `BoosterBarUI` — 3-slot UI, arm/disarm logic, Shake/ExtraMoves fire immediately, Catalyst requires board tap
- `BoosterSlotUI` — button, charge text, armed highlight
- `BoosterManagerTests` — 15 tests covering charges, shake (color preservation, orb preservation, cluster guarantee), catalyst (token→orb, rejects non-token), extra moves (adds 3, one-use guard, reset)
- Integrated into `BoardPresenter`: `ArmCatalyst()`, `DisarmCatalyst()`, `ActivateShake()`, `ActivateExtraMoves()`

**Phase 3 — Audio System**:

- `SfxId` enum — 16 sound effect identifiers
- `AudioConfigSO` — ScriptableObject mapping SfxId to clip variants, priority, pitch variation; 6 music stem slots (4 normal + 2 tension)
- `AudioManager` — singleton, 8 SFX + 4 music AudioSources, round-robin clip variants, semitone-based pitch variation, priority-based voice stealing, `DuckMusic()` for brew moments
- `AdaptiveAudioController` — monitors MoveTracker, 3 states (Normal, LowMoves, Critical), cross-fades stems, swaps percussion to tense variant at ≤5 moves, drops to pad+heartbeat at 1 move, 4s recovery ramp

**Phase 4 — Haptic System**:

- `HapticManager` — static class, platform-abstracted (iOS DllImport bridge, Android AndroidJavaObject Vibrator), 5 feedback types (Light, Medium, Heavy, Success, Selection), settings-respecting, `ContinuousRumble()` for brew crescendo

**Phase 5 — Visual Effects & Animation**:

- `ScreenShakeConfigSO` — 4 presets (Fusion 0.5/0.1s, Chain 1.0/0.15s, Brew 2.0/0.3s, LevelComplete 1.5/0.5s)
- `ScreenShakeController` — Perlin noise camera shake, stacking (max envelope), chain length scaling (+0.2 per step), settings toggle, unscaled time
- `ParticleManager` — pooled ParticleSystem instances (cap 5), 6 effect types (FusionSparkles, CascadeTrail, BrewBubbles, BrewBurst, LevelCompleteConfetti, ChainIndicator), runtime template creation, OnParticleSystemStopped auto-return
- `TokenAnimator` — idle shimmer (scale 0.98-1.02, 3s sine), selected pulse (1.08→1.05 ease-out-back), fusing rush (ease-in, squash-and-stretch, white flash), AnimationCurve-based
- `BrewAnimationController` — 4-phase 1.8s brew sequence (Ignition → Eruption → Formation → Celebration), haptic integration, particle integration, screen shake at eruption, fly-to-vial arc with fade
- Updated `TokenView` — exposes `SpriteRenderer` and `TokenAnimator`, auto-starts idle shimmer on Initialize

**Phase 6 — Tutorial System**:

- `TutorialLevelData` — pure C#, defines all 5 tutorial levels with hand-tuned board layouts, step sequences, text prompts per tutorial-flow.md
  - `TutorialStep` data class with type, text, highlighted cells, finger indicator, auto-dismiss, celebration config
  - `TutorialBoardSetup` with per-cell IngredientColor layout
- `TutorialController` — MonoBehaviour driving tutorial flow, step-by-step coroutine sequence, input gating (validates taps against allowed cells), hint system stubs (5s/10s/15s)
- `TutorialOverlay` — full-screen dim panel, spotlight glow rings with pulse animation, animated finger indicator (tap every 1.5s), text bubble, celebration text display

**Phase 7 — UI Polish & Settings**:

- `SettingsManager` — static PlayerPrefs-backed settings (MusicVolume, SfxVolume, IsMuted, HapticsEnabled, ScreenShakeEnabled), change events
- `SettingsPanel` — MonoBehaviour with sliders and toggles, syncs with SettingsManager on enable/disable
- Updated `HudController` — animated move counter (pulse at ≤5 moves, larger pulse at 1 move), smoothstep score tween, critical move color
- Updated `BoardPresenter` — correct hex colors from art-direction.md (Ember #E05A3A, Frost #6FB8D9, Vine #6DAF5E, Sun #F2C745, Shadow #6B4E9B), integrated ScreenShake/ParticleManager/BrewAnimator/TutorialController, brew animation sequence replaces instant removal, haptic triggers at fusion/chain/brew/win events

## In Progress

- None. All Feel & Juice code is complete. Ready for Unity Editor testing and Gate 3 preparation.

## Next 3 Tasks

1. Open project in Unity Editor. Run `Brew > Create Gameplay Scene` to generate the scene. Run `Brew > Setup Gameplay Scene`. Verify all prefabs and serialized references are wired correctly.
2. Run all EditMode unit tests (now 14 test files, ~155+ methods). Fix any compilation or test failures.
3. Create/assign AudioConfigSO and ScreenShakeConfigSO assets. Import placeholder SFX clips. Playtest levels 1-5 (tutorial) through level 10 — verify tutorial flow, booster activation, audio playback, haptic feedback, particle effects, and screen shake.

## Blockers / Risks

- Risk: AudioClip assets not yet imported — AudioManager will silently skip playback until clips are assigned to AudioConfigSO.
- Risk: Token art still uses colored circles — need final sprites per art-direction.md token silhouettes (teardrop, hexagon, diamond, circle-with-rays, oval).
- Risk: BrewAnimationController requires a SpriteRenderer flash overlay — needs to be assigned in scene.
- Risk: TutorialOverlay needs UI prefabs for glow ring and finger indicator — currently creates runtime fallbacks.
- Risk: Music stems (72s loop, 4 separate tracks) not yet produced — game will run silent until audio assets are added.
- Blocker: Gate 3 requires external playtest with 5 non-team members — schedule needed.

## Decisions Recorded This Session

- `AudioManager` is a singleton with DontDestroyOnLoad — survives scene transitions.
- Adaptive audio uses 4 music stems controlled by individual volume, not time-stretching — simpler implementation that still achieves tension/relief contrast.
- `HapticManager` is static (no MonoBehaviour needed) — haptics are fire-and-forget events.
- `ParticleManager` uses a single runtime-created ParticleSystem template instead of per-effect prefabs — allows the system to work before art assets are imported; prefab slots are reserved for future art pass.
- `TokenAnimator` uses pure Unity AnimationCurve + Mathf.Lerp — no DOTween dependency.
- `BoosterManager` defaults to infinite charges (int.MaxValue) — economy integration will set finite charges later in Gate 4.
- Tutorial levels 1-5 define hand-tuned board layouts as IngredientColor[,] arrays — guaranteed board states per tutorial-flow.md spec.
- `BrewAnimationController` timing (1.8s total) matches art-direction.md exactly: 0.3s ignition + 0.4s eruption + 0.5s formation + 0.6s celebration.

## Files Touched This Session

### Created — Core (pure C#)

- `Assets/_Project/Scripts/Core/BoosterType.cs`
- `Assets/_Project/Scripts/Core/BoosterManager.cs`
- `Assets/_Project/Scripts/Core/TutorialLevelData.cs`

### Created — Data

- `Assets/_Project/Scripts/Data/SfxId.cs`
- `Assets/_Project/Scripts/Data/AudioConfigSO.cs`
- `Assets/_Project/Scripts/Data/ScreenShakeConfigSO.cs`
- `Assets/_Project/Scripts/Data/SettingsManager.cs`

### Created — Presentation

- `Assets/_Project/Scripts/Presentation/AudioManager.cs`
- `Assets/_Project/Scripts/Presentation/AdaptiveAudioController.cs`
- `Assets/_Project/Scripts/Presentation/HapticManager.cs`
- `Assets/_Project/Scripts/Presentation/ScreenShakeController.cs`
- `Assets/_Project/Scripts/Presentation/ParticleManager.cs`
- `Assets/_Project/Scripts/Presentation/TokenAnimator.cs`
- `Assets/_Project/Scripts/Presentation/BrewAnimationController.cs`
- `Assets/_Project/Scripts/Presentation/TutorialController.cs`
- `Assets/_Project/Scripts/Presentation/TutorialOverlay.cs`
- `Assets/_Project/Scripts/Presentation/BoosterBarUI.cs`
- `Assets/_Project/Scripts/Presentation/BoosterSlotUI.cs`
- `Assets/_Project/Scripts/Presentation/SettingsPanel.cs`

### Created — Tests

- `Assets/Tests/EditMode/Core/BoosterManagerTests.cs`

### Modified — Presentation

- `Assets/_Project/Scripts/Presentation/BoardPresenter.cs` — booster integration, brew animation hook, particle/haptic/shake triggers, correct color palette
- `Assets/_Project/Scripts/Presentation/HudController.cs` — animated move counter, score tween, critical color
- `Assets/_Project/Scripts/Presentation/TokenView.cs` — TokenAnimator reference, idle shimmer on init

### Modified — Editor

- `Assets/_Project/Scripts/Editor/BrewSceneSetup.cs` — full scene hierarchy, scene file creation, UI prefab creation, build settings registration

## Verification Notes

- All new Core classes (`BoosterManager`, `TutorialLevelData`) have zero `using UnityEngine;` — pure C# contract maintained.
- BoosterManager.ActivateShake uses Fisher-Yates shuffle per core-mechanic.md §15.1, with guaranteed valid cluster post-shuffle.
- BoosterManager.ActivateCatalyst creates orb with token_count=1 per core-mechanic.md §15.2.
- BoosterManager.ActivateExtraMoves enforces one-use-per-attempt per core-mechanic.md §15.3.
- AudioManager priority system matches sound-design.md: brew(0) > fusion(1) > chain(2) > cascade(3) > highlight(4) > tap(5) > UI(6).
- ScreenShakeController uses Perlin noise (not random) per Week 5-6 prompt §5 specification.
- HapticManager intensity values match haptic map from sound-design.md (light=0.4, medium=0.6, heavy=0.9).
- BrewAnimationController timing matches art-direction.md brew animation sequence (1.8s total).
- Tutorial level board layouts match tutorial-flow.md exactly for levels 1-5.
- Color palette updated to exact hex values from art-direction.md.

## Handoff Checklist

- Decision log updated
- This handoff file updated
- Context snapshot regenerated (requires running `python scripts/context/build_context_snapshot.py`)
- ADR index validated (no new ADRs needed)
- CI/local readiness checks run and results recorded (requires Unity Editor)
- Unity scene created and verified (run `Brew > Create Gameplay Scene` in Editor)

