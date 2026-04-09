# Brew

**Tap. Fuse. Brew.**

A mobile puzzle game for iOS and Android where players tap clusters of ingredients to fuse them into glowing orbs, chain orbs together, and brew potions to fill recipes — all before running out of moves.

## What Makes Brew Different

Every top casual puzzle game is about **clearing**: popping candy, sorting colors, removing screws. Brew inverts this. The core interaction is about **building**: fusing small ingredients into growing, glowing orbs that eventually transform into crafted potions. The moment-to-moment feeling is creation, not destruction.

The strategic depth comes from **spatial planning**: where an orb ends up after fusion matters, because orbs need to be adjacent to chain-fuse. A smaller cluster in a better board position can beat a larger one in a worse position. This creates decision-making that blast/match games lack while keeping the same one-tap accessibility.

## Project Status

**Phase: Pre-Production / Documentation Complete**

All design, technical, production, and creative specifications are finalized and ready for implementation.

## Documentation Map

### Game Design
| Document | Purpose |
|----------|---------|
| [GDD](docs/game-design/GDD.md) | Complete game design document — vision, loop, rules, systems |
| [Core Mechanic](docs/game-design/core-mechanic.md) | Deep technical spec — fusion rules, chain logic, board behavior |
| [Level Design Framework](docs/game-design/level-design-framework.md) | 40-level difficulty curve, procedural generation, blocker design |
| [Tutorial Flow](docs/game-design/tutorial-flow.md) | First 5 guided levels — step-by-step teaching sequence |
| [Meta Systems](docs/game-design/meta-systems.md) | Potion shelf, workshop, daily brew, win streaks, weekly events |

### Economy & Monetization
| Document | Purpose |
|----------|---------|
| [Economy Model](docs/economy/economy-model.md) | Currency flow, earning/spending rates, pacing |
| [Monetization Plan](docs/economy/monetization-plan.md) | IAP catalog, ad placements, ethical guardrails |

### Technical
| Document | Purpose |
|----------|---------|
| [Architecture](docs/technical/architecture.md) | Tech stack (Unity + Firebase), system layers, state machine |
| [Data Model](docs/technical/data-model.md) | All schemas — player, level, event, economy, config |
| [Infrastructure](docs/technical/infrastructure.md) | CI/CD, Firebase setup, analytics events, security |

### Production
| Document | Purpose |
|----------|---------|
| [MVP Build Plan](docs/production/mvp-build-plan.md) | 10-week week-by-week plan with deliverables and owners |
| [Milestone Gates](docs/production/milestone-gates.md) | Go/no-go criteria at each milestone, kill thresholds |
| [Risk Register](docs/production/risk-register.md) | 15+ risks with likelihood, impact, and specific mitigations |

### Creative
| Document | Purpose |
|----------|---------|
| [Art Direction](docs/creative/art-direction.md) | Visual style, color palette, token/orb design, workshop scene |
| [Sound Design](docs/creative/sound-design.md) | Audio identity, SFX catalog, haptic map, adaptive music |
| [UA Creative Strategy](docs/creative/ua-creative-strategy.md) | Ad concepts, channel mix, CPI targets, ASO plan |

### Analytics
| Document | Purpose |
|----------|---------|
| [KPI Framework](docs/analytics/kpi-framework.md) | All KPIs with targets, thresholds, and measurement cadence |
| [Event Tracking Plan](docs/analytics/event-tracking-plan.md) | Every analytics event with parameters and payloads |
| [A/B Test Plan](docs/analytics/ab-test-plan.md) | First 6 experiments with hypotheses and decision criteria |

### Live Operations
| Document | Purpose |
|----------|---------|
| [Event Templates](docs/liveops/event-templates.md) | Weekly event system, daily brew, theme swapping workflow |
| [Content Calendar](docs/liveops/content-calendar.md) | First 90 days post-soft-launch — events, tests, tuning |

### API & SDK
| Document | Purpose |
|----------|---------|
| [Cloud Functions API](docs/api/cloud-functions-api.md) | Firebase Cloud Functions — endpoints, schemas, auth |
| [Remote Config Schema](docs/api/remote-config-schema.md) | Every tunable server-side parameter with defaults |
| [Unity Architecture](docs/sdk/unity-architecture.md) | Module structure, dependencies, public interfaces |
| [Event Bus Design](docs/sdk/event-bus-design.md) | Internal event system — typed events, publishers, subscribers |
| [State Machine Spec](docs/sdk/state-machine-spec.md) | Board state machine — states, transitions, edge cases |

