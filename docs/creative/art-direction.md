# Brew — Art Direction Guide

## Visual Style References

1. **Ni no Kuni (Level-5 / Studio Ghibli)** — Hand-painted warmth, saturated but never garish. The gold-hour lighting on villages and interiors is the exact emotional register we want for the workshop.
2. **Stardew Valley** — Pixel warmth translated to feeling: cozy clutter, objects that tell a story, a space the player inhabits rather than just looks at.
3. **Spirited Away — Zeniba's cottage scenes** — Firelight, hanging herbs, wooden textures, steam from a kettle. Domestic magic.
4. **Hades (Supergiant)** — Not the tone, but the craft: layered parallax backgrounds, per-element lighting, VFX that feel hand-drawn. Our brew animation should hit this production bar.
5. **Luma Fusion / Alto's Odyssey** — Soft gradients, atmospheric depth through color temperature shift (warm foreground → cool distant background).
6. **Katamari Damacy** — Joyful absurdity in object design. Tokens should have personality without being literal. A fire essence doesn't need to be a flame; it can be a warm, pulsing teardrop.
7. **Heikala (illustrator)** — Ink + watercolor aesthetic, whimsical subjects, warm limited palettes. Key reference for promotional illustration and loading screens.
8. **Cozy Grove** — Painterly textures on 3D forms, muted-but-warm palette, the feeling of unlocking beauty through play.

---

## Color Palette

### Primary Colors

| Name          | Hex       | Usage                                      |
|---------------|-----------|---------------------------------------------|
| Cauldron Amber| `#E8A44A` | Primary brand color, buttons, highlights    |
| Warm Cream    | `#F5ECD7` | Backgrounds, card fills, text fields        |
| Hearthstone   | `#C2703E` | Secondary accent, borders, active states    |
| Soft Plum     | `#8E6BA1` | Tertiary accent, premium features, Shadow ingredient |

### Ingredient Colors

| Ingredient | Primary    | Highlight  | Shadow     |
|------------|-----------|------------|------------|
| Ember      | `#E05A3A` | `#F4A261`  | `#9B2D1F`  |
| Frost      | `#6FB8D9` | `#B5E2F0`  | `#3A7CA5`  |
| Vine       | `#6DAF5E` | `#A8D89A`  | `#3B6E30`  |
| Sun        | `#F2C745` | `#FBE89A`  | `#C49B1A`  |
| Shadow     | `#6B4E9B` | `#A98ED4`  | `#3D2A63`  |

### Background Tones

| Name              | Hex       | Usage                                   |
|-------------------|-----------|-----------------------------------------|
| Workshop Walnut   | `#3E2C1E` | Deep background behind the board        |
| Shelf Oak         | `#5C3D2E` | Wooden surfaces, shelf panels           |
| Stone Hearth      | `#7A6B5D` | Stone elements, inactive areas          |
| Night Nook        | `#2A1F2D` | Darkest shadow areas, vignette edges    |

### UI Accents

| Name          | Hex       | Usage                                       |
|---------------|-----------|----------------------------------------------|
| Gold Coin     | `#D4A843` | Currency, rewards, premium UI elements       |
| Parchment     | `#F0E4C8` | Modals, tooltips, text containers            |
| Ink           | `#2E2019` | Body text on light backgrounds               |
| Cloud         | `#FAF6EE` | Light text on dark backgrounds               |
| Alert Berry   | `#D94E5C` | Error states, urgency (use sparingly)        |
| Success Moss  | `#5EA35E` | Completion confirmations, positive feedback  |

---

## Typography

### UI Text — "Nunito" family

- **Nunito Bold** — Headers, button labels, move counter
- **Nunito SemiBold** — Subheaders, recipe names, ingredient labels
- **Nunito Regular** — Body text, descriptions, tooltips
- Fallback stack: `Nunito, "Rounded Mplus 1c", system-ui, sans-serif`

Rationale: Rounded terminals, generous x-height, excellent legibility at small sizes, full Unicode/Latin Extended coverage for global launch.

### Display / Logo — "Playfair Display" (modified)

