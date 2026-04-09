using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Immutable recipe target: brew a specific ingredient color N times to fill a vial.
    /// </summary>
    public readonly struct RecipeTarget
    {
        public IngredientColor Color { get; }
        public int Count { get; }

        public RecipeTarget(IngredientColor color, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Recipe target count must be positive.");
            Color = color;
            Count = count;
        }
    }

    /// <summary>
    /// Immutable level configuration parsed from JSON or ScriptableObject.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class LevelConfig
    {
        public int LevelId { get; }
        public int GridWidth { get; }
        public int GridHeight { get; }
        public IReadOnlyList<IngredientColor> IngredientPool { get; }
        public IReadOnlyList<RecipeTarget> RecipeTargets { get; }
        public int MoveLimit { get; }
        public IReadOnlyList<int> StarThresholds { get; }
        public bool IsTutorial { get; }

        public LevelConfig(
            int levelId,
            int gridWidth,
            int gridHeight,
            IReadOnlyList<IngredientColor> ingredientPool,
            IReadOnlyList<RecipeTarget> recipeTargets,
            int moveLimit,
            IReadOnlyList<int> starThresholds,
            bool isTutorial)
        {
            if (gridWidth < 5 || gridWidth > 9)
                throw new ArgumentOutOfRangeException(nameof(gridWidth), gridWidth, "Grid width must be 5-9.");
            if (gridHeight < 5 || gridHeight > 11)
                throw new ArgumentOutOfRangeException(nameof(gridHeight), gridHeight, "Grid height must be 5-11.");
            if (ingredientPool == null || ingredientPool.Count == 0)
                throw new ArgumentException("Ingredient pool must contain at least one color.", nameof(ingredientPool));
            if (recipeTargets == null || recipeTargets.Count == 0)
                throw new ArgumentException("At least one recipe target is required.", nameof(recipeTargets));
            if (moveLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(moveLimit), moveLimit, "Move limit must be positive.");
            if (starThresholds == null || starThresholds.Count != 3)
                throw new ArgumentException("Exactly 3 star thresholds are required.", nameof(starThresholds));

            LevelId = levelId;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            IngredientPool = ingredientPool;
            RecipeTargets = recipeTargets;
            MoveLimit = moveLimit;
            StarThresholds = starThresholds;
            IsTutorial = isTutorial;
        }
    }

    public enum LevelOutcome
    {
        InProgress,
        Win,
        Lose
    }
}
