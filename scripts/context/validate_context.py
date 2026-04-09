#!/usr/bin/env python3
"""
Brew — Context Integrity Validator

Checks context continuity artifacts for freshness and structural integrity.
"""

from __future__ import annotations

import argparse
import re
import sys
from datetime import UTC, datetime
from pathlib import Path


DATE_PATTERN = re.compile(r"^\d{4}-\d{2}-\d{2}$")
LINK_PATTERN = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")


class Reporter:
    def __init__(self) -> None:
        self.failed = False

    def ok(self, msg: str) -> None:
        print(f"[OK]   {msg}")

    def fail(self, msg: str) -> None:
        self.failed = True
        print(f"[FAIL] {msg}")


def read_file(path: Path, reporter: Reporter) -> str | None:
    if not path.is_file():
        reporter.fail(f"Missing file: {path}")
        return None
    return path.read_text(encoding="utf-8")


def parse_last_updated(text: str) -> str | None:
    for line in text.splitlines():
        if line.startswith("> Last updated:"):
            return line.split(":", 1)[1].strip()
    return None


def validate_handoff(handoff_text: str, max_age_days: int, reporter: Reporter) -> None:
    required_sections = [
        "## Current Milestone Focus",
        "## Completed Since Last Handoff",
        "## Next 3 Tasks",
        "## Blockers / Risks",
        "## Handoff Checklist",
    ]
    for section in required_sections:
        if section in handoff_text:
            reporter.ok(f"Handoff includes section: {section}")
        else:
            reporter.fail(f"Handoff missing section: {section}")

    last_updated = parse_last_updated(handoff_text)
    if not last_updated:
        reporter.fail("Handoff missing '> Last updated:' line")
        return

    if not DATE_PATTERN.match(last_updated):
        reporter.fail(f"Handoff last-updated has invalid format: {last_updated}")
        return

    updated_date = datetime.strptime(last_updated, "%Y-%m-%d").replace(tzinfo=UTC)
    now = datetime.now(UTC)
    age_days = (now - updated_date).days
    if age_days <= max_age_days:
        reporter.ok(f"Handoff freshness within threshold ({age_days} days <= {max_age_days})")
    else:
        reporter.fail(f"Handoff is stale ({age_days} days > {max_age_days})")


def parse_adr_rows(adr_text: str) -> list[tuple[str, str, str, str, str]]:
    rows: list[tuple[str, str, str, str, str]] = []
    in_table = False
    for line in adr_text.splitlines():
        stripped = line.strip()
        if stripped == "## ADR Index":
            in_table = True
            continue
        if in_table and stripped.startswith("|") and "Number" not in stripped and "---" not in stripped:
            cells = [c.strip() for c in stripped.strip("|").split("|")]
            if len(cells) >= 5:
                rows.append((cells[0], cells[1], cells[2], cells[3], cells[4]))
        if in_table and stripped.startswith("## ") and stripped != "## ADR Index":
            break
    return rows


def validate_adr(adr_text: str, adr_readme_path: Path, reporter: Reporter) -> None:
    if "## ADR Index" not in adr_text:
        reporter.fail("ADR README missing '## ADR Index' section")
        return
    reporter.ok("ADR README includes ADR Index section")

    rows = parse_adr_rows(adr_text)
    if not rows:
        reporter.fail("ADR Index has no entries")
        return
    reporter.ok(f"ADR Index contains {len(rows)} entries")

    for number, title, status, date, file_cell in rows:
        if not DATE_PATTERN.match(date):
            reporter.fail(f"ADR {number} has invalid date: {date}")
        if status.lower() not in {"proposed", "accepted", "superseded", "deprecated"}:
            reporter.fail(f"ADR {number} has invalid status: {status}")
        else:
            reporter.ok(f"ADR {number} status valid: {status}")

        link_match = LINK_PATTERN.search(file_cell)
        file_ref = link_match.group(2) if link_match else file_cell
        adr_file = (adr_readme_path.parent / file_ref).resolve()
        if adr_file.is_file():
            reporter.ok(f"ADR file exists for {number}: {file_ref}")
        else:
            reporter.fail(f"ADR file missing for {number}: {file_ref}")

        if not title:
            reporter.fail(f"ADR {number} title is empty")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Validate Brew context continuity artifacts.")
    parser.add_argument("--project-root", default=".", help="Repository root path")
    parser.add_argument(
        "--handoff-file",
        default="docs/project-management/session-handoff.md",
        help="Path to handoff markdown file (relative to project root)",
    )
    parser.add_argument(
        "--adr-readme",
        default="docs/adr/README.md",
        help="Path to ADR index markdown file (relative to project root)",
    )
    parser.add_argument(
        "--max-handoff-age-days",
        type=int,
        default=14,
        help="Maximum allowed age of session handoff in days",
    )
    args = parser.parse_args(argv)

    root = Path(args.project_root).resolve()
    handoff_path = (root / args.handoff_file).resolve()
    adr_readme_path = (root / args.adr_readme).resolve()

    reporter = Reporter()

    handoff_text = read_file(handoff_path, reporter)
    adr_text = read_file(adr_readme_path, reporter)

    if handoff_text is not None:
        validate_handoff(handoff_text, args.max_handoff_age_days, reporter)

    if adr_text is not None:
        validate_adr(adr_text, adr_readme_path, reporter)

    return 1 if reporter.failed else 0


if __name__ == "__main__":
    sys.exit(main())
