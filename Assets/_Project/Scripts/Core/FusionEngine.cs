using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Executes player-initiated and cascade-triggered fusions on the board.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class FusionEngine
    {
        private readonly BoardModel _board;
        private readonly ClusterDetector _detector;
        private readonly int _minClusterSize;
        private readonly int _brewThreshold;

        public FusionEngine(BoardModel board, ClusterDetector detector, int minClusterSize, int brewThreshold)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _minClusterSize = minClusterSize;
            _brewThreshold = brewThreshold;
        }

        /// <summary>
        /// Attempts a player-initiated fusion at the tapped coordinate.
        /// Returns null if the tap is invalid (not a token, cluster too small).
        /// The orb is placed at the tapped cell per core-mechanic.md §4.3.
        /// </summary>
        public FusionResult TryPlayerFusion(GridCoord tappedCell)
        {
            var cell = _board.GetCell(tappedCell);
            if (!cell.IsToken)
                return null;

            var cluster = _detector.FindCluster(tappedCell);
            if (cluster.Count < _minClusterSize)
                return null;

            return ExecuteFusion(cluster, tappedCell);
        }

        /// <summary>
        /// Executes an automatic cascade fusion. The orb is placed at the
        /// bottom-most, then left-most cell in the cluster (core-mechanic.md §7.3).
        /// </summary>
        public FusionResult ExecuteCascadeFusion(List<GridCoord> cluster)
        {
            if (cluster == null || cluster.Count < _minClusterSize)
                return null;

            var orbCell = FindCascadeOrbCell(cluster);
            return ExecuteFusion(cluster, orbCell);
        }

        /// <summary>
        /// Executes chain fusion for a group of same-color adjacent orbs.
        /// Survivor is bottom-most, then left-most (core-mechanic.md §5.3).
        /// Returns the survivor coordinate and merged token count.
        /// </summary>
        public ChainFusionResult ExecuteChainFusion(List<GridCoord> orbGroup)
        {
            if (orbGroup == null || orbGroup.Count < 2)
                return null;

            var survivor = FindCascadeOrbCell(orbGroup);
            int mergedCount = 0;
            var consumed = new List<GridCoord>();

            foreach (var coord in orbGroup)
            {
                var orb = _board.GetCell(coord);
                mergedCount += orb.TokenCount;

                if (coord != survivor)
                {
                    _board.SetCell(coord, CellContent.Empty);
                    consumed.Add(coord);
                }
            }

            bool brews = mergedCount >= _brewThreshold;
            _board.SetCell(survivor, CellContent.Orb(_board.GetCell(survivor).Color, mergedCount));

            return new ChainFusionResult(consumed, survivor, mergedCount, brews);
        }

        private FusionResult ExecuteFusion(List<GridCoord> cluster, GridCoord orbCell)
        {
            var color = _board.GetCell(cluster[0]).Color;
            int tokenCount = cluster.Count;

            foreach (var coord in cluster)
                _board.SetCell(coord, CellContent.Empty);

            bool brews = tokenCount >= _brewThreshold;
            var orb = CellContent.Orb(color, tokenCount);
            _board.SetCell(orbCell, orb);

            return new FusionResult(cluster, orbCell, orb, brews);
        }

        /// <summary>
        /// Bottom-most (highest row), then left-most (lowest col).
        /// </summary>
        private static GridCoord FindCascadeOrbCell(List<GridCoord> cells)
        {
            var best = cells[0];
            for (int i = 1; i < cells.Count; i++)
            {
                var c = cells[i];
                if (c.Row > best.Row || (c.Row == best.Row && c.Col < best.Col))
                    best = c;
            }
            return best;
        }
    }

    /// <summary>
    /// Result of a chain-fusion merge between adjacent same-color orbs.
    /// </summary>
    public sealed class ChainFusionResult
    {
        public IReadOnlyList<GridCoord> ConsumedOrbs { get; }
        public GridCoord SurvivorCell { get; }
        public int MergedTokenCount { get; }
        public bool TriggeredBrew { get; }

        public ChainFusionResult(
            IReadOnlyList<GridCoord> consumedOrbs,
            GridCoord survivorCell,
            int mergedTokenCount,
            bool triggeredBrew)
        {
            ConsumedOrbs = consumedOrbs;
            SurvivorCell = survivorCell;
            MergedTokenCount = mergedTokenCount;
            TriggeredBrew = triggeredBrew;
        }
    }
}
