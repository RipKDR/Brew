# Brew — MVP Build Plan

**Version:** 1.0  
**Last updated:** 2026-04-09  
**Owner:** Game Designer / Producer  
**Timeline:** 10 weeks (Weeks 1–10)  
**Team:** 1 Designer/Producer, 2 Unity Devs, 1 Artist, 0.5 Sound Designer, 0.5 QA

---

## Roles Key

| Abbreviation | Role                        |
| ------------ | --------------------------- |
| **GP**       | Game Designer / Producer    |
| **UD-1**     | Unity Developer (Core)      |
| **UD-2**     | Unity Developer (UI / Meta) |
| **ART**      | Artist                      |
| **SFX**      | Sound Designer (contractor) |
| **QA**       | QA (shared)                 |

---

## Week 1 — Project Skeleton & Grid System

### Deliverables

| #   | Task                                                                                                                                                  | Owner    | Est   |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 1.1 | Create Unity 2022 LTS project. Set folder structure: `Assets/{Scripts, Prefabs, Art, Audio, Data, Scenes, Plugins, Editor}`. Configure .gitignore.    | UD-1     | 0.5 d |
| 1.2 | Configure build pipeline: iOS (Xcode export) and Android (Gradle). Verify clean builds on both platforms from a blank scene.                          | UD-1     | 1 d   |
| 1.3 | Set up CI: GitHub Actions or Unity Cloud Build — auto-build on `main` push, distribute via TestFlight / Firebase App Distribution.                    | UD-2     | 1 d   |
| 1.4 | Implement `BoardManager`: generates an NxM grid of cells. Configurable via ScriptableObject (`BoardConfig`: width, height, cell size, spacing).       | UD-1     | 1.5 d |
| 1.5 | Implement `TokenSpawner`: populates grid cells with random ingredient tokens (5 types). Ensures no pre-made clusters ≥ 3 at spawn.                   | UD-1     | 1 d   |
| 1.6 | Implement `InputHandler`: detect tap on grid cell, find connected cluster of same-type tokens (flood fill, min cluster size = 3).                     | UD-2     | 1 d   |
| 1.7 | Create placeholder token sprites: 5 colored circles (red, blue, green, yellow, purple), 64×64 px.                                                    | ART      | 0.5 d |
| 1.8 | Create game design document skeleton: ingredient types, orb tiers, fusion rules, recipe structure, economy draft.                                     | GP       | 2 d   |
| 1.9 | Draft level schema (JSON): board size, ingredient distribution, recipe targets, move limit. Create 3 test levels.                                     | GP       | 1 d   |

### Dependencies
- 1.4 blocks 1.5 blocks 1.6.
- 1.7 needed by 1.5 for visual representation.
- 1.8 informs all subsequent design work.

### Risks
- Unity version mismatch with target devices — mitigate by running test builds on oldest supported devices (iPhone SE 2nd gen, Android API 24) by end of week.
- CI pipeline setup can eat time on first attempt — UD-2 should timebox to 1 day, fall back to manual builds if needed.

---

## Week 2 — Fusion Mechanic & Placeholder Loop

### Deliverables

| #   | Task                                                                                                                                                    | Owner    | Est   |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 2.1 | Implement basic fusion: tapping a valid cluster (≥ 3 same-type tokens) removes all tokens in cluster and spawns a Tier-1 orb at the tap position.       | UD-1     | 1.5 d |
| 2.2 | Implement gravity system: after tokens are removed, remaining tokens fall to fill gaps. New tokens spawn at top to refill empty cells.                   | UD-1     | 1.5 d |
| 2.3 | Animate fusion: tokens tween toward tap point (0.15 s ease-in), orb scales up from 0 (0.2 s elastic ease). Use DOTween or LeanTween.                   | UD-2     | 1 d   |
| 2.4 | Animate gravity: tokens drop with slight acceleration (0.1 s per row). New tokens enter from off-screen top.                                            | UD-2     | 1 d   |
| 2.5 | Implement cluster highlighting: when player touches a valid cluster, tokens pulse/glow before confirming tap.                                           | UD-2     | 0.5 d |
| 2.6 | Wire up full tap loop: touch → highlight → confirm → fuse → gravity → refill → board settles. Log state transitions.                                   | UD-1     | 1 d   |
| 2.7 | Create placeholder orb sprites: 3 tiers of orbs (small, medium, large circles with tier number overlay).                                               | ART      | 0.5 d |
| 2.8 | Begin ingredient token concept art: silhouette exploration for 5 ingredient types (herb, mushroom, crystal, flower, root).                              | ART      | 3 d   |
| 2.9 | Define orb tier thresholds and fusion math: cluster of 3–4 → Tier 1, 5–7 → Tier 2, 8+ → Tier 3. Document in GDD.                                     | GP       | 1 d   |
| 2.10| Smoke-test on 2 physical devices (1 iOS, 1 Android). File any input or rendering issues.                                                               | QA       | 1 d   |

