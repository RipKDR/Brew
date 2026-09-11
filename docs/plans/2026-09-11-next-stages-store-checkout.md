---
title: Next stages roadmap + DeadlockResolver + IAP store checkout
date: 2026-09-11
artifact_contract: ce-unified-plan/v1
artifact_readiness: implementation-ready
product_contract_source: ce-plan
execution: code
---

## Goal Capsule

Document Gate 5 → global sequencing in `docs/production/next-stages.md`, extract `DeadlockResolver` (config-capped reshuffles), and stop `StoreUI` from granting IAP locally (ADR 0005).

## Product Contract

### Requirements

- R1: Canonical next-stages doc covering Stages A–D, Editor-blocked vs code-unblocked work, and known contradictions.
- R2: Continuity artifacts (handoff, weekly-focus, project-summary, snapshot, changelog, ADR index) match the new sequencing.
- R3: Store product tap requests a purchase; it must not call `IAPManager.CompletePurchase`.
- R4: Restore uses `UnityIAPBridge.RestorePurchases`.
- R5: EditMode tests cover the request seam (no currency grant).
- R6: `DeadlockResolver` after CheckWin when `InProgress`; shuffle cap from `BoardConfigSO.MaxReshuffles`; orbs unmoved; win/lose skip.

### Non-goals

- Unity SDK import, ScriptableObject `.asset` creation, Cloud Functions, ice blockers, levels 41–80, localization package.

## Planning Contract

- session-settled: Stones already on master (ADR 0004). Ice stays deferred.
- session-settled: `weekly_deal` stays in catalog; Stage C adds weekly gating (do not delete SKU).
- ADR 0004 is stone-only blockers; sequencing lives in `next-stages.md` (do not reuse 0004).
- ADR 0005 records the checkout path.

## Implementation Units

- U1 Docs: `docs/production/next-stages.md`, ADR 0005, continuity, changelog, mvp-build-plan pointer.
- U2 DeadlockResolver: Core class + BoardPresenter CheckWin hook + `BoardConfigSO.MaxReshuffles` + `DeadlockResolverTests`.
- U3 Checkout: `StoreCheckout`, `StoreUI`, `GameFlowController` + `UnityIAPBridge`, `LogIAPPurchaseStart`, tests.

## Verification Contract

- `python scripts/context/validate_context.py --project-root .`
- `python scripts/context/build_context_snapshot.py --project-root .`
- EditMode: `DeadlockResolverTests`, `StoreCheckoutTests` (cannot run Unity here; tests are the contract).

## Definition of Done

- Next-stages doc is linked from production map and mvp-build-plan.
- Deadlock shuffle retries are not a magic number in engine logic.
- Store tap cannot mint currency without a store callback.
- Snapshot regenerated; handoff last-updated is today.
