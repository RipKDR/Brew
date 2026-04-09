# Brew — Game Design Document

**Version:** 1.0.0
**Last Updated:** 2026-04-09
**Status:** Pre-production

---

## 1. Game Overview

### 1.1 One-Line Pitch

Tap clusters to fuse ingredients into glowing orbs. Chain orbs together to brew potions. Craft every recipe before you run out of moves.

### 1.2 Vision

Brew is a mobile puzzle game that elevates the casual cluster-tap genre with a single strategic twist: **positional fusing**. Unlike blast games where tapping the largest cluster is always optimal, Brew rewards players who think about *where* their resulting orb will land and how it can chain with future orbs. The game wraps this mechanic in a warm, tactile potion-crafting theme with short sessions, satisfying spectacle, and a non-predatory monetization model.

### 1.3 Design Pillars

| Pillar | Meaning |
|---|---|
| **One More Brew** | Sessions end with a clear "almost had it" feeling that invites replay, never frustration. |
| **Position Over Size** | Strategic depth comes from *where* you fuse, not just *what* you fuse. |
| **Spectacle on Completion** | Brewing a potion is the payoff — it must feel magical every single time. |
| **Respectful Monetization** | Players pay to extend fun, never to overcome manufactured paywalls. |

### 1.4 Reference Games

- Toon Blast (cluster-tap core loop)
- Potion Punch (crafting theme)
- Triple Town (positional merging strategy)

---

## 2. Target Audience & Platform

### 2.1 Audience

- **Primary:** Casual mobile puzzle players, ages 18–45, all genders.
- **Secondary:** Midcore puzzle enthusiasts who enjoy optimization (ages 25–40).
- **Tertiary:** Younger players (13–17) drawn to the crafting/collection theme.

### 2.2 Platform

- iOS (iPhone, iPad) — minimum iOS 15.
- Android — minimum API level 26 (Android 8.0).
- Portrait orientation only.
- One-handed, single-tap input. No swipes, drags, or multi-touch required.

### 2.3 Session Length

- **Target:** 60–90 seconds per level attempt.
- **Session cadence:** 3–8 levels per sitting (5–12 minutes total).

---

## 3. Core Gameplay Loop

```
┌─────────────────────────────────────────────────┐
│                  LEVEL START                     │
│  Show recipe (e.g. 2× Red, 1× Blue potion)     │
│  Show move counter                              │
└──────────────────────┬──────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────┐
│               PLAYER SCANS BOARD                │
│  Identify clusters of 3+ same-color tokens      │
│  Plan fusion sequence for positional advantage   │
└──────────────────────┬──────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────┐
│               PLAYER TAPS CLUSTER               │
│  Tokens fuse → Brew Orb at tap position         │
│  Move counter decrements by 1                   │
└──────────────────────┬──────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────┐
│              CHAIN RESOLUTION                   │
│  If orb adjacent to same-color orb → auto-merge │
│  If merged orb ≥ 6 tokens absorbed → BREW       │
│  Brew animation → recipe vial fills             │
└──────────────────────┬──────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────┐
│           GRAVITY + REFILL + CASCADE            │
│  Tokens drop to fill gaps                       │
│  New tokens enter from top                      │
│  If new 3+ clusters form → auto-fuse (free)     │
│  Repeat until board is stable                   │
└──────────────────────┬──────────────────────────┘
                       ▼
┌─────────────────────────────────────────────────┐
│              STATE CHECK                        │
│  Recipe complete? → WIN screen                  │
│  Moves remaining? → Back to SCAN                │
│  No moves left? → FAIL screen                   │
│  No valid clusters? → Auto-shuffle (free)       │
└─────────────────────────────────────────────────┘
```

### 3.1 Win Condition

All recipe vials are filled before the move counter reaches zero.

### 3.2 Lose Condition

Move counter reaches zero with one or more recipe vials unfilled.

---

## 4. Ingredient Tokens

### 4.1 Token Types

