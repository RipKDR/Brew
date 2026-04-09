# Brew — Data Model Specification

> **Version:** 0.1.0-draft · **Last updated:** 2026-04-09
> **Status:** Pre-production · MVP scope

---

## 1. Overview

Brew uses a dual-storage architecture:

- **Local (device):** Encrypted JSON file in `Application.persistentDataPath`. Written after every move and every meta-state change. Source of truth for gameplay; enables fully offline play.
- **Cloud (Firestore):** Document-per-player model under `players/{uid}`. Synced on connectivity. Source of truth for currencies and purchase history (anti-exploit).

All timestamps are ISO 8601 / UTC. All IDs are lowercase alphanumeric with underscores.

---

## 2. Player Profile

**Firestore path:** `players/{uid}`
**Local file:** `save/player_profile.json`

```jsonc
{
  // Identity
  "player_id": "abc123def456",           // Firebase Auth UID
  "display_name": "Brewer42",            // Player-chosen, max 20 chars
  "avatar_id": "avatar_default",         // Unlockable avatar
  "created_at": "2026-05-01T12:00:00Z",
  "last_login": "2026-06-15T08:30:00Z",
  "platform": "ios",                     // "ios" | "android"
  "app_version": "1.0.0",
  "device_model": "iPhone14,6",

  // Progression
  "total_levels_completed": 47,
  "current_level": 48,                   // Next level to play on the map
  "highest_level_unlocked": 48,

  // Economy
  "essence_balance": 3200,               // Soft currency
  "gem_balance": 85,                     // Premium currency

  // Collection
  "potions_collected": [                  // Array of potion IDs brewed at least once
    "healing_draught",
    "fire_elixir",
    "frost_tonic"
  ],

  // Workshop
  "workshop": {
    "upgrades_completed": [
      "cauldron_repair",
      "shelf_restore",
      "garden_unlock"
    ],
    "current_upgrade_index": 3,          // Next upgrade to purchase
    "total_essence_spent": 4500
  },

  // Streaks
  "streak_count": 5,                     // Current consecutive days
  "streak_best": 12,                     // All-time best
  "streak_last_date": "2026-06-15",      // Date of last streak-qualifying action (YYYY-MM-DD UTC)

  // Daily Brew
  "daily_brew_last_completed": "2026-06-15",  // Date string; compared against today UTC

  // Events
  "event_progress": {
    "halloween_2026": {
      "levels_completed": 6,
      "rewards_claimed": [0, 1],         // Indices into event rewards array
      "last_played": "2026-10-28T19:00:00Z"
    }
  },

  // Boosters
  "booster_inventory": {
    "extra_moves_5": 2,
    "color_bomb": 1,
    "row_clear": 0
  },

  // Level Stars (sparse map — only levels with recorded progress)
  "level_stars": {
    "1": 3,
    "2": 3,
    "3": 2,
    "47": 1
  },

  // Settings
  "settings": {
    "music_enabled": true,
    "sfx_enabled": true,
    "notifications_enabled": true,
    "haptics_enabled": true,
    "language": "en"
  },

  // Sync Metadata (local only — not stored in Firestore)
  "_local": {
    "last_sync_at": "2026-06-15T08:30:00Z",
    "pending_sync_queue": [],
    "dirty": false
  }
}
```

### Field Constraints

| Field | Type | Constraints |
|---|---|---|
| `player_id` | string | Firebase UID, immutable after creation |
| `display_name` | string | 1–20 characters, alphanumeric + spaces, profanity-filtered server-side |
| `essence_balance` | int | ≥ 0, server-authoritative |
| `gem_balance` | int | ≥ 0, server-authoritative |
| `current_level` | int | ≥ 1, monotonically increasing |
| `streak_count` | int | ≥ 0, resets to 0 if `streak_last_date` is >1 calendar day ago |
| `level_stars` | map<string, int> | Key: level_id as string. Value: 1–3. |
| `booster_inventory` | map<string, int> | Key: booster_type_id. Value: ≥ 0. |

---

## 3. Level Definition

**Storage:** ScriptableObject in Unity build (`Assets/_Project/ScriptableObjects/Levels/`), exportable as JSON. Event levels may be fetched from Cloud Storage.

