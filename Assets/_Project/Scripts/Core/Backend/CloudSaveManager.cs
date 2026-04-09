using System;

namespace Brew.Core.Backend
{
    /// <summary>
    /// Offline-first save buffer; persistence and Firebase sync are wired later. Pure C#.
    /// </summary>
    public sealed class CloudSaveManager
    {
        private PlayerSaveData _localData;
        private bool _isDirty;

        public bool IsDirty => _isDirty;

        public DateTime? LastSyncUtc { get; private set; }

        public event Action<PlayerSaveData> OnSaveRequested;

        public event Action<PlayerSaveData> OnSyncRequired;

        public void SaveLocal(PlayerSaveData data)
        {
            _localData = data == null ? null : PlayerSaveData.CopyFrom(data);
            OnSaveRequested?.Invoke(_localData);
        }

        public PlayerSaveData GetLocalData() => _localData;

        public void MarkDirty()
        {
            _isDirty = true;
            OnSyncRequired?.Invoke(_localData);
        }

        public void MarkSynced(DateTime utcNow)
        {
            _isDirty = false;
            LastSyncUtc = utcNow;
        }

        public sealed class PlayerSaveData
        {
            public int Essence;
            public int Gems;
            public int CurrentLevel;
            public int[] CompletedLevels;
            public int[] StarRatings;
            public int WorkshopLevel;
            public int[] UnlockedPotions;
            public int[] ClaimedMilestones;
            public int WinStreak;
            public string LastDailyBrewDate;
            public int DailyBrewStreak;
            public string[] PurchasedProducts;

            public static PlayerSaveData CopyFrom(PlayerSaveData source)
            {
                if (source == null)
                    return null;

                return new PlayerSaveData
                {
                    Essence = source.Essence,
                    Gems = source.Gems,
                    CurrentLevel = source.CurrentLevel,
                    CompletedLevels = CloneArray(source.CompletedLevels),
                    StarRatings = CloneArray(source.StarRatings),
                    WorkshopLevel = source.WorkshopLevel,
                    UnlockedPotions = CloneArray(source.UnlockedPotions),
                    ClaimedMilestones = CloneArray(source.ClaimedMilestones),
                    WinStreak = source.WinStreak,
                    LastDailyBrewDate = source.LastDailyBrewDate,
                    DailyBrewStreak = source.DailyBrewStreak,
                    PurchasedProducts = CloneStringArray(source.PurchasedProducts)
                };
            }

            private static int[] CloneArray(int[] source) =>
                source == null ? null : (int[])source.Clone();

            private static string[] CloneStringArray(string[] source) =>
                source == null ? null : (string[])source.Clone();
        }
    }
}