### Dependencies
- 2.1 requires 1.4–1.6.
- 2.2 must complete before 2.6.
- 2.3–2.4 can run in parallel with 2.1–2.2 using stub animations.

### Risks
- Gravity system edge cases (tokens falling through each other, double-fills) — UD-1 should write unit tests for column compaction.
- Touch input may behave differently on Android vs iOS — QA smoke test on real devices by Friday.

---

## ✅ MILESTONE GATE 1 — End of Week 2

**Gate:** Can tap a cluster and see tokens fuse into an orb on screen.

> Full criteria defined in `milestone-gates.md`, Gate 1.

---

## Week 3 — Chain Fusion, Brew Mechanic, & Recipe System

### Deliverables

| #   | Task                                                                                                                                                       | Owner    | Est   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 3.1 | Implement chain fusion: after gravity settles, scan board for adjacent same-type orbs → auto-fuse into next tier. Repeat until no more fusions possible.    | UD-1     | 2 d   |
| 3.2 | Implement brew trigger: when an orb reaches Tier 3 (or configured threshold), trigger brew event. Orb transforms into potion liquid that fills a vial.     | UD-1     | 1.5 d |
| 3.3 | Implement recipe system: `RecipeData` ScriptableObject defines target potions (type + count). `RecipeTracker` monitors brew events against recipe targets.  | UD-2     | 1.5 d |
| 3.4 | Implement move counter: decrement on each player tap. Display remaining moves. Trigger fail state when moves reach 0 and recipe incomplete.                | UD-2     | 0.5 d |
| 3.5 | Build HUD: move counter (top-left), recipe tracker with potion vials (top-right), current score (top-center). Use Unity UI Toolkit or uGUI.                | UD-2     | 1 d   |
| 3.6 | Implement cascade detection: after each chain resolves, check if gravity refill creates new clusters → trigger automatic fusions. Cap at 20 cascades.      | UD-1     | 1 d   |
| 3.7 | Finalize ingredient token art: render final sprites for 5 types at 128×128 px with clean outlines for readability at mobile scale.                         | ART      | 3 d   |
| 3.8 | Begin orb tier art: design 3 visually distinct tiers per ingredient type (15 orb variants total). Tier 1 = small glow, Tier 2 = pulsing, Tier 3 = radiant. | ART      | 2 d   |
| 3.9 | Write first 10 level definitions in JSON. Levels 1–5 are tutorial-paced (single recipe target, generous moves). Levels 6–10 introduce 2 recipe targets.    | GP       | 2 d   |

### Dependencies
- 3.1 requires 2.1 + 2.2 (fusion + gravity).
- 3.2 requires 3.1 (chain system determines when orbs reach brew threshold).
- 3.3 requires 3.2 (recipe tracker consumes brew events).
- 3.6 depends on 3.1 + gravity system.

### Risks
- Cascades can loop infinitely if gravity refill constantly creates new clusters — enforce cascade cap (20) and add a "board shuffle" fallback if no valid moves remain.
- Chain fusion timing must feel intentional, not confusing — GP should observe chain speed and adjust delays (target: 0.3 s between each auto-fusion step).

---

## Week 4 — Level Flow, Data Pipeline, & Core Loop Closure

