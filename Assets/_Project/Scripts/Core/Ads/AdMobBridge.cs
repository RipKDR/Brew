using System;
using UnityEngine;

namespace Brew.Core.Ads
{
#if ADMOB
    using GoogleMobileAds.Api;

    public sealed class AdMobBridge
    {
        private readonly AdManager _adManager;
        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;

        private string _rewardedAdUnitId;
        private string _interstitialAdUnitId;
        private AdPlacement _pendingPlacement;

        public bool IsRewardedAdReady => _rewardedAd != null && _rewardedAd.CanShowAd();
        public bool IsInterstitialReady => _interstitialAd != null && _interstitialAd.CanShowAd();

        public event Action<AdPlacement> OnRewardGranted;
        public event Action OnAdLoadFailed;

        public AdMobBridge(AdManager adManager)
        {
            _adManager = adManager ?? throw new ArgumentNullException(nameof(adManager));
        }

        public void Initialize(string rewardedAdUnitId, string interstitialAdUnitId)
        {
            _rewardedAdUnitId = rewardedAdUnitId;
            _interstitialAdUnitId = interstitialAdUnitId;

            MobileAds.Initialize(status =>
            {
                Debug.Log("[AdMobBridge] AdMob initialized.");
                LoadRewardedAd();
                LoadInterstitialAd();
            });
        }

        public void ShowRewardedAd(AdPlacement placement)
        {
            if (_rewardedAd == null || !_rewardedAd.CanShowAd())
            {
                Debug.LogWarning("[AdMobBridge] Rewarded ad not ready.");
                return;
            }

            _pendingPlacement = placement;
            _rewardedAd.Show(reward =>
            {
                _adManager.RecordRewardedAdWatched(_pendingPlacement);
                OnRewardGranted?.Invoke(_pendingPlacement);
                Debug.Log($"[AdMobBridge] Rewarded ad completed: {_pendingPlacement}");
                LoadRewardedAd();
            });
        }

        public void ShowInterstitial()
        {
            if (_interstitialAd == null || !_interstitialAd.CanShowAd())
            {
                Debug.LogWarning("[AdMobBridge] Interstitial not ready.");
                return;
            }

            _interstitialAd.Show();
            _adManager.RecordInterstitialShown();
            Debug.Log("[AdMobBridge] Interstitial shown.");

            _interstitialAd.Destroy();
            _interstitialAd = null;
            LoadInterstitialAd();
        }

        private void LoadRewardedAd()
        {
            if (string.IsNullOrEmpty(_rewardedAdUnitId)) return;

            var request = new AdRequest();
            RewardedAd.Load(_rewardedAdUnitId, request, (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[AdMobBridge] Rewarded ad load failed: {error.GetMessage()}");
                    OnAdLoadFailed?.Invoke();
                    _rewardedAd = null;
                    return;
                }

                _rewardedAd = ad;
            });
        }

        private void LoadInterstitialAd()
        {
            if (string.IsNullOrEmpty(_interstitialAdUnitId)) return;

            var request = new AdRequest();
            InterstitialAd.Load(_interstitialAdUnitId, request, (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[AdMobBridge] Interstitial load failed: {error.GetMessage()}");
                    _interstitialAd = null;
                    return;
                }

                _interstitialAd = ad;
            });
        }

        public void Dispose()
        {
            _rewardedAd?.Destroy();
            _interstitialAd?.Destroy();
        }
    }
#else
    public sealed class AdMobBridge
    {
        public bool IsRewardedAdReady => false;
        public bool IsInterstitialReady => false;

#pragma warning disable CS0067
        public event Action<AdPlacement> OnRewardGranted;
        public event Action OnAdLoadFailed;
#pragma warning restore CS0067

        public AdMobBridge(AdManager adManager)
        {
            if (adManager == null) throw new ArgumentNullException(nameof(adManager));
            Debug.LogWarning("[AdMobBridge] ADMOB not defined. Ad system disabled.");
        }

        public void Initialize(string rewardedAdUnitId, string interstitialAdUnitId) { }

        public void ShowRewardedAd(AdPlacement placement) { }

        public void ShowInterstitial() { }

        public void Dispose() { }
    }
#endif
}
