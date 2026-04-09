# Brew — Week 5-6: Feel & Juice

You are continuing the build of **Brew**, a mobile puzzle game (Unity 6 / C#). This prompt covers Weeks 5-6 — art integration, particles, sound, haptics, tutorial, and boosters. **This is the most critical milestone in the entire project.** A mechanically correct game that doesn't *feel* good will not retain players. Every deliverable here serves one goal: making the game feel warm, satisfying, and impossible to put down.

---

## Before You Start

1. Read `docs/prompts/00-context-recovery.md` to restore full project context.
2. Read `AGENTS.md` — follow every convention.
3. Read `docs/creative/art-direction.md` **carefully** — it defines the exact visual identity (color palette, ingredient colors with primary/highlight/shadow hex values, typography, token design, orb tiers, brew animation sequence, UI style). Do not deviate from it.
4. Read `docs/creative/sound-design.md` **carefully** — it defines every SFX (with sonic character descriptions), the music loop structure (72s, D major, kalimba + guitar), adaptive audio behavior, and the haptic mapping.
5. Read `docs/game-design/tutorial-flow.md` — it specifies the exact board layouts, step-by-step flows, text prompts, and interaction constraints for all 5 tutorial levels. Implement it **exactly** as written.
6. Read `docs/game-design/core-mechanic.md` — reference the booster section for Shake, Catalyst, and Extra Moves specifications.
7. Read `docs/production/milestone-gates.md`, Gate 3 — this is the highest-bar gate in the project.

**Prerequisite:** Week 3-4 milestone (Gate 2) has PASSED. The complete core loop works — recipes, win/lose, level progression, scoring, blockers, event bus, all 40 levels loading. If any Gate 2 criterion is failing, fix it first.

---

## Week 5 Deliverables

### 1. Token Art Integration

Replace placeholder colored circles with proper ingredient token sprites. The 5 types per `art-direction.md`:
- **Ember** (red, `#E05A3A`) — flame/teardrop silhouette
- **Frost** (blue, `#6FB8D9`) — snowflake/hexagon silhouette
- **Vine** (green, `#6DAF5E`) — leaf/organic curve silhouette
- **Sun** (yellow, `#F2C745`) — star/radiating spokes silhouette
- **Shadow** (purple, `#6B4E9B`) — crescent/wisp silhouette

Each token must have a **distinct silhouette** — players should be able to identify types by shape alone, not just color (accessibility requirement). Implement three animation states:
- **Idle:** Subtle shimmer/breathing animation (scale oscillation 0.98–1.02 over 2s, ease-in-out)
- **Selected/highlighted:** Pulse with glow ring when player touches a valid cluster
- **Fusing:** Tokens rush toward the merge point (ease-in curve, 150ms per `core-mechanic.md` §4.1)

### 2. Brew Orb Art Integration

Implement 3 visual tiers for orbs, each per ingredient color (15 variants total):
- **Small (token_count 3-4):** Gentle inner glow, slow pulse (1.5s cycle)
- **Medium (token_count 5):** Brighter glow, faster pulse (1.0s cycle), subtle particle emission (2-3 sparkles/sec)
- **Large (token_count 6+, about to brew):** Intense glow, rapid pulse (0.5s cycle), heavy particle emission (8-10 sparkles/sec), audible hum

The orb progression must visually communicate "this is building toward something." Players should *anticipate* the brew before it triggers.

### 3. Brew Animation — THE MONEY SHOT

This is the single most important animation in the game. When an orb reaches brew threshold:

1. **Crescendo (0.5s):** Orb pulses rapidly, glow intensifies, particles spiral inward. Screen shake builds. A rising bubbling SFX plays.
2. **Transform (0.5s):** Orb bursts into sparkles. A potion bottle materializes at the cell. Liquid fills the bottle from bottom to top with a satisfying pour.
3. **Celebration (0.5-1.0s):** Potion bottle glows and bounces. Matching vial on the HUD fills simultaneously. Particles radiate outward. If this completes a recipe target, extra confetti burst.

Total sequence: 1.5-2.0 seconds. This must feel **magical**. Reference `art-direction.md` brew animation section for exact visual spec. The brew moment is why players keep playing — it must be the visual and emotional highlight of every level.

### 4. Particle Systems

Implement ALL particle effects using **object pooling** (never Instantiate during gameplay — use the project's `ObjectPool<T>` from `Scripts/Utilities/`):
- **Fusion sparkles:** Burst at merge point, 15-20 particles, 0.3s lifetime, ingredient color
- **Cascade trails:** Streak following falling tokens during gravity, short-lived (0.2s)
- **Brew bubbles and steam:** Rising from orb during crescendo, transitioning to sparkle burst
- **Level complete confetti:** Screen-wide celebration, 3-star gets more/bigger confetti
- **Chain cascade indicator:** Brief connecting line/trail between chain-fusing orbs

Cap simultaneous particle systems at 5 to protect low-end device performance. Use GPU instancing where supported.

### 5. Screen Shake

Implement via a configurable `ScreenShakeConfig` ScriptableObject in `ScriptableObjects/Config/`:
- **Fusion:** Subtle (intensity 0.5, duration 0.1s)
- **Chain fusion:** Medium (intensity 1.0, duration 0.15s, increases with chain length)
- **Brew:** Large (intensity 2.0, duration 0.3s)
- **Level complete:** Celebratory (intensity 1.5, duration 0.5s)

Use Perlin noise for organic shake, not random. Provide a user toggle in Settings to disable screen shake.

### 6. Haptic Feedback

Implement per `sound-design.md` haptic section:
- **Token tap:** Light impact (iOS: `UIImpactFeedbackGenerator(.light)`, Android: 10ms vibration)
- **Fusion:** Medium impact (iOS: `.medium`, Android: 25ms)
- **Chain fusion:** Medium impact, one per chain step
- **Brew:** Heavy impact (iOS: `.heavy`, Android: 50ms)
- **Level complete:** Success notification (iOS: `UINotificationFeedbackGenerator(.success)`, Android: pattern)

Provide a user toggle in Settings. Default ON for devices with haptic engines, OFF for devices without.

---

## Week 6 Deliverables

### 1. Sound Integration

Implement ALL SFX from `sound-design.md` SFX catalog. Build an `AudioManager` in `Scripts/Presentation/` with pooled AudioSources (minimum 8 concurrent). Key sounds:
- `sfx_token_tap` — soft ceramic tink on wood
- `sfx_fusion` — whoosh + sparkle, with subtle pitch variation per instance (±50 cents random)
- `sfx_chain` — ascending chime sequence, pitch rises per chain step (+2 semitones each)
- `sfx_brew` — the signature sound: bubbling crescendo → cork pop → sparkle shimmer. 1.5-2s.
- `sfx_cascade` — rapid plinks, randomized pitch in a pentatonic scale
- `sfx_level_complete` — triumphant jingle (2s)
- `sfx_level_fail` — gentle descending notes (1.5s, sympathetic not punishing)
- `sfx_booster_activate` — distinct per booster type

Volume controls: separate sliders for Music and SFX in Settings. Mute toggle. All volumes persist across sessions.

### 2. Music

Integrate the workshop ambient loop per `sound-design.md`: 72 seconds, seamless loop, D major, kalimba + fingerpicked guitar. If final music isn't ready, use a high-quality royalty-free placeholder that matches the sonic brief (warm, organic, 70-80 BPM, major key). The loop must crossfade seamlessly — the player should never notice the loop point.

### 3. Adaptive Audio

When the player is on their last 5 moves, add a subtle tension layer to the music:
- A low sustained pad fades in underneath the main loop
- Percussion gains a slightly faster rhythmic element
- SFX for fusions become slightly more reverberant (longer tail)

When the player completes a recipe target with moves remaining, the tension layer fades out and a brief relief stinger plays. This should feel organic, not jarring — the player should feel the shift subconsciously.

### 4. Tutorial System

Implement the tutorial per `docs/game-design/tutorial-flow.md` **EXACTLY**. Five guided levels:

- **Level 1 — "What is Fusing?":** 3×3 grid, 2 colors, hand-tuned layout. Teaches tap-to-fuse. Move counter hidden. Cannot fail.
- **Level 2 — "Orbs Grow":** 4×4 grid, 2 colors. Teaches that fusing near an existing orb grows it (chain fusion).
- **Level 3 — "Brew a Potion":** 5×5 grid, 3 colors. Teaches brew threshold and recipe completion.
- **Level 4 — "Tools of the Trade":** 5×5 grid, 3 colors. Introduces one booster (Shake). Tight move limit forces booster use.
- **Level 5 — "Master Brewer":** 6×6 grid, 4 colors, 2 recipe targets. Full gameplay with move limit.

Tutorial UX rules (from the spec — non-negotiable):
- **One sentence max** per prompt. No paragraphs.
- **Dim and spotlight:** Board dims to 30% brightness, target area at 100% with pulsing glow ring.
- **Finger indicator:** Translucent animated hand tapping every 1.5s until the player acts.
- **Celebrate everything:** Particles, screen shake, sound, and a brief word ("Nice!", "Great brew!") on every success.
- **No fail state in Level 1.** Move counter hidden, unlimited moves.
- **Progressive disclosure.** One concept per level. Never two new ideas at once.

### 5. Boosters

Implement all 3 boosters per `core-mechanic.md` booster section:

- **Shake (Mortar & Pestle):** Shuffles all tokens randomly. Guarantees at least one valid cluster ≥ 3 after shuffle. Does NOT cost a move. Costs 1 booster charge.
- **Catalyst Drop:** Converts all tokens of one tapped color to another tapped color. Tap booster → tap a token of the source color → tap a token of the target color → all source-color tokens transform. Costs 1 booster charge. Does NOT cost a move.
- **Extra Moves (+3):** Adds 3 moves to the move counter. Can only be used once per level attempt. Costs 1 booster charge.

Build a booster bar UI: 3 slots below or beside the board. Show remaining charges (placeholder: infinite charges for now — economy integration comes later). Tap to activate, tap board to apply (or tap booster again to cancel). Each booster gets a distinct activation animation and SFX.

### 6. UI Polish

Implement the full HUD per `art-direction.md` UI section:
- **Moves remaining:** Top-left, large and readable. Pulses red on last 5 moves.
- **Recipe vials:** Top-right, one per target. Fill animation on brew. "Complete!" stamp when full.
- **Score:** Top-center, running tally with animated increment.
- **Streak indicator:** Below score (placeholder — shows current cascade/chain count during resolution).
- **Booster bar:** Below the board, 3 slots with charge counts.

Level select screen: warm background, scrollable grid, star display per level, locked padlock icons. Results popup: stars (animated reveal), potions brewed, score, Essence earned.

All tap targets minimum **44pt** (Apple HIG guideline). Warm tones (Cauldron Amber `#E8A44A`, Warm Cream `#F5ECD7`, Hearthstone `#C2703E`). Rounded corners on all containers and buttons. Font: use the typeface specified in `art-direction.md` Typography section.

---

## Milestone Gate 3 — THE MOST IMPORTANT GATE

Reference `docs/production/milestone-gates.md`, Gate 3. The test:

> **"A non-team-member can pick up the game, complete the tutorial, play 5 levels, and WANT to play more."**

This is tested by having someone who has never seen the game play it cold, with zero instructions from you. You observe silently. If they don't naturally want to continue after 5 levels, this milestone **FAILS**.

Specific pass criteria:
- [ ] Tutorial: all 5 levels implemented, each teaches one concept
- [ ] Tutorial completion: ≥ 4/5 external testers complete without asking for help
- [ ] Continued play: ≥ 3/5 testers voluntarily continue past tutorial without prompting
- [ ] Art: all placeholder art replaced with final or near-final sprites
- [ ] Particles: all effects (fusion, brew, cascade, celebration) fire correctly
- [ ] Sound: all core SFX integrated, no missing sounds, no volume spikes
- [ ] Haptics: fusion, chain, and brew trigger appropriate feedback
- [ ] Boosters: all 3 function correctly, accessible from booster bar
- [ ] Performance: no crashes in 15 minutes of continuous play; 60fps on iPhone SE 2nd gen
- [ ] Qualitative: at least 1 tester spontaneously expresses positive reaction during chain or brew
- [ ] Music: ambient loop plays, creates cozy atmosphere, loops seamlessly
- [ ] Adaptive audio: tension layer engages in last 5 moves

---

## What NOT to Do

- No meta systems (Workshop, Potion Shelf, Streaks, Daily Brew)
- No real monetization (no ads, no IAP)
- No Firebase integration
- No analytics event wiring
- No cloud save
- No economy balancing (placeholder Essence amounts are fine)

The **only** thing that matters this phase is feel. If a fusion doesn't feel satisfying — fix it. If the brew animation doesn't make people smile — rework it. If the tutorial confuses anyone — simplify it. Everything else can wait.
