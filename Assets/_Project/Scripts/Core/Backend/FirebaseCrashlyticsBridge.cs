using System;
using UnityEngine;

namespace Brew.Core.Backend
{
#if FIREBASE_CRASHLYTICS
    using Firebase.Crashlytics;

    public static class FirebaseCrashlyticsBridge
    {
        public static void Initialize()
        {
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            Debug.Log("[FirebaseCrashlyticsBridge] Initialized with fatal uncaught exceptions.");
        }

        public static void SetUserId(string userId)
        {
            Crashlytics.SetUserId(userId ?? string.Empty);
        }

        public static void Log(string message)
        {
            Crashlytics.Log(message ?? string.Empty);
        }

        public static void LogException(Exception ex)
        {
            if (ex != null)
                Crashlytics.LogException(ex);
        }
    }
#else
    public static class FirebaseCrashlyticsBridge
    {
        public static void Initialize()
        {
            Debug.Log("[FirebaseCrashlyticsBridge] FIREBASE_CRASHLYTICS not defined. Crash reporting disabled.");
        }

        public static void SetUserId(string userId) { }

        public static void Log(string message) { }

        public static void LogException(Exception ex) { }
    }
#endif
}
