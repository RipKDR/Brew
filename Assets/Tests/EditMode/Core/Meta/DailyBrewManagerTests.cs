using System;
using Brew.Core.Meta;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class DailyBrewManagerTests
    {
        private static DailyStreakBonusDefinition[] DefaultBonuses() =>
            new[]
            {
                new DailyStreakBonusDefinition(3, 1.25f, 5),
                new DailyStreakBonusDefinition(5, 1.5f, 10),
                new DailyStreakBonusDefinition(7, 2.0f, 15)
            };

        private DateTime _clockUtc;
        private DailyBrewManager _daily;

        [SetUp]
        public void SetUp()
        {
            _clockUtc = new DateTime(2026, 4, 1, 15, 30, 0, DateTimeKind.Utc);
            _daily = new DailyBrewManager(100, 5, DefaultBonuses(), () => _clockUtc);
        }

        [Test]
        public void TryComplete_FirstTime_ReturnsBaseRewards()
        {
            var (e, g) = _daily.TryComplete(_clockUtc);
            Assert.AreEqual(100, e);
            Assert.AreEqual(5, g);
            Assert.IsTrue(_daily.IsCompletedToday);
        }

        [Test]
        public void TryComplete_AlreadyCompletedSameUtcDay_ReturnsZero()
        {
            _daily.TryComplete(_clockUtc);
            var (e, g) = _daily.TryComplete(_clockUtc);
            Assert.AreEqual(0, e);
            Assert.AreEqual(0, g);
        }

        [Test]
        public void TryComplete_ConsecutiveDays_IncrementsStreak()
        {
            _daily.TryComplete(_clockUtc);
            _clockUtc = _clockUtc.AddDays(1);

            int lastStreak = -1;
            _daily.OnDailyStreakChanged += s => lastStreak = s;

            _daily.TryComplete(_clockUtc);
            Assert.AreEqual(2, _daily.CurrentDailyStreak);
            Assert.AreEqual(2, lastStreak);
        }

        [Test]
        public void CheckNewDay_MissedCalendarDay_ResetsStreakBeforeComplete()
        {
            _daily.TryComplete(_clockUtc);
            Assert.AreEqual(1, _daily.CurrentDailyStreak);

            var skipDay = _clockUtc.AddDays(2);
            _daily.CheckNewDay(skipDay);
            Assert.AreEqual(0, _daily.CurrentDailyStreak);

            _daily.TryComplete(skipDay);
            Assert.AreEqual(1, _daily.CurrentDailyStreak);
        }

        [Test]
        public void TryComplete_ThirdStreak_AppliesEssenceMultiplierAndBonusGems()
        {
            _daily.TryComplete(_clockUtc);
            _clockUtc = _clockUtc.AddDays(1);
            _daily.TryComplete(_clockUtc);
            _clockUtc = _clockUtc.AddDays(1);

            var (e, g) = _daily.TryComplete(_clockUtc);
            Assert.AreEqual(125, e);
            Assert.AreEqual(10, g);
        }

        [Test]
        public void NextResetUtc_IsStartOfNextUtcDay()
        {
            var next = _daily.NextResetUtc;
            Assert.AreEqual(_clockUtc.Date.AddDays(1), next);
        }

        [Test]
        public void OnDailyBrewCompleted_FiresWithRewards()
        {
            int ee = 0, gg = 0;
            _daily.OnDailyBrewCompleted += (x, y) => { ee = x; gg = y; };
            _daily.TryComplete(_clockUtc);
            Assert.AreEqual(100, ee);
            Assert.AreEqual(5, gg);
        }
    }
}
