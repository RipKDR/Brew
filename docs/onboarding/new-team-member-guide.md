# Brew — New Team Member Guide

> Last updated: 2026-04-09

---

## Welcome

Brew is a mobile puzzle game where players tap clusters of same-colored ingredients to fuse them into glowing orbs, chain orbs together, and brew potions to fill recipes before running out of moves. It differentiates from blast and match-3 games through positional strategy — where an orb lands matters as much as what you tap — and a crafting theme built around creation rather than destruction.

You are joining a lean team (3–8 people) building a 10-week MVP. Every document in this project is designed to be read, challenged, and updated. If something is wrong, fix the doc first, then fix the code. The documentation set is the single source of truth for what Brew is, how it works, and why decisions were made.

---

## Day 1 Reading List

Read these in order. Total time: ~50 minutes.

| # | Document | Time | What You'll Learn |
|---|----------|------|-------------------|
| 1 | [README.md](../../README.md) | 5 min | Project overview, doc map, tech stack, key metrics |
| 2 | [Game Design Document](../game-design/GDD.md) | 20 min | Vision, design pillars, full gameplay loop, all systems |
| 3 | [Core Mechanic Spec](../game-design/core-mechanic.md) | 15 min | Grid rules, fusion logic, chain resolution, edge cases |
| 4 | [MVP Build Plan](../production/mvp-build-plan.md) | 10 min | 10-week timeline, deliverables, who owns what |

---

## Day 1 Setup

### 1. Clone the Repository

```bash
git clone <repo-url>
cd Brew
```

### 2. Install Unity

Download and install **Unity 6 (6000.1 LTS)** via Unity Hub. Use the exact version specified in `ProjectSettings/ProjectVersion.txt` if the project has already been initialized.

### 3. Install Python

Install **Python 3.11+** (used for build scripts and tooling).

```bash
python --version   # verify 3.11+
```

### 4. Install Firebase CLI

```bash
npm install -g firebase-tools
firebase login
```

### 5. Validate Environment

```bash
python scripts/build/build_config.py
```

This script checks that Unity, Python, Firebase CLI, and required dependencies are correctly installed and configured.

### 6. Install Git Hooks

```bash
# macOS / Linux
cp hooks/pre-commit.sh .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit

# Windows (Git Bash)
cp hooks/pre-commit.sh .git/hooks/pre-commit
```

The pre-commit hook runs linting and prevents commits with common issues.

---

## Role-Specific Reading

After the Day 1 list, read the documents relevant to your role.

### Developer (UD-1, UD-2)

| Document | Why |
|----------|-----|
| [Architecture](../technical/architecture.md) | System layers, module boundaries, tech decisions |
| [Data Model](../technical/data-model.md) | All schemas — player profile, level config, economy, events |
| [Infrastructure](../technical/infrastructure.md) | Firebase setup, CI/CD pipeline, environments |
| [AGENTS.md](../../AGENTS.md) | AI coding assistant rules and project conventions |

### Designer (GP)

| Document | Why |
|----------|-----|
| [Level Design Framework](../game-design/level-design-framework.md) | Difficulty curve, lever introduction schedule, pacing |
| [Tutorial Flow](../game-design/tutorial-flow.md) | First 5 levels step-by-step, UX principles |
| [Economy Model](../economy/economy-model.md) | Currency flow, earning/spending rates, pacing targets |
| [A/B Test Plan](../analytics/ab-test-plan.md) | First 6 experiments, hypotheses, decision criteria |

### Artist (ART)

| Document | Why |
|----------|-----|
| [Art Direction](../creative/art-direction.md) | Visual style, color palette, token/orb/potion design, workshop |
| [Sound Design](../creative/sound-design.md) | Audio identity, SFX catalog, haptic map |

### QA

| Document | Why |
|----------|-----|
| [Milestone Gates](../production/milestone-gates.md) | Pass/fail criteria you will verify at each gate |
| [Risk Register](../production/risk-register.md) | Known risks and what to watch for |
| [Event Tracking Plan](../analytics/event-tracking-plan.md) | Every analytics event — verify they fire correctly |

### Producer (GP)

| Document | Why |
|----------|-----|
| [MVP Build Plan](../production/mvp-build-plan.md) | Week-by-week plan, deliverables, owners |
| [Milestone Gates](../production/milestone-gates.md) | Go/no-go criteria at each checkpoint |
| [Risk Register](../production/risk-register.md) | 15+ risks with likelihood, impact, and mitigations |
| [Project Summary](../project-management/project-summary.md) | Living status doc, decision log, blockers |

---

## Communication Norms

### Where Things Live

| What | Where |
|------|-------|
| **Decisions** | [Project Summary — Decision Log](../project-management/project-summary.md) |
| **Game design specs** | `docs/game-design/` |
| **Economy values** | `docs/economy/economy-model.md` + Remote Config |
| **Technical specs** | `docs/technical/` |
| **Production planning** | `docs/production/` |
| **Terminology** | [Glossary](../project-management/glossary.md) |

### How to Propose Changes

1. **Update the doc first.** If your change affects game design, economy, or architecture, edit the relevant document with your proposed change.
2. **Discuss the doc change.** Get agreement from the relevant owner (GP for design/economy, UD-1 for architecture).
3. **Then implement.** Code follows docs, not the other way around.

This order prevents drift between what the team thinks the game is and what the code actually does.

### Commit and PR Expectations

- Branch naming: `feature/xxx`, `fix/xxx`, `docs/xxx`
- Commit messages: imperative mood, <72 characters first line
- PRs must pass CI, have at least 1 review, and reference relevant docs if design-impacting
- See [Style Guide](../project-management/style-guide.md) for full coding conventions

---

## Key Rules

These are non-negotiable constraints. Every team member must internalize them.

### 1. Never Add Currencies

Brew has exactly two currencies: **Essence** (soft) and **Gems** (premium). No event tokens, no tickets, no energy, no seasonal coins. If a feature seems to require a new currency, redesign the feature. This was a deliberate design decision to keep the player's mental model simple.

### 2. Never Hardcode Economy Values

All economy values (prices, earn rates, thresholds, multipliers) must be defined in **ScriptableObjects** or **Firebase Remote Config** — never as magic numbers in code. Tuning must be possible without a code change or app update.

### 3. Never Skip Milestone Gates

Every milestone gate (Weeks 2, 4, 6, 8, 10) is a mandatory pass/fail checkpoint. The team does not proceed to the next phase until the gate criteria are met. This protects against building on a broken foundation.

### 4. Always Fire Analytics Events

Every player action, state change, and economy transaction must fire the corresponding analytics event as defined in the [Event Tracking Plan](../analytics/event-tracking-plan.md). Analytics are not optional polish — they are infrastructure. A feature without analytics is an incomplete feature.

### 5. Docs Before Code

If it changes the game, it changes the doc first. If there is no doc for it, write one before implementing. The documentation set is the contract between design intent and implementation.
