# Brew — Weekly Focus

> Short-horizon operational focus for the current week.
> Update this document at least once per week and whenever priorities shift.
> Week of: 2026-09-11

---

## Primary Outcome

Gate 5 soft-launch readiness. Next-stage sequencing is documented (`docs/production/next-stages.md`). Store checkout no longer grants IAP locally (ADR 0005). Remaining work is Unity Editor verification (tests + ScriptableObjects + SDK import) and device playthrough.

## Must Complete This Week

- [x] Document post-Gate-5 stages (A Editor close-out → B soft launch → C live-ops → D global)
- [x] Stop StoreUI from calling `CompletePurchase` on tap; route through `UnityIAPBridge`
- [x] ADR 0005 + EditMode coverage for the checkout seam
- [ ] Run all EditMode unit tests in Unity Editor — including `StoreCheckoutTests` and `StoneGameplayTests`
- [ ] Create ScriptableObject assets (`Brew/Generate Config Assets`) and assign to `GameFlowController`
- [ ] Import Firebase / Unity IAP / AdMob SDKs and set scripting defines

## Secondary Work

- Begin app store asset preparation (icon, screenshots)
- Align remaining IAP analytics names (`iap_purchase` → `iap_purchase_complete` / `_fail`)
- Full play session including stone levels and a shop tap (no local gem grant)
- Complete Gate 5 checklist items (5.5–5.15)

## Risks to Watch

- Without `UNITY_IAP`, Buy is a no-op — do not reintroduce Editor cheats
- Ice blockers still deferred (ADR 0004)
- Firebase / IAP / AdMob SDKs not imported — bridges remain no-ops without defines
- Unity EditMode suite not yet executed in this environment

## Owners

- Engineering: Unity Editor test pass, SDK import, SO wiring
- Product/Engineering: Gate 5 device checklist, store assets; follow `docs/production/next-stages.md`
