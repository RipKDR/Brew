using System.Runtime.InteropServices;
using Brew.Data;
using UnityEngine;

namespace Brew.Presentation
{
    /// <summary>
    /// Platform-abstracted haptic feedback. Safe in Editor and when native APIs are unavailable.
    /// </summary>
    public static class HapticManager
    {
        private static bool _enabled = true;

        public static bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal", EntryPoint = "Brew_IOS_Impact")]
        private static extern void Native_IOS_Impact(int style);

        [DllImport("__Internal", EntryPoint = "Brew_IOS_Notification")]
        private static extern void Native_IOS_Notification(int type);

        [DllImport("__Internal", EntryPoint = "Brew_IOS_Selection")]
        private static extern void Native_IOS_Selection();
#endif

        public static void LightImpact()
        {
            if (!AllowHaptics()) return;
#if UNITY_IOS && !UNITY_EDITOR
            SafeNative(() => Native_IOS_Impact(0));
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrate(10L);
#else
            // Editor / other platforms
#endif
        }

        public static void MediumImpact()
        {
            if (!AllowHaptics()) return;
#if UNITY_IOS && !UNITY_EDITOR
            SafeNative(() => Native_IOS_Impact(1));
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrate(25L);
#else
#endif
        }

        public static void HeavyImpact()
        {
            if (!AllowHaptics()) return;
#if UNITY_IOS && !UNITY_EDITOR
            SafeNative(() => Native_IOS_Impact(2));
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrate(50L);
#else
#endif
        }

        public static void SuccessPattern()
        {
            if (!AllowHaptics()) return;
#if UNITY_IOS && !UNITY_EDITOR
            SafeNative(() => Native_IOS_Notification(0));
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibratePattern(new long[] { 0L, 30L, 50L, 30L }, -1);
#else
#endif
        }

        public static void SelectionImpact()
        {
            if (!AllowHaptics()) return;
#if UNITY_IOS && !UNITY_EDITOR
            SafeNative(Native_IOS_Selection);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrate(5L);
#else
#endif
        }

        /// <summary>Optional: short continuous buzz (legacy / gameplay hooks).</summary>
        public static void ContinuousRumble(float durationSeconds)
        {
            if (!AllowHaptics()) return;
            long ms = (long)Mathf.Max(1f, durationSeconds * 1000f);
#if UNITY_IOS && !UNITY_EDITOR
            SafeNative(() => Native_IOS_Impact(2));
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidVibrate(ms);
#else
#endif
        }

        public static bool IsHapticSupported()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return true;
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                return vibrator != null && vibrator.Call<bool>("hasVibrator");
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }

        private static bool AllowHaptics() => _enabled && SettingsManager.HapticsEnabled;

#if UNITY_IOS && !UNITY_EDITOR
        private static void SafeNative(System.Action invoke)
        {
            try
            {
                invoke?.Invoke();
            }
            catch
            {
                // Native bridge unavailable or failed; never crash gameplay.
            }
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void AndroidVibrate(long durationMs)
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator == null) return;

                if (AndroidApiLevel() >= 26)
                {
                    using var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    int amplitude = (int)Mathf.Clamp(durationMs * 4L, 1L, 255L);
                    using var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createOneShot", durationMs, amplitude);
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", durationMs);
                }
            }
            catch
            {
            }
        }

        private static void AndroidVibratePattern(long[] pattern, int repeat)
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator == null) return;

                if (AndroidApiLevel() >= 26)
                {
                    using var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    using var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createWaveform", pattern, repeat);
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", pattern, repeat);
                }
            }
            catch
            {
            }
        }

        private static int AndroidApiLevel()
        {
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            return version.GetStatic<int>("SDK_INT");
        }
#endif
    }
}
