using System;
using Brew.Core;
using Brew.Data;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Data
{
    [TestFixture]
    public class EventLevelLoaderTests
    {
        private const string SimpleEventJson = @"{
            ""level_id"": 1001,
            ""grid_width"": 7,
            ""grid_height"": 9,
            ""grid_mask"": [],
            ""ingredient_pool"": [""ember"", ""frost"", ""vine"", ""themed""],
            ""recipe_targets"": [
                { ""ingredient"": ""ember"", ""count"": 1 },
                { ""ingredient"": ""frost"", ""count"": 1 }
            ],
            ""move_limit"": 20,
            ""blocker_placements"": [],
            ""star_thresholds"": [800, 1600, 2400],
            ""is_tutorial"": false,
            ""tutorial_steps"": []
        }";

        private const string ThemedRecipeJson = @"{
            ""level_id"": 1007,
            ""grid_width"": 7,
            ""grid_height"": 9,
            ""grid_mask"": [],
            ""ingredient_pool"": [""ember"", ""frost"", ""vine"", ""themed""],
            ""recipe_targets"": [
                { ""ingredient"": ""frost"", ""count"": 3 },
                { ""ingredient"": ""vine"", ""count"": 3 },
                { ""ingredient"": ""themed"", ""count"": 2 }
            ],
            ""move_limit"": 32,
            ""blocker_placements"": [
                { ""row"": 1, ""col"": 1, ""type"": ""stone"" }
            ],
            ""star_thresholds"": [2000, 4000, 6000],
            ""is_tutorial"": false,
            ""tutorial_steps"": []
        }";

        [Test]
        public void LoadEventLevel_ResolvesThemedIngredient_InPool()
        {
            var config = LevelLoader.LoadEventLevel(SimpleEventJson, "ember");

            Assert.AreEqual(4, config.IngredientPool.Count);
            Assert.AreEqual(IngredientColor.Ember, config.IngredientPool[3]);
        }

        [Test]
        public void LoadEventLevel_ResolvesThemedIngredient_InRecipeTargets()
        {
            var config = LevelLoader.LoadEventLevel(ThemedRecipeJson, "shadow");

            Assert.AreEqual(3, config.RecipeTargets.Count);
            Assert.AreEqual(IngredientColor.Shadow, config.RecipeTargets[2].Color);
            Assert.AreEqual(2, config.RecipeTargets[2].Count);
        }

        [Test]
        public void LoadEventLevel_FallsBackToShadow_WhenThemedIdIsEmpty()
        {
            var config = LevelLoader.LoadEventLevel(SimpleEventJson, "");

            Assert.AreEqual(IngredientColor.Shadow, config.IngredientPool[3]);
        }

        [Test]
        public void LoadEventLevel_FallsBackToShadow_WhenThemedIdIsNull()
        {
            var config = LevelLoader.LoadEventLevel(SimpleEventJson, null);

            Assert.AreEqual(IngredientColor.Shadow, config.IngredientPool[3]);
        }

        [Test]
        public void LoadEventLevel_PreservesNonThemedIngredients()
        {
            var config = LevelLoader.LoadEventLevel(SimpleEventJson, "sun");

            Assert.AreEqual(IngredientColor.Ember, config.IngredientPool[0]);
            Assert.AreEqual(IngredientColor.Frost, config.IngredientPool[1]);
            Assert.AreEqual(IngredientColor.Vine, config.IngredientPool[2]);
            Assert.AreEqual(IngredientColor.Sun, config.IngredientPool[3]);
        }

        [Test]
        public void LoadEventLevel_PreservesLevelMetadata()
        {
            var config = LevelLoader.LoadEventLevel(SimpleEventJson, "ember");

            Assert.AreEqual(1001, config.LevelId);
            Assert.AreEqual(7, config.GridWidth);
            Assert.AreEqual(9, config.GridHeight);
            Assert.AreEqual(20, config.MoveLimit);
            Assert.AreEqual(800, config.StarThresholds[0]);
            Assert.AreEqual(1600, config.StarThresholds[1]);
            Assert.AreEqual(2400, config.StarThresholds[2]);
            Assert.IsFalse(config.IsTutorial);
        }

        [Test]
        public void LoadEventLevel_InvalidThemedIngredient_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                LevelLoader.LoadEventLevel(SimpleEventJson, "nonexistent_ingredient"));
        }

        [Test]
        public void LoadFromJson_WithThemedInPool_ThrowsWithoutOverload()
        {
            Assert.Throws<ArgumentException>(() =>
                LevelLoader.LoadFromJson(SimpleEventJson));
        }

        [Test]
        public void LoadEventLevel_NullJson_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                LevelLoader.LoadEventLevel(null, "ember"));
        }

        [Test]
        public void LoadEventLevel_EmptyJson_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                LevelLoader.LoadEventLevel("", "ember"));
        }

        [Test]
        public void RemoteConfigDefaults_KeyCount_MatchesJsonFile()
        {
            var defaults = RemoteConfigDefaults.GetAll();

            Assert.AreEqual(68, defaults.Count,
                "RemoteConfigDefaults.GetAll() key count must match remote-config-defaults.json");
        }
    }
}
