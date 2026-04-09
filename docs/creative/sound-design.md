# Brew — Sound Design Brief

## Audio Identity

Brew sounds like a place, not a product. The audio should make the player feel like they're sitting in a warm workshop with a crackling fire, clinking glass, and the quiet hum of something magical happening just out of sight.

**Core qualities:**
- **Warm** — No sharp digital edges. Every sound should feel like it passed through wood and stone.
- **Organic** — Prefer recorded-source sounds over pure synthesis. Layer real materials (glass, water, wood, metal) with light processing.
- **Magical** — The supernatural is subtle. Magic here sounds like resonance and shimmer, not laser beams and explosions.
- **Consistent scale** — This is a tabletop game in a small room. Nothing should sound like it's happening in a cathedral or a stadium. Reverb tails are short and warm (small room IR or plate reverb with early reflections).

**Instrument palette:**
- Kalimba and music box (primary melodic voice)
- Soft tubular bells and wind chimes
- Wooden percussion: woodblock, tongue drum, shaker
- Glass: wine glass rims, glass chimes, bottle taps
- Subtle warm synth pads (analog-modeled, low-passed, used for sustained tones only)
- Acoustic guitar (fingerpicked, single notes, for transitions)
- Bowed vibraphone (for magical moments)

---

## Music

### Workshop Ambient Loop

**Duration:** 72 seconds, seamless loop (crossfade region: final 4 seconds mirror opening 4 seconds)

**Tempo:** 78 BPM, swung eighth notes. Slow enough to feel calm, rhythmic enough to feel alive.

**Key:** D major (warm, open, optimistic). Modulates briefly to B minor in the middle section for gentle contrast, returns to D.

**Structure:**
- **0–18s:** Kalimba melody over a fingerpicked guitar arpeggiation. Simple, memorable 4-bar phrase. A soft pad holds the root note.
- **18–36s:** Melody drops out. Tongue drum takes over with a gentle rhythmic pattern. Chime accents on beats 2 and 4. A subtle bass note (warm sine sub) anchors the harmony.
- **36–54s:** B minor modulation. Bowed vibraphone plays a slow counter-melody. The pad swells slightly. A distant wind chime enters (randomized timing, low velocity).
- **54–72s:** Return to D major. Kalimba melody reprises, now doubled with a music box an octave higher. The texture thins to match the opening, enabling the seamless loop.

**Production notes:**
- Master at -14 LUFS integrated for comfortable background listening
- No compression on the master bus — dynamics are part of the charm
- Mix in stereo with wide panning on chimes and percussion, centered melody and bass
- Provide stems: melody, harmony/pad, percussion, FX/chimes (for adaptive layering)

**Reference tracks:**
- "Stardew Valley — Spring (The Valley Comes Alive)" — rhythmic warmth, major key optimism
- "Minecraft — Wet Hands" — spacious, contemplative, beautiful in simplicity
- "Ori and the Blind Forest — Restoring the Light" — emotional warmth without sentimentality

---

## SFX Catalog

Each sound is described by its sonic character, duration, and emotional intent. Reference implementation names are in `monospace`.

### Core Gameplay

#### `sfx_token_tap`
**Description:** A soft "tink" — like a fingertip tapping a ceramic bead on a wooden surface. Slight low-end warmth from the wood. A tiny room reverb tail (100ms decay).
**Duration:** 180ms
**Variations:** 3 round-robin variants (slight pitch/timing differences) to avoid machine-gun repetition on rapid taps.
**Emotional intent:** Tactile confirmation. "I touched something real."

#### `sfx_cluster_highlight`
**Description:** A gentle ascending shimmer — like running a finger across 3 small wind chimes in quick succession. The pitch corresponds loosely to the ingredient type (Ember = lower register, Frost = higher). A soft harmonic hum sustains briefly underneath.
**Duration:** 400ms
**Emotional intent:** Recognition and invitation. "This group is special — go ahead."

#### `sfx_fusion`
**Description:** A satisfying "whoosh" of air being pulled inward (reverse cymbal, very short), followed by a crystalline merge tone — a bell-like chime with a glassy overtone. The pitch rises slightly with cluster size: 3-token cluster is the base pitch, each additional token adds a semitone.
**Duration:** 500ms
**Pitch scaling:** Base = C5. +1 token = C#5, +2 = D5, etc.
**Layer detail:** Underneath the chime, a soft "thud" grounds the moment (like a heavy marble landing on felt).
**Emotional intent:** Satisfying compression. Things coming together.

#### `sfx_chain_fusion`
**Description:** The fusion sound, but cascading. Each successive fusion in the chain plays with a 200ms delay, and each step is pitched one whole tone higher than the previous. A subtle sustain pad swells underneath the chain, growing in volume with each step. If the chain reaches 4+ fusions, a gentle tremolo enters the pad.
**Duration per step:** 400ms (overlapping). Total chain duration varies.
**Emotional intent:** Building momentum. Escalating delight.

