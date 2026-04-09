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
        [SerializeField] private Button _closeButton;

        private bool _isInitializing;

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

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);

            _isInitializing = false;
        }

        private void OnDisable()
        {
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (_hapticsToggle != null) _hapticsToggle.onValueChanged.RemoveListener(OnHapticsChanged);
            if (_screenShakeToggle != null) _screenShakeToggle.onValueChanged.RemoveListener(OnScreenShakeChanged);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Hide);
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
    }
}
