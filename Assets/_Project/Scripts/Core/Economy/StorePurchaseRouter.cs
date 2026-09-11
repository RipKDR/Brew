namespace Brew.Core.Economy
{
    /// <summary>
    /// Production Buy path used by <c>GameFlowController</c>.
    /// Requests a store purchase; never calls <see cref="IAPManager.CompletePurchase"/>.
    /// </summary>
    public static class StorePurchaseRouter
    {
        public static bool OnBuyRequested(string productId, UnityIAPBridge bridge)
        {
            bool started = false;
            StoreCheckout.RequestPurchase(productId, id =>
            {
                if (bridge != null)
                    started = bridge.PurchaseProduct(id);
            });
            return started;
        }
    }
}
