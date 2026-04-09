using System;
using Brew.Core.Economy;
using Brew.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Brew.Presentation
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _hapticsToggle;
        [SerializeField] private Toggle _screenShakeToggle;
        [SerializeField] private Toggle _notificationsToggle;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _restorePurchasesButton;
        [SerializeField] private GameObject _creditsPanel;
        [SerializeField] private string _privacyPolicyUrl = "https://brew-game.com/privacy";

        private IAPManager _iapManager;
        private bool _isInitializing;

        public event Action OnRestorePurchasesRequested;

        public void Initialize(IAPManager iapManager)
        {
            _iapManager = iapManager;
        }

        private void OnEnable()
        {
            _isInitializing = true;

            if (_musicSlider != null)
            {
                _musicSlider.value = SettingsManager.MusicVolume;
                _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.value = SettingsManager.SfxVolume;
                _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_hapticsToggle != null)
            {
                _hapticsToggle.isOn = SettingsManager.HapticsEnabled;
                HapticManager.Enabled = SettingsManager.HapticsEnabled;
                _hapticsToggle.onValueChanged.AddListener(OnHapticsChanged);
            }

            if (_screenShakeToggle != null)
            {
                _screenShakeToggle.isOn = SettingsManager.ScreenShakeEnabled;
                _screenShakeToggle.onValueChanged.AddListener(OnScreenShakeChanged);
            }

            if (_notificationsToggle != null)
            {
                _notificationsToggle.isOn = SettingsManager.NotificationsEnabled;
                _notificationsToggle.onValueChanged.AddListener(OnNotificationsChanged);
            }

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            if (_privacyPolicyButton != null)
                _privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);

            if (_creditsButton != null)
                _creditsButton.onClick.AddListener(OnCreditsClicked);

            if (_restorePurchasesButton != null)
                _restorePurchasesButton.onClick.AddListener(OnRestorePurchasesClicked);

            _isInitializing = false;
        }

        private void OnDisable()
        {
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (_hapticsToggle != null) _hapticsToggle.onValueChanged.RemoveListener(OnHapticsChanged);
            if (_screenShakeToggle != null) _screenShakeToggle.onValueChanged.RemoveListener(OnScreenShakeChanged);
            if (_notificationsToggle != null) _notificationsToggle.onValueChanged.RemoveListener(OnNotificationsChanged);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Hide);
            if (_privacyPolicyButton != null) _privacyPolicyButton.onClick.RemoveListener(OnPrivacyPolicyClicked);
            if (_creditsButton != null) _creditsButton.onClick.RemoveListener(OnCreditsClicked);
            if (_restorePurchasesButton != null) _restorePurchasesButton.onClick.RemoveListener(OnRestorePurchasesClicked);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void OnMusicVolumeChanged(float value)
        {
            if (_isInitializing) return;
            SettingsManager.MusicVolume = value;
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_isInitializing) return;
            SettingsManager.SfxVolume = value;
        }

        private void OnHapticsChanged(bool value)
        {
            if (_isInitializing) return;
            SettingsManager.HapticsEnabled = value;
            HapticManager.Enabled = value;
        }

        private void OnScreenShakeChanged(bool value)
        {
            if (_isInitializing) return;
            SettingsManager.ScreenShakeEnabled = value;
        }

        private void OnNotificationsChanged(bool value)
        {
            if (_isInitializing) return;
            SettingsManager.NotificationsEnabled = value;
        }

        private void OnPrivacyPolicyClicked()
        {
            if (!string.IsNullOrEmpty(_privacyPolicyUrl))
                Application.OpenURL(_privacyPolicyUrl);
        }

        private void OnCreditsClicked()
        {
            if (_creditsPanel != null)
                _creditsPanel.SetActive(!_creditsPanel.activeSelf);
        }

        private void OnRestorePurchasesClicked()
        {
            OnRestorePurchasesRequested?.Invoke();
        }
    }
}
