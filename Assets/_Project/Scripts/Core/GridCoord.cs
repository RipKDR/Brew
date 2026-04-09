using System;

namespace Brew.Core
{
    /// <summary>
    /// Immutable grid coordinate. Col increases left-to-right, Row increases top-to-bottom.
    /// Origin (0,0) is the top-left cell.
    /// </summary>
    public readonly struct GridCoord : IEquatable<GridCoord>
    {
        public int Col { get; }
        public int Row { get; }

        public GridCoord(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public GridCoord Up => new(Col, Row - 1);
        public GridCoord Down => new(Col, Row + 1);
        public GridCoord Left => new(Col - 1, Row);
        public GridCoord Right => new(Col + 1, Row);

        public bool Equals(GridCoord other) => Col == other.Col && Row == other.Row;
        public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Col, Row);
        public override string ToString() => $"({Col}, {Row})";

        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
    }
}
