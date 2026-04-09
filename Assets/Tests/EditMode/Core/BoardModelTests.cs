using System;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class BoardModelTests
    {
        [Test]
        public void Constructor_ValidDimensions_CreatesBoard()
        {
            var board = new BoardModel(7, 9);
            Assert.AreEqual(7, board.Width);
            Assert.AreEqual(9, board.Height);
            Assert.AreEqual(63, board.CellCount);
        }

        [TestCase(4, 9)]
        [TestCase(10, 9)]
        public void Constructor_InvalidWidth_Throws(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardModel(width, height));
        }

        [TestCase(7, 4)]
        [TestCase(7, 12)]
        public void Constructor_InvalidHeight_Throws(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardModel(width, height));
        }

        [Test]
        public void NewBoard_AllCellsEmpty()
        {
            var board = new BoardModel(5, 5);
            for (int c = 0; c < 5; c++)
                for (int r = 0; r < 5; r++)
                    Assert.IsTrue(board.GetCell(new GridCoord(c, r)).IsEmpty);
        }

        [Test]
        public void SetCell_GetCell_RoundTrips()
        {
            var board = new BoardModel(7, 9);
            var coord = new GridCoord(3, 4);
            var token = CellContent.Token(IngredientColor.Ember);

            board.SetCell(coord, token);
            var result = board.GetCell(coord);

            Assert.IsTrue(result.IsToken);
            Assert.AreEqual(IngredientColor.Ember, result.Color);
        }

        [Test]
        public void SetCell_Orb_PreservesTokenCount()
        {
            var board = new BoardModel(7, 9);
            var coord = new GridCoord(2, 3);
            var orb = CellContent.Orb(IngredientColor.Frost, 5);

            board.SetCell(coord, orb);
            var result = board.GetCell(coord);

            Assert.IsTrue(result.IsOrb);
            Assert.AreEqual(IngredientColor.Frost, result.Color);
            Assert.AreEqual(5, result.TokenCount);
        }

        [Test]
        public void InBounds_ValidCoord_ReturnsTrue()
        {
            var board = new BoardModel(7, 9);
            Assert.IsTrue(board.InBounds(new GridCoord(0, 0)));
            Assert.IsTrue(board.InBounds(new GridCoord(6, 8)));
            Assert.IsTrue(board.InBounds(new GridCoord(3, 4)));
        }

        [Test]
        public void InBounds_InvalidCoord_ReturnsFalse()
        {
            var board = new BoardModel(7, 9);
            Assert.IsFalse(board.InBounds(new GridCoord(-1, 0)));
            Assert.IsFalse(board.InBounds(new GridCoord(0, -1)));
            Assert.IsFalse(board.InBounds(new GridCoord(7, 0)));
            Assert.IsFalse(board.InBounds(new GridCoord(0, 9)));
        }

        [Test]
        public void GetCell_OutOfBounds_Throws()
        {
            var board = new BoardModel(7, 9);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => board.GetCell(new GridCoord(-1, 0)));
        }

        [Test]
        public void TryGetCell_OutOfBounds_ReturnsFalse()
        {
            var board = new BoardModel(7, 9);
            bool found = board.TryGetCell(new GridCoord(-1, 0), out var content);
            Assert.IsFalse(found);
            Assert.IsTrue(content.IsEmpty);
        }

        [Test]
        public void TryGetCell_ValidCoord_ReturnsTrue()
        {
            var board = new BoardModel(7, 9);
            var coord = new GridCoord(3, 4);
            board.SetCell(coord, CellContent.Token(IngredientColor.Vine));

            bool found = board.TryGetCell(coord, out var content);

            Assert.IsTrue(found);
            Assert.IsTrue(content.IsToken);
            Assert.AreEqual(IngredientColor.Vine, content.Color);
        }

        [Test]
        public void Clear_RemovesAllContent()
        {
            var board = new BoardModel(5, 5);
            board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            board.SetCell(new GridCoord(2, 3), CellContent.Orb(IngredientColor.Frost, 4));

            board.Clear();

            for (int c = 0; c < 5; c++)
                for (int r = 0; r < 5; r++)
                    Assert.IsTrue(board.GetCell(new GridCoord(c, r)).IsEmpty);
        }

        [Test]
        public void Clone_ProducesIndependentCopy()
        {
            var board = new BoardModel(5, 5);
            board.SetCell(new GridCoord(1, 1), CellContent.Token(IngredientColor.Sun));

            var clone = board.Clone();
            clone.SetCell(new GridCoord(1, 1), CellContent.Empty);

            Assert.IsTrue(board.GetCell(new GridCoord(1, 1)).IsToken);
            Assert.IsTrue(clone.GetCell(new GridCoord(1, 1)).IsEmpty);
        }

        [TestCase(5, 5)]
        [TestCase(7, 9)]
        [TestCase(9, 11)]
        public void Constructor_BoundaryDimensions_Succeeds(int width, int height)
        {
            var board = new BoardModel(width, height);
            Assert.AreEqual(width, board.Width);
            Assert.AreEqual(height, board.Height);
        }
    }
}
