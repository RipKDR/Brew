using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Populates the board with tokens while avoiding pre-made clusters.
    /// Also handles mid-game deadlock detection and recovery (core-mechanic.md §11).
    /// Pure C# — no Unity dependencies. Uses injectable System.Random for testability.
    /// </summary>
    public sealed class TokenSpawner
    {
        private const int MaxReshuffleAttempts = 10;

        private static readonly IngredientColor[] AllColors =
        {
            IngredientColor.Ember,
            IngredientColor.Frost,
            IngredientColor.Vine,
            IngredientColor.Sun,
            IngredientColor.Shadow
        };

        private readonly Random _rng;
        private readonly IngredientColor[] _availableColors;

        public TokenSpawner(Random rng, IngredientColor[] availableColors = null)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _availableColors = availableColors ?? AllColors;

            if (_availableColors.Length < 2)
                throw new ArgumentException("At least 2 colors are required.", nameof(availableColors));
        }

        /// <summary>
        /// Fills all empty cells in the board with tokens, avoiding clusters >= maxClusterAtSpawn.
        /// Stones and orbs already on the board are preserved.
        /// Per core-mechanic.md §10.4, max initial cluster size is 5 (no free brews at start).
        /// </summary>
        public void PopulateBoard(BoardModel board, int maxClusterAtSpawn = 5)
        {
            for (int r = 0; r < board.Height; r++)
            {
                for (int c = 0; c < board.Width; c++)
                {
                    var coord = new GridCoord(c, r);
                    if (board.GetCell(coord).IsEmpty)
                    {
                        var color = ChooseColorAvoidingClusters(board, coord);
                        board.SetCell(coord, CellContent.Token(color));
                    }
                }
            }

            BreakLargeClusters(board, maxClusterAtSpawn);
            EnsureMinimumValidClusters(board, minClusters: 3);
        }

        /// <summary>
        /// Generates a single random token color from the available pool.
        /// Used for refilling empty cells after gravity.
        /// </summary>
        public IngredientColor GenerateRandomColor()
        {
            return _availableColors[_rng.Next(_availableColors.Length)];
        }

        /// <summary>
        /// Fills a single empty cell with a random token.
        /// </summary>
        public void FillCell(BoardModel board, GridCoord coord)
        {
            board.SetCell(coord, CellContent.Token(GenerateRandomColor()));
        }

        /// <summary>
        /// True when the board has no token cluster ≥ 3 and no orb chain group ≥ 2.
        /// Stones do not create plays by themselves. Per core-mechanic.md §11.1.
        /// </summary>
        public bool IsDeadlocked(BoardModel board)
        {
            var detector = new ClusterDetector(board);
            if (detector.FindAllClusters(minClusterSize: 3).Count > 0)
                return false;
            if (detector.FindAllOrbChainGroups().Count > 0)
                return false;
            return true;
        }

        /// <summary>
        /// Attempts to recover from a deadlock by reshuffling token colors in place
        /// (orbs and stones stay fixed). Falls back to regenerating all tokens while
        /// preserving orbs/stones. Returns true if a playable board was produced.
        /// </summary>
        public bool TryRecoverDeadlock(BoardModel board)
        {
            for (int attempt = 0; attempt < MaxReshuffleAttempts; attempt++)
            {
                Reshuffle(board);
                if (!IsDeadlocked(board))
                    return true;
            }

            ClearTokensPreserveOrbsAndStones(board);
            PopulateBoard(board);
            return !IsDeadlocked(board);
        }

        private static void ClearTokensPreserveOrbsAndStones(BoardModel board)
        {
            for (int c = 0; c < board.Width; c++)
            {
                for (int r = 0; r < board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    if (board.GetCell(coord).IsToken)
                        board.SetCell(coord, CellContent.Empty);
                }
            }
        }

        private IngredientColor ChooseColorAvoidingClusters(BoardModel board, GridCoord coord)
        {
            var excluded = new HashSet<IngredientColor>();

            if (coord.Col >= 2)
            {
                var left1 = board.GetCell(new GridCoord(coord.Col - 1, coord.Row));
                var left2 = board.GetCell(new GridCoord(coord.Col - 2, coord.Row));
                if (left1.IsToken && left2.IsToken && left1.Color == left2.Color)
                    excluded.Add(left1.Color);
            }

            if (coord.Row >= 2)
            {
                var up1 = board.GetCell(new GridCoord(coord.Col, coord.Row - 1));
                var up2 = board.GetCell(new GridCoord(coord.Col, coord.Row - 2));
                if (up1.IsToken && up2.IsToken && up1.Color == up2.Color)
                    excluded.Add(up1.Color);
            }

            var candidates = new List<IngredientColor>();
            foreach (var color in _availableColors)
            {
                if (!excluded.Contains(color))
                    candidates.Add(color);
            }

            if (candidates.Count == 0)
                return _availableColors[_rng.Next(_availableColors.Length)];

            return candidates[_rng.Next(candidates.Count)];
        }

        private void BreakLargeClusters(BoardModel board, int maxSize)
        {
            var detector = new ClusterDetector(board);
            var clusters = detector.FindAllClusters(minClusterSize: maxSize + 1);
            int safety = 0;

            while (clusters.Count > 0 && safety < 100)
            {
                foreach (var cluster in clusters)
                {
                    var target = cluster[_rng.Next(cluster.Count)];
                    var currentColor = board.GetCell(target).Color;
                    var newColor = PickDifferentColor(currentColor);
                    board.SetCell(target, CellContent.Token(newColor));
                }

                clusters = detector.FindAllClusters(minClusterSize: maxSize + 1);
                safety++;
            }
        }

        private void EnsureMinimumValidClusters(BoardModel board, int minClusters)
        {
            var detector = new ClusterDetector(board);
            var validClusters = detector.FindAllClusters(minClusterSize: 3);
            int attempts = 0;

            while (validClusters.Count < minClusters && attempts < 100)
            {
                Reshuffle(board);
                validClusters = detector.FindAllClusters(minClusterSize: 3);
                attempts++;
            }
        }

        private void Reshuffle(BoardModel board)
        {
            var tokenCoords = new List<GridCoord>();
            var colors = new List<IngredientColor>();

            for (int c = 0; c < board.Width; c++)
            {
                for (int r = 0; r < board.Height; r++)
                {
                    var coord = new GridCoord(c, r);
                    var cell = board.GetCell(coord);
                    if (cell.IsToken)
                    {
                        tokenCoords.Add(coord);
                        colors.Add(cell.Color);
                    }
                }
            }

            for (int i = colors.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (colors[i], colors[j]) = (colors[j], colors[i]);
            }

            for (int i = 0; i < tokenCoords.Count; i++)
                board.SetCell(tokenCoords[i], CellContent.Token(colors[i]));
        }

        private IngredientColor PickDifferentColor(IngredientColor exclude)
        {
            IngredientColor result;
            do
            {
                result = _availableColors[_rng.Next(_availableColors.Length)];
            } while (result == exclude && _availableColors.Length > 1);
            return result;
        }
    }
}
