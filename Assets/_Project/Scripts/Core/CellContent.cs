namespace Brew.Core
{
    /// <summary>
    /// Represents the content of a single grid cell: Token, Orb, Stone, or Empty.
    /// Immutable value type — create new instances via static factory methods.
    /// </summary>
    public readonly struct CellContent
    {
        public CellContentType Type { get; }
        public IngredientColor Color { get; }
        public int TokenCount { get; }

        private CellContent(CellContentType type, IngredientColor color, int tokenCount)
        {
            Type = type;
            Color = color;
            TokenCount = tokenCount;
        }

        public bool IsEmpty => Type == CellContentType.Empty;
        public bool IsToken => Type == CellContentType.Token;
        public bool IsOrb => Type == CellContentType.Orb;
        public bool IsStone => Type == CellContentType.Stone;

        public static readonly CellContent Empty = new(CellContentType.Empty, default, 0);

        public static readonly CellContent Stone = new(CellContentType.Stone, default, 0);

        public static CellContent Token(IngredientColor color) =>
            new(CellContentType.Token, color, 0);

        public static CellContent Orb(IngredientColor color, int tokenCount) =>
            new(CellContentType.Orb, color, tokenCount);

        public CellContent WithTokenCount(int newCount) =>
            new(Type, Color, newCount);

        public override string ToString() => Type switch
        {
            CellContentType.Empty => "Empty",
            CellContentType.Token => $"Token({Color})",
            CellContentType.Orb => $"Orb({Color}, {TokenCount})",
            CellContentType.Stone => "Stone",
            _ => "Unknown"
        };
    }
}