### Deliverables

| #   | Task                                                                                                                                                     | Owner    | Est   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 4.1 | Implement `LevelLoader`: reads level JSON, configures board size, ingredient weights, recipe targets, move limit. Validate schema on load.               | UD-1     | 1 d   |
| 4.2 | Implement level complete flow: recipe fulfilled → score tally screen → star rating (1–3 stars based on remaining moves) → "Next Level" button.           | UD-2     | 1.5 d |
| 4.3 | Implement level fail flow: moves exhausted → fail screen → "Retry" button (free) → reload same level.                                                   | UD-2     | 0.5 d |
| 4.4 | Implement level select screen: scrollable grid showing levels 1–40. Locked/unlocked states. Stars displayed for completed levels.                        | UD-2     | 1.5 d |
| 4.5 | Implement local save system: player progress (current level, stars, currency) saved to `PlayerPrefs` or JSON file. Load on app start.                    | UD-1     | 1 d   |
| 4.6 | Integrate final ingredient token art into game. Replace all placeholders. Verify readability on small screens.                                           | UD-2     | 0.5 d |
| 4.7 | Complete orb tier art (all 15 variants). Export sprite atlas for orbs.                                                                                    | ART      | 3 d   |
| 4.8 | Design vial UI: potion vial graphic that fills as brew events occur. One vial per recipe target. Visual states: empty, filling, complete.                 | ART      | 2 d   |
| 4.9 | Playtest levels 1–10. Record completion rates, average moves used, time per level. Adjust ingredient distribution and move limits.                       | GP       | 2 d   |
| 4.10| Full regression pass: fusion, chain, brew, gravity, cascades, level load, win/lose, save/load. Test on 3 devices.                                        | QA       | 2 d   |

### Dependencies
- 4.1 requires 3.9 (level JSON files exist).
- 4.2–4.3 require 3.3–3.4 (recipe + move systems).
- 4.6 requires 3.7 (final token art ready).

### Risks
- Level select UI needs to handle 40 levels without performance issues on low-end devices — use object pooling or paginated scroll.
- Save system corruption risk — implement version header in save file, add integrity check, always write to temp file then rename.

---

## ✅ MILESTONE GATE 2 — End of Week 4

**Gate:** Can play a complete level from start to win/lose with working fusions, chains, brews, and recipe tracking.

> Full criteria defined in `milestone-gates.md`, Gate 2.

---

## Week 5 — Art Integration, Particles, & Sound

### Deliverables

| #   | Task                                                                                                                                                      | Owner    | Est   |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 5.1 | Integrate orb tier art. Map all 15 orb variants to correct ingredient × tier combinations. Verify transitions between tiers during fusion.                | UD-2     | 1 d   |
| 5.2 | Build particle systems: fusion sparkles (burst on merge), brew bubbles (rising from vial), cascade trails (streak following falling tokens).               | UD-2     | 2 d   |
| 5.3 | Build level-complete celebration: particles, vial "pop" effect, star reveal animation. Screen-wide confetti for 3-star completion.                        | UD-2     | 1 d   |
| 5.4 | Integrate vial UI into HUD. Animate fill progress (smooth lerp per brew event). Add "Complete!" stamp when vial is full.                                  | UD-1     | 1 d   |
| 5.5 | Implement screen shake on large cascades (≥ 4 cascades) and brew events. Configurable intensity (light/medium/strong) via ScriptableObject.               | UD-1     | 0.5 d |
| 5.6 | Implement haptic feedback (iOS Taptic Engine, Android Vibration API): light tap on fusion, medium on chain, strong on brew. Add user toggle in settings.   | UD-1     | 1 d   |
| 5.7 | SFX delivery #1: tap (soft click), fuse (whoosh + sparkle), chain (ascending chime, pitch increases per chain step), brew (bubbling + cork pop).          | SFX      | 3 d   |
| 5.8 | SFX delivery #2: win (triumphant jingle, 2 s), lose (descending notes, 1.5 s), cascade (rapid plinks), UI tap (subtle click).                            | SFX      | 2 d   |
| 5.9 | Integrate all SFX. Build `AudioManager` singleton: pooled audio sources, volume control, mute toggle. Map each game event to its SFX clip.                | UD-1     | 1 d   |
| 5.10| Design workshop scene concept: single room with cauldron center, shelf left, ingredient storage right. Perspective: isometric or 3/4 top-down.            | ART      | 3 d   |
| 5.11| Write levels 11–25 in JSON. Introduce multi-ingredient recipes, tighter move limits, and larger board sizes.                                              | GP       | 2 d   |

