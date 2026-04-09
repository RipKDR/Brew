using System;

namespace Brew.Core
{
    /// <summary>
    /// Tracks remaining moves for a level. Decremented on each valid player tap.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class MoveTracker
    {
        private int _movesRemaining;

        public int MoveLimit { get; }
        public int MovesRemaining => _movesRemaining;
        public int MovesUsed => MoveLimit - _movesRemaining;
        public bool IsExhausted => _movesRemaining <= 0;

        public event Action<int> OnMovesChanged;

        public MoveTracker(int moveLimit)
        {
            if (moveLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(moveLimit), moveLimit, "Move limit must be positive.");
            MoveLimit = moveLimit;
            _movesRemaining = moveLimit;
        }

        /// <summary>
        /// Consumes one move. Returns true if successful, false if already exhausted.
        /// Per core-mechanic.md §9.3: decremented at the start of the fusion animation.
        /// </summary>
        public bool TryConsumeMove()
        {
            if (_movesRemaining <= 0)
                return false;

            _movesRemaining--;
            OnMovesChanged?.Invoke(_movesRemaining);
            return true;
        }

        /// <summary>
        /// Grants additional moves (e.g., from booster or rewarded ad).
        /// </summary>
        public void AddMoves(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must add a positive number of moves.");
            _movesRemaining += count;
            OnMovesChanged?.Invoke(_movesRemaining);
        }
    }
}
