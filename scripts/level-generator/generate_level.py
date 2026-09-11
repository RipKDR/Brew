#!/usr/bin/env python3
"""
Brew — Level JSON Generator

Generates level configuration files for the Brew puzzle game.
Supports single-level generation, difficulty presets, and batch
generation with automatic difficulty ramping.

Usage:
    # Single level
    python generate_level.py --level-id 1 --difficulty easy --output-dir levels/

    # Custom recipe
    python generate_level.py --level-id 5 --recipe "ember:2,frost:1" --output-dir levels/

    # Batch generate levels 1-40 with auto-ramping difficulty
    python generate_level.py --batch-start 1 --batch-end 40 --difficulty-curve --output-dir levels/
"""

from __future__ import annotations

import argparse
import json
import math
import os
import random
import sys
from pathlib import Path

INGREDIENTS = ["ember", "frost", "vine", "brine", "glow"]

GRID_MIN, GRID_MAX = 5, 11

DIFFICULTY_PRESETS = {
    "easy": {
        "width": 7,
        "height": 9,
        "colors": 3,
        "move_bonus": 8,       # extra moves added to base
        "blocker_chance": 0.0,
        "recipe_max": 2,
    },
    "medium": {
        "width": 7,
        "height": 9,
        "colors": 4,
        "move_bonus": 3,
        "blocker_chance": 0.15,
        "recipe_max": 3,
    },
    "hard": {
        "width": 8,
        "height": 10,
        "colors": 5,
        "move_bonus": -2,      # fewer moves than base
        "blocker_chance": 0.30,
        "recipe_max": 4,
    },
}


def pick_ingredient_pool(count: int) -> list[str]:
    return INGREDIENTS[:count]


def generate_recipe(pool: list[str], max_targets: int) -> list[dict]:
    n_targets = random.randint(1, min(max_targets, len(pool)))
    chosen = random.sample(pool, n_targets)
    return [{"ingredient": ing, "count": random.randint(1, 3)} for ing in chosen]


def calculate_base_moves(recipe: list[dict], width: int, height: int) -> int:
    total_potions = sum(t["count"] for t in recipe)
    cells = width * height
    base = int(total_potions * 8 + cells * 0.15)
    return max(base, 10)


def calculate_star_thresholds(moves: int, recipe: list[dict]) -> list[int]:
    recipe_weight = sum(t["count"] for t in recipe)
    base_score = moves * 40 + recipe_weight * 200
    return [
        int(base_score * 0.5),
        int(base_score * 1.0),
        int(base_score * 1.6),
    ]


def generate_blockers(width: int, height: int, chance: float) -> list[dict]:
    """Place stones with soft-launch constraints: no top row, no orthogonal adjacency."""
    if chance <= 0:
        return []
    blockers: list[dict] = []
    occupied: set[tuple[int, int]] = set()
    # row 0 is top — never place stones there (blocks refill entry)
    for r in range(1, height - 1):
        for c in range(1, width - 1):
            if random.random() >= chance * 0.1:
                continue
            neighbors = {(r - 1, c), (r + 1, c), (r, c - 1), (r, c + 1)}
            if occupied & neighbors:
                continue
            occupied.add((r, c))
            blockers.append({"row": r, "col": c, "type": "stone"})
    return blockers



def validate_level(level: dict) -> list[str]:
    errors = []
    w, h = level["grid_width"], level["grid_height"]
    if not (GRID_MIN <= w <= GRID_MAX):
        errors.append(f"grid_width {w} outside [{GRID_MIN}, {GRID_MAX}]")
    if not (GRID_MIN <= h <= GRID_MAX):
        errors.append(f"grid_height {h} outside [{GRID_MIN}, {GRID_MAX}]")
    if level["move_limit"] <= 0:
        errors.append(f"move_limit must be > 0, got {level['move_limit']}")
    pool = set(level["ingredient_pool"])
    for target in level["recipe_targets"]:
        if target["ingredient"] not in pool:
            errors.append(
                f"recipe ingredient '{target['ingredient']}' not in pool {pool}"
            )
        if target["count"] <= 0:
            errors.append(
                f"recipe count for '{target['ingredient']}' must be > 0"
            )
    stars = level["star_thresholds"]
    if len(stars) != 3 or stars != sorted(stars):
        errors.append(f"star_thresholds must be 3 ascending values, got {stars}")

    blockers = level.get("blocker_placements") or []
    stones = []
    for b in blockers:
        if str(b.get("type", "")).lower() != "stone":
            continue
        row, col = int(b["row"]), int(b["col"])
        if row <= 0:
            errors.append(f"stone at ({row},{col}) cannot be in top row")
        stones.append((row, col))
    stone_set = set(stones)
    for row, col in stone_set:
        if (row, col + 1) in stone_set or (row + 1, col) in stone_set:
            errors.append(f"adjacent stones near ({row},{col})")

    return errors


def parse_recipe_string(recipe_str: str) -> list[dict]:
    targets = []
    for pair in recipe_str.split(","):
        pair = pair.strip()
        if ":" not in pair:
            raise ValueError(f"Invalid recipe token '{pair}', expected 'name:count'")
        name, count_str = pair.split(":", 1)
        targets.append({"ingredient": name.strip(), "count": int(count_str.strip())})
    return targets


