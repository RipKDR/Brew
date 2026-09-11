using System;
using System.Text;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class DeadlockResolverTests
    {
        private const int MaxReshuffles = 10;
        private const int MinClusterSize = 3;

        private BoardModel _board;
        private TokenSpawner _spawner;
        private DeadlockResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(7, 9);
            _spawner = new TokenSpawner(new Random(42));
            _resolver = new DeadlockResolver(_spawner, MaxReshuffles, MinClusterSize);
        }

        [Test]
        public void Constructor_RejectsNonPositiveMaxReshuffles()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _ = new DeadlockResolver(_spawner, 0, MinClusterSize));
        }

        [Test]
        public void IsDeadlocked_TrueWhenNoClustersOrOrbChains()
        {
            FillCheckerboardNoClusters();
            Assert.IsTrue(_resolver.IsDeadlocked(_board));
        }

        [Test]
        public void IsDeadlocked_FalseWhenValidTokenClusterExists()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));
            FillRemainingWithAlternating();

            Assert.IsFalse(_resolver.IsDeadlocked(_board));
        }

        [Test]
        public void TryRecover_EmptyClusterBoard_ReshufflesIntoPlayableCluster()
        {
            FillCheckerboardNoClusters();
            Assert.IsTrue(_resolver.IsDeadlocked(_board));

            var result = _resolver.TryRecover(_board, LevelOutcome.InProgress);

            Assert.IsTrue(result.Attempted);
            Assert.IsTrue(result.Recovered);
            Assert.AreEqual(DeadlockRecoveryKind.Reshuffled, result.Kind);
            Assert.Greater(result.ReshuffleAttempts, 0);
            Assert.LessOrEqual(result.ReshuffleAttempts, MaxReshuffles);
            Assert.IsFalse(_resolver.IsDeadlocked(_board));
            Assert.GreaterOrEqual(
                new ClusterDetector(_board).FindAllClusters(MinClusterSize).Count, 1);
        }

        [Test]
        public void TryRecover_PreservesOrbPositionsAndTokenCount()
        {
            var orb = new GridCoord(1, 1);
            _board.SetCell(orb, CellContent.Orb(IngredientColor.Frost, 4));
            FillCheckerboardNoClusters();

            var result = _resolver.TryRecover(_board, LevelOutcome.InProgress);

            Assert.IsTrue(result.Recovered);
            Assert.IsTrue(_board.GetCell(orb).IsOrb);
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(orb).Color);
            Assert.AreEqual(4, _board.GetCell(orb).TokenCount);
        }

        [Test]
        public void TryRecover_PreservesStones()
        {
            var stone = new GridCoord(3, 3);
            _board.SetCell(stone, CellContent.Stone);
            FillCheckerboardNoClusters();

            _resolver.TryRecover(_board, LevelOutcome.InProgress);

            Assert.IsTrue(_board.GetCell(stone).IsStone);
        }

        [Test]
        public void TryRecover_TenFailedShuffles_ThenRegenerates()
        {
            FillBoardWithOrbsExceptTwoTokens();

            var snapshot = Snapshot();
            var result = _resolver.TryRecover(_board, LevelOutcome.InProgress);

            Assert.AreEqual(DeadlockRecoveryKind.Regenerated, result.Kind);
            Assert.IsTrue(result.UsedFullRegeneration);
            Assert.AreEqual(MaxReshuffles, result.ReshuffleAttempts);
            Assert.AreNotEqual(snapshot, Snapshot());

            Assert.IsTrue(_board.GetCell(new GridCoord(0, 0)).IsOrb);
            Assert.AreEqual(IngredientColor.Sun, _board.GetCell(new GridCoord(0, 0)).Color);
            Assert.AreEqual(5, _board.GetCell(new GridCoord(0, 0)).TokenCount);
        }

        [Test]
        public void TryRecover_Win_SkipsReshuffle()
        {
            FillCheckerboardNoClusters();
            var before = Snapshot();

            var result = _resolver.TryRecover(_board, LevelOutcome.Win);

            Assert.AreEqual(DeadlockRecoveryKind.Skipped, result.Kind);
            Assert.IsFalse(result.Attempted);
            Assert.AreEqual(before, Snapshot());
            Assert.IsTrue(_resolver.IsDeadlocked(_board));
        }

        [Test]
        public void TryRecover_Lose_SkipsReshuffle()
        {
            FillCheckerboardNoClusters();
            var before = Snapshot();

            var result = _resolver.TryRecover(_board, LevelOutcome.Lose);

            Assert.AreEqual(DeadlockRecoveryKind.Skipped, result.Kind);
            Assert.IsFalse(result.Attempted);
            Assert.AreEqual(before, Snapshot());
        }

        [Test]
        public void TryRecover_InProgressPlayableBoard_DoesNotShuffle()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));
            FillRemainingWithAlternating();
            var before = Snapshot();

            var result = _resolver.TryRecover(_board, LevelOutcome.InProgress);

            Assert.AreEqual(DeadlockRecoveryKind.NotDeadlocked, result.Kind);
            Assert.AreEqual(before, Snapshot());
        }

        private string Snapshot()
        {
            var chars = new StringBuilder();
            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                {
                    chars.Append(_board.GetCell(new GridCoord(c, r)));
                    chars.Append('|');
                }
            }

            return chars.ToString();
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

        /// <summary>
        /// Two isolated tokens cannot form a cluster of 3 no matter how colors shuffle.
        /// After max reshuffles the resolver must regenerate (orbs stay).
        /// </summary>
        private void FillBoardWithOrbsExceptTwoTokens()
        {
            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                    _board.SetCell(new GridCoord(c, r), CellContent.Orb(IngredientColor.Sun, 5));
            }

            _board.SetCell(new GridCoord(0, 0), CellContent.Orb(IngredientColor.Sun, 5));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(4, 0), CellContent.Token(IngredientColor.Frost));
        }
    }
}
