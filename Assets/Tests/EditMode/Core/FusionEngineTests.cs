using System;
using System.Linq;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class FusionEngineTests
    {
        private BoardModel _board;
        private ClusterDetector _detector;
        private FusionEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(7, 9);
            _detector = new ClusterDetector(_board);
            _engine = new FusionEngine(_board, _detector, minClusterSize: 3, brewThreshold: 6);
        }

        [Test]
        public void TryPlayerFusion_ValidCluster_RemovesTokensAndPlacesOrb()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(3, 3), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(4, 3), CellContent.Token(IngredientColor.Ember));

            var result = _engine.TryPlayerFusion(new GridCoord(3, 3));

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.ConsumedCells.Count);
            Assert.AreEqual(new GridCoord(3, 3), result.OrbCell);
            Assert.IsTrue(result.CreatedOrb.IsOrb);
            Assert.AreEqual(IngredientColor.Ember, result.CreatedOrb.Color);
            Assert.AreEqual(3, result.CreatedOrb.TokenCount);
            Assert.IsFalse(result.TriggeredBrew);
        }

        [Test]
        public void TryPlayerFusion_OrbPlacedAtTappedCell()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Frost));
            _board.SetCell(new GridCoord(3, 3), CellContent.Token(IngredientColor.Frost));
            _board.SetCell(new GridCoord(4, 3), CellContent.Token(IngredientColor.Frost));

            var result = _engine.TryPlayerFusion(new GridCoord(2, 3));

            Assert.AreEqual(new GridCoord(2, 3), result.OrbCell);
            Assert.IsTrue(_board.GetCell(new GridCoord(2, 3)).IsOrb);
        }

        [Test]
        public void TryPlayerFusion_ClusterTooSmall_ReturnsNull()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(3, 3), CellContent.Token(IngredientColor.Ember));

            var result = _engine.TryPlayerFusion(new GridCoord(2, 3));

            Assert.IsNull(result);
        }

        [Test]
        public void TryPlayerFusion_TappedOrb_ReturnsNull()
        {
            _board.SetCell(new GridCoord(3, 3), CellContent.Orb(IngredientColor.Ember, 4));

            var result = _engine.TryPlayerFusion(new GridCoord(3, 3));

            Assert.IsNull(result);
        }

        [Test]
        public void TryPlayerFusion_TappedEmpty_ReturnsNull()
        {
            var result = _engine.TryPlayerFusion(new GridCoord(3, 3));

            Assert.IsNull(result);
        }

        [Test]
        public void TryPlayerFusion_ConsumedCellsBecomeEmpty()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(3, 3), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(4, 3), CellContent.Token(IngredientColor.Vine));

            _engine.TryPlayerFusion(new GridCoord(3, 3));

            Assert.IsTrue(_board.GetCell(new GridCoord(2, 3)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(4, 3)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(3, 3)).IsOrb);
        }

        [Test]
        public void TryPlayerFusion_SixTokens_TriggersBrewFlag()
        {
            for (int c = 0; c < 6; c++)
                _board.SetCell(new GridCoord(c, 3), CellContent.Token(IngredientColor.Sun));

            var result = _engine.TryPlayerFusion(new GridCoord(3, 3));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.TriggeredBrew);
            Assert.AreEqual(6, result.CreatedOrb.TokenCount);
        }

        [Test]
        public void TryPlayerFusion_FiveTokens_DoesNotBrewYet()
        {
            for (int c = 0; c < 5; c++)
                _board.SetCell(new GridCoord(c, 3), CellContent.Token(IngredientColor.Shadow));

            var result = _engine.TryPlayerFusion(new GridCoord(2, 3));

            Assert.IsNotNull(result);
            Assert.IsFalse(result.TriggeredBrew);
            Assert.AreEqual(5, result.CreatedOrb.TokenCount);
        }

        [Test]
        public void ExecuteCascadeFusion_PlacesOrbAtBottomLeft()
        {
            var cluster = new System.Collections.Generic.List<GridCoord>
            {
                new GridCoord(3, 2),
                new GridCoord(3, 3),
                new GridCoord(4, 3)
            };

            foreach (var coord in cluster)
                _board.SetCell(coord, CellContent.Token(IngredientColor.Ember));

            var result = _engine.ExecuteCascadeFusion(cluster);

            Assert.IsNotNull(result);
            Assert.AreEqual(new GridCoord(3, 3), result.OrbCell);
        }

        [Test]
        public void ExecuteChainFusion_MergesOrbs_SurvivorIsBottomLeft()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Orb(IngredientColor.Frost, 3));
            _board.SetCell(new GridCoord(3, 3), CellContent.Orb(IngredientColor.Frost, 4));

            var group = new System.Collections.Generic.List<GridCoord>
            {
                new GridCoord(2, 3),
                new GridCoord(3, 3)
            };

            var result = _engine.ExecuteChainFusion(group);

            Assert.IsNotNull(result);
            Assert.AreEqual(new GridCoord(2, 3), result.SurvivorCell);
            Assert.AreEqual(7, result.MergedTokenCount);
            Assert.IsTrue(result.TriggeredBrew);
        }

        [Test]
        public void ExecuteChainFusion_BelowThreshold_DoesNotBrew()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Orb(IngredientColor.Frost, 2));
            _board.SetCell(new GridCoord(3, 3), CellContent.Orb(IngredientColor.Frost, 3));

            var group = new System.Collections.Generic.List<GridCoord>
            {
                new GridCoord(2, 3),
                new GridCoord(3, 3)
            };

            var result = _engine.ExecuteChainFusion(group);

            Assert.AreEqual(5, result.MergedTokenCount);
            Assert.IsFalse(result.TriggeredBrew);
        }

        [Test]
        public void ExecuteChainFusion_ConsumedOrbsBecomeEmpty()
        {
            _board.SetCell(new GridCoord(2, 4), CellContent.Orb(IngredientColor.Vine, 3));
            _board.SetCell(new GridCoord(3, 4), CellContent.Orb(IngredientColor.Vine, 3));
            _board.SetCell(new GridCoord(3, 5), CellContent.Orb(IngredientColor.Vine, 3));

            var group = new System.Collections.Generic.List<GridCoord>
            {
                new GridCoord(2, 4),
                new GridCoord(3, 4),
                new GridCoord(3, 5)
            };

            var result = _engine.ExecuteChainFusion(group);

            Assert.AreEqual(new GridCoord(3, 5), result.SurvivorCell);
            Assert.IsTrue(_board.GetCell(new GridCoord(2, 4)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(3, 4)).IsEmpty);
            Assert.IsTrue(_board.GetCell(new GridCoord(3, 5)).IsOrb);
            Assert.AreEqual(9, _board.GetCell(new GridCoord(3, 5)).TokenCount);
        }
    }
}