### Dependencies
- 5.1 requires 4.7 (orb art complete).
- 5.7–5.8 are external contractor deliverables — SFX brief must be sent by start of Week 5 at latest.
- 5.9 requires 5.7–5.8 (audio assets delivered).

### Risks
- Particle overdraw on low-end devices — limit simultaneous particle systems to 5, use GPU instancing where possible, test on iPhone SE 2nd gen.
- SFX contractor delivery delay — have UD-1 prepare placeholder (royalty-free) sounds as fallback so integration work isn't blocked.

---

## Week 6 — Tutorial, Boosters, & "Feel" Polish

### Deliverables

| #   | Task                                                                                                                                                       | Owner    | Est   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 6.1 | Implement tutorial system: overlay-based instruction panels, hand/finger pointer, forced-first-move sequences. Data-driven via JSON.                       | UD-2     | 2 d   |
| 6.2 | Build 5 tutorial levels: L1 (tap a cluster), L2 (chains), L3 (brew a potion), L4 (use a booster), L5 (complete a recipe with a move limit).                | GP       | 2 d   |
| 6.3 | Implement Booster 1 — "Mortar & Pestle": tap any single token to destroy it and its 8 neighbors. Costs 1 booster charge.                                  | UD-1     | 1 d   |
| 6.4 | Implement Booster 2 — "Catalyst Drop": converts all tokens of one type to another chosen type. Tap booster, then tap target type, then tap desired type.   | UD-1     | 1.5 d |
| 6.5 | Implement Booster 3 — "Stir the Pot": shuffles all tokens on the board randomly. Guarantees at least one valid cluster of ≥ 3 after shuffle.               | UD-1     | 1 d   |
| 6.6 | Build booster bar UI: 3 booster slots below the board. Show remaining charges. Tap to activate, tap again on board to apply. Cancel by tapping booster.     | UD-2     | 1 d   |
| 6.7 | Ambient audio: looping workshop background (bubbling cauldron, gentle music). Crossfade between gameplay and menus.                                        | SFX      | 2 d   |
| 6.8 | Polish pass: refine all animation timings, particle densities, SFX volumes. GP and ART review every interaction for "feel."                                | GP + ART | 2 d   |
| 6.9 | Integrate ambient audio. Add music volume slider separate from SFX in settings.                                                                            | UD-1     | 0.5 d |
| 6.10| External playtest #1: recruit 3–5 non-team members. Observe them play tutorial + levels 1–10 silently. Record: where they hesitate, tap wrong, get stuck.  | GP       | 2 d   |
| 6.11| QA: full regression on tutorial flow, all 3 boosters, level complete/fail with boosters active, save/load with booster charges.                            | QA       | 2 d   |

### Dependencies
- 6.1 requires all core mechanics (fusion, chain, brew) to be stable.
- 6.2 requires 6.1 (tutorial system) + 6.3–6.5 (boosters, for L4).
- 6.10 requires 6.8 (polish pass complete — game must feel good for valid playtest data).

### Risks
- Tutorial is too hand-holdy or too sparse — iterate based on 6.10 playtest observations. Budget 1 day rework.
- Boosters may trivialize levels — GP tracks booster usage in playtest. If >50% of wins use boosters on early levels, rebalance move limits or booster costs.

---

## ✅ MILESTONE GATE 3 — End of Week 6

**Gate:** The game FEELS good. A non-team-member can pick it up, complete the tutorial, and want to play more.

> Full criteria defined in `milestone-gates.md`, Gate 3. **This is the most critical gate.**

---

## Week 7 — Meta Systems: Potion Shelf, Workshop, Daily Brew