```jsonc
{
  "level_id": 48,
  "level_type": "normal",                // "normal" | "boss" | "tutorial" | "daily" | "event"

  // Grid
  "grid_width": 8,
  "grid_height": 10,
  "grid_mask": [                          // 1 = active cell, 0 = blocked/empty
    [1,1,1,1,1,1,1,1],
    [1,1,1,1,1,1,1,1],
    [1,1,1,0,0,1,1,1],
    [1,1,1,1,1,1,1,1],
    [1,1,1,1,1,1,1,1],
    [1,1,1,1,1,1,1,1],
    [1,1,1,1,1,1,1,1],
    [1,1,0,1,1,0,1,1],
    [1,1,1,1,1,1,1,1],
    [1,1,1,1,1,1,1,1]
  ],

  // Ingredients
  "ingredient_pool": [                    // Colors available for random token spawns
    "red", "blue", "green", "yellow"
  ],
  "ingredient_weights": {                 // Spawn probability weights (optional, defaults to uniform)
    "red": 1.0,
    "blue": 1.0,
    "green": 0.8,
    "yellow": 0.6
  },

  // Objectives
  "recipe_targets": [                     // Win condition: brew these potions
    { "potion_id": "healing_draught", "count": 2 },
    { "potion_id": "fire_elixir", "count": 1 }
  ],

  // Constraints
  "move_limit": 25,

  // Blockers (pre-placed obstacles)
  "blocker_placements": [
    { "type": "ice", "x": 3, "y": 2, "hp": 1 },
    { "type": "ice", "x": 4, "y": 2, "hp": 2 },
    { "type": "stone", "x": 2, "y": 7, "hp": 1 },
    { "type": "stone", "x": 5, "y": 7, "hp": 1 }
  ],

  // Initial board state (optional — if omitted, board is randomly generated)
  "initial_board": null,

  // Scoring
  "star_thresholds": [1000, 3000, 6000], // Score needed for 1★, 2★, 3★

  // RNG
  "seed_mode": "random",                 // "random" | "fixed"
  "fixed_seed": null,                    // Used for daily/event levels where all players get same board

  // Tutorial
  "is_tutorial": false,
  "tutorial_steps": []                   // Only populated for tutorial levels
}
```

### Tutorial Step Schema

```jsonc
{
  "step_index": 0,
  "type": "highlight_tap",              // "highlight_tap" | "dialog" | "arrow_point" | "force_tap"
  "message": "Tap a group of 3+ same-colored tokens to fuse them!",
  "target_cells": [{"x": 2, "y": 3}, {"x": 3, "y": 3}, {"x": 4, "y": 3}],
  "allow_other_input": false
}
```

### Blocker Types (MVP)

| Type | Behavior | HP |
|---|---|---|
| `ice` | Covers a token. Adjacent fusions crack ice (1 HP per adjacent fusion). Token underneath revealed when destroyed. | 1–2 |
| `stone` | Occupies a cell. Cannot be fused. Destroyed by adjacent fusions. | 1–3 |
| `lock` | Locks a token in place (cannot fall due to gravity). Destroyed by including the locked token in a fusion. | 1 |

---

## 4. Level Result

**Firestore path:** `players/{uid}/level_results/{level_id}_{timestamp}`
**Local:** appended to `save/level_results.json` (rolling buffer, last 200 results)

```jsonc
{
  "level_id": 48,
  "player_id": "abc123def456",
  "completed": true,
  "stars": 2,
  "score": 4200,
  "moves_used": 22,
  "moves_remaining": 3,
  "potions_brewed": [
    { "potion_id": "healing_draught", "count": 2 },
    { "potion_id": "fire_elixir", "count": 1 }
  ],
  "boosters_used": ["extra_moves_5"],
  "ads_watched": [
    { "type": "rewarded", "placement": "extra_moves", "timestamp": "2026-06-15T08:32:00Z" }
  ],
  "cascade_max_chain": 4,
  "largest_fusion_size": 7,
  "duration_seconds": 72,
  "timestamp": "2026-06-15T08:33:00Z",
  "app_version": "1.0.0",
  "is_first_attempt": false,
  "attempt_number": 2
}
```

---

