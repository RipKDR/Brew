#!/usr/bin/env python3
"""
Brew — Economy Simulator

Simulates a player's currency accumulation and spending over time to validate
that the economy pacing (workshop completion, gem income, booster usage)
matches design targets from docs/economy/economy-model.md.

Usage:
    # Regular non-payer simulation
    python simulate_economy.py --days 45

    # High-engagement player
    python simulate_economy.py --days 30 --levels-per-day 20 --star-average 2.5 --streak-average 5

    # Payer simulation
    python simulate_economy.py --days 30 --payer
"""

from __future__ import annotations

import argparse
import math
import sys

# ---------------------------------------------------------------------------
# Economy constants (must match docs/economy/economy-model.md)
# ---------------------------------------------------------------------------

ESSENCE_PER_STAR = {1: 30, 2: 50, 3: 80}

STREAK_THRESHOLDS = [
    (1,  1.0),
    (2,  1.25),
    (3,  1.5),
    (5,  2.0),
    (8,  2.5),
    (10, 3.0),
]

DAILY_BREW_ESSENCE = 100
DAILY_BREW_GEMS = 5

# Workshop upgrade costs (12 tiers, 36 350 total)
WORKSHOP_COSTS = [50, 100, 200, 400, 600, 1000, 1500, 2500, 4000, 6000, 8000, 12000]
WORKSHOP_TOTAL = sum(WORKSHOP_COSTS)  # 36 350

BOOSTER_SHAKE_ESSENCE = 50
BOOSTER_CATALYST_ESSENCE = 100

# Shelf milestone gem rewards (one-time)
SHELF_MILESTONE_GEMS = {25: 25, 50: 50, 75: 100, 100: 250}

# Weekly event rewards
EVENT_ESSENCE_MAX = 1275
EVENT_GEMS_CLEAR = 30
EVENT_GEMS_3STAR = 20

# Payer bonuses
STARTER_BUNDLE_GEMS = 50
STARTER_BUNDLE_ESSENCE = 500

TARGET_WORKSHOP_DAYS_MIN = 30
TARGET_WORKSHOP_DAYS_MAX = 45


def streak_multiplier(streak: int) -> float:
    result = 1.0
    for threshold, mult in STREAK_THRESHOLDS:
        if streak >= threshold:
            result = mult
    return result


def essence_for_star_avg(star_avg: float) -> float:
    """Weighted average essence from fractional star average."""
    lower = int(star_avg)
    upper = min(lower + 1, 3)
    frac = star_avg - lower
    lower = max(lower, 1)
    return ESSENCE_PER_STAR[lower] * (1 - frac) + ESSENCE_PER_STAR[upper] * frac


