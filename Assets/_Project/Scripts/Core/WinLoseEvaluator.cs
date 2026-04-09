using System;

namespace Brew.Core
{
    /// <summary>
    /// Evaluates whether a level is won, lost, or still in progress.
    /// Called during the CheckWin phase after full resolution completes.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class WinLoseEvaluator
    {
        private readonly RecipeTracker _recipeTracker;
        private readonly MoveTracker _moveTracker;

        public WinLoseEvaluator(RecipeTracker recipeTracker, MoveTracker moveTracker)
        {
            _recipeTracker = recipeTracker ?? throw new ArgumentNullException(nameof(recipeTracker));
            _moveTracker = moveTracker ?? throw new ArgumentNullException(nameof(moveTracker));
        }

        /// <summary>
        /// Evaluates the current level state. Win takes priority over Lose:
        /// a final-move cascade can complete the recipe, which is a win even though
        /// moves are exhausted. Per core-mechanic.md §9.3.
        /// </summary>
        public LevelOutcome Evaluate()
        {
            if (_recipeTracker.IsComplete)
                return LevelOutcome.Win;

            if (_moveTracker.IsExhausted)
                return LevelOutcome.Lose;

            return LevelOutcome.InProgress;
        }
    }
}
