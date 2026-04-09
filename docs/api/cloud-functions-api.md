# Cloud Functions API Contract

> Brew — Firebase Cloud Functions (2nd gen, Node.js 20)
> All functions deploy to `us-central1`. All require Firebase Auth JWT unless noted.

---

## Global Conventions

| Aspect | Detail |
|---|---|
| **Base URL** | `https://us-central1-<PROJECT_ID>.cloudfunctions.net` |
| **Auth** | `Authorization: Bearer <Firebase ID Token>` on every request |
| **Content-Type** | `application/json` for all POST bodies and responses |
| **Rate Limiting** | Per-UID token bucket; limits noted per endpoint |
| **Timestamps** | ISO 8601, UTC (`2026-04-09T14:30:00Z`) |
| **Idempotency** | POST endpoints accept an optional `idempotency_key` (UUID v4). Duplicate keys within 5 minutes return the cached response. |
| **Error Shape** | All errors return `{ "error": { "code": string, "message": string, "details"?: object } }` |

### Common Error Codes

| Code | HTTP | Meaning |
|---|---|---|
| `AUTH_REQUIRED` | 401 | Missing or expired JWT |
| `AUTH_INVALID` | 403 | Valid JWT but user banned or token revoked |
| `RATE_LIMITED` | 429 | Too many requests; retry after `Retry-After` header (seconds) |
| `INVALID_REQUEST` | 400 | Schema validation failed |
| `INTERNAL` | 500 | Unhandled server error |
| `MAINTENANCE` | 503 | Function temporarily disabled via Remote Config kill-switch |

---

## 1. `validateIAPReceipt`

Validates an in-app purchase receipt against Apple App Store or Google Play, grants purchased items, and records the transaction.

| Field | Value |
|---|---|
| **Endpoint** | `POST /validateIAPReceipt` |
| **Rate Limit** | 10 req/min per UID |
| **Idempotent** | Yes — duplicate `receipt_token` values return the original result |

### Request

```jsonc
{
  "platform": "ios" | "android",
  "product_id": "com.brew.gems_100",      // Store product identifier
  "receipt_token": "<base64 receipt>",      // iOS: App Store receipt. Android: purchase token.
  "transaction_id": "GPA.3312-...",         // Platform transaction ID
  "idempotency_key": "550e8400-..."         // Optional
}
```

| Param | Type | Required | Constraints |
|---|---|---|---|
| `platform` | `"ios" \| "android"` | Yes | Enum |
| `product_id` | `string` | Yes | Must match a known product in server catalog |
| `receipt_token` | `string` | Yes | Max 50 KB |
| `transaction_id` | `string` | Yes | Platform-specific format |
| `idempotency_key` | `string` | No | UUID v4 |

### Response — 200 OK

```jsonc
{
  "valid": true,
  "product_id": "com.brew.gems_100",
  "granted_items": {
    "gems": 100,
    "essence": 0,
    "items": []               // e.g. ["booster_catalyst_x3"]
  },
  "transaction_id": "GPA.3312-...",
  "server_timestamp": "2026-04-09T14:30:00Z"
}
```

### Response — 200 (Invalid Receipt)

```jsonc
{
  "valid": false,
  "reason": "RECEIPT_INVALID",  // or "RECEIPT_ALREADY_USED", "PRODUCT_MISMATCH"
  "server_timestamp": "2026-04-09T14:30:00Z"
}
```

### Error Codes

| Code | HTTP | When |
|---|---|---|
| `RECEIPT_INVALID` | 200 | Receipt failed Apple/Google verification |
| `RECEIPT_ALREADY_USED` | 200 | Receipt token already consumed for this user |
| `PRODUCT_MISMATCH` | 200 | `product_id` does not match receipt contents |
| `PLATFORM_UNAVAILABLE` | 502 | Apple/Google verification server unreachable |
| `UNKNOWN_PRODUCT` | 400 | `product_id` not in server catalog |

### Implementation Notes

- iOS: Call Apple's `verifyReceipt` endpoint (production first, sandbox fallback for status 21007).
- Android: Use Google Play Developer API `purchases.products.get` with service account credentials.
- On success: write to `users/{uid}/transactions` subcollection, update currency balances atomically via Firestore transaction.
- Log every attempt (valid or not) to `iap_audit_log` collection for fraud analysis.

---

## 2. `syncPlayerState`

