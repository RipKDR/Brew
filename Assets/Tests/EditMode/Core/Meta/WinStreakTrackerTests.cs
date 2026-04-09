using Brew.Core.Meta;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class WinStreakTrackerTests
    {
        private static (int threshold, float multiplier)[] DefaultTiers() =>
            new[]
            {
                (1, 1.0f),
                (2, 1.25f),
                (3, 1.5f),
                (5, 2.0f),
                (8, 2.5f),
                (10, 3.0f)
            };

        private WinStreakTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new WinStreakTracker(DefaultTiers());
        }

        [Test]
        public void IncrementStreak_RaisesStreakAndUpdatesMultiplierTier()
        {
            Assert.AreEqual(0, _tracker.CurrentStreak);
            Assert.AreEqual(1.0f, _tracker.CurrentMultiplier);

            _tracker.IncrementStreak();
            Assert.AreEqual(1, _tracker.CurrentStreak);
            Assert.AreEqual(1.0f, _tracker.CurrentMultiplier);

            _tracker.IncrementStreak();
            Assert.AreEqual(2, _tracker.CurrentStreak);
            Assert.AreEqual(1.25f, _tracker.CurrentMultiplier);
        }

        [Test]
        public void ResetStreak_ClearsToZero_AndMultiplierReturnsToBase()
        {
            _tracker.SetStreak(10);
            Assert.AreEqual(3.0f, _tracker.CurrentMultiplier);

            _tracker.ResetStreak();
            Assert.AreEqual(0, _tracker.CurrentStreak);
            Assert.AreEqual(1.0f, _tracker.CurrentMultiplier);
        }

        [Test]
        public void Boundary_AtThresholdFive_UsesTwoPointZeroMultiplier()
        {
            _tracker.SetStreak(4);
            Assert.AreEqual(1.5f, _tracker.CurrentMultiplier);

            _tracker.SetStreak(5);
            Assert.AreEqual(2.0f, _tracker.CurrentMultiplier);
        }

        [Test]
        public void OnStreakChanged_FiresWithCurrentValues()
        {
            int s = -1;
            float m = -1f;
            _tracker.OnStreakChanged += (ns, nm) => { s = ns; m = nm; };

            _tracker.IncrementStreak();
            Assert.AreEqual(1, s);
            Assert.AreEqual(1.0f, m);
        }

        [Test]
        public void SetStreak_SameValue_DoesNotFireEvent()
        {
            int fires = 0;
            _tracker.OnStreakChanged += (_, _) => fires++;
            _tracker.SetStreak(3);
            _tracker.SetStreak(3);
            Assert.AreEqual(1, fires);
        }
    }
}
