using System;
using Brew.Core.Backend;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class FirebaseAuthManagerTests
    {
        private FirebaseAuthManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new FirebaseAuthManager();
        }

        [Test]
        public void DefaultConstructor_IsNotAuthenticated_AndUserIdIsNull()
        {
            Assert.IsFalse(_manager.IsAuthenticated);
            Assert.IsNull(_manager.UserId);
            Assert.IsFalse(_manager.IsAnonymous);
        }

        [Test]
        public void Constructor_WithInitialUserId_IsAuthenticatedAnonymous_AndUserIdMatches()
        {
            const string expectedId = "abc123initial";
            var auth = new FirebaseAuthManager(expectedId);

            Assert.IsTrue(auth.IsAuthenticated);
            Assert.IsTrue(auth.IsAnonymous);
            Assert.AreEqual(expectedId, auth.UserId);
        }

        [Test]
        public void SignInAnonymously_GeneratesThirtyTwoCharacterHexString()
        {
            _manager.SignInAnonymously();

            Assert.IsNotNull(_manager.UserId);
            Assert.AreEqual(32, _manager.UserId.Length);
            foreach (var c in _manager.UserId)
            {
                Assert.IsTrue((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
            }
        }

        [Test]
        public void SignInAnonymously_SetsIsAuthenticatedAndIsAnonymousTrue()
        {
            _manager.SignInAnonymously();

            Assert.IsTrue(_manager.IsAuthenticated);
            Assert.IsTrue(_manager.IsAnonymous);
        }

        [Test]
        public void SignInAnonymously_FiresOnAuthStateChangedWithTrueAndUserId()
        {
            bool? receivedAuthenticated = null;
            string receivedUserId = null;

            _manager.OnAuthStateChanged += (authenticated, userId) =>
            {
                receivedAuthenticated = authenticated;
                receivedUserId = userId;
            };

            _manager.SignInAnonymously();

            Assert.IsTrue(receivedAuthenticated.HasValue);
            Assert.IsTrue(receivedAuthenticated.Value);
            Assert.IsNotNull(receivedUserId);
            Assert.AreEqual(_manager.UserId, receivedUserId);
        }

        [Test]
        public void SignOut_ClearsState_AndFiresOnAuthStateChangedWithFalseAndEmptyString()
        {
            _manager.SignInAnonymously();

            bool? receivedAuthenticated = null;
            string receivedUserId = null;

            _manager.OnAuthStateChanged += (authenticated, userId) =>
            {
                receivedAuthenticated = authenticated;
                receivedUserId = userId;
            };

            _manager.SignOut();

            Assert.IsFalse(_manager.IsAuthenticated);
            Assert.IsNull(_manager.UserId);
            Assert.IsFalse(_manager.IsAnonymous);
            Assert.IsTrue(receivedAuthenticated.HasValue);
            Assert.IsFalse(receivedAuthenticated.Value);
            Assert.IsNotNull(receivedUserId);
            Assert.AreEqual(string.Empty, receivedUserId);
        }

        [Test]
        public void LinkAccount_FiresOnAccountLinkedWithProviderName()
        {
            string received = null;
            _manager.OnAccountLinked += provider => { received = provider; };

            _manager.LinkAccount("google");

            Assert.IsNotNull(received);
            Assert.AreEqual("google", received);
        }

        [Test]
        public void LinkAccount_WithNullProvider_FiresOnAccountLinkedWithEmptyString()
        {
            string received = null;
            _manager.OnAccountLinked += provider => { received = provider; };

            _manager.LinkAccount(null);

            Assert.IsNotNull(received);
            Assert.AreEqual(string.Empty, received);
        }

        [Test]
        public void MultipleSignInSignOutCycles_ProduceDifferentUserIds()
        {
            _manager.SignInAnonymously();
            var firstId = _manager.UserId;

            _manager.SignOut();
            _manager.SignInAnonymously();
            var secondId = _manager.UserId;

            _manager.SignOut();
            _manager.SignInAnonymously();
            var thirdId = _manager.UserId;

            Assert.IsFalse(firstId == secondId);
            Assert.IsFalse(secondId == thirdId);
            Assert.IsFalse(firstId == thirdId);
        }
    }
}