Client uploads its full player state snapshot. Server validates data integrity, merges with the authoritative server copy (server wins on conflicts), and returns the merged result.

| Field | Value |
|---|---|
| **Endpoint** | `POST /syncPlayerState` |
| **Rate Limit** | 5 req/min per UID |
| **Idempotent** | Yes — `idempotency_key` supported |

### Request

```jsonc
{
  "client_state": {
    "version": 3,                           // Schema version for forward compat
    "last_sync_timestamp": "2026-04-09T14:00:00Z",
    "currencies": {
      "essence": 1250,
      "gems": 47
    },
    "level_progress": {
      "current_chapter": 2,
      "current_level": 14,                  // Monotonically increasing
      "stars": { "1": 3, "2": 2, "13": 1 } // level_id → star count (1-3)
    },
    "workshop": {
      "cauldron_level": 3,
      "flask_level": 2,
      "mortar_level": 1
    },
    "potions": [
      { "potion_id": "clarity", "quantity": 2 }
    ],
    "boosters": {
      "shake": 5,
      "catalyst": 2,
      "transmute": 1
    },
    "streaks": {
      "current": 4,
      "best": 7,
      "last_win_date": "2026-04-09"
    },
    "daily_brew": {
      "last_completed_date": "2026-04-08",
      "total_completed": 12
    },
    "settings": {
      "sfx_enabled": true,
      "music_enabled": true,
      "notifications_enabled": true,
      "haptics_enabled": true
    }
  },
  "idempotency_key": "550e8400-..."
}
```

### Validation Rules (Server-Side)

| Rule | Rejection Trigger |
|---|---|
| Currency non-negative | `essence < 0` or `gems < 0` |
| Level progress monotonic | `current_level` decreased vs. server |
| Star bounds | Star count not in `{1, 2, 3}` |
| Workshop level bounds | Any upgrade level < 0 or > max defined in config |
| Booster non-negative | Any booster count < 0 |
| Streak plausibility | `current` streak > calendar days since account creation |
| Schema version | `version` must be ≥ minimum supported version in Remote Config |

### Merge Strategy

| Field | Rule |
|---|---|
| `currencies` | **Server wins.** Client values ignored; balances are only modified by server-validated transactions. |
| `level_progress` | **Max wins.** `current_level = max(client, server)`. Stars per level = max of both. |
| `workshop` | **Server wins.** Upgrades only via validated transactions. |
| `potions` / `boosters` | **Server wins.** |
| `streaks` | **Server wins** for `current` and `best`. Client `last_win_date` accepted if it equals today. |
| `settings` | **Client wins.** These are local preferences. |

### Response — 200 OK

```jsonc
{
  "merged_state": { /* same shape as client_state */ },
  "sync_timestamp": "2026-04-09T14:30:00Z",
  "conflicts": [
    {
      "field": "currencies.essence",
      "client_value": 1250,
      "server_value": 1100,
      "resolution": "server_wins"
    }
  ]
}
```

### Error Codes

| Code | HTTP | When |
|---|---|---|
| `STATE_VALIDATION_FAILED` | 400 | Integrity rules violated; `details` lists every violation |
| `SCHEMA_VERSION_TOO_OLD` | 400 | Client must update app before syncing |
| `SYNC_CONFLICT_UNRESOLVABLE` | 409 | Rare: manual intervention needed (e.g., clock manipulation detected) |

---

## 3. `getDailyBrew`

Returns today's daily challenge level definition. All players receive the same level, seeded by the UTC date.

| Field | Value |
|---|---|
| **Endpoint** | `GET /getDailyBrew` |
| **Rate Limit** | 30 req/min per UID |
| **Cache** | CDN-cacheable; `Cache-Control: public, max-age=3600` |

### Request

No body. Auth header required.

Query params: none.

### Response — 200 OK

```jsonc
{
  "date": "2026-04-09",
  "level": {
    "level_id": "daily_2026-04-09",
    "board_width": 7,
    "board_height": 9,
    "move_limit": 25,
    "recipe": [
      { "potion_id": "clarity", "required_brews": 2 },
      { "potion_id": "vigor", "required_brews": 1 }
    ],
    "token_distribution": {
      "fire": 0.20,
      "water": 0.20,
      "earth": 0.20,
      "air": 0.20,
      "spirit": 0.20
    },
    "special_rules": [],
    "seed": 20260409
  },
  "rewards": {
    "completion": { "gems": 5, "essence": 100 },
    "three_star": { "gems": 10, "essence": 200 }
  },
  "already_completed": false,
  "expires_at": "2026-04-10T00:00:00Z"
}
```

