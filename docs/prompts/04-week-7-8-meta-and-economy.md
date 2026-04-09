# Week 7-8: Meta Systems & Economy

You are continuing work on **Brew**, a mobile puzzle game at `T:\Brew`. This prompt covers Weeks 7-8: building meta-progression systems, implementing the full economy, integrating IAP and ads, and wiring up Firebase backend services.

---

## Prerequisites

Before starting this phase, verify the **Week 5-6 milestone** has PASSED. Run through `docs/production/milestone-gates.md` Gate 3 criteria:

- Core gameplay feels good — tap, fuse, brew, cascade loop is satisfying
- Tutorial teaches all mechanics and completes at 85%+ rate in playtesting
- All 3 booster types work correctly
- Art and sound are integrated — tokens animate, SFX play, particles fire
- VFX juice (screen shake, glow, combo text) is in place

If any of these fail, stop and fix them before proceeding. Do not build meta systems on top of broken core gameplay.

---

## Step 1 — Read Source Documents

Read these files in order before writing any code:

1. `AGENTS.md` — project rules, architecture, naming conventions
2. `.cursor/rules/brew-project.mdc` — project-specific Cursor rules
3. `.cursor/rules/economy-guard.mdc` — economy coding rules
4. `docs/game-design/meta-systems.md` — full spec for potion shelf, workshop, streaks, daily brew, weekly events
5. `docs/economy/economy-model.md` — currency flows, earning rates, cost tables, pacing targets
6. `docs/economy/monetization-plan.md` — IAP catalog, ad placements, frequency caps, ethical guardrails
7. `docs/technical/architecture.md` — Firebase integration architecture, backend service design
8. `docs/analytics/event-tracking-plan.md` — every analytics event that must fire
9. `docs/project-management/session-handoff.md` — current progress and blockers

---

## Step 2 — Week 7: Meta Progression Systems

Build these six systems. Each must follow the project's architecture: pure C# logic in `Scripts/Core/`, MonoBehaviours only for presentation in `Scripts/Presentation/`, all tunable values in ScriptableObjects or Remote Config.

### 2.1 Potion Shelf UI

Reference: `docs/game-design/meta-systems.md` → Potion Shelf section.

- Create `PotionShelfManager.cs` in `Scripts/Core/Meta/` — tracks which potions the player has brewed, stores completion state
- Create `PotionShelfView.cs` in `Scripts/Presentation/Meta/` — visual collection grid accessible from the main menu
- Each unique potion type has a slot in the grid. Empty slots show a locked silhouette sprite. Filled slots show the potion with a soft glow shader effect
- Milestone rewards trigger at 25%, 50%, 75%, and 100% shelf completion. Rewards are defined in `PotionShelfConfig.asset` (ScriptableObject) — reference the reward values from `docs/economy/economy-model.md`
- When a new potion is brewed for the first time during gameplay, fire `potion_shelf_unlock` event and queue a shelf reveal animation for the results screen
- Persist shelf state to Firestore under the player document

### 2.2 Workshop Scene

Reference: `docs/game-design/meta-systems.md` → Workshop section.

- Create `WorkshopManager.cs` in `Scripts/Core/Meta/` — manages upgrade states, costs, purchase flow
- Create `WorkshopView.cs` in `Scripts/Presentation/Meta/` — renders the workshop room scene
- One room with 10-15 upgrade slots. Each upgrade has: name, Essence cost (pull from `docs/economy/economy-model.md` workshop cost table), visual decoration, locked/unlocked state
- Store all upgrade definitions in `WorkshopConfig.asset` (ScriptableObject). Costs must NOT be hardcoded in code
- Purchasing an upgrade: deduct Essence via `CurrencyManager`, play unlock animation (decoration fades/slides into place over 0.5-1s), fire `workshop_upgrade` analytics event
- Visual before/after must be clear — greyed-out placeholder → colorful decoration
- Workshop completion percentage displayed in the UI. This drives long-term retention

### 2.3 Win Streak System

Reference: `docs/game-design/meta-systems.md` → Win Streaks section.

- Create `WinStreakTracker.cs` in `Scripts/Core/Meta/` — tracks consecutive level completions
- Multiplier progression: 1x (0 streak) → 1.5x (1) → 2x (2) → 2.5x (3) → 3x (4+). These values come from `WinStreakConfig.asset`
- Streak resets to 0 on level failure
- HUD indicator shows current streak count and next multiplier tier with a fire/glow intensity that increases with streak
- Streak protection placeholder: when streak would reset, show a "Protect Streak?" prompt. Wire this to a rewarded ad callback but leave the ad call as a stub for now (Week 8 will connect it)
- Multiplier applies to Essence earned at level completion. Calculated in `RewardCalculator.cs`
- Fire `streak_update` event on every change (increment or reset)

