# Brew — Project Summary

> **Living document.** Updated as decisions are made, milestones are reached, and blockers emerge.
> Last updated: 2026-04-09

---

## At a Glance

| Field | Value |
|-------|-------|
| **Project** | Brew — Mobile Puzzle Game |
| **Status** | Pre-Production (Documentation Complete, Implementation Not Started) |
| **Pitch** | *"Tap clusters to fuse ingredients into glowing orbs. Chain orbs together to brew potions. Craft every recipe before you run out of moves."* |
| **Platforms** | iOS (15+), Android (8.0+) — portrait only |
| **Engine** | Unity 6 (6000.1 LTS), C# |
| **Backend** | Firebase (Auth, Firestore, Cloud Functions, Remote Config, Analytics, Crashlytics) |
| **Ads** | Google AdMob + mediation (AppLovin MAX) |
| **IAP** | Unity IAP 4.x |

---

## Key Dates

| Milestone | Target Date | Status |
|-----------|-------------|--------|
| Documentation complete | 2026-04-09 | Done |
| Implementation start | TBD | — |
| **Milestone 1** — Foundation | Week 2 | Not started |
| **Milestone 2** — Core Loop | Week 4 | Not started |
| **Milestone 3** — Feel & Juice | Week 6 | Not started |
| **Milestone 4** — Meta & Economy | Week 8 | Not started |
| **Milestone 5** — Content & Polish | Week 10 | Not started |
| Soft launch target | TBD | — |
| Global launch target | TBD | — |

---

## Team

| Role | Count | Responsibilities |
|------|-------|------------------|
| Designer / Producer (GP) | 1 | Game design, level design, economy tuning, milestone gates, sprint planning |
| Unity Developer — Core (UD-1) | 1 | Grid system, fusion engine, cascade resolver, state machine, performance |
| Unity Developer — UI/Meta (UD-2) | 1 | UI, meta systems, Firebase integration, analytics, ad/IAP integration |
| Artist (ART) | 1 | Token/orb sprites, VFX, workshop scene, potion shelf, UI art |
| Sound Designer (SFX) | 0.5 | SFX catalog, adaptive music, haptic mapping (contractor) |
| QA | 0.5 | Test plans, regression, device matrix, milestone gate verification (shared) |
| **Total** | **~5** (3–8 scaling range) | |

---

## Tech Stack

| Layer | Technology | Notes |
|-------|-----------|-------|
| Engine | Unity 6 (6000.1 LTS) | C# 11, cross-platform iOS/Android |
| Backend | Firebase | Auth, Firestore, Cloud Functions (2nd gen, Node 20), Remote Config |
| Analytics | Firebase Analytics (GA4) | Custom event tracking per event-tracking-plan.md |
| Crash Reporting | Firebase Crashlytics | Native + managed crash reporting |
| Ads | AdMob + AppLovin MAX | Rewarded video + interstitial; mediation across networks |
| IAP | Unity IAP 4.x | Unified API; server-side receipt verification via Cloud Functions |
| Push | Firebase Cloud Messaging | Re-engagement: daily brew, events, streak-at-risk |
| CI/CD | GitHub Actions + Fastlane | GameCI Docker images; TestFlight + Firebase App Distribution |
| Source Control | Git + GitHub | Monorepo, Git LFS for binary assets |
| Art Pipeline | Spine / Lottie + TextMeshPro | Spine for character/potion anims, Lottie for UI micro-animations |

---

## Target Metrics

| Metric | Target | No-Go Threshold |
|--------|--------|-----------------|
| D1 Retention | >= 40% | < 32% |
| D7 Retention | >= 15% | < 10% |
| D30 Retention | >= 7% | — |
| Tutorial Completion | >= 85% | < 70% |
| Level Retry Rate | >= 40% | < 25% |
| CPI (iOS, Meta) | < $2.50 | > $4.00 |
| Session Count / DAU | >= 2.5 | < 1.5 |
| ARPDAU | >= $0.08 | < $0.03 |

---

## Kill Criteria

The project is stopped or significantly pivoted if any of the following persist after two tuning cycles in soft launch:

| Criterion | Threshold |
|-----------|-----------|
| D1 Retention | < 32% |
| D7 Retention | < 10% |
| CPI (iOS, Meta) | > $4.00 |
| Tutorial Completion | < 70% |

See [Milestone Gates](../production/milestone-gates.md) for per-milestone go/no-go criteria.

---

## Current Blockers

| # | Blocker | Owner | Status |
|---|---------|-------|--------|
| — | *(none — ready to begin)* | — | — |

---

## Decision Log