### Deliverables

| #   | Task                                                                                                                                                        | Owner    | Est   |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 7.1 | Implement Potion Shelf screen: grid of all potion types. Locked potions greyed out. Brewed potions displayed with count and "first brewed" date.             | UD-2     | 1.5 d |
| 7.2 | Implement Workshop scene: render isometric room. Cauldron upgradeable (3 tiers: copper, silver, gold). Each tier unlocks a passive bonus (e.g., +1 move).   | UD-2     | 2 d   |
| 7.3 | Implement Workshop upgrade flow: tap cauldron → upgrade panel → show cost (Essence) → confirm → upgrade animation → passive bonus applied.                  | UD-2     | 1 d   |
| 7.4 | Implement Daily Brew: one special level per day, unique recipe. Reward: bonus Essence + exclusive potion. Reset at midnight UTC. Calendar UI showing streak. | UD-1     | 2 d   |
| 7.5 | Implement Win Streak system: consecutive level wins increment streak counter. Streak bonuses at 3, 7, 14 wins. Streak breaks on level fail.                 | UD-1     | 1 d   |
| 7.6 | Render workshop art assets: room background, cauldron (3 tiers), shelf, storage containers. Export as layered sprites for parallax.                          | ART      | 5 d   |
| 7.7 | Design potion bottle art: 8 unique potion bottle shapes/colors matching the 5 ingredient types + 3 combination potions.                                     | ART      | 2 d   |
| 7.8 | Write levels 26–35. Focus on variety: varying board sizes (6×6, 7×8, 8×8), multi-potion recipes, ingredient-restricted levels.                              | GP       | 2 d   |
| 7.9 | QA: workshop upgrade persistence across sessions, daily brew reset logic (test by advancing device clock), win streak break/resume edge cases.               | QA       | 2 d   |

### Dependencies
- 7.2 requires 7.6 (workshop art) — UD-2 builds with placeholder gray boxes, art swapped in as available.
- 7.4 requires level loading system (4.1) + recipe system (3.3).
- 7.7 needed by 7.1 for shelf display.

### Risks
- Workshop scope creep (multiple rooms, decoration, etc.) — strictly limit to 1 room, 1 upgradeable item (cauldron) for MVP. Document cut features in backlog.
- Daily Brew time zone handling — use server time (Firebase) to prevent clock manipulation. If Firebase isn't ready, use device time with drift detection.

---

## Week 8 — Economy, Monetization, & Backend

### Deliverables

| #   | Task                                                                                                                                                             | Owner    | Est   |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 8.1 | Implement dual currency system: Essence (soft, earned from levels/daily brew) and Gems (hard, earned slowly or bought via IAP). Wallet UI in header.              | UD-1     | 1 d   |
| 8.2 | Implement IAP: Gem bundles (small $0.99 / 80 gems, medium $4.99 / 500 gems, large $9.99 / 1200 gems). Integrate Unity IAP plugin. Handle purchase/restore flow. | UD-1     | 2 d   |
| 8.3 | Implement rewarded ad — fail recovery: on level fail, offer "Watch ad to get +5 moves." Use Unity Ads or AdMob mediation. Once per level attempt.                | UD-2     | 1 d   |
| 8.4 | Implement rewarded ad — 2× daily brew reward: after daily brew win, offer "Watch ad to double your Essence reward."                                              | UD-2     | 0.5 d |
| 8.5 | Implement rewarded ad — streak protection: on level fail with active streak ≥ 3, offer "Watch ad to keep your streak."                                           | UD-2     | 0.5 d |
| 8.6 | Implement interstitial ads: show after every 3rd level completion (not after fails). 30 s cooldown between interstitials. Frequency tunable via Remote Config.    | UD-2     | 1 d   |
| 8.7 | Firebase setup: Authentication (anonymous + Google Sign-In), Cloud Firestore (player profile, progress), Remote Config (ad frequency, economy values, feature flags). | UD-1  | 2 d   |
| 8.8 | Firebase Analytics: log events — `level_start`, `level_complete`, `level_fail`, `booster_used`, `ad_watched`, `iap_purchase`, `daily_brew_complete`, `streak_milestone`. | UD-1 | 1 d |
| 8.9 | Cloud save: sync local progress to Firestore on level complete and app backgrounding. Conflict resolution: server wins for currency, latest timestamp wins for progress. | UD-1 | 1.5 d |
| 8.10| Economy tuning spreadsheet: model Essence earn rate, Gem earn rate, upgrade costs, booster costs. Target: F2P player can upgrade cauldron fully in ~14 days.      | GP       | 2 d   |
| 8.11| QA: IAP sandbox testing (iOS sandbox, Google Play test track), ad display frequency, currency grant/deduct accuracy, cloud save round-trip.                      | QA       | 2 d   |

