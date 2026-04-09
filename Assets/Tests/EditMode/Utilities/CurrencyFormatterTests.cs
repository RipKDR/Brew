using Brew.Utilities;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Utilities
{
    [TestFixture]
    public class CurrencyFormatterTests
    {
        // ── Exact range (0–999) ─────────────────────────────

        [Test]
        public void Format_Zero_ReturnsZero()
        {
            Assert.AreEqual("0", CurrencyFormatter.Format(0));
        }

        [Test]
        public void Format_SingleDigit_ReturnsExact()
        {
            Assert.AreEqual("5", CurrencyFormatter.Format(5));
        }

        [Test]
        public void Format_Hundreds_ReturnsExact()
        {
            Assert.AreEqual("847", CurrencyFormatter.Format(847));
        }

        [Test]
        public void Format_999_Returns999()
        {
            Assert.AreEqual("999", CurrencyFormatter.Format(999));
        }

        // ── K range with decimal (1,000–9,999) ─────────────

        [Test]
        public void Format_1000_Returns1Point0K()
        {
            Assert.AreEqual("1.0K", CurrencyFormatter.Format(1000));
        }

        [Test]
        public void Format_1500_Returns1Point5K()
        {
            Assert.AreEqual("1.5K", CurrencyFormatter.Format(1500));
        }

        [Test]
        public void Format_9999_Returns10Point0K()
        {
            Assert.AreEqual("10.0K", CurrencyFormatter.Format(9999));
        }

        // ── K range without decimal (10,000–999,999) ────────

        [Test]
        public void Format_10000_Returns10K()
        {
            Assert.AreEqual("10K", CurrencyFormatter.Format(10000));
        }

        [Test]
        public void Format_54321_Returns54K()
        {
            Assert.AreEqual("54K", CurrencyFormatter.Format(54321));
        }

        [Test]
        public void Format_999999_Returns999K()
        {
            Assert.AreEqual("999K", CurrencyFormatter.Format(999999));
        }

        // ── M range with decimal (1,000,000–9,999,999) ─────

        [Test]
        public void Format_1000000_Returns1Point0M()
        {
            Assert.AreEqual("1.0M", CurrencyFormatter.Format(1_000_000));
        }

        [Test]
        public void Format_2500000_Returns2Point5M()
        {
            Assert.AreEqual("2.5M", CurrencyFormatter.Format(2_500_000));
        }

        // ── M range without decimal (10,000,000+) ──────────

        [Test]
        public void Format_10000000_Returns10M()
        {
            Assert.AreEqual("10M", CurrencyFormatter.Format(10_000_000));
        }

        // ── Exact boundary transitions ────────────────────────

        [Test]
        public void Format_999_IsExact_1000_IsDecimalK()
        {
            Assert.AreEqual("999", CurrencyFormatter.Format(999));
            Assert.AreEqual("1.0K", CurrencyFormatter.Format(1000));
        }

        [Test]
        public void Format_9999_IsDecimalK_10000_IsIntegerK()
        {
            Assert.AreEqual("10.0K", CurrencyFormatter.Format(9999));
            Assert.AreEqual("10K", CurrencyFormatter.Format(10000));
        }

        [Test]
        public void Format_999999_IsIntegerK_1000000_IsDecimalM()
        {
            Assert.AreEqual("999K", CurrencyFormatter.Format(999999));
            Assert.AreEqual("1.0M", CurrencyFormatter.Format(1000000));
        }

        [Test]
        public void Format_9999999_IsDecimalM_10000000_IsIntegerM()
        {
            Assert.AreEqual("10.0M", CurrencyFormatter.Format(9999999));
            Assert.AreEqual("10M", CurrencyFormatter.Format(10000000));
        }

        [Test]
        public void Format_1999_RoundsToDecimalK()
        {
            Assert.AreEqual("2.0K", CurrencyFormatter.Format(1999));
        }

        // ── Edge cases ──────────────────────────────────────

        [Test]
        public void Format_Negative_ReturnsZero()
        {
            Assert.AreEqual("0", CurrencyFormatter.Format(-42));
        }

        [Test]
        public void Format_MaxValue_HandledGracefully()
        {
            string result = CurrencyFormatter.Format(int.MaxValue);
            Assert.AreEqual("2147M", result);
        }
    }
}
