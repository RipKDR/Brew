using System;
using Brew.Core;
using Brew.Data;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Data
{
    [TestFixture]
    public class LevelLoaderTests
    {
        private const string ValidLevelJson = @"{
            ""level_id"": 1,
            ""grid_width"": 7,
            ""grid_height"": 9,
            ""ingredient_pool"": [""ember"", ""frost"", ""vine""],
            ""recipe_targets"": [{""ingredient"": ""ember"", ""count"": 3}],
            ""move_limit"": 25,
            ""star_thresholds"": [1000, 3000, 6000],
            ""is_tutorial"": true
        }";

        [Test]
        public void LoadFromJson_ValidJson_ReturnsLevelConfig()
        {
            var config = LevelLoader.LoadFromJson(ValidLevelJson);

            Assert.AreEqual(1, config.LevelId);
            Assert.AreEqual(7, config.GridWidth);
            Assert.AreEqual(9, config.GridHeight);
            Assert.AreEqual(3, config.IngredientPool.Count);
            Assert.AreEqual(IngredientColor.Ember, config.IngredientPool[0]);
            Assert.AreEqual(IngredientColor.Frost, config.IngredientPool[1]);
            Assert.AreEqual(IngredientColor.Vine, config.IngredientPool[2]);
            Assert.AreEqual(1, config.RecipeTargets.Count);
            Assert.AreEqual(IngredientColor.Ember, config.RecipeTargets[0].Color);
            Assert.AreEqual(3, config.RecipeTargets[0].Count);
            Assert.AreEqual(25, config.MoveLimit);
            Assert.AreEqual(1000, config.StarThresholds[0]);
            Assert.AreEqual(3000, config.StarThresholds[1]);
            Assert.AreEqual(6000, config.StarThresholds[2]);
            Assert.IsTrue(config.IsTutorial);
        }

        [Test]
        public void LoadFromJson_MultipleRecipeTargets_ParsesAll()
        {
            string json = @"{
                ""level_id"": 10,
                ""grid_width"": 7,
                ""grid_height"": 9,
                ""ingredient_pool"": [""ember"", ""frost"", ""vine""],
                ""recipe_targets"": [
                    {""ingredient"": ""frost"", ""count"": 1},
                    {""ingredient"": ""vine"", ""count"": 2}
                ],
                ""move_limit"": 33,
                ""star_thresholds"": [860, 1720, 2752],
                ""is_tutorial"": false
            }";

            var config = LevelLoader.LoadFromJson(json);
            Assert.AreEqual(2, config.RecipeTargets.Count);
            Assert.AreEqual(IngredientColor.Frost, config.RecipeTargets[0].Color);
            Assert.AreEqual(1, config.RecipeTargets[0].Count);
            Assert.AreEqual(IngredientColor.Vine, config.RecipeTargets[1].Color);
            Assert.AreEqual(2, config.RecipeTargets[1].Count);
        }

        [Test]
        public void LoadFromJson_AlternativeColorNames_MapsCorrectly()
        {
            string json = @"{
                ""level_id"": 30,
                ""grid_width"": 8,
                ""grid_height"": 10,
                ""ingredient_pool"": [""ember"", ""frost"", ""vine"", ""brine"", ""glow""],
                ""recipe_targets"": [
                    {""ingredient"": ""brine"", ""count"": 1},
                    {""ingredient"": ""glow"", ""count"": 2}
                ],
                ""move_limit"": 58,
                ""star_thresholds"": [1760, 3520, 5632],
                ""is_tutorial"": false
            }";

            var config = LevelLoader.LoadFromJson(json);
            Assert.AreEqual(5, config.IngredientPool.Count);
            Assert.AreEqual(IngredientColor.Sun, config.IngredientPool[3]);
            Assert.AreEqual(IngredientColor.Shadow, config.IngredientPool[4]);
            Assert.AreEqual(IngredientColor.Sun, config.RecipeTargets[0].Color);
            Assert.AreEqual(IngredientColor.Shadow, config.RecipeTargets[1].Color);
        }

        [Test]
        public void LoadFromJson_NullJson_Throws()
        {
            Assert.Throws<ArgumentException>(() => LevelLoader.LoadFromJson(null));
        }

        [Test]
        public void LoadFromJson_EmptyJson_Throws()
        {
            Assert.Throws<ArgumentException>(() => LevelLoader.LoadFromJson(""));
        }

        [Test]
        public void ParseColor_AllCanonicalNames()
        {
            Assert.AreEqual(IngredientColor.Ember, LevelLoader.ParseColor("ember"));
            Assert.AreEqual(IngredientColor.Frost, LevelLoader.ParseColor("frost"));
            Assert.AreEqual(IngredientColor.Vine, LevelLoader.ParseColor("vine"));
            Assert.AreEqual(IngredientColor.Sun, LevelLoader.ParseColor("sun"));
            Assert.AreEqual(IngredientColor.Shadow, LevelLoader.ParseColor("shadow"));
        }

        [Test]
        public void ParseColor_AlternativeNames()
        {
            Assert.AreEqual(IngredientColor.Sun, LevelLoader.ParseColor("brine"));
            Assert.AreEqual(IngredientColor.Shadow, LevelLoader.ParseColor("glow"));
        }

        [Test]
        public void ParseColor_Unknown_Throws()
        {
            Assert.Throws<ArgumentException>(() => LevelLoader.ParseColor("unknown"));
        }

        [Test]
        public void ParseColor_CaseInsensitive()
        {
            Assert.AreEqual(IngredientColor.Ember, LevelLoader.ParseColor("Ember"));
            Assert.AreEqual(IngredientColor.Ember, LevelLoader.ParseColor("EMBER"));
        }

        [Test]
        public void LoadFromJson_LargerBoard_8x10()
        {
            string json = @"{
                ""level_id"": 25,
                ""grid_width"": 8,
                ""grid_height"": 10,
                ""ingredient_pool"": [""ember"", ""frost"", ""vine"", ""brine""],
                ""recipe_targets"": [{""ingredient"": ""ember"", ""count"": 2}],
                ""move_limit"": 40,
                ""star_thresholds"": [1500, 3500, 7000],
                ""is_tutorial"": false
            }";

            var config = LevelLoader.LoadFromJson(json);
            Assert.AreEqual(8, config.GridWidth);
            Assert.AreEqual(10, config.GridHeight);
        }
    }

    [TestFixture]
    public class PlayerProgressTests
    {
        [Test]
        public void NewProgress_DefaultValues()
        {
            var progress = new PlayerProgress();
            Assert.AreEqual(1, progress.SaveVersion);
            Assert.AreEqual(1, progress.CurrentLevel);
            Assert.AreEqual(0, progress.TotalLevelsCompleted);
            Assert.IsNotNull(progress.LevelStars);
            Assert.AreEqual(0, progress.LevelStars.Count);
        }

        [Test]
        public void GetStars_NoEntry_ReturnsZero()
        {
            var progress = new PlayerProgress();
            Assert.AreEqual(0, progress.GetStars(1));
        }

        [Test]
        public void SetStars_RecordsEntry()
        {
            var progress = new PlayerProgress();
            progress.SetStars(5, 2);
            Assert.AreEqual(2, progress.GetStars(5));
        }

        [Test]
        public void SetStars_OnlyKeepsBest()
        {
            var progress = new PlayerProgress();
            progress.SetStars(5, 3);
            progress.SetStars(5, 1);
            Assert.AreEqual(3, progress.GetStars(5));
        }

        [Test]
        public void SetStars_UpgradesBest()
        {
            var progress = new PlayerProgress();
            progress.SetStars(5, 1);
            progress.SetStars(5, 3);
            Assert.AreEqual(3, progress.GetStars(5));
        }

        [Test]
        public void RecordLevelComplete_AdvancesLevel()
        {
            var progress = new PlayerProgress();
            progress.RecordLevelComplete(1, 2);
            Assert.AreEqual(2, progress.CurrentLevel);
            Assert.AreEqual(1, progress.TotalLevelsCompleted);
            Assert.AreEqual(2, progress.GetStars(1));
        }

        [Test]
        public void RecordLevelComplete_MultipleLevels()
        {
            var progress = new PlayerProgress();
            progress.RecordLevelComplete(1, 3);
            progress.RecordLevelComplete(2, 2);
            progress.RecordLevelComplete(3, 1);
            Assert.AreEqual(4, progress.CurrentLevel);
            Assert.AreEqual(3, progress.TotalLevelsCompleted);
        }

        [Test]
        public void RecordLevelComplete_ReplayDoesNotRegress()
        {
            var progress = new PlayerProgress();
            progress.RecordLevelComplete(1, 1);
            progress.RecordLevelComplete(2, 2);

            progress.RecordLevelComplete(1, 3);
            Assert.AreEqual(3, progress.CurrentLevel);
            Assert.AreEqual(3, progress.GetStars(1));
        }

        [Test]
        public void IsLevelUnlocked_LevelOneAlwaysUnlocked()
        {
            var progress = new PlayerProgress();
            Assert.IsTrue(progress.IsLevelUnlocked(1));
        }

        [Test]
        public void IsLevelUnlocked_FutureLevelLocked()
        {
            var progress = new PlayerProgress();
            Assert.IsFalse(progress.IsLevelUnlocked(2));
        }

        [Test]
        public void IsLevelUnlocked_CompletedLevelUnlocked()
        {
            var progress = new PlayerProgress();
            progress.RecordLevelComplete(1, 2);
            Assert.IsTrue(progress.IsLevelUnlocked(1));
            Assert.IsTrue(progress.IsLevelUnlocked(2));
            Assert.IsFalse(progress.IsLevelUnlocked(3));
        }
    }
}
