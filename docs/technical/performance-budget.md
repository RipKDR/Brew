# Performance Budget

Hard limits and measurable targets for Brew across all supported devices. Every value in this document is a gate — builds that exceed maximum thresholds must not ship.

---

## Target Devices (Performance Floor)

All budgets must be met on these baseline devices. If it runs well here, it runs well everywhere.


| Platform | Device                    | SoC        | RAM  | Display         |
| -------- | ------------------------- | ---------- | ---- | --------------- |
| iOS      | iPhone SE 2nd gen (2020)  | A13 Bionic | 3 GB | 750×1334 @60Hz  |
| Android  | Samsung Galaxy A14 (2023) | Exynos 850 | 4 GB | 1080×2408 @60Hz |


Higher-end devices may receive enhanced VFX (particle density, shader quality) via a quality tier system, but the baseline experience must be indistinguishable in gameplay feel.

---

## Frame Rate

All measurements taken via Unity Profiler or platform GPU profilers (Xcode Instruments, Android GPU Inspector).


| Scene                       | Target | Minimum | Measurement Method                    |
| --------------------------- | ------ | ------- | ------------------------------------- |
| Gameplay — idle board       | 60 fps | 55 fps  | Average over 10s, no input            |
| Gameplay — fusion animation | 60 fps | 50 fps  | P95 during fusion VFX                 |
| Gameplay — cascade + chain  | 60 fps | 45 fps  | Floor during peak cascade (5+ groups) |
| Gameplay — brew animation   | 60 fps | 50 fps  | P95 during brew VFX burst             |
| Workshop scene              | 60 fps | 55 fps  | While scrolling and interacting       |
| Menus / UI                  | 60 fps | 58 fps  | During screen transitions             |


### Frame Time Thresholds


| Metric                             | Budget                           |
| ---------------------------------- | -------------------------------- |
| Average frame time                 | ≤ 16.6ms                         |
| P99 frame time                     | ≤ 22ms                           |
| Frame time spikes (single frame)   | ≤ 33ms (never drop below 30 fps) |
| GC allocation per frame (gameplay) | ≤ 0 bytes (zero-alloc hot path)  |


---

## Memory

Measured via Xcode Memory Debugger (iOS) and Android Studio Profiler.


| Category              | Budget | Maximum | Notes                                     |
| --------------------- | ------ | ------- | ----------------------------------------- |
| **Total app memory**  | 250 MB | 300 MB  | On low-end devices                        |
| Texture memory        | 80 MB  | 100 MB  | ETC2 (Android), ASTC (iOS), atlased       |
| Audio memory          | 20 MB  | 30 MB   | Compressed clips (Vorbis); music streamed |
| Particle systems      | 15 MB  | 20 MB   | Object-pooled, shared materials           |
| Mesh/geometry         | 5 MB   | 10 MB   | 2D game — minimal 3D geometry             |
| Level data            | 3 MB   | 5 MB    | All 40 MVP levels can remain loaded       |
| Script heap (managed) | 30 MB  | 50 MB   | Monitor with Unity Profiler               |
| Reserved headroom     | 50 MB+ | —       | For OS, background apps, and spikes       |


### Memory Rules

- No textures larger than 2048×2048. Prefer 1024×1024 atlases.
- Music tracks: OGG Vorbis, streamed from disk, never fully loaded into memory.
- SFX clips: compressed, ≤ 200KB each, loaded on scene entry.
- Font atlases: generate per-language character sets, not full Unicode.

---

## Install Size


| Platform                    | Target  | Maximum  | Notes                                   |
| --------------------------- | ------- | -------- | --------------------------------------- |
| iOS (IPA)                   | < 80 MB | < 100 MB | Below App Store cellular download limit |
| Android APK                 | < 80 MB | < 100 MB | For sideload and legacy stores          |
| Android AAB (download size) | < 60 MB | < 80 MB  | Google Play optimized delivery          |


