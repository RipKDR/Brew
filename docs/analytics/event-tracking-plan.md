# Brew — Event Tracking Plan

> Owner: Analytics Lead · Implemented by: Client Engineering
> SDK: Firebase Analytics (primary) + custom backend pipeline
> Last updated: 2026-04-09

---

## Conventions

- **Event names:** `snake_case`, max 40 chars.
- **Parameter names:** `snake_case`, max 40 chars.
- **String values:** lowercase, max 100 chars.
- **Numeric values:** integers unless noted. Currency amounts in smallest unit (cents / single Gems).
- **Timestamps:** ISO 8601 UTC, added automatically by the SDK (`event_timestamp`).
- **User properties** (set once or on change): `install_date`, `platform` (ios/android), `app_version`, `country`, `ab_group` (pipe-delimited list of active experiments, e.g. `tut_short|moves_gen`).

---

## 1. Session Events

### `session_start`

Fires when the app enters foreground and no active session exists (30-second debounce).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `session_id` | string | UUID for this session | `"a1b2c3d4-..."` |
| `session_number` | int | Lifetime session count for this device | `14` |
| `days_since_install` | int | Calendar days since first install | `3` |

```json
{
  "event": "session_start",
  "params": {
    "session_id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "session_number": 14,
    "days_since_install": 3
  }
}
```

### `session_end`

Fires when the app backgrounds for > 30 seconds or is terminated.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `session_id` | string | Matches the opening `session_start` | `"a1b2c3d4-..."` |
| `duration_sec` | int | Wall-clock seconds from session_start | `387` |
| `levels_played` | int | Levels started during this session | `5` |
| `levels_completed` | int | Levels completed during this session | `4` |

```json
{
  "event": "session_end",
  "params": {
    "session_id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "duration_sec": 387,
    "levels_played": 5,
    "levels_completed": 4
  }
}
```

---

## 2. Tutorial Events

### `tutorial_start`

Fires when the player taps "Play" on the title screen for the first time and the tutorial sequence begins.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `tutorial_version` | string | A/B variant identifier | `"v1_5step"` |

```json
{
  "event": "tutorial_start",
  "params": {
    "tutorial_version": "v1_5step"
  }
}
```

### `tutorial_step_complete`

Fires each time a tutorial step (hand-hold prompt) is dismissed by the player.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `step_id` | string | Unique step identifier | `"tap_cluster"` |
| `step_index` | int | 1-based ordinal position | `2` |
| `time_on_step_sec` | int | Seconds spent on this step | `8` |

```json
{
  "event": "tutorial_step_complete",
  "params": {
    "step_id": "tap_cluster",
    "step_index": 2,
    "time_on_step_sec": 8
  }
}
```

### `tutorial_complete`

Fires when the final tutorial step is completed.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `total_time_sec` | int | Total seconds from `tutorial_start` to now | `120` |
| `steps_completed` | int | Number of steps completed | `5` |

```json
{
  "event": "tutorial_complete",
  "params": {
    "total_time_sec": 120,
    "steps_completed": 5
  }
}
```

### `tutorial_skip`

Fires if the player taps "Skip" at any point during the tutorial (if skip is offered).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `step_id` | string | Step the player was on when they skipped | `"chain_orbs"` |
| `step_index` | int | Ordinal position of that step | `3` |
| `time_in_tutorial_sec` | int | Seconds elapsed since `tutorial_start` | `45` |

```json
{
  "event": "tutorial_skip",
  "params": {
    "step_id": "chain_orbs",
    "step_index": 3,
    "time_in_tutorial_sec": 45
  }
}
```

---

## 3. Level Events

### `level_start`

Fires when the level board is loaded and the player can make their first move.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `level_id` | int | Level number (1-based) | `12` |
| `level_type` | string | `"main"`, `"daily"`, `"event"` | `"main"` |
| `attempt_number` | int | How many times this user has started this level | `2` |
| `boosters_equipped` | string | Comma-separated list of booster type IDs, or `"none"` | `"extra_moves,color_bomb"` |
| `moves_budget` | int | Total moves granted for this attempt | `25` |

```json
{
  "event": "level_start",
  "params": {
    "level_id": 12,
    "level_type": "main",
    "attempt_number": 2,
    "boosters_equipped": "extra_moves,color_bomb",
    "moves_budget": 25
  }
}
```

### `level_move`

