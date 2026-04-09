using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Manages booster activation logic: Shake (reshuffle token colors), Catalyst (promote
    /// token to orb), and ExtraMoves. Tracks per-type charges and fires events on use.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class BoosterManager
    {
        private readonly Dictionary<BoosterType, int> _charges = new()
        {
            { BoosterType.Shake, int.MaxValue },
            { BoosterType.Catalyst, int.MaxValue },
            { BoosterType.ExtraMoves, int.MaxValue }
        };

        private readonly Random _rng;
        private bool _extraMovesUsedThisAttempt;

        public event Action<BoosterType> OnBoosterActivated;
        public event Action<BoosterType, int> OnChargesChanged;

        public BoosterManager(Random rng = null)
        {
            _rng = rng ?? new Random();
        }

        public int GetCharges(BoosterType type) => _charges[type];

        public bool TryUseCharge(BoosterType type)
        {
            if (_charges[type] <= 0)
                return false;

            if (_charges[type] != int.MaxValue)
            {
                _charges[type]--;
                OnChargesChanged?.Invoke(type, _charges[type]);
            }

            return true;
        }

        public void AddCharges(BoosterType type, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Must add a positive number of charges.");

            if (_charges[type] == int.MaxValue)
                return;

            _charges[type] += amount;
            OnChargesChanged?.Invoke(type, _charges[type]);
        }

        /// <summary>For tests and editor tooling — sets an exact charge count (use int.MaxValue for infinite).</summary>
        public void SetCharges(BoosterType type, int amount)
        {
            _charges[type] = amount;
            OnChargesChanged?.Invoke(type, _charges[type]);
        }

        /// <summary>
        /// Fisher-Yates shuffles colors among TOKEN cells only (orbs and empty unchanged).
        /// Retries up to 10 shuffles until a valid cluster exists; if still none, forces a horizontal triple.
        /// Does not run cascades. <paramref name="spawner"/> is reserved for future use.
        /// </summary>
        public void ActivateShake(BoardModel board, TokenSpawner spawner, ClusterDetector detector, int minClusterSize)
        {
            _ = spawner;

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

            if (tokenCoords.Count == 0)
                return;

            if (!TryUseCharge(BoosterType.Shake))
                return;

            var hasValidCluster = false;

            for (int attempt = 1; attempt <= 10; attempt++)
            {
                FisherYatesShuffle(colors);

                for (int i = 0; i < tokenCoords.Count; i++)
                    board.SetCell(tokenCoords[i], CellContent.Token(colors[i]));

                if (detector.FindAllClusters(minClusterSize).Count > 0)
                {
                    hasValidCluster = true;
                    break;
                }
            }

            if (!hasValidCluster)
                ForcePlaceClusterOfThree(board, minClusterSize);

            OnBoosterActivated?.Invoke(BoosterType.Shake);
        }

        /// <summary>
        /// Converts a token at <paramref name="target"/> to an orb (token_count = 1, same color).
        /// Returns false if the cell is not a token or a charge cannot be spent.
        /// </summary>
        public bool ActivateCatalyst(BoardModel board, GridCoord target)
        {
            var cell = board.GetCell(target);
            if (!cell.IsToken)
                return false;

            if (!TryUseCharge(BoosterType.Catalyst))
                return false;

            board.SetCell(target, CellContent.Orb(cell.Color, 1));
            OnBoosterActivated?.Invoke(BoosterType.Catalyst);
            return true;
        }

        public bool ActivateExtraMoves(MoveTracker moveTracker, int amount = 3)
        {
            if (_extraMovesUsedThisAttempt)
                return false;

            if (!TryUseCharge(BoosterType.ExtraMoves))
                return false;

            moveTracker.AddMoves(amount);
            _extraMovesUsedThisAttempt = true;

            OnBoosterActivated?.Invoke(BoosterType.ExtraMoves);
            return true;
        }

        public void ResetForNewAttempt()
        {
            _extraMovesUsedThisAttempt = false;
        }

        private void FisherYatesShuffle(List<IngredientColor> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Forces a contiguous horizontal run of same-colored tokens (at least 3, or <paramref name="minClusterSize"/> if larger).
        /// </summary>
        private static void ForcePlaceClusterOfThree(BoardModel board, int minClusterSize)
        {
            int run = Math.Max(3, minClusterSize);
            if (run > board.Width)
                run = board.Width;

            IngredientColor color = FindMostCommonTokenColor(board);

            for (int row = 0; row < board.Height; row++)
            {
                for (int col = 0; col <= board.Width - run; col++)
                {
                    bool allTokens = true;
                    for (int k = 0; k < run; k++)
                    {
                        var cell = board.GetCell(new GridCoord(col + k, row));
                        if (!cell.IsToken) { allTokens = false; break; }
                    }
                    if (!allTokens) continue;

                    for (int k = 0; k < run; k++)
                        board.SetCell(new GridCoord(col + k, row), CellContent.Token(color));
                    return;
                }
            }
        }

        private static IngredientColor FindMostCommonTokenColor(BoardModel board)
        {
            var counts = new Dictionary<IngredientColor, int>();
            for (int c = 0; c < board.Width; c++)
            {
                for (int r = 0; r < board.Height; r++)
                {
                    var cell = board.GetCell(new GridCoord(c, r));
                    if (!cell.IsToken) continue;
                    counts.TryGetValue(cell.Color, out int count);
                    counts[cell.Color] = count + 1;
                }
            }

            IngredientColor best = IngredientColor.Ember;
            int bestCount = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value > bestCount) { best = kvp.Key; bestCount = kvp.Value; }
            }
            return best;
        }
    }
}