- Used exclusively for the "Brew" wordmark and chapter titles
- Slightly swashed serifs on B and w, custom ligature on "ew"
- Weight: Bold, with a subtle outer glow in Cauldron Amber at 15% opacity
- Not used in gameplay UI — reserved for brand moments

### Numerals — "Nunito Bold" tabular figures

- Move counter, scores, currency amounts
- Tabular (monospaced) lining figures to prevent layout jitter during count animations

---

## Ingredient Token Design

### Shape Language

Each token is a **rounded, gem-like shape** — roughly 56×56 pt at 1× scale. Not a perfect circle; slightly organic, like a polished river stone. Each type has a unique silhouette modifier:

| Ingredient | Base Shape             | Silhouette Detail              |
|------------|------------------------|--------------------------------|
| Ember      | Teardrop, point up     | Subtle inner flicker texture   |
| Frost      | Hexagonal, soft edges  | Crystalline facet lines        |
| Vine       | Rounded diamond        | Tiny leaf bump on one edge     |
| Sun        | Circle with soft rays  | Four subtle ray nubs           |
| Shadow     | Oval, slightly tapered | Wisp tendril on the bottom     |

### Texture

- NOT flat: each token has a subtle radial gradient (lighter center, darker rim)
- A faint inner noise texture (~3% opacity) gives a tactile, slightly mineral feel
- Ingredient color applied as the dominant fill; highlight color catches a top-left "light source" reflection

### Animation States

**Idle Shimmer**
- Slow, continuous 3-second loop
- Highlight reflection drifts ~4px across the surface diagonally
- Opacity oscillates between 80% and 100% on the highlight
- Tokens breathe: scale oscillates ±1% on a staggered sine wave so the board feels alive

**Selected Pulse**
- Triggered on tap of a valid cluster
- Scale pops to 108% then settles at 105% (ease-out-back, 150ms)
- A ring of the ingredient's highlight color expands outward and fades (ripple, 300ms)
- All tokens in the cluster pulse simultaneously; border highlight appears (2pt, highlight color, 60% opacity)

**Fusing Rush**
- Tokens accelerate toward the cluster centroid
- Each token stretches ~10% along its travel vector (squash-and-stretch)
- Trail: 3-frame ghost trail in the token's highlight color at decreasing opacity (60% → 30% → 10%)
- Arrival: tokens overlap at center, flash white for 1 frame, then resolve into the orb

**Orb Glow Tiers** — see next section

---

## Brew Orb Visual Progression

Orbs replace the cluster of fused tokens at the centroid position. They occupy a single grid cell but visually overflow by ~20%.

### Tier 1 — Small Orb (3 tokens fused)

- Size: 64×64 pt (slightly larger than a token)
- Shape: Perfect circle with a soft, liquid surface distortion (shader or sprite animation — gentle wobble, 2-second loop)
- Color: Dominant ingredient color at 70% saturation, lighter than the source tokens
- Glow: Faint outer glow, 4px radius, ingredient highlight color at 20% opacity
- Inner detail: a single bright caustic highlight drifting slowly across the surface
- Particle: 1-2 tiny sparkle particles orbit lazily

### Tier 2 — Medium Orb (4–5 tokens fused)

- Size: 72×72 pt
- Surface distortion increases: the liquid wobble is more visible, surface looks thicker
- Color: saturation rises to 85%, a secondary swirl of the highlight color becomes visible inside the orb (like oil in water)
- Glow: outer glow expands to 8px, opacity increases to 35%
- Inner detail: 2-3 caustic highlights, moving faster
- Particle: 3-4 sparkle particles, orbit tightens
- A subtle "hum" ring appears at the base (thin arc of light)

### Tier 3 — Large Orb / Brew-Ready (6+ tokens)

- Size: 80×80 pt, visually dominant on the board
- Surface: active, swirling liquid with visible currents. The orb looks like it's about to overflow.
- Color: fully saturated ingredient color with veins of the highlight color pulsing through
- Glow: outer glow at 12px, 50% opacity, glow pulses (brightens/dims on a 1-second cycle)
- Inner detail: bright core develops — a white-gold point of light at the center, reminiscent of a star
- Particle: 6-8 sparkles in a tight orbit; occasional larger sparkle escapes upward
- Hum ring becomes a full circle, brighter, slowly rotating
- **Brew-ready indicator**: a subtle upward steam wisp rises from the orb (2-3 translucent plumes)

