using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Applies gravity (tokens/orbs fall down) and refills empty cells from the top.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class CascadeResolver
    {
        private readonly BoardModel _board;
        private readonly TokenSpawner _spawner;

        public CascadeResolver(BoardModel board, TokenSpawner spawner)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        }

        /// <summary>
        /// Applies gravity across all columns: non-empty cells fall to fill gaps below.
        /// Returns the list of movements for the presentation layer to animate.
        /// Per core-mechanic.md §8.1, columns are processed independently.
        /// </summary>
        public List<GravityStep> ApplyGravity()
        {
            var steps = new List<GravityStep>();

            for (int col = 0; col < _board.Width; col++)
            {
                ApplyGravityToColumn(col, steps);
            }

            return steps;
        }

        /// <summary>
        /// Fills all empty cells at the top of each column with new random tokens.
        /// Returns the list of spawned entries for animation.
        /// Per core-mechanic.md §8.3, after gravity all empties are at the top.
        /// </summary>
        public List<GravityStep> RefillColumns()
        {
            var steps = new List<GravityStep>();

            for (int col = 0; col < _board.Width; col++)
            {
                int emptyCount = 0;
                for (int row = 0; row < _board.Height; row++)
                {
                    var coord = new GridCoord(col, row);
                    if (_board.GetCell(coord).IsEmpty)
                        emptyCount++;
                    else
                        break;
                }

                for (int row = 0; row < emptyCount; row++)
                {
                    var coord = new GridCoord(col, row);
                    _spawner.FillCell(_board, coord);

                    int enterDistance = emptyCount - row;
                    var fromAbove = new GridCoord(col, row - enterDistance);
                    steps.Add(new GravityStep(fromAbove, coord, enterDistance, isNewSpawn: true));
                }
            }

            return steps;
        }

        /// <summary>
        /// Applies gravity + refill in sequence and returns combined animation data.
        /// </summary>
        public (List<GravityStep> drops, List<GravityStep> spawns) ApplyGravityAndRefill()
        {
            var drops = ApplyGravity();
            var spawns = RefillColumns();
            return (drops, spawns);
        }

        private void ApplyGravityToColumn(int col, List<GravityStep> steps)
        {
            int writeRow = _board.Height - 1;

            for (int readRow = _board.Height - 1; readRow >= 0; readRow--)
            {
                var coord = new GridCoord(col, readRow);
                var cell = _board.GetCell(coord);

                if (!cell.IsEmpty)
                {
                    if (readRow != writeRow)
                    {
                        var target = new GridCoord(col, writeRow);
                        _board.SetCell(target, cell);
                        _board.SetCell(coord, CellContent.Empty);

                        int distance = writeRow - readRow;
                        steps.Add(new GravityStep(coord, target, distance, isNewSpawn: false));
                    }
                    writeRow--;
                }
            }
        }
    }
}