### Dependencies
- 8.2 requires Apple Developer account + Google Play Console setup (GP handles, should be done by Week 7).
- 8.3–8.6 require ad SDK integration — start SDK setup early in week.
- 8.7 blocks 8.8 and 8.9.
- 8.10 informs all currency values in 8.1.

### Risks
- IAP receipt validation is complex — for MVP, use Unity IAP's built-in validation. Server-side receipt validation deferred to post-soft-launch.
- Ad SDK initialization failures on certain Android devices — implement graceful fallback (hide ad buttons if SDK fails to init, log error to analytics).
- Firebase Firestore costs can spike with write-heavy patterns — batch writes, limit cloud save frequency to once per level complete (not per action).

---

## ✅ MILESTONE GATE 4 — End of Week 8

**Gate:** Full session loop works: play levels → earn currency → upgrade workshop → return for daily brew → maintain streak.

> Full criteria defined in `milestone-gates.md`, Gate 4.

---

## Week 9 — Content Completion, Weekly Event, & Tuning

### Deliverables

| #   | Task                                                                                                                                                           | Owner    | Est   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 9.1 | Write levels 36–40. These are "challenge" levels: large boards, 3+ recipe targets, tight move limits. Require mastery of chains and boosters.                  | GP       | 1.5 d |
| 9.2 | Implement weekly event template: "Potion Festival" — 5 special event levels available Mon–Sun, unique ingredient (e.g., "Moonflower"), leaderboard (local).    | UD-1     | 2 d   |
| 9.3 | Build event UI: event banner on main menu, event level select, event reward screen, event timer countdown.                                                     | UD-2     | 1.5 d |
| 9.4 | Create event art: "Moonflower" token sprite, event banner, event-themed board background.                                                                      | ART      | 2 d   |
| 9.5 | Difficulty curve tuning: GP plays all 40 levels sequentially. Target completion rates: L1–10 (95%), L11–25 (80%), L26–35 (65%), L36–40 (45%).                  | GP       | 3 d   |
| 9.6 | Internal playtest: entire team plays levels 1–40. Each person records: levels where they got stuck, levels that felt too easy, booster usage frequency.         | ALL      | 1 d   |
| 9.7 | External playtest #2: recruit 5–8 new players. Measure: tutorial completion rate, session length, voluntary return rate (do they open the app again next day?). | GP       | 2 d   |
| 9.8 | Implement settings screen: SFX volume, music volume, haptics toggle, notifications toggle, privacy policy link, credits, restore purchases button.             | UD-2     | 1 d   |
| 9.9 | Implement basic notification: daily brew reminder (configurable time, default 7 PM local), streak-at-risk reminder (if streak ≥ 3, remind at 8 PM).            | UD-1     | 1 d   |
| 9.10| QA: play through all 40 levels on 2 iOS + 2 Android devices. Log any level that crashes, soft-locks, or has incorrect recipe tracking.                         | QA       | 3 d   |

### Dependencies
- 9.1 requires playtesting data from 7.8 levels to calibrate difficulty.
- 9.2–9.3 can be built independently of level content.
- 9.5 blocks 9.6 (tuning must happen before team playtest validates it).

### Risks
- Level 36–40 may be too hard or require specific RNG to complete — GP should verify each level is completable in ≤ 3 attempts with optimal play (no boosters).
- External playtest recruitment takes time — GP should begin recruiting during Week 8.

