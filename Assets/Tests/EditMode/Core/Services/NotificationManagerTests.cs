using System;
using System.Collections.Generic;
using Brew.Core.Services;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core.Services
{
    [TestFixture]
    public class NotificationManagerTests
    {
        private MockScheduler _scheduler;
        private NotificationManager _manager;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _scheduler = new MockScheduler();
            _manager = new NotificationManager(_scheduler, maxPerDay: 1, rePromptCooldownDays: 7);
            _now = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        }

        // --- Daily Brew ---

        [Test]
        public void ScheduleDailyBrewReminder_WhenNotCompleted_SchedulesNotification()
        {
            _manager.ScheduleDailyBrewReminder(_now, dailyBrewCompleted: false, hourOffset: 1);

            Assert.AreEqual(1, _scheduler.Scheduled.Count);
            Assert.AreEqual("daily_brew", _scheduler.Scheduled[0].Id);
            Assert.AreEqual(_now.Date.AddDays(1).AddHours(1), _scheduler.Scheduled[0].FireTime);
        }

        [Test]
        public void ScheduleDailyBrewReminder_WhenCompleted_DoesNotSchedule()
        {
            _manager.ScheduleDailyBrewReminder(_now, dailyBrewCompleted: true);

            Assert.AreEqual(0, _scheduler.Scheduled.Count);
        }

        // --- Streak At Risk ---

        [Test]
        public void ScheduleStreakAtRisk_WhenStreakActiveAndNotPlayed_SchedulesNotification()
        {
            _manager.ScheduleStreakAtRisk(_now, hasPlayedToday: false, currentStreak: 3, localHour: 20);

            Assert.AreEqual(1, _scheduler.Scheduled.Count);
            Assert.AreEqual("streak_at_risk", _scheduler.Scheduled[0].Id);
            Assert.AreEqual(_now.Date.AddHours(20), _scheduler.Scheduled[0].FireTime);
        }

        [Test]
        public void ScheduleStreakAtRisk_WhenAlreadyPlayedToday_DoesNotSchedule()
        {
            _manager.ScheduleStreakAtRisk(_now, hasPlayedToday: true, currentStreak: 3);

            Assert.AreEqual(0, _scheduler.Scheduled.Count);
        }

        [Test]
        public void ScheduleStreakAtRisk_WhenStreakIsZero_DoesNotSchedule()
        {
            _manager.ScheduleStreakAtRisk(_now, hasPlayedToday: false, currentStreak: 0);

            Assert.AreEqual(0, _scheduler.Scheduled.Count);
        }

        // --- Event Ending Soon ---

        [Test]
        public void ScheduleEventEndingSoon_WhenEventActiveAndLevelsIncomplete_SchedulesNotification()
        {
            long eventEnd = new DateTimeOffset(_now.AddDays(3)).ToUnixTimeSeconds();
            _manager.ScheduleEventEndingSoon(_now, eventEnd, allEventLevelsComplete: false, hoursBeforeEnd: 24);

            Assert.AreEqual(1, _scheduler.Scheduled.Count);
            Assert.AreEqual("event_ending_soon", _scheduler.Scheduled[0].Id);

            var expectedFire = _now.AddDays(3).AddHours(-24);
            Assert.AreEqual(expectedFire, _scheduler.Scheduled[0].FireTime);
        }

        [Test]
        public void ScheduleEventEndingSoon_WhenAllLevelsComplete_DoesNotSchedule()
        {
            long eventEnd = new DateTimeOffset(_now.AddDays(3)).ToUnixTimeSeconds();
            _manager.ScheduleEventEndingSoon(_now, eventEnd, allEventLevelsComplete: true);

            Assert.AreEqual(0, _scheduler.Scheduled.Count);
        }

        [Test]
        public void ScheduleEventEndingSoon_WhenNoActiveEvent_DoesNotSchedule()
        {
            _manager.ScheduleEventEndingSoon(_now, eventEndTimestamp: 0, allEventLevelsComplete: false);

            Assert.AreEqual(0, _scheduler.Scheduled.Count);
        }

        [Test]
        public void ScheduleEventEndingSoon_WhenFireTimeAlreadyPassed_DoesNotSchedule()
        {
            long eventEnd = new DateTimeOffset(_now.AddHours(12)).ToUnixTimeSeconds();
            _manager.ScheduleEventEndingSoon(_now, eventEnd, allEventLevelsComplete: false, hoursBeforeEnd: 24);

            Assert.AreEqual(0, _scheduler.Scheduled.Count);
        }

        // --- Daily Cap ---

        [Test]
        public void DailyCap_PreventsSecondNotificationSameDay()
        {
            _manager.ScheduleDailyBrewReminder(_now, dailyBrewCompleted: false);
            Assert.AreEqual(1, _scheduler.Scheduled.Count);

            _manager.ScheduleStreakAtRisk(_now, hasPlayedToday: false, currentStreak: 5);
            Assert.AreEqual(1, _scheduler.Scheduled.Count);
        }

        [Test]
        public void ResetDailyCount_AllowsSchedulingAgain()
        {
            _manager.ScheduleDailyBrewReminder(_now, dailyBrewCompleted: false);
            Assert.AreEqual(1, _scheduler.Scheduled.Count);

            var nextDay = _now.AddDays(1);
            _manager.ResetDailyCount(nextDay);
            _manager.ScheduleStreakAtRisk(nextDay, hasPlayedToday: false, currentStreak: 5, localHour: 20);
            Assert.AreEqual(2, _scheduler.Scheduled.Count);
        }

        // --- Permission ---

        [Test]
        public void CanSchedule_ReturnsFalse_WhenNoPermission()
        {
            _scheduler.GrantPermission = false;

            Assert.IsFalse(_manager.CanSchedule(_now));
        }

        [Test]
        public void CanSchedule_ReturnsTrue_WhenPermissionGrantedAndUnderCap()
        {
            Assert.IsTrue(_manager.CanSchedule(_now));
        }

        // --- Re-prompt Cooldown ---

        [Test]
        public void RecordPermissionDenied_SetsCooldown()
        {
            _manager.RecordPermissionDenied(_now);

            Assert.IsFalse(_manager.ShouldRePromptPermission(_now.AddDays(1)));
        }

        [Test]
        public void ShouldRePromptPermission_ReturnsFalse_WithinCooldown()
        {
            _manager.RecordPermissionDenied(_now);

            Assert.IsFalse(_manager.ShouldRePromptPermission(_now.AddDays(6)));
        }

        [Test]
        public void ShouldRePromptPermission_ReturnsTrue_AfterCooldown()
        {
            _manager.RecordPermissionDenied(_now);

            Assert.IsTrue(_manager.ShouldRePromptPermission(_now.AddDays(7)));
        }

        [Test]
        public void ShouldRePromptPermission_ReturnsTrue_WhenNeverDenied()
        {
            Assert.IsTrue(_manager.ShouldRePromptPermission(_now));
        }

        // --- Event Callback ---

        [Test]
        public void OnNotificationScheduled_FiresWithCorrectType()
        {
            NotificationType? firedType = null;
            _manager.OnNotificationScheduled += t => firedType = t;

            _manager.ScheduleDailyBrewReminder(_now, dailyBrewCompleted: false);

            Assert.AreEqual(NotificationType.DailyBrew, firedType);
        }

        [Test]
        public void OnNotificationScheduled_FiresStreakAtRiskType()
        {
            NotificationType? firedType = null;
            _manager.OnNotificationScheduled += t => firedType = t;

            _manager.ScheduleStreakAtRisk(_now, hasPlayedToday: false, currentStreak: 2, localHour: 20);

            Assert.AreEqual(NotificationType.StreakAtRisk, firedType);
        }

        // --- Mock ---

        private class MockScheduler : INotificationScheduler
        {
            public bool GrantPermission = true;
            public readonly List<ScheduledEntry> Scheduled = new();
            public readonly List<string> Cancelled = new();
            public bool AllCancelled;

            public void Schedule(string id, string title, string body, DateTime fireTime)
            {
                Scheduled.Add(new ScheduledEntry(id, title, body, fireTime));
            }

            public void Cancel(string id) => Cancelled.Add(id);
            public void CancelAll() => AllCancelled = true;
            public bool HasPermission() => GrantPermission;

            public record ScheduledEntry(string Id, string Title, string Body, DateTime FireTime);
        }
    }
}
