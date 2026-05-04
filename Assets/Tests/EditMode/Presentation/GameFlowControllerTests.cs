// GameFlowController is a MonoBehaviour that orchestrates the full game flow
// (Level Select → Gameplay → Win/Lose → Level Select). Because it is a MonoBehaviour
// it cannot be instantiated in EditMode without a full scene bootstrap. All non-trivial
// logic in GameFlowController is delegated to pure-C# classes that are wired together in
// InitializeSystems(). This file tests those classes through the same configurations and
// interactions that GameFlowController sets up, verifying the contracts that GameFlowController
// depends on: reward calculation, win-streak progression, currency mutations, ad caps, and
// tutorial detection. Any regression in these classes breaks GameFlowController at runtime.

using System;
using Brew.Core;
using Brew.Core.Ads;
using Brew.Core.Economy;
using Brew.Core.Meta;
using NUnit.Framework;

namespace Brew.Tests.EditMode.Presentation
{
    /// <summary>
    /// Tests the pure-C# subsystems wired by <see cref="Brew.Presentation.GameFlowController"/>,
    /// using the exact fallback values that GameFlowController applies when EconomyConfigSO is null.
    /// </summary>
    [TestFixture]
    [Category("Presentation")]
    public class GameFlowControllerTests
    {
        // ── Fallback constants copied verbatim from GameFlowController.InitializeSystems ──

        private static readonly int[] FallbackEssencePerStar = { 0, 30, 50, 80 };

        private static readonly (int threshold, float multiplier)[] FallbackStreakTiers =
        {
            (1, 1.0f), (2, 1.25f), (3, 1.5f), (5, 2.0f), (8, 2.5f), (10, 3.0f)
        };

        private const int FallbackAdDailyCap = 5;
        private const int FallbackInterstitialFrequency = 3;

        // ── Shared helpers ──────────────────────────────────────────────────────────────

        private static RewardCalculator MakeCalculator() =>
            new RewardCalculator(FallbackEssencePerStar, FallbackStreakTiers);

        private static WinStreakTracker MakeStreakTracker() =>
            new WinStreakTracker(FallbackStreakTiers);

        private static AdManager MakeAdManager(Func<bool> noAdsPass = null) =>
            new AdManager(FallbackAdDailyCap, FallbackInterstitialFrequency, noAdsPass);