def build_level(
    level_id: int,
    width: int,
    height: int,
    colors: int,
    moves: int | None,
    recipe: list[dict] | None,
    difficulty: str,
    is_tutorial: bool = False,
) -> dict:
    preset = DIFFICULTY_PRESETS.get(difficulty, DIFFICULTY_PRESETS["medium"])

    width = width or preset["width"]
    height = height or preset["height"]
    colors = max(2, min(5, colors or preset["colors"]))

    pool = pick_ingredient_pool(colors)

    if recipe is None:
        recipe = generate_recipe(pool, preset["recipe_max"])

    if moves is None:
        base = calculate_base_moves(recipe, width, height)
        moves = max(5, base + preset["move_bonus"])

    blockers = generate_blockers(width, height, preset["blocker_chance"])
    stars = calculate_star_thresholds(moves, recipe)

    level = {
        "level_id": level_id,
        "grid_width": width,
        "grid_height": height,
        "grid_mask": [],
        "ingredient_pool": pool,
        "recipe_targets": recipe,
        "move_limit": moves,
        "blocker_placements": blockers,
        "star_thresholds": stars,
        "is_tutorial": is_tutorial,
        "tutorial_steps": [],
    }

    errors = validate_level(level)
    if errors:
        raise ValueError(
            f"Level {level_id} failed validation:\n  " + "\n  ".join(errors)
        )
    return level


def difficulty_for_index(index: int, total: int) -> str:
    """Map a position in a batch range to a difficulty tier."""
    pct = index / max(total, 1)
    if pct < 0.35:
        return "easy"
    if pct < 0.70:
        return "medium"
    return "hard"


def write_level(level: dict, output_dir: Path) -> Path:
    output_dir.mkdir(parents=True, exist_ok=True)
    filename = f"level_{level['level_id']:03d}.json"
    path = output_dir / filename
    with open(path, "w", encoding="utf-8") as f:
        json.dump(level, f, indent=2)
    return path


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Generate Brew level JSON files.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )

    single = parser.add_argument_group("single level")
    single.add_argument("--level-id", type=int, help="Level ID for single generation")
    single.add_argument("--width", type=int, default=None, help="Grid width (default from difficulty preset)")
    single.add_argument("--height", type=int, default=None, help="Grid height (default from difficulty preset)")
    single.add_argument("--colors", type=int, default=None, help="Number of ingredient colors (2-5)")
    single.add_argument("--moves", type=int, default=None, help="Move limit (auto-calculated if omitted)")
    single.add_argument(
        "--recipe",
        type=str,
        default=None,
        help='Recipe targets, e.g. "ember:2,frost:1"',
    )
    single.add_argument(
        "--difficulty",
        choices=["easy", "medium", "hard"],
        default="medium",
        help="Difficulty preset (default: medium)",
    )

    batch = parser.add_argument_group("batch generation")
    batch.add_argument("--batch-start", type=int, help="First level ID in batch")
    batch.add_argument("--batch-end", type=int, help="Last level ID in batch")
    batch.add_argument(
        "--difficulty-curve",
        action="store_true",
        help="Auto-ramp difficulty across the batch range",
    )

    parser.add_argument(
        "--output-dir",
        type=str,
        default="levels",
        help="Directory to write level JSON files (default: levels/)",
    )
    parser.add_argument("--seed", type=int, default=None, help="Random seed for reproducibility")

    args = parser.parse_args(argv)
    out = Path(args.output_dir)

    if args.seed is not None:
        random.seed(args.seed)

    recipe = None
    if args.recipe:
        try:
            recipe = parse_recipe_string(args.recipe)
        except ValueError as e:
            print(f"ERROR: {e}", file=sys.stderr)
            return 1

    is_batch = args.batch_start is not None and args.batch_end is not None

    if not is_batch and args.level_id is None:
        parser.error("Provide --level-id for single generation or --batch-start/--batch-end for batch.")

    if is_batch:
        if args.batch_start > args.batch_end:
            parser.error("--batch-start must be <= --batch-end")

        total = args.batch_end - args.batch_start + 1
        generated = 0

        for lid in range(args.batch_start, args.batch_end + 1):
            diff = (
                difficulty_for_index(lid - args.batch_start, total)
                if args.difficulty_curve
                else args.difficulty
            )
            try:
                level = build_level(
                    level_id=lid,
                    width=args.width,
                    height=args.height,
                    colors=args.colors,
                    moves=args.moves,
                    recipe=recipe,
                    difficulty=diff,
                    is_tutorial=(lid <= 3),
                )
                path = write_level(level, out)
                generated += 1
                print(f"  [OK] {path}  (difficulty={diff})")
            except ValueError as e:
                print(f"  [FAIL] Level {lid}: {e}", file=sys.stderr)
                return 1

        print(f"\nBatch complete: {generated} levels written to {out}/")
    else:
        try:
            level = build_level(
                level_id=args.level_id,
                width=args.width,
                height=args.height,
                colors=args.colors,
                moves=args.moves,
                recipe=recipe,
                difficulty=args.difficulty,
            )
            path = write_level(level, out)
            print(f"  [OK] {path}")
            print(json.dumps(level, indent=2))
        except ValueError as e:
            print(f"  [FAIL] {e}", file=sys.stderr)
            return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