### Size Breakdown Targets


| Asset Category                      | Target Size |
| ----------------------------------- | ----------- |
| Textures (compressed)               | < 35 MB     |
| Audio (SFX + music)                 | < 15 MB     |
| Fonts (all languages)               | < 12 MB     |
| Code (IL2CPP)                       | < 10 MB     |
| Shaders (compiled)                  | < 3 MB      |
| Level data                          | < 2 MB      |
| Other (config, localization tables) | < 3 MB      |


---

## Load Times

Measured on target floor devices with a stopwatch timer in a release build (not editor).


| Transition                     | Target | Maximum | Notes                           |
| ------------------------------ | ------ | ------- | ------------------------------- |
| Cold start → main menu         | < 3s   | < 5s    | Splash screen shown immediately |
| Main menu → level start        | < 1s   | < 2s    | Pool pre-warming included       |
| Level complete → result screen | < 0.5s | < 1s    | No loading screen needed        |
| Result → next level start      | < 1s   | < 2s    | Reuse pooled objects            |
| Workshop load                  | < 1s   | < 2s    | Lazy-load decoration previews   |
| Settings screen                | < 0.3s | < 0.5s  | Instant overlay                 |
| Shop screen                    | < 0.5s | < 1s    | IAP product fetch can be async  |


### Loading Rules

- Never show a loading bar for transitions under 1s. Use a quick crossfade instead.
- Pre-load the next scene's critical assets during idle moments (e.g., while result screen is displayed).
- Use `Addressables.LoadAssetAsync` for non-critical assets; block only on gameplay-essential data.

---

## Battery

Measured on iPhone SE 2nd gen at 50% brightness, Wi-Fi on, Bluetooth off, starting from 100% charge.


| Scenario                      | Target      | Maximum     |
| ----------------------------- | ----------- | ----------- |
| 30-minute gameplay session    | < 8% drain  | < 10% drain |
| 60-minute gameplay session    | < 15% drain | < 20% drain |
| Idle on main menu (5 minutes) | < 1% drain  | < 2% drain  |


### Battery Optimization Rules

- Cap target frame rate to 60 fps (do not render faster than display refresh).
- Reduce render frequency on static screens (menus, pause) to 30 fps or on-demand rendering.
- Disable GPS, accelerometer, and gyroscope — Brew does not use them.
- Minimize background network polling. Use push notifications, not polling loops.
- Compress and limit particle system complexity on low-battery mode if the OS signals it.

---

## Network

All network operations must be resilient to offline play. The game is playable without any network connection.


| Operation              | Target Latency | Timeout | Failure Behavior                                        |
| ---------------------- | -------------- | ------- | ------------------------------------------------------- |
| Cloud save sync        | < 500ms        | 5s      | Graceful offline fallback; queue and retry on reconnect |
| Remote config fetch    | < 300ms        | 3s      | Use cached values; refresh in background                |
| Ad load (interstitial) | < 2s           | 5s      | Skip ad placement if timeout; do not block gameplay     |
| Ad load (rewarded)     | < 2s           | 5s      | Show "Ad unavailable" message                           |
| IAP verification       | < 1s           | 10s     | Retry once; grant provisionally and verify async        |
| Analytics event batch  | < 200ms        | 3s      | Fire-and-forget; buffer locally if offline              |
| Leaderboard fetch      | < 1s           | 5s      | Show cached data; refresh async                         |


### Network Rules

- All network calls are non-blocking on the main thread.
- Use exponential backoff for retries (1s, 2s, 4s, max 30s).
- Batch analytics events — send at most every 30s or on app background.
- Cloud save: conflict resolution uses "latest timestamp wins" with server reconciliation.

---

## Object Pooling

**Rule: NEVER call `Instantiate()` or `Destroy()` during active gameplay.** All frequently used objects must be pre-warmed in pools during level load.


