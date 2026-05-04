# Brew — Project CLAUDE.md

Brew is a mobile puzzle game for iOS and Android built in Unity 6 (6000.1 LTS). Players tap clusters of ingredients to fuse them into orbs, chain orbs together, and brew potions to fill recipes before running out of moves.

## Before You Start

Read these first — do not skip:

- `AGENTS.md` — authoritative conventions, architecture, naming rules
- `docs/project-management/session-handoff.md` — current branch, blockers, next 3 tasks
- `docs/project-management/context-snapshot.md` — condensed project state

## Hard Rules

Violating any of these requires explicit user approval:

1. **Currencies:** Exactly 2 — Essence (soft) and Gems (premium). Never add a third.
2. **Genre:** This is NOT a match-3 game. No swapping, no line-matching. It is tap-cluster-fuse-brew.
3. **Economy values:** Never hardcoded. All numbers come from RemoteConfig or ScriptableObjects.
4. **Board state machine:** Must traverse in strict order: `Idle → PlayerInput → Fusing → Cascading → Settling → CheckBrew → CheckWin → Idle`. Never skip states.
5. **MonoBehaviour scope:** MonoBehaviours handle Unity lifecycle only. Game logic is pure C#.

## Current Milestone

Gate 5 — Soft Launch Readiness | Branch: `cursor/audio-config-interface-extraction`

## Quick Directory Reference

| Path | Purpose |
|---|---|
| `Assets/_Project/Scripts/Core` | Board, state machine, game rules |
| `Assets/_Project/Scripts/Meta` | Progression, currencies, IAP |
| `Assets/_Project/Scripts/Data` | ScriptableObjects, RemoteConfig bindings |
| `Assets/_Project/Scripts/Presentation` | UI, audio, VFX, MonoBehaviours |
| `Assets/_Project/Tests/EditMode` | Pure C# unit tests (no Unity runtime) |
