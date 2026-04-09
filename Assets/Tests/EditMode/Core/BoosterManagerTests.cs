using System;
using System.Collections.Generic;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class BoosterManagerTests
    {
        private const int Seed = 42;

        private BoardModel _board;
        private TokenSpawner _spawner;
        private ClusterDetector _detector;
        private BoosterManager _manager;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(7, 9);
            _spawner = new TokenSpawner(new Random(Seed));
            _detector = new ClusterDetector(_board);
            _manager = new BoosterManager(new Random(Seed));
            _manager.SetCharges(BoosterType.Shake, int.MaxValue);
            _manager.SetCharges(BoosterType.Catalyst, int.MaxValue);
            _manager.SetCharges(BoosterType.ExtraMoves, int.MaxValue);
        }

        // ── Charges ─────────────────────────────────────────

        [Test]
        public void GetCharges_DefaultsToZero()
        {
            var fresh = new BoosterManager();
            Assert.AreEqual(0, fresh.GetCharges(BoosterType.Shake));
            Assert.AreEqual(0, fresh.GetCharges(BoosterType.Catalyst));
            Assert.AreEqual(0, fresh.GetCharges(BoosterType.ExtraMoves));
        }

        [Test]
        public void TryUseCharge_WithFiniteCharges_Decrements()
        {
            _manager.SetCharges(BoosterType.Shake, 3);

            Assert.IsTrue(_manager.TryUseCharge(BoosterType.Shake));
            Assert.AreEqual(2, _manager.GetCharges(BoosterType.Shake));
        }

        [Test]
        public void TryUseCharge_AtZero_ReturnsFalse()
        {
            var fresh = new BoosterManager();

            Assert.IsFalse(fresh.TryUseCharge(BoosterType.Shake));
            Assert.AreEqual(0, fresh.GetCharges(BoosterType.Shake));
        }

        [Test]
        public void TryUseCharge_Infinite_DoesNotDecrement()
        {
            _manager.SetCharges(BoosterType.Catalyst, int.MaxValue);
            Assert.IsTrue(_manager.TryUseCharge(BoosterType.Catalyst));
            Assert.AreEqual(int.MaxValue, _manager.GetCharges(BoosterType.Catalyst));
        }

        [Test]
        public void AddCharges_IncreasesCount()
        {
            _manager.SetCharges(BoosterType.Catalyst, 2);
            _manager.AddCharges(BoosterType.Catalyst, 5);
            Assert.AreEqual(7, _manager.GetCharges(BoosterType.Catalyst));
        }

        [Test]
        public void OnChargesChanged_FiresOnTryUseCharge_WhenFinite()
        {
            _manager.SetCharges(BoosterType.Shake, 3);

            BoosterType? firedType = null;
            var firedAmount = -1;
            _manager.OnChargesChanged += (t, a) => { firedType = t; firedAmount = a; };

            _manager.TryUseCharge(BoosterType.Shake);

            Assert.AreEqual(BoosterType.Shake, firedType);
            Assert.AreEqual(2, firedAmount);
        }

        // ── Shake ───────────────────────────────────────────

        [Test]
        public void Shake_PreservesMultisetOfTokenColors()
        {
            _spawner.PopulateBoard(_board);

            var before = CollectTokenColors(_board);
            before.Sort();

            _manager.ActivateShake(_board, _spawner, _detector, 3);

            var after = CollectTokenColors(_board);
            after.Sort();

            CollectionAssert.AreEqual(before, after);
        }

        [Test]
        public void Shake_PreservesOrbCells()
        {
            _spawner.PopulateBoard(_board);

            var orbCoord = new GridCoord(3, 4);
            var orbContent = CellContent.Orb(IngredientColor.Ember, 2);
            _board.SetCell(orbCoord, orbContent);

            _manager.ActivateShake(_board, _spawner, _detector, 3);

            var afterCell = _board.GetCell(orbCoord);
            Assert.IsTrue(afterCell.IsOrb, "Orb cell type and placement should be preserved.");
            Assert.AreEqual(IngredientColor.Ember, afterCell.Color);
            Assert.AreEqual(2, afterCell.TokenCount);
        }

        [Test]
        public void Shake_GuaranteesValidClusterAfterShuffle()
        {
            _spawner.PopulateBoard(_board);

            _manager.ActivateShake(_board, _spawner, _detector, 3);

            var clusters = _detector.FindAllClusters(3);
            Assert.Greater(clusters.Count, 0, "Board must have at least one valid cluster after Shake.");
        }

        [Test]
        public void Shake_FiresOnBoosterActivated()
        {
            _spawner.PopulateBoard(_board);

            BoosterType? firedType = null;
            _manager.OnBoosterActivated += t => firedType = t;

            _manager.ActivateShake(_board, _spawner, _detector, 3);

            Assert.AreEqual(BoosterType.Shake, firedType);
        }

        [Test]
        public void Shake_DoesNotConsumeChargeWhenNoTokensOnBoard()
        {
            _manager.SetCharges(BoosterType.Shake, 2);

            _manager.ActivateShake(_board, _spawner, _detector, 3);

            Assert.AreEqual(2, _manager.GetCharges(BoosterType.Shake));
        }

        // ── Catalyst ────────────────────────────────────────

        [Test]
        public void Catalyst_CreatesOrbWithTokenCount1()
        {
            var coord = new GridCoord(2, 3);
            _board.SetCell(coord, CellContent.Token(IngredientColor.Frost));

            Assert.IsTrue(_manager.ActivateCatalyst(_board, coord));

            var boardCell = _board.GetCell(coord);
            Assert.IsTrue(boardCell.IsOrb);
            Assert.AreEqual(IngredientColor.Frost, boardCell.Color);
            Assert.AreEqual(1, boardCell.TokenCount);
        }

        [Test]
        public void Catalyst_RejectsNonTokenCell_Empty()
        {
            var coord = new GridCoord(0, 0);
            Assert.IsFalse(_manager.ActivateCatalyst(_board, coord));
        }

        [Test]
        public void Catalyst_RejectsNonTokenCell_Orb()
        {
            var coord = new GridCoord(1, 1);
            _board.SetCell(coord, CellContent.Orb(IngredientColor.Vine, 3));

            Assert.IsFalse(_manager.ActivateCatalyst(_board, coord));
        }

        [Test]
        public void Catalyst_FiresOnBoosterActivated()
        {
            var coord = new GridCoord(2, 2);
            _board.SetCell(coord, CellContent.Token(IngredientColor.Sun));

            BoosterType? firedType = null;
            _manager.OnBoosterActivated += t => firedType = t;

            Assert.IsTrue(_manager.ActivateCatalyst(_board, coord));

            Assert.AreEqual(BoosterType.Catalyst, firedType);
        }

        // ── ExtraMoves ──────────────────────────────────────

        [Test]
        public void ExtraMoves_Adds3Moves()
        {
            var tracker = new MoveTracker(10);
            tracker.TryConsumeMove();
            Assert.AreEqual(9, tracker.MovesRemaining);

            Assert.IsTrue(_manager.ActivateExtraMoves(tracker, 3));
            Assert.AreEqual(12, tracker.MovesRemaining);
        }

        [Test]
        public void ExtraMoves_CustomAmount()
        {
            var tracker = new MoveTracker(5);

            Assert.IsTrue(_manager.ActivateExtraMoves(tracker, 7));
            Assert.AreEqual(12, tracker.MovesRemaining);
        }

        [Test]
        public void ExtraMoves_OneUsePerAttempt_ReturnsFalseOnSecondCall()
        {
            var tracker = new MoveTracker(10);

            Assert.IsTrue(_manager.ActivateExtraMoves(tracker, 3));
            Assert.IsFalse(_manager.ActivateExtraMoves(tracker, 3));
            Assert.AreEqual(13, tracker.MovesRemaining);
        }

        [Test]
        public void ExtraMoves_ResetForNewAttempt_EnablesReuse()
        {
            var tracker = new MoveTracker(10);

            Assert.IsTrue(_manager.ActivateExtraMoves(tracker, 3));
            _manager.ResetForNewAttempt();
            Assert.IsTrue(_manager.ActivateExtraMoves(tracker, 3));
            Assert.AreEqual(16, tracker.MovesRemaining);
        }

        [Test]
        public void ExtraMoves_FiresOnBoosterActivated()
        {
            var tracker = new MoveTracker(10);

            BoosterType? firedType = null;
            _manager.OnBoosterActivated += t => firedType = t;

            Assert.IsTrue(_manager.ActivateExtraMoves(tracker, 5));

            Assert.AreEqual(BoosterType.ExtraMoves, firedType);
        }

        // ── Integration ─────────────────────────────────────

        [Test]
        public void FullSequence_UseAllThreeBoostersInOrder()
        {
            _spawner.PopulateBoard(_board);
            var tracker = new MoveTracker(10);

            var activated = new List<BoosterType>();
            _manager.OnBoosterActivated += t => activated.Add(t);

            _manager.ActivateShake(_board, _spawner, _detector, 3);

            var tokenCoord = FindFirstToken();
            Assert.IsNotNull(tokenCoord, "Board should have at least one token.");
            Assert.IsTrue(_manager.ActivateCatalyst(_board, tokenCoord.Value));

            _manager.ActivateExtraMoves(tracker, 5);

            Assert.AreEqual(3, activated.Count);
            Assert.AreEqual(BoosterType.Shake, activated[0]);
            Assert.AreEqual(BoosterType.Catalyst, activated[1]);
            Assert.AreEqual(BoosterType.ExtraMoves, activated[2]);
        }

        private static List<IngredientColor> CollectTokenColors(BoardModel board)
        {
            var list = new List<IngredientColor>();
            for (int c = 0; c < board.Width; c++)
            {
                for (int r = 0; r < board.Height; r++)
                {
                    var cell = board.GetCell(new GridCoord(c, r));
                    if (cell.IsToken)
                        list.Add(cell.Color);
                }
            }

            return list;
        }

        private GridCoord? FindFirstToken()
        {
            for (int c = 0; c < _board.Width; c++)
                for (int r = 0; r < _board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (_board.GetCell(coord).IsToken)
                        return coord;
                }
            return null;
        }
    }
}
