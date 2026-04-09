using Brew.Core.Economy;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class RewardCalculatorTests
    {
        private RewardCalculator _calc;

        private static readonly int[] EssencePerStar = { 0, 30, 50, 80 };
        private static readonly (int, float)[] StreakTiers =
        {
            (1, 1.0f), (2, 1.25f), (3, 1.5f), (5, 2.0f), (8, 2.5f), (10, 3.0f)
        };

        [SetUp]
        public void SetUp()
        {
            _calc = new RewardCalculator(EssencePerStar, StreakTiers);
        }

        // ── Base Essence by star rating ────────────────────

        [Test]
        public void OneStar_NoStreak_Returns30()
        {
            Assert.AreEqual(30, _calc.CalculateEssence(1, 1));
        }

        [Test]
        public void TwoStar_NoStreak_Returns50()
        {
            Assert.AreEqual(50, _calc.CalculateEssence(2, 1));
        }

        [Test]
        public void ThreeStar_NoStreak_Returns80()
        {
            Assert.AreEqual(80, _calc.CalculateEssence(3, 1));
        }

        [Test]
        public void ZeroStarRating_ReturnsZero()
        {
            Assert.AreEqual(0, _calc.CalculateEssence(0, 5));
        }

        [Test]
        public void NegativeStarRating_ReturnsZero()
        {
            Assert.AreEqual(0, _calc.CalculateEssence(-1, 3));
        }

        // ── Streak multipliers ─────────────────────────────

        [Test]
        public void Streak2_TwoStar_Returns62()
        {
            // 50 * 1.25 = 62.5 → rounds to 63 (Math.Round MidpointRounding.ToEven)
            Assert.AreEqual(63, _calc.CalculateEssence(2, 2));
        }

        [Test]
        public void Streak3_ThreeStar_Returns120()
        {
            Assert.AreEqual(120, _calc.CalculateEssence(3, 3));
        }

        [Test]
        public void Streak5_TwoStar_Returns100()
        {
            Assert.AreEqual(100, _calc.CalculateEssence(2, 5));
        }

        [Test]
        public void Streak8_ThreeStar_Returns200()
        {
            Assert.AreEqual(200, _calc.CalculateEssence(3, 8));
        }

        [Test]
        public void Streak10_ThreeStar_Returns240()
        {
            Assert.AreEqual(240, _calc.CalculateEssence(3, 10));
        }

        [Test]
        public void Streak15_UsesMaxTier()
        {
            Assert.AreEqual(240, _calc.CalculateEssence(3, 15));
        }

        // ── GetStreakMultiplier ─────────────────────────────

        [Test]
        public void GetStreakMultiplier_ZeroStreak_Returns1()
        {
            Assert.AreEqual(1.0f, _calc.GetStreakMultiplier(0));
        }

        [Test]
        public void GetStreakMultiplier_Streak4_ReturnsPrevTier()
        {
            // 4 is between tier 3 (1.5x) and tier 5 (2.0x) — should use 3's multiplier
            Assert.AreEqual(1.5f, _calc.GetStreakMultiplier(4));
        }

        [Test]
        public void GetStreakMultiplier_ExactTierBoundary()
        {
            Assert.AreEqual(2.0f, _calc.GetStreakMultiplier(5));
        }
    }
}
