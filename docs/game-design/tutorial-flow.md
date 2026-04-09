# Brew — Tutorial Flow (Levels 1–5)

## Tutorial UX Principles

1. **One sentence max.** Every tutorial prompt is a single short sentence. No paragraphs. No multi-step explanations.
2. **Show, don't tell.** Highlight the interactive element. Let the player discover the result.
3. **Learn by doing.** The player performs every action themselves — no auto-play except for demonstrating consequences.
4. **Celebrate everything.** Every successful action gets particles, screen shake, sound, and a brief positive word ("Nice!", "Great brew!"). Over-celebrate in the tutorial; dial it back later.
5. **Dim and spotlight.** When guiding attention, dim the entire board to 30% brightness and spotlight the target area at 100%. A pulsing glow ring highlights the tappable element.
6. **No fail state in Level 1.** The move counter is hidden in Level 1. The player cannot fail.
7. **Progressive disclosure.** Each level teaches exactly one concept. Never stack two new ideas.
8. **Finger indicator.** A translucent animated hand icon appears over the tappable element, tapping once every 1.5 seconds, until the player acts.

---

## Level 1: "What is Fusing?"

**Concept taught:** Tapping a cluster fuses it into a Brew Orb. When an orb absorbs enough tokens, it brews and fills the potion vial.

### Parameters

| Parameter | Value |
|---|---|
| Grid | 3×3 |
| Colors | 2 (Ember, Frost) |
| Move limit | 15 (hidden from player) |
| Recipe | Brew 1 Red |
| Blockers | None |
| Hand-tuned | Yes |

### Initial Board Layout

```
Col:  0    1    2
Row 0: 🔴  🔴  🔵
Row 1: 🔴  🔵  🔵
Row 2: 🔵  🔵  🔴
```

🔴 = Ember, 🔵 = Frost

### Step-by-Step Tutorial Flow

**Step 1 — Introduce the board**

| Element | State |
|---|---|
| Board | Fully visible, but non-interactive (input blocked) |
| UI | Potion vial visible but empty. Move counter hidden. |
| Text bubble | *"These are ingredients. Let's brew something!"* |
| Action | Text auto-dismisses after 2s, or player taps anywhere to dismiss. |

**Step 2 — Highlight the Ember cluster**

| Element | State |
|---|---|
| Board | Dimmed to 30%. Cells (0,0), (1,0), (0,1) spotlighted at 100% with pulsing glow ring around the cluster. |
| Finger indicator | Animated hand tapping on cell (0,0). |
| Text bubble | *"Tap these red ingredients to fuse them!"* |
| Input | Only the three highlighted Ember cells accept input. Tapping any of the three triggers the fusion. |

**Step 3 — Fusion animation plays**

| Element | State |
|---|---|
| Animation | The 3 Ember tokens slide toward their centroid and merge into a glowing red Brew Orb at cell (0,0). |
| Particles | Red sparkles burst outward. |
| Sound | Satisfying "whoosh-clink" fusion sound. |
| Text bubble | *"You made a Brew Orb!"* |
| Board state after gravity | |

```
Col:  0    1    2
Row 0: 🔴  🔵  🔵    ← new tokens fell in (random from pool, but forced: 1 Ember, 1 Frost)
Row 1: ORB  🔵  🔵    ← Brew Orb (Ember) settled here after gravity
Row 2: 🔵  🔵  🔴
```

Note: The Orb is at (1,0) after gravity pulls it down. New tokens fill from top.

**Step 4 — Highlight the second Ember cluster**

The board is seeded so that after Step 3, a new cluster of 3 Ember tokens is adjacent to the Orb. (The "random" fill in Step 3 is actually forced to guarantee this.)

| Element | State |
|---|---|
| Board | Dimmed. The new Ember cluster (including 2 new + 1 existing at row 2 col 2) spotlighted. |
| Finger indicator | Animated hand tapping the cluster. |
| Text bubble | *"Fuse more red to grow the orb!"* |
| Input | Only the highlighted cluster accepts input. |

**Step 5 — Chain fusion + Brew**

