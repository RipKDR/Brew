# ADR 0002 — Offline-First Cloud Save Strategy

**Status:** accepted  
**Date:** 2026-04-09  
**Deciders:** Engineering  

## Context

Brew needs to persist player data (currency, progress, workshop, potion shelf, streaks) both locally and in the cloud. Players may be offline (airplane mode, poor connectivity), and the game must remain fully functional without a network connection.

## Decision

Adopt an **offline-first** cloud save architecture:

1. All writes go to local storage immediately (via `CloudSaveManager.SaveLocal`).
2. A dirty flag tracks whether local data has diverged from the server.
3. When connectivity is available, the game syncs local state to Firestore at `players/{uid}`.
4. Conflict resolution uses **last-write-wins** by timestamp for MVP. This is acceptable because only one device is expected to be active at a time for most players.

`CloudSaveManager` is pure C# with no Firebase dependency. The actual Firestore sync is handled by a presentation-layer adapter that subscribes to `OnSyncRequired`.

## Consequences

- The game works fully offline — no "connecting..." gates or loading screens.
- If a player plays on two devices simultaneously without syncing, the last device to sync wins. This is a known limitation acceptable for MVP.
- Server-side validation of currency operations is deferred to post-MVP (receipt validation for IAP is server-side, but level reward grants are trusted locally).

## Alternatives Considered

- **Server-authoritative**: All currency changes require server round-trips. Rejected — too slow for mobile puzzle game flow, and breaks offline play.
- **CRDT-based merge**: Each currency operation is a delta that merges conflict-free. Overly complex for MVP with a single-player game. Could revisit post-launch if multi-device sync becomes a real use case.
