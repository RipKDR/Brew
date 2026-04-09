using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Brew.Core.Backend
{
#if FIREBASE_ANALYTICS
    using Firebase.Analytics;

    public sealed class FirebaseAnalyticsBridge
    {
        private AnalyticsManager _manager;
        private string _userId;

        public void Initialize(AnalyticsManager manager)
        {
            if (_manager != null)
                _manager.OnEventLogged -= OnEventLogged;

            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _manager.OnEventLogged += OnEventLogged;

            if (!string.IsNullOrEmpty(_userId))
                FirebaseAnalytics.SetUserId(_userId);

            Debug.Log("[FirebaseAnalyticsBridge] Initialized.");
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
            if (string.IsNullOrEmpty(userId))
                FirebaseAnalytics.SetUserId(null);
            else
                FirebaseAnalytics.SetUserId(userId);
        }

        private void OnEventLogged(string eventName, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParams = MapToParameters(parameters);
            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }

        private static Parameter[] MapToParameters(Dictionary<string, object> dict)
        {
            var result = new Parameter[dict.Count];
            int i = 0;
            foreach (var kv in dict)
            {
                result[i++] = CreateParameter(kv.Key, kv.Value);
            }
            return result;
        }

        private static Parameter CreateParameter(string key, object value)
        {
            return value switch
            {
                int intVal => new Parameter(key, intVal),
                long longVal => new Parameter(key, longVal),
                float floatVal => new Parameter(key, floatVal),
                double doubleVal => new Parameter(key, doubleVal),
                bool boolVal => new Parameter(key, boolVal ? 1L : 0L),
                string strVal => new Parameter(key, strVal),
                _ => new Parameter(key, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)
            };
        }

        public void Dispose()
        {
            if (_manager != null)
                _manager.OnEventLogged -= OnEventLogged;
        }
    }
#else
    public sealed class FirebaseAnalyticsBridge
    {
        private string _userId;

        public void Initialize(AnalyticsManager manager)
        {
            Debug.Log("[FirebaseAnalyticsBridge] FIREBASE_ANALYTICS not defined. Events logged in-memory only.");
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
        }

        public void Dispose() { }
    }
#endif
}
