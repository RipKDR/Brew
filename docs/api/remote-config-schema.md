# Remote Config Schema

> Brew — Firebase Remote Config
> All keys live in a single Firebase Remote Config template.
> Client fetches via `getRemoteConfig` Cloud Function (cached with 30-min TTL).

---

## Overview

Remote Config is the primary tuning surface for designers and live-ops. Every numeric value that affects game feel, economy balance, or feature availability is stored here — never hard-coded in the client.

**Change workflow:** Designer proposes value in shared spreadsheet → Engineer reviews for range validity → Value updated in Firebase Console → Clients pick up change on next fetch (≤ 30 min).

---

## Economy

| Key | Type | Default | Valid Range | Description | Owner |
|---|---|---|---|---|---|
| `essence_per_star_1` | `int` | `50` | 10–500 | Essence rewarded for completing a level with 1 star | Designer |
| `essence_per_star_2` | `int` | `100` | 20–1000 | Essence rewarded for 2 stars | Designer |
| `essence_per_star_3` | `int` | `150` | 30–1500 | Essence rewarded for 3 stars | Designer |
| `gem_daily_brew_reward` | `int` | `5` | 1–50 | Gems granted for completing the daily brew | Designer |
| `workshop_upgrade_costs` | `JSON array<int>` | `[100, 250, 500, 1000, 2000]` | Each element 1–99999; array length = max upgrade level | Essence cost for each workshop station upgrade level (index 0 = level 1→2) | Designer |
| `booster_cost_shake_essence` | `int` | `200` | 50–5000 | Essence cost to purchase one Shake booster | Designer |
| `booster_cost_shake_gems` | `int` | `5` | 1–100 | Gem cost to purchase one Shake booster | Designer |
| `booster_cost_catalyst_essence` | `int` | `350` | 50–5000 | Essence cost to purchase one Catalyst booster | Designer |
| `booster_cost_catalyst_gems` | `int` | `8` | 1–100 | Gem cost to purchase one Catalyst booster | Designer |
| `extra_moves_gem_cost` | `int` | `10` | 1–100 | Gems to buy +5 extra moves mid-level | Designer |

### Economy Design Notes

- Essence is the soft currency (earned freely). Gems are the hard currency (earned slowly, purchased via IAP).
- Workshop costs follow an exponential curve. The array allows designers to hand-tune each tier.
- Booster dual-pricing (essence OR gems) gives players a choice: grind or spend.

---

## Gameplay

| Key | Type | Default | Valid Range | Description | Owner |
|---|---|---|---|---|---|
| `default_move_limit` | `int` | `25` | 10–99 | Default move limit when a level definition doesn't specify its own | Engineer |
| `min_cluster_size` | `int` | `3` | 2–5 | Minimum number of adjacent same-type tokens required to form a valid fusion | Engineer |
| `brew_threshold` | `int` | `7` | 4–15 | Number of same-type orbs in an orb that triggers a brew (potion creation) | Engineer |
| `cascade_settle_delay_ms` | `int` | `100` | 50–500 | Milliseconds between each row of tokens falling during a cascade. Controls cascade "speed feel." | Engineer |
| `max_boosters_per_level` | `int` | `3` | 0–10 | Maximum number of boosters a player can activate in a single level attempt | Designer |

### Gameplay Design Notes

- `min_cluster_size = 3` is the genre standard. Increasing to 4 dramatically increases difficulty.
- `brew_threshold` controls the core pacing: lower = more brews = easier levels.
- `cascade_settle_delay_ms` is purely cosmetic but affects perceived game speed. 100ms feels snappy; 200ms feels deliberate.

---

## Ads

| Key | Type | Default | Valid Range | Description | Owner |
|---|---|---|---|---|---|
| `rewarded_ad_extra_moves_count` | `int` | `5` | 1–15 | Number of extra moves granted when a player watches a rewarded ad | Designer |
| `interstitial_frequency` | `int` | `3` | 1–20 | Show an interstitial ad every N level completions. Higher = less intrusive. | Designer |
| `max_rewarded_ads_per_session` | `int` | `5` | 1–20 | Maximum rewarded ads a player can watch in a single app session | Designer |
| `ad_cooldown_seconds` | `int` | `30` | 10–300 | Minimum seconds between consecutive rewarded ad offers | Designer |

### Ads Design Notes

- `interstitial_frequency = 3` means a player sees an interstitial after every 3rd level. This resets on app restart.
- The session limit on rewarded ads prevents ad-farming exploits.
- `ad_cooldown_seconds` prevents spam-tapping the "watch ad" button.
- Interstitials are never shown after a loss (player is already frustrated).

---

## Streaks

| Key | Type | Default | Valid Range | Description | Owner |
|---|---|---|---|---|---|
| `streak_multipliers` | `JSON array<float>` | `[1.0, 1.0, 1.25, 1.5, 1.75, 2.0, 2.0, 2.5]` | Each element 1.0–5.0; array length ≥ 1 | Essence reward multiplier for consecutive wins. Index = streak count (0 = no streak). Values beyond array length use last element. | Designer |
| `streak_protection_enabled` | `bool` | `true` | — | When `true`, first loss after a streak of ≥ 3 does not reset the streak to 0 (resets to streak - 1 instead). One protection per streak. | Designer |

### Streak Design Notes