## 5. Event Definition

**Storage:** Firebase Remote Config (JSON parameter `active_events`, array of event objects). Can also be stored in Cloud Storage as JSON and referenced by URL.

```jsonc
{
  "event_id": "halloween_2026",
  "theme_name": "Spooky Brews",
  "description": "Brew haunted potions before the witching hour!",

  // Schedule
  "start_date": "2026-10-25T00:00:00Z",
  "end_date": "2026-11-01T23:59:59Z",
  "visible_before_start_hours": 24,       // Show "Coming Soon" banner this many hours before start

  // Levels
  "levels": [
    "evt_hw_01", "evt_hw_02", "evt_hw_03",
    "evt_hw_04", "evt_hw_05", "evt_hw_06",
    "evt_hw_07", "evt_hw_08", "evt_hw_09"
  ],
  "level_source": "remote",               // "bundled" (in app) | "remote" (download from Cloud Storage)
  "level_bundle_url": "gs://brew-prod/events/halloween_2026/levels.json",

  // Rewards (cumulative based on levels completed)
  "rewards": [
    { "threshold": 3, "type": "essence", "amount": 500, "label": "500 Essence" },
    { "threshold": 6, "type": "potion", "potion_id": "spooky_elixir", "label": "Spooky Elixir" },
    { "threshold": 9, "type": "gems", "amount": 50, "label": "50 Gems" }
  ],

  // Display
  "display_config": {
    "banner_asset": "events/halloween_2026/banner",
    "icon_asset": "events/halloween_2026/icon",
    "color_primary": "#FF6B00",
    "color_secondary": "#1A0A2E",
    "background_asset": "events/halloween_2026/bg"
  }
}
```

---

## 6. Workshop State

Part of the player profile (nested object). Defined separately here for clarity.

```jsonc
{
  "upgrades_completed": [
    "cauldron_repair",
    "shelf_restore",
    "garden_unlock"
  ],
  "current_upgrade_index": 3,
  "total_essence_spent": 4500
}
```

### Workshop Upgrade Definitions (Remote Config)

```jsonc
{
  "workshop_upgrades": [
    {
      "upgrade_id": "cauldron_repair",
      "display_name": "Repair the Cauldron",
      "description": "Fix the old cauldron to start brewing!",
      "cost_essence": 500,
      "reward_type": "unlock_feature",
      "reward_value": "potion_shelf",
      "art_asset": "workshop/cauldron_repair"
    },
    {
      "upgrade_id": "shelf_restore",
      "display_name": "Restore the Shelves",
      "description": "Organize your potion collection.",
      "cost_essence": 1000,
      "reward_type": "booster_slot",
      "reward_value": "extra_moves_5",
      "art_asset": "workshop/shelf_restore"
    },
    {
      "upgrade_id": "garden_unlock",
      "display_name": "Unlock the Garden",
      "description": "Grow rare ingredients.",
      "cost_essence": 2000,
      "reward_type": "ingredient",
      "reward_value": "purple",
      "art_asset": "workshop/garden_unlock"
    }
    // ...additional upgrades defined in Remote Config, added without app update
  ]
}
```

---

## 7. Potion Collection

**Firestore path:** `players/{uid}` → `potions_collected` (array of IDs)
**Definition data (read-only):** Baked into build as ScriptableObjects + available in Remote Config for additions.

### Potion Definition (ScriptableObject / JSON)

```jsonc
{
  "potion_id": "healing_draught",
  "name": "Healing Draught",
  "description": "A soothing crimson potion with restorative properties.",
  "color": "#E63946",
  "rarity": "common",                    // "common" | "rare" | "epic" | "legendary" | "event"
  "recipe": {
    "ingredients": [
      { "color": "red", "min_tier": 3, "count": 3 },
      { "color": "blue", "min_tier": 2, "count": 2 }
    ]
  },
  "shelf_slot": 1,                       // Position on potion shelf UI
  "icon_asset": "potions/healing_draught",
  "brew_animation": "brew_standard",
  "first_available_level": 5
}
```

### Player Potion Progress (per-potion, local save)

```jsonc
{
  "potion_id": "healing_draught",
  "level_first_brewed": 5,
  "times_brewed": 14,
  "last_brewed_at": "2026-06-15T08:33:00Z"
}
```