| Object Type              | Pool Size | Rationale                                   |
| ------------------------ | --------- | ------------------------------------------- |
| Token sprites            | 100       | Max grid is 9×11 = 99 tokens                |
| Brew Orb sprites         | 20        | Max simultaneous orbs during brew animation |
| Particle — fusion burst  | 10        | Max concurrent fusions in a cascade         |
| Particle — brew VFX      | 5         | One brew at a time, with overlap            |
| Particle — cascade trail | 30        | Trails per falling token during cascade     |
| Particle — match sparkle | 15        | Sparkle on matched clusters                 |
| Text popup — score       | 10        | Floating score numbers                      |
| Text popup — combo       | 5         | Combo callout labels                        |
| UI — toast notification  | 3         | System messages                             |


### Pool Management

- Pre-warm all pools during the level load screen.
- If a pool is exhausted, log a warning (debug builds only) and reuse the oldest active object.
- Return objects to pool on disable, not destroy.
- Pools are reset (not destroyed) between levels.

---

## Draw Calls

Measured via Unity Frame Debugger and Xcode GPU capture.


| Scene    | Target | Maximum | Notes                               |
| -------- | ------ | ------- | ----------------------------------- |
| Gameplay | < 50   | < 80    | Tokens + orbs + UI + background     |
| Workshop | < 40   | < 60    | Static scene, few animated elements |
| Menus    | < 30   | < 40    | UI-only screens                     |


### Draw Call Reduction Strategies

- **Sprite atlasing:** All tokens, orbs, boosters, and UI icons packed into shared atlases (max 2048×2048 per atlas).
- **SRP Batcher:** All shaders must be SRP Batcher compatible. Verify in Frame Debugger.
- **UI batching:** Group UI elements sharing the same material and texture atlas. Avoid overlapping Canvas sort orders that break batches.
- **Dynamic batching:** Enable for small meshes (< 300 vertices) as a fallback.
- **Shader variants:** Strip unused shader variants in build settings. Target < 50 unique shader variants total.

---

## GPU Budget


| Metric               | Budget                 | Maximum                         |
| -------------------- | ---------------------- | ------------------------------- |
| GPU frame time       | < 10ms                 | < 14ms                          |
| Overdraw (gameplay)  | < 3x                   | < 4x                            |
| Fill rate (gameplay) | < 2 full-screen passes | < 3                             |
| Shader complexity    | Simple (unlit/sprite)  | No multi-pass shaders on tokens |


### GPU Rules

- Token shaders: unlit sprite shader with color tint. No real-time lighting.
- Particle shaders: additive blend, no depth write.
- Background: single draw call, pre-rendered texture.
- Post-processing: none on low-end tier. Optional subtle vignette on high-end.

---

## Profiling Cadence


| Frequency                     | Activity                                      | Devices                                        |
| ----------------------------- | --------------------------------------------- | ---------------------------------------------- |
| Weekly                        | Unity Profiler deep dive (CPU + GPU + memory) | iPhone SE 2nd gen, Samsung A14                 |
| Every milestone gate          | Full performance audit against all budgets    | Both floor devices + one high-end per platform |
| Before every build submission | Automated performance smoke test              | Both floor devices                             |
| After every new VFX addition  | Particle budget and draw call check           | Samsung A14 (GPU-constrained)                  |


### Profiling Checklist (per session)

- Average fps ≥ minimum for each scene
- No GC allocations during gameplay hot path
- Total memory ≤ 300 MB
- Texture memory ≤ 100 MB
- Draw calls ≤ maximum for each scene
- No frame time spikes > 33ms
- Load times within budget
- No `Instantiate` / `Destroy` calls during gameplay (verify via Profiler search)
- Install size within budget

### Performance Regression Policy

- Any PR that increases average frame time by > 1ms on a floor device must include a justification.
- Any PR that adds > 2 MB to install size must include an asset audit.
- Performance budgets are checked in CI via automated profiler snapshots (stretch goal).