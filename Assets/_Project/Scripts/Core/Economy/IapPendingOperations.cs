using System;

namespace Brew.Core.Economy
{
    public enum IapPurchaseDisposition
    {
        Ignored,
        Queued,
        DispatchNow
    }

    public enum IapRestoreDisposition
    {
        Queued,
        DispatchNow
    }

    /// <summary>
    /// Queues Buy/Restore while Unity IAP is still initializing (ADR 0005).
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public sealed class IapPendingOperations
    {
        public string PendingPurchaseId { get; private set; }
        public bool PendingRestore { get; private set; }

        public IapPurchaseDisposition RequestPurchase(string productId, bool storeReady)
        {
            if (string.IsNullOrEmpty(productId))
                return IapPurchaseDisposition.Ignored;

            if (!storeReady)
            {
                PendingPurchaseId = productId;
                return IapPurchaseDisposition.Queued;
            }

            PendingPurchaseId = null;
            return IapPurchaseDisposition.DispatchNow;
        }

        public IapRestoreDisposition RequestRestore(bool storeReady)
        {
            if (!storeReady)
            {
                PendingRestore = true;
                return IapRestoreDisposition.Queued;
            }

            PendingRestore = false;
            return IapRestoreDisposition.DispatchNow;
        }

        public bool TryTakeRestore()
        {
            if (!PendingRestore)
                return false;

            PendingRestore = false;
            return true;
        }

        public bool TryTakePurchase(out string productId)
        {
            productId = PendingPurchaseId;
            PendingPurchaseId = null;
            return !string.IsNullOrEmpty(productId);
        }
    }
}
