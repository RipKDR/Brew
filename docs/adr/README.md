# Architecture Decision Records (ADR)

This directory stores durable technical decisions for Brew.

Use ADRs for decisions that are:
- Expensive to reverse
- Cross-cutting across systems/docs
- Likely to be revisited in future milestones

## File Naming

- Format: `NNNN-short-kebab-title.md`
- Example: `0001-canonical-unity-version.md`

## Required Sections

Each ADR should include:

1. Status (`proposed`, `accepted`, `superseded`, or `deprecated`)
2. Context
3. Decision
4. Consequences
5. Alternatives considered
6. References (docs, issues, commits, links)

## Workflow

1. Create or update ADR when a qualifying decision is made.
2. Add an entry to the ADR index table below.
3. If superseding an existing ADR, update both records.
4. Reference the ADR in `docs/project-management/project-summary.md` decision log where relevant.

## ADR Index

| Number | Title | Status | Date | File |
|---|---|---|---|---|
| 0001 | Canonical Unity Version | accepted | 2026-04-09 | [0001-canonical-unity-version.md](0001-canonical-unity-version.md) |
| 0002 | Offline-First Cloud Save | accepted | 2026-04-09 | [0002-offline-first-cloud-save.md](0002-offline-first-cloud-save.md) |
| 0003 | Remote Config as Config Source | accepted | 2026-04-09 | [0003-remote-config-as-config-source.md](0003-remote-config-as-config-source.md) |
| 0004 | Stone-Only Blockers for Soft Launch | accepted | 2026-09-11 | [0004-stone-only-blockers-soft-launch.md](0004-stone-only-blockers-soft-launch.md) |

