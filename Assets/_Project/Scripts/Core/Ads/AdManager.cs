using System;

namespace Brew.Core.Ads
{
    /// <summary>
    /// Tracks rewarded-ad caps, per-placement daily limits, and interstitial frequency.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class AdManager
    {
        private readonly int _maxRewardedPerDay;
        private readonly int _interstitialFrequency;
        private readonly Func<bool> _hasNoAdsPass;

        private int _rewardedAdsWatchedToday;
        private int _levelWinsSinceLastInterstitial;
        private DateTime _lastResetDate;
        private bool _lastWinWasTutorial;

        private bool _freeDailyBoosterUsedToday;
        private int _streakProtectionAdsToday;

        public event Action<AdPlacement> OnRewardedAdCompleted;
        public event Action OnInterstitialShown;

        public int RewardedAdsWatchedToday => _rewardedAdsWatchedToday;
        public int LevelWinsSinceLastInterstitial => _levelWinsSinceLastInterstitial;
        public DateTime LastResetDate => _lastResetDate;

        public AdManager(
            int maxRewardedPerDay = 5,
            int interstitialFrequency = 3,
            Func<bool> hasNoAdsPass = null)
        {
            if (maxRewardedPerDay < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRewardedPerDay));
            if (interstitialFrequency < 1)
                throw new ArgumentOutOfRangeException(nameof(interstitialFrequency));

            _maxRewardedPerDay = maxRewardedPerDay;
            _interstitialFrequency = interstitialFrequency;
            _hasNoAdsPass = hasNoAdsPass ?? (() => false);
        }

        public bool CanShowRewarded(AdPlacement placement)
        {
            if (placement == AdPlacement.Interstitial)
                return false;

            if (_rewardedAdsWatchedToday >= _maxRewardedPerDay)
                return false;

            return CanShowPlacement(placement);
        }

        public bool CanShowPlacement(AdPlacement placement)
        {
            switch (placement)
            {
                case AdPlacement.FailRecovery:
                case AdPlacement.DoublePotionReward:
                case AdPlacement.DailyBrewDoubler:
                    return true;

                case AdPlacement.FreeDailyBooster:
                    return !_freeDailyBoosterUsedToday;

                case AdPlacement.StreakProtection:
                    return _streakProtectionAdsToday < 2;

                case AdPlacement.Interstitial:
                    return false;

                default:
                    return false;
            }
        }

        public void RecordRewardedAdWatched(AdPlacement placement)
        {
            if (placement == AdPlacement.Interstitial)
                return;

            _rewardedAdsWatchedToday++;

            switch (placement)
            {
                case AdPlacement.FreeDailyBooster:
                    _freeDailyBoosterUsedToday = true;
                    break;
                case AdPlacement.StreakProtection:
                    _streakProtectionAdsToday++;
                    break;
            }

            OnRewardedAdCompleted?.Invoke(placement);
        }

        public bool ShouldShowInterstitial()
        {
            if (_hasNoAdsPass())
                return false;

            if (_lastWinWasTutorial)
                return false;

            return _levelWinsSinceLastInterstitial >= _interstitialFrequency;
        }

        public void RecordLevelWin(bool isTutorialLevel)
        {
            if (_hasNoAdsPass())
                return;

            _lastWinWasTutorial = isTutorialLevel;

            if (!isTutorialLevel)
                _levelWinsSinceLastInterstitial++;
        }

        public void RecordInterstitialShown()
        {
            _levelWinsSinceLastInterstitial = 0;
            OnInterstitialShown?.Invoke();
        }

        public void CheckNewDay(DateTime utcNow)
        {
            var today = utcNow.Date;

            if (_lastResetDate == default || today > _lastResetDate.Date)
            {
                _rewardedAdsWatchedToday = 0;
                _levelWinsSinceLastInterstitial = 0;
                _freeDailyBoosterUsedToday = false;
                _streakProtectionAdsToday = 0;
                _lastResetDate = today;
            }
        }
    }
}
