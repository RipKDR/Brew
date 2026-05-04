using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Brew.Core.Backend
{
#if FIREBASE_FIRESTORE
    using Firebase.Firestore;

    public sealed class FirebaseCloudSaveBridge
    {
        private CloudSaveManager _cloudSaveManager;
        private string _userId;

        public void Initialize(CloudSaveManager manager, string userId)
        {
            _cloudSaveManager = manager ?? throw new ArgumentNullException(nameof(manager));
            _userId = userId;
            _cloudSaveManager.OnSyncRequired += OnSyncRequired;
            Debug.Log($"[FirebaseCloudSaveBridge] Initialized for user {userId}.");
        }

        public async Task SaveToCloudAsync(CloudSaveManager.PlayerSaveData data, string userId)
        {
            if (data == null || string.IsNullOrEmpty(userId)) return;

            try
            {
                var db = FirebaseFirestore.DefaultInstance;
                var docRef = db.Collection("players").Document(userId)
                               .Collection("saves").Document("current");

                var dict = SerializeSaveData(data);
                dict["updatedAt"] = FieldValue.ServerTimestamp;

                await docRef.SetAsync(dict);
                _cloudSaveManager?.MarkSynced(DateTime.UtcNow);
                Debug.Log("[FirebaseCloudSaveBridge] Save synced to Firestore.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseCloudSaveBridge] Save failed: {ex.Message}");
            }
        }

        public async Task<CloudSaveManager.PlayerSaveData> LoadFromCloudAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            try
            {
                var db = FirebaseFirestore.DefaultInstance;
                var docRef = db.Collection("players").Document(userId)
                               .Collection("saves").Document("current");

                var snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists) return null;

                return DeserializeSaveData(snapshot);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseCloudSaveBridge] Load failed: {ex.Message}");
                return null;
            }
        }

        public static CloudSaveManager.PlayerSaveData ResolveConflict(
            CloudSaveManager.PlayerSaveData local,
            CloudSaveManager.PlayerSaveData remote)
        {
            if (local == null) return remote;
            if (remote == null) return local;

            var merged = CloudSaveManager.PlayerSaveData.CopyFrom(remote);

            merged.CurrentLevel = Math.Max(local.CurrentLevel, remote.CurrentLevel);
            merged.CompletedLevels = MergeMaxArray(local.CompletedLevels, remote.CompletedLevels);
            merged.StarRatings = MergeMaxArray(local.StarRatings, remote.StarRatings);

            if (string.CompareOrdinal(local.LastDailyBrewDate ?? "", remote.LastDailyBrewDate ?? "") > 0)
            {
                merged.LastDailyBrewDate = local.LastDailyBrewDate;
                merged.DailyBrewStreak = local.DailyBrewStreak;
            }

            return merged;
        }

        private async void OnSyncRequired(CloudSaveManager.PlayerSaveData data)
        {
            if (string.IsNullOrEmpty(_userId) || data == null) return;

            try
            {
                await SaveToCloudAsync(data, _userId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCloudSaveBridge] Unhandled sync error: {ex.Message}");
            }
        }

        private static Dictionary<string, object> SerializeSaveData(CloudSaveManager.PlayerSaveData data)
        {
            return new Dictionary<string, object>
            {
                ["essence"] = data.Essence,
                ["gems"] = data.Gems,
                ["currentLevel"] = data.CurrentLevel,
                ["completedLevels"] = data.CompletedLevels ?? Array.Empty<int>(),
                ["starRatings"] = data.StarRatings ?? Array.Empty<int>(),
                ["workshopLevel"] = data.WorkshopLevel,
                ["unlockedPotions"] = data.UnlockedPotions ?? Array.Empty<int>(),
                ["claimedMilestones"] = data.ClaimedMilestones ?? Array.Empty<int>(),
                ["winStreak"] = data.WinStreak,
                ["lastDailyBrewDate"] = data.LastDailyBrewDate ?? string.Empty,
                ["dailyBrewStreak"] = data.DailyBrewStreak,
                ["purchasedProducts"] = data.PurchasedProducts ?? Array.Empty<string>()
            };
        }

        private static CloudSaveManager.PlayerSaveData DeserializeSaveData(DocumentSnapshot snapshot)
        {
            return new CloudSaveManager.PlayerSaveData
            {
                Essence = snapshot.GetValue<int>("essence"),
                Gems = snapshot.GetValue<int>("gems"),
                CurrentLevel = snapshot.GetValue<int>("currentLevel"),
                CompletedLevels = snapshot.ContainsField("completedLevels")
                    ? snapshot.GetValue<int[]>("completedLevels") : null,
                StarRatings = snapshot.ContainsField("starRatings")
                    ? snapshot.GetValue<int[]>("starRatings") : null,
                WorkshopLevel = snapshot.GetValue<int>("workshopLevel"),
                UnlockedPotions = snapshot.ContainsField("unlockedPotions")
                    ? snapshot.GetValue<int[]>("unlockedPotions") : null,
                ClaimedMilestones = snapshot.ContainsField("claimedMilestones")
                    ? snapshot.GetValue<int[]>("claimedMilestones") : null,
                WinStreak = snapshot.GetValue<int>("winStreak"),
                LastDailyBrewDate = snapshot.ContainsField("lastDailyBrewDate")
                    ? snapshot.GetValue<string>("lastDailyBrewDate") : null,
                DailyBrewStreak = snapshot.GetValue<int>("dailyBrewStreak"),
                PurchasedProducts = snapshot.ContainsField("purchasedProducts")
                    ? snapshot.GetValue<string[]>("purchasedProducts") : null
            };
        }

        private static int[] MergeMaxArray(int[] a, int[] b)
        {
            if (a == null) return b;
            if (b == null) return a;

            int len = Math.Max(a.Length, b.Length);
            var result = new int[len];
            for (int i = 0; i < len; i++)
            {
                int va = i < a.Length ? a[i] : 0;
                int vb = i < b.Length ? b[i] : 0;
                result[i] = Math.Max(va, vb);
            }
            return result;
        }

        public void Dispose()
        {
            if (_cloudSaveManager != null)
                _cloudSaveManager.OnSyncRequired -= OnSyncRequired;
        }
    }