### Error Codes

| Code | HTTP | When |
|---|---|---|
| `DAILY_BREW_DISABLED` | 404 | Feature flag `daily_brew_enabled` is `false` |

---

## 4. `getActiveEvents`

Returns all currently active weekly/seasonal events with their level pools and reward configurations.

| Field | Value |
|---|---|
| **Endpoint** | `GET /getActiveEvents` |
| **Rate Limit** | 20 req/min per UID |
| **Cache** | `Cache-Control: public, max-age=300` (5 min) |

### Response — 200 OK

```jsonc
{
  "events": [
    {
      "event_id": "spring_bloom_2026",
      "title": "Spring Bloom Festival",
      "description": "Brew rare floral potions to earn exclusive rewards!",
      "type": "weekly",
      "starts_at": "2026-04-07T00:00:00Z",
      "ends_at": "2026-04-14T00:00:00Z",
      "levels": [
        {
          "level_id": "event_spring_01",
          "board_width": 7,
          "board_height": 9,
          "move_limit": 30,
          "recipe": [
            { "potion_id": "blossom", "required_brews": 3 }
          ],
          "token_distribution": {
            "fire": 0.15,
            "water": 0.25,
            "earth": 0.25,
            "air": 0.15,
            "spirit": 0.20
          },
          "special_rules": ["bonus_earth_tokens"],
          "seed": 70001
        }
        // ... additional event levels
      ],
      "reward_tiers": [
        { "threshold": 3,  "rewards": { "essence": 200, "gems": 0 } },
        { "threshold": 7,  "rewards": { "essence": 500, "gems": 10 } },
        { "threshold": 12, "rewards": { "essence": 1000, "gems": 25, "items": ["potion_blossom_x1"] } }
      ],
      "player_progress": {
        "levels_completed": 5,
        "current_tier": 1,
        "rewards_claimed": [0]
      }
    }
  ],
  "server_time": "2026-04-09T14:30:00Z"
}
```

### Error Codes

| Code | HTTP | When |
|---|---|---|
| `EVENTS_DISABLED` | 404 | Feature flag `weekly_events_enabled` is `false` |

---

## 5. `submitLevelResult`

Client submits level completion data. Server validates plausibility, updates player state, and returns rewards.

| Field | Value |
|---|---|
| **Endpoint** | `POST /submitLevelResult` |
| **Rate Limit** | 15 req/min per UID |
| **Idempotent** | Yes — duplicate `level_id` + `attempt_id` pairs return cached result |

### Request

```jsonc
{
  "level_id": "2-14",                       // or "daily_2026-04-09", "event_spring_01"
  "attempt_id": "a1b2c3d4-...",             // Client-generated UUID for this attempt
  "result": "win",                          // "win" | "lose"
  "stars": 3,                               // 1-3, only present on win
  "score": 14250,
  "moves_used": 18,
  "moves_limit": 25,                        // Client's view of the move limit
  "potions_brewed": [
    { "potion_id": "clarity", "count": 2 },
    { "potion_id": "vigor", "count": 1 }
  ],
  "boosters_used": [
    { "booster_id": "shake", "count": 1 }
  ],
  "extra_moves_purchased": 0,              // Number of +5 move packs bought mid-level
  "duration_seconds": 142,
  "client_timestamp": "2026-04-09T14:30:00Z",
  "idempotency_key": "550e8400-..."
}
```

| Param | Type | Required | Constraints |
|---|---|---|---|
| `level_id` | `string` | Yes | Must exist in level catalog or be a valid daily/event ID |
| `attempt_id` | `string` | Yes | UUID v4 |
| `result` | `"win" \| "lose"` | Yes | |
| `stars` | `int` | If `result == "win"` | 1–3 |
| `score` | `int` | Yes | ≥ 0 |
| `moves_used` | `int` | Yes | 1 ≤ n ≤ `moves_limit` + (`extra_moves_purchased` × 5) |
| `moves_limit` | `int` | Yes | Must match server's level definition |
| `potions_brewed` | `array` | Yes | Each `potion_id` must be in level recipe |
| `boosters_used` | `array` | Yes | Each `booster_id` must be a known booster type |
| `extra_moves_purchased` | `int` | Yes | ≥ 0 |
| `duration_seconds` | `int` | Yes | > 0, < 3600 |
| `client_timestamp` | `string` | Yes | ISO 8601 |

