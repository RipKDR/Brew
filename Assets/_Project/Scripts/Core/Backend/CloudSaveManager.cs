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
            public int Essence { get; set; }
            public int Gems { get; set; }
            public int CurrentLevel { get; set; }
            public int[] CompletedLevels { get; set; }
            public int[] StarRatings { get; set; }
            public int WorkshopLevel { get; set; }
            public int[] UnlockedPotions { get; set; }
            public int[] ClaimedMilestones { get; set; }
            public int WinStreak { get; set; }
            public string LastDailyBrewDate { get; set; }
            public int DailyBrewStreak { get; set; }
            public string[] PurchasedProducts { get; set; }

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