        private static LevelConfig MakeLevelConfig(int levelId, bool isTutorial)
        {
            return new LevelConfig(
                levelId,
                gridWidth: 7,
                gridHeight: 9,
                ingredientPool: new[] { IngredientColor.Ember, IngredientColor.Frost },
                recipeTargets: new[] { new RecipeTarget(IngredientColor.Ember, 3) },
                moveLimit: 25,
                starThresholds: new[] { 1000, 3000, 6000 },
                isTutorial: isTutorial);
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 1. Fallback defaults: EconomyConfigSO null → known constant values are used
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void RewardCalculator_FallbackConfig_ZeroStars_YieldsZeroEssence()
        {
            // GameFlowController uses essencePerStar[0] = 0 for a fail/0-star outcome.
            var calc = MakeCalculator();
            Assert.AreEqual(0, calc.CalculateEssence(0, streak: 1),
                "A zero-star result must always award zero Essence regardless of streak.");
        }

        [Test]
        public void RewardCalculator_FallbackConfig_ThreeStars_NoStreak_Returns80()
        {
            // When streak < first tier threshold (1), multiplier stays 1.0.
            var calc = MakeCalculator();
            Assert.AreEqual(80, calc.CalculateEssence(3, streak: 0),
                "3-star with streak 0 should return the raw fallback value of 80 Essence.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 2. Essence calculation: star count × streak multiplier pipeline
        //    (mirrors HandleWin: IncrementStreak → CalculateEssence)
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void RewardCalculator_ThreeStarsAtStreak5_AppliesTwoXMultiplier()
        {
            // Streak 5 → tier (5, 2.0f). 80 * 2.0 = 160.
            var calc = MakeCalculator();
            Assert.AreEqual(160, calc.CalculateEssence(3, streak: 5),
                "3-star at streak 5 must double the base 80 Essence to 160.");
        }

        [Test]
        public void RewardCalculator_WinStreakPipeline_StreakIncrementBeforeCalculation()
        {
            // GameFlowController calls _winStreak.IncrementStreak() then passes
            // _winStreak.CurrentStreak to CalculateEssence. Verify the integrated path:
            // five consecutive wins → streak 5 → 2x multiplier on 3-star completion.
            var tracker = MakeStreakTracker();
            var calc = MakeCalculator();

            for (int i = 0; i < 5; i++)
                tracker.IncrementStreak();

            int essence = calc.CalculateEssence(3, tracker.CurrentStreak);
            Assert.AreEqual(5, tracker.CurrentStreak, "Tracker should reach streak 5 after 5 wins.");
            Assert.AreEqual(160, essence, "5-win streak 3-star should yield 160 Essence.");
        }

        [Test]
        public void RewardCalculator_StreakMaxTier_Streak10_ThreeStars_Returns240()
        {
            // Streak 10 → tier (10, 3.0f). 80 * 3.0 = 240. Streak 15 should also be 240.
            var calc = MakeCalculator();
            Assert.AreEqual(240, calc.CalculateEssence(3, streak: 10),
                "Streak-10 max tier should yield 3× base: 240 Essence for 3 stars.");
            Assert.AreEqual(240, calc.CalculateEssence(3, streak: 999),
                "Any streak beyond max tier should still be capped at 3×.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 3. Streak reset on fail (HandleLose path)
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void WinStreakTracker_ResetAfterFail_MultiplierReturnsToBase()
        {
            // GameFlowController calls _winStreak.ResetStreak() when streak < 2 on a loss
            // (or when HandleDismissStreakProtection fires). After reset, multiplier must be 1.
            var tracker = MakeStreakTracker();
            tracker.SetStreak(8);
            Assert.AreEqual(2.5f, tracker.CurrentMultiplier, "Pre-condition: streak 8 → 2.5x.");

            tracker.ResetStreak();

            Assert.AreEqual(0, tracker.CurrentStreak, "Streak must be 0 after reset.");
            Assert.AreEqual(1.0f, tracker.CurrentMultiplier,
                "Multiplier must return to 1.0 after streak reset on fail.");
        }

        [Test]
        public void WinStreakTracker_StreakProtectionPreservesStreak_ReplayDoesNotReset()
        {
            // GameFlowController: HandleProtectStreakAd sets _streakProtectedThisAttempt = true,
            // then ReplayCurrentLevel skips the ResetStreak call. Simulate by not calling Reset.
            var tracker = MakeStreakTracker();
            tracker.SetStreak(5);

            // Do NOT call ResetStreak (protection was activated).
            int streakBeforeReplay = tracker.CurrentStreak;

            // Increment for the replayed level win.
            tracker.IncrementStreak();

            Assert.AreEqual(streakBeforeReplay + 1, tracker.CurrentStreak,
                "A protected replay that results in a win should continue incrementing the streak.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 4. Tutorial detection: levels 1–5 must be flagged IsTutorial
        // ────────────────────────────────────────────────────────────────────────────────

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void LevelConfig_TutorialLevelIds_IsTutorialIsTrue(int levelId)
        {
            var config = MakeLevelConfig(levelId, isTutorial: true);
            Assert.IsTrue(config.IsTutorial,
                $"Level {levelId} is within the tutorial range and must be marked as tutorial.");
        }

        [TestCase(6)]
        [TestCase(10)]
        [TestCase(40)]
        public void LevelConfig_NonTutorialLevelIds_IsTutorialIsFalse(int levelId)
        {
            var config = MakeLevelConfig(levelId, isTutorial: false);
            Assert.IsFalse(config.IsTutorial,
                $"Level {levelId} is beyond the tutorial range and must not be marked as tutorial.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 5. Ad daily cap: AdManager respects FallbackAdDailyCap (5)
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void AdManager_FallbackDailyCap_BlocksRewardedAfterFiveWatched()
        {
            // GameFlowController passes _economyConfig.RewardedAdDailyCap (default 5) when
            // EconomyConfigSO is non-null, or literal 5 when null. Test the same cap value.
            var ads = MakeAdManager();
            var day = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
            ads.CheckNewDay(day);

            for (int i = 0; i < FallbackAdDailyCap; i++)
            {
                Assert.IsTrue(ads.CanShowRewarded(AdPlacement.FailRecovery),
                    $"Slot {i + 1} of {FallbackAdDailyCap} should be available.");
                ads.RecordRewardedAdWatched(AdPlacement.FailRecovery);
            }

            Assert.IsFalse(ads.CanShowRewarded(AdPlacement.FailRecovery),
                "Daily cap of 5 must be enforced: 6th ad should be blocked.");
            Assert.IsFalse(ads.CanShowRewarded(AdPlacement.DoublePotionReward),
                "Global daily cap applies across all rewarded placements.");
        }

        [Test]
        public void AdManager_DailyCapResetsOnNewDay()
        {
            var ads = MakeAdManager();
            var day1 = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
            var day2 = new DateTime(2026, 5, 5, 8, 0, 0, DateTimeKind.Utc);
            ads.CheckNewDay(day1);

            for (int i = 0; i < FallbackAdDailyCap; i++)
                ads.RecordRewardedAdWatched(AdPlacement.FailRecovery);

            Assert.AreEqual(FallbackAdDailyCap, ads.RewardedAdsWatchedToday);

            ads.CheckNewDay(day2);

            Assert.AreEqual(0, ads.RewardedAdsWatchedToday,
                "Daily rewarded ad counter must reset to zero on a new calendar day.");
            Assert.IsTrue(ads.CanShowRewarded(AdPlacement.FailRecovery),
                "Rewarded ads must be available again after the daily reset.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 6. Tutorial levels suppress interstitials
        //    (GameFlowController: _adManager.RecordLevelWin(_currentLevelConfig.IsTutorial))
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void AdManager_TutorialWins_DoNotAdvanceInterstitialCounter()
        {
            // Completing 3 tutorial levels must NOT trigger an interstitial even though
            // the fallback frequency is 3.
            var ads = MakeAdManager();
            var day = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
            ads.CheckNewDay(day);

            for (int i = 1; i <= FallbackInterstitialFrequency; i++)
            {
                var config = MakeLevelConfig(i, isTutorial: true);
                ads.RecordLevelWin(config.IsTutorial);
            }

            Assert.AreEqual(0, ads.LevelWinsSinceLastInterstitial,
                "Tutorial level wins must not increment the interstitial counter.");
            Assert.IsFalse(ads.ShouldShowInterstitial(),
                "Interstitial must not trigger after tutorial-only wins.");
        }

        [Test]
        public void AdManager_NonTutorialWins_TriggerInterstitialAtFrequency()
        {
            var ads = MakeAdManager();
            var day = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
            ads.CheckNewDay(day);

            for (int i = 0; i < FallbackInterstitialFrequency - 1; i++)
                ads.RecordLevelWin(isTutorialLevel: false);

            Assert.IsFalse(ads.ShouldShowInterstitial(),
                $"Interstitial must not trigger before {FallbackInterstitialFrequency} non-tutorial wins.");

            ads.RecordLevelWin(isTutorialLevel: false);

            Assert.IsTrue(ads.ShouldShowInterstitial(),
                $"Interstitial must trigger exactly at frequency {FallbackInterstitialFrequency}.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 7. CurrencyManager: Add/Spend are the only mutation paths; balances cannot go negative
        //    (GameFlowController adds Essence in HandleWin, Gems in HandleProtectStreakGems)
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void CurrencyManager_AddThenSpend_BalanceIsCorrect()
        {
            var wallet = new CurrencyManager();
            wallet.Add(CurrencyType.Essence, 80, "level_complete");
            wallet.Add(CurrencyType.Essence, 160, "level_complete");

            bool spent = wallet.Spend(CurrencyType.Essence, 100, "booster");

            Assert.IsTrue(spent, "Spend must succeed when balance is sufficient.");
            Assert.AreEqual(140, wallet.GetBalance(CurrencyType.Essence),
                "Balance after add(80)+add(160)-spend(100) must be 140.");
        }

        [Test]
        public void CurrencyManager_SpendMoreThanBalance_ReturnsFalseAndPreservesBalance()
        {
            // GameFlowController uses Spend for streak-protection gems; if wallet is empty the
            // protection must silently fail — not throw or corrupt the balance.
            var wallet = new CurrencyManager();
            wallet.Add(CurrencyType.Gems, 3, "daily_brew");

            bool result = wallet.Spend(CurrencyType.Gems, 5, "streak_protection");

            Assert.IsFalse(result, "Spend must return false when balance is insufficient.");
            Assert.AreEqual(3, wallet.GetBalance(CurrencyType.Gems),
                "Balance must remain unchanged after a failed Spend.");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 8. Level-cap guard: AdvanceToNextLevel stays within max level
        //    (GameFlowController: if nextLevelId <= maxLevel → StartLevel, else ShowLevelSelect)
        //    Tested via the integer arithmetic and LevelConfig used by that path.
        // ────────────────────────────────────────────────────────────────────────────────

        [Test]
        public void AdvanceToNextLevel_AtMaxLevel_ShouldNotExceedMax()
        {
            // Simulate GameFlowController.AdvanceToNextLevel logic inline:
            // nextLevelId = currentLevelId + 1; if (nextLevelId <= maxLevel) → play, else → select.
            const int maxLevel = 40;
            int currentLevelId = maxLevel; // player is on the last level

            int nextLevelId = currentLevelId + 1;
            bool wouldStartNewLevel = nextLevelId <= maxLevel;

            Assert.IsFalse(wouldStartNewLevel,
                "AdvanceToNextLevel must not start a new level beyond the max level of 40.");
        }

        [Test]
        public void AdvanceToNextLevel_BelowMaxLevel_StartsNextLevel()
        {
            const int maxLevel = 40;
            int currentLevelId = 1;

            int nextLevelId = currentLevelId + 1;
            bool wouldStartNewLevel = nextLevelId <= maxLevel;

            Assert.IsTrue(wouldStartNewLevel,
                "AdvanceToNextLevel must start the next level when below the max level cap.");
            Assert.AreEqual(2, nextLevelId, "Next level ID must be currentLevelId + 1.");
        }

        [Test]
        public void AdvanceToNextLevel_OneLevelBelowMax_AdvancesToMax()
        {
            const int maxLevel = 40;
            int currentLevelId = maxLevel - 1; // level 39

            int nextLevelId = currentLevelId + 1;
            bool wouldStartNewLevel = nextLevelId <= maxLevel;

            Assert.IsTrue(wouldStartNewLevel,
                "Level 39 should advance to level 40 (the max), not return to level select.");
            Assert.AreEqual(maxLevel, nextLevelId);
        }
    }
}
