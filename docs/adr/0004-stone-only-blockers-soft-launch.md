# ADR 0004: Stone-Only Blockers for Soft Launch

- Status: accepted
- Date: 2026-09-11

## Context

Level JSON already ships `blocker_placements` (16 campaign levels, all `type: "stone"`), but the board engine previously ignored them. Design docs disagreed:

- `core-mechanic.md` historically claimed no blocked/wall cells in MVP.
- `level-design-framework.md` §12 defines Stone (clear on adjacent brew) and Ice (overlay thaw).
- `data-model.md` described ice/stone/lock with `x`/`y`/`hp`, which does not match shipped `{type,row,col}` JSON.

Soft launch needs playable late-game difficulty without expanding into ice overlays or HP combat systems.

## Decision

1. Soft-launch blockers are **stone-only**.
2. Stones are immovable cell occupants (`CellContentType.Stone`).
3. Gravity uses **segment gravity** around fixed stones.
4. Stones clear when an **orthogonally adjacent brew** resolves (framework §12).
5. Ice, lock, and HP fields are **post soft-launch**; docs mark them deferred.
6. Shipped JSON schema `{type,row,col}` is canonical; data-model aligns to it.

## Consequences

- Positive: 16 authored levels gain intended obstruction without new content tools.
- Positive: Pure C# + EditMode tests can verify behavior without Unity Editor/SDK work.
- Trade-off: Depth-phase ice intro (framework L22+) is not playable at soft launch; difficulty must come from stones, recipes, and move budgets only.
- Follow-up: Author ice only after overlay gameplay ships; update framework intro table when ice lands.

## Alternatives Considered

1. **Implement ice + stone together** — Rejected for soft-launch scope; no ice in JSON; overlay model is a larger cell-state change.
2. **HP / adjacent-fusion damage from data-model** — Rejected; contradicts shipped JSON (no `hp`) and framework brew-clear rule.
3. **Strip blocker data and ship without blockers** — Rejected; late levels would be silently easier than designed.

## References

- `docs/game-design/core-mechanic.md` §1.3, §11
- `docs/game-design/level-design-framework.md` §12
- `docs/technical/data-model.md` §3 blockers
- `Assets/_Project/Resources/Levels/level_*.json` `blocker_placements`
