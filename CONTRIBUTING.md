# Contributing to Brew

Thanks for helping build Brew! This guide covers how we work together.

## Getting Started

1. Clone the repo
2. Install [Unity 2022 LTS](https://unity.com/releases/editor/qa/lts-releases)
3. Install [Python 3.11+](https://www.python.org/downloads/)
4. Set up git hooks:
   ```bash
   python scripts/setup_hooks.py
   ```
5. Open the project in Unity and let it import

## Branching

Use the following branch prefixes:

| Prefix      | Use for                        |
|-------------|--------------------------------|
| `feature/`  | New functionality              |
| `fix/`      | Bug fixes                      |
| `docs/`     | Documentation changes          |
| `hotfix/`   | Critical production fixes      |

Examples: `feature/booster-ui`, `fix/board-crash-on-shuffle`, `docs/economy-rebalance`

## Commit Messages

- Use imperative mood: "Add booster preview" not "Added booster preview"
- Keep the subject line under 72 characters
- Reference relevant docs if the change impacts game design or economy:
  ```
  Add move-limit scaling for levels 41-60

  Implements curve from docs/economy/economy-model.md section 3.2.
  ```

## Pull Requests

Every PR must:

- Pass CI (lint, build, tests)
- Have at least one approving review
- Reference docs if it changes game design or economy behavior
- Use the PR template — don't delete sections, write "N/A" if not applicable

## Code Review Expectations

When reviewing, watch for:

- **Hardcoded values** — economy numbers belong in config, not code
- **Missing analytics events** — new features need tracking per the tracking plan
- **State machine compliance** — board logic must go through the state machine; no shortcuts
- **Config-driven design** — if a value might change during tuning, it should be in config

## Contributing Levels

1. Generate the level skeleton with `generate_level.py`:
   ```bash
   python tools/generate_level.py --id 42 --grid 9x9 --colors 4 --moves 25
   ```
2. Validate the config:
   ```bash
   python tools/build_config.py --validate levels/level_042.json
   ```
3. Playtest in-game — target the completion rate for the level's difficulty tier
4. Include playtest results in your PR description

## Economy Changes

Economy touches are high-risk. Follow this order strictly:

1. **Update the doc first** — edit `docs/economy/economy-model.md` with the proposed change
2. **Get approval** — economy changes need explicit sign-off before implementation
3. **Run the economy sim** to verify pacing isn't broken:
   ```bash
   python tools/economy_sim.py --config docs/economy/economy-model.md
   ```
4. **Implement** only after the above steps pass
5. **Include sim results** in your PR

## Design Changes

1. Write up the proposal in `docs/` (use an existing template or create a new doc)
2. Get approval from the team
3. Implement after approval
4. Update any related docs to stay in sync

## What NOT to Do

- **Don't add new currencies.** The currency set is locked. If you think we need one, open a discussion first.
- **Don't skip milestone gates.** Milestone levels are hand-tuned and gated intentionally.
- **Don't hardcode economy values.** Everything goes through config files.
- **Don't commit without running the pre-commit hook.** If the hook fails, fix the issue before pushing.
- **Don't merge your own PR** without a review.
