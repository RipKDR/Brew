using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Tracks remaining brew counts for each recipe target in a level.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public readonly struct RecipeProgress
    {
        public IngredientColor Color { get; }
        public int Required { get; }
        public int Remaining { get; }
        public bool IsFilled => Remaining <= 0;

        public RecipeProgress(IngredientColor color, int required, int remaining)
        {
            Color = color;
            Required = required;
            Remaining = remaining;
        }
    }

    /// <summary>
    /// Monitors brew events against recipe targets. Tracks which recipe vials
    /// are filled and when the recipe is fully complete.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class RecipeTracker
    {
        private readonly int[] _remaining;
        private readonly IngredientColor[] _colors;
        private readonly int[] _required;

        public event Action<IngredientColor, int> OnRecipeProgressChanged;
        public event Action OnRecipeComplete;

        public RecipeTracker(IReadOnlyList<RecipeTarget> targets)
        {
            if (targets == null || targets.Count == 0)
                throw new ArgumentException("At least one recipe target is required.", nameof(targets));

            _remaining = new int[targets.Count];
            _colors = new IngredientColor[targets.Count];
            _required = new int[targets.Count];

            for (int i = 0; i < targets.Count; i++)
            {
                _colors[i] = targets[i].Color;
                _required[i] = targets[i].Count;
                _remaining[i] = targets[i].Count;
            }
        }

        public bool IsComplete
        {
            get
            {
                for (int i = 0; i < _remaining.Length; i++)
                    if (_remaining[i] > 0) return false;
                return true;
            }
        }

        /// <summary>
        /// Returns true if the given color matches any unfilled recipe target.
        /// Used by ScoreCalculator to determine 200 vs 100 brew score.
        /// </summary>
        public bool IsTargetColor(IngredientColor color)
        {
            for (int i = 0; i < _colors.Length; i++)
                if (_colors[i] == color && _remaining[i] > 0) return true;
            return false;
        }

        /// <summary>
        /// Registers a brew of the given color. Decrements the first matching
        /// unfilled recipe target. Per core-mechanic.md §6.2.
        /// </summary>
        public void OnBrew(IngredientColor color)
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                if (_colors[i] == color && _remaining[i] > 0)
                {
                    _remaining[i]--;
                    OnRecipeProgressChanged?.Invoke(color, _remaining[i]);

                    if (IsComplete)
                        OnRecipeComplete?.Invoke();

                    return;
                }
            }
        }

        public IReadOnlyList<RecipeProgress> CurrentProgress
        {
            get
            {
                var progress = new RecipeProgress[_remaining.Length];
                for (int i = 0; i < _remaining.Length; i++)
                    progress[i] = new RecipeProgress(_colors[i], _required[i], _remaining[i]);
                return progress;
            }
        }
    }
}
