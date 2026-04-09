using System;
using Brew.Core.Ads;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class AdManagerTests
    {
        private static readonly DateTime Day1 = new(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime Day2 = new(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc);

        [Test]
        public void CanShowRewarded_AfterDailyCap_ReturnsFalse()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(ads.CanShowRewarded(AdPlacement.FailRecovery), $"Expected slot {i} available.");
                ads.RecordRewardedAdWatched(AdPlacement.FailRecovery);
            }

            Assert.IsFalse(ads.CanShowRewarded(AdPlacement.FailRecovery));
        }

        [Test]
        public void ShouldShowInterstitial_AfterThirdNonTutorialWin_ReturnsTrue()
        {
            var ads = new AdManager(interstitialFrequency: 3);
            ads.CheckNewDay(Day1);

            ads.RecordLevelWin(isTutorialLevel: false);
            Assert.IsFalse(ads.ShouldShowInterstitial());
            ads.RecordLevelWin(isTutorialLevel: false);
            Assert.IsFalse(ads.ShouldShowInterstitial());
            ads.RecordLevelWin(isTutorialLevel: false);
            Assert.IsTrue(ads.ShouldShowInterstitial());
        }

        [Test]
        public void ShouldShowInterstitial_NoAdsPass_ReturnsFalse()
        {
            var ads = new AdManager(interstitialFrequency: 3, hasNoAdsPass: () => true);
            ads.CheckNewDay(Day1);

            for (int i = 0; i < 5; i++)
                ads.RecordLevelWin(isTutorialLevel: false);

            Assert.IsFalse(ads.ShouldShowInterstitial());
            Assert.AreEqual(0, ads.LevelWinsSinceLastInterstitial);
        }

        [Test]
        public void CheckNewDay_ResetsRewardedAndInterstitialCounters()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            ads.RecordRewardedAdWatched(AdPlacement.FailRecovery);
            ads.RecordRewardedAdWatched(AdPlacement.FailRecovery);
            ads.RecordRewardedAdWatched(AdPlacement.FreeDailyBooster);
            ads.RecordLevelWin(isTutorialLevel: false);
            ads.RecordLevelWin(isTutorialLevel: false);

            Assert.IsFalse(ads.CanShowPlacement(AdPlacement.FreeDailyBooster));

            ads.CheckNewDay(Day2);

            Assert.AreEqual(0, ads.RewardedAdsWatchedToday);
            Assert.AreEqual(0, ads.LevelWinsSinceLastInterstitial);
            Assert.IsTrue(ads.CanShowPlacement(AdPlacement.FreeDailyBooster));
        }

        [Test]
        public void FreeDailyBooster_CanShowPlacement_OncePerDay()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            Assert.IsTrue(ads.CanShowPlacement(AdPlacement.FreeDailyBooster));
            ads.RecordRewardedAdWatched(AdPlacement.FreeDailyBooster);
            Assert.IsFalse(ads.CanShowPlacement(AdPlacement.FreeDailyBooster));
        }

        [Test]
        public void StreakProtection_CanShowPlacement_TwicePerDay()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            Assert.IsTrue(ads.CanShowPlacement(AdPlacement.StreakProtection));
            ads.RecordRewardedAdWatched(AdPlacement.StreakProtection);
            Assert.IsTrue(ads.CanShowPlacement(AdPlacement.StreakProtection));
            ads.RecordRewardedAdWatched(AdPlacement.StreakProtection);
            Assert.IsFalse(ads.CanShowPlacement(AdPlacement.StreakProtection));
        }

        [Test]
        public void OnRewardedAdCompleted_FiresWithCorrectPlacement()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);
            AdPlacement? firedPlacement = null;
            ads.OnRewardedAdCompleted += p => firedPlacement = p;

            ads.RecordRewardedAdWatched(AdPlacement.FailRecovery);

            Assert.AreEqual(AdPlacement.FailRecovery, firedPlacement);
        }

        [Test]
        public void OnInterstitialShown_FiresOnRecord()
        {
            var ads = new AdManager(interstitialFrequency: 1);
            ads.CheckNewDay(Day1);
            bool fired = false;
            ads.OnInterstitialShown += () => fired = true;

            ads.RecordLevelWin(isTutorialLevel: false);
            ads.RecordInterstitialShown();

            Assert.IsTrue(fired);
            Assert.AreEqual(0, ads.LevelWinsSinceLastInterstitial);
        }

        [Test]
        public void RecordLevelWin_TutorialLevel_DoesNotAdvanceInterstitialCounter()
        {
            var ads = new AdManager(interstitialFrequency: 3);
            ads.CheckNewDay(Day1);

            ads.RecordLevelWin(isTutorialLevel: true);
            ads.RecordLevelWin(isTutorialLevel: true);
            ads.RecordLevelWin(isTutorialLevel: true);

            Assert.AreEqual(0, ads.LevelWinsSinceLastInterstitial);
            Assert.IsFalse(ads.ShouldShowInterstitial());
        }

        [Test]
        public void CanShowRewarded_InterstitialPlacement_ReturnsFalse()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            Assert.IsFalse(ads.CanShowRewarded(AdPlacement.Interstitial));
        }

        [Test]
        public void RecordRewardedAdWatched_InterstitialPlacement_IsNoOp()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            ads.RecordRewardedAdWatched(AdPlacement.Interstitial);

            Assert.AreEqual(0, ads.RewardedAdsWatchedToday);
        }

        [Test]
        public void Constructor_NegativeMaxRewarded_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdManager(maxRewardedPerDay: -1));
        }

        [Test]
        public void Constructor_ZeroInterstitialFrequency_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdManager(interstitialFrequency: 0));
        }

        [Test]
        public void CanShowPlacement_DoublePotionReward_ReturnsTrue()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            Assert.IsTrue(ads.CanShowPlacement(AdPlacement.DoublePotionReward));
        }

        [Test]
        public void CanShowPlacement_DailyBrewDoubler_ReturnsTrue()
        {
            var ads = new AdManager();
            ads.CheckNewDay(Day1);

            Assert.IsTrue(ads.CanShowPlacement(AdPlacement.DailyBrewDoubler));
        }

        [Test]
        public void ShouldShowInterstitial_AfterTutorialWin_ReturnsFalse()
        {
            var ads = new AdManager(interstitialFrequency: 1);
            ads.CheckNewDay(Day1);

            ads.RecordLevelWin(isTutorialLevel: false);
            ads.RecordLevelWin(isTutorialLevel: true);

            Assert.IsFalse(ads.ShouldShowInterstitial());
        }
    }
}
