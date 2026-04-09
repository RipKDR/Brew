# Context Recovery — Paste This at the Start of Every New Session

You are continuing work on **Brew**, a mobile puzzle game for iOS and Android. The project lives at `T:\Brew`. Brew is a one-tap cluster puzzle where players fuse same-colored ingredient tokens into growing Brew Orbs that chain-fuse and eventually brew into potions to fill recipes — all before running out of moves. It is NOT a match-3 game. The core mechanic is tap-cluster-fuse-brew.

**Tech stack:** Unity 6 (6000.1 LTS), C# 11, Firebase (Auth/Firestore/Cloud Functions/Remote Config/Analytics/Crashlytics), AdMob + AppLovin MAX mediation, Unity IAP, GitHub Actions + Fastlane CI/CD.

---

## Step 1 — Read Project Rules

Read `AGENTS.md` in the project root. This is the non-negotiable source of truth for architecture, code conventions, naming, folder structure, the board state machine, testing expectations, and common mistakes to avoid. Follow every rule in it.

Then read all files in `.cursor/rules/` — there are four rule files (`brew-project.mdc`, `analytics-tracking.mdc`, `economy-guard.mdc`, `unity-csharp.mdc`) that enforce project-specific coding standards. Internalize these before writing any code.

## Step 2 — Read Session Continuity

Read these files in order to understand where the project stands:

1. `docs/project-management/session-handoff.md` — tells you the current milestone focus, what was completed last session, what's in progress, what the next 3 tasks are, and any blockers.
2. `docs/project-management/context-snapshot.md` — a machine-generated summary of the project's structural state (file counts, code metrics, doc coverage). If this file doesn't exist or is stale, skip it.
3. `docs/project-management/project-summary.md` — the living project summary with key dates, milestone status, team roles, decision log, and open questions.

## Step 3 — Check Recent Work

Run `git log --oneline -20` to see the last 20 commits and understand what was recently built or changed.

Run `git status` to see if there's any work in progress (uncommitted changes, untracked files).

## Step 4 — Determine Current Phase

Based on what you read, determine which build phase the project is in:

- **Pre-implementation:** No Unity gameplay code exists beyond the starter stubs in `Assets/Scripts/`. Begin with `01-week-1-2-foundation.md`.
- **Foundation (Weeks 1-2):** BoardManager, TokenSpawner, InputHandler, basic fusion, gravity. Working toward Milestone Gate 1.
- **Core Loop (Weeks 3-4):** Chain fusion, brew detection, recipe tracking, level loading, win/lose. Working toward Milestone Gate 2.
- **Feel & Juice (Weeks 5-6):** Tutorial, art integration, VFX, SFX, haptics, boosters. Working toward Milestone Gate 3.
- **Meta & Economy (Weeks 7-8):** Workshop, potion shelf, streaks, daily brew, IAP, ads, Firebase. Working toward Milestone Gate 4.
- **Content & Polish (Weeks 9-10):** All 40 levels tuned, performance optimization, store assets, soft launch prep. Working toward Milestone Gate 5.

Tell me which phase you've determined and what you plan to work on before starting.

## Step 5 — Critical Rules (Never Violate These)

1. **Two currencies only:** Essence (soft) and Gems (premium). Never add a third currency — no energy, tickets, event tokens, or seasonal coins.
2. **No hardcoded values:** Every economy number, gameplay tunable, and threshold must come from a ScriptableObject or Firebase Remote Config. If you're typing a currency amount into a `.cs` file, stop.
3. **Board state machine is sacred:** `Idle → WaitingForInput → ProcessingFusion → Cascading → Settling → CheckingBrew → CheckingWinLose → Idle`. Never skip states, never process input during resolution. See `docs/sdk/state-machine-spec.md`.
4. **Fire analytics events:** Every gameplay action, economy transaction, and meta event must fire its corresponding event from `docs/analytics/event-tracking-plan.md`. Missing analytics is a blocking bug.
5. **Pure C# for game logic:** Board logic, fusion, cascading, scoring live in `Scripts/Core/` as plain C# classes with zero MonoBehaviour dependency. MonoBehaviours are only for Unity lifecycle hooks and presentation.
6. **Object pooling mandatory:** Never `Instantiate`/`Destroy` tokens, orbs, or particles in the gameplay loop.
7. **Milestone gates are mandatory:** Before moving to the next phase, verify all criteria in `docs/production/milestone-gates.md` for the current gate. Do not skip ahead.
8. **Docs are source of truth:** If a proposed change contradicts any spec in `docs/`, stop and ask before implementing.

## Step 6 — Session Completion

At the END of every session, before you finish, you MUST:

1. Update `docs/project-management/session-handoff.md` with:
  - What milestone you're focused on
  - What was completed this session
  - What's in progress (unfinished)
  - Next 3 tasks for the next session
  - Any new blockers or risks
  - Files touched this session
2. Update `docs/project-management/project-summary.md` Decision Log if any architectural or design decisions were made.
3. Run `python scripts/context/build_context_snapshot.py --project-root .` to regenerate the context snapshot.
4. If any cross-cutting architectural decisions were made, add an ADR to `docs/adr/` and update `docs/adr/README.md`.

---

Now read the files listed above and tell me: What phase is the project in? What was done last? What should we work on next?