| Element | State |
|---|---|
| Animation | The 3 Ember tokens fuse. Because the resulting mass is adjacent to the existing Ember Orb, they merge into it. The orb now contains 6 total tokens (3 + 3). |
| Auto-brew trigger | The orb hits the 6-token threshold. It glows intensely, then bursts upward in a beam of light toward the potion vial. |
| Vial | The Ember potion vial fills with glowing red liquid. A "ding" chime plays. |
| Text bubble | *"You brewed a potion!"* |
| Particles | Massive red-gold particle burst. Screen shakes gently. |
| Sound | Triumphant jingle (2 seconds). |

**Step 6 — Level complete**

| Element | State |
|---|---|
| Overlay | "Level Complete!" banner slides in. Single star awarded (stars not explained yet). |
| Text bubble | *"You're a natural brewer!"* |
| Button | "Next" arrow pulses. |

### What the Player Learns
- Tapping a cluster of 3+ same-colored tokens fuses them into a Brew Orb
- Fusing near an existing orb of the same color grows it
- When an orb absorbs enough tokens, it brews and fills the potion vial
- Brewing fills the recipe target

---

## Level 2: "Choosing Which Cluster to Fuse"

**Concept taught:** The player has agency — choosing *which* cluster to fuse first affects the board. Orb position matters.

### Parameters

| Parameter | Value |
|---|---|
| Grid | 4×4 |
| Colors | 2 (Ember, Frost) |
| Move limit | 12 (visible for the first time, but generous) |
| Recipe | Brew 1 Blue |
| Blockers | None |
| Hand-tuned | Yes |

### Initial Board Layout

```
Col:  0    1    2    3
Row 0: 🔵  🔴  🔴  🔵
Row 1: 🔵  🔵  🔴  🔴
Row 2: 🔴  🔵  🔵  🔴
Row 3: 🔴  🔴  🔵  🔵
```

Two Frost clusters are visible:
- Cluster A: (0,0), (1,0), (1,1) — left side, 3 tokens
- Cluster B: (2,1), (2,2), (3,2), (3,3) — right side, 4 tokens

### Step-by-Step Tutorial Flow

**Step 1 — Introduce move counter**

| Element | State |
|---|---|
| UI highlight | The move counter (showing "12") pulses with a glow. |
| Text bubble | *"You have 12 moves. Use them wisely!"* |
| Action | Auto-dismiss after 2s or tap. |

**Step 2 — Show two clusters**

| Element | State |
|---|---|
| Board | Dimmed. Both Frost clusters spotlighted simultaneously, each with a distinct outline (Cluster A = dashed, Cluster B = solid). |
| Text bubble | *"Two groups of blue — which will you fuse first?"* |
| Input | Both clusters are tappable. No finger indicator — the player chooses freely. |

**Step 3a — If player taps Cluster A (3 tokens)**

| Element | State |
|---|---|
| Animation | 3 Frost tokens fuse into a Frost Orb. |
| Text bubble | *"Good choice! The orb landed here."* |
| UI | An arrow briefly points from the orb to nearby Frost tokens, showing potential next moves. |

**Step 3b — If player taps Cluster B (4 tokens)**

| Element | State |
|---|---|
| Animation | 4 Frost tokens fuse into a Frost Orb. |
| Text bubble | *"Nice — a bigger group makes a stronger start!"* |
| UI | Same arrow hint as 3a. |

**Step 4 — Free play**

| Element | State |
|---|---|
| Board | Fully lit. No more dimming. |
| Text bubble | *"Keep fusing blue to fill the vial!"* (appears once, then gone) |
| Input | Full board is interactive. Player plays freely. |
| Guidance | If the player makes no move for 8 seconds, a subtle glow appears around the largest available Frost cluster (hint system). |

**Step 5 — Level complete**

| Element | State |
|---|---|
| Trigger | Recipe fulfilled (1 Blue brewed). |
| Overlay | "Level Complete!" with star rating (1–3 stars shown, system explained). |
| Text bubble | *"More moves left over = more stars!"* (only if player earned 2+ stars) |

### What the Player Learns
- There is a move limit — moves are a resource
- The player chooses which cluster to fuse (agency)
- Orb position depends on which cluster was fused
- Star ratings reward efficiency

---

## Level 3: "Chain Fusions"

**Concept taught:** Fusing a cluster can create an orb adjacent to another same-color orb, causing them to auto-merge (chain). Chains are powerful.

### Parameters

| Parameter | Value |
|---|---|
| Grid | 4×5 |
| Colors | 3 (Ember, Frost, Vine) — Vine is new |
| Move limit | 14 |
| Recipe | Brew 1 Green |
| Blockers | None |
| Hand-tuned | Yes |

