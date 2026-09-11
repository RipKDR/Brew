# Brew — Next Stages After Gate 5 Code Complete

**Version:** 1.0  
**Last updated:** 2026-09-11  
**Owner:** Game Designer / Producer + Engineering  
**Status:** Canonical sequencing for post-MVP-code work

---

## Current position

Code-side Gate 5 systems exist: core loop, meta/economy, weekly events, Firebase/IAP/AdMob **bridges**, stone blockers (ADR 0004), deadlock recovery (`DeadlockResolver` + `BoardConfigSO.MaxReshuffles`), and store checkout that does not grant IAP locally (ADR 0005). The game is **not** soft-launch ready until Unity Editor SDK import, ScriptableObject assignment, device matrix, and store assets land.

Post-Gate-5 **sequencing** lives in this document (not ADR 0004 — that number is stone-only blockers).

This document sequences what happens next. It does not add currencies, match-3 mechanics, or undocumented meta layers.

Companion docs: [mvp-build-plan.md](mvp-build-plan.md), [milestone-gates.md](milestone-gates.md), [soft-launch-plan.md](soft-launch-plan.md), [content-calendar.md](../liveops/content-calendar.md).

```
Stage A  Gate 5 close-out (Editor + remaining code)
    │
Stage B  Soft launch (CA / AU / NZ)
    │
Stage C  Live-ops 90 days
    │
Stage D  Scale and global
```

---

## Stage A — Gate 5 close-out

### A1. Unity Editor (human-blocked)

1. Import Firebase + EDM4U. Scripting defines: `FIREBASE_AUTH`, `FIREBASE_ANALYTICS`, `FIREBASE_REMOTE_CONFIG`, `FIREBASE_CRASHLYTICS`, `FIREBASE_FIRESTORE`.
2. Import Unity IAP + AdMob. Defines: `UNITY_IAP`, `ADMOB`.
3. Generate config assets: menu `Brew/Generate Config Assets` or Unity batchmode `Brew.Editor.ConfigAssetGenerator.GenerateAllConfigAssets`. Assign all SOs on `GameFlowController`.
4. Run the full EditMode suite in the Editor (including `DeadlockResolverTests`, `StoreCheckoutTests`, `StoneGameplayTests`).
5. Device playthrough: campaign 1–5, a stone level (16+), event, daily brew, workshop, streak fail. Verify Firebase DebugView and Crashlytics.

### A2. Gate 5 checklist (mixed)

Per [milestone-gates.md](milestone-gates.md) 5.1–5.15: zero P0, ≤3 P1, all 40 levels completable by ≥2 testers, FPS/RAM/cold start, install size, IAP sandbox, real ads, two-device cloud save, Remote Config live, analytics DebugView, store assets, legal placeholder replacement (`[COMPANY NAME]`, `[CONTACT EMAIL]`).

### A3. Remaining code (unblocked without Editor)

| Item | Status |
|------|--------|
| Stone blockers | Done (ADR 0004) |
| Deadlock reshuffle | Done — `DeadlockResolver` during CheckWin; shuffle cap from `BoardConfigSO.MaxReshuffles` (default 10); orbs/stones stay; win/lose skip recovery |
| Store tap must not grant IAP locally | Done — `StoreCheckout` + `UnityIAPBridge.PurchaseProduct` (ADR 0005) |
| Cloud Functions (`verifyPurchase`, `syncPlayerState`, `deleteAccount`) | Stage D (before scaling UA) |
| Ice blockers | Post soft-launch (ADR 0004) |
| Deadlock reshuffle juice (overlay / scatter) | Soft-launch polish; correctness already ships |
| Align `AnalyticsManager` IAP event names to `iap_purchase_start` / `_complete` / `_fail` | Stage A follow-up |

---

## Stage B — Soft launch

Follow [soft-launch-plan.md](soft-launch-plan.md) and [prompts/09-soft-launch-prep.md](../prompts/09-soft-launch-prep.md).

- Markets: Canada, Australia, New Zealand. UA budget $1,000. Target ≥2,000 installs.
- Activate `AB_TUTORIAL_LENGTH` and `AB_MOVE_BUDGET` via Remote Config ([ab-test-plan.md](../analytics/ab-test-plan.md)). Do not invent event names.
- Week 2 / 4 / 8 checkpoints. Kill if any two kill criteria in [milestone-gates.md](milestone-gates.md) Soft Launch Gate are true after ≥1,000 installs.

---

## Stage C — Live-ops 90 days

Follow [content-calendar.md](../liveops/content-calendar.md) and [event-templates.md](../liveops/event-templates.md). Event JSON already supports a `"themed"` ingredient overlay.

- Weekly events as data overlays, not a third currency.
- After IAP conversion is observed: implement the **weekly_deal rotation** as specified in [monetization-plan.md](../economy/monetization-plan.md) §2.4. The SKU already exists in `IAPManager.Catalog`; the plan's "not in catalog" note is stale — treat the SKU as **present but not weekly-gated** until this stage.
- Migrate level JSON `brine` / `glow` aliases to canonical `sun` / `shadow` when touching those files.

---

## Stage D — Scale and global

Only after green-light (D1 ≥ 40%, D7 ≥ 15%, D30 ≥ 6%, LTV/CPI > 2×, crash rate < 0.5%):

- Levels 41–80.
- Localization: [localization-plan.md](../localization/localization-plan.md) (`com.unity.localization`). No localization runtime exists today.
- Cloud Functions from [infrastructure.md](../technical/infrastructure.md) §9. `config/firebase/functions` is referenced by `firebase.json` but the directory is missing. Economy-guard requires server receipt validation before granting IAP at scale.
- Quality tiers via already-imported Adaptive Performance (Gate 5 fail path 5.5–5.8).
- Ice blockers only after amending cell overlay design (ADR 0004 follow-up).

---

## Known contradictions (do not silently "fix" in engine)

1. **Blockers:** `core-mechanic.md` historically said no blocked/wall cells in MVP. Late campaign JSON already ships stones. **ADR 0004** ships stone-only; ice/lock/HP stay deferred. Do not strip stones to match the old MVP sentence.
2. **`weekly_deal`:** code catalog has the SKU; monetization-plan §2.4 says it is post-MVP / not in catalog. Stage C implements weekly gating; until then leave the SKU in the catalog.
3. **Ingredient aliases:** JSON `brine` / `glow` map to Sun / Shadow in `LevelLoader`. Canonical names stay Ember / Frost / Vine / Sun / Shadow. Stage C cleanup: migrate JSON to canonical names when touching those files.
4. **Grid size:** [level-design-framework.md](../game-design/level-design-framework.md) still lists 3–7×3–8; engine/`core-mechanic.md` is 5–9×5–11. Engine is source of truth.
5. **IAP analytics:** tracking plan uses `iap_purchase_start` / `iap_purchase_complete` / `iap_purchase_fail`; `AnalyticsManager.LogIAPPurchase` still logs `iap_purchase`. Align names before DebugView sign-off (5.15).

---

## Out of scope forever (unless docs are amended first)

- A third currency (energy, tickets, event tokens).
- Match-3 swap / line-match.
- Essence ↔ Gem conversion.
- Granting IAP rewards on the client without store (and later server) confirmation.
