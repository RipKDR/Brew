using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// BFS flood-fill cluster detection for same-color tokens.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class ClusterDetector
    {
        private readonly BoardModel _board;

        public ClusterDetector(BoardModel board)
        {
            _board = board;
        }

        /// <summary>
        /// Finds the maximal connected component of same-color tokens
        /// containing the given seed coordinate. Only considers TOKEN cells;
        /// ORB and EMPTY cells are excluded.
        /// </summary>
        public List<GridCoord> FindCluster(GridCoord seed)
        {
            var cluster = new List<GridCoord>();

            if (!_board.TryGetCell(seed, out var seedContent) || !seedContent.IsToken)
                return cluster;

            var targetColor = seedContent.Color;
            var visited = new HashSet<GridCoord>();
            var queue = new Queue<GridCoord>();

            queue.Enqueue(seed);
            visited.Add(seed);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (!_board.TryGetCell(current, out var cell))
                    continue;

                if (!cell.IsToken || cell.Color != targetColor)
                    continue;

                cluster.Add(current);

                EnqueueIfNew(current.Up, visited, queue);
                EnqueueIfNew(current.Down, visited, queue);
                EnqueueIfNew(current.Left, visited, queue);
                EnqueueIfNew(current.Right, visited, queue);
            }

            return cluster;
        }

        /// <summary>
        /// Finds all valid clusters (size >= minClusterSize) on the board.
        /// Each cell belongs to at most one cluster.
        /// </summary>
        public List<List<GridCoord>> FindAllClusters(int minClusterSize)
        {
            var allClusters = new List<List<GridCoord>>();
            var globalVisited = new HashSet<GridCoord>();

            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (globalVisited.Contains(coord))
                        continue;

                    var cell = _board.GetCell(coord);
                    if (!cell.IsToken)
                        continue;

                    var cluster = FindCluster(coord);
                    foreach (var member in cluster)
                        globalVisited.Add(member);

                    if (cluster.Count >= minClusterSize)
                        allClusters.Add(cluster);
                }
            }

            return allClusters;
        }

        /// <summary>
        /// Finds all connected groups of same-color orbs (size >= 2).
        /// Used for chain-fusion detection.
        /// </summary>
        public List<List<GridCoord>> FindAllOrbChainGroups()
        {
            var groups = new List<List<GridCoord>>();
            var globalVisited = new HashSet<GridCoord>();

            for (int c = 0; c < _board.Width; c++)
            {
                for (int r = 0; r < _board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (globalVisited.Contains(coord))
                        continue;

                    var cell = _board.GetCell(coord);
                    if (!cell.IsOrb)
                        continue;

                    var group = FindOrbChainGroup(coord);
                    foreach (var member in group)
                        globalVisited.Add(member);

                    if (group.Count >= 2)
                        groups.Add(group);
                }
            }

            return groups;
        }

        /// <summary>
        /// Finds the connected group of same-color orbs containing the seed.
        /// </summary>
        public List<GridCoord> FindOrbChainGroup(GridCoord seed)
        {
            var group = new List<GridCoord>();

            if (!_board.TryGetCell(seed, out var seedContent) || !seedContent.IsOrb)
                return group;

            var targetColor = seedContent.Color;
            var visited = new HashSet<GridCoord>();
            var queue = new Queue<GridCoord>();

            queue.Enqueue(seed);
            visited.Add(seed);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (!_board.TryGetCell(current, out var cell))
                    continue;

                if (!cell.IsOrb || cell.Color != targetColor)
                    continue;

                group.Add(current);

                EnqueueIfNew(current.Up, visited, queue);
                EnqueueIfNew(current.Down, visited, queue);
                EnqueueIfNew(current.Left, visited, queue);
                EnqueueIfNew(current.Right, visited, queue);
            }

            return group;
        }

        private static void EnqueueIfNew(GridCoord coord, HashSet<GridCoord> visited, Queue<GridCoord> queue)
        {
            if (visited.Add(coord))
                queue.Enqueue(coord);
        }
    }
}
