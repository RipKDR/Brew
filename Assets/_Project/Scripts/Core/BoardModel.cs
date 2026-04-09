using System;

namespace Brew.Core
{
    /// <summary>
    /// Pure C# grid model. Stores cell contents for an NxM board.
    /// Origin (0,0) is top-left. Col increases right, Row increases down.
    /// No Unity dependencies.
    /// </summary>
    public sealed class BoardModel
    {
        private readonly CellContent[,] _cells;

        public int Width { get; }
        public int Height { get; }
        public int CellCount => Width * Height;

        public BoardModel(int width, int height)
        {
            if (width < 5 || width > 9)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be 5–9.");
            if (height < 5 || height > 11)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be 5–11.");

            Width = width;
            Height = height;
            _cells = new CellContent[width, height];

            for (int c = 0; c < width; c++)
                for (int r = 0; r < height; r++)
                    _cells[c, r] = CellContent.Empty;
        }

        public bool InBounds(GridCoord coord) =>
            coord.Col >= 0 && coord.Col < Width &&
            coord.Row >= 0 && coord.Row < Height;

        public CellContent GetCell(GridCoord coord)
        {
            ValidateBounds(coord);
            return _cells[coord.Col, coord.Row];
        }

        public bool TryGetCell(GridCoord coord, out CellContent content)
        {
            if (!InBounds(coord))
            {
                content = CellContent.Empty;
                return false;
            }
            content = _cells[coord.Col, coord.Row];
            return true;
        }

        public void SetCell(GridCoord coord, CellContent content)
        {
            ValidateBounds(coord);
            _cells[coord.Col, coord.Row] = content;
        }

        public void Clear()
        {
            for (int c = 0; c < Width; c++)
                for (int r = 0; r < Height; r++)
                    _cells[c, r] = CellContent.Empty;
        }

        /// <summary>
        /// Creates a deep copy of the current board state.
        /// </summary>
        public BoardModel Clone()
        {
            var clone = new BoardModel(Width, Height);
            for (int c = 0; c < Width; c++)
                for (int r = 0; r < Height; r++)
                    clone._cells[c, r] = _cells[c, r];
            return clone;
        }

        private void ValidateBounds(GridCoord coord)
        {
            if (!InBounds(coord))
                throw new ArgumentOutOfRangeException(
                    nameof(coord), coord, $"Coordinate out of bounds for {Width}x{Height} board.");
        }
    }
}