---

## Brew Animation Sequence

Duration: 1.8 seconds total. This is the signature moment of the game — it must feel spectacular and rewarding.

### Phase 1 — Ignition (0–300ms)

- The orb contracts sharply to 60% size (anticipation squeeze)
- Screen dims slightly around the orb (radial vignette, 20% darken)
- All other board tokens pause their idle shimmer
- A low rumble haptic fires
- Sound: rising sub-bass hum

### Phase 2 — Eruption (300–700ms)

- Orb expands rapidly to 150% with a bright white flash at center
- The orb's liquid surface explodes outward as a ring of ingredient-colored particles
- 20-30 particle sparks fly in a starburst pattern, trailing light
- The orb's shape distorts — briefly amoebic — then snaps into the silhouette of a potion vial
- Sound: bubbling crescendo hits its peak → pop

### Phase 3 — Formation (700–1200ms)

- The potion vial shape solidifies: glass outline appears, drawn in a quick flourish (animated stroke, gold)
- Liquid fills the vial from bottom to top — the liquid is the ingredient's primary color with bubbles rising through it
- Cork materializes at the top with a soft bounce
- A label unfurls on the vial (the potion name fades in)
- Sound: liquid pour, glass crystallization tone, label paper flutter

### Phase 4 — Celebration (1200–1800ms)

- The completed potion vial does a small float-up and gentle rotation (±5°)
- A circular burst of golden sparkles radiates outward
- Tiny stars rain down in the background
- The vignette releases; board tokens resume their shimmer
- The potion miniaturizes and flies in an arc toward the potion shelf
- Sound: magical sparkle flourish → satisfying "ding" as potion lands on shelf

---

## Board Visual Design

### Background

The board floats in front of the workshop scene. It sits on a **worn wooden table surface** — visible grain, slightly lighter in the center where use has worn the finish. The table edge is visible at the bottom of the board area, curving slightly to suggest a round worktable.

### Grid

- No visible grid lines. Token positions are defined by spacing alone.
- Tokens sit 4pt apart. The gap is filled by the table surface showing through.
- A very faint circular shadow (~5% opacity, 2px blur) sits under each token, grounding it on the table.

### Empty Spaces During Cascade

When tokens are removed and new ones fall in:
- The empty cell shows the bare table surface
- A faint dust-mote particle effect plays briefly where the token was removed (2-3 motes, 400ms fade)
- New tokens fall from above with a slight bounce (ease-out-bounce, 250ms). They cast a momentary downward shadow during the fall.

### Board Boundaries

- The board area has no hard border. Instead, the table surface gradually darkens toward the edges (radial gradient) and the workshop scene elements (shelves, cauldron) frame it naturally.
- The playable area is inset from screen edges by at least 16pt on each side.

---

## Workshop Scene

The workshop is the persistent background and meta-progression canvas. It is rendered in a **2.5D parallax style** — 3-4 depth layers that shift subtly with device tilt or scroll.

### Room Description

A circular stone-and-timber room, viewed from a slightly elevated angle (about 30° above horizontal). The room feels underground or in a hillside — a hobbit-hole workshop.

**Back wall (deepest layer)**
- Rough stone with patches of warm plaster
- A round window at upper-left, letting in golden sunlight that casts a shaft of volumetric light across the room
- Wooden shelves line the wall: the potion shelf lives here

**Mid layer**
- The worktable (where the game board sits) is center-frame
- To the left: a cauldron over a low fire, perpetually bubbling, with lazy steam rising
- To the right: a tall apothecary cabinet with many small drawers (unlockable via progression)
- Hanging herbs and dried flowers dangle from ceiling beams

**Foreground layer**
- The bottom of the screen shows the front edge of the worktable
- A mortar and pestle sits to the lower-left corner (decorative, interactive Easter egg — tap to make it wobble)
- A sleeping cat curls near the fire (unlockable companion, animates: breathing loop, occasional ear twitch, stretches when a potion is brewed)

