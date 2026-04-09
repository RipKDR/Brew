# ADR 0003 — Remote Config as Single Configuration Source

**Status:** accepted  
**Date:** 2026-04-09  
**Deciders:** Engineering  

## Context

Brew has numerous tunable values: economy rates, ad frequency caps, booster costs, feature flags. These need to be adjustable without app updates, especially during soft launch tuning.

## Decision

Use **Firebase Remote Config** as the single source of truth for all tunable game values at runtime.

1. All config keys and their defaults are defined in `config/firebase/remote-config-defaults.json` (56 keys at time of writing).
2. `RemoteConfigDefaults.cs` mirrors these as compile-time constants for local fallback.
3. `RemoteConfigManager` loads defaults on startup, then fetches server values with a 12-hour cache.
4. Server values override defaults. If the server is unreachable, the app runs on defaults.
5. All game code reads config through `RemoteConfigManager.GetInt/GetFloat/GetBool/GetString` — never directly from ScriptableObjects or hardcoded values at runtime.

ScriptableObjects (`EconomyConfigSO`, etc.) are used for Unity Editor workflows and as the source for default values. At runtime, `RemoteConfigManager` takes precedence.

## Consequences

- Economy tuning during soft launch requires only Firebase Console changes — no app update, no code deployment.
- Feature flags (e.g., `weekly_events_enabled`) can gate features remotely.
- The 12-hour cache means changes take up to 12 hours to reach all users (acceptable for economy tuning; can be reduced for urgent fixes via force-fetch).
- The 56-key defaults file is the canonical reference for all tunable values.

## Alternatives Considered

- **ScriptableObjects only**: Values baked into the build. Rejected — requires app update for any tuning change. Unacceptable during soft launch.
- **Custom backend API**: A REST endpoint serving config. Rejected — Firebase Remote Config provides this out of the box with caching, A/B testing integration, and conditional targeting.