### 2.4 Daily Brew

Reference: `docs/game-design/meta-systems.md` → Daily Brew section.

- Create `DailyBrewManager.cs` in `Scripts/Core/Meta/` — generates daily challenge, tracks completion
- One challenge level per day, seeded by the UTC date (`System.DateTime.UtcNow.Date.GetHashCode()` as seed). Level is procedurally selected/configured, not hand-authored
- Reward: 200 Essence + 5 Gems on completion. Values from `DailyBrewConfig.asset`
- Daily streak tracker: consecutive days of completion. Bonus reward at 7-day streak (defined in config)
- Main menu shows Daily Brew availability: "Available" with glow when not yet completed today, "Completed ✓" greyed when done
- Cooldown resets at midnight UTC. Use server time from Firebase if available, fall back to device time
- Fire `daily_brew_start`, `daily_brew_complete`, `daily_brew_streak` events

### 2.5 Essence Economy

- Create `CurrencyManager.cs` in `Scripts/Core/Economy/` — single source of truth for all currency operations
- Two currencies: `Essence` (soft) and `Gems` (premium). No others. Ever
- Earning Essence: level completion awards 50 / 80 / 120 for 1 / 2 / 3 stars, multiplied by win streak multiplier. Values from `EconomyConfig.asset`
- Spending Essence: workshop upgrades, booster purchases (costs from `docs/economy/economy-model.md`)
- All transactions go through `CurrencyManager.Add()` and `CurrencyManager.Spend()`. Spend returns false if insufficient. Balance cannot go below 0
- Fire `currency_earned` and `currency_spent` events with amount, source/sink, and new balance
- Persist balances to Firestore. Offline-first: update local immediately, sync to server when connected

### 2.6 Gem Economy

- Gems use the same `CurrencyManager` — just a different `CurrencyType` enum
- Gem sources: Daily Brew (5/day), shelf milestones (per config), weekly events, IAP purchase
- Gem sinks: booster purchases, extra moves (+3 for 10 Gems), cosmetics (future)
- Same transaction logging and analytics as Essence
- Gem balance shown in HUD alongside Essence

---

## Step 3 — Week 8: Monetization & Backend

### 3.1 IAP Integration

Reference: `docs/economy/monetization-plan.md` → IAP Catalog section.

- Integrate Unity IAP package for both iOS and Android
- Create `IAPManager.cs` in `Scripts/Core/Economy/` — handles initialization, purchase flow, receipt validation
- Implement Store UI as a modal accessible from main menu and from "Get More" buttons near currency displays
- Products from the monetization plan:
  - **Starter Bundle** — $1.99, one-time purchase: 500 Gems + 2000 Essence + 1 of each booster. Show "Best Value" badge. Only available until level 15
  - **Gem Packs** — $0.99 (80 Gems), $4.99 (500 Gems), $9.99 (1200 Gems), $19.99 (2800 Gems)
  - **No-Ads Pass** — $4.99, one-time purchase: removes all interstitial ads permanently
- Server-side receipt validation: send receipt to `validateReceipt` Cloud Function (see `docs/technical/architecture.md`). Only credit currency after server confirms valid receipt
- Handle edge cases: purchase interrupted, receipt pending, restore purchases, duplicate receipts
- Fire `iap_purchase_start`, `iap_purchase_complete`, `iap_purchase_fail` events with product ID and price

### 3.2 Rewarded Ads

Reference: `docs/economy/monetization-plan.md` → Ad Placements section.

- Integrate AdMob SDK via `AdManager.cs` in `Scripts/Core/Ads/`
- Four rewarded video placements:
  1. **Fail Recovery** — after losing a level, offer "+3 Moves" to continue. One chance per level attempt
  2. **Double Potion Reward** — on level complete, offer to double the potion/Essence reward
  3. **Free Daily Booster** — on main menu, offer one free booster per day via ad
  4. **Streak Protection** — connect to the stub from Week 7. Watch ad to prevent streak reset on fail
- Frequency cap: maximum 5 rewarded ads per session. Load this cap from Firebase Remote Config key `rewarded_ad_max_per_session` (default: 5)
- Pre-load ads so they're ready instantly when the placement triggers
- Fire `ad_rewarded_offer`, `ad_rewarded_start`, `ad_rewarded_complete`, `ad_rewarded_skip` events

### 3.3 Interstitial Ads

- Show interstitial ad after every 3rd level completion (not on fail, not during tutorial levels 1-5)
- Skippable after 5 seconds
- No-Ads Pass (`IAPManager.HasNoAdsPass`) suppresses all interstitials
- Frequency controlled by Remote Config key `interstitial_frequency` (default: 3)
- Fire `ad_interstitial_show` event

### 3.4 Firebase Integration