#### `sfx_brew` ★ Hero Sound
**Description:** This is the money shot. The most important sound in the game.
- **Phase 1 (0–600ms):** Bubbling — real water bubbling (pitch-shifted up slightly for sweetness), layered with a low rumble that rises in pitch. The bubbling accelerates.
- **Phase 2 (600–1000ms):** Crescendo — the rumble peaks, a reverse cymbal sweeps upward, and a sharp crystalline "POP" cuts through — like a champagne cork made of glass. A single clean bell note rings at the ingredient's signature pitch.
- **Phase 3 (1000–1800ms):** Sparkle flourish — a cascade of tiny chime hits (8–12 notes) raining down in a descending arpeggiated pattern. A warm pad resolves to a major chord and sustains. The final note is a soft, low "ommm" — a resonant hum that fades naturally.
**Duration:** 1.8 seconds
**Emotional intent:** Pure reward. The player should feel a small rush of joy every single time. This sound must never get annoying — it should be as satisfying on the 500th brew as the 1st. Avoid anything shrill or piercing. Warmth and sparkle, not flash and bang.

#### `sfx_potion_fill`
**Description:** Liquid pouring into a glass vessel — a realistic pour (recorded: water into a wine glass from 3 inches), pitch-shifted slightly lower for a thicker feel. The pour slows as the vial fills. At the top, a clean glass "ding" — a single finger-flick on a wine glass rim — rings out and sustains for 800ms.
**Duration:** 1.2 seconds
**Emotional intent:** Completion. Craftsmanship. The potion is real now.

### Level Events