Fires after every player move resolves. **High volume** — batch or sample at 100% during soft-launch, then evaluate cost.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `level_id` | int | Current level | `12` |
| `move_number` | int | 1-based move index | `7` |
| `action_type` | string | `"tap_cluster"`, `"use_booster"`, `"swap"` | `"tap_cluster"` |
| `result` | string | `"fuse"`, `"chain"`, `"no_match"`, `"booster_effect"` | `"fuse"` |
| `board_state_hash` | string | 8-char hash of board state (for replay/debug) | `"3f8a21bc"` |

```json
{
  "event": "level_move",
  "params": {
    "level_id": 12,
    "move_number": 7,
    "action_type": "tap_cluster",
    "result": "fuse",
    "board_state_hash": "3f8a21bc"
  }
}
```

### `level_fusion`

Fires each time a cluster tap results in a fusion (ingredients merging into an orb, or orb growing).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `level_id` | int | Current level | `12` |
| `cluster_size` | int | Number of tiles in the tapped cluster | `4` |
| `ingredient_type` | string | Ingredient ID that was fused | `"moonpetal"` |
| `orb_tier` | int | Resulting orb tier (1 = new orb, 2+ = growth) | `2` |
| `chain_depth` | int | How many consecutive automatic fusions cascaded from this tap | `3` |

```json
{
  "event": "level_fusion",
  "params": {
    "level_id": 12,
    "cluster_size": 4,
    "ingredient_type": "moonpetal",
    "orb_tier": 2,
    "chain_depth": 3
  }
}
```

### `level_brew`

Fires each time a potion recipe is completed within a level.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `level_id` | int | Current level | `12` |
| `potion_type` | string | Recipe/potion ID | `"healing_elixir"` |
| `total_fusions` | int | Number of fusions used to complete this potion | `6` |
| `moves_used` | int | Moves consumed before this brew | `14` |

```json
{
  "event": "level_brew",
  "params": {
    "level_id": 12,
    "potion_type": "healing_elixir",
    "total_fusions": 6,
    "moves_used": 14
  }
}
```

### `level_complete`

Fires when all recipe targets are fulfilled and the victory screen appears.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `level_id` | int | Level number | `12` |
| `level_type` | string | `"main"`, `"daily"`, `"event"` | `"main"` |
| `stars` | int | Star rating earned (1–3) | `3` |
| `moves_used` | int | Moves consumed | `18` |
| `moves_remaining` | int | Moves left over | `7` |
| `score` | int | Final score | `12400` |
| `time_spent_sec` | int | Wall-clock seconds from `level_start` | `94` |
| `boosters_used` | string | Boosters consumed during play | `"extra_moves"` |
| `attempt_number` | int | Which attempt this was | `2` |

```json
{
  "event": "level_complete",
  "params": {
    "level_id": 12,
    "level_type": "main",
    "stars": 3,
    "moves_used": 18,
    "moves_remaining": 7,
    "score": 12400,
    "time_spent_sec": 94,
    "boosters_used": "extra_moves",
    "attempt_number": 2
  }
}
```

### `level_fail`

Fires when all moves are exhausted without completing all recipes.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `level_id` | int | Level number | `12` |
| `level_type` | string | `"main"`, `"daily"`, `"event"` | `"main"` |
| `moves_used` | int | Total moves consumed (equals budget) | `25` |
| `recipes_total` | int | Number of potions required | `3` |
| `recipes_completed` | int | Number of potions brewed before failure | `2` |
| `recipe_progress_pct` | int | 0–100 % progress on the incomplete potion | `65` |
| `closest_potion_distance` | int | Fewest fusions remaining to complete any unfinished potion | `2` |
| `time_spent_sec` | int | Wall-clock seconds | `110` |
| `attempt_number` | int | Which attempt | `1` |

```json
{
  "event": "level_fail",
  "params": {
    "level_id": 12,
    "level_type": "main",
    "moves_used": 25,
    "recipes_total": 3,
    "recipes_completed": 2,
    "recipe_progress_pct": 65,
    "closest_potion_distance": 2,
    "time_spent_sec": 110,
    "attempt_number": 1
  }
}
```

---

## 4. Economy Events

### `currency_earn`

