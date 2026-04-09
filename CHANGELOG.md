# Changelog

All notable changes to Brew are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), with project-specific sections for context and documentation integrity.

## [Unreleased]

### Added

- Added canonical session continuity doc at `docs/project-management/session-handoff.md`.
- Added ADR system scaffolding:
  - `docs/adr/README.md`
  - `docs/adr/0001-canonical-unity-version.md`

### Changed

- Updated CI required-doc checks in `.github/workflows/ci.yml` to match actual repository paths.
- Added context continuity CI job (`validate-context`) in `.github/workflows/ci.yml`.
- Aligned canonical engine version to Unity 6 (6000.1 LTS) across:
  - `docs/project-management/project-summary.md`
  - `docs/onboarding/new-team-member-guide.md`
  - `README.md`