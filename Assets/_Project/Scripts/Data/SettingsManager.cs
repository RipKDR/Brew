using System;
using UnityEngine;

namespace Brew.Data
{
    public static class SettingsManager
    {
        private const string KeyMusicVolume = "Brew_MusicVolume";
        private const string KeySfxVolume = "Brew_SfxVolume";
        private const string KeyIsMuted = "Brew_IsMuted";
        private const string KeyHapticsEnabled = "Brew_HapticsEnabled";
        private const string KeyScreenShakeEnabled = "Brew_ScreenShakeEnabled";

        public static event Action OnSettingsChanged;

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(KeyMusicVolume, 0.7f);
            set
            {
                PlayerPrefs.SetFloat(KeyMusicVolume, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(KeySfxVolume, 1f);
            set
            {
                PlayerPrefs.SetFloat(KeySfxVolume, Mathf.Clamp01(value));
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }

        public static bool IsMuted
        {
            get => PlayerPrefs.GetInt(KeyIsMuted, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyIsMuted, value ? 1 : 0);
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }

        public static bool HapticsEnabled
        {
            get => PlayerPrefs.GetInt(KeyHapticsEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyHapticsEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }

        public static bool ScreenShakeEnabled
        {
            get => PlayerPrefs.GetInt(KeyScreenShakeEnabled, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KeyScreenShakeEnabled, value ? 1 : 0);
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }
    }
}
