using System;
using Brew.Core.Economy;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class CurrencyManagerTests
    {
        private CurrencyManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new CurrencyManager();
        }

        // ── Initial state ──────────────────────────────────

        [Test]
        public void InitialBalance_IsZero_ForBothCurrencies()
        {
            Assert.AreEqual(0, _manager.GetBalance(CurrencyType.Essence));
            Assert.AreEqual(0, _manager.GetBalance(CurrencyType.Gems));
        }

        // ── Add ────────────────────────────────────────────

        [Test]
        public void Add_IncreasesBalance()
        {
            _manager.Add(CurrencyType.Essence, 100, "level_complete");
            Assert.AreEqual(100, _manager.GetBalance(CurrencyType.Essence));
        }

        [Test]
        public void Add_MultipleCallsAccumulate()
        {
            _manager.Add(CurrencyType.Gems, 50, "daily_brew");
            _manager.Add(CurrencyType.Gems, 25, "milestone");
            Assert.AreEqual(75, _manager.GetBalance(CurrencyType.Gems));
        }

        [Test]
        public void Add_ZeroOrNegative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.Add(CurrencyType.Essence, 0, "test"));
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.Add(CurrencyType.Essence, -5, "test"));
        }

        [Test]
        public void Add_FiresEarnedAndBalanceEvents()
        {
            CurrencyType earnedType = default;
            int earnedAmount = 0;
            string earnedSource = null;
            int newBalance = -1;

            _manager.OnCurrencyEarned += (t, a, s) => { earnedType = t; earnedAmount = a; earnedSource = s; };
            _manager.OnBalanceChanged += (t, b) => { newBalance = b; };

            _manager.Add(CurrencyType.Essence, 80, "level_win");

            Assert.AreEqual(CurrencyType.Essence, earnedType);
            Assert.AreEqual(80, earnedAmount);
            Assert.AreEqual("level_win", earnedSource);
            Assert.AreEqual(80, newBalance);
        }

        // ── Spend ──────────────────────────────────────────

        [Test]
        public void Spend_DecreasesBalance_WhenSufficient()
        {
            _manager.Add(CurrencyType.Essence, 200, "test");
            bool result = _manager.Spend(CurrencyType.Essence, 50, "booster_shake");
            Assert.IsTrue(result);
            Assert.AreEqual(150, _manager.GetBalance(CurrencyType.Essence));
        }

        [Test]
        public void Spend_ReturnsFalse_WhenInsufficient()
        {
            _manager.Add(CurrencyType.Gems, 5, "test");
            bool result = _manager.Spend(CurrencyType.Gems, 10, "booster_catalyst");
            Assert.IsFalse(result);
            Assert.AreEqual(5, _manager.GetBalance(CurrencyType.Gems));
        }

        [Test]
        public void Spend_ReturnsFalse_WhenExactlyInsufficient()
        {
            _manager.Add(CurrencyType.Essence, 49, "test");
            Assert.IsFalse(_manager.Spend(CurrencyType.Essence, 50, "workshop"));
            Assert.AreEqual(49, _manager.GetBalance(CurrencyType.Essence));
        }

        [Test]
        public void Spend_Succeeds_WhenExactBalance()
        {
            _manager.Add(CurrencyType.Essence, 50, "test");
            Assert.IsTrue(_manager.Spend(CurrencyType.Essence, 50, "workshop"));
            Assert.AreEqual(0, _manager.GetBalance(CurrencyType.Essence));
        }

        [Test]
        public void Spend_ZeroOrNegative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.Spend(CurrencyType.Essence, 0, "test"));
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.Spend(CurrencyType.Essence, -1, "test"));
        }

        [Test]
        public void Spend_FiresSpentAndBalanceEvents()
        {
            _manager.Add(CurrencyType.Gems, 20, "test");

            CurrencyType spentType = default;
            int spentAmount = 0;
            string spentSink = null;

            _manager.OnCurrencySpent += (t, a, s) => { spentType = t; spentAmount = a; spentSink = s; };

            _manager.Spend(CurrencyType.Gems, 10, "catalyst");

            Assert.AreEqual(CurrencyType.Gems, spentType);
            Assert.AreEqual(10, spentAmount);
            Assert.AreEqual("catalyst", spentSink);
        }

        [Test]
        public void Spend_DoesNotFireEvents_WhenInsufficient()
        {
            bool eventFired = false;
            _manager.OnCurrencySpent += (_, _, _) => eventFired = true;

            _manager.Spend(CurrencyType.Essence, 100, "workshop");
            Assert.IsFalse(eventFired);
        }

        // ── CanAfford ──────────────────────────────────────

        [Test]
        public void CanAfford_ReturnsCorrectly()
        {
            _manager.Add(CurrencyType.Essence, 50, "test");
            Assert.IsTrue(_manager.CanAfford(CurrencyType.Essence, 50));
            Assert.IsTrue(_manager.CanAfford(CurrencyType.Essence, 49));
            Assert.IsFalse(_manager.CanAfford(CurrencyType.Essence, 51));
        }

        // ── SetBalance ─────────────────────────────────────

        [Test]
        public void SetBalance_OverwritesExistingBalance()
        {
            _manager.Add(CurrencyType.Essence, 100, "test");
            _manager.SetBalance(CurrencyType.Essence, 999);
            Assert.AreEqual(999, _manager.GetBalance(CurrencyType.Essence));
        }

        [Test]
        public void SetBalance_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _manager.SetBalance(CurrencyType.Essence, -1));
        }

        [Test]
        public void SetBalance_FiresBalanceChanged_NotEarnedOrSpent()
        {
            bool earnedFired = false;
            bool spentFired = false;
            bool balanceFired = false;

            _manager.OnCurrencyEarned += (_, _, _) => earnedFired = true;
            _manager.OnCurrencySpent += (_, _, _) => spentFired = true;
            _manager.OnBalanceChanged += (_, _) => balanceFired = true;

            _manager.SetBalance(CurrencyType.Gems, 500);

            Assert.IsFalse(earnedFired);
            Assert.IsFalse(spentFired);
            Assert.IsTrue(balanceFired);
        }

        // ── Rapid spend guard ──────────────────────────────

        [Test]
        public void RapidSpend_BalanceNeverGoesNegative()
        {
            _manager.Add(CurrencyType.Essence, 100, "test");

            int successes = 0;
            for (int i = 0; i < 10; i++)
            {
                if (_manager.Spend(CurrencyType.Essence, 50, "rapid_buy"))
                    successes++;
            }

            Assert.AreEqual(2, successes);
            Assert.AreEqual(0, _manager.GetBalance(CurrencyType.Essence));
        }

        // ── Currency independence ──────────────────────────

        [Test]
        public void Currencies_AreIndependent()
        {
            _manager.Add(CurrencyType.Essence, 500, "test");
            _manager.Add(CurrencyType.Gems, 10, "test");
            _manager.Spend(CurrencyType.Essence, 200, "workshop");

            Assert.AreEqual(300, _manager.GetBalance(CurrencyType.Essence));
            Assert.AreEqual(10, _manager.GetBalance(CurrencyType.Gems));
        }
    }
}
