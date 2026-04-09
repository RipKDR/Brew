using System;
using System.Collections.Generic;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class TokenSpawnerTests
    {
        private const int Seed = 42;

        [Test]
        public void PopulateBoard_FillsAllCells()
        {
            var board = new BoardModel(7, 9);
            var spawner = new TokenSpawner(new Random(Seed));

            spawner.PopulateBoard(board);

            for (int c = 0; c < board.Width; c++)
                for (int r = 0; r < board.Height; r++)
                    Assert.IsFalse(board.GetCell(new GridCoord(c, r)).IsEmpty,
                        $"Cell ({c},{r}) should not be empty after populate.");
        }

        [Test]
        public void PopulateBoard_AllCellsAreTokens()
        {
            var board = new BoardModel(7, 9);
            var spawner = new TokenSpawner(new Random(Seed));

            spawner.PopulateBoard(board);

            for (int c = 0; c < board.Width; c++)
                for (int r = 0; r < board.Height; r++)
                    Assert.IsTrue(board.GetCell(new GridCoord(c, r)).IsToken,
                        $"Cell ({c},{r}) should be a token.");
        }

        [Test]
        public void PopulateBoard_NoClusterExceedsMaxSize()
        {
            var board = new BoardModel(7, 9);
            var spawner = new TokenSpawner(new Random(Seed));

            spawner.PopulateBoard(board, maxClusterAtSpawn: 5);

            var detector = new ClusterDetector(board);
            var clusters = detector.FindAllClusters(minClusterSize: 6);

            Assert.AreEqual(0, clusters.Count,
                "No cluster should exceed size 5 at spawn.");
        }

        [Test]
        public void PopulateBoard_HasAtLeastThreeValidClusters()
        {
            var board = new BoardModel(7, 9);
            var spawner = new TokenSpawner(new Random(Seed));

            spawner.PopulateBoard(board);

            var detector = new ClusterDetector(board);
            var clusters = detector.FindAllClusters(minClusterSize: 3);

            Assert.GreaterOrEqual(clusters.Count, 3,
                "Board should have at least 3 valid clusters.");
        }

        [Test]
        public void PopulateBoard_UsesOnlyAvailableColors()
        {
            var colors = new[] { IngredientColor.Ember, IngredientColor.Frost, IngredientColor.Vine };
            var board = new BoardModel(7, 9);
            var spawner = new TokenSpawner(new Random(Seed), colors);

            spawner.PopulateBoard(board);

            var allowedSet = new HashSet<IngredientColor>(colors);
            for (int c = 0; c < board.Width; c++)
                for (int r = 0; r < board.Height; r++)
                {
                    var cell = board.GetCell(new GridCoord(c, r));
                    Assert.IsTrue(allowedSet.Contains(cell.Color),
                        $"Cell ({c},{r}) has color {cell.Color} which is not in the allowed set.");
                }
        }

        [Test]
        public void PopulateBoard_DeterministicWithSameSeed()
        {
            var board1 = new BoardModel(7, 9);
            var board2 = new BoardModel(7, 9);
            new TokenSpawner(new Random(Seed)).PopulateBoard(board1);
            new TokenSpawner(new Random(Seed)).PopulateBoard(board2);

            for (int c = 0; c < 7; c++)
                for (int r = 0; r < 9; r++)
                {
                    var coord = new GridCoord(c, r);
                    Assert.AreEqual(board1.GetCell(coord).Color, board2.GetCell(coord).Color,
                        $"Cell ({c},{r}) should be identical with same seed.");
                }
        }

        [Test]
        public void PopulateBoard_DifferentSeedsProduceDifferentBoards()
        {
            var board1 = new BoardModel(7, 9);
            var board2 = new BoardModel(7, 9);
            new TokenSpawner(new Random(100)).PopulateBoard(board1);
            new TokenSpawner(new Random(999)).PopulateBoard(board2);

            bool anyDifferent = false;
            for (int c = 0; c < 7; c++)
                for (int r = 0; r < 9; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (board1.GetCell(coord).Color != board2.GetCell(coord).Color)
                    {
                        anyDifferent = true;
                        break;
                    }
                }

            Assert.IsTrue(anyDifferent, "Different seeds should produce different boards.");
        }

        [Test]
        public void GenerateRandomColor_ReturnsValidColor()
        {
            var spawner = new TokenSpawner(new Random(Seed));

            for (int i = 0; i < 100; i++)
            {
                var color = spawner.GenerateRandomColor();
                Assert.IsTrue(Enum.IsDefined(typeof(IngredientColor), color));
            }
        }

        [Test]
        public void FillCell_PlacesTokenAtCoord()
        {
            var board = new BoardModel(7, 9);
            var spawner = new TokenSpawner(new Random(Seed));
            var coord = new GridCoord(3, 4);

            spawner.FillCell(board, coord);

            Assert.IsTrue(board.GetCell(coord).IsToken);
        }

        [Test]
        public void PopulateBoard_MultipleRuns_AllValid()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var board = new BoardModel(7, 9);
                var spawner = new TokenSpawner(new Random(seed));

                spawner.PopulateBoard(board);

                var detector = new ClusterDetector(board);
                var largeClusters = detector.FindAllClusters(minClusterSize: 6);
                Assert.AreEqual(0, largeClusters.Count,
                    $"Seed {seed}: no cluster should exceed size 5.");

                var validClusters = detector.FindAllClusters(minClusterSize: 3);
                Assert.GreaterOrEqual(validClusters.Count, 3,
                    $"Seed {seed}: at least 3 valid clusters required.");
            }
        }
    }
}