#### `sfx_level_complete`
**Description:** A celebratory ascending chime sequence — 5 notes climbing a major pentatonic scale (D-E-F#-A-B), each played on kalimba with increasing velocity. After the 5th note, a cluster of 3 soft bell tones ring simultaneously (D major triad, high register). A faint "crowd" layer plays underneath — not literal applause, but a warm wash of reverberant shimmer that suggests collective joy. Like distant fairy celebration.
**Duration:** 2.0 seconds
**Emotional intent:** Achievement, warmth, "well done."

#### `sfx_level_fail`
**Description:** Three descending notes on a music box — F#, E, D — played slowly (250ms between notes). The third note sustains and fades with a gentle vibrato. No harsh tones. A soft, low pad holds a D minor chord underneath, but it resolves to D major in the tail (the sadness lifts — encouraging, not punishing).
**Duration:** 1.5 seconds
**Emotional intent:** "That's okay. You'll get it next time." Gentle disappointment, not failure.

### Boosters

#### `sfx_booster_shake`
**Description:** A tumbling rattle — like a wooden box of glass marbles being shaken twice. Loose, percussive, playful. A slight whoosh at the start suggests the shake motion.
**Duration:** 700ms

#### `sfx_booster_catalyst`
**Description:** A sharp electric spark — a quick "zzt" with a bright harmonic overtone, followed by a soft sizzle that fades. Think static discharge through a crystal. Not aggressive — curious and energizing.
**Duration:** 500ms

#### `sfx_booster_extra_moves`
**Description:** A clock tower chime — a single deep bell note (C3) followed by two lighter chimes (G3, C4). Evocative of time being granted. A soft tick-tock plays underneath for 400ms then stops.
**Duration:** 900ms

### Meta / Workshop

#### `sfx_workshop_upgrade`
**Description:** A layered transformation sound. Starts with a construction element — two soft wooden hammer taps — then transitions into a magical shimmer (ascending harp glissando, compressed and reverbed). Ends with a soft, resonant "settling" tone — like a heavy stone finding its final resting place. A tiny sparkle accent finishes the tail.
**Duration:** 1.5 seconds
**Emotional intent:** Something old becoming something beautiful.

#### `sfx_daily_brew_available`
**Description:** A gentle notification — a single music box note (A4) followed 300ms later by the same note an octave higher (A5). A tiny shimmer tail. Quiet enough to be ambient, distinct enough to be noticed.
**Duration:** 600ms
**Volume:** -6dB from standard SFX volume. This should never startle.

#### `sfx_win_streak_milestone`
**Description:** Scales with streak tier:
- **Streak 3:** The level complete chime, slightly louder, with an added tambourine shake.
- **Streak 5:** Above, plus a short fanfare (3-note brass-like synth phrase, major triad).
- **Streak 7:** Above, plus a deeper bass drum hit and a longer sparkle tail.
- **Streak 10+:** Full celebration: all of the above, plus a sustained "choir" pad (aaah, synth-voiced, warm) and a final cymbal swell.
**Duration:** 2.0–3.5 seconds depending on tier.
**Emotional intent:** Escalating accomplishment. The game is cheering for you.

### UI

#### `sfx_ui_tap`
**Description:** A soft, dry click — like a wooden button being pressed on a felt surface. No reverb. Minimal frequency content above 4kHz.
**Duration:** 80ms
**Volume:** -8dB from standard SFX volume. Background-level confirmation.

#### `sfx_ui_page_turn`
**Description:** A paper page-turning sound — soft, breathy. Slightly slower than a real page turn (300ms). A faint fabric rustle layer underneath.
**Duration:** 300ms

#### `sfx_ui_modal_open`
**Description:** A soft upward "whoosh" — like a scroll being unfurled. Very short. Rising pitch from ~200Hz to ~600Hz over 200ms, heavily filtered.
**Duration:** 200ms

#### `sfx_ui_modal_close`
**Description:** The inverse: a short downward "whoosh." Same character, reversed pitch direction.
**Duration:** 200ms

---

## Haptic Feedback Map

Target devices: iPhone (Taptic Engine) and Android (wide-band haptic actuators). All haptics are opt-in with a settings toggle. Intensity values reference Apple's `UIImpactFeedbackGenerator` weight scale.

| Action                 | Haptic Type         | Intensity | Notes                                    |
|------------------------|--------------------|-----------|--------------------------------------------|
| Token tap              | Light impact        | 0.4       | Crisp, immediate                          |
| Cluster highlight      | Soft impact         | 0.3       | Gentle confirmation                       |
| Fusion                 | Medium impact       | 0.6       | Satisfying "thunk"                        |
| Chain fusion (per step)| Medium impact       | 0.5–0.8   | Increases with each chain step            |
| Brew (Phase 1)         | Continuous (200ms)  | 0.3       | Low rumble during bubbling                |
| Brew (Phase 2 — pop)   | Heavy impact        | 0.9       | The big moment                            |
| Brew (Phase 3)         | Light impact ×4     | 0.2       | Sparkle taps, 150ms apart                 |
| Level complete         | Success pattern     | 0.5       | Double-tap pattern                        |
| Level fail             | None                | —         | No haptic on fail — avoids punishment     |
| Booster activate       | Medium impact       | 0.6       | Single firm tap                           |
| Workshop upgrade       | Heavy impact + fade | 0.7→0.2   | Knock then settle                         |
| UI tap                 | Selection impact    | 0.2       | Barely perceptible, confirms touch        |

### Android Considerations

- Use `VibrationEffect.createPredefined()` for standardized patterns where available
- Fall back to `VibrationEffect.createOneShot()` with amplitude scaling for older devices
- Disable continuous haptics on devices that only support on/off vibration motors

---

## Adaptive Audio System

The music responds to game state, creating subtle emotional shifts without jarring transitions.

### Low Moves Warning (≤5 moves remaining)

**Trigger:** Move counter reaches 5 or below.
**Musical response:**
- The percussion stem increases in tempo by ~8% (78 BPM → 84 BPM), accomplished via time-stretching the percussion stem independently
- A new layer enters: a soft, pulsing heartbeat-like bass note (quarter notes, C2, sine wave, very low volume)
- The melody stem thins — the kalimba drops to half velocity, creating space and tension
- Chime accents shift from beats 2 and 4 to every beat, subtly increasing urgency

**Recovery:** If moves are restored (booster or chain creates new moves), the changes reverse over 4 seconds (cross-fade back to the relaxed stems).

### Critical Move (1 move remaining)

**Trigger:** Move counter reaches 1.
**Musical response:**
- All stems except the pad and heartbeat bass drop out
- The pad shifts to a sustained minor chord (B minor)
- A single high note (music box, D6) enters and sustains with gentle tremolo
- Volume decreases by 3dB — the silence is louder than the music

**Recovery:** On successful fusion, the full arrangement snaps back in with a cymbal swell (200ms transition). The emotional relief is the payoff.

### Win Streak Active (3+ consecutive wins)

**Trigger:** Third consecutive level completion.
**Musical response:**
- A subtle shaker loop layer is added to the music (festive, rhythmic)
- The melody stem is doubled with a second instrument: 3-streak = music box, 5-streak = glockenspiel, 7+ = both
- Volume of the celebration layer increases 1dB per streak level (capped at +4dB)

### Brew Moment Override

**Trigger:** Brew animation begins.
**Musical response:**
- All music stems duck to -12dB over 200ms (sidechain-like)
- The brew SFX plays at full volume with no competing elements
- Music returns to full volume over 800ms after the brew SFX ends
- This ensures the hero sound is always heard clearly

### Implementation Notes

- Music is delivered as 4 separate stems (melody, harmony/pad, percussion, FX/chimes) that play in sync
- Adaptive changes are accomplished by volume automation, time-stretch, and crossfade between stem variants
- All transitions use linear or equal-power crossfades to avoid clicks
- The audio engine should support at least 8 simultaneous SFX channels + 4 music stems
- SFX priority system: brew > fusion > chain > cluster highlight > token tap > UI
