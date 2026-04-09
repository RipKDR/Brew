# Build Continuation Prompts

Index of all prompts for continuing the Brew build across new AI agent conversations.

## How to Use

1. Open a new Cursor/AI conversation in the `T:\Brew` workspace
2. Paste the appropriate prompt from this folder
3. The agent will read the referenced docs and execute the phase

## Prompt Index


| Prompt                               | When to Use                                   | Phase        |
| ------------------------------------ | --------------------------------------------- | ------------ |
| `00-context-recovery.md`             | Start of EVERY new session                    | Always first |
| `01-week-1-2-foundation.md`          | Beginning implementation                      | Weeks 1-2    |
| `02-week-3-4-core-loop.md`           | After foundation milestone passes             | Weeks 3-4    |
| `03-week-5-6-feel-and-juice.md`      | After core loop milestone passes              | Weeks 5-6    |
| `04-week-7-8-meta-and-economy.md`    | After feel milestone passes                   | Weeks 7-8    |
| `05-week-9-10-content-and-polish.md` | After meta milestone passes                   | Weeks 9-10   |
| `06-qa-testing.md`                   | Before any milestone gate or build submission | Any time     |
| `07-economy-tuning.md`               | When adjusting currency/monetization values   | Any time     |
| `08-level-design.md`                 | When creating or modifying levels             | Any time     |
| `09-soft-launch-prep.md`             | After Week 10, preparing for soft launch      | Post-MVP     |
| `10-post-launch-liveops.md`          | After soft launch, running live operations    | Post-launch  |


## Rules

- ALWAYS start with `00-context-recovery.md` in a new session
- Follow prompts IN ORDER for the main build (01 through 05)
- Use utility prompts (06-10) as needed alongside the main build
- Never skip a milestone gate — check `docs/production/milestone-gates.md` before proceeding