using System.Linq;
using Brew.Core.Meta;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class PotionShelfManagerTests
    {
        private static PotionShelfMilestoneDefinition[] DefaultMilestones() =>
            new[]
            {
                new PotionShelfMilestoneDefinition(25, 500, 25, "Apprentice"),
                new PotionShelfMilestoneDefinition(50, 1000, 50, "Brewer"),
                new PotionShelfMilestoneDefinition(75, 2000, 100, "Alchemist"),
                new PotionShelfMilestoneDefinition(100, 5000, 250, "Master Brewer")
            };

        private PotionShelfManager _shelf;

        [SetUp]
        public void SetUp()
        {
            _shelf = new PotionShelfManager(100, DefaultMilestones());
        }

        [Test]
        public void TryUnlockPotion_NewId_ReturnsTrue_AndCountsTowardBrewed()
        {
            Assert.IsTrue(_shelf.TryUnlockPotion(7));
            Assert.AreEqual(1, _shelf.BrewedCount);
            Assert.IsTrue(_shelf.HasPotion(7));
        }

        [Test]
        public void TryUnlockPotion_Duplicate_ReturnsFalse()
        {
            Assert.IsTrue(_shelf.TryUnlockPotion(3));
            Assert.IsFalse(_shelf.TryUnlockPotion(3));
            Assert.AreEqual(1, _shelf.BrewedCount);
        }

        [Test]
        public void CheckMilestones_ReturnsNewlyClaimed_Once_AndFiresEvent()
        {
            int eventCount = 0;
            _shelf.OnMilestoneReached += (_, _, _, _) => eventCount++;

            for (int i = 1; i <= 25; i++)
                _shelf.TryUnlockPotion(i);

            var first = _shelf.CheckMilestones().ToList();
            Assert.AreEqual(1, first.Count);
            Assert.AreEqual(25, first[0].RequiredCount);
            Assert.AreEqual(1, eventCount);

            var second = _shelf.CheckMilestones().ToList();
            Assert.AreEqual(0, second.Count);
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void CompletionPercent_ScalesWithTotalPotions()
        {
            var small = new PotionShelfManager(10, new[] { new PotionShelfMilestoneDefinition(10, 1, 1, "Done") });
            for (int i = 0; i < 3; i++)
                small.TryUnlockPotion(i);

            Assert.AreEqual(30f, small.CompletionPercent, 0.001f);
        }

        [Test]
        public void LoadState_RestoresPotionsAndClaimedMilestones()
        {
            _shelf.LoadState(new[] { 1, 2, 3 }, new[] { 25 });

            Assert.AreEqual(3, _shelf.BrewedCount);
            Assert.IsTrue(_shelf.HasPotion(2));

            for (int i = 4; i <= 25; i++)
                _shelf.TryUnlockPotion(i);

            var crossed = _shelf.CheckMilestones().ToList();
            Assert.AreEqual(0, crossed.Count);
        }

        [Test]
        public void OnPotionUnlocked_FiresOnNewUnlockOnly()
        {
            int fires = 0;
            _shelf.OnPotionUnlocked += _ => fires++;
            _shelf.TryUnlockPotion(99);
            _shelf.TryUnlockPotion(99);
            Assert.AreEqual(1, fires);
        }
    }
}
