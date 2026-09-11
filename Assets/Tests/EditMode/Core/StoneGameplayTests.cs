using System;
using System.Collections.Generic;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class StoneGameplayTests
    {
        private BoardModel _board;
        private TokenSpawner _spawner;
        private CascadeResolver _resolver;
        private ClusterDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(7, 9);
            _spawner = new TokenSpawner(new Random(42));
            _resolver = new CascadeResolver(_board, _spawner);
            _detector = new ClusterDetector(_board);
        }

        [Test]
        public void PlaceStones_ThenPopulate_PreservesStonesAndHasValidCluster()
        {
            var placements = new[]
            {
                new BlockerPlacement("stone", row: 3, col: 2),
                new BlockerPlacement("stone", row: 5, col: 4),
                new BlockerPlacement("ice", row: 1, col: 1) // ignored for soft launch
            };

            // Mirrors BoardPresenter init order: stones first, then populate around them.
            PlaceStones(_board, placements);
            _spawner.PopulateBoard(_board);

            Assert.IsTrue(_board.GetCell(new GridCoord(2, 3)).IsStone);
            Assert.IsTrue(_board.GetCell(new GridCoord(4, 5)).IsStone);
            Assert.IsFalse(_board.GetCell(new GridCoord(1, 1)).IsStone);
            Assert.GreaterOrEqual(
                _detector.FindAllClusters(minClusterSize: 3).Count,
                1,
                "PopulateBoard after stones must leave at least one valid cluster.");
        }

        [Test]
        public void Gravity_FallsAroundStone_StoneDoesNotMove()
        {
            var stone = new GridCoord(0, 5);
            _board.SetCell(stone, CellContent.Stone);
            _board.SetCell(new GridCoord(0, 1), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(0, 7), CellContent.Token(IngredientColor.Frost));

            _resolver.ApplyGravity();

            Assert.IsTrue(_board.GetCell(stone).IsStone, "Stone must stay fixed.");
            // Token above stone falls to just above the stone (row 4).
            Assert.AreEqual(IngredientColor.Ember, _board.GetCell(new GridCoord(0, 4)).Color);
            // Token below stone falls to bottom.
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(new GridCoord(0, 8)).Color);
            Assert.IsTrue(_board.GetCell(new GridCoord(0, 1)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(0, 7)).IsEmpty);
        }

        [Test]
        public void Refill_FillsEmptiesAboveStone()
        {
            _board.SetCell(new GridCoord(0, 4), CellContent.Stone);
            _board.SetCell(new GridCoord(0, 8), CellContent.Token(IngredientColor.Ember));

            _resolver.ApplyGravityAndRefill();

            Assert.IsTrue(_board.GetCell(new GridCoord(0, 4)).IsStone);
            for (int r = 0; r < 4; r++)
                Assert.IsTrue(_board.GetCell(new GridCoord(0, r)).IsToken, $"Row {r} should refill above stone.");
            for (int r = 5; r < 8; r++)
                Assert.IsTrue(_board.GetCell(new GridCoord(0, r)).IsToken, $"Row {r} should refill below stone.");
            Assert.IsTrue(_board.GetCell(new GridCoord(0, 8)).IsToken);
        }

        [Test]
        public void ClearAdjacentStones_ClearsOrthogonalNeighborsOnly()
        {
            var brew = new GridCoord(3, 3);
            var ortho = new GridCoord(3, 2);
            var diagonal = new GridCoord(4, 2);
            var far = new GridCoord(0, 0);

            _board.SetCell(ortho, CellContent.Stone);
            _board.SetCell(diagonal, CellContent.Stone);
            _board.SetCell(far, CellContent.Stone);

            var cleared = StoneClearer.ClearAdjacentStones(_board, brew);

            Assert.AreEqual(1, cleared.Count);
            Assert.AreEqual(ortho, cleared[0]);
            Assert.IsTrue(_board.GetCell(ortho).IsEmpty);
            Assert.IsTrue(_board.GetCell(diagonal).IsStone);
            Assert.IsTrue(_board.GetCell(far).IsStone);
        }

        [Test]
        public void ClearAdjacentStones_NonAdjacentBrew_DoesNotClear()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Stone);

            var cleared = StoneClearer.ClearAdjacentStones(_board, new GridCoord(3, 3));

            Assert.AreEqual(0, cleared.Count);
            Assert.IsTrue(_board.GetCell(new GridCoord(0, 0)).IsStone);
        }

        [Test]
        public void ClusterDetection_IgnoresStoneCells()
        {
            // Three embers in a row interrupted by a stone would otherwise be size 3 if stones counted.
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Stone);
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(3, 0), CellContent.Token(IngredientColor.Ember));

            var clusters = _detector.FindAllClusters(minClusterSize: 3);

            Assert.AreEqual(0, clusters.Count);

            var pair = _detector.FindCluster(new GridCoord(2, 0));
            Assert.AreEqual(2, pair.Count);
        }

        [Test]
        public void IsDeadlocked_TrueWhenNoClustersOrOrbChains()
        {
            // Checkerboard-ish tokens with no 3-group, no orbs.
            FillCheckerboardNoClusters();

            Assert.IsTrue(_spawner.IsDeadlocked(_board));
        }

        [Test]
        public void IsDeadlocked_FalseWhenValidTokenClusterExists()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));
            FillRemainingWithAlternating();

            Assert.IsFalse(_spawner.IsDeadlocked(_board));
        }

        [Test]
        public void TryRecoverDeadlock_PreservesOrbsAndStones_AndCreatesPlay()
        {
            var stone = new GridCoord(3, 3);
            var orb = new GridCoord(1, 1);
            _board.SetCell(stone, CellContent.Stone);
            _board.SetCell(orb, CellContent.Orb(IngredientColor.Frost, 4));
            FillCheckerboardNoClusters();
            // Re-apply orb/stone in case fill overwrote — fill only wrote tokens on empty.
            Assert.IsTrue(_board.GetCell(stone).IsStone);
            Assert.IsTrue(_board.GetCell(orb).IsOrb);

            Assert.IsTrue(_spawner.IsDeadlocked(_board));
            bool recovered = _spawner.TryRecoverDeadlock(_board);

            Assert.IsTrue(recovered);
            Assert.IsFalse(_spawner.IsDeadlocked(_board));
            Assert.IsTrue(_board.GetCell(stone).IsStone);
            Assert.IsTrue(_board.GetCell(orb).IsOrb);
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(orb).Color);
            Assert.AreEqual(4, _board.GetCell(orb).TokenCount);
        }

        private static void PlaceStones(BoardModel board, IEnumerable<BlockerPlacement> placements)
        {
            foreach (var placement in placements)
            {
                if (!string.Equals(placement.Type, "stone", StringComparison.OrdinalIgnoreCase))
                    continue;

                var coord = new GridCoord(placement.Col, placement.Row);
                if (!board.InBounds(coord))
                    continue;

                board.SetCell(coord, CellContent.Stone);
            }
        }

        private void FillCheckerboardNoClusters()
        {
            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (!_board.GetCell(coord).IsEmpty)
                        continue;

                    // Alternate colors in a 2-pattern so no 3-in-a-row orthogonally of same color.
                    var color = ((c + r) % 2 == 0) ? IngredientColor.Ember : IngredientColor.Frost;
                    _board.SetCell(coord, CellContent.Token(color));
                }
            }
        }

        private void FillRemainingWithAlternating()
        {
            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (!_board.GetCell(coord).IsEmpty)
                        continue;
                    var color = ((c + r) % 2 == 0) ? IngredientColor.Vine : IngredientColor.Sun;
                    _board.SetCell(coord, CellContent.Token(color));
                }
            }
        }
    }
}