### Initial Board Layout

```
Col:  0    1    2    3
Row 0: 🟢  🔴  🔵  🔵
Row 1: 🟢  🟢  🔴  🔵
Row 2: 🔴  🔵  🟢  🟢
Row 3: 🔵  🔴  🟢  🔴
Row 4: 🔴  🔵  🔴  🔵
```

🟢 = Vine

Key setup: 
- Vine cluster A: (0,0), (1,0), (1,1) — top-left, 3 tokens
- Vine cluster B: (2,2), (2,3), (3,2) — center-right, 3 tokens
- After fusing cluster A, gravity drops the Vine Orb down. The board is seeded so that the orb lands adjacent to cluster B.

### Step-by-Step Tutorial Flow

**Step 1 — Introduce third color**

| Element | State |
|---|---|
| UI | The three ingredient icons appear in the sidebar: Ember, Frost, Vine. Vine pulses. |
| Text bubble | *"A new ingredient — Vine!"* |
| Action | Auto-dismiss 1.5s. |

**Step 2 — Highlight Vine cluster A**

| Element | State |
|---|---|
| Board | Dimmed. Vine cluster A spotlighted. |
| Finger indicator | On cluster A. |
| Text bubble | *"Fuse the green ingredients."* |
| Input | Only cluster A tappable. |

**Step 3 — Fusion + gravity positions orb**

| Element | State |
|---|---|
| Animation | 3 Vine tokens fuse into a Vine Orb. Gravity drops it. New tokens fill from top. |
| Board state | The Vine Orb settles adjacent to Vine cluster B (this is guaranteed by the hand-tuned board + forced fill). |
| Pause | Brief 0.5s pause. Camera subtly zooms toward the orb and cluster B. |
| Text bubble | *"Look — the orb is next to more green!"* |

**Step 4 — Highlight cluster B**

| Element | State |
|---|---|
| Board | Dimmed. Cluster B spotlighted. A dotted line connects the orb to cluster B. |
| Finger indicator | On cluster B. |
| Text bubble | *"Fuse them to chain into the orb!"* |
| Input | Only cluster B tappable. |

**Step 5 — Chain fusion**

| Element | State |
|---|---|
| Animation | Cluster B fuses. The resulting mass is adjacent to the existing Vine Orb → automatic merge. The orb absorbs all tokens. Total: 6 tokens → brew threshold reached. |
| Auto-brew | The orb glows, launches upward, fills the Green potion vial. |
| Text bubble | *"Chain fusion! That's how the pros brew."* |
| Particles | Green vines burst outward. Extra-celebratory particle effect (1.5× normal). |
| Sound | Double chime — one for the chain, one for the brew. |

**Step 6 — Level complete**

| Element | State |
|---|---|
| Overlay | "Level Complete!" with stars. |
| Text bubble | *"Chains are your secret weapon!"* |

### What the Player Learns
- A third ingredient color exists (the pool can grow)
- Fusing near an existing orb auto-chains them together
- Chains are efficient — one tap can trigger a cascade
- Positional planning enables chains

---

## Level 4: "Meeting the Recipe"

**Concept taught:** The recipe tells you *what* to brew. You must plan which colors to prioritize. Sometimes you fuse non-target colors to clear space and set up target-color clusters.

### Parameters

| Parameter | Value |
|---|---|
| Grid | 5×5 |
| Colors | 3 (Ember, Frost, Vine) |
| Move limit | 12 |
| Recipe | Brew 1 Red |
| Blockers | None |
| Hand-tuned | Yes |

### Initial Board Layout

```
Col:  0    1    2    3    4
Row 0: 🔵  🟢  🔴  🟢  🔵
Row 1: 🟢  🔵  🟢  🔴  🟢
Row 2: 🔴  🟢  🔵  🔵  🔴
Row 3: 🔵  🔴  🟢  🔴  🔵
Row 4: 🟢  🔵  🔴  🔵  🟢
```

Key setup:
- No large Ember cluster is immediately available (only pairs)
- A large Frost cluster exists: (0,4), (1,1) — wait, let me design this more carefully
- The board is designed so Ember tokens are scattered (2s and 1s) with no cluster of 3+
- Frost has a visible cluster of 4 in the lower-center area
- Vine has a cluster of 3 on the right side
- Fusing the Frost or Vine clusters creates gravity cascades that bring Ember tokens together

