using System;
using System.Collections.Generic;

namespace Brew.Core
{
    public enum TutorialStepType
    {
        ShowText,
        HighlightAndWaitForTap,
        FreePlay,
        LevelComplete
    }

    public sealed class TutorialStep
    {
        public TutorialStepType Type { get; }
        public string Text { get; }
        public GridCoord[] HighlightedCells { get; }
        public GridCoord? FingerIndicatorCell { get; }
        public float AutoDismissTime { get; }
        public bool CelebrationOnComplete { get; }
        public string CelebrationText { get; }

        public TutorialStep(
            TutorialStepType type,
            string text,
            GridCoord[] highlightedCells = null,
            GridCoord? fingerIndicatorCell = null,
            float autoDismissTime = 0f,
            bool celebrationOnComplete = false,
            string celebrationText = null)
        {
            Type = type;
            Text = text;
            HighlightedCells = highlightedCells ?? Array.Empty<GridCoord>();
            FingerIndicatorCell = fingerIndicatorCell;
            AutoDismissTime = autoDismissTime;
            CelebrationOnComplete = celebrationOnComplete;
            CelebrationText = celebrationText;
        }
    }

    public sealed class TutorialBoardSetup
    {
        public int Width { get; }
        public int Height { get; }
        public IngredientColor[,] Layout { get; }

        public TutorialBoardSetup(int width, int height, IngredientColor[,] layout)
        {
            Width = width;
            Height = height;
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        }
    }

    public sealed class TutorialLevelDefinition
    {
        public int LevelId { get; }
        public TutorialBoardSetup BoardSetup { get; }
        public IReadOnlyList<RecipeTarget> Targets { get; }
        public int MoveLimit { get; }
        public bool HideMoveCounter { get; }
        public IReadOnlyList<TutorialStep> Steps { get; }

        public TutorialLevelDefinition(
            int levelId,
            TutorialBoardSetup boardSetup,
            IReadOnlyList<RecipeTarget> targets,
            int moveLimit,
            bool hideMoveCounter,
            IReadOnlyList<TutorialStep> steps)
        {
            LevelId = levelId;
            BoardSetup = boardSetup;
            Targets = targets;
            MoveLimit = moveLimit;
            HideMoveCounter = hideMoveCounter;
            Steps = steps;
        }
    }

    public static class TutorialLevelData
    {
        private static readonly IngredientColor E = IngredientColor.Ember;
        private static readonly IngredientColor F = IngredientColor.Frost;
        private static readonly IngredientColor V = IngredientColor.Vine;

        public static TutorialLevelDefinition GetLevel(int levelId)
        {
            return levelId switch
            {
                1 => BuildLevel1(),
                2 => BuildLevel2(),
                3 => BuildLevel3(),
                4 => BuildLevel4(),
                5 => BuildLevel5(),
                _ => null
            };
        }

        public static bool IsTutorialLevel(int levelId) => levelId >= 1 && levelId <= 5;

        private static TutorialLevelDefinition BuildLevel1()
        {
            var layout = new IngredientColor[3, 3]
            {
                { E, E, F },
                { E, F, F },
                { F, F, E }
            };

            var steps = new List<TutorialStep>
            {
                new(TutorialStepType.ShowText,
                    "These are ingredients. Let's brew something!",
                    autoDismissTime: 2f),

                new(TutorialStepType.HighlightAndWaitForTap,
                    "Tap these red ingredients to fuse them!",
                    highlightedCells: new[] { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(0, 1) },
                    fingerIndicatorCell: new GridCoord(0, 0)),

                new(TutorialStepType.ShowText,
                    "You made a Brew Orb!",
                    celebrationOnComplete: true,
                    celebrationText: "Nice!",
                    autoDismissTime: 1.5f),

                new(TutorialStepType.HighlightAndWaitForTap,
                    "Fuse more red to grow the orb!",
                    highlightedCells: Array.Empty<GridCoord>(),
                    fingerIndicatorCell: null),

                new(TutorialStepType.ShowText,
                    "You brewed a potion!",
                    celebrationOnComplete: true,
                    celebrationText: "Great brew!",
                    autoDismissTime: 2f),

                new(TutorialStepType.LevelComplete,
                    "You're a natural brewer!")
            };

            return new TutorialLevelDefinition(
                1,
                new TutorialBoardSetup(3, 3, layout),
                new[] { new RecipeTarget(E, 1) },
                moveLimit: 15,
                hideMoveCounter: true,
                steps);
        }

