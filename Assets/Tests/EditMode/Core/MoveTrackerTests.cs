using System;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class MoveTrackerTests
    {
        [Test]
        public void Constructor_ValidLimit_SetsProperties()
        {
            var tracker = new MoveTracker(25);
            Assert.AreEqual(25, tracker.MoveLimit);
            Assert.AreEqual(25, tracker.MovesRemaining);
            Assert.AreEqual(0, tracker.MovesUsed);
            Assert.IsFalse(tracker.IsExhausted);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_InvalidLimit_Throws(int limit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MoveTracker(limit));
        }

        [Test]
        public void TryConsumeMove_Decrements_ReturnsTrue()
        {
            var tracker = new MoveTracker(10);
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.AreEqual(9, tracker.MovesRemaining);
            Assert.AreEqual(1, tracker.MovesUsed);
        }

        [Test]
        public void TryConsumeMove_WhenExhausted_ReturnsFalse()
        {
            var tracker = new MoveTracker(1);
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.IsFalse(tracker.TryConsumeMove());
            Assert.AreEqual(0, tracker.MovesRemaining);
        }

        [Test]
        public void IsExhausted_TrueWhenAllMovesConsumed()
        {
            var tracker = new MoveTracker(3);
            tracker.TryConsumeMove();
            tracker.TryConsumeMove();
            Assert.IsFalse(tracker.IsExhausted);
            tracker.TryConsumeMove();
            Assert.IsTrue(tracker.IsExhausted);
        }

        [Test]
        public void AddMoves_IncreasesRemaining()
        {
            var tracker = new MoveTracker(5);
            tracker.TryConsumeMove();
            tracker.TryConsumeMove();
            Assert.AreEqual(3, tracker.MovesRemaining);

            tracker.AddMoves(5);
            Assert.AreEqual(8, tracker.MovesRemaining);
            Assert.IsFalse(tracker.IsExhausted);
        }

        [Test]
        public void AddMoves_AfterExhaustion_Recovers()
        {
            var tracker = new MoveTracker(1);
            tracker.TryConsumeMove();
            Assert.IsTrue(tracker.IsExhausted);

            tracker.AddMoves(5);
            Assert.AreEqual(5, tracker.MovesRemaining);
            Assert.IsFalse(tracker.IsExhausted);
            Assert.IsTrue(tracker.TryConsumeMove());
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void AddMoves_InvalidCount_Throws(int count)
        {
            var tracker = new MoveTracker(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.AddMoves(count));
        }

        [Test]
        public void OnMovesChanged_FiresOnConsume()
        {
            var tracker = new MoveTracker(10);
            int receivedValue = -1;
            tracker.OnMovesChanged += v => receivedValue = v;

            tracker.TryConsumeMove();
            Assert.AreEqual(9, receivedValue);
        }

        [Test]
        public void OnMovesChanged_FiresOnAdd()
        {
            var tracker = new MoveTracker(5);
            tracker.TryConsumeMove();

            int receivedValue = -1;
            tracker.OnMovesChanged += v => receivedValue = v;

            tracker.AddMoves(3);
            Assert.AreEqual(7, receivedValue);
        }

        [Test]
        public void OnMovesChanged_DoesNotFireWhenExhaustedAndConsumeAttempted()
        {
            var tracker = new MoveTracker(1);
            tracker.TryConsumeMove();

            bool fired = false;
            tracker.OnMovesChanged += _ => fired = true;

            tracker.TryConsumeMove();
            Assert.IsFalse(fired);
        }

        [Test]
        public void FullSequence_ConsumeAllThenAdd()
        {
            var tracker = new MoveTracker(3);
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.IsFalse(tracker.TryConsumeMove());
            Assert.AreEqual(3, tracker.MovesUsed);

            tracker.AddMoves(2);
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.IsTrue(tracker.TryConsumeMove());
            Assert.IsFalse(tracker.TryConsumeMove());
        }
    }
}