---

## Week 10 — Bug Fixing, Optimization, & Soft Launch Prep

### Deliverables

| #   | Task                                                                                                                                                          | Owner    | Est   |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- | ----- |
| 10.1| Bug fix sprint: prioritize all P0 (crash/data loss) and P1 (gameplay-breaking) bugs from QA and playtests. Target: zero P0, ≤ 3 P1 (with workarounds).       | UD-1/2   | 3 d   |
| 10.2| Performance optimization: profile on lowest-spec target devices. Targets: 60 fps during gameplay, < 3 s cold start, < 100 MB install size.                   | UD-1     | 2 d   |
| 10.3| Memory optimization: audit texture atlases (max 2048×2048), pool particle systems, unload unused scenes. Target: < 200 MB peak RAM on low-end Android.        | UD-1     | 1 d   |
| 10.4| Device matrix testing: test on minimum 8 devices spanning iPhone SE 2nd gen → iPhone 15 Pro, low-end Android (2 GB RAM, Snapdragon 4xx) → flagship.           | QA       | 3 d   |
| 10.5| Create app store assets: icon (1024×1024), 5 screenshots per platform (gameplay, tutorial, workshop, potion shelf, daily brew), short description (80 char).  | ART      | 3 d   |
| 10.6| Write app store description (4000 char max), keywords, and category selection. Optimize for discovery: "puzzle," "potion," "match," "brew."                   | GP       | 1 d   |
| 10.7| Record preview video: 30 s gameplay capture showing fusion → chain → brew → level win. Add text overlays. Export for App Store and Google Play.                | ART + GP | 1 d   |
| 10.8| Final economy sanity check: verify all currency earn/spend rates match design spreadsheet. Simulate 100-level playthrough in spreadsheet.                      | GP       | 1 d   |
| 10.9| Prepare soft launch build: set version to 1.0.0-beta. Configure Firebase Remote Config defaults. Enable analytics. Disable debug logs.                        | UD-1     | 0.5 d |
| 10.10| Submit to TestFlight and Google Play internal testing track. Verify builds install and run correctly from store distribution.                                 | UD-1     | 1 d   |
| 10.11| Prepare soft launch plan document: target geos (Canada, Australia, New Zealand), UA budget ($500–$1000), success metrics, kill criteria, timeline.            | GP       | 1 d   |

### Dependencies
- 10.1 requires complete QA pass (9.10).
- 10.5 requires final game art to be in-engine (no placeholder art remaining).
- 10.9 requires 10.1 (no P0 bugs).
- 10.10 requires 10.9.

### Risks
- P0 bugs discovered late could delay soft launch — reserve 2 days of buffer in Week 10 for critical fixes.
- App Store review rejection (usually metadata issues) — submit early in Week 10, respond to any rejection same-day.
- Performance may not hit targets on oldest devices — have a fallback plan: reduce particle density, lower resolution on low-end devices via quality tiers.

---

## ✅ MILESTONE GATE 5 — End of Week 10

**Gate:** Build is stable, performant, and ready for soft launch.

> Full criteria defined in `milestone-gates.md`, Gate 5.

---

## Appendix: Weekly Rhythm

Every week follows this cadence:

| Day       | Activity                                                        |
| --------- | --------------------------------------------------------------- |
| Monday    | Week kickoff standup (30 min). Review previous week, plan week. |
| Tue–Thu   | Heads-down build time. Async check-ins via Slack/Discord.       |
| Friday AM | Playtest session (30 min) — everyone plays latest build.        |
| Friday PM | Week retro (30 min). Demo deliverables. Flag blockers.          |

## Appendix: Tool Stack

| Tool              | Purpose                           |
| ----------------- | --------------------------------- |
| Unity 2022 LTS    | Game engine                       |
| Firebase          | Auth, Firestore, Analytics, RC    |
| GitHub             | Version control, CI/CD           |
| Notion / Sheets   | GDD, level data, economy model   |
| TestFlight / FATD | Build distribution                |
| Slack / Discord   | Team communication                |
| Miro              | Design brainstorming              |
