using System.Linq;
using Brew.Core.Economy;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class IAPManagerTests
    {
        private CurrencyManager _currency;
        private IAPManager _iap;

        [SetUp]
        public void SetUp()
        {
            _currency = new CurrencyManager();
            _iap = new IAPManager(_currency);
        }

        [Test]
        public void Catalog_HasSixEntries()
        {
            Assert.AreEqual(6, IAPManager.Catalog.Count);
        }

        [Test]
        public void CompletePurchase_GemPack_AddsGems()
        {
            _iap.CompletePurchase("gem_50");
            Assert.AreEqual(50, _currency.GetBalance(CurrencyType.Gems));
        }

        [Test]
        public void CompletePurchase_NoAds_MarksNonConsumablePurchased()
        {
            _iap.CompletePurchase("no_ads");
            Assert.IsTrue(_iap.HasPurchased("no_ads"));
        }

        [Test]
        public void GetAvailableProducts_AfterStarterBundlePurchase_ExcludesStarterBundle()
        {
            _iap.CompletePurchase("starter_bundle");

            var available = _iap.GetAvailableProducts(currentLevel: 5);
            Assert.IsFalse(available.Any(p => p.ProductId == "starter_bundle"));
        }

        [Test]
        public void GetAvailableProducts_AboveLevelFifteen_ExcludesStarterBundle()
        {
            var available = _iap.GetAvailableProducts(currentLevel: 16);
            Assert.IsFalse(available.Any(p => p.ProductId == "starter_bundle"));
        }

        [Test]
        public void HasNoAdsPass_AfterNoAdsPurchase_IsTrue()
        {
            _iap.CompletePurchase("no_ads");
            Assert.IsTrue(_iap.HasNoAdsPass);
        }
    }
}
