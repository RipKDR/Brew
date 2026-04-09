using Brew.Core.Economy;
using Brew.Core.Meta;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class WorkshopManagerTests
    {
        private static (string name, int essenceCost)[] SampleUpgrades() =>
            new[]
            {
                ("Sweep the Floor", 50),
                ("Light the Hearth", 100),
                ("Repair the Workbench", 200),
                ("Hang the Shelves", 400),
                ("Install the Cauldron", 600),
                ("Stock the Herb Rack", 1000),
                ("Place the Star Map", 1500),
                ("Add the Crystal Array", 2500),
                ("Build the Distillery", 4000),
                ("Enchant the Windows", 6000),
                ("Summon the Familiar", 8000),
                ("Master's Flourish", 12000)
            };

        private CurrencyManager _currency;
        private WorkshopManager _workshop;

        [SetUp]
        public void SetUp()
        {
            _currency = new CurrencyManager();
            _currency.Add(CurrencyType.Essence, 100_000, "test_seed");
            _workshop = new WorkshopManager(SampleUpgrades(), _currency);
        }

        [Test]
        public void TryPurchaseNext_PurchasesSequentially_AndSpendsEssence()
        {
            Assert.AreEqual(0, _workshop.CurrentLevel);
            Assert.IsTrue(_workshop.TryPurchaseNext());
            Assert.AreEqual(1, _workshop.CurrentLevel);
            Assert.AreEqual(100_000 - 50, _currency.GetBalance(CurrencyType.Essence));

            Assert.IsTrue(_workshop.TryPurchaseNext());
            Assert.AreEqual(2, _workshop.CurrentLevel);
        }

        [Test]
        public void TryPurchaseNext_InsufficientFunds_ReturnsFalse()
        {
            var broke = new CurrencyManager();
            broke.Add(CurrencyType.Essence, 10, "tiny");
            var w = new WorkshopManager(SampleUpgrades(), broke);

            Assert.IsFalse(w.TryPurchaseNext());
            Assert.AreEqual(0, w.CurrentLevel);
            Assert.AreEqual(10, broke.GetBalance(CurrencyType.Essence));
        }

        [Test]
        public void TryPurchaseNext_AtMax_ReturnsFalse()
        {
            for (int i = 0; i < 12; i++)
                Assert.IsTrue(_workshop.TryPurchaseNext(), $"step {i}");

            Assert.AreEqual(12, _workshop.CurrentLevel);
            Assert.IsFalse(_workshop.TryPurchaseNext());
        }

        [Test]
        public void GetNextUpgradeName_AndCost_ReturnNullAndNegativeOne_WhenMaxed()
        {
            _workshop.LoadLevel(12);
            Assert.IsNull(_workshop.GetNextUpgradeName());
            Assert.AreEqual(-1, _workshop.GetNextUpgradeCost());
        }

        [Test]
        public void OnUpgradePurchased_FiresWithLevelAndName()
        {
            int level = -1;
            string name = null;
            _workshop.OnUpgradePurchased += (l, n) => { level = l; name = n; };

            _workshop.TryPurchaseNext();

            Assert.AreEqual(1, level);
            Assert.AreEqual("Sweep the Floor", name);
        }

        [Test]
        public void LoadLevel_RestoresProgress()
        {
            _workshop.LoadLevel(5);
            Assert.AreEqual(5, _workshop.CurrentLevel);
            Assert.AreEqual("Stock the Herb Rack", _workshop.GetNextUpgradeName());
            Assert.AreEqual(1000, _workshop.GetNextUpgradeCost());
        }
    }
}
