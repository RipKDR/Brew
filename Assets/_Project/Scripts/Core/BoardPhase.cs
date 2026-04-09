namespace Brew.Core
{
    /// <summary>
    /// Board state machine phases. Transitions must follow the strict sequence:
    /// Idle -> PlayerInput -> Fusing -> Cascading -> Settling -> CheckBrew -> CheckWin -> Idle
    /// </summary>
    public enum BoardPhase
    {
        Idle = 0,
        PlayerInput = 1,
        Fusing = 2,
        Cascading = 3,
        Settling = 4,
        CheckBrew = 5,
        CheckWin = 6
    }
}