- Multipliers are applied server-side during `submitLevelResult` reward calculation.
- `streak_multipliers[0]` and `[1]` are both `1.0` — no bonus until the 3rd consecutive win.
- The array is intentionally index-addressed (not formulaic) so designers can create custom reward curves.
- Streak protection is a retention mechanic: losing one game shouldn't erase days of progress.

---

## Feature Flags

| Key | Type | Default | Description | Owner |
|---|---|---|---|---|
| `daily_brew_enabled` | `bool` | `true` | Master toggle for the daily challenge feature. When `false`, `getDailyBrew` returns 404. Client hides the daily brew UI entry point. | Engineer |
| `weekly_events_enabled` | `bool` | `true` | Master toggle for weekly event system. When `false`, `getActiveEvents` returns empty list. | Engineer |
| `workshop_enabled` | `bool` | `true` | Toggle for the workshop upgrade UI. When `false`, workshop tab is hidden but existing upgrades remain active. | Engineer |
| `win_streak_enabled` | `bool` | `true` | Toggle for streak tracking and streak bonus rewards. When `false`, streaks are not tracked and multipliers are always 1.0. | Engineer |

### Feature Flag Design Notes

- Flags default to `true` for MVP. They exist as safety valves: if a feature has a critical bug post-launch, we can disable it without a client update.
- Flags are **engineer-owned** because toggling them affects multiple systems (UI, backend, analytics).
- Client must respect flags both for UI visibility and for preventing API calls to disabled features.

---

## Events

| Key | Type | Default | Description | Owner |
|---|---|---|---|---|
| `active_event_id` | `string` | `""` | ID of the currently running event. Empty string = no active event. Must match an event document in `events` Firestore collection. | Designer |
| `event_config` | `JSON string` | `"{}"` | Full event configuration blob. Parsed by `getActiveEvents`. Contains level pool, reward tiers, theming data, and schedule. | Designer |

### Event Config JSON Shape

```jsonc
{
  "event_id": "spring_bloom_2026",
  "title": "Spring Bloom Festival",
  "description": "Brew rare floral potions!",
  "type": "weekly",
  "theme_color": "#4CAF50",
  "icon_asset": "event_spring_icon",
  "starts_at": "2026-04-07T00:00:00Z",
  "ends_at": "2026-04-14T00:00:00Z",
  "level_ids": ["event_spring_01", "event_spring_02", "event_spring_03"],
  "reward_tiers": [
    { "threshold": 3,  "rewards": { "essence": 200 } },
    { "threshold": 7,  "rewards": { "essence": 500, "gems": 10 } },
    { "threshold": 12, "rewards": { "essence": 1000, "gems": 25, "items": ["potion_blossom_x1"] } }
  ]
}
```

### Event Design Notes

- `active_event_id` is set via Firebase Console when launching an event. It points to a pre-created Firestore document with the full level data.
- `event_config` is the quick-reference config cached on the client. It contains UI theming and reward structure but not full level definitions (those come from `getActiveEvents`).
- Empty `active_event_id` gracefully hides all event UI.

---

## Tuning

| Key | Type | Default | Valid Range | Description | Owner |
|---|---|---|---|---|---|
| `difficulty_modifier` | `float` | `1.0` | 0.5–2.0 | Global multiplier on all level move limits. `0.8` = 20% fewer moves (harder). `1.2` = 20% more moves (easier). Applied server-side; client reads effective move limit from level definitions. | Designer |

### Tuning Design Notes

- This is a blunt instrument for global difficulty adjustment. Use sparingly.
- Applied as `effective_moves = floor(level_move_limit × difficulty_modifier)`.
- Intended for post-launch tuning if analytics show levels are systematically too hard/easy.
- Per-level tuning is done by editing individual level definitions, not this modifier.

---

## Client Integration

### Fetch & Cache

```
1. App launch → call getRemoteConfig(client_version, platform, etag)
2. On 200: store config in PlayerPrefs/local JSON, update etag
3. On 304: use cached config
4. Background refresh every 30 minutes while app is foregrounded
5. Force refresh on: level fail (difficulty may have changed), returning from background after > 1 hour
```

### Fallback Values

The Unity client embeds a `default_config.json` in the build that mirrors every key's default value. If the network call fails, the client uses these defaults. The embedded defaults are updated every release.

### Version Compatibility

When adding a new config key:
1. Add the key to Firebase Remote Config with its default value.
2. Add the key and default to the client's `default_config.json`.
3. Client code must handle the key being absent (older cached configs).

When removing a config key:
1. Stop reading the key in client code first.
2. Ship the update.
3. Remove from Firebase Remote Config after the minimum supported version no longer references it.

---

## Key Naming Conventions

| Convention | Example | Rationale |
|---|---|---|
| `snake_case` | `essence_per_star_1` | Consistent, readable in Firebase Console |
| Category prefix implied by grouping | Economy keys start with context | Firebase Console groups by prefix when alphabetized |
| Boolean flags use `_enabled` suffix | `daily_brew_enabled` | Clear intent without reading the value |
| JSON arrays/objects noted in type | `workshop_upgrade_costs` is `JSON array<int>` | Parsers know to `JSON.parse()` |
| No abbreviations | `interstitial_frequency`, not `interstitial_freq` | Avoid ambiguity |
