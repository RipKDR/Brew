using System;

namespace Brew.Core
{
    /// <summary>
    /// Implements the scoring formula from core-mechanic.md §13-14.
    /// Tracks chain/cascade multipliers within a resolution, accumulates total score.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class ScoreCalculator
    {
        private double _chainMultiplier = 1.0;
        private double _cascadeMultiplier = 1.0;
        private int _brewCountThisResolution;

        public int TotalScore { get; private set; }

        public event Action<int> OnScoreChanged;

        /// <summary>
        /// Resets per-resolution multipliers. Called at the start of each player move.
        /// Per core-mechanic.md §14.4: both multipliers reset to 1.0 at start of each tap.
        /// </summary>
        public void ResetForNewMove()
        {
            _chainMultiplier = 1.0;
            _cascadeMultiplier = 1.0;
            _brewCountThisResolution = 0;
        }

        /// <summary>
        /// Awards points for a fusion event (player tap or cascade auto-fuse).
        /// Returns the points awarded for this fusion.
        /// Formula: cluster_size * 10 * chain_multiplier * cascade_multiplier
        /// </summary>
        public int OnFusion(int clusterSize)
        {
            int points = (int)(clusterSize * 10 * _chainMultiplier * _cascadeMultiplier);
            TotalScore += points;
            OnScoreChanged?.Invoke(TotalScore);
            return points;
        }

        /// <summary>
        /// Called when a chain-fusion merge step occurs. Bumps the chain multiplier by 0.5.
        /// Per core-mechanic.md §14.1: the multiplier increases AFTER the chain step.
        /// </summary>
        public void OnChainStep()
        {
            _chainMultiplier += 0.5;
        }

        /// <summary>
        /// Called when a cascade wave triggers. Bumps the cascade multiplier by 0.5.
        /// </summary>
        public void OnCascadeWave()
        {
            _cascadeMultiplier += 0.5;
        }

        /// <summary>
        /// Awards flat brew score. 200 for target color, 100 for non-target.
        /// Per core-mechanic.md §13.1: brew score is flat, not multiplied.
        /// Returns the points awarded.
        /// </summary>
        public int OnBrew(bool isTargetColor)
        {
            _brewCountThisResolution++;
            int points = isTargetColor ? 200 : 100;

            if (_brewCountThisResolution > 1)
                points += 100;

            TotalScore += points;
            OnScoreChanged?.Invoke(TotalScore);
            return points;
        }

        /// <summary>
        /// End-of-level bonus for remaining moves (win only).
        /// Per core-mechanic.md §13.2: remaining_moves * 50.
        /// </summary>
        public int CalculateEndOfLevelBonus(int movesRemaining)
        {
            int bonus = movesRemaining * 50;
            TotalScore += bonus;
            OnScoreChanged?.Invoke(TotalScore);
            return bonus;
        }

        /// <summary>
        /// Returns 0-3 stars based on score thresholds.
        /// thresholds[0] = 1-star, thresholds[1] = 2-star, thresholds[2] = 3-star.
        /// </summary>
        public static int CalculateStars(int score, System.Collections.Generic.IReadOnlyList<int> thresholds)
        {
            if (thresholds == null || thresholds.Count < 3)
                return 0;

            if (score >= thresholds[2]) return 3;
            if (score >= thresholds[1]) return 2;
            if (score >= thresholds[0]) return 1;
            return 0;
        }
    }
}
