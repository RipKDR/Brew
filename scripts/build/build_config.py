#!/usr/bin/env python3
"""
Brew — Build Configuration Validator

Validates project readiness before a build:
  - Environment variables
  - Level JSON files (existence, uniqueness, schema)
  - Remote config defaults
  - Version number format

Exit code 0 = all checks pass, 1 = at least one failure.
Designed for CI integration.

Usage:
    python build_config.py
    python build_config.py --levels-dir levels/ --level-range 1-40
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from pathlib import Path

REQUIRED_ENV_VARS = [
    "FIREBASE_PROJECT_ID",
]

ANDROID_ENV_VARS = [
    "KEYSTORE_PATH",
]

IOS_ENV_VARS = [
    "APPLE_TEAM_ID",
]

REQUIRED_REMOTE_CONFIG_KEYS = [
    "daily_brew_essence",
    "daily_brew_gems",
    "booster_shake_cost",
    "booster_catalyst_cost",
    "streak_multiplier_cap",
    "event_enabled",
    "maintenance_mode",
]

VERSION_PATTERN = re.compile(r"^\d+\.\d+\.\d+$")

LEVEL_SCHEMA_REQUIRED_KEYS = {
    "level_id": int,
    "grid_width": int,
    "grid_height": int,
    "ingredient_pool": list,
    "recipe_targets": list,
    "move_limit": int,
    "star_thresholds": list,
}


class CheckResult:
    def __init__(self, name: str):
        self.name = name
        self.passed = True
        self.messages: list[str] = []

    def fail(self, msg: str) -> None:
        self.passed = False
        self.messages.append(f"  FAIL: {msg}")

    def warn(self, msg: str) -> None:
        self.messages.append(f"  WARN: {msg}")

    def ok(self, msg: str) -> None:
        self.messages.append(f"  OK:   {msg}")

    def print(self) -> None:
        status = "PASS" if self.passed else "FAIL"
        print(f"\n[{status}] {self.name}")
        for m in self.messages:
            print(m)


def check_env_vars() -> CheckResult:
    result = CheckResult("Environment Variables")

    for var in REQUIRED_ENV_VARS:
        if os.environ.get(var):
            result.ok(f"{var} is set")
        else:
            result.fail(f"{var} is not set")

    for var in ANDROID_ENV_VARS:
        if os.environ.get(var):
            result.ok(f"{var} is set (Android)")
        else:
            result.warn(f"{var} is not set (needed for Android builds)")

    for var in IOS_ENV_VARS:
        if os.environ.get(var):
            result.ok(f"{var} is set (iOS)")
        else:
            result.warn(f"{var} is not set (needed for iOS builds)")

    return result


def check_level_files(levels_dir: Path, level_start: int, level_end: int) -> CheckResult:
    result = CheckResult(f"Level Files ({level_start}-{level_end})")

    if not levels_dir.is_dir():
        result.fail(f"Levels directory not found: {levels_dir}")
        return result

    json_files = sorted(levels_dir.glob("*.json"))
    if not json_files:
        result.fail(f"No JSON files found in {levels_dir}")
        return result

    seen_ids: dict[int, Path] = {}
    valid_count = 0

    for jf in json_files:
        try:
            with open(jf, "r", encoding="utf-8") as f:
                data = json.load(f)
        except (json.JSONDecodeError, OSError) as e:
            result.fail(f"{jf.name}: invalid JSON - {e}")
            continue

        # Schema validation
        schema_ok = True
        for key, expected_type in LEVEL_SCHEMA_REQUIRED_KEYS.items():
            if key not in data:
                result.fail(f"{jf.name}: missing required key '{key}'")
                schema_ok = False
            elif not isinstance(data[key], expected_type):
                result.fail(
                    f"{jf.name}: '{key}' should be {expected_type.__name__}, "
                    f"got {type(data[key]).__name__}"
                )
                schema_ok = False

        if not schema_ok:
            continue

        lid = data["level_id"]

        # Duplicate check
        if lid in seen_ids:
            result.fail(f"{jf.name}: duplicate level_id {lid} (also in {seen_ids[lid].name})")
        else:
            seen_ids[lid] = jf

        # Grid bounds
        if not (5 <= data["grid_width"] <= 11):
            result.fail(f"{jf.name}: grid_width {data['grid_width']} outside [5, 11]")
        if not (5 <= data["grid_height"] <= 11):
            result.fail(f"{jf.name}: grid_height {data['grid_height']} outside [5, 11]")

        # Move limit
        if data["move_limit"] <= 0:
            result.fail(f"{jf.name}: move_limit must be > 0")

        # Star thresholds
        stars = data.get("star_thresholds", [])
        if len(stars) != 3 or stars != sorted(stars):
            result.fail(f"{jf.name}: star_thresholds must be 3 ascending values")

        # Recipe targets reference pool
        pool = set(data.get("ingredient_pool", []))
        for target in data.get("recipe_targets", []):
            if target.get("ingredient") not in pool:
                result.fail(
                    f"{jf.name}: recipe ingredient '{target.get('ingredient')}' "
                    f"not in pool"
                )

        valid_count += 1

    # Check expected range coverage
    for lid in range(level_start, level_end + 1):
        if lid not in seen_ids:
            result.fail(f"Level {lid} not found in {levels_dir}")

    if valid_count == len(json_files):
        result.ok(f"All {valid_count} level files pass schema validation")
    else:
        result.fail(f"{valid_count}/{len(json_files)} level files valid")

    return result


def check_remote_config(project_root: Path) -> CheckResult:
    result = CheckResult("Remote Config Defaults")

    candidates = [
        project_root / "remote_config_defaults.json",
        project_root / "Assets" / "StreamingAssets" / "remote_config_defaults.json",
        project_root / "config" / "remote_config_defaults.json",
    ]

    config_path = None
    for c in candidates:
        if c.is_file():
            config_path = c
            break

    if config_path is None:
        result.warn(
            "remote_config_defaults.json not found (checked project root, "
            "Assets/StreamingAssets/, config/). Skipping key validation."
        )
        return result

    try:
        with open(config_path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (json.JSONDecodeError, OSError) as e:
        result.fail(f"Cannot parse {config_path}: {e}")
        return result

    result.ok(f"Found config at {config_path}")

    for key in REQUIRED_REMOTE_CONFIG_KEYS:
        if key in data:
            result.ok(f"Key '{key}' present")
        else:
            result.fail(f"Missing required key '{key}'")

    return result


def check_version(project_root: Path) -> CheckResult:
    result = CheckResult("Version Number")

    settings_path = project_root / "ProjectSettings" / "ProjectSettings.asset"
    if not settings_path.is_file():
        result.warn(
            f"{settings_path} not found - cannot verify version format. "
            "This is expected outside a Unity project directory."
        )
        return result

    try:
        content = settings_path.read_text(encoding="utf-8", errors="replace")
    except OSError as e:
        result.fail(f"Cannot read {settings_path}: {e}")
        return result

    match = re.search(r"bundleVersion:\s*(.+)", content)
    if not match:
        result.fail("bundleVersion not found in ProjectSettings.asset")
        return result

    version = match.group(1).strip()
    if VERSION_PATTERN.match(version):
        result.ok(f"Version '{version}' matches major.minor.patch format")
    else:
        result.fail(f"Version '{version}' does not match major.minor.patch format")

    return result


def check_required_docs(project_root: Path) -> CheckResult:
    result = CheckResult("Required Documentation")

    required_docs = [
        "docs/game-design/GDD.md",
        "docs/game-design/core-mechanic.md",
        "docs/economy/economy-model.md",
        "docs/technical/architecture.md",
        "docs/production/mvp-build-plan.md",
        "docs/production/milestone-gates.md",
        "docs/project-management/project-summary.md",
    ]

    for doc in required_docs:
        path = project_root / doc
        if path.is_file():
            result.ok(f"{doc}")
        else:
            result.fail(f"{doc} not found")

    return result


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Validate Brew build configuration.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--project-root",
        type=str,
        default=".",
        help="Project root directory (default: current directory)",
    )
    parser.add_argument(
        "--levels-dir",
        type=str,
        default=None,
        help="Level JSON directory (default: <project-root>/levels/)",
    )
    parser.add_argument(
        "--level-range",
        type=str,
        default="1-40",
        help="Expected level ID range as START-END (default: 1-40)",
    )

    args = parser.parse_args(argv)
    root = Path(args.project_root).resolve()
    levels_dir = Path(args.levels_dir) if args.levels_dir else root / "levels"

    start_str, end_str = args.level_range.split("-", 1)
    level_start, level_end = int(start_str), int(end_str)

    checks = [
        check_env_vars(),
        check_level_files(levels_dir, level_start, level_end),
        check_remote_config(root),
        check_version(root),
        check_required_docs(root),
    ]

    print("=" * 50)
    print("  Brew Build Validation Report")
    print("=" * 50)

    for c in checks:
        c.print()

    total = len(checks)
    passed = sum(1 for c in checks if c.passed)
    failed = total - passed

    print("\n" + "=" * 50)
    print(f"  Results: {passed}/{total} checks passed", end="")
    if failed:
        print(f", {failed} FAILED")
    else:
        print(" -- all clear")
    print("=" * 50)

    return 0 if failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