---

## 8. Remote Config Schema

All values served from Firebase Remote Config. Organized by parameter group.

### 8.1 Economy

| Parameter | Type | Default | Description |
|---|---|---|---|
| `essence_per_star_1` | int | 50 | Essence awarded for 1-star completion |
| `essence_per_star_2` | int | 100 | Essence awarded for 2-star completion |
| `essence_per_star_3` | int | 200 | Essence awarded for 3-star completion |
| `gems_daily_brew` | int | 5 | Gems for completing daily brew |
| `gems_streak_day_3` | int | 10 | Gems at streak day 3 |
| `gems_streak_day_7` | int | 25 | Gems at streak day 7 |
| `booster_cost_extra_moves` | int | 100 | Essence cost for Extra Moves booster |
| `booster_cost_color_bomb` | int | 200 | Essence cost for Color Bomb booster |
| `booster_cost_row_clear` | int | 150 | Essence cost for Row Clear booster |
| `booster_gem_cost_extra_moves` | int | 10 | Gem cost for Extra Moves booster |
| `booster_gem_cost_color_bomb` | int | 20 | Gem cost for Color Bomb booster |
| `booster_gem_cost_row_clear` | int | 15 | Gem cost for Row Clear booster |

### 8.2 Ad Frequency Caps

| Parameter | Type | Default | Description |
|---|---|---|---|
| `interstitial_cooldown_seconds` | int | 120 | Minimum seconds between interstitials |
| `max_interstitials_per_session` | int | 5 | Hard cap per session |
| `interstitial_start_after_level` | int | 5 | Don't show interstitials before this level |
| `rewarded_cooldown_seconds` | int | 30 | Cooldown between rewarded ads |
| `rewarded_extra_moves_count` | int | 5 | Moves granted per rewarded ad |

### 8.3 Feature Flags

| Parameter | Type | Default | Description |
|---|---|---|---|
| `feature_events_enabled` | bool | true | Master toggle for events system |
| `feature_workshop_enabled` | bool | true | Show/hide workshop |
| `feature_daily_brew_enabled` | bool | true | Enable daily brew level |
| `feature_push_notifications` | bool | true | Enable push notification sends |
| `maintenance_mode` | bool | false | Show maintenance screen, block gameplay |
| `force_update_min_version` | string | "1.0.0" | Minimum app version; older shows force-update dialog |

### 8.4 Booster Config

```jsonc
{
  "boosters": [
    {
      "booster_id": "extra_moves_5",
      "display_name": "Extra Moves",
      "description": "+5 moves",
      "type": "pre_game",
      "effect": "add_moves",
      "effect_value": 5,
      "cost_essence": 100,
      "cost_gems": 10,
      "icon_asset": "boosters/extra_moves",
      "enabled": true
    },
    {
      "booster_id": "color_bomb",
      "display_name": "Color Bomb",
      "description": "Clears all tokens of one color",
      "type": "in_game",
      "effect": "clear_color",
      "effect_value": 1,
      "cost_essence": 200,
      "cost_gems": 20,
      "icon_asset": "boosters/color_bomb",
      "enabled": true
    },
    {
      "booster_id": "row_clear",
      "display_name": "Row Clear",
      "description": "Clears an entire row",
      "type": "in_game",
      "effect": "clear_row",
      "effect_value": 1,
      "cost_essence": 150,
      "cost_gems": 15,
      "icon_asset": "boosters/row_clear",
      "enabled": true
    }
  ]
}
```

---

## 9. Local vs Cloud Storage Strategy