def simulate(
    days: int,
    levels_per_day: int,
    star_average: float,
    win_rate: float,
    is_payer: bool,
    streak_average: int,
) -> dict:
    essence = 0
    gems = 0
    workshop_tier = 0
    potions_collected = 0
    total_boosters_used = 0
    workshop_complete_day = None

    base_essence_per_level = essence_for_star_avg(star_average)
    mult = streak_multiplier(streak_average)

    if is_payer:
        essence += STARTER_BUNDLE_ESSENCE
        gems += STARTER_BUNDLE_GEMS

    daily_log: list[dict] = []

    # Estimate shelf progress based on levels played
    total_levels_played = 0
    shelf_pct_milestones_hit: set[int] = set()
    total_shelf_potions = 100  # assumed full shelf size

    for day in range(1, days + 1):
        day_essence_earned = 0
        day_gems_earned = 0
        day_essence_spent = 0

        # --- Level rewards ---
        wins = int(levels_per_day * win_rate)
        level_essence = int(wins * base_essence_per_level * mult)
        day_essence_earned += level_essence

        # Count potions brewed (roughly 1 potion per 2 wins)
        new_potions = wins // 2
        potions_collected += new_potions
        total_levels_played += levels_per_day

        # --- Daily brew ---
        day_essence_earned += DAILY_BREW_ESSENCE
        day_gems_earned += DAILY_BREW_GEMS

        # Daily brew streak bonuses (simplified: every 3rd day give bonus)
        if day % 3 == 0:
            day_essence_earned += int(DAILY_BREW_ESSENCE * 0.25)
            day_gems_earned += 5
        if day % 5 == 0:
            day_essence_earned += int(DAILY_BREW_ESSENCE * 0.5)
            day_gems_earned += 10
        if day % 7 == 0:
            day_essence_earned += int(DAILY_BREW_ESSENCE * 1.0)
            day_gems_earned += 15

        # --- Weekly event (every 7 days) ---
        if day % 7 == 0:
            day_essence_earned += EVENT_ESSENCE_MAX
            day_gems_earned += EVENT_GEMS_CLEAR
            if star_average >= 2.5:
                day_gems_earned += EVENT_GEMS_3STAR

        # --- Shelf milestones ---
        shelf_pct = min(100, int(potions_collected / total_shelf_potions * 100))
        for pct, gem_reward in SHELF_MILESTONE_GEMS.items():
            if shelf_pct >= pct and pct not in shelf_pct_milestones_hit:
                shelf_pct_milestones_hit.add(pct)
                day_gems_earned += gem_reward

        # --- Spending: boosters ---
        # Assume ~1 booster per 5 levels played
        boosters_today = max(1, levels_per_day // 5)
        booster_cost = boosters_today * BOOSTER_SHAKE_ESSENCE
        day_essence_spent += booster_cost
        total_boosters_used += boosters_today

        # --- Spending: workshop upgrades ---
        net_today = day_essence_earned - day_essence_spent
        essence += net_today
        gems += day_gems_earned

        # Buy workshop upgrades when affordable
        while (
            workshop_tier < len(WORKSHOP_COSTS)
            and essence >= WORKSHOP_COSTS[workshop_tier]
        ):
            essence -= WORKSHOP_COSTS[workshop_tier]
            day_essence_spent += WORKSHOP_COSTS[workshop_tier]
            workshop_tier += 1
            if workshop_tier == len(WORKSHOP_COSTS) and workshop_complete_day is None:
                workshop_complete_day = day

        daily_log.append({
            "day": day,
            "essence_earned": day_essence_earned,
            "essence_spent": day_essence_spent,
            "essence_balance": essence,
            "gems_earned": day_gems_earned,
            "gems_balance": gems,
            "workshop_progress": f"{workshop_tier}/{len(WORKSHOP_COSTS)}",
            "potions_collected": potions_collected,
        })

    return {
        "daily_log": daily_log,
        "workshop_complete_day": workshop_complete_day,
        "workshop_tier": workshop_tier,
        "total_gems": gems,
        "total_boosters": total_boosters_used,
        "potions_collected": potions_collected,
    }


def print_daily_report(log: list[dict]) -> None:
    header = (
        f"{'Day':>4}  {'Ess Earn':>9}  {'Ess Spent':>10}  "
        f"{'Ess Bal':>8}  {'Gems +':>7}  {'Gems Bal':>9}  "
        f"{'Workshop':>9}  {'Potions':>8}"
    )
    print(header)
    print("-" * len(header))
    for row in log:
        print(
            f"{row['day']:>4}  {row['essence_earned']:>9,}  {row['essence_spent']:>10,}  "
            f"{row['essence_balance']:>8,}  {row['gems_earned']:>7}  {row['gems_balance']:>9,}  "
            f"{row['workshop_progress']:>9}  {row['potions_collected']:>8}"
        )


def print_ascii_chart(log: list[dict], width: int = 60) -> None:
    """Simple ASCII bar chart of essence balance over time."""
    balances = [r["essence_balance"] for r in log]
    max_bal = max(balances) if balances else 1
    min_bal = min(0, min(balances))
    span = max(max_bal - min_bal, 1)

    print("\n  Essence Balance Over Time")
    print("  " + "-" * (width + 12))

    step = max(1, len(log) // 30)
    for i in range(0, len(log), step):
        row = log[i]
        bar_len = int((row["essence_balance"] - min_bal) / span * width)
        bar = "#" * bar_len
        print(f"  D{row['day']:>3} |{bar} {row['essence_balance']:>,}")

    print("  " + "-" * (width + 12))
    print(f"  Scale: 0 {'-' * (width - 6)} {max_bal:>,}")


def print_summary(result: dict, days: int, is_payer: bool) -> None:
    print("\n" + "=" * 50)
    print("  SIMULATION SUMMARY")
    print("=" * 50)

    player_type = "Payer" if is_payer else "Non-Payer"
    print(f"  Player type:            {player_type}")
    print(f"  Simulation length:      {days} days")

    if result["workshop_complete_day"]:
        print(f"  Workshop completed:     Day {result['workshop_complete_day']}")
    else:
        print(f"  Workshop progress:      {result['workshop_tier']}/{len(WORKSHOP_COSTS)} tiers")
        remaining = sum(WORKSHOP_COSTS[result["workshop_tier"]:])
        print(f"  Essence still needed:   {remaining:,}")

    print(f"  Total gems earned:      {result['total_gems']:,}")
    print(f"  Total boosters used:    {result['total_boosters']}")
    print(f"  Potions collected:      {result['potions_collected']}")

    # Validation check
    print("\n  -- Pacing Validation --")
    if not is_payer:
        wd = result["workshop_complete_day"]
        if wd is None:
            print(f"  [!] Workshop NOT completed in {days} days.")
            print(f"      Target range: {TARGET_WORKSHOP_DAYS_MIN}-{TARGET_WORKSHOP_DAYS_MAX} days.")
        elif wd < TARGET_WORKSHOP_DAYS_MIN:
            print(f"  [!] Workshop completed too FAST (day {wd}).")
            print(f"      Target range: {TARGET_WORKSHOP_DAYS_MIN}-{TARGET_WORKSHOP_DAYS_MAX} days.")
            print("      Consider increasing upgrade costs or reducing earn rates.")
        elif wd > TARGET_WORKSHOP_DAYS_MAX:
            print(f"  [!] Workshop completed too SLOW (day {wd}).")
            print(f"      Target range: {TARGET_WORKSHOP_DAYS_MIN}-{TARGET_WORKSHOP_DAYS_MAX} days.")
            print("      Consider decreasing upgrade costs or increasing earn rates.")
        else:
            print(f"  [OK] Workshop completed on day {wd} -- within target range.")
    else:
        print("  (Pacing validation applies to non-payer path only.)")

    print("=" * 50)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Simulate Brew player economy over time.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("--days", type=int, default=30, help="Number of days to simulate (default: 30)")
    parser.add_argument("--levels-per-day", type=int, default=10, help="Average levels played per day (default: 10)")
    parser.add_argument("--star-average", type=float, default=2.0, help="Average star rating per level (1.0-3.0, default: 2.0)")
    parser.add_argument("--win-rate", type=float, default=0.7, help="Win rate as decimal (default: 0.7)")
    parser.add_argument("--payer", action="store_true", help="Simulate a paying player (receives starter bundle)")
    parser.add_argument("--streak-average", type=int, default=3, help="Average win streak length (default: 3)")
    parser.add_argument("--quiet", action="store_true", help="Only print summary, skip daily report")

    args = parser.parse_args(argv)

    if not (1.0 <= args.star_average <= 3.0):
        print("ERROR: --star-average must be between 1.0 and 3.0", file=sys.stderr)
        return 1
    if not (0.0 < args.win_rate <= 1.0):
        print("ERROR: --win-rate must be between 0.0 and 1.0", file=sys.stderr)
        return 1

    result = simulate(
        days=args.days,
        levels_per_day=args.levels_per_day,
        star_average=args.star_average,
        win_rate=args.win_rate,
        is_payer=args.payer,
        streak_average=args.streak_average,
    )

    if not args.quiet:
        print_daily_report(result["daily_log"])

    print_ascii_chart(result["daily_log"])
    print_summary(result, args.days, args.payer)

    # CI exit code: fail if non-payer workshop completion is outside target range
    if not args.payer:
        wd = result["workshop_complete_day"]
        if wd is None or wd < TARGET_WORKSHOP_DAYS_MIN or wd > TARGET_WORKSHOP_DAYS_MAX:
            return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
