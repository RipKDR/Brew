using System;
using System.Linq;

namespace Brew.Core.Meta
{
    /// <summary>
    /// Tracks consecutive wins and the active streak multiplier tier. Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class WinStreakTracker
    {
        private readonly (int threshold, float multiplier)[] _tiers;
        private int _streak;

        public event Action<int, float> OnStreakChanged;

        public WinStreakTracker((int threshold, float multiplier)[] tiers)
        {
            _tiers = tiers ?? throw new ArgumentNullException(nameof(tiers));

            if (_tiers.Length == 0)
                throw new ArgumentException("At least one streak tier is required.", nameof(tiers));

            foreach (var (t, m) in _tiers)
            {
                if (t < 0)
                    throw new ArgumentOutOfRangeException(nameof(tiers), t, "Threshold cannot be negative.");

                if (m <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(tiers), m, "Multiplier must be positive.");
            }

            _tiers = _tiers.OrderBy(x => x.threshold).ToArray();
        }

        public int CurrentStreak => _streak;

        public float CurrentMultiplier => ResolveMultiplier(_streak);

        public void IncrementStreak()
        {
            _streak++;
            RaiseChanged();
        }

        public void ResetStreak()
        {
            if (_streak == 0)
                return;

            _streak = 0;
            RaiseChanged();
        }

        public void SetStreak(int streak)
        {
            if (streak < 0)
                throw new ArgumentOutOfRangeException(nameof(streak), streak, "Streak cannot be negative.");

            if (streak == _streak)
                return;

            _streak = streak;
            RaiseChanged();
        }

        private float ResolveMultiplier(int streak)
        {
            float mult = 1f;
            foreach (var (threshold, multiplier) in _tiers)
            {
                if (streak >= threshold)
                    mult = multiplier;
            }

            return mult;
        }

        private void RaiseChanged() => OnStreakChanged?.Invoke(_streak, CurrentMultiplier);
    }
}
