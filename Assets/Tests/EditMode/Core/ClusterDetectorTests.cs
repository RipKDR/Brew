using System.Collections.Generic;
using System.Linq;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class ClusterDetectorTests
    {
        private BoardModel _board;
        private ClusterDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardModel(7, 9);
            _detector = new ClusterDetector(_board);
        }

        [Test]
        public void FindCluster_SingleToken_ReturnsClusterOfOne()
        {
            _board.SetCell(new GridCoord(3, 4), CellContent.Token(IngredientColor.Ember));

            var cluster = _detector.FindCluster(new GridCoord(3, 4));

            Assert.AreEqual(1, cluster.Count);
            Assert.AreEqual(new GridCoord(3, 4), cluster[0]);
        }

        [Test]
        public void FindCluster_ThreeHorizontal_FindsAll()
        {
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Frost));
            _board.SetCell(new GridCoord(3, 3), CellContent.Token(IngredientColor.Frost));
            _board.SetCell(new GridCoord(4, 3), CellContent.Token(IngredientColor.Frost));

            var cluster = _detector.FindCluster(new GridCoord(3, 3));

            Assert.AreEqual(3, cluster.Count);
            Assert.IsTrue(cluster.Contains(new GridCoord(2, 3)));
            Assert.IsTrue(cluster.Contains(new GridCoord(3, 3)));
            Assert.IsTrue(cluster.Contains(new GridCoord(4, 3)));
        }

        [Test]
        public void FindCluster_ThreeVertical_FindsAll()
        {
            _board.SetCell(new GridCoord(1, 2), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(1, 3), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(1, 4), CellContent.Token(IngredientColor.Vine));

            var cluster = _detector.FindCluster(new GridCoord(1, 3));

            Assert.AreEqual(3, cluster.Count);
        }

        [Test]
        public void FindCluster_LShape_FindsAll()
        {
            _board.SetCell(new GridCoord(2, 2), CellContent.Token(IngredientColor.Sun));
            _board.SetCell(new GridCoord(2, 3), CellContent.Token(IngredientColor.Sun));
            _board.SetCell(new GridCoord(2, 4), CellContent.Token(IngredientColor.Sun));
            _board.SetCell(new GridCoord(3, 4), CellContent.Token(IngredientColor.Sun));
            _board.SetCell(new GridCoord(4, 4), CellContent.Token(IngredientColor.Sun));

            var cluster = _detector.FindCluster(new GridCoord(2, 2));

            Assert.AreEqual(5, cluster.Count);
        }

        [Test]
        public void FindCluster_DiagonalNotAdjacent_SeparateClusters()
        {
            _board.SetCell(new GridCoord(2, 2), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(3, 3), CellContent.Token(IngredientColor.Ember));

            var cluster = _detector.FindCluster(new GridCoord(2, 2));

            Assert.AreEqual(1, cluster.Count);
        }

        [Test]
        public void FindCluster_DifferentColor_StopsAtBoundary()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Frost));

            var cluster = _detector.FindCluster(new GridCoord(0, 0));

            Assert.AreEqual(2, cluster.Count);
            Assert.IsFalse(cluster.Contains(new GridCoord(2, 0)));
        }

        [Test]
        public void FindCluster_EmptyCell_ReturnsEmpty()
        {
            var cluster = _detector.FindCluster(new GridCoord(0, 0));
            Assert.AreEqual(0, cluster.Count);
        }

        [Test]
        public void FindCluster_OrbCell_ReturnsEmpty()
        {
            _board.SetCell(new GridCoord(3, 3), CellContent.Orb(IngredientColor.Ember, 4));

            var cluster = _detector.FindCluster(new GridCoord(3, 3));

            Assert.AreEqual(0, cluster.Count);
        }

        [Test]
        public void FindCluster_TokensDoNotCrossThroughOrbs()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Orb(IngredientColor.Ember, 3));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));

            var cluster = _detector.FindCluster(new GridCoord(0, 0));

            Assert.AreEqual(1, cluster.Count);
        }

        [Test]
        public void FindAllClusters_MinSize3_FiltersSmallClusters()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));

            _board.SetCell(new GridCoord(4, 0), CellContent.Token(IngredientColor.Frost));
            _board.SetCell(new GridCoord(5, 0), CellContent.Token(IngredientColor.Frost));

            var allClusters = _detector.FindAllClusters(minClusterSize: 3);

            Assert.AreEqual(1, allClusters.Count);
            Assert.AreEqual(3, allClusters[0].Count);
        }

        [Test]
        public void FindAllClusters_MultipleClusters_FindsAllValid()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(1, 0), CellContent.Token(IngredientColor.Ember));
            _board.SetCell(new GridCoord(2, 0), CellContent.Token(IngredientColor.Ember));

            _board.SetCell(new GridCoord(0, 2), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(0, 3), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(0, 4), CellContent.Token(IngredientColor.Vine));
            _board.SetCell(new GridCoord(0, 5), CellContent.Token(IngredientColor.Vine));

            var allClusters = _detector.FindAllClusters(minClusterSize: 3);

            Assert.AreEqual(2, allClusters.Count);
            var sizes = allClusters.Select(c => c.Count).OrderBy(s => s).ToList();
            Assert.AreEqual(3, sizes[0]);
            Assert.AreEqual(4, sizes[1]);
        }

        [Test]
        public void FindOrbChainGroup_TwoAdjacentSameColor_FindsBoth()
        {
            _board.SetCell(new GridCoord(3, 3), CellContent.Orb(IngredientColor.Frost, 3));
            _board.SetCell(new GridCoord(3, 4), CellContent.Orb(IngredientColor.Frost, 4));

            var group = _detector.FindOrbChainGroup(new GridCoord(3, 3));

            Assert.AreEqual(2, group.Count);
        }

        [Test]
        public void FindOrbChainGroup_DifferentColors_SingleOrb()
        {
            _board.SetCell(new GridCoord(3, 3), CellContent.Orb(IngredientColor.Frost, 3));
            _board.SetCell(new GridCoord(3, 4), CellContent.Orb(IngredientColor.Ember, 4));

            var group = _detector.FindOrbChainGroup(new GridCoord(3, 3));

            Assert.AreEqual(1, group.Count);
        }

        [Test]
        public void FindAllOrbChainGroups_FindsGroupsOfTwoOrMore()
        {
            _board.SetCell(new GridCoord(0, 0), CellContent.Orb(IngredientColor.Ember, 3));
            _board.SetCell(new GridCoord(1, 0), CellContent.Orb(IngredientColor.Ember, 3));
            _board.SetCell(new GridCoord(5, 5), CellContent.Orb(IngredientColor.Frost, 2));

            var groups = _detector.FindAllOrbChainGroups();

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(2, groups[0].Count);
        }
    }
}