### Upgrade States

| Level  | Reveals                                                         |
|--------|------------------------------------------------------------------|
| Start  | Bare room: empty shelves, cold hearth, dusty table              |
| 5      | Fire lit in hearth, basic cauldron appears, one shelf cleaned    |
| 15     | Herbs hanging, second shelf active, window lets light in         |
| 30     | Apothecary cabinet appears, rug on floor, ambient particles     |
| 50     | Cat companion arrives, crystal decorations, full lighting        |
| 75     | Enchanted ceiling (stars/fireflies), golden trim on furniture    |
| 100    | Workshop at full splendor: every surface detailed, ambient magic |

### Lighting

- Primary: warm point light from the hearth (lower left), casting long shadows to the upper right
- Secondary: cool daylight from the round window (upper left), providing contrast and depth
- Tertiary: subtle glow from potion shelf (ingredient-colored, varies with collection)
- Ambient: a low-level warm fill so no area is fully black

### Ambient Details

- Dust motes drift in the window light shaft (slow, random, 10-15 particles)
- Steam from the cauldron (continuous, 3 plume sprites cycling)
- Fire flicker: the hearth light subtly pulses (±5% brightness, randomized 0.5–1.5s intervals)
- Occasional sparkle on potion vials on the shelf (random, every 3-8 seconds)

---

## Potion Shelf

The shelf is mounted on the back wall, visible in the workshop scene and accessible as a full-screen collection view.

### Layout

- 3 rows of 8 slots = 24 visible slots (scrollable for larger collections)
- Each slot is a shallow wooden niche with a faint recessed shadow

### Empty Slot

- Dark wood interior, slightly darker than the shelf surface
- A faint dotted-line silhouette of a potion vial in 15% opacity Cloud color
- No glow, no interaction — just a quiet invitation

### Filled Slot

- The potion vial sits centered in the niche
- Liquid inside the vial is the potion's color — it sloshes subtly on tilt/scroll (gyro-reactive on supported devices, otherwise a slow idle animation)
- A soft glow emanates from the vial: color-matched, 6px radius, 25% opacity
- The potion's name appears below the slot in Nunito SemiBold, 11pt
- Tap opens the potion detail card (recipe, lore, brew count)

### Newly Brewed

- For 24 hours after first brewing, a "New!" badge (Cauldron Amber, pill shape) sits on the upper-right corner of the slot
- The glow intensity is doubled for the first session after brewing

### Complete Row

- When a full row of 8 potions is collected, the shelf niche gains a golden inlay trim
- A tiny trophy icon appears at the row's left edge

---

## UI Design Principles

1. **Minimal chrome.** The board is the star. UI elements stay at the top and bottom edges. No sidebars, no overlays during active play except move counter and score.

2. **Warm tones.** UI backgrounds use Parchment or Warm Cream, never pure white. Text uses Ink, never pure black. Borders use Hearthstone at 40% opacity if needed — prefer borderless.

3. **Rounded corners.** All UI containers use a corner radius of 16pt. Buttons use 12pt. Pills and badges use full-round (half-height).

4. **Large tap targets.** Minimum 44×44 pt for all interactive elements. Preferred 48×48 pt. Spacing between adjacent targets: minimum 8pt.

5. **Clear hierarchy.** One primary action per screen (Cauldron Amber fill, Cloud text). Secondary actions are outlined or text-only. Destructive actions require confirmation and are never the most prominent element.

6. **Motion with purpose.** Every animation communicates state change. No decorative animation in UI chrome. Transitions use ease-out curves, 200-300ms. Modals slide up from bottom (not fade-in — feels more tangible).

7. **Accessible contrast.** All text meets WCAG AA contrast ratios (4.5:1 for body, 3:1 for large text). Ingredient colors are distinguishable in common forms of color blindness — verified with Sim Daltonism; shape silhouettes provide redundant coding.

8. **Score and moves.** Displayed top-center. Moves remaining is the largest numeral on screen during play (Nunito Bold, 32pt). Score is secondary (20pt).