#else
    public sealed class FirebaseCloudSaveBridge
    {
        public void Initialize(CloudSaveManager manager, string userId)
        {
            Debug.Log("[FirebaseCloudSaveBridge] FIREBASE_FIRESTORE not defined. Cloud save disabled.");
        }

        public Task SaveToCloudAsync(CloudSaveManager.PlayerSaveData data, string userId)
        {
            return Task.CompletedTask;
        }

        public Task<CloudSaveManager.PlayerSaveData> LoadFromCloudAsync(string userId)
        {
            return Task.FromResult<CloudSaveManager.PlayerSaveData>(null);
        }

        public static CloudSaveManager.PlayerSaveData ResolveConflict(
            CloudSaveManager.PlayerSaveData local,
            CloudSaveManager.PlayerSaveData remote)
        {
            if (local == null) return remote;
            if (remote == null) return local;

            var merged = CloudSaveManager.PlayerSaveData.CopyFrom(remote);
            merged.CurrentLevel = Math.Max(local.CurrentLevel, remote.CurrentLevel);
            merged.CompletedLevels = MergeMaxArray(local.CompletedLevels, remote.CompletedLevels);
            merged.StarRatings = MergeMaxArray(local.StarRatings, remote.StarRatings);

            if (string.CompareOrdinal(local.LastDailyBrewDate ?? "", remote.LastDailyBrewDate ?? "") > 0)
            {
                merged.LastDailyBrewDate = local.LastDailyBrewDate;
                merged.DailyBrewStreak = local.DailyBrewStreak;
            }

            return merged;
        }

        private static int[] MergeMaxArray(int[] a, int[] b)
        {
            if (a == null) return b;
            if (b == null) return a;

            int len = Math.Max(a.Length, b.Length);
            var result = new int[len];
            for (int i = 0; i < len; i++)
            {
                int va = i < a.Length ? a[i] : 0;
                int vb = i < b.Length ? b[i] : 0;
                result[i] = Math.Max(va, vb);
            }
            return result;
        }

        public void Dispose() { }
    }
#endif
}