Fires each time a player receives currency from any source.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `currency_type` | string | `"essence"` or `"gem"` | `"essence"` |
| `amount` | int | Quantity earned | `200` |
| `source` | string | Origin: `"level_complete"`, `"daily_brew"`, `"streak_bonus"`, `"event_reward"`, `"rewarded_ad"`, `"iap"`, `"workshop_refund"` | `"level_complete"` |
| `balance_after` | int | Player's balance after earning | `1450` |

```json
{
  "event": "currency_earn",
  "params": {
    "currency_type": "essence",
    "amount": 200,
    "source": "level_complete",
    "balance_after": 1450
  }
}
```

### `currency_spend`

Fires each time a player spends currency.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `currency_type` | string | `"essence"` or `"gem"` | `"gem"` |
| `amount` | int | Quantity spent | `50` |
| `sink` | string | Destination: `"booster_purchase"`, `"workshop_upgrade"`, `"extra_moves"`, `"continue_after_fail"` | `"workshop_upgrade"` |
| `item_id` | string | ID of item purchased (if applicable) | `"cauldron_tier2"` |
| `balance_after` | int | Player's balance after spending | `30` |

```json
{
  "event": "currency_spend",
  "params": {
    "currency_type": "gem",
    "amount": 50,
    "sink": "workshop_upgrade",
    "item_id": "cauldron_tier2",
    "balance_after": 30
  }
}
```

### `booster_use`

Fires when a booster is activated during a level (in addition to tracking in `level_start` equipped list).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `booster_type` | string | Booster ID | `"color_bomb"` |
| `level_id` | int | Level where used | `12` |
| `level_type` | string | `"main"`, `"daily"`, `"event"` | `"main"` |
| `trigger` | string | `"pre_level"` (equipped) or `"mid_level"` (purchased during play) | `"pre_level"` |
| `currency_cost` | int | Cost paid (0 if free/equipped) | `0` |

```json
{
  "event": "booster_use",
  "params": {
    "booster_type": "color_bomb",
    "level_id": 12,
    "level_type": "main",
    "trigger": "pre_level",
    "currency_cost": 0
  }
}
```

---

## 5. Meta / Progression Events

### `workshop_upgrade`

Fires when a player completes a workshop restoration upgrade.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `upgrade_id` | string | Specific upgrade identifier | `"cauldron_tier2"` |
| `workshop_stage` | int | Overall workshop stage after upgrade | `3` |
| `cost_type` | string | `"essence"` or `"gem"` | `"essence"` |
| `cost_amount` | int | Amount spent | `500` |

```json
{
  "event": "workshop_upgrade",
  "params": {
    "upgrade_id": "cauldron_tier2",
    "workshop_stage": 3,
    "cost_type": "essence",
    "cost_amount": 500
  }
}
```

### `potion_collected`

Fires when a player adds a new potion to their shelf for the first time (collection milestone).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `potion_id` | string | Potion identifier | `"healing_elixir"` |
| `collection_count` | int | Total unique potions now collected | `8` |
| `collection_pct` | int | % of total potions collected (0–100) | `20` |

```json
{
  "event": "potion_collected",
  "params": {
    "potion_id": "healing_elixir",
    "collection_count": 8,
    "collection_pct": 20
  }
}
```

### `streak_milestone`

Fires when the player's consecutive daily play streak reaches a milestone (every day the streak extends).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `streak_count` | int | Current streak length in days | `7` |
| `is_milestone` | bool | True if this is a reward tier (3, 7, 14, 30) | `true` |
| `reward_type` | string | Reward given (if milestone) | `"gem"` |
| `reward_amount` | int | Quantity (if milestone) | `25` |

```json
{
  "event": "streak_milestone",
  "params": {
    "streak_count": 7,
    "is_milestone": true,
    "reward_type": "gem",
    "reward_amount": 25
  }
}
```

### `daily_brew_complete`

Fires when the player completes the daily brew challenge.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `day_count` | int | Consecutive daily brews completed | `4` |
| `day_of_week` | string | `"monday"` … `"sunday"` | `"wednesday"` |
| `time_spent_sec` | int | Seconds to complete | `85` |
| `moves_used` | int | Moves used | `15` |
| `reward_essence` | int | Essence earned | `200` |
| `reward_gems` | int | Gems earned | `5` |

```json
{
  "event": "daily_brew_complete",
  "params": {
    "day_count": 4,
    "day_of_week": "wednesday",
    "time_spent_sec": 85,
    "moves_used": 15,
    "reward_essence": 200,
    "reward_gems": 5
  }
}
```

---

