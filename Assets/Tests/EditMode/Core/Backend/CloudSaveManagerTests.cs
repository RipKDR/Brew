using System;
using Brew.Core.Backend;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class CloudSaveManagerTests
    {
        private CloudSaveManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new CloudSaveManager();
        }

        [Test]
        public void GetLocalData_ReturnsNull_BeforeFirstSave()
        {
            Assert.IsNull(_manager.GetLocalData());
        }

        [Test]
        public void SaveLocal_StoresData_RetrievableByGetLocalData()
        {
            var data = new CloudSaveManager.PlayerSaveData
            {
                Essence = 120,
                Gems = 5,
                CurrentLevel = 3,
                CompletedLevels = new[] { 1, 2 },
                StarRatings = new[] { 3, 2 },
                WorkshopLevel = 1,
                UnlockedPotions = new[] { 0, 1 },
                ClaimedMilestones = new[] { 0 },
                WinStreak = 4,
                LastDailyBrewDate = "2026-04-09",
                DailyBrewStreak = 2,
                PurchasedProducts = new[] { "gem_pack_small" }
            };

            _manager.SaveLocal(data);

            var loaded = _manager.GetLocalData();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(120, loaded.Essence);
            Assert.AreEqual(5, loaded.Gems);
            Assert.AreEqual(3, loaded.CurrentLevel);
            CollectionAssert.AreEqual(new[] { 1, 2 }, loaded.CompletedLevels);
            CollectionAssert.AreEqual(new[] { 3, 2 }, loaded.StarRatings);
            Assert.AreEqual(1, loaded.WorkshopLevel);
            CollectionAssert.AreEqual(new[] { 0, 1 }, loaded.UnlockedPotions);
            CollectionAssert.AreEqual(new[] { 0 }, loaded.ClaimedMilestones);
            Assert.AreEqual(4, loaded.WinStreak);
            Assert.AreEqual("2026-04-09", loaded.LastDailyBrewDate);
            Assert.AreEqual(2, loaded.DailyBrewStreak);
            CollectionAssert.AreEqual(new[] { "gem_pack_small" }, loaded.PurchasedProducts);
        }

        [Test]
        public void SaveLocal_CopiesArrays_SoMutatingSourceDoesNotChangeStored()
        {
            var completed = new[] { 1 };
            var data = new CloudSaveManager.PlayerSaveData { CompletedLevels = completed };
            _manager.SaveLocal(data);

            completed[0] = 99;
            Assert.AreEqual(1, _manager.GetLocalData().CompletedLevels[0]);
        }

        [Test]
        public void MarkDirty_SetsIsDirty_MarkSynced_ClearsAndRecordsTime()
        {
            Assert.IsFalse(_manager.IsDirty);
            Assert.IsNull(_manager.LastSyncUtc);

            _manager.SaveLocal(new CloudSaveManager.PlayerSaveData { Essence = 1 });
            _manager.MarkDirty();
            Assert.IsTrue(_manager.IsDirty);

            var t = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc);
            _manager.MarkSynced(t);

            Assert.IsFalse(_manager.IsDirty);
            Assert.AreEqual(t, _manager.LastSyncUtc);
        }

        [Test]
        public void SaveLocal_FiresOnSaveRequested_WithStoredData()
        {
            CloudSaveManager.PlayerSaveData received = null;
            _manager.OnSaveRequested += d => { received = d; };

            var data = new CloudSaveManager.PlayerSaveData { Essence = 42, Gems = 7 };
            _manager.SaveLocal(data);

            Assert.IsNotNull(received);
            Assert.AreEqual(42, received.Essence);
            Assert.AreEqual(7, received.Gems);
        }
    }
}