| Token | Color | Visual Motif | Potion Name |
|---|---|---|---|
| Ember | Red (#E04040) | Flickering flame particle | Blaze Elixir |
| Frost | Blue (#4080E0) | Snowflake crystalline | Cryo Draught |
| Vine | Green (#40B040) | Leaf with tendrils | Growth Tonic |
| Sun | Yellow (#E0C020) | Radiant starburst | Solaris Brew |
| Shadow | Purple (#8040C0) | Wisp of dark smoke | Umbra Potion |

### 4.2 Token States

| State | Description | Visual |
|---|---|---|
| **Idle** | Standard token on board. | Base icon, subtle idle animation (bob/shimmer). |
| **Highlighted** | Part of a valid tappable cluster. | Faint glow pulse when finger hovers nearby (optional hint mode). |
| **Fusing** | Being consumed into an orb. | Shrink + streak toward orb center. Duration: 200ms. |
| **Dropping** | Falling due to gravity. | Vertical translate with slight bounce on land. |
| **Entering** | New token filling from top. | Fade-in + drop from above top edge. |

### 4.3 Spawn Weights

Default per-token spawn probability: uniform 20% each. Level configs can override weights to skew difficulty (e.g., 5-color levels vs. 3-color levels that exclude two types).

---

## 5. Brew Orbs

### 5.1 Definition

A Brew Orb is a special token created when a player (or cascade) fuses a cluster. It occupies one cell. It stores a `token_count` — the total number of ingredient tokens it has consumed.

### 5.2 Orb Visual Tiers

| Tier | Token Count | Visual | Size |
|---|---|---|---|
| Small | 3–4 | Dim glow, subtle bubble animation. | 1.0× cell |
| Medium | 5 | Brighter glow, swirling particles. | 1.0× cell, visual overlay extends slightly beyond cell boundary |
| Ready | 6+ | Intense glow, pulsing aura, sparkle trail. Immediately triggers brew. | 1.0× cell with full spectacle overlay |

An orb always occupies exactly one grid cell regardless of visual tier.

### 5.3 Brew Threshold

**6 tokens consumed.** When an orb's `token_count` reaches 6 or higher, it immediately triggers the Brew sequence.

### 5.4 Brew Sequence

1. Orb plays "brew burst" animation (expanding ring of particles, 600ms).
2. Orb transforms into a completed potion bottle icon for 400ms.
3. Potion bottle flies to the corresponding recipe vial on the HUD (arc trajectory, 500ms).
4. Recipe vial fills with liquid animation.
5. Orb's cell becomes empty. Gravity takes effect.

Total brew animation: ~1500ms. Player input is blocked during brew animation.

### 5.5 Non-Target Brews

Any color can fuse and brew regardless of the current recipe. If a brew completes for a color not in the recipe, the potion still animates (smaller celebration), clears the cell, awards score — but does not fill any recipe vial. This is strategically useful for clearing board space.

---

## 6. Fusion & Chaining Rules

### 6.1 Cluster Fusion (Player-Initiated)

1. Player taps a token that belongs to a cluster of 3 or more same-colored, orthogonally adjacent tokens.
2. All tokens in that cluster are consumed.
3. A Brew Orb of that color appears at the **tapped cell's position**.
4. The orb's `token_count` = number of tokens consumed.
5. Move counter decrements by 1.

If the tapped token is not part of a valid cluster (fewer than 3 connected), the tap is rejected with a subtle shake animation. No move is consumed.

### 6.2 Chain Fusion (Auto-Triggered)

After any orb is created or relocated (via gravity), the game checks adjacency:

1. If a Brew Orb is orthogonally adjacent to one or more same-colored Brew Orbs, they **auto-merge**.
2. The surviving orb occupies the cell of the **lowest, then leftmost** orb in the chain (deterministic).
3. The surviving orb's `token_count` = sum of all merged orbs' `token_count` values.
4. If `token_count` ≥ 6, the orb immediately brews.
5. Chain fusion does **not** consume a move.

Chain fusion cascades: if the merge causes the surviving orb to become adjacent to yet another same-colored orb, the process repeats before gravity runs.

### 6.3 Cascade Fusion (Auto-Triggered, Post-Gravity)

After gravity + refill settles the board:

1. Scan the board for any cluster of 3+ same-colored **tokens** (not orbs) that are orthogonally adjacent.
2. If found, auto-fuse each cluster into an orb. Orb placement: the cell of the **bottom-most, then left-most** token in the cluster.
3. Check for chain fusion on newly created orbs.
4. Run gravity + refill again.
5. Repeat until the board is stable (no clusters of 3+ remain).

Cascade fusions do **not** consume moves. They are "free" bonus actions.

---

## 7. Board Behavior

### 7.1 Grid

- Default size: 7 columns × 9 rows (portrait).
- Level configs may specify different dimensions (minimum 5×5, maximum 9×11).
- Coordinate system: column 0 at left, row 0 at top. Gravity pulls tokens from lower row indices toward higher row indices (top to bottom on screen).

### 7.2 Gravity

When a cell becomes empty (token fused, orb brewed):

1. All tokens and orbs **above** the empty cell in the same column drop down to fill the gap.
2. Drop happens simultaneously across all affected columns.
3. Drop animation: 50ms per row of travel, with a 30ms bounce on landing.

### 7.3 Refill

After gravity completes:

1. Empty cells at the top of each column are filled with new random tokens.
2. New tokens use the level's spawn weight table.
3. New tokens animate in: drop from one row above the visible grid with the same timing as gravity.

### 7.4 Settle Detection

The board is "settled" when:

- All gravity drops are complete (no token has empty space below it).
- All refill tokens have landed.
- No cascade fusions are pending.

Only after settling does the game check for deadlock or return control to the player.

### 7.5 Deadlock (No Valid Moves)

If the settled board has zero clusters of 3+ same-colored tokens AND the player has moves remaining:

1. Display "Reshuffling..." overlay (500ms).
2. Randomly rearrange all tokens and orbs on the board.
3. Re-check for valid clusters. If still deadlocked, reshuffle again (max 10 attempts).
4. If 10 reshuffles fail to produce a valid cluster, regenerate the entire board (excluding existing orbs, which keep their positions).
5. Reshuffle does **not** consume a move.

---

## 8. Level Structure

### 8.1 Level Definition

Each level is defined by a config containing:

| Field | Type | Description |
|---|---|---|
| `level_id` | int | Unique identifier. |
| `grid_width` | int | Number of columns (5–9). |
| `grid_height` | int | Number of rows (5–11). |
| `move_limit` | int | Maximum player-initiated fusions allowed. |
| `recipe` | list | Required brews, e.g. `[{color: "red", count: 2}, {color: "blue", count: 1}]`. |
| `token_types` | list | Which of the 5 token types appear (3–5 types). |
| `spawn_weights` | dict | Per-token spawn probability overrides. |
| `boosters_allowed` | list | Which boosters can be used. |
| `par_moves` | int | Number of moves for 3-star rating. |

### 8.2 Difficulty Curve

| World | Levels | Colors | Grid | Moves | Recipe Complexity |
|---|---|---|---|---|---|
| 1 (Tutorial) | 1–15 | 3 | 7×7 | 20–25 | 1 potion, single color |
| 2 | 16–40 | 4 | 7×8 | 18–22 | 1–2 potions, 1–2 colors |
| 3 | 41–70 | 4 | 7×9 | 15–20 | 2 potions, 2 colors |
| 4 | 71–100 | 5 | 7×9 | 12–18 | 2–3 potions, 2–3 colors |
| 5+ | 101+ | 5 | 7×9 (varied) | 10–15 | 3 potions, 2–3 colors |

### 8.3 Star Rating

| Stars | Condition |
|---|---|
| 1 ★ | Recipe completed (any remaining moves). |
| 2 ★★ | Recipe completed with ≥ 30% of moves remaining. |
| 3 ★★★ | Recipe completed with ≥ 60% of moves remaining (at or under `par_moves`). |

Stars unlock cosmetic rewards and are tracked on the level-select map.

### 8.4 Tutorial Sequence

| Level | Teaches |
|---|---|
| 1 | Tap a highlighted cluster. "Tap these red ingredients!" |
| 2 | Fuse a second cluster to create a chain. "Now fuse these to make them meet!" |
| 3 | Chain two orbs to trigger a brew. "Watch them combine and brew!" |
| 4 | Introduce move counter. "Plan carefully — you have limited moves." |
| 5 | Introduce 2-potion recipe. "Brew both colors to win!" |
| 6–10 | Guided play with shrinking hint frequency. |
| 11–15 | Unguided play, 3-color boards. |

---

## 9. Fail States & Recovery

### 9.1 Standard Fail

Move counter reaches zero. Recipe is incomplete.

**Fail screen flow:**

1. Board darkens. Unfilled vials pulse.
2. If the player was within **2 fusions** of completing the recipe (near-miss): show "+3 Moves" offer.
3. Otherwise: show "Try Again" and "Quit" options.

### 9.2 Near-Miss Recovery

Near-miss is determined by a server-validated heuristic: the player's board state at fail time could plausibly complete the recipe in ≤ 3 additional moves. This is **not** manufactured — the game does not inflate difficulty to force this state.

**+3 Moves offer:**

- **Option A — Rewarded Ad:** Watch a 15–30s video ad to receive +3 moves. Available once per level attempt.
- **Option B — Gem Purchase:** Spend 20 Gems for +3 moves. No limit.
- **Option C — Decline:** Return to level-select. No penalty.

### 9.3 Retry

Retrying a level costs no currency. Move limit and recipe reset. Board is regenerated.

---

## 10. Boosters

Three boosters available at MVP. Boosters are consumed on use and do not regenerate for free.

### 10.1 Shake

- **Effect:** Randomly rearrange all tokens on the board. Orbs stay in place.
- **Use timing:** Before making a move (pre-move booster).
- **Cost:** 1 Shake item (earned or purchased).
- **Move cost:** Does not consume a move.
- **Guarantee:** Post-shuffle board has at least one valid 3+ cluster.

### 10.2 Catalyst

- **Effect:** Player taps any single token. The largest cluster containing that token auto-fuses into an orb, regardless of whether the cluster meets the minimum size. Minimum cluster size for Catalyst: 1 (single token becomes an orb with `token_count` = 1).
- **Use timing:** Instead of a normal move (replaces one move).
- **Cost:** 1 Catalyst item.
- **Move cost:** Consumes 1 move.
- **Note:** The resulting orb follows all normal chain-fusion and cascade rules.

### 10.3 Extra Moves

- **Effect:** Add +5 to the move counter.
- **Use timing:** Can be activated at any point during the level, including at the fail screen.
- **Cost:** 1 Extra Moves item.
- **Limit:** Once per level attempt.

### 10.4 Booster Acquisition

| Source | Details |
|---|---|
| Level rewards | 1 random booster every 5 levels (first clear only). |
| Workshop milestones | Specific upgrades grant booster packs. |
| IAP | Booster packs available in the shop. |
| Rewarded ad | "Free Booster Trial": use one booster for free this level (once per day). |

---

## 11. Scoring

### 11.1 Base Score

| Action | Points |
|---|---|
| Token fused into orb | 10 per token |
| Brew completed (target color) | 200 |
| Brew completed (non-target color) | 100 |
| Unused moves at win | 50 per remaining move |

### 11.2 Multipliers

| Multiplier | Condition | Value |
|---|---|---|
| Chain bonus | Each chain-fusion step in a single resolution | ×0.5 added per step (step 1 = 1.5×, step 2 = 2.0×, etc.) |
| Cascade bonus | Each cascade auto-fuse wave | ×0.5 added per wave (wave 1 = 1.5×, wave 2 = 2.0×, etc.) |
| Multi-brew | Multiple brews triggered in a single move's resolution | +100 bonus per additional brew |

Multipliers apply to the tokens-fused points for that resolution sequence. Brew bonuses and unused-move bonuses are flat (not multiplied).

### 11.3 Score Display

- Running score in the HUD (top-right).
- Pop-up point values float from fusion locations.
- End-of-level summary shows breakdown.

---

## 12. Economy

### 12.1 Currencies

| Currency | Name | Icon | Acquisition | Usage |
|---|---|---|---|---|
| Soft | Essence | Blue droplet | Every level completion (win or lose, scaled to performance) | Workshop upgrades, minor cosmetics |
| Premium | Gems | Faceted purple gem | IAP, rare level drops (every ~20 levels), daily login streak | Boosters, +3 moves, premium cosmetics |

### 12.2 Essence Earn Rate

| Condition | Essence Earned |
|---|---|
| Level failed | 5 |
| Level completed, 1 star | 15 + (2 × level_number) |
| Level completed, 2 stars | 20 + (3 × level_number) |
| Level completed, 3 stars | 30 + (4 × level_number) |
| Daily Brew completed | 50 bonus |

### 12.3 Gem Earn Rate (Free)

| Source | Amount | Frequency |
|---|---|---|
| Every 20th level first clear | 10 | One-time |
| Daily login day 7 | 15 | Weekly |
| Win streak milestone (5, 10) | 5, 10 | Repeating |

---

## 13. Monetization

### 13.1 Rewarded Ads

| Placement | Trigger | Reward | Frequency Cap |
|---|---|---|---|
| +3 Moves | Near-miss fail screen | 3 additional moves | 1 per attempt |
| 2× Potion Reward | Win screen | Double Essence for that level | 1 per level |
| Free Booster Trial | Pre-level booster selection | Use 1 booster without consuming inventory | 1 per day |

### 13.2 In-App Purchases

| Product | Price (USD) | Contents |
|---|---|---|
| Starter Bundle | $1.99 | 100 Gems + 3 of each booster (one-time) |
| Small Gem Pack | $2.99 | 80 Gems |
| Medium Gem Pack | $9.99 | 300 Gems |
| Large Gem Pack | $19.99 | 700 Gems |
| No-Ads Pass | $4.99 | Remove all interstitials (rewarded ads remain opt-in) |
| Booster Pack | $2.99 | 5 of each booster |

### 13.3 Non-Predatory Principles

- Near-miss detection is honest: the heuristic only triggers when the board state genuinely supports completion in ≤ 3 moves.
- No energy/lives system. Players can retry indefinitely.
- No loot boxes. All purchases have deterministic contents.
- No pay-to-win: boosters provide convenience, not exclusive power. All levels are completable without spending.
- No interstitial ads between every level. Maximum: 1 interstitial per 3 completed levels (removed by No-Ads Pass).

---

## 14. Meta Systems

Detailed specifications for meta systems are maintained in separate documents. This section provides an overview.

### 14.1 Potion Shelf

A visual collection screen displaying all potion types the player has brewed. Each potion has a shelf slot showing:

- Potion icon and name.
- Total times brewed (counter).
- "First Brewed" date stamp.
- Visual upgrade at milestones (10, 50, 100 brews): shelf decoration evolves.

### 14.2 Workshop

A single-room environment the player progressively restores and decorates.

- **States:** 10–15 upgrade tiers, each costing Essence.
- **Progression:** Linear unlock path. Each tier adds/restores a visual element (cauldron, shelves, window, garden view, etc.).
- **Function:** Purely cosmetic and motivational. Some tiers grant one-time booster rewards.
- **Interaction:** Tap to upgrade when currency is sufficient. No mini-games.

### 14.3 Daily Brew

- One special recipe per day, available for 24 hours (reset at 00:00 UTC).
- Uses the same core mechanic with a unique ingredient color palette or modified grid.
- Reward: 50 bonus Essence + 1 random booster.
- Missing a day has no penalty — no streak requirement for Daily Brew.

### 14.4 Win Streak

- Tracks consecutive level completions (any level, any star count).
- **Multiplier tiers:** 3 wins = 1.2× Essence, 5 wins = 1.5× Essence, 10 wins = 2.0× Essence.
- Streak resets on fail.
- **Streak Protection:** On fail, offer to preserve streak via rewarded ad (once per day).

### 14.5 Weekly Event

- Runs Monday 00:00 UTC through Sunday 23:59 UTC.
- Themed ingredient set with unique visual skins (e.g., "Lunar Ingredients" with moon motifs).
- 10 event-specific levels with unique recipes.
- Reward track: Essence and Gems at completion milestones.
- Template-driven: new events are data configs, not new code.

---

## 15. Session Flow

### 15.1 App Launch

1. Splash screen (studio logo, 1.5s).
2. Load player profile from local cache (immediate) + sync with server (background).
3. Home screen: Workshop room with overlaid buttons.

### 15.2 Home Screen

| Element | Action |
|---|---|
| **Play button** | Opens level-select map. |
| **Daily Brew badge** | Opens Daily Brew level (if uncompleted today). |
| **Potion Shelf button** | Opens collection screen. |
| **Shop button** | Opens IAP/booster shop. |
| **Settings gear** | Opens settings (sound, notifications, account, language). |
| **Event banner** | Opens weekly event (when active). |

### 15.3 Level Select

- Scrollable map with numbered level nodes.
- Current level pulses. Cleared levels show star count.
- Tap a node to see level preview (recipe, move limit, best score).
- "Play" starts the level. Pre-level booster selection screen appears first.

### 15.4 Pre-Level

- Show recipe vials and move count.
- Booster slots (up to 3). Tap to equip from inventory.
- "Free Booster Trial" ad button (if available).
- "Start" button.

### 15.5 Gameplay

- Board fills with drop animation (500ms).
- HUD: recipe vials (left), move counter (top-center), score (top-right), pause button (top-left).
- Player interacts per core loop (Section 3).

### 15.6 Win Screen

1. Celebration animation (particles, potion bottles popping).
2. Star rating reveal (1–3 stars, sequential).
3. Score breakdown.
4. Essence earned (with optional 2× ad button).
5. "Next Level" and "Home" buttons.

### 15.7 Fail Screen

1. Board darkens.
2. Recipe progress shown (e.g., "1/2 Red, 0/1 Blue").
3. Near-miss offer (if applicable).
4. "Retry" and "Quit" buttons.

---

## 16. Audio Design

### 16.1 Music

- **Home/Workshop:** Warm ambient track, acoustic/organic instruments. Loopable, low-energy.
- **Gameplay:** Upbeat but not frantic. Light percussion, melodic layers. Tempo increases subtly when ≤ 3 moves remain.
- **Events:** Themed variation on gameplay track.

### 16.2 Sound Effects

| Event | Sound Profile |
|---|---|
| Token tap (valid cluster) | Soft "clink" with pitch variation by cluster size. |
| Token tap (invalid) | Dull thud / muted buzz. |
| Fusion | Ascending chime sequence. Pitch rises with cluster size. |
| Chain fusion | Deeper resonant merge sound, layered overtones. |
| Brew trigger | Bubbling + satisfying "pop" + sparkle. Brew sound varies by potion color. |
| Gravity drop | Soft tumbling / patter. |
| Win | Celebratory jingle, 3 seconds. |
| Fail | Descending gentle tones. Not punishing. |

### 16.3 Haptics (iOS)

- Token tap: light impact.
- Fusion: medium impact.
- Brew: heavy impact + success notification.
- Fail: soft notification.

---

## 17. Visual Style

### 17.1 Art Direction

- **Style:** Stylized 2D, hand-painted textures with soft gradients. Not flat, not hyper-realistic.
- **Palette:** Warm, saturated. Dark wood/stone backgrounds contrast with vibrant tokens and glowing orbs.
- **Tokens:** Rounded shapes with inner glow. Distinct silhouettes for colorblind accessibility (see Section 18).
- **Orbs:** Same shape language as tokens but with particle auras, inner light, and subtle animation.
- **UI:** Parchment/leather textures for panels. Warm gold accents.

### 17.2 Animation Priorities

1. Brew completion (highest fidelity, most particles).
2. Chain fusion (satisfying merge, secondary particles).
3. Cascade reactions (rapid, energetic, screen shake option).
4. Gravity drops (physics-feeling, slight squash on land).
5. Idle states (minimal, ambient — save GPU).

---

## 18. Accessibility

### 18.1 Colorblind Support

- All 5 token types have **distinct silhouette shapes** in addition to color:
  - Ember: Flame / triangle.
  - Frost: Snowflake / hexagon.
  - Vine: Leaf / organic curve.
  - Sun: Star / radiating points.
  - Shadow: Crescent / wisp.
- Optional "high-contrast symbols" mode overlays geometric symbols (circle, square, triangle, diamond, star) on tokens.

### 18.2 Motor Accessibility

- One-handed, one-tap play (no drag, swipe, or multi-touch).
- Minimum tap target: 44×44 points (Apple HIG).
- No time pressure during gameplay (move-limited, not time-limited).
- Optional "confirm tap" mode: first tap highlights cluster, second tap confirms fusion.

### 18.3 Visual Accessibility

- Minimum text size: 14pt (scalable with system font size).
- High-contrast mode option for UI elements.
- Reduce-motion mode: disables particle effects and replaces with simple fade transitions.
- Screen reader support for menus and HUD elements (VoiceOver / TalkBack).

### 18.4 Cognitive Accessibility

- Tutorial pacing is slow with skip option.
- Optional hint system: after 10 seconds of inactivity, a valid cluster gently pulses.
- Undo last move option (costs 1 Gem, available in settings — not default-on to preserve challenge).

---

## 19. Localization

### 19.1 Supported Languages (MVP)

English (en), Spanish (es), French (fr), German (de), Portuguese (pt-BR), Japanese (ja), Korean (ko), Simplified Chinese (zh-Hans).

### 19.2 Localization Guidelines

- All UI strings externalized to localization tables. No hardcoded text.
- Ingredient and potion names are localized (not transliterated).
- Number formatting respects locale (decimal separators, digit grouping).
- Date/time formatting respects locale (Daily Brew timer, event schedule).
- Text containers support 40% expansion (German/French tend to be longer than English).
- Right-to-left (RTL) layout support is deferred post-MVP but UI should not hard-code directional assumptions.
- Currency symbols use locale-appropriate formatting (IAP prices from store API, not hardcoded).
- Tutorial overlay text must fit within speech-bubble UI at all supported languages — QA required per locale.

### 19.3 Cultural Sensitivity

- "Brew/Potion" theming is fantasy-generic and avoids real-world cultural, religious, or occult references.
- Token colors and shapes avoid national flag or political symbolism.
- Character art (if added post-MVP) should represent diverse ethnicities and body types.

---

## 20. Technical Considerations

### 20.1 Target Performance

- 60 FPS on mid-range devices (2022 and newer).
- 30 FPS minimum on low-end devices.
- Cold start to home screen: ≤ 3 seconds.
- Level load: ≤ 1 second.

### 20.2 Data Sync

- Player progress synced to server on level completion and app foreground/background transitions.
- Offline play supported: queue results, sync on reconnect.
- Conflict resolution: server wins for currency, client wins for level progress (player keeps the better star rating).

### 20.3 Analytics Events (MVP)

| Event | Data |
|---|---|
| `level_start` | level_id, boosters_equipped |
| `level_complete` | level_id, stars, moves_used, score, brews_by_color |
| `level_fail` | level_id, moves_used, recipe_progress, near_miss_offered |
| `near_miss_response` | level_id, choice (ad / gem / decline) |
| `booster_used` | booster_type, level_id, source (inventory / ad_trial) |
| `iap_purchase` | product_id, price, currency |
| `ad_watched` | placement, level_id, completed |
| `session_start` | timestamp, platform, app_version |
| `session_end` | timestamp, levels_played, total_duration |

---

*End of Game Design Document.*