### Server Validation

| Check | Rule | Failure Code |
|---|---|---|
| Level exists | `level_id` in catalog | `UNKNOWN_LEVEL` |
| Move limit match | Client `moves_limit` == server definition (+ extra moves) | `MOVE_LIMIT_MISMATCH` |
| Score plausibility | `score` ≤ theoretical max for level (precomputed) | `SCORE_IMPLAUSIBLE` |
| Moves plausibility | `moves_used` ≤ `moves_limit` + (`extra_moves_purchased` × 5) | `MOVES_EXCEEDED` |
| Duration plausibility | `duration_seconds` ≥ `moves_used × 0.5` (half-second minimum per move) | `DURATION_IMPLAUSIBLE` |
| Booster ownership | Player owns the boosters claimed as used | `BOOSTER_NOT_OWNED` |
| Stars match score | Star thresholds match level definition | `STAR_MISMATCH` |
| Not already completed with higher stars | Only process if new stars > existing (for replays) | Skip reward, return existing state |

### Response — 200 OK (Win)

```jsonc
{
  "accepted": true,
  "rewards": {
    "essence": 150,                         // Based on star count × essence_per_star
    "gems": 0,
    "potions": [
      { "potion_id": "clarity", "quantity": 2 }
    ],
    "streak_bonus": {
      "multiplier": 1.5,
      "extra_essence": 75
    }
  },
  "updated_state": {
    "currencies": { "essence": 1325, "gems": 47 },
    "level_progress": { "current_chapter": 2, "current_level": 15 },
    "streaks": { "current": 5, "best": 7 }
  },
  "new_best": true,                         // True if this submission improved star rating
  "server_timestamp": "2026-04-09T14:30:00Z"
}
```

### Response — 200 OK (Lose)

```jsonc
{
  "accepted": true,
  "rewards": null,
  "updated_state": {
    "currencies": { "essence": 1100, "gems": 47 },
    "streaks": { "current": 0, "best": 7 }   // Streak resets on loss (unless protected)
  },
  "streak_protection_used": false,
  "server_timestamp": "2026-04-09T14:30:00Z"
}
```

### Error Codes

| Code | HTTP | When |
|---|---|---|
| `UNKNOWN_LEVEL` | 400 | Level ID not found |
| `SCORE_IMPLAUSIBLE` | 400 | Flagged; logged for review |
| `MOVES_EXCEEDED` | 400 | `moves_used` exceeds allowed maximum |
| `DURATION_IMPLAUSIBLE` | 400 | Impossibly fast completion |
| `BOOSTER_NOT_OWNED` | 400 | Player doesn't have the claimed boosters |
| `MOVE_LIMIT_MISMATCH` | 400 | Client sent wrong move limit |
| `STAR_MISMATCH` | 400 | Claimed stars don't match score thresholds |

---

## 6. `getRemoteConfig`

Returns all tunable game parameters. The client caches the response locally and refreshes on a TTL basis.

| Field | Value |
|---|---|
| **Endpoint** | `GET /getRemoteConfig` |
| **Rate Limit** | 10 req/min per UID |
| **Cache** | `Cache-Control: public, max-age=1800` (30 min) |

### Request

Query params:

| Param | Type | Required | Description |
|---|---|---|---|
| `client_version` | `string` | Yes | e.g. `"1.2.0"` — server may return version-specific overrides |
| `platform` | `string` | Yes | `"ios"` or `"android"` |
| `config_etag` | `string` | No | ETag from previous response; if unchanged, server returns 304 |

### Response — 200 OK