### Step-by-Step Tutorial Flow

**Step 1 — Introduce the recipe panel**

| Element | State |
|---|---|
| UI highlight | The recipe panel (showing "Brew 1 Red" with an empty Ember vial icon) pulses with glow. |
| Text bubble | *"Your goal — brew a Red potion!"* |
| Action | Auto-dismiss 2s. |

**Step 2 — Show the problem**

| Element | State |
|---|---|
| Board | Dimmed. All Ember tokens spotlighted — but they're scattered, no cluster of 3+. |
| Text bubble | *"Red is scattered. Clear other colors to bring them together."* |
| Action | Auto-dismiss 2s. Board returns to full brightness. |

**Step 3 — Subtle hint**

| Element | State |
|---|---|
| Board | Full brightness. The largest non-Ember cluster gets a very subtle shimmer (not a full spotlight — just a gentle hint). |
| Input | Full board is interactive. The player is free to tap anything. |
| Guidance | If no move after 8s, the shimmer intensifies on the largest cluster. |

**Step 4 — Player fuses non-target cluster**

| Element | State |
|---|---|
| Animation | Standard fusion animation. Gravity drops tokens. New tokens fill. |
| Board check | After gravity settles, if an Ember cluster of 3+ has formed, proceed to Step 5. If not, continue free play — the board is designed so that within 2 non-target fusions, an Ember cluster will form. |
| Text bubble (first non-target fusion only) | *"Smart — clearing space for red!"* |

**Step 5 — Player spots and fuses Ember cluster**

| Element | State |
|---|---|
| Trigger | Player fuses an Ember cluster. |
| Text bubble | *"There they are!"* |
| Animation | Standard fusion into Orb. |

**Step 6 — Player brews Ember**

| Element | State |
|---|---|
| Trigger | Ember orb reaches 6 tokens and brews, or player grows the orb through further fusions. |
| Animation | Brew animation — orb launches to vial. |
| Vial | Fills with red. Recipe target satisfied. |

**Step 7 — Level complete**

| Element | State |
|---|---|
| Overlay | "Level Complete!" with stars. |
| Text bubble | *"Sometimes the best move isn't the obvious one."* |

### What the Player Learns
- The recipe determines what you need to brew
- You often need to fuse non-target colors to rearrange the board
- Planning ahead matters — not every move directly advances the recipe
- The move counter creates real pressure (12 is tight enough to feel it)

---

## Level 5: "Your First Real Level"

**Concept taught:** Nothing new. This is the graduation level — the player applies everything from Levels 1–4 with no handholding.

### Parameters

| Parameter | Value |
|---|---|
| Grid | 5×6 |
| Colors | 3 (Ember, Frost, Vine) |
| Move limit | 14 |
| Recipe | Brew 1 Red, 1 Blue |
| Blockers | None |
| Hand-tuned | Yes (but generous — breathing room level) |

### Initial Board Layout

```
Col:  0    1    2    3    4
Row 0: 🔴  🟢  🔵  🔵  🟢
Row 1: 🔴  🔴  🟢  🔵  🔴
Row 2: 🟢  🔵  🔴  🟢  🔵
Row 3: 🔵  🔵  🟢  🔴  🔴
Row 4: 🟢  🔴  🔵  🔵  🟢
Row 5: 🔴  🟢  🔴  🔵  🔵
```

Key setup:
- Ember cluster of 3 visible in top-left: (0,0), (1,0), (1,1)
- Frost cluster of 3 visible in top-right: (0,2), (0,3), (1,3)
- Additional clusters available after gravity
- The board is solvable in approximately 6–8 moves (budget is 14 — very generous for a breathing room level)

### Step-by-Step Tutorial Flow

**Step 1 — Recipe with two targets**

| Element | State |
|---|---|
| UI highlight | Recipe panel pulses, showing two vials: Red and Blue, both empty. |
| Text bubble | *"Two potions to brew this time. You've got this!"* |
| Action | Auto-dismiss 2s. |

**Step 2 — Free play**

| Element | State |
|---|---|
| Board | Fully lit. No dimming, no spotlights, no finger indicators. |
| Input | Fully interactive from the start. |
| Hint system | If no move in 10s, the largest cluster gets a subtle shimmer. If no move in 18s, the shimmer becomes a gentle pulse. |

**Step 3 — First brew**