### Testing & QA
| Document | Purpose |
|----------|---------|
| [Test Plan](docs/testing/test-plan.md) | Unit, integration, and manual test coverage |
| [Device Matrix](docs/testing/device-matrix.md) | Target devices with priority and test protocols |
| [Regression Checklist](docs/testing/regression-checklist.md) | 60-check pre-release validation |

### Legal & Compliance
| Document | Purpose |
|----------|---------|
| [Privacy Policy](docs/legal/privacy-policy-template.md) | GDPR/CCPA-compliant privacy policy template |
| [Terms of Service](docs/legal/terms-of-service-template.md) | User terms template |
| [App Store Compliance](docs/legal/app-store-compliance.md) | Apple + Google submission checklists |
| [Data Collection Inventory](docs/legal/data-collection-inventory.md) | Every data point mapped to purpose and controls |

### Project Management
| Document | Purpose |
|----------|---------|
| [Project Summary](docs/project-management/project-summary.md) | Living status document with decision log |
| [Glossary](docs/project-management/glossary.md) | All game terminology defined |
| [Style Guide](docs/project-management/style-guide.md) | C# conventions, git workflow, code patterns |

### Onboarding
| Document | Purpose |
|----------|---------|
| [New Team Member Guide](docs/onboarding/new-team-member-guide.md) | Day-1 reading list and setup by role |

### Agent Intelligence
| File | Purpose |
|------|---------|
| [AGENTS.md](AGENTS.md) | AI agent project guide — decisions, conventions, pitfalls |
| [.cursor/rules/brew-project.mdc](.cursor/rules/brew-project.mdc) | 15 strict plan adherence rules |
| [.cursor/rules/unity-csharp.mdc](.cursor/rules/unity-csharp.mdc) | Unity C# coding standards |
| [.cursor/rules/economy-guard.mdc](.cursor/rules/economy-guard.mdc) | Economy integrity guardrails |
| [.cursor/rules/analytics-tracking.mdc](.cursor/rules/analytics-tracking.mdc) | Analytics event tracking standards |
| [.cursor/skills/brew-level-design.md](.cursor/skills/brew-level-design.md) | Skill: level design and validation |
| [.cursor/skills/brew-economy-tuning.md](.cursor/skills/brew-economy-tuning.md) | Skill: economy tuning and balance |
| [.cursor/skills/brew-qa-checklist.md](.cursor/skills/brew-qa-checklist.md) | Skill: pre-merge and pre-build QA |

### Scripts & Automation
| File | Purpose |
|------|---------|
| [scripts/level-generator/generate_level.py](scripts/level-generator/generate_level.py) | Generate level JSONs with difficulty presets |
| [scripts/economy-sim/simulate_economy.py](scripts/economy-sim/simulate_economy.py) | Simulate player economy over N days |
| [scripts/build/build_config.py](scripts/build/build_config.py) | Pre-build validation checks |
| [hooks/pre-commit.sh](hooks/pre-commit.sh) | Git pre-commit hook — schema, economy, size checks |
| [.github/workflows/ci.yml](.github/workflows/ci.yml) | CI: level validation, economy sim, doc checks |
| [.github/workflows/build.yml](.github/workflows/build.yml) | Build: tag-triggered Unity Cloud Build |

## Key Metrics (Targets)

| Metric | Target | No-Go |
|--------|--------|-------|
| Tutorial Completion | >= 85% | < 70% |
| D1 Retention | >= 40% | < 32% |
| D7 Retention | >= 15% | < 10% |
| D30 Retention | >= 7% | — |
| Level Retry Rate | >= 40% | < 25% |
| CPI (iOS, Meta) | < $2.50 | > $4.00 |

## Tech Stack

- **Engine**: Unity 6 (6000.1 LTS), C# 11
- **Backend**: Firebase (Auth, Firestore, Cloud Functions, Remote Config, Analytics)
- **Ads**: AdMob + mediation (AppLovin MAX)
- **IAP**: Unity IAP
- **CI/CD**: GitHub Actions + Fastlane
- **Analytics**: Firebase Analytics + custom events

## Reading Order for New Team Members

1. `docs/game-design/GDD.md` — understand the game
2. `docs/game-design/core-mechanic.md` — understand the rules
3. `docs/game-design/tutorial-flow.md` — understand the player's first experience
4. `docs/production/mvp-build-plan.md` — understand what you're building and when
5. `docs/technical/architecture.md` — understand how it's built
6. Everything else as needed for your role
