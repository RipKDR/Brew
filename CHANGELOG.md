# Changelog

All notable changes to Brew are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), with project-specific sections for context and documentation integrity.

## [Unreleased]

### Added — Feel & Juice (Gate 3 prep)

- **Booster System**: `BoosterType`, `BoosterManager` (pure C#), `BoosterBarUI`, `BoosterSlotUI` with full Shake/Catalyst/ExtraMoves logic per core-mechanic.md §15
- **Audio System**: `AudioManager` singleton (8 SFX + 4 music AudioSources, priority voice-stealing, round-robin variants, pitch variation), `AudioConfigSO`, `SfxId` enum (16 SFX types), `AdaptiveAudioController` (tension stems at ≤5 moves, critical state at 1 move, brew ducking)
- **Haptic System**: `HapticManager` (static, platform-abstracted iOS/Android, 5 feedback types: Light/Medium/Heavy/Success/Selection)
- **Screen Shake**: `ScreenShakeConfigSO` (4 presets), `ScreenShakeController` (Perlin noise, stacking, chain length scaling)
- **Particle System**: `ParticleManager` (pooled, max 5 concurrent, 6 effect types: FusionSparkles/CascadeTrail/BrewBubbles/BrewBurst/LevelCompleteConfetti/ChainIndicator)
- **Token Animation**: `TokenAnimator` (idle shimmer, selected pulse, fusing rush with squash-and-stretch)
- **Brew Animation**: `BrewAnimationController` (1.8s 4-phase sequence: Ignition→Eruption→Formation→Celebration per art-direction.md)
- **Tutorial System**: `TutorialLevelData` (pure C#, 5 hand-tuned levels per tutorial-flow.md), `TutorialController` (step-by-step flow, input gating, hint system), `TutorialOverlay` (dim/spotlight/glow ring/finger indicator)
- **Settings**: `SettingsManager` (PlayerPrefs-backed), `SettingsPanel` (volume sliders, haptics/shake toggles)
- **Scene Setup**: Enhanced `BrewSceneSetup` with `Brew > Create Gameplay Scene` menu, full UI hierarchy creation, `RecipeVialUI.prefab` and `LevelButton.prefab` generation
- **Tests**: `BoosterManagerTests` (15 tests covering charges, shake, catalyst, extra moves)

### Changed

- Updated `BoardPresenter`: integrated boosters, brew animation, particle/haptic/shake triggers, tutorial controller, correct art-direction.md color palette (#E05A3A, #6FB8D9, #6DAF5E, #F2C745, #6B4E9B)
- Updated `HudController`: animated move counter with pulse (red at ≤5, critical at 1), smoothstep score tween
- Updated `TokenView`: exposes SpriteRenderer/TokenAnimator, auto-starts idle shimmer
- Updated `BrewSceneSetup`: generates full Canvas hierarchy, prefabs, GameFlowController wiring, EditorBuildSettings

### Previously Added

- Added canonical session continuity doc at `docs/project-management/session-handoff.md`.
- Added ADR system scaffolding:
  - `docs/adr/README.md`
  - `docs/adr/0001-canonical-unity-version.md`

### Previously Changed

- Updated CI required-doc checks in `.github/workflows/ci.yml` to match actual repository paths.
- Added context continuity CI job (`validate-context`) in `.github/workflows/ci.yml`.
- Aligned canonical engine version to Unity 6 (6000.1 LTS) across:
  - `docs/project-management/project-summary.md`
  - `docs/onboarding/new-team-member-guide.md`
  - `README.md`