```jsonc
{
  "config": {
    // Economy
    "essence_per_star_1": 50,
    "essence_per_star_2": 100,
    "essence_per_star_3": 150,
    "gem_daily_brew_reward": 5,
    "workshop_upgrade_costs": [100, 250, 500, 1000, 2000],
    "booster_cost_shake_essence": 200,
    "booster_cost_shake_gems": 5,
    "booster_cost_catalyst_essence": 350,
    "booster_cost_catalyst_gems": 8,
    "extra_moves_gem_cost": 10,

    // Gameplay
    "default_move_limit": 25,
    "min_cluster_size": 3,
    "brew_threshold": 7,
    "cascade_settle_delay_ms": 100,
    "max_boosters_per_level": 3,

    // Ads
    "rewarded_ad_extra_moves_count": 5,
    "interstitial_frequency": 3,
    "max_rewarded_ads_per_session": 5,
    "ad_cooldown_seconds": 30,

    // Streaks
    "streak_multipliers": [1.0, 1.0, 1.25, 1.5, 1.75, 2.0, 2.0, 2.5],
    "streak_protection_enabled": true,

    // Feature flags
    "daily_brew_enabled": true,
    "weekly_events_enabled": true,
    "workshop_enabled": true,
    "win_streak_enabled": true,

    // Event
    "active_event_id": "spring_bloom_2026",
    "event_config": "{ ... }",

    // Tuning
    "difficulty_modifier": 1.0
  },
  "etag": "W/\"a1b2c3\"",
  "schema_version": 3,
  "server_timestamp": "2026-04-09T14:30:00Z"
}
```

### Response — 304 Not Modified

Returned when `config_etag` matches current server ETag. Empty body.

### Error Codes

| Code | HTTP | When |
|---|---|---|
| `UNSUPPORTED_VERSION` | 400 | `client_version` below minimum supported |

---

## 7. `reportCheat`

Client-side anomaly detection flags suspicious activity for manual review. This is fire-and-forget from the client's perspective.

| Field | Value |
|---|---|
| **Endpoint** | `POST /reportCheat` |
| **Rate Limit** | 3 req/min per UID |
| **Auth** | Required (the reporter must be authenticated) |

### Request

```jsonc
{
  "report_type": "self",                   // "self" = client detected own anomaly
  "anomaly_type": "impossible_score",       // Enum of detection types
  "level_id": "2-14",
  "details": {
    "expected_max_score": 15000,
    "actual_score": 99999,
    "moves_used": 1,
    "duration_seconds": 2
  },
  "client_version": "1.2.0",
  "device_info": {
    "platform": "android",
    "os_version": "14",
    "device_model": "Pixel 8",
    "is_emulator": false,
    "is_rooted": true
  },
  "client_timestamp": "2026-04-09T14:30:00Z"
}
```

| `anomaly_type` Values | Description |
|---|---|
| `impossible_score` | Score exceeds theoretical maximum |
| `impossible_speed` | Level completed faster than physically possible |
| `currency_mismatch` | Local currency doesn't match expected after transaction |
| `modified_binary` | Integrity check on game binary failed |
| `time_manipulation` | Device clock moved backwards between events |
| `memory_tampering` | Memory scanner or cheat engine detected |

### Response — 200 OK

```jsonc
{
  "received": true,
  "report_id": "rpt_a1b2c3d4"
}
```

The server always returns 200 to avoid leaking detection status. Reports are written to a `cheat_reports` Firestore collection and flagged for manual review. Repeated reports from the same UID increment a `suspicion_score` on the user document.

### Error Codes

None exposed to client. Malformed requests are silently dropped and logged.

---

## Authentication Flow

All endpoints require a valid Firebase Auth ID token:

```
Authorization: Bearer eyJhbGciOiJSUzI1NiIs...
```

1. Client signs in via Firebase Auth (Anonymous → upgrade to email/Apple/Google).
2. Client calls `FirebaseAuth.currentUser.getIdToken()` before each API call.
3. Server verifies token with `admin.auth().verifyIdToken(token)`.
4. Server extracts `uid` from decoded token for all database operations.
5. Tokens expire after 1 hour; the Firebase SDK refreshes them automatically.

### Banned Users

If `users/{uid}.banned == true`, all endpoints return:

```jsonc
// 403 Forbidden
{
  "error": {
    "code": "AUTH_INVALID",
    "message": "Account suspended. Contact support."
  }
}
```

---

## Deployment & Infrastructure

| Concern | Detail |
|---|---|
| **Runtime** | Cloud Functions 2nd gen, Node.js 20, 256 MB / 1 vCPU |
| **Timeout** | 60s default; `validateIAPReceipt` gets 120s (external API calls) |
| **Min instances** | 1 for `submitLevelResult` and `syncPlayerState` (latency-sensitive) |
| **Secrets** | Apple shared secret and Google service account key stored in Secret Manager, injected via `defineSecret()` |
| **Monitoring** | All functions emit structured logs. Error rate alerts at > 1% over 5-min window. Latency P99 alert at > 5s. |
| **Kill switch** | Each function checks `Remote Config` for `<function_name>_enabled`. If `false`, returns 503 MAINTENANCE. |