## 6. Monetization Events

### `rewarded_ad_offered`

Fires when a rewarded-ad prompt is displayed to the player.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `placement` | string | Where in the game: `"fail_extra_moves"`, `"bonus_essence"`, `"free_booster"`, `"double_reward"` | `"fail_extra_moves"` |
| `level_id` | int | Associated level (if applicable) | `12` |
| `ad_network` | string | Mediation network selected | `"admob"` |

```json
{
  "event": "rewarded_ad_offered",
  "params": {
    "placement": "fail_extra_moves",
    "level_id": 12,
    "ad_network": "admob"
  }
}
```

### `rewarded_ad_watched`

Fires when the player watches a rewarded ad to completion.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `placement` | string | Same as `rewarded_ad_offered` | `"fail_extra_moves"` |
| `reward_type` | string | `"extra_moves"`, `"essence"`, `"booster"`, `"multiplier"` | `"extra_moves"` |
| `reward_amount` | int | Quantity rewarded | `5` |
| `level_id` | int | Associated level | `12` |
| `watch_duration_sec` | int | Ad playback duration | `30` |
| `ad_network` | string | Network that served the ad | `"admob"` |

```json
{
  "event": "rewarded_ad_watched",
  "params": {
    "placement": "fail_extra_moves",
    "reward_type": "extra_moves",
    "reward_amount": 5,
    "level_id": 12,
    "watch_duration_sec": 30,
    "ad_network": "admob"
  }
}
```

### `rewarded_ad_declined`

Fires when the player dismisses a rewarded-ad offer without watching.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `placement` | string | Same placement key | `"fail_extra_moves"` |
| `level_id` | int | Associated level | `12` |
| `time_on_prompt_sec` | int | Seconds the prompt was visible before dismissal | `3` |

```json
{
  "event": "rewarded_ad_declined",
  "params": {
    "placement": "fail_extra_moves",
    "level_id": 12,
    "time_on_prompt_sec": 3
  }
}
```

### `interstitial_shown`

Fires when an interstitial ad is displayed.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `trigger` | string | `"post_level"`, `"return_to_map"` | `"post_level"` |
| `levels_since_last` | int | Levels completed since last interstitial | `3` |
| `session_interstitial_count` | int | Interstitials shown this session | `2` |
| `ad_network` | string | Network | `"admob"` |

```json
{
  "event": "interstitial_shown",
  "params": {
    "trigger": "post_level",
    "levels_since_last": 3,
    "session_interstitial_count": 2,
    "ad_network": "admob"
  }
}
```

### `iap_offer_shown`

Fires when an IAP offer UI is presented to the player.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `sku` | string | Product identifier | `"gem_pack_small"` |
| `price_usd` | float | Price in USD | `1.99` |
| `placement` | string | Where shown: `"shop"`, `"fail_popup"`, `"special_offer"`, `"out_of_gems"` | `"fail_popup"` |
| `is_first_offer` | bool | True if this is the player's first time seeing any IAP offer | `true` |

```json
{
  "event": "iap_offer_shown",
  "params": {
    "sku": "gem_pack_small",
    "price_usd": 1.99,
    "placement": "fail_popup",
    "is_first_offer": true
  }
}
```

### `iap_purchase_start`

Fires when the player taps "Buy" and the store payment flow begins.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `sku` | string | Product identifier | `"gem_pack_small"` |
| `price_usd` | float | Listed price | `1.99` |
| `placement` | string | Where initiated | `"shop"` |

```json
{
  "event": "iap_purchase_start",
  "params": {
    "sku": "gem_pack_small",
    "price_usd": 1.99,
    "placement": "shop"
  }
}
```

### `iap_purchase_complete`

Fires when the store confirms a successful transaction.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `sku` | string | Product identifier | `"gem_pack_small"` |
| `price_usd` | float | Final price charged | `1.99` |
| `currency_code` | string | ISO 4217 currency code | `"USD"` |
| `local_price` | float | Price in local currency | `1.99` |
| `transaction_id` | string | Store transaction ID | `"GPA.1234-5678"` |
| `is_first_purchase` | bool | First ever IAP for this user | `true` |
| `placement` | string | Where initiated | `"shop"` |

```json
{
  "event": "iap_purchase_complete",
  "params": {
    "sku": "gem_pack_small",
    "price_usd": 1.99,
    "currency_code": "USD",
    "local_price": 1.99,
    "transaction_id": "GPA.1234-5678",
    "is_first_purchase": true,
    "placement": "shop"
  }
}
```

