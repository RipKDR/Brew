using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Clears stone blockers orthogonally adjacent to a brew cell.
    /// Pure C# — no Unity dependencies. Per level-design-framework §12 / ADR 0004.
    /// </summary>
    public static class StoneClearer
    {
        private static readonly GridCoord[] OrthogonalOffsets =
        {
            new(0, -1),
            new(0, 1),
            new(-1, 0),
            new(1, 0)
        };

        /// <summary>
        /// Destroys any stone in an orthogonal neighbor of <paramref name="brewCell"/>.
        /// Returns the coordinates that were cleared.
        /// </summary>
        public static List<GridCoord> ClearAdjacentStones(BoardModel board, GridCoord brewCell)
        {
            var cleared = new List<GridCoord>(4);

            foreach (var offset in OrthogonalOffsets)
            {
                var neighbor = new GridCoord(brewCell.Col + offset.Col, brewCell.Row + offset.Row);
                if (!board.TryGetCell(neighbor, out var content))
                    continue;

                if (!content.IsStone)
                    continue;

                board.SetCell(neighbor, CellContent.Empty);
                cleared.Add(neighbor);
            }

            return cleared;
        }
    }
}
