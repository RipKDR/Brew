using System;
using System.Linq;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class CascadeResolverTests
    {
        private BoardModel _board;
        private TokenSpawner _spawner;
        private CascadeResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(7, 9);
            _spawner = new TokenSpawner(new Random(42));
            _resolver = new CascadeResolver(_board, _spawner);
        }

        [Test]
        public void ApplyGravity_TokensFallToFillGaps()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(0, 2), CellContent.Token(IngredientColor.Frost));

            var steps = _resolver.ApplyGravity();

            Assert.IsTrue(_board.GetCell(new GridCoord(0, 0)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(0, 2)).IsEmpty);

            Assert.AreEqual(IngredientColor.Ember, _board.GetCell(new GridCoord(0, 7)).Color);
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(new GridCoord(0, 8)).Color);

            Assert.IsTrue(steps.Count > 0);
        }

        [Test]
        public void ApplyGravity_NoGaps_NoSteps()
        {
            for (int r = 0; r < _board.Height; r++)
                _board.SetCell(new GridCoord(0, r), CellContent.Token(IngredientColor.Ember));

            var steps = _resolver.ApplyGravity();

            Assert.AreEqual(0, steps.Count);
        }

        [Test]
        public void ApplyGravity_OrbsAlsoFall()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Orb(IngredientColor.Ember, 3));

            var steps = _resolver.ApplyGravity();

            Assert.IsTrue(_board.GetCell(new GridCoord(0, 0)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(0, 8)).IsOrb);
            Assert.AreEqual(3, _board.GetCell(new GridCoord(0, 8)).TokenCount);
        }

        [Test]
        public void ApplyGravity_PreservesColumnOrder()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(0, 2), CellContent.Token(IngredientColor.Frost));
            _board.SetCell(new GridCoord(0, 4), CellContent.Token(IngredientColor.Vine));

            _resolver.ApplyGravity();

            Assert.AreEqual(IngredientColor.Ember, _board.GetCell(new GridCoord(0, 6)).Color);
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(new GridCoord(0, 7)).Color);
            Assert.AreEqual(IngredientColor.Vine, _board.GetCell(new GridCoord(0, 8)).Color);
        }

        [Test]
        public void RefillColumns_FillsEmptyCellsAtTop()
        {
            _board.SetCell(new GridCoord(0, 7), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(0, 8), CellContent.Token(IngredientColor.Frost));

            var spawns = _resolver.RefillColumns();

            for (int r = 0; r < 7; r++)
                Assert.IsTrue(_board.GetCell(new GridCoord(0, r)).IsToken,
                    $"Row {r} should be filled.");

            Assert.AreEqual(7, spawns.Count);
            Assert.IsTrue(spawns.All(s => s.IsNewSpawn));
        }

        [Test]
        public void ApplyGravityAndRefill_LeavesNoEmptyCells()
        {
            _board.SetCell(new GridCoord(0, 2), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(0, 5), CellContent.Token(IngredientColor.Frost));

            _resolver.ApplyGravityAndRefill();

            for (int r = 0; r < _board.Height; r++)
                Assert.IsFalse(_board.GetCell(new GridCoord(0, r)).IsEmpty,
                    $"Row {r} should not be empty after gravity + refill.");
        }

        [Test]
        public void GravityStep_ReportsCorrectDistance()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));

            var steps = _resolver.ApplyGravity();

            Assert.AreEqual(1, steps.Count);
            Assert.AreEqual(8, steps[0].Distance);
            Assert.AreEqual(new GridCoord(0, 0), steps[0].From);
            Assert.AreEqual(new GridCoord(0, 8), steps[0].To);
            Assert.IsFalse(steps[0].IsNewSpawn);
        }

        [Test]
        public void ApplyGravity_ColumnsIndependent()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 4), CellContent.Token(IngredientColor.Frost));

            _resolver.ApplyGravity();

            Assert.AreEqual(IngredientColor.Ember, _board.GetCell(new GridCoord(0, 8)).Color);
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(new GridCoord(1, 8)).Color);
        }

        [Test]
        public void ApplyGravity_GapInMiddleOfColumn_CompactsCorrectly()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(0, 1), CellContent.Token(IngredientColor.Frost));
            // gap at row 2
            _board.SetCell(new GridCoord(0, 3), CellContent.Token(IngredientColor.Vine));

            _resolver.ApplyGravity();

            Assert.AreEqual(IngredientColor.Ember, _board.GetCell(new GridCoord(0, 6)).Color);
            Assert.AreEqual(IngredientColor.Frost, _board.GetCell(new GridCoord(0, 7)).Color);
            Assert.AreEqual(IngredientColor.Vine, _board.GetCell(new GridCoord(0, 8)).Color);
        }
    }
}
