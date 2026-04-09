using System;
using System.Linq;

namespace Brew.Core.Meta
{
    /// <summary>
    /// Daily Brew completion and UTC calendar streak tracking. Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class DailyBrewManager
    {
        private readonly int _baseEssence;
        private readonly int _baseGems;
        private readonly DailyStreakBonusDefinition[] _streakBonuses;
        private readonly Func<DateTime> _utcNow;

        private DateTime _lastCompletionDate = DateTime.MinValue;
        private int _dailyStreak;

        public event Action<int, int> OnDailyBrewCompleted;
        public event Action<int> OnDailyStreakChanged;

        public DailyBrewManager(
            int baseEssence,
            int baseGems,
            DailyStreakBonusDefinition[] streakBonuses,
            Func<DateTime> utcNow = null)
        {
            if (baseEssence < 0)
                throw new ArgumentOutOfRangeException(nameof(baseEssence));

            if (baseGems < 0)
                throw new ArgumentOutOfRangeException(nameof(baseGems));

            _baseEssence = baseEssence;
            _baseGems = baseGems;
            _streakBonuses = streakBonuses ?? throw new ArgumentNullException(nameof(streakBonuses));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);

            foreach (var b in _streakBonuses)
            {
                if (b.RequiredStreak <= 0)
                    throw new ArgumentOutOfRangeException(nameof(streakBonuses), b.RequiredStreak, "Required streak must be positive.");

                if (b.EssenceMultiplier <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(streakBonuses), b.EssenceMultiplier, "Essence multiplier must be positive.");

                if (b.BonusGems < 0)
                    throw new ArgumentOutOfRangeException(nameof(streakBonuses), b.BonusGems, "Bonus gems cannot be negative.");
            }
        }

        public bool IsCompletedToday
        {
            get
            {
                var today = _utcNow().Date;
                return _lastCompletionDate == today;
            }
        }

        public int CurrentDailyStreak => _dailyStreak;

        public DateTime NextResetUtc => _utcNow().Date.AddDays(1);

        /// <summary>
        /// If the UTC calendar has advanced past the last completion day, clears same-day completion semantics.
        /// If one or more full UTC days were skipped since last completion, resets the daily streak.
        /// </summary>
        public void CheckNewDay(DateTime utcNow)
        {
            var today = utcNow.Date;

            if (_lastCompletionDate == DateTime.MinValue)
                return;

            if (_lastCompletionDate >= today)
                return;

            var daysSince = (today - _lastCompletionDate).Days;
            if (daysSince > 1 && _dailyStreak != 0)
            {
                _dailyStreak = 0;
                OnDailyStreakChanged?.Invoke(_dailyStreak);
            }
        }

        /// <summary>
        /// Completes the daily brew for the current UTC day if not already done. Returns rewards or (0,0).
        /// </summary>
        public (int essence, int gems) TryComplete(DateTime utcNow)
        {
            CheckNewDay(utcNow);

            var today = utcNow.Date;
            if (_lastCompletionDate == today)
                return (0, 0);

            int newStreak;
            if (_lastCompletionDate == DateTime.MinValue)
            {
                newStreak = 1;
            }
            else
            {
                var gapDays = (today - _lastCompletionDate).Days;
                newStreak = gapDays == 1 ? _dailyStreak + 1 : 1;
            }

            var (essenceMult, bonusGems) = ResolveStreakBonuses(newStreak);
            int essence = (int)Math.Round(_baseEssence * essenceMult, MidpointRounding.AwayFromZero);
            int gems = _baseGems + bonusGems;

            _lastCompletionDate = today;

            if (newStreak != _dailyStreak)
            {
                _dailyStreak = newStreak;
                OnDailyStreakChanged?.Invoke(_dailyStreak);
            }

            OnDailyBrewCompleted?.Invoke(essence, gems);
            return (essence, gems);
        }

        public void LoadState(DateTime lastCompletionDate, int dailyStreak)
        {
            if (dailyStreak < 0)
                throw new ArgumentOutOfRangeException(nameof(dailyStreak));

            _lastCompletionDate = lastCompletionDate == DateTime.MinValue ? DateTime.MinValue : lastCompletionDate.Date;
            _dailyStreak = dailyStreak;
        }

        private (float essenceMultiplier, int bonusGems) ResolveStreakBonuses(int streakAfterComplete)
        {
            float mult = 1f;
            int bonus = 0;

            foreach (var b in _streakBonuses.OrderByDescending(x => x.RequiredStreak))
            {
                if (streakAfterComplete < b.RequiredStreak)
                    continue;

                mult = b.EssenceMultiplier;
                bonus = b.BonusGems;
                break;
            }

            return (mult, bonus);
        }
    }

    public readonly struct DailyStreakBonusDefinition : IEquatable<DailyStreakBonusDefinition>
    {
        public int RequiredStreak { get; }
        public float EssenceMultiplier { get; }
        public int BonusGems { get; }

        public DailyStreakBonusDefinition(int requiredStreak, float essenceMultiplier, int bonusGems)
        {
            RequiredStreak = requiredStreak;
            EssenceMultiplier = essenceMultiplier;
            BonusGems = bonusGems;
        }

        public bool Equals(DailyStreakBonusDefinition other) =>
            RequiredStreak == other.RequiredStreak &&
            EssenceMultiplier.Equals(other.EssenceMultiplier) &&
            BonusGems == other.BonusGems;

        public override bool Equals(object obj) => obj is DailyStreakBonusDefinition other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(RequiredStreak, EssenceMultiplier, BonusGems);
    }
}