### `iap_purchase_fail`

Fires when a purchase attempt fails or is cancelled.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `sku` | string | Product identifier | `"gem_pack_small"` |
| `reason` | string | `"user_cancelled"`, `"payment_declined"`, `"network_error"`, `"store_error"` | `"user_cancelled"` |
| `placement` | string | Where initiated | `"shop"` |

```json
{
  "event": "iap_purchase_fail",
  "params": {
    "sku": "gem_pack_small",
    "reason": "user_cancelled",
    "placement": "shop"
  }
}
```

---

## 7. Event / Live-Ops Events

### `event_start`

Fires when a player opens an event for the first time (enters the event map).

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `event_id` | string | Event identifier | `"lunar_brew_w3"` |
| `event_type` | string | `"weekly"`, `"seasonal"`, `"special"` | `"weekly"` |
| `days_since_install` | int | Player maturity | `5` |
| `current_level` | int | Player's highest main-campaign level | `18` |

```json
{
  "event": "event_start",
  "params": {
    "event_id": "lunar_brew_w3",
    "event_type": "weekly",
    "days_since_install": 5,
    "current_level": 18
  }
}
```

### `event_level_complete`

Fires when a player completes a level within an event.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `event_id` | string | Event identifier | `"lunar_brew_w3"` |
| `event_level_id` | int | 1-based event level index | `3` |
| `stars` | int | Star rating | `2` |
| `moves_used` | int | Moves consumed | `20` |
| `time_spent_sec` | int | Seconds to complete | `78` |

```json
{
  "event": "event_level_complete",
  "params": {
    "event_id": "lunar_brew_w3",
    "event_level_id": 3,
    "stars": 2,
    "moves_used": 20,
    "time_spent_sec": 78
  }
}
```

### `event_complete`

Fires when a player finishes all levels in an event.

| Parameter | Type | Description | Example |
|-----------|------|-------------|---------|
| `event_id` | string | Event identifier | `"lunar_brew_w3"` |
| `total_stars` | int | Sum of stars across all event levels | `18` |
| `days_to_complete` | int | Calendar days from first event level to completion | `3` |
| `reward_essence` | int | Essence reward | `500` |
| `reward_gems` | int | Gem reward | `25` |
| `reward_potion` | string | Exclusive potion ID (if any) | `"lunar_glow"` |

```json
{
  "event": "event_complete",
  "params": {
    "event_id": "lunar_brew_w3",
    "total_stars": 18,
    "days_to_complete": 3,
    "reward_essence": 500,
    "reward_gems": 25,
    "reward_potion": "lunar_glow"
  }
}
```

---

## 8. Event Volume Estimates (Soft-Launch, 5K DAU)

| Event | Estimated Daily Volume | Storage Impact |
|-------|----------------------|----------------|
| `session_start` / `session_end` | 12.5 K each | Low |
| `tutorial_*` | < 1 K (new users only) | Negligible |
| `level_start` / `level_complete` / `level_fail` | 40 K / 30 K / 10 K | Low |
| `level_move` | **200 K** | **High — consider sampling** |
| `level_fusion` | 120 K | Medium |
| `level_brew` | 25 K | Low |
| `currency_earn` / `currency_spend` | 35 K / 20 K | Low |
| `rewarded_ad_*` | 8 K total | Low |
| `interstitial_shown` | 10 K | Low |
| `iap_*` | < 500 | Negligible |
| `event_*` | 5 K during events | Low |

**Cost note:** `level_move` generates ~60% of event volume. During soft-launch, log at 100% for game balance tuning. Post global launch at >50K DAU, move to 10% sampling or aggregate server-side.

---

## 9. Implementation Checklist

- [ ] All events use the common schema (event name + params map).
- [ ] `session_id` is attached as a global property to every event within a session.
- [ ] `device_id` and `user_id` (if authenticated) are set as user-level identifiers.
- [ ] Timestamps are in UTC.
- [ ] Offline events are queued and sent on next connectivity with original timestamps preserved.
- [ ] Event payload size is validated (< 25 KB per event for Firebase).
- [ ] QA test plan covers every event with a golden-path walkthrough and edge cases (e.g., force-kill mid-level).
- [ ] Data pipeline validates against this schema; unknown events or missing required params trigger alerts.