        private static TutorialLevelDefinition BuildLevel2()
        {
            var layout = new IngredientColor[4, 4]
            {
                { F, E, E, F },
                { F, F, E, E },
                { E, F, F, E },
                { E, E, F, F }
            };

            var steps = new List<TutorialStep>
            {
                new(TutorialStepType.ShowText,
                    "You have 12 moves. Use them wisely!",
                    autoDismissTime: 2f),

                new(TutorialStepType.HighlightAndWaitForTap,
                    "Two groups of blue \u2014 which will you fuse first?",
                    highlightedCells: new[]
                    {
                        new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(1, 1),
                        new GridCoord(2, 1), new GridCoord(2, 2), new GridCoord(3, 2), new GridCoord(3, 3)
                    }),

                new(TutorialStepType.FreePlay,
                    "Keep fusing blue to fill the vial!"),

                new(TutorialStepType.LevelComplete,
                    "More moves left over = more stars!")
            };

            return new TutorialLevelDefinition(
                2,
                new TutorialBoardSetup(4, 4, layout),
                new[] { new RecipeTarget(F, 1) },
                moveLimit: 12,
                hideMoveCounter: false,
                steps);
        }

        private static TutorialLevelDefinition BuildLevel3()
        {
            var layout = new IngredientColor[4, 5]
            {
                { V, E, F, F },
                { V, V, E, F },
                { E, F, V, V },
                { F, E, V, E },
                { E, F, E, F }
            };

            var steps = new List<TutorialStep>
            {
                new(TutorialStepType.ShowText,
                    "A new ingredient \u2014 Vine!",
                    autoDismissTime: 1.5f),

                new(TutorialStepType.HighlightAndWaitForTap,
                    "Fuse the green ingredients.",
                    highlightedCells: new[] { new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(1, 1) },
                    fingerIndicatorCell: new GridCoord(0, 0)),

                new(TutorialStepType.ShowText,
                    "Look \u2014 the orb is next to more green!",
                    autoDismissTime: 1.5f),

                new(TutorialStepType.HighlightAndWaitForTap,
                    "Fuse them to chain into the orb!",
                    highlightedCells: new[] { new GridCoord(2, 2), new GridCoord(2, 3), new GridCoord(3, 2) },
                    fingerIndicatorCell: new GridCoord(2, 2)),

                new(TutorialStepType.ShowText,
                    "Chain fusion! That's how the pros brew.",
                    celebrationOnComplete: true,
                    celebrationText: "Amazing!",
                    autoDismissTime: 2f),

                new(TutorialStepType.LevelComplete,
                    "Chains are your secret weapon!")
            };

            return new TutorialLevelDefinition(
                3,
                new TutorialBoardSetup(4, 5, layout),
                new[] { new RecipeTarget(V, 1) },
                moveLimit: 14,
                hideMoveCounter: false,
                steps);
        }

        private static TutorialLevelDefinition BuildLevel4()
        {
            var layout = new IngredientColor[5, 5]
            {
                { F, V, E, V, F },
                { V, F, V, E, V },
                { E, V, F, F, E },
                { F, E, V, E, F },
                { V, F, E, F, V }
            };

            var steps = new List<TutorialStep>
            {
                new(TutorialStepType.ShowText,
                    "Your goal \u2014 brew a Red potion!",
                    autoDismissTime: 2f),

                new(TutorialStepType.ShowText,
                    "Red is scattered. Clear other colors to bring them together.",
                    autoDismissTime: 2f),

                new(TutorialStepType.FreePlay,
                    "Smart \u2014 clearing space for red!"),

                new(TutorialStepType.LevelComplete,
                    "Sometimes the best move isn't the obvious one.")
            };

            return new TutorialLevelDefinition(
                4,
                new TutorialBoardSetup(5, 5, layout),
                new[] { new RecipeTarget(E, 1) },
                moveLimit: 12,
                hideMoveCounter: false,
                steps);
        }

        private static TutorialLevelDefinition BuildLevel5()
        {
            var layout = new IngredientColor[5, 6]
            {
                { E, V, F, F, V },
                { E, E, V, F, E },
                { V, F, E, V, F },
                { F, F, V, E, E },
                { V, E, F, F, V },
                { E, V, E, F, F }
            };

            var steps = new List<TutorialStep>
            {
                new(TutorialStepType.ShowText,
                    "Two potions to brew this time. You've got this!",
                    autoDismissTime: 2f),

                new(TutorialStepType.FreePlay,
                    null),

                new(TutorialStepType.LevelComplete,
                    "You're ready to brew on your own!")
            };

            return new TutorialLevelDefinition(
                5,
                new TutorialBoardSetup(5, 6, layout),
                new[] { new RecipeTarget(E, 1), new RecipeTarget(F, 1) },
                moveLimit: 14,
                hideMoveCounter: false,
                steps);
        }
    }
}
