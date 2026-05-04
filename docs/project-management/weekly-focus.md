# Brew — Weekly Focus

> Short-horizon operational focus for the current week.
> Update this document at least once per week and whenever priorities shift.
> Week of: 2026-05-04

---

## Primary Outcome

Gate 5 workspace hardening sprint: compile fix, missing level, session doc refresh, infrastructure gaps (CLAUDE.md, Fastfile, CI improvements), and test coverage for GameFlowController + board state machine integration.

## Must Complete This Week

- [x] Fix BrewSceneSetup.cs — restore 4 missing using directives (compile error on current branch)
- [x] Add level_001.json — 39/40 levels existed; Gate 5 criterion 5.3 requires all 40
- [x] Add project-level CLAUDE.md for AI session continuity
- [x] Add Fastfile + ExportOptions.plist (referenced in build.yml, previously absent)
- [ ] Improve ci.yml — pip cache on validate-economy, secret presence check, job summaries
- [ ] Write GameFlowControllerTests — 8+ tests for the main system orchestrator
- [ ] Write BoardStateMachineIntegrationTests — full cycle integration test
- [ ] Import Firebase/IAP/AdMob SDKs in Unity Editor
- [ ] Create ScriptableObject assets and wire to GameFlowController inspector
- [ ] Run all EditMode tests in Unity Editor — fix any failures

## Secondary Work

- App store asset preparation (icon, 5 screenshots per platform, preview video)
- Device matrix testing (8+ devices for Gate 5 criterion 5.5)
- Verify weekly event flow end-to-end in Unity Editor

## Risks to Watch

- Firebase SDK not yet imported — bridge classes compile only with `#if FIREBASE_*` defines
- Unity IAP and AdMob SDKs not yet configured — managers are pure C# stubs until imported
- Gate 5 criterion 5.13 (app store assets) is an art dependency, not yet started

## Owners

- Engineering: CI fixes, test coverage, Unity Editor SDK integration
- Product/Engineering: App store assets, device testing, final Gate 5 sign-off
