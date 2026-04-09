using System;

namespace Brew.Core.Economy
{
    /// <summary>
    /// Calculates Essence rewards for level completion based on star rating and win streak.
    /// All values are injected via constructor — nothing hardcoded.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class RewardCalculator
    {
        private readonly int[] _essencePerStar;
        private readonly (int threshold, float multiplier)[] _streakTiers;

        /// <param name="essencePerStar">Essence for star indices 0=fail, 1=one-star, 2=two-star, 3=three-star.</param>
        /// <param name="streakTiers">Ascending by threshold. Each entry: (minStreak, multiplier).</param>
        public RewardCalculator(int[] essencePerStar, (int threshold, float multiplier)[] streakTiers)
        {
            _essencePerStar = essencePerStar ?? throw new ArgumentNullException(nameof(essencePerStar));
            _streakTiers = streakTiers ?? throw new ArgumentNullException(nameof(streakTiers));
        }

        public int CalculateEssence(int starRating, int currentStreak)
        {
            if (starRating < 0 || starRating >= _essencePerStar.Length)
                return 0;

            int baseEssence = _essencePerStar[starRating];
            float multiplier = GetStreakMultiplier(currentStreak);
            return (int)Math.Round(baseEssence * multiplier);
        }

        public float GetStreakMultiplier(int streak)
        {
            float multiplier = 1.0f;
            for (int i = _streakTiers.Length - 1; i >= 0; i--)
            {
                if (streak >= _streakTiers[i].threshold)
                {
                    multiplier = _streakTiers[i].multiplier;
                    break;
                }
            }
            return multiplier;
        }
    }
}
