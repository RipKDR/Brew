# ADR 0005: IAP Store Checkout Path

- Status: accepted
- Date: 2026-09-11

## Context

`StoreUI` previously called `IAPManager.CompletePurchase` on product tap. That grants Gems and Essence locally with no store sheet, no receipt, and no server validation. Economy-guard and [infrastructure.md](../technical/infrastructure.md) require store (and later Cloud Function) confirmation before credit.

`UnityIAPBridge` already fulfills via `ProcessPurchase` → `CompletePurchase` when `UNITY_IAP` is defined. Settings restore called `IAPManager.RestorePurchases(null)`, which is a no-op (`null` returns immediately).

## Decision

1. Store product taps **request** a purchase. They never call `CompletePurchase`.
2. `StoreCheckout.RequestPurchase` is the pure-C# seam: invoke an initiator callback only.
3. `GameFlowController` wires the initiator to `UnityIAPBridge.PurchaseProduct`.
4. Restore (Settings and Store) goes through `UnityIAPBridge.RestorePurchases`.
5. `CompletePurchase` remains the **fulfillment** API used by the bridge after a store callback (and by EditMode tests).
6. Without `UNITY_IAP`, `PurchaseProduct` is a no-op. The Editor must not grant currency as a fake checkout.
7. Buy and Restore tapped before Unity IAP finishes initializing are **queued** (`IapPendingOperations`) and flushed on `OnInitialized`. They are not silently dropped.
8. Restore is platform-specific: Apple uses `IAppleExtensions.RestoreTransactions`; Google Play restores non-consumables from receipts (re-scanned on Restore and at init). `iap_purchase_start` fires only when the store sheet actually opens (`OnPurchaseInitiated`). The store list refreshes after init/restore so owned non-consumables drop out.

Server-side `verifyPurchase` remains Stage D (directory `config/firebase/functions` does not exist yet). This ADR closes the client-side grant hole first.

## Consequences

- Positive: tapping Buy in the shop cannot mint currency in production or Editor.
- Positive: EditMode tests can assert the request seam without Unity IAP.
- Positive: Early Restore/Buy during async init is not lost.
- Trade-off: Editor play without the IAP SDK cannot complete a purchase. That is intentional.
- Follow-up: Cloud Function receipt validation before scaling UA; align analytics event names to `iap_purchase_start` / `_complete` / `_fail`.

## Alternatives Considered

1. **Keep local grant as an Editor cheat** — Rejected; too easy to ship, violates economy-guard.
2. **Wait for Cloud Functions before changing StoreUI** — Rejected; local grant is exploitable as soon as a build includes the current UI.
3. **Hide the store until SDKs are imported** — Rejected; listing and restore UX should exist; fulfillment stays store-gated.

## References

- `docs/economy/economy-model.md` / economy-guard (IAP receipt validation)
- `docs/technical/infrastructure.md` §9 `verifyPurchase`
- `Assets/_Project/Scripts/Core/Economy/UnityIAPBridge.cs`
- `Assets/_Project/Scripts/Presentation/Economy/StoreUI.cs`
- `docs/production/next-stages.md` Stage A3
