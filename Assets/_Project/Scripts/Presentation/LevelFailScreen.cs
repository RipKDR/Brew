using System;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    /// <summary>
    /// Displayed when moves are exhausted and recipe is not complete.
    /// Offers retry, ad-for-extra-moves, and streak protection.
    /// </summary>
    public class LevelFailScreen : MonoBehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Button _retryButton;

        [Header("Recovery")]
        [SerializeField] private Button _watchAdButton;
        [SerializeField] private Text _watchAdLabel;

        [Header("Streak Protection")]
        [SerializeField] private GameObject _streakProtectionPanel;
        [SerializeField] private Button _protectStreakAdButton;
        [SerializeField] private Button _protectStreakGemButton;
        [SerializeField] private Text _protectStreakGemCostText;
        [SerializeField] private Button _dismissStreakButton;

        public event Action OnRetry;
        public event Action OnWatchAdForMoves;
        public event Action OnProtectStreakAd;
        public event Action OnProtectStreakGems;
        public event Action OnDismissStreakProtection;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(() => OnRetry?.Invoke());
            if (_watchAdButton != null) _watchAdButton.onClick.AddListener(() => OnWatchAdForMoves?.Invoke());
            if (_protectStreakAdButton != null) _protectStreakAdButton.onClick.AddListener(() => OnProtectStreakAd?.Invoke());
            if (_protectStreakGemButton != null) _protectStreakGemButton.onClick.AddListener(() => OnProtectStreakGems?.Invoke());
            if (_dismissStreakButton != null) _dismissStreakButton.onClick.AddListener(() => OnDismissStreakProtection?.Invoke());
        }

        public void Show() => Show(false, false, 0);

        public void Show(bool canWatchAd, bool showStreakProtection, int streakProtectionGemCost)
        {
            gameObject.SetActive(true);

            if (_messageText != null)
                _messageText.text = "Out of Moves!";

            if (_watchAdButton != null)
                _watchAdButton.gameObject.SetActive(canWatchAd);

            if (_streakProtectionPanel != null)
                _streakProtectionPanel.SetActive(showStreakProtection);

            if (_protectStreakGemCostText != null && streakProtectionGemCost > 0)
                _protectStreakGemCostText.text = $"{streakProtectionGemCost} Gems";
        }

        public void HideAdButton()
        {
            if (_watchAdButton != null) _watchAdButton.gameObject.SetActive(false);
        }

        public void HideStreakProtection()
        {
            if (_streakProtectionPanel != null) _streakProtectionPanel.SetActive(false);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