| Data | Local | Cloud (Firestore) | Authority | Sync Frequency |
|---|---|---|---|---|
| Player profile | ✅ Full copy | ✅ Full copy | Cloud for currencies; local for progress | On foreground, level complete, connectivity change |
| Level stars | ✅ | ✅ (in profile) | max(local, cloud) per level | With profile sync |
| Level results | ✅ Last 200 | ✅ All (subcollection) | Cloud | Batched on connectivity |
| Booster inventory | ✅ | ✅ (in profile) | Cloud | With profile sync |
| Potion collection | ✅ | ✅ (in profile) | union(local, cloud) | With profile sync |
| Workshop state | ✅ | ✅ (in profile) | max(index) | With profile sync |
| Streak data | ✅ | ✅ (in profile) | Cloud | With profile sync |
| Event progress | ✅ | ✅ (in profile) | Cloud for rewards claimed; local for levels played | With profile sync |
| Settings | ✅ | ❌ | Local only | Never synced |
| Level definitions | ✅ (ScriptableObjects) | ❌ (Cloud Storage for remote levels) | Build + Cloud Storage | On-demand download |
| Remote Config | ✅ (cached) | — (Remote Config service) | Server | On foreground, 1-hour cache TTL |
| IAP receipts | ❌ | ✅ (Cloud Function) | Server (receipt verification) | On purchase |
| Ad history | ✅ (session-scoped) | ❌ | Local | Never synced |

---

## 10. Conflict Resolution

When local and cloud state diverge (e.g., player plays offline on two devices):

| Field | Resolution Strategy | Rationale |
|---|---|---|
| `essence_balance` | **Server wins** | Prevents duplication. Offline essence earnings are tracked as deltas in `pending_sync_queue` and applied server-side via Cloud Function that validates against level results. |
| `gem_balance` | **Server wins** | Same as essence. Gems from IAP are only granted after server-side receipt verification. |
| `current_level` / `highest_level_unlocked` | **max(local, cloud)** | Player should never lose forward progress. |
| `level_stars` | **max(local, cloud) per level** | Player keeps their best performance. |
| `potions_collected` | **union(local, cloud)** | Never remove a collected potion. |
| `workshop.current_upgrade_index` | **max(local, cloud)** | Workshop progress is forward-only. Essence cost validated server-side. |
| `streak_count` | **Server wins** | Server timestamp is authoritative for streak continuity. |
| `booster_inventory` | **Server wins** | Prevents duplication. Booster consumption tracked in level results, validated server-side. |
| `event_progress.rewards_claimed` | **union(local, cloud)** | Never un-claim a reward. Reward granting validated server-side. |
| `settings` | **Local wins** | Device-specific preferences, not synced. |

### Sync Flow

```
App Foreground
    │
    ├─► Fetch Remote Config (non-blocking, use cache if offline)
    │
    ├─► Read local save
    │
    ├─► Check connectivity
    │       │
    │       ├─ ONLINE ─► Pull cloud profile
    │       │              │
    │       │              ├─► Merge (rules above)
    │       │              ├─► Write merged state locally
    │       │              ├─► Push merged state to cloud
    │       │              └─► Flush pending_sync_queue
    │       │
    │       └─ OFFLINE ─► Use local save as-is
    │
    └─► Enter game
```

---

## 11. Firestore Document Size Estimates

| Document | Estimated Size | Notes |
|---|---|---|
| Player profile | 2–5 KB | Grows slowly with potions_collected and level_stars. At 500 levels completed, ~8 KB. |
| Level result | 0.5–1 KB | Subcollection document. |

Firestore document limit is 1 MB — no risk of exceeding even at 10,000+ levels.

### Firestore Indexes Required

| Collection | Fields | Type |
|---|---|---|
| `players` | `created_at` | ASC (for admin queries) |
| `players/{uid}/level_results` | `level_id`, `timestamp` | Composite ASC, DESC |
| `players/{uid}/level_results` | `timestamp` | DESC (for recent results) |

---

## 12. Data Validation Rules (Firestore Security Rules)

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    match /players/{uid} {
      allow read: if request.auth != null && request.auth.uid == uid;
      allow write: if request.auth != null && request.auth.uid == uid
                   && request.resource.data.essence_balance >= 0
                   && request.resource.data.gem_balance >= 0;

      match /level_results/{resultId} {
        allow read: if request.auth != null && request.auth.uid == uid;
        allow create: if request.auth != null && request.auth.uid == uid
                      && request.resource.data.level_id is int
                      && request.resource.data.stars >= 0
                      && request.resource.data.stars <= 3;
        allow update, delete: if false;
      }
    }

    match /leaderboards/{boardId} {
      allow read: if request.auth != null;
      allow write: if false; // Only Cloud Functions can write
    }
  }
}
```