Reference: `docs/technical/architecture.md` → Firebase section.

- **Auth:** Anonymous authentication on first launch. Create `FirebaseAuthManager.cs`. User can link to email/Google/Apple later (post-MVP)
- **Firestore:** Player document at `players/{uid}` stores: currency balances, level progress, workshop state, potion shelf, win streak, daily brew history, settings. Create `CloudSaveManager.cs` with offline-first design — write to local cache first, sync to Firestore when online. Handle merge conflicts with last-write-wins for now
- **Remote Config:** Create `RemoteConfigManager.cs`. Fetch on app launch with 12-hour cache. All economy values, ad frequency caps, feature flags. Local fallback defaults in `config/firebase/remote-config-defaults.json`
- **Analytics:** Already partially wired from earlier phases. Now fire EVERY event listed in `docs/analytics/event-tracking-plan.md`. Create `AnalyticsManager.cs` as a single facade. Verify all events in Firebase DebugView during development

### 3.5 Analytics Completeness

- Go through `docs/analytics/event-tracking-plan.md` event by event
- For each event, verify: (a) it fires at the correct trigger point, (b) all required parameters are included, (c) parameter values are correct types
- Use Firebase DebugView on a real device to confirm events arrive. Log any missing events as P1 bugs
- Key events that MUST be verified this phase: all `currency_*`, all `iap_*`, all `ad_*`, `workshop_upgrade`, `potion_shelf_unlock`, `streak_update`, `daily_brew_*`

### 3.6 Remote Config Setup

- Create `config/firebase/remote-config-defaults.json` with all keys and their default values
- Keys must include: every economy value (earning rates, costs, prices), ad frequency caps (`rewarded_ad_max_per_session`, `interstitial_frequency`), feature flags (`weekly_events_enabled`, `daily_brew_enabled`), tuning knobs (`streak_multipliers`, `star_thresholds`)
- `RemoteConfigManager` loads defaults from this file on startup, then fetches server values. Server values override defaults
- Never read economy values directly from code. Always go through `RemoteConfigManager.GetInt()`, `GetFloat()`, etc.

---

## Step 4 — Milestone Gate (Week 7-8)

Before proceeding to Week 9-10, verify ALL of the following. Check against `docs/production/milestone-gates.md` Gate 4:

### Full Session Loop
Play this exact sequence on a real device:
1. Launch game → complete a level → earn Essence and Gems → see streak increment
2. Open Workshop → purchase an upgrade with Essence → see decoration appear
3. Return to main menu → complete another level → see streak multiplier boost earnings
4. Complete Daily Brew → earn 200 Essence + 5 Gems → see daily streak update
5. Fail a level → see streak protection offer → decline → streak resets

If this loop doesn't feel smooth and connected, it's not ready.

### IAP Verification
- Complete a purchase of every product on both iOS (sandbox) and Android (test track)
- Verify receipt validation Cloud Function returns success
- Verify currency is credited after server confirmation
- Verify Starter Bundle disappears after purchase
- Verify No-Ads Pass removes interstitials
- Verify "Restore Purchases" works on a fresh install with the same account

### Ad Verification
- Watch a rewarded ad at each of the 4 placements. Verify correct reward is granted
- Verify frequency cap stops offering ads after 5 in one session
- Verify interstitial shows after every 3rd level win (not on fail)
- Verify No-Ads Pass suppresses interstitials but not rewarded ads

### Analytics Verification
- Enable Firebase DebugView on a test device
- Play a full session covering all features
- Confirm every event from `docs/analytics/event-tracking-plan.md` appears with correct parameters

### Cloud Save Verification
- Play on Device A, reach level 10, purchase a workshop upgrade
- Sign into same anonymous account on Device B
- Verify: level progress, currency balances, workshop state, potion shelf all sync correctly

### Economy Pacing
- Run `python scripts/economy-sim/simulate_economy.py` with current config values
- Verify: a player completing 10 levels/day finishes the workshop in 30-45 days
- Verify: daily gem income (Daily Brew + shelf milestones) covers ~1 booster every 2-3 days without IAP

### Security
- Verify `CurrencyManager.Spend()` rejects negative amounts and amounts exceeding balance
- Verify currency cannot be earned by replaying already-completed levels (or if it can, it's intentional and capped)
- Verify IAP receipts are validated server-side, not client-only

---

## Step 5 — Session Completion

Before ending this session, follow the session completion protocol from `00-context-recovery.md` Step 6:

1. Update `docs/project-management/session-handoff.md`
2. Update `docs/project-management/project-summary.md` Decision Log if needed
3. Run `python scripts/context/build_context_snapshot.py --project-root .`
4. Log any new ADRs in `docs/adr/`

If the milestone gate passes, note in the handoff that the project is ready for `05-week-9-10-content-and-polish.md`.
