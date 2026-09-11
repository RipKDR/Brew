# Brew — Weekly Focus

> Short-horizon operational focus for the current week.
> Update this document at least once per week and whenever priorities shift.
> Week of: 2026-09-11

---

## Primary Outcome

Gate 5 soft-launch readiness. Stone blocker gameplay and mid-game deadlock recovery are implemented in pure C# with EditMode tests. Remaining work is Unity Editor verification (tests + ScriptableObjects + SDK import) and device playthrough.

## Must Complete This Week

- [x] Document-first blocker reconciliation (core-mechanic, data-model, level-design-framework, ADR 0004)
- [x] Implement `CellContentType.Stone` + stone factories
- [x] Segment gravity around stones in `CascadeResolver`
- [x] Place stones from level JSON; clear on orthogonal brew (both brew paths)
- [x] Deadlock detect + recover on `TokenSpawner`; hook in `BoardPresenter` after CheckWin
- [x] Fix adjacent stones in `level_037.json`
- [x] Add `StoneGameplayTests` EditMode coverage
- [ ] Run all EditMode unit tests in Unity Editor — fix any failures from stone/deadlock work
- [ ] Create ScriptableObject assets and assign to `GameFlowController`
- [ ] Import Firebase / Unity IAP / AdMob SDKs and set scripting defines

## Secondary Work

- Begin app store asset preparation (icon, screenshots)
- Implement `INotificationScheduler` platform adapters for iOS and Android
- Full play session including stone levels (e.g. 16, 23, 36–40)
- Complete Gate 5 checklist items (5.5–5.15)

## Risks to Watch

- Ice blockers still deferred (ADR 0004) — Depth-phase design assumed ice; soft launch uses stones + move pressure only
- Firebase / IAP / AdMob SDKs not imported — bridges remain no-ops without defines
- Deadlock reshuffle is functionally correct but has no dedicated VFX yet
- Unity EditMode suite not yet executed in this environment

## Owners

- Engineering: Unity Editor test pass, SDK import, SO wiring
- Product/Engineering: Gate 5 device checklist, store assets
