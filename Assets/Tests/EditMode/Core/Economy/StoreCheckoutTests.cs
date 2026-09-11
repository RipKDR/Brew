using Brew.Core.Economy;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class StoreCheckoutTests
    {
        [Test]
        public void RequestPurchase_InvokesInitiatorOnly_DoesNotFulfill()
        {
            var currency = new CurrencyManager();
            var iap = new IAPManager(currency);
            string requested = null;

            StoreCheckout.RequestPurchase("gem_50", id => requested = id);

            Assert.AreEqual("gem_50", requested);
            Assert.AreEqual(0, currency.GetBalance(CurrencyType.Gems));
            Assert.IsFalse(iap.HasPurchased("gem_50"));

            iap.CompletePurchase("gem_50");
            Assert.AreEqual(50, currency.GetBalance(CurrencyType.Gems));
        }

        [Test]
        public void RequestPurchase_WhenInitiatorIsCompletePurchase_WouldGrant_SoStoreMustNotWireThat()
        {
            var currency = new CurrencyManager();
            var iap = new IAPManager(currency);

            StoreCheckout.RequestPurchase("gem_50", iap.CompletePurchase);

            Assert.AreEqual(50, currency.GetBalance(CurrencyType.Gems));
        }

        [Test]
        public void RequestPurchase_NullInitiator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => StoreCheckout.RequestPurchase("gem_50", null));
        }

        [Test]
        public void RequestPurchase_EmptyProductId_DoesNotInvokeInitiator()
        {
            var invoked = false;
            StoreCheckout.RequestPurchase(string.Empty, _ => invoked = true);
            StoreCheckout.RequestPurchase(null, _ => invoked = true);
            Assert.IsFalse(invoked);
        }

        [Test]
        public void TryParseUsdPrice_ParsesCatalogDisplay()
        {
            Assert.IsTrue(StoreCheckout.TryParseUsdPrice("$0.99", out var usd));
            Assert.AreEqual(0.99f, usd, 0.0001f);
        }

        [Test]
        public void TryParseUsdPrice_RejectsEmpty()
        {
            Assert.IsFalse(StoreCheckout.TryParseUsdPrice(" ", out _));
            Assert.IsFalse(StoreCheckout.TryParseUsdPrice(null, out _));
        }
    }
}
