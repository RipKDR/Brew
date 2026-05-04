using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Brew.Core.Backend
{
#if FIREBASE_REMOTE_CONFIG
    using Firebase.RemoteConfig;

    public sealed class FirebaseRemoteConfigBridge
    {
        public async Task FetchAndActivateAsync(RemoteConfigManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));

            try
            {
                var remoteConfig = FirebaseRemoteConfig.DefaultInstance;

                var settings = new ConfigSettings
                {
                    MinimumFetchIntervalInMilliseconds = 43200UL * 1000UL
                };
                await remoteConfig.SetConfigSettingsAsync(settings);

                await remoteConfig.FetchAsync(TimeSpan.FromHours(12));

                if (remoteConfig.Info.LastFetchStatus != LastFetchStatus.Success)
                {
                    Debug.LogWarning($"[FirebaseRemoteConfigBridge] Fetch failed: {remoteConfig.Info.LastFetchStatus}");
                    return;
                }

                await remoteConfig.ActivateAsync();

                var serverValues = new Dictionary<string, object>();
                foreach (var key in remoteConfig.AllValues.Keys)
                {
                    serverValues[key] = remoteConfig.AllValues[key].StringValue;
                }

                manager.ApplyServerValues(serverValues);
                manager.MarkFetched(DateTime.UtcNow);
                Debug.Log($"[FirebaseRemoteConfigBridge] Fetched and activated {serverValues.Count} values.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseRemoteConfigBridge] Fetch failed: {ex.Message}");
            }
        }
    }
#else
    public sealed class FirebaseRemoteConfigBridge
    {
        public Task FetchAndActivateAsync(RemoteConfigManager manager)
        {
            Debug.LogWarning("[FirebaseRemoteConfigBridge] FIREBASE_REMOTE_CONFIG not defined. Using local defaults.");
            return Task.CompletedTask;
        }
    }
#endif
}
