using System;

namespace Brew.Core
{
    public enum DeadlockRecoveryKind
    {
        Skipped,
        NotDeadlocked,
        Reshuffled,
        Regenerated
    }

    public readonly struct DeadlockRecoveryResult
    {
        public DeadlockRecoveryKind Kind { get; }
        public bool Recovered { get; }
        public int ReshuffleAttempts { get; }

        public bool UsedFullRegeneration => Kind == DeadlockRecoveryKind.Regenerated;

        public bool Attempted =>
            Kind == DeadlockRecoveryKind.Reshuffled || Kind == DeadlockRecoveryKind.Regenerated;

        public DeadlockRecoveryResult(DeadlockRecoveryKind kind, bool recovered, int reshuffleAttempts)
        {
            Kind = kind;
            Recovered = recovered;
            ReshuffleAttempts = reshuffleAttempts;
        }

        public static readonly DeadlockRecoveryResult Skipped =
            new(DeadlockRecoveryKind.Skipped, false, 0);

        public static readonly DeadlockRecoveryResult AlreadyPlayable =
            new(DeadlockRecoveryKind.NotDeadlocked, false, 0);
    }

    /// <summary>
    /// Detects and recovers board deadlocks per core-mechanic.md §11.
    /// Shuffle retries and regeneration counts come from config — never a magic 10 in this class.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class DeadlockResolver
    {
        private readonly TokenSpawner _spawner;
        private readonly int _maxReshuffles;
        private readonly int _minClusterSize;

        public DeadlockResolver(TokenSpawner spawner, int maxReshuffles, int minClusterSize = 3)
        {
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            if (maxReshuffles < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxReshuffles),
                    maxReshuffles,
                    "Max reshuffles must be at least 1.");
            }

            if (minClusterSize < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minClusterSize),
                    minClusterSize,
                    "Min cluster size must be at least 2.");
            }

            _maxReshuffles = maxReshuffles;
            _minClusterSize = minClusterSize;
        }

        public int MaxReshuffles => _maxReshuffles;
        public int MinClusterSize => _minClusterSize;

        /// <summary>
        /// True when there is no token cluster ≥ min cluster size and no orb chain group ≥ 2.
        /// Stones do not create plays. Per core-mechanic.md §11.1.
        /// </summary>
        public bool IsDeadlocked(BoardModel board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            var detector = new ClusterDetector(board);
            if (detector.FindAllClusters(_minClusterSize).Count > 0)
                return false;
            if (detector.FindAllOrbChainGroups().Count > 0)
                return false;
            return true;
        }

        /// <summary>
        /// Recovers a deadlocked in-progress board: shuffle token colors in place (orbs/stones stay),
        /// retry up to <see cref="MaxReshuffles"/>, then regenerate tokens excluding orbs/stones.
        /// Win and lose skip recovery so a fail/win screen is not interrupted by a reshuffle.
        /// </summary>
        public DeadlockRecoveryResult TryRecover(BoardModel board, LevelOutcome outcome)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (outcome != LevelOutcome.InProgress)
                return DeadlockRecoveryResult.Skipped;

            if (!IsDeadlocked(board))
                return DeadlockRecoveryResult.AlreadyPlayable;

            for (int attempt = 1; attempt <= _maxReshuffles; attempt++)
            {
                _spawner.ReshuffleTokenColors(board);
                if (!IsDeadlocked(board))
                {
                    return new DeadlockRecoveryResult(
                        DeadlockRecoveryKind.Reshuffled, recovered: true, reshuffleAttempts: attempt);
                }
            }

            _spawner.RegenerateTokensPreservingOrbsAndStones(board);
            return new DeadlockRecoveryResult(
                DeadlockRecoveryKind.Regenerated,
                recovered: !IsDeadlocked(board),
                reshuffleAttempts: _maxReshuffles);
        }
    }
}
