using System;
using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class BoardStateMachineTests
    {
        private BoardStateMachine _sm;

        [SetUp]
        public void SetUp()
        {
            _sm = new BoardStateMachine();
        }

        [Test]
        public void InitialState_IsIdle()
        {
            Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase);
        }

        [Test]
        public void AcceptsInput_OnlyDuringPlayerInput()
        {
            Assert.IsFalse(_sm.AcceptsInput);
            _sm.TransitionTo(BoardPhase.PlayerInput);
            Assert.IsTrue(_sm.AcceptsInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            Assert.IsFalse(_sm.AcceptsInput);
        }

        [Test]
        public void FullSequence_CompletesWithoutError()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);

            Assert.AreEqual(BoardPhase.CheckWin, _sm.CurrentPhase);
        }

        [Test]
        public void CheckWin_TransitionsBackToIdle()
        {
            RunFullSequence();
            _sm.TransitionTo(BoardPhase.Idle);
            Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase);
        }

        [Test]
        public void InvalidTransition_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => _sm.TransitionTo(BoardPhase.Fusing));
        }

        [Test]
        public void SkippingState_Throws()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            Assert.Throws<InvalidOperationException>(
                () => _sm.TransitionTo(BoardPhase.Cascading));
        }

        [Test]
        public void TryTransitionTo_InvalidTransition_ReturnsFalse()
        {
            bool result = _sm.TryTransitionTo(BoardPhase.Fusing);
            Assert.IsFalse(result);
            Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase);
        }

        [Test]
        public void TryTransitionTo_ValidTransition_ReturnsTrue()
        {
            bool result = _sm.TryTransitionTo(BoardPhase.PlayerInput);
            Assert.IsTrue(result);
            Assert.AreEqual(BoardPhase.PlayerInput, _sm.CurrentPhase);
        }

        [Test]
        public void Advance_MovesToNextPhase()
        {
            _sm.Advance();
            Assert.AreEqual(BoardPhase.PlayerInput, _sm.CurrentPhase);

            _sm.Advance();
            Assert.AreEqual(BoardPhase.Fusing, _sm.CurrentPhase);
        }

        [Test]
        public void OnPhaseChanged_FiresWithCorrectArgs()
        {
            BoardPhase capturedFrom = BoardPhase.CheckWin;
            BoardPhase capturedTo = BoardPhase.CheckWin;

            _sm.OnPhaseChanged += (from, to) =>
            {
                capturedFrom = from;
                capturedTo = to;
            };

            _sm.TransitionTo(BoardPhase.PlayerInput);

            Assert.AreEqual(BoardPhase.Idle, capturedFrom);
            Assert.AreEqual(BoardPhase.PlayerInput, capturedTo);
        }

        [Test]
        public void Reset_ReturnsToIdle()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.Reset();

            Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase);
        }

        [Test]
        public void NoInputDuringResolution_FusingRejectsInput()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            Assert.IsFalse(_sm.AcceptsInput);
        }

        [Test]
        public void NoInputDuringResolution_CascadingRejectsInput()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            Assert.IsFalse(_sm.AcceptsInput);
        }

        [Test]
        public void NoInputDuringResolution_SettlingRejectsInput()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _sm.TransitionTo(BoardPhase.Settling);
            Assert.IsFalse(_sm.AcceptsInput);
        }

        [Test]
        public void MultipleLoops_WorkCorrectly()
        {
            for (int i = 0; i < 3; i++)
            {
                RunFullSequence();
                _sm.TransitionTo(BoardPhase.Idle);
            }

            Assert.AreEqual(BoardPhase.Idle, _sm.CurrentPhase);
        }

        [Test]
        public void IsValidTransition_StaticMethod_ReturnsCorrectly()
        {
            Assert.IsTrue(BoardStateMachine.IsValidTransition(BoardPhase.Idle, BoardPhase.PlayerInput));
            Assert.IsTrue(BoardStateMachine.IsValidTransition(BoardPhase.PlayerInput, BoardPhase.Fusing));
            Assert.IsTrue(BoardStateMachine.IsValidTransition(BoardPhase.CheckWin, BoardPhase.Idle));
            Assert.IsFalse(BoardStateMachine.IsValidTransition(BoardPhase.Idle, BoardPhase.Fusing));
            Assert.IsFalse(BoardStateMachine.IsValidTransition(BoardPhase.Fusing, BoardPhase.Idle));
        }

        private void RunFullSequence()
        {
            _sm.TransitionTo(BoardPhase.PlayerInput);
            _sm.TransitionTo(BoardPhase.Fusing);
            _sm.TransitionTo(BoardPhase.Cascading);
            _sm.TransitionTo(BoardPhase.Settling);
            _sm.TransitionTo(BoardPhase.CheckBrew);
            _sm.TransitionTo(BoardPhase.CheckWin);
        }
    }
}