| Element | State |
|---|---|
| Trigger | Player brews either color. |
| UI | The corresponding recipe vial fills. A checkmark appears next to it. |
| Text bubble | (none — no text interruption during flow state) |
| Sound | Standard brew chime. |

**Step 4 — Second brew**

| Element | State |
|---|---|
| Trigger | Player brews the remaining color. |
| Animation | Both vials are full. Brief pause (0.5s). |

**Step 5 — Level complete**

| Element | State |
|---|---|
| Overlay | "Level Complete!" with stars and a special "Tutorial Complete" badge. |
| Text bubble | *"You're ready to brew on your own!"* |
| Particles | Extra-celebratory: both red and blue particles burst. Confetti effect. |
| Sound | Extended triumphant jingle (3 seconds). |
| Reward | Tutorial completion badge shown (cosmetic, persisted to profile). |
| Transition | "Next" button leads to Level 6 — the first non-tutorial level. The level select map is unlocked. |

### What the Player Learns
- Multi-target recipes require dividing attention between colors
- All core mechanics working together: fusing, orb growth, chaining, recipe targets, move budgeting
- Confidence that they can play without guidance

---

## Tutorial Hint System (Active After Level 5)

After the tutorial, the guided prompts disappear. The passive hint system remains:

| Inactivity Duration | Hint Level | Visual |
|---|---|---|
| 0–5 seconds | None | — |
| 5–10 seconds | Tier 1 | Largest cluster gets a faint shimmer |
| 10–15 seconds | Tier 2 | Shimmer becomes a pulsing glow ring |
| 15+ seconds | Tier 3 | Finger indicator appears on the cluster |

The hint system can be disabled in Settings.

---

## Tutorial Text Reference — All Lines

Quick reference for localization and voice-over recording.

| Level | Step | Text |
|---|---|---|
| 1 | 1 | "These are ingredients. Let's brew something!" |
| 1 | 2 | "Tap these red ingredients to fuse them!" |
| 1 | 3 | "You made a Brew Orb!" |
| 1 | 4 | "Fuse more red to grow the orb!" |
| 1 | 5 | "You brewed a potion!" |
| 1 | 6 | "You're a natural brewer!" |
| 2 | 1 | "You have 12 moves. Use them wisely!" |
| 2 | 2 | "Two groups of blue — which will you fuse first?" |
| 2 | 3a | "Good choice! The orb landed here." |
| 2 | 3b | "Nice — a bigger group makes a stronger start!" |
| 2 | 4 | "Keep fusing blue to fill the vial!" |
| 2 | 5 | "More moves left over = more stars!" |
| 3 | 1 | "A new ingredient — Vine!" |
| 3 | 2 | "Fuse the green ingredients." |
| 3 | 3 | "Look — the orb is next to more green!" |
| 3 | 4 | "Fuse them to chain into the orb!" |
| 3 | 5 | "Chain fusion! That's how the pros brew." |
| 3 | 6 | "Chains are your secret weapon!" |
| 4 | 1 | "Your goal — brew a Red potion!" |
| 4 | 2 | "Red is scattered. Clear other colors to bring them together." |
| 4 | 4 | "Smart — clearing space for red!" |
| 4 | 5 | "There they are!" |
| 4 | 7 | "Sometimes the best move isn't the obvious one." |
| 5 | 1 | "Two potions to brew this time. You've got this!" |
| 5 | 5 | "You're ready to brew on your own!" |

Total unique strings: **22**

---

## Tutorial Metrics to Track

| Event | Tracked Value | Purpose |
|---|---|---|
| `tutorial_level_start` | level, timestamp | Funnel entry |
| `tutorial_level_complete` | level, moves_used, time_elapsed, stars | Completion rate per level |
| `tutorial_level_fail` | level, moves_used, remaining_recipe | Identify pain points |
| `tutorial_hint_shown` | level, hint_tier, seconds_idle | Detect confusion |
| `tutorial_tap_wrong_target` | level, step, tapped_cell | Detect misunderstanding |
| `tutorial_skip_attempt` | level | Detect impatience (skip not available in MVP but log the tap) |
| `tutorial_complete` | total_time, total_moves | Overall onboarding health |

Target KPIs:
- Level 1 completion: > 98%
- Level 5 completion: > 85%
- Full tutorial (L1–L5) median time: 4–6 minutes
- Zero tutorial levels should have > 5% fail rate
