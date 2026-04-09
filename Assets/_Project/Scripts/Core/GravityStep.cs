namespace Brew.Core
{
    /// <summary>
    /// Describes a single cell movement during gravity for the presentation layer to animate.
    /// </summary>
    public readonly struct GravityStep
    {
        /// <summary>Where the content was before gravity.</summary>
        public GridCoord From { get; }

        /// <summary>Where the content landed after gravity.</summary>
        public GridCoord To { get; }

        /// <summary>Number of rows dropped.</summary>
        public int Distance { get; }

        /// <summary>True if this is a newly spawned token entering from above the grid.</summary>
        public bool IsNewSpawn { get; }

        public GravityStep(GridCoord from, GridCoord to, int distance, bool isNewSpawn)
        {
            From = from;
            To = to;
            Distance = distance;
            IsNewSpawn = isNewSpawn;
        }
    }
}