| Date | Decision | Rationale | Owner |
|------|----------|-----------|-------|
| 2026-04-09 | Game concept finalized as fusion-chain-brew puzzle | Differentiated from blast/match-3 by positional strategy and creation satisfaction | Product |
| 2026-04-09 | Two-currency economy only (Essence + Gems) | Simplifies mental model; prevents currency bloat common in mobile puzzles | Product |
| 2026-04-09 | No energy/lives system | Unlimited free play reduces churn; monetize through convenience, not access gating | Product |
| 2026-04-09 | Unity 6 (6000.1 LTS) selected as the canonical engine version | Matches architecture docs and AGENTS guidance; minimizes configuration drift across implementation sessions | Engineering |
| 2026-04-09 | Firebase as sole backend | Fastest path for lean team; single platform covers auth, database, config, analytics, crash reporting | Engineering |
| 2026-04-09 | 10-week MVP timeline with 5 milestone gates | Tight scope forces discipline; gates catch fundamental problems before sunk-cost accumulates | Product |
| 2026-04-09 | Context continuity became CI-enforced via handoff + ADR checks | Prevents session drift and ensures deterministic resume behavior for human and AI contributors | Engineering |

---

## Open Questions

| # | Question | Priority | Owner | Status |
|---|----------|----------|-------|--------|
| — | *(none critical)* | — | — | — |

---

## Documentation Map

### Game Design
| Document | Path |
|----------|------|
| Game Design Document | [docs/game-design/GDD.md](../game-design/GDD.md) |
| Core Mechanic Spec | [docs/game-design/core-mechanic.md](../game-design/core-mechanic.md) |
| Level Design Framework | [docs/game-design/level-design-framework.md](../game-design/level-design-framework.md) |
| Tutorial Flow | [docs/game-design/tutorial-flow.md](../game-design/tutorial-flow.md) |
| Meta Systems | [docs/game-design/meta-systems.md](../game-design/meta-systems.md) |

### Economy & Monetization
| Document | Path |
|----------|------|
| Economy Model | [docs/economy/economy-model.md](../economy/economy-model.md) |
| Monetization Plan | [docs/economy/monetization-plan.md](../economy/monetization-plan.md) |

### Technical
| Document | Path |
|----------|------|
| Architecture | [docs/technical/architecture.md](../technical/architecture.md) |
| Data Model | [docs/technical/data-model.md](../technical/data-model.md) |
| Infrastructure & DevOps | [docs/technical/infrastructure.md](../technical/infrastructure.md) |

### Production
| Document | Path |
|----------|------|
| MVP Build Plan | [docs/production/mvp-build-plan.md](../production/mvp-build-plan.md) |
| Milestone Gates | [docs/production/milestone-gates.md](../production/milestone-gates.md) |
| Risk Register | [docs/production/risk-register.md](../production/risk-register.md) |

### Creative
| Document | Path |
|----------|------|
| Art Direction | [docs/creative/art-direction.md](../creative/art-direction.md) |
| Sound Design | [docs/creative/sound-design.md](../creative/sound-design.md) |
| UA Creative Strategy | [docs/creative/ua-creative-strategy.md](../creative/ua-creative-strategy.md) |

### Analytics
| Document | Path |
|----------|------|
| KPI Framework | [docs/analytics/kpi-framework.md](../analytics/kpi-framework.md) |
| Event Tracking Plan | [docs/analytics/event-tracking-plan.md](../analytics/event-tracking-plan.md) |
| A/B Test Plan | [docs/analytics/ab-test-plan.md](../analytics/ab-test-plan.md) |

### Live Operations
| Document | Path |
|----------|------|
| Event Templates | [docs/liveops/event-templates.md](../liveops/event-templates.md) |
| Content Calendar | [docs/liveops/content-calendar.md](../liveops/content-calendar.md) |

### Project Management
| Document | Path |
|----------|------|
| Project Summary (this doc) | [docs/project-management/project-summary.md](project-summary.md) |
| Glossary | [docs/project-management/glossary.md](glossary.md) |
| Style Guide | [docs/project-management/style-guide.md](style-guide.md) |
| Session Handoff | [docs/project-management/session-handoff.md](session-handoff.md) |
| Context Snapshot | [docs/project-management/context-snapshot.md](context-snapshot.md) |
| Enhanced Agent Prompt | [docs/project-management/agent-prompt.md](agent-prompt.md) |
| Weekly Focus | [docs/project-management/weekly-focus.md](weekly-focus.md) |

### Architecture Decisions
| Document | Path |
|----------|------|
| ADR Index | [docs/adr/README.md](../adr/README.md) |

### Onboarding
| Document | Path |
|----------|------|
| New Team Member Guide | [docs/onboarding/new-team-member-guide.md](../onboarding/new-team-member-guide.md) |
