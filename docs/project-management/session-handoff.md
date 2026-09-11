# Brew — Session Handoff

> Canonical continuity file for engineering and AI sessions.
> Update this file at the end of every substantial work session.
> Last updated: 2026-09-11

---

## Current Milestone Focus

- Milestone: **Gate 5 — Soft Launch Readiness**
- Goal: Next-stage sequencing is documented. Store checkout no longer grants currency locally (ADR 0005). Remaining Gate 5 work needs Unity Editor (SDK import, ScriptableObject assets, EditMode test run) and device playthrough.
- Source plan: `docs/production/next-stages.md` (post-Gate-5 sequence), `docs/production/mvp-build-plan.md`

## Current Branch + Baseline

- Branch: `cursor/next-stages-deadlock-iap-8f0e`
- Baseline tag/commit: Not tagged yet
- CI expectation: `validate-docs`, `validate-context`, `validate-levels`, `validate-economy`, `lint`, and `unity-tests` should pass

## Completed Since Last Handoff

### Next stages + IAP checkout (This Session)

- Wrote canonical `docs/production/next-stages.md` (Stages A–D: Editor close-out, soft launch, live-ops, global).
- ADR 0005: store taps request IAP via `UnityIAPBridge`; `CompletePurchase` is fulfillment after a store callback only.
- `StoreCheckout` request seam + USD parse for analytics; `StoreUI` no longer calls `CompletePurchase`.
- `GameFlowController` constructs `UnityIAPBridge`, wires purchase + restore, logs `iap_purchase_start`.
- EditMode: `StoreCheckoutTests`, `LogIAPPurchaseStart` test.
- Continuity: project-summary blockers/decision log, changelog, mvp-build-plan pointer, ADR index.

### Prior on master (stone + deadlock)

Stone blockers (ADR 0004) and deadlock recovery (`TokenSpawner.TryRecoverDeadlock`) already landed — not reimplemented here.

## In Progress

- None. Awaiting Unity Editor verification.

## Next 3 Tasks

1. **Run EditMode tests in Unity Editor** (including `StoreCheckoutTests` + `StoneGameplayTests`). Fix any compile/test failures.
2. **Create ScriptableObject assets** via `Brew/Generate Config Assets`; assign to `GameFlowController`. Import Firebase/IAP/AdMob SDKs + scripting defines.
3. **Device playthrough** including shop tap (must open store sheet, must not mint Gems without payment) and stone levels 16–40; then Gate 5.5–5.15.

## Blockers / Risks

- Blocker: Full Unity EditMode suite + SO assignment still require Unity Editor
- Blocker: Firebase / Unity IAP / AdMob SDKs not imported (bridges compile as no-ops without defines)
- Risk: Editor play without `UNITY_IAP` cannot complete a purchase by design (ADR 0005)
- Risk: Ice blockers still deferred (ADR 0004)
- Risk: `AnalyticsManager.LogIAPPurchase` still logs `iap_purchase` instead of `iap_purchase_complete` — Stage A follow-up in next-stages.md

## Decisions Recorded This Session

- Post-Gate-5 work is sequenced in `docs/production/next-stages.md`; do not skip to levels 41–80 or localization before Gate 5 + soft launch
- Store product taps never call `CompletePurchase` (ADR 0005)
- `weekly_deal` stays in the catalog; weekly rotation gating is Stage C, not a SKU deletion

## Files Touched This Session

- `docs/production/next-stages.md`
- `docs/production/mvp-build-plan.md`
- `docs/adr/0005-iap-store-checkout-path.md`
- `docs/adr/README.md`
- `docs/plans/2026-09-11-next-stages-store-checkout.md`
- `Assets/_Project/Scripts/Core/Economy/StoreCheckout.cs`
- `Assets/_Project/Scripts/Presentation/Economy/StoreUI.cs`
- `Assets/_Project/Scripts/Presentation/GameFlowController.cs`
- `Assets/_Project/Scripts/Core/Backend/AnalyticsManager.cs`
- `Assets/Tests/EditMode/Core/Economy/StoreCheckoutTests.cs`
- `Assets/Tests/EditMode/Core/Backend/AnalyticsManagerTests.cs`
- Continuity: `session-handoff.md`, `weekly-focus.md`, `project-summary.md`, `context-snapshot.md`, `CHANGELOG.md`

## Handoff Checklist

- [x] Decision log updated (ADR 0005 + project-summary)
- [x] This handoff file updated
- [x] Changelog updated
- [x] Weekly focus updated
- [x] Context snapshot regenerated
- [x] ADR index validated
- [ ] CI/local Unity EditMode run (requires Unity Editor)
- [ ] ScriptableObject assets created and assigned (requires Unity Editor)
- [ ] Firebase SDK imported and scripting defines set (requires Unity Editor)
