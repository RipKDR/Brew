#!/usr/bin/env python3
"""
Brew — Context Snapshot Builder

Generates a compact project snapshot for fast session resume.
The output file is designed for both humans and coding agents.
"""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
from datetime import UTC, datetime
from pathlib import Path


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def extract_line(prefix: str, text: str) -> str:
    for line in text.splitlines():
        if line.startswith(prefix):
            return line[len(prefix) :].strip()
    return "Unknown"


def extract_section(md: str, heading: str) -> list[str]:
    lines = md.splitlines()
    start = None
    for idx, line in enumerate(lines):
        if line.strip() == heading:
            start = idx + 1
            break
    if start is None:
        return []

    end = len(lines)
    for idx in range(start, len(lines)):
        if lines[idx].startswith("## "):
            end = idx
            break
    return [line for line in lines[start:end] if line.strip()]


def parse_adr_index(adr_readme: str) -> list[dict[str, str]]:
    entries: list[dict[str, str]] = []
    in_table = False
    for line in adr_readme.splitlines():
        stripped = line.strip()
        if stripped == "## ADR Index":
            in_table = True
            continue
        if in_table and stripped.startswith("|") and "Number" not in stripped and "---" not in stripped:
            parts = [cell.strip() for cell in stripped.strip("|").split("|")]
            if len(parts) >= 5:
                entries.append(
                    {
                        "number": parts[0],
                        "title": parts[1],
                        "status": parts[2],
                        "date": parts[3],
                        "file": parts[4],
                    }
                )
        if in_table and stripped.startswith("## ") and stripped != "## ADR Index":
            break
    return entries


def git_output(project_root: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=project_root,
        capture_output=True,
        text=True,
        check=False,
    )
    return result.stdout.strip() if result.returncode == 0 else "Unavailable"


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Generate context-snapshot.md for Brew.")
    parser.add_argument("--project-root", default=".", help="Repository root path")
    parser.add_argument(
        "--output",
        default="docs/project-management/context-snapshot.md",
        help="Output markdown path (relative to project root)",
    )
    args = parser.parse_args(argv)

    root = Path(args.project_root).resolve()
    output_path = (root / args.output).resolve()

    project_summary_path = root / "docs/project-management/project-summary.md"
    handoff_path = root / "docs/project-management/session-handoff.md"
    changelog_path = root / "CHANGELOG.md"
    adr_readme_path = root / "docs/adr/README.md"

    required = [project_summary_path, handoff_path, changelog_path, adr_readme_path]
    missing = [str(path.relative_to(root)) for path in required if not path.is_file()]
    if missing:
        print("Missing required files for snapshot generation:")
        for path in missing:
            print(f"- {path}")
        return 1

    project_summary = read_text(project_summary_path)
    handoff = read_text(handoff_path)
    changelog = read_text(changelog_path)
    adr_readme = read_text(adr_readme_path)

    summary_last_updated = extract_line("> Last updated:", project_summary)
    handoff_last_updated = extract_line("> Last updated:", handoff)
    generated_at = datetime.now(UTC).strftime("%Y-%m-%d %H:%M UTC")

    branch = git_output(root, "branch", "--show-current")
    recent_commits = git_output(root, "log", "-5", "--pretty=format:%h %ad %s", "--date=short")

    milestone_focus = extract_section(handoff, "## Current Milestone Focus")
    next_tasks = extract_section(handoff, "## Next 3 Tasks")
    blockers = extract_section(handoff, "## Blockers / Risks")
    decisions = extract_section(handoff, "## Decisions Recorded This Session")
    adr_entries = parse_adr_index(adr_readme)

    changelog_match = re.search(r"## \[Unreleased\](.*?)(?:\n## \[|\Z)", changelog, flags=re.S)
    unreleased_section = changelog_match.group(1).strip() if changelog_match else "No unreleased notes found."

    output_lines: list[str] = []
    output_lines.append("# Brew — Context Snapshot")
    output_lines.append("")
    output_lines.append(f"> Generated: {generated_at}")
    output_lines.append(f"> Project summary updated: {summary_last_updated}")
    output_lines.append(f"> Session handoff updated: {handoff_last_updated}")
    output_lines.append("")
    output_lines.append("## Quick Resume")
    output_lines.append("")
    output_lines.append(f"- Branch: `{branch}`")
    output_lines.append("- Canonical handoff: `docs/project-management/session-handoff.md`")
    output_lines.append("- ADR index: `docs/adr/README.md`")
    output_lines.append("- Changelog: `CHANGELOG.md`")
    output_lines.append("")
    output_lines.append("## Current Milestone Focus")
    output_lines.append("")
    if milestone_focus:
        output_lines.extend(milestone_focus)
    else:
        output_lines.append("- Not specified in session handoff.")
    output_lines.append("")
    output_lines.append("## Next 3 Tasks")
    output_lines.append("")
    if next_tasks:
        output_lines.extend(next_tasks)
    else:
        output_lines.append("- No next tasks listed.")
    output_lines.append("")
    output_lines.append("## Blockers / Risks")
    output_lines.append("")
    if blockers:
        output_lines.extend(blockers)
    else:
        output_lines.append("- No blockers listed.")
    output_lines.append("")
    output_lines.append("## Decisions This Session")
    output_lines.append("")
    if decisions:
        output_lines.extend(decisions)
    else:
        output_lines.append("- No session decisions recorded.")
    output_lines.append("")
    output_lines.append("## ADR Summary")
    output_lines.append("")
    if adr_entries:
        output_lines.append("| Number | Title | Status | Date |")
        output_lines.append("|---|---|---|---|")
        for entry in adr_entries:
            output_lines.append(
                f"| {entry['number']} | {entry['title']} | {entry['status']} | {entry['date']} |"
            )
    else:
        output_lines.append("- No ADR entries found.")
    output_lines.append("")
    output_lines.append("## Changelog (Unreleased)")
    output_lines.append("")
    output_lines.append(unreleased_section)
    output_lines.append("")
    output_lines.append("## Recent Git Commits")
    output_lines.append("")
    if recent_commits and recent_commits != "Unavailable":
        for line in recent_commits.splitlines():
            output_lines.append(f"- {line}")
    else:
        output_lines.append("- Unavailable")
    output_lines.append("")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text("\n".join(output_lines), encoding="utf-8")
    print(f"Wrote context snapshot: {output_path.relative_to(root)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
