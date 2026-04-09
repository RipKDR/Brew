using System;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class LevelConfigTests
    {
        private static LevelConfig MakeValid(
            int levelId = 1,
            int width = 7,
            int height = 9,
            int moveLimit = 25,
            bool isTutorial = false)
        {
            return new LevelConfig(
                levelId,
                width,
                height,
                new[] { IngredientColor.Ember, IngredientColor.Frost, IngredientColor.Vine },
                new[] { new RecipeTarget(IngredientColor.Ember, 2) },
                moveLimit,
                new[] { 1000, 3000, 6000 },
                isTutorial);
        }

        [Test]
        public void Constructor_ValidArgs_SetsAllProperties()
        {
            var config = MakeValid(levelId: 5, width: 8, height: 10, moveLimit: 30, isTutorial: true);
            Assert.AreEqual(5, config.LevelId);
            Assert.AreEqual(8, config.GridWidth);
            Assert.AreEqual(10, config.GridHeight);
            Assert.AreEqual(3, config.IngredientPool.Count);
            Assert.AreEqual(1, config.RecipeTargets.Count);
            Assert.AreEqual(30, config.MoveLimit);
            Assert.AreEqual(3, config.StarThresholds.Count);
            Assert.IsTrue(config.IsTutorial);
        }

        [TestCase(4)]
        [TestCase(10)]
        public void Constructor_InvalidWidth_Throws(int width)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MakeValid(width: width));
        }

        [TestCase(4)]
        [TestCase(12)]
        public void Constructor_InvalidHeight_Throws(int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MakeValid(height: height));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_InvalidMoveLimit_Throws(int limit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MakeValid(moveLimit: limit));
        }

        [Test]
        public void Constructor_NullIngredientPool_Throws()
        {
            Assert.Throws<ArgumentException>(() => new LevelConfig(
                1, 7, 9,
                null,
                new[] { new RecipeTarget(IngredientColor.Ember, 1) },
                25, new[] { 1000, 3000, 6000 }, false));
        }

        [Test]
        public void Constructor_EmptyIngredientPool_Throws()
        {
            Assert.Throws<ArgumentException>(() => new LevelConfig(
                1, 7, 9,
                Array.Empty<IngredientColor>(),
                new[] { new RecipeTarget(IngredientColor.Ember, 1) },
                25, new[] { 1000, 3000, 6000 }, false));
        }

        [Test]
        public void Constructor_NullRecipeTargets_Throws()
        {
            Assert.Throws<ArgumentException>(() => new LevelConfig(
                1, 7, 9,
                new[] { IngredientColor.Ember },
                null,
                25, new[] { 1000, 3000, 6000 }, false));
        }

        [Test]
        public void Constructor_WrongStarThresholdCount_Throws()
        {
            Assert.Throws<ArgumentException>(() => new LevelConfig(
                1, 7, 9,
                new[] { IngredientColor.Ember },
                new[] { new RecipeTarget(IngredientColor.Ember, 1) },
                25, new[] { 1000, 3000 }, false));
        }

        [Test]
        public void RecipeTarget_ValidConstruction()
        {
            var target = new RecipeTarget(IngredientColor.Frost, 3);
            Assert.AreEqual(IngredientColor.Frost, target.Color);
            Assert.AreEqual(3, target.Count);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void RecipeTarget_InvalidCount_Throws(int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RecipeTarget(IngredientColor.Ember, count));
        }
    }
}
