using System.Collections.Generic;

namespace Brew.Core
{
    /// <summary>
    /// Describes the result of a single fusion operation for the presentation layer to animate.
    /// </summary>
    public sealed class FusionResult
    {
        /// <summary>Coordinates of all tokens that were consumed.</summary>
        public IReadOnlyList<GridCoord> ConsumedCells { get; }

        /// <summary>Where the orb was placed (the tapped cell for player fusions).</summary>
        public GridCoord OrbCell { get; }

        /// <summary>The orb that was created.</summary>
        public CellContent CreatedOrb { get; }

        /// <summary>True if the created orb immediately brews (token_count >= brew threshold).</summary>
        public bool TriggeredBrew { get; }

        public FusionResult(
            IReadOnlyList<GridCoord> consumedCells,
            GridCoord orbCell,
            CellContent createdOrb,
            bool triggeredBrew)
        {
            ConsumedCells = consumedCells;
            OrbCell = orbCell;
            CreatedOrb = createdOrb;
            TriggeredBrew = triggeredBrew;
        }
    }
}
