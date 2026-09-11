using System;
using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Applies gravity (tokens/orbs fall down) and refills empty cells from the top.
    /// Stones are immovable anchors — gravity resolves independently in each column segment
    /// between stones (ADR 0004 / level-design-framework §12).
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
        /// Applies gravity across all columns: movable cells fall to fill gaps below,
        /// stopping on stones. Returns animation steps for the presentation layer.
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
        /// Fills all empty cells with new random tokens (including gaps above stones
        /// after segment gravity). Stones are left untouched.
        /// </summary>
        public List<GravityStep> RefillColumns()
        {
            var steps = new List<GravityStep>();

            for (int col = 0; col < _board.Width; col++)
            {
                RefillColumn(col, steps);
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

                if (cell.IsStone)
                {
                    // Stone is fixed. Next movable cells fall into the segment above it.
                    writeRow = readRow - 1;
                    continue;
                }

                if (cell.IsEmpty)
                    continue;

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

        private void RefillColumn(int col, List<GravityStep> steps)
        {
            // After segment gravity, empties sit at the top of each segment.
            // Fill every empty cell; stones remain.
            int emptyRun = 0;
            for (int row = 0; row < _board.Height; row++)
            {
                var coord = new GridCoord(col, row);
                var cell = _board.GetCell(coord);

                if (cell.IsStone)
                {
                    emptyRun = 0;
                    continue;
                }

                if (cell.IsEmpty)
                {
                    emptyRun++;
                    _spawner.FillCell(_board, coord);

                    var fromAbove = new GridCoord(col, row - emptyRun);
                    steps.Add(new GravityStep(fromAbove, coord, emptyRun, isNewSpawn: true));
                }
                else
                {
                    emptyRun = 0;
                }
            }
        }
    }
}