---

## App Icon Concepts

### Concept A — "Bubbling Cauldron"

A round cauldron viewed from a 3/4 angle, sitting on the Cauldron Amber background. The cauldron is dark iron (Ink color) with a subtle metallic sheen. Three bubbles rise from the liquid inside — each a different ingredient color (Ember red, Vine green, Sun gold). The liquid surface glows warmly. A wisp of steam curls off the top edge. No text. The cauldron occupies ~60% of the icon area, centered vertically and slightly below center.

**Strengths:** Immediately communicates "potion/brewing." High contrast. Distinctive silhouette at small sizes.

### Concept B — "Glowing Brew Orb"

A single Tier 3 brew orb, centered on a deep Night Nook background. The orb is Sun-gold with swirling highlights, a bright core, and orbiting sparkle particles. Below the orb, a faint reflection on a dark surface suggests the worktable. The glow from the orb illuminates the edges of the icon, creating a warm vignette in reverse (bright center, dark edges).

**Strengths:** Abstract and intriguing. Eye-catching glow effect. Works well as a recognizable home-screen element.

### Concept C — "Potion Vial"

A single potion vial, slightly tilted (10° clockwise), on a Warm Cream background. The vial is clear glass with a cork stopper and a small label. The liquid inside is a gradient from Ember red at the bottom to Sun gold at the top, with tiny bubbles frozen in the liquid. A small sparkle sits at the top-right of the vial. Beneath the vial, a faint shadow grounds it.

**Strengths:** Communicates the reward/goal of the game. Clean, readable at all sizes. Approachable and non-intimidating.

### Recommendation

Begin A/B testing with Concepts A and C. Concept A has stronger silhouette differentiation in the App Store grid. Concept C is more approachable and may test better with casual audiences. Concept B is a backup if both underperform — its glow effect is attention-grabbing but may not communicate "game" clearly enough.

---

## App Store Screenshot Composition Guide

All screenshots at 1290×2796 px (iPhone 15 Pro Max). Android equivalents rendered from same source at 1080×2400 px.

### Screenshot 1 — "Gameplay Action"

- Full board visible, mid-game state
- A finger (or subtle glow cursor) is tapping a 4-token Ember cluster
- The cluster is in Selected Pulse state, ripple rings visible
- Top caption: **"Tap. Fuse. Brew."** (Nunito Bold, 28pt, Cloud color)
- Background: workshop scene visible behind the board, warm and inviting
- Purpose: Immediately show what the game IS

### Screenshot 2 — "Brew Moment"

- Freeze-frame of Phase 2 of the brew animation
- The orb is mid-eruption: particles flying, white flash, vial shape forming
- Screen vignette is active, focusing attention on the center
- Top caption: **"Every Brew Feels Magical"**
- Purpose: Convey the emotional payoff and visual spectacle

### Screenshot 3 — "Recipe Completion"

- Recipe card UI visible: a completed recipe with all ingredient orbs checked off
- The brewed potion vial is shown large, liquid shimmering
- A "+150 XP" reward badge floats nearby
- Top caption: **"Discover 50+ Potions"**
- Purpose: Show depth, variety, and progression

### Screenshot 4 — "Workshop"

- Full workshop scene at upgrade level 50+ (cat, full shelves, warm lighting)
- Potion shelf partially visible with several filled slots glowing
- No board visible — this is the meta layer
- Top caption: **"Build Your Dream Workshop"**
- Purpose: Show the cozy meta-game and aspiration

### Screenshot 5 — "Potion Shelf Collection"

- Full-screen potion shelf view
- 16+ potions collected, arranged in rows, each glowing its color
- One empty row remains — teasing completionist drive
- A finger is tapping one potion, revealing its detail card
- Top caption: **"Collect Them All"**
- Purpose: Collection/completionist appeal, visual variety of potion designs

### Screenshot Styling

- Captions always at top, inside a frosted Parchment pill with rounded corners
- Device frame: none (borderless per current store best practices)
- All screenshots share a consistent warm color temperature — no cold/blue casts
- Each screenshot can standalone — no narrative dependency between them
