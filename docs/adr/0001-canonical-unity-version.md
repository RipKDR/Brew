# ADR 0001: Canonical Unity Version

- Status: accepted
- Date: 2026-04-09

## Context

Project documentation contained conflicting engine versions between agent guidance, project summary, and onboarding/setup docs. This introduced planning and implementation risk because build assumptions, package compatibility decisions, and tooling setup can diverge when the canonical version is unclear.

## Decision

Adopt **Unity 6 (6000.1 LTS)** as the canonical engine version for Brew and align project-facing docs accordingly.

## Consequences

- Positive: Removes ambiguity for implementation, onboarding, and automation.
- Positive: Keeps architecture and agent instructions in sync.
- Trade-off: Teams or plugins pinned to Unity 2022 need explicit migration/compatibility checks.

## Alternatives Considered

1. Stay on Unity 2022 LTS  
   Rejected due to current architecture and agent guidance already being standardized around Unity 6.

2. Keep version unspecified in docs  
   Rejected because this reintroduces drift and setup errors.

## References

- `AGENTS.md`
- `docs/project-management/project-summary.md`
- `docs/onboarding/new-team-member-guide.md`
- `README.md`
