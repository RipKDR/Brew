using System;
using System.Collections.Generic;

namespace Brew.Core.Services
{
    public enum NotificationType { DailyBrew, StreakAtRisk, EventEndingSoon }

    public interface INotificationScheduler
    {
        void Schedule(string id, string title, string body, DateTime fireTime);
        void Cancel(string id);
        void CancelAll();
        bool HasPermission();
    }

    public sealed class NotificationManager
    {
        private readonly INotificationScheduler _scheduler;
        private readonly int _maxPerDay;
        private readonly int _rePromptCooldownDays;
        private int _scheduledToday;
        private DateTime _lastScheduledDate;
        private DateTime _lastPermissionDeniedDate;

        public event Action<NotificationType> OnNotificationScheduled;

        public NotificationManager(INotificationScheduler scheduler, int maxPerDay = 1, int rePromptCooldownDays = 7)
        {
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _maxPerDay = maxPerDay;
            _rePromptCooldownDays = rePromptCooldownDays;
            _lastScheduledDate = DateTime.MinValue;
            _lastPermissionDeniedDate = DateTime.MinValue;
        }

        public void ScheduleDailyBrewReminder(DateTime utcNow, bool dailyBrewCompleted, int hourOffset = 1)
        {
            if (dailyBrewCompleted) return;
            if (!CanSchedule(utcNow)) return;

            var fireTime = utcNow.Date.AddDays(1).AddHours(hourOffset);
            _scheduler.Schedule("daily_brew", "Your daily brew awaits!",
                "A fresh challenge is ready. Keep your streak alive!", fireTime);

            RecordScheduled(utcNow, NotificationType.DailyBrew);
        }

        /// <summary>
        /// Schedules a streak-at-risk notification. <paramref name="utcHour"/> is the hour
        /// on the UTC calendar day (0-23) at which the notification fires.
        /// </summary>
        public void ScheduleStreakAtRisk(DateTime utcNow, bool hasPlayedToday, int currentStreak, int utcHour = 20)
        {
            if (hasPlayedToday) return;
            if (currentStreak <= 0) return;
            if (!CanSchedule(utcNow)) return;

            var fireTime = utcNow.Date.AddHours(utcHour);
            if (fireTime <= utcNow)
                fireTime = fireTime.AddDays(1);

            _scheduler.Schedule("streak_at_risk", "Your streak is at risk!",
                "Play today to keep your win streak going.", fireTime);

            RecordScheduled(utcNow, NotificationType.StreakAtRisk);
        }

        public void ScheduleEventEndingSoon(DateTime utcNow, long eventEndTimestamp, bool allEventLevelsComplete, int hoursBeforeEnd = 24)
        {
            if (eventEndTimestamp <= 0) return;
            if (allEventLevelsComplete) return;
            if (!CanSchedule(utcNow)) return;

            var eventEnd = DateTimeOffset.FromUnixTimeSeconds(eventEndTimestamp).UtcDateTime;
            var fireTime = eventEnd.AddHours(-hoursBeforeEnd);

            if (fireTime <= utcNow) return;

            _scheduler.Schedule("event_ending_soon", "Event ending soon!",
                "Complete the remaining event levels before time runs out.", fireTime);

            RecordScheduled(utcNow, NotificationType.EventEndingSoon);
        }

        public bool CanSchedule(DateTime utcNow)
        {
            if (!_scheduler.HasPermission()) return false;

            if (_lastScheduledDate.Date == utcNow.Date && _scheduledToday >= _maxPerDay)
                return false;

            return true;
        }

        public bool ShouldRePromptPermission(DateTime utcNow)
        {
            if (_lastPermissionDeniedDate == DateTime.MinValue) return true;
            return (utcNow - _lastPermissionDeniedDate).TotalDays >= _rePromptCooldownDays;
        }

        public void RecordPermissionDenied(DateTime utcNow)
        {
            _lastPermissionDeniedDate = utcNow;
        }

        public void ResetDailyCount(DateTime utcNow)
        {
            _scheduledToday = 0;
            _lastScheduledDate = utcNow;
        }

        private void RecordScheduled(DateTime utcNow, NotificationType type)
        {
            if (_lastScheduledDate.Date != utcNow.Date)
            {
                _scheduledToday = 0;
                _lastScheduledDate = utcNow;
            }

            _scheduledToday++;
            OnNotificationScheduled?.Invoke(type);
        }
    }
}
