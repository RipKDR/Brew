using System.IO;
using Brew.Core;
using Brew.Data;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Data
{
    /// <summary>
    /// Validates that all 40 level JSON files parse into valid LevelConfig instances.
    /// </summary>
    [TestFixture]
    public class LevelDataIntegrationTests
    {
        private static readonly string LevelsDirectory = Path.Combine(
            System.IO.Directory.GetCurrentDirectory(), "..", "..", "levels");

        private static string GetLevelJsonPath(int levelId)
        {
            string projectRoot = FindProjectRoot();
            return Path.Combine(projectRoot, "levels", $"level_{levelId:D3}.json");
        }

        private static string FindProjectRoot()
        {
            string dir = System.IO.Directory.GetCurrentDirectory();
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir, "AGENTS.md")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..");
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(40)]
        public void Level_ParsesSuccessfully(int levelId)
        {
            string path = GetLevelJsonPath(levelId);
            if (!File.Exists(path))
            {
                Assert.Inconclusive($"Level file not found at {path}. Skipping.");
                return;
            }

            string json = File.ReadAllText(path);
            var config = LevelLoader.LoadFromJson(json);

            Assert.AreEqual(levelId, config.LevelId);
            Assert.GreaterOrEqual(config.GridWidth, 5);
            Assert.LessOrEqual(config.GridWidth, 9);
            Assert.GreaterOrEqual(config.GridHeight, 5);
            Assert.LessOrEqual(config.GridHeight, 11);
            Assert.Greater(config.IngredientPool.Count, 0);
            Assert.Greater(config.RecipeTargets.Count, 0);
            Assert.Greater(config.MoveLimit, 0);
            Assert.AreEqual(3, config.StarThresholds.Count);
        }

        [Test]
        public void AllLevels_HavePositiveStarThresholds()
        {
            for (int id = 1; id <= 40; id++)
            {
                string path = GetLevelJsonPath(id);
                if (!File.Exists(path)) continue;

                string json = File.ReadAllText(path);
                var config = LevelLoader.LoadFromJson(json);

                Assert.Greater(config.StarThresholds[0], 0, $"Level {id} star threshold 1");
                Assert.Greater(config.StarThresholds[1], config.StarThresholds[0],
                    $"Level {id} star threshold 2 must exceed threshold 1");
                Assert.Greater(config.StarThresholds[2], config.StarThresholds[1],
                    $"Level {id} star threshold 3 must exceed threshold 2");
            }
        }

        [Test]
        public void AllLevels_RecipeTargetsUsePoolColors()
        {
            for (int id = 1; id <= 40; id++)
            {
                string path = GetLevelJsonPath(id);
                if (!File.Exists(path)) continue;

                string json = File.ReadAllText(path);
                var config = LevelLoader.LoadFromJson(json);

                foreach (var target in config.RecipeTargets)
                {
                    bool found = false;
                    for (int i = 0; i < config.IngredientPool.Count; i++)
                    {
                        if (config.IngredientPool[i] == target.Color)
                        {
                            found = true;
                            break;
                        }
                    }
                    Assert.IsTrue(found,
                        $"Level {id}: recipe target color {target.Color} not in ingredient pool");
                }
            }
        }
    }
}
