using System;

namespace Brew.Core
{
    /// <summary>
    /// Enforces the strict board state sequence:
    /// Idle -> PlayerInput -> Fusing -> Cascading -> Settling -> CheckBrew -> CheckWin -> Idle
    /// No states can be skipped. Input is only accepted during PlayerInput.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class BoardStateMachine
    {
        public BoardPhase CurrentPhase { get; private set; } = BoardPhase.Idle;
        public bool AcceptsInput => CurrentPhase == BoardPhase.PlayerInput;

        public event Action<BoardPhase, BoardPhase> OnPhaseChanged;

        public void TransitionTo(BoardPhase target)
        {
            if (!IsValidTransition(CurrentPhase, target))
                throw new InvalidOperationException(
                    $"Invalid board phase transition: {CurrentPhase} -> {target}. " +
                    $"Expected next phase: {GetNextPhase(CurrentPhase)}.");

            var previous = CurrentPhase;
            CurrentPhase = target;
            OnPhaseChanged?.Invoke(previous, target);
        }

        /// <summary>
        /// Attempts a transition. Returns false if the transition is invalid.
        /// </summary>
        public bool TryTransitionTo(BoardPhase target)
        {
            if (!IsValidTransition(CurrentPhase, target))
                return false;

            var previous = CurrentPhase;
            CurrentPhase = target;
            OnPhaseChanged?.Invoke(previous, target);
            return true;
        }

        /// <summary>
        /// Advances to the next phase in the fixed sequence.
        /// </summary>
        public void Advance()
        {
            TransitionTo(GetNextPhase(CurrentPhase));
        }

        /// <summary>
        /// Resets to Idle. Only valid from CheckWin or as an emergency reset.
        /// </summary>
        public void Reset()
        {
            var previous = CurrentPhase;
            CurrentPhase = BoardPhase.Idle;
            if (previous != BoardPhase.Idle)
                OnPhaseChanged?.Invoke(previous, BoardPhase.Idle);
        }

        public static bool IsValidTransition(BoardPhase from, BoardPhase to)
        {
            return to == GetNextPhase(from);
        }

        public static BoardPhase GetNextPhase(BoardPhase current) => current switch
        {
            BoardPhase.Idle => BoardPhase.PlayerInput,
            BoardPhase.PlayerInput => BoardPhase.Fusing,
            BoardPhase.Fusing => BoardPhase.Cascading,
            BoardPhase.Cascading => BoardPhase.Settling,
            BoardPhase.Settling => BoardPhase.CheckBrew,
            BoardPhase.CheckBrew => BoardPhase.CheckWin,
            BoardPhase.CheckWin => BoardPhase.Idle,
            _ => throw new ArgumentOutOfRangeException(nameof(current), current, "Unknown board phase.")
        };
    }
}
