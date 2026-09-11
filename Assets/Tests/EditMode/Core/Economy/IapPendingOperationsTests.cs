using Brew.Core.Economy;
using NUnit.Framework;

namespace Brew.Tests.EditMode
{
    [TestFixture]
    public class IapPendingOperationsTests
    {
        [Test]
        public void RequestPurchase_WhenStoreNotReady_QueuesInsteadOfDispatching()
        {
            var pending = new IapPendingOperations();

            var disposition = pending.RequestPurchase("gem_50", storeReady: false);

            Assert.AreEqual(IapPurchaseDisposition.Queued, disposition);
            Assert.AreEqual("gem_50", pending.PendingPurchaseId);
        }

        [Test]
        public void RequestPurchase_WhenStoreReady_DispatchesNow()
        {
            var pending = new IapPendingOperations();

            var disposition = pending.RequestPurchase("gem_50", storeReady: true);

            Assert.AreEqual(IapPurchaseDisposition.DispatchNow, disposition);
            Assert.IsNull(pending.PendingPurchaseId);
        }

        [Test]
        public void RequestRestore_WhenStoreNotReady_Queues()
        {
            var pending = new IapPendingOperations();

            var disposition = pending.RequestRestore(storeReady: false);

            Assert.AreEqual(IapRestoreDisposition.Queued, disposition);
            Assert.IsTrue(pending.PendingRestore);
        }

        [Test]
        public void TryTake_FlushesQueuedRestoreThenPurchase()
        {
            var pending = new IapPendingOperations();
            pending.RequestRestore(storeReady: false);
            pending.RequestPurchase("no_ads", storeReady: false);

            Assert.IsTrue(pending.TryTakeRestore());
            Assert.IsFalse(pending.PendingRestore);
            Assert.IsTrue(pending.TryTakePurchase(out var productId));
            Assert.AreEqual("no_ads", productId);
            Assert.IsFalse(pending.TryTakeRestore());
            Assert.IsFalse(pending.TryTakePurchase(out _));
        }

        [Test]
        public void RequestPurchase_EmptyId_Ignored()
        {
            var pending = new IapPendingOperations();
            Assert.AreEqual(IapPurchaseDisposition.Ignored, pending.RequestPurchase("", storeReady: false));
            Assert.IsNull(pending.PendingPurchaseId);
        }
    }
}
