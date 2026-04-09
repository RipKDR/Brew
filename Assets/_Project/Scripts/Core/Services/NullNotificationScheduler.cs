using System;

namespace Brew.Core.Services
{
    /// <summary>
    /// No-op scheduler for platforms that do not support local notifications
    /// (Editor, standalone builds). Prevents null-reference crashes while
    /// allowing all scheduling logic to run unchanged.
    /// </summary>
    public sealed class NullNotificationScheduler : INotificationScheduler
    {
        public void Schedule(string id, string title, string body, DateTime fireTime) { }
        public void Cancel(string id) { }
        public void CancelAll() { }
        public bool HasPermission() => false;
    }
}
