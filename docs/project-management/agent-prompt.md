# Brew — Enhanced Master Prompt

Use this prompt at the start of any substantial implementation session to preserve continuity and reduce re-discovery.

---

## Prompt Template

You are implementing work in Brew.  
Before changing code, follow this exact sequence:

1. Read `AGENTS.md`.
2. Read `docs/project-management/session-handoff.md`.
3. Read `docs/project-management/context-snapshot.md`.
4. Read `docs/project-management/project-summary.md` (Decision Log and Current Blockers).
5. Read relevant domain docs for the requested task.

Then:
- Restate current milestone focus and next 3 tasks from the handoff.
- Identify any contradictions between docs, rules, and workflow checks.
- If contradiction exists, stop and reconcile docs first.
- Only after that, implement.

During implementation:
- Follow `docs/production/mvp-build-plan.md` sequence.
- Respect `docs/production/milestone-gates.md`.
- Never bypass economy and analytics requirements.

At completion:
- Update `docs/project-management/session-handoff.md`.
- Regenerate `docs/project-management/context-snapshot.md` using:
  - `python scripts/context/build_context_snapshot.py --project-root .`
- Update `docs/project-management/project-summary.md` Decision Log if new decisions were made.
- Add/update ADRs in `docs/adr/` for cross-cutting decisions.
- Run readiness checks before declaring done.

---

## Resume Checklist

- [ ] I read AGENTS + handoff + context snapshot.
- [ ] I confirmed the current milestone and next tasks.
- [ ] I confirmed no doc/rule contradictions remain.
- [ ] I identified the smallest valid scope for this session.

## Decision Logging Checklist

- [ ] Decision belongs in ADR? (cross-cutting, expensive-to-reverse, architectural)
- [ ] Project summary Decision Log updated when needed.
- [ ] Handoff updated with what changed and what is next.

## Done-Definition Checklist

- [ ] Required docs remain consistent and linked.
- [ ] Context snapshot regenerated successfully.
- [ ] Context validation script passes.
- [ ] Build/config validation scripts pass.
- [ ] Any remaining risks are documented in handoff.
