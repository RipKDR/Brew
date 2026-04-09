using System;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class TutorialLevelDataTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void GetLevel_ReturnsNonNull_ForLevelsOneThroughFive(int levelId)
        {
            Assert.IsNotNull(TutorialLevelData.GetLevel(levelId));
        }

        [TestCase(0)]
        [TestCase(6)]
        [TestCase(-1)]
        public void GetLevel_ReturnsNull_ForOutOfRange(int levelId)
        {
            Assert.IsNull(TutorialLevelData.GetLevel(levelId));
        }

        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(0, false)]
        [TestCase(6, false)]
        public void IsTutorialLevel_ReturnsExpected(int levelId, bool expected)
        {
#pragma warning disable CS0618
            Assert.AreEqual(expected, TutorialLevelData.IsTutorialLevel(levelId));
#pragma warning restore CS0618
        }

        [Test]
        public void Level1_HasExpectedBoardTargetsMovesHideCounterAndStepCount()
        {
            var level = TutorialLevelData.GetLevel(1);
            Assert.IsNotNull(level);
            Assert.AreEqual(5, level.BoardSetup.Width);
            Assert.AreEqual(5, level.BoardSetup.Height);
            Assert.AreEqual(1, level.Targets.Count);
            Assert.AreEqual(IngredientColor.Ember, level.Targets[0].Color);
            Assert.AreEqual(1, level.Targets[0].Count);
            Assert.AreEqual(15, level.MoveLimit);
            Assert.IsTrue(level.HideMoveCounter);
            Assert.AreEqual(6, level.Steps.Count);
        }

        [Test]
        public void Level5_HasTwoRecipeTargets()
        {
            var level = TutorialLevelData.GetLevel(5);
            Assert.IsNotNull(level);
            Assert.AreEqual(2, level.Targets.Count);
            Assert.AreEqual(IngredientColor.Ember, level.Targets[0].Color);
            Assert.AreEqual(1, level.Targets[0].Count);
            Assert.AreEqual(IngredientColor.Frost, level.Targets[1].Color);
            Assert.AreEqual(1, level.Targets[1].Count);
        }

        [Test]
        public void Level5_BoardIsFiveBySix()
        {
            var level = TutorialLevelData.GetLevel(5);
            Assert.IsNotNull(level);
            Assert.AreEqual(5, level.BoardSetup.Width);
            Assert.AreEqual(6, level.BoardSetup.Height);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EachLevel_HasAtLeastOneStep(int levelId)
        {
            var level = TutorialLevelData.GetLevel(levelId);
            Assert.IsNotNull(level);
            Assert.IsTrue(level.Steps.Count >= 1);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EachLevel_LastStep_IsLevelComplete(int levelId)
        {
            var level = TutorialLevelData.GetLevel(levelId);
            Assert.IsNotNull(level);
            var last = level.Steps[level.Steps.Count - 1];
            Assert.AreEqual(TutorialStepType.LevelComplete, last.Type);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void EachLevel_BoardLayoutDimensions_MatchWidthAndHeight(int levelId)
        {
            var level = TutorialLevelData.GetLevel(levelId);
            Assert.IsNotNull(level);
            var setup = level.BoardSetup;
            Assert.AreEqual(setup.Height, setup.Layout.GetLength(0));
            Assert.AreEqual(setup.Width, setup.Layout.GetLength(1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void GetLevel_LevelIdMatchesRequestedId(int levelId)
        {
            var level = TutorialLevelData.GetLevel(levelId);
            Assert.IsNotNull(level);
            Assert.AreEqual(levelId, level.LevelId);
        }

        [Test]
        public void Level2_HasExpectedConfiguration()
        {
            var level = TutorialLevelData.GetLevel(2);
            Assert.IsNotNull(level);
            Assert.AreEqual(5, level.BoardSetup.Width);
            Assert.AreEqual(5, level.BoardSetup.Height);
            Assert.AreEqual(1, level.Targets.Count);
            Assert.AreEqual(IngredientColor.Frost, level.Targets[0].Color);
            Assert.AreEqual(1, level.Targets[0].Count);
            Assert.AreEqual(12, level.MoveLimit);
            Assert.IsFalse(level.HideMoveCounter);
            Assert.AreEqual(4, level.Steps.Count);
        }

        [Test]
        public void Level3_HasExpectedConfiguration()
        {
            var level = TutorialLevelData.GetLevel(3);
            Assert.IsNotNull(level);
            Assert.AreEqual(5, level.BoardSetup.Width);
            Assert.AreEqual(5, level.BoardSetup.Height);
            Assert.AreEqual(1, level.Targets.Count);
            Assert.AreEqual(IngredientColor.Vine, level.Targets[0].Color);
            Assert.AreEqual(1, level.Targets[0].Count);
            Assert.AreEqual(14, level.MoveLimit);
            Assert.AreEqual(6, level.Steps.Count);
        }

        [Test]
        public void Level4_HasExpectedConfiguration()
        {
            var level = TutorialLevelData.GetLevel(4);
            Assert.IsNotNull(level);
            Assert.AreEqual(5, level.BoardSetup.Width);
            Assert.AreEqual(5, level.BoardSetup.Height);
            Assert.AreEqual(1, level.Targets.Count);
            Assert.AreEqual(IngredientColor.Ember, level.Targets[0].Color);
            Assert.AreEqual(1, level.Targets[0].Count);
            Assert.AreEqual(12, level.MoveLimit);
            Assert.AreEqual(4, level.Steps.Count);
        }

        [Test]
        public void Level5_HasExpectedMoveLimitAndStepCount()
        {
            var level = TutorialLevelData.GetLevel(5);
            Assert.IsNotNull(level);
            Assert.AreEqual(14, level.MoveLimit);
            Assert.AreEqual(3, level.Steps.Count);
        }

        [Test]
        public void TutorialBoardSetup_NullLayout_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TutorialBoardSetup(1, 1, null));
        }

        [Test]
        public void TutorialBoardSetup_LayoutDimensionsMismatch_ThrowsArgumentException()
        {
            var layout = new IngredientColor[2, 3];
            Assert.Throws<ArgumentException>(() => new TutorialBoardSetup(5, 5, layout));
        }

        [Test]
        public void TutorialStep_DefaultHighlightCells_IsEmptyNotNull()
        {
            var step = new TutorialStep(TutorialStepType.ShowText, "x");
            Assert.IsNotNull(step.HighlightedCells);
            Assert.AreEqual(0, step.HighlightedCells.Length);
        }
    }
}
