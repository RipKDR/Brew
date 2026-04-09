using Brew.Core;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Core
{
    [TestFixture]
    public class ScoreCalculatorTests
    {
        private ScoreCalculator _calc;

        [SetUp]
        public void SetUp()
        {
            _calc = new ScoreCalculator();
        }

        [Test]
        public void InitialScore_IsZero()
        {
            Assert.AreEqual(0, _calc.TotalScore);
        }

        [Test]
        public void OnFusion_BaseFormula_ClusterSizeTimes10()
        {
            _calc.ResetForNewMove();
            int points = _calc.OnFusion(5);
            Assert.AreEqual(50, points);
            Assert.AreEqual(50, _calc.TotalScore);
        }

        [Test]
        public void OnFusion_MinCluster3_Awards30()
        {
            _calc.ResetForNewMove();
            Assert.AreEqual(30, _calc.OnFusion(3));
        }

        [Test]
        public void ChainMultiplier_IncreasesBy05PerStep()
        {
            _calc.ResetForNewMove();
            _calc.OnFusion(4); // 40 at 1.0x

            _calc.OnChainStep(); // now 1.5x
            int points = _calc.OnFusion(3);
            Assert.AreEqual(45, points); // 3*10*1.5 = 45

            _calc.OnChainStep(); // now 2.0x
            points = _calc.OnFusion(3);
            Assert.AreEqual(60, points); // 3*10*2.0 = 60
        }

        [Test]
        public void CascadeMultiplier_IncreasesBy05PerWave()
        {
            _calc.ResetForNewMove();
            _calc.OnFusion(4); // 40 at 1.0x

            _calc.OnCascadeWave(); // now 1.5x
            int points = _calc.OnFusion(4);
            Assert.AreEqual(60, points); // 4*10*1.5 = 60

            _calc.OnCascadeWave(); // now 2.0x
            points = _calc.OnFusion(4);
            Assert.AreEqual(80, points); // 4*10*2.0 = 80
        }

        [Test]
        public void MultiplierStacking_ChainTimesCascade()
        {
            _calc.ResetForNewMove();

            _calc.OnChainStep();   // chain = 1.5
            _calc.OnChainStep();   // chain = 2.0
            _calc.OnCascadeWave(); // cascade = 1.5

            int points = _calc.OnFusion(4);
            // 4 * 10 * 2.0 * 1.5 = 120
            Assert.AreEqual(120, points);
        }

        [Test]
        public void ResetForNewMove_ClearsMultipliers()
        {
            _calc.ResetForNewMove();
            _calc.OnChainStep();
            _calc.OnCascadeWave();

            _calc.ResetForNewMove();
            int points = _calc.OnFusion(4);
            Assert.AreEqual(40, points); // back to 1.0x * 1.0x
        }

        [Test]
        public void ResetForNewMove_DoesNotClearTotalScore()
        {
            _calc.ResetForNewMove();
            _calc.OnFusion(5); // 50
            _calc.ResetForNewMove();
            _calc.OnFusion(3); // 30
            Assert.AreEqual(80, _calc.TotalScore);
        }

        [Test]
        public void OnBrew_TargetColor_Awards200()
        {
            _calc.ResetForNewMove();
            int points = _calc.OnBrew(isTargetColor: true);
            Assert.AreEqual(200, points);
            Assert.AreEqual(200, _calc.TotalScore);
        }

        [Test]
        public void OnBrew_NonTarget_Awards100()
        {
            _calc.ResetForNewMove();
            int points = _calc.OnBrew(isTargetColor: false);
            Assert.AreEqual(100, points);
        }

        [Test]
        public void OnBrew_MultiBrew_AwardsExtra100()
        {
            _calc.ResetForNewMove();

            int first = _calc.OnBrew(isTargetColor: true);
            Assert.AreEqual(200, first);

            int second = _calc.OnBrew(isTargetColor: true);
            Assert.AreEqual(300, second); // 200 + 100 multi-brew bonus

            int third = _calc.OnBrew(isTargetColor: false);
            Assert.AreEqual(200, third); // 100 + 100 multi-brew bonus
        }

        [Test]
        public void OnBrew_MultiBrewResetsPerMove()
        {
            _calc.ResetForNewMove();
            _calc.OnBrew(true);
            _calc.OnBrew(true); // multi-brew bonus

            _calc.ResetForNewMove();
            int points = _calc.OnBrew(true);
            Assert.AreEqual(200, points); // no multi-brew bonus after reset
        }

        [Test]
        public void BrewScore_NotAffectedByMultipliers()
        {
            _calc.ResetForNewMove();
            _calc.OnChainStep();   // chain = 1.5
            _calc.OnCascadeWave(); // cascade = 1.5

            int points = _calc.OnBrew(isTargetColor: true);
            Assert.AreEqual(200, points); // flat, not multiplied
        }

        [Test]
        public void CalculateEndOfLevelBonus_50PerMove()
        {
            int bonus = _calc.CalculateEndOfLevelBonus(7);
            Assert.AreEqual(350, bonus);
            Assert.AreEqual(350, _calc.TotalScore);
        }

        [Test]
        public void CalculateEndOfLevelBonus_ZeroMovesRemaining()
        {
            int bonus = _calc.CalculateEndOfLevelBonus(0);
            Assert.AreEqual(0, bonus);
        }

        [Test]
        public void OnScoreChanged_FiresOnFusion()
        {
            int received = -1;
            _calc.OnScoreChanged += v => received = v;

            _calc.ResetForNewMove();
            _calc.OnFusion(4);
            Assert.AreEqual(40, received);
        }

        [Test]
        public void OnScoreChanged_FiresOnBrew()
        {
            int received = -1;
            _calc.OnScoreChanged += v => received = v;

            _calc.ResetForNewMove();
            _calc.OnBrew(true);
            Assert.AreEqual(200, received);
        }

        [Test]
        public void CalculateStars_ScoreBelowAll_Returns0()
        {
            Assert.AreEqual(0, ScoreCalculator.CalculateStars(500, new[] { 1000, 3000, 6000 }));
        }

        [Test]
        public void CalculateStars_ScoreAt1Star_Returns1()
        {
            Assert.AreEqual(1, ScoreCalculator.CalculateStars(1000, new[] { 1000, 3000, 6000 }));
            Assert.AreEqual(1, ScoreCalculator.CalculateStars(2999, new[] { 1000, 3000, 6000 }));
        }

        [Test]
        public void CalculateStars_ScoreAt2Stars_Returns2()
        {
            Assert.AreEqual(2, ScoreCalculator.CalculateStars(3000, new[] { 1000, 3000, 6000 }));
            Assert.AreEqual(2, ScoreCalculator.CalculateStars(5999, new[] { 1000, 3000, 6000 }));
        }

        [Test]
        public void CalculateStars_ScoreAt3Stars_Returns3()
        {
            Assert.AreEqual(3, ScoreCalculator.CalculateStars(6000, new[] { 1000, 3000, 6000 }));
            Assert.AreEqual(3, ScoreCalculator.CalculateStars(99999, new[] { 1000, 3000, 6000 }));
        }

        [Test]
        public void FullResolution_FusionChainBrewCascade_AccumulatesCorrectly()
        {
            _calc.ResetForNewMove();

            // Player fuses cluster of 5: 5*10*1.0*1.0 = 50
            _calc.OnFusion(5);

            // Chain step occurs
            _calc.OnChainStep(); // chain = 1.5

            // Another fusion from chain: 3*10*1.5*1.0 = 45
            _calc.OnFusion(3);

            // Brew target: +200
            _calc.OnBrew(true);

            // Cascade wave
            _calc.OnCascadeWave(); // cascade = 1.5

            // Cascade auto-fuse: 4*10*1.5*1.5 = 90
            _calc.OnFusion(4);

            // Total: 50 + 45 + 200 + 90 = 385
            Assert.AreEqual(385, _calc.TotalScore);
        }
    }
